using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationKind;

namespace DirRX.OverrideSolution
{
  partial class VacationKindClientHandlers
  {

    public virtual void IsStudyDirRXValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        Functions.VacationKind.SetPropertiesRequiredAndVisible(_obj);
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);

      Functions.VacationKind.SetPropertiesRequiredAndVisible(_obj);
    }

    public virtual void PaidVacationDeadlineDirRXValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        if (e.NewValue < 0)
          e.AddError(DirRX.OverrideSolution.VacationKinds.Resources.PositiveValueError);
      }
    }

    public virtual void DaysBeforeRegistrationDirRXValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue < 0)
        e.AddError(VacationKinds.Resources.PositiveValueError);
    }

  }
}