using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.ExternalSystemFlow;

namespace DirRX.NNIntegration
{
  partial class ExternalSystemFlowClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      if (!_obj.State.IsInserted)
        Functions.ExternalSystemFlow.SetPropertiesRequired(_obj);
    }
  }
}