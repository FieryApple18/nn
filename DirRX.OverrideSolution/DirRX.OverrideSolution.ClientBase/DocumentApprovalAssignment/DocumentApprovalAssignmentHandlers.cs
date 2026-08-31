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
    }

  }
}