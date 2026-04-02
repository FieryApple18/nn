using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.ExternalSystemFlow;

namespace DirRX.NNIntegration
{
  partial class ExternalSystemFlowSharedHandlers
  {

    public virtual void DirectionChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        Functions.ExternalSystemFlow.SetPropertiesRequired(_obj);
      }
    }
  }
}