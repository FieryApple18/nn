using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentKind;

namespace DirRX.OverrideSolution.Client
{
  partial class DocumentKindFunctions
  {

    /// <summary>
    /// Скрыть свойства настройки шаблонов для всех документов, кроме документов с таким типом: Заявление и согласие по отпуску, Приказ об отпуске,
    /// Извещение об отпуске, График отпусков, Индивидуальный лист ознакомления, Заявление на выплату по ЛНА.
    /// </summary>       
    public override void SetVisibilityOfTemplatesSettings()
    {
      var templatesPropertiesIsVisible = _obj.DocumentType != null &&
        (_obj.DocumentType.DocumentTypeGuid == NNOtherProcesses.Server.LrdPaymentStatementDocument.ClassTypeGuid.ToString() ||
         _obj.DocumentType.DocumentTypeGuid == HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument.ToString() ||
         _obj.DocumentType.DocumentTypeGuid == HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationScheduleDocument.ToString() ||
         _obj.DocumentType.DocumentTypeGuid == HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument.ToString() ||
         _obj.DocumentType.DocumentTypeGuid == HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationNoticeDocument.ToString() ||
         _obj.DocumentType.DocumentTypeGuid == HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationIndividualAcquaintanceDocument.ToString());
      
      _obj.State.Properties.TemplatesLKToBusinessUnitHRPro.IsVisible = templatesPropertiesIsVisible;
      _obj.State.Properties.TemplatesToBusinessUnitHRPro.IsVisible = templatesPropertiesIsVisible;
      _obj.State.Properties.IsNeedTemplatesLKHRPro.IsVisible = templatesPropertiesIsVisible;
      _obj.State.Properties.IsNeedTemplatesRXHRPro.IsVisible = templatesPropertiesIsVisible;
    }

  }
}