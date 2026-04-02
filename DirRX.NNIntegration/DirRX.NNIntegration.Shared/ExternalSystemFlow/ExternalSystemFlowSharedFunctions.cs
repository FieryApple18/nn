using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.ExternalSystemFlow;

namespace DirRX.NNIntegration.Shared
{
  partial class ExternalSystemFlowFunctions
  {
    public void SetPropertiesRequired()
    {
      _obj.State.Properties.ExternalSystem.IsRequired = _obj.Direction == NNIntegration.ExternalSystemFlow.Direction.Outgoing;
      _obj.State.Properties.ActionName.IsRequired = _obj.Direction == NNIntegration.ExternalSystemFlow.Direction.Outgoing;
      
      _obj.State.Properties.WaitResponse.IsEnabled = _obj.Direction == NNIntegration.ExternalSystemFlow.Direction.Outgoing;
    }
  }
}