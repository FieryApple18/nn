using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationConfirmationAssignment;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations.Client
{
  partial class VacationConfirmationAssignmentActions
  {
    public virtual bool CanStartShiftTask(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
        (_obj.Performer.Equals(Users.Current) ||
         (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowed.HasValue && _obj.IsSubstitutionAllowed.Value == true));
    }

    public virtual void StartShiftTask(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (!HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer))
      {
        e.AddError(HRPro.Vacations.VacationConfirmationAssignments.Resources.CanPerformerOnly);
        return;
      }
      // TODO Нужно скорректировать на нашу задачу переноса отпуска.
      // HRPro.Vacations.PublicFunctions.Module.Remote.CreateNewVacationChangeTask(alertVacations, _obj).Show();
      // TODO Удалить после добавления вызова задачи на перенос, выполнение будет там.
      _obj.Complete(VacationConfirmationAssignment.Result.Shift);
      e.CloseFormAfterAction = true;
    }

    public virtual void Shift(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      // TODO Сформировать задачу на перенос отпуска.
    }

    public virtual bool CanShift(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return (_obj.Performer.Equals(Users.Current) ||
              (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowed.HasValue && _obj.IsSubstitutionAllowed.Value == true));
    }

    public virtual void Confirm(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (!DirRX.EssPlatform.PublicFunctions.Module.GetPersonEmployeeIds(Employees.As(_obj.Performer).Person.Id).Contains(Users.Current.Id) &&
          !HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer))
      {
        e.AddError(HRPro.Vacations.VacationConfirmationAssignments.Resources.CanPerformerOnly);
        return;
      }
    }

    public virtual bool CanConfirm(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return (_obj.Performer.Equals(Users.Current) ||
              (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowed.HasValue && _obj.IsSubstitutionAllowed.Value == true));
    }

  }

}