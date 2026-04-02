using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationOrderDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationOrderDocumentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.Constants.Module.DocumentKind.VacationOrderKind);
      _obj.LifeCycleState = VacationOrderDocument.LifeCycleState.Draft;
    }

  }
}