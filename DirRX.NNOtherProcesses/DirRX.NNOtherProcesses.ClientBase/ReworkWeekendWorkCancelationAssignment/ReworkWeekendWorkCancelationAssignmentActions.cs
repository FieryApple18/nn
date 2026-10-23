using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.ReworkWeekendWorkCancelationAssignment;

namespace DirRX.NNOtherProcesses.Client
{
  partial class ReworkWeekendWorkCancelationAssignmentActions
  {
    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {

    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}