using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.InitiatingApprovalAssignment;

namespace DirRX.NNVacations
{
  partial class InitiatingApprovalAssignmentServerHandlers
  {
    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      var doc = _obj.DocumentGroup.VacationScheduleDocuments.FirstOrDefault();
      if (doc != null)
      {
        var lockOwner = string.Empty;
        var docLockInfo = Locks.GetLockInfo(doc);
        
        if (docLockInfo.IsLocked)
          lockOwner = docLockInfo.OwnerName;
        else if (doc.HasVersions)
        {
          var versionLockInfo = Locks.GetLockInfo(doc.Versions.FirstOrDefault().Body);
          if (versionLockInfo.IsLocked)
            lockOwner = versionLockInfo.OwnerName;
        }
        
        if (!string.IsNullOrEmpty(lockOwner))
          e.AddError(NNVacations.InitiatingApprovalAssignments.Resources.VacationScheduleDocumentLockedErrorFormat(doc.Name, doc.Id, lockOwner));
      }
    }
  }
}