using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationStatementDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationStatementDocumentSharedHandlers
  {

    public virtual void ApplicationDateDirRXChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      this.FillName();
    }

    public override void EmployeeChanged(HRPro.Vacations.Shared.VacationPersonalDocumentBaseEmployeeChangedEventArgs e)
    {
      base.EmployeeChanged(e);
      
      if (e.NewValue != null)
      {
        _obj.Department = e.NewValue?.Department;
        _obj.BusinessUnit = e.NewValue?.Department?.BusinessUnit;
      }
    }

  }
}