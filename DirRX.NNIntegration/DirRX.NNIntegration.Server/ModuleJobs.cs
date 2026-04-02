using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;

namespace DirRX.NNIntegration.Server
{
  public partial class ModuleJobs
  {

    /// <summary>
    /// Отправка сообщений из справочника Push-уведомления в Супернику.
    /// </summary>
    public virtual void SupernikaPushNotificationsSending()
    {
      var logPrefix = "SupernikaPushNotificationsSending";
      Logger.DebugFormat("{0}. Старт фонового процесса.", logPrefix);
      
      // Получить записи справочника "Push-уведомления в Супернику".
      var countToSendConstant = NNCommons.PublicFunctions.Module.Remote.GetConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsSendMessageCount.Guid);
      var countToSend = Constants.Module.ConstantsDefaultValues.SupernikaPushNotificationsSendMessageCount;
      
      if(countToSendConstant != null && (countToSendConstant.IntegerValue == null || countToSendConstant.IntegerValue.Value == 0))
        countToSend = countToSendConstant.IntegerValue.Value;
      
      var notifications = NNIntegration.SupernikaPushNotifications.GetAll(x => x.ProcessingStatus == NNIntegration.SupernikaPushNotification.ProcessingStatus.QueuedForMB && x.PlannedShipDate <= Calendar.Now).Take(countToSend);
      var count = notifications.Count();
      Logger.DebugFormat("{0}. Записей для отправки: {1}.", logPrefix, count);
      
      if(count > 0)
      {
        foreach(var notification in notifications)
        {
          Logger.DebugFormat("{0}. ИД записи {1}. Начало обработки записи.", logPrefix, notification.Id);
          
          // Проверить блокировку записи. Если запись заблокирована тогда отложить ее обработку.
          var isLock = Locks.TryLock(notification);
          if(isLock)
          {
            // Проверить прочитанность уведомления / выполнение задания.
            var isCompleted = DirRX.NNIntegration.Functions.Module.VerifyTaskCompletion(notification.EntityId.Value, notification.IsNotification == true);
            
            // Если задание выполненно или уведомление прочитано, тогда не нужно отправлять уведомление. Запись переводим в статус Отменено.
            if(isCompleted == true)
            {
              Logger.DebugFormat("{0}. ИД записи {1}. Задание выполенно, либо уведомление прочитано. Отправка уведомления не будет производиться.", logPrefix, notification.Id);
              notification.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Canceled;
              notification.Save();
              Logger.DebugFormat("{0}. ИД записи {1}. Обновлен статус на: {2}.", logPrefix, notification.Id, NNIntegration.SupernikaPushNotifications.Info.Properties.ProcessingStatus.GetLocalizedValue(NNIntegration.SupernikaPushNotification.ProcessingStatus.Canceled));
              
            }
            else
            {
              // Содержание уведомления для Суперники (ПМД).
              var notificationParams = DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.GetSupernikaPushContent(notification.EntityId.Value, notification.IsNotification.Value);
              
              // Заполнение атрибута properties для отображения push-сообщения на мобильных телефонах.
              var properties = new Dictionary<string, string>();
              properties.Add(NNIntegration.PublicConstants.Module.MessageBrokerNotificationParameters.PushMessageProperties.PushTitleKey, notificationParams.Subject);
              properties.Add(NNIntegration.PublicConstants.Module.MessageBrokerNotificationParameters.PushMessageProperties.PushContentKey,
                             DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.ExtractTextFromHtml(notificationParams.Text));
              
              // Вложения уведомления.
              var attachments = new Dictionary<string, long>();
              
              
              Logger.DebugFormat("{0} Уведомлять по email: {1}.", logPrefix, notificationParams.NeedEmailNotification);
              
              // Дублирование сообщения на email.
              if (notificationParams.NeedEmailNotification && !string.IsNullOrEmpty(notification.Employee.Email))
              {
                Logger.DebugFormat("{0} Уведомление будет продублировано на эл. почту исполнителя.", logPrefix);
                
                var objectTypeName = notification.IsNotification == true ? 
                  HRPro.HRDocFlow.PublicConstants.Module.ObjectCardTypeNames.HRNotice : 
                  HRPro.HRDocFlow.PublicConstants.Module.ObjectCardTypeNames.DocumentApprovalAssignment;
                
                // Url-ссылка на задание/уведомление.
                var url = DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.CreateHyperLinkToSelfOffice(HRPro.HRProSolution.PublicConstants.Module.TargetNameSpace, 
                                                                                                                 DirRX.EssPlatform.PublicConstants.Module.SelfOfficeObjectTypes.WorkItems, 
                                                                                                                 objectTypeName, 
                                                                                                                 notification.EntityId.ToString());
                var textForHyperlink = NNCommons.PublicFunctions.Module.GetStringValue(NNIntegration.PublicConstants.Module.ConstantsSetting.TextForHyperlink.Guid);
                var htmlUrl = string.Format("<a href=\"{0}\">{1}</a>", url, textForHyperlink);
                var emailBody = string.Format("{0}{1}{2}", notificationParams.Text, Environment.NewLine, htmlUrl);
                var error = DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.SendSupernikaPushOnEmail(notification.Employee.Email, notificationParams.Subject, emailBody);
                if (!string.IsNullOrEmpty(error))
                  Logger.ErrorFormat("{0} Ошибка при дублировании уведомления на эл. почту сотрудника. Текст ошибки: {1}.", logPrefix, error);
              }
              
              // Передать сообщение в Message Broker.
              var messageGuid = DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.SendNotificationToSupernika(notification.EntityId.Value, notification.Employee, notificationParams.Subject, notification.MessageText, 0, properties, notificationParams.Actions, attachments);
              
              if(!string.IsNullOrWhiteSpace(messageGuid))
              {
                Logger.DebugFormat("{0}. ИД записи {1}. Запись передана на Message Broker. GUID сообщения: {2}", logPrefix, notification.Id, messageGuid);
                notification.NotificationGuid = messageGuid;
                notification.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Sended;
                notification.SendingDate = Calendar.Now;
                notification.Save();
                Logger.DebugFormat("{0}. ИД записи {1}. В запись справочника записан GUID.", logPrefix, notification.Id);
              }
            }
          }
          else
          {
            var lockInfo = Locks.GetLockInfo(notification);
            Logger.DebugFormat("{0}. ИД записи {1}. Запись заблокирована {2} в {3}. Запись будет пропущена и обработана в следующий раз.", logPrefix, notification.Id, lockInfo.OwnerName, lockInfo.LockTime);
          }
          
          
          Logger.DebugFormat("{0}. ИД записи {1}. Окончание обработки записи.", logPrefix, notification.Id);
        }
      }
      
      Logger.DebugFormat("{0}. Завершение работы фонового процесса.", logPrefix);
    }

    /// <summary>
    /// Очистка справочника Push-уведомления в Супернику.
    /// </summary>
    public virtual void SupernikaPushNotificationsCleaning()
    {
      var logPrefix = "SupernikaPushNotificationsCleaning";
      Logger.DebugFormat("{0}. Старт фонового процесса.", logPrefix);
      
      // Количество дней для хранения push-уведомлений.
      var daysCount = Constants.Module.ConstantsDefaultValues.SupernikaPushNotificationsCleaningJobDaysDefault;
      var days = DirRX.NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.SupernikaPushNotificationsCleaningJobDays.Guid);
      if (days == null || days.Value < 0)
        Logger.ErrorFormat("{0}. Не удалось определить количество дней, по истечению которых необходо удалить push-уведомление. Заполните прикладную константу: {1}. Текущее значение: {2}. Будет использоваться значение по умолчанию: {3}.", logPrefix, Constants.Module.ConstantsSetting.SupernikaPushNotificationsCleaningJobDays.Name, days, daysCount);
      else
        daysCount = days.Value;
      var overdueDate = Calendar.Now.AddDays(-daysCount);
      
      // Максимальное число записей, которые удаляются за один запуск ФП.
      var recordsNumber = Constants.Module.ConstantsDefaultValues.SupernikaNotificationsCleaningNumberDefault;
      var recordsNumberConst = DirRX.NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.SupernikaNotificationsCleaningNumber.Guid);
      if (recordsNumberConst == null || recordsNumberConst.Value < 0)
        Logger.ErrorFormat("{0}. Не удалось определить максимальное число записей, которые удаляются за один запуск ФП. Заполните прикладную константу: {1}. Текущее значение: {2}. Будет использоваться значение по умолчанию: {3}.", logPrefix, Constants.Module.ConstantsSetting.SupernikaNotificationsCleaningNumber.Name, days, recordsNumber);
      else
        recordsNumber = recordsNumberConst.Value;
      
      Logger.DebugFormat("{0}. Срок хранения push-уведомления: {1}. Будут удалены уведомления, у которых дата обработки меньше: {2}.", logPrefix, daysCount, overdueDate.ToString("dd.MM.yyyy"));
      try
      {
        var pushes = SupernikaPushNotifications.GetAll(x => x.ReceivedDate < overdueDate || (x.Created != null && x.Created < overdueDate));
        if (pushes.Any())
        {
          Logger.DebugFormat("{0}. Всего необходимо удалить push-уведомлений: {1}.", logPrefix, pushes.Count());
          
          // Удаление push-сообщениями порциями.
          Functions.Module.SupernikaNotificationsCleaning(logPrefix, pushes, recordsNumber);
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при выполненнии фонового процесса. Текст ошибки: {1}, StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
      }
      
      Logger.DebugFormat("{0}. Завершение работы фонового процесса.", logPrefix);
    }

    /// <summary>
    /// Выполнение заданий из push-уведомлений в Супернику.
    /// </summary>
    public virtual void CompleteSupernikaPushNotificationJob()
    {
      var logPrefix = "CompleteSupernikaPushNotification";
      Logger.DebugFormat("{0}. Старт фонового процесса.", logPrefix);
      
      var notifications = SupernikaPushNotifications.GetAll(x => x.ProcessingStatus == NNIntegration.SupernikaPushNotification.ProcessingStatus.Received);
      Logger.DebugFormat("{0}. Всего необходимо обработать push-уведомлений: {1}.", logPrefix, notifications.Count());
      
      #region Обработка заданий.
      foreach (var push in notifications.Where(x => x.IsNotification != true))
      {
        var assignment = Sungero.Workflow.Assignments.GetAll().Where(x => x.Id == push.EntityId).FirstOrDefault();
        if (assignment == null)
        {
          Logger.DebugFormat("{0}. ИД записи: {1}. Не найдено задание по ИД: {2}.", logPrefix, push.Id, push.EntityId);
          continue;
        }
        
        // Установить блокировку на push - уведомление.
        var isLocked = Locks.TryLock(push);
        if (!Locks.GetLockInfo(push).IsLockedByMe && !isLocked)
        {
          var lockInfo = Locks.GetLockInfo(push);
          Logger.DebugFormat("{0}. ИД записи: {1}. Не удалось установить блокировку. Push-уведомление заблокировано пользователем: {2}. Время установки блокировки: {3}.",
                             logPrefix, push.Id, lockInfo.OwnerName, lockInfo.LockTime);
          continue;
        }
        
        var assignmentIsLocked = false;
        try
        {
          // Задание - выполнено.
          if (assignment.Status != Sungero.Workflow.Assignment.Status.InProcess)
          {
            push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Processed;
            Logger.DebugFormat("{0}. ИД записи: {1}. Задание находится в состоянии \"{2}\". Для push-уведомления установлен статус \"Обработано\".", logPrefix, push.Id,
                               Sungero.Workflow.Assignments.Info.Properties.Status.GetLocalizedValue(assignment.Status));
          }
          // Задание - в работе.
          else
          {
            // Установить блокировку на задание RX.
            assignmentIsLocked = Locks.TryLock(assignment);
            if (!Locks.GetLockInfo(assignment).IsLockedByMe && !assignmentIsLocked)
            {
              Locks.Unlock(push);
              var lockInfo = Locks.GetLockInfo(assignment);
              Logger.DebugFormat("{0}. ИД записи: {1}. Ид задания: {2}. Не удалось установить блокировку. Задание заблокировано пользователем: {3}. Время установки блокировки: {4}.",
                                 logPrefix, push.Id, assignment.Id, lockInfo.OwnerName, lockInfo.LockTime);
              continue;
            }
            
            var action = NNIntegration.Functions.SupernikaPushNotificationAction.GetAction(assignment, push.ActionName);
            if (action == null)
            {
              var error = "Не удалось сопоставить нажатое действие в Супернике с результатом выполнения в RX";
              Logger.ErrorFormat("{0}. ИД записи: {1}. {2}.", logPrefix, push.Id, error);
              push.ErrorText = error;
              push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Error;
              continue;
            }
            else
              Logger.DebugFormat("{0}. ИД записи: {1}. Выполнено действие {2}: Суперника: {3} - RX: {4}.",
                                 logPrefix, push.Id, action.Name, action.ActionType, action.ResultRx);
            
            if (!string.IsNullOrEmpty(push.Comment))
              assignment.ActiveText = push.Comment;
            
            var completeError = NNIntegration.Functions.SupernikaPushNotificationAction.CompleteAssignment(action, assignment);
            if (!string.IsNullOrWhiteSpace(completeError))
            {
              Logger.ErrorFormat("{0}. ИД записи: {1}. {2}.", logPrefix, push.Id, completeError);
              push.ErrorText = completeError;
              push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Error;
            }
            else
            {
              Logger.DebugFormat("{0}. ИД записи: {1}. Задание выполнено. ИД задания: {2}.", logPrefix, push.Id, assignment.Id);
              push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Processed;
            }
          }
          
          Logger.DebugFormat("{0}. ИД записи: {1}. ИД задания: {2}. Состояние задания: {3}. Статус push-уведомления: {4}.", logPrefix, push.Id, assignment.Id,
                             Sungero.Workflow.Assignments.Info.Properties.Status.GetLocalizedValue(assignment.Status),
                             DirRX.NNIntegration.SupernikaPushNotifications.Info.Properties.ProcessingStatus.GetLocalizedValue(push.ProcessingStatus));
        }
        catch (Exception ex)
        {
          var error = string.Format("Ошибка при выполнении задания из ответа от push-уведомления. Текст ошибки: {0}", ex.Message);
          Logger.ErrorFormat("{0}. ИД записи: {1}. {2}. Ошибка: {3}. StackTrace: {4}.", logPrefix, push.Id, error, ex.Message, ex.StackTrace);
          push.ErrorText = error;
        }
        finally
        {
          // Сохранение push-уведомления.
          if (push.State.IsChanged)
          {
            push.ProcessingDate = Calendar.Now;
            var saveError = PublicFunctions.SupernikaPushNotification.SaveMessage(push, logPrefix);
            if (!string.IsNullOrEmpty(saveError))
              Logger.ErrorFormat("{0}. ИД записи: {1}. Ошибка при сохранении: {2}.", logPrefix, push.Id, saveError);
          }
          
          if (assignmentIsLocked)
            Locks.Unlock(assignment);
          
          Locks.Unlock(push);
        }
        
      }
      #endregion
      
      #region Обработка уведомлений.
      foreach (var push in notifications.Where(x => x.IsNotification == true))
      {
        var notice = Sungero.Workflow.Notices.GetAll().Where(x => x.Id == push.EntityId).FirstOrDefault();
        if (notice == null)
        {
          Logger.DebugFormat("{0}. ИД записи: {1}. Не найдено уведомление по ИД: {2}.", logPrefix, push.Id, push.EntityId);
          continue;
        }
        
        // Установить блокировку на push - уведомление.
        var isLocked = Locks.TryLock(push);
        if (!Locks.GetLockInfo(push).IsLockedByMe && !isLocked)
        {
          var lockInfo = Locks.GetLockInfo(push);
          Logger.DebugFormat("{0}. ИД записи: {1}. Не удалось установить блокировку. Push-уведомление заблокировано пользователем: {2}. Время установки блокировки: {3}.",
                             logPrefix, push.Id, lockInfo.OwnerName, lockInfo.LockTime);
          continue;
        }
        
        try
        {
          // Уведомление - прочитано.
          if (notice.IsRead == true)
          {
            push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Processed;
            Logger.DebugFormat("{0}. ИД записи: {1}. Уведомление уже прочитано. Для push-уведомления установлен статус \"Обработано\".", logPrefix, push.Id);
          }
          else
          {
            var action = NNIntegration.Functions.SupernikaPushNotificationAction.GetAction(notice, push.ActionName);
            if (action == null)
            {
              var error = "Не удалось сопоставить нажатое действие в Супернике с результатом выполнения в RX";
              Logger.ErrorFormat("{0}. ИД записи: {1}. {2}.", logPrefix, push.Id, error);
              push.ErrorText = error;
              push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Error;
              continue;
            }
            else
              Logger.DebugFormat("{0}. ИД записи: {1}. Выполнено действие {2}: Суперника: {3} - RX: {4}.",
                                 logPrefix, push.Id, action.Name, action.ActionType, action.ResultRx);
            
            var completeError = NNIntegration.Functions.SupernikaPushNotificationAction.CompleteAssignment(action, notice);
            if (!string.IsNullOrWhiteSpace(completeError))
            {
              Logger.ErrorFormat("{0}. ИД записи: {1}. {2}.", logPrefix, push.Id, completeError);
              push.ErrorText = completeError;
              push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Error;
            }
            else
            {
              Logger.DebugFormat("{0}. ИД записи: {1}. Уведомление обработано. ИД уведомления: {2}.", logPrefix, push.Id, notice.Id);
              push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Processed;
            }
          }
          
          Logger.DebugFormat("{0}. ИД записи: {1}. ИД уведомления: {2}. Уведомление прочитано?: {3}. Статус push-уведомления: {4}.", logPrefix, push.Id, notice.Id,
                             notice.IsRead,
                             DirRX.NNIntegration.SupernikaPushNotifications.Info.Properties.ProcessingStatus.GetLocalizedValue(push.ProcessingStatus));
        }
        catch (Exception ex)
        {
          var error = string.Format("Ошибка при выполнении задания из push-уведомления. Текст ошибки: {0}", ex.Message);
          Logger.ErrorFormat("{0}. ИД записи: {1}. {2}. Ошибка: {3}. StackTrace: {4}.", logPrefix, push.Id, error, ex.Message, ex.StackTrace);
          push.ErrorText = error;
        }
        finally
        {
          // Сохранение push-уведомления.
          if (push.State.IsChanged)
          {
            push.ProcessingDate = Calendar.Now;
            var saveError = PublicFunctions.SupernikaPushNotification.SaveMessage(push, logPrefix);
            if (!string.IsNullOrEmpty(saveError))
              Logger.ErrorFormat("{0}. ИД записи: {1}. Ошибка при сохранении push-уведомления: {2}.", logPrefix, push.Id, saveError);
          }
          Locks.Unlock(push);
        }
      }
      #endregion
      
      Logger.DebugFormat("{0}. Завершение фонового процесса.", logPrefix);
    }
    
    /// <summary>
    /// Очистка реестра интеграционных сообщений.
    /// </summary>
    public virtual void IntegrationMessagesCleaningJob()
    {
      var logPrefix = "IntegrationMessagesCleaning";
      Logger.DebugFormat("{0}. Старт фонового процесса \"Очистка реестра интеграционных сообщений\".", logPrefix);
      var daysCount = Constants.Module.ConstantsDefaultValues.IntegrationMessagesCleaningJobDays;
      var days = DirRX.NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.IntegrationMessagesCleaningJobDays.Guid);
      if (days == null || days.Value < 0)
        Logger.DebugFormat("{0}. Не удалось определить количество дней, по истечению которых необходо удалить сообщение. Заполните прикладную константу: {1}. Текущее значение: {2}.", logPrefix,
                           Constants.Module.ConstantsSetting.IntegrationMessagesCleaningJobDays.Guid, days);
      else
        daysCount = days.Value;
      
      var overdueDate = Calendar.Now.AddDays(-daysCount);
      Logger.DebugFormat("{0}. Срок хранения интеграционного сообщения: {1}. Будут удалены сообщения у которых дата обработки меньше: {2}.", logPrefix, daysCount, overdueDate.ToString("dd.MM.yyyy"));
      
      var countToDelete = NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.IntegrationMessagesCleaningJobCount.Guid) ??
        Constants.Module.ConstantsDefaultValues.IntegrationMessagesCleaningJobCount;
      
      try
      {
        // Очистка входящих сообщений.
        var incomingMessages = IncomingMessages.GetAll(x => x.ProcessingDate < overdueDate).Take(countToDelete);
        var incomingMessagesTotalCount = IncomingMessages.GetAll(x => x.ProcessingDate < overdueDate).Count();
        Logger.DebugFormat("{0}. Всего необходимо удалить входящих сообщений: {1}. В рамках итерации будет удалено не более {2} записей.", logPrefix, incomingMessagesTotalCount, countToDelete);
        
        if (incomingMessagesTotalCount > 0)
          Functions.Module.IntegrationMessagesCleaning(string.Format("{0}. {1}", logPrefix, "Входящие сообщения"), incomingMessages);
        
        // Очистка исходящих сообщений.
        var outgoingMessages = OutgoingMessages.GetAll(x => x.ProcessingDate < overdueDate).Take(countToDelete);
        var outgoingMessagesTotalCount = OutgoingMessages.GetAll(x => x.ProcessingDate < overdueDate).Count();
        Logger.DebugFormat("{0}. Всего необходимо удалить исходящих сообщений: {1}. В рамках итерации будет удалено не более {2} записей.", logPrefix, outgoingMessagesTotalCount, countToDelete);
        
        if (outgoingMessagesTotalCount > 0)
          Functions.Module.IntegrationMessagesCleaning(string.Format("{0}. {1}", logPrefix, "Исходящие сообщения"), outgoingMessages);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при выполненнии фонового процесса \"Очистка реестра интеграционных сообщений\". Текст ошибки: {1}, StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
      }
      Logger.DebugFormat("{0}. Завершение работы фонового процесса \"Очистка реестра интеграционных сообщений\".", logPrefix);
    }
    
    /// <summary>
    /// Очистка очередей организационной структуры.
    /// </summary>
    public virtual void CompanyQueuesClearingJob()
    {
      var logPrefix = "CompanyQueuesClearingJob";
      Logger.DebugFormat("{0}. Старт фонового процесса \"Очистка очередей организационной структуры\".", logPrefix);
      
      // Метод для получения ответственного из справочника "Потоки внешних систем" по guid потока.
      var responsible = Functions.Module.GetFlowResponsible(Constants.Module.ExternalSystemsFlowGuids.CompanySynchronization);
      
      try
      {
        // Очистка устаревших записей организационной структуры.
        PublicFunctions.Module.DeleteOldCompanyQueueRecords(logPrefix);
        
        // Получение информации по НОР о последних успешных передачах данных, при синхронизации организационной структуры.
        PublicFunctions.Module.SendNotificationAboutUnmodifiedBusinessUnits(logPrefix, responsible);

      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при выполненнии фонового процесса \"Очистка очередей организационной структуры\". Текст ошибки: {1}, StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
        
        if (responsible != null)
        {
          var subject = DirRX.NNIntegration.Resources.CompanyQueuesClearingJobErrorSubjectFormat(Calendar.Now);
          var notification = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, responsible);
          notification.ActiveText = DirRX.NNIntegration.Resources.CompanyQueuesClearingJobErrorTextFormat(ex.Message);
          notification.Save();
          notification.Start();
        }
      }
      Logger.DebugFormat("{0}. Конец фонового процесса \"Очистка очередей организационной структуры\".", logPrefix);
    }

    /// <summary>
    /// Обновление организационной структуры.
    /// </summary>
    public virtual void CompanySynchronizationJob()
    {
      var logPrefix = "CompanySynchronizationJob";
      Logger.DebugFormat("{0}. Старт фонового процесса \"Обновление организационной структуры\".", logPrefix);
      
      // Список записей очередей орг. структуры, в которых возникла ошибка обработки.
      var errors = new List<ICompanyQueueBase>();
      
      // Получение ответственного из справочника "Потоки внешних систем" по guid потока.
      var responsible = Functions.Module.GetFlowResponsible(Constants.Module.ExternalSystemsFlowGuids.CompanySynchronization);

      try
      {
        // Закрытие устаревших записей очередей орг. структуры.
        var oldRecords = Functions.Module.CompanyQueueOldRecordsProcessing(logPrefix);
        
        // Синхронизация подразделений.
        Functions.Module.DepartmentQueueProcessing(logPrefix, oldRecords, errors);
        
        // Синхронизация должностей.
        Functions.Module.JobTitleQueueProcessing(logPrefix, oldRecords, errors);
        
        // Синхронизация сотрудников.
        Functions.Module.EmployeeQueueProcessing(logPrefix, oldRecords, errors);
        
        // Обновление сотрудников роли "Линейные руководители".
        Functions.Module.LineManagersProccessing(logPrefix, responsible);
        
        // Обработка ошибок снхронизации.
        if (errors.Any())
          Functions.Module.SendCompanySynchronizationErrorsNotification(responsible, logPrefix, errors);
        
        // Изменение состояния записей справочника "Сотрудники". Установка статуса "Действующая".
        Functions.Module.UpdateActiveEmployeeRecords(logPrefix);
        
        // Изменение состояния записей справочника "Сотрудники". Установка статуса "Закрытая".
        Functions.Module.UpdateClosedEmployeeRecords(logPrefix);
        
        // Изменение состояния записей справочника "Подразделения". Установка статуса "Закрытая".
        Functions.Module.UpdateClosedDepartmentRecords(logPrefix);
        
        // Изменение состояния записей справочника "Должности". Установка статуса "Закрытая".
        Functions.Module.UpdateClosedJobTitleRecords(logPrefix);
        
        // Запустить обработку УЗ сотрудников.
        Functions.Module.ProcessEmployeesLogin(logPrefix);
        
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при выполненнии фонового процесса \"Обновление организационной структуры\". Текст ошибки: {1}, StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
        
        if (responsible != null)
          Functions.Module.SendNotificationToResponsible(responsible, DirRX.NNIntegration.Resources.CompanySynchronizationJobErrorSubjectFormat(Calendar.Now),
                                                         DirRX.NNIntegration.Resources.CompanySynchronizationJobErrorTextFormat(ex.Message));
      }
      Logger.DebugFormat("{0}. Конец фонового процесса \"Обновление организационной структуры\".", logPrefix);
    }

  }
}