using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationsApprovalPeriod;

namespace DirRX.NNVacations
{
  partial class VacationsApprovalPeriodServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.VacationsApprovalPeriod.Remote.GetDuplicates(_obj).Any())
        e.AddError(DirRX.NNVacations.VacationsApprovalPeriods.Resources.DuplicateRecordsFoundError, _obj.Info.Actions.ShowDuplicates);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.PaymentCompleted = false;
      _obj.Top = false;
      _obj.Status = NNVacations.VacationsApprovalPeriod.Status.Active;
    }
  }

}