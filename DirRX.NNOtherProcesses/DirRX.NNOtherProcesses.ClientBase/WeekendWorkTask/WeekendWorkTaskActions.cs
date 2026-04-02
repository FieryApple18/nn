using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkTask;

namespace DirRX.NNOtherProcesses.Client
{
  partial class WeekendWorkTaskActions
  {
    public override void Restart(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.Restart(e);
    }

    public override bool CanRestart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false;
    }

  }

}