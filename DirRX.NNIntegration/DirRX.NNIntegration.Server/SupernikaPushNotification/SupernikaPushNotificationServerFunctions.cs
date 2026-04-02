using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.SupernikaPushNotification;

namespace DirRX.NNIntegration.Server
{
  partial class SupernikaPushNotificationFunctions
  {

    /// <summary>
    /// Сохранить push-уведомление.
    /// </summary>
    /// <param name="logPrefix">Префикс лога.</param>
    /// <returns>Ошибка. Если ошибок нет, то возвращается пустая строка.</returns>
    [Public]
    public string SaveMessage(string logPrefix)
    {
      var isSaved = Transactions.Execute(() =>
                                         {
                                           _obj.Save();
                                         });
      if (isSaved)
      {
        Logger.DebugFormat("{0}. Push-уведомление успешно сохранено. ИД push-уведомления: {1}. Guid push-уведомления: {2}.",
                           logPrefix, _obj.Id, _obj.NotificationGuid);
        return string.Empty;
      }
      else
      {
        var error = string.Format("Не удалось сохранить push-уведомление. ИД push-уведомления: {1}. Guid push-уведомления: {2}.", logPrefix, _obj.Id, _obj.NotificationGuid);
        Logger.ErrorFormat("{0}. {1}", logPrefix, error);
        return error;
      }
    }

    /// <summary>
    /// Получить отправленное push-уведомление в Супернику.
    /// </summary>
    /// <param name="messageGuid">GUID push-уведомление.</param>
    /// <returns>Push-уведомление.</returns>
    [Public]
    public static ISupernikaPushNotification GetSendedMessage(string messageGuid)
    {
      return SupernikaPushNotifications.GetAll()
        .Where(x => x.NotificationGuid == messageGuid)
        .FirstOrDefault();
    }

    /// <summary>
    /// Создать запись справочника push-сообщения в Супернику.
    /// </summary>
    /// <param name="messageGuid">Guid отправленного уедомления в Message broker.</param>
    /// <param name="employeeId">Ид сотрудника.</param>
    /// <param name="assignmentId">Ид задания.</param>
    /// <param name="isNotice">Признак того, что сообщение - уведомление.</param>
    /// <returns>Push-уведомление. Если возникли ошибки - пустое значение.</returns>
    [Public]
    public static ISupernikaPushNotification CreateMessagae(string messageGuid, long employeeId, long assignmentId, bool isNotice)
    {
      var message = SupernikaPushNotifications.Create();
      message.NotificationGuid = messageGuid;
      message.EmployeeId = employeeId;
      message.EntityId = assignmentId;
      message.IsNotification = isNotice;
      message.SendingDate = Calendar.Now;
      message.ProcessingStatus = DirRX.NNIntegration.SupernikaPushNotification.ProcessingStatus.Sended;
      try
      {
        message.Save();
        Logger.DebugFormat("SupernikaPushNotification. Ид уведомления: {0}. Guid push-уведомления: {1}. Push-уведомление успешно сохранено. Отправлено в Message broker: {2}.",
                           message.Id, message.NotificationGuid, message.SendingDate.Value.ToString("dd.MM.yyyy hh:mm"));
        return message;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("SupernikaPushNotification. Guid push-уведомления: {0}. Ошибка при сохранении Push-уведомление. Отправлено в Message broker: {1}. Текст ошибки: {2}. StackTrace: {3}.",
                           message.NotificationGuid, message.SendingDate.Value.ToString("dd.MM.yyyy hh:mm"), ex.Message, ex.StackTrace);
        return SupernikaPushNotifications.Null;
      }
    }

  }
}