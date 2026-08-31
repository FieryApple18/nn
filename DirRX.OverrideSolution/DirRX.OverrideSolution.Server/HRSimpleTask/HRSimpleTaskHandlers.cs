using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.HRSimpleTask;

namespace DirRX.OverrideSolution
{
  partial class HRSimpleTaskServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (_obj.DeadlineDirRX.HasValue && _obj.DeadlineDirRX.Value < Calendar.Now)
        e.AddError(DirRX.OverrideSolution.HRSimpleTasks.Resources.DeadlineError);
    }
  }

}