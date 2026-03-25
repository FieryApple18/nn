using System;
using System.Collections.Generic;
using System.Linq;
using DirRX.OverrideSolution;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.LinqExpressions;
using Sungero.Workflow;
using Sungero.Workflow.AssignmentBase;

namespace DirRX.NNCommons.Server
{
  partial class MassApproveFolderHandlers
  {

    public virtual IQueryable<DirRX.OverrideSolution.IESignAssignment> MassApproveDataQuery(IQueryable<DirRX.OverrideSolution.IESignAssignment> query)
    {
      var assignments =  query.Where(c =>  c.MassSigningDirRX.HasValue && 
                                           c.MassSigningDirRX == true &&
                                           c.AllowSignSubstitution == true);
      
      if (!assignments.Any())
        return assignments;
      
      if (_filter == null)
        return assignments;
      
      if (_filter.InWork)
        assignments = assignments.Where(c => c.Status == Status.InProcess);

      if (_filter.ViewFirstHundredTasks)
        assignments = assignments.Take(100);

      DateTime? startDate = null;

      if (_filter.ThirtyDays)
        startDate = Calendar.AddWorkingDays(Calendar.Today, -30);

      if (_filter.NinetyDays)
        startDate = Calendar.AddWorkingDays(Calendar.Today, -90);

      if (_filter.OneHundredEightyDays)
        startDate = Calendar.AddWorkingDays(Calendar.Today, -180);

      if (startDate != null)
        assignments = assignments.Where(c => Calendar.Between(c.Created, startDate.Value, Calendar.Now));

      return assignments;
    }
  }

  partial class NNCommonsHandlers
  {
  }
}