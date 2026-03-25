using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.CertificateIssueErrorAssignment;

namespace DirRX.NNCommons.Client
{
  partial class CertificateIssueErrorAssignmentActions
  {
    public virtual void CertifIssue(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var task = CertificateIssueTasks.As(_obj.Task);
      var employee = EssPlatformSolution.Employees.As(task.Employee);
      
      var startCertificateIssueTaskResult = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.StartCertificateIssueTask(employee, task);
      if (!startCertificateIssueTaskResult.IsStarted)
      {
        if (string.IsNullOrEmpty(startCertificateIssueTaskResult.StartTaskError) && !startCertificateIssueTaskResult.DataErrorList.Any())
          e.Cancel();
        else
        {
          foreach (var error in startCertificateIssueTaskResult.DataErrorList)
            throw AppliedCodeException.Create(error.Error);
          if (!string.IsNullOrEmpty(startCertificateIssueTaskResult.StartTaskError))
            throw AppliedCodeException.Create(DirRX.EssPlatformSolution.Employees.Resources.BeforeCertificateIssueTaskErrorFormat(startCertificateIssueTaskResult.StartTaskError));
        }
      }
    }

    public virtual bool CanCertifIssue(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}