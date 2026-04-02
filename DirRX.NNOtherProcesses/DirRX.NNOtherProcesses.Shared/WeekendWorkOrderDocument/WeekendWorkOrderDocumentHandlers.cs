using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkOrderDocument;
using DirRX.OverrideSolution;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkOrderDocumentSharedHandlers
  {

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      base.DocumentKindChanged(e);
      
      // Обновить параметр для вида документа при его изменении.
      if (!DocumentKinds.Equals(e.NewValue, e.OldValue))
      {
        var isCancelationOrder = Functions.WeekendWorkOrderDocument.Remote.IsCancelWork(e.NewValue);
        var paramName = string.Format(Constants.WeekendWorkOrderDocument.DocKindParamName, _obj.Id);
        e.Params.AddOrUpdate(paramName, isCancelationOrder);
      }
    }

    public virtual void WeekendWorkStatementChanged(DirRX.NNOtherProcesses.Shared.WeekendWorkOrderDocumentWeekendWorkStatementChangedEventArgs e)
    {
      if (!WeekendWorkStatementDocuments.Equals(e.NewValue, e.OldValue))
      {
        _obj.IsGroup = e.NewValue?.IsGroup;
        _obj.TopDirRX = e.NewValue?.TopDirRX;
        Functions.WeekendWorkOrderDocument.FillName(_obj);
      }
    }

  }
}