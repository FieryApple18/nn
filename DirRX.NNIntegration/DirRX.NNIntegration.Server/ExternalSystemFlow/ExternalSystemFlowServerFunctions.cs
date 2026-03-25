using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.ExternalSystemFlow;

namespace DirRX.NNIntegration.Server
{
  partial class ExternalSystemFlowFunctions
  {
    /// <summary>
    /// Получить Поток внешней системы по guid.
    /// </summary>
    /// <param name="guid">Guid потока.</param>
    /// <returns>Поток внешней системы.</returns>
    [Public]
    public static IExternalSystemFlow GetExternalSystemFlow(string guid)
    {
      return ExternalSystemFlows.GetAll().FirstOrDefault(f => f.FlowGuid == guid);
    }
  }
}