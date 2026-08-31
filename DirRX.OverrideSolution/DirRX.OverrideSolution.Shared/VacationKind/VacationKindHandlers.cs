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
      if (e.NewValue != e.OldValue)
      {
        _obj.SAPIdDirRX = string.Empty;
        
        foreach (var actualAbsence in _obj.ActualAbsenceDirRX.ToList())
          _obj.ActualAbsenceDirRX.Remove(actualAbsence);
      }
    }

    public virtual void BusinessUnitDirRXChanged(DirRX.OverrideSolution.Shared.VacationKindBusinessUnitDirRXChangedEventArgs e)
    {
      if (e.NewValue != null && !Equals(e.NewValue, e.OldValue))
      {
        var removeActualAbsence = _obj.ActualAbsenceDirRX.Where(a => Equals(a.VacationReason?.BusinessUnitDirRX, e.OldValue)).ToList();
        
        foreach (var actualAbsence in removeActualAbsence)
          _obj.ActualAbsenceDirRX.Remove(actualAbsence);
      }
    }

  }
}