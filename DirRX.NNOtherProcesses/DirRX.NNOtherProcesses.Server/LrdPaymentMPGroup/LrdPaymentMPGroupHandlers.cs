using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentMPGroup;

namespace DirRX.NNOtherProcesses
{
  partial class LrdPaymentMPGroupServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.LrdPaymentMPGroup.Remote.GetDuplicates(_obj).Any())
        e.AddError(DirRX.NNOtherProcesses.LrdPaymentMPGroups.Resources.FindDuplicatesError, _obj.Info.Actions.ShowDuplicates);
    }
  }

}