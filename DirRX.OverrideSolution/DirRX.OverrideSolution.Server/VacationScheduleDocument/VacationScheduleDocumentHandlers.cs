using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationScheduleDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationScheduleDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (_obj.TradeUnionOpinionDirRX.Count > Constants.Vacations.VacationScheduleDocument.MaxTradeUnionOpinionsCount)
        e.AddError(DirRX.OverrideSolution.VacationScheduleDocuments.Resources.MaxTradeUnionOpinionsCountError);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.Constants.Module.DocumentKind.VacationScheduleKind);
    }

  }
}