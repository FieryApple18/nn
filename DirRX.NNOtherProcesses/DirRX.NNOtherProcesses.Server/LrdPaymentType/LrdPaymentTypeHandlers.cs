using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentType;

namespace DirRX.NNOtherProcesses
{
  partial class LrdPaymentTypeServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.LrdPaymentType.Remote.GetDuplicates(_obj).Any())
        e.AddError(DirRX.NNOtherProcesses.LrdPaymentTypes.Resources.FindDuplicatesError, _obj.Info.Actions.ShowDuplicates);
    }
  }

}