using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationPersonalDocumentBase;

namespace DirRX.OverrideSolution
{
  partial class VacationPersonalDocumentBaseSharedHandlers
  {

    public override void EmployeeChanged(HRPro.Vacations.Shared.VacationPersonalDocumentBaseEmployeeChangedEventArgs e)
    {
      base.EmployeeChanged(e);
      
      _obj.EmployeeDirRX = e.NewValue != null ? DirRX.OverrideSolution.Employees.As(e.NewValue) : null;
    }

  }
}