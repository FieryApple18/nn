using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.SubstitutionPersonalAccount;

namespace DirRX.NNOtherProcesses
{
  partial class SubstitutionPersonalAccountServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.SentStatus = DirRX.NNOtherProcesses.SubstitutionPersonalAccount.SentStatus.Created;
    }
  }

}