using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.BusinessUnit;

namespace DirRX.OverrideSolution
{
  partial class BusinessUnitClientHandlers
  {

    public override IEnumerable<Enumeration> NeedNotifyHRRepeatDirRXSungeroFiltering(IEnumerable<Enumeration> query)
    {
      query = query.Where(q => q == BusinessUnit.NeedNotifyHRRepeatDirRXSungero.No || q == BusinessUnit.NeedNotifyHRRepeatDirRXSungero.SMS);
      
      return query;
    }

    public override IEnumerable<Enumeration> NeedNotifyExpiredHRAssignmentDirRXSungeroFiltering(IEnumerable<Enumeration> query)
    {
      query = query.Where(q => q == BusinessUnit.NeedNotifyExpiredHRAssignmentDirRXSungero.No || q == BusinessUnit.NeedNotifyExpiredHRAssignmentDirRXSungero.SMS);
      
      return query;
    }

    public override IEnumerable<Enumeration> NeedNotifyNewHRAssignmentDirRXSungeroFiltering(IEnumerable<Enumeration> query)
    {
      query = query.Where(q => q == BusinessUnit.NeedNotifyNewHRAssignmentDirRXSungero.No || q == BusinessUnit.NeedNotifyNewHRAssignmentDirRXSungero.SMS);
      
      return query;
    }

  }
}