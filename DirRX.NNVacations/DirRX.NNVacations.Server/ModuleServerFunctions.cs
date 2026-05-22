using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using DirRX.OverrideSolution;
using DocKind = DirRX.NNVacations.Constants.Module.DocumentKind;
using DocKindFunctions = Sungero.Docflow.PublicFunctions.DocumentKind;
using TableDataKeys = DirRX.NNVacations.Constants.Module.TableDataKeys;

namespace DirRX.NNVacations.Server
{
  public partial class ModuleFunctions
  {
    
    #region Назначение замещающих на период отпуска.

    #region Основные методы создания/обновления/закрытия замещений.

    /// <summary>
    /// Создать замещения по данным из задачи.
    /// </summary>
    /// <param name="task">Задача.</param>
    [Public]
    public virtual void CreateSubstitutionsFromTask(Sungero.Workflow.ITask task)
    {
      var logPrefix = "CreateSubstitutionsFromTask";
      
      Logger.DebugFormat("{0}. Start.", logPrefix);

      var data = TryExtractDataForSubstitutionFromTask(task, 0, logPrefix);
      if (data == null)
        return;
      
      var system = DirRX.NNOtherProcesses.PublicFunctions.SubstitutionSystem.GetDefaultRecord();
      
      var substitutions = GetSubstitutionInfosFromTask(task.MainTask);
      if (!substitutions.Any())
      {
        Logger.DebugFormat("{0}. No substitutions found for mainTaskId={1}.", logPrefix, task.MainTask.Id);
        return;
      }

      foreach (var item in substitutions)
      {
        var substitutionPersonalAccount = DirRX.NNOtherProcesses.PublicFunctions.SubstitutionPersonalAccount.CreateRecord(item, data.Employee, task.MainTask.Id, logPrefix);
        if (system != null && item.SubstitutionSystem.Id == system.Id)
        {
          var substitution = CreateSubstitution(item, data.Employee, data.OrderId.ToString());
          if (substitution != null)
          {
            substitutionPersonalAccount.Created = GetCreateDateForSubstitution(substitution);
            substitutionPersonalAccount.SentStatus = DirRX.NNOtherProcesses.SubstitutionPersonalAccount.SentStatus.Assigned;
            
            substitutionPersonalAccount.Save();
          }
        }
      }

      Logger.DebugFormat("{0}. Completed for mainTaskId={1}, created {2} substitutions.", logPrefix, task.MainTask.Id, substitutions.Count());
    }

    /// <summary>
    /// Обновить даты замещения по данным из задачи.
    /// </summary>
    /// <param name="task">Задача.</param>
    [Public]
    public virtual void UpdateSubstitutionsFromTask(Sungero.Workflow.ITask task)
    {
      var logPrefix = "UpdateSubstitutionsFromTask";
      
      Logger.DebugFormat("{0}. Start.", logPrefix);

      var data = TryExtractDataForSubstitutionFromTask(task, 1, logPrefix);
      if (data == null)
        return;

      foreach (var subst in FindExistingSubstitutions(data.Employee, data.OriginalPeriod, logPrefix))
      {
        var subStart = subst.StartDate.Value;
        var subEnd = subst.EndDate.Value;
        var recallStart = data.RecallPeriod.StartDate.Value;
        var recallEnd = data.RecallPeriod.EndDate.Value;
        
        if (subStart > recallEnd || subEnd < recallStart)
          continue;
        
        DirRX.NNOtherProcesses.PublicFunctions.SubstitutionPersonalAccount.CancelRecords(subst, logPrefix);
        
        if (recallStart <= subStart && recallEnd >= subEnd)
        {
          subst.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;
          subst.Save();
          
          Logger.DebugFormat("{0}. Closed substitutionId={1}.", logPrefix, subst.Id);
          
          continue;
        }
        
        if (recallStart > subStart && recallEnd < subEnd)
        {
          var tail = Sungero.CoreEntities.Substitutions.Copy(subst);
          tail.StartDate = recallEnd.AddDays(1);
          tail.Save();
          
          DirRX.NNOtherProcesses.PublicFunctions.SubstitutionPersonalAccount.CreateRecord(tail, task.MainTask.Id, logPrefix);
          
          subst.EndDate = recallStart.AddDays(-1);
          subst.Save();
          
          DirRX.NNOtherProcesses.PublicFunctions.SubstitutionPersonalAccount.CreateRecord(subst, task.MainTask.Id, logPrefix);
          
          continue;
        }
        
        if (recallStart <= subStart)
        {
          var newStartDate = recallEnd.AddDays(1);
          if (newStartDate > subEnd)
          {
            subst.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;
            subst.Save();
            
            Logger.DebugFormat("{0}. Closed substitutionId={1}, newStartDate {2} > endDate {3}.", logPrefix, subst.Id, newStartDate, subEnd);
            
            continue;
          }
          
          subst.StartDate = recallEnd.AddDays(1);
          subst.Save();
          
          Logger.DebugFormat("{0}. Updated substitutionId={1}, oldStartDate={2}, newStartDate={3}.", logPrefix, subst.Id, subStart, newStartDate);
          
          DirRX.NNOtherProcesses.PublicFunctions.SubstitutionPersonalAccount.CreateRecord(subst, task.MainTask.Id, logPrefix);
          
          continue;
        }
        
        if (recallEnd >= subEnd)
        {
          var newEndDate = recallStart.AddDays(-1);
          if (newEndDate < subStart)
          {
            subst.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;
            subst.Save();
            
            Logger.DebugFormat("{0}. Closed substitutionId={1}, newEndDate {2} < startDate {3}.", logPrefix, subst.Id, newEndDate, subStart);
            
            continue;
          }
          
          subst.EndDate = newEndDate;
          subst.Save();
          
          Logger.DebugFormat("{0}. Updated substitutionId={1}, oldEndDate={2}, newEndDate={3}.", logPrefix, subst.Id, subEnd, newEndDate);
          
          DirRX.NNOtherProcesses.PublicFunctions.SubstitutionPersonalAccount.CreateRecord(subst, task.MainTask.Id, logPrefix);
          
          continue;
        }
      }
      
      Logger.DebugFormat("{0}. Completed for mainTaskId={1}.", logPrefix, task.MainTask.Id);
    }

    /// <summary>
    /// Закрыть замещения по данным из задачи.
    /// </summary>
    /// <param name="task">Задача.</param>
    [Public]
    public virtual void CloseSubstitutionsFromTask(Sungero.Workflow.ITask task)
    {
      var logPrefix = "CloseSubstitutionsFromTask";
      
      Logger.DebugFormat("{0}. Start.", logPrefix);

      var data = TryExtractDataForSubstitutionFromTask(task, 2, logPrefix);
      if (data == null)
        return;
      
      foreach (var subst in FindExistingSubstitutions(data.Employee, data.OriginalPeriod, logPrefix))
      {
        DirRX.NNOtherProcesses.PublicFunctions.SubstitutionPersonalAccount.CancelRecords(subst, logPrefix);
        
        subst.Status = Sungero.CoreEntities.DatabookEntry.Status.Closed;
        subst.Save();

        Logger.DebugFormat("{0}. Closed substitutionId={1}.", logPrefix, subst.Id);
      }

      Logger.DebugFormat("{0}. Completed for mainTaskId={1}.", logPrefix, task.MainTask.Id);
    }

    #endregion

    #region Вспомогательные методы.

    /// <summary>
    /// Выполнить валидацию задачи и собрать данные для работы с замещениями.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="operationType">Тип операции: 0 - создание, 1 - изменение, 2 - закрытие.</param>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <returns>Данные для работы или null, если валидация не пройдена.</returns>
    private DirRX.NNVacations.Structures.Module.IDataForSubstitutionFromTask TryExtractDataForSubstitutionFromTask(Sungero.Workflow.ITask task, int operationType, string logPrefix)
    {
      var result = DirRX.NNVacations.Structures.Module.DataForSubstitutionFromTask.Create();
      
      if (task == null)
      {
        Logger.DebugFormat("{0}. Task is null.", logPrefix);
        return null;
      }
      
      if (task.MainTask == null)
      {
        Logger.DebugFormat("{0}. MainTask is null.", logPrefix);
        return null;
      }
      
      var order = GetVacationOrderDocumentFromTask(task);
      if (order == null)
      {
        Logger.DebugFormat("{0}. Order is null.", logPrefix);
        return null;
      }
      
      result.OrderId = order.Id;
      
      var employee = DirRX.OverrideSolution.Employees.As(order.Employee);
      if (employee == null)
      {
        Logger.DebugFormat("{0}. Employee is null.", logPrefix);
        return null;
      }
      
      result.Employee = employee;
      
      if (operationType != 0)
      {
        var statement = GetVacationStatementDocumentFromTask(task.MainTask);
        if (statement == null)
        {
          Logger.DebugFormat("{0}. Statement is null.", logPrefix);
          return null;
        }
        
        var originalPeriod = GetOriginalPeriodFromStatement(statement);
        if (originalPeriod == null)
        {
          Logger.DebugFormat("{0}. No original period found.", logPrefix);
          return null;
        }
        
        result.OriginalPeriod = originalPeriod;
        
        if (operationType == 1)
        {
          var recallPeriod = GetRecallPeriodFromStatement(statement);
          if (recallPeriod == null)
          {
            Logger.DebugFormat("{0}. No recall period found.", logPrefix);
            return null;
          }
          
          result.RecallPeriod = recallPeriod;
        }
      }
      
      var logParams = string.Empty;
      
      if (operationType == 2)
        logParams += string.Format(", originalPeriod={0}-{1}", result.OriginalPeriod.StartDate, result.OriginalPeriod.EndDate);
      if (operationType == 1)
        logParams += string.Format(", recallPeriod={0}-{1}", result.RecallPeriod.StartDate, result.RecallPeriod.EndDate);
      
      Logger.DebugFormat("{0}. Params: taskId={1}, orderId={2}, employeeId={3}{4}.", logPrefix, task.Id, order.Id, employee.Id, logParams);
      
      return result;
    }

    /// <summary>
    /// Получить приказ из группы вложений задачи.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <returns>Приказ или null, если приказ не найден.</returns>
    private static DirRX.OverrideSolution.IVacationOrderDocument GetVacationOrderDocumentFromTask(Sungero.Workflow.ITask task)
    {
      if (HRPro.HRDocFlow.HRProcessTasks.Is(task))
      {
        var document = HRPro.HRDocFlow.HRProcessTasks.As(task).DocumentGroup.OfficialDocuments.FirstOrDefault();
        if (document != null && DirRX.OverrideSolution.VacationOrderDocuments.Is(document))
        {
          return DirRX.OverrideSolution.VacationOrderDocuments.As(document);
        }
      }
      
      return DirRX.OverrideSolution.VacationOrderDocuments.Null;
    }

    /// <summary>
    /// Получить заявление из группы вложений задачи.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <returns>Заявление или null, если заявление не найдено.</returns>
    private static DirRX.OverrideSolution.IVacationStatementDocument GetVacationStatementDocumentFromTask(Sungero.Workflow.ITask task)
    {
      if (DirRX.OverrideSolution.StatementTasks.Is(task))
      {
        var document = DirRX.OverrideSolution.StatementTasks.As(task).DocumentGroup.DocRelatedToWorkBases.FirstOrDefault();
        if (document != null && DirRX.OverrideSolution.VacationStatementDocuments.Is(document))
        {
          return DirRX.OverrideSolution.VacationStatementDocuments.As(document);
        }
      }
      
      return DirRX.OverrideSolution.VacationStatementDocuments.Null;
    }

    /// <summary>
    /// Получить исходный период отпуска из заявления.
    /// </summary>
    /// <param name="statement">Заявление.</param>
    /// <returns>Период отпуска или null, если не найден.</returns>
    private static DirRX.NNVacations.Structures.Module.IVacationPeriod GetOriginalPeriodFromStatement(DirRX.OverrideSolution.IVacationStatementDocument statement)
    {
      if (statement.StartDateDirRX.HasValue && statement.EndDateDirRX.HasValue)
      {
        return DirRX.NNVacations.Structures.Module.VacationPeriod.Create(statement.StartDateDirRX.Value, statement.EndDateDirRX.Value);
      }
      
      return null;
    }

    /// <summary>
    /// Получить период отзыва/прерывания из заявления.
    /// </summary>
    /// <param name="statement">Заявление.</param>
    /// <returns>Период отзыва или null, если не найден.</returns>
    private static DirRX.NNVacations.Structures.Module.IVacationPeriod GetRecallPeriodFromStatement(DirRX.OverrideSolution.IVacationStatementDocument statement)
    {
      if (statement.InterruptStartDateDirRX.HasValue && statement.InterruptEndDateDirRX.HasValue)
      {
        return DirRX.NNVacations.Structures.Module.VacationPeriod.Create(statement.InterruptStartDateDirRX.Value, statement.InterruptEndDateDirRX.Value);
      }
      
      return null;
    }

    /// <summary>
    /// Получить периоды переноса отпуска из заявления.
    /// </summary>
    /// <param name="statement">Заявление.</param>
    /// <returns>Список периодов переноса отпуска или пустой список.</returns>
    private static List<DirRX.NNVacations.Structures.Module.IVacationPeriod> GetShiftPeriodsFromStatement(DirRX.OverrideSolution.IVacationStatementDocument statement)
    {
      var result = new List<DirRX.NNVacations.Structures.Module.IVacationPeriod>();

      if (statement.AdditionalVacationDirRX.Any())
      {
        foreach (var data in statement.AdditionalVacationDirRX)
        {
          var period = DirRX.NNVacations.Structures.Module.VacationPeriod.Create();
          
          period.StartDate = data.StartDate.Value;
          period.EndDate = data.EndDate.Value;
          
          result.Add(period);
        }
      }
      
      return result;
    }

    /// <summary>
    /// Получить периоды с коллизией из заявления.
    /// </summary>
    /// <param name="statement">Заявление.</param>
    /// <param name="startDate">Дата начала.</param>
    /// <param name="endDate">Дата окончания.</param>
    /// <returns>Список периодов с коллизией.</returns>
    private List<DirRX.NNVacations.Structures.Module.IVacationPeriod> GetPeriodsFromCollisionDocument(DirRX.OverrideSolution.IVacationStatementDocument statement, DateTime startDate, DateTime endDate)
    {
      var result = new List<DirRX.NNVacations.Structures.Module.IVacationPeriod>();
      var allPeriods = new List<DirRX.NNVacations.Structures.Module.IVacationPeriod>();
      
      var annualKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationStatementKind);
      var otherKind = DocKindFunctions.GetNativeDocumentKind(DirRX.NNVacations.PublicConstants.Module.DocumentKind.OtherVacationKind);
      var shiftKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationShiftStatementKind);
      var selfRecallKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationSelfRecallStatementKind);
      var recallKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallConsentKind);
      
      if (Equals(statement.DocumentKind, annualKind) || Equals(statement.DocumentKind, otherKind))
      {
        allPeriods.Add(GetOriginalPeriodFromStatement(statement));
      }
      else if (Equals(statement.DocumentKind, recallKind) || Equals(statement.DocumentKind, selfRecallKind) || Equals(statement.DocumentKind, shiftKind))
      {
        allPeriods = GetShiftPeriodsFromStatement(statement);
      }
      
      foreach (var period in allPeriods)
      {
        if (startDate <= period.EndDate && endDate >= period.StartDate)
        {
          result.Add(period);
        }
      }
      
      return result;
    }

    /// <summary>
    /// Найти существующие активные замещения по периоду.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="period">Период для поиска.</param>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <returns>Список найденных замещений.</returns>
    private List<Sungero.CoreEntities.ISubstitution> FindExistingSubstitutions(DirRX.OverrideSolution.IEmployee employee, DirRX.NNVacations.Structures.Module.IVacationPeriod period, string logPrefix)
    {
      var existingSubsts = Sungero.CoreEntities.Substitutions.GetAll()
        .Where(s => Equals(s.User, Sungero.CoreEntities.Users.As(employee)) &&
               s.StartDate.HasValue && s.EndDate.HasValue &&
               s.StartDate.Value >= period.StartDate &&
               s.EndDate.Value <= period.EndDate &&
               s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active &&
               s.Comment.Contains(DirRX.NNVacations.Resources.SubstitutionNote))
        .ToList();
      
      Logger.DebugFormat("{0}. Found {1} active substitutions for employeeId={2}.", logPrefix, existingSubsts.Count, employee.Id);
      
      return existingSubsts;
    }

    /// <summary>
    /// Получить информацию о замещениях из задачи.
    /// </summary>
    /// <param name="sourceTask">Задача.</param>
    /// <returns>Список структур с информацией о замещениях из задачи.</returns>
    private static List<DirRX.NNOtherProcesses.Structures.SubstitutionPersonalAccount.ISubstitutionInfo> GetSubstitutionInfosFromTask(Sungero.Workflow.ITask sourceTask)
    {
      var result = new List<DirRX.NNOtherProcesses.Structures.SubstitutionPersonalAccount.ISubstitutionInfo>();

      if (DirRX.NNVacations.RegistrationOfAnnualLeaves.Is(sourceTask))
      {
        var task = DirRX.NNVacations.RegistrationOfAnnualLeaves.As(sourceTask);
        foreach (var substitution in task.SubstitutionsDirRX)
        {
          var info = DirRX.NNOtherProcesses.Structures.SubstitutionPersonalAccount.SubstitutionInfo.Create();

          info.Substitute = substitution.Substitute;
          info.SubstitutionSystem = substitution.SubstitutionSystem;
          info.StartDate = substitution.StartDate.Value;
          info.EndDate = substitution.EndDate.Value;
          
          result.Add(info);
        }
        
        return result;
      }
      
      if (DirRX.OverrideSolution.StatementTasks.Is(sourceTask))
      {
        var task = DirRX.OverrideSolution.StatementTasks.As(sourceTask);
        foreach (var substitution in task.SubstitutionsDirRX)
        {
          var info = DirRX.NNOtherProcesses.Structures.SubstitutionPersonalAccount.SubstitutionInfo.Create();

          info.Substitute = substitution.Substitute;
          info.SubstitutionSystem = substitution.SubstitutionSystem;
          info.StartDate = substitution.StartDate.Value;
          info.EndDate = substitution.EndDate.Value;
          
          result.Add(info);
        }
        
        return result;
      }

      return result;
    }

    /// <summary>
    /// Создать запись в справочнике "Замещение".
    /// </summary>
    /// <param name="info">Структура с информацией о замещении.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="sourceId">ИД приказа для записи в примечание.</param>
    /// <returns>Запись справочника "Замещение".</returns>
    private static Sungero.CoreEntities.ISubstitution CreateSubstitution(DirRX.NNOtherProcesses.Structures.SubstitutionPersonalAccount.ISubstitutionInfo info, DirRX.OverrideSolution.IEmployee employee, string sourceId)
    {
      Logger.DebugFormat("CreateSubstitution. Start: substituteId={0}, employeeId={1}, sourceId={2}.", info.Substitute.Id, employee.Id, sourceId);
      
      var existing = Sungero.CoreEntities.Substitutions.GetAll()
        .Where(s => s.User.Id == employee.Id &&
               s.Substitute.Id == info.Substitute.Id &&
               s.StartDate.HasValue && s.EndDate.HasValue &&
               s.StartDate.Value == info.StartDate &&
               s.EndDate.Value == info.EndDate &&
               s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active &&
               s.Comment.Contains(DirRX.NNVacations.Resources.SubstitutionNote))
        .FirstOrDefault();

      if (existing != null)
      {
        Logger.DebugFormat("CreateSubstitution. Duplicate found, skip, existingId={0}.", existing.Id);
        return existing;
      }

      var substitution = Sungero.CoreEntities.Substitutions.Create();
      
      substitution.User = employee;
      substitution.Substitute = info.Substitute;
      substitution.StartDate = info.StartDate;
      substitution.EndDate = info.EndDate;
      substitution.Comment = DirRX.NNVacations.Resources.SubstitutionNote_AutoCreatedFormat(Calendar.Now, sourceId);
      
      substitution.Save();
      
      Logger.DebugFormat("CreateSubstitution. Created substitutionId={0}.", substitution.Id);
      
      return substitution;
    }

    /// <summary>
    /// Получить дату создания записи в справочнике "Замещение".
    /// </summary>
    /// <param name="substitution">Запись справочника "Замещение".</param>
    /// <returns>Дата создания</returns>
    [Public]
    public static DateTime? GetCreateDateForSubstitution(Sungero.CoreEntities.ISubstitution substitution)
    {
      return substitution.History.GetAll().OrderBy(h => h.HistoryDate)
        .Where(h => h.Action == Sungero.CoreEntities.History.Action.Create)
        .Select(h => h.HistoryDate)
        .FirstOrDefault();
    }

    /// <summary>
    /// Получить информацию о замещениях для инструкции задания на согласование.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <returns>Текст с информацией о замещениях или пустая строка, если замещений нет.</returns>
    [ExpressionElement("GetSubstitutionTextForEmployeeFunctionName", "GetSubstitutionTextForEmployeeFunctionDescription")]
    public virtual string GetSubstitutionTextForEmployee(Sungero.Workflow.ITask task)
    {
      var substitutions = GetSubstitutionInfosFromTask(task);
      
      if (!substitutions.Any())
        return string.Empty;

      var lines = substitutions.Select(r => string.Format("{0} в {1} с {2:d} по {3:d}",
                                                          r.Substitute.DisplayValue,
                                                          r.SubstitutionSystem.DisplayValue,
                                                          r.StartDate,
                                                          r.EndDate));

      return string.Join("\n", lines);
    }

    #endregion

    #region Интеграционные методы для ЛК.

    /// <summary>
    /// Получить строку с перечнем активных систем для замещения.
    /// </summary>
    /// <returns>Строка с перечнем активных систем для замещения.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string GetSubstitutionSystemsInfo()
    {
      var systems = DirRX.NNOtherProcesses.SubstitutionSystems.GetAll(s => s.Status == DirRX.NNOtherProcesses.SubstitutionSystem.Status.Active)
        .OrderBy(s => s.Id)
        .Select(s => string.Format("<b>{0}</b> - {1}", s.ShortName, s.Functions));
      
      return string.Join("<br/>", systems);
    }

    /// <summary>
    /// Получить список активных систем для замещения.
    /// </summary>
    /// <returns>Список структур с информацией об активных системах для замещения.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual List<DirRX.NNCommons.Structures.Module.IOptionItem> GetSubstitutionSystems()
    {
      return DirRX.NNOtherProcesses.SubstitutionSystems
        .GetAll(s => s.Status == DirRX.NNOtherProcesses.SubstitutionSystem.Status.Active)
        .OrderBy(s => s.Id)
        .Select(s => DirRX.NNCommons.Structures.Module.OptionItem.Create(s.Id, s.ShortName))
        .ToList();
    }

    /// <summary>
    /// Получить сотрудников для выбора в качестве замещающего.
    /// </summary>
    /// <param name="initiatorId">ИД инициатора (замещаемого сотрудника).</param>
    /// <param name="query">Строка поиска (Like-ввод).</param>
    /// <param name="takeCount">Количество элементов для загрузки (по умолчанию 25).</param>
    /// <param name="skipCount">Количество элементов для пропуска (по умолчанию 0).</param>
    /// <returns>Список структур с данными сотрудников для выбора в качестве замещающих.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual List<HRPro.HRCommons.Structures.Module.ISubstituteEmployeeInfo> GetEmployeesForSubstitution(long initiatorId, string query = null, int takeCount = 25, int skipCount = 0)
    {
      Logger.DebugFormat("GetEmployeesForSubstitution. Start: initiatorId={0}, query={1}, takeCount={2}, skipCount={3}.", initiatorId, query ?? "null", takeCount, skipCount);
      
      var result = new List<HRPro.HRCommons.Structures.Module.ISubstituteEmployeeInfo>();
      
      try
      {
        var initiator = DirRX.OverrideSolution.Employees.GetAll(e => e.Id == initiatorId).FirstOrDefault();
        
        if (initiator == null || initiator.Department == null || initiator.JobTitle == null)
        {
          Logger.DebugFormat("GetEmployeesForSubstitution. Initiator not found or has no department/jobtitle. InitiatorId={0}.", initiatorId);
          return result;
        }
        
        var initiatorJobTitle = DirRX.OverrideSolution.JobTitles.As(initiator.JobTitle);
        var initiatorEvaluationGroup = initiatorJobTitle.EvaluationGroupDirRX;
        var isTop = (initiatorEvaluationGroup == DirRX.OverrideSolution.JobTitle.EvaluationGroupDirRX.Top10) ||
          (initiatorEvaluationGroup == DirRX.OverrideSolution.JobTitle.EvaluationGroupDirRX.Top100);
        
        Logger.DebugFormat("GetEmployeesForSubstitution. Params: isTop={0}, departmentId={1}.", isTop, initiator.Department.Id);
        
        var employeesQuery = DirRX.OverrideSolution.Employees.GetAll(e => e.Status == DirRX.OverrideSolution.Employee.Status.Active &&
                                                                     e.IsVirtualDirRX.Value == false &&
                                                                     e.Person != null && e.Person.Id != initiator.Person.Id);
        
        if (!string.IsNullOrEmpty(query))
        {
          var searchQuery = query.ToLower().Trim();
          
          employeesQuery = employeesQuery.Where(e => (e.Name != null && e.Name.ToLower().Contains(searchQuery)) ||
                                                (e.Department != null && e.Department.Name != null && e.Department.Name.ToLower().Contains(searchQuery)) ||
                                                (e.JobTitle != null && e.JobTitle.Name != null && e.JobTitle.Name.ToLower().Contains(searchQuery)));
          
          Logger.DebugFormat("GetEmployeesForSubstitution. Applied search filter '{0}'. Found={1}.", searchQuery, employeesQuery.Select(e => e.Id).Count());
        }
        
        var topEmployeeIdsInDep = employeesQuery.Where(e => e.Department != null && e.Department.Id == initiator.Department.Id &&
                                                       e.JobTitle != null && DirRX.OverrideSolution.JobTitles.As(e.JobTitle).EvaluationGroupDirRX.HasValue &&
                                                       (DirRX.OverrideSolution.JobTitles.As(e.JobTitle).EvaluationGroupDirRX.Value == DirRX.OverrideSolution.JobTitle.EvaluationGroupDirRX.Top10 ||
                                                        DirRX.OverrideSolution.JobTitles.As(e.JobTitle).EvaluationGroupDirRX.Value == DirRX.OverrideSolution.JobTitle.EvaluationGroupDirRX.Top100))
          .Select(e => e.Id)
          .ToList();
        
        var topEmployeeIds = employeesQuery.Where(e => e.Department == null || (e.Department != null && e.Department.Id != initiator.Department.Id) &&
                                                  e.JobTitle != null && DirRX.OverrideSolution.JobTitles.As(e.JobTitle).EvaluationGroupDirRX.HasValue &&
                                                  (DirRX.OverrideSolution.JobTitles.As(e.JobTitle).EvaluationGroupDirRX.Value == DirRX.OverrideSolution.JobTitle.EvaluationGroupDirRX.Top10 ||
                                                   DirRX.OverrideSolution.JobTitles.As(e.JobTitle).EvaluationGroupDirRX.Value == DirRX.OverrideSolution.JobTitle.EvaluationGroupDirRX.Top100))
          .Select(e => e.Id)
          .ToList();
        
        var filteredEmployees = new List<DirRX.OverrideSolution.IEmployee>();
        
        filteredEmployees.AddRange(employeesQuery.Where(e => !(topEmployeeIds.Contains(e.Id)) && !(topEmployeeIdsInDep.Contains(e.Id))).ToList());
        
        if (isTop)
        {
          filteredEmployees.AddRange(employeesQuery.Where(e => topEmployeeIdsInDep.Contains(e.Id)).ToList());
          Logger.Debug("GetEmployeesForSubstitution. TOP filter applied.");
        }
        else
          Logger.Debug("GetEmployeesForSubstitution. Non-TOP filter applied.");
        
        var sortedEmployees = new List<DirRX.OverrideSolution.IEmployee>();
        
        sortedEmployees.AddRange(filteredEmployees.Where(e => e.Department != null && e.Department.Id == initiator.Department.Id).OrderBy(e => e.Name).ToList());
        sortedEmployees.AddRange(filteredEmployees.Where(e => e.Department == null || (e.Department != null && e.Department.Id != initiator.Department.Id))
                                 .OrderBy(e => e.BusinessUnit?.Name).ThenBy(e => e.Department?.Name).ThenBy(e => e.Name).ToList());
        
        sortedEmployees = sortedEmployees.Skip(skipCount).Take(takeCount).ToList();
        
        Logger.DebugFormat("GetEmployeesForSubstitution. Found {0} employees after pagination.", sortedEmployees.Select(e => e.Id).Count());

        foreach (var employee in sortedEmployees)
        {
          var employeeInfo = HRPro.HRCommons.Structures.Module.SubstituteEmployeeInfo.Create();
          
          employeeInfo.Id = employee.Id;
          employeeInfo.Name = employee.Name;
          employeeInfo.BUName = employee.BusinessUnit?.Name;
          employeeInfo.DepartmentName = employee.Department?.Name;
          employeeInfo.JobTitle = employee.JobTitle?.Name;
          
          result.Add(employeeInfo);
        }
        
        Logger.DebugFormat("GetEmployeesForSubstitution. Returning {0} employees for initiatorId={1}.", result.Select(r => r.Id).Count(), initiatorId);
        
        return result;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetEmployeesForSubstitution. Error {0}.", ex);
        
        return result;
      }
    }

    /// <summary>
    /// Проверить доступность замещающего сотрудника во всех системах.
    /// </summary>
    /// <param name="substituteId">ИД замещающего сотрудника для проверки.</param>
    /// <param name="startDate">Дата начала замещения в строковом формате.</param>
    /// <param name="endDate">Дата окончания замещения в строковом формате.</param>
    /// <param name="vacationKindId">ИД вида отпуска.</param>
    /// <returns>Сообщение об ошибке или пустая строка, если замещающий доступен.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual string ValidateSubstitute(long substituteId, string startDate, string endDate, long vacationKindId)
    {
      Logger.DebugFormat("ValidateSubstitute. Start. Params: substituteId={0}, startDate={1}, endDate={2}, vacationKindId={3}.", substituteId, startDate, endDate, vacationKindId);
      
      var startDateTime = DateTime.Parse(startDate);
      var endDateTime = DateTime.Parse(endDate);
      
      var vacationKind = DirRX.OverrideSolution.VacationKinds.GetAll().Where(v => v.Id == vacationKindId).FirstOrDefault();
      if (vacationKind == null)
      {
        return DirRX.NNVacations.Resources.VacationKindNotFoundError.ToString();
      }
      
      var vacations = new List<Structures.Module.IVerificationVacationData>();
      vacations.Add(Structures.Module.VerificationVacationData.Create(vacationKind, null, startDateTime, (endDateTime - startDateTime).Days + 1, endDateTime));
      
      var statement = GetDocumentTimeCollisions(substituteId, 0, vacations);
      if (statement != null)
      {
        var periods = GetPeriodsFromCollisionDocument(statement, startDateTime, endDateTime);
        if (periods.Any())
        {
          var periodDates = periods.Select(p => string.Format("{0} - {1}", p.StartDate.Value.ToString("dd.MM.yyyy"), p.EndDate.Value.ToString("dd.MM.yyyy")));
          
          Logger.DebugFormat("ValidateSubstitute. Collision detected in RX: documentId={0}, periods={1}.", statement.Id, string.Join("; ", periodDates));
          
          return DirRX.NNVacations.Resources.SubstituteCollisionErrorFormat(string.Join(", ", periodDates));
        }
      }
      
      var substitute = DirRX.OverrideSolution.Employees.GetAll(e => e.Id == substituteId).FirstOrDefault();
      if (substitute == null)
      {
        return DirRX.NNVacations.Resources.CurrentUserNotFoundError.ToString();
      }
      
      var personnelNumber = substitute.PersonnelNumber;
      if (string.IsNullOrEmpty(personnelNumber))
      {
        return DirRX.NNVacations.Resources.CurrentUserPersonalNumberNotFoundError.ToString();
      }
      
      Logger.DebugFormat("ValidateSubstitute. Call SAP validation: personnelNumber={0}, vacationsCount={1}.", personnelNumber, vacations.Count);
      
      var result = ValidateEmployeeVacations(substituteId, personnelNumber, vacations);
      if (!string.IsNullOrEmpty(result.ErrorText))
      {
        Logger.DebugFormat("ValidateSubstitute. SAP returned error: {0}.", result.ErrorText);
        return result.ErrorText;
      }
      
      Logger.Debug("ValidateSubstitute. Completed successfully. No collisions detected.");
      
      return string.Empty;
    }

    /// <summary>
    /// Получить наименования сотрудников и систем для замещений (готовые строки).
    /// </summary>
    /// <param name="substituteIdsString">Строка со списком ИД замещающих сотрудников через точку с запятой.</param>
    /// <param name="systemIdsString">Строка со списком ИД систем через точку с запятой.</param>
    /// <returns>Структура с наименованиями сотрудников и систем.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual DirRX.NNVacations.Structures.Module.ISubstituteNamesResult GetSubstituteAndSystemNames(string substituteIdsString, string systemIdsString)
    {
      Logger.DebugFormat("GetSubstituteAndSystemNames. Start. Params: substituteIds={0}, systemIds={1}.", substituteIdsString ?? "null", systemIdsString ?? "null");
      
      var result = DirRX.NNVacations.Structures.Module.SubstituteNamesResult.Create();
      result.SubstituteNamesString = string.Empty;
      result.SystemNamesString = string.Empty;
      
      try
      {
        var substituteIds = !string.IsNullOrEmpty(substituteIdsString) ? substituteIdsString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList() : new List<long>();
        var systemIds = !string.IsNullOrEmpty(systemIdsString) ? systemIdsString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList() : new List<long>();
        
        var employeesDictionary = new Dictionary<long, string>();
        if (substituteIds.Count > 0)
        {
          var uniqueSubstituteIds = substituteIds.Distinct().ToList();
          var employees = DirRX.OverrideSolution.Employees.GetAll().Where(e => uniqueSubstituteIds.Contains(e.Id)).ToList();
          foreach (var emp in employees)
          {
            if (!employeesDictionary.ContainsKey(emp.Id))
            {
              employeesDictionary.Add(emp.Id, emp.Name);
            }
          }
        }
        
        var substituteNamesList = new List<string>();
        foreach (var subId in substituteIds)
        {
          substituteNamesList.Add(employeesDictionary.ContainsKey(subId) ? employeesDictionary[subId] : string.Empty);
        }
        
        result.SubstituteNamesString = string.Join(";", substituteNamesList);
        
        var systemsDictionary = new Dictionary<long, string>();
        if (systemIds.Count > 0)
        {
          var uniqueSystemIds = systemIds.Distinct().ToList();
          var systems = DirRX.NNOtherProcesses.SubstitutionSystems.GetAll().Where(s => uniqueSystemIds.Contains(s.Id)).ToList();
          foreach (var sys in systems)
          {
            if (!systemsDictionary.ContainsKey(sys.Id))
            {
              systemsDictionary.Add(sys.Id, sys.ShortName);
            }
          }
        }
        
        var systemNamesList = new List<string>();
        foreach (var sysId in systemIds)
        {
          systemNamesList.Add(systemsDictionary.ContainsKey(sysId) ? systemsDictionary[sysId] : string.Empty);
        }
        
        result.SystemNamesString = string.Join(";", systemNamesList);
        
        Logger.DebugFormat("GetSubstituteAndSystemNames. Completed.");
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetSubstituteAndSystemNames. Error {0}.", ex);
      }
      
      return result;
    }

    /// <summary>
    /// Получить информацию об отпуске по ИД задания.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Информация об отпуске для настройки замещений.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual DirRX.NNVacations.Structures.Module.IVacationInfoForSubstitutionSetup GetVacationInfoByAssignmentId(long assignmentId)
    {
      Logger.DebugFormat("GetVacationInfoByAssignmentId. Started for assignmentId={0}.", assignmentId);
      
      var result = DirRX.NNVacations.Structures.Module.VacationInfoForSubstitutionSetup.Create();
      
      try
      {
        var assignment = Sungero.Workflow.Assignments.GetAll(a => a.Id == assignmentId).FirstOrDefault();
        if (assignment == null)
        {
          Logger.Debug("GetVacationInfoByAssignmentId. Assignment not found.");
          return result;
        }
        
        if (DirRX.NNVacations.VacationConfirmationAssignments.Is(assignment))
        {
          var document = DirRX.NNVacations.VacationConfirmationAssignments.As(assignment).DocumentGroup.VacationNoticeDocuments.FirstOrDefault();
          if (document == null)
          {
            Logger.Debug("GetVacationInfoByAssignmentId. Document not found.");
            return result;
          }

          var notice = DirRX.OverrideSolution.VacationNoticeDocuments.As(document);
          
          result.VacationKindId = notice.VacationDataDirRX.OrderBy(d => d.Id).FirstOrDefault().VacationType.Id;
          result.Periods = notice.VacationDataDirRX.Select(d => DirRX.NNVacations.Structures.Module.VacationPeriodString
                                                           .Create(d.StartDate.Value.Date.ToString("yyyy-MM-dd"),
                                                                   d.EndDate.Value.Date.ToString("yyyy-MM-dd"))).ToList();
        }
        
        if (DirRX.OverrideSolution.ESignAssignments.Is(assignment))
        {
          var document = DirRX.OverrideSolution.ESignAssignments.As(assignment).DocumentGroup.OfficialDocuments.FirstOrDefault();
          if (document == null)
          {
            Logger.Debug("GetVacationInfoByAssignmentId. Document not found.");
            return result;
          }
          
          if (DirRX.OverrideSolution.VacationStatementDocuments.Is(document))
          {
            var statement = DirRX.OverrideSolution.VacationStatementDocuments.As(document);
            var documentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallConsentKind);
            if (documentKind != null && statement.DocumentKind != documentKind)
            {
              Logger.Debug("GetVacationInfoByAssignmentId. Document kind statement is not supported.");
              return result;
            }
            
            result.VacationKindId = statement.AdditionalVacationDirRX.OrderBy(d => d.Id).FirstOrDefault().VacationType.Id;
            result.Periods = GetShiftPeriodsFromStatement(statement)
              .Select(p => DirRX.NNVacations.Structures.Module.VacationPeriodString.Create(p.StartDate.Value.Date.ToString("yyyy-MM-dd"),
                                                                                           p.EndDate.Value.Date.ToString("yyyy-MM-dd"))).ToList();
          }
        }
        
        Logger.DebugFormat("GetVacationInfoByAssignmentId. Found {0} periods.", result.Periods.Count());
        
        return result;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetVacationInfoByAssignmentId. Error {0}.", ex);
        return result;
      }
    }

    /// <summary>
    /// Получить замещения из задачи по ИД задания.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Список структур с информацией о замещениях.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual List<DirRX.NNVacations.Structures.Module.ISubstitutionData> GetSubstitutionsForTaskFromAssignment(long assignmentId)
    {
      Logger.DebugFormat("GetSubstitutionsForTaskFromAssignment. Started for assignmentId={0}.", assignmentId);
      
      var result = new List<DirRX.NNVacations.Structures.Module.ISubstitutionData>();
      
      var assignment = Sungero.Workflow.Assignments.GetAll(a => a.Id == assignmentId).FirstOrDefault();
      if (assignment == null || assignment.Task == null)
      {
        Logger.DebugFormat("GetSubstitutionsForTaskFromAssignment. Assignment or task not found, assignmentId={0}.", assignmentId);
        return result;
      }
      
      foreach (var substitution in GetSubstitutionInfosFromTask(assignment.Task))
      {
        var item = DirRX.NNVacations.Structures.Module.SubstitutionData.Create();
        
        item.SubstituteId = substitution.Substitute.Id;
        item.SubstituteName = substitution.Substitute.Name;
        item.SystemId = substitution.SubstitutionSystem.Id;
        item.SystemName = substitution.SubstitutionSystem.ShortName;
        item.StartDate = substitution.StartDate;
        item.EndDate = substitution.EndDate;
        
        result.Add(item);
      }
      
      Logger.DebugFormat("GetSubstitutionsForTaskFromAssignment. Found {0} substitutions.", result.Count());
      
      return result;
    }

    /// <summary>
    /// Сохранить замещения в задачу.
    /// </summary>
    /// <param name="taskId">ИД задачи.</param>
    /// <param name="substituteIdsString">Cтрока со списком ИД замещающих сотрудников.</param>
    /// <param name="systemIdsString">Cтрока со списком ИД систем для замещения.</param>
    /// <param name="startDatesString">Cтрока со списком дат начала замещений.</param>
    /// <param name="endDatesString">Cтрока со списком дат окончания замещений.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual void SaveSubstitutionsToTask(long taskId, string substituteIdsString, string systemIdsString, string startDatesString, string endDatesString)
    {
      Logger.DebugFormat("SaveSubstitutionsToTask. Started for taskId={0}.", taskId);
      
      var task = Sungero.Workflow.Tasks.GetAll(t => t.Id == taskId).FirstOrDefault();
      if (task == null)
      {
        Logger.DebugFormat("SaveSubstitutionsToTask. Task {0} not found.", taskId);
        return;
      }
      
      var substituteIds = !string.IsNullOrEmpty(substituteIdsString) ? substituteIdsString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList() : new List<long>();
      var systemIds = !string.IsNullOrEmpty(systemIdsString) ? systemIdsString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList() : new List<long>();
      var startDates = !string.IsNullOrEmpty(startDatesString) ? startDatesString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(DateTime.Parse).ToList() : new List<DateTime>();
      var endDates = !string.IsNullOrEmpty(endDatesString) ? endDatesString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(DateTime.Parse).ToList() : new List<DateTime>();
      
      Logger.DebugFormat("SaveSubstitutionsToTask. Parsed counts: substituteIds {0}, systemIds {1}, startDates {2}, endDates {3}.", substituteIds.Count(), systemIds.Count(), startDates.Count(), endDates.Count());

      try
      {
        if (Locks.TryLock(task))
        {
          if (DirRX.NNVacations.RegistrationOfAnnualLeaves.Is(task))
          {
            var annualLeaveTask = DirRX.NNVacations.RegistrationOfAnnualLeaves.As(task);
            annualLeaveTask.SubstitutionsDirRX.Clear();
            
            if (substituteIds.Count() == 0)
            {
              Logger.DebugFormat("SaveSubstitutionsToTask. No substitutions to save.");
            }
            
            for (var i = 0; i < substituteIds.Count(); i++)
            {
              var item = annualLeaveTask.SubstitutionsDirRX.AddNew();
              
              item.Substitute = DirRX.OverrideSolution.Employees.GetAll(e => e.Id == substituteIds[i]).FirstOrDefault();
              item.SubstitutionSystem = DirRX.NNOtherProcesses.SubstitutionSystems.GetAll(s => s.Id == systemIds[i]).FirstOrDefault();
              item.StartDate = startDates[i];
              item.EndDate = endDates[i];
            }
            
            annualLeaveTask.Save();
            
            Logger.DebugFormat("SaveSubstitutionsToTask. Saved {0} substitutions to RegistrationOfAnnualLeaves.", substituteIds.Count());
          }
          
          if (DirRX.OverrideSolution.StatementTasks.Is(task))
          {
            var statementTask = DirRX.OverrideSolution.StatementTasks.As(task);
            statementTask.SubstitutionsDirRX.Clear();
            
            if (substituteIds.Count() == 0)
            {
              Logger.DebugFormat("SaveSubstitutionsToTask. No substitutions to save.");
            }
            
            for (var i = 0; i < substituteIds.Count(); i++)
            {
              var item = statementTask.SubstitutionsDirRX.AddNew();
              
              item.Substitute = DirRX.OverrideSolution.Employees.GetAll(e => e.Id == substituteIds[i]).FirstOrDefault();
              item.SubstitutionSystem = DirRX.NNOtherProcesses.SubstitutionSystems.GetAll(s => s.Id == systemIds[i]).FirstOrDefault();
              item.StartDate = startDates[i];
              item.EndDate = endDates[i];
            }
            
            statementTask.Save();
            
            Logger.DebugFormat("SaveSubstitutionsToTask. Saved {0} substitutions to StatementTasks.", substituteIds.Count());
          }
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(task);
          Logger.ErrorFormat("SaveSubstitutionsToTask. Cannot lock taskId={0}, locked by {1}.", taskId, lockInfo.OwnerName);
        }
        
        if (Locks.GetLockInfo(task).IsLockedByMe)
          Locks.Unlock(task);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("SaveSubstitutionsToTask. Error {0}.", ex);
        throw;
      }
    }

    /// <summary>
    /// Сохранить замещения в задачу по ИД задания.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <param name="substituteIdsString">Cтрока со списком ИД замещающих сотрудников.</param>
    /// <param name="systemIdsString">Cтрока со списком ИД систем для замещения.</param>
    /// <param name="startDatesString">Cтрока со списком дат начала замещений.</param>
    /// <param name="endDatesString">Cтрока со списком дат окончания замещений.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual void SaveSubstitutionsToTaskFromAssignment(long assignmentId, string substituteIdsString, string systemIdsString, string startDatesString, string endDatesString)
    {
      Logger.DebugFormat("SaveSubstitutionsToTaskFromAssignment. Started for assignmentId={0}.", assignmentId);
      
      var assignment = Sungero.Workflow.Assignments.GetAll(a => a.Id == assignmentId).FirstOrDefault();
      if (assignment == null || assignment.Task == null)
      {
        Logger.DebugFormat("SaveSubstitutionsToTaskFromAssignment. Assignment or task not found, assignmentId={0}.", assignmentId);
        return;
      }
      
      SaveSubstitutionsToTask(assignment.Task.Id, substituteIdsString, systemIdsString, startDatesString, endDatesString);
    }

    #endregion

    #endregion

    #region Функции для работы с отчетом "Контроль ознакомления с графиком отпусков".
    /// <summary>
    /// Получить список НОР доступных сотруднику.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Список доступных НОР.</returns>
    [Public, Remote(IsPure=true)]
    public List<DirRX.OverrideSolution.IBusinessUnit> GetAvailableBusinessUnits(DirRX.OverrideSolution.IEmployee employee)
    {
      var generalRolesList = new List<NNCommons.IGeneralRole>();
      var limitations = NNCommons.DocumentsVisabilityLimitations.GetAll().Where(x => x.Status == NNCommons.DocumentsVisabilityLimitation.Status.Active &&
                                                                                x.Employees.Any(emp => DirRX.OverrideSolution.Employees.Equals(emp.Employee, employee)));
      var vacationScheduleProcess = NNCommons.BusinessProcesses.GetAll(b => b.Name == NNVacations.Resources.BusinessProcessVacationScheduleManagement &&
                                                                       b.Status == NNCommons.BusinessProcess.Status.Active).FirstOrDefault();
      var businessRoles = NNCommons.BusinessRoles.GetAll(r => r.Status == NNCommons.BusinessRole.Status.Active &&
                                                         r.RoleType == NNCommons.BusinessRole.RoleType.ByProcess &&
                                                         Equals(r.Process, vacationScheduleProcess)).ToList();
      
      foreach (var role in businessRoles)
      {
        var generalRoleMembers = role.RecipientLinks.Select(r => NNCommons.GeneralRoles.As(r.Member)).ToList();
        generalRolesList.AddRange(generalRoleMembers);
      }
      var businessUnitsIds = limitations.Where(l => generalRolesList.Contains(l.Role)).Select(l => l.BusinessUnit.Id).Distinct();
      
      return DirRX.OverrideSolution.BusinessUnits.GetAll(b => businessUnitsIds.Contains(b.Id)).ToList();
    }
    #endregion
    
    #region Функции для работы с личным кабинетом через сервис интеграции.
    
    #region Виджет "Мои отпуска".
    
    /// <summary>
    /// Получить информацию по отпускам сотрудника для отображения в "Мои отпуска" в формате json для ЛК.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Детальная информация по отпускам сотрудника в формате json. Работает с учетом совмещения.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string GetJsonVacationFullInfoByPerson(long personId)
    {
      var person = People.GetAll(x => x.Id == personId).FirstOrDefault();
      if (person == null)
        return "[]";
      return NNVacations.PublicFunctions.Module.GetCachedDataForMyVacations(person);
    }
    
    /// <summary>
    /// Получить информацию по отпускам сотрудника в формате списка структур.
    /// </summary>
    /// <param name="limits">Доступные лимиты.</param>
    /// <returns>Детальная информация по отпускам сотрудника в формате списка структур.</returns>
    public virtual List<DirRX.NNVacations.Structures.Module.IEmployeeVacationData> GetVacationDataByPerson(List<Structures.Module.IFormattedVacationAvailableLimits> limits)
    {
      var vacationDataList = new List<DirRX.NNVacations.Structures.Module.IEmployeeVacationData>();
      foreach (var limit in limits)
      {
        var vacationData = DirRX.NNVacations.Structures.Module.EmployeeVacationData.Create();
        
        // Если хотя бы один тип отпуска оплачен, ставим признак «оплачено» в карточке документа.
        vacationData.Paid = limit.Vacations.Any(x => x.Paid);
        
        if (limits.Count > 1)
        {
          var employee = Employees.GetAll().FirstOrDefault(x => x.Id == limit.employeeId);
          vacationData.EmployeeJobTitile = employee.JobTitle?.Name;
          vacationData.EmployeeDepartment = employee.Department?.Name;
          vacationData.EmployeeBusinessUnit = employee.BusinessUnit?.Name;
        }
        
        if (!string.IsNullOrEmpty(limit.ErrorText))
        {
          vacationData.ErrorText = DirRX.NNVacations.Resources.ErrorOccurred.ToString();
          vacationDataList.Add(vacationData);
          continue;
        }
        
        vacationData.VacationStatusDetail = new List<DirRX.NNVacations.Structures.Module.IVacationStatusDetail>();
        
        #region Право на отпуск
        
        var availableVacations = CreateAvailableVacationStatusDetail(
          DirRX.NNVacations.Resources.AvailableNumberOfVacationDaysTitle,
          limit.Vacations,
          DirRX.NNVacations.Resources.GetAvailableNumberOfVacationDaysError);
        
        vacationData.VacationStatusDetail.Add(availableVacations);
        
        #endregion
        
        // Получаем списки типов
        var plannedVacationsTypes = new List<string>();
        var registeredVacationsTypes = new List<string>();
        
        var plannedVacationsConstantValue = NNCommons.PublicFunctions.Module.GetStringValue(NNVacations.Constants.Module.ConstantsSetting.PlannedVacationsCodes.Guid);
        var registeredVacationsConstantValue = NNCommons.PublicFunctions.Module.GetStringValue(NNVacations.Constants.Module.ConstantsSetting.RegisteredVacationsCodes.Guid);
        
        if (string.IsNullOrEmpty(plannedVacationsConstantValue) || string.IsNullOrEmpty(registeredVacationsConstantValue))
        {
          vacationData.ErrorText = DirRX.NNVacations.Resources.ErrorOccurred.ToString();
          Logger.ErrorFormat("GetVacationDataByPerson. Не заполнены прикладные константы для типов отпусков: {0}, {1}.",
                             NNVacations.Constants.Module.ConstantsSetting.PlannedVacationsCodes.Name,
                             NNVacations.Constants.Module.ConstantsSetting.RegisteredVacationsCodes.Name);
          vacationDataList.Add(vacationData);
          continue;
        }
        
        plannedVacationsTypes = plannedVacationsConstantValue
          .Split(',')
          .Select(x => x.Trim())
          .ToList();

        registeredVacationsTypes = registeredVacationsConstantValue
          .Split(',')
          .Select(x => x.Trim())
          .ToList();

        // Запланированные отпуска.
        var plannedVacations = CreateVacationStatusDetail(
          plannedVacationsTypes,
          DirRX.NNVacations.Resources.PlannedVacationTitle,
          limit.Vacations,
          HRPro.Vacations.Resources.NoDataDefaultText);

        // Предоставленные отпуска.
        var registeredVacations = CreateVacationStatusDetail(
          registeredVacationsTypes,
          DirRX.NNVacations.Resources.RegisteredVacationTitle,
          limit.Vacations,
          HRPro.Vacations.Resources.NoDataDefaultText);

        // Добавляем в данные.
        vacationData.VacationStatusDetail.Add(plannedVacations);
        vacationData.VacationStatusDetail.Add(registeredVacations);
        vacationDataList.Add(vacationData);
      }
      return vacationDataList;
    }
    
    /// <summary>
    /// Создает структуру VacationStatusDetail для указанных типов отпусков.
    /// </summary>
    /// <param name="vacationTypes">Список кодов типов отпусков.</param>
    /// <param name="vacationStatus">Заголовок статуса отпуска.</param>
    /// <param name="vacations">Список отпусков для обработки.</param>
    /// <param name="noDataText">Текст при отсутствии данных.</param>
    /// <returns>Заполненная структура VacationStatusDetail.</returns>
    public static DirRX.NNVacations.Structures.Module.IVacationStatusDetail CreateVacationStatusDetail(
      List<string> vacationTypes,
      string vacationStatus,
      List<Structures.Module.IFormattedVacationData> vacations,
      string noDataText)
    {
      // Создаем базовую структуру для хранения информации
      var statusDetail = DirRX.NNVacations.Structures.Module.VacationStatusDetail.Create();
      
      // Инициализируем список групп деталей по отпускам и создаём одну общую группу.
      statusDetail.VacationGroupDetail = new List<DirRX.NNVacations.Structures.Module.IVacationGroupDetail>();
      var groupDetail = DirRX.NNVacations.Structures.Module.VacationGroupDetail.Create();
      
      // Инициализируем список для хранения деталей по каждому типу отпуска
      groupDetail.VacationDetail = new List<DirRX.NNVacations.Structures.Module.IVacationDataDetail>();
      
      // Устанавливаем заголовок категории отпусков
      statusDetail.VacationStatus = vacationStatus;

      // Проверяем, есть ли отпуска для обработки
      if (vacations != null && vacations.Any())
      {
        // Группируем отпуска по типу и фильтруем по заданным кодам типов
        // Исключаем типы, содержащие слово "vacation" в нижнем регистре
        foreach (var typeGroup in vacations
                 .Where(x => !x.Type.ToLower().Contains(Constants.Module.vacationLimitPrefix) && vacationTypes.Contains(x.Type))
                 .GroupBy(x => x.Type))
        {
          // Создаем детализацию для текущего типа отпуска
          var vacationDetail = DirRX.NNVacations.Structures.Module.VacationDataDetail.Create();
          var type = typeGroup.FirstOrDefault()?.Type;
          
          // Получаем читаемое название типа отпуска из справочника VacationKinds.
          // При поиске записи справочника по полю "Фактическое отсутствие", ограничивается список записями, где в поле "Вид отпуска" указан "Ежегодный отпуск".
          vacationDetail.VacationKind = VacationKinds.GetAll(v => v.Status == OverrideSolution.VacationKind.Status.Active && (
            v.PlannedAbsenceDirRX == type ||
            v.PlannedAbsenceShiftDirRX == type ||
            (v.SAPIdDirRX == type && v.VacationKindDirRX == OverrideSolution.VacationKind.VacationKindDirRX.AnnualLeave)))
            .FirstOrDefault()?.Name;
          
          // Формируем список периодов отпуска в текстовом формате
          vacationDetail.Dates = new List<string>();
          foreach (var period in typeGroup)
          {
            // Форматируем период отпуска по шаблону "дд.мм.гггг - дд.мм.гггг (N дн.)"
            vacationDetail.Dates.Add(string.Format(
              DirRX.NNVacations.Resources.PeriodOfPlannedVacation,
              period.DateStart.ToString("dd.MM.yyyy"),  // Дата начала
              period.DateEnd.ToString("dd.MM.yyyy"),    // Дата окончания
              period.DaysCount));                       // Количество дней
          }
          
          // Добавляем детализацию по типу отпуска в общий список
          groupDetail.VacationDetail.Add(vacationDetail);
        }
        
        if (groupDetail.VacationDetail.Any())
          statusDetail.VacationGroupDetail.Add(groupDetail);
      }
      else
        // Устанавливаем текст по умолчанию, если отпусков нет
        statusDetail.NoDataText = noDataText;

      // Возвращаем заполненную структуру с данными об отпусках
      return statusDetail;
    }

    
    /// <summary>
    /// Создает структуру VacationStatusDetail для доступных дней по отпускам.
    /// </summary>
    /// <param name="vacationStatus">Заголовок статуса отпуска (лимитов).</param>
    /// <param name="vacations">Список отпусков для обработки.</param>
    /// <param name="noDataText">Текст при отсутствии данных.</param>
    /// <returns>Заполненная структура VacationStatusDetail.</returns>
    public static DirRX.NNVacations.Structures.Module.IVacationStatusDetail CreateAvailableVacationStatusDetail(
      string vacationStatus,
      List<DirRX.NNVacations.Structures.Module.IFormattedVacationData> vacations,
      string noDataText)
    {
      // Создаем базовую структуру для хранения информации.
      var statusDetail = DirRX.NNVacations.Structures.Module.VacationStatusDetail.Create();
      statusDetail.VacationStatus = vacationStatus;
      statusDetail.VacationGroupDetail = new List<DirRX.NNVacations.Structures.Module.IVacationGroupDetail>();

      // Проверяем, есть ли отпуска для обработки.
      if (vacations != null && vacations.Any())
      {
        // Фильтруем отпуска по типам с префиксом "Vacation", обрабатывая тем самым только лимиты.
        // Также группируем по имени группы.
        foreach (var limitsGroup in vacations
                 .Where(x => x.Type.ToLower().Contains(Constants.Module.vacationLimitPrefix))
                 .GroupBy(x => x.Group))
        {
          // Создаем группу.
          var vacationGroup = DirRX.NNVacations.Structures.Module.VacationGroupDetail.Create();
          vacationGroup.VacationDetail = new List<DirRX.NNVacations.Structures.Module.IVacationDataDetail>();
          vacationGroup.Group = limitsGroup.Key;
          vacationGroup.Priority = limitsGroup.FirstOrDefault().GroupPriority;
          
          foreach (var limitsType in limitsGroup.GroupBy(g => g.Name))
          {
            // Создаем детализацию для текущей группы.
            var vacationDetail = DirRX.NNVacations.Structures.Module.VacationDataDetail.Create();
            vacationDetail.VacationKind = limitsType.Key;
            vacationDetail.Dates = new List<string>();
            
            var availableDays = limitsType.Select(x => x.DaysCount).Sum();
            vacationDetail.NumberOfDays = string.Format(DirRX.NNVacations.Resources.NumberOfDays, availableDays);

            var priorityStr = limitsType.FirstOrDefault().Type.Replace(Constants.Module.vacationLimitPrefix, string.Empty);
            var priority = 0;
            int.TryParse(priorityStr, out priority);
            vacationDetail.Priority = priority;
            
            vacationGroup.VacationDetail.Add(vacationDetail);
          }
          
          if (vacationGroup.VacationDetail.Any())
          {
            vacationGroup.VacationDetail = vacationGroup.VacationDetail.OrderBy(d => d.Priority).ToList();
            statusDetail.VacationGroupDetail.Add(vacationGroup);
          }
        }
        
        statusDetail.VacationGroupDetail = statusDetail.VacationGroupDetail.OrderBy(s => s.Priority).ToList();
      }
      else
        statusDetail.NoDataText = noDataText;

      // Возвращаем заполненную структуру с данными об отпусках
      return statusDetail;
    }
    
    /// <summary>
    /// Обновить кешированные данные для виджета "Мои отпуска".
    /// </summary>
    /// <param name="limits">Доступные лимиты.</param>
    /// <param name="personId">ИД персоны.</param>
    /// <returns>Структура по отпускам.</returns>
    public virtual Structures.Module.IEmployeeMyVacation PrepareDataForMyVacations(List<Structures.Module.IFormattedVacationAvailableLimits> limits, long personId)
    {
      var logPrefix = string.Format("PrepareDataForMyVacations. ИД персоны: {0}", personId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      
      var myVacations = DirRX.NNVacations.Structures.Module.EmployeeMyVacation.Create();
      var vacationDataList = new List<DirRX.NNVacations.Structures.Module.IEmployeeVacationByYearData>();
      
      var currentYear = DirRX.NNVacations.Structures.Module.EmployeeVacationByYearData.Create();
      currentYear.EmployeeVacationData = this.GetVacationDataByPerson(limits);
      
      vacationDataList.Add(currentYear);
      
      myVacations.PersonId = personId;
      myVacations.EmployeeVacationByYearData = vacationDataList;
      
      myVacations.UpdateTime = string.Format("Дата последнего обновления: {0}", Calendar.Now.ToString("g"));
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return myVacations;
    }
    
    /// <summary>
    /// Обновить кешированные данные для виджета "Мои отпуска".
    /// </summary>
    /// <param name="person">Персона.</param>
    /// <returns>Структура по отпускам.</returns>
    public virtual Structures.Module.IEmployeeMyVacation UpdateCachedDataForMyVacations(IPerson person)
    {
      var logPrefix = string.Format("UpdateCachedDataForMyVacations. ИД персоны: {0}", person.Id);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      
      var key = string.Format(Constants.Module.MyVacationsCacheKey, person.Id);
      var myVacations = Structures.Module.EmployeeMyVacation.Create();
      
      var limits = new List<Structures.Module.IFormattedVacationAvailableLimits>();
      var employeeIds = HRPro.HRDocFlow.PublicFunctions.Module.Remote.GetPartTimeEmployeeIds(person.Id);
      foreach (var employeeId in employeeIds)
      {
        var limit = this.GetVacationAvailableLimits(employeeId);
        if (!string.IsNullOrEmpty(limit.ErrorText))
          Logger.ErrorFormat("{0}. ИД сотрудника: {1}. Ошибка получения доступных лимитов отпуска. Текст ошибки: {2}", logPrefix, employeeId, limit.ErrorText);
        limit.employeeId = employeeId;
        limits.Add(limit);
      }
      
      myVacations = this.PrepareDataForMyVacations(limits, person.Id);
      Cache.AddOrUpdate(key, myVacations, Calendar.Today.AddDays(1));
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return myVacations;
    }
    
    /// <summary>
    /// Получить кешированные данные для виджета "Мои отпуска".
    /// Обновить данные при отсутствии кешированных.
    /// </summary>
    /// <param name="personId">ИД Персоны.</param>
    /// <returns>Структура по отпускам.</returns>
    [Public]
    public virtual string GetCachedDataForMyVacations(IPerson person)
    {
      var logPrefix = string.Format("GetCachedDataForMyVacations. ИД персоны: {0}", person.Id);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      
      var key = string.Format(Constants.Module.MyVacationsCacheKey, person.Id);
      DirRX.NNVacations.Structures.Module.IEmployeeMyVacation cachedData;
      // Получить данные для виджета из кэша, обновить кэш сотрудника.
      if (!Cache.TryGetValue(key, out cachedData))
        cachedData = this.UpdateCachedDataForMyVacations(person);
      
      var result = IsolatedFunctions.Json.SerializeInJson(cachedData);
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return result;
    }
    
    /// <summary>
    /// Обновить данные для виджета "Мои отпуска".
    /// </summary>
    /// <param name="personId">ИД Персоны.</param>
    /// <returns>True, если успешно обновлено.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string UpdateWidgetMyVacations(long personId)
    {
      var logPrefix = string.Format("UpdateWidgetMyVacations. ИД персоны: {0}", personId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      
      try
      {
        var person = People.GetAll().First(x => x.Id == personId);
        this.UpdateCachedDataForMyVacations(person);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка обновления данных: {1}. StackTrace: {2}", logPrefix, ex.Message, ex.StackTrace);
        return DirRX.NNVacations.Resources.ErrorOccurred.ToString();
      }
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return string.Empty;
    }
    
    #endregion
    
    #region Проверки отпуска при оформлении заявления.
    
    /// <summary>
    /// Получить документ с коллизией временных данных запрашиваемого отпуска.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="vacations">Список отпусков.</param>
    /// <returns>Документ совпадающий датами отпусков.</returns>
    public IVacationStatementDocument GetDocumentTimeCollisions(long employeeId, long? documentId, List<Structures.Module.IVerificationVacationData> vacations)
    {
      var logPrefix = string.Format("GetDocumentTimeCollisions. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      
      var onApproval = OverrideSolution.VacationStatementDocument.InternalApprovalState.OnApproval;
      var onRework = OverrideSolution.VacationStatementDocument.InternalApprovalState.OnRework;
      var annualKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationStatementKind);
      var otherKind = DocKindFunctions.GetNativeDocumentKind(DirRX.NNVacations.PublicConstants.Module.DocumentKind.OtherVacationKind);
      var shiftKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationShiftStatementKind);
      var selfRecallKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationSelfRecallStatementKind);
      var recallKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallConsentKind);
      // Виды документов с заполняемой таблицей по отпускам.
      var additionalVacationKinds = new List<Sungero.Docflow.IDocumentKind>() { annualKind, shiftKind, selfRecallKind, recallKind };
      // Получить заявления на отпуска по сотруднику, которые на согласовании или на доработке. Таких документов немного, поэтому можно ToList().
      var statements = VacationStatementDocuments.GetAll(x => x.Employee != null && x.Employee.Id == employeeId && x.Id != documentId &&
                                                         (x.InternalApprovalState == onApproval || x.InternalApprovalState == onRework)).ToList();
      
      var statement = statements.FirstOrDefault(x => ((Equals(x.DocumentKind, otherKind) || Equals(x.DocumentKind, annualKind)) && vacations.Any(y => x.StartDateDirRX <= y.EndDate && x.EndDateDirRX >= y.BeginDate)) ||
                                                (additionalVacationKinds.Contains(x.DocumentKind) && x.AdditionalVacationDirRX.Any(y => vacations.Any(v => y.StartDate <= v.EndDate && y.EndDate >= v.BeginDate))));
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return statement;
    }
    
    /// <summary>
    /// Проверить, доступно ли сотруднику оформление отпуска авансом для выбранных отпусков.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <param name="vacationTypes">Список типов отпусков.</param>
    /// <returns>True, если отпуск авансом доступен.</returns>
    public bool IsAdvanceAvailable(IBusinessUnit businessUnit, List<IVacationKind> vacationTypes)
    {
      return vacationTypes.All(v => v.BusinessUnitsDirRX.Any(x => BusinessUnits.Equals(x.BusinessUnit, businessUnit)));
    }
    
    /// <summary>
    /// Проверить, что до планируемого отпуска осталось N дней.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="vacations">Список отпусков.</param>
    /// <param name="isPaid">Отпуск оплачен.</param>
    /// <returns>Структура с ответными данными.</returns>
    public Structures.Module.IValidateVacationResult CheckDaysBeforeStart(IEmployee employee, List<Structures.Module.IVerificationVacationData> vacations, bool isPaid = false)
    {
      var result = Structures.Module.ValidateVacationResult.Create();
      // Если сотрудник входит в топ10 или топ100, то проверки нет.
      var evaluationGroup = JobTitles.As(employee.JobTitle)?.EvaluationGroupDirRX;
      var employeeIsTop = evaluationGroup == OverrideSolution.JobTitle.EvaluationGroupDirRX.Top10 || evaluationGroup == OverrideSolution.JobTitle.EvaluationGroupDirRX.Top100;
      var now = Calendar.Now.BeginningOfDay();
      var daysBeforeStartEnable = 0;
      foreach (var vacation in vacations)
      {
        if (vacation.VacationType?.RelevantForTOPDirRX == false && employeeIsTop)
          daysBeforeStartEnable = 0;
        else
          daysBeforeStartEnable = isPaid ? (vacation.VacationType?.PaidVacationDeadlineDirRX ?? 0) :
            ((vacation.VacationReason?.DaysBeforeStartEnable ?? vacation.VacationType?.DaysBeforeStartEnableDirRX) ?? 0);
        if (vacation.BeginDate < now.AddWorkingDays(employee, daysBeforeStartEnable))
        {
          result.ErrorText = Resources.DaysBeforeStartEnableErrorFormat(daysBeforeStartEnable);
          return result;
        }
      }
      return result;
    }
    
    /// <summary>
    /// Получить критичные ошибки по отпускам.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="documentKindId">ИД вида документа.</param>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="vacationTypeIds">ИД типов дополнительных отпусков.</param>
    /// <param name="beginDates">Даты начала дополнительных отпусков.</param>
    /// <param name="endDates">Даты окончания дополнительных отпусков.</param>
    /// <param name="durations">Продолжительности дополнительных отпусков.</param>
    /// <returns>Json с результатом проверки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string ValidateVacationData(long employeeId, long documentKindId, long? documentId, List<string> vacationTypeIds, List<string> beginDates, List<string> endDates, List<string> durations)
    {
      var logPrefix = string.Format("ValidateVacationData. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create(0, string.Empty, string.Empty);
      
      #region Проверка заполненности полей, подготовка данных.
      
      var errorMessages = new List<string>();
      var personnelNumber = string.Empty;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      if (employee == null)
      {
        errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserNotFoundError.ToString());
      }
      else
      {
        personnelNumber = employee.PersonnelNumber;
        if (string.IsNullOrEmpty(personnelNumber))
          errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserPersonalNumberNotFoundError.ToString());
      }
      var docKind = DocumentKinds.GetAll(x => x.Id == documentKindId).FirstOrDefault();
      if (docKind == null)
        errorMessages.Add(DirRX.NNVacations.Resources.DocumentKindNotFoundError);
      if (errorMessages.Any())
      {
        result.ErrorText = string.Join(Constants.Module.ListIdsDelimiter.ToString(), errorMessages);
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      var annualVacationKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationStatementKind);
      var isAnnualVacation = DocumentKinds.Equals(docKind, annualVacationKind);
      var vacations = new List<Structures.Module.IVerificationVacationData>();
      for (var i = 0; i < beginDates.Count; i++)
      {
        var vacation = Structures.Module.VerificationVacationData.Create();
        var vacationTypeId = Int32.Parse(vacationTypeIds[i]);
        vacation.VacationType = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault();
        vacation.BeginDate = DateTime.Parse(beginDates[i]);
        vacation.EndDate = DateTime.Parse(endDates[i]);
        vacation.Duration = Int32.Parse(durations[i]);
        vacations.Add(vacation);
      }
      
      var fullDurations = vacations.Sum(x => x.Duration);
      
      #endregion
      
      // Проверка коллизий временных данных в RX.
      var documentCollision = this.GetDocumentTimeCollisions(employeeId, documentId, vacations);
      if (documentCollision != null)
      {
        result.ErrorText = Resources.DocumentTimeCollisionError;
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      // Проверка что до планируемого отпуска осталось N дней. Если сотрудник входит в топ10 или топ100, то проверки нет.
      result = this.CheckDaysBeforeStart(employee, vacations);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка коллизий временных данных в SAP и получение признака "Отсутствие отпуска в 14 дней.
      result = this.ValidateEmployeeVacations(employeeId, personnelNumber, vacations);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка доступного лимита для ежегодного отпуска.
      // Если есть превышение дней отпуска, проверяем доступен ли отпуск авансом.
      if (result.VacationDaysExcess > 0)
      {
        var vacationTypeList = vacations.Where(v => v.VacationType != null).Select(x => x.VacationType).ToList();
        if (!this.IsAdvanceAvailable(BusinessUnits.As(employee.BusinessUnit), vacationTypeList))
        {
          result.ErrorText = Resources.VacationAvailableLimitsError.ToString();
          return IsolatedFunctions.Json.SerializeInJson(result);
        }
      }
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить критичные ошибки по прочим отпускам.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="vacationTypeId">ИД типа отпуска.</param>
    /// <param name="vacationReasonId">ИД причины отпуска.</param>
    /// <param name="beginDate">Дата начала.</param>
    /// <param name="endDate">Дата окончания.</param>
    /// <param name="duration">Продолжительность отпуска.</param>
    /// <returns>Json с результатом проверки.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string ValidateOtherVacationData(long employeeId, long? documentId, long vacationTypeId, long? vacationReasonId, string beginDate, string endDate, int duration)
    {
      var logPrefix = string.Format("ValidateOtherVacationData. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create(0, string.Empty, string.Empty);
      
      #region Проверка заполненности полей, подготовка данных.
      
      var errorMessages = new List<string>();
      var personnelNumber = string.Empty;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var dateBegin = NNIntegration.PublicFunctions.Module.ParseStringToDate(beginDate);
      var dateEnd = NNIntegration.PublicFunctions.Module.ParseStringToDate(endDate);
      if (employee == null)
        errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserNotFoundError.ToString());
      else if (string.IsNullOrEmpty(employee.PersonnelNumber))
        errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserPersonalNumberNotFoundError.ToString());
      else
        personnelNumber = employee.PersonnelNumber;
      if (!dateBegin.HasValue)
        errorMessages.Add(DirRX.NNVacations.Resources.ParseStringToDateErrorFormat(beginDate));
      if (!dateEnd.HasValue)
        errorMessages.Add(DirRX.NNVacations.Resources.ParseStringToDateErrorFormat(endDate));
      
      if (errorMessages.Any())
      {
        result.ErrorText = string.Join(Constants.Module.ListIdsDelimiter.ToString(), errorMessages);
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      var vacations = new List<Structures.Module.IVerificationVacationData>();
      var vacation = Structures.Module.VerificationVacationData.Create();
      vacation.VacationType = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault();
      vacation.VacationReason = VacationReasonses.GetAll(v => v.Id == vacationReasonId).FirstOrDefault();
      vacation.BeginDate = dateBegin.Value;
      vacation.EndDate = dateEnd.Value;
      vacation.Duration = duration;
      vacations.Add(vacation);
      
      #endregion
      
      // Проверка коллизий временных данных в RX.
      var documentCollision = this.GetDocumentTimeCollisions(employeeId, documentId, vacations);
      if (documentCollision != null)
      {
        result.ErrorText = Resources.DocumentTimeCollisionError;
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      // Проверка что до планируемого отпуска осталось N дней. Если сотрудник входит в топ10 или топ100, то проверки нет.
      result = this.CheckDaysBeforeStart(employee, vacations);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка данных в SAP.
      result = this.ValidateEmployeeOtherVacations(employeeId, personnelNumber, vacationTypeId, vacationReasonId, vacation.BeginDate, vacation.EndDate, duration);
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить критичные ошибки по отпускам для переноса.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="noticeId">ИД уведомления о начале отпуска.</param>
    /// <param name="vacationTypeIds">ИД типов новых отпусков.</param>
    /// <param name="beginDates">Даты начала новых отпусков.</param>
    /// <param name="endDates">Даты окончания новых отпусков.</param>
    /// <param name="durations">Продолжительности новых отпусков.</param>
    /// <param name="oldVacationsJson">Json с информацией по старому отпуску.</param>
    /// <param name="assignmentId">ИД задания на подтверждение дат отпуска.</param>
    /// <returns>Json с результатом проверки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string ValidateShiftVacationData(long employeeId, long? documentId, long noticeId,
                                            List<string> vacationTypeIds, List<string> beginDates, List<string> endDates, List<string> durations,
                                            string oldVacationsJson, long? assignmentId)
    {
      var logPrefix = string.Format("ValidateShiftVacationData. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create(0, string.Empty, string.Empty);
      
      // Проверка состояния задания на подтверждение дат отпуска.
      var assignment = VacationConfirmationAssignments.GetAll(a => a.Id == assignmentId).FirstOrDefault();
      if (assignment != null && assignment.Status != NNVacations.VacationConfirmationAssignment.Status.InProcess)
      {
        result.ErrorText = Resources.VacationConfirmationAssignmentError;
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      #region Проверка заполненности полей, подготовка данных.
      
      var errorMessages = new List<string>();
      var personnelNumber = string.Empty;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      if (employee == null)
        errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserNotFoundError.ToString());
      else if (string.IsNullOrEmpty(employee.PersonnelNumber))
        errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserPersonalNumberNotFoundError.ToString());
      else
        personnelNumber = employee.PersonnelNumber;
      
      if (errorMessages.Any())
      {
        result.ErrorText = string.Join(Constants.Module.ListIdsDelimiter.ToString(), errorMessages);
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      var oldVacations = IsolatedFunctions.Json.DeserializeInStruct(oldVacationsJson, Structures.Module.FormattedVacationForShiftByStatement.Create());
      var oldDateStart = oldVacations.OrderBy(v => v.DateStart).First().DateStart;
      
      var vacations = new List<Structures.Module.IVerificationVacationData>();
      for (var i = 0; i < beginDates.Count; i++)
      {
        var vacation = Structures.Module.VerificationVacationData.Create();
        var vacationTypeId = Int32.Parse(vacationTypeIds[i]);
        vacation.VacationType = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault();
        vacation.BeginDate = DateTime.Parse(beginDates[i]);
        vacation.EndDate = DateTime.Parse(endDates[i]);
        vacation.Duration = Int32.Parse(durations[i]);
        vacations.Add(vacation);
      }
      
      #endregion
      
      // Проверка коллизий временных данных в RX.
      var documentCollision = this.GetDocumentTimeCollisions(employeeId, documentId, vacations);
      if (documentCollision != null)
      {
        result.ErrorText = Resources.DocumentTimeCollisionInShiftError;
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      // Проверка что до планируемого отпуска осталось N дней. Если сотрудник входит в топ10 или топ100, то проверки нет.
      result = this.CheckDaysBeforeStart(employee, vacations);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка переноса только в пределах календарного года.
      result = CheckYearShiftRestriction(oldDateStart, vacations);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка того, что нельзя перенести только дни на проезд без дней ежегодного отпуска.
      result = CheckTravelDaysRestriction(vacations);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка количества дней для ежегодного отпуска и других типов.
      result = CheckVacationDurationsRestriction(vacations, oldVacations, employee);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка данных в SAP.
      result = this.ValidateEmployeeShiftVacations(employeeId, personnelNumber, noticeId, vacations, new List<Structures.Module.IFormattedVacationForShiftByStatement>());
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Проверить то, что отпуска переносятся только в пределах календарного года
    /// (для отпусков в ноябре-декабре - в пределах следующего года или того же года).
    /// </summary>
    /// <param name="originalStartDate">Дата начала первоначального отпуска.</param>
    /// <param name="newVacations">Данные об отпусках.</param>
    /// <returns>Результат.</returns>
    public virtual Structures.Module.IValidateVacationResult CheckYearShiftRestriction(DateTime originalStartDate, List<Structures.Module.IVerificationVacationData> newVacations)
    {
      var result = Structures.Module.ValidateVacationResult.Create(0, string.Empty, string.Empty);
      var originalYear = originalStartDate.Year;
      var isNovemberDecember = originalStartDate.Month >= 11;
      
      foreach (var vacation in newVacations)
      {
        if (vacation == null)
          continue;
        
        // Для отпусков в ноябре-декабре разрешаем перенос как в следующий год, так и в текущий
        if (isNovemberDecember)
        {
          if (vacation.BeginDate.Year != originalYear && vacation.BeginDate.Year != originalYear + 1)
          {
            result.ErrorText = DirRX.NNVacations.Resources.ValidateVacationDataYearShiftError;
            return result;
          }
        }
        else
        {
          // Для отпусков в других месяцах разрешаем перенос только в пределах текущего года
          if (vacation.BeginDate.Year != originalYear)
          {
            result.ErrorText = DirRX.NNVacations.Resources.ValidateVacationDataYearShiftError;
            return result;
          }
        }
      }
      
      return result;
    }
    
    /// <summary>
    /// Проверить то, что нельзя перенести только дни на проезд без дней ежегодного отпуска.
    /// </summary>
    /// <param name="vacations">Данные об отпусках.</param>
    /// <returns>Результат.</returns>
    public virtual Structures.Module.IValidateVacationResult CheckTravelDaysRestriction(List<Structures.Module.IVerificationVacationData> vacations)
    {
      var result = Structures.Module.ValidateVacationResult.Create(0, string.Empty, string.Empty);
      
      // Получить отпуска, которые можно перенести только как присоединенные к ежегодному оплачиваемому отпуску.
      var attachedVacations = vacations.Where(x => x.VacationType?.CanBeTransferredSeparatelyDirRX == false);
      
      // Если таких отпусков нет, то проверка пройдена.
      if (!attachedVacations.Any())
        return result;
      
      // Определить коды ежегодного отпуска.
      var annualVacationsConstantValue = NNCommons.PublicFunctions.Module.GetStringValue(NNVacations.Constants.Module.ConstantsSetting.AnnualVacationsCodes.Guid);
      if (string.IsNullOrEmpty(annualVacationsConstantValue))
      {
        result.ErrorText = DirRX.NNVacations.Resources.ErrorOccurred.ToString();
        Logger.ErrorFormat("CheckTravelDaysRestriction. Не заполнена прикладная константа: {0}.", NNVacations.Constants.Module.ConstantsSetting.AnnualVacationsCodes.Name);
        return result;
      }
      
      var annualVacationsTypes = new List<string> ();
      try
      {
        annualVacationsTypes = annualVacationsConstantValue
          .Split(',')
          .Select(x => x.Trim())
          .ToList();
      }
      catch (Exception ex)
      {
        result.ErrorText = DirRX.NNVacations.Resources.ErrorOccurred.ToString();
        Logger.ErrorFormat("CheckTravelDaysRestriction. Ошибка при получении списка кодов для ежгодного отпуска из константы. Текст ошибки: {0}. StackTrace: {1}.",
                           ex.Message, ex.StackTrace);
        return result;
      }
      
      // Получить ежегодные отпуски.
      var annualVacations = vacations
        .Where(x => annualVacationsTypes.Contains(x.VacationType.PlannedAbsenceDirRX) ||
               annualVacationsTypes.Contains(x.VacationType.PlannedAbsenceShiftDirRX) ||
               annualVacationsTypes.Contains(x.VacationType.SAPIdDirRX))
        .OrderBy(x => x.BeginDate)
        .ToList();
      
      // Проверить для каждого присоединенного отпуска:
      foreach (var attachedVacation in attachedVacations)
      {
        // Найти ближайший ежегодный отпуск, который идет перед текущим присоединенным.
        var precedingAnnual = annualVacations
          .Where(a => a.EndDate.Date.AddDays(1) == attachedVacation.BeginDate.Date)
          .FirstOrDefault();
        
        // Если не найден подходящий ежегодный отпуск (нет предшествующего без разрыва).
        if (precedingAnnual == null)
        {
          result.ErrorText = DirRX.NNVacations.Resources.ValidateVacationAttachedToAnnualError;
          return result;
        }
      }

      return result;
    }

    /// <summary>
    /// Проверить количество дней для ежегодного отпуска и других типов.
    /// </summary>
    /// <param name="vacations">Данные об отпусках.</param>
    /// <param name="oldVacations">Данные первоначального отпуска.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Результат.</returns>
    public virtual Structures.Module.IValidateVacationResult CheckVacationDurationsRestriction(List<Structures.Module.IVerificationVacationData> vacations,
                                                                                               List<Structures.Module.IFormattedVacationForShiftByStatement> oldVacations,
                                                                                               IEmployee employee)
    {
      var result = Structures.Module.ValidateVacationResult.Create(0, string.Empty, string.Empty);
      
      // Проверить, только для типов отпусков, у которых признак Необходимо перенести полностью - Да.
      var oldVacationTypeIds = oldVacations.Select(x => x.VacationTypeId);
      var oldVacationTypes = VacationKinds.GetAll(x => oldVacationTypeIds.Contains(x.Id) && x.NeedsTransferredCompletelyDirRX == true);
      var newVacations = vacations.Where(x => x.VacationType.NeedsTransferredCompletelyDirRX == true);
      if (oldVacationTypes.Any() && !newVacations.Any())
      {
        result.ErrorText = DirRX.NNVacations.Resources.ValidateVacationDurationRestrictionError;
        return result;
      }
      
      foreach (var vacation in newVacations.GroupBy(x => x.VacationType))
      {
        var vacationType = vacation.First().VacationType;
        var oldDuration = oldVacations.Where(x => x.VacationTypeId == vacationType.Id).Sum(x => x.DaysCount);
        var newDuration = vacation.Sum(x => x.Duration);
        
        // Для ежегодного отпуска количество дней должно совпадать.
        if (newDuration != oldDuration)
        {
          result.ErrorText = DirRX.NNVacations.Resources.ValidateVacationDurationRestrictionError;
          return result;
        }
      }
      
      return result;
    }

    /// <summary>
    /// Проверить количество дней для ежегодного отпуска и других типов (для отзыва).
    /// </summary>
    /// <param name="vacations">Данные об отпусках.</param>
    /// <param name="oldVacations">Данные первоначального отпуска.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="daysCompensation">Количество дней компенсации.</param>
    /// <returns>Результат.</returns>
    public virtual Structures.Module.IValidateVacationResult CheckVacationDurationsRestriction(List<Structures.Module.IVerificationVacationData> vacations,
                                                                                               List<Structures.Module.IVerificationVacationData> oldVacations,
                                                                                               IEmployee employee, int daysCompensation)
    {
      var result = Structures.Module.ValidateVacationResult.Create(0, string.Empty, string.Empty);
      
      // Проверить, только для типов отпусков, у которых признак Необходимо перенести полностью - Да.
      var currentVacations = oldVacations.Where(x => x.VacationType.NeedsTransferredCompletelyDirRX == true);
      var currentVacationsWithoutCompensated = currentVacations.Where(x => x.VacationType.IsCompensatedDirRX != true);
      var newVacations = vacations.Where(x => x.VacationType.NeedsTransferredCompletelyDirRX == true);
      // Если нет переноса необходимых текущих отпусков и не выбрана компенсация или есть некомпенсируемый период, то возвращается ошибка.
      if (currentVacations.Any() && !newVacations.Any() && (daysCompensation == 0 || currentVacationsWithoutCompensated.Any()))
      {
        result.ErrorText = DirRX.NNVacations.Resources.ValidateRecallVacationDurationRestrictionError;
        return result;
      }
      
      foreach (var vacation in newVacations.GroupBy(x => x.VacationType))
      {
        var vacationType = vacation.First().VacationType;
        if (vacationType.IsCompensatedDirRX == true && daysCompensation > 0)
          continue;
        
        var oldDuration = oldVacations.Where(x => x.VacationType.Id == vacationType.Id).Sum(x => x.Duration);
        var newDuration = vacation.Sum(x => x.Duration);
        
        // Для ежегодного отпуска количество дней должно совпадать.
        if (newDuration != oldDuration)
        {
          result.ErrorText = DirRX.NNVacations.Resources.ValidateRecallVacationDurationRestrictionError;
          return result;
        }
      }
      
      return result;
    }
    
    /// <summary>
    /// Получить критичные ошибки по отпускам для переноса по заявлению.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="noticeId">ИД уведомления о начале отпуска.</param>
    /// <param name="vacationTypeIds">ИД типов новых отпусков.</param>
    /// <param name="beginDates">Даты начала новых отпусков.</param>
    /// <param name="endDates">Даты окончания новых отпусков.</param>
    /// <param name="durations">Продолжительности новых отпусков.</param>
    /// <param name="oldVacationsJson">Json с информацией по старому отпуску.</param>
    /// <param name="isPaid">Отпуск оплачен.</param>
    /// <returns>Json с результатом проверки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string ValidateShiftByStatementVacationData(long employeeId, long? documentId,
                                                       List<string> vacationTypeIds, List<string> beginDates, List<string> endDates, List<string> durations,
                                                       string oldVacationsJson, bool isPaid)
    {
      var logPrefix = string.Format("ValidateShiftByStatementVacationData. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create(0, string.Empty, string.Empty);
      
      #region Проверка заполненности полей, подготовка данных.
      
      var errorMessages = new List<string>();
      var personnelNumber = string.Empty;
      var pendingSign = OverrideSolution.VacationNoticeDocument.InternalApprovalState.PendingSign;
      var onApproval = OverrideSolution.VacationStatementDocument.InternalApprovalState.OnApproval;
      var onRework = OverrideSolution.VacationStatementDocument.InternalApprovalState.OnRework;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      if (employee == null)
        errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserNotFoundError.ToString());
      else if (string.IsNullOrEmpty(employee.PersonnelNumber))
        errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserPersonalNumberNotFoundError.ToString());
      else
        personnelNumber = employee.PersonnelNumber;
      
      if (errorMessages.Any())
      {
        result.ErrorText = string.Join(Constants.Module.ListIdsDelimiter.ToString(), errorMessages);
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      var oldVacations = IsolatedFunctions.Json.DeserializeInStruct(oldVacationsJson, Structures.Module.FormattedVacationForShiftByStatement.Create());
      var oldDateStart = oldVacations.OrderBy(v => v.DateStart).First().DateStart;
      
      var vacations = new List<Structures.Module.IVerificationVacationData>();
      for (var i = 0; i < beginDates.Count; i++)
      {
        var vacation = Structures.Module.VerificationVacationData.Create();
        var vacationTypeId = Int32.Parse(vacationTypeIds[i]);
        vacation.VacationType = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault();
        vacation.BeginDate = DateTime.Parse(beginDates[i]);
        vacation.EndDate = DateTime.Parse(endDates[i]);
        vacation.Duration = Int32.Parse(durations[i]);
        vacations.Add(vacation);
      }
      
      #endregion
      
      // Проверка на отсутствие уведомлений в работе.
      Logger.DebugFormat("{0}. Проверка на отсутствие уведомлений об отпуске.", logPrefix);
      var notices = VacationNoticeDocuments.GetAll(x => Equals(employee, x.Employee) && x.InternalApprovalState == pendingSign).ToList();
      var hasNotice = notices.Any(x => x.VacationDataDirRX.Count == oldVacations.Count &&
                                  x.VacationDataDirRX.All(y => oldVacations.Any(v => y.VacationType.Id == v.VacationTypeId &&
                                                                                y.StartDate == v.DateStart &&
                                                                                y.EndDate == v.DateEnd &&
                                                                                y.Days == v.DaysCount)));
      if (hasNotice)
      {
        result.ErrorText = Resources.VacationNoticeInProcessError;
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      // Проверка на отсутствие заявлений на перенос одного и того же периода отпуска.
      Logger.DebugFormat("{0}. Проверка на отсутствие заявлений на перенос одного и того же периода отпуска.", logPrefix);
      var statementsDuplicate = VacationStatementDocuments.GetAll(x => Equals(employee, x.Employee) && x.Id != documentId &&
                                                                  (x.InternalApprovalState == onApproval || x.InternalApprovalState == onRework)).ToList();
      var hasStatementDuplicate = statementsDuplicate.Any(x => x.VacationDataDirRX.Count == oldVacations.Count &&
                                                          x.VacationDataDirRX.All(y => oldVacations.Any(v => y.VacationType.Id == v.VacationTypeId &&
                                                                                                        y.StartDate == v.DateStart &&
                                                                                                        y.EndDate == v.DateEnd &&
                                                                                                        y.Days == v.DaysCount)));
      if (hasStatementDuplicate)
      {
        result.ErrorText = Resources.ShiftVacationStatementDuplicateError;
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      // Проверка коллизий временных данных в RX.
      var documentCollision = this.GetDocumentTimeCollisions(employeeId, documentId, vacations);
      if (documentCollision != null)
      {
        result.ErrorText = Resources.DocumentTimeCollisionInShiftError;
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      // Проверка что до планируемого отпуска осталось N дней. Если сотрудник входит в топ10 или топ100, то проверки нет.
      result = this.CheckDaysBeforeStart(employee, vacations, isPaid);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка переноса только в пределах календарного года.
      result = CheckYearShiftRestriction(oldDateStart, vacations);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка того, что нельзя перенести только дни на проезд без дней ежегодного отпуска.
      result = CheckTravelDaysRestriction(vacations);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка количества дней для ежегодного отпуска и других типов.
      result = CheckVacationDurationsRestriction(vacations, oldVacations, employee);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return IsolatedFunctions.Json.SerializeInJson(result);
      
      // Проверка данных в SAP.
      result = this.ValidateEmployeeShiftVacations(employeeId, personnelNumber, 0, vacations, oldVacations);
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить критичные ошибки по отпускам для отзыва.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="beginRecall">Дата начала отзыва.</param>
    /// <param name="endRecall">Дата окончания отзыва.</param>
    /// <param name="daysRecall">Количество дней отзыва.</param>
    /// <param name="beginShift">Дата начала переноса.</param>
    /// <param name="endShift">Дата окончания переноса.</param>
    /// <param name="daysShift">Количество дней переноса.</param>
    /// <param name="daysCompensation">Количество дней компенсации.</param>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    /// <returns>Json с результатом проверки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string ValidateRecallVacationData(long employeeId, long? documentId,
                                             string beginRecall, string endRecall, int daysRecall,
                                             string beginShift, string endShift, int? daysShift,
                                             int daysCompensation, string currentVacationData)
    {
      var logPrefix = string.Format("ValidateRecallVacationData. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create(0, string.Empty, string.Empty);
      
      #region Проверка заполненности полей, подготовка данных.
      
      var errorMessages = new List<string>();
      var personnelNumber = string.Empty;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var beginDateRecall = NNIntegration.PublicFunctions.Module.ParseStringToDate(beginRecall);
      var endDateRecall = NNIntegration.PublicFunctions.Module.ParseStringToDate(endRecall);
      var beginDateShift = NNIntegration.PublicFunctions.Module.ParseStringToDate(beginShift);
      var endDateShift = NNIntegration.PublicFunctions.Module.ParseStringToDate(endShift);
      
      if (employee == null)
        errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserNotFoundError.ToString());
      else if (string.IsNullOrEmpty(employee.PersonnelNumber))
        errorMessages.Add(DirRX.NNVacations.Resources.CurrentUserPersonalNumberNotFoundError.ToString());
      else
        personnelNumber = employee.PersonnelNumber;
      if (!beginDateRecall.HasValue)
        errorMessages.Add(DirRX.NNVacations.Resources.ParseStringToDateErrorFormat(beginRecall));
      if (!endDateRecall.HasValue)
        errorMessages.Add(DirRX.NNVacations.Resources.ParseStringToDateErrorFormat(endRecall));
      
      // Данные текущего отпуска.
      var currentVacations = this.ParseCurrentVacationData(currentVacationData);
      
      // Данные прерывания/отзыва из отпуска.
      var recallVacations = new List<Structures.Module.IVerificationVacationData>();
      foreach (var vacInfo in currentVacations)
      {
        if (beginDateRecall > vacInfo.EndDate || endDateRecall < vacInfo.BeginDate)
          continue;
        
        var vacationRecall = Structures.Module.VerificationVacationData.Create();
        vacationRecall.VacationType = vacInfo.VacationType;
        vacationRecall.BeginDate = vacInfo.BeginDate <= beginDateRecall.Value ? beginDateRecall.Value : vacInfo.BeginDate;
        vacationRecall.EndDate = vacInfo.EndDate >= endDateRecall.Value ? endDateRecall.Value : vacInfo.EndDate;
        vacationRecall.Duration = this.DurationInCalendarDaysWithoutHoliday(vacationRecall.BeginDate, vacationRecall.EndDate, employee, vacationRecall.VacationType, null);
        recallVacations.Add(vacationRecall);
      }
      
      var vacationTypesRequireShiftDays = recallVacations.Where(v => v.VacationType != null && v.VacationType.RequireShiftDaysWhenRecallDirRX.HasValue &&
                                                                v.VacationType.RequireShiftDaysWhenRecallDirRX.Value == true).Select(t => t.VacationType.SAPIdDirRX);
      
      // Признак проверки переносимых дат отпуска.
      var requireShiftDaysWhenRecall = vacationTypesRequireShiftDays.Any();
      
      if ((!beginDateShift.HasValue || !endDateShift.HasValue) && requireShiftDaysWhenRecall)
      {
        var vacTypesRequireShiftDaysMessage = string.Join(Constants.Module.ListDelimiter, vacationTypesRequireShiftDays);
        errorMessages.Add(DirRX.NNVacations.Resources.EmptyShiftDaysWhenRecallErrorFormat(vacTypesRequireShiftDaysMessage));
      }
      
      if (errorMessages.Any())
      {
        result.ErrorText = string.Join(Constants.Module.ListDelimiter, errorMessages);
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      if (requireShiftDaysWhenRecall)
      {
        // Подготовка данных для проверки коллизий временных данных в RX.
        var vacations = new List<Structures.Module.IVerificationVacationData>();
        var vacation = Structures.Module.VerificationVacationData.Create();
        vacation.BeginDate = beginDateShift.Value;
        vacation.EndDate = endDateShift.Value;
        vacation.Duration = daysShift.GetValueOrDefault();
        vacations.Add(vacation);
        
        // Данные переноса отпуска.
        var shiftVacations = new List<Structures.Module.IVerificationVacationData>();
        var beginDateShiftPart = beginDateShift.Value;
        foreach (var vacInfo in recallVacations)
        {
          var vacationShift = Structures.Module.VerificationVacationData.Create();
          vacationShift.VacationType = vacInfo.VacationType;
          vacationShift.BeginDate = beginDateShiftPart;
          var endDateShiftPart = this.GetVacationEndDate(vacationShift.BeginDate, vacInfo.Duration, employee, vacInfo.VacationType, null);
          vacationShift.EndDate = endDateShiftPart >= endDateShift.Value ? endDateShift.Value : endDateShiftPart;
          vacationShift.Duration = this.DurationInCalendarDaysWithoutHoliday(vacationShift.BeginDate, vacationShift.EndDate, employee, vacationShift.VacationType, null);
          shiftVacations.Add(vacationShift);
          
          if (vacationShift.EndDate == endDateShift.Value.Date)
            break;
          beginDateShiftPart = vacationShift.EndDate.AddDays(1);
        }
        
        #endregion
        
        // Проверка коллизий временных данных в RX.
        var documentCollision = this.GetDocumentTimeCollisions(employeeId, documentId, vacations);
        if (documentCollision != null)
        {
          result.ErrorText = Resources.DocumentTimeCollisionInShiftError;
          return IsolatedFunctions.Json.SerializeInJson(result);
        }
        
        // Проверка что до планируемого отпуска осталось N дней. Если сотрудник входит в топ10 или топ100, то проверки нет.
        result = this.CheckDaysBeforeStart(employee, shiftVacations);
        if (!string.IsNullOrEmpty(result.ErrorText))
          return IsolatedFunctions.Json.SerializeInJson(result);
        
        // Проверка количества дней для ежегодного отпуска и других типов.
        result = CheckVacationDurationsRestriction(shiftVacations, recallVacations, employee, daysCompensation);
        if (!string.IsNullOrEmpty(result.ErrorText))
          return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      // Проверка данных в SAP.
      result = this.ValidateRecallVacation(employeeId, personnelNumber, beginDateRecall.Value, endDateRecall.Value, beginDateShift, endDateShift, daysCompensation);
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить текущий лимит для оформления отпуска.
    /// </summary>
    /// <param name="limits">Структура доступных лимитов.</param>
    /// <returns>Количество дней доступных для оформления.</returns>
    public int GetCurrentAvailableLimit(DirRX.NNVacations.Structures.Module.IFormattedVacationAvailableLimits limits)
    {
      return limits.Vacations.Where(x => x.Type.ToLower().Contains("vacation")).Select(x => x.DaysCount).Sum();
    }
    
    #endregion
    
    #region Функции для работы с услугой компенсацией отпуска в ЛК
    
    /// <summary>
    /// Заполнить данные в заявлении на компенсацию отпуска.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="documentKindId">ИД вида документа.</param>
    /// <param name="daysSelected">Выбрано дней.</param>
    /// <param name="availableDays">Доступно дней.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillStatementDataForCompensation(string document, long documentKindId, int daysSelected, string availableDays)
    {
      var documentId = IsolatedFunctions.Json.GetDocumentIdFromLK(document);
      var logPrefix = string.Format("FillStatementDataForCompensation. ИД документа: {0}. ИД вида документа: {1}. Выбрано дней: {2}. Доступно дней: {3}",
                                    documentId, documentKindId, daysSelected, availableDays);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      var statement = VacationStatementDocuments.GetAll(d => d.Id == documentId).FirstOrDefault();
      statement.ApplicationDateDirRX = Calendar.Today;
      statement.DocumentKind = DocumentKinds.GetAll(d => d.Id == documentKindId).FirstOrDefault();
      statement.SelectedDaysDirRX = daysSelected;
      statement.AvailableDaysDirRX = availableDays;
      
      try
      {
        statement.Save();
        Logger.DebugFormat("{0}. Документ успешно сохранен.", logPrefix);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Не удалось сохранить документ. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Получить данные для доработки заявления на компенсацию.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Json с данными по компенсации.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetStatementDataForReworkCompensation(long assignmentId)
    {
      var logPrefix = string.Format("GetStatementDataForReworkCompensation. ИД задания: {0}", assignmentId);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      var assignment = NNCommons.StatementReworkAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      var docRelated = assignment.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var statement = VacationStatementDocuments.As(docRelated);
      if (statement == null)
      {
        Logger.ErrorFormat("{0}. Документ не найден.", logPrefix);
        return string.Empty;
      }
      
      var employee = statement.EmployeeDirRX;
      var result = Structures.Module.StatementDataForReworkCompensation.Create();
      result.InitiatorId = employee.Id;
      result.StatementId = statement.Id;
      result.DocumentKind = statement.DocumentKind.Id;
      result.DaysSelected = statement.SelectedDaysDirRX.Value;
      result.PerformerInfo = string.Join(Environment.NewLine, employee.BusinessUnit, employee.Department, employee.JobTitle);
      
      var resultJson = IsolatedFunctions.Json.SerializeInJson(result);
      Logger.DebugFormat("{0}. Конец выполнения.", logPrefix);
      return resultJson;
    }
    
    /// <summary>
    /// Проверить заявление на компенсацию части отпуска.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="daysSelected">Кол-во дней.</param>
    [Public(WebApiRequestType = RequestType.Get)]
    public string ValidateVacationCompensation(long employeeId, int daysSelected)
    {
      var logPrefix = string.Format("ValidateVacationCompensation. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      
      var result = Structures.Module.ValidateVacationResult.Create();
      var errorMessage = string.Empty;
      var personnelNumber = string.Empty;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      if (employee == null)
        errorMessage = DirRX.NNVacations.Resources.CurrentUserNotFoundError;
      else if (string.IsNullOrEmpty(employee.PersonnelNumber))
        errorMessage = DirRX.NNVacations.Resources.CurrentUserPersonalNumberNotFoundError;
      else
        personnelNumber = employee.PersonnelNumber;
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        result.ErrorText = errorMessage;
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      // Проверка доступных дней выполняется на стороне ЛК, остается проверить заявление в АСУП.
      result = this.ValidateVacationCompensation(personnelNumber, daysSelected);
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить текущий лимит для оформления компенсации части отпуска.
    /// </summary>
    /// <param name="employeeId">Сотрудник.</param>
    /// <returns>Json доступных лимитов по компенсации отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetCurrentCompensationAvailableDays(long employeeId)
    {
      var structure = this.GetCompensationAvailableDays(employeeId);
      var daysOnApproving = this.GetCompensationDaysOnApproving(employeeId);
      structure.DayCount -= daysOnApproving;
      structure.DaysOnApproving = daysOnApproving;
      var delimiter = Constants.Module.ListIdsDelimiter.ToString();
      structure.DayCountString = structure.DayCountString?.Replace(delimiter, delimiter + Constants.Module.DelimiterNewLineForLK); // кол-во дней строкой без учёта дней на согласовании.
      return IsolatedFunctions.Json.SerializeInJson(structure);
    }
    
    /// <summary>
    /// Получить виды документов для компенсации.
    /// </summary>
    /// <returns>Json с видами.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetCompensationDocumentKinds()
    {
      // Первый вид документа будет выбран по умолчанию в услуге.
      var compensationKind = DocKindFunctions.GetNativeDocumentKind(DirRX.NNVacations.PublicConstants.Module.DocumentKind.CompensationVacationKind);
      // На стороне ЛК считаем что второй вид это "Компенсация части ежегодного дополнительного оплачиваемого отпуска (за ВУТ/АУТ)"
      var compensationAdditionalKind = DocKindFunctions.GetNativeDocumentKind(DirRX.NNVacations.PublicConstants.Module.DocumentKind.CompensationAdditionalVacationKind);
      var docKinds = new List<Sungero.Docflow.IDocumentKind>(){ compensationKind, compensationAdditionalKind };
      var result = new List<NNVacations.Structures.Module.IDocumentKind>();
      foreach (var docKind in docKinds.Where(x => x != null && x.Status == OverrideSolution.DocumentKind.Status.Active))
      {
        var row = NNVacations.Structures.Module.DocumentKind.Create();
        row.Id = docKind.Id;
        row.Name = docKind.Name;
        result.Add(row);
      }
      
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить ИД шаблона "Заявление на компенсацию части отпуска".
    /// </summary>
    /// <param name="docKindId">ИД вида документа.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>ИД шаблона.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string GetCompensationVacationTemplateId(long docKindId, long employeeId)
    {
      var logPrefix = string.Format("GetCompensationVacationTemplateId. ИД вида документа: {0}. ИД сотрудника: {1}.", docKindId, employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      
      var errorText = new List<string>();
      var result = NNVacations.Structures.Module.VacationStatementData.Create(0, 0, string.Empty);
      var docKind = DocumentKinds.GetAll().FirstOrDefault(x => x.Id == docKindId);
      if (docKind == null)
        errorText.Add(DirRX.NNVacations.Resources.DocumentKindNotFound);
      var employee = Employees.GetAll(e => e.Id == employeeId).FirstOrDefault();
      if (employee == null)
      {
        errorText.Add(DirRX.NNVacations.Resources.EmployeeNotFound);
        result.Error = string.Join(Constants.Module.ListDelimiter, errorText);
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      var businessUnit = employee.BusinessUnit;
      if (businessUnit == null)
        errorText.Add(DirRX.NNVacations.Resources.BusinessUnitNotFound);
      
      if (!errorText.Any())
      {
        var settingTemplates = HRPro.HRProSolution.DocumentKinds.As(docKind).TemplatesLKToBusinessUnitHRPro;
        var template = settingTemplates.FirstOrDefault(t => Equals(t.BusinessUnit, businessUnit))?.TemplateLK;
        if (template == null)
          template = settingTemplates.FirstOrDefault(t => t.BusinessUnit == null)?.TemplateLK;
        if (template != null)
        {
          result.TemplateId = template.Id;
          Logger.DebugFormat("{0}. Найден подходящий шаблон с ИД: {0}.", logPrefix, template.Id);
          return IsolatedFunctions.Json.SerializeInJson(result);
        }
        else
        {
          errorText.Add(DirRX.NNVacations.Resources.VacationKindTemplateNotFoundErrorFormat(docKind.Name));
        }
      }
      
      result.Error = string.Join(Constants.Module.ListDelimiter, errorText);
      Logger.DebugFormat("{0}. Не найден подходящий шаблон по причине: {0}", logPrefix, result.Error);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    #endregion
    
    #region Функции для работы с услугами отзыва из отпуска в ЛК
    
    /// <summary>
    /// Получить временный шаблон с заполненной таблицей.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="templateId">ИД шаблона.</param>
    /// <param name="templateWithTableName">Имя временного шаблона.</param>
    /// <param name="beginRecall">Дата начала прерывания отпуска.</param>
    /// <param name="endRecall">Дата окончания прерывания отпуска.</param>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    /// <returns>ИД временного шаблона с заполненной таблицей.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public long GetTemplateRecallWithTableId(long employeeId, long templateId, string templateWithTableName, string beginRecall, string endRecall, string currentVacationData)
    {
      var logPrefix = string.Format("GetTemplateRecallWithTableId. ИД сотрудника: {0}. ИД шаблона: {1}.", employeeId, templateId);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      
      var beginDateRecall = DateTime.Parse(beginRecall).Date;
      var endDateRecall = DateTime.Parse(endRecall).Date;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var template = Sungero.Docflow.DocumentTemplates.GetAll(t => t.Id == templateId).FirstOrDefault();
      var templateWithTable = Sungero.Docflow.SimpleDocuments.CreateFrom(template);
      templateWithTable.Name = templateWithTableName;
      templateWithTable.Save();
      
      var recallVacationsTable = NNCommons.Structures.Module.TableData.Create();
      recallVacationsTable.NumberKey = TableDataKeys.OldVacations;
      recallVacationsTable.Values = new List<Dictionary<string, string>>();
      var currentVacations = this.ParseCurrentVacationData(currentVacationData);
      foreach (var vacation in currentVacations)
      {
        if (beginDateRecall > vacation.EndDate || endDateRecall < vacation.BeginDate)
          continue;
        
        var vacationType = vacation.VacationType;
        var beginDate = vacation.BeginDate <= beginDateRecall ? beginDateRecall : vacation.BeginDate;
        var endDate = vacation.EndDate >= endDateRecall ? endDateRecall : vacation.EndDate;
        var duration = this.DurationInCalendarDaysWithoutHoliday(beginDate, endDate, employee, vacationType, null);
        
        var tableLineDictionary = new Dictionary<string, string>();
        tableLineDictionary.Add(TableDataKeys.VacationTypeName, vacationType.Name);
        tableLineDictionary.Add(TableDataKeys.DaysAmount, duration.ToString());
        tableLineDictionary.Add(TableDataKeys.BeginDate, beginDate.ToShortDateString());
        tableLineDictionary.Add(TableDataKeys.EndDate, endDate.ToShortDateString());
        recallVacationsTable.Values.Add(tableLineDictionary);
      }
      
      try
      {
        using (var document = templateWithTable.LastVersion.Body.Read())
        {
          using (var templateStream = NNCommons.IsolatedFunctions.WordInsertTable.WordInsertTableInTag(logPrefix, document, recallVacationsTable))
          {
            templateWithTable.LastVersion.Body.Write(templateStream);
            templateWithTable.Save();
          }
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Заполнить таблицу отпусков. Ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      HRPro.HRCommons.PublicFunctions.Module.Remote.EnqueueDocumentDeleteJob(Sungero.Docflow.OfficialDocuments.As(templateWithTable));
      
      Logger.DebugFormat("{0}. ИД временного шаблона: {1}. Конец выполнения.", logPrefix, templateWithTable.Id);
      return templateWithTable.Id;
    }
    
    /// <summary>
    /// Получить список ИД и имен сотрудников линейного руководителя.
    /// </summary>
    /// <param name="lineManagerId">ИД линейного руководителя.</param>
    /// <returns>Список ИД и имен сотрудников линейного руководителя.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetEmployeesFromLineManager(long lineManagerId)
    {
      var employeesInfo = new List<Structures.Module.IEmployeeInfo>();
      var lineManager = Employees.GetAll(emp => emp.Id == lineManagerId).FirstOrDefault();
      if (lineManager != null)
      {
        var employees = Employees.GetAll().Where(emp => emp.Status == OverrideSolution.Employee.Status.Active &&
                                                 Equals(emp.LineManagerDirRX, lineManager) &&
                                                 emp.ConsentHRPro == OverrideSolution.Employee.ConsentHRPro.Signed);
        foreach (var employee in employees)
        {
          var employeeInfo = Structures.Module.EmployeeInfo.Create();
          employeeInfo.Id = employee.Id;
          employeeInfo.Name = employee.Name;
          employeesInfo.Add(employeeInfo);
        }
      }
      return IsolatedFunctions.Json.SerializeInJson(employeesInfo);
    }
    
    /// <summary>
    /// Получить наименования должности, подразделения и НОР сотрудника.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Строка с наименованиями должности, подразделения и НОР сотрудника.</returns>
    /// <remarks>Копирование коробочной функции c изменением формата возвращаемой строки.</remarks>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetSubstitutePositionAndJobTitle(long employeeId)
    {
      var employee = Employees.GetAll(emp => emp.Id == employeeId).FirstOrDefault();
      if (employee != null)
      {
        var businessUnit = employee.Department?.BusinessUnit != null ? employee.Department.BusinessUnit.Name : string.Empty;
        var department = employee.Department != null ? employee.Department.Name : string.Empty;
        var jobTitle = employee.JobTitle != null ? employee.JobTitle.Name : string.Empty;
        return string.Format("{1}{0}{2}{0}{3}", Constants.Module.DelimiterNewLineForLK, jobTitle, department, businessUnit);
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Проверить, что у персоны есть сотрудник, который входит в роль "Линейные руководители".
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <returns>True если хоть один из сотрудников персоны входит в роль "Линейные руководители", иначе false.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public bool IsLineManager(long personId)
    {
      var employees = Employees.GetAll().Where(emp => emp.Status == OverrideSolution.Employee.Status.Active && emp.Person.Id == personId);
      foreach (var employee in employees)
        if (employee.IncludedIn(NNCommons.PublicConstants.Module.RolesGuid.LineManager))
          return true;
      return false;
    }
    
    /// <summary>
    /// Заполнить данные об отпуске в заявлении на отзыв из отпуска.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="beginDateCurrent">Дата начала текущего отпуска.</param>
    /// <param name="endDateCurrent">Дата окончания текущего отпуска.</param>
    /// <param name="durationCurrent">Продолжительность текущего отпуска.</param>
    /// <param name="beginDateRecall">Дата начала прерывания отпуска.</param>
    /// <param name="endDateRecall">Дата окончания прерывания отпуска.</param>
    /// <param name="durationRecall">Продолжительность прерывания отпуска.</param>
    /// <param name="beginDateShift">Дата начала переноса отпуска.</param>
    /// <param name="endDateShift">Дата окончания переноса отпуска.</param>
    /// <param name="durationShift">Продолжительность переноса отпуска.</param>
    /// <param name="compensationKind">Вид компенсации.</param>
    /// <param name="dayCountCompensation">Количество дней для компенсации.</param>
    /// <param name="reason">Причина прерывания.</param>
    /// <param name="isVacationPeriodBroken">Отсутствие отпуска в 14 дней.</param>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillVacationDataForRecall(string document,
                                          DateTime? beginDateCurrent, DateTime? endDateCurrent, int? durationCurrent,
                                          string beginRecall, string endRecall, int? durationRecall,
                                          string beginShift, string endShift, int? durationShift,
                                          int? compensationKind, int? dayCountCompensation, string reason, string isVacationPeriodBroken, string currentVacationData)
    {
      var documentId = IsolatedFunctions.Json.GetDocumentIdFromLK(document);
      beginDateCurrent = beginDateCurrent?.ToUserTime();
      endDateCurrent = endDateCurrent?.ToUserTime();
      Logger.DebugFormat("FillVacationDataForRecall. ИД документа: {0}. ИД типа отпуска: {1}. Дата начала текущего отпуска: {2}. Дата окончания текущего отпуска: {3}. Продолжительность текущего отпуска: {4}. Начало выполнения.",
                         documentId, beginDateCurrent, endDateCurrent, durationCurrent);
      var beginDateRecall = NNIntegration.PublicFunctions.Module.ParseStringToDate(beginRecall);
      beginDateRecall = beginDateRecall.HasValue ? beginDateRecall.Value.Date : beginDateRecall;
      
      var endDateRecall = NNIntegration.PublicFunctions.Module.ParseStringToDate(endRecall);
      endDateRecall = endDateRecall.HasValue ? endDateRecall.Value.Date : endDateRecall;
      
      var beginDateShift = NNIntegration.PublicFunctions.Module.ParseStringToDate(beginShift);
      beginDateShift = beginDateShift.HasValue ? beginDateShift.Value.Date : beginDateShift;
      
      var endDateShift = NNIntegration.PublicFunctions.Module.ParseStringToDate(endShift);
      endDateShift = endDateShift.HasValue ? endDateShift.Value.Date : endDateShift;
      
      var statement = VacationStatementDocuments.GetAll(d => d.Id == documentId).FirstOrDefault();
      var employee = statement.EmployeeDirRX;
      statement.ApplicationDateDirRX = Calendar.Today;
      statement.StartDateDirRX = beginDateCurrent;
      statement.EndDateDirRX = endDateCurrent;
      statement.DaysAmountDirRX = durationCurrent;
      statement.InterruptStartDateDirRX = beginDateRecall;
      statement.InterruptEndDateDirRX = endDateRecall;
      statement.InterruptDaysDirRX = durationRecall;
      statement.CompensationDaysDirRX = dayCountCompensation;
      statement.CompensationTypeDirRX = compensationKind == 1 ? OverrideSolution.VacationStatementDocument.CompensationTypeDirRX.WithPayment : OverrideSolution.VacationStatementDocument.CompensationTypeDirRX.WithoutPayment;
      statement.Note = reason;
      statement.IsVacationPeriodBrokenDirRX = isVacationPeriodBroken;
      
      var currentVacations = this.ParseCurrentVacationData(currentVacationData);
      statement.VacationTypeDirRX = currentVacations.FirstOrDefault()?.VacationType;
      
      // Данные текущего отпуска.
      if (!statement.VacationDataDirRX.Any())
      {
        foreach (var vacation in currentVacations)
        {
          var vacationCurrent = statement.VacationDataDirRX.AddNew();
          vacationCurrent.VacationType = vacation.VacationType;
          vacationCurrent.StartDate = vacation.BeginDate;
          vacationCurrent.EndDate = vacation.EndDate;
          vacationCurrent.Days = vacation.Duration;
        }
      }
      
      // Данные прерывания/отзыва из отпуска.
      statement.InterruptVacationDirRX.Clear();
      foreach (var vacation in currentVacations)
      {
        var dateStart = vacation.BeginDate;
        var dateEnd = vacation.EndDate;
        if (beginDateRecall > dateEnd || endDateRecall < dateStart)
          continue;
        
        var vacationRecall = statement.InterruptVacationDirRX.AddNew();
        vacationRecall.VacationType = vacation.VacationType;
        vacationRecall.StartDate = dateStart <= beginDateRecall ? beginDateRecall : dateStart;
        vacationRecall.EndDate = dateEnd >= endDateRecall ? endDateRecall : dateEnd;
        vacationRecall.Days = this.DurationInCalendarDaysWithoutHoliday(vacationRecall.StartDate.Value, vacationRecall.EndDate.Value, employee, vacationRecall.VacationType, null);
      }
      
      // Данные переноса отпуска
      statement.AdditionalVacationDirRX.Clear();
      if (beginDateShift.HasValue && endDateShift.HasValue)
      {
        var beginDateShiftPart = beginDateShift;
        foreach (var vacation in statement.InterruptVacationDirRX)
        {
          var vacationShift = statement.AdditionalVacationDirRX.AddNew();
          vacationShift.VacationType = vacation.VacationType;
          vacationShift.StartDate = beginDateShiftPart;
          var endDateShiftPart = this.GetVacationEndDate(vacationShift.StartDate.Value, vacation.Days.Value, employee, vacationShift.VacationType, null);
          vacationShift.EndDate = endDateShiftPart >= endDateShift ? endDateShift : endDateShiftPart;
          vacationShift.Days = this.DurationInCalendarDaysWithoutHoliday(vacationShift.StartDate.Value, vacationShift.EndDate.Value, employee, vacationShift.VacationType, null);
          
          if (vacationShift.EndDate == endDateShift)
            break;
          beginDateShiftPart = vacationShift.EndDate.Value.AddDays(1);
        }
      }
      
      
      try
      {
        statement.Save();
        Logger.DebugFormat("FillVacationDataForRecall. ИД документа: {0}. Документ успешно сохранен.", documentId);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("FillVacationDataForRecall. ИД документа: {0}. Не удалось сохранить документ. Текст ошибки: {1}. StackTrace: {2}.", documentId, ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Подсчет количества дней для компенсации.
    /// </summary>
    /// <param name="beginRecall">Дата начала прерывания отпуска.</param>
    /// <param name="endRecall">Дата окончания прерывания отпуска.</param>
    /// <param name="durationRecall">Продолжительность переноса отпуска.</param>
    /// <param name="durationShift">Продолжительность прерывания отпуска.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    /// <returns>Количество дней для компенсации.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public int GetDayCountCompensation(string beginRecall, string endRecall, int durationRecall, int durationShift, long employeeId, string currentVacationData)
    {
      var daysCompensation = 0;
      var beginDateRecall = NNIntegration.PublicFunctions.Module.ParseStringToDate(beginRecall);
      var endDateRecall = NNIntegration.PublicFunctions.Module.ParseStringToDate(endRecall);
      if (!beginDateRecall.HasValue || !endDateRecall.HasValue || durationRecall <= durationShift)
        return daysCompensation;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var currentVacations = this.ParseCurrentVacationData(currentVacationData);
      var beginDateCompensation = beginDateRecall.Value;
      var holidays = this.GetHolidaysInPeriodRecall(beginDateRecall, endDateRecall, employee, currentVacations);
      if (!holidays.Any())
      {
        beginDateCompensation = beginDateCompensation.AddDays(durationShift);
      }
      else
      {
        var daysCounted = 0;
        while (daysCounted < durationShift)
        {
          beginDateCompensation = beginDateCompensation.AddDays(1);
          if (!holidays.Contains(beginDateCompensation))
            daysCounted++;
        }
      }
      for (DateTime dateCompensation = beginDateCompensation; dateCompensation <= endDateRecall; dateCompensation = dateCompensation.AddDays(1))
      {
        var vacationType = currentVacations.Where(x => x.BeginDate <= dateCompensation && dateCompensation <= x.EndDate).FirstOrDefault()?.VacationType;
        if (vacationType?.IsCompensatedDirRX == true && !holidays.Contains(dateCompensation))
          daysCompensation++;
      }
      return daysCompensation;
    }
    
    /// <summary>
    /// Преобразовать данные текущего отпуска в список отпусков.
    /// </summary>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    /// <returns>Список текущих отпусков.</returns>
    private List<Structures.Module.IVerificationVacationData> ParseCurrentVacationData(string currentVacationData)
    {
      var vacations = new List<Structures.Module.IVerificationVacationData>();
      try
      {
        var currentVacations = IsolatedFunctions.Json.DeserializeInStruct(currentVacationData, Structures.Module.VacationInfoIFS10392.Create());
        foreach (var vacation in currentVacations)
        {
          var vacInfo = Structures.Module.VerificationVacationData.Create();
          vacInfo.VacationType = VacationKinds.GetAll(v => v.SAPIdDirRX == vacation.Type && v.VacationKindDirRX == OverrideSolution.VacationKind.VacationKindDirRX.RecallVacation).FirstOrDefault();
          vacInfo.BeginDate = DateTime.Parse(vacation.CurrentDateStart);
          vacInfo.EndDate = DateTime.Parse(vacation.CurrentDateEnd);
          vacInfo.Duration = vacation.DayCount;
          vacations.Add(vacInfo);
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("ParseCurrentVacationData. Данные текущего отпуска: {0}. Не удалось преобразовать данные текущего отпуска в список отпусков. Текст ошибки: {1}. StackTrace: {2}.", currentVacationData, ex.Message, ex.StackTrace);
      }
      return vacations.OrderBy(v => v.BeginDate).ToList();
    }
    
    /// <summary>
    /// Подсчет календарных дней в промежутке времени отзыва за вычетом праздничных дней.
    /// </summary>
    /// <param name="startDate">Начало.</param>
    /// <param name="endDate">Окончание.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    /// <returns>Количество календарных дней.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public int RecallDurationInCalendarDaysWithoutHoliday(DateTime startDate, DateTime endDate, long employeeId, string currentVacationData)
    {
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var currentVacations = this.ParseCurrentVacationData(currentVacationData);
      var holidaysCount = this.GetHolidaysInPeriodRecall(startDate, endDate, employee, currentVacations).Count;
      return (int)((TimeSpan)(endDate - startDate)).TotalDays + 1 - holidaysCount;
    }
    
    /// <summary>
    /// Проверить, что период отзыва содержит праздничные дни.
    /// </summary>
    /// <param name="periodBegin">Начало.</param>
    /// <param name="periodEnd">Окончание.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    /// <returns>Текст сообщения о наличии праздничных дней в периоде отзыва из отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string CheckVacationHasHolidayRecall(string periodBegin, string periodEnd, long employeeId, string currentVacationData)
    {
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var currentVacations = this.ParseCurrentVacationData(currentVacationData);
      var beginDate = NNIntegration.PublicFunctions.Module.ParseStringToDate(periodBegin);
      var endDate = NNIntegration.PublicFunctions.Module.ParseStringToDate(periodEnd);
      if (beginDate.HasValue && endDate.HasValue && employee != null)
      {
        var holidays = this.GetHolidaysInPeriodRecall(beginDate, endDate, employee, currentVacations);
        if (holidays.Any())
        {
          var dates = string.Join(", ", holidays.Select(d => d.ToShortDateString()));
          var fullDuration = (int)((TimeSpan)(endDate - beginDate)).TotalDays + 1;
          return HRPro.Vacations.Resources.VacationHasHolidayFormat(dates, fullDuration);
        }
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Проверить, что прерываемый период отпуска требует переноса.
    /// </summary>
    /// <param name="periodBegin">Начало.</param>
    /// <param name="periodEnd">Окончание.</param>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    /// <returns>Текст сообщения о требуемом переносе отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string CheckRequireShiftDaysWhenRecall(string periodBegin, string periodEnd, string currentVacationData)
    {
      var currentVacations = this.ParseCurrentVacationData(currentVacationData);
      var beginDate = NNIntegration.PublicFunctions.Module.ParseStringToDate(periodBegin);
      var endDate = NNIntegration.PublicFunctions.Module.ParseStringToDate(periodEnd);
      if (beginDate.HasValue && endDate.HasValue)
      {
        foreach (var vacInfo in currentVacations)
        {
          if (beginDate > vacInfo.EndDate || endDate < vacInfo.BeginDate)
            continue;

          if (vacInfo.VacationType != null && vacInfo.VacationType.RequireShiftDaysWhenRecallDirRX.HasValue && vacInfo.VacationType.RequireShiftDaysWhenRecallDirRX.Value == true)
            return DirRX.NNVacations.Resources.RequiredShiftDaysWhenRecall;
        }
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Вернуть список праздничных дней в периоде отзыва.
    /// </summary>
    /// <param name="periodBegin">Начало периода.</param>
    /// <param name="periodEnd">Конец периода.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <param name="currentVacations">Данные текущего отпуска.</param>
    /// <returns>Даты праздничных дней.</returns>
    [Public]
    public List<DateTime> GetHolidaysInPeriodRecall(DateTime? periodBegin, DateTime? periodEnd, IRecipient recipient, List<Structures.Module.IVerificationVacationData> currentVacations)
    {
      var result = new List<DateTime>();
      var holidaysList = HRPro.Vacations.PublicFunctions.Module.Remote.GetHolidaysInPeriod(periodBegin, periodEnd, recipient);
      foreach (var holiday in holidaysList)
      {
        var vacationType = currentVacations.Where(x => x.BeginDate <= holiday && holiday <= x.EndDate).FirstOrDefault()?.VacationType;
        if (vacationType?.CalculationDaysDirRX != OverrideSolution.VacationKind.CalculationDaysDirRX.WithoutHoliday)
          result.Add(holiday);
      }
      return result;
    }
    
    /// <summary>
    /// Подсчет календарных дней в промежутке времени переноса из отзыва за вычетом праздничных дней.
    /// </summary>
    /// <param name="beginDateRecall">Начало периода отзыва.</param>
    /// <param name="endDateRecall">Конец периода отзыва.</param>
    /// <param name="beginDateShift">Начало периода переноса.</param>
    /// <param name="endDateShift">Конец периода переноса.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    /// <returns>Количество календарных дней.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public int ShiftDurationInCalendarDaysWithoutHoliday(DateTime beginDateRecall, DateTime endDateRecall, DateTime beginDateShift, DateTime endDateShift,
                                                         long employeeId, string currentVacationData)
    {
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var currentVacations = this.ParseCurrentVacationData(currentVacationData);
      var holidaysCount = this.GetHolidaysInPeriodShift(beginDateRecall, endDateRecall, beginDateShift, endDateShift, employee, currentVacations).Count;
      return (int)((TimeSpan)(endDateShift - beginDateShift)).TotalDays + 1 - holidaysCount;
    }
    
    /// <summary>
    /// Проверить, что период переноса из отзыва содержит праздничные дни.
    /// </summary>
    /// <param name="beginRecall">Начало периода отзыва.</param>
    /// <param name="endRecall">Конец периода отзыва.</param>
    /// <param name="beginShift">Начало периода переноса.</param>
    /// <param name="endShift">Конец периода переноса.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="currentVacationData">Данные текущего отпуска.</param>
    /// <returns>Текст сообщения о наличии праздничных дней в периоде отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string CheckVacationHasHolidayShift(string beginRecall, string endRecall, string beginShift, string endShift,
                                               long employeeId, string currentVacationData)
    {
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var currentVacations = this.ParseCurrentVacationData(currentVacationData);
      var beginDateRecall = NNIntegration.PublicFunctions.Module.ParseStringToDate(beginRecall);
      var endDateRecall = NNIntegration.PublicFunctions.Module.ParseStringToDate(endRecall);
      var beginDateShift = NNIntegration.PublicFunctions.Module.ParseStringToDate(beginShift);
      var endDateShift = NNIntegration.PublicFunctions.Module.ParseStringToDate(endShift);
      if (beginDateRecall.HasValue && endDateRecall.HasValue && beginDateShift.HasValue && endDateShift.HasValue && employee != null)
      {
        var holidays = this.GetHolidaysInPeriodShift(beginDateRecall.Value, endDateRecall.Value, beginDateShift.Value, endDateShift.Value, employee, currentVacations);
        if (holidays.Any())
        {
          var dates = string.Join(", ", holidays.Select(d => d.ToShortDateString()));
          var fullDuration = (int)((TimeSpan)(endDateShift - beginDateShift)).TotalDays + 1;
          return HRPro.Vacations.Resources.VacationHasHolidayFormat(dates, fullDuration);
        }
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Вернуть список праздничных дней в периоде переноса из отзыва.
    /// </summary>
    /// <param name="beginDateRecall">Начало периода отзыва.</param>
    /// <param name="endDateRecall">Конец периода отзыва.</param>
    /// <param name="beginDateShift">Начало периода переноса.</param>
    /// <param name="endDateShift">Конец периода переноса.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="currentVacations">Данные текущего отпуска.</param>
    /// <returns>Даты праздничных дней.</returns>
    [Public]
    public List<DateTime> GetHolidaysInPeriodShift(DateTime beginDateRecall, DateTime endDateRecall, DateTime beginDateShift, DateTime endDateShift,
                                                   IEmployee employee, List<Structures.Module.IVerificationVacationData> currentVacations)
    {
      var result = new List<DateTime>();
      var holidaysList = HRPro.Vacations.PublicFunctions.Module.Remote.GetHolidaysInPeriod(beginDateShift, endDateShift, employee);
      if (!holidaysList.Any())
        return result;
      
      // Данные прерывания/отзыва из отпуска.
      var recallVacations = new List<Structures.Module.IVerificationVacationData>();
      foreach (var vacation in currentVacations)
      {
        if (beginDateRecall > vacation.EndDate || endDateRecall < vacation.BeginDate)
          continue;
        
        var vacationRecall = Structures.Module.VerificationVacationData.Create();
        vacationRecall.VacationType = vacation.VacationType;
        vacationRecall.BeginDate = vacation.BeginDate <= beginDateRecall ? beginDateRecall : vacation.BeginDate;
        vacationRecall.EndDate = vacation.EndDate >= endDateRecall ? endDateRecall : vacation.EndDate;
        vacationRecall.Duration = this.DurationInCalendarDaysWithoutHoliday(vacationRecall.BeginDate, vacationRecall.EndDate, employee, vacationRecall.VacationType, null);
        recallVacations.Add(vacationRecall);
      }
      
      // Данные переноса отпуска.
      var shiftVacations = new List<Structures.Module.IVerificationVacationData>();
      var beginDateShiftPart = beginDateShift;
      foreach (var vacation in recallVacations)
      {
        var vacationShift = Structures.Module.VerificationVacationData.Create();
        vacationShift.VacationType = vacation.VacationType;
        vacationShift.BeginDate = beginDateShiftPart;
        var endDateShiftPart = this.GetVacationEndDate(vacationShift.BeginDate, vacation.Duration, employee, vacation.VacationType, null);
        vacationShift.EndDate = endDateShiftPart >= endDateShift ? endDateShift : endDateShiftPart;
        vacationShift.Duration = this.DurationInCalendarDaysWithoutHoliday(vacationShift.BeginDate, vacationShift.EndDate, employee, vacation.VacationType, null);
        shiftVacations.Add(vacationShift);
        
        if (vacationShift.EndDate == endDateShift.Date)
          break;
        beginDateShiftPart = vacationShift.EndDate.AddDays(1);
      }
      
      foreach (var holiday in holidaysList)
      {
        var vacationType = shiftVacations.Where(x => x.BeginDate <= holiday && holiday <= x.EndDate).FirstOrDefault()?.VacationType;
        if (vacationType?.CalculationDaysDirRX != OverrideSolution.VacationKind.CalculationDaysDirRX.WithoutHoliday)
          result.Add(holiday);
      }
      
      return result;
    }
    
    /// <summary>
    /// Получить информацию о совместителях.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Строка с информацией о совместителях.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetPartTimeEmployeesInfo(long employeeId)
    {
      var employee = Employees.GetAll(emp => emp.Id == employeeId).FirstOrDefault();
      var partTimeEmployees = Employees.GetAll(emp => emp.Status == OverrideSolution.Employee.Status.Active &&
                                               emp.Person.Id == employee.Person.Id && emp.Id != employee.Id)
        .Select(emp => string.Format("{0}, {1}, {2}", emp.BusinessUnit, emp.Department, emp.JobTitle));
      if (partTimeEmployees.Any())
        return string.Join("; ", partTimeEmployees);
      return string.Empty;
    }
    
    /// <summary>
    /// Получить информацию о текущем отпуске для отзыва.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Json с данными по отпуску.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetCurrentVacationForRecall(long employeeId)
    {
      var logPrefix = string.Format("GetCurrentVacationForRecall. ИД сотрудника: {0}", employeeId);
      var result = Structures.Module.CurrentVacationDataForRecall.Create();
      var model = this.GetCurrentVacationData(employeeId);
      if (!string.IsNullOrEmpty(model.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка получения текущего отпуска сотрудника. Текст ошибки: {1}", logPrefix, model.ErrorText);
        result.ErrorText = model.ErrorText;
        return IsolatedFunctions.Json.SerializeInJson(result);
      }
      
      var currentVacations = model.CurrentVacationDataAnswer.VacationData;
      var vacations = new List<Structures.Module.IVerificationVacationData>();
      foreach (var currentVacation in currentVacations)
      {
        var vacation = Structures.Module.VerificationVacationData.Create();
        vacation.VacationType = VacationKinds.GetAll(x => x.Status == OverrideSolution.VacationKind.Status.Active &&
                                                     x.SAPIdDirRX == currentVacation.Type && x.VacationKindDirRX == OverrideSolution.VacationKind.VacationKindDirRX.RecallVacation).FirstOrDefault();
        vacation.BeginDate = DateTime.Parse(currentVacation.CurrentDateStart);
        vacation.EndDate = DateTime.Parse(currentVacation.CurrentDateEnd);
        vacation.Duration = currentVacation.DayCount;
        vacations.Add(vacation);
      }
      result.CurrentDateStart = vacations.OrderBy(v => v.BeginDate).FirstOrDefault().BeginDate;
      result.CurrentDateEnd = vacations.OrderByDescending(v => v.EndDate).FirstOrDefault().EndDate;
      result.DayCount = currentVacations.Sum(v => v.DayCount);
      var vacationPeriods = vacations.Select(x => Resources.CurrentVacationPeriodForShiftFormat(x.VacationType?.Name, x.BeginDate.ToShortDateString(), x.EndDate.ToShortDateString(), x.Duration));
      result.VacationPeriod = string.Join(Constants.Module.DelimiterNewLineForLK, vacationPeriods);
      result.VacationTypeId = vacations.OrderBy(v => v.BeginDate).FirstOrDefault().VacationType.Id;
      result.CurrentVacations = currentVacations;
      
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    #endregion
    
    #region Функции для работы с услугами переноса отпуска по заявлению в ЛК
    
    /// <summary>
    /// Заполнить дату прекращения в задаче на согласование заявления.
    /// </summary>
    /// <param name="taskId">ИД задачи на согласование заявления.</param>
    /// <param name="abortingDate">Дата прекращения.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillAbortingDateForStatementTask(long taskId, string abortingDate)
    {
      Logger.DebugFormat("FillAbortingDateForStatementTask. ИД задачи: {0}. Начало выполнения.", taskId);
      try
      {
        var task = StatementTasks.GetAll(t => t.Id == taskId).FirstOrDefault();
        task.AbortingDateDirRX = !string.IsNullOrEmpty(abortingDate) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(abortingDate) : null;
        task.Save();
        Logger.DebugFormat("FillAbortingDateForStatementTask. ИД задачи: {0}. Задача успешно сохранена.", taskId);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("FillAbortingDateForStatementTask. ИД задачи: {0}. Не удалось сохранить задачу. Текст ошибки: {1}. StackTrace: {2}.", taskId, ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Получить информацию об отпуске для переноса по заявлению из задания на доработку.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Json с данными по отпуску.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationDataForReworkShiftByStatement(long assignmentId)
    {
      Logger.DebugFormat("GetVacationDataForReworkShiftByStatement. ИД задания: {0}. Начало выполнения.", assignmentId);
      var assignment = NNCommons.StatementReworkAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      var docRelated = assignment.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var statement = VacationStatementDocuments.As(docRelated);
      if (statement == null)
      {
        Logger.ErrorFormat("GetVacationDataForReworkShiftByStatement. ИД задания: {0}. Документ не найден.", assignmentId);
        return string.Empty;
      }
      var employee = statement.EmployeeDirRX;
      var beginDate = statement.StartDateDirRX.Value;
      var endDate = statement.EndDateDirRX.Value;
      var days = statement.DaysAmountDirRX.Value;
      
      var result = Structures.Module.VacationDataForReworkShiftByStatement.Create();
      result.PerformerId = assignment.Performer.Id;
      result.PerformerInfo = string.Join(Environment.NewLine, employee.JobTitle, employee.Department, employee.BusinessUnit);
      result.StatementId = statement.Id;
      result.TaskId = assignment.Task.Id;
      result.BeginDate = beginDate;
      result.EndDate = endDate;
      result.Duration = days;
      result.Paid = statement.PaymentCompletedDirRX.GetValueOrDefault();
      result.ShiftKind = statement.ChangePeriodKindDirRX.Value.Value;
      result.ShiftPeriod = Resources.CurrentVacationPeriodForRecallFormat(beginDate.ToShortDateString(), endDate.ToShortDateString(), days);
      var vacationPeriods = statement.VacationDataDirRX.Select(x => Resources.CurrentVacationPeriodForShiftFormat(x.VacationType.Name, x.StartDate.Value.ToShortDateString(), x.EndDate.Value.ToShortDateString(), x.Days));
      result.ShiftPeriodInfo = string.Join(Constants.Module.DelimiterNewLineForLK, vacationPeriods);
      result.Reason = statement.Reason.Replace(DirRX.NNVacations.Resources.ShiftVacationReason.ToString(), string.Empty);
      result.Types = statement.VacationDataDirRX.Select(x => x.Absence).ToList();
      
      result.OldVacations = new List<Structures.Module.IFormattedVacationForShiftByStatement>();
      foreach (var oldVacation in statement.VacationDataDirRX)
      {
        var oldVacationData = Structures.Module.FormattedVacationForShiftByStatement.Create();
        oldVacationData.VacationTypeId = oldVacation.VacationType.Id;
        oldVacationData.Type = oldVacation.Absence;
        oldVacationData.DateStart = oldVacation.StartDate.Value;
        oldVacationData.DaysCount = oldVacation.Days.Value;
        oldVacationData.DateEnd = oldVacation.EndDate.Value;
        result.OldVacations.Add(oldVacationData);
      }
      
      result.NewVacations = new List<Structures.Module.IAdditionalVacationData>();
      foreach (var newVacation in statement.AdditionalVacationDirRX)
      {
        var newVacationData = Structures.Module.AdditionalVacationData.Create();
        newVacationData.VacationTypeId = newVacation.VacationType.Id;
        newVacationData.BeginDate = newVacation.StartDate.Value;
        newVacationData.Duration = newVacation.Days.Value;
        newVacationData.EndDate = newVacation.EndDate.Value;
        result.NewVacations.Add(newVacationData);
      }
      
      var resultJson = IsolatedFunctions.Json.SerializeInJson(result);
      Logger.DebugFormat("GetVacationDataForReworkShiftByStatement. ИД задания: {0}. Конец выполнения.", assignmentId);
      return resultJson;
    }
    
    /// <summary>
    /// Заполнить даты текущего отпуска в заявлении на перенос отпуска.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="vacationsJson">Json с информацией по отпуску.</param>
    /// <param name="shiftKind">Вид переноса.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillCurrentVacationDatesForShiftByStatement(string document, string vacationsJson, int shiftKind)
    {
      var documentId = IsolatedFunctions.Json.GetDocumentIdFromLK(document);
      Logger.DebugFormat("FillCurrentVacationDatesForShiftByStatement. ИД документа: {0}. Начало выполнения.", documentId);
      var statement = VacationStatementDocuments.GetAll(d => d.Id == documentId).FirstOrDefault();
      statement.ChangePeriodKindDirRX = shiftKind == 1 ?
        OverrideSolution.VacationStatementDocument.ChangePeriodKindDirRX.PlannedVacation :
        OverrideSolution.VacationStatementDocument.ChangePeriodKindDirRX.GrantedVacation;
      var vacations = IsolatedFunctions.Json.DeserializeInStruct(vacationsJson, Structures.Module.FormattedVacationForShiftByStatement.Create());
      foreach (var vacInfo in vacations)
      {
        var vacation = statement.VacationDataDirRX.AddNew();
        vacation.VacationType = VacationKinds.GetAll(v => v.Id == vacInfo.VacationTypeId).FirstOrDefault();
        vacation.Absence = vacInfo.Type;
        vacation.StartDate = vacInfo.DateStart;
        vacation.EndDate = vacInfo.DateEnd;
        vacation.Days = vacInfo.DaysCount;
      }
      try
      {
        statement.Save();
        Logger.DebugFormat("FillCurrentVacationDatesForShiftByStatement. ИД документа: {0}. Документ успешно сохранен.", documentId);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("FillCurrentVacationDatesForShiftByStatement. ИД документа: {0}. Не удалось сохранить документ. Текст ошибки: {1}. StackTrace: {2}.", documentId, ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Получить типы отпуска для услуги перенос отпуска по заявлению.
    /// </summary>
    /// <param name="types">Коды типов отпусков.</param>
    /// <returns>Json с данными по типам отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string GetVacationTypesForShiftByStatement(List<string> types)
    {
      if (types == null || !types.Any())
        return "[]";
      var vacationTypes = VacationKinds.GetAll(x => x.Status == OverrideSolution.VacationKind.Status.Active &&
                                               x.VacationKindDirRX == OverrideSolution.VacationKind.VacationKindDirRX.AnnualLeave &&
                                               (types.Contains(x.PlannedAbsenceDirRX) ||
                                                types.Contains(x.PlannedAbsenceShiftDirRX) ||
                                                types.Contains(x.SAPIdDirRX)));
      return this.GetJsonVacationTypes(vacationTypes);
    }
    
    /// <summary>
    /// Получить информацию о доступных отпусках для переноса по заявлению.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Json с данными по отпускам.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationDataForShiftByStatement(long employeeId)
    {
      Logger.DebugFormat("GetVacationDataForShiftByStatement. ИД сотрудника: {0}. Начало выполнения.", employeeId);
      var vacationsData = new List<Structures.Module.IVacationDataForShiftByStatement>();
      
      var limit = this.GetVacationAvailableLimits(employeeId);
      if (limit.Vacations == null || !limit.Vacations.Any())
        return IsolatedFunctions.Json.SerializeInJson(vacationsData);
      var vacationsInfo = limit.Vacations.Where(v => !v.Type.ToLower().Contains("vacation")).OrderBy(v => v.DateStart).ToList();
      if (!vacationsInfo.Any())
        return IsolatedFunctions.Json.SerializeInJson(vacationsData);
      
      var plannedVacationsConstantValue = NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.PlannedVacationsCodes.Guid);
      if (string.IsNullOrEmpty(plannedVacationsConstantValue))
      {
        Logger.ErrorFormat("GetVacationDataForShiftByStatement. Не заполнены прикладные константы для типов отпусков: {0}.", Constants.Module.ConstantsSetting.PlannedVacationsCodes.Name);
        return IsolatedFunctions.Json.SerializeInJson(vacationsData);
      }
      var plannedVacationsTypes = plannedVacationsConstantValue.Split(',').Select(x => x.Trim()).ToList();
      
      // Сформировать словарь соответствий кодов и наименований типов отпусков.
      var vacTypeNameDictionary = new Dictionary<string, Structures.Module.VacationTypeInfo>();
      foreach (var vacInfo in vacationsInfo.GroupBy(x => x.Type))
      {
        var type = vacInfo.First().Type;
        var vacationType = VacationKinds.GetAll(x => x.Status == OverrideSolution.VacationKind.Status.Active &&
                                                x.VacationKindDirRX == OverrideSolution.VacationKind.VacationKindDirRX.AnnualLeave &&
                                                (type == x.PlannedAbsenceDirRX ||
                                                 type == x.PlannedAbsenceShiftDirRX ||
                                                 type == x.SAPIdDirRX)).FirstOrDefault();
        var vacationTypeInfo = Structures.Module.VacationTypeInfo.Create(vacationType.Id, vacationType.Name);
        vacTypeNameDictionary.Add(type, vacationTypeInfo);
      }
      
      // Сформировать список по группам подряд идущих отпусков.
      var vacationsList = new List<List<Structures.Module.IFormattedVacationForShiftByStatement>>();
      var vacationsGroup = new List<Structures.Module.IFormattedVacationForShiftByStatement>();
      var vacInfoFirst = vacationsInfo.First();
      var vacGroupFirst = Structures.Module.FormattedVacationForShiftByStatement.Create(vacInfoFirst.DateStart, vacInfoFirst.DateEnd, vacTypeNameDictionary[vacInfoFirst.Type].Id, vacInfoFirst.Type, vacInfoFirst.DaysCount, vacInfoFirst.Paid);
      vacationsGroup.Add(vacGroupFirst);
      foreach (var vacInfo in vacationsInfo.Skip(1))
      {
        if ((!vacInfo.ChainDateStart.HasValue && vacInfo.DateStart != vacationsGroup.OrderByDescending(v => v.DateStart).First().DateEnd.AddDays(1)) ||
            (vacInfo.ChainDateStart.HasValue && vacInfo.ChainDateStart.Value != vacationsGroup.OrderBy(v => v.DateStart).First().DateStart))
        {
          vacationsList.Add(vacationsGroup);
          vacationsGroup = new List<Structures.Module.IFormattedVacationForShiftByStatement>();
        }
        var vacGroupItem = Structures.Module.FormattedVacationForShiftByStatement.Create(vacInfo.DateStart, vacInfo.DateEnd, vacTypeNameDictionary[vacInfo.Type].Id, vacInfo.Type, vacInfo.DaysCount, vacInfo.Paid);
        vacationsGroup.Add(vacGroupItem);
      }
      vacationsList.Add(vacationsGroup);
      
      var i = 0;
      foreach (var vacGroup in vacationsList)
      {
        var beginDate = vacGroup.OrderBy(v => v.DateStart).First().DateStart;
        var endDate = vacGroup.OrderByDescending(v => v.DateEnd).First().DateEnd;
        var daysCount = vacGroup.Sum(v => v.DaysCount);
        var vacationData = Structures.Module.VacationDataForShiftByStatement.Create();
        vacationData.Number = ++i;
        vacationData.DateStart = beginDate;
        vacationData.DateEnd = endDate;
        vacationData.DaysCount = daysCount;
        vacationData.Vacations = vacGroup;
        vacationData.Paid = vacGroup.Any(x => x.Paid);
        vacationData.IsPlanned = vacGroup.Any(x => plannedVacationsTypes.Contains(x.Type));
        vacationData.VacationPeriod = Resources.CurrentVacationPeriodForRecallFormat(beginDate.ToShortDateString(), endDate.ToShortDateString(), daysCount);
        var vacGroupPeriods = vacGroup.Select(x => Resources.CurrentVacationPeriodForShiftFormat(vacTypeNameDictionary[x.Type].Name, x.DateStart.ToShortDateString(), x.DateEnd.ToShortDateString(), x.DaysCount));
        vacationData.VacationPeriodInfo = string.Join(Constants.Module.DelimiterNewLineForLK, vacGroupPeriods);
        vacationsData.Add(vacationData);
      }
      
      Logger.DebugFormat("GetVacationDataForShiftByStatement. ИД сотрудника: {0}. Конец выполнения.", employeeId);
      return IsolatedFunctions.Json.SerializeInJson(vacationsData);
    }
    
    #endregion
    
    /// <summary>
    /// Получить временный шаблон с заполненной таблицей.
    /// </summary>
    /// <param name="templateId">ИД шаблона.</param>
    /// <param name="templateWithTableName">Имя временного шаблона.</param>
    /// <param name="vacationTypeIds">ИД типов отпусков.</param>
    /// <param name="beginDates">Даты начала отпусков.</param>
    /// <param name="endDates">Даты окончания отпусков.</param>
    /// <param name="durations">Продолжительности отпусков.</param>
    /// <param name="oldVacationsJson">Json с информацией по старому отпуску.</param>
    /// <returns>ИД временного шаблона с заполненной таблицей.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public long GetTemplateWithTableId(long templateId, string templateWithTableName, List<long> vacationTypeIds, List<DateTime> beginDates, List<DateTime> endDates, List<int> durations, string oldVacationsJson)
    {
      var delimiter = Constants.Module.ListIdsDelimiter.ToString();
      var logPrefix = string.Format("GetTemplateWithTableId. ИД шаблона: {0}. ИД типа отпуска: {1}. Даты начала отпусков: {2}. Даты окончания отпусков: {3}. Продолжительности отпусков: {4}",
                                    templateId, string.Join(delimiter, vacationTypeIds), string.Join(delimiter, beginDates), string.Join(delimiter, endDates), string.Join(delimiter, durations));
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      
      var template = Sungero.Docflow.DocumentTemplates.GetAll(t => t.Id == templateId).FirstOrDefault();
      var templateWithTable = Sungero.Docflow.SimpleDocuments.CreateFrom(template);
      templateWithTable.Name = templateWithTableName;
      templateWithTable.Save();
      
      // Таблица с информацией по старым отпускам.
      var oldVacations = IsolatedFunctions.Json.DeserializeInStruct(oldVacationsJson, Structures.Module.FormattedVacationForShiftByStatement.Create());
      var oldVacationsTable = NNCommons.Structures.Module.TableData.Create();
      oldVacationsTable.NumberKey = TableDataKeys.OldVacations;
      oldVacationsTable.Values = new List<Dictionary<string, string>>();
      foreach (var oldVacation in oldVacations)
      {
        var tableLineDictionary = new Dictionary<string, string>();
        var vacationType = VacationKinds.GetAll(v => v.Id == oldVacation.VacationTypeId).FirstOrDefault();
        tableLineDictionary.Add(TableDataKeys.VacationTypeName, vacationType.Name);
        tableLineDictionary.Add(TableDataKeys.DaysAmount, oldVacation.DaysCount.ToString());
        tableLineDictionary.Add(TableDataKeys.BeginDate, oldVacation.DateStart.ToShortDateString());
        tableLineDictionary.Add(TableDataKeys.EndDate, oldVacation.DateEnd.ToShortDateString());
        oldVacationsTable.Values.Add(tableLineDictionary);
      }
      
      // Таблица с информацией по новым отпускам.
      var newVacationsTable = NNCommons.Structures.Module.TableData.Create();
      newVacationsTable.NumberKey = TableDataKeys.Table;
      newVacationsTable.Values = new List<Dictionary<string, string>>();
      for (var i = 0; i < vacationTypeIds.Count; i++)
      {
        var tableLineDictionary = new Dictionary<string, string>();
        var vacationType = VacationKinds.GetAll(v => v.Id == vacationTypeIds[i]).FirstOrDefault();
        tableLineDictionary.Add(TableDataKeys.VacationTypeName, vacationType.Name);
        tableLineDictionary.Add(TableDataKeys.DaysAmount, durations[i].ToString());
        tableLineDictionary.Add(TableDataKeys.BeginDate, beginDates[i].ToShortDateString());
        tableLineDictionary.Add(TableDataKeys.EndDate, endDates[i].ToShortDateString());
        newVacationsTable.Values.Add(tableLineDictionary);
      }
      
      using (var memory = new System.IO.MemoryStream())
      {
        try
        {
          using (Sungero.Domain.Shared.HistoryLocker.WithoutWriteHistory(templateWithTable.LastVersion.ElectronicDocument))
          {
            templateWithTable.LastVersion.Body.Read().CopyTo(memory);
          }
          var documentStream = NNCommons.IsolatedFunctions.WordInsertTable.WordInsertTableInTag(logPrefix, memory, oldVacationsTable);
          documentStream = NNCommons.IsolatedFunctions.WordInsertTable.WordInsertTableInTag(logPrefix, documentStream, newVacationsTable);
          using (var stream = new System.IO.MemoryStream())
          {
            documentStream.CopyTo(stream);
            templateWithTable.LastVersion.Body.Write(stream);
            templateWithTable.Save();
          }
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. Заполнить таблицу отпусков. Ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        }
      }
      
      HRPro.HRCommons.PublicFunctions.Module.Remote.EnqueueDocumentDeleteJob(Sungero.Docflow.OfficialDocuments.As(templateWithTable));
      
      Logger.DebugFormat("{0}. ИД временного шаблона: {1}. Конец выполнения.", logPrefix, templateWithTable.Id);
      return templateWithTable.Id;
    }
    
    /// <summary>
    /// Получить типы отпуска для услуги перенос ежегодного отпуска.
    /// </summary>
    /// <param name="noticeId">ИД уведомления о начале отпуска.</param>
    /// <returns>Json с данными по типам отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationTypesForShift(long noticeId)
    {
      var notice = VacationNoticeDocuments.GetAll(d => d.Id == noticeId).FirstOrDefault();
      if (notice == null)
      {
        Logger.ErrorFormat("GetVacationTypesForShift. ИД уведомления о начале отпуска: {0}. Документ не найден.", noticeId);
        return "[]";
      }
      var vacationTypes = notice.VacationDataDirRX.Select(v => v.VacationType).AsQueryable();
      return this.GetJsonVacationTypes(vacationTypes);
    }
    
    /// <summary>
    /// Получить информацию об отпуске для переноса из задания на доработку.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Json с данными по отпуску.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationDataForReworkShift(long assignmentId)
    {
      Logger.DebugFormat("GetVacationDataForReworkShift. ИД задания: {0}. Начало выполнения.", assignmentId);
      var docRelated = OfficialDocuments.Null;
      // NNStatementReworkAssignments - устаревшее, сделано для совместимости.
      var assignment = Sungero.Workflow.Assignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      if (NNStatementReworkAssignments.Is(assignment))
        docRelated = NNStatementReworkAssignments.As(assignment).DocumentGroup.DocRelatedToWorkBases.FirstOrDefault();
      else if (NNCommons.StatementReworkAssignments.Is(assignment))
        docRelated = NNCommons.StatementReworkAssignments.As(assignment).DocumentGroup.OfficialDocuments.FirstOrDefault();
      var statement = VacationStatementDocuments.As(docRelated);
      if (statement == null)
      {
        Logger.ErrorFormat("GetVacationDataForReworkShift. ИД задания: {0}. Документ не найден.", assignmentId);
        return string.Empty;
      }
      var shiftTask = StatementTasks.As(assignment.Task);
      var confirmTask = RegistrationOfAnnualLeaves.GetAll(t => t.ShiftTaskId == shiftTask.Id).FirstOrDefault();
      var noticeId = confirmTask?.DocumentGroup.VacationNoticeDocuments.FirstOrDefault()?.Id;
      var employee = statement.EmployeeDirRX;
      var beginDate = statement.StartDateDirRX.Value;
      var endDate = statement.EndDateDirRX.Value;
      
      var result = Structures.Module.VacationDataForReworkShift.Create();
      result.PerformerId = assignment.Performer.Id;
      result.StatementId = statement.Id;
      result.TaskId = shiftTask.Id;
      result.Reason = statement.Reason.Replace(DirRX.NNVacations.Resources.ShiftVacationReason.ToString(), string.Empty);
      var vacationInfo = Structures.Module.CurrentVacationInfoForShift.Create();
      vacationInfo.NoticeId = noticeId ?? 0;
      vacationInfo.BeginDate = beginDate;
      vacationInfo.EndDate = endDate;
      vacationInfo.PerformerInfo = string.Join(Environment.NewLine, employee.BusinessUnit, employee.Department, employee.JobTitle);
      result.VacationInfo = vacationInfo;
      
      var vacationsPeriod = new List<string>();
      var vacations = new List<DirRX.NNVacations.Structures.Module.IAdditionalVacationData>();
      foreach (var vacation in statement.VacationDataDirRX)
      {
        var info = Structures.Module.AdditionalVacationData.Create();
        info.BeginDate = vacation.StartDate.Value;
        info.EndDate = vacation.EndDate.Value;
        info.Duration = vacation.Days.Value;
        info.VacationTypeId = vacation.VacationType.Id;
        vacations.Add(info);
        var vacationPeriod = Resources.CurrentVacationPeriodForShiftFormat(vacation.VacationType.Name, info.BeginDate.ToShortDateString(), info.EndDate.ToShortDateString(), info.Duration);
        vacationsPeriod.Add(vacationPeriod);
      }
      vacationInfo.Duration = vacations.Sum(v => v.Duration);
      vacationInfo.Vacations = vacations;
      vacationInfo.VacationPeriod = string.Join(Constants.Module.DelimiterNewLineForLK, vacationsPeriod);
      
      result.AdditionalVacationData = new List<Structures.Module.IAdditionalVacationData>();
      foreach (var newVacation in statement.AdditionalVacationDirRX)
      {
        var newVacationData = Structures.Module.AdditionalVacationData.Create();
        newVacationData.VacationTypeId = newVacation.VacationType.Id;
        newVacationData.BeginDate = newVacation.StartDate.Value;
        newVacationData.Duration = newVacation.Days.Value;
        newVacationData.EndDate = newVacation.EndDate.Value;
        result.AdditionalVacationData.Add(newVacationData);
      }
      var resultJson = IsolatedFunctions.Json.SerializeInJson(result);
      Logger.DebugFormat("GetVacationDataForReworkShift. ИД задания: {0}. Конец выполнения.", assignmentId);
      return resultJson;
    }
    
    /// <summary>
    /// Получить данные об отпуске для доработки заявления на прочий отпуск.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Строка с информацией по отпуску.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationDataForReworkOther(long assignmentId)
    {
      Logger.DebugFormat("GetVacationDataForReworkOther. ИД задания: {0}. Начало выполнения.", assignmentId);
      var docRelated = OfficialDocuments.Null;
      // NNStatementReworkAssignments - устаревшее, сделано для совместимости.
      var assignment = Sungero.Workflow.Assignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      if (NNStatementReworkAssignments.Is(assignment))
        docRelated = NNStatementReworkAssignments.As(assignment).DocumentGroup.DocRelatedToWorkBases.FirstOrDefault();
      else if (NNCommons.StatementReworkAssignments.Is(assignment))
        docRelated = NNCommons.StatementReworkAssignments.As(assignment).DocumentGroup.OfficialDocuments.FirstOrDefault();
      var document = VacationStatementDocuments.As(docRelated);
      if (document == null)
      {
        Logger.ErrorFormat("GetVacationDataForReworkOther. ИД задания: {0}. Документ не найден.", assignmentId);
        return string.Empty;
      }
      var employee = document.EmployeeDirRX;
      var result = Structures.Module.VacationDataForReworkOther.Create();
      result.InitiatorId = employee.Id;
      result.StatementId = document.Id;
      result.PerformerInfo = string.Join(Environment.NewLine, employee.BusinessUnit, employee.Department, employee.JobTitle);
      var vacationType = document.VacationTypeDirRX;
      result.VacationTypeId = vacationType.Id;
      result.VacationKind = vacationType.VacationKindDirRX.Value.Value;
      result.VacationReasonId = document.VacationReasonDirRX != null ? document.VacationReasonDirRX.Id : 0;
      result.BeginDate = document.StartDateDirRX.Value;
      result.EndDate = document.EndDateDirRX.Value;
      result.Duration = document.DaysAmountDirRX.Value;
      result.IsAspirant = document.IsPostgraduateDirRX.GetValueOrDefault();
      
      Logger.DebugFormat("GetVacationDataForReworkOther. ИД задания: {0}. Конец выполнения.", assignmentId);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить данные об отпуске для доработки заявления на ежегодный отпуск.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Строка с информацией по отпуску.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationDataForReworkAnnual(long assignmentId)
    {
      Logger.DebugFormat("GetVacationDataForReworkAnnual. ИД задания: {0}. Начало выполнения.", assignmentId);
      var docRelated = OfficialDocuments.Null;
      // NNStatementReworkAssignments - устаревшее, сделано для совместимости.
      var assignment = Sungero.Workflow.Assignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      if (NNStatementReworkAssignments.Is(assignment))
        docRelated = NNStatementReworkAssignments.As(assignment).DocumentGroup.DocRelatedToWorkBases.FirstOrDefault();
      else if (NNCommons.StatementReworkAssignments.Is(assignment))
        docRelated = NNCommons.StatementReworkAssignments.As(assignment).DocumentGroup.OfficialDocuments.FirstOrDefault();
      var document = VacationStatementDocuments.As(docRelated);
      if (document == null)
      {
        Logger.ErrorFormat("GetVacationDataForReworkAnnual. ИД задания: {0}. Документ не найден.", assignmentId);
        return string.Empty;
      }
      var employee = document.EmployeeDirRX;
      var result = Structures.Module.VacationDataForReworkAnnual.Create();
      result.InitiatorId = employee.Id;
      result.StatementId = document.Id;
      result.PerformerInfo = string.Join(Environment.NewLine, employee.BusinessUnit, employee.Department, employee.JobTitle);
      result.VacationTypeId = document.VacationTypeDirRX.Id;
      result.BeginDate = document.StartDateDirRX.Value;
      result.EndDate = document.EndDateDirRX.Value;
      result.Duration = document.DaysAmountDirRX.Value;
      
      result.AdditionalVacationData = new List<Structures.Module.IAdditionalVacationData>();
      foreach (var newVacation in document.AdditionalVacationDirRX)
      {
        var newVacationData = Structures.Module.AdditionalVacationData.Create();
        newVacationData.VacationTypeId = newVacation.VacationType.Id;
        newVacationData.BeginDate = newVacation.StartDate.Value;
        newVacationData.Duration = newVacation.Days.Value;
        newVacationData.EndDate = newVacation.EndDate.Value;
        result.AdditionalVacationData.Add(newVacationData);
      }
      
      Logger.DebugFormat("GetVacationDataForReworkAnnual. ИД задания: {0}. Конец выполнения.", assignmentId);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить данные об отпуске для доработки заявления отзыв из отпуска.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Json с информацией по отпуску.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationDataForReworkRecall(long assignmentId)
    {
      var logPrefix = string.Format("GetVacationDataForReworkRecall. ИД задания: {0}", assignmentId);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      var assignment = NNCommons.StatementReworkAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      var docRealated = assignment.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var document = VacationStatementDocuments.As(docRealated);
      if (document == null)
      {
        Logger.ErrorFormat("{0}. Документ не найден.", logPrefix);
        return string.Empty;
      }
      var employee = document.EmployeeDirRX;
      var initiator = Employees.As(document.Author);
      var result = Structures.Module.VacationDataForReworkRecall.Create();
      result.InitiatorId = initiator.Id;
      result.StatementId = document.Id;
      result.EmployeeId = employee.Id;
      result.PerformerInfo = string.Join(Environment.NewLine, initiator.JobTitle, initiator.Department, initiator.BusinessUnit);
      result.VacationTypeId = document.VacationTypeDirRX?.Id ?? 0;
      var vacationPeriods = document.VacationDataDirRX.Select(x => Resources.CurrentVacationPeriodForShiftFormat(x.VacationType.Name, x.StartDate.Value.ToShortDateString(), x.EndDate.Value.ToShortDateString(), x.Days));
      result.VacationPeriod = string.Join(Constants.Module.DelimiterNewLineForLK, vacationPeriods);
      
      result.BeginDateCurrent = document.StartDateDirRX.Value;
      result.EndDateCurrent = document.EndDateDirRX.Value;
      result.DurationCurrent = document.DaysAmountDirRX.Value;
      
      result.BeginDateRecall = document.InterruptStartDateDirRX.Value;
      result.EndDateRecall = document.InterruptEndDateDirRX.Value;
      result.DurationRecall = document.InterruptDaysDirRX.Value;
      
      var shiftVacations = document.AdditionalVacationDirRX;
      
      if (shiftVacations.Any())
      {
        result.BeginDateShift = shiftVacations.OrderBy(v => v.StartDate).First().StartDate.Value;
        result.EndDateShift = shiftVacations.OrderByDescending(v => v.EndDate).First().EndDate.Value;
        result.DurationShift = shiftVacations.Sum(v => v.Days).Value;
      }
      else
        result.DurationShift = 0;
      
      result.CompensationKind = document.CompensationTypeDirRX.Value == OverrideSolution.VacationStatementDocument.CompensationTypeDirRX.WithPayment ? "1" : "2";
      result.Reason = document.Note;
      
      result.CurrentVacations = new List<Structures.Module.IVacationInfoIFS10392>();
      foreach (var vacation in document.VacationDataDirRX)
      {
        var currentVacation = Structures.Module.VacationInfoIFS10392.Create();
        currentVacation.Type = vacation.VacationType.SAPIdDirRX;
        currentVacation.CurrentDateStart = vacation.StartDate.Value.ToShortDateString();
        currentVacation.CurrentDateEnd = vacation.EndDate.Value.ToShortDateString();
        currentVacation.DayCount = vacation.Days.Value;
        result.CurrentVacations.Add(currentVacation);
      }
      
      var resultJson = string.Empty;
      try
      {
        resultJson = IsolatedFunctions.Json.SerializeInJson(result);
        Logger.DebugFormat("{0}. Конец выполнения.", logPrefix);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. При получении данных об отпуске для доработки заявления отзыва из отпуска возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      return resultJson;
    }
    
    /// <summary>
    /// Заполнить данные об отпуске в заявлении на прочий отпуск.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="vacationTypeId">ИД типа отпуска.</param>
    /// <param name="vacationReasonId">ИД причины отпуска.</param>
    /// <param name="beginDate">Дата начала отпуска.</param>
    /// <param name="endDate">Дата окончания отпуска.</param>
    /// <param name="duration">Продолжительность отпуска.</param>
    /// <param name="aspirant">Аспирант.</param>
    /// <param name="vacationDaysExcess">Кол-во дней превышения.</param>
    /// <param name="isVacationPeriodBroken">Отсутствие отпуска в 14 дней.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillVacationDataForOther(string document, long? vacationTypeId, long? vacationReasonId, DateTime? beginDate, DateTime? endDate, int? duration, bool? aspirant)
    {
      var documentId = IsolatedFunctions.Json.GetDocumentIdFromLK(document);
      beginDate = beginDate?.ToUserTime();
      endDate = endDate?.ToUserTime();
      Logger.DebugFormat("FillVacationDataForOther. ИД документа: {0}. ИД типа отпуска: {1}. ИД причины отпуска: {2}. Дата начала отпуска: {3}. Дата окончания отпуска: {4}. Продолжительность отпуска: {5}. Аспирант: {6}. Начало выполнения.",
                         documentId, vacationTypeId, vacationReasonId, beginDate, endDate, duration, aspirant);
      var statement = VacationStatementDocuments.GetAll(d => d.Id == documentId).FirstOrDefault();
      var vacationType = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault();
      statement.ApplicationDateDirRX = Calendar.Today;
      statement.VacationTypeDirRX = vacationType;
      statement.StartDateDirRX = beginDate;
      statement.EndDateDirRX = endDate;
      statement.DaysAmountDirRX = duration;
      if (this.CheckForStudyVacationType(vacationType.Id))
      {
        statement.IsPostgraduateDirRX = aspirant;
      }
      else
      {
        statement.VacationReasonDirRX = VacationReasonses.GetAll(v => v.Id == vacationReasonId).FirstOrDefault();
      }
      
      try
      {
        statement.Save();
        Logger.DebugFormat("FillVacationDataForOther. ИД документа: {0}. Документ успешно сохранен.", documentId);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("FillVacationDataForOther. ИД документа: {0}. Не удалось сохранить документ. Текст ошибки: {1}. StackTrace: {2}.", documentId, ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Заполнить даты отпусков в заявлении на ежегодный отпуск.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="vacationTypeAnnualId">ИД типа ежегодного отпуска.</param>
    /// <param name="beginDateAnnual">Дата начала ежегодного отпуска.</param>
    /// <param name="endDateAnnual">Дата окончания ежегодного отпуска.</param>
    /// <param name="durationAnnual">Продолжительность ежегодного отпуска.</param>
    /// <param name="vacationDaysExcess">Кол-во дней превышения.</param>
    /// <param name="vacationTypeIds">ИД типов дополнительных отпусков.</param>
    /// <param name="beginDates">Даты начала дополнительных отпусков.</param>
    /// <param name="endDates">Даты окончания дополнительных отпусков.</param>
    /// <param name="durations">Продолжительности дополнительных отпусков.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillVacationDatesForAnnual(string document, long? vacationTypeAnnualId, DateTime? beginDateAnnual, DateTime? endDateAnnual, int? durationAnnual, int? vacationDaysExcess,
                                           List<long?> vacationTypeIds, List<DateTime?> beginDates, List<DateTime?> endDates, List<int?> durations)
    {
      var documentId = IsolatedFunctions.Json.GetDocumentIdFromLK(document);
      beginDateAnnual = beginDateAnnual?.ToUserTime();
      endDateAnnual = endDateAnnual?.ToUserTime();
      Logger.DebugFormat("FillVacationDatesForAnnual. ИД документа: {0}. ИД типа отпуска: {1}. Дата начала отпуска: {2}. Дата окончания отпуска: {3}. Продолжительность отпуска: {4}. Кол-во дней превышения {5}. Начало выполнения.",
                         documentId, vacationTypeAnnualId, beginDateAnnual, endDateAnnual, durationAnnual, vacationDaysExcess);
      var statement = VacationStatementDocuments.GetAll(d => d.Id == documentId).FirstOrDefault();
      statement.ApplicationDateDirRX = Calendar.Today;
      statement.VacationTypeDirRX = VacationKinds.GetAll(v => v.Id == vacationTypeAnnualId).FirstOrDefault();
      statement.StartDateDirRX = beginDateAnnual;
      statement.EndDateDirRX = endDateAnnual;
      statement.DaysAmountDirRX = durationAnnual;
      statement.VacationDaysExcessDirRX = vacationDaysExcess;
      statement.AdditionalVacationDirRX.Clear();
      for (var i = 0; i < vacationTypeIds.Count; i++)
      {
        var vacation = statement.AdditionalVacationDirRX.AddNew();
        vacation.VacationType = VacationKinds.GetAll(v => v.Id == vacationTypeIds[i]).FirstOrDefault();
        vacation.StartDate = beginDates[i].ToUserTime();
        vacation.EndDate = endDates[i].ToUserTime();
        vacation.Days = durations[i];
      }
      
      try
      {
        statement.Save();
        Logger.DebugFormat("FillVacationDatesForAnnual. ИД документа: {0}. Документ успешно сохранен.", documentId);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("FillVacationDatesForAnnual. ИД документа: {0}. Не удалось сохранить документ. Текст ошибки: {1}. StackTrace: {2}.", documentId, ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Заполнить даты текущего отпуска в заявлении на перенос отпуска.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="noticeId">ИД уведомления о начале отпуска.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillCurrentVacationDatesForShift(string document, long noticeId)
    {
      var documentId = IsolatedFunctions.Json.GetDocumentIdFromLK(document);
      Logger.DebugFormat("FillCurrentVacationDatesForShift. ИД документа: {0}. ИД уведомления о начале отпуска: {1}. Начало выполнения.", documentId, noticeId);
      var statement = VacationStatementDocuments.GetAll(d => d.Id == documentId).FirstOrDefault();
      var notice = VacationNoticeDocuments.GetAll(d => d.Id == noticeId).FirstOrDefault();
      foreach (var vacationData in notice.VacationDataDirRX)
      {
        var vacation = statement.VacationDataDirRX.AddNew();
        vacation.VacationType = vacationData.VacationType;
        vacation.StartDate = vacationData.StartDate;
        vacation.EndDate = vacationData.EndDate;
        vacation.Days = vacationData.Days;
      }
      try
      {
        statement.Save();
        Logger.DebugFormat("FillCurrentVacationDatesForShift. ИД документа: {0}. Документ успешно сохранен.", documentId);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("FillCurrentVacationDatesForShift. ИД документа: {0}. Не удалось сохранить документ. Текст ошибки: {1}. StackTrace: {2}.", documentId, ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Заполнить даты новых отпусков в заявлении на перенос отпуска.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="vacationTypeIds">ИД типов отпусков.</param>
    /// <param name="beginDates">Даты начала отпусков.</param>
    /// <param name="endDates">Даты окончания отпусков.</param>
    /// <param name="durations">Продолжительности отпусков.</param>
    /// <param name="reason">Причина.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillNewVacationDatesForShift(string document, List<long?> vacationTypeIds, List<DateTime?> beginDates, List<DateTime?> endDates, List<int?> durations, string isVacationPeriodBroken, string reason)
    {
      var documentId = IsolatedFunctions.Json.GetDocumentIdFromLK(document);
      var delimiter = Constants.Module.ListIdsDelimiter.ToString();
      beginDates = beginDates.Select(d => d?.ToUserTime()).ToList();
      endDates = endDates.Select(d => d?.ToUserTime()).ToList();
      Logger.DebugFormat("FillNewVacationDatesForShift. ИД документа: {0}. ИД типа отпуска: {1}. Даты начала отпусков: {2}. Даты окончания отпусков: {3}. Продолжительности отпусков: {4}. Начало выполнения.",
                         documentId, string.Join(delimiter, vacationTypeIds), string.Join(delimiter, beginDates), string.Join(delimiter, endDates), string.Join(delimiter, durations));
      var statement = VacationStatementDocuments.GetAll(d => d.Id == documentId).FirstOrDefault();
      statement.IsVacationPeriodBrokenDirRX = isVacationPeriodBroken;
      statement.Reason = !string.IsNullOrWhiteSpace(reason) ? string.Format("{0}{1}", DirRX.NNVacations.Resources.ShiftVacationReason, reason) : string.Empty;
      statement.AdditionalVacationDirRX.Clear();
      for (var i = 0; i < vacationTypeIds.Count; i++)
      {
        var vacation = statement.AdditionalVacationDirRX.AddNew();
        vacation.VacationType = VacationKinds.GetAll(v => v.Id == vacationTypeIds[i]).FirstOrDefault();
        vacation.StartDate = beginDates[i];
        vacation.EndDate = endDates[i];
        vacation.Days = durations[i];
      }
      try
      {
        statement.Save();
        Logger.DebugFormat("FillNewVacationDatesForShift. ИД документа: {0}. Документ успешно сохранен.", documentId);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("FillNewVacationDatesForShift. ИД документа: {0}. Не удалось сохранить документ. Текст ошибки: {1}. StackTrace: {2}.", documentId, ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Получить строку с информацией по новым датам отпусков для заполнения параметра шаблона заявления на перенос отпуска.
    /// </summary>
    /// <param name="employeeId">ИД отрудника.</param>
    /// <param name="beginDates">Даты начала отпусков.</param>
    /// <param name="endDates">Даты окончания отпусков.</param>
    /// <returns>Строка с информацией по новым датам отпусков.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetNewVacationDatesForShift(long employeeId, List<string> beginDates, List<string> endDates)
    {
      Logger.DebugFormat("GetNewVacationDatesForShift. ИД сотрудника: {0}. Даты начала отпусков: {1}. Даты окончания отпусков: {2}. Начало выполнения.",
                         employeeId, string.Join(",", beginDates), string.Join(",", endDates));
      var vacationString = string.Empty;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var businessUnit = employee.Department.BusinessUnit;
      var dateFormat = businessUnit != null ? BusinessUnits.As(businessUnit).DateFormatStringHRPro : "d MMMM yyyy г.";
      var vacationDatesList = new List<string>();
      for (var i = 0; i < beginDates.Count; i++)
      {
        var beginDate = DateTime.Parse(beginDates[i]).ToString(dateFormat);
        var endDate = DateTime.Parse(endDates[i]).ToString(dateFormat);
        vacationDatesList.Add(Resources.NewVacationDatesForShiftFormat(beginDate, endDate));
      }
      if (vacationDatesList.Any())
        vacationString = string.Join(Environment.NewLine, vacationDatesList);
      Logger.DebugFormat("GetNewVacationDatesForShift. ИД сотрудника: {0}. Результат: {1}. Конец выполнения.", employeeId, vacationString);
      return vacationString;
    }
    
    /// <summary>
    /// Получить информацию об отпуске для переноса из задания на подтверждение.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Json с данными по отпуску.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetCurrentVacationInfoForShift(long assignmentId)
    {
      Logger.DebugFormat("GetCurrentVacationInfoForShift. ИД задания: {0}. Начало выполнения.", assignmentId);
      var assignment = VacationConfirmationAssignments.GetAll(a => a.Id == assignmentId).FirstOrDefault();
      var document = assignment.DocumentGroup.VacationNoticeDocuments.FirstOrDefault();
      var vacationNoticeDocument = VacationNoticeDocuments.As(document);
      var employee = vacationNoticeDocument.EmployeeDirRX;
      var beginDate = vacationNoticeDocument.StartDateDirRX.Value;
      var endDate = vacationNoticeDocument.EndDateDirRX.Value;
      
      var vacationInfo = Structures.Module.CurrentVacationInfoForShift.Create();
      vacationInfo.NoticeId = vacationNoticeDocument.Id;
      vacationInfo.BeginDate = beginDate;
      vacationInfo.EndDate = endDate;
      vacationInfo.PerformerInfo = string.Join(Environment.NewLine, employee.BusinessUnit, employee.Department, employee.JobTitle);
      
      var vacationsPeriod = new List<string>();
      var vacations = new List<DirRX.NNVacations.Structures.Module.IAdditionalVacationData>();
      foreach (var vacation in vacationNoticeDocument.VacationDataDirRX)
      {
        var info = Structures.Module.AdditionalVacationData.Create();
        info.BeginDate = vacation.StartDate.Value;
        info.EndDate = vacation.EndDate.Value;
        info.Duration = vacation.Days.Value;
        info.VacationTypeId = vacation.VacationType.Id;
        vacations.Add(info);
        var vacationPeriod = Resources.CurrentVacationPeriodForShiftFormat(vacation.VacationType.Name, info.BeginDate.ToShortDateString(), info.EndDate.ToShortDateString(), info.Duration);
        vacationsPeriod.Add(vacationPeriod);
      }
      vacationInfo.Duration = vacations.Sum(v => v.Duration);
      vacationInfo.Vacations = vacations;
      vacationInfo.VacationPeriod = string.Join(Constants.Module.DelimiterNewLineForLK, vacationsPeriod);
      
      Logger.DebugFormat("GetCurrentVacationInfoForShift. ИД задания: {0}. Конец выполнения.", assignmentId);
      return IsolatedFunctions.Json.SerializeInJson(vacationInfo);
    }
    
    /// <summary>
    /// Получить строку с информацией по доп. отпускам для заполнения параметра шаблона заявления на ежегодный отпуск.
    /// </summary>
    /// <param name="employeeId">ИД отрудника.</param>
    /// <param name="vacationTypeIds">ИД типов отпусков.</param>
    /// <param name="beginDates">Даты начала отпусков.</param>
    /// <param name="endDates">Даты окончания отпусков.</param>
    /// <param name="durations">Продолжительности отпусков.</param>
    /// <returns>Строка с информацией по доп отпускам.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string GetAttachableVacationsInfo(long employeeId, List<string> vacationTypeIds, List<string> beginDates, List<string> endDates, List<string> durations)
    {
      var vacationString = string.Empty;
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var businessUnit = employee.Department.BusinessUnit;
      var dateFormat = BusinessUnits.As(businessUnit).DateFormatStringHRPro;
      var vacationDatesList = new List<string>();
      for (var i = 0; i < vacationTypeIds.Count; i++)
      {
        var vacationTypeId = Int32.Parse(vacationTypeIds[i]);
        var vacationTypeName = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault().Name;
        var beginDate = DateTime.Parse(beginDates[i]).ToString(dateFormat);
        var endDate = DateTime.Parse(endDates[i]).ToString(dateFormat);
        var duration = Int32.Parse(durations[i]);
        vacationDatesList.Add(Resources.AttachableVacationsInfoFormat(HRPro.Vacations.Resources.BulletMark, vacationTypeName, beginDate, endDate, duration));
      }
      if (vacationDatesList.Any())
        vacationString = Resources.AttachableVacationsInfoHeaderFormat(string.Join(Environment.NewLine, vacationDatesList));
      return vacationString;
    }
    
    /// <summary>
    /// Получить дату в соответствии с настройками модуля.
    /// </summary>
    /// <param name="employeeId">ИД отрудника.</param>
    /// <param name="dates">Список дат.</param>
    /// <returns>Даты в формате настроек.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string GetFormatDates(long employeeId, List<string> dates)
    {
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var businessUnit = employee.Department.BusinessUnit;
      var dateFormat = BusinessUnits.As(businessUnit).DateFormatStringHRPro;
      string [] formats = {"yyyy-MM-dd","yyyy-M-d", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy", "MM/dd/yyyy"};
      for (var i = 0; i < dates.Count; i++)
        dates[i] = DateTime.ParseExact(dates[i], formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None).ToString(dateFormat);
      return EssPlatform.PublicFunctions.Module.SerializedToJson(dates);
    }
    
    /// <summary>
    /// Получить значение макропеременной для заявления.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="fieldName">Макропеременная.</param>
    /// <returns>Значение макропеременной.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetDocumentFieldValue(long employeeId, string fieldName)
    {
      var employee = Employees.Get(employeeId);
      var department = employee.Department;
      var businessUnit = BusinessUnits.As(department.BusinessUnit);
      var personnelNumber = employee.PersonnelNumber;
      var vicePresident = Employees.Null;
      if (businessUnit != null)
        vicePresident = businessUnit.VicePresidentDirRX;
      
      switch (fieldName)
      {
        case "VICE_PRESIDENT":
          if (vicePresident != null)
            return string.Format("{0} {1}", vicePresident.JobTitle, vicePresident.Name);
          return string.Empty;
        case "CURRENT_USER_JOB_TITLE_NOMINATIVE":
          return Sungero.Company.PublicFunctions.Employee.GetJobTitle(employee, DeclensionCase.Nominative);
        case "CURRENT_USER_FULLNAME":
          return employee.Name;
        case "CURRENT_USER_SHORTNAME":
          var reverseShortName = Sungero.Company.PublicFunctions.Employee.GetReverseShortName(employee);
          return reverseShortName;
        case "CURRENT_USER_SHORTNAME_GENITIVE":
          var shortNameGenitive = Sungero.Company.PublicFunctions.Employee.GetShortName(employee, DeclensionCase.Genitive, false);
          return shortNameGenitive;
        case "DEPARTMENT_NAME":
          if (department != null)
            return this.GetDepartmentsStructure(department);
          return string.Empty;
        case "CURRENT_USER_PERSONNEL_NUMBER":
          if (!string.IsNullOrEmpty(personnelNumber))
            return personnelNumber;
          return string.Empty;
        default:
          return string.Empty;
      }
    }
    
    /// <summary>
    /// Сформировать цепочку подразделений от вышестоящих до текущего.
    /// </summary>
    /// <param name="department">Подразделение.</param>
    /// <returns>Цепочка подразделений.</returns>
    [Public, Remote(IsPure = true)]
    public virtual string GetDepartmentsStructure(Sungero.Company.IDepartment department)
    {
      if (department == null)
        return string.Empty;
      
      var result = department.Name;
      
      while (department.HeadOffice != null)
      {
        result = string.Format("{0}, {1}", department.HeadOffice.Name, result);
        department = department.HeadOffice;
      }
      
      if (department.BusinessUnit != null)
        return string.Format("{0}, {1}", department.BusinessUnit.Name, result);
      return result;
    }
    
    /// <summary>
    /// Получить присоединяемые типы отпуска.
    /// </summary>
    /// <returns>Json с данными по типам отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetAttachableVacationTypes()
    {
      var vacationTypes = VacationKinds.GetAll().Where(v => v.Status == DirRX.OverrideSolution.VacationKind.Status.Active && v.AttachableDirRX.GetValueOrDefault());
      return this.GetJsonVacationTypes(vacationTypes);
    }
    
    /// <summary>
    /// Получить json с данными по типам отпуска.
    /// </summary>
    /// <param name="vacationTypes">Типы отпусков.</param>
    /// <returns>Json с данными по типам отпуска.</returns>
    public string GetJsonVacationTypes(IQueryable<IVacationKind> vacationTypes)
    {
      var result = new List<Structures.Module.VacationTypeInfo>();
      foreach (var vacationType in vacationTypes.Distinct())
      {
        var vacationTypeInfo = Structures.Module.VacationTypeInfo.Create();
        vacationTypeInfo.Id = vacationType.Id;
        vacationTypeInfo.Name = vacationType.Name;
        result.Add(vacationTypeInfo);
      }
      return EssPlatform.PublicFunctions.Module.SerializedToJson(result);
    }
    
    /// <summary>
    /// Получить json с данными по причинам отпуска.
    /// </summary>
    /// <param name="vacationReasons">Причины отпуска.</param>
    /// <returns>Json с данными по причинам отпуска.</returns>
    public string GetJsonVacationReasons(IQueryable<IVacationReasons> vacationReasons)
    {
      var result = new List<Structures.Module.VacationReasonInfo>();
      foreach (var vacationReason in vacationReasons.Distinct())
      {
        var vacationReasonInfo = Structures.Module.VacationReasonInfo.Create();
        vacationReasonInfo.Id = vacationReason.Id;
        vacationReasonInfo.Name = vacationReason.Name;
        result.Add(vacationReasonInfo);
      }
      return EssPlatform.PublicFunctions.Module.SerializedToJson(result);
    }
    
    /// <summary>
    /// Получить сводку по доступным дням для оформления ежегодного отпуска.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Сводка по доступным дням.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetAvailableDaysInfo(long employeeId)
    {
      var resultValue = DirRX.NNVacations.Resources.GetAvailableDaysInfoDefaultText.ToString();
      var limits = this.GetVacationAvailableLimits(employeeId);
      if (string.IsNullOrEmpty(limits.ErrorText))
        resultValue = this.GetCurrentAvailableLimit(limits).ToString();
      return string.Format(DirRX.NNVacations.Resources.AvailableDaysInfo, resultValue);
    }
    
    /// <summary>
    /// Получить текст инструкции с ссылкой на ЛНА.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Текст инструкции с ссылкой на ЛНА.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetInstructionWithLinkLNA(long employeeId)
    {
      var employee = Employees.GetAll(e => e.Id == employeeId).FirstOrDefault();
      var businessUnit = BusinessUnits.As(employee.Department.BusinessUnit);
      var lna = businessUnit.LNADirRX;
      if (lna == null)
        return string.Empty;
      
      var text = NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.InstructionForAttachingDocuments.Guid);
      if (string.IsNullOrEmpty(text))
        return string.Empty;
      
      var previewParams = string.Format("{{id: '{0}', name: 'IElectronicDocuments', title: '{1}', system: 'HRPro',}}", lna.Id, lna.Name);
      return string.Format(text, string.Format("<a onClick=\"ESS.preview.openDocumentPreview({0});\">{1}</a>", previewParams, lna.Name));
    }
    
    /// <summary>
    /// Получить имя типа отпуска.
    /// </summary>
    /// <param name="vacationTypeId">Id типа отпуска.</param>
    /// <returns>Json имени.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationTypeName(long vacationTypeId)
    {
      var vacationType = VacationKinds.GetAll().FirstOrDefault(x => x.Id == vacationTypeId);
      return vacationType != null ? vacationType.Name : string.Empty;
    }
    
    /// <summary>
    /// Получить имя причины отпуска.
    /// </summary>
    /// <param name="id">Id причины отпуска.</param>
    /// <returns>Json имени.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationReasonName(long? id)
    {
      var vacationReason = VacationReasonses.GetAll().FirstOrDefault(x => x.Id == id);
      return vacationReason != null ? vacationReason.Name : string.Empty;
    }
    
    /// <summary>
    /// Получить доступные причины отпуска.
    /// </summary>
    /// <param name="vacationTypeId">Id типа отпуска.</param>
    /// <returns>Json список причин отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationTypeReasons(long vacationTypeId)
    {
      var result = new List<DirRX.NNVacations.Structures.Module.VacationReasonInfo>();
      var vacationType = VacationKinds.GetAll().FirstOrDefault(x => x.Id == vacationTypeId);
      if (vacationType != null)
      {
        var vacationReasons = vacationType.ActualAbsenceDirRX.Select(x => x.VacationReason).Where(x => x.Status == NNVacations.VacationReasons.Status.Active);
        foreach (var vacationReason in vacationReasons)
        {
          var reason = NNVacations.Structures.Module.VacationReasonInfo.Create();
          reason.Id = vacationReason.Id;
          reason.Name = vacationReason.Name;
          result.Add(reason);
        }
      }
      return EssPlatform.PublicFunctions.Module.SerializedToJson(result);
    }

    /// <summary>
    /// Получить доступные типы отпусков по виду.
    /// </summary>
    /// <param name="vacationKind">Вид отпуска.</param>
    /// <returns>Json список типов отпусков.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetVacationTypes(string vacationKind)
    {
      var vacationKindEnumeration = new Enumeration(vacationKind);
      var vacationsTypes = VacationKinds.GetAll(x => x.Status == OverrideSolution.VacationKind.Status.Active)
        .Where(v => Equals(v.VacationKindDirRX, vacationKindEnumeration));
      if (vacationKindEnumeration == OverrideSolution.VacationKind.VacationKindDirRX.AnnualLeave)
        vacationsTypes = vacationsTypes.Where(v => !v.AttachableDirRX.GetValueOrDefault());
      return this.GetJsonVacationTypes(vacationsTypes);
    }
    
    /// <summary>
    /// Проверить является ли тип отпуск учебным.
    /// </summary>
    /// <param name="vacationTypeId">Id типа отпуска.</param>
    /// <returns>True, если отпуск "Учебный".</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public bool CheckForStudyVacationType(long vacationTypeId)
    {
      var vacationType = VacationKinds.GetAll().FirstOrDefault(x => x.Id == vacationTypeId);
      return VacationKinds.Equals(this.GetStudyVacationType(Constants.Module.ConstantsSetting.StudyWithSalaryVacationTypeId.Guid), vacationType) ||
        VacationKinds.Equals(this.GetStudyVacationType(Constants.Module.ConstantsSetting.StudyWithoutSalaryVacationTypeId.Guid), vacationType);
    }
    
    /// <summary>
    /// Получить тип отпуска Учебный.
    /// </summary>
    /// <param name="guid">guid прикладной константы.</param>
    /// <returns>Учебный тип отпуска.</returns>
    public IVacationKind GetStudyVacationType(string guid)
    {
      long? studyVacationTypeId = 0;
      Sungero.Core.AccessRights.AllowRead(
        () =>
        {
          studyVacationTypeId = NNCommons.PublicFunctions.Module.GetIdentifierValue(guid);
        });
      return VacationKinds.GetAll(k => k.Id == studyVacationTypeId).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить ИД шаблона заявления на прочий отпуск.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>JSON строку с ИД шаблона "Шаблон заявления на отпуск (ЛК)" и ИД вида документа "Заявление на отпуск".</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string GetOtherVacationStatementTemplateId(long employeeId)
    {
      var documentKindGuid = DocKind.OtherVacationKind;
      var docKind = DocKindFunctions.GetNativeDocumentKind(documentKindGuid);
      var errorText = DirRX.NNVacations.Resources.OtherVacationTemplateNotFoundError;
      var employee = Sungero.Company.Employees.GetAll(e => e.Id == employeeId).FirstOrDefault();
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
      var settingTemplates = HRPro.HRProSolution.DocumentKinds.As(docKind).TemplatesLKToBusinessUnitHRPro;
      var settingTemplate = settingTemplates.Where(t => Equals(t.BusinessUnit, businessUnit));
      if (!settingTemplate.Any())
        settingTemplate = settingTemplates.Where(t => t.BusinessUnit == null);
      var setting = settingTemplate.FirstOrDefault()?.TemplateLK;
      
      var result = NNVacations.Structures.Module.VacationStatementData.Create();
      if (setting != null)
      {
        result.TemplateId = setting.Id;
        result.DocumentKindId = docKind.Id;
        result.Error = string.Empty;
      }
      else
      {
        result.TemplateId = 0;
        result.DocumentKindId = 0;
        result.Error = errorText;
      }
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить срок согласования отпуска.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="documentKindId">ИД вида документа.</param>
    /// <param name="vacationType">Тип отпуска.</param>
    /// <param name="paymentCompleted">Отпуск оплачен.</param>
    /// <param name="isStatement">По заявлению.</param>
    /// <param name="oldStart">Дата начала старого отпуска.</param>
    /// <param name="newStart">Дата начала нового отпуска.</param>
    /// <returns>Срок согласования.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public DateTime GetVacationApprovalDate(long employeeId, long documentKindId, string vacationType, bool paymentCompleted, bool isStatement, string oldStart, string newStart)
    {
      var employee = Employees.GetAll(e => e.Id == employeeId).FirstOrDefault();
      var businessUnitId = employee.BusinessUnit.Id;
      var top = employee.IsNGDDirRX.Value;
      var oldStartDate = DateTime.Parse(oldStart);
      var newStartDate = DateTime.Parse(newStart);
      
      // Получить локализованную строку для планнового типа отпуска.
      var plannedVacation = NNVacations.VacationsApprovalPeriod.VacationType.Planned.Value;
      
      // Определить нужный тип отпуска по локализованному значению из ЛК.
      var vacationTypeEnum = vacationType == plannedVacation?
        NNVacations.VacationsApprovalPeriod.VacationType.Planned :
        NNVacations.VacationsApprovalPeriod.VacationType.Registered;
      
      // Получить запись справочника "Cрок согласования отпуска".
      var vacationApprovalPeriod = PublicFunctions.VacationsApprovalPeriod.GetVacationsApprovalPeriod(businessUnitId, documentKindId, vacationTypeEnum, paymentCompleted, top);
      if (vacationApprovalPeriod == null)
      {
        Logger.DebugFormat("GetVacationApprovalDate. BusinessUnitId: {0}. DocumentKindId: {1}. VacationType: {2}. PaymentCompleted: {3}. Top: {4}. " +
                           "Не найден срок согласования отпуска.", businessUnitId, documentKindId, vacationType, paymentCompleted, top);
        
        // Отправка уведомления администратору.
        var subject = DirRX.NNVacations.Resources.VacationApprovalPeriodNotFoundSubject;
        var activeText = DirRX.NNVacations.Resources.VacationApprovalPeriodNotFoundMessageFormat(businessUnitId, documentKindId,
                                                                                                 VacationsApprovalPeriods.Info.Properties.VacationType.GetLocalizedValue(vacationTypeEnum),
                                                                                                 paymentCompleted ? DirRX.NNVacations.Resources.VacationApprovalPeriodTrueBoolValue : DirRX.NNVacations.Resources.VacationApprovalPeriodFalseBoolValue,
                                                                                                 top ? DirRX.NNVacations.Resources.VacationApprovalPeriodTrueBoolValue : DirRX.NNVacations.Resources.VacationApprovalPeriodFalseBoolValue);
        var processKindId = NNCommons.PublicFunctions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.VacationApprovalDateNotificationProcessId.Guid);
        if (processKindId > 0)
        {
          var task = HRPro.HRDocFlow.HRSimpleTasks.Create();
          task.Subject = subject;
          task.ProcessKind = Sungero.Workflow.ProcessKinds.GetAll(p => p.Id == processKindId).FirstOrDefault();
          task.Performers.AddNew().Performer = Roles.Administrators;
          task.ActiveText = activeText;
          task.Start();
        }
        else
        {
          var notification = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, Roles.Administrators.RecipientLinks.Select(x => x.Member).ToArray());
          notification.ActiveText = activeText;
          notification.Start();
        }
        return oldStartDate < newStartDate ? oldStartDate.BeginningOfWorkingDay() : newStartDate.BeginningOfWorkingDay();
      }
      
      var oldVacationDays = isStatement ?
        vacationApprovalPeriod.StatementOldVacationDays ?? 0 :
        vacationApprovalPeriod.NotificationOldVacationDays ?? 0;
      var nextVacationDays = vacationApprovalPeriod.NextVacationDays ?? 0;
      
      oldStartDate = oldStartDate.AddWorkingDays(-oldVacationDays);
      newStartDate = newStartDate.AddWorkingDays(-nextVacationDays);
      
      return oldStartDate < newStartDate ? oldStartDate : newStartDate;
    }
    
    /// <summary>
    /// Получить срок согласования отпуска.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="documentKindId">ИД вида документа.</param>
    /// <param name="date">Дата начала отпуска.</param>
    /// <returns>Срок согласования.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public DateTime GetDateDeadlineForApprovalLK(long employeeId, long documentKindId, string date)
    {
      var employee = Employees.Get(employeeId);
      var documentKind = DocumentKinds.Get(documentKindId);
      var newDate = DateTime.Parse(date);
      var resDate = GetDateDeadlineForApproval(newDate, employee, documentKind);
      
      if (resDate == null)
        throw new InvalidOperationException(Resources.ErrorDeadlineForApprovalFormat(employeeId));
      
      return resDate.Value;
    }
    
    #endregion
    
    #region Адаптация коробочных функций
    
    /// <summary>
    /// Проверить, что отпуск содержит праздничные дни во время отпуска.
    /// </summary>
    /// <param name="periodBegin">Начало отпуска.</param>
    /// <param name="periodEnd">Окончание отпуска.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="vacationTypeId">ИД типа отпуска.</param>
    /// <param name="vacationReasonId">ИД причины отпуска.</param>
    /// <returns>Текст сообщения о наличии праздничных дней в периоде отпуска.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string CheckVacationHasHoliday(string periodBegin, string periodEnd, long employeeId, long? vacationTypeId, long? vacationReasonId)
    {
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var beginDate = NNIntegration.PublicFunctions.Module.ParseStringToDate(periodBegin);
      var endDate = NNIntegration.PublicFunctions.Module.ParseStringToDate(periodEnd);
      if (beginDate.HasValue && endDate.HasValue && employee != null)
      {
        var isWithHoliday = true;
        var vacationType = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault();
        var vacationReason = VacationReasonses.GetAll(v => v.Id == vacationReasonId).FirstOrDefault();
        if (vacationReason?.CalculationDays != null)
          isWithHoliday = vacationReason.CalculationDays == NNVacations.VacationReasons.CalculationDays.WithHoliday;
        else if (vacationType?.CalculationDaysDirRX != null)
          isWithHoliday = vacationType.CalculationDaysDirRX == OverrideSolution.VacationKind.CalculationDaysDirRX.WithHoliday;
        
        if (isWithHoliday)
        {
          var holidays = HRPro.Vacations.PublicFunctions.Module.Remote.GetHolidaysInPeriod(beginDate, endDate, employee);
          if (holidays.Any())
          {
            var dates = string.Join(", ", holidays.Select(d => d.ToShortDateString()));
            var fullDuration = (int)((TimeSpan)(endDate - beginDate)).TotalDays + 1;
            return HRPro.Vacations.Resources.VacationHasHolidayFormat(dates, fullDuration);
          }
        }
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Получение последнего дня отпуска по виду отпуска. (с учетом рабочего календаря для оплачиваемых).
    /// </summary>
    /// <param name="dateBegin">Начало отпуска.</param>
    /// <param name="duration">Продолжительность отпуска.</param>
    /// <param name="userId">ИД сотрудника.</param>
    /// <param name="vacationTypeId">ИД типа отпуска.</param>
    /// <param name="vacationReasonId">ИД причины отпуска.</param>
    /// <returns>Последний день отпуска с учетом рабочего календаря.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public DateTime GetVacationEndDate(DateTime dateBegin, int duration, long userId, long? vacationTypeId, long? vacationReasonId)
    {
      dateBegin = dateBegin.ToUserTime();
      var employee = Employees.GetAll(x => x.Id == userId).FirstOrDefault();
      var vacationType = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault();
      var vacationReason = VacationReasonses.GetAll(v => v.Id == vacationReasonId).FirstOrDefault();
      var vacationEndDate = this.GetVacationEndDate(dateBegin, duration, employee, vacationType, vacationReason);
      return vacationEndDate.FromUserTime();
    }
    
    /// <summary>
    /// Получить дату окончания отпуска. (с учетом праздничных дней).
    /// </summary>
    /// <param name="dateBegin">Начало.</param>
    /// <param name="duration">Продолжительность отпуска.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <param name="vacationType">Тип отпуска.</param>
    /// <param name="vacationReason">Причина отпуска.</param>
    /// <returns>Дата окончания отпуска с учетом праздничных дней.</returns>
    [Public, Remote]
    public DateTime GetVacationEndDate(DateTime dateBegin, int duration, IRecipient recipient, IVacationKind vacationType, IVacationReasons vacationReason)
    {
      var dateEnd = dateBegin.AddDays(-1);
      var durationWithoutHolidays = 0;
      do
      {
        dateEnd = dateEnd.AddDays(duration - durationWithoutHolidays);
        durationWithoutHolidays = this.DurationInCalendarDaysWithoutHoliday(dateBegin, dateEnd, recipient, vacationType, vacationReason);
      }
      while (durationWithoutHolidays != duration);
      return dateEnd;
    }
    
    /// <summary>
    /// Подсчет календарных дней в промежутке времени за вычетом праздничных дней.
    /// </summary>
    /// <param name="startDate">Начало.</param>
    /// <param name="endDate">Окончание.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Количество календарных дней.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public int DurationInCalendarDaysWithoutHoliday(DateTime startDate, DateTime endDate, long employeeId, long? vacationTypeId, long? vacationReasonId)
    {
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      var recipient = Recipients.As(employee);
      var vacationType = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault();
      var vacationReason = VacationReasonses.GetAll(v => v.Id == vacationReasonId).FirstOrDefault();
      return this.DurationInCalendarDaysWithoutHoliday(startDate, endDate, recipient, vacationType, vacationReason);
    }
    
    /// <summary>
    /// Подсчет календарных дней в промежутке времени за вычетом праздничных дней.
    /// </summary>
    /// <param name="startDate">Начало.</param>
    /// <param name="endDate">Окончание.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <param name="vacationType">Тип отпуска.</param>
    /// <param name="vacationReason">Причина отпуска.</param>
    /// <returns>Количество календарных дней.</returns>
    [Public, Remote]
    public int DurationInCalendarDaysWithoutHoliday(DateTime startDate, DateTime endDate, IRecipient recipient, IVacationKind vacationType, IVacationReasons vacationReason)
    {
      var holidaysCount = 0;
      if (vacationReason?.CalculationDays != null)
        holidaysCount = vacationReason.CalculationDays == NNVacations.VacationReasons.CalculationDays.WithHoliday ? this.GetHolidaysAmountInPeriod(startDate, endDate, recipient) : 0;
      else if (vacationType?.CalculationDaysDirRX != null)
        holidaysCount = vacationType.CalculationDaysDirRX == OverrideSolution.VacationKind.CalculationDaysDirRX.WithHoliday ? this.GetHolidaysAmountInPeriod(startDate, endDate, recipient) : 0;
      else
        holidaysCount = this.GetHolidaysAmountInPeriod(startDate, endDate, recipient);
      return (int)((TimeSpan)(endDate - startDate)).TotalDays + 1 - holidaysCount;
    }
    
    /// <summary>
    /// Вернуть количество праздничных дней в периоде.
    /// </summary>
    /// <param name="periodBegin">Начало периода.</param>
    /// <param name="periodEnd">Конец периода.</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <returns>Количество праздничных дней.</returns>
    public int GetHolidaysAmountInPeriod(DateTime? periodBegin, DateTime? periodEnd, IRecipient recipient)
    {
      // HACK: Копирование коробочной функции.
      if (!periodBegin.HasValue || !periodEnd.HasValue)
        return 0;
      return HRPro.Vacations.PublicFunctions.Module.Remote.GetHolidaysInPeriod(periodBegin, periodEnd, recipient).Count;
    }
    
    #endregion.
    
    #region Разное.
    
    #region Работа с XML.
    
    /// <summary>
    /// Сериализация структуры в xml строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <param name="errorMessage">Ошибка.</param>
    /// <typeparam name="T">Тип структуры.</typeparam>
    /// <returns>Строка xml.</returns>
    private string SerializeInXml<T>(T structure, out string errorMessage)
    {
      errorMessage = string.Empty;
      try
      {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using (var stringWriter = new StringWriter())
        {
          using (var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = System.Xml.Formatting.Indented })
          {
            xmlSerializer.Serialize(xmlTextWriter, (T)structure);
            return stringWriter.ToString();
          }
        }
      }
      catch (Exception e)
      {
        errorMessage = "Произошла ошибка сериализации структуры в xml строку.";
        Logger.ErrorFormat("SerializeInXml. {0} Ошибка: {1}. StackTrace: {2}.", errorMessage, e.Message, e.StackTrace);
      }

      return string.Empty;
    }
    
    #endregion
    
    /// <summary>
    /// Получить шаблон документа по виду.
    /// </summary>
    /// <param name="docKind">Вид документа.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Шаблон.</returns>
    public virtual Sungero.Content.IElectronicDocumentTemplate GetTemplateByKind(Sungero.Docflow.IDocumentKind docKind, IBusinessUnit businessUnit)
    {
      if (docKind == null)
      {
        Logger.Debug("GetTemplateByKind. Не найден вид документа.");
        return null;
      }
      var docKindHRPro =  DocumentKinds.As(docKind);
      if (docKindHRPro.IsNeedTemplatesRXHRPro.HasValue && docKindHRPro.IsNeedTemplatesRXHRPro.Value)
      {
        // Если есть шаблон для конкретной НОР, то использовать его, иначе - шаблон по умолчанию.
        var docKindBUTemplate = docKindHRPro.TemplatesToBusinessUnitHRPro.Where(t => Equals(t.BusinessUnit, businessUnit));
        if (!docKindBUTemplate.Any())
          docKindBUTemplate = docKindHRPro.TemplatesToBusinessUnitHRPro.Where(t => t.BusinessUnit == null);
        
        return Sungero.Content.ElectronicDocumentTemplates.As(docKindBUTemplate.Select(t => t.TemplateRX).First());
      }
      else
      {
        Logger.DebugFormat("GetTemplateByKind. ИД вида документа: {0}. Не найден шаблон для вида документа.", docKind.Id);
        return null;
      }
    }
    
    /// <summary>
    /// Получить подчиненные подразделения.
    /// </summary>
    /// <param name="headDepartments">Головные подразделения.</param>
    /// <returns>Подчиненные подразделения.</returns>
    [Public]
    public List<IDepartment> GetSubDepartments(List<IDepartment> headDepartments)
    {
      var subDepartments = Departments.GetAll().Where(x => x.Status == DirRX.OverrideSolution.Department.Status.Active &&
                                                      headDepartments.Contains(Departments.As(x.HeadOffice))).ToList();
      if (subDepartments.Any())
        subDepartments.AddRange(this.GetSubDepartments(subDepartments));
      
      return subDepartments.Distinct().ToList();
    }

    /// <summary>
    /// Создать и отправить подзадачу на подписание документа.
    /// </summary>
    /// <param name="mainTaskID">ID главной задачи.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="document">Документ.</param>
    private void CreateAndStartNewSignDocumentSubtask(long mainTaskID, OverrideSolution.IEmployee employee, IOfficialDocument document)
    {
      var docTypeGuid = Guid.Parse(document.DocumentKind.DocumentType.DocumentTypeGuid);
      var taskInProcess = HRPro.HRDocFlow.HRProcessTasks.GetAll(x => x.AttachmentDetails.Any(y => y.AttachmentId == document.Id &&
                                                                                             y.EntityTypeGuid == docTypeGuid) &&
                                                                x.Status == Sungero.Workflow.Task.Status.InProcess).FirstOrDefault();
      if (taskInProcess == null)
      {
        var mainTask = Sungero.Workflow.Tasks.GetAll(x => x.Id == mainTaskID).FirstOrDefault();
        var subTask = HRPro.HRDocFlow.HRProcessTasks.CreateAsSubtask(mainTask);
        subTask.Author = employee;
        if (employee.BusinessUnit != null)
          subTask.BusinessUnit = employee.BusinessUnit;
        subTask.NeedApprove = true;
        subTask.NeedAcquaintance = false;
        subTask.DocumentGroup.OfficialDocuments.Add(document);
        
        subTask.Start();
        
        Logger.DebugFormat("CreateAndStartNewSignDocumentSubtask. ИД задачи: {0}. Отправлена новая задача на подписание документа с ИД: {1}. ИД главной задачи: {2}.",
                           subTask.Id, document.Id, mainTaskID);
      }
    }
    
    /// <summary>
    /// Создать или обновить связанный документ.
    /// </summary>
    /// <param name="document">Главный документ.</param>
    /// <param name="documentKindGuid">Guid вида документа.</param>
    /// <param name="relationTypeName">Тип связи.</param>
    /// <param name="body">Тело связанного документа.</param>
    /// <param name="extension">Расширение связанного документа.</param>
    /// <returns>Текст ошибки.</returns>
    private string CreateOrUpdateRelatedDocument(IOfficialDocument document, Guid documentKindGuid, string relationTypeName, string body, string extension)
    {
      Logger.DebugFormat("CreateOrUpdateRelatedDocument. Создание связанного документа. ИД ведущего документа: {0}. Тип связи: {1}", document.Id, relationTypeName);
      var errorMessage = string.Empty;
      var docToRelateKind = DocKindFunctions.GetNativeDocumentKind(documentKindGuid);
      
      var relatedDocs = document.Relations.GetRelatedDocuments().Where(x => OverrideSolution.DocRelatedToWorks.Is(x)).Cast<OverrideSolution.IDocRelatedToWork>();
      var docToRelate = relatedDocs.Where(x => Sungero.Docflow.DocumentKinds.Equals(x.DocumentKind, docToRelateKind)).FirstOrDefault();
      
      if (docToRelate == null) {
        docToRelate = OverrideSolution.DocRelatedToWorks.Create();
        docToRelate.DocumentKind = docToRelateKind;
        document.Relations.Add(relationTypeName, docToRelate);
      }
      
      if (!document.State.IsInserted && !Locks.GetLockInfo(docToRelate).IsLockedByMe && !Locks.TryLock(docToRelate))
      {
        var lockInfo = Locks.GetLockInfo(docToRelate);
        errorMessage = lockInfo.LockedMessage;
        Logger.ErrorFormat("CreateOrUpdateRelatedDocument. ИД ведущего документа: {0}. Ошибка: {1}.", document.Id, errorMessage);
        return errorMessage;
      }
      
      try
      {
        docToRelate.PreparedBy = document.PreparedBy;
        // Документ сохраняется при создании новой версии.
        errorMessage = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(docToRelate, body, extension);
        if (!string.IsNullOrWhiteSpace(errorMessage))
          throw AppliedCodeException.Create(errorMessage);
        
        Logger.DebugFormat("CreateOrUpdateRelatedDocument. ИД ведущего документа: {0}. Документ с ИД: {1} был сохранен.", document.Id, docToRelate.Id);
      }
      catch (Exception ex)
      {
        errorMessage = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", docToRelate.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("CreateOrUpdateRelatedDocument. {0}", errorMessage);
        return errorMessage;
      }
      finally
      {
        if (Locks.GetLockInfo(docToRelate).IsLockedByMe)
          Locks.Unlock(docToRelate);
      }
      
      return errorMessage;
    }
    
    #endregion
    
    #region Интеграционный сценарий "Передача графика отпусков"
    
    #region IF-S-6365. Создание карточки согласования графика отпусков из АСУП в КЭДО.
    
    /// <summary>
    /// Обработка создания или обновления карточки "График отпусков".
    /// </summary>
    /// <param name="idIncomingMessage">Id записи справочника Входящее информационное сообщение.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public NNIntegration.Structures.Module.IProcessIncomingMessageResult ProcessingCreateVacationScheduleDocument(long idIncomingMessage)
    {
      var logPrefix = string.Format("ProcessingCreateVacationScheduleDocument. Ид входящего сообщения: {0}", idIncomingMessage);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = NNIntegration.Structures.Module.ProcessIncomingMessageResult.Create();
      result.Retry = false;
      var incomingMessage = NNIntegration.IncomingMessages.GetAll(c => c.Id == idIncomingMessage).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.Retry = true;
        result.ErrorText = "Не найдена запись справочника Входящие интеграционные сообщения.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      if (string.IsNullOrWhiteSpace(message))
      {
        result.Retry = true;
        result.ErrorText = "Не удалось получить тело запроса из записи справочника.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var structure = NNIntegration.Structures.Module.VacationScheduleDocumentInfoFromSAP.Create();
      try
      {
        structure = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Ошибка десериализации Json.";
        Logger.ErrorFormat("{0}. {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, result.ErrorText, ex.Message, ex.StackTrace);
        return result;
      }
      var doc = this.GetVacationScheduleDocumentBySAPID(structure.SAPID);
      var businessUnit = NNIntegration.PublicFunctions.Module.GetBusinessUnitBySAPId(structure.OrgType + structure.OrgCode);
      
      if (doc == null)
      {
        try
        {
          Logger.DebugFormat("{0}. SAP Id документа: {1}. Создание документа.", logPrefix, structure.SAPID);
          doc = VacationScheduleDocuments.Create();
          doc.BusinessUnit = businessUnit;
          doc.Year = Calendar.GetDate(int.Parse(structure.Year), 1, 1);
          doc.ExternalId = structure.SAPID;
          Locks.Lock(doc);
          var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.VacationScheduleDocumentExtension.Guid);
          var errorMessage = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(doc, structure.Body, extension);
          if (Locks.GetLockInfo(doc).IsLockedByMe)
            Locks.Unlock(doc);
          if (!string.IsNullOrEmpty(errorMessage))
          {
            result.Retry = true;
            result.ErrorText = errorMessage;
            Logger.ErrorFormat("{0}. SAP Id документа: {1}. {2}", logPrefix, structure.SAPID, result.ErrorText);
            return result;
          }
          
          var async = AsyncHandlers.CreateVacationInfoAndStartTask.Create();
          async.IncomingMessageId = idIncomingMessage;
          async.VacationScheduleDocumentId = doc.Id;
          async.ExecuteAsync();
          
          Logger.DebugFormat("{0}. SAP Id документа: {1}. Успешно создан и обработан. Документ ИД: {2}.", logPrefix, structure.SAPID, doc.Id);
          result.Retry = false;
          result.EntityId = doc.Id;
          return result;
        }
        catch (Exception ex)
        {
          result.EntityId = doc?.Id;
          result.ErrorText = string.Format("SAP Id документа: {0}. Произошла ошибка обработки документа. Текст ошибки: {1}.", structure.SAPID, ex.Message);
          Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, result.ErrorText, ex.StackTrace);
        }
      }
      else
      {
        result.EntityId = doc.Id;
        if (doc.LifeCycleState == DirRX.OverrideSolution.VacationScheduleDocument.LifeCycleState.Draft && !doc.InternalApprovalState.HasValue)
        {
          if (Locks.TryLock(doc))
          {
            try
            {
              Logger.DebugFormat("{0}. SAP Id документа: {1}. Получен документ с ИД: {2}.", logPrefix, structure.SAPID, doc.Id);
              doc.BusinessUnit = businessUnit;
              doc.Year = Calendar.GetDate(int.Parse(structure.Year), 1, 1);
              var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.VacationScheduleDocumentExtension.Guid);
              var errorMessage = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(doc, structure.Body, extension);
              if (!string.IsNullOrEmpty(errorMessage))
              {
                result.Retry = true;
                result.ErrorText = errorMessage;
                Logger.ErrorFormat("{0}. SAP Id документа: {1}. {2}", logPrefix, structure.SAPID, result.ErrorText);
                return result;
              }
              
              Logger.DebugFormat("{0}. SAP Id документа: {1}. Документ ИД: {2}. Успешно обработан.", logPrefix, structure.SAPID, doc.Id);
              result.Retry = false;
              return result;
            }
            catch (Exception ex)
            {
              result.ErrorText = string.Format("SAP Id документа: {0}. Документ ИД: {1}. Произошла ошибка обработки документа. Текст ошибки: {2}.", structure.SAPID, doc.Id, ex.Message);
              Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, result.ErrorText, ex.StackTrace);
            }
            finally
            {
              if (Locks.GetLockInfo(doc).IsLockedByMe)
                Locks.Unlock(doc);
            }
          }
          else
          {
            var lockInfo = Locks.GetLockInfo(doc);
            result.ErrorText = string.Format("SAP Id документа: {0}. Документ ИД: {1}. Не удалось заблокировать документ. Заблокирован: {2}. Время установки блокировки: {3}",
                                             structure.SAPID, doc.Id, lockInfo.OwnerName, lockInfo.LockTime);
            Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          }
        }
        else
        {
          result.ErrorText = string.Format("SAP Id документа: {0}. Документ ИД: {1}. Не удовлетворяет условию: Состояние документа HR Pro = В разработке и Согласование = Пусто.", structure.SAPID, doc.Id);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          result.Retry = false;
          return result;
        }
      }
      
      result.Retry = true;
      return result;
    }
    
    /// <summary>
    /// Получить График отпусков по SAP ID.
    /// </summary>
    /// <param name="sapId">SAP ID.</param>
    /// <returns>НОР.</returns>
    public IVacationScheduleDocument GetVacationScheduleDocumentBySAPID(string sapId)
    {
      return VacationScheduleDocuments.GetAll(d => d.ExternalId == sapId).FirstOrDefault();
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий "Перенос отпуска".
    
    #region IF-S-10536. Формирование проводки на перенос отпуска из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Создание структуры для формирования проводки на перенос отпуска.
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="guidMessage">Guid сообщения.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public NNIntegration.Structures.Module.IResultGenerateRequestText CreateRegistrationShiftVacation(long mainTaskId, string guidMessage)
    {
      var logPrefix = string.Format("CreateRegistrationShiftVacation. Ид основной задачи: {0}", mainTaskId);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = VacationStatementDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      logPrefix = string.Format("{0}. Ид документа: {1}", logPrefix, doc.Id);
      
      #region Проверка заполненности полей, подготовка данных.
      
      var errorMessage = string.Empty;
      var emplPersNum = string.Empty;
      var oldDateStart = doc.StartDateDirRX.HasValue ? doc.StartDateDirRX.Value.ToString("dd.MM.yyyy") : string.Empty;
      var oldDateEnd = doc.EndDateDirRX.HasValue ? doc.EndDateDirRX.Value.ToString("dd.MM.yyyy") : string.Empty;
      if (doc.EmployeeDirRX == null)
        errorMessage += "Не заполнено поле Сотрудник. ";
      else if (string.IsNullOrEmpty(doc.EmployeeDirRX.PersonnelNumber))
        errorMessage += string.Format("Не заполнен Табельный номер у сотрудника с ИД: {0}. ", doc.EmployeeDirRX.Id);
      else
        emplPersNum = doc.EmployeeDirRX.PersonnelNumber;
      if (string.IsNullOrEmpty(oldDateStart))
        errorMessage += "Не заполнено поле Дата начала первоначального отпуска. ";
      if (string.IsNullOrEmpty(oldDateEnd))
        errorMessage += "Не заполнено поле Дата окончания первоначального отпуска. ";
      if (!doc.AdditionalVacationDirRX.Any())
        errorMessage += "Не заполнены Данные нового отпуска. ";
      
      var vacationsData = new List<Structures.Module.IVerificationVacationData>();
      foreach (var vacation in doc.AdditionalVacationDirRX)
      {
        if (!vacation.StartDate.HasValue)
          errorMessage += "Не заполнено поле Дата начала в таблице Данные нового отпуска. ";
        if (vacation.VacationType == null)
          errorMessage += "Не заполнено поле Тип отпуска в таблице Данные нового отпуска. ";
        if (vacation.Days == null)
          errorMessage += "Не заполнено поле Кол-во дней в таблице Данные нового отпуска. ";
        
        if (string.IsNullOrEmpty(errorMessage))
        {
          var newVacation = Structures.Module.VerificationVacationData.Create();
          newVacation.BeginDate = vacation.StartDate.Value;
          newVacation.EndDate = vacation.EndDate.Value;
          newVacation.VacationType = vacation.VacationType;
          newVacation.Duration = vacation.Days.Value;
          vacationsData.Add(newVacation);
        }
      }
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return result;
      }
      
      #endregion
      
      var listOldVacations = new List<Structures.Module.IOldVacationInfoIFS10536>();
      var listVacations = new List<Structures.Module.IVacationInfoIFS10536>();
      try
      {
        var confirmTask = RegistrationOfAnnualLeaves.GetAll(t => t.ShiftTaskId == mainTaskId).FirstOrDefault();
        var notice = confirmTask?.DocumentGroup.VacationNoticeDocuments.FirstOrDefault();
        var structure = Structures.Module.VacationInfoIFS10536.Create();
        listOldVacations = notice != null ? this.CreateListOldVacationsForSendToSAP(notice) : this.CreateListOldVacationsForSendToSAP(doc);
        listVacations = this.CreateListVacationsForSendToSAP(notice, doc, listOldVacations, vacationsData, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка при формировании списков отпусков для отправки в SAP.";
        Logger.ErrorFormat("{0}. При формировании списка отпусков для формирования проводки на перенос отпуска возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      var model = Structures.Module.RegistrationShiftVacation.Create(guidMessage,
                                                                     emplPersNum,
                                                                     listOldVacations,
                                                                     mainTaskId,
                                                                     listVacations);
      
      try
      {
        result.RequestText = NNVacations.IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для формирования проводки на перенос отпуска.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для формирования проводки на перенос отпуска возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      return result;
    }
    
    #endregion
    
    #region IF-S-10406. Проверка заявления на перенос отпуска (АСУП -> КЭДО).
    
    /// <summary>
    /// Проверить отпуска сотрудника при оформлении заявления на перенос отпуска в SAP.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="noticeId">ИД уведомления о начале отпуска.</param>
    /// <param name="vacations">Список новых отпусков.</param>
    /// <param name="oldVacations">Список старых отпусков.</param>
    /// <returns>Структура с ответными данными.</returns>
    public Structures.Module.IValidateVacationResult ValidateEmployeeShiftVacations(long employeeId, string personnelNumber, long noticeId,
                                                                                    List<Structures.Module.IVerificationVacationData> vacations,
                                                                                    List<Structures.Module.IFormattedVacationForShiftByStatement> oldVacations)
    {
      var logPrefix = string.Format("ValidateEmployeeShiftVacations. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create();
      
      var resultRequest = this.GenerateStructForValidateEmployeeShiftVacations(employeeId, personnelNumber, noticeId, vacations, oldVacations);
      if (!string.IsNullOrEmpty(resultRequest.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка формирования текста запроса. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.RequestTextGenerationError.ToString();
        return result;
      }
      
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.ValidateEmployeeShiftVacations;
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(resultRequest.RequestText, flowGuid, employeeId);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.SendMessageToExternalSystemError.ToString();
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.ValidateEmployeeVacationsResult.Create();
      try
      {
        Logger.DebugFormat("{0}. AnswerText: {1}", logPrefix, resultSendingMessage.AnswerText);
        model = DirRX.NNVacations.IsolatedFunctions.Json.DeserializeInStruct(resultSendingMessage.AnswerText, model);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        result.ErrorText = Resources.DeserializeInStructError.ToString();
        return result;
      }
      
      if (!string.IsNullOrEmpty(model.ErrorText))
      {
        if (model.Result == true)
          result.IsVacationPeriodBroken = model.ErrorText;
        else
          result.ErrorText = model.ErrorText;
      }
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return result;
    }
    
    /// <summary>
    /// Создание структуры проверки данных при оформлении заявления на перенос отпуска в SAP.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="noticeId">ИД уведомления о начале отпуска.</param>
    /// <param name="vacations">Список новых отпусков.</param>
    /// <param name="oldVacations">Список старых отпусков.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public NNIntegration.Structures.Module.IResultGenerateRequestText GenerateStructForValidateEmployeeShiftVacations(long employeeId, string personnelNumber, long noticeId,
                                                                                                                      List<Structures.Module.IVerificationVacationData> vacations,
                                                                                                                      List<Structures.Module.IFormattedVacationForShiftByStatement> oldVacations)
    {
      var logPrefix = string.Format("GenerateStructForValidateEmployeeShiftVacations. ИД сотрудника: {0}", employeeId);
      var result = DirRX.NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      Logger.DebugFormat("{0}. Запуск создания структуры проверки данных по отпускам в SAP.", logPrefix);
      
      var vacationsList = new List<Structures.Module.IVacationInfoIFS10406>();
      var listOldVacations = new List<Structures.Module.IOldVacationInfoIFS10536>();
      try
      {
        var notice = VacationNoticeDocuments.GetAll(d => d.Id == noticeId).FirstOrDefault();
        var structure = Structures.Module.VacationInfoIFS10406.Create();
        listOldVacations = notice != null ? this.CreateListOldVacationsForSendToSAP(notice) : this.CreateListOldVacationsForSendToSAP(oldVacations);
        vacationsList = this.CreateListVacationsForSendToSAP(notice, oldVacations, listOldVacations, vacations, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка при формировании списка отпусков для отправки в SAP.";
        Logger.ErrorFormat("{0}. При формировании списка отпусков для проверки данных по переносу отпуска в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.EmployeeShiftVacationsValidation.Create(
        Guid.NewGuid().ToString(),
        personnelNumber,
        listOldVacations,
        vacationsList);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры проверки данных по переносу отпуска в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры проверки данных по переносу отпуска в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #region Сформировать список отпусков для отправки в SAP.
    
    #region Сформировать список старых отпусков для отправки в SAP.
    
    /// <summary>
    /// Сформировать список старых отпусков для отправки в SAP для переноса отпуска.
    /// </summary>
    /// <param name="notice">Уведомление о начале отпуска.</param>
    /// <returns>Список отпусков для отправки в SAP.</returns>
    public List<Structures.Module.IOldVacationInfoIFS10536> CreateListOldVacationsForSendToSAP(IVacationNoticeDocument notice)
    {
      var listOldVacations = new List<Structures.Module.IOldVacationInfoIFS10536>();
      foreach (var vacation in notice.VacationDataDirRX)
      {
        var oldVacation = Structures.Module.OldVacationInfoIFS10536.Create();
        oldVacation.VacationCode = Guid.NewGuid().ToString();
        oldVacation.TypeOldVacation = vacation.PlannedAbsence;
        oldVacation.OldDateStart = vacation.StartDate.Value.ToString("dd.MM.yyyy");
        oldVacation.OldDateEnd = vacation.EndDate.Value.ToString("dd.MM.yyyy");
        listOldVacations.Add(oldVacation);
      }
      return listOldVacations;
    }
    
    /// <summary>
    /// Сформировать список старых отпусков для отправки в SAP для проверки переноса отпуска по заявлению.
    /// </summary>
    /// <param name="oldVacations">Список старых отпусков.</param>
    /// <returns>Список отпусков для отправки в SAP.</returns>
    public List<Structures.Module.IOldVacationInfoIFS10536> CreateListOldVacationsForSendToSAP(List<Structures.Module.IFormattedVacationForShiftByStatement> oldVacations)
    {
      var listOldVacations = new List<Structures.Module.IOldVacationInfoIFS10536>();
      foreach (var vacation in oldVacations)
      {
        var oldVacation = Structures.Module.OldVacationInfoIFS10536.Create();
        oldVacation.VacationCode = Guid.NewGuid().ToString();
        oldVacation.TypeOldVacation = vacation.Type;
        oldVacation.OldDateStart = vacation.DateStart.ToString("dd.MM.yyyy");
        oldVacation.OldDateEnd = vacation.DateEnd.ToString("dd.MM.yyyy");
        listOldVacations.Add(oldVacation);
      }
      return listOldVacations;
    }
    
    /// <summary>
    /// Сформировать список старых отпусков для отправки в SAP для формирования проводки на перенос отпуска по заявлению.
    /// </summary>
    /// <param name="statement">Заявление на перенос отпуска.</param>
    /// <returns>Список отпусков для отправки в SAP.</returns>
    public List<Structures.Module.IOldVacationInfoIFS10536> CreateListOldVacationsForSendToSAP(IVacationStatementDocument statement)
    {
      var listOldVacations = new List<Structures.Module.IOldVacationInfoIFS10536>();
      foreach (var vacation in statement.VacationDataDirRX)
      {
        var oldVacation = Structures.Module.OldVacationInfoIFS10536.Create();
        oldVacation.VacationCode = Guid.NewGuid().ToString();
        oldVacation.TypeOldVacation = vacation.Absence;
        oldVacation.OldDateStart = vacation.StartDate.Value.ToString("dd.MM.yyyy");
        oldVacation.OldDateEnd = vacation.EndDate.Value.ToString("dd.MM.yyyy");
        listOldVacations.Add(oldVacation);
      }
      return listOldVacations;
    }
    
    #endregion
    
    #region Сформировать список новых отпусков для отправки в SAP.
    
    /// <summary>
    /// Сформировать список отпусков для отправки в SAP для формирования проводки на перенос отпуска.
    /// </summary>
    /// <param name="notice">Уведомление о начале отпуска.</param>
    /// <param name="statement">Заявление на перенос отпуска.</param>
    /// <param name="listOldVacations">Список старых отпусков.</param>
    /// <param name="vacationsData">Список новых отпусков.</param>
    /// <param name="structure">Структура.</param>
    /// <returns>Список отпусков для отправки в SAP.</returns>
    public List<Structures.Module.IVacationInfoIFS10536> CreateListVacationsForSendToSAP(IVacationNoticeDocument notice,
                                                                                         IVacationStatementDocument statement,
                                                                                         List<Structures.Module.IOldVacationInfoIFS10536> listOldVacations,
                                                                                         List<Structures.Module.IVerificationVacationData> vacationsData,
                                                                                         Structures.Module.IVacationInfoIFS10536 structure)
    {
      var oldVacationsDictionary = new Dictionary<Structures.Module.IOldVacationInfoIFS10536, int>();
      if (notice != null)
      {
        var vacationData = notice.VacationDataDirRX.ToList();
        for (var i = 0; i < vacationData.Count; i++)
          oldVacationsDictionary.Add(listOldVacations[i], vacationData[i].Days.Value);
      }
      else
      {
        var vacationData = statement.VacationDataDirRX.ToList();
        for (var i = 0; i < vacationData.Count; i++)
          oldVacationsDictionary.Add(listOldVacations[i], vacationData[i].Days.Value);
      }
      
      var listVacations = new List<Structures.Module.IVacationInfoIFS10536>();
      foreach (var vacation in vacationsData)
      {
        // Фиксируем исходную продолжительность отпуска.
        var duration = vacation.Duration;
        
        // Разбиваем отпуск на части, пока не распределим все дни.
        while (duration > 0)
        {
          // Ищем подходящий старый отпуск с доступными днями и совместимым типом.
          var oldVacation = oldVacationsDictionary.FirstOrDefault(v => v.Value > 0 &&
                                                                  (v.Key.TypeOldVacation == vacation.VacationType.PlannedAbsenceDirRX ||
                                                                   v.Key.TypeOldVacation == vacation.VacationType.PlannedAbsenceShiftDirRX ||
                                                                   v.Key.TypeOldVacation == vacation.VacationType.SAPIdDirRX));
          var newVacation = Structures.Module.VacationInfoIFS10536.Create();
          
          // Рассчитываем дату начала: начало отпуска + смещение (учитывает уже распределенные дни).
          var startDate = vacation.BeginDate.AddDays(vacation.Duration - duration);
          newVacation.DateStart = startDate.ToString("dd.MM.yyyy");
          
          // Рассчитываем дату окончания:
          // - Если дней достаточно, берем исходную дату окончания
          // - Если дней недостаточно, вычисляем дату окончания на основе доступных дней.
          newVacation.DateEnd = duration <= oldVacation.Value ? vacation.EndDate.ToString("dd.MM.yyyy") : startDate.AddDays(oldVacation.Value - 1).ToString("dd.MM.yyyy");
          newVacation.VacationCode = oldVacation.Key.VacationCode;
          newVacation.TypeVacation = oldVacation.Key.TypeOldVacation;
          
          // Определяем количество дней для новой записи:
          // Берем либо оставшиеся дни нового отпуска, либо доступные дни старого.
          newVacation.DaysCount = duration <= oldVacation.Value ? duration : oldVacation.Value;
          
          // Уменьшаем количество доступных дней в старом отпуске.
          oldVacationsDictionary[oldVacation.Key] -= newVacation.DaysCount;
          
          // Уменьшаем оставшуюся продолжительность нового отпуска.
          duration -= newVacation.DaysCount;
          listVacations.Add(newVacation);
        }
      }
      
      return listVacations;
    }
    
    /// <summary>
    /// Сформировать список отпусков для отправки в SAP для проверки данных по переносу отпуска.
    /// </summary>
    /// <param name="notice">Уведомление о начале отпуска.</param>
    /// <param name="oldVacations">Список старых отпусков.</param>
    /// <param name="listOldVacations">Список старых отпусков в формате для отправки в SAP.</param>
    /// <param name="vacationsData">Список новых отпусков.</param>
    /// <param name="structure">Структура.</param>
    /// <returns>Список отпусков для отправки в SAP.</returns>
    public List<Structures.Module.IVacationInfoIFS10406> CreateListVacationsForSendToSAP(IVacationNoticeDocument notice,
                                                                                         List<Structures.Module.IFormattedVacationForShiftByStatement> oldVacations,
                                                                                         List<Structures.Module.IOldVacationInfoIFS10536> listOldVacations,
                                                                                         List<Structures.Module.IVerificationVacationData> vacationsData,
                                                                                         Structures.Module.IVacationInfoIFS10406 structure)
    {
      var oldVacationsDictionary = new Dictionary<Structures.Module.IOldVacationInfoIFS10536, int>();
      if (notice != null)
      {
        var vacationData = notice.VacationDataDirRX.ToList();
        for (var i = 0; i < vacationData.Count; i++)
          oldVacationsDictionary.Add(listOldVacations[i], vacationData[i].Days.Value);
      }
      else
      {
        for (var i = 0; i < oldVacations.Count; i++)
          oldVacationsDictionary.Add(listOldVacations[i], oldVacations[i].DaysCount);
      }
      
      var listVacations = new List<Structures.Module.IVacationInfoIFS10406>();
      foreach (var vacation in vacationsData)
      {
        // Фиксируем исходную продолжительность отпуска.
        var duration = vacation.Duration;
        
        // Разбиваем отпуск на части, пока не распределим все дни.
        while (duration > 0)
        {
          var oldVacation = oldVacationsDictionary.FirstOrDefault(v => v.Value > 0 &&
                                                                  (v.Key.TypeOldVacation == vacation.VacationType.PlannedAbsenceDirRX ||
                                                                   v.Key.TypeOldVacation == vacation.VacationType.PlannedAbsenceShiftDirRX ||
                                                                   v.Key.TypeOldVacation == vacation.VacationType.SAPIdDirRX));
          var newVacation = Structures.Module.VacationInfoIFS10406.Create();
          
          // Рассчитываем дату начала: начало отпуска + смещение (учитывает уже распределенные дни).
          var startDate = vacation.BeginDate.AddDays(vacation.Duration - duration);
          newVacation.VacationCode = oldVacation.Key.VacationCode;
          newVacation.TypeVacation = oldVacation.Key.TypeOldVacation;
          newVacation.DateStart = startDate.ToString("dd.MM.yyyy");
          
          // Рассчитываем дату окончания:
          // - Если дней достаточно, берем исходную дату окончания
          // - Если дней недостаточно, вычисляем дату окончания на основе доступных дней.
          newVacation.DateEnd = duration <= oldVacation.Value ? vacation.EndDate.ToString("dd.MM.yyyy") : startDate.AddDays(oldVacation.Value - 1).ToString("dd.MM.yyyy");
          
          // Определяем количество дней для новой записи:
          // Берем либо оставшиеся дни нового отпуска, либо доступные дни старого.
          newVacation.DaysCount = duration <= oldVacation.Value ? duration : oldVacation.Value;
          
          // Уменьшаем количество доступных дней в старом отпуске.
          oldVacationsDictionary[oldVacation.Key] -= newVacation.DaysCount;
          
          // Уменьшаем оставшуюся продолжительность нового отпуска.
          duration -= newVacation.DaysCount;
          listVacations.Add(newVacation);
        }
      }
      
      return listVacations;
    }
    
    #endregion
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий "Оформление ежегодного отпуска по графику".
    
    #region IF-S-9336. Передача уведомления об отпуске из АСУП в КЭДО (АСУП-> КЭДО).
    
    /// <summary>
    /// Запуск АО Автоматическое формирование задач на подтверждение отпуска.
    /// </summary>
    /// <param name="incomingMessageId">ИД записи входящих сообщений.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public NNIntegration.Structures.Module.IProcessIncomingMessageResult CreateVacationNoticesAndStartTasks(long incomingMessageId)
    {
      var logPrefix = string.Format("CreateVacationNoticesAndStartTasks. ИД записи справочника Входящие интеграционные сообщения: {0}", incomingMessageId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = NNIntegration.Structures.Module.ProcessIncomingMessageResult.Create();
      result.Retry = false;
      result.ErrorText = string.Empty;
      var incomingMessage = NNIntegration.IncomingMessages.GetAll(m => m.Id == incomingMessageId).FirstOrDefault();
      if (incomingMessage == null)
      {
        result.ErrorText += "Не найдено входящее сообщение";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      var messageText = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      if (string.IsNullOrEmpty(messageText))
      {
        result.ErrorText += "Пустой текст сообщения";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      var async = NNVacations.AsyncHandlers.CreateVacationNoticesAndStartTasks.Create();
      async.IncomingMessageId = incomingMessageId;
      async.ExecuteAsync();
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return result;
    }
    
    #endregion
    
    #region IF-S-6373. Регистрация ежегодного отпуска (по графику отпусков) из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Создание структуры для регистрации ежегодного отпуска по графику отпусков в SAP.
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="guidMessage">Guid сообщения.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public NNIntegration.Structures.Module.IResultGenerateRequestText CreateRegistrationAnnualVacationBySchedule(long mainTaskId, string guidMessage)
    {
      var logPrefix = string.Format("CreateRegistrationAnnualVacationBySchedule. Ид основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = VacationNoticeDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      logPrefix = string.Format("{0}. Ид документа: {1}", logPrefix, doc.Id);
      
      var errorMessage = string.Empty;
      var emplPersNum = string.Empty;
      if (doc.EmployeeDirRX == null)
        errorMessage += "Не заполнено поле Сотрудник. ";
      else if (string.IsNullOrEmpty(doc.EmployeeDirRX.PersonnelNumber))
        errorMessage += string.Format("Не заполнен Табельный номер у сотрудника с ИД: {0}. ", doc.EmployeeDirRX.Id);
      else
        emplPersNum = doc.EmployeeDirRX.PersonnelNumber;
      if (!doc.VacationDataDirRX.Any())
        errorMessage += "Не заполнены Данные отпуска. ";
      
      var listVacations = new List<Structures.Module.IVacationInfoIFS6373>();
      foreach (var vacation in doc.VacationDataDirRX)
      {
        var dateStart = vacation.StartDate.HasValue ? vacation.StartDate.Value.ToString("dd.MM.yyyy") : string.Empty;
        var typeVacation = vacation.PlannedAbsence;
        var days = vacation.Days;
        if (string.IsNullOrEmpty(dateStart))
          errorMessage += "Не заполнено поле Дата начала в таблице Данные отпуска. ";
        if (string.IsNullOrEmpty(typeVacation))
          errorMessage += "Не заполнено поле Вид планового отпуска в таблице Данные отпуска. ";
        if (days == null)
          errorMessage += "Не заполнено поле Кол-во дней в таблице Данные отпуска. ";
        
        if (string.IsNullOrEmpty(errorMessage))
        {
          var vacInfo = Structures.Module.VacationInfoIFS6373.Create();
          vacInfo.DateStart = dateStart;
          vacInfo.TypeVacation = typeVacation;
          vacInfo.DayCount = days.Value;
          listVacations.Add(vacInfo);
        }
      }
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return result;
      }
      
      var model = Structures.Module.RegistrationAnnualVacationBySchedule.Create(guidMessage,
                                                                                emplPersNum,
                                                                                mainTaskId,
                                                                                listVacations);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для регистрации ежегодного отпуска по графику отпусков в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для регистрации ежегодного отпуска по графику отпусков в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      return result;
    }
    
    #endregion
    
    #region IF-S-25553. Передача статусов отправки уведомлений об отпуске из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Передача статуса отправки уведомления об отпуске в SAP.
    /// </summary>
    /// <param name="vacationInfo">Структура с информацией по отпуску сотрудника.</param>
    /// <param name="result">Результат обработки.</param>
    /// <param name="errorText">Текст ошибки.</param>
    [Public]
    public void SendingStatusVacationNoticeToSap(NNIntegration.Structures.Module.IVacationInfoDateTime vacationInfo, string result, string errorText)
    {
      var logPrefix = string.Format("SendingStatusVacationNoticeToSap. Табельный номер сотрудника: {0}", vacationInfo.PersonnelNumber);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var resultRequest = DirRX.NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      var model = DirRX.NNVacations.Structures.Module.VacationInfoIFS25553.Create(vacationInfo.PersonnelNumber,
                                                                                  vacationInfo.DateStart.ToString("dd.MM.yyyy"),
                                                                                  vacationInfo.DateEnd.ToString("dd.MM.yyyy"),
                                                                                  vacationInfo.TypeVacation,
                                                                                  result,
                                                                                  errorText);
      
      try
      {
        resultRequest.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры передачи статуса отправки уведомления об отпуске в SAP.", logPrefix);
        // TODO Логирование для тестирования.
        Logger.DebugFormat("{0}. TODO Информация об отпуске: {1}.", logPrefix, resultRequest.RequestText);
      }
      catch (Exception ex)
      {
        resultRequest.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. Ошибка формирования текста запроса. При создании структуры передачи статуса отправки уведомления об отпуске в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      // Отправить исходящее сообщение.
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.SendStatusVacationNoticeInSap;
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(resultRequest.RequestText, flowGuid, 0);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultSendingMessage.ErrorText);
      }
      Logger.DebugFormat("{0}. Конец обработки", logPrefix);
    }
    
    
    /// <summary>
    /// Отправить на почту письмо со списком работников, по которым не удалось отправить уведомление.
    /// </summary>
    /// <param name="errorEmployees">Список информаций по отпускам с ошибочными сотрудниками.</param>
    /// <param name="remainingEmployees">Список необработанных информаций по отпускам.</param>
    /// <param name="email">Адрес эл. почты.</param>
    [Public]
    public void SendVacationNoticeErrorToEmail(List<NNIntegration.Structures.Module.IVacationInfoDateTime> errorEmployees,
                                               List<NNIntegration.Structures.Module.IVacationInfoDateTime> remainingEmployees,
                                               string email)
    {
      var logPrefix = "SendVacationNoticeErrorToEmail";
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var table = DirRX.NNVacations.Resources.VacationNoticeMessageHeader;
      foreach(var personnelNumber in errorEmployees.Select(e => e.PersonnelNumber).Distinct())
      {
        table += DirRX.NNVacations.Resources.VacationNoticeMessageRowFormat(personnelNumber, string.Empty, "Не найден сотрудник по ТН");
      }
      foreach(var personnelNumber in remainingEmployees.Select(e => e.PersonnelNumber).Distinct())
      {
        var employee = Employees.GetAll(e => e.PersonnelNumber == personnelNumber).FirstOrDefault();
        table += DirRX.NNVacations.Resources.VacationNoticeMessageRowFormat(personnelNumber, employee?.Name, "Ошибка при отправке уведомления");
      }
      
      var style = DirRX.NNVacations.Resources.VacationNoticeMessageStyle;
      var message = Mail.CreateMailMessage();
      message.Subject = DirRX.NNVacations.Resources.VacationNoticeMessageSubject;
      message.Body = DirRX.NNVacations.Resources.VacationNoticeMessageBodyFormat(style, table);
      message.IsBodyHtml = true;
      message.Priority = MailPriority.High;
      message.To.Add(email);
      
      try
      {
        Mail.Send(message);
        Logger.DebugFormat("{0}. Письмо отправлено на почту.", logPrefix);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Произошла ошибка при отправке письма на почту: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      Logger.DebugFormat("{0}. Конец обработки", logPrefix);
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий "Оформление отпуска по заявлению".
    
    #region IF-S-10403. Передача данных о доступных остатках отпуска сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Получить доступные лимиты по отпускам сотрудника из SAP.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Структура доступных лимитов.</returns>
    public DirRX.NNVacations.Structures.Module.IFormattedVacationAvailableLimits GetVacationAvailableLimits(long employeeId)
    {
      var logPrefix = string.Format("GetVacationAvailableLimits. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки", logPrefix);
      var model = DirRX.NNVacations.Structures.Module.FormattedVacationAvailableLimits.Create();
      
      var resultRequest = this.GenerateStructForVacationAvailableLimits(employeeId);
      if (!string.IsNullOrEmpty(resultRequest.ErrorText))
      {
        var errorText = "Ошибка формирования текста запроса";
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}.", logPrefix, errorText, resultRequest.ErrorText);
        model.ErrorText = errorText;
        return model;
      }
      
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.GetVacationAvailableLimits;
      
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(resultRequest.RequestText, flowGuid, employeeId);
      
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        var errorText = "Ошибка отправки исходящего сообщения";
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}.", logPrefix, errorText, resultRequest.ErrorText);
        model.ErrorText = errorText;
        return model;
      }
      
      try
      {
        Logger.DebugFormat("{0}. AnswerText: {1}", logPrefix, resultSendingMessage.AnswerText);
        model = DirRX.NNVacations.IsolatedFunctions.Json.DeserializeInStruct(resultSendingMessage.AnswerText, model);
      }
      catch (Exception ex)
      {
        var errorText = "Ошибка десериализации в структуру";
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorText, ex.Message, ex.StackTrace);
        model.ErrorText = errorText;
        return model;
      }
      
      if (!string.IsNullOrEmpty(model.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка выполнения: {1}.", logPrefix, model.ErrorText);
        return model;
      }
      
      Logger.DebugFormat("{0}. Конец обработки", logPrefix);
      return model;
    }
    
    /// <summary>
    /// Получение доступных лимитов по отпускам сотрудника.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public DirRX.NNIntegration.Structures.Module.IResultGenerateRequestText GenerateStructForVacationAvailableLimits(long employeeId)
    {
      var logPrefix = string.Format("GenerateStructForVacationAvailableLimits. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Запуск создания структуры для получения доступных лимитов в SAP.", logPrefix);
      var result = DirRX.NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var employee = Employees.GetAll().FirstOrDefault(x => x.Id == employeeId);
      if (employee == null || string.IsNullOrEmpty(employee.PersonnelNumber))
      {
        result.ErrorText = "Не удалось получить табельный номер сотрудника";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.EmployeeVacationAvailableLimits.Create(
        Guid.NewGuid().ToString(),
        employee.PersonnelNumber);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для получения доступных лимитов в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры получения доступных лимитов в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #region IF-S-10405. Проверка данных при оформлении заявления на отпуск (АСУП -> КЭДО).
    
    /// <summary>
    /// Проверить отпуска сотрудника при оформлении заявления на отпуск в SAP.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="vacations">Список отпусков.</param>
    /// <returns>Структура с ответными данными.</returns>
    public Structures.Module.IValidateVacationResult ValidateEmployeeVacations(long employeeId, string personnelNumber, List<Structures.Module.IVerificationVacationData> vacations)
    {
      var logPrefix = string.Format("ValidateEmployeeVacations. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create();
      
      var resultRequest = this.GenerateStructForValidateEmployeeVacations(employeeId, personnelNumber, vacations);
      if (!string.IsNullOrEmpty(resultRequest.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка формирования текста запроса. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.RequestTextGenerationError.ToString();
        return result;
      }
      
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.ValidateEmployeeVacations;
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(resultRequest.RequestText, flowGuid, employeeId);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.SendMessageToExternalSystemError.ToString();
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.EmployeeVacationsValidationResult.Create();
      try
      {
        Logger.DebugFormat("{0}. AnswerText: {1}", logPrefix, resultSendingMessage.AnswerText);
        model = DirRX.NNVacations.IsolatedFunctions.Json.DeserializeInStruct(resultSendingMessage.AnswerText, model);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        result.ErrorText = Resources.DeserializeInStructError.ToString();
        return result;
      }
      // Заполняем количество дней превышения из SAP.
      result.VacationDaysExcess = model.AdvanceDays ?? 0;
      
      if (model.Result == false && !string.IsNullOrEmpty(model.ErrorText))
        result.ErrorText = model.ErrorText;
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return result;
    }
    
    /// <summary>
    /// Создание структуры проверки данных при оформлении заявления на отпуск в SAP.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="vacations">Список отпусков.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public NNIntegration.Structures.Module.IResultGenerateRequestText GenerateStructForValidateEmployeeVacations(long employeeId, string personnelNumber,
                                                                                                                 List<Structures.Module.IVerificationVacationData> vacations)
    {
      var logPrefix = string.Format("GenerateStructForValidateEmployeeVacations. ИД сотрудника: {0}", employeeId);
      var result = DirRX.NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      Logger.DebugFormat("{0}. Запуск создания структуры проверки данных по отпускам в SAP.", logPrefix);
      
      var vacationsList = new List<Structures.Module.IVacationData>();
      foreach (var vacation in vacations)
      {
        var vacationData = Structures.Module.VacationData.Create();
        vacationData.DateStart = vacation.BeginDate.ToString("dd.MM.yyyy");
        vacationData.DateEnd = vacation.EndDate.ToString("dd.MM.yyyy");
        vacationData.DaysCount = vacation.Duration;
        vacationData.Type = vacation.VacationType?.SAPIdDirRX;
        vacationsList.Add(vacationData);
      }
      
      var model = DirRX.NNVacations.Structures.Module.EmployeeVacationsValidation.Create(
        Guid.NewGuid().ToString(),
        personnelNumber,
        vacationsList);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры проверки данных по ежегодному отпуску в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры проверки данных по ежегодному отпуску в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #region IF-S-10408. Передача приказа на предоставление отпуска из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Изменить или создать приказ на ежегодный отпуск.
    /// </summary>
    /// <param name="idIncomingMessage">Id записи справочника Входящее информационное сообщение.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public NNIntegration.Structures.Module.IProcessIncomingMessageResult UpdateOrCreateAnnualVacationOrder(long idIncomingMessage)
    {
      var logPrefix = string.Format("UpdateOrCreateAnnualVacationOrder. Ид входящего сообщения: {0}.", idIncomingMessage);
      Logger.Debug(logPrefix);
      
      var result = NNIntegration.Structures.Module.ProcessIncomingMessageResult.Create();
      result.Retry = false;
      var errorMessage = string.Empty;
      
      var message = NNIntegration.IncomingMessages.GetAll(x => x.Id == idIncomingMessage).FirstOrDefault();
      if (message == null)
      {
        result.ErrorText = "Не найдена запись справочника Входящие интеграционные сообщения.";
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var jsonMessage = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(message);
      
      if (string.IsNullOrWhiteSpace(jsonMessage))
      {
        result.ErrorText = "Не удалось получить тело запроса.";
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var model = NNIntegration.Structures.Module.AnnualVacationOrderCard.Create();
      try
      {
        model = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка десериализации в структуру AnnualVacationOrderCard.";
        Logger.ErrorFormat("{0} {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorMessage, ex.Message, ex.StackTrace);
        return result;
      }
      
      var employeeResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Employee);
      var signedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Signed);
      var preparedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Prepared);
      
      if (!string.IsNullOrWhiteSpace(employeeResult.ErrorMessage))
        errorMessage += string.Format("EmployeeError: {0}. ", employeeResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(signedResult.ErrorMessage))
        errorMessage += string.Format("SignedError: {0}. ", signedResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(preparedResult.ErrorMessage))
        errorMessage += string.Format("PreparedError: {0}.", preparedResult.ErrorMessage);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0} {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var employee = employeeResult.Employee;
      var signed = signedResult.Employee;
      var prepared = preparedResult.Employee;
      
      Transactions.Execute(
        () => {
          
          // Поиск / создание нового документа и заполнение его полей.
          var document = OverrideSolution.PublicFunctions.VacationOrderDocument.GetVacationOrderDocumentBySAPID(model.SAPID);
          if (document == null)
          {
            Logger.DebugFormat("{0} Документ по SAPID: {1} не найден. Создание нового документа.", logPrefix, model.SAPID);
            
            // Определить гуид вида документа по его SAP коду.
            var docKindGuid = NNVacations.Functions.Module.GetVacationOrderDocKindGuidBySapCode(model.Kind);
            if (docKindGuid == null)
            {
              result.ErrorText = string.Format("Не удалось определить вид документа приказа по его коду SAP. Полученное значение: {0}.", model.Kind);
              Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
              return;
            }
            
            document = OverrideSolution.PublicFunctions.VacationOrderDocument.CreateVacationDocumentWithEmployee(employee, docKindGuid.Value);
          }
          
          result.EntityId = document?.Id;
          
          if (document.LifeCycleState != HRPro.HRProSolution.OfficialDocument.LifeCycleState.Draft || document.InternalApprovalState.HasValue)
          {
            result.ErrorText = string.Format("Версия документа с ID: {0} не создана. Создать новую версию можно только для документа, у которого значения полей жизненного цикла: " +
                                             "\"Состояние документа HR Pro\" - \"В разработке\", \"Согласование\" - не заполнено.", document.Id);
            Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
            return;
          }
          
          if (!Locks.GetLockInfo(document).IsLockedByMe && !Locks.TryLock(document))
          {
            var lockInfo = Locks.GetLockInfo(document);
            result.ErrorText = lockInfo.LockedMessage;
            Logger.ErrorFormat("{0} {1}", logPrefix, errorMessage);
            return;
          }
          
          try
          {
            document.Employee = employee;
            document.OurSignatory = signed;
            document.PreparedBy = prepared;
            document.RegistrationNumber = model.RegNumber;
            document.RegistrationDate = model.Date;
            document.ExternalId = model.SAPID;
            var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.AnnualVacationDocumentExtension.Guid);
            // Документ сохраняется при создании новой версии.
            errorMessage = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, model.Body, extension);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
              result.ErrorText = errorMessage;
              result.Retry = true;
              return;
            }
            Logger.DebugFormat("{0} Документ с ИД: {1} был сохранен.", logPrefix, document.Id);
          }
          catch (Exception ex)
          {
            result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", document.Id, ex.Message, ex.StackTrace);
            Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
            result.Retry = true;
            return;
          }
          finally
          {
            if (Locks.GetLockInfo(document).IsLockedByMe)
              Locks.Unlock(document);
          }
          
          if (!string.IsNullOrWhiteSpace(model.BodyAvia))
          {
            var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.LetterToAirlineExtension.Guid);
            errorMessage += this.CreateOrUpdateRelatedDocument(document, NNCommons.PublicConstants.Module.DocumentKind.AirlineLetter, "Addendum", model.BodyAvia, extension);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
              result.ErrorText = errorMessage;
              result.Retry = true;
              return;
            }
          }
          
          if (!string.IsNullOrWhiteSpace(model.BodyOVD))
          {
            var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.LetterToOVDExtension.Guid);
            errorMessage += this.CreateOrUpdateRelatedDocument(document, NNCommons.PublicConstants.Module.DocumentKind.OVDLetter, "Addendum", model.BodyOVD, extension);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
              result.ErrorText = errorMessage;
              result.Retry = true;
              return;
            }
          }
          
        });
      
      if (!string.IsNullOrWhiteSpace(result.ErrorText))
        return result;
      
      try
      {
        var document = OverrideSolution.PublicFunctions.VacationOrderDocument.GetVacationOrderDocumentBySAPID(model.SAPID);
        this.CreateAndStartNewSignDocumentSubtask(model.TaskID, prepared, document);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка при создании или отправке задачи на подписание документа. Текст ошибки: {0}. StackTrace: {1}",
                                         ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
      }
      
      return result;
    }
    
    /// <summary>
    /// Получить гуид вида документа для приказа об отпуске по коду SAP.
    /// </summary>
    /// <param name="sapCode">Код SAP.</param>
    /// <returns>Гуид вида документа.</returns>
    public virtual Guid? GetVacationOrderDocKindGuidBySapCode(string sapCode)
    {
      if (sapCode == NNVacations.PublicConstants.Module.SapKindsCodes.VacationOrderKind)
        return HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationOrderKind;
      else if (sapCode == NNVacations.PublicConstants.Module.SapKindsCodes.ChangeVacationPeriodOrder)
        return NNVacations.PublicConstants.Module.DocumentKind.ChangeVacationPeriodOrder;
      else if (sapCode == NNVacations.PublicConstants.Module.SapKindsCodes.CancelVacationOrder)
        return NNVacations.PublicConstants.Module.DocumentKind.CancelVacationOrder;
      else
        return null;
    }
    
    #endregion
    
    #region IF-S-6372. Регистрация ежегодного отпуска (по заявлению) из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Создание структуры для регистрации ежегодного отпуска по заявлению в SAP.
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="guidMessage">Guid сообщения.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public NNIntegration.Structures.Module.IResultGenerateRequestText CreateRegistrationAnnualVacationByStatement(long mainTaskId, string guidMessage)
    {
      var logPrefix = string.Format("CreateRegistrationAnnualVacationByStatement. Ид основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = VacationStatementDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      logPrefix = string.Format("{0}. Ид документа: {1}", logPrefix, doc.Id);
      
      var errorMessage = string.Empty;
      var listVacations = new List<Structures.Module.IVacationInfoIFS6372>();
      var annualVacation = Structures.Module.VacationInfoIFS6372.Create();
      var emplPersNum = string.Empty;
      var dateStart = doc.StartDateDirRX.HasValue ? doc.StartDateDirRX.Value.ToString("dd.MM.yyyy") : string.Empty;
      var type = doc.VacationTypeDirRX?.SAPIdDirRX;
      var dayCount = doc.DaysAmountDirRX;
      var reason = Constants.Module.EventReasonCode.Annual;
      if (doc.EmployeeDirRX == null)
        errorMessage += "Не заполнено поле Сотрудник. ";
      else if (string.IsNullOrEmpty(doc.EmployeeDirRX.PersonnelNumber))
        errorMessage += string.Format("Не заполнен Табельный номер у сотрудника с ИД: {0}. ", doc.EmployeeDirRX.Id);
      else
        emplPersNum = doc.EmployeeDirRX.PersonnelNumber;
      if (string.IsNullOrEmpty(dateStart))
        errorMessage += "Не заполнено поле Дата начала. ";
      if (string.IsNullOrEmpty(type))
        errorMessage += "Не заполнено поле Тип отпуска. ";
      if (dayCount == null)
        errorMessage += "Не заполнено поле Кол-во дней. ";
      if (string.IsNullOrEmpty(errorMessage))
      {
        annualVacation.DateStart = dateStart;
        annualVacation.Type = type;
        annualVacation.DayCount = dayCount.Value;
        annualVacation.Reason = reason;
        listVacations.Add(annualVacation);
      }
      foreach (var vacation in doc.AdditionalVacationDirRX)
      {
        dateStart = vacation.StartDate.HasValue ? vacation.StartDate.Value.ToString("dd.MM.yyyy") : string.Empty;
        type = vacation.VacationType?.SAPIdDirRX;
        var days = vacation.Days;
        if (string.IsNullOrEmpty(dateStart))
          errorMessage += "Не заполнено поле Дата начала в таблице Данные дополнительного отпуска. ";
        if (string.IsNullOrEmpty(type))
          errorMessage += "Не заполнено поле Тип отпуска в таблице Данные дополнительного отпуска. ";
        if (days == null)
          errorMessage += "Не заполнено поле Кол-во дней в таблице Данные дополнительного отпуска. ";
        
        if (string.IsNullOrEmpty(errorMessage))
        {
          var additionalVacation = Structures.Module.VacationInfoIFS6372.Create();
          additionalVacation.DateStart = dateStart;
          additionalVacation.Type = type;
          additionalVacation.DayCount = days.Value;
          additionalVacation.Reason = reason;
          listVacations.Add(additionalVacation);
        }
      }
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return result;
      }
      
      var model = Structures.Module.RegistrationAnnualVacationByStatement.Create(guidMessage,
                                                                                 emplPersNum,
                                                                                 mainTaskId,
                                                                                 listVacations);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для регистрации ежегодного отпуска по заявлению в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для регистрации ежегодного отпуска по заявлению в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      return result;
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий "Регистрация прочих отпусков".
    
    #region IF-S-10411. Передача приказа на предоставление прочего отпуска из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Изменить или создать приказ на прочий отпуск.
    /// </summary>
    /// <param name="idIncomingMessage">Id записи справочника Входящее информационное сообщение.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public NNIntegration.Structures.Module.IProcessIncomingMessageResult UpdateOrCreateOtherVacationOrder(long idIncomingMessage)
    {
      var logPrefix = string.Format("UpdateOrCreateOtherVacationOrder. Ид входящего сообщения: {0}.", idIncomingMessage);
      Logger.Debug(logPrefix);
      
      var result = NNIntegration.Structures.Module.ProcessIncomingMessageResult.Create();
      result.Retry = false;
      var errorMessage = string.Empty;
      
      var message = NNIntegration.IncomingMessages.GetAll(x => x.Id == idIncomingMessage).FirstOrDefault();
      if (message == null)
      {
        result.ErrorText = "Не найдена запись справочника Входящие интеграционные сообщения.";
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        result.Retry = true;
        return result;
      }
      
      var jsonMessage = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(message);
      
      if (string.IsNullOrWhiteSpace(jsonMessage))
      {
        result.ErrorText = "Не удалось получить тело запроса.";
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var model = NNIntegration.Structures.Module.OtherVacationOrderCard.Create();
      try
      {
        model = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка десериализации в структуру OtherVacationOrderCard.";
        Logger.ErrorFormat("{0} {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorMessage, ex.Message, ex.StackTrace);
        return result;
      }
      
      var employeeResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Employee);
      var signedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Signed);
      var preparedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Prepared);
      
      if (!string.IsNullOrWhiteSpace(employeeResult.ErrorMessage))
        errorMessage += string.Format("EmployeeError: {0}. ", employeeResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(signedResult.ErrorMessage))
        errorMessage += string.Format("SignedError: {0}. ", signedResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(preparedResult.ErrorMessage))
        errorMessage += string.Format("PreparedError: {0}.", preparedResult.ErrorMessage);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var employee = employeeResult.Employee;
      var signed = signedResult.Employee;
      var prepared = preparedResult.Employee;
      
      // Поиск / создание нового документа и заполнение его полей.
      var document = OverrideSolution.PublicFunctions.VacationOrderDocument.GetVacationOrderDocumentBySAPID(model.SAPID);
      
      if (document == null)
      {
        Logger.DebugFormat("{0} Документ по SAPID: {1} не найден. Создание нового документа.", logPrefix, model.SAPID);
        var docKind = OverrideSolution.PublicFunctions.DocumentKind.GetDocKindBySAPCode(model.Kind);
        if (docKind == null)
        {
          result.ErrorText = string.Format("Не найден вид документа по коду SAP \"{0}\"", model.Kind);
          Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
          return result;
        }
        document = OverrideSolution.PublicFunctions.VacationOrderDocument.CreateVacationDocumentWithEmployee(employee, docKind);
      }
      
      result.EntityId = document?.Id;
      
      if (document.LifeCycleState != HRPro.HRProSolution.OfficialDocument.LifeCycleState.Draft || document.InternalApprovalState.HasValue)
      {
        result.ErrorText = string.Format("Версия документа с ID: {0} не создана. Создать новую версию можно только для документа, у которого значения полей жизненного цикла: " +
                                         "\"Состояние документа HR Pro\" - \"В разработке\", \"Согласование\" - не заполнено.", document.Id);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      if (!Locks.GetLockInfo(document).IsLockedByMe && !Locks.TryLock(document))
      {
        var lockInfo = Locks.GetLockInfo(document);
        result.ErrorText = lockInfo.LockedMessage;
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      try
      {
        document.Employee = employee;
        document.OurSignatory = signed;
        document.PreparedBy = prepared;
        document.RegistrationNumber = model.RegNumber;
        document.RegistrationDate = model.Date;
        document.ExternalId = model.SAPID;
        var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.OtherVacationDocumentExtension.Guid);
        // Документ сохраняется при создании новой версии.
        errorMessage = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, model.Body, extension);
        if (!string.IsNullOrWhiteSpace(errorMessage))
          throw AppliedCodeException.Create(errorMessage);
        
        Logger.DebugFormat("{0} Документ с ИД: {1} был сохранен.", logPrefix, document.Id);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", document.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }
      
      try
      {
        this.CreateAndStartNewSignDocumentSubtask(model.TaskID, prepared, document);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка при создании или отправке задачи на подписание документа. Текст ошибки: {0}. StackTrace: {1}",
                                         ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
      }
      
      return result;
    }
    
    #endregion
    
    #region IF-S-6376. Регистрация прочего отпуска из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Создание структуры для регистрации прочего отпуска в SAP.
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="guidMessage">Guid сообщения.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public NNIntegration.Structures.Module.IResultGenerateRequestText CreateRegistrationOtherVacation(long mainTaskId, string guidMessage)
    {
      var logPrefix = string.Format("CreateRegistrationOtherVacation. Ид основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = VacationStatementDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      logPrefix = string.Format("{0}. Ид документа: {1}", logPrefix, doc.Id);
      
      var errorMessage = string.Empty;
      var emplPersNum = string.Empty;
      var type = string.Empty;
      var dateStart = doc.StartDateDirRX.HasValue ? doc.StartDateDirRX.Value.ToString("dd.MM.yyyy") : string.Empty;
      var vacationType = doc.VacationTypeDirRX;
      var vacationReason = doc.VacationReasonDirRX;
      var dayCount = doc.DaysAmountDirRX;
      
      if (doc.EmployeeDirRX == null)
        errorMessage += "Не заполнено поле Сотрудник. ";
      else if (string.IsNullOrEmpty(doc.EmployeeDirRX.PersonnelNumber))
        errorMessage += string.Format("Не заполнен Табельный номер у сотрудника с ИД: {0}. ", doc.EmployeeDirRX.Id);
      else
        emplPersNum = doc.EmployeeDirRX.PersonnelNumber;
      if (string.IsNullOrEmpty(dateStart))
        errorMessage += "Не заполнено поле Дата начала. ";
      if (vacationType == null)
        errorMessage += "Не заполнено поле Тип отпуска. ";
      if (dayCount == null)
        errorMessage += "Не заполнено поле Кол-во дней. ";
      
      var isStudy = doc.VacationTypeDirRX != null ? NNVacations.PublicFunctions.Module.CheckForStudyVacationType(doc.VacationTypeDirRX.Id) : false;
      bool? isStudent = false;
      var reason = string.Empty;
      if (isStudy)
      {
        type = vacationType?.SAPIdDirRX;
        reason = Constants.Module.EventReasonCode.Study;
        isStudent = doc.IsPostgraduateDirRX;
        if (isStudent == null)
          errorMessage += "Не заполнено поле Аспирант. ";
      }
      else
      {
        type = vacationType?.ActualAbsenceDirRX.Where(a => Equals(a.VacationReason, vacationReason)).FirstOrDefault()?.ActualAbsence;
        reason = vacationReason?.SAPId;
        if (string.IsNullOrEmpty(reason))
          errorMessage += "Не заполнено поле Причина отпуска. ";
      }
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return result;
      }
      
      var model = Structures.Module.RegistrationOtherVacation.Create(guidMessage,
                                                                     emplPersNum,
                                                                     type,
                                                                     reason,
                                                                     dateStart,
                                                                     dayCount.Value,
                                                                     isStudent,
                                                                     mainTaskId);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для регистрации прочего отпуска в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для регистрации прочего отпуска в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      return result;
    }
    
    #endregion
    
    #region IF-S-10409. Проверка заявления на предоставление прочего отпуска (АСУП -> КЭДО).
    
    /// <summary>
    /// Проверить отпуска сотрудника при оформлении заявления на прочий отпуск в SAP.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="vacationTypeId">ИД типа отпуска.</param>
    /// <param name="vacationReasonId">ИД причины отпуска.</param>
    /// <param name="dateStart">Дата начала.</param>
    /// <param name="dateEnd">Дата окончания.</param>
    /// <param name="daysCount">Количество дней.</param>
    /// <returns>Структура с ответными данными.</returns>
    public Structures.Module.IValidateVacationResult ValidateEmployeeOtherVacations(long employeeId, string personnelNumber, long vacationTypeId, long? vacationReasonId,
                                                                                    DateTime dateStart, DateTime dateEnd, int daysCount)
    {
      var logPrefix = string.Format("ValidateEmployeeOtherVacations. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create();
      
      var resultRequest = this.GenerateStructForValidateEmployeeOtherVacations(employeeId, personnelNumber, vacationTypeId, vacationReasonId, dateStart, dateEnd, daysCount);
      if (!string.IsNullOrEmpty(resultRequest.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка формирования текста запроса. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.RequestTextGenerationError.ToString();
        return result;
      }
      
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.ValidateEmployeeOtherVacations;
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(resultRequest.RequestText, flowGuid, employeeId);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.SendMessageToExternalSystemError.ToString();
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.ValidateEmployeeVacationsResult.Create();
      try
      {
        Logger.DebugFormat("{0}. AnswerText: {1}", logPrefix, resultSendingMessage.AnswerText);
        model = DirRX.NNVacations.IsolatedFunctions.Json.DeserializeInStruct(resultSendingMessage.AnswerText, model);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        result.ErrorText = Resources.DeserializeInStructError.ToString();
        return result;
      }
      
      if (model.Result == false && !string.IsNullOrEmpty(model.ErrorText))
        result.ErrorText = model.ErrorText;
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return result;
    }
    
    /// <summary>
    /// Создание структуры проверки данных при оформлении заявления на прочий отпуск в SAP.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="vacationTypeId">ИД типа отпуска.</param>
    /// <param name="vacationReasonId">ИД причины отпуска.</param>
    /// <param name="dateStart">Дата начала.</param>
    /// <param name="dateEnd">Дата окончания.</param>
    /// <param name="daysCount">Кол-во дней.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public NNIntegration.Structures.Module.IResultGenerateRequestText GenerateStructForValidateEmployeeOtherVacations(long employeeId, string personnelNumber, long vacationTypeId, long? vacationReasonId,
                                                                                                                      DateTime dateStart, DateTime dateEnd, int daysCount)
    {
      var logPrefix = string.Format("GenerateStructForValidateEmployeeOtherVacations. ИД сотрудника: {0}", employeeId);
      var result = DirRX.NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      Logger.DebugFormat("{0}. Запуск создания структуры проверки данных по отпускам в SAP.", logPrefix);
      
      var vacationType = VacationKinds.GetAll(v => v.Id == vacationTypeId).FirstOrDefault();
      var vacationReason = VacationReasonses.GetAll(v => v.Id == vacationReasonId).FirstOrDefault();
      var type = this.CheckForStudyVacationType(vacationTypeId) ? vacationType?.SAPIdDirRX : vacationType?.ActualAbsenceDirRX.Where(a => Equals(a.VacationReason, vacationReason)).FirstOrDefault()?.ActualAbsence;
      
      var model = DirRX.NNVacations.Structures.Module.EmployeeOtherVacationsValidation.Create(
        Guid.NewGuid().ToString(),
        personnelNumber,
        type,
        dateStart.ToString("dd.MM.yyyy"),
        dateEnd.ToString("dd.MM.yyyy"),
        daysCount);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры проверки данных по прочему отпуску в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры проверки данных по прочему отпуску в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий "Коменсация части ежегодного оплачиваемого отпуска".
    
    #region IF-S-10384. Получение данных о доступном количестве дней на компенсацию из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Получить доступное количество дней на компенсацию из АСУП.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Структура доступных дней для компенсации.</returns>
    [Public]
    public DirRX.NNVacations.Structures.Module.ICompensationAvailableDaysResult GetCompensationAvailableDays(long employeeId)
    {
      var logPrefix = string.Format("GetCompensationAvailableDays. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки", logPrefix);
      var model = DirRX.NNVacations.Structures.Module.CompensationAvailableDaysResult.Create();
      model.ErrorText = string.Empty;
      
      var resultRequest = this.GenerateStructForCompensationAvailableDays(employeeId);
      if (!string.IsNullOrEmpty(resultRequest.ErrorText))
      {
        var errorText = string.Format("Ошибка формирования текста запроса: {0}", resultRequest.ErrorText);
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}.", logPrefix, errorText, resultRequest.ErrorText);
        model.ErrorText = errorText;
        return model;
      }
      
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.GetCompensationAvailableDays;
      
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(resultRequest.RequestText, flowGuid, employeeId);
      
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        var errorText = string.Format("Ошибка отправки исходящего сообщения: {0}", resultSendingMessage.ErrorText);
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}.", logPrefix, errorText, resultRequest.ErrorText);
        model.ErrorText = errorText;
        return model;
      }
      
      try
      {
        model = DirRX.NNVacations.IsolatedFunctions.Json.DeserializeInStruct(resultSendingMessage.AnswerText, model);
      }
      catch (Exception ex)
      {
        var errorText = string.Format("Ошибка десериализации в структуру: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorText, ex.Message, ex.StackTrace);
        model.ErrorText = errorText;
        return model;
      }
      
      if (!string.IsNullOrEmpty(model.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка выполнения: {1}.", logPrefix, model.ErrorText);
        return model;
      }
      
      Logger.DebugFormat("{0}. Конец обработки", logPrefix);
      return model;
    }
    
    /// <summary>
    /// Создание структуры для получение доступных дней для компенсации отпуска сотрудника.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public DirRX.NNIntegration.Structures.Module.IResultGenerateRequestText GenerateStructForCompensationAvailableDays(long employeeId)
    {
      var logPrefix = string.Format("GenerateStructForCompensationAvailableDays. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = DirRX.NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var employee = Employees.GetAll().FirstOrDefault(x => x.Id == employeeId);
      if (employee == null || string.IsNullOrEmpty(employee.PersonnelNumber))
      {
        result.ErrorText = "Не удалось получить табельный номер сотрудника";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.CompensationAvailableDays.Create(
        Guid.NewGuid().ToString(),
        employee.PersonnelNumber);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры получения доступных дней для компенсации отпуска в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры получения доступных дней для компенсации отпуска в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    /// <summary>
    /// Получить количество дней компенсации отпуска на согласовании.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Количество дней на согласовании.</returns>
    public int GetCompensationDaysOnApproving(long employeeId)
    {
      var statements = VacationStatementDocuments.GetAll()
        .Where(d => d.Employee != null && d.Employee.Id == employeeId)
        .Where(d => d.DocumentKind != null && DocumentKinds.As(d.DocumentKind).SAPCodeDirRX == Constants.Module.SapKindsCodes.CompensationVacationKind)
        .Where(d => d.SelectedDaysDirRX != null);
      
      var daysCount = 0;
      foreach (var statement in statements)
      {
        var onApproving = StatementTasks.GetAll()
          .Where(t => t.Status == DirRX.OverrideSolution.StatementTask.Status.InProcess || t.Status == DirRX.OverrideSolution.StatementTask.Status.UnderReview)
          .Where(t => t.AttachmentDetails.Any(d => d.EntityId == statement.Id && d.GroupId == Guid.Parse("b80b6b22-03cb-43f6-9211-75b93cf9710e")))
          .Any();
        
        if (onApproving)
          daysCount += statement.SelectedDaysDirRX.Value;
      }
      
      return daysCount;
    }
    
    #endregion
    
    #region IF-S-11187. Проверка данных в АСУП для заявления на компенсацию части ежегодного отпуска (АСУП -> КЭДО).
    
    /// <summary>
    /// Проверить данные в АСУП для заявления на компенсацию части ежегодного отпуска.
    /// </summary>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="daysSelected">Кол-во дней.</param>
    /// <returns>Структура с ответными данными.</returns>
    [Public]
    public Structures.Module.IValidateVacationResult ValidateVacationCompensation(string personnelNumber, int daysSelected)
    {
      var logPrefix = string.Format("ValidateVacationCompensation. Табельный номер сотрудника: {0}", personnelNumber);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create();
      
      var resultRequest = this.GenerateStructForValidateVacationCompensation(personnelNumber, daysSelected);
      if (!string.IsNullOrEmpty(resultRequest.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка формирования текста запроса. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.RequestTextGenerationError;
        return result;
      }
      
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.ValidateVacationCompensation;
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(resultRequest.RequestText, flowGuid, 0);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.SendMessageToExternalSystemError;
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.ValidateEmployeeVacationsResult.Create();
      try
      {
        model = DirRX.NNVacations.IsolatedFunctions.Json.DeserializeInStruct(resultSendingMessage.AnswerText, model);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        result.ErrorText = Resources.DeserializeInStructError;
        return result;
      }
      
      if (!model.Result && !string.IsNullOrEmpty(model.ErrorText))
        result.ErrorText = model.ErrorText;
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return result;
    }
    
    /// <summary>
    /// Создание структуры проверки данных при оформлении заявления на компенсацию части ежегодного отпуска в SAP.
    /// </summary>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="daysSelected">Кол-во дней.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public DirRX.NNIntegration.Structures.Module.IResultGenerateRequestText GenerateStructForValidateVacationCompensation(string personnelNumber, int daysSelected)
    {
      var logPrefix = string.Format("GenerateStructForValidateVacationCompensation. Табельный номер сотрудника: {0}", personnelNumber);
      var result = DirRX.NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      
      var model = DirRX.NNVacations.Structures.Module.ValidateVacationCompensation.Create(
        Guid.NewGuid().ToString(),
        personnelNumber,
        daysSelected);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры проверки данных по компенсации отпуска в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры проверки данных по компенсации отпуска в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #region IF-S-6369. Автоматическое создание мероприятия «Компенсация части ежегодного оплачиваемого отпуска» из КЭДО в АСУП.
    
    /// <summary>
    /// Создание структуры для компенсации части ежегодного оплачиваемого отпуска в SAP.
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="guidMessage">Guid сообщения.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public NNIntegration.Structures.Module.IResultGenerateRequestText CreateCompensationForPartOfAnnualPaidLeave(long mainTaskId, string guidMessage)
    {
      var logPrefix = string.Format("CreateCompensationForPartOfAnnualPaidLeave. ИД основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = VacationStatementDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      logPrefix = string.Format("{0}. ИД документа: {1}", logPrefix, doc.Id);
      
      var errorMessage = string.Empty;
      var emplPersNum = string.Empty;
      var daysSelected = doc.SelectedDaysDirRX;
      var date = string.Empty;
      
      if (doc.EmployeeDirRX == null)
        errorMessage += "Не заполнено поле Сотрудник. ";
      else if (string.IsNullOrEmpty(doc.EmployeeDirRX.PersonnelNumber))
        errorMessage += string.Format("Не заполнен Табельный номер у сотрудника с ИД: {0}. ", doc.EmployeeDirRX.Id);
      else
        emplPersNum = doc.EmployeeDirRX.PersonnelNumber;
      if (!daysSelected.HasValue)
        errorMessage += "Не заполнено поле Выбрано дней. ";
      if (!doc.RegistrationDate.HasValue)
        errorMessage += "Не заполнено поле Дата документа. ";
      else
        date = doc.RegistrationDate.Value.ToString("dd.MM.yyyy");
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return result;
      }
      
      var model = Structures.Module.CompensationForPartOfAnnualPaidLeave.Create(guidMessage,
                                                                                emplPersNum,
                                                                                daysSelected.Value,
                                                                                date,
                                                                                mainTaskId);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для компенсация части ежегодного оплачиваемого отпуска в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для компенсация части ежегодного оплачиваемого отпуска в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      return result;
    }
    
    #endregion
    
    #region IF-S-10396. Передача приказа на компенсацию части ежегодного оплачиваемого отпуска из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Изменить или создать приказ на компенсацию части ежегодного оплачиваемого отпуска.
    /// </summary>
    /// <param name="idIncomingMessage">Id записи справочника Входящее информационное сообщение.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public NNIntegration.Structures.Module.IProcessIncomingMessageResult UpdateOrCreateVacationCompensationOrder(long idIncomingMessage)
    {
      var logPrefix = string.Format("UpdateOrCreateVacationCompensationOrder. Ид входящего сообщения: {0}.", idIncomingMessage);
      Logger.Debug(logPrefix);
      
      var result = NNIntegration.Structures.Module.ProcessIncomingMessageResult.Create();
      result.Retry = false;
      var errorMessage = string.Empty;
      
      var message = NNIntegration.IncomingMessages.GetAll(x => x.Id == idIncomingMessage).FirstOrDefault();
      if (message == null)
      {
        result.ErrorText = "Не найдена запись справочника Входящие интеграционные сообщения.";
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        result.Retry = true;
        return result;
      }
      
      var jsonMessage = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(message);
      
      if (string.IsNullOrWhiteSpace(jsonMessage))
      {
        result.ErrorText = "Не удалось получить тело запроса.";
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var model = NNIntegration.Structures.Module.VacationCompensationOrderCard.Create();
      try
      {
        model = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка десериализации в структуру VacationCompensationOrderCard.";
        Logger.ErrorFormat("{0} {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorMessage, ex.Message, ex.StackTrace);
        result.Retry = true;
        return result;
      }
      
      var employeeResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Employee);
      var signedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Signed);
      var preparedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Prepared);
      
      if (!string.IsNullOrWhiteSpace(employeeResult.ErrorMessage))
        errorMessage += string.Format("EmployeeError: {0}. ", employeeResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(signedResult.ErrorMessage))
        errorMessage += string.Format("SignedError: {0}. ", signedResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(preparedResult.ErrorMessage))
        errorMessage += string.Format("PreparedError: {0}.", preparedResult.ErrorMessage);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var employee = employeeResult.Employee;
      var signed = signedResult.Employee;
      var prepared = preparedResult.Employee;
      
      // Поиск / создание нового документа и заполнение его полей.
      var document = OverrideSolution.PublicFunctions.VacationOrderDocument.GetVacationOrderDocumentBySAPID(model.SAPID);
      
      if (document == null)
      {
        Logger.DebugFormat("{0} Документ по SAPID: {1} не найден. Создание нового документа.", logPrefix, model.SAPID);
        document = OverrideSolution.PublicFunctions.VacationOrderDocument.CreateVacationDocumentWithEmployee(employee, NNVacations.PublicConstants.Module.DocumentKind.VacationCompensationOrderKind);
      }
      
      result.EntityId = document?.Id;
      
      if (document.LifeCycleState != HRPro.HRProSolution.OfficialDocument.LifeCycleState.Draft || document.InternalApprovalState.HasValue)
      {
        result.ErrorText = string.Format("Версия документа с ID: {0} не создана. Создать новую версию можно только для документа, у которого значения полей жизненного цикла: " +
                                         "\"Состояние документа HR Pro\" - \"В разработке\", \"Согласование\" - не заполнено.", document.Id);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      if (!Locks.GetLockInfo(document).IsLockedByMe && !Locks.TryLock(document))
      {
        var lockInfo = Locks.GetLockInfo(document);
        result.ErrorText = lockInfo.LockedMessage;
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      try
      {
        document.Employee = employee;
        document.OurSignatory = signed;
        document.PreparedBy = prepared;
        document.RegistrationNumber = model.RegNumber;
        document.RegistrationDate = model.Date;
        document.ExternalId = model.SAPID;
        var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.RecallAndCompensationVacationDocumentExtension.Guid);
        // Документ сохраняется при создании новой версии.
        errorMessage = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, model.Body, extension);
        if (!string.IsNullOrWhiteSpace(errorMessage))
          throw AppliedCodeException.Create(errorMessage);
        
        Logger.DebugFormat("{0} Документ с ИД: {1} был сохранен.", logPrefix, document.Id);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", document.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }
      
      try
      {
        this.CreateAndStartNewSignDocumentSubtask(model.TaskID, prepared, document);
        Logger.DebugFormat("{0} Задача на подписание документа с ИД: {1} успешно отправлена.", logPrefix, document.Id);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка при создании или отправке задачи на подписание документа. Текст ошибки: {0}. StackTrace: {1}",
                                         ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
      }
      
      return result;
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий "Отзыв из отпуска".
    
    #region IF-S-10391. Передача приказа об отзыве из отпуска из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Изменить или создать приказ об отзыве из отпуска.
    /// </summary>
    /// <param name="idIncomingMessage">Id записи справочника Входящее информационное сообщение.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public NNIntegration.Structures.Module.IProcessIncomingMessageResult UpdateOrCreateRecallVacationOrder(long idIncomingMessage)
    {
      var logPrefix = string.Format("UpdateOrCreateRecallVacationOrder. Ид входящего сообщения: {0}.", idIncomingMessage);
      Logger.Debug(logPrefix);
      
      var result = NNIntegration.Structures.Module.ProcessIncomingMessageResult.Create();
      result.Retry = false;
      var errorMessage = string.Empty;
      
      var message = NNIntegration.IncomingMessages.GetAll(x => x.Id == idIncomingMessage).FirstOrDefault();
      if (message == null)
      {
        result.ErrorText = "Не найдена запись справочника Входящие интеграционные сообщения.";
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        result.Retry = true;
        return result;
      }
      
      var jsonMessage = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(message);
      
      if (string.IsNullOrWhiteSpace(jsonMessage))
      {
        result.ErrorText = "Не удалось получить тело запроса.";
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var model = NNIntegration.Structures.Module.RecallVacationOrderCard.Create();
      try
      {
        model = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка десериализации в структуру RecallVacationOrderCard.";
        Logger.ErrorFormat("{0} {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorMessage, ex.Message, ex.StackTrace);
        result.Retry = true;
        return result;
      }
      
      var employeeResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Employee);
      var signedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Signed);
      var preparedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Prepared);
      
      if (!string.IsNullOrWhiteSpace(employeeResult.ErrorMessage))
        errorMessage += string.Format("EmployeeError: {0}. ", employeeResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(signedResult.ErrorMessage))
        errorMessage += string.Format("SignedError: {0}. ", signedResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(preparedResult.ErrorMessage))
        errorMessage += string.Format("PreparedError: {0}.", preparedResult.ErrorMessage);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var employee = employeeResult.Employee;
      var signed = signedResult.Employee;
      var prepared = preparedResult.Employee;
      
      // Поиск / создание нового документа и заполнение его полей.
      var document = OverrideSolution.PublicFunctions.VacationOrderDocument.GetVacationOrderDocumentBySAPID(model.SAPID);
      
      if (document == null)
      {
        Logger.DebugFormat("{0} Документ по SAPID: {1} не найден. Создание нового документа.", logPrefix, model.SAPID);
        document = OverrideSolution.PublicFunctions.VacationOrderDocument.CreateVacationDocumentWithEmployee(employee, HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallOrderKind);
      }
      
      result.EntityId = document?.Id;
      
      if (document.LifeCycleState != HRPro.HRProSolution.OfficialDocument.LifeCycleState.Draft || document.InternalApprovalState.HasValue)
      {
        result.ErrorText = string.Format("Версия документа с ID: {0} не создана. Создать новую версию можно только для документа, у которого значения полей жизненного цикла: " +
                                         "\"Состояние документа HR Pro\" - \"В разработке\", \"Согласование\" - не заполнено.", document.Id);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      if (!Locks.GetLockInfo(document).IsLockedByMe && !Locks.TryLock(document))
      {
        var lockInfo = Locks.GetLockInfo(document);
        result.ErrorText = lockInfo.LockedMessage;
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      try
      {
        document.Employee = employee;
        document.OurSignatory = signed;
        document.PreparedBy = prepared;
        document.RegistrationNumber = model.RegNumber;
        document.RegistrationDate = model.Date;
        document.Note = model.Basis;
        document.ExternalId = model.SAPID;
        var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.RecallAndCompensationVacationDocumentExtension.Guid);
        // Документ сохраняется при создании новой версии.
        errorMessage = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, model.Body, extension);
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
          result.ErrorText = string.Format("Произошла ошибка обработки документа с ID: {0}. Текст ошибки: {1}.", document.Id, errorMessage);
          Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
          return result;
        }
        Logger.DebugFormat("{0} Документ с ИД: {1} был сохранен.", logPrefix, document.Id);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки документа с ID: {0}. Текст ошибки: {1}. StackTrace: {2}.", document.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }
      
      try
      {
        this.CreateAndStartNewSignDocumentSubtask(model.TaskID, prepared, document);
        Logger.DebugFormat("{0} Задача на подписание документа с ИД: {1} успешно отправлена.", logPrefix, document.Id);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка при создании или отправке задачи на подписание документа. Текст ошибки: {0}. StackTrace: {1}",
                                         ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
      }
      
      return result;
    }
    
    #endregion
    
    #region IF-S-10392. Получение данных о текущем отпуске сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Получить данные о текущем отпуске сотрудника из АСУП.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Структура данных о текущем отпуске сотрудника.</returns>
    [Public]
    public DirRX.NNVacations.Structures.Module.ICurrentVacationDataResult GetCurrentVacationData(long employeeId)
    {
      var logPrefix = string.Format("GetCurrentVacationData. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки", logPrefix);
      var result = DirRX.NNVacations.Structures.Module.CurrentVacationDataResult.Create();
      
      var resultRequest = this.GenerateStructForCurrentVacationData(employeeId);
      if (!string.IsNullOrEmpty(resultRequest.ErrorText))
      {
        var errorText = "Ошибка формирования текста запроса";
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}.", logPrefix, errorText, resultRequest.ErrorText);
        result.ErrorText = errorText;
        return result;
      }
      
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.GetCurrentVacationData;
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(resultRequest.RequestText, flowGuid, employeeId);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        var errorText = "Ошибка отправки исходящего сообщения";
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}.", logPrefix, errorText, resultRequest.ErrorText);
        result.ErrorText = errorText;
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.CurrentVacationDataAnswer.Create();
      try
      {
        model = DirRX.NNVacations.IsolatedFunctions.Json.DeserializeInStruct(resultSendingMessage.AnswerText, model);
      }
      catch (Exception ex)
      {
        var errorText = "Ошибка десериализации в структуру";
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorText, ex.Message, ex.StackTrace);
        result.ErrorText = errorText;
        return result;
      }
      
      if (model.VacationData == null || !model.VacationData.Any() || string.IsNullOrEmpty(model.VacationData.First().Type) ||
          string.IsNullOrEmpty(model.VacationData.First().CurrentDateStart) || string.IsNullOrEmpty(model.VacationData.First().CurrentDateEnd))
      {
        result.ErrorText = "Нет отпусков, доступных для прерывания";
        return result;
      }
      
      result.CurrentVacationDataAnswer = model;
      
      Logger.DebugFormat("{0}. Конец обработки", logPrefix);
      return result;
    }
    
    /// <summary>
    /// Создание структуры для получения данных о текущем отпуске сотрудника.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public DirRX.NNIntegration.Structures.Module.IResultGenerateRequestText GenerateStructForCurrentVacationData(long employeeId)
    {
      var logPrefix = string.Format("GenerateStructForCurrentVacationData. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = DirRX.NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var errorMessage = string.Empty;
      var personnelNumber = string.Empty;
      var employee = Employees.GetAll().FirstOrDefault(x => x.Id == employeeId);
      if (employee == null)
        errorMessage = DirRX.NNVacations.Resources.CurrentUserNotFoundError.ToString();
      else if (string.IsNullOrEmpty(employee.PersonnelNumber))
        errorMessage = DirRX.NNVacations.Resources.CurrentUserPersonalNumberNotFoundError.ToString();
      else
        personnelNumber = employee.PersonnelNumber;
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.CurrentVacationData.Create(Guid.NewGuid().ToString(),
                                                                                 personnelNumber);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры получения данных о текущем отпуске в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры получения данных о текущем отпуске в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #region IF-S-10394. Проверка данных в АСУП для оформления отзыва из отпуска (АСУП -> КЭДО).
    
    /// <summary>
    /// Проверить данные в АСУП для оформления отзыва из отпуска.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="presenceDateStart">Дата начала отзыва.</param>
    /// <param name="presenceDateEnd">Дата окончания отзыва.</param>
    /// <param name="dateTransfer">Дата переноса дней отпуска.</param>
    /// <param name="dateTransferEnd">Дата окончания переноса дней отпуска.</param>
    /// <param name="dayCountCompensation">Количество дней компенсации.</param>
    [Public]
    public Structures.Module.IValidateVacationResult ValidateRecallVacation(long employeeId, string personnelNumber, DateTime presenceDateStart, DateTime presenceDateEnd,
                                                                            DateTime? dateTransferStart, DateTime? dateTransferEnd, int dayCountCompensation)
    {
      var logPrefix = string.Format("ValidateRecallVacation. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      var result = Structures.Module.ValidateVacationResult.Create();
      
      var resultRequest = this.GenerateStructForValidateRecallVacation(employeeId, personnelNumber, presenceDateStart, presenceDateEnd, dateTransferStart, dateTransferEnd, dayCountCompensation);
      if (!string.IsNullOrEmpty(resultRequest.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка формирования текста запроса. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.RequestTextGenerationError.ToString();
        return result;
      }
      
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.ValidateVacationRecall;
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(resultRequest.RequestText, flowGuid, employeeId);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultRequest.ErrorText);
        result.ErrorText = Resources.SendMessageToExternalSystemError.ToString();
        return result;
      }
      
      var model = DirRX.NNVacations.Structures.Module.ValidateEmployeeVacationsResult.Create();
      try
      {
        model = DirRX.NNVacations.IsolatedFunctions.Json.DeserializeInStruct(resultSendingMessage.AnswerText, model);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        result.ErrorText = Resources.DeserializeInStructError.ToString();
        return result;
      }
      
      if (!string.IsNullOrEmpty(model.ErrorText))
      {
        if (model.Result)
          result.IsVacationPeriodBroken = model.ErrorText;
        else
          result.ErrorText = model.ErrorText;
      }
      
      Logger.DebugFormat("{0}. Конец обработки.", logPrefix);
      return result;
    }
    
    /// <summary>
    /// Создание структуры проверки данных при оформлении отзыва из отпуска.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="personnelNumber">Табельный номер сотрудника.</param>
    /// <param name="presenceDateStart">Дата начала отзыва.</param>
    /// <param name="presenceDateEnd">Дата окончания отзыва.</param>
    /// <param name="dateTransferStart">Дата переноса дней отпуска.</param>
    /// <param name="dateTransferEnd">Дата окончания переноса дней отпуска.</param>
    /// <param name="dayCountCompensation">Количество дней компенсации.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public DirRX.NNIntegration.Structures.Module.IResultGenerateRequestText GenerateStructForValidateRecallVacation(long employeeId, string personnelNumber, DateTime presenceDateStart, DateTime presenceDateEnd,
                                                                                                                    DateTime? dateTransferStart, DateTime? dateTransferEnd, int dayCountCompensation)
    {
      var logPrefix = string.Format("GenerateStructForValidateRecallVacation. ИД сотрудника: {0}", employeeId);
      var result = DirRX.NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      Logger.DebugFormat("{0}. Начало обработки.", logPrefix);
      
      var savedDateTransferStart = dateTransferStart?.ToString("dd.MM.yyyy") ?? "";
      var savedDateTransferEnd = dateTransferEnd?.ToString("dd.MM.yyyy") ?? "";
      
      var model = DirRX.NNVacations.Structures.Module.ValidateVacationRecall.Create(Guid.NewGuid().ToString(),
                                                                                    personnelNumber,
                                                                                    presenceDateStart.ToString("dd.MM.yyyy"),
                                                                                    presenceDateEnd.ToString("dd.MM.yyyy"),
                                                                                    savedDateTransferStart,
                                                                                    savedDateTransferEnd,
                                                                                    dayCountCompensation);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры проверки данных по отзыву из отпуска в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры проверки данных по отзыву из отпуска в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #region IF-S-6361. Автоматическое создание мероприятия "Отзыв из отпуска" из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Создание структуры для отзыва из отпуска в SAP.
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="guidMessage">Guid сообщения.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    [Public]
    public NNIntegration.Structures.Module.IResultGenerateRequestText CreateRegistrationRecallVacation(long mainTaskId, string guidMessage)
    {
      var logPrefix = string.Format("CreateRegistrationRecallVacation. Ид основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = VacationStatementDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      logPrefix = string.Format("{0}. Ид документа: {1}", logPrefix, doc.Id);
      
      var errorMessage = string.Empty;
      var emplPersNum = string.Empty;
      var dateTransferStart = string.Empty;
      var dateTransferEnd = string.Empty;
      var dayCount = 0;
      var eventSAP = Constants.Module.RecallEventSAP;
      var currentDateStart = doc.StartDateDirRX.HasValue ? doc.StartDateDirRX.Value.ToString("dd.MM.yyyy") : string.Empty;
      var currentDateEnd = doc.EndDateDirRX.HasValue ? doc.EndDateDirRX.Value.ToString("dd.MM.yyyy") : string.Empty;
      var presenceDateStart = doc.InterruptStartDateDirRX.HasValue ? doc.InterruptStartDateDirRX.Value.ToString("dd.MM.yyyy") : string.Empty;
      var presenceDateEnd = doc.InterruptEndDateDirRX.HasValue ? doc.InterruptEndDateDirRX.Value.ToString("dd.MM.yyyy") : string.Empty;
      var type = doc.InterruptVacationDirRX.OrderBy(v => v.StartDate).FirstOrDefault()?.VacationType?.SAPIdDirRX;
      var dayCountCompensation = doc.CompensationDaysDirRX;
      var newVacations = doc.AdditionalVacationDirRX;
      
      var requireShiftDaysWhenRecall = doc.InterruptVacationDirRX.Any(v => v.VacationType != null && v.VacationType.RequireShiftDaysWhenRecallDirRX.HasValue &&
                                                                      v.VacationType.RequireShiftDaysWhenRecallDirRX.Value == true);
      
      if (doc.EmployeeDirRX == null)
        errorMessage += "Не заполнено поле Сотрудник. ";
      else if (string.IsNullOrEmpty(doc.EmployeeDirRX.PersonnelNumber))
        errorMessage += string.Format("Не заполнен Табельный номер у сотрудника с ИД: {0}. ", doc.EmployeeDirRX.Id);
      else
        emplPersNum = doc.EmployeeDirRX.PersonnelNumber;
      if (string.IsNullOrEmpty(currentDateStart))
        errorMessage += "Не заполнено поле Дата начала текущего отпуска. ";
      if (string.IsNullOrEmpty(currentDateEnd))
        errorMessage += "Не заполнено поле Дата окончания текущего отпуска. ";
      if (string.IsNullOrEmpty(presenceDateStart))
        errorMessage += "Не заполнено поле Дата начала периода прерывания/отзыва из отпуска. ";
      if (string.IsNullOrEmpty(presenceDateEnd))
        errorMessage += "Не заполнено поле Дата окончания периода прерывания/отзыва из отпуска. ";
      if (string.IsNullOrEmpty(type))
        errorMessage += "Не заполнено поле Тип отпуска. ";
      if (dayCountCompensation == null)
        errorMessage += "Не заполнено поле Количество дней для компенсации. ";
      if (!newVacations.Any() && requireShiftDaysWhenRecall)
      {
        errorMessage += "Не заполнены Данные нового отпуска. ";
      }
      else
      {
        var startDateShift = newVacations.OrderBy(v => v.StartDate).FirstOrDefault()?.StartDate;
        var endDateShift = newVacations.OrderByDescending(v => v.EndDate).FirstOrDefault()?.EndDate;
        dateTransferStart = startDateShift.HasValue ? startDateShift.Value.ToString("dd.MM.yyyy") : string.Empty;
        dateTransferEnd = endDateShift.HasValue ? endDateShift.Value.ToString("dd.MM.yyyy") : string.Empty;
        dayCount = newVacations.Sum(v => v.Days ?? 0);
        if (string.IsNullOrEmpty(dateTransferStart) && requireShiftDaysWhenRecall)
          errorMessage += "Не заполнено поле Дата начала в таблице Данные нового отпуска. ";
        if (string.IsNullOrEmpty(dateTransferEnd) && requireShiftDaysWhenRecall)
          errorMessage += "Не заполнено поле Дата окончания в таблице Данные нового отпуска. ";
        if (dayCount == 0 && requireShiftDaysWhenRecall)
          errorMessage += "Не заполнено поле Кол-во дней в таблице Данные нового отпуска. ";
      }
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return result;
      }
      
      var model = Structures.Module.RegistrationRecallVacation.Create(guidMessage,
                                                                      emplPersNum,
                                                                      eventSAP,
                                                                      currentDateStart,
                                                                      currentDateEnd,
                                                                      presenceDateStart,
                                                                      presenceDateEnd,
                                                                      type,
                                                                      dayCountCompensation.Value,
                                                                      dateTransferStart,
                                                                      dateTransferEnd,
                                                                      dayCount,
                                                                      mainTaskId);
      
      try
      {
        result.RequestText = IsolatedFunctions.Json.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для отзыва из отпуска в SAP.", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для отзыва из отпуска в SAP возникла ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      return result;
    }
    
    #endregion
    
    #endregion
    
    #region Функции вычисляемых выражений.
    
    /// <summary>
    /// Определить срок согласования.
    /// </summary>
    /// <param name="date">Дата начала отпуска.</param>
    /// <returns>Возвращает дату срок согласования</returns>
    [ExpressionElement("Определить срок согласования", "Возвращает дату срок согласования", "", "Сотрудник", "Вид документа", "Тип отпуска", "Рабочие дни")]
    public DateTime? GetDateDeadlineForApproval(DateTime date, IEmployee empl, IDocumentKind documentKind)
    {
      if (date == null)
      {
        Logger.ErrorFormat("GetDateDeadlineForApproval. Не заполнена дата.");
        return null;
      }
      
      if (empl == null)
      {
        Logger.ErrorFormat("GetDateDeadlineForApproval. Не заполнен сотрудник.");
        return null;
      }
      
      if (documentKind == null)
      {
        Logger.ErrorFormat("GetDateDeadlineForApproval. Не заполнен вид документа.");
        return null;
      }
      
      var top =  empl.JobTitle != null && OverrideSolution.JobTitles.As(empl.JobTitle).EvaluationGroupDirRX?.Value != null &&
        OverrideSolution.JobTitles.As(empl.JobTitle).EvaluationGroupDirRX.Value != OverrideSolution.JobTitle.EvaluationGroupDirRX.OutOfCategories;
      
      // Получить запись справочника "Cрок согласования отпуска".
      var vacationApprovalPeriod = PublicFunctions.VacationsApprovalPeriod.GetVacationsApprovalPeriod(empl.BusinessUnit.Id, documentKind.Id, null, false, top);
      var nextVacationDays = vacationApprovalPeriod?.NextVacationDays ?? 0;
      DateTime? newDate;
      
      if (vacationApprovalPeriod != null && vacationApprovalPeriod.WorkingDays.GetValueOrDefault(true))
        newDate = date.AddWorkingDays(-nextVacationDays);
      else
        newDate = date.AddDays(-nextVacationDays);
      
      if (!string.IsNullOrEmpty(vacationApprovalPeriod?.ExactTimeDeadline))
        newDate = SetTimeInDate(newDate.Value, empl, int.Parse(vacationApprovalPeriod.ExactTimeDeadline.Substring(0,2)), int.Parse(vacationApprovalPeriod.ExactTimeDeadline.Substring(3,2)));

      return newDate;
    }
    
    /// <summary>
    /// Установить время по часовому поясу сотрудника.
    /// </summary>
    /// <param name="date">Дата.</param>
    /// <returns>Дату со временем по часовому поясу сотрудника</returns>
    [ExpressionElement("Установить время по часовому поясу сотрудника", "Возвращает дату со временем по часовому поясу сотрудника", "", "Сотрудник", "Часы", "Минуты")]
    public DateTime? SetTimeInDate(DateTime date, IEmployee empl, int hour, int minute)
    {
      if (date == null)
      {
        Logger.ErrorFormat("SetTimeInDate. Не заполнена дата.");
        return null;
      }
      
      if (empl == null)
      {
        Logger.ErrorFormat("SetTimeInDate. Не заполнен сотрудник.");
        return null;
      }
      
      var utcOffset = Users.GetUtcOffset(empl);
      
      if (utcOffset == null)
      {
        Logger.ErrorFormat("SetTimeInDate. Не заполнен часовой пояс сотрудника.");
        return null;
      }
      
      var dataTimeEmpl = new DateTimeOffset(date.Year, date.Month, date.Day, hour, minute, 0, utcOffset);

      return Calendar.FromUtcTime(dataTimeEmpl.UtcDateTime);
    }

    
    /// <summary>
    /// Получить максимальную дату окончания отпуска.
    /// </summary>
    /// <param name="document">Заявление на отпуск.</param>
    /// <returns>Максимальная дата окончания отпуска.</returns>
    [ExpressionElement("GetMaxVacationEndDateFunctionName", "GetMaxVacationEndDateFunctionDescription")]
    public string GetMaxVacationEndDate(IVacationStatementDocument document)
    {
      if (document == null)
      {
        Logger.ErrorFormat("GetMaxVacationEndDate. Не заполнено заявление на отпуск.");
        return string.Empty;
      }
      
      if (!document.AdditionalVacationDirRX.Any())
      {
        Logger.DebugFormat("GetMaxVacationEndDate. ИД документа: {0}. Нет информации о данных дополнительных отпусков.", document.Id);
        return string.Empty;
      }

      // Находим максимальную дату окончания
      var vacationWithMaxEndDate = document.AdditionalVacationDirRX.OrderByDescending(x => x.EndDate).FirstOrDefault();
      if (vacationWithMaxEndDate == null)
      {
        Logger.DebugFormat("GetMaxVacationEndDate. ИД документа: {0}. Не удалось определить максимальную дату окончания отпуска.", document.Id);
        return string.Empty;
      }
      
      // Форматируем дату в требуемый формат
      return vacationWithMaxEndDate.EndDate.Value.ToString("dd.MM.yyyy");
    }
    
    /// <summary>
    /// Получить даты новых отпусков.
    /// </summary>
    /// <param name="document">Заявление на отпуск.</param>
    /// <returns>Получить даты новых отпусков</returns>
    [ExpressionElement("GetNewVacationDatesFunctionName", "GetNewVacationDatesFunctionDescription")]
    public string GetNewVacationDates(IVacationStatementDocument document)
    {
      if (document == null)
      {
        Logger.ErrorFormat("GetNewVacationDates. Не заполнено заявление на отпуск.");
        return string.Empty;
      }
      
      if (!document.AdditionalVacationDirRX.Any())
      {
        Logger.DebugFormat("GetNewVacationDates. ИД документа: {0}. Нет информации о данных дополнительных отпусков.", document.Id);
        return string.Empty;
      }

      var mergedPeriods = MergeVacationPeriods(document);
      return FormatVacationPeriods(mergedPeriods);
    }
    
    /// <summary>
    /// Объеденить периоды отпусков.
    /// </summary>
    /// <param name="periods">Периоды отпусков.</param>
    /// <returns>Объединённые периоды отпусков.</returns>
    public virtual List<Structures.Module.IVacationPeriod> MergeVacationPeriods(IVacationStatementDocument document)
    {
      // Получаем все периоды отпусков и сортируем по дате начала
      var periods = document.AdditionalVacationDirRX
        .Where(x => x.StartDate.HasValue && x.EndDate.HasValue)
        .Select(v => Structures.Module.VacationPeriod.Create(v.StartDate, v.EndDate))
        .OrderBy(p => p.StartDate)
        .ToList();
      
      if (periods.Count == 0)
        return periods;

      var merged = new List<Structures.Module.IVacationPeriod>();
      var current = periods[0];
      bool isFirst = true;

      foreach (var next in periods)
      {
        // Пропускаем первый элемент.
        if (isFirst)
        {
          isFirst = false;
          continue;
        }
        
        // Проверяем наличие промежутков между отпусками.
        if (current.EndDate.Value.AddDays(1) >= next.StartDate)
        {
          current = Structures.Module.VacationPeriod.Create(
            current.StartDate < next.StartDate ? current.StartDate : next.StartDate,
            current.EndDate > next.EndDate ? current.EndDate : next.EndDate);
        }
        else
        {
          merged.Add(current);
          current = next;
        }
      }

      merged.Add(current);
      return merged;
    }
    
    /// <summary>
    /// Сформировать строку с периодами отпусков.
    /// </summary>
    /// <param name="periods">Периоды.</param>
    /// <returns>Строка, с периодами отпусков.</returns>
    public virtual string FormatVacationPeriods(List<Structures.Module.IVacationPeriod> periods)
    {
      return string.Join(", ",
                         periods.Select(p => DirRX.NNVacations.Resources.VacationPeriodStringFormat(
                           p.StartDate.Value.ToString("dd.MM.yyyy"),
                           p.EndDate.Value.ToString("dd.MM.yyyy"))
                                       ));
    }
    
    #endregion
  }
}