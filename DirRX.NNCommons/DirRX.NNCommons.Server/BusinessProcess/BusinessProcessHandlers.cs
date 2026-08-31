using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.BusinessProcess;
using DirRX.OverrideSolution;

namespace DirRX.NNCommons
{
  partial class BusinessProcessServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (!_obj.State.IsInserted && _obj.Status == BusinessProcess.Status.Closed &&
          Functions.BusinessProcess.GetBusinessProcessLinks(_obj).Any())
      {
        e.AddError(BusinessProcesses.Resources.FindBusinessProcessLinksError, BusinessProcesses.Info.Actions.GetBusinessProcessLinks);
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Status = BusinessProcess.Status.Active;
    }
  }

}