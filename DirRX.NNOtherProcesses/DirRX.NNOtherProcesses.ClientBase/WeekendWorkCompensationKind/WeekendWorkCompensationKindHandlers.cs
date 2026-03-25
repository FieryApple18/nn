using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkCompensationKind;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkCompensationKindClientHandlers
  {
    public virtual void NeedRestDateValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      _obj.State.Properties.IsRestDateRequired.IsEnabled = e.NewValue == true;
    }
    
    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.IsRestDateRequired.IsEnabled = _obj.NeedRestDate == true;
    }
  }
}