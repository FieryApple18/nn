using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.InitiatingApprovalAssignment;

namespace DirRX.NNVacations.Client
{
  partial class InitiatingApprovalAssignmentFunctions
  {
    public virtual void SetPropertiesIsRequired(bool isRequired)
    {
      _obj.State.Properties.Approvers.IsRequired = isRequired;
      _obj.State.Properties.AssertingApprover.IsRequired = isRequired;
      _obj.State.Properties.AssertingApproverJobTitle.IsRequired = isRequired;
      _obj.State.Properties.AssertingApprovalDeadline.IsRequired = isRequired;
      _obj.State.Properties.EmployeeAcquaintancePeriod.IsRequired = isRequired;
      _obj.State.Properties.DaysToRemind.IsRequired = isRequired;
      _obj.State.Properties.ScheduleResponsible.IsRequired = isRequired;
      _obj.State.Properties.ApprovalDeadline.IsRequired = isRequired;
      _obj.State.Properties.RegistrationNumber.IsRequired = isRequired;
      _obj.State.Properties.RegistrationDate.IsRequired = isRequired;
    }
  }
}