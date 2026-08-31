using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationKind;

namespace DirRX.OverrideSolution.Client
{
  partial class VacationKindFunctions
  {

    /// <summary>
    /// Устновить видимость и обязательность для свойств.
    /// </summary>
    public void SetPropertiesRequiredAndVisible()
    {
      var isAnnualLeave = _obj.VacationKindDirRX == VacationKind.VacationKindDirRX.AnnualLeave;
      var isRecallVacation = _obj.VacationKindDirRX == VacationKind.VacationKindDirRX.RecallVacation;
      var isStudy = NNVacations.PublicFunctions.Module.HasStudyVacationType(_obj);
      
      if (isAnnualLeave || isRecallVacation || isStudy)
      {
        _obj.State.Properties.SAPIdDirRX.IsRequired = true;
        _obj.State.Properties.SAPIdDirRX.IsVisible = true;
        _obj.State.Properties.ActualAbsenceDirRX.IsVisible = false;
      }
      else
      {
        _obj.State.Properties.SAPIdDirRX.IsRequired = false;
        _obj.State.Properties.SAPIdDirRX.IsVisible = false;
        _obj.State.Properties.ActualAbsenceDirRX.IsVisible = true;
      }
    }

  }
}