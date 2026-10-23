using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationRefinementReason;

namespace DirRX.NNVacations
{
  partial class VacationRefinementReasonServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (Functions.VacationRefinementReason.CheackDuplicateVacationRefinementReason(_obj))
      {
        e.AddError(NNVacations.Resources.DuplicateRecordFound);
      }
    }
  }

}