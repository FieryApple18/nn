using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationStatementDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationStatementDocumentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.LifeCycleState = VacationStatementDocument.LifeCycleState.Draft;
      _obj.InternalApprovalState = VacationStatementDocument.InternalApprovalState.OnApproval;
      _obj.IsShiftByStatementDirRX = false;
    }

  }
}