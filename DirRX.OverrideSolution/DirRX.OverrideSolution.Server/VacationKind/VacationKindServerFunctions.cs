using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationKind;

namespace DirRX.OverrideSolution.Server
{
  partial class VacationKindFunctions
  {

    /// <summary>
    /// Проверить на дубли виды отпусков.
    /// </summary>
    public virtual bool CheackDuplicateVacationKind()
    {
      var vacantionKinds = OverrideSolution.VacationKinds.GetAll(v => !Equals(v, _obj) &&
                                                                 Equals(_obj.BusinessUnitDirRX, v.BusinessUnitDirRX) &&
                                                                 _obj.VacationKindDirRX == v.VacationKindDirRX &&
                                                                 v.Status == OverrideSolution.VacationKind.Status.Active)
        .ToList();
      
      var isAnnualLeave = _obj.VacationKindDirRX == OverrideSolution.VacationKind.VacationKindDirRX.AnnualLeave;
      var isRecallVacation = _obj.VacationKindDirRX == OverrideSolution.VacationKind.VacationKindDirRX.RecallVacation;
      var isStudy = NNVacations.PublicFunctions.Module.HasStudyVacationType(_obj);
      
      if (isAnnualLeave || isRecallVacation || isStudy)
        vacantionKinds = vacantionKinds.Where(v => _obj.SAPIdDirRX == v.SAPIdDirRX)
          .ToList();
      else
        vacantionKinds = vacantionKinds.Where(c => _obj.ActualAbsenceDirRX.Where(a => c.ActualAbsenceDirRX.Any(ca => ca.ActualAbsence == a.ActualAbsence))
                                              .Any() || (!_obj.ActualAbsenceDirRX.Any() && !c.ActualAbsenceDirRX.Any()))
          .ToList();
      
      return vacantionKinds.Any();
    }

  }
}