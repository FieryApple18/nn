using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.TransferRequestDocument;

namespace DirRX.NNOtherProcesses
{
  partial class TransferRequestDocumentTransferReasonClarificationPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> TransferReasonClarificationFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_obj.TransferReason != null)
        return query.Where(q => q.TransferReason.Id == _obj.TransferReason.Id);
      
      return query.Where(q => false);
    }
  }

  partial class TransferRequestDocumentTransferReasonPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> TransferReasonFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_obj.BusinessUnit != null)
        return query.Where(q => q.BusinessUnits.Any(b => b.BusinessUnit.Id == _obj.BusinessUnit.Id));
      
      return query.Where(q => false);
    }
  }

}