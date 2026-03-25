using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using DirRX.NNIntegration;
using Sungero.Core;
using DirRX.NNOtherProcesses.NNRetentionStatementDocument;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Workflow;
using DirRX.NNIntegration.Structures.Module;
using DirRX.OverrideSolution;
using HRPro.HRDocFlow.Server;
using Sungero.Workflow.Task;
using Sungero.Domain.Shared;

namespace DirRX.NNOtherProcesses.Server
{
  public partial class ModuleFunctions
  {

    #region Общие функции.
    
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
    /// Создать и отправить подзадачу на подписание документа.
    /// </summary>
    /// <param name="mainTaskID">ID главной задачи.</param>
    /// <param name="employee">Автор задачи.</param>
    /// <param name="document">Документ.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    /// <remarks>Создание и отправка задачи выполняется в транзакции.</remarks>
    private string CreateAndStartNewSignDocumentSubtask(long mainTaskID, OverrideSolution.IEmployee employee, OverrideSolution.IOfficialDocument document)
    {
      var logPrefix = string.Format("CreateAndStartNewSignDocumentSubtask. ИД документа {0}. ИД основной задачи {1}", document.Id, mainTaskID);
      
      var exsitingTaskId = HRPro.HRDocFlow.HRProcessTasks.GetAll()
        .Where(x => x.MainDoc.Id == document.Id && x.Status == Status.InProcess)
        .Select(x => x.Id)
        .FirstOrDefault();
      if (exsitingTaskId > 0)
      {
        Logger.DebugFormat("{0}. Найдена задача на подписание приказа в работе с ИД {1}. Повторная отправка документа на подписание не требуется.", logPrefix, exsitingTaskId);
        return string.Empty;
      }
      
      var executionResult = false;
      var errorMessage = string.Empty;
      executionResult = Transactions.Execute(
        () =>
        {
          var mainTask = Sungero.Workflow.Tasks.GetAll(x => x.Id == mainTaskID).FirstOrDefault();
          if (mainTask == null)
          {
            errorMessage = string.Format("Не найдена ведущая задача по ИД {0}.", mainTaskID);
            return;
          }
          
          try
          {
            var subTask = HRPro.HRDocFlow.HRProcessTasks.CreateAsSubtask(mainTask);
            subTask.Author = employee;
            if (employee.BusinessUnit != null)
              subTask.BusinessUnit = employee.BusinessUnit;
            subTask.NeedApprove = true;
            subTask.NeedAcquaintance = false;
            subTask.DocumentGroup.OfficialDocuments.Add(document);
            
            subTask.Start();

            Logger.DebugFormat("{0}. Отправлена новая задача на подписание документа с ИД: {1}.", logPrefix, subTask.Id);
          }
          catch (Exception e)
          {
            errorMessage = string.Format("Произошла ошибка отправки задачи на подписание приказа. Текст ошибки: {0}.", e.Message);
            Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, errorMessage, e.StackTrace);
            
          }
        });
      
      if (!executionResult || !string.IsNullOrEmpty(errorMessage))
      {
        if (string.IsNullOrEmpty(errorMessage))
        {
          errorMessage = "Произошла ошибка отправки задачи на подписание приказа на командировку. Транзакция не выполнена.";
          Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        }
        return errorMessage;
      }
      else
        return string.Empty;
    }
    
    #endregion
    
    #region Интеграционный сценарий «Участие в благотворительности».

    #region IF-S-10397. Автоматическое создание карточки документа из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Обработка создания или обновления карточки "Заявление на удержание (программа личных пожертвований)".
    /// </summary>
    /// <returns>Результат обработки.</returns>
    [Public]
    public IProcessIncomingMessageResult ProcessingCreateRetentionStatementDocument(long idIncomingMessage)
    {
      var result = ProcessIncomingMessageResult.Create();
      var logPrefix = string.Format("ProcessingCreateRetentionStatementDocument. Ид записи: {0}", idIncomingMessage);
      var errorMessage = string.Empty;
      var retry = false;
      DateTime? processingDate = null;
      var model = GetStructureFromIncomingMessage(idIncomingMessage, out errorMessage, out retry, out processingDate);

      if (!string.IsNullOrEmpty(errorMessage))
      {
        result.ErrorText = errorMessage;
        result.Retry = retry;
        return result;
      }
      
      var employee = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(model.Employee)).FirstOrDefault();

      if (employee == null)
      {
        result.ErrorText = string.Format("Не найден сотрудник с табельным номером: {0}.", model.Employee);
        result.Retry = false;
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }

      var document = DirRX.NNOtherProcesses.NNRetentionStatementDocuments.GetAll(c => c.ExternalId.Equals(model.SAPID)).FirstOrDefault();
      document = document ?? NNRetentionStatementDocuments.Create();
      result = UpdateOrCreateRetentionStatementDocument(model, employee, document, processingDate);
      result.EntityId = document?.Id;

      if (string.IsNullOrEmpty(result.ErrorText))
      {
        var task = NNIntegration.ExternalSystemDataExchangeTasks.Create();
        Logger.DebugFormat("{0}. Создана задача с ИД: {1}.", logPrefix, task.Id);
        var processId = DirRX.NNCommons.PublicFunctions.Module.GetIdentifierValue(DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.SendRetentionStatementStatusProcessKindId.Guid);
        var process = ProcessKinds.GetAllMatches(task).FirstOrDefault(c => c.Id == processId);

        if (process == null)
        {
          result.ErrorText = string.Format("Действующий вариант процесса для типа задачи 'Обмен данными с внешней системой' с ИД: {0} не найден. Проверьте значение в константе \"{1}\"",
                                           processId, DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.SendRetentionStatementStatusProcessKindId.Guid);
          result.Retry = false;
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;
        }

        task.Subject = DirRX.NNOtherProcesses.Resources.SubjectTaskSendRetentionStatementStatusFormat(document.Name);
        task.ProcessKind = process;
        task.Documents.AddNew().Document = document;
        try
        {
          task.Start();
          Logger.DebugFormat("{0}. Отправлена задача с ИД: {1}.", logPrefix, task.Id);
        }
        catch (Exception ex)
        {
          result.ErrorText = string.Format("Не удалось запустить задачу на передачу статуса в АСУП. Текст ошибки: {0} StackTrace: {1}.", ex.Message, ex.StackTrace);
          result.Retry = false;
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;;
        }
      }

      if (string.IsNullOrEmpty(result.ErrorText))
      {
        Logger.DebugFormat("{0}. Документ SAPID: {1} успешно обработан.", logPrefix, model.SAPID);
      }
      return result;
    }

    /// <summary>
    /// Получить струткуру из записи 'Входящие информационные сообщения'.
    /// </summary>
    /// <param name="idIncomingMessage">Ид записи 'Входящие информационные сообщения'.</param>
    /// <param name="errorMessage">Ошибка.</param>
    /// <param name="isRestart">True если нужен перезапуск, иначе false.</param>
    /// <param name="processingDate">Дата последней обработки.</param>
    /// <returns>Структура.</returns>
    private IRetentionStatementDocumentCard GetStructureFromIncomingMessage(long idIncomingMessage, out string errorMessage,
                                                                            out bool isRestart, out DateTime? processingDate)
    {
      isRestart = false;
      errorMessage = string.Empty;
      processingDate = null;
      var logPrefix = string.Format("GetStructureFromIncomingMessage. incomingMessageId = {0}", idIncomingMessage);
      var message = IncomingMessages.GetAll(c => c.Id == idIncomingMessage).FirstOrDefault();

      if (message == null)
      {
        errorMessage = string.Format("Не найдена запись в справочнике 'Входящие информационные сообщения' c ID {0}.", idIncomingMessage);
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return null;
      }
      
      processingDate = message.ProcessingDate;
      var jsonMessage = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(message);

      if (string.IsNullOrEmpty(jsonMessage))
      {
        errorMessage = string.Format("Не удалось получить тело запроса записи из справочника 'Входящие информационные сообщения' c ID {0}.", idIncomingMessage);
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        isRestart = true;
        return null;
      }
      
      IRetentionStatementDocumentCard model = null;
      
      try
      {
        model = DirRX.NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      
      catch (Exception ex)
      {
        isRestart = true;
        errorMessage = "Ошибка десериализации.";
        Logger.ErrorFormat("{0}. {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorMessage, ex.Message, ex.StackTrace);
        return null;
      }

      if (model != null)
        return model;
      
      isRestart = true;

      return null;
    }

    /// <summary>
    /// Создать или обновить документ "Заявление на удержание".
    /// </summary>
    /// <param name="model">Структура с данными.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="document">Документ.</param>
    /// <param name="processingDate">Дата последней обработки.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    [Public]
    public IProcessIncomingMessageResult UpdateOrCreateRetentionStatementDocument(DirRX.NNIntegration.Structures.Module.IRetentionStatementDocumentCard model, DirRX.OverrideSolution.IEmployee employee, INNRetentionStatementDocument document, DateTime? processingDate)
    {
      var result = ProcessIncomingMessageResult.Create();
      
      if (!Locks.TryLock(document))
      {
        var locksInfo = Locks.GetLockInfo(document);
        result.ErrorText = string.Format("ИД документа: {0}. Текст ошибки: Не удалось заблокировать документ. Заблокирован: {1}. Время установки блокировки: {2}.", document.Id, locksInfo.OwnerName, locksInfo.LockTime);
        result.Retry = true;
        Logger.ErrorFormat("UpdateOrCreateRetentionStatementDocument. {0}.", result.ErrorText);
        return result;
      }

      try
      {
        if (document.State.IsInserted || !document.HasVersions || processingDate == null || document.LastVersion.Created > processingDate)
          result.ErrorText = FillDocumentProperties(document, model, employee);

        var lastVersionIsSigned = Signatures.Get(document.LastVersion,
                                                 q => q.Where(x => x.Comment == DirRX.NNOtherProcesses.Resources.ApproveComment &&
                                                              x.SignatureType == SignatureType.Approval)).FirstOrDefault() != null;
        if (!lastVersionIsSigned)
        {
          try
          {
            // HACK отключено событие до подписание, так как подписывает служебный пользователь и не нужно заполнять поля.
            using (EntityEvents.Disable(HRPro.HRProSolution.OfficialDocuments.Info.Events.BeforeSigning))
              Signatures.Approve(document.LastVersion, null, DirRX.NNOtherProcesses.Resources.ApproveComment);
          }
          catch (Exception ex)
          {
            result.ErrorText = string.Format("Произошла ошибка подписания документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", document.Id, ex.Message, ex.StackTrace);
            Logger.ErrorFormat("UpdateOrCreateRetentionStatementDocument. {0}", result.ErrorText);
            return result;
          }
        }
        
        var docCollection = new List<Sungero.Docflow.IOfficialDocument>()
        {
          Sungero.Docflow.OfficialDocuments.As(document)
        };
        
        HRPro.HRDocFlow.PublicFunctions.Module.CreateOrUpdateEmployeeDocumentLink(employee, docCollection, HRPro.HRDocFlow.EmployeeDocumentLink.Status.Active);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", document.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("UpdateOrCreateRetentionStatementDocument. {0}", result.ErrorText);
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }

      return result;
    }
    
    /// <summary>
    /// Заполнить свойства документа.
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="structure">Структура с данными документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    private string FillDocumentProperties(INNRetentionStatementDocument document, IRetentionStatementDocumentCard structure, DirRX.OverrideSolution.IEmployee employee)
    {
      var isInserted = document.State.IsInserted;
      
      if (document.State.IsInserted)
      {
        document.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DirRX.NNOtherProcesses.PublicConstants.Module.DocumentKind.RetentionStatementDocument);
        // В HR св-во HRLifeCycleStateHRPro продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
        // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
        // Используется в HRLifeCycleStateHRProChanged официального документа.
        document.HRLifeCycleStateHRPro = NNOtherProcesses.NNRetentionStatementDocument.HRLifeCycleStateHRPro.Active;
        document.BusinessUnit = employee.BusinessUnit;
        document.Department = employee.Department;
        document.RegistrationDate = Calendar.Today;
        document.InternalApprovalState = InternalApprovalState.Approved;
      }
      
      if (isInserted || !document.ProgramId.Equals(structure.ProgramID))
        document.ProgramId = structure.ProgramID;
      if (isInserted || !document.Program.Equals(structure.ProgramName))
        document.Program = structure.ProgramName;
      if (isInserted || !Employees.Equals(document.EmployeeDirRX, employee))
        document.EmployeeDirRX = employee;
      if (isInserted || document.PreparationDate != structure.Date)
        document.PreparationDate = structure.Date;
      if (isInserted || document.Login.Equals(structure.Login))
        document.Login = structure.Login;
      if (isInserted || document.SignDate != structure.DateSigned)
        document.SignDate = structure.DateSigned;
      if (isInserted || !document.ExternalId.Equals(structure.SAPID))
        document.ExternalId = structure.SAPID;

      var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.RetentionStatementDocumentExtension.Guid);

      if (string.IsNullOrEmpty(extension))
        return "Прикладная константа 'Интеграция. Формат файла для документа \"Заявление на удержание\", принимаемый из АСУП' не заполнена.";
      
      // Документ сохраняется во время создания новой версии.
      var error = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, structure.Body, extension);

      return !string.IsNullOrEmpty(error) ? error : string.Empty;
    }

    #endregion
    
    #region IF-S-10398. Автоматическое формирование задания на ручное разблокирование ИТ0015 в АСУП (АСУП -> КЭДО).

    /// <summary>
    /// Процесс создания задания на ручное проведение мероприятия.
    /// </summary>
    /// <param name="incomingMessageId">ИД записи входящих сообщений.</param>
    /// <returns>Структура ответа.</returns>
    [Public]
    public IProcessIncomingMessageResult ProcessManualHoldingEvent(long incomingMessageId)
    {
      var logPrefix = string.Format("ProcessManualHoldingEvent. Ид входящей записи: {0}", incomingMessageId);
      var result = ProcessIncomingMessageResult.Create();
      result.Retry = false;
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == incomingMessageId).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.ErrorText = string.Format("Не найдена запись справочника 'Входящие интеграционные сообщения'.");
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      IManualHoldingEventStruct structure = null;
      
      try
      {
        structure = DirRX.NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Ошибка десериализации строки в структуру.";
        Logger.ErrorFormat("{0}. {1} Текст ошибки: {2} StackTrace: {3}.", logPrefix, result.ErrorText, ex.Message, ex.StackTrace);
        return result;
      }
      
      var doc = DirRX.NNOtherProcesses.NNRetentionStatementDocuments.GetAll(c => c.ExternalId.Equals(structure.SAPID)).FirstOrDefault();

      if (doc == null)
      {
        result.ErrorText = string.Format("Не удалось найти документ с SAPID: {0}.", structure.SAPID);
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }

      result.ErrorText = NNOtherProcesses.PublicFunctions.NNRetentionStatementDocument.CreateManualHoldingEvent(doc);
      result.EntityId = doc.Id;
      return result;
    }

    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий «Оформление командировки».
    
    #region IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП.

    /// <summary>
    /// Обработка создания приказа на командировку.
    /// </summary>
    /// <param name="incomingMessageId">ИД записи справочника 'Входящие информационные сообщения'.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public IProcessIncomingMessageResult ProcessCreateBusinessTripOrder(long incomingMessageId)
    {
      var logPrefix = string.Format("ProcessCreateBusinessTripOrder. Ид записи справочника 'Входящие информационные сообщения': {0}", incomingMessageId);
      var result = ProcessIncomingMessageResult.Create();
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == incomingMessageId).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.ErrorText = "Не найдена запись в справочнике 'Входящие информационные сообщения'.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      ICreateBusinessTripOrder structure = null;
      
      try
      {
        structure = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Ошибка десериализации. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, result.ErrorText,  ex.StackTrace);
        return result;
      }
      
      var employee = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Employee)).FirstOrDefault();
      var ourSignatory = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Supervisor)).FirstOrDefault();
      var employeeWhoPrepared = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Prepared)).FirstOrDefault();

      if (employee == null || ourSignatory == null || employeeWhoPrepared == null)
      {
        if (employee == null)
          result.ErrorText += string.Format("Не найден сотрудник с табельным номером: {0}. ", structure.Employee);
        if (ourSignatory == null)
          result.ErrorText += string.Format("Не найден подписывающий с табельным номером: {0}. ", structure.Supervisor);
        if (employeeWhoPrepared == null)
          result.ErrorText += string.Format("Не найден сотрудник подготовивший документ с табельным номером: {0}. ", structure.Prepared);

        result.ErrorText = result.ErrorText.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var document = DirRX.NNOtherProcesses.NNBusinessTripOrderDocuments.GetAll(c => c.ExternalId.Equals(structure.SAPID)).FirstOrDefault();
      result.EntityId = document?.Id;
      
      if (document != null)
      {
        var checkResult = OverrideSolution.PublicFunctions.DocRelatedToWorkBase.CanUpdateDocFromASUP(document);
        if (!string.IsNullOrEmpty(checkResult))
        {
          result.ErrorText = string.Format("Версия документа с ID: {0} не создана. {1}.", document.Id, checkResult);
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;
        }
      }
      
      document = document ?? NNBusinessTripOrderDocuments.Create();
      result = UpdateOrCreateBusinessTripOrder(document, structure, employee, ourSignatory, employeeWhoPrepared);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return result;

      result.ErrorText = SendTaskToSignBusinessTripOrder(document, employeeWhoPrepared);
      
      if (string.IsNullOrEmpty(result.ErrorText))
        Logger.DebugFormat("{0}. Успешно обработан.", logPrefix);
      
      result.EntityId = document?.Id;
      return result;
    }
    
    /// <summary>
    /// Создать или обновить документ "Приказ на командировку".
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="structure">Структура с данными документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="ourSignatory">Подписывающий сотрудник.</param>
    /// <param name="preparedBy">Сотрудник подготовивший документ.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    private IProcessIncomingMessageResult UpdateOrCreateBusinessTripOrder(INNBusinessTripOrderDocument document, ICreateBusinessTripOrder structure, DirRX.OverrideSolution.IEmployee employee,
                                                                          DirRX.OverrideSolution.IEmployee ourSignatory, DirRX.OverrideSolution.IEmployee preparedBy)
    {
      var result = ProcessIncomingMessageResult.Create();
      
      if (!Locks.TryLock(document))
      {
        var locksInfo = Locks.GetLockInfo(document);
        result.ErrorText = string.Format("ИД документа: {0}. Текст ошибки: Не удалось заблокировать документ. Заблокирован: {1}. Время установки блокировки: {2}.", document.Id, locksInfo.OwnerName, locksInfo.LockTime);
        result.Retry = true;
        Logger.ErrorFormat("UpdateOrCreateBusinessTripOrder. {0}", result.ErrorText);
        return result;
      }
      
      try
      {
        result.ErrorText = FillDocumentProperties(document, structure, employee, ourSignatory, preparedBy);
        
        if (!string.IsNullOrEmpty(result.ErrorText))
          return result;
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }
      
      return result;
    }

    /// <summary>
    /// Заполнить свойства документа.
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="structure">Структура с данными документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="ourSignatory">Подписывающий сотрудник.</param>
    /// <param name="preparedBy">Сотрудник подготовивший документ.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    private string FillDocumentProperties(INNBusinessTripOrderDocument document, ICreateBusinessTripOrder structure, DirRX.OverrideSolution.IEmployee employee,
                                          DirRX.OverrideSolution.IEmployee ourSignatory, DirRX.OverrideSolution.IEmployee preparedBy)
    {
      var isInserted = document.State.IsInserted;
      
      if (document.State.IsInserted)
      {
        var documentKind = GetDocumentKindForSignDocumentTask(structure.Kind);

        if (documentKind == null)
          return string.Format("Не удалось найти вид документа для кода вида: {0}", structure.Kind);
        
        document.DocumentKind = documentKind;
        // В HR св-во HRLifeCycleStateHRPro продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
        // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
        // Используется в HRLifeCycleStateHRProChanged официального документа.
        document.HRLifeCycleStateHRPro = NNOtherProcesses.NNBusinessTripOrderDocument.HRLifeCycleStateHRPro.Draft;
        document.BusinessUnit = employee.BusinessUnit;
        document.Department = employee.Department;
      }
      
      if (isInserted || !document.RegistrationNumber.Equals(structure.RegNumber))
        document.RegistrationNumber = structure.RegNumber;
      if (isInserted || document.RegistrationDate != structure.Date)
        document.RegistrationDate = structure.Date;
      if (isInserted || !document.SAPApplicationNumberDirRX.Equals(structure.Request))
        document.SAPApplicationNumberDirRX = structure.Request;
      if (isInserted || !Employees.Equals(document.EmployeeDirRX, employee))
        document.EmployeeDirRX = employee;
      if (isInserted || !Employees.Equals(document.PreparedBy, preparedBy))
        document.PreparedBy = preparedBy;
      if (isInserted || !document.PlaceResidence.Equals(structure.Destination))
        document.PlaceResidence = structure.Destination;
      if (isInserted || document.CalendarDays != structure.Days)
        document.CalendarDays = structure.Days;
      if (isInserted || document.StartDate != structure.DateStart)
        document.StartDate = structure.DateStart;
      if (isInserted || document.EndDate != structure.DateEnd)
        document.EndDate = structure.DateEnd;
      if (isInserted || !document.ExternalId.Equals(structure.SAPID))
        document.ExternalId = structure.SAPID;

      var cancelBusinessKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.CancelBusinessTripOrder);
      var changeBusinessKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.ChangeBusinessTripOrder);
      
      if (document.DocumentKind.Equals(changeBusinessKind) || document.DocumentKind.Equals(cancelBusinessKind))
      {
        var mainOrder = NNBusinessTripOrderDocuments.GetAll(c => c.RegistrationNumber.Equals(structure.MainOrderRegNumber) && c.RegistrationDate == structure.MainOrderDate).FirstOrDefault();

        if (mainOrder != null && (isInserted || document.FondationDocument == null || !document.FondationDocument.Equals(mainOrder)))
          document.FondationDocument = mainOrder;
      }
      
      var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.BusinessTripOrderExtension.Guid);

      if (string.IsNullOrEmpty(extension))
        return "Прикладная константа 'Интеграция. Формат файла для документа \"Приказ на командировку\", принимаемый из АСУП' не заполнена.";
      
      // Документ сохраняется во время создания новой версии.
      var error = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, structure.Body, extension);

      if (!string.IsNullOrEmpty(error))
        return error;

      // HACK Подписывающий может задаваться только после сохранения документа. Иначе на проверки права подписи падает в ошибку.
      document.OurSignatory = ourSignatory;
      document.Save();
      
      return string.Empty;
    }

    /// <summary>
    /// Отправить задачу для подписания приказа на командировку.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="author">Автор.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    /// <remarks>Создание и отправка задачи выполняется в транзакции.</remarks>
    private string SendTaskToSignBusinessTripOrder(INNBusinessTripOrderDocument document, IEmployee author)
    {
      var logPrefix = string.Format("SendTaskToSignBusinessTripOrder. Документ ИД: {0}", document.Id);
      
      var isStarted = HRPro.HRDocFlow.HRProcessTasks.GetAll(c => c.MainDoc.Id == document.Id && c.Status == Status.InProcess).Any();
      if (isStarted)
      {
        Logger.DebugFormat("{0}. Задача на подписание приказа была запущена ранее. Повторная отправка не требуется.", logPrefix);
        return string.Empty;
      }
      
      var needApprove = DirRX.NNCommons.PublicFunctions.Module.Remote.GetBoolValue(Constants.Module.ConstantsSetting.BusinessTripOrderNeedApprove.Guid);
      if (needApprove == null)
        return "Не заполнена прикладная константа 'Интеграция. Приказы на командировку. Значение свойства \"Требуется подписание\" в задаче на обработку документов, связанных с работой, при отправке документов из АСУП'.";
      
      var needAcquaintance = DirRX.NNCommons.PublicFunctions.Module.Remote.GetBoolValue(Constants.Module.ConstantsSetting.BusinessTripOrderNeedAcquaintance.Guid);
      if (needAcquaintance == null)
        return "Не заполнена прикладная константа 'Интеграция. Приказы на командировку. Значение свойства \"Требуется ознакомление сотрудников\" в задаче на обработку документов, связанных с работой, при отправке документов из АСУП'.";
      
      var executionResult = false;
      var errorMessage = string.Empty;
      executionResult = Transactions.Execute(
        () =>
        {
          try
          {
            var task = HRPro.HRDocFlow.PublicFunctions.Module.Remote.CreateNewSignDocumentTask(author, needApprove.Value, needAcquaintance.Value);
            task.DocumentGroup.OfficialDocuments.Add(document);
            
            var processKind = Sungero.Workflow.ProcessKinds.GetAllMatches(task).FirstOrDefault();
            if (processKind == null)
            {
              errorMessage = "Не удалось найти подходящий вариант процесса для отправки задачи на подписание приказа на командировку.";
              return;
            }
            task.Start();
          }
          catch (Exception e)
          {
            errorMessage = string.Format("Произошла ошибка отправки задачи на подписание приказа на командировку. Текст ошибки: {0}", e.Message);
            Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, errorMessage, e.StackTrace);
          }
        });

      if (!executionResult || !string.IsNullOrEmpty(errorMessage))
      {
        if (string.IsNullOrEmpty(errorMessage))
        {
          errorMessage = "Произошла ошибка отправки задачи на подписание приказа на командировку. Транзакция не выполнена.";
          Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        }
        return errorMessage;
      }
      else
        return string.Empty;
    }
    
    /// <summary>
    /// Получить вид документа по коду из АСУП.
    /// </summary>
    /// <param name="kindCode">Код вида документа из АСУП.</param>
    /// <returns>Вид документа.</returns>
    private Sungero.Docflow.IDocumentKind GetDocumentKindForSignDocumentTask(string kindCode)
    {
      var businessTripOrder = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.BusinessTripOrder.Guid);
      var changeBusinessTripOrder = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.ChangeBusinessTripOrder.Guid);
      var cancelBusinessTripOrder = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.CancelBusinessTripOrder.Guid);
      var kindGuid = Guid.Empty;

      if (businessTripOrder.Contains(kindCode))
        kindGuid = Constants.Module.DocumentKind.BusinessTripOrder;
      else if (changeBusinessTripOrder.Contains(kindCode))
        kindGuid = Constants.Module.DocumentKind.ChangeBusinessTripOrder;
      else if (cancelBusinessTripOrder.Contains(kindCode))
        kindGuid = Constants.Module.DocumentKind.CancelBusinessTripOrder;

      return Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(kindGuid);
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий «Регистрация Работы ВД/ПД/СР».
    
    #region IF-S-10413. Передача приказа на выход сотрудника в выходной и/или праздничный день из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Обработка создания приказа на выход сотрудника в выходной и/или праздничный день.
    /// </summary>
    /// <param name="incomingMessageId">ИД записи справочника 'Входящие информационные сообщения'.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public IProcessIncomingMessageResult CreateWeekendWorkOrderDocumentHandler(long incomingMessageId)
    {
      var logPrefix = string.Format("ProcessCreateWeekendWorkOrderDocument. Ид записи справочника 'Входящие информационные сообщения': {0}", incomingMessageId);
      var result = ProcessIncomingMessageResult.Create();
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == incomingMessageId).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.ErrorText = "Не найдена запись в справочнике 'Входящие информационные сообщения'.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      ICreateWeekendWorkOrderStructure structure = null;
      
      try
      {
        structure = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Ошибка десериализации. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, result.ErrorText,  ex.StackTrace);
        return result;
      }
      
      var ourSignatory = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Signed)).FirstOrDefault();
      var employeeWhoPrepared = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Prepared)).FirstOrDefault();

      if (ourSignatory == null || employeeWhoPrepared == null)
      {
        if (ourSignatory == null)
          result.ErrorText += string.Format("Не найден подписывающий с табельным номером: {0}. ", structure.Signed);
        if (employeeWhoPrepared == null)
          result.ErrorText += string.Format("Не найден сотрудник подготовивший документ с табельным номером: {0}. ", structure.Prepared);

        result.ErrorText = result.ErrorText.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var cancelationTask = WeekendWorkCancelationTasks.Null;
      var canceledOrder = WeekendWorkOrderDocuments.Null;
      var task = WeekendWorkTasks.Null;
      var statement = WeekendWorkStatementDocuments.Null;
      
      // Если вид документа - отмена приказа ВД/ПД/СР.
      if (structure.Kind == NNIntegration.PublicConstants.Module.WeekendWorkOrderKindCodes.Cancelation)
      {
        cancelationTask = WeekendWorkCancelationTasks.GetAll(x => x.Id == structure.TaskID).FirstOrDefault();
        if (cancelationTask == null)
        {
          result.ErrorText = string.Format("Не найдена задача для отмены приказа ВД/ПД/СР по Ид: {0}", structure.TaskID);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          return result;
        }
        
        canceledOrder = cancelationTask.WeekendWorkOrder;
        if (canceledOrder == null)
        {
          result.ErrorText = string.Format("Отсутствует ссылка на приказ в задаче на отмену приказа ВД/ПД/СР. Ид задачи: {0}", structure.TaskID);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          return result;
        }
        
        statement = canceledOrder.WeekendWorkStatement;
      }
      else
      {
        task = WeekendWorkTasks.GetAll(x => x.Id == structure.TaskID).FirstOrDefault();
        if (task == null)
        {
          result.ErrorText = string.Format("Не найдена задача на согласование заявки ВД/ПД/СР по Ид: {0}", structure.TaskID);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          return result;
        }
        statement = WeekendWorkStatementDocuments.As(task.DocumentGroup.OfficialDocuments.FirstOrDefault());
      }
      
      if (statement == null)
      {
        result.ErrorText = string.Format("Отсутствует ссылка на заявку во вложениях задачи или в приказе ВД/ПД/СР");
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var order = WeekendWorkOrderDocuments.GetAll(x => x.ExternalId == structure.SAPID).FirstOrDefault();
      result.EntityId = order?.Id;
      
      if (order != null)
      {
        var checkResult = OverrideSolution.PublicFunctions.DocRelatedToWorkBase.CanUpdateDocFromASUP(order);
        if (!string.IsNullOrEmpty(checkResult))
        {
          result.ErrorText = string.Format("Версия документа с ID: {0} не создана. {1}.", order.Id, checkResult);
          Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
          return result;
        }
      }
      
      order = order ?? WeekendWorkOrderDocuments.Create();
      
      // Блокировка необходима для создания тела документа.
      if (!Locks.GetLockInfo(order).IsLockedByMe && !Locks.TryLock(order))
      {
        var locksInfo = Locks.GetLockInfo(order);
        result.ErrorText = string.Format("ИД документа: {0}. Текст ошибки: Не удалось заблокировать документ. Заблокирован: {1}. Время установки блокировки: {2}.", order.Id, locksInfo.OwnerName, locksInfo.LockTime);
        result.Retry = true;
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      try
      {
        var errorMessage = Functions.WeekendWorkOrderDocument.FillWeekendWorkOrderProperties(order, structure, statement, canceledOrder, ourSignatory, employeeWhoPrepared);
        if (!string.IsNullOrWhiteSpace(errorMessage))
          throw AppliedCodeException.Create(errorMessage);
        
        Logger.DebugFormat("{0} Документ с ИД: {1} был сохранен.", logPrefix, order.Id);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", order.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      finally
      {
        if (Locks.GetLockInfo(order).IsLockedByMe)
          Locks.Unlock(order);
      }
      result.EntityId = order?.Id;
      
      result.ErrorText = this.CreateAndStartNewSignDocumentSubtask(structure.TaskID, employeeWhoPrepared, order);
      if (string.IsNullOrEmpty(result.ErrorText))
        Logger.DebugFormat("{0}. Успешно обработан.", logPrefix);
      
      return result;
    }
    
    /// <summary>
    /// Получить вид документа по коду из АСУП.
    /// </summary>
    /// <param name="kindCode">Код вида документа из АСУП.</param>
    /// <returns>Вид документа.</returns>
    public Sungero.Docflow.IDocumentKind GetDocumentKindForWeekendWorkOrder(string kindCode)
    {
      var kindGuid = Guid.Empty;
      if (string.IsNullOrEmpty(kindCode))
      {
        Logger.ErrorFormat("GetDocumentKindForWeekendWorkOrder. Код вида документа не может быть null или пустым.");
        return Sungero.Docflow.DocumentKinds.Null;
      }
      
      if (kindCode == NNIntegration.PublicConstants.Module.WeekendWorkOrderKindCodes.Cancelation)
        kindGuid = Constants.Module.DocumentKind.WeekendWorkCancelationOrderDocument;
      else if (kindCode == NNIntegration.PublicConstants.Module.WeekendWorkOrderKindCodes.WeekendWork)
        kindGuid = Constants.Module.DocumentKind.WeekendWorkOrderDocument;
      else if (kindCode == NNIntegration.PublicConstants.Module.WeekendWorkOrderKindCodes.OvertimeWork)
        kindGuid = Constants.Module.DocumentKind.OvertimeWorkOrderDocument;
      else
        Logger.ErrorFormat("GetDocumentKindForWeekendWorkOrder. Не удалось определить вид документа ВД/ПД/СР по его коду. Код: {0}.", kindCode);

      return kindGuid == Guid.Empty ? Sungero.Docflow.DocumentKinds.Null : Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(kindGuid);
    }
    
    #endregion
    
    #region IF-S-10412. Автоматическая проверка заявления на работу в выходной и/или праздничный день (АСУП -> КЭДО).
    
    /// <summary>
    /// Проверить заявление ВД/ПД/СР во внешней системе (базовая перегрузка).
    /// </summary>
    /// <param name="request">Запрос.</param>
    /// <param name="statementId">ИД заявления.</param>
    /// <returns>Ответ.</returns>
    /// <remarks>Если заявление ещё не создано в RX, в параметре ИД заявление передаётся 0.</remarks>
    public IValidateWeekendWorkStatementResponse ValidateWeekendWorkStatementInExternalSystem(NNIntegration.Structures.Module.IValidateWeekendWorkStatementRequest request, long statementId)
    {
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.ValidateWeekendWorkStatement;
      var logPrefix = string.Format("ValidateWeekendWorkStatementInExternalSystem. Гуид потока: {0}. ИД заявления: {1}", flowGuid, statementId);
      Logger.DebugFormat("{0}. Старт синхронной отправки сообщения во внешнюю систему.", logPrefix);
      
      var errorMessage = string.Empty;
      var requestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(request);
      if (!string.IsNullOrEmpty(errorMessage))
        Logger.ErrorFormat("{0}. При формировании текста запроса для проверки заявления ВД/ПД/СР во внешней системе возникла ошибка: {1}.", logPrefix, errorMessage);
      
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(requestText, flowGuid, statementId);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultSendingMessage.ErrorText);
        return null;
      }
      
      var response = NNIntegration.Structures.Module.ValidateWeekendWorkStatementResponse.Create();
      try
      {
        response = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(resultSendingMessage.AnswerText, response);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        return null;
      }
      
      Logger.DebugFormat("{0}. Конец синхронной отправки сообщения.", logPrefix);
      return response;
    }
    
    /// <summary>
    /// Проверить заявление ВД/ПД/СР во внешней системе (перегрузка для RX).
    /// </summary>
    /// <param name="statement">Заявление ВД/ПД/СР.</param>
    /// <returns>Ответ.</returns>
    public IValidateWeekendWorkStatementResponse ValidateWeekendWorkStatementInExternalSystem(IWeekendWorkStatementDocument statement)
    {
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWorkDocKind(statement);
      if (isOvertimeWork == null)
        isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWork(statement.DocumentKind);
      
      var kindCode = isOvertimeWork.Value ? NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.OvertimeWork :
        NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.WeekendWork;
      
      // Информация о сотрудниках.
      var employeeInfoList = new List<NNIntegration.Structures.Module.IValidateWeekendWorkStatementEmployeeInfoRequest>();
      foreach (var employeeInfo in statement.Employees)
      {
        var employeeInfoRequest = NNIntegration.Structures.Module.ValidateWeekendWorkStatementEmployeeInfoRequest.Create();
        employeeInfoRequest.Employee = employeeInfo.Employee.PersonnelNumber;
        employeeInfoRequest.DateStart = employeeInfo.WorkDateFrom.Value.ToString("dd.MM.yyyy");
        employeeInfoRequest.TimeStart = employeeInfo.WorkDateFrom.Value.ToString("HH:mm");
        employeeInfoRequest.DateEnd = employeeInfo.WorkDateTo.Value.ToString("dd.MM.yyyy");
        employeeInfoRequest.TimeEnd = employeeInfo.WorkDateTo.Value.ToString("HH:mm");
        employeeInfoRequest.DateWeekendStart = employeeInfo.RestDateFrom.HasValue ? employeeInfo.RestDateFrom.Value.ToString("dd.MM.yyyy") : string.Empty;
        employeeInfoRequest.DateWeekendEnd = employeeInfo.RestDateTo.HasValue ? employeeInfo.RestDateTo.Value.ToString("dd.MM.yyyy") : string.Empty;
        employeeInfoRequest.TimeWeekendStart = employeeInfo.RestDateFrom.HasValue ? employeeInfo.RestDateFrom.Value.ToString("HH:mm") : string.Empty;
        employeeInfoRequest.TimeWeekendEnd = employeeInfo.RestDateTo.HasValue ? employeeInfo.RestDateTo.Value.ToString("HH:mm") : string.Empty;
        employeeInfoList.Add(employeeInfoRequest);
      }
      
      var request = NNIntegration.Structures.Module.ValidateWeekendWorkStatementRequest.Create(
        Guid.NewGuid().ToString(),
        kindCode,
        statement.IsGroup.HasValue && statement.IsGroup.Value,
        employeeInfoList
       );
      
      var response = ValidateWeekendWorkStatementInExternalSystem(request, statement.Id);
      if (response == null || !response.Employees.Any())
        Logger.DebugFormat("ValidateWeekendWorkStatementInExternalSystem. {0} Пустой ответ или нет информации по сотрудникам: {1}.",
                           DirRX.NNOtherProcesses.Resources.ExternalSystemValidationWeekendWorkError, response == null || !response.Employees.Any());
      
      return response;
    }
    
    
    /// <summary>
    /// Проверить заявление ВД/ПД/СР во внешней системе (перегрузка для ЛК).
    /// </summary>
    /// <param name="DocKindGuid">Гуид вида документа.</param>
    /// <param name="EmployeeId">ИД сотрудника.</param>
    /// <param name="WorkDateFrom">Дата работы с.</param>
    /// <param name="WorkDateTo">Дата работы по.<</param>
    /// <param name="RestDateFrom">Дата доп. отдыха с.</param>
    /// <param name="RestDateTo">Дата доп. отдыха по.</param>
    /// <returns>Ошибка. Если ошибки нет - возвращается пустая строка.</returns>
    /// <remarks>Из ЛК заявление всегда для одного сотрудника.</remarks>
    [Public(WebApiRequestType = RequestType.Get)]
    public string ValidateWeekendWorkStatementInExternalSystem(string DocKindGuid, long EmployeeId, string WorkDateFrom, string WorkDateTo,
                                                               string RestDateFrom, string RestDateTo)
    {
      var request = NNIntegration.Structures.Module.ValidateWeekendWorkStatementRequest.Create();
      var docKinId = NNCommons.PublicFunctions.Module.GetDocumentKindId(DocKindGuid);
      var documentKind = OverrideSolution.DocumentKinds.Get(docKinId.Value);
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWork(documentKind);
      var kindCode = isOvertimeWork ? NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.OvertimeWork :
        NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.WeekendWork;
      var employee = OverrideSolution.Employees.Get(EmployeeId);
      
      request.RequestCode = Guid.NewGuid().ToString();
      request.Kind = kindCode;
      request.IsGroup = false;
      request.Employees = new List<NNIntegration.Structures.Module.IValidateWeekendWorkStatementEmployeeInfoRequest>();
      
      var workDateFrom = NNIntegration.PublicFunctions.Module.ParseStringToDate(WorkDateFrom).Value;
      var workDateTo = NNIntegration.PublicFunctions.Module.ParseStringToDate(WorkDateTo).Value;
      
      // Информация о сотруднике.
      var employeeInfoRequest = NNIntegration.Structures.Module.ValidateWeekendWorkStatementEmployeeInfoRequest.Create();
      employeeInfoRequest.Employee = employee.PersonnelNumber;
      employeeInfoRequest.DateStart = workDateFrom.ToString("dd.MM.yyyy");
      employeeInfoRequest.TimeStart = workDateFrom.ToString("HH:mm");
      employeeInfoRequest.DateEnd = workDateTo.ToString("dd.MM.yyyy");
      employeeInfoRequest.TimeEnd = workDateTo.ToString("HH:mm");
      employeeInfoRequest.DateWeekendStart = !string.IsNullOrEmpty(RestDateFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(RestDateFrom).Value.ToString("dd.MM.yyyy") : string.Empty;
      employeeInfoRequest.DateWeekendEnd = !string.IsNullOrEmpty(RestDateTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(RestDateTo).Value.ToString("dd.MM.yyyy") : string.Empty;
      employeeInfoRequest.TimeWeekendStart = !string.IsNullOrEmpty(RestDateFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(RestDateFrom).Value.ToString("HH:mm") : string.Empty;
      employeeInfoRequest.TimeWeekendEnd = !string.IsNullOrEmpty(RestDateTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(RestDateTo).Value.ToString("HH:mm") : string.Empty;
      request.Employees.Add(employeeInfoRequest);
      
      var response = ValidateWeekendWorkStatementInExternalSystem(request, 0);
      if (response == null || !response.Employees.Any())
      {
        Logger.DebugFormat("ValidateWeekendWorkStatementInExternalSystem. {0} Пустой ответ или нет информации по сотрудникам: {1}.",
                           DirRX.NNOtherProcesses.Resources.ExternalSystemValidationWeekendWorkError, response == null || !response.Employees.Any());
        return DirRX.NNOtherProcesses.Resources.ExternalSystemValidationWeekendWorkError;
      }
      
      return response.Employees.FirstOrDefault().ErrorText;
    }
    
    #endregion
    
    #region IF-S-6363. Автоматическое создание мероприятия «Регистрация работы в выходной и/или праздничный день» из КЭДО в АСУП (КЭДО -> АСУП)
    
    /// <summary>
    /// Процесс создания мероприятия «Регистрация работы в выходной и/или праздничный день».
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="messageGuid">Гуид сообщения.</param>
    /// <returns>Ответ.</returns>
    [Public]
    public IResultGenerateRequestText CreateWeekendWorkEvent(long mainTaskId, string messageGuid)
    {
      var logPrefix = string.Format("CreateWeekendWorkEvent. ИД основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = WeekendWorkStatementDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      logPrefix = string.Format("{0}. ИД документа: {1}", logPrefix, doc.Id);
      
      var agreedEmployees = doc.Employees.Where(e => e.RejectedByApproval != true);
      if (!agreedEmployees.Any())
      {
        result.ErrorText = "В документе не найдено сотрудников, по которым согласован выход";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var errorMessage = string.Empty;
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWork(doc.DocumentKind);
      var kindCode = isOvertimeWork ? NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.OvertimeWork :
        NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.WeekendWork;
      var isGroup = doc.IsGroup == true;
      var reason = doc.Reason != null ? doc.Reason.Code : string.Empty;
      var employeesInfoList = new List<NNIntegration.Structures.Module.ICreateWeekendWorkEventEmployeesInfo>();
      
      var dateFormat = "dd.MM.yyyy";
      var timeFormat = "HH:mm";
      foreach (var employee in agreedEmployees)
      {
        var employeeInfo = NNIntegration.Structures.Module.CreateWeekendWorkEventEmployeesInfo.Create();
        employeeInfo.Employee = employee.PersonnelNumber;
        employeeInfo.DateStart = employee.WorkDateFrom.HasValue? employee.WorkDateFrom.Value.ToString(dateFormat) : string.Empty;
        employeeInfo.DateEnd = employee.WorkDateTo.HasValue? employee.WorkDateTo.Value.ToString(dateFormat) : string.Empty;
        employeeInfo.TimeStart = employee.WorkDateFrom.HasValue? employee.WorkDateFrom.Value.ToString(timeFormat) : string.Empty;
        employeeInfo.TimeEnd = employee.WorkDateTo.HasValue? employee.WorkDateTo.Value.ToString(timeFormat) : string.Empty;
        employeeInfo.DateWeekendStart = employee.RestDateFrom.HasValue ? employee.RestDateFrom.Value.ToString(dateFormat) : string.Empty;
        employeeInfo.DateWeekendEnd = employee.RestDateTo.HasValue ? employee.RestDateTo.Value.ToString(dateFormat) : string.Empty;
        employeeInfo.TimeWeekendStart = employee.RestDateFrom.HasValue ? employee.RestDateFrom.Value.ToString(timeFormat) : string.Empty;
        employeeInfo.TimeWeekendEnd = employee.RestDateTo.HasValue ? employee.RestDateTo.Value.ToString(timeFormat) : string.Empty;
        employeeInfo.FirstBreakStart = employee.FirstBreakFrom.HasValue? employee.FirstBreakFrom.Value.ToString(timeFormat) : string.Empty;
        employeeInfo.FirstBreakEnd = employee.FirstBreakTo.HasValue? employee.FirstBreakTo.Value.ToString(timeFormat) : string.Empty;
        employeeInfo.SecondBreakStart = employee.SecondBreakFrom.HasValue? employee.SecondBreakFrom.Value.ToString(timeFormat) : string.Empty;
        employeeInfo.SecondBreakEnd = employee.SecondBreakTo.HasValue? employee.SecondBreakTo.Value.ToString(timeFormat) : string.Empty;
        employeeInfo.WithoutABreak = string.IsNullOrEmpty(employeeInfo.FirstBreakStart) && string.IsNullOrEmpty(employeeInfo.FirstBreakEnd) &&
          string.IsNullOrEmpty(employeeInfo.SecondBreakStart) && string.IsNullOrEmpty(employeeInfo.SecondBreakEnd);
        employeeInfo.Compensation = employee.Compensation.Code;
        employeesInfoList.Add(employeeInfo);
      }
      
      var model = NNIntegration.Structures.Module.CreateWeekendWorkEvent.Create(messageGuid,
                                                                                kindCode,
                                                                                isGroup,
                                                                                mainTaskId,
                                                                                employeesInfoList,
                                                                                reason);
      
      try
      {
        result.RequestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для проведения мероприятия \"Регистрация работы в выходной и/или праздничный день\".", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для проведения мероприятия \"Регистрация работы в выходной и/или праздничный день\": {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #region IF-S-6362. Автоматическое внесение изменений в действующий приказ из КЭДО в АСУП (КЭДО -> АСУП)
    
    /// <summary>
    ///  Процесс создания мероприятия «Внесение изменений в действующий приказ ВД/ПД/СР».
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="messageGuid">Гуид сообщения.</param>
    /// <returns>Ответ.</returns>
    [Public]
    public IResultGenerateRequestText CreateWeekendWorkCancelationEvent(long mainTaskId, string messageGuid)
    {
      var logPrefix = string.Format("CreateWeekendWorkCancelationEvent. ИД основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var task = WeekendWorkCancelationTasks.GetAll(x => x.Id == mainTaskId).FirstOrDefault();
      if (task == null)
      {
        result.ErrorText = "Задача типа \"Оформление отмены работы в ВД/ПД/СР\" не найдена";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      if (task.WeekendWorkOrder == null)
      {
        result.ErrorText = "В задачу типа \"Оформление отмены работы в ВД/ПД/СР\" не вложен основной приказ.";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      if (task.WeekendWorkOrder.WeekendWorkStatement == null)
      {
        result.ErrorText = "В приказ (распоряжение) о работе в ВД/ПД/СР не вложена заявка.";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var errorMessage = string.Empty;
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWork(task.WeekendWorkOrder.WeekendWorkStatement.DocumentKind);
      var kindCode = isOvertimeWork ? NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.OvertimeWork :
        NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.WeekendWork;
      var employeesInfoList = new List<NNIntegration.Structures.Module.ICreateWeekendWorkCancelationEventEmployeesInfo>();
      
      var dateFormat = "dd.MM.yyyy";
      var cancelsNumber = task.WeekendWorkOrder.RegistrationNumber;
      var cancelsDate = task.WeekendWorkOrder.RegistrationDate.HasValue ?
        task.WeekendWorkOrder.RegistrationDate.Value.ToString(dateFormat) :
        string.Empty;
      
      foreach (var employeeData in task.Employees)
      {
        var employeeInfo = NNIntegration.Structures.Module.CreateWeekendWorkCancelationEventEmployeesInfo.Create();
        employeeInfo.Employee = employeeData.Employee.PersonnelNumber;
        employeesInfoList.Add(employeeInfo);
      }
      
      var model = NNIntegration.Structures.Module.CreateWeekendWorkCancelationEvent.Create(messageGuid,
                                                                                           kindCode,
                                                                                           cancelsNumber,
                                                                                           cancelsDate,
                                                                                           mainTaskId,
                                                                                           employeesInfoList);
      
      try
      {
        result.RequestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для проведения мероприятия \"Автоматическое внесение изменений в действующий приказ\".", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для проведения мероприятия \"Автоматическое внесение изменений в действующий приказ\": {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #endregion
    
    #region Функции для работы с личным кабинетом через сервис интеграции.
    
    #region Регистрация работы в выходной и/или праздничный день
    
    /// <summary>
    /// Проверить наличие превышения количества рабочих часов.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="workFrom">Дата работы с.</param>
    /// <param name="workTo">Дата работы по.</param>
    /// <param name="firstBreakFrom">Первый перерыв с.</param>
    /// <param name="firstBreakTo">Первый перерыв по.</param>
    /// <param name="secondBreakFrom">Второй перерыв с.</param>
    /// <param name="secondBreakTo">Второй перерыв по.</param>
    /// <returns>Текст ошибки валидации.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string ValidateWeekendWorkStatement(long employeeId,
                                               string workFrom, string workTo,
                                               string firstBreakFrom, string firstBreakTo,
                                               string secondBreakFrom, string secondBreakTo)
    {
      Logger.DebugFormat("ValidateWeekendWorkStatement. ИД сотрудника: {0}. Начало выполнения.", employeeId);
      var employee = Employees.GetAll(emp => emp.Id == employeeId).FirstOrDefault();
      
      var workDateFrom = NNIntegration.PublicFunctions.Module.ParseStringToDate(workFrom).Value;
      var workDateTo = NNIntegration.PublicFunctions.Module.ParseStringToDate(workTo).Value;
      var firstBreakDateFrom = !string.IsNullOrEmpty(firstBreakFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(firstBreakFrom) : null;
      var firstBreakDateTo = !string.IsNullOrEmpty(firstBreakTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(firstBreakTo) : null;
      var secondBreakDateFrom = !string.IsNullOrEmpty(secondBreakFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(secondBreakFrom) : null;
      var secondBreakDateTo = !string.IsNullOrEmpty(secondBreakTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(secondBreakTo) : null;
      
      // Проверка перерывов.
      if ((firstBreakDateFrom.HasValue && !firstBreakDateTo.HasValue) ||
          (firstBreakDateTo.HasValue && !firstBreakDateFrom.HasValue) ||
          (secondBreakDateFrom.HasValue && !secondBreakDateTo.HasValue) ||
          (secondBreakDateTo.HasValue && !secondBreakDateFrom.HasValue))
        return DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.BreakIncompleteValidationError;
      
      // Проверка наличия превышения количества рабочих часов.
      var businessUnit = BusinessUnits.As(employee.BusinessUnit);
      var isWorkingHoursExceeded = Functions.WeekendWorkStatementDocument.IsWorkingHoursExceeded(businessUnit, workDateFrom, workDateTo, firstBreakDateFrom, secondBreakDateFrom);
      Logger.DebugFormat("ValidateWeekendWorkStatement. ИД сотрудника: {0}. Результат проверки: {1}. Конец выполнения.", employeeId, isWorkingHoursExceeded);
      if (isWorkingHoursExceeded)
      {
        var workingHoursWithoutBreak = businessUnit.WorkingHoursWithoutBreakDirRX.Value;
        return Resources.ErrorCheckWorkingHoursExceededFormat(workingHoursWithoutBreak, NNCommons.PublicFunctions.Module.GetHoursDeclension(workingHoursWithoutBreak));
      }
      
      return string.Empty;
    }
    
    /// <summary>
    /// Заполнить данные в заявлении на ВД/ПД/СР.
    /// </summary>
    /// <param name="docId">ИД документа.</param>
    /// <param name="docKindGuid">Гуид вида документа.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="reasonId">ИД причины для работы в ВД/ПД/СР</param>
    /// <param name="compensation">Компенсация.</param>
    /// <param name="description">Основание.</param>
    /// <param name="workFrom">Дата работы с.</param>
    /// <param name="workTo">Дата работы по.</param>
    /// <param name="firstBreakFrom">Первый перерыв с.</param>
    /// <param name="firstBreakTo">Первый перерыв по.</param>
    /// <param name="secondBreakFrom">Второй перерыв с.</param>
    /// <param name="secondBreakTo">Второй перерыв по.</param>
    /// <param name="restFrom">Дата доп. отдыха с.</param>
    /// <param name="restTo">Дата доп. отдыха по.</param>
    /// <param name="additionalApproverId">ИД доп. согласующего.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public string FillStatementDataForWeekendWork(long? docId, string docKindGuid, long employeeId,
                                                  long reasonId, long compensation, string description,
                                                  string workFrom, string workTo,
                                                  string firstBreakFrom, string firstBreakTo,
                                                  string secondBreakFrom, string secondBreakTo,
                                                  string restFrom, string restTo, long? additionalApproverId)
    {
      Logger.DebugFormat("FillStatementDataForWeekendWork. ИД сотрудника: {0}. ИД документа: {1}. Гуид вида документа: {2}. ИД причины для работы в ВД/ПД/СР: {3}. Компенсация: {4}. Основание: {5}. " +
                         "Дата работы с: {6}. Дата работы по: {7}. Первый перерыв с: {8}. Первый перерыв по: {9}. Второй перерыв с: {10}. Второй перерыв по: {11}. " +
                         "Дата доп. отдыха с: {12}. Дата доп. отдыха по: {13}. ИД доп. согласующего: {14}. Начало выполнения.",
                         employeeId, docId, docKindGuid, reasonId, compensation, description,
                         workFrom, workTo, firstBreakFrom, firstBreakTo, secondBreakFrom, secondBreakTo, restFrom, restTo, additionalApproverId);
      
      var statement = WeekendWorkStatementDocuments.GetAll(d => d.Id == docId).FirstOrDefault();
      if (statement == null)
      {
        var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Guid.Parse(docKindGuid));
        statement = WeekendWorkStatementDocuments.Create();
        statement.DocumentKind = docKind;
        statement.RegistrationDate = Calendar.Today;
      }
      statement.ReasonDescription = description;
      statement.Reason = WeekendWorkReasons.GetAll(r => r.Id == reasonId).FirstOrDefault();
      statement.AdditionalApprover = Employees.GetAll(emp => emp.Id == additionalApproverId).FirstOrDefault();
      var workDateFrom = NNIntegration.PublicFunctions.Module.ParseStringToDate(workFrom);
      var workDateTo = NNIntegration.PublicFunctions.Module.ParseStringToDate(workTo);
      statement.WorkDateFrom = workDateFrom;
      statement.WorkDateTo = workDateTo;
      statement.Employees.Clear();
      var line = statement.Employees.AddNew();
      line.Employee = Employees.GetAll(emp => emp.Id == employeeId).FirstOrDefault();
      line.WorkDateFrom = workDateFrom;
      line.WorkDateTo = workDateTo;
      line.FirstBreakFrom = !string.IsNullOrEmpty(firstBreakFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(firstBreakFrom) : null;
      line.FirstBreakTo = !string.IsNullOrEmpty(firstBreakTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(firstBreakTo) : null;
      line.SecondBreakFrom = !string.IsNullOrEmpty(secondBreakFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(secondBreakFrom) : null;
      line.SecondBreakTo = !string.IsNullOrEmpty(secondBreakTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(secondBreakTo) : null;
      line.RestDateFrom = !string.IsNullOrEmpty(restFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(restFrom) : null;
      line.RestDateTo = !string.IsNullOrEmpty(restTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(restTo) : null;
      line.Compensation = WeekendWorkCompensationKinds.GetAll(k => k.Id == compensation).FirstOrDefault();

      statement.Save();
      Logger.DebugFormat("FillStatementDataForWeekendWork. ИД сотрудника: {0}. ИД документа: {1}. Документ успешно сохранен.", employeeId, statement.Id);

      var importedDocuments = new List<NNCommons.Structures.Module.IDocumentInfoFromLK>();
      var importedDocument = NNCommons.Structures.Module.DocumentInfoFromLK.Create();
      importedDocument.Id = statement.Id.ToString();
      importedDocument.Type = string.Format("I{0}s", statement.GetEntityMetadata().IntegrationServiceName);
      importedDocument.Title = statement.Name;
      importedDocument.System = HRPro.HRProSolution.PublicConstants.Module.TargetNameSpace;
      importedDocuments.Add(importedDocument);
      return NNCommons.IsolatedFunctions.Json.SerializeInJson(importedDocuments);
    }
    
    /// <summary>
    /// Получить данные для доработки заявления на ВД/ПД/СР.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Строка с информацией.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetStatementDataForReworkWeekendWork(long assignmentId)
    {
      Logger.DebugFormat("GetStatementDataForReworkWeekendWork. ИД задания: {0}. Начало выполнения.", assignmentId);
      var assignment = NNCommons.StatementReworkAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      var docRealated = assignment.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var document = WeekendWorkStatementDocuments.As(docRealated);
      if (document == null)
      {
        Logger.ErrorFormat("GetStatementDataForReworkWeekendWork. ИД задания: {0}. Документ не найден.", assignmentId);
        return string.Empty;
      }
      var line = document.Employees.FirstOrDefault();
      var employee = line.Employee;
      var result = Structures.Module.StatementDataForReworkWeekendWork.Create();
      result.InitiatorId = employee.Id;
      result.StatementId = document.Id;
      result.Reason = document.Reason?.Id ?? 0;
      result.ReasonDescription = document.ReasonDescription;
      result.PerformerInfo = string.Join(Environment.NewLine, employee.JobTitle, employee.Department, employee.BusinessUnit);
      result.AdditionalApproverId = document.AdditionalApprover?.Id ?? 0;
      result.WorkDateFrom = line.WorkDateFrom.Value;
      result.WorkDateTo = line.WorkDateTo.Value;
      result.FirstBreakFrom = line.FirstBreakFrom;
      result.FirstBreakTo = line.FirstBreakTo;
      result.SecondBreakFrom = line.SecondBreakFrom;
      result.SecondBreakTo = line.SecondBreakTo;
      result.RestDateFrom = line.RestDateFrom;
      result.RestDateTo = line.RestDateTo;
      result.Compensation = line.Compensation.Id;
      
      Logger.DebugFormat("GetStatementDataForReworkWeekendWork. ИД задания: {0}. Конец выполнения.", assignmentId);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Получить список видов компенсаций.
    /// </summary>
    /// <param name="docKindGuid">Гуид вида документа.</param>
    /// <returns>Json, содержащий список выплат.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetCompensationKindsByDocKind(string docKindGuid)
    {
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Guid.Parse(docKindGuid));
      if (docKind == null)
      {
        Logger.DebugFormat("GetCompensationKindsByDocKind. Гуид вида документа: {0}. Не найден вид документа.", docKindGuid);
        return "[]";
      }
      var compensationKinds = WeekendWorkCompensationKinds.GetAll(w => w.Status == NNOtherProcesses.WeekendWorkCompensationKind.Status.Active && Equals(w.DocumentKind, docKind));
      var structureList = new List<Structures.Module.ICompensationKindStructure>();
      foreach (var compensationKind in compensationKinds)
      {
        var structure = Structures.Module.CompensationKindStructure.Create(compensationKind.Id, compensationKind.Name,
                                                                           compensationKind.NeedRestDate.GetValueOrDefault(),
                                                                           compensationKind.IsRestDateRequired.GetValueOrDefault());
        structureList.Add(structure);
      }
      
      var result = string.Empty;
      try
      {
        result = IsolatedFunctions.Json.SerializeInJson(structureList);
      }
      catch (Exception ex)
      {
        var errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("GetCompensationKindsByDocKind. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #region Регистрация выплат по ЛНА.
    
    /// <summary>
    /// Получить список типов выплат доступных сотруднику.
    /// </summary>
    /// <param name="employeeId">ИД сотруднкиа.</param>
    /// <returns>Json, содержащий список выплат.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetPaymentTypesByEmployeId(long employeeId)
    {
      Logger.DebugFormat("GetPaymentTypesByEmployeId. EmployeeID: {0}. Начало работы функции. ", employeeId);
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      if (employee == null)
      {
        Logger.DebugFormat("GetPaymentTypesByEmployeId. EmployeeID: {0}. Не найден сотрудник по ИД.", employeeId);
        return "[]";
      }
      
      var paymentTypeSettings = LrdPaymentTypeSettings.GetAll(x => x.PaymentType != null).Where(x => BusinessUnits.Equals(x.BusinessUnit, employee.BusinessUnit) &&
                                                                                                x.Status == NNOtherProcesses.LrdPaymentTypeSetting.Status.Active &&
                                                                                                NNOtherProcesses.LrdPaymentMPGroups.Equals(x.MPGroup, employee.MPGroupDirRX));
      
      var paymentTypes = LrdPaymentTypes.GetAll(x => x.Status == NNOtherProcesses.LrdPaymentType.Status.Active &&
                                                paymentTypeSettings.Any(s => LrdPaymentTypes.Equals(x, s.PaymentType))).ToList();
      
      return GetJsonPaymentTypes(paymentTypes);
    }
    
    /// <summary>
    /// Получить Json, содержащий список типов выплат ЛНА для передачи в ЛК.
    /// </summary>
    /// <param name="paymentTypes">Список типов выплат.</param>
    /// <returns>Строка с Json.</returns>
    [Public]
    public string GetJsonPaymentTypes(List<NNOtherProcesses.ILrdPaymentType> paymentTypes)
    {
      var structureList = new List<NNIntegration.Structures.Module.IPaymentTypeStructure>();
      foreach (var paymentType in paymentTypes)
      {
        var structure = NNIntegration.Structures.Module.PaymentTypeStructure.Create(paymentType.Id, paymentType.Name);
        structureList.Add(structure);
      }
      
      var result = string.Empty;
      try
      {
        result = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(structureList);
      }
      catch (Exception ex)
      {
        var errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("GetJsonPaymentTypes. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    /// <summary>
    /// Получить настройку типа выплаты для ЛК.
    /// </summary>
    /// <param name="paymentTypeId">ИД типа выплаты.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Json, содержащий настройку типа выплаты.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetPaymentTypeSetting(long paymentTypeId, long employeeId)
    {
      var prefix = string.Format("GetPaymentTypeSetting. PaymentTypeId: {0}. EmployeeId: {1}", paymentTypeId, employeeId);
      Logger.DebugFormat("{0}. Старт функции для поиска настройки типа выплаты.", prefix);
      var employee = Employees.GetAll(x => employeeId.Equals(x.Id)).FirstOrDefault();
      if (employee == null)
      {
        Logger.DebugFormat("{0}. Не найден сотрудник по ИД.", prefix);
        return string.Empty;
      }
      
      var paymentTypeExist = LrdPaymentTypes.GetAll(x => paymentTypeId.Equals(x.Id)).Any();
      if (!paymentTypeExist)
      {
        Logger.DebugFormat("{0}. Не найден тип выплаты по ИД.", prefix);
        return string.Empty;
      }
      
      var paymentTypeSetting = LrdPaymentTypeSettings.GetAll(x =>
                                                             BusinessUnits.Equals(x.BusinessUnit, employee.BusinessUnit) &&
                                                             x.PaymentType != null && x.PaymentType.Id == paymentTypeId &&
                                                             NNOtherProcesses.LrdPaymentMPGroups.Equals(x.MPGroup, employee.MPGroupDirRX)).FirstOrDefault();
      if (paymentTypeSetting == null)
      {
        Logger.DebugFormat("{0}. Не найдена настройка типа выплаты по ИД типа выплаты: {1}, ИД нашей организации сотрудника: {2}.", prefix, paymentTypeId, employee.BusinessUnit.Id);
        return string.Empty;
      }
      
      var constant = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.ChildSupportPaymentTypesIds.Guid);
      if (string.IsNullOrEmpty(constant))
      {
        Logger.DebugFormat("{0}. Прикладная константа '{1}' не заполнена.", prefix, Constants.Module.ConstantsSetting.ChildSupportPaymentTypesIds.Guid);
        return string.Empty;
      }
      
      var childSupportTypesIds = new List<long>();
      try
      {
        childSupportTypesIds = constant.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => long.Parse(id.Trim())).ToList();
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("{0}. Не удалось извлеч список ИД из прикладной константы: {1}. Текст ошибки: {2}. StackTrace: {3}.",
                           prefix, Constants.Module.ConstantsSetting.ChildSupportPaymentTypesIds.Guid,
                           ex.Message, ex.StackTrace);
        return string.Empty;
      }
      
      var isChildSupport = childSupportTypesIds.Contains(paymentTypeSetting.PaymentType.Id);
      
      var structure = NNIntegration.Structures.Module.PaymentTypeSettingStructure.Create(
        paymentTypeSetting.Id,
        paymentTypeSetting.FixedPayment == true,
        paymentTypeSetting.PaymentAmount.ToString(),
        paymentTypeSetting.Instruction,
        isChildSupport,
        employee.Person.Sex == OverrideSolution.Person.Sex.Male);
      
      return NNIntegration.IsolatedFunctions.Convert.SerializeInJson(structure);
    }
    
    /// <summary>
    /// Получить json строку из ЛК для подстановки в источник данных параметра в ЛК.
    /// </summary>
    /// <param name="json">Json с данными.</param>
    /// <returns>Json с данными.</returns>
    /// <remarks>Функция сделана POST из-за ограничения ЛК на длину параметров в GET-запросах.</remarks>
    [Public(WebApiRequestType = RequestType.Post)]
    public string GetJsonForDataSource(string json)
    {
      return json;
    }
    
    /// <summary>
    /// Получить информацию о предыдущем заявлении для выплаты ЛНА по заданию на доработку.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Json, содержащий настройку типа выплаты.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetLrdPaymentDataForRework(long assignmentId)
    {
      Logger.DebugFormat("GetLnaPaymentDataForRework. ИД задания: {0}. Начало выполнения.", assignmentId);
      var assignment = NNCommons.StatementReworkAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      var docRelated = assignment.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var statement = LrdPaymentStatementDocuments.As(docRelated);
      if (statement == null)
      {
        Logger.ErrorFormat("GetLnaPaymentDataForRework. ИД задания: {0}. Документ не найден.", assignmentId);
        return string.Empty;
      }
      var employee = statement.EmployeeDirRX;
      var performerInfo = string.Join(Environment.NewLine, employee.BusinessUnit, employee.Department, employee.JobTitle);
      
      var assignmentInfo = NNIntegration.Structures.Module.LrdPaymentAssignmentInfo.Create(
        assignment.Performer.Id,
        statement.Id,
        statement.PaymentType.Id,
        statement.PaymentType.Name,
        statement.ChildName,
        statement.ChildBirthDate,
        statement.RequestedAmount.ToString(),
        statement.Reason,
        performerInfo);
      
      return NNIntegration.IsolatedFunctions.Convert.SerializeInJson(assignmentInfo);
    }
    
    /// <summary>
    /// Заполнить данные в заявлении на выплату по ЛНА.
    /// </summary>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="paymentTypeId">ИД типа выплаты.</param>
    /// <param name="childName">ФИО ребенка.</param>
    /// <param name="childBirthDate">Дата рождения.</param>
    /// <param name="amount">Сумма.</param>
    /// <param name="reason">Причина.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillLrdPaymentData(long documentId, long paymentTypeId, string childName, string childBirthDate, string amount, string reason)
    {
      var logPrefix = string.Format("FillLrdPaymentData. ИД документа: {0}", documentId);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      var statement = LrdPaymentStatementDocuments.GetAll(d => d.Id == documentId).FirstOrDefault();
      statement.PaymentType = LrdPaymentTypes.GetAll(p => p.Id == paymentTypeId).FirstOrDefault();
      statement.ChildName = childName;
      statement.RequestedAmount = Double.Parse(amount);
      statement.ApprovedAmount = statement.RequestedAmount;
      statement.Reason = reason;
      statement.ChildBirthDate = !string.IsNullOrEmpty(childBirthDate) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(childBirthDate) : null;
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
    /// Получить ИД шаблона заявление на выплату по ЛНА.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>JSON строку с ИД шаблона и ИД вида документа.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string GetLrdPaymentStatementTemplateId(long employeeId)
    {
      var logPrefix = string.Format("GetLrdPaymentStatementTemplateId. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      var documentKindGuid = Constants.Module.DocumentKind.LrdPaymentStatementDocument;
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(documentKindGuid);
      var employee = Employees.GetAll(e => e.Id == employeeId).FirstOrDefault();
      var businessUnit = employee.BusinessUnit;
      var settingTemplates = HRPro.HRProSolution.DocumentKinds.As(docKind).TemplatesLKToBusinessUnitHRPro;
      var template = settingTemplates.FirstOrDefault(t => Equals(t.BusinessUnit, businessUnit))?.TemplateLK;
      if (template == null)
        template = settingTemplates.FirstOrDefault(t => t.BusinessUnit == null)?.TemplateLK;
      
      var result = NNCommons.Structures.Module.StatementData.Create();
      if (template != null)
      {
        result.TemplateId = template.Id;
        result.DocumentKindId = docKind.Id;
        result.Error = string.Empty;
      }
      else
      {
        result.TemplateId = 0;
        result.DocumentKindId = 0;
        result.Error = DirRX.NNOtherProcesses.Resources.LrdPaymentTemplateNotFoundErrorFormat(docKind.Name);
      }
      Logger.DebugFormat("{0}. Конец выполнения.", logPrefix);
      return NNCommons.IsolatedFunctions.Json.SerializeInJson(result);
    }
    
    /// <summary>
    /// Проверить, что тип выплаты - "Материальная помощь работнику, имеющему ребенка-инвалида в возрасте до 18 лет".
    /// </summary>
    /// <param name="paymentTypeId">ИД типа выплаты.</param>
    /// <returns>True, если тип выплаты - "Материальная помощь работнику, имеющему ребенка-инвалида в возрасте до 18 лет".</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public bool CheckForDisabledChildPaymentType(long? paymentTypeId)
    {
      var disabledChildPaymentTypeId = NNCommons.PublicFunctions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.DisabledChildPaymentTypeId.Guid);
      return paymentTypeId == disabledChildPaymentTypeId;
    }
    
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetPaymentTypeNameById(long? paymentTypeId)
    {
      var paymentType = DirRX.NNOtherProcesses.LrdPaymentTypes.GetAll(x => x.Id == paymentTypeId).FirstOrDefault();
      return paymentType?.Name ?? "";
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий «Регистрация выплат по ЛНА».
    
    #region IF-S-6366. Автоматическое создание мероприятия «Регистрация выплат ЛНА» из КЭДО в АСУП (КЭДО → АСУП)
    
    /// <summary>
    /// Процесс создания мероприятия «Регистрация выплат ЛНА».
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="messageGuid">Гуид сообщения.</param>
    /// <returns>Ответ.</returns>
    [Public]
    public IResultGenerateRequestText CreateLrdPaymentEvent(long mainTaskId, string messageGuid)
    {
      var logPrefix = string.Format("CreateLrdPaymentEvent. ИД основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = LrdPaymentStatementDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      logPrefix = string.Format("{0}. ИД документа: {1}", logPrefix, doc.Id);
      
      if (doc.ApprovedAmount == null)
      {
        result.ErrorText = string.Format("Не заполнено поле {0}.", doc.Info.Properties.ApprovedAmount.LocalizedName);
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var errorMessage = string.Empty;
      var payDate = doc.PayDate.HasValue ? doc.PayDate.Value.ToString("dd.MM.yyyy") : string.Empty;
      var childBirthDate = doc.ChildBirthDate.HasValue ? doc.ChildBirthDate.Value.ToString("dd.MM.yyyy") : string.Empty;
      var purposeCode = doc.PaymentType.PaymentPurpose == null ? string.Empty : doc.PaymentType.PaymentPurpose.Code;
      var model = NNIntegration.Structures.Module.CreateLrdPaymentEvent.Create(messageGuid,
                                                                               doc.EmployeeDirRX.PersonnelNumber,
                                                                               doc.PaymentType.EventReason.Code,
                                                                               doc.PaymentType.EventReasonClarification.Code,
                                                                               purposeCode,
                                                                               payDate,
                                                                               doc.ApprovedAmount.Value,
                                                                               doc.ChildName,
                                                                               childBirthDate,
                                                                               mainTaskId);
      
      try
      {
        result.RequestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для проведения мероприятия \"Регистрация выплат по ЛНА\".", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для проведения мероприятия \"Регистрация выплат по ЛНА\": {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    #endregion
    
    #region IF-S-10400. Проверка данных по заявлению на выплату ЛНА (АСУП → КЭДО)
    
    /// <summary>
    /// Проверить заявление на выплату ЛНА во внешней системе (базовая перегрузка).
    /// </summary>
    /// <param name="request">Запрос.</param>
    /// <param name="statementId">ИД заявления.</param>
    /// <returns>Ответ.</returns>
    /// <remarks>Если заявление ещё не создано в RX, в параметре ИД заявление передаётся 0.</remarks>
    public ILrdPaymentCheckResponse CheckLrdPaymentInExternalSystem(NNIntegration.Structures.Module.ILrdPaymentCheckRequest request, long statementId)
    {
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.LrdPaymentCheck;
      var logPrefix = string.Format("CheckLrdPaymentInExternalSystem. GUID потока: {0}. ID заявления: {1}", flowGuid, statementId);
      Logger.DebugFormat("{0}. Старт синхронной отправки сообщения во внешнюю систему.", logPrefix);
      
      var requestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(request);
      
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(requestText, flowGuid, statementId);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultSendingMessage.ErrorText);
        return null;
      }
      
      var response = NNIntegration.Structures.Module.LrdPaymentCheckResponse.Create();
      try
      {
        response = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(resultSendingMessage.AnswerText, response);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        return null;
      }
      
      Logger.DebugFormat("{0}. Конец синхронной отправки сообщения.", logPrefix);
      return response;
    }
    
    /// <summary>
    /// Проверить заявление на выплату ЛНА во внешней системе (перегрузка для ЛК).
    /// </summary>
    /// <param name="EmployeeId">Таб. номер сотрудника.</param>
    /// <param name="PaymentTypeId">ИД типа выплаты.</param>
    /// <param name="ChildName">ФИО ребенка.</param>
    /// <param name="BirthDate">Дата рождения.</param>
    /// <returns>Текст ошибки. Если ошибок - нет, то возвращается пуствя строка.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string CheckLrdPaymentInExternalSystem(long EmployeeId, long PaymentTypeId, string ChildName, string BirthDate)
    {
      Logger.DebugFormat("CheckLrdPaymentInExternalSystem. EmployeeId: {0}. PaymentTypeId: {1}. Cтарт проверки заявления на выплату ЛНА во внешней системе.",
                         EmployeeId, PaymentTypeId);
      
      var employee = Employees.GetAll().Where(x => x.Id == EmployeeId).FirstOrDefault();
      if (employee == null)
      {
        Logger.ErrorFormat("CheckLrdPaymentInExternalSystem. EmployeeId: {0}. Не найден сотрудник по ИД.", EmployeeId);
        return DirRX.NNOtherProcesses.Resources.ExternalSystemCheckLrdPaymentError;
      }
      
      var paymentType = LrdPaymentTypes.GetAll().Where(x => x.Id == PaymentTypeId).FirstOrDefault();
      if (paymentType == null)
      {
        Logger.ErrorFormat("CheckLrdPaymentInExternalSystem. PaymentTypeId: {0}. Не найден тип выплат по ИД.", PaymentTypeId);
        return DirRX.NNOtherProcesses.Resources.ExternalSystemCheckLrdPaymentError;
      }
      
      var request = NNIntegration.Structures.Module.LrdPaymentCheckRequest.Create(Guid.NewGuid().ToString(),
                                                                                  employee.PersonnelNumber,
                                                                                  paymentType.EventReason.Code,
                                                                                  paymentType.EventReasonClarification.Code,
                                                                                  paymentType.PaymentPurpose != null ? paymentType.PaymentPurpose.Code : string.Empty,
                                                                                  ChildName,
                                                                                  BirthDate,
                                                                                  Calendar.Now.ToString("dd.MM.yyyy"));
      var response = CheckLrdPaymentInExternalSystem(request, 0);
      if (response == null)
      {
        Logger.DebugFormat("CheckLrdPaymentInExternalSystem. {0}", DirRX.NNOtherProcesses.Resources.ExternalSystemCheckLrdPaymentError);
        return DirRX.NNOtherProcesses.Resources.ExternalSystemCheckLrdPaymentError;
      }
      
      return response.Result ? string.Empty :
        !string.IsNullOrWhiteSpace(response.RemarkText) ? response.RemarkText : Resources.CheckLrdPaymentError;
    }
    
    #endregion
    
    #region IF-S-25360. Получение списка детей работника (АСУП → КЭДО).
    
    /// <summary>
    /// Получить информацию о детях сотрудника из внешней системы.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Список детей. Если возникла ошибка или у сотрудника нет детей - пустой список.</returns>
    public List<IChild> GetEmployeeChildrenFromExternalSystem(long employeeId)
    {
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.LrdPaymentChildren;
      var logPrefix = string.Format("GetEmployeeChildrenFromExternalSystem. GUID потока: {0}. ИД сотрудника: {1}.", flowGuid, employeeId);
      var children = new List<IChild>();
      
      var employee = Employees.GetAll().Where(x => x.Id == employeeId).FirstOrDefault();
      if (employee == null)
      {
        Logger.ErrorFormat("{0} Не удалось найти сотрудника по ИД.", logPrefix);
        return children;
      }
      
      var request = NNIntegration.Structures.Module.LrdPaymentChildrenRequest.Create(Guid.NewGuid().ToString(),
                                                                                     employee.PersonnelNumber);
      
      Logger.DebugFormat("{0} Старт синхронной отправки сообщения во внешнюю систему.", logPrefix);
      var requestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(request);
      
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(requestText, flowGuid, 0);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0} Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultSendingMessage.ErrorText);
        return children;
      }
      
      var response = NNIntegration.Structures.Module.LrdPaymentChildrenResponse.Create();
      try
      {
        response = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(resultSendingMessage.AnswerText, response);
        children = response.Children;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0} Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        return children;
      }
      
      Logger.DebugFormat("{0} Конец синхронной отправки сообщения.", logPrefix);
      return children;
    }
    
    /// <summary>
    /// Получить информацию о детях сотрудника (перегрузка для ЛК).
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Текст ошибки. Если ошибок - нет, то возвращается пуствя строка.</returns>
    /// <remarks>Функция выполняет запрос во внешнюю систему для получения списка детей и кэширует его в БД.</remarks>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetChildrenByEmployeeId(long employeeId)
    {
      Logger.DebugFormat("GetChildrenInfoByEmployeeId. EmployeeId: {0}. Получен запрос на предоставление информации о детях сотрудника.", employeeId);
      
      // Поиск кэшированного ответа в БД на исходящий запрос для получения информации о детях сотрудника.
      var cachedResponse = GetCachedChildrenInfoByEmployee(employeeId);
      if (!string.IsNullOrEmpty(cachedResponse))
        return cachedResponse;
      
      // Выполнение запроса во внешнюю систему для получения списка детей.
      var children = GetEmployeeChildrenFromExternalSystem(employeeId);
      var json = EssPlatform.PublicFunctions.Module.SerializedToJson(children);
      
      // Кэширование полученного ответа с информацией о детях сотрудника в БД.
      InsertOrUpdateChildrenResponseCache(employeeId, json);
      return json;
    }
    
    /// <summary>
    /// Получить информацию о детях сотрудника из кэша (таблица в БД).
    /// </summary>
    /// <param name="employee">ИД сотрудника.</param>
    /// <returns>Json c инфомрацией о детях. Если информации нет - то пустая строка.</returns>
    public string GetCachedChildrenInfoByEmployee(long employeeId)
    {
      var response = string.Empty;
      try
      {
        var key = string.Format(Constants.Module.LrdPaymentChildrenCacheKeyFormat, employeeId);
        if (Cache.TryGetValue(key, out response))
          Logger.DebugFormat("GetCachedChildrenInfoByEmployee. ИД сотрудника: {0}. Кэшированный запрос - обнаружен.", employeeId);
        else
          Logger.DebugFormat("GetCachedChildrenInfoByEmployee. ИД сотрудника: {0}. Кэшированный запрос - не обнаружен.", employeeId);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetCachedChildrenInfoByEmployee. ИД сотрудника: {0}. Ошибка: {1}. StackTrace: {2}.", employeeId, ex.Message, ex.StackTrace);
      }
      
      return response;
    }
    
    /// <summary>
    /// Обновить информацию о детях сотрудника в кэше (таблица в БД).
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="response">Ответ.</param>
    /// <returns>Json c инфомрацией о детях. Если информации нет - то пустая строка.</returns>
    public void InsertOrUpdateChildrenResponseCache(long employeeId, string response)
    {
      try
      {
        // Получить время в минутах из прикладной константы, по истечению которых необходимо чистить кэшированные запросы.
        var minutes = DirRX.NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.LrdPaymentChildrenCacheTime.Guid);
        if (minutes == null || minutes.Value < 0)
        {
          Logger.ErrorFormat("InsertOrUpdateChildrenResponseCache. Константа: {0}. Значение: {1}. Не удалось определить количество минут, по истечению которых необходо удалить кэш. Будет использовано значение по умолчанию: {2}.",
                             Constants.Module.ConstantsSetting.LrdPaymentChildrenCacheTime.Guid, minutes, Constants.Module.ConstantsDefaultValues.LrdPaymentChildrenCacheTime);
          minutes = Constants.Module.ConstantsDefaultValues.LrdPaymentChildrenCacheTime;
        }
        
        var key = string.Format(Constants.Module.LrdPaymentChildrenCacheKeyFormat, employeeId);
        Cache.AddOrUpdate(key, response, Calendar.Now.AddMinutes(minutes.Value));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("InsertOrUpdateChildrenResponseCache. ИД сотрудника: {0}. Ошибка: {1}, StackTrace: {2}.", employeeId, ex.Message, ex.StackTrace);
      }
    }
    
    #endregion
    
    #region IF-S-10399. Передача приказа на выплату по ЛНА из АСУП в КЭДО.
    
    /// <summary>
    /// Изменить или создать приказ на приказа на выплату по ЛНА.
    /// </summary>
    /// <param name="idIncomingMessage">Id записи справочника Входящее информационное сообщение.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public NNIntegration.Structures.Module.IProcessIncomingMessageResult UpdateOrCreateLrdPaymentOrder(long idIncomingMessage)
    {
      var logPrefix = string.Format("UpdateOrCreateLrdPaymentOrder. Ид входящего сообщения: {0}.", idIncomingMessage);
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
      
      var model = NNIntegration.Structures.Module.LrdPaymentOrderRequest.Create();
      try
      {
        model = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка десериализации в структуру LrdPaymentOrderRequest.";
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
      var document = LrdPaymentOrderDocuments.GetAll(x => x.ExternalId == model.SAPID).FirstOrDefault();
      if (document == null)
      {
        Logger.DebugFormat("{0} Документ по SAPID: {1} не найден. Создание нового документа.", logPrefix, model.SAPID);
        document = NNOtherProcesses.PublicFunctions.LrdPaymentOrderDocument.CreateLrdPaymentOrderWithEmployee(employee, NNOtherProcesses.PublicConstants.Module.DocumentKind.LrdPaymentOrderDocument);
        if (document == null)
        {
          result.ErrorText = string.Format("Не удалось создать документ с SAP ID: {0}. Подробные сведения об ошибках в лог файлах.", model.SAPID);
          Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
          return result;
        }
      }
      
      result.EntityId = document?.Id;
      
      var checkResult = OverrideSolution.PublicFunctions.DocRelatedToWorkBase.CanUpdateDocFromASUP(document);
      if (!string.IsNullOrEmpty(checkResult))
      {
        result.ErrorText = string.Format("Версия документа с ID: {0} не создана. {1}.", document.Id, checkResult);
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
        document.EmployeeDirRX = employee;
        document.OurSignatory = signed;
        document.PreparedBy = prepared;
        document.RegistrationNumber = model.RegNumber;
        document.RegistrationDate = model.Date;
        document.ExternalId = model.SAPID;
        var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.LrdPaymentOrderExtension.Guid);
        
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

      result.ErrorText = this.CreateAndStartNewSignDocumentSubtask(model.TaskID, prepared, document);
      if (string.IsNullOrEmpty(result.ErrorText))
        Logger.DebugFormat("{0}. Успешно обработан.", logPrefix);
      
      return result;
    }
    
    #endregion
    
    #endregion
  }
}