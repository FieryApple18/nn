using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentApprovalAssignment;

namespace DirRX.OverrideSolution.Client
{
  partial class DocumentApprovalAssignmentActions
  {
    public virtual void ForwardDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.AddresseeDirRX == null)
      {
        e.AddError(Module.HRDocFlow.Resources.CantForwardWithoutAddressee);
        return;
      }
      if (Employees.Equals(_obj.AddresseeDirRX, _obj.Performer))
      {
        e.AddError(Module.HRDocFlow.Resources.PerformerAlreadyExistsFormat(_obj.AddresseeDirRX.Person.ShortName));
        return;
      }
      
      _obj.Forward(_obj.AddresseeDirRX, _obj.Deadline);
      
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanForwardDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == DocumentApprovalAssignment.Status.InProcess &&
        !(_obj.IsCompetitionWorkDirRX.HasValue && _obj.IsCompetitionWorkDirRX.Value) &&
        _obj.CanForwardDirRX.HasValue && _obj.CanForwardDirRX.Value;
    }


    public override void CreateSubtask(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.CreateSubtask(e);
    }

    public override bool CanCreateSubtask(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateSubtask(e) && _obj.AllowOpinionDirRX != true;
    }

    public virtual void CreateOpinonTaskDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var opinionTask = Functions.DocumentApprovalAssignment.Remote.CreateOpinionSubTask(_obj);
      if (opinionTask == null)
        e.AddError(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.OpinionTaskNotCreatedError);
      else
        opinionTask.Show();
    }

    public virtual bool CanCreateOpinonTaskDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == DirRX.OverrideSolution.DocumentApprovalAssignment.Status.InProcess && _obj.AllowOpinionDirRX == true &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public virtual void AddApproverDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.DocumentApprovalAssignment.ShowAddApproverDialog(_obj);
    }

    public virtual bool CanAddApproverDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == DirRX.OverrideSolution.DocumentApprovalAssignment.Status.InProcess && _obj.AddApproverDirRX == true &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public override void ForRework(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.ForRework(e);
    }

    public override bool CanForRework(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanForRework(e) &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public override void Approved(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (_obj.NeedLrdPaymentInfoDirRX == true &&
          !Functions.DocumentApprovalAssignment.Remote.IsPerformerAdditional(_obj) &&
          !Functions.DocumentApprovalAssignment.ShowLrdPaymentInfoDialog(_obj))
        e.Cancel();
      
      base.Approved(e);
    }

    public override bool CanApproved(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanApproved(e) &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public override void Refuse(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Refuse(e);
    }

    public override bool CanRefuse(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanRefuse(e) &&
        Module.HRDocFlow.Functions.Module.CheckBaseExecutionResults(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj);
    }

    public virtual void Forward(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // TODO Nikitin_II. Старая конкурентная работа.
      // Вариант выполнения задания можно убрать, когда завершатся все задачи, запущенные до обновления на 25.2/2.7.
      
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
      // TODO Nikitin_II. Старая конкурентная работа.
      // Вариант выполнения задания можно убрать, когда завершатся все задачи, запущенные до обновления на 25.2/2.7.
      
      return _obj.IsCompetitionWorkDirRX.HasValue && _obj.IsCompetitionWorkDirRX.Value &&
        _obj.CanForwardDirRX.HasValue && _obj.CanForwardDirRX.Value;
    }

    public virtual void ReturnToQueueDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.AddresseeDirRX = _obj.VirtualEmployeeDirRX;
      e.Params.AddOrUpdate(Module.HRDocFlow.PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName, false);
      _obj.Complete(DocumentApprovalAssignment.Result.Forward);
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
      _obj.Complete(DocumentApprovalAssignment.Result.Forward);
    }

    public virtual bool CanTakeToWorkDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Module.HRDocFlow.Functions.Module.CheckCompetitiveWorkActions(_obj.IsCompetitionWorkDirRX, Employees.As(_obj.Performer), _obj.Status, true);
    }

  }
}