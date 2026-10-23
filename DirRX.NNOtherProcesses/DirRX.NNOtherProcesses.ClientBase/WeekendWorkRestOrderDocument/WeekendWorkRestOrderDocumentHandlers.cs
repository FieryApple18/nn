using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkRestOrderDocument;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkRestOrderDocumentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      var cancelRestDocKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(Constants.Module.DocumentKind.CancelationRestOrderDocument);
      var isCancelRestOrder = _obj.DocumentKind.Equals(cancelRestDocKind);
      e.Params.AddOrUpdate(string.Format(Constants.WeekendWorkRestOrderDocument.DocKindParamName, _obj.Id), isCancelRestOrder);
    }

  }
}