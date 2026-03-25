using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.SignAcquaintanceListAssignment;

namespace DirRX.NNVacations
{
  partial class SignAcquaintanceListAssignmentServerHandlers
  {
    public override void BeforeComplete(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      var isTakeToWork = false;
      if (e.Params.TryGetValue(OverrideSolution.Module.HRDocFlow.PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName, out isTakeToWork))
      {
        e.Result = isTakeToWork ? OverrideSolution.Module.HRDocFlow.Resources.TakeToWorkMessage : OverrideSolution.Module.HRDocFlow.Resources.ReturnToQueueMessage;
        e.Params.Remove(OverrideSolution.Module.HRDocFlow.PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName);
      }
    }
  }
}