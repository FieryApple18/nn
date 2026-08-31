using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.ExternalSystemDataExchangeTask;

namespace DirRX.NNIntegration
{
  partial class ExternalSystemDataExchangeTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      if (_obj.Documents != null)
      {
        foreach (var document in _obj.Documents)
        {
          if (!_obj.DocumentGroup.OfficialDocuments.Contains(document.Document))
            _obj.DocumentGroup.OfficialDocuments.Add(document.Document);
        }
      }
    }
  }


}