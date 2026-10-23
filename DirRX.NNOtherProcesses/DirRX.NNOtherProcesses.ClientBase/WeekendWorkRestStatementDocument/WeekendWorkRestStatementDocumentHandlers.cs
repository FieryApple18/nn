using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkRestStatementDocument;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkRestStatementDocumentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      var weekendWorkDocKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(Constants.Module.DocumentKind.WeekendWorkRestStatementDocument);
      var isWeekendWork = _obj.DocumentKind.Equals(weekendWorkDocKind);
      e.Params.AddOrUpdate(string.Format(Constants.WeekendWorkRestStatementDocument.DocKindParamName, _obj.Id), isWeekendWork);
    }

  }
}