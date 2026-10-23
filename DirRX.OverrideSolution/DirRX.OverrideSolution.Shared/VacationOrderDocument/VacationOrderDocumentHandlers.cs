using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationOrderDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationOrderDocumentSharedHandlers
  {

    public override void EmployeeChanged(HRPro.Vacations.Shared.VacationPersonalDocumentBaseEmployeeChangedEventArgs e)
    {
      base.EmployeeChanged(e);
      
      if (e.NewValue != null)
      {
        _obj.BusinessUnit = e.NewValue?.Department?.BusinessUnit;
        _obj.Department = e.NewValue?.Department;
      }
    }

  }
}