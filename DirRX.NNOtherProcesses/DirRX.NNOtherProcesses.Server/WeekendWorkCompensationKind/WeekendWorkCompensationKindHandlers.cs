using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkCompensationKind;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkCompensationKindServerHandlers
  {
    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.NeedRestDate = false;
      _obj.IsRestDateRequired = false;
    }
  }

  partial class WeekendWorkCompensationKindDocumentKindPropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> DocumentKindFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(d => d.DocumentType.DocumentTypeGuid == Constants.Module.DocumentType.WeekendWorkStatementDocument.ToString());
    }
  }
}