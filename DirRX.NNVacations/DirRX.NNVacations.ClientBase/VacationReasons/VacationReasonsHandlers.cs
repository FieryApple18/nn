using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationReasons;

namespace DirRX.NNVacations
{
  partial class VacationReasonsClientHandlers
  {

    public virtual void DaysBeforeStartEnableValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue < 0)
        e.AddError(VacationReasonses.Resources.DaysCountNegativeError);
    }

  }
}