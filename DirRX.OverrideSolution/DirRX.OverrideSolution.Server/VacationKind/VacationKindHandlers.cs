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
      var vacantionKind = OverrideSolution.VacationKinds.As(_obj.RootEntity);
      
      return query.Where(q => q.Status == NNVacations.VacationReasons.Status.Active &&
                         (Equals(q.BusinessUnitDirRX, vacantionKind.BusinessUnitDirRX) || q.BusinessUnitDirRX == null));
    }

  }

  partial class VacationKindServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (Functions.VacationKind.CheackDuplicateVacationKind(_obj))
      {
        e.AddError(DirRX.OverrideSolution.VacationKinds.Resources.DuplicateRecordFound);
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
      _obj.AttachableDirRX = false;
      _obj.RelevantForTOPDirRX = false;
      _obj.IsCompensatedDirRX = false;
      _obj.CanBeTransferredSeparatelyDirRX = false;
      _obj.NeedsTransferredCompletelyDirRX = false;
      _obj.RequireShiftDaysWhenRecallDirRX = true;
      _obj.CanTakeAdvanceDirRX = false;
      _obj.IsStudyDirRX = false;
    }

  }
}