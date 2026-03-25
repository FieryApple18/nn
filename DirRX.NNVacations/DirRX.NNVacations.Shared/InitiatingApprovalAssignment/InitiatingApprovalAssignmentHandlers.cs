using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.InitiatingApprovalAssignment;

namespace DirRX.NNVacations
{
  partial class InitiatingApprovalAssignmentSharedHandlers
  {
    public virtual void BusinessUnitChanged(DirRX.NNVacations.Shared.InitiatingApprovalAssignmentBusinessUnitChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        _obj.ScheduleResponsible = OverrideSolution.Employees.As(e.NewValue.VacationScheduleResponsibleDirRX);
      }
    }
  }
}