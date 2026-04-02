using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationStatementDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationStatementDocumentAdditionalVacationDirRXClientHandlers
  {

    public virtual void AdditionalVacationDirRXDaysValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue)
      {
        if (e.NewValue.Value <= 0)
          e.AddError(VacationStatementDocuments.Resources.NegativeDurationError);
      }
    }

  }

  partial class VacationStatementDocumentClientHandlers
  {

    public virtual void DaysAmountDirRXValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue)
      {
        if (e.NewValue.Value <= 0)
          e.AddError(VacationStatementDocuments.Resources.NegativeDurationError);
      }
    }

  }
}