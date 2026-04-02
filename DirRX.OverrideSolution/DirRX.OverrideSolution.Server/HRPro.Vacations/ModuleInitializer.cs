using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;
using DocKind = HRPro.Vacations.Constants.Module.DocumentKind;

namespace DirRX.OverrideSolution.Module.Vacations.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      base.Initializing(e);
    }

    /// <summary>
    /// Создать виды документов.
    /// </summary>
    public override void CreateDocumentKinds()
    {
      InitializationLogger.Debug("Init Vacations module: Create document kinds.");

      var notNumerable = Sungero.Docflow.DocumentKind.NumberingType.NotNumerable;
      var registrable = Sungero.Docflow.DocumentKind.NumberingType.Registrable;
      var innerDocumentFlow = Sungero.Docflow.DocumentRegister.DocumentFlow.Inner;
      
      var actions = new Sungero.Domain.Shared.IActionInfo[]
      {
        HRPro.HRProSolution.OfficialDocuments.Info.Actions.SendApproveAndAcquaintanceHRPro,
        HRPro.HRProSolution.OfficialDocuments.Info.Actions.SendApproveHRPro,
        HRPro.HRProSolution.OfficialDocuments.Info.Actions.SendAcquaintanceHRPro
      };
      
      // Модули инициализируются в случайном порядке. Поэтому, если действия по отправке еще
      // не созданы - то создаем их.
      if (!HRPro.HRDocFlow.PublicInitializationFunctions.Module.IsHRDocumentSendActionsCreated(actions))
      {
        HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentSendActions();
      }
      
      #region Приказы.
      
      // Приказ на ежегодный отпуск.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(DirRX.OverrideSolution.Module.Vacations.Resources.VacationOrder, DirRX.OverrideSolution.Module.Vacations.Resources.VacationOrder,
                                                                                registrable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationOrderDocument.ClassTypeGuid, actions, DocKind.VacationOrderKind, true,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 3, 3, HRPro.HRDocFlow.Constants.Module.DocumentСodes.OtherDocuments, true, false);
      
      // Приказ о переносе отпуска.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(HRDocFlow.Resources.ShiftVacationOrder, HRDocFlow.Resources.ShiftVacationOrder,
                                                                                registrable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationOrderDocument.ClassTypeGuid, actions, DocKind.VacationShiftOrderKind, false,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 3, 3, HRPro.HRDocFlow.Constants.Module.DocumentСodes.OtherDocuments, true, false);
      
      // Приказ об отзыве из отпуска.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(HRDocFlow.Resources.RecallVacationOrder, HRDocFlow.Resources.RecallVacationOrder,
                                                                                registrable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationOrderDocument.ClassTypeGuid, actions, DocKind.VacationRecallOrderKind, false,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 3, 1, HRPro.HRDocFlow.Constants.Module.DocumentСodes.OtherDocuments, true, false);
      
      #endregion
      
      #region Заявления и согласия.
      
      // Заявление на ежегодный отпуск.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(DirRX.OverrideSolution.Module.Vacations.Resources.DocKindName_VacationStatement, DirRX.OverrideSolution.Module.Vacations.Resources.DocKindName_VacationStatement,
                                                                                notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationStatementDocument.ClassTypeGuid, null, DocKind.VacationStatementKind, true,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, null, null, HRPro.HRDocFlow.Constants.Module.DocumentСodes.OtherDocuments, true, true);
      
      // Заявление на перенос отпуска.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(DirRX.OverrideSolution.Module.Vacations.Resources.VacationChangePeriodStatement, DirRX.OverrideSolution.Module.Vacations.Resources.VacationChangePeriodStatement,
                                                                                notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationStatementDocument.ClassTypeGuid, null, DocKind.VacationShiftStatementKind, false,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, null, null, HRPro.HRDocFlow.Constants.Module.DocumentСodes.EmployeesStatementPostponementAnnualPaidLeave, true, true);
      
      // Согласие на отзыв из отпуска.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(HRDocFlow.Resources.VacationRecallConsent, HRDocFlow.Resources.VacationRecallConsent,
                                                                                notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationStatementDocument.ClassTypeGuid, null, DocKind.VacationRecallConsentKind, false,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, null, null, HRPro.HRDocFlow.Constants.Module.DocumentСodes.OtherDocuments, true, true);
      // Заявление на досрочное прерывание отпуска.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(DirRX.OverrideSolution.Module.Vacations.Resources.DocKindName_VacationSelfRecallStatement, DirRX.OverrideSolution.Module.Vacations.Resources.DocKindName_VacationSelfRecallStatement,
                                                                                notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationStatementDocument.ClassTypeGuid, null, DocKind.VacationSelfRecallStatementKind, false,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, null, null, HRPro.HRDocFlow.Constants.Module.DocumentСodes.OtherDocuments, true, true);
      
      #endregion
      
      #region Извещения.
      
      // Уведомление о начале отпуска.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(DirRX.OverrideSolution.Module.Vacations.Resources.VacationNotice, DirRX.OverrideSolution.Module.Vacations.Resources.VacationNotice,
                                                                                notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationNoticeDocument.ClassTypeGuid, null, DocKind.VacationBeginningNoticeKind, false,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, null, null, HRPro.HRDocFlow.Constants.Module.DocumentСodes.StartTimeOfVacationNotification, true, false);
      
      #endregion
      
      #region Графики отпусков.
      
      // График отпусков.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(HRDocFlow.Resources.VacationSchedule, HRDocFlow.Resources.VacationSchedule,
                                                                                notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationScheduleDocument.ClassTypeGuid, actions, DocKind.VacationScheduleKind, false,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 7, 3, HRPro.HRDocFlow.Constants.Module.DocumentСodes.OtherDocuments, true, false);
      
      #endregion
      
      #region Индивидуальный лист ознакомления.
      
      // Индивидуальный лист ознакомления.
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(HRDocFlow.Resources.IndividualAcquaintance, HRDocFlow.Resources.ShortIndividualAcquaintance,
                                                                                notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.Server.VacationIndividualAcquaintanceDocument.ClassTypeGuid, null, DocKind.VacationIndividualAcquaintanceKind, false,
                                                                                HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 7, null, HRPro.HRDocFlow.Constants.Module.DocumentСodes.AcquaintanceWithLNARelatedLaborActivity, true, false);
      
      #endregion
    }

  }
}