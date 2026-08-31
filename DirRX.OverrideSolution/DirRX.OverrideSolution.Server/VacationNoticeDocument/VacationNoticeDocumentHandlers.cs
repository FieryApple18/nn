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

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      // Формат текста для уведомления: "Тип отпуска Дата начала - Дата окончания, количество дней: Количество дней; ...".
      var periods = new List<string>();
      foreach (var vacationData in _obj.VacationDataDirRX)        
        periods.Add(DirRX.OverrideSolution.VacationNoticeDocuments.Resources.VacationPeriodTextFormat(vacationData.VacationType?.Name, 
                                                                                                      vacationData.StartDate.HasValue? vacationData.StartDate.Value.ToShortDateString() : string.Empty,
                                                                                                      vacationData.EndDate.HasValue? vacationData.EndDate.Value.ToShortDateString() : string.Empty, 
                                                                                                      vacationData.Days));  
      _obj.VacationPeriodsDirRX = string.Format("\n{0}", string.Join(";\n", periods));
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.Constants.Module.DocumentKind.VacationBeginningNoticeKind);
      _obj.LifeCycleState = VacationNoticeDocument.LifeCycleState.Draft;
      _obj.InternalApprovalState = VacationNoticeDocument.InternalApprovalState.PendingSign;
    }

  }
}