using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.TransferRequestDocument;

namespace DirRX.NNOtherProcesses
{
  partial class TransferRequestDocumentTransferReasonPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> TransferReasonFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_obj.BusinessUnit != null)
        query = query.Where(r => r.BusinessUnits.Any(b => b.BusinessUnit.Id == _obj.BusinessUnit.Id));
      return query;
    }
  }

}