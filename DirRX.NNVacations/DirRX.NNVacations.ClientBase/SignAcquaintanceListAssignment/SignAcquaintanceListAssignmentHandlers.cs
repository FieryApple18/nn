using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.SignAcquaintanceListAssignment;

namespace DirRX.NNVacations
{
  partial class SignAcquaintanceListAssignmentClientHandlers
  {
    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      var canForward = _obj.CanForward == true;
      _obj.State.Properties.Addressee.IsVisible = canForward;
    }
  }
}