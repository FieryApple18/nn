using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.ExternalSystemFlow;

namespace DirRX.NNIntegration
{
  partial class ExternalSystemFlowServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.ClearAfterProcessing = false;
      _obj.WaitResponse = false;
    }
  }

}