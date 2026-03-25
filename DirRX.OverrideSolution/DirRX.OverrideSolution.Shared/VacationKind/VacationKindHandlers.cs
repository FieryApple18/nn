using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationKind;

namespace DirRX.OverrideSolution
{
  partial class VacationKindSharedHandlers
  {

    public virtual void VacationKindDirRXChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      if (e.NewValue != null && !Equals(e.NewValue, e.OldValue))
      {
        var isAnnual = e.NewValue == VacationKind.VacationKindDirRX.AnnualLeave;
        var isRecall = e.NewValue == VacationKind.VacationKindDirRX.RecallVacation;
        var isStudy = NNVacations.PublicFunctions.Module.CheckForStudyVacationType(_obj.Id);
        _obj.State.Properties.SAPIdDirRX.IsRequired = isAnnual || isRecall || isStudy;
        _obj.State.Properties.SAPIdDirRX.IsVisible = isAnnual || isRecall || isStudy;
        _obj.State.Properties.ActualAbsenceDirRX.IsVisible = !(isAnnual || isRecall || isStudy);
      }
    }

  }
}