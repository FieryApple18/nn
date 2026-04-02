using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Employee;

namespace DirRX.OverrideSolution
{
  partial class EmployeeSharedHandlers
  {

    public virtual void IsVirtualDirRXChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue == true)
      {
        _obj.NeedNotifyAssignmentsSummary = false;
        _obj.NeedNotifyExpiredAssignments = false;
        _obj.NeedNotifyNewAssignments = false;
      }
      else
      {
        if (_obj.NeedCreateSubstitutionsDirRX == true)
          _obj.NeedCreateSubstitutionsDirRX = false;
        
        if (_obj.PerformersDirRX.Any())
          _obj.PerformersDirRX.Clear();
      }
    }

  }
}