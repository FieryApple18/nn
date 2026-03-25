using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentTypeSetting;

namespace DirRX.NNOtherProcesses
{
  partial class LrdPaymentTypeSettingServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.LrdPaymentTypeSetting.Remote.GetDuplicates(_obj).Any())
        e.AddError(DirRX.NNOtherProcesses.LrdPaymentTypeSettings.Resources.FindDuplicatesError, _obj.Info.Actions.ShowDuplicates);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.IsSputnik = true;
      _obj.FixedPayment = true;
      _obj.Periodicity = NNOtherProcesses.LrdPaymentTypeSetting.Periodicity.OneTime;
      Functions.LrdPaymentTypeSetting.FillName(_obj);
    }
  }

}