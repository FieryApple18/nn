using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocRelatedToWorkBase;

namespace DirRX.OverrideSolution
{
  partial class DocRelatedToWorkBaseClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      Functions.DocRelatedToWorkBase.CheckRightsOnEDoc(_obj, e);
      
      base.Showing(e);
    }

  }
}