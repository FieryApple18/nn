using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentApprovalAssignment;

namespace DirRX.OverrideSolution
{
  partial class DocumentApprovalAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      _obj.State.Properties.AddresseeDirRX.IsVisible = _obj.CanForwardDirRX == true;
      
      var block = OverrideSolution.Module.HRDocFlow.Blocks.DocumentApprovalAssignmentBlocks.Get(_obj.Task.Scheme, _obj.BlockUid);
      if (block != null)
        _obj.State.Properties.AdditionalApproversDirRX.IsVisible = block.ShowAddApproversDirRX == true;
    }

  }
}