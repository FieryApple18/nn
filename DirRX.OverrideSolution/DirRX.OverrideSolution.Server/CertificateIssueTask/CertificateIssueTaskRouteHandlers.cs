using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.OverrideSolution.CertificateIssueTask;

namespace DirRX.OverrideSolution.Server
{
  partial class CertificateIssueTaskRouteHandlers
  {

    public override void StartBlock5(DirRX.SignPlatform.Server.CertificateIssueStatementAcceptAssignmentArguments e)
    {
      base.StartBlock5(e);
      
      var deadlineDays = NNCommons.PublicFunctions.Module.GetIntValue(NNCommons.PublicConstants.Module.ConstantsSetting.CertificateIssueAssingmentDeadlineDays.Guid) ??
        NNCommons.PublicConstants.Module.DefaultCertificateIssueAssingmentDeadlineDays;
      e.Block.RelativeDeadlineDays = deadlineDays;
    }

    public override void Script34Execute()
    {
      var response = DirRX.SignPlatform.PublicFunctions.Module.SendCertificateIssueRequest(_obj.Employee, _obj.ProviderId, _obj.IdentificationType.Value.Value);
      if (!string.IsNullOrEmpty(response.Error))
      {
        var errorSubjectPropertyLength = _obj.Info.Properties.ErrorAssignmentSubject.Length;
        _obj.ErrorAssignmentSubject = DirRX.SignPlatform.PublicFunctions.Module.GetLimitStringWithEllipsis(response.Error, errorSubjectPropertyLength);
        _obj.ActiveText = response.Error;
        _obj.Save();
        return;
      }
      _obj.RequestId = response.RequestId;
      _obj.Save();
      
      var certificateRequestEntry = DirRX.SignPlatform.CertificateRequests.Create();
      certificateRequestEntry.RequestId = _obj.RequestId;
      certificateRequestEntry.TaskId = _obj.Id;
      certificateRequestEntry.CreateDate = Calendar.Now;
      certificateRequestEntry.Employee = _obj.Employee;
      certificateRequestEntry.IssueStatus = SignPlatform.CertificateRequest.IssueStatus.DocVerification;
      certificateRequestEntry.Executor = SignPlatform.CertificateRequest.Executor.TaskScheme;
      certificateRequestEntry.IdentificationType = _obj.IdentificationType;
      var name = DirRX.SignPlatform.Resources.CertificateRequestEntryNameFormat(certificateRequestEntry.Id, certificateRequestEntry.Employee.Person.DisplayValue).ToString();
      var namePropertyLength = certificateRequestEntry.Info.Properties.Name.Length;
      certificateRequestEntry.Name = DirRX.SignPlatform.PublicFunctions.Module.GetLimitStringWithEllipsis(name, namePropertyLength);
      certificateRequestEntry.ProviderId = _obj.ProviderId;
      certificateRequestEntry.ProviderType = _obj.ProviderType.Value.Value;
      certificateRequestEntry.Save();
      
      _obj.CertificateRequestAttachmentGroup.CertificateRequests.Add(certificateRequestEntry);
      _obj.Save();
    }

    public override void StartBlock52(DirRX.SignPlatform.Server.CertificateIssueErrorNoticeArguments e)
    {
      base.StartBlock52(e);
      
      e.Block.Performers.Clear();
      
      var recipients = HRPro.HRDocFlow.PublicFunctions.Module.GetRoleRecipients(DirRX.EssPlatform.PublicConstants.Module.AdminElEmployeeInteractionSystem);
      
      foreach (var recipient in recipients)
        e.Block.Performers.Add(recipient);
    }

  }
}