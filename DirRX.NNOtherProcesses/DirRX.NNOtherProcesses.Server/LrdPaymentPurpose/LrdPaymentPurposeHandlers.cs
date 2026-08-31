using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentPurpose;

namespace DirRX.NNOtherProcesses
{
  partial class LrdPaymentPurposeServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.LrdPaymentPurpose.Remote.GetDuplicates(_obj).Any())
        e.AddWarning(DirRX.NNOtherProcesses.LrdPaymentPurposes.Resources.FindDuplicatesError, _obj.Info.Actions.ShowDuplicates);
    }
  }

}