using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentStatementDocument;

namespace DirRX.NNOtherProcesses
{
  partial class LrdPaymentStatementDocumentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      if (_obj.PaymentType?.Instruction != null)
        e.AddInformation(_obj.PaymentType.Instruction);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      _obj.State.Properties.RegistrationDate.IsVisible = true;
      _obj.State.Properties.InternalApprovalState.IsVisible = true;
      _obj.State.Properties.PreparationDate.IsVisible = true;
      e.Params.AddOrUpdate(Sungero.Docflow.Constants.OfficialDocument.ShowParam, true);
    }

  }
}