using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.BusinessProcess;
using DirRX.OverrideSolution;

namespace DirRX.NNCommons.Client
{
  partial class BusinessProcessActions
  {
    public virtual void GetBusinessProcessLinks(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.BusinessProcess.Remote.GetBusinessProcessLinks(_obj).ShowModal();
    }

    public virtual bool CanGetBusinessProcessLinks(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }
}