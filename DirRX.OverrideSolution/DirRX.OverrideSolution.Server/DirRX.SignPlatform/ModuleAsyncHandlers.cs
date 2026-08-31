using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.SignPlatform.Server
{
  partial class ModuleAsyncHandlers
  {

    public override void AbortIssueTask(DirRX.SignPlatform.Server.AsyncHandlerInvokeArgs.AbortIssueTaskInvokeArgs args)
    {
      Logger.DebugFormat("DirRX.OverrideSolution.SignPlatform.AsyncHandlers.AbortIssueTask started, iteration: {0}", args.RetryIteration);
      if (!string.IsNullOrEmpty(args.TaskIds))
      {
        var ids = args.TaskIds.Split(',').Select(id => long.Parse(id));
        try
        {
          foreach (var id in ids)
          {
            Logger.DebugFormat("DirRX.OverrideSolution.SignPlatform.AsyncHandlers.AbortIssueTask start of processing certificateIssueTask: {0}, iteration: {1}", id, args.RetryIteration);
            var certificateIssueTask = Sungero.Workflow.Tasks.GetAll(i => i.Id == id).FirstOrDefault();
            if (certificateIssueTask != null && certificateIssueTask.Status == Sungero.Workflow.Task.Status.InProcess)
              certificateIssueTask.Abort();
            var certRequests = DirRX.SignPlatform.CertificateRequests.GetAll(cr => cr.TaskId == id && cr.IssueStatus != DirRX.SignPlatform.CertificateRequest.IssueStatus.Error);
            foreach (var certRequest in certRequests)
            {
              certRequest.IssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
              certRequest.Errors = DirRX.SignPlatform.Resources.ByDisconnectingFromESS;
              certRequest.Save();
            }
          }
          Logger.DebugFormat("DirRX.OverrideSolution.SignPlatform.AsyncHandlers.AbortIssueTask ended, iteration: {0}", args.RetryIteration);
        }
        catch (Exception ex)
        {
          args.Retry = true;
          Logger.DebugFormat("DirRX.OverrideSolution.SignPlatform.AsyncHandlers.AbortIssueTask error: \"{0}\". Iteration: {1}", ex.Message, args.RetryIteration);
        }
      }
    }

  }
}