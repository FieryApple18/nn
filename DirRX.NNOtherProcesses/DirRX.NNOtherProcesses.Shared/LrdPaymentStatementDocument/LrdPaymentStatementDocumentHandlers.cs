using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentStatementDocument;

namespace DirRX.NNOtherProcesses
{
  partial class LrdPaymentStatementDocumentSharedHandlers
  {

    public virtual void PaymentTypeChanged(DirRX.NNOtherProcesses.Shared.LrdPaymentStatementDocumentPaymentTypeChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.CodeVO = e.NewValue?.CodeVO;
    }

    public override void PreparationDateChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      base.PreparationDateChanged(e);
      Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
    }

    public override void EmployeeDirRXChanged(DirRX.OverrideSolution.Shared.DocRelatedToWorkBaseEmployeeDirRXChangedEventArgs e)
    {
      base.EmployeeDirRXChanged(e);
      Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
    }

  }
}