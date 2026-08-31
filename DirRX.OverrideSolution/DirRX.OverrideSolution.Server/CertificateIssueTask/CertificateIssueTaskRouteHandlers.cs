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