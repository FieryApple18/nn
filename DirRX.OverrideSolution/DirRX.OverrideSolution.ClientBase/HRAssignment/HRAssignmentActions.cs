using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.HRAssignment;

namespace DirRX.OverrideSolution.Client
{
  partial class HRAssignmentActions
  {

    public override void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Complete(e);
      
      if (_obj.NeedActiveTextDirRX == true && string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(DirRX.OverrideSolution.HRAssignments.Resources.ActiveTextNotFilledError);
      else if (!string.IsNullOrEmpty(_obj.ActiveText))
        _obj.ActiveText.Trim();
    }

    public override bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanComplete(e) &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public override void ForRevision(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.ForRevision(e);
    }

    public override bool CanForRevision(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanForRevision(e) &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public override void Approved(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Approved(e);
    }

    public override bool CanApproved(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanApproved(e) &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public override void Terminate(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Terminate(e);
    }

    public override bool CanTerminate(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanTerminate(e) &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public override void Reject(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Reject(e);
    }

    public override bool CanReject(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanReject(e) &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public virtual void ReturnToQueueDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.AddresseeDirRX = _obj.VirtualEmployeeDirRX;
      e.Params.AddOrUpdate(Module.HRDocFlow.PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName, false);
      _obj.Complete(HRAssignment.Result.Forward);
    }

    public virtual bool CanReturnToQueueDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.VirtualEmployeeDirRX != null &&
        Module.HRDocFlow.Functions.Module.CheckCompetitiveWorkActions(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj.Status, false);
    }

    public virtual void TakeToWorkDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (Employees.Current == null)
      {
        e.AddError(Module.HRDocFlow.Resources.UserIsSystemErrorMessage);
        return;
      }
      
      _obj.AddresseeDirRX = Employees.Current;
      e.Params.AddOrUpdate(Module.HRDocFlow.PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName, true);
      _obj.Complete(HRAssignment.Result.Forward);
    }

    public virtual bool CanTakeToWorkDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Module.HRDocFlow.Functions.Module.CheckCompetitiveWorkActions(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj.Status, true);
    }

    public virtual void Forward(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (_obj.AddresseeDirRX == null)
      {
        e.AddError(Module.HRDocFlow.Resources.CantForwardWithoutAddressee);
        e.Cancel();
      }
      if (Employees.Equals(_obj.AddresseeDirRX, _obj.Performer))
      {
        e.AddError(Module.HRDocFlow.Resources.PerformerAlreadyExistsFormat(_obj.AddresseeDirRX.Person.ShortName));
        e.Cancel();
      }
    }

    public virtual bool CanForward(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.CanForwardDirRX.HasValue && _obj.CanForwardDirRX.Value;
    }

    public virtual void Noted(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanNoted(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.CanNotedDirRX.HasValue && _obj.CanNotedDirRX.Value &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

  }
}