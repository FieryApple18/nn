using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Employee;

namespace DirRX.OverrideSolution
{
  partial class EmployeeClientHandlers
  {

    public override IEnumerable<Enumeration> NeedNotifyNewHRAssignmentDirRXFiltering(IEnumerable<Enumeration> query)
    {
      query = query.Where(q => q == Employee.NeedNotifyNewHRAssignmentDirRX.No || q == Employee.NeedNotifyNewHRAssignmentDirRX.SMS);
      
      return query;
    }

    public override IEnumerable<Enumeration> NeedNotifyExpiredHRAssignmentDirRXFiltering(IEnumerable<Enumeration> query)
    {
      query = query.Where(q => q == Employee.NeedNotifyExpiredHRAssignmentDirRX.No || q == Employee.NeedNotifyExpiredHRAssignmentDirRX.SMS);
      
      return query;
    }

    public override IEnumerable<Enumeration> NeedNotifyHRRepeatDirRXFiltering(IEnumerable<Enumeration> query)
    {
      query = query.Where(q => q == Employee.NeedNotifyHRRepeatDirRX.No || q == Employee.NeedNotifyHRRepeatDirRX.SMS);
      
      return query;
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      Functions.Employee.UpdateForm(_obj);
      ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params[DirRX.OverrideSolution.PublicConstants.Docflow.DocumentKind.IsVisualModeParamName] = true;
    }

  }
}