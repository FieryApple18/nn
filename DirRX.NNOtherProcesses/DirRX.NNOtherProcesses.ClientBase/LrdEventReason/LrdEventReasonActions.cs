using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdEventReason;

namespace DirRX.NNOtherProcesses.Client
{
  partial class LrdEventReasonActions
  {
    public virtual void ShowDuplicates(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var duplicates = PublicFunctions.LrdEventReason.Remote.GetDuplicates(_obj);
      duplicates.Show();
    }

    public virtual bool CanShowDuplicates(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}