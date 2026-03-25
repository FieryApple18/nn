using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.SignAcquaintanceListAssignment;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations.Client
{
  partial class SignAcquaintanceListAssignmentActions
  {
    public virtual void VacationScheduleAcquaintanceLists(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.SignAcquaintanceListAssignment.Remote.GetVacationScheduleAcquaintanceLists(_obj).Show(SignAcquaintanceListAssignments.Resources.TitleVacationScheduleAcquaintanceLists);
    }

    public virtual bool CanVacationScheduleAcquaintanceLists(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void CreateIndividualAcquiantanceVacScheduleLists(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.SignAcquaintanceListAssignment.CreateAcquiantanceVacScheduleLists(_obj, "CreateIndividualAcquiantanceVacScheduleLists");
    }

    public virtual bool CanCreateIndividualAcquiantanceVacScheduleLists(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.IsStartedCreateLists != true || Sungero.Docflow.PublicFunctions.Module.Remote.IsAdministrator();
    }

    public virtual void CreateGroupAcquiantanceVacScheduleLists(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.SignAcquaintanceListAssignment.CreateAcquiantanceVacScheduleLists(_obj, "CreateGroupAcquiantanceVacScheduleLists");
    }

    public virtual bool CanCreateGroupAcquiantanceVacScheduleLists(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.IsStartedCreateLists != true || Sungero.Docflow.PublicFunctions.Module.Remote.IsAdministrator();
    }

    public virtual bool CanForward(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.CanForward.HasValue && _obj.CanForward.Value;
    }

    public virtual void Forward(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (_obj.Addressee == null)
      {
        e.AddError(DirRX.OverrideSolution.Module.HRDocFlow.Resources.CantForwardWithoutAddressee);
        e.Cancel();
      }
      if (Employees.Equals(_obj.Addressee, _obj.Performer))
      {
        e.AddError(DirRX.OverrideSolution.Module.HRDocFlow.Resources.PerformerAlreadyExistsFormat(_obj.Addressee.Person.ShortName));
        e.Cancel();
      }
    }

    public virtual void ReturnToQueue(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Addressee = _obj.VirtualEmployee;
      e.Params.AddOrUpdate(DirRX.OverrideSolution.Module.HRDocFlow.PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName, false);
      _obj.Complete(SignAcquaintanceListAssignment.Result.Forward);
    }

    public virtual bool CanReturnToQueue(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.VirtualEmployee != null &&
        DirRX.OverrideSolution.Module.HRDocFlow.PublicFunctions.Module.CheckCompetitiveWorkActions(_obj.IsCompetitionWork, Employees.As(_obj.Performer), _obj.Status, false);
    }

    public virtual void TakeToWork(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (Employees.Current == null)
      {
        e.AddError(DirRX.OverrideSolution.Module.HRDocFlow.Resources.UserIsSystemErrorMessage);
        return;
      }
      
      _obj.Addressee = Employees.Current;
      e.Params.AddOrUpdate(DirRX.OverrideSolution.Module.HRDocFlow.PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName, true);
      _obj.Complete(SignAcquaintanceListAssignment.Result.Forward);
    }

    public virtual bool CanTakeToWork(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return DirRX.OverrideSolution.Module.HRDocFlow.PublicFunctions.Module.CheckCompetitiveWorkActions(_obj.IsCompetitionWork, Employees.As(_obj.Performer), _obj.Status, true);
    }

    public virtual void Reject(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (!HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer))
      {
        e.AddError(HRPro.HRDocFlow.Resources.PerformerCanOnly);
        return;
      }
      
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(HRPro.HRDocFlow.HRAssignments.Resources.ErrorTextRejectNoActiveText);
    }

    public virtual bool CanReject(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.CanRefuse.HasValue && _obj.CanRefuse.Value &&
        OverrideSolution.Module.HRDocFlow.PublicFunctions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWork, Employees.As(_obj.Performer), _obj);
    }

    public virtual void Terminate(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (!HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer))
      {
        e.AddError(HRPro.HRDocFlow.Resources.PerformerCanOnly);
        return;
      }
      
      if (string.IsNullOrEmpty(_obj.ActiveText))
        e.AddError(HRPro.HRDocFlow.HRAssignments.Resources.ErrorTextTerminateNoActiveText);
    }

    public virtual bool CanTerminate(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.CanTerminate.HasValue && _obj.CanTerminate.Value &&
        OverrideSolution.Module.HRDocFlow.PublicFunctions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWork, Employees.As(_obj.Performer), _obj);
    }

    public virtual void Approved(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (!HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer))
      {
        e.AddError(HRPro.HRDocFlow.Resources.PerformerCanOnly);
        return;
      }
    }

    public virtual bool CanApproved(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.CanApprove.HasValue && _obj.CanApprove.Value &&
        OverrideSolution.Module.HRDocFlow.PublicFunctions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWork, Employees.As(_obj.Performer), _obj);
    }

    public virtual void ForRevision(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (!HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer))
      {
        e.AddError(HRPro.HRDocFlow.Resources.PerformerCanOnly);
        return;
      }
    }

    public virtual bool CanForRevision(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.CanForRevision.HasValue && _obj.CanForRevision.Value &&
        OverrideSolution.Module.HRDocFlow.PublicFunctions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWork, Employees.As(_obj.Performer), _obj);
    }

    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (!HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer))
      {
        e.AddError(HRPro.HRDocFlow.Resources.PerformerCanOnly);
        return;
      }
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.CanComplete.HasValue && _obj.CanComplete.Value &&
        OverrideSolution.Module.HRDocFlow.PublicFunctions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWork, Employees.As(_obj.Performer), _obj);
    }
  }
}