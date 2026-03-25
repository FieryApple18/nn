using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.BusinessUnit;

namespace DirRX.OverrideSolution
{
  partial class BusinessUnitServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.ActivateLineManagersDirRX = false;
      _obj.IsNeedNotifyCompletionUKEPDirRX = false;
      _obj.SendPushDuringWorkHoursDirRX = false;
    }

  }
}