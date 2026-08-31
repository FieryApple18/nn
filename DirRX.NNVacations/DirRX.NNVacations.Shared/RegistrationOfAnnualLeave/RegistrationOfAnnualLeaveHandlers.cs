using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.RegistrationOfAnnualLeave;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations
{
  partial class RegistrationOfAnnualLeaveSharedHandlers
  {

    public virtual void OrderChanged(DirRX.NNVacations.Shared.RegistrationOfAnnualLeaveOrderChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        _obj.OrderAttachmentGroup.VacationOrderDocuments.Clear();
        _obj.OrderAttachmentGroup.VacationOrderDocuments.Add(e.NewValue);
      }
    }

    public virtual void NoticeChanged(DirRX.NNVacations.Shared.RegistrationOfAnnualLeaveNoticeChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        _obj.DocumentGroup.VacationNoticeDocuments.Clear();
        _obj.DocumentGroup.VacationNoticeDocuments.Add(e.NewValue);
      }
    }

    public virtual void EmployeeChanged(DirRX.NNVacations.Shared.RegistrationOfAnnualLeaveEmployeeChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        _obj.EmployeeAttachmentGroup.Employees.Clear();
        _obj.EmployeeAttachmentGroup.Employees.Add(e.NewValue);
      }
    }
  }
}