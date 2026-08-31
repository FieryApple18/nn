using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.CertificateIssueTask;

namespace DirRX.NNCommons
{
  partial class CertificateIssueTaskServerHandlers
  {

    public override void AfterSuspend(Sungero.Workflow.Server.AfterSuspendEventArgs e)
    {
      var errorText = _obj.History.GetAll()
        .Where(h => h.Operation == Sungero.Workflow.WorkflowHistory.Operation.Suspend)
        .OrderBy(h => h.HistoryDate)
        .Select(h => h.Comment)
        .FirstOrDefault();
      
      if (string.IsNullOrEmpty(errorText))
        return;

      DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.SendCertificateIssueErrorNotice(_obj.Id, errorText);
    }

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      var request = _obj.CertificateRequestAttachmentGroup.CertificateRequests.FirstOrDefault();
      if (request != null)
      {
        var cancelCertificateRequest = SignPlatform.AsyncHandlers.CancelCertificateIssueRequest.Create();
        cancelCertificateRequest.RequestId = request.Id;
        cancelCertificateRequest.ExecuteAsync();
      }
    }

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      var subject = DirRX.SignPlatform.Resources.CertificateIssueTaskSubjectFormat(_obj.Employee.Name).ToString();
      var subjectPropetyLength = _obj.Info.Properties.Subject.Length;
      _obj.Subject = DirRX.SignPlatform.PublicFunctions.Module.GetLimitStringWithEllipsis(subject, subjectPropetyLength);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Subject = Sungero.Docflow.Resources.AutoformatTaskSubject;
    }
  }

}