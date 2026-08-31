using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.CertificateIssueTask;

namespace DirRX.OverrideSolution
{
  partial class CertificateIssueTaskServerHandlers
  {

    public override void AfterSuspend(Sungero.Workflow.Server.AfterSuspendEventArgs e)
    {
      base.AfterSuspend(e);
      
      var errorText = _obj.History.GetAll()
        .Where(h => h.Operation == Sungero.Workflow.WorkflowHistory.Operation.Suspend)
        .OrderBy(h => h.HistoryDate)
        .Select(h => h.Comment)
        .FirstOrDefault();
      
      if (string.IsNullOrEmpty(errorText))
        return;
      
      DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.SendCertificateIssueErrorNotice(_obj.Id, errorText);
    }
  }

}