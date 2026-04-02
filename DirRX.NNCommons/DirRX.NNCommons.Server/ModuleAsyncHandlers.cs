using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;

namespace DirRX.NNCommons.Server
{
  public partial class ModuleAsyncHandlers
  {

    /// <remarks>
    /// АО для обхода бага 446803, который выводил в счётчике прекращённые непрочитанные задания в папке потока.
    /// </remarks>
    public virtual void MarkAsReadAbortedCompetitiveAssignments(DirRX.NNCommons.Server.AsyncHandlerInvokeArgs.MarkAsReadAbortedCompetitiveAssignmentsInvokeArgs args)
    {
      Logger.DebugFormat("MarkAsReadAbortedCompetitiveAssignments. Start. Task: {0}. Block: {1}.", args.TaskId, args.BlockUId);
      
      var assignments = Sungero.Workflow.Assignments.GetAll(a => a.Task.Id == args.TaskId &&
                                                            a.BlockUid == args.BlockUId &&
                                                            a.IsCompetitive.GetValueOrDefault() &&
                                                            a.Status == Sungero.Workflow.Assignment.Status.Aborted &&
                                                            a.IsRead == false);
      foreach (var assignment in assignments)
      {
        if (!Locks.GetLockInfo(assignment).IsLocked)
        {
          try
          {
            Locks.Lock(assignment);
            
            assignment.IsRead = true;
            assignment.Save();
          }
          catch (Exception ex)
          {
            Logger.DebugFormat("MarkAsReadAbortedCompetitiveAssignments. Assignment: {0}. Error: {1}.", assignment.Id, ex);
          }
          finally
          {
            if (Locks.GetLockInfo(assignment).IsLockedByMe)
              Locks.Unlock(assignment);
          }
        }
      }
      
      Logger.DebugFormat("MarkAsReadAbortedCompetitiveAssignments. End.");
    }

    /// <summary>
    /// Запись даты и времени подписания документов сотрудником/работодателем в соответствующие свойства.
    /// </summary>
    /// <param name="args"></param>
    public virtual void UpdateSigningDateInDocs(DirRX.NNCommons.Server.AsyncHandlerInvokeArgs.UpdateSigningDateInDocsInvokeArgs args)
    {
      var logPrefix = string.Format("UpdateSigningDateInDocs. RetryIteration: {0}.", args.RetryIteration);
      Logger.DebugFormat("{0} Начало выполнения асинхронного обработчика. ИД документов: {1}", logPrefix, args.DocsIds);
      var separator = NNCommons.PublicConstants.Module.Separator;
      var errorDocsIds = new List<long>();
      var processedDocsIds = new List<long>();
      // Обработать документы.
      foreach (var docId in args.DocsIds.Split(separator))
      {
        try
        {
          var documentId = long.Parse(docId);
          var document = DocRelatedToWorkBases.GetAll(d => d.Id == documentId).FirstOrDefault();
          if (Locks.TryLock(document))
          {
            if (args.IsEmployer)
              document.SignedByCheifDateDirRX = args.SigningDate;
            else
              document.SignedByEmpDateDirRX = args.SigningDate;
            document.Save();
            processedDocsIds.Add(documentId);
            Locks.Unlock(document);
          }
          else
          {
            var lockInfo = Locks.GetLockInfo(document);
            if (lockInfo.IsLocked && lockInfo.IsLockedByOther)
            {
              Logger.DebugFormat("{0}. Документ с ИД {1}. Не удалось установить блокировку документа: {2}. Асинхронный обработчик будет перезапущен.",
                                 logPrefix, document.Id, lockInfo.LockedMessage);
              errorDocsIds.Add(documentId);
              args.Retry = true;
              continue;
            }
          }
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0} Ошибка при выполнении АО. ИД документа {1}. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, docId, ex.Message, ex.StackTrace);
          errorDocsIds.Add(long.Parse(docId));
        }
      }

      var errorDocumentsIds = string.Join(separator.ToString(), errorDocsIds);
      var processedDocumentsIds = string.Join(separator.ToString(), processedDocsIds);
      if (args.Retry && !NNIntegration.PublicFunctions.Module.IsLastAsyncHandlerRetry(args.RetryIteration))
      {
        Logger.DebugFormat("{0} Документы с ИД:{1} обработаны.", logPrefix, processedDocumentsIds);
        Logger.DebugFormat("{0} Документы с ИД:{1} будут обработаны повторно.", logPrefix, errorDocumentsIds);
        args.DocsIds = errorDocumentsIds;
        Logger.DebugFormat("{0}. Работа АО завершена. Асинхронный обработчик будет запущен повторно.", logPrefix);
        return;
      }
      else
      {
        Logger.DebugFormat("{0}. Работа АО завершена. ИД обработанных документов: {1}", logPrefix, processedDocumentsIds);
      }
    }

    /// <summary>
    /// Добавление сотрудника в роль.
    /// </summary>
    /// <param name="args">args.EmployeeId - ИД сотрудника, которого нужно включить в роль.</param>
    /// <param name="args">args.RoleId - ИД роли, в которую нужно включить сотрудника.</param>
    public virtual void AddEmployeeToRole(DirRX.NNCommons.Server.AsyncHandlerInvokeArgs.AddEmployeeToRoleInvokeArgs args)
    {
      var logPrefix = string.Format("AddEmployeeToRole. RetryIteration: {0}. Ид сотрудника: {1}. Ид роли: {2}.", args.RetryIteration, args.EmployeeId, args.RoleId);
      Logger.DebugFormat("{0} Начало выполнения асинхронного обработчика.", logPrefix);

      var employee = OverrideSolution.Employees.GetAll(e => e.Id == args.EmployeeId).FirstOrDefault();
      if (employee == null)
      {
        Logger.DebugFormat("{0} Не найден сотрудник по Ид. Завершение выполнения асинхронного обработчика.", logPrefix);
        args.Retry = false;
        return;
      }
      
      var role = Sungero.CoreEntities.Roles.GetAll(r => r.Id == args.RoleId).FirstOrDefault();
      if (role == null)
      {
        Logger.DebugFormat("{0} Не найдена роль по Ид. Завершение выполнения асинхронного обработчика.", logPrefix);
        args.Retry = false;
        return;
      }
      
      if (!employee.IncludedIn(role))
      {
        var roleIsLocked = Locks.TryLock(role);
        if (!roleIsLocked)
        {
          var lockInfo = Locks.GetLockInfo(role);
          Logger.DebugFormat("{0} Не удалось установить блокировку на роль. Роль заблокирована пользователем: {1}. Время установки блокировки: {2}. Обработка повторится при следующем запуске АО.",
                             logPrefix, lockInfo.OwnerName, lockInfo.LockTime);
          args.Retry = true;
          return;
        }
        
        try
        {
          role.RecipientLinks.AddNew().Member = employee;
          role.Save();
          Logger.DebugFormat("{0} Сотрудник успешно включен роль. Завершение выполнения асинхронного обработчика.", logPrefix);
        }
        catch(Exception ex)
        {
          Logger.ErrorFormat("{0} Произошла ошибка при включении сотрудника в роль. Текст ошибки: {1}. StackTrace: {2}. Обработка повторится при следующем запуске АО.",
                             logPrefix, ex.Message, ex.StackTrace);
          args.Retry = true;
          return;
        }
      }
      else
        Logger.DebugFormat("{0} Cотрудник уже включен в роль. Завершение выполнения асинхронного обработчика.", logPrefix);
    }
    
    /// <summary>
    /// Усовершенствование подписи до формата CAdES-XL.
    /// </summary>
    /// <param name="args">args.entryId - ИД записи справочника Усовершенствование электронной подписи.</param>
    public virtual void ImprovementSignatureToCAdESXL(DirRX.NNCommons.Server.AsyncHandlerInvokeArgs.ImprovementSignatureToCAdESXLInvokeArgs args)
    {
      var entry = HRPro.HRCommons.ImproveSignatureDocuments.GetAll(e => e.Id == args.entryId && e.Status != HRPro.HRCommons.ImproveSignatureDocument.Status.Closed).FirstOrDefault();
      if (entry != null)
      {
        Logger.DebugFormat("ImprovementSignatureToCAdESXL. Начало обогащения подписи для документа с Id {0}, версия документа: {1}. Запись справочника ImproveSignatureDocument Id: {2}.", entry.Document.Id, entry.DocumentVersionId, entry.Id);
        var signatures = Signatures.Get(entry.Document.Versions.SingleOrDefault(v => v.Id == entry.DocumentVersionId)).Where(l => l.SignCertificate != null && !Functions.Module.IsSimpleSignature(l));
        var status = HRPro.HRCommons.ImproveSignatureDocument.Status.Active;
        var improveResult = Sungero.Core.ImproveSignatureResult.Success;
        var nextTryDate = Calendar.SqlMaxValue;
        var nextRetryTime = Calendar.SqlMaxValue;
        var improveMaxRetriesCount = 5;
        var signatureUpdateDate = Functions.Module.GetSignatureUpdateDate();
        var hasError = false;
        var exceptionCatched = false;
        
        Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}. Найдено подписей: {1}. Обогащение подписей нужно начинать за (дней): {2}.", entry.Id, signatures.Count(), signatureUpdateDate);
        
        foreach (var signature in signatures)
        {
          var expirationDateInSignature = HRPro.HRCommons.PublicFunctions.Module.GetCertificateExpirationDateInSignature(Signatures.GetSignatureInfo(signature));
          Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}. Дата окончания действия сертификата: {1}.", entry.Id, expirationDateInSignature);
          
          var signatureFormat = Signatures.GetSignatureInfo(signature).SignatureFormat;
          
          // Добавлена проверка на тип подписи. Если подпись имеет формат CadesT то обогащение нужно провести не зависимо от срока действия сертификата иначе обогащение до CAdES-XL может никогда не закончится.
          if (expirationDateInSignature < Calendar.Today.AddDays(signatureUpdateDate) || signatureFormat.Equals(SignatureFormat.CadesT))
          {
            try
            {
              if (Signatures.GetSignatureInfo(signature).SignatureFormat.Equals(SignatureFormat.CadesBes))
              {
                Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}, Ид подписи: {1}. Подпись будет обогащена до формата CadesT. ", entry.Id, signature.Id);
                improveResult = Signatures.ImproveToCadesT(signature);
                Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}, Ид подписи: {1}. Результат обогащения до формата CadesT: {2}. ", entry.Id, signature.Id, improveResult);
              }
              if (Signatures.GetSignatureInfo(signature).SignatureFormat.Equals(SignatureFormat.CadesT))
              {
                Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}, Ид подписи: {1}. Подпись будет обогащена до формата CadesXL. ", entry.Id, signature.Id);
                improveResult = Signatures.ImproveToCadesXL(signature, out nextTryDate);
                Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}, Ид подписи: {1}. Результат обогащения до формата CadesXL: {2}. Дата следующей попытки: {3}.",
                                   entry.Id, signature.Id, improveResult, nextTryDate);
              }
            }
            catch (Exception ex)
            {
              Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}, Ид подписи: {1}. Не удалось обогатить подпись. StackTrace: {2}", entry.Id, signature.Id, ex.StackTrace);
              
              if (exceptionCatched == false)
              {
                if (args.RetryIteration < improveMaxRetriesCount)
                {
                  var repeatInterval = (args.RetryIteration + 1) * 5;
                  nextRetryTime = Calendar.Now.AddMinutes(repeatInterval);
                  status = HRPro.HRCommons.ImproveSignatureDocument.Status.InProcessing;
                  Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}. Дата следующей попытки: {1}. Номер текущей попытки: {2}", entry.Id, nextRetryTime, args.RetryIteration);
                }
                else
                {
                  args.Retry = false;
                  hasError = true;
                  Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}. Превышено количество попыток обогащения подписи.", entry.Id);
                }
              }
              exceptionCatched = true;
            }
            
            if (improveResult.Equals(ImproveSignatureResult.SignatureVerificationFailed) || improveResult.Equals(ImproveSignatureResult.OutdatedSignatureCertificates))
            {
              hasError = true;
            }
            else if (improveResult.Equals(ImproveSignatureResult.TryLater))
            {
              if (nextTryDate < nextRetryTime)
                nextRetryTime = nextTryDate.AddHours(1);
              status = HRPro.HRCommons.ImproveSignatureDocument.Status.InProcessing;
            }
          }
          else
            Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}. Дата окончания действия сертификата: {1}. Дата обогащения еще не наступила.", entry.Id, expirationDateInSignature);
        }
        
        if (hasError == true)
          status = HRPro.HRCommons.ImproveSignatureDocument.Status.Error;
        
        if (nextRetryTime < Calendar.SqlMaxValue && nextRetryTime > Calendar.SqlMinValue && status != HRPro.HRCommons.ImproveSignatureDocument.Status.Error)
        {
          args.NextRetryTime = nextRetryTime;
          args.Retry = true;
        }
        
        Logger.DebugFormat("ImprovementSignatureToCAdESXL. Ид записи: {0}. Статус для обновления записи: {1}. Необходимость повтора: {2}.", entry.Id, status, args.Retry);
        
        var asyncRightsHandler = HRPro.HRCommons.AsyncHandlers.UpdateImproveStatus.Create();
        asyncRightsHandler.entryId = entry.Id;
        asyncRightsHandler.status = status.ToString();
        asyncRightsHandler.LastImproveDate = Calendar.Now;
        asyncRightsHandler.ExecuteAsync();
        
        Logger.DebugFormat("ImprovementSignatureToCAdESXL. Завершено обогащение подписи для документа с Ид: {0}, версия документа: {1}. Запись справочника ImproveSignatureDocument Id: {2}.", entry.Document.Id, entry.DocumentVersionId, entry.Id);
      }
      else
      {
        args.Retry = false;
        if (HRPro.HRCommons.ImproveSignatureDocuments.GetAll(e => e.Id == args.entryId && e.Status == HRPro.HRCommons.ImproveSignatureDocument.Status.Closed).Any())
          Logger.ErrorFormat("ImprovementSignatureToCAdESXL. Не найдены записи справочника ImproveSignatureDocument с Id {0} в статусе Закрыто.", args.entryId);
        else
          Logger.ErrorFormat("ImprovementSignatureToCAdESXL. Не найдены записи справочника ImproveSignatureDocument с Id {0}.", args.entryId);
      }
    }
    
    /// <summary>
    /// Обновить состав участников обобщённой роли после изменения ограничения видимости документов.
    /// </summary>
    /// <param name="args">Аргументы АО.</param>
    public virtual void UpdateGeneralRoleAfterChange(DirRX.NNCommons.Server.AsyncHandlerInvokeArgs.UpdateGeneralRoleAfterChangeInvokeArgs args)
    {
      var logPrefix = string.Format("UpdateGeneralRoleAfterDelete. RetryIteration: {0}. Ид ограничения видимости: {1}.", args.RetryIteration, args.DocumentsVisabilityLimitationId);
      Logger.DebugFormat("{0} Начало выполнения асинхронного обработчика.", logPrefix);
      
      var limitation = DocumentsVisabilityLimitations.GetAll(x => x.Id == args.DocumentsVisabilityLimitationId).FirstOrDefault();
      if (limitation == null)
      {
        Logger.DebugFormat("{0} Не найдено ограничение видимости документов. Конец выполнения асинхронного обработчика.", logPrefix);
        args.Retry = false;
        return;
      }
      
      logPrefix += string.Format(" Ид обобщённой роли: {0}.", limitation.Role.Id);
      Logger.DebugFormat("{0} Признак изменения состава: {1}. Признак изменения состояния: {2}. Текущее состояние: {3}.",
                         logPrefix, args.EmployeesIsChanged, args.StatusIsChanged, DocumentsVisabilityLimitations.Info.Properties.Status.GetLocalizedValue(limitation.Status));
      
      // Найти и заблокировать обобщённую роль.
      var result = Functions.Module.TryFindAndBlockGeneralRole(logPrefix, limitation.Role.Id);
      var roleIsLocked = result.IsLocked;
      var generalRole = result.GeneralRole;
      if (generalRole == null || !roleIsLocked)
      {
        args.Retry = generalRole != null;
        Logger.DebugFormat("{0} Конец выполнения асинхронного обработчика.", logPrefix);
        return;
      }
      
      Logger.DebugFormat("{0} Всего сотрудников в составе обобщённой роли до синхронизации: {1}.", logPrefix, generalRole.RecipientLinks.Count);
      
      // Синхронизация состава обобщённой роли.
      try
      {
        
        #region Изменение состава ограничения видимости.
        if (limitation.Status == NNCommons.DocumentsVisabilityLimitation.Status.Active && !args.StatusIsChanged && args.EmployeesIsChanged)
        {
          // Удалить сотрудников.
          var notRemovedEmployees = generalRole.RecipientLinks
            .Where(e =>
                   !limitation.Employees.Any(x => x.Employee.Id == e.Member.Id) &&
                   !PublicFunctions.GeneralRole.HasActiveLimitationsForEmployee(generalRole, Employees.As(e.Member)))
            .Select(e => e.Member.Id).ToList();
          
          PublicFunctions.GeneralRole.RemoveEmployees(generalRole, notRemovedEmployees, false);
          
          // Добавить сотрудников.
          var notAddedEmployees = limitation.Employees.Where(e => !generalRole.RecipientLinks.Any(x => x.Member.Id == e.Employee.Id)).Select(e => e.Employee).ToList();
          PublicFunctions.GeneralRole.AddEmployees(generalRole, notAddedEmployees);
        }
        #endregion
        
        #region Изменение состояния ограничения видимости.
        if (args.StatusIsChanged && limitation.Status == NNCommons.DocumentsVisabilityLimitation.Status.Closed)
        {
          // Удалить сотрудников.
          var removedEmployees = limitation.Employees.Select(e => e.Employee.Id).ToList();
          PublicFunctions.GeneralRole.RemoveEmployees(generalRole, removedEmployees, true);
        }
        
        if (args.StatusIsChanged && limitation.Status == NNCommons.DocumentsVisabilityLimitation.Status.Active)
        {
          // Добавить сотрудников.
          var addedEmployees = limitation.Employees.Select(e => e.Employee).ToList();
          PublicFunctions.GeneralRole.AddEmployees(generalRole, addedEmployees);
        }
        #endregion
        
        generalRole.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0} Ошибка при выполнении АО. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      finally
      {
        Locks.Unlock(generalRole);
      }
      
      Logger.DebugFormat("{0} Всего сотрудников в составе обобщённой роли после синхронизации: {1}.", logPrefix, generalRole.RecipientLinks.Count);
      Logger.DebugFormat("{0} Конец выполнения асинхронного обработчика.", logPrefix);
    }

    /// <summary>
    /// Обновить состав участников обобщённой роли после удаления или изменения обобщённой роли ограничения записи видимости документов.
    /// </summary>
    /// <param name="args">Аргументы АО.</param>
    public virtual void UpdateGeneralRoleAfterDelete(DirRX.NNCommons.Server.AsyncHandlerInvokeArgs.UpdateGeneralRoleAfterDeleteInvokeArgs args)
    {
      var logPrefix = string.Format("UpdateGeneralRoleAfterDelete. RetryIteration: {0}. Ид обобщённой роли: {1}.", args.RetryIteration, args.GeneralRoleId);
      Logger.DebugFormat("{0} Начало выполнения асинхронного обработчика. ", logPrefix);
      Logger.DebugFormat("{0} Список Ид сотрудников из удалённой/изменённой записи справочника, которых нужно удалить из обобщённой роли: {1}.", logPrefix, args.EmployeesIds);
      
      // Найти и заблокировать обобщённую роль.
      var result = Functions.Module.TryFindAndBlockGeneralRole(logPrefix, args.GeneralRoleId);
      var roleIsLocked = result.IsLocked;
      var generalRole = result.GeneralRole;
      if (generalRole == null || !roleIsLocked)
      {
        args.Retry = generalRole != null;
        Logger.DebugFormat("{0} Конец выполнения асинхронного обработчика.", logPrefix);
        return;
      }
      
      Logger.DebugFormat("{0} Всего сотрудников в составе обобщённой роли до синхронизации: {1}.", logPrefix, generalRole.RecipientLinks.Count);
      
      // Синхронизация состава обобщённой роли.
      try
      {
        var employeesIds = args.EmployeesIds.Split(',').Select(id => long.Parse(id)).ToList();
        PublicFunctions.GeneralRole.RemoveEmployees(generalRole, employeesIds, true);
        generalRole.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0} Ошибка при выполнении АО. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      finally
      {
        Locks.Unlock(generalRole);
      }
      
      Logger.DebugFormat("{0} Всего сотрудников в составе обобщённой роли после синхронизации: {1}.", logPrefix, generalRole.RecipientLinks.Count);
      Logger.DebugFormat("{0} Конец выполнения асинхронного обработчика.", logPrefix);
    }
    
    /// <summary>
    /// Запустить проверку документов в файловом хранилище.
    /// </summary>
    /// <param name="args">args.employeeId - ИД сотрудника инициатора.</param>
    /// <param name="args">args.beginDate - Дата начала, для поиска измененных файлов.</param>
    /// <param name="args">args.endDate - Дата окончания, для поиска измененных файлов.</param>
    /// <param name="args">args.inception - Признак первого запуска АО.</param>
    /// <param name="args">args.iteration - Номер итерации.</param>
    public virtual void VerifyDocumentsInStorage(DirRX.NNCommons.Server.AsyncHandlerInvokeArgs.VerifyDocumentsInStorageInvokeArgs args)
    {
      var logPrefix = string.Format("VerifyDocumentsInStorage. ИД сотрудника: {0}, Дата начала: {1}, Дата окончания: {2}, Итерация: {3}",
                                    args.employeeId, args.beginDate, args.endDate, args.iteration++);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      
      var constant = PublicFunctions.Module.Remote.GetConstant(Constants.Module.ConstantsSetting.VerifyDocumentsCountInIteration.Guid);
      var lockInfo = Locks.GetLockInfo(constant);
      var step = Functions.Module.GetIntValue(Constants.Module.ConstantsSetting.VerifyDocumentsCountInIteration.Guid) ?? Constants.Module.DefaultDocsCountInIteration;
      var tableName = Constants.Module.VerifyDocumentsTableName;
      
      if (constant == null)
      {
        Logger.ErrorFormat("{0}. Не найдена прикладная константа: {1}. Асинхронный обработчик будет приостановлен.",
                           logPrefix, Constants.Module.ConstantsSetting.VerifyDocumentsCountInIteration.Name);
        args.Retry = false;
        return;
      }
      
      if (lockInfo.IsLocked)
      {
        Logger.ErrorFormat("{0}. Прикладная константа: {1}. Не удалось установить блокировку прикладной константы: {2}. Асинхронный обработчик будет перезапущен.",
                           logPrefix, Constants.Module.ConstantsSetting.VerifyDocumentsCountInIteration.Name, lockInfo.LockedMessage);
        args.Retry = true;
        return;
      }
      
      Locks.Lock(constant);
      try
      {
        // Словарь <id документа, версии через запятую>.
        var docs = new Dictionary<long, string>();
        using (var command = SQL.GetCurrentConnection().CreateCommand())
        {
          // При первом запуске, подготовить таблицу с обрабатываемыми документами.
          if (args.inception)
          {
            // Создать чистую таблицу.
            Logger.DebugFormat("{0}. Таблица: {1}. Выполнение SQL-запроса очистки таблицы.", logPrefix, tableName);
            command.CommandText = string.Format(NNCommons.Queries.Module.PrepareChangedDocsTable, tableName);
            command.ExecuteNonQuery();
            // Заполнить таблицу.
            Logger.DebugFormat("{0}. Таблица: {1}. Выполнение SQL-запроса запись данных в таблицу.", logPrefix, tableName);
            command.CommandText = string.Format(NNCommons.Queries.Module.FillChangedDocsTable, tableName, Constants.Module.Separator, args.beginDate, args.endDate);
            command.ExecuteNonQuery();
          }
          
          Logger.DebugFormat("{0}. Таблица: {1}. Выполнение SQL-запроса чтения данных.", logPrefix, tableName);
          command.CommandText = string.Format(NNCommons.Queries.Module.SelectFromChangedDocsTable, tableName, step);
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
              docs[reader.GetInt64(0)] = reader.GetString(1);
          }
        }
        
        var errorText = string.Empty;
        var docName = string.Empty;
        var hyperlink = string.Empty;
        foreach (var doc in docs)
        {
          var document = Sungero.Content.ElectronicDocuments.GetAll().FirstOrDefault(x => x.Id == doc.Key);
          if (document != null)
          {
            // Получить измененные и сохранившиеся версии.
            var versions = doc.Value.Split(Constants.Module.Separator).Select(x => int.Parse(x));
            var changedVersions = document.Versions.Where(x => versions.Any(v => v == x.Number));
            var versionInfoList = new List<Structures.Module.IVersionVerifyInfo>();
            foreach (var version in changedVersions)
            {
              var result = Functions.Module.CheckReadingDocumentBody(version.Body);
              result.Number = version.Number;
              versionInfoList.Add(result);
            }
            
            if (document.HasPublicBody && changedVersions.Any(x => x.Number == document.LastVersion.Number))
            {
              var result = Functions.Module.CheckReadingDocumentBody(document.LastVersion.PublicBody);
              result.Number = document.LastVersion.Number;
              result.IsPublicBody = true;
              versionInfoList.Add(result);
            }
            
            errorText = string.Join("; ", versionInfoList.Where(x => !x.Valid).Select(x => string.Format("{0}. {1}", x.Number, x.Error)));
            docName = document.Name;
            hyperlink = Hyperlinks.Get(document);
          }
          else
          {
            errorText = "Документ не найден.";
          }
          
          // Запись результата во временную таблицу.
          using (var command = SQL.GetCurrentConnection().CreateCommand())
          {
            command.CommandText = string.Format(NNCommons.Queries.Module.UpdateRowDocsTable, tableName, doc.Key, docName, hyperlink, "true", errorText);
            command.ExecuteNonQuery();
          }
        }
        
        // Завершение.
        if (docs.Count() < step)
        {
          args.Retry = false;
          var documentsCount = 0;
          
          var invalidResults = new List<NNCommons.Structures.Module.IDocumentVerifyInfo>();
          using (var command = SQL.GetCurrentConnection().CreateCommand())
          {
            
            Logger.DebugFormat("{0}. Таблица: {1}. Выполнение SQL-запроса чтения данных.", logPrefix, tableName);
            command.CommandText = string.Format(NNCommons.Queries.Module.GetAllBrokenDocumentsDocsTable, tableName);
            using (var reader = command.ExecuteReader())
            {
              while (reader.Read())
              {
                var documentVerifyInfo = NNCommons.Structures.Module.DocumentVerifyInfo.Create();
                documentVerifyInfo.DocumentId = reader.GetInt64(0);
                documentVerifyInfo.DocumentName = reader.GetString(1);
                documentVerifyInfo.Hyperlink = reader.GetString(2);
                documentVerifyInfo.Versions = reader.GetString(3);
                documentVerifyInfo.Valid = false;
                documentVerifyInfo.Error = reader.GetString(4);
                invalidResults.Add(documentVerifyInfo);
                documentsCount = documentsCount + 1;
              }
            }
          }
          
          // Сформировать отчет и записать его в тело документа.
          Logger.DebugFormat("{0}. Занесение результатов в отчет.", logPrefix);
          var documentReport = Sungero.Docflow.SimpleDocuments.Create();
          documentReport.Name = DirRX.NNCommons.Resources.VerifyDocumentsInStorageReportName;
          using (var stream = new MemoryStream())
          {
            var title = DirRX.NNCommons.Resources.VerifyDocumentsInStorageReportTitleFormat(args.beginDate, args.endDate);
            var streamReport = DirRX.NNCommons.IsolatedFunctions.VerifyDocumentsInStorage.BuildVerifyDocumentsReport(stream, invalidResults, true, title, documentsCount);
            var createdVersion = documentReport.CreateVersionFrom(streamReport, Constants.Module.VerifyRepordDocExtension);
            createdVersion.Note = string.Format("Версия {0} (c {1} по {2}), ",
                                                createdVersion.Number,
                                                args.beginDate.ToShortDateString(),
                                                args.endDate.ToShortDateString());
          }
          documentReport.Save();
          
          Logger.DebugFormat("{0}. Отправка задачи с отчетом инициатору.", logPrefix);
          // Создать задачу инициатору.
          var performers = Sungero.Company.Employees.GetAll().FirstOrDefault(x => x.Id == args.employeeId);
          var subject = string.Format(DirRX.NNCommons.Resources.VerifyDocumentsInStorageTaskName,
                                      args.beginDate.ToShortDateString(),
                                      args.endDate.ToShortDateString());
          
          var task = Sungero.Workflow.SimpleTasks.Create(subject, performers);
          task.ActiveText = DirRX.NNCommons.Resources.VerifyDocumentsInStorageTaskActiveTextFormat(args.beginDate,
                                                                                                   args.endDate);
          task.Attachments.Add(documentReport);
          task.Start();
        }
        else
        {
          args.Retry = true;
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Произошла ошибка в работе АО: {1}. Асинхронный обработчик будет приостановлен. StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
        args.Retry = false;
      }
      finally
      {
        Locks.Unlock(constant);
      }
      
      if (args.Retry)
      {
        Logger.DebugFormat("{0}. Конец выполнения итерации. Асинхронный обработчик будет запущен повторно.", logPrefix);
        Functions.Module.VerifyDocumentsInStorage(args.beginDate, args.endDate, args.employeeId, false, args.iteration);
        args.Retry = false;
        return;
      }
      else
      {
        Logger.DebugFormat("{0}. Работа АО завершена.", logPrefix);
      }
    }
  }
}