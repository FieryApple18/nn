using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.SupernikaPushNotification;

namespace DirRX.NNIntegration
{
  partial class SupernikaPushNotificationServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.IsNotification = false;
      _obj.Created = Calendar.Now;
      _obj.ProcessingStatus = ProcessingStatus.QueuedForMB;
    }
  }

}