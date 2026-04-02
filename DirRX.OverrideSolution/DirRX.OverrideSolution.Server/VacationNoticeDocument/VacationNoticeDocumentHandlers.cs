using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationNoticeDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationNoticeDocumentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.Constants.Module.DocumentKind.VacationBeginningNoticeKind);
      _obj.LifeCycleState = VacationNoticeDocument.LifeCycleState.Draft;
      _obj.InternalApprovalState = VacationNoticeDocument.InternalApprovalState.PendingSign;
    }

  }
}