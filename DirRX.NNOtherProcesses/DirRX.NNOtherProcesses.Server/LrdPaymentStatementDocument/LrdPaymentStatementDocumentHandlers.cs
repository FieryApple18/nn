using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentStatementDocument;

namespace DirRX.NNOtherProcesses
{
  partial class LrdPaymentStatementDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      if (_obj.State.Properties.Versions.IsChanged)
        _obj.PreparationDate = Calendar.Now;
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.RegistrationDate = Calendar.Now;
      _obj.PreparationDate = Calendar.Now;
    }
  }

}