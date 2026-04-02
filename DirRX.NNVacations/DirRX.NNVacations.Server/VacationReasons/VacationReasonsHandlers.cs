using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationReasons;

namespace DirRX.NNVacations
{
  partial class VacationReasonsServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
      _obj.Attachable = false;
    }
  }

}