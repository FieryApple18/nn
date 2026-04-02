using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.HRSimpleDocument;

namespace DirRX.NNCommons
{
  partial class HRSimpleDocumentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      _obj.State.Properties.Name.IsEnabled = _obj.DocumentKind != null && _obj.DocumentKind.GenerateDocumentName != true;
    }
  }

}