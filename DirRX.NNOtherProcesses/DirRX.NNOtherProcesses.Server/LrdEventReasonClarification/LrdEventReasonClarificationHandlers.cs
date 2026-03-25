using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdEventReasonClarification;

namespace DirRX.NNOtherProcesses
{
  partial class LrdEventReasonClarificationServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.LrdEventReasonClarification.Remote.GetDuplicates(_obj).Any())
        e.AddWarning(DirRX.NNOtherProcesses.LrdEventReasonClarifications.Resources.FindDuplicatesError, _obj.Info.Actions.ShowDuplicates);
    }
  }

}