using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationsApprovalPeriod;

namespace DirRX.NNVacations
{
  partial class VacationsApprovalPeriodClientHandlers
  {

    public virtual void ExactTimeDeadlineValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      if (e.NewValue != null && e.NewValue != e.OldValue && !System.Text.RegularExpressions.Regex.IsMatch(e.NewValue, NNVacations.PublicConstants.Module.PatternValidTime))
        e.AddError(DirRX.NNVacations.VacationsApprovalPeriods.Resources.ValidExactTimeDeadlineError);
    }

    public virtual void NotificationOldVacationDaysValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        if (e.NewValue < 0)
          e.AddError(DirRX.NNVacations.VacationsApprovalPeriods.Resources.PositiveValueError);
      }
    }

    public virtual void StatementOldVacationDaysValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        if (e.NewValue < 0)
          e.AddError(DirRX.NNVacations.VacationsApprovalPeriods.Resources.PositiveValueError);
      }
    }

  }
}