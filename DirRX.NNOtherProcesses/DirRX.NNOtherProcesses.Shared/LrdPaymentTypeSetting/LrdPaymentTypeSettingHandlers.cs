using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentTypeSetting;

namespace DirRX.NNOtherProcesses
{
  partial class LrdPaymentTypeSettingSharedHandlers
  {

    public virtual void PaymentTypeChanged(DirRX.NNOtherProcesses.Shared.LrdPaymentTypeSettingPaymentTypeChangedEventArgs e)
    {
      Functions.LrdPaymentTypeSetting.FillName(_obj);
    }

    public virtual void BusinessUnitChanged(DirRX.NNOtherProcesses.Shared.LrdPaymentTypeSettingBusinessUnitChangedEventArgs e)
    {
      Functions.LrdPaymentTypeSetting.FillName(_obj);
    }

    public virtual void FixedPaymentChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        Functions.LrdPaymentTypeSetting.SetRequiredProperties(_obj);
    }

  }
}