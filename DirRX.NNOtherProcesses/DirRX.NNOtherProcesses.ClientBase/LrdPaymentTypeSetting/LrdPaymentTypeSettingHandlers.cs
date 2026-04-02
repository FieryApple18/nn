using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentTypeSetting;

namespace DirRX.NNOtherProcesses
{
  partial class LrdPaymentTypeSettingClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      Functions.LrdPaymentTypeSetting.SetRequiredProperties(_obj);
    }

  }
}