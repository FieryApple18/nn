using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkOrderDocument;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkOrderDocumentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      var isCancelationOrder = Functions.WeekendWorkOrderDocument.Remote.IsCancelWork(_obj.DocumentKind);
      e.Params.AddOrUpdate(string.Format(Constants.WeekendWorkOrderDocument.DocKindParamName, _obj.Id), isCancelationOrder);
    }

  }
}