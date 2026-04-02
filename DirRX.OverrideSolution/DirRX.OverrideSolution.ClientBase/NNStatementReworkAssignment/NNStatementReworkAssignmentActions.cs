using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.NNStatementReworkAssignment;

namespace DirRX.OverrideSolution.Client
{
  partial class NNStatementReworkAssignmentActions
  {
    public virtual void Abort(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanAbort(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }
  }
}