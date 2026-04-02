using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.HRAssignment;

namespace DirRX.OverrideSolution
{
  partial class HRAssignmentServerHandlers
  {

    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      base.BeforeComplete(e);
      
      Module.HRDocFlow.Functions.Module.SetCompetitionWorkActionResult(e);
    }

  }
}