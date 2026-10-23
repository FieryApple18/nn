using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations.Server
{
  public partial class ModuleAsyncHandlers
  {

    /// <summary>
    /// Закрыть все записи спр. Информация по отпускам, связанные с графиком отпусков.
    /// Запускается порционно.
    /// </summary>
    /// <param name="args">InformationIds - ИД записей для закрытия.</param>
    public virtual void CloseVacationInformations(DirRX.NNVacations.Server.AsyncHandlerInvokeArgs.CloseVacationInformationsInvokeArgs args)
    {
      var logPrefix = "CloseVacationInformations";
      Logger.DebugFormat("{0}.Список ИД записей: {1}. Итерация: {2}. Начало выполнения.", logPrefix, args.InformationIds, args.RetryIteration);
      
      var allInfoIds = args.InformationIds.Split(Constants.Module.ListIdsDelimiter).Select(x => long.Parse(x)).ToList();
      var processedInfoIds = new List<long>();
      foreach (var infoId in allInfoIds)
      {
        var info = VacationsInformations.GetAll().Where(i => i.Id == infoId).FirstOrDefault();
        if (info != null && info.Status != NNVacations.VacationsInformation.Status.Closed)
        {
          try
          {
            if (!Locks.TryLock(info))
            {
              Logger.DebugFormat("{0}. ИД информации по отпуску: {1} заблокирован.", logPrefix, info.Id);
              continue;
            }
            
            info.Status = NNVacations.VacationsInformation.Status.Closed;
            info.Save();
            processedInfoIds.Add(infoId);
          }
          catch (Exception ex)
          {
            Logger.ErrorFormat("{0}. ИД информации по отпуску: {1}. Ошибка при сохранении элемента: {2}. StackTrace: {3}.",
                               logPrefix, infoId, ex.Message, ex.StackTrace);
          }
          finally
          {
            if (Locks.GetLockInfo(info).IsLockedByMe)
              Locks.Unlock(info);
          }
        }
        else
        {
          processedInfoIds.Add(infoId);
          Logger.DebugFormat("{0}. ИД информации по отпуску: {1}. Элемент пропущен.", logPrefix, infoId);
        }
      }
      var remainingIds = allInfoIds.Except(processedInfoIds);
      args.Retry = remainingIds.Any();
      if (args.Retry && args.RetryIteration >= args.MaxRetryCount)
      {
        Logger.ErrorFormat("{0}. Превышено количество попыток. ИД необработанных записей: {1}", logPrefix, string.Join(Constants.Module.ListIdsDelimiter.ToString(), remainingIds.Select(x => x.ToString())));
        args.Retry = false;
        var document = VacationScheduleDocuments.GetAll().Where(x => x.Id == args.VacationScheduleDocumentId).FirstOrDefault();
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(DirRX.NNVacations.Resources.CloseVacationInformationErrorNoticeSubject, document?.ScheduleResponsibleDirRX);
        notice.ActiveText = DirRX.NNVacations.Resources.CloseVacationInformationErrorNoticeActiveTextFormat(string.Join(Constants.Module.ListIdsDelimiter.ToString(), remainingIds.Select(x => x.ToString())));
        notice.Start();
      }
      else if (args.Retry)
      {
        args.InformationIds = string.Join(Constants.Module.ListIdsDelimiter.ToString(), remainingIds);
        Logger.DebugFormat("{0}. Конец выполнения итерации. Асинхронный обработчик будет запущен повторно", logPrefix);
      }
      else
      {
        Logger.DebugFormat("{0}. Завершение работы Асинхронного обработчика.", logPrefix);
      }
    }
    
    /// <summary>
    /// Создать и запустить задачу Ведение графика отпусков.
    /// </summary>
    /// <param name="args">IncomingMessageId - ИД записи справочника очереди входящих сообщений интеграции.</param>
    /// <param name="args">VacationScheduleDocumentId - ИД документа график отпусков.</param>
    /// <param name="args">IterationNumber - номер итерации загрузки данных.</param>
    public virtual void CreateVacationInfoAndStartTask(DirRX.NNVacations.Server.AsyncHandlerInvokeArgs.CreateVacationInfoAndStartTaskInvokeArgs args)
    {
      var logPrefix = string.Format("CreateVacationInfoAndStartTask. ИД записи: {0}. ИД документа: {1}. Номер шага: {2}",
                                    args.IncomingMessageId, args.VacationScheduleDocumentId, args.StepNumber);
      Logger.DebugFormat("{0}. Итерация: {1}. Начало выполнения.", logPrefix, args.RetryIteration);
      var incomingMessage = NNIntegration.IncomingMessages.GetAll(m => m.Id == args.IncomingMessageId).FirstOrDefault();
      var vacationScheduleDocument = OverrideSolution.VacationScheduleDocuments.GetAll(d => d.Id == args.VacationScheduleDocumentId).FirstOrDefault();
      var errorMessage = string.Empty;
      if (incomingMessage == null)
        errorMessage += "Не найдена запись справочника Входящие интеграционные сообщения. ";
      if (vacationScheduleDocument == null)
        errorMessage += "Не найден документ график отпусков.";
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return;
      }
      var messageText = string.Empty;
      var vacScheduleDocInfoFromSAP = NNIntegration.Structures.Module.VacationScheduleDocumentInfoFromSAP.Create();
      try
      {
        messageText = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
        vacScheduleDocInfoFromSAP = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(messageText, vacScheduleDocInfoFromSAP);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Асинхронный обработчик остановлен. Произошла ошибка при обработке входящего интеграционного сообщения: {1}. StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
        return;
      }
      var step = NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.CountCreateVacationInfoAndStartTaskInIteration.Guid) ??
        Constants.Module.ConstantsDefaultValues.CountCreateVacationInfoAndStartTaskInIteration;
      var vacInformations = vacScheduleDocInfoFromSAP.Employees;
      var partVacInformations = vacInformations.Skip(step * args.StepNumber).Take(step).ToList();
      
      foreach (var vacInfo in partVacInformations)
      {
        Logger.DebugFormat("{0}. Табельный номер сотрудника: {1}. Начало обработки.", logPrefix, vacInfo.EmplPersNum);
        Transactions.Execute(() =>
                             {
                               try
                               {
                                 this.CreateVacationsInformation(logPrefix, vacationScheduleDocument, vacInfo);
                               }
                               catch (Exception ex)
                               {
                                 Logger.ErrorFormat("{0}. Табельный номер сотрудника: {1}. Ошибка при сохранении информации по отпуску: {2}. StackTrace: {3}.",
                                                    logPrefix, vacInfo.EmplPersNum, ex.Message, ex.StackTrace);
                               }
                             });
      }
      
      if (partVacInformations.Count != 0)
      {
        Logger.DebugFormat("{0}. Конец выполнения шага. Будет запущен новый асинхронный обработчик для выполнения дальнейшей обработки.", logPrefix);
        
        var nextIterationHandler = AsyncHandlers.CreateVacationInfoAndStartTask.Create();
        nextIterationHandler.IncomingMessageId = args.IncomingMessageId;
        nextIterationHandler.VacationScheduleDocumentId = args.VacationScheduleDocumentId;
        nextIterationHandler.StepNumber = ++args.StepNumber;
        nextIterationHandler.ExecuteAsync();
        
        return;
      }
      else
      {
        var processedEmployees = VacationsInformations.GetAll()
          .Where(v => Equals(v.BusinessUnit, vacationScheduleDocument.BusinessUnit) &&
                 (vacationScheduleDocument.Department == null || Equals(v.Department, vacationScheduleDocument.Department)) &&
                 v.Year.Value.Year == vacationScheduleDocument.Year.Value.Year)
          .Select(v => v.Employee.PersonnelNumber);
        
        //Собираем список ошибок неудачных созданий информации об отпуске чтобы потом отправить уведомление ответственному.
        var errors = new List<string>();
        
        var unprocessedEmployees = vacInformations.Select(v => v.EmplPersNum).Except(processedEmployees).ToList();
        if (unprocessedEmployees.Any())
        {
          var unprocessedVacInformations = vacInformations.Where(v => unprocessedEmployees.Contains(v.EmplPersNum));
          foreach (var vacInfo in unprocessedVacInformations)
          {
            Logger.DebugFormat("{0}. Табельный номер сотрудника: {1}. Начало повторной обработки.", logPrefix, vacInfo.EmplPersNum);
            Transactions.Execute(() =>
                                 {
                                   try
                                   {
                                     this.CreateVacationsInformation(logPrefix, vacationScheduleDocument, vacInfo);
                                   }
                                   catch (Exception ex)
                                   {
                                     Logger.ErrorFormat("{0}. Табельный номер сотрудника: {1}. Ошибка при сохранении информации по отпуску: {2}. StackTrace: {3}.",
                                                        logPrefix, vacInfo.EmplPersNum, ex.Message, ex.StackTrace);
                                     errors.Add(DirRX.NNVacations.Resources.CreateVacationsInformationErrorFormat(vacInfo.EmplPersNum, ex.Message));
                                     args.Retry = true;
                                   }
                                 });
          }
        }
        
        var retryExceeded = args.Retry && args.RetryIteration >= args.MaxRetryCount;
        if (retryExceeded)
          Logger.ErrorFormat("{0}. Превышено количество попыток создания записей справочника Информация по отпуску.", logPrefix);
        
        if (!args.Retry || retryExceeded)
        {
          args.Retry = false;
          
          var errorsText = string.Join(Environment.NewLine, errors);
          
          Logger.DebugFormat("{0}. Начало создания задачи Ведение графика отпусков.", logPrefix);
          Transactions.Execute(() =>
                               {
                                 try
                                 {
                                   var task = VacationScheduleManagementTasks.Create();
                                   task.Subject = Resources.SubjectVacationScheduleManagementTaskFormat(vacationScheduleDocument.Name);
                                   task.BusinessUnit = BusinessUnits.As(vacationScheduleDocument.BusinessUnit);
                                   task.Department = Departments.As(vacationScheduleDocument.Department);
                                   task.Year = vacationScheduleDocument.Year;
                                   task.DocumentGroup.VacationScheduleDocuments.Add(vacationScheduleDocument);
                                   task.LoadingErrorMessage = errorsText;
                                   task.Save();
                                   task.Start();
                                   Logger.DebugFormat("{0}. Ид задачи Ведение графика отпусков: {1}. Задача успешно запущена.", logPrefix, task.Id);
                                 }
                                 catch (Exception ex)
                                 {
                                   var errorFormat = string.Format("{0}. Ошибка при запуске задачи Ведение графика отпусков: {1}. StackTrace: {2}.",
                                                      logPrefix, ex.Message, ex.StackTrace);
                                   
                                   Logger.ErrorFormat(errorFormat);
                                   var errorSendNotifyText = NNIntegration.PublicFunctions.Module.SendNotificationAboutErrors(Resources.CreateVacationInfoAndStartTaskSubject, errorFormat, true, incomingMessage);
                                   if (string.IsNullOrEmpty(errorSendNotifyText))
                                     Logger.DebugFormat("{0}. Уведомление ответственному за ошибки успешно отправлено.", logPrefix);
                                   else
                                     Logger.ErrorFormat("{0}. {1}.", logPrefix, errorSendNotifyText);
                                 }
                               });
        }
      }
      
      if (args.Retry)
        Logger.DebugFormat("{0}. Асинхронный обработчик будет запущен повторно", logPrefix);
      else
        Logger.DebugFormat("{0}. Завершение Асинхронного обработчика \"Создать и запустить задачу Ведение графика отпусков\".", logPrefix);
    }
    
    /// <summary>
    /// Создать запись справочника Информация по отпуску.
    /// </summary>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <param name="vacationScheduleDocument">Документ График отпусков.</param>
    /// <param name="vacInfo">Информация об отпуске.</param>
    /// <remarks>Используется в АО CreateVacationInfoAndStartTask.</remarks>
    private void CreateVacationsInformation(string logPrefix, IVacationScheduleDocument vacationScheduleDocument, NNIntegration.Structures.Module.IVacationInfoDateTimeFromSAP vacInfo)
    {
      var errorMessage = string.Empty;
      var employee = Employees.GetAll(e => e.PersonnelNumber == vacInfo.EmplPersNum).FirstOrDefault();
      
      if (employee == null)
        errorMessage += "Не найден Сотрудник. ";
      if (!vacInfo.DateStart.HasValue)
        errorMessage += "Не заполнена Дата начала. ";
      if (!vacInfo.DateEnd.HasValue)
        errorMessage += "Не заполнена Дата окончания. ";
      if (vacInfo.DayCount == null)
        errorMessage += "Не заполнено Кол-во дней. ";
      if (!string.IsNullOrEmpty(errorMessage))
      {
        throw AppliedCodeException.Create(errorMessage.Trim());
      }
      else
      {
        var vacationType = VacationKinds.GetAll(v => v.PlannedAbsenceDirRX == vacInfo.TypeVacation &&
                                                Equals(v.BusinessUnitDirRX, employee.BusinessUnit))
          .FirstOrDefault();
        
        if (vacationType == null)
          throw AppliedCodeException.Create(string.Format("Тип отпуска: {0}. Не найден Тип отпуска. ", vacInfo.TypeVacation));
        
        var vacationsInformation = VacationsInformations.Create();
        vacationsInformation.BusinessUnit = BusinessUnits.As(vacationScheduleDocument.BusinessUnit);
        vacationsInformation.Department = Departments.As(employee.Department);
        vacationsInformation.Year = vacationScheduleDocument.Year;
        vacationsInformation.Employee = employee;
        vacationsInformation.VacationType = vacationType;
        vacationsInformation.StartDate = vacInfo.DateStart;
        vacationsInformation.EndDate = vacInfo.DateEnd;
        vacationsInformation.DaysAmount = vacInfo.DayCount;
        vacationsInformation.VacationSchedule = vacationScheduleDocument;
        vacationsInformation.ConsentHRPro = employee.ConsentHRPro == DirRX.OverrideSolution.Employee.ConsentHRPro.Signed;
        vacationsInformation.Save();
        Logger.DebugFormat("{0}. Табельный номер сотрудника: {1}. Запись справочника Информация по отпуску успешно создана.", logPrefix, vacInfo.EmplPersNum);
      }
    }
    
    /// <summary>
    /// Сформировать индивидуальные листы ознакомления.
    /// </summary>
    /// <param name="args">AcquiantanceOption - Вариант ознакомления (Бумажный/Электронный).</param>
    /// <param name="args">Consent - Согласие на КЭДО (true/false).</param>
    /// <param name="args">VacationScheduleManagementTaskId - Ид задачи Ведение графика отпусков.</param>
    /// <param name="args">EntitiesIds - Ид сущностей.</param>
    /// <param name="args">PerformerId - Ид исполнителя задания.</param>
    public virtual void CreateIndividualAcquiantanceVacScheduleLists(DirRX.NNVacations.Server.AsyncHandlerInvokeArgs.CreateIndividualAcquiantanceVacScheduleListsInvokeArgs args)
    {
      var logPrefix = string.Format("CreateIndividualAcquiantanceVacScheduleLists. Ид задачи Ведение графика отпусков: {0}. Вариант ознакомления: {1}. Согласие на КЭДО: {2}",
                                    args.VacationScheduleManagementTaskId, args.AcquiantanceOption, args.Consent);
      Logger.DebugFormat("{0}. Итерация: {1}. Начало выполнения.", logPrefix, args.RetryIteration);
      var acquiantanceOption = DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.InPaper.Value == args.AcquiantanceOption ?
        DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.InPaper :
        DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.Electronic;
      var consent = args.Consent;
      var performer = Users.GetAll(u => u.Id == args.PerformerId).FirstOrDefault();
      var task = VacationScheduleManagementTasks.GetAll(t => t.Id == args.VacationScheduleManagementTaskId).FirstOrDefault();
      if (task == null)
      {
        Logger.ErrorFormat("{0}. Не найдена задача Ведение графика отпусков. Асинхронный обработчик остановлен.", logPrefix);
        return;
      }
      var vacationScheduleDocument = task.DocumentGroup.VacationScheduleDocuments.FirstOrDefault();
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.IndividualAcquiantanceVacScheduleListKind);
      var template = Functions.Module.GetTemplateByKind(docKind, task.BusinessUnit);
      
      var errorMessage = string.Empty;
      if (vacationScheduleDocument == null)
        errorMessage += "Не найден документ График отпусков. ";
      if (template == null)
        errorMessage += "Не найден шаблон индивидуального листа ознакомления. ";
      if (docKind == null)
        errorMessage += "Не найден вид документа Индивидуальный лист ознакомления с графиком отпусков.";
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.NoticeSubjectCreateAcquiantanceVacScheduleListsError, task.BusinessUnit.VacationScheduleResponsibleDirRX);
        notice.ActiveText = errorMessage;
        notice.Start();
        return;
      }
      
      var countCreateAcquiantanceListsInIteration = NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.CountCreateAcquiantanceListsInIteration.Guid) ??
        Constants.Module.ConstantsDefaultValues.CountCreateAcquiantanceListsInIteration;
      if (consent)
      {
        var allEntitiesIds = new List<string>();
        var processedEntitesIds = new List<string>();
        if (!string.IsNullOrEmpty(args.EntitiesIds))
        {
          allEntitiesIds = args.EntitiesIds.Split(Constants.Module.ListIdsDelimiter).ToList();
        }
        else
        {
          var documents = Functions.VacationScheduleManagementTask.GetExpiredVacationScheduleDocuments(task);
          allEntitiesIds = documents.Select(d => d.Id.ToString()).ToList();
        }
        var processingCount = countCreateAcquiantanceListsInIteration >= allEntitiesIds.Count ? allEntitiesIds.Count : countCreateAcquiantanceListsInIteration;
        for (int i = 0; i < processingCount; i++)
        {
          Logger.DebugFormat("{0}. Ид документа: {1}. Начало обработки документа.", logPrefix, allEntitiesIds[i]);
          Transactions.Execute(() =>
                               {
                                 var document = VacationScheduleDocuments.GetAll(e => e.Id == long.Parse(allEntitiesIds[i])).FirstOrDefault();
                                 var lockInfo = Locks.GetLockInfo(document);
                                 if (!lockInfo.IsLocked)
                                 {
                                   try
                                   {
                                     using(var stream = template.LastVersion.Body.Read())
                                     {
                                       document.CreateVersionFrom(stream, template.AssociatedApplication.Extension);
                                       var internalEntity = (Sungero.Domain.Shared.IExtendedEntity)document;
                                       internalEntity.Params[Sungero.Content.Shared.ElectronicDocumentUtils.FromTemplateIdKey] = template.Id;
                                       document.Save();
                                     }
                                     // В HR св-во HRLifeCycleStateHRPro продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
                                     // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
                                     // Используется в HRLifeCycleStateHRProChanged официального документа.
                                     document.HRLifeCycleStateHRPro = DirRX.OverrideSolution.VacationScheduleDocument.HRLifeCycleStateHRPro.Active;
                                     document.AcquiantanceOptionDirRX = acquiantanceOption;
                                     document.ConsentDirRX = consent;
                                     document.AccessRights.Grant(performer, DefaultAccessRightsTypes.Change);
                                     OverrideSolution.PublicFunctions.VacationScheduleDocument.Remote.CreateFromTemplate(document, logPrefix);
                                     processedEntitesIds.Add(allEntitiesIds[i]);
                                     Logger.DebugFormat("{0}. Ид документа: {1}. Версия документа успешно создана.", logPrefix, allEntitiesIds[i]);
                                   }
                                   catch (Exception ex)
                                   {
                                     Logger.ErrorFormat("{0}. Ид документа: {1}. Ошибка при сохранении индивидуального листа ознакомления: {2}. StackTrace: {3}.",
                                                        logPrefix, document.Id, ex.Message, ex.StackTrace);
                                   }
                                 }
                                 else
                                 {
                                   Logger.ErrorFormat("{0}. Ид документа: {1}. Документ пропущен т.к. не удалось заблокировать документ. Заблокирован: {2}. Время установки блокировки: {3}",
                                                      logPrefix, allEntitiesIds[i], lockInfo.OwnerName, lockInfo.LockTime);
                                 }
                               });
        }
        var remainingIds = allEntitiesIds.Except(processedEntitesIds);
        args.Retry = remainingIds.Any();
        if (args.Retry)
          args.EntitiesIds = string.Join(Constants.Module.ListIdsDelimiter.ToString(), remainingIds);
      }
      else
      {
        var employeesIds = new List<string>();
        var departmentId = task.Department != null ? task.Department.Id : 0;
        var year = task.Year;
        try
        {
          Logger.DebugFormat("{0}. Начало выполнения SQL-запроса получения ид сотрудников.", logPrefix);
          var command = string.Format(Queries.Module.GetEmployeesIdsForCreateIndividualLists, vacationScheduleDocument.Id, docKind.Id, countCreateAcquiantanceListsInIteration);
          var commandExecutionResult = Sungero.Docflow.PublicFunctions.Module.ExecuteScalarSQLCommand(command);
          if (!(commandExecutionResult is DBNull) && commandExecutionResult != null)
          {
            var result = commandExecutionResult.ToString();
            employeesIds = result.Split(Constants.Module.ListIdsDelimiter).ToList();
          }
          Logger.DebugFormat("{0}. Завершено выполнение SQL-запроса получения ид сотрудников. Количество полученных записей: {1}.", logPrefix, employeesIds.Count);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. Асинхронный обработчик остановлен. Ошибка при получении ид сотрудников: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
          return;
        }
        args.Retry = employeesIds.Count == countCreateAcquiantanceListsInIteration;
        foreach (var employeeId in employeesIds)
        {
          Logger.DebugFormat("{0}. Ид сотрудника: {1}. Начало обработки.", logPrefix, employeeId);
          Transactions.Execute(() =>
                               {
                                 var employee = Employees.GetAll(e => e.Id == long.Parse(employeeId)).FirstOrDefault();
                                 try
                                 {
                                   var document = OverrideSolution.VacationScheduleDocuments.CreateFrom(template);
                                   document.DocumentKind = docKind;
                                   document.Department = employee.Department;
                                   document.Year = year;
                                   document.EmployeeDirRX = employee;
                                   document.AcquiantanceOptionDirRX = acquiantanceOption;
                                   document.ConsentDirRX = consent;
                                   document.RegistrationNumber = vacationScheduleDocument.RegistrationNumber;
                                   document.RegistrationDate = vacationScheduleDocument.RegistrationDate;
                                   document.VacationScheduleDocumentDirRX = vacationScheduleDocument;
                                   document.AccessRights.Grant(performer, DefaultAccessRightsTypes.Change);
                                   document.UpdateTemplateParameters();
                                   document.Save();
                                   OverrideSolution.PublicFunctions.VacationScheduleDocument.Remote.CreateFromTemplate(document, logPrefix);
                                   Logger.DebugFormat("{0}. Ид сотрудника: {1}. Документ успешно создан.", logPrefix, employeeId);
                                 }
                                 catch (Exception ex)
                                 {
                                   Logger.ErrorFormat("{0}. Ид сотрудника: {1}. Ошибка при сохранении индивидуального листа ознакомления: {2}. StackTrace: {3}.",
                                                      logPrefix, employeeId, ex.Message, ex.StackTrace);
                                   if (employeesIds.Count < countCreateAcquiantanceListsInIteration)
                                     args.Retry = true;
                                 }
                               });
        }
      }
      if (args.Retry)
      {
        Logger.DebugFormat("{0}. Конец выполнения итерации. Асинхронный обработчик будет запущен повторно", logPrefix);
      }
      else
      {
        Logger.DebugFormat("{0}. Завершение Асинхронного обработчика \"Сформировать индивидуальные листы ознакомления\".", logPrefix);
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.NoticeSubjectCreateAcquiantanceVacScheduleLists, task.BusinessUnit.VacationScheduleResponsibleDirRX);
        notice.ActiveText = Hyperlinks.Get(task);
        notice.Start();
      }
    }
    
    /// <summary>
    /// Сформировать групповые листы ознакомления.
    /// </summary>
    /// <param name="args">AcquiantanceOption - Вариант ознакомления (Бумажный/Электронный).</param>
    /// <param name="args">Consent - Согласие на КЭДО (true/false).</param>
    /// <param name="args">VacationScheduleManagementTaskId - Ид задачи Ведение графика отпусков.</param>
    /// <param name="args">DepartmentsIds - Ид подразделений.</param>
    /// <param name="args">PerformerId - Ид исполнителя задания.</param>
    public virtual void CreateGroupAcquiantanceVacScheduleLists(DirRX.NNVacations.Server.AsyncHandlerInvokeArgs.CreateGroupAcquiantanceVacScheduleListsInvokeArgs args)
    {
      var logPrefix = string.Format("CreateGroupAcquiantanceVacScheduleLists. Ид задачи Ведение графика отпусков: {0}. Вариант ознакомления: {1}. Согласие на КЭДО: {2}",
                                    args.VacationScheduleManagementTaskId, args.AcquiantanceOption, args.Consent);
      Logger.DebugFormat("{0}. Итерация: {1}. Начало выполнения.", logPrefix, args.RetryIteration);
      var acquiantanceOption = DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.InPaper.Value == args.AcquiantanceOption ?
        DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.InPaper :
        DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.Electronic;
      var consent = args.Consent;
      var performer = Users.GetAll(u => u.Id == args.PerformerId).FirstOrDefault();
      var task = VacationScheduleManagementTasks.GetAll(t => t.Id == args.VacationScheduleManagementTaskId).FirstOrDefault();
      if (task == null)
      {
        Logger.ErrorFormat("{0}. Не найдена задача Ведение графика отпусков. Асинхронный обработчик остановлен.", logPrefix);
        return;
      }
      var vacationScheduleDocument = task.DocumentGroup.VacationScheduleDocuments.FirstOrDefault();
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.GroupAcquiantanceVacScheduleListKind);
      
      var errorMessage = string.Empty;
      if (vacationScheduleDocument == null)
        errorMessage += "Не найден документ График отпусков. ";
      if (docKind == null)
        errorMessage += "Не найден вид документа Групповой лист ознакомления с графиком отпусков.";
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.NoticeSubjectCreateAcquiantanceVacScheduleListsError, task.BusinessUnit.VacationScheduleResponsibleDirRX);
        notice.ActiveText = errorMessage;
        notice.Start();
        return;
      }
      
      var year = vacationScheduleDocument.Year.Value.Year;
      var allDepartmentIds = new List<string>();
      var processedDepartmentsIds = new List<string>();
      if (!string.IsNullOrEmpty(args.DepartmentsIds))
      {
        allDepartmentIds = args.DepartmentsIds.Split(Constants.Module.ListIdsDelimiter).ToList();
      }
      else
      {
        var departments = consent ? Functions.VacationScheduleManagementTask.GetExpiredVacationScheduleDocuments(task).Select(d => d.Department) :
          Functions.VacationScheduleManagementTask.GetEmployeesWithoutConsent(task).Select(e => e.Department);
        allDepartmentIds = departments.Distinct().Select(d => d.Id.ToString()).ToList();
      }
      var countCreateAcquiantanceListsInIteration = NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.CountCreateAcquiantanceListsInIteration.Guid) ??
        Constants.Module.ConstantsDefaultValues.CountCreateAcquiantanceListsInIteration;
      var processingCount = countCreateAcquiantanceListsInIteration >= allDepartmentIds.Count ? allDepartmentIds.Count : countCreateAcquiantanceListsInIteration;
      for (int i = 0; i < processingCount; i++)
      {
        Logger.DebugFormat("{0}. Ид подразделения: {1}. Начало обработки.", logPrefix, allDepartmentIds[i]);
        Transactions.Execute(() =>
                             {
                               var department = Departments.GetAll(e => e.Id == long.Parse(allDepartmentIds[i])).FirstOrDefault();
                               if (!VacationScheduleDocuments.GetAll(v => DocumentKinds.Equals(v.DocumentKind, docKind) &&
                                                                     Departments.Equals(v.Department, department) &&
                                                                     v.Year.Value.Year == year &&
                                                                     v.ConsentDirRX == consent &&
                                                                     v.AcquiantanceOptionDirRX == acquiantanceOption &&
                                                                     v.VacationScheduleDocumentDirRX == vacationScheduleDocument).Any())
                               {
                                 try
                                 {
                                   var document = VacationScheduleDocuments.Create();
                                   document.DocumentKind = docKind;
                                   document.AcquiantanceOptionDirRX = acquiantanceOption;
                                   document.ConsentDirRX = consent;
                                   document.Department = department;
                                   document.Year = vacationScheduleDocument.Year;
                                   document.RegistrationNumber = vacationScheduleDocument.RegistrationNumber;
                                   document.RegistrationDate = vacationScheduleDocument.RegistrationDate;
                                   document.ApprovalDateDirRX = vacationScheduleDocument.ApprovalDateDirRX;
                                   document.VacationScheduleDocumentDirRX = vacationScheduleDocument;
                                   document.AccessRights.Grant(performer, DefaultAccessRightsTypes.Change);
                                   
                                   var report = Reports.GetGroupAcquiantanceVacScheduleReport();
                                   report.BusinessUnitId = vacationScheduleDocument.BusinessUnit.Id;
                                   report.DepartmentId = department.Id;
                                   report.Consent = consent;
                                   report.Year = year;
                                   report.ApprovalDate = vacationScheduleDocument.ApprovalDateDirRX;
                                   report.RegDate = vacationScheduleDocument.RegistrationDate;
                                   report.RegNumber = vacationScheduleDocument.RegistrationNumber;
                                   report.VacationScheduleId = vacationScheduleDocument.Id;
                                   Logger.DebugFormat("BusinessUnitId: {0}. DepartmentId: {1}. Consent: {2}. Year: {3}. ApprovalDate: {4}. RegDate: {5}. RegNumber: {6}",
                                                      vacationScheduleDocument.BusinessUnit.Id, department.Id, consent, year, vacationScheduleDocument.ApprovalDateDirRX,
                                                      vacationScheduleDocument.RegistrationDate, vacationScheduleDocument.RegistrationNumber);
                                   
                                   report.ExportTo(document);
                                   document.Save();
                                   processedDepartmentsIds.Add(allDepartmentIds[i]);
                                   Logger.DebugFormat("{0}. Ид подразделения: {1}. Ид документа: {2}. Документ успешно создан.", logPrefix, allDepartmentIds[i], document.Id);
                                 }
                                 catch (Exception ex)
                                 {
                                   Logger.ErrorFormat("{0}. Ид подразделения: {1}. Ошибка при создании группового листа ознакомления: {2}. StackTrace: {3}.",
                                                      logPrefix, department.Id, ex.Message, ex.StackTrace);
                                 }
                               }
                               else
                               {
                                 processedDepartmentsIds.Add(allDepartmentIds[i]);
                                 Logger.DebugFormat("{0}. Ид подразделения: {1}. Подразделение пропущено т.к. по нему уже создан документ.", logPrefix, department.Id);
                               }
                             });
      }
      
      var remainingIds = allDepartmentIds.Except(processedDepartmentsIds);
      args.Retry = remainingIds.Any();
      if (args.Retry)
      {
        Logger.DebugFormat("{0}. Конец выполнения итерации. Асинхронный обработчик будет запущен повторно", logPrefix);
        args.DepartmentsIds = string.Join(Constants.Module.ListIdsDelimiter.ToString(), remainingIds);
      }
      else
      {
        Logger.DebugFormat("{0}. Завершение Асинхронного обработчика \"Сформировать групповые листы ознакомления\".", logPrefix);
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.NoticeSubjectCreateAcquiantanceVacScheduleLists, task.BusinessUnit.VacationScheduleResponsibleDirRX);
        notice.ActiveText = Hyperlinks.Get(task);
        notice.Start();
      }
    }
    
    /// <summary>
    /// Создать и запустить задачи Ознакомление с индивидуальным листом.
    /// </summary>
    /// <param name="args">VacationScheduleManagementTaskId - Ид задачи Ведение графика отпусков.</param>
    /// <param name="args">EmployeesIds - Ид сотрудников.</param>
    public virtual void CreateIndividualAcquiantanceListTasks(DirRX.NNVacations.Server.AsyncHandlerInvokeArgs.CreateIndividualAcquiantanceListTasksInvokeArgs args)
    {
      var logPrefix = string.Format("CreateIndividualAcquiantanceListTasks. Ид задачи Ведение графика отпусков: {0}", args.VacationScheduleManagementTaskId);
      Logger.DebugFormat("{0}. Ид сотрудников: {1}. Итерация: {2}. Начало выполнения.", logPrefix, args.EmployeesIds, args.RetryIteration);
      var task = VacationScheduleManagementTasks.GetAll(t => t.Id == args.VacationScheduleManagementTaskId).FirstOrDefault();
      if (task == null)
      {
        Logger.ErrorFormat("{0}. Не найдена задача Ведение графика отпусков. Асинхронный обработчик остановлен.", logPrefix);
        return;
      }
      var allEmployeesIds = args.EmployeesIds.Split(Constants.Module.ListIdsDelimiter).Select(x => long.Parse(x)).ToList();
      var processedEmployeesIds = new List<long>();
      foreach (var employeeId in allEmployeesIds)
      {
        Logger.DebugFormat("{0}. Ид сотрудника: {1}. Начало обработки.", logPrefix, employeeId);
        Transactions.Execute(() =>
                             {
                               var employee = Employees.GetAll(e => e.Id == employeeId).FirstOrDefault();
                               if (!IndividualVacationScheduleAcquaintanceTasks.GetAll(t => t.VacationScheduleManagementTaskId == task.Id && Employees.Equals(t.Employee, employee)).Any())
                               {
                                 try
                                 {
                                   var acquaintanceTask = IndividualVacationScheduleAcquaintanceTasks.Create();
                                   acquaintanceTask.Subject = Resources.SubjectIndividualVacationScheduleAcquaintanceTask;
                                   acquaintanceTask.VacationScheduleManagementTaskId = task.Id;
                                   acquaintanceTask.AcquaintancePeriod = task.EmployeeAcquaintancePeriod;
                                   acquaintanceTask.DaysToRemind = task.DaysToRemind;
                                   acquaintanceTask.Employee = employee;
                                   acquaintanceTask.Start();
                                   processedEmployeesIds.Add(employeeId);
                                   Logger.DebugFormat("{0}. Ид сотрудника: {1}. Ид задачи Ознакомление с индивидуальным листом: {2}. Задача успешно запущена.", logPrefix, employeeId, acquaintanceTask.Id);
                                 }
                                 catch (Exception ex)
                                 {
                                   Logger.ErrorFormat("{0}. Ид сотрудника: {1}. Ошибка при старте задачи Ознакомление с индивидуальным листом: {2}. StackTrace: {3}.",
                                                      logPrefix, employeeId, ex.Message, ex.StackTrace);
                                 }
                               }
                               else
                               {
                                 processedEmployeesIds.Add(employeeId);
                                 Logger.DebugFormat("{0}. Ид сотрудника: {1}. Сотрудник пропущен т.к. по нему уже запущена задача.", logPrefix, employeeId);
                               }
                             });
      }
      var remainingIds = allEmployeesIds.Except(processedEmployeesIds);
      args.Retry = remainingIds.Any();
      if (args.Retry)
      {
        args.EmployeesIds = string.Join(Constants.Module.ListIdsDelimiter.ToString(), remainingIds);
        Logger.DebugFormat("{0}. Конец выполнения итерации. Асинхронный обработчик будет запущен повторно", logPrefix);
      }
      else
      {
        Logger.DebugFormat("{0}. Ид сотрудников: {1}. Завершение Асинхронного обработчика \"Создать и запустить задачи Ознакомление с индивидуальным листом\".", logPrefix, args.EmployeesIds);
      }
    }
    
    /// <summary>
    /// Создать и запустить задачу Регистрация ежегодного отпуска.
    /// </summary>
    /// <param name="args">IncomingMessageId - ИД записи справочника очереди входящих сообщений интеграции.</param>
    public virtual void CreateVacationNoticesAndStartTasks(DirRX.NNVacations.Server.AsyncHandlerInvokeArgs.CreateVacationNoticesAndStartTasksInvokeArgs args)
    {
      var logPrefix = string.Format("CreateVacationNoticesAndStartTasks. Ид входящего сообщения: {0}", args.IncomingMessageId);
      Logger.DebugFormat("{0}. Итерация: {1}. Начало выполнения.", logPrefix, args.RetryIteration);
      
      var sapLegalEntityCodeDirRX = string.Empty;
      var dateStart = Calendar.Now;
          
      try
      {
        //Загружаем входящее сообщение.
        var incomingMessage = NNIntegration.IncomingMessages.GetAll(m => m.Id == args.IncomingMessageId).FirstOrDefault();
        if (incomingMessage == null)
        {
          Logger.ErrorFormat("{0}. Входящее сообщение не найдено.", logPrefix);
          return;
        }
        
        //Получаем данные по отпускам (десериализуем JSON входящего сообщения).
        var messageText = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
        var vacationsInfo = DirRX.NNIntegration.Structures.Module.VacationsInfo.Create();
        //TODO: DeserializeInStruct имеет проверку: если DateStart и DateEnd не заполнены - запись не прошла десериализацию,
        //в vacationsInfo заполнится ErrorText.
        vacationsInfo = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(messageText, vacationsInfo);
        
        //Если в vacationsInfo заполнено ErrorText. По таким ТН не отправляем статус в sap, только логируем.
        if(!string.IsNullOrEmpty(vacationsInfo.ErrorText))
          Logger.ErrorFormat("{0}. {1}.", logPrefix, vacationsInfo.ErrorText);
        
        var totalEmployeesCount = vacationsInfo.EmployeesVacationsInfo
          .Select(v => v.PersonnelNumber)
          .Distinct()
          .Count();
          
        Logger.DebugFormat("{0}. Итерация: {1}. Всего получено табельных номеров: {2}. Всего отпусков: {3}.", logPrefix, args.RetryIteration, totalEmployeesCount, vacationsInfo.EmployeesVacationsInfo.Count);
        
        #region Проверяем наличие сотрудника по ТН и наличие типа отпуска.
        //Проверяем наличие сотрудника по ТН.
        var errorPersNumVacInfo = vacationsInfo.EmployeesVacationsInfo
          .Where(v => !OverrideSolution.Employees.GetAll(e => e.PersonnelNumber == v.PersonnelNumber).Any())
          .ToList();
        
        if (errorPersNumVacInfo.Any())
        {
          foreach (var errorVacInfo in errorPersNumVacInfo)
            Functions.Module.SendingStatusVacationNoticeToSap(errorVacInfo, Constants.Module.CreateVacationNoticeResult.Error, "Не найден сотрудник по ТН");
          
          //Удаляем ошибочные записи, чтобы не проверять их дальше.
          vacationsInfo.EmployeesVacationsInfo = vacationsInfo.EmployeesVacationsInfo.Except(errorPersNumVacInfo).ToList();
        }
        
        //Проверяем наличие типа отпуска.
        var personnelNumbers = vacationsInfo.EmployeesVacationsInfo.Select(v => v.PersonnelNumber).Distinct().ToList();
        var employeeBusinessUnits = OverrideSolution.Employees.GetAll(e => personnelNumbers.Contains(e.PersonnelNumber) && e.BusinessUnitDirRX != null)
          .Select(e => new NNVacations.Structures.Module.EmployeeBusinessUnitInfo
                  {
                    PersonnelNumber = e.PersonnelNumber,
                    BusinessUnitDirRXId = e.BusinessUnitDirRX.Id
                  })
          .ToList()
          .ToDictionary(e => e.PersonnelNumber, e=> e.BusinessUnitDirRXId);
        
        var errorVacKindVacInfo = vacationsInfo.EmployeesVacationsInfo
          .Where(v => employeeBusinessUnits.ContainsKey(v.PersonnelNumber) &&
                 !OverrideSolution.VacationKinds
                 .GetAll(x => (x.PlannedAbsenceDirRX == v.TypeVacation ||
                               x.PlannedAbsenceShiftDirRX == v.TypeVacation ||
                               x.SAPIdDirRX == v.TypeVacation ||
                               x.ActualAbsenceDirRX.Select(a => a.ActualAbsence).Contains(v.TypeVacation)) &&
                         x.BusinessUnitDirRX != null &&
                         x.BusinessUnitDirRX.Id == employeeBusinessUnits[v.PersonnelNumber])
                 .Any())
          .ToList();
        
        if (errorVacKindVacInfo.Any())
        {
          foreach (var errorVacInfo in errorVacKindVacInfo)
            Functions.Module.SendingStatusVacationNoticeToSap(errorVacInfo, Constants.Module.CreateVacationNoticeResult.Error, "Не найден тип отпуска по ТН");
          
          //Удаляем ошибочные записи, чтобы не обрабатывать их дальше.
          vacationsInfo.EmployeesVacationsInfo = vacationsInfo.EmployeesVacationsInfo.Except(errorVacKindVacInfo).ToList();
        }
        
        //Отправляем результаты проверки на почту (Email приходит не всегда).
        var emails = vacationsInfo.Emails?.Select(x => x.Email).ToList();
        if ((emails != null && emails.Count > 0)
            && (errorPersNumVacInfo.Any() || errorVacKindVacInfo.Any()))
        {
          if (employeeBusinessUnits.Any())
          {
            var employeeBusinessUnit = employeeBusinessUnits.First();
            var businessUnit = OverrideSolution.BusinessUnits.Get(employeeBusinessUnit.Value);
            sapLegalEntityCodeDirRX = businessUnit.SapLegalEntityCodeDirRX;
            
            var employeesVacationsInfo = vacationsInfo.EmployeesVacationsInfo.Where(x => x.PersonnelNumber == employeeBusinessUnit.Key).FirstOrDefault();
            if (employeesVacationsInfo != null)
              dateStart = employeesVacationsInfo.DateStart;
          }
          
          Functions.Module.SendVacationNoticeErrorToEmail(errorPersNumVacInfo, errorVacKindVacInfo, emails, sapLegalEntityCodeDirRX, dateStart);
        }
        #endregion
        
        //Разбиваем валидные отпуска на пачки по 100 ТН.
        var uniqueValidEmployees = vacationsInfo.EmployeesVacationsInfo
          .Select(v => v.PersonnelNumber)
          .Distinct()
          .ToList();
        
        Logger.DebugFormat("{0}. Итерация: {1}. Всего валидных табельных номеров {2}.", logPrefix, args.RetryIteration, uniqueValidEmployees.Count);
   
        for (int batchNumber = 0; batchNumber < uniqueValidEmployees.Count; batchNumber+= NNVacations.Constants.Module.DefaultCountInIterationAO)
        {
          // Берём 100 элементов или меньше (если это последняя пачка).
          int count = Math.Min(NNVacations.Constants.Module.DefaultCountInIterationAO, uniqueValidEmployees.Count - batchNumber);
          List<string> batchPersonalNumbersList = uniqueValidEmployees.GetRange(batchNumber, count);
          
          // Диапазон обрабатываемых ТН сотрудников.
          var firstPersonnelNumber = batchNumber + 1;
          var lastPersonnelNumber = batchNumber + count;
          var batchNumberString = string.Format("{0}-{1}", firstPersonnelNumber, lastPersonnelNumber);
          
          // Создаём и запускаем АО.
          Logger.DebugFormat("{0}. Создан асинхронный обработчик CreateVacationBatchProcessingTasks. Пачка: {1}.", logPrefix, batchNumberString);
          var async = NNVacations.AsyncHandlers.CreateVacationBatchProcessingTasks.Create();
          async.IncomingMessageId = args.IncomingMessageId;
          async.PersonnelNumbers = string.Join(Constants.Module.ListIdsDelimiter.ToString(), batchPersonalNumbersList.ToArray());
          async.BatchNumber = batchNumberString;
          async.ExecuteAsync();
          Logger.DebugFormat("{0}. Старт асинхронного обработчика CreateVacationBatchProcessingTasks. Пачка: {1}.", logPrefix, batchNumberString);
        }
        
        var persNumbers = vacationsInfo.EmployeesVacationsInfo.Select(v => v.PersonnelNumber);
        
        //Собираем данные для текста письма по необработанным сотрудникам.
        var employeeWithBU = Employees.GetAll(e => persNumbers.Contains(e.PersonnelNumber) && e.BusinessUnit != null).FirstOrDefault();
        if (employeeWithBU != null)
        {
          sapLegalEntityCodeDirRX = OverrideSolution.BusinessUnits.As(employeeWithBU.BusinessUnit)?.SapLegalEntityCodeDirRX ?? string.Empty;
          
          var employeesVacationsInfo = vacationsInfo.EmployeesVacationsInfo.Where(x => x.PersonnelNumber == employeeWithBU.PersonnelNumber).FirstOrDefault();
          if (employeesVacationsInfo != null)
            dateStart = employeesVacationsInfo.DateStart;
        }
      }
      catch (Exception ex)
      {
        if (args.RetryIteration < Constants.Module.CreateVacationNoticesAndStartTasksCountIterationError)
        {
          Logger.ErrorFormat("{0}. Ошибка выполнения. Асинхронный обработчик будет запущен повторно. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
          args.Retry = true;
        }
        else
        {
          Logger.ErrorFormat("{0}. Ошибка выполнения. Асинхронный обработчик будет приостановлен. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
          
          NNCommons.PublicFunctions.Module.SendSimpleTask(Roles.Administrators,
                                                          Resources.CreateVacationNoticesAndStartTasksSubject,
                                                          ex.Message);
          args.Retry = false;
        }
      }
      
      if (!args.Retry)
        Logger.DebugFormat("{0}. Завершение асинхронного обработчика.", logPrefix);
    }
    
    /// <summary>
    /// Запустить задачу Регистрация ежегодного отпуска.
    /// </summary>
    /// <param name="args">IncomingMessageId - ИД записи справочника очереди входящих сообщений интеграции, PersonalNumbers - список ТН из очереди, по которым надо запустить задачу.</param>
    public virtual void CreateVacationBatchProcessingTasks(DirRX.NNVacations.Server.AsyncHandlerInvokeArgs.CreateVacationBatchProcessingTasksInvokeArgs args)
    {
      var logPrefix = string.Format("CreateVacationBatchProcessingTasks. Ид входящего сообщения: {0}. Номер пачки: {1}", args.IncomingMessageId, args.BatchNumber);
      Logger.DebugFormat("{0}. Итерация: {1}. Начало выполнения.", logPrefix, args.RetryIteration);
      
      try
      {
        //Загружаем входящее сообщение.
        var incomingMessage = NNIntegration.IncomingMessages.GetAll(m => m.Id == args.IncomingMessageId).FirstOrDefault();
        if (incomingMessage == null)
        {
          Logger.ErrorFormat("{0}. Входящее сообщение не найдено.", logPrefix);
          return;
        }
        
        //Получаем данные по отпускам (десериализуем JSON входящего сообщения).
        var messageText = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
        var vacationsInfo = DirRX.NNIntegration.Structures.Module.VacationsInfo.Create();
        //TODO: DeserializeInStruct имеет проверку: если DateStart и DateEnd не заполнены - запись не прошла десериализацию,
        //в vacationsInfo заполнится ErrorText.
        vacationsInfo = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(messageText, vacationsInfo);
        
        //Парсим табельные номера из args.
        List<string> personnelNumbers = args.PersonnelNumbers
          .Split(new [] { Constants.Module.ListIdsDelimiter.ToString() }, StringSplitOptions.RemoveEmptyEntries)
          .Select(p => p.Trim())
          .ToList();
        
        //Фильтруем записи и группируем по ТН.
        var filteredVacations = vacationsInfo.EmployeesVacationsInfo.Where(v => personnelNumbers.Contains(v.PersonnelNumber));
        
        Logger.DebugFormat("{0}. Итерация: {1}. Список обрабатываемых ТН {2}.", logPrefix, args.RetryIteration, args.PersonnelNumbers);
        
        var groupedVacations = filteredVacations
          .GroupBy(v => v.PersonnelNumber)
          .ToList();
        
        foreach (var vacInfo in groupedVacations)
        {
          var errorVacInfo = Functions.RegistrationOfAnnualLeave.CreateRegistrationOfAnnualLeaveTask(logPrefix, vacInfo.ToList());
          if (errorVacInfo.Any())
            Logger.ErrorFormat("{0}. Табельный номер сотрудника: {1}. Ошибка выполнения. Для сотрудника будет запущена повторная обработка.", logPrefix, errorVacInfo.First().PersonnelNumber);
        }
        
        //Определяем, кто уже обработан, а кто нет.
        var employeePersonnelNumbers = RegistrationOfAnnualLeaves
          .GetAll(x => x.Started != null && x.Started >  incomingMessage.ReceivedDate.Value)
          .Select(t => t.Employee.PersonnelNumber)
          .ToList();
        
        Logger.DebugFormat("{0}. Итерация: {1}. Список обработанных ТН {2}.", logPrefix, args.RetryIteration, string.Join(Constants.Module.ListIdsDelimiter.ToString(), employeePersonnelNumbers.ToArray()));
        
        var unprocessedPersonnelNumbers = filteredVacations
          .Select(v => v.PersonnelNumber)
          .Except(employeePersonnelNumbers)
          .ToList();
        
        Logger.DebugFormat("{0}. Итерация: {1}. Список необработанных ТН {2}.", logPrefix, args.RetryIteration, string.Join(Constants.Module.ListIdsDelimiter.ToString(), unprocessedPersonnelNumbers.ToArray()));
        
        var remainingVacationsInfo = new List<NNIntegration.Structures.Module.IVacationInfoDateTime>();
        
        if (unprocessedPersonnelNumbers.Any())
        {
          var unprocessedVacInformations = filteredVacations
            .Where(v => unprocessedPersonnelNumbers.Contains(v.PersonnelNumber))
            .GroupBy(v => v.PersonnelNumber);
          
          foreach (var vacInfo in unprocessedVacInformations)
          {
            Logger.DebugFormat("{0}. Начало повторной обработки.", logPrefix);
            
            var errorVacInfoRepeat = Functions.RegistrationOfAnnualLeave.CreateRegistrationOfAnnualLeaveTask(logPrefix, vacInfo.ToList());
            if (errorVacInfoRepeat.Any())
              remainingVacationsInfo.AddRange(errorVacInfoRepeat);
            
            Logger.DebugFormat("{0}. Повторная обработка завершена.", logPrefix);
          }
          
          //Уведомить администратора о необработанных записях.
          if (remainingVacationsInfo.Any())
          {
            var text = string.Format(Resources.CreateVacationNoticesAndStartTasksActiveText, string.Join(", ", remainingVacationsInfo.Select(e => e.PersonnelNumber)));
            NNCommons.PublicFunctions.Module.SendSimpleTask(Roles.Administrators,
                                                            Resources.CreateVacationNoticesAndStartTasksSubject,
                                                            text);
          }
        }
        
        //Отправляем статус в SAP о успешной отправке или об ошибке.
        var processedEmployees = filteredVacations.Where(v => !remainingVacationsInfo.Contains(v)).ToList();
        foreach (var vacInfo in processedEmployees)
          Functions.Module.SendingStatusVacationNoticeToSap(vacInfo, Constants.Module.CreateVacationNoticeResult.Successfully, string.Empty);
        foreach (var vacInfo in remainingVacationsInfo)
          Functions.Module.SendingStatusVacationNoticeToSap(vacInfo, Constants.Module.CreateVacationNoticeResult.Error, "Ошибка при отправке уведомления");
        
        Logger.DebugFormat("{0}. Успешно обработанно {1} записей.", logPrefix, processedEmployees.Count);
        Logger.DebugFormat("{0}. Не обработано {1} записей. Ошибки при отправке уведомлений.", logPrefix, remainingVacationsInfo.Count);
      }
      catch (Exception ex)
      {
        
        if (args.RetryIteration < Constants.Module.CreateVacationNoticesAndStartTasksCountIterationError)
        {
          Logger.ErrorFormat("{0}. Ошибка выполнения. Асинхронный обработчик будет запущен повторно. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
          args.Retry = true;
        }
        else
        {
          Logger.ErrorFormat("{0}. Ошибка выполнения. Асинхронный обработчик будет приостановлен. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
          
          NNCommons.PublicFunctions.Module.SendSimpleTask(Roles.Administrators,
                                                          Resources.CreateVacationNoticesAndStartTasksSubject,
                                                          ex.Message);
          args.Retry = false;
        }
      }
      
      if (!args.Retry)
        Logger.DebugFormat("{0}. Завершение асинхронного обработчика.", logPrefix);
    }
  }
}