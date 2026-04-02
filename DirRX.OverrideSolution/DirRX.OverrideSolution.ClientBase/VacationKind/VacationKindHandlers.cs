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

    public virtual void PaidVacationDeadlineDirRXValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        if (e.NewValue < 0)
          e.AddError(DirRX.OverrideSolution.VacationKinds.Resources.PositiveValueError);
      }
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      var isAnnual = _obj.VacationKindDirRX == VacationKind.VacationKindDirRX.AnnualLeave;
      var isRecall = _obj.VacationKindDirRX == VacationKind.VacationKindDirRX.RecallVacation;
      var isStudy = NNVacations.PublicFunctions.Module.CheckForStudyVacationType(_obj.Id);
      _obj.State.Properties.SAPIdDirRX.IsRequired = isAnnual || isRecall || isStudy;
      _obj.State.Properties.SAPIdDirRX.IsVisible = isAnnual || isRecall || isStudy;
      _obj.State.Properties.ActualAbsenceDirRX.IsVisible = !(isAnnual || isRecall || isStudy);
      _obj.State.Properties.VacationKindDirRX.IsRequired = true;
    }

    public virtual void DaysBeforeStartEnableDirRXValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue < 0)
        e.AddError(VacationKinds.Resources.PositiveValueError);
    }

  }
}