using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentType;

namespace DirRX.NNOtherProcesses.Client
{
  partial class LrdPaymentTypeActions
  {
    public virtual void ShowDuplicates(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var duplicates = PublicFunctions.LrdPaymentType.Remote.GetDuplicates(_obj);
      duplicates.Show();
    }

    public virtual bool CanShowDuplicates(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}