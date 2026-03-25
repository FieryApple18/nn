using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentApprovalAssignment;

namespace DirRX.OverrideSolution
{
  partial class DocumentApprovalAssignmentServerHandlers
  {

    public override void BeforeStartWork(Sungero.Workflow.Server.BeforeStartWorkEventArgs e)
    {
      base.BeforeStartWork(e);
      
      //HACK Для обхода бага 446803, который выводил в счётчике прекращённые непрочитанные задания в папке потока.
      var assignmentReader = NNCommons.AsyncHandlers.MarkAsReadAbortedCompetitiveAssignments.Create();
      assignmentReader.TaskId = _obj.Task.Id;
      assignmentReader.BlockUId = _obj.BlockUid;
      assignmentReader.ExecuteAsync();
    }

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      base.BeforeComplete(e);
      
      Module.HRDocFlow.Functions.Module.SetCompetitionWorkActionResult(e);
    }

  }
}