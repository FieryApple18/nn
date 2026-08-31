using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocRelatedToWorkBase;
using DirRX.NNCommons;

namespace DirRX.OverrideSolution.Server
{
  partial class DocRelatedToWorkBaseFunctions
  {

    /// <summary>
    /// Запустить АО по запуску задач для повторной проштамповки документов.
    /// <param name="documentsIds">Список ИД документов.</param>"
    /// <param name="documentsIds">ИД варианта процесса.</param>"
    /// </summary>
    [Remote]
    public static void StartRestampingAsyncHandler(List<long> documentsIds, long processKindId)
    {
      var handler = NNIntegration.AsyncHandlers.CreateRestampingTasksAsync.Create();
      handler.UserId = Users.Current.Id;
      handler.ProcessKindId = processKindId;
      handler.DocumentsIds = string.Join(",", documentsIds);
      handler.ExecuteAsync();
    }

    /// <summary>
    /// Проверить возможность обновления документа из АСУП.
    /// </summary>
    /// <returns>string.Empty - если можно обновлять документ, текст причины почему обновлять документ нельзя.</returns>
    /// <remarks>Документ может быть обновлён если поля "Состояние" - "В разработке", "Согласование" - не заполнено.</remarks>
    [Public]
    public virtual string CanUpdateDocFromASUP()
    {
      if (_obj.LifeCycleState == HRPro.HRProSolution.OfficialDocument.LifeCycleState.Draft && !_obj.InternalApprovalState.HasValue)
        return string.Empty;
      else
        return "Обновление возможно только для документов, у которых значения полей жизненного цикла: \"Состояние документа HR Pro\" - \"В разработке\", \"Согласование\" - не заполнено";
    }

    /// <summary>
    /// Проверить наличие прав на просмотр документа по ролевой модели для текущего пользователя.
    /// </summary>
    /// <returns>Признак наличия прав. True - если права есть. False - если нет.</returns>
    [Public, Remote(IsPure = true)]
    public virtual bool HasRightsOnEDoc()
    {
      return NNCommons.PublicFunctions.Module.Remote.HasRightsOnEDoc(Employees.Current, _obj);
    }

    /// <summary>
    /// Подбор ролей для выдачи экземплярных прав на документ.
    /// </summary>
    [Public]
    public virtual void IssueRightsForBusinessRoles()
    {
      var businessRolesByKind = GetBusinessRolesByDocumentKind();
      var businessRolesByProcess = GetBusinessRolesByProcess();
      
      // Удаление старых экземплярных прав для бизнес ролей.
      var oldBusinessRoles = _obj.AccessRights.Current
        .Where(x => x.AccessRightsType == DefaultAccessRightsTypes.Read &&
               BusinessRoles.Is(x.Recipient) &&
               !businessRolesByKind.Any(b => b.Id == x.Recipient.Id) &&
               !businessRolesByProcess.Any(b => b.Id == x.Recipient.Id))
        .Select(x => x.Recipient);
      
      foreach (var role in oldBusinessRoles)
        _obj.AccessRights.Revoke(role, DefaultAccessRightsTypes.Read);
      
      // Выдача новых экземплярных прав для бизнес ролей по виду документа.
      foreach (var role in businessRolesByKind)
        if (!_obj.AccessRights.IsGranted(DefaultAccessRightsTypes.Read, role))
          _obj.AccessRights.Grant(role, DefaultAccessRightsTypes.Read);
      
      // Выдача новых экземплярных прав для бизнес ролей по процессу.
      foreach (var role in businessRolesByProcess)
        if (!_obj.AccessRights.IsGranted(DefaultAccessRightsTypes.Read, role))
          _obj.AccessRights.Grant(role, DefaultAccessRightsTypes.Read);
    }

    /// <summary>
    /// Получить роли для выдачи экземплярных прав по виду документа.
    /// </summary>
    public virtual IQueryable<IBusinessRole> GetBusinessRolesByDocumentKind()
    {
      return BusinessRoles.GetAll().Where(x => x.Status == NNCommons.BusinessRole.Status.Active &&
                                          x.Top == _obj.TopDirRX &&
                                          x.RoleType == NNCommons.BusinessRole.RoleType.ByKind &&
                                          DocumentKinds.Equals(x.DocumentKind, _obj.DocumentKind));
    }

    /// <summary>
    /// Получить роли для выдачи экземплярных прав по процессу.
    /// </summary>
    public virtual IQueryable<IBusinessRole> GetBusinessRolesByProcess()
    {
      var businessRolesByProcess = BusinessRoles.GetAll().Where(x => x.Status == NNCommons.BusinessRole.Status.Active &&
                                                                x.Top == _obj.TopDirRX &&
                                                                x.RoleType == NNCommons.BusinessRole.RoleType.ByProcess &&
                                                                BusinessProcesses.Equals(x.Process, DocumentKinds.As(_obj.DocumentKind).BusinessProcessDirRX));
      
      // Если обнаружены роли по процессу, то получить расширенный список ИД бизнесс ролей с учётом иерархии справочника Бизнесс процессы.
      if (businessRolesByProcess.Any())
      {
        var businessRolesIds = NNCommons.PublicFunctions.BusinessRole.GetBusinessRolesByProccessHierarchy(businessRolesByProcess.Select(x => x.Id).ToList());
        businessRolesByProcess = BusinessRoles.GetAll(x => businessRolesIds.Contains(x.Id) && x.Top == _obj.TopDirRX);
      }
      
      return businessRolesByProcess;
    }

    /// <summary>
    /// Получить список с информацией о вариантах процесса для повторной проштамповки.
    /// </summary>
    [Public, Remote]
    public static IQueryable<Sungero.Workflow.IProcessKind> GetRestampingProcessKinds()
    {
      var processKinds = Enumerable.Empty<Sungero.Workflow.IProcessKind>().AsQueryable();
      
      // Поиск константы для списка ИД вариантов процесса для отправки документов на повторную проштамповку.
      var constant = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(NNIntegration.PublicConstants.Module.ConstantsSetting.RestampingProcessKindsIds.Guid);
      if (string.IsNullOrEmpty(constant))
      {
        Logger.DebugFormat("GetRestampingProcessKinds. Прикладная константа '{0}' не заполнена.", NNIntegration.PublicConstants.Module.ConstantsSetting.RestampingProcessKindsIds.Guid);
        return processKinds;
      }
      
      // Парсинг строки в список идентификаторов.
      var processKindsIds = new List<long>();
      try
      {
        processKindsIds = constant.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => long.Parse(id.Trim())).ToList();
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("GetRestampingProcessKinds. Не удалось извлечь список ИД из прикладной константы: {0}. Текст ошибки: {1}. StackTrace: {2}.",
                           NNIntegration.PublicConstants.Module.ConstantsSetting.RestampingProcessKindsIds.Guid,
                           ex.Message, ex.StackTrace);
        return processKinds;
      }
      
      // Поиск действующих вариантов процесса и заполнения структуры с информацией по ним.
      processKinds = Sungero.Workflow.ProcessKinds.GetAll(x => x.Status == Sungero.Workflow.ProcessKind.Status.Active && processKindsIds.Contains(x.Id));
      
      return processKinds;
    }

    /// <summary>
    /// Получить документы для повторной проштамповки.
    /// </summary>
    /// <param name="ids">Список ИД документов.</param>
    /// <returns>Документы.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<DirRX.OverrideSolution.IDocRelatedToWorkBase> GetRestampingDocuments(List<long> ids)
    {
      return DirRX.OverrideSolution.DocRelatedToWorkBases.GetAll(x => ids.Contains(x.Id));
    }

    /// <summary>
    /// Запустить задачу для перештамповки документа.
    /// </summary>
    /// <param name="documents">Документ.</param>
    /// <param name="performer">Исполнитель.</param>
    /// <param name="processKind">Вариант процесса.</param>
    /// <returns>Ошибка.</returns>
    [Public, Remote]
    public static string StartRestampingHRProTask(IDocRelatedToWorkBase document, IUser performer, Sungero.Workflow.IProcessKind processKind)
    {
      Logger.DebugFormat("StartRestampingHRProTasks. ИД документа: {0}. Создание задачи для перештамповки документа.", document.Id);
      var error = string.Empty;
      try
      {
        var task = HRPro.HRDocFlow.HRSimpleTasks.Create();
        task.ProcessKind = processKind;
        task.Performers.AddNew().Performer = performer;
        task.Subject = DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.RestampingTaskSubjectFormat(document.Name);
        task.Author = performer;
        task.DocumentGroup.OfficialDocuments.Add(document);
        task.Start();
        Logger.DebugFormat("StartRestampingHRProTasks. ИД документа: {0}. ИД задачи: {1}. Запущена задача для перештамповки документа по варианту процесса: {2}.",
                           document.Id, task.Id, processKind.Id);
      }
      catch (Exception ex)
      {
        error = ex.Message;
        Logger.ErrorFormat("StartRestampingHRProTasks. ИД документа: {0}. Ошибка: {1}. StackTrace: {2}.", document.Id, ex.Message, ex.StackTrace);
      }
      
      return error;
    }

  }
}