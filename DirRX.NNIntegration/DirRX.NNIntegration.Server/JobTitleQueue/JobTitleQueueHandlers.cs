using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.JobTitleQueue;

namespace DirRX.NNIntegration
{
  partial class JobTitleQueueServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.IsEmployeeExists = true;
    }
  }

}