using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.SupernikaPushNotification;

namespace DirRX.NNIntegration.Client
{
  partial class SupernikaPushNotificationActions
  {
    public virtual void CancelSending(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.ProcessingStatus = ProcessingStatus.Canceled;
      _obj.Save();
    }

    public virtual bool CanCancelSending(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Доступно если запись справочника имеет статус "Запланирована отправка в Message Broker"
      // и запись справочника сохранена и у пользователя есть права на измение записи.
      return _obj.ProcessingStatus == ProcessingStatus.QueuedForMB 
        && !_obj.State.IsInserted
        && _obj.AccessRights.CanUpdate();
    }

  }


}