using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationConfirmationAssignment;

namespace DirRX.NNVacations
{
  partial class VacationConfirmationAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      if (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowed.HasValue && _obj.IsSubstitutionAllowed.Value == false)
      {
        e.AddError(HRPro.HRDocFlow.Resources.PerformerAndPartTimeWorkersCanOnly);
        return;
      }
    }

  }
}