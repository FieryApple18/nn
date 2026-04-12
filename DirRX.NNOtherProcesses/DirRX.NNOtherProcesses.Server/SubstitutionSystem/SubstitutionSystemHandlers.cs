using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.SubstitutionSystem;

namespace DirRX.NNOtherProcesses
{
  partial class SubstitutionSystemServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Status = DirRX.NNOtherProcesses.SubstitutionSystem.Status.Active;
    }
  }

}