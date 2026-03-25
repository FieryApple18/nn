using System;
using System.Collections.Generic;
using System.Linq;
using DirRX.NNIntegration.Structures.Module;
using DirRX.NNOtherProcesses;
using Newtonsoft.Json;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;
using DirRX.OverrideSolution.Employee;

namespace DirRX.NNIntegration.Server
{
  public partial class ModuleAsyncHandlers
  {
    
    /// <summary>
    /// Создать задачи для повторной проштамповки документов.
    /// </summary>
    /// <param name="args">Аргументы АО.</param>
    public virtual void CreateRestampingTasksAsync(DirRX.NNIntegration.Server.AsyncHandlerInvokeArgs.CreateRestampingTasksAsyncInvokeArgs args)
    {
      var logPrefix = string.Format("CreateRestampingTasksAsync. RetryIteration: {0}. ИД инициатора: {1}. ИД варианта процесса: {2}", args.RetryIteration, args.UserId, args.ProcessKindId);
      Logger.DebugFormat("{0}. Старт асинхронного обработчика \"Создать задачи для повторной проштамповки документов\".", logPrefix);
      
      var initiator = Users.GetAll(x => x.Id == args.UserId).FirstOrDefault();
      if (initiator == null)
      {
        Logger.ErrorFormat("{0}. Не найден инициатор по ИД: {1}. Завершение выполнения асинхронного обработчика \"Создать задачи для повторной проштамповки документов\".", logPrefix, args.UserId);
        return;
      }
      
      var processKind = Sungero.Workflow.ProcessKinds.GetAll(x => x.Id == args.ProcessKindId).FirstOrDefault();
      if (processKind == null)
      {
        Logger.ErrorFormat("{0}. Не найден вариант процесса по ИД: {1}. Завершение выполнения асинхронного обработчика \"Создать задачи для повторной проштамповки документов\".", logPrefix, args.ProcessKindId);
        return;
      }
      
      // Получить список ИД документов для перештамповки.
      List<long> documentsIds;
      try
      {
        documentsIds = args.DocumentsIds.Split(',').Select(x => long.Parse(x)).ToList();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Не удалось опредилить список ИД документов для перештамповки из аргумента АО: {1}. Ошибка: {2}.", logPrefix, args.DocumentsIds, ex.Message);
        Logger.DebugFormat("{0}. Завершение выполнения асинхронного обработчика \"Создать задачи для повторной проштамповки документов\".", logPrefix);
        return;
      }
      
      var errorsList = new List<string>();
      var documents = DirRX.OverrideSolution.DocRelatedToWorkBases.GetAll(x => documentsIds.Contains(x.Id));
      var documentsCount = documents.Count();
      foreach (var document in documents)
      {
        // Проверить возможность отправить документ на перештамповку.
        var error = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.Remote.CanSendToRestamping(document);
        
        // Стартовать задачу для повторной перештамповки.
        if (string.IsNullOrEmpty(error))
          error = DirRX.OverrideSolution.PublicFunctions.DocRelatedToWorkBase.Remote.StartRestampingHRProTask(document, initiator, processKind);
        
        // Обработка ошибок.
        if (!string.IsNullOrEmpty(error))
        {
          var hyperLink = Hyperlinks.Get(document);
          error = string.Format("{0}: {1}", hyperLink, error);
          errorsList.Add(error);
        }
      }
      
      // Отправка уведомления с результатами.
      var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.RestampingDocsNoticeSubject, initiator);
      var errorsCount = errorsList.Count;
      var allDocuments = string.Format(DirRX.NNIntegration.Resources.RestampingDocsNoticeTotalInfo, documentsCount);
      var sentDocuments = string.Format(DirRX.NNIntegration.Resources.RestampingDocsNoticeSendedInfo, documentsCount - errorsCount);
      var notSentDocuments = string.Format(DirRX.NNIntegration.Resources.RestampingDocsNoticeNotSendedInfo, errorsCount);
      
      Logger.DebugFormat("{0}. Всего документов: {1}. {2}. {3}.", logPrefix, allDocuments, sentDocuments, notSentDocuments);
      
      var results = string.Format("{0}{1}{2}{3}{4}", allDocuments, Environment.NewLine, sentDocuments, Environment.NewLine, notSentDocuments);
      if (errorsCount > 0)
      {
        var errorText = string.Join(Environment.NewLine, errorsList);
        results = string.Format("{0}{1}{2}", results, Environment.NewLine, errorText);
      }
      
      notice.ActiveText = results;
      notice.Start();
      
      Logger.DebugFormat("{0}. Завершение выполнения асинхронного обработчика \"Создать задачи для повторной проштамповки документов\".", logPrefix);
    }
    
    /// <summary>
    /// Перевыпустить асинхронно сертификат сотруднику.
    /// </summary>
    /// <param name="args">EmployeeId - ид сотрудника, для которого перевыпускается сертификат.</param>
    public virtual void ReissueCertificateAsync(DirRX.NNIntegration.Server.AsyncHandlerInvokeArgs.ReissueCertificateAsyncInvokeArgs args)
    {
      var logPrefix = string.Format("ReissueCertificateAsync. RetryIteration: {0}. ИД сотрудника: {1}", args.RetryIteration, args.EmployeeId);
      Logger.DebugFormat("{0}. Старт асинхронного обработчика \"Асинхронный перевыпуск сертификата ЭП для сотрудника\".", logPrefix);
      
      var employee = Employees.GetAll(x => x.Id == args.EmployeeId).FirstOrDefault();
      if (employee == null)
      {
        Logger.ErrorFormat("{0}. Не найден сотрудник по ИД. Конец выполнения асинхронного обработчика \"Асинхронный перевыпуск сертификата ЭП для сотрудника\".", logPrefix);
        return;
      }
      
      // Блокировка сотрудника.
      Locks.TryLock(employee);
      var lockInfo = Locks.GetLockInfo(employee);
      if (!lockInfo.IsLockedByMe || employee.LockedByAsync == true)
      {
        Logger.DebugFormat("{0}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}. IsLockedByAsync: {3}.", logPrefix, lockInfo.OwnerName, lockInfo.LockTime, employee.LockedByAsync == true);
        args.Retry = true;
        return;
      }
      
      try
      {
        Logger.DebugFormat("{0}. Перевыпуск сертификата ЭП сотруднику.", logPrefix);
        DirRX.OverrideSolution.PublicFunctions.Employee.ReissueCertificate(employee);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при перевыпуске сертификата для сотрудника. Ошибка: {1}. StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
        args.Retry = true;
      }
      finally
      {
        Locks.Unlock(employee);
        Logger.DebugFormat("{0}. Конец выполнения асинхронного обработчика \"Асинхронный перевыпуск сертификата ЭП для сотрудника\".", logPrefix);
      }
    }
    
    /// <summary>
    /// Выпустить асинхронно сертификат основному сотруднику.
    /// </summary>
    /// <param name="args">EmployeeId - ид основного сотрудника, для которого выпускается сертификат.</param>
    /// <param name="args">NewPhoneNumber - новый личный телефон сотрудника, который будет использован при выпуске сертификата.</param>
    public virtual void IssueCertificateToMainEmployeeAsync(DirRX.NNIntegration.Server.AsyncHandlerInvokeArgs.IssueCertificateToMainEmployeeAsyncInvokeArgs args)
    {
      var logPrefix = string.Format("IssueCertificateToMainEmployeeAsync. RetryIteration: {0}. ИД сотрудника: {1}", args.RetryIteration, args.MainEmployeeId);
      Logger.DebugFormat("{0}. Старт асинхронного обработчика \"Асинхронный выпуск сертификата ЭП для основного сотрудника\".", logPrefix);
      
      var mainEmployee = Employees.GetAll(x => x.Id == args.MainEmployeeId).FirstOrDefault();
      if (mainEmployee == null)
      {
        Logger.ErrorFormat("{0}. Не найден сотрудник по ИД. Конец выполнения асинхронного обработчика \"Асинхронный выпуск сертификата ЭП для основного сотрудника\".", logPrefix);
        return;
      }
      
      if (string.IsNullOrEmpty(args.NewPhoneNumber))
      {
        Logger.ErrorFormat("{0}. Не заполнен новый личный телефон сотрудника. Конец выполнения асинхронного обработчика \"Асинхронный выпуск сертификата ЭП для основного сотрудника\".", logPrefix);
        return;
      }
      
      // Блокировка сотрудника.
      Locks.TryLock(mainEmployee);
      var lockInfo = Locks.GetLockInfo(mainEmployee);
      if (!lockInfo.IsLockedByMe || mainEmployee.LockedByAsync == true)
      {
        Logger.DebugFormat("{0}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}. IsLockedByAsync: {3}.", logPrefix, lockInfo.OwnerName, lockInfo.LockTime, mainEmployee.LockedByAsync == true);
        args.Retry = true;
        return;
      }
      
      try
      {
        Logger.DebugFormat("{0}. Выпуск сертификата ЭП основному сотруднику сотруднику.", logPrefix);
        OverrideSolution.PublicFunctions.Employee.IssueCertificateToMainEmployee(mainEmployee, args.NewPhoneNumber);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Не выполнено изменение фиктивного номера телефона сотрудника для перевыпуска сертификата. Ошибка: {1}. StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
        args.Retry = true;
      }
      finally
      {
        Locks.Unlock(mainEmployee);
        Logger.DebugFormat("{0}. Конец выполнения асинхронного обработчика \"Асинхронный выпуск сертификата ЭП для основного сотрудника\".", logPrefix);
      }
    }

    /// <summary>
    /// Сохранение статуса сообщения полученного из Суперники.
    /// </summary>
    /// <param name="args.RXIncomingMessageId">RXIncomingMessageId - Идентификатор записи справочника "Входящее интеграционное сообщение".</param>
    /// <param name="args.MBMessageId">MBMessageId - Идентификатор записи в таблице "Messages".</param>
    /// <param name="args.DateTime">DateTime - Дата и время создания записи справочника "Входящее интеграционное сообщение".</param>
    /// <param name="args.Status">Status - Статус-код обработки.</param>
    /// <param name="args.SelectedButton">SelectedButton - Тип нажатой кнопки.</param>
    /// <param name="args.Comment">Comment - Комментарий в случае если нажата кнопка.</param>
    /// <param name="args.ErrorMessage">ErrorMessage - Текст ошибки, полученный из ПМД.</param>
    public virtual void SaveStatusSupernikaPushNotification(DirRX.NNIntegration.Server.AsyncHandlerInvokeArgs.SaveStatusSupernikaPushNotificationInvokeArgs args)
    {
      var logPrefix = string.Format("SaveStatusSupernikaPushNotification. RetryIteration: {0}, RXIncomingMessageId: {1}, MBMessageId: {2}.", args.RetryIteration, args.RXIncomingMessageId, args.MBMessageId);
      
      Logger.DebugFormat("{0} Старт асинхронного обработчика. Параметры: DateTime: {1}, Status: {2}, SelectedButton: {3}, Comment: {4}, ErrorMessage: {5}.",
                         logPrefix, args.DateTime, args.Status, args.SelectedButton, args.Comment, args.ErrorMessage);
      
      // Параметры подключения к базе данных.
      var server = NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBServer.Guid);
      var port = NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBPort.Guid);
      var database = NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBName.Guid);
      var user = NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBUserName.Guid);
      var password = NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBPassword.Guid);
      if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
      {
        Logger.ErrorFormat("{0} Не заполнены константы для строки подключения к БД MessageBroker.", logPrefix);
        args.Retry = true;
        Logger.DebugFormat("{0} Завершение асинхронного обработчика.", logPrefix);
        return;
      }
      
      // Преобразовать время к UTC+0, т.к. npgsql.dll поддерживает вставку в timestamp with time zone только при UTC+0.
      DateTime postedUtc0 = args.DateTime - Calendar.UtcOffset;
      var connectionString = string.Format("server={0};port={1};database={2};user id={3};password={4}", server, port, database, user, password);
      var result = IsolatedFunctions.SaveStatusInMB.SaveStatusSupernikaPushNotification(Queries.Module.SaveStatusSupernikaPushNotification, connectionString,
                                                                                        args.RXIncomingMessageId, args.MBMessageId, postedUtc0, 
                                                                                        args.Status, args.SelectedButton, args.Comment, args.ErrorMessage);
      
      if(result)
        Logger.DebugFormat("{0} Данные успешно вставлены.", logPrefix);
      else
      {
        Logger.ErrorFormat("{0} Произошла ошибка при вставке данных.", logPrefix);
        args.Retry = true;
      }
      
      Logger.DebugFormat("{0} Завершение асинхронного обработчика.", logPrefix);
    }
    
    /// <summary>
    /// Создать и отправить исходящие сообщения.
    /// </summary>
    /// <param name="args">EntitiesIds - список id сущностей.</param>
    /// <param name="args">FlowGuid - guid потока.</param>
    /// <param name="args">VersionIds - версии документов.</param>
    public virtual void CreateOutgoingMessages(DirRX.NNIntegration.Server.AsyncHandlerInvokeArgs.CreateOutgoingMessagesInvokeArgs args)
    {
      var isHaveVersionsDocs = !string.IsNullOrEmpty(args.VersionsIds);
      
      var logPrefix = string.Format("CreateOutgoingMessages. RetryIteration: {0}. FlowGuid: {1}. EntitesIds: {2}", args.RetryIteration, args.FlowGuid, args.EntitiesIds);
      if (isHaveVersionsDocs)
        logPrefix += string.Format(". VersionsIds: {0}", args.VersionsIds);
      var allEntitiesIds = args.EntitiesIds.Split(Constants.Module.ListIdsDelimiter).ToList();

      var allVersionIds = isHaveVersionsDocs ? args.VersionsIds.Split(Constants.Module.ListIdsDelimiter).ToList() : new List<string>();
      var processedEntitesIds = new List<string> {};
      var errorEntitesIds = new List<string> {};
      var processedVersionsIds = new List<string> {};
      var errorVersionsIds = new List<string> {};
      var countCreateOutgoingMessageInIteration = NNCommons.PublicFunctions.Module.GetIntValue(NNIntegration.PublicConstants.Module.ConstantsSetting.CountCreateOutgoingMessageInIteration.Guid);
      
      Logger.DebugFormat("{0}. Количество EntitiesIds: {1}. Старт асинхронного обработчика \"Создать и отправить исходящие сообщения\".", logPrefix, allEntitiesIds.Count);
      
      var flow = ExternalSystemFlows.GetAll().FirstOrDefault(f => f.FlowGuid == args.FlowGuid);
      if (flow == null)
      {
        Logger.ErrorFormat("{0}. Количество EntitiesIds: {1}. Поток внешней системы с заданным guid не найден.", logPrefix, allEntitiesIds.Count);
        args.Retry = false;
      }
      
      var processingCount = countCreateOutgoingMessageInIteration >= allEntitiesIds.Count ? allEntitiesIds.Count : countCreateOutgoingMessageInIteration;
      for (int i = 0; i <= processingCount-1; i++)
      {
        Logger.DebugFormat("{0}. entityId: {1}. Создание записи справочника Исходящие информационные сообщения.", logPrefix, allEntitiesIds[i]);
        var versionId = isHaveVersionsDocs ? allVersionIds[i] : string.Empty;
        var resultCreateMessage = NNIntegration.Functions.Module.CreateOutgoingMessage(flow, allEntitiesIds[i], versionId);
        if (string.IsNullOrEmpty(resultCreateMessage))
        {
          Logger.DebugFormat("{0}. entityId: {1}. Создание записи успешно завершено.", logPrefix, allEntitiesIds[i]);
          processedEntitesIds.Add(allEntitiesIds[i]);
          if (isHaveVersionsDocs)
            processedVersionsIds.Add(versionId);
        }
        else
        {
          Logger.ErrorFormat("{0}. entityId: {1}. При создании записи произошла ошибка: {2}. Запись будет обработана в рамках следующей итерации.", logPrefix, allEntitiesIds[i], resultCreateMessage);
          errorEntitesIds.Add(allEntitiesIds[i]);
          if (isHaveVersionsDocs)
            errorVersionsIds.Add(versionId);
        }
      }
      
      var remainingEntityIds = allEntitiesIds.Except(processedEntitesIds);
      Logger.DebugFormat("{0}. Асинхронный обработчик завершил свое выполнение. Обработано {1} записей, произошли ошибки у {2} записей, {3} записей будет обработано в следующей итерации.",
                         logPrefix, processedEntitesIds.Count, errorEntitesIds.Count, remainingEntityIds.Count());
      args.Retry = remainingEntityIds.Any();
      
      if(args.Retry)
      {
        args.EntitiesIds = string.Join(Constants.Module.ListIdsDelimiter.ToString(), remainingEntityIds);
        if (isHaveVersionsDocs)
          args.VersionsIds = string.Join(Constants.Module.ListIdsDelimiter.ToString(), allVersionIds.Except(processedVersionsIds));
      }
    }
    
    /// <summary>
    /// Отправка исходящего сообщения.
    /// </summary>
    /// <param name="args">OutgoingMessageId - Id записи Исходящее информационное сообщение.</param>
    public virtual void SendOutgoingMessage(DirRX.NNIntegration.Server.AsyncHandlerInvokeArgs.SendOutgoingMessageInvokeArgs args)
    {
      var logPrefix = string.Format("SendOutgoingMessage. OutgoingMessageId: {0}. RetryIteration: {1}", args.OutgoingMessageId, args.RetryIteration);
      Logger.DebugFormat("{0}. Старт Асинхронного обработчика \"Отправить исходящее сообщение\".", logPrefix);
      
      var outgoingMessageId = args.OutgoingMessageId;
      var outgoingMessage = OutgoingMessages.GetAll().FirstOrDefault(m => m.Id == outgoingMessageId);
      if (outgoingMessage == null)
      {
        Logger.ErrorFormat("{0}. Исходящее сообщение не найдено.", logPrefix);
        args.Retry = false;
        return;
      }
      logPrefix += string.Format(". Guid сообщения: {0}", outgoingMessage.GuidMessage);
      if (outgoingMessage.StatusMessage != NNIntegration.OutgoingMessage.StatusMessage.Created && outgoingMessage.StatusMessage != NNIntegration.OutgoingMessage.StatusMessage.Error)
      {
        Logger.ErrorFormat("{0}. У Исходящего сообщения статус не \"Создано\" и не \"Ошибка\". Обработка сообщения не требуется.", logPrefix);
        args.Retry = false;
        return;
      }
      
      var isLocked = Locks.TryLock(outgoingMessage);
      if (!Locks.GetLockInfo(outgoingMessage).IsLockedByMe && !isLocked)
      {
        var lockInfo = Locks.GetLockInfo(outgoingMessage);
        Logger.DebugFormat("{0}. Не удалось установить блокировку сообщения. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}. " +
                           "Будет запущена повторная обработка сообщения.",
                           logPrefix, lockInfo.OwnerName, lockInfo.LockTime);
        args.Retry = true;
        return;
      }
      
      outgoingMessage.ProcessingDate = Calendar.Now;
      
      var textMessage = PublicFunctions.OutgoingMessage.GetOutgoingMessageText(outgoingMessage);
      var isGenerateText = false;
      
      if (string.IsNullOrEmpty(textMessage))
      {
        // Сформировать текст запроса.
        var resultGetMessageText = NNIntegration.Functions.Module.GetMessageText(outgoingMessage.ExternalSystemFlow.FlowGuid, outgoingMessage.EntityId.Value, outgoingMessage.GuidMessage);
        
        if (!string.IsNullOrEmpty(resultGetMessageText.ErrorText))
        {
          Logger.ErrorFormat("{0}. При генерации текста сообщения произошла ошибка: {1}.", logPrefix, resultGetMessageText.ErrorText);
          var textNotification = string.Format("{0}: {1}", Resources.NotifyAdminGetTextOutgoingMessageErrorText, resultGetMessageText.ErrorText);
          var errorSendNotifyText = NNIntegration.Functions.Module.SendNotificationAboutErrors(Resources.NotifyAdminGetTextOutgoingMessageErrorSubject, textNotification, false, outgoingMessage);
          if (string.IsNullOrEmpty(errorSendNotifyText))
            Logger.DebugFormat("{0}. Уведомление об ошибке успешно отправлено администратору.", logPrefix);
          else
            Logger.ErrorFormat("{0}. {1}.", logPrefix, errorSendNotifyText);

          outgoingMessage.StatusMessage = NNIntegration.OutgoingMessage.StatusMessage.Error;
          outgoingMessage.ErrorText = resultGetMessageText.ErrorText;
          outgoingMessage.Save();
          if (Locks.GetLockInfo(outgoingMessage).IsLockedByMe)
            Locks.Unlock(outgoingMessage);
          
          args.Retry = false;
          return;
        }
        isGenerateText = true;
        textMessage = resultGetMessageText.RequestText;
      }

      // Отправить исходящее сообщение.
      try
      {
        var needLogBody = NNCommons.PublicFunctions.Module.Remote.GetBoolValue(PublicConstants.Module.ConstantsSetting.NeedLogKSIPMessagesDetails.Guid);
        if (needLogBody == true && !string.IsNullOrEmpty(textMessage))
          Logger.DebugFormat("{0}. TextMessage: {1}", logPrefix, textMessage);
        
        var resultSendingMessage = NNIntegration.Functions.Module.SendMessageToExternalSystem(outgoingMessage, textMessage);
        bool isNeedRetry = true;
        if (string.IsNullOrEmpty(resultSendingMessage.ErrorText))
        {
          Logger.DebugFormat("{0}. Исходящее сообщение успешно отправлено.", logPrefix);
          outgoingMessage.AnswerText = resultSendingMessage.AnswerText;
          var answerStruct = AnswerOnOutgoingMessage.Create();
          
          // Если в результате пришёл положительны ответ без тела запроса, то устанавливаем статус - успешно,
          // иначе десериализуем тело запроса в структуру и определяем статус по ответу.
          if (string.IsNullOrEmpty(resultSendingMessage.AnswerText))
            answerStruct.Result = Constants.Module.Request.ResultCode.Successfully;
          else
          {
            try
            {
              answerStruct = IsolatedFunctions.Convert.DeserializeInStruct(resultSendingMessage.AnswerText, AnswerOnOutgoingMessage.Create());
            }
            catch (Exception ex)
            {
              answerStruct.Result = Constants.Module.Request.ResultCode.Error;
              var error = "Получен ответ на исходящее сообщение от внешней системы, но не удалось обработать текст ответа";
              Logger.ErrorFormat("{0}. {1}. Текст ответа: {2}. Ошибка: {3}. StackTrace: {4}.", logPrefix, error, resultSendingMessage.AnswerText, ex.Message, ex.StackTrace);
              answerStruct.ErrorText = error;
            }
          }
          
          if (answerStruct.Result == Constants.Module.Request.ResultCode.Successfully)
          {
            outgoingMessage.StatusMessage = NNIntegration.OutgoingMessage.StatusMessage.Sended;
            outgoingMessage.ProcessingDate = Calendar.Now;
            if (!string.IsNullOrEmpty(outgoingMessage.ErrorText))
              outgoingMessage.ErrorText = string.Empty;
          }
          else if (answerStruct.Result == Constants.Module.Request.ResultCode.Error || answerStruct.Result == Constants.Module.Request.ResultCode.Block)
          {
            resultSendingMessage.ErrorText = answerStruct.ErrorText;
            if (answerStruct.Result == Constants.Module.Request.ResultCode.Block)
              isNeedRetry = false;
          }
          outgoingMessage.StatusMessage = NNIntegration.OutgoingMessage.StatusMessage.Sended;
          outgoingMessage.SendingDate = Calendar.Now;
        }
        if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
        {
          Logger.ErrorFormat("{0}. При отправке исходящего сообщения произошла ошибка: {1}. Будет запущена повторная обработка сообщения.", logPrefix, resultSendingMessage.ErrorText);
          var textNotification = string.Format("{0}: {1}", Resources.NotifyResponsibleSendOutgoingMessageErrorText, resultSendingMessage.ErrorText);
          var errorSendNotifyText = NNIntegration.Functions.Module.SendNotificationAboutErrors(Resources.NotifyResponsibleSendOutgoingMessageErrorSubject, textNotification, true, outgoingMessage);
          if (string.IsNullOrEmpty(errorSendNotifyText))
            Logger.DebugFormat("{0}. Уведомление ответственному за ошибки успешно отправлено.", logPrefix);
          else
            Logger.ErrorFormat("{0}. {1}.", logPrefix, errorSendNotifyText);
          args.Retry = isNeedRetry;
          outgoingMessage.StatusMessage = NNIntegration.OutgoingMessage.StatusMessage.Error;
          outgoingMessage.ErrorText = resultSendingMessage.ErrorText;
        }
      }
      catch (Exception e)
      {
        Logger.ErrorFormat("{0}. При отправке исходящего сообщения произошла ошибка. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, e.Message, e.StackTrace);
        return;
      }
      finally
      {
        if (Locks.GetLockInfo(outgoingMessage).IsLockedByMe)
          Locks.Unlock(outgoingMessage);
      }
      
      if (isGenerateText && (args.Retry || (!args.Retry && outgoingMessage.ExternalSystemFlow.ClearAfterProcessing == false)))
        PublicFunctions.OutgoingMessage.SetOutgoingMessageTextAndSaveRecord(outgoingMessage, textMessage);
      else if (outgoingMessage.ExternalSystemFlow.ClearAfterProcessing == true && !args.Retry)
        PublicFunctions.OutgoingMessage.SetOutgoingMessageTextAndSaveRecord(outgoingMessage, string.Empty);
      else
        outgoingMessage.Save();
      
      Logger.DebugFormat("{0}. Завершение Асинхронного обработчика \"Отправить исходящее сообщение\".", logPrefix);
    }
    
    /// <summary>
    /// Обработка входящих сообщений.
    /// </summary>
    /// <param name="args">Аргументы.</param>
    public virtual void ProcessingIncomingMessages(DirRX.NNIntegration.Server.AsyncHandlerInvokeArgs.ProcessingIncomingMessagesInvokeArgs args)
    {
      var logPrefix = string.Format("ProcessingIncomingMessages. IncomingMessageId: {0}. RetryIteration: {1}.", args.IncomingMessageId, args.RetryIteration);
      Logger.DebugFormat("{0}. Старт асинхронного обработчика \"Обработка входящих интеграционных сообщений\".", logPrefix);
      
      // Поиск входящего сообщения.
      var incomingMessage = IncomingMessages.GetAll(m => m.Id == args.IncomingMessageId).FirstOrDefault();
      if (incomingMessage == null)
      {
        Logger.ErrorFormat("{0}. Входящее сообщение не найдено. Завершение асинхронного обработчика \"Обработка входящих интеграционных сообщений\".", logPrefix);
        args.Retry = false;
        return;
      }
      
      logPrefix += string.Format(". flowGuid: {0}", incomingMessage.ExternalSystemFlow.FlowGuid);
      
      // Проверить статус обработки сообщения.
      if (incomingMessage.StatusMessage != NNIntegration.IncomingMessage.StatusMessage.NeedProcessing)
      {
        Logger.ErrorFormat("{0}. Сообщение не требуется обрабатывать. Статус сообщения: {1}. Завершение асинхронного обработчика \"Обработка входящих интеграционных сообщений\".",
                           logPrefix, IncomingMessages.Info.Properties.StatusMessage.GetLocalizedValue(incomingMessage.StatusMessage));
        args.Retry = false;
        return;
      }
      
      // Установить блокировку на запись.
      var isLocked = Locks.TryLock(incomingMessage);
      if (!Locks.GetLockInfo(incomingMessage).IsLockedByMe && !isLocked)
      {
        var lockInfo = Locks.GetLockInfo(incomingMessage);
        Logger.DebugFormat("{0}. Не удалось установить блокировку. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}. Будет запущена повторная обработка входящего сообщения.",
                           logPrefix, lockInfo.OwnerName, lockInfo.LockTime);
        args.Retry = true;
        return;
      }
      
      var error = string.Empty;
      
      // Связывание входящего сообщения с исходящим сообщением.
      var outgoingMessage = OutgoingMessages.Null;
      if (!string.IsNullOrEmpty(incomingMessage.GuidOutgoingMessage))
      {
        outgoingMessage = OutgoingMessages.GetAll(x => x.GuidMessage == incomingMessage.GuidOutgoingMessage).FirstOrDefault();
        if (outgoingMessage == null)
        {
          error = string.Format("Не найдено исходящее сообщение для входящего по guid: {0}.", incomingMessage.GuidOutgoingMessage);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, error);
        }
      }
      
      // Вызов case-функции для функции входящего потока.
      if (string.IsNullOrEmpty(error))
      {
        var result = Functions.Module.ProcessIncomingMessage(incomingMessage.ExternalSystemFlow.FlowGuid, incomingMessage.Id);
        
        // Повтор выполнения АО.
        if (result.Retry && !PublicFunctions.Module.IsLastAsyncHandlerRetry(args.RetryIteration))
        {
          args.Retry = true;
          Logger.DebugFormat("{0}. Будет запущена повторная обработка входящего сообщения. Ошибка: {1}.", logPrefix, result.ErrorText);
          Locks.Unlock(incomingMessage);
          return;
        }
        error = result.ErrorText;
        if (result.EntityId != null)
          incomingMessage.EntityId = result.EntityId;
      }
      
      incomingMessage.ProcessingDate = Calendar.Now;
      
      // Обработка ошибок.
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("{0}. При обработке входящего сообщения произошла ошибка: {1}.", logPrefix, error);
        incomingMessage.StatusMessage = NNIntegration.IncomingMessage.StatusMessage.Error;
        incomingMessage.ErrorText = error;
        var errorSendNotifyText = NNIntegration.Functions.Module.SendNotificationAboutErrors(Resources.NotifyProcessingIncomingMessageErrorSubject, error, true, incomingMessage);
        if (string.IsNullOrEmpty(errorSendNotifyText))
          Logger.DebugFormat("{0}. Уведомление ответственному за ошибки успешно отправлено.", logPrefix);
        else
          Logger.ErrorFormat("{0}. {1}.", logPrefix, errorSendNotifyText);
      }
      else
        incomingMessage.StatusMessage = NNIntegration.IncomingMessage.StatusMessage.Processed;
      
      // Очистка сообщения/сообщений после обработки.
      var isCleared = false;
      if (incomingMessage.ExternalSystemFlow.ClearAfterProcessing == true)
      {
        var message = string.Format("{0}. Для потока необходимо очищать сообщения после обработки. Будет произведено удаление сообщения/сообщений.", logPrefix);
        message += string.Format(" Guid входящего сообщения: {0}.", incomingMessage.GuidIncomingMessage);
        if (outgoingMessage != null)
          message += string.Format(" Guid исходящего сообщения: {0}.", outgoingMessage.GuidMessage);
        Logger.Debug(message);
        isCleared = Transactions.Execute(() =>
                                         {
                                           // Сессия для атомарности операции.
                                           using (var session = Sungero.Domain.Session.CreateIsolatedSession())
                                           {
                                             if (outgoingMessage != null)
                                               session.Delete(outgoingMessage);
                                             session.Delete(incomingMessage);
                                             session.SubmitChanges();
                                           }
                                         });
        if (isCleared)
          Logger.DebugFormat("{0}. Сообщения успешно удалены.", logPrefix);
        else
          Logger.ErrorFormat("{0}. Не удалось удалить сообщения.", logPrefix);
      }
      
      // Сохранение входящего сообщения.
      if (!isCleared)
      {
        try
        {
          incomingMessage.Save();
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. Не удалось сохранить входящее сообщение. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        }
        finally
        {
          if(Locks.GetLockInfo(incomingMessage).IsLockedByMe)
            Locks.Unlock(incomingMessage);
        }
      }
      Logger.DebugFormat("{0}. Завершение асинхронного обработчика \"Обработка входящих интеграционных сообщений\".", logPrefix);
    }

    #region Удаление учетныных записей у совместителей.

    /// <summary>
    /// Удаление УЗ у совместителей.
    /// </summary>
    /// <param name="args">ID сотрудника с основным местом работы.</param>
    public virtual void ClearAccountInPartTimersAsync(DirRX.NNIntegration.Server.AsyncHandlerInvokeArgs.ClearAccountInPartTimersAsyncInvokeArgs args)
    {
      var logPrefix = string.Format("ClearAccountInPartTimersAsync. ИД сотрудника: {0}", args.MainEmployeeId);
      Logger.DebugFormat("{0}. Запущено удаление УЗ у совместителей сотрудника.", logPrefix);
      var mainEmployee = DirRX.OverrideSolution.Employees.GetAll(c => c.Id == args.MainEmployeeId).FirstOrDefault();

      if (mainEmployee == null)
      {
        Logger.ErrorFormat("{0}. Сотрудник не найден.", logPrefix);
        args.Retry = false;
        return;
      }

      if (mainEmployee.Person == null)
      {
        Logger.ErrorFormat("ClearAccountInPartTimersAsync. Сотрудник ID: '{0}' не имеет персоны", args.MainEmployeeId);
        args.Retry = false;
        return;
      }

      var employees = DirRX.OverrideSolution.Employees.GetAll(c =>
                                                              c.Id != mainEmployee.Id &&
                                                              c.Person.Id == mainEmployee.Person.Id &&
                                                              c.Login != null &&
                                                              c.EmploymentType.HasValue &&
                                                              c.EmploymentType != EmploymentType.MainPlace);
      if (!employees.Any())
      {
        Logger.DebugFormat("{0}. Не найдены совместители с УЗ для отчистки.", logPrefix);
        args.Retry = false;
        return;
      }
      
      foreach (var employee in employees)
      {
        if (DirRX.OverrideSolution.PublicFunctions.Employee.ClearAccountInEmployee(employee))
          continue;
        
        Logger.ErrorFormat("{0}. Не удалось отчистить УЗ у совместителя с ИД = {1}.", logPrefix, employee.Id);
        args.Retry = true;
        return;
      }
      
      Logger.DebugFormat("{0}. Отчистка успешно завершена.", logPrefix);
    }

    #endregion
    
  }
}