using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.InitiatingApprovalAssignment;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations.Client
{
  partial class InitiatingApprovalAssignmentActions
  {
    public override void ReturnUncompleted(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.InitiatingApprovalAssignment.SetPropertiesIsRequired(_obj, false);
      
      base.ReturnUncompleted(e);
    }

    public override bool CanReturnUncompleted(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanReturnUncompleted(e);
    }


    public virtual void Corrected(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanCorrected(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return Employees.As(_obj.Performer).IsVirtualDirRX != true && _obj.CanCorrected.GetValueOrDefault();
    }

    public virtual void Forward(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (_obj.Addressee == null)
      {
        e.AddError(OverrideSolution.Module.HRDocFlow.Resources.CantForwardWithoutAddressee);
        e.Cancel();
      }
      if (Employees.Equals(_obj.Addressee, _obj.Performer))
      {
        e.AddError(OverrideSolution.Module.HRDocFlow.Resources.PerformerAlreadyExistsFormat(_obj.Addressee.Person.ShortName));
        e.Cancel();
      }
    }

    public virtual bool CanForward(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return false;
    }

    public virtual void ReturnToQueue(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Addressee = _obj.VirtualEmployee;
      e.Params.AddOrUpdate(OverrideSolution.Module.HRDocFlow.PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName, false);
      foreach (var property in _obj.State.Properties)
        property.IsRequired = false;
      _obj.Complete(InitiatingApprovalAssignment.Result.Forward);
    }

    public virtual bool CanReturnToQueue(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.VirtualEmployee != null &&
        OverrideSolution.Module.HRDocFlow.PublicFunctions.Module.CheckCompetitiveWorkActions(_obj.IsCompetitionWork, Employees.As(_obj.Performer), _obj.Status, false);
    }

    public virtual void TakeToWork(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (Employees.Current == null)
      {
        e.AddError(OverrideSolution.Module.HRDocFlow.Resources.UserIsSystemErrorMessage);
        return;
      }
      
      _obj.Addressee = Employees.Current;
      e.Params.AddOrUpdate(OverrideSolution.Module.HRDocFlow.PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName, true);
      _obj.Complete(InitiatingApprovalAssignment.Result.Forward);
    }

    public virtual bool CanTakeToWork(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return OverrideSolution.Module.HRDocFlow.PublicFunctions.Module.CheckCompetitiveWorkActions(_obj.IsCompetitionWork, Employees.As(_obj.Performer), _obj.Status, true);
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return Employees.As(_obj.Performer).IsVirtualDirRX != true && _obj.CanComplete.GetValueOrDefault();
    }

  }
}