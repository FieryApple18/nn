using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.InitiatingApprovalAssignment;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations
{
  partial class InitiatingApprovalAssignmentApproversClientHandlers
  {
    public virtual void ApproversApproverValueInput(DirRX.NNVacations.Client.InitiatingApprovalAssignmentApproversApproverValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        _obj.ApproverJobTitle = OverrideSolution.JobTitles.As(e.NewValue.JobTitle);
      }
    }
  }

  partial class InitiatingApprovalAssignmentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      // TODO Nikitin_II. Старая конкурентная работа.
      // Можно убрать, когда завершатся все задачи, запущенные до обновления на 25.2/2.7.
      
      var isOldCompetition = _obj.IsCompetitionWork.HasValue && _obj.IsCompetitionWork.Value;
      var isCompetition = _obj.IsCompetitive.HasValue && _obj.IsCompetitive.Value;
      if ((!isOldCompetition && !isCompetition) ||
          (isOldCompetition && Employees.As(_obj.Performer)?.IsVirtualDirRX != true) ||
          (isCompetition && _obj.IsCompetitiveWorkStarted.Value))
        Functions.InitiatingApprovalAssignment.SetPropertiesIsRequired(_obj, true);
    }

    public virtual void AssertingApproverValueInput(DirRX.NNVacations.Client.InitiatingApprovalAssignmentAssertingApproverValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue && e.NewValue != null)
      {
        _obj.AssertingApproverJobTitle = OverrideSolution.JobTitles.As(e.NewValue.JobTitle);
      }
    }
  }
}