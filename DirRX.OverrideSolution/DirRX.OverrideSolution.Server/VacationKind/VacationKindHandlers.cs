using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationKind;

namespace DirRX.OverrideSolution
{
  partial class VacationKindActualAbsenceDirRXVacationReasonPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> ActualAbsenceDirRXVacationReasonFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(q => q.Status == NNVacations.VacationReasons.Status.Active);
    }

  }

  partial class VacationKindServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
      _obj.AttachableDirRX = false;
      _obj.RelevantForTOPDirRX = false;
      _obj.IsCompensatedDirRX = false;
      _obj.CanBeTransferredSeparatelyDirRX = false;
      _obj.NeedsTransferredCompletelyDirRX = false;
    }

  }
}