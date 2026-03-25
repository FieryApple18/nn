using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkReason;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkReasonServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Status = WeekendWorkReason.Status.Active;
    }
  }

}