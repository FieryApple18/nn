using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.ESignAssignment;

namespace DirRX.OverrideSolution
{
  partial class ESignAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      if (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowedDirRX.HasValue && _obj.IsSubstitutionAllowedDirRX.Value == false)
      {
        e.AddError(HRPro.HRDocFlow.Resources.PerformerAndPartTimeWorkersCanOnly);
        return;
      }
    }

  }
}