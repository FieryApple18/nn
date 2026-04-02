using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdEventReason;

namespace DirRX.NNOtherProcesses
{
  partial class LrdEventReasonServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.LrdEventReason.Remote.GetDuplicates(_obj).Any())
        e.AddWarning(DirRX.NNOtherProcesses.LrdEventReasons.Resources.FindDuplicatesError, _obj.Info.Actions.ShowDuplicates);
    }
  }

}