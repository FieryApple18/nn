using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;
using DocKind = DirRX.NNVacations.Constants.Module.DocumentKind;
using DocType = HRPro.HRDocFlow.PublicConstants.Module;

namespace DirRX.NNVacations.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateDocumentKinds();
      CreateConstantsGroup();
      this.CreateConstants();
      GrantRightsOnFolders();
      GrantRightsOnReports();
      GrantRightsOnDataBooks();
      CreateReportTables();
      FillBusinessProcessInDocKinds();
    }
    
    /// <summary>
    /// Создание видов документов.
    /// </summary>
    private static void CreateDocumentKinds()
    {
      InitializationLogger.Debug("Init Vacations module: Create document kinds.");

      var notNumerable = Sungero.Docflow.DocumentKind.NumberingType.NotNumerable;
      var registrable = Sungero.Docflow.DocumentKind.NumberingType.Registrable;
      var innerDocumentFlow = Sungero.Docflow.DocumentRegister.DocumentFlow.Inner;
      
      #region Бизнес-процессы.
      
      // КЭДО.
      var businessProcessKEDO = NNCommons.PublicFunctions.Module.CreateBusinessProcess(NNCommons.Resources.BusinessProcessKEDO, null);
      // Отпуска.
      var businessProcessVacations = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessVacations, businessProcessKEDO);
      // Ведение графика отпусков.
      var vacationScheduleManagement = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessVacationScheduleManagement, businessProcessVacations);
      // Компенсация части ежегодного оплачиваемого отпуска.
      var compensationPartAnnualVacation = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessCompensationPartAnnualVacation, businessProcessVacations);
      // Регистрация прочих отпусков.
      var registrationOtherVacation = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessRegistrationOtherVacation, businessProcessVacations);
      // Регистрация ежегодного отпуска.
      var registrationAnnualVacation = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessRegistrationAnnualVacation, businessProcessVacations);
      // Отзыв из отпуска.
      var recallVacation = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessRecallVacation, businessProcessVacations);
      
      #endregion
      
      // Подтверждающий документ к заявлению на ежегодный отпуск
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.SupportingDocumentForAnnualVacationDocKind,
                                                                            DirRX.NNCommons.Resources.SupportingDocumentForAnnualVacationDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, false, false,
                                                                            NNOtherProcesses.PublicConstants.Module.DocumentType.HRSimpleDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            NNCommons.PublicConstants.Module.DocumentKind.SupportingDocumentForAnnualVacation, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationAnnualVacation, false,
                                                                            Constants.Module.SapKindsCodes.SupportingDocumentForAnnualVacationDocKind);
      
      // Подтверждающий документ к заявлению на прочий отпуск
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.SupportingDocumentForOtherVacationDocKind,
                                                                            DirRX.NNCommons.Resources.SupportingDocumentForOtherVacationDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, false, false,
                                                                            NNOtherProcesses.PublicConstants.Module.DocumentType.HRSimpleDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            NNCommons.PublicConstants.Module.DocumentKind.SupportingDocumentForOtherVacation, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationOtherVacation, false,
                                                                            Constants.Module.SapKindsCodes.SupportingDocumentForOtherVacationDocKind);
      
      // Письмо в авиакомпанию
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.AirlineLetterDocKind,
                                                                            DirRX.NNCommons.Resources.AirlineLetterDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            NNOtherProcesses.PublicConstants.Module.DocumentType.HRSimpleDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            NNCommons.PublicConstants.Module.DocumentKind.AirlineLetter, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationAnnualVacation, false,
                                                                            Constants.Module.SapKindsCodes.AirlineLetterDocKind);
      
      // Письмо в ОВД
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.OVDLetterDocKind,
                                                                            DirRX.NNCommons.Resources.OVDLetterDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            NNOtherProcesses.PublicConstants.Module.DocumentType.HRSimpleDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            NNCommons.PublicConstants.Module.DocumentKind.OVDLetter, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationAnnualVacation, false,
                                                                            Constants.Module.SapKindsCodes.OVDLetterDocKind);
      
      // Индивидуальный лист ознакомления с графиком отпусков.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(NNVacations.Resources.IndividualAcquiantanceVacScheduleListKindName,
                                                                            NNVacations.Resources.IndividualAcquiantanceVacScheduleListKindName,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationScheduleDocument, null,
                                                                            DocKind.IndividualAcquiantanceVacScheduleListKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 1, 1, string.Empty, true, true, false, vacationScheduleManagement, false,
                                                                            Constants.Module.SapKindsCodes.IndividualAcquiantanceVacScheduleListKind);
      // Групповой лист ознакомления с графиком отпусков.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(NNVacations.Resources.GroupAcquiantanceVacScheduleListKindName,
                                                                            NNVacations.Resources.GroupAcquiantanceVacScheduleListKindName,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationScheduleDocument, null,
                                                                            DocKind.GroupAcquiantanceVacScheduleListKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 1, 1, string.Empty, true, true, false, vacationScheduleManagement, false,
                                                                            Constants.Module.SapKindsCodes.GroupAcquiantanceVacScheduleListKind);
      // Заявление на прочий отпуск.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(NNVacations.Resources.OtherVacationKindName, NNVacations.Resources.OtherVacationKindName,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument,
                                                                            null, DocKind.OtherVacationKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, 1, string.Empty, true, true, false, registrationOtherVacation, false,
                                                                            Constants.Module.SapKindsCodes.OtherVacationKind);
      
      // Компенсация части ежегодного оплачиваемого отпуска (в том числе дополнительного).
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(NNVacations.Resources.CompensationVacationKindName, NNVacations.Resources.CompensationVacationKindName,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument,
                                                                            null, DocKind.CompensationVacationKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, 1, string.Empty, true, true, false, compensationPartAnnualVacation, false,
                                                                            Constants.Module.SapKindsCodes.CompensationVacationKind);
      
      // Компенсация части ежегодного дополнительного оплачиваемого отпуска (за ВУТ/ОУТ).
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(NNVacations.Resources.CompensationAdditionalVacationKindName, NNVacations.Resources.CompensationAdditionalVacationKindName,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument,
                                                                            null, DocKind.CompensationAdditionalVacationKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, 1, string.Empty, true, true, false, compensationPartAnnualVacation, false,
                                                                            Constants.Module.SapKindsCodes.CompensationAdditionalVacationKind);
      // Приказ на прочий отпуск.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(NNVacations.Resources.OtherVacationOrderKindName, NNVacations.Resources.OtherVacationOrderKindName,
                                                                            registrable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument,
                                                                            null, DocKind.OtherVacationOrderKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, 1, string.Empty, true, true, false, registrationOtherVacation, false,
                                                                            Constants.Module.SapKindsCodes.OtherVacationOrderKind);
      // Приказ на предоставление учебного отпуска.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNVacations.Resources.StudyVacationOrderKindName, DirRX.NNVacations.Resources.StudyVacationOrderKindName,
                                                                            registrable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument,
                                                                            null, DocKind.StudyVacationOrderKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, 1, string.Empty, true, true, false, registrationOtherVacation, false,
                                                                            Constants.Module.SapKindsCodes.StudyVacationOrderKind);
      
      // Приказ о замене части отпуска работнику денежной компенсации.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(NNVacations.Resources.VacationCompensationOrderKindName, NNVacations.Resources.VacationCompensationOrderKindName,
                                                                            registrable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument,
                                                                            null, DocKind.VacationCompensationOrderKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, 1, string.Empty, true, true, true, compensationPartAnnualVacation, false,
                                                                            Constants.Module.SapKindsCodes.VacationCompensationOrderKind);
      
      // Приказ на ежегодный отпуск.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.OverrideSolution.Module.Vacations.Resources.VacationOrder, DirRX.OverrideSolution.Module.Vacations.Resources.VacationOrder,
                                                                            registrable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument,
                                                                            null, HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationOrderKind, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 3, 3, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.OtherDocuments, true, false,
                                                                            false, registrationAnnualVacation, false, Constants.Module.SapKindsCodes.VacationOrderKind);
      // Заявление о переносе отпуска.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.OverrideSolution.Module.Vacations.Resources.VacationChangePeriodStatement, DirRX.OverrideSolution.Module.Vacations.Resources.VacationChangePeriodStatement,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument,
                                                                            null, HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationShiftStatementKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, null, null, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.EmployeesStatementPostponementAnnualPaidLeave, true, true,
                                                                            false, registrationAnnualVacation, false, Constants.Module.SapKindsCodes.VacationChangePeriodStatement);
      
      // Заявление на ежегодный отпуск.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.OverrideSolution.Module.Vacations.Resources.DocKindName_VacationStatement, DirRX.OverrideSolution.Module.Vacations.Resources.DocKindName_VacationStatement,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument,
                                                                            null, HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationStatementKind, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, null, null, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.OtherDocuments, true, true,
                                                                            false, registrationAnnualVacation, false, Constants.Module.SapKindsCodes.VacationStatement);
      
      // Уведомление о начале отпуска.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.OverrideSolution.Module.Vacations.Resources.VacationNotice, DirRX.OverrideSolution.Module.Vacations.Resources.VacationNotice,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationNoticeDocument,
                                                                            null, HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationBeginningNoticeKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, null, null, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.StartTimeOfVacationNotification, true, false,
                                                                            false, registrationAnnualVacation, false, Constants.Module.SapKindsCodes.VacationBeginningNoticeKind);
      // Заявление о досрочном прекращении отпуска.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(HRPro.HRDocFlow.Resources.VacationSelfRecallStatement, HRPro.HRDocFlow.Resources.VacationSelfRecallStatement,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument,
                                                                            null, HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationSelfRecallStatementKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, null, null, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.OtherDocuments, true, true,
                                                                            false, recallVacation, false, Constants.Module.SapKindsCodes.VacationSelfRecallStatement);
      
      // Согласие на отзыв из отпуска.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(HRPro.HRDocFlow.Resources.VacationRecallConsent, HRPro.HRDocFlow.Resources.VacationRecallConsent,
                                                                            notNumerable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument,
                                                                            null, HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallConsentKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, null, null, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.OtherDocuments, true, true,
                                                                            false, recallVacation, false, Constants.Module.SapKindsCodes.VacationRecallConsent);
      
      // Приказ об отзыве из отпуска.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(HRPro.HRDocFlow.Resources.RecallVacationOrder, HRPro.HRDocFlow.Resources.RecallVacationOrder,
                                                                            registrable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument,
                                                                            null, HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallOrderKind, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 3, 1, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.OtherDocuments, true, false,
                                                                            true, recallVacation, false, Constants.Module.SapKindsCodes.RecallVacationOrder);
      // Приказ о переносе сроков отпуска.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNVacations.Resources.ChangeVacationPeriodOrderKindName, DirRX.NNVacations.Resources.ChangeVacationPeriodOrderKindName,
                                                                            registrable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument,
                                                                            null, DocKind.ChangeVacationPeriodOrder, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, null, 1, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.OtherDocuments, true, false,
                                                                            false, registrationAnnualVacation, false, Constants.Module.SapKindsCodes.ChangeVacationPeriodOrder);
      // Приказ об отмене приказа.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNVacations.Resources.CancelVacationOrderKindName, DirRX.NNVacations.Resources.CancelVacationOrderKindName,
                                                                            registrable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument,
                                                                            null, DocKind.CancelVacationOrder, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, null, 1, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.OtherDocuments, true, false,
                                                                            false, registrationAnnualVacation, false, Constants.Module.SapKindsCodes.CancelVacationOrder);
      
      // Приказ (распоряжение) об изменении сроков отпуска без сохранения заработной платы.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNVacations.Resources.ChangingDatesVacationOrderKindName, DirRX.NNVacations.Resources.ChangingDatesVacationOrderKindName,
                                                                            registrable, innerDocumentFlow, true, false, HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] { Sungero.Docflow.OfficialDocuments.Info.Actions.SendActionItem, 
                                                                                                                      Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval, 
                                                                                                                      Sungero.Docflow.OfficialDocuments.Info.Actions.SendForDocumentFlow },
                                                                            DocKind.ChangingDatesVacationOrder, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 1, 1, HRPro.HRDocFlow.PublicConstants.Module.DocumentСodes.OtherDocuments, true, false,
                                                                            false, registrationAnnualVacation, false, Constants.Module.SapKindsCodes.ChangingDatesVacationOrder);
    }
    
    /// <summary>
    /// Создать прикладные константы.
    /// </summary>
    public virtual void CreateConstants()
    {
      InitializationLogger.Debug("Init Vacations module: Create сonstants.");
      
      // Отпуска. Код типа отпуска Учебный (с сохранением ЗП).
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.StudyWithSalaryVacationTypeId.Name, true, string.Empty,
                                                             Constants.Module.ConstantsSetting.StudyWithSalaryVacationTypeId.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Код типа отпуска Учебный (без сохранения ЗП).
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.StudyWithoutSalaryVacationTypeId.Name, true, string.Empty,
                                                             Constants.Module.ConstantsSetting.StudyWithoutSalaryVacationTypeId.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Список ИД причин отпуска, для которых необходимо указывать уточнение причины.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.VacationReasonsWitnClarificationIds.Name, true, string.Empty,
                                                             Constants.Module.ConstantsSetting.VacationReasonsWitnClarificationIds.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Кол-во сотрудников для формирования индивидуальных листов ознакомления.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.EmployeesAmountForIndividualAcquaintanceList.Name, true,
                                                             Constants.Module.ConstantsDefaultValues.EmployeesAmountForIndividualAcquiantanceList,
                                                             Constants.Module.ConstantsSetting.EmployeesAmountForIndividualAcquaintanceList.Guid,
                                                             Constants.Module.ConstantsGroup.MaintenanceOfVacationSchedule.Guid);
      
      // Формат приказа о ежегодном отпуске по заявлению.
      DirRX.NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.AnnualVacationDocumentExtension.Name, true, "doc",
                                                                   Constants.Module.ConstantsSetting.AnnualVacationDocumentExtension.Guid,
                                                                   Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Формат приказа о прочем отпуске по заявлению.
      DirRX.NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.OtherVacationDocumentExtension.Name, true, "doc",
                                                                   Constants.Module.ConstantsSetting.OtherVacationDocumentExtension.Guid,
                                                                   Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Формат графика отпусков.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.VacationScheduleDocumentExtension.Name, true, "pdf",
                                                             Constants.Module.ConstantsSetting.VacationScheduleDocumentExtension.Guid,
                                                             Constants.Module.ConstantsGroup.MaintenanceOfVacationSchedule.Guid);
      
      // Формат файла для документов "Приказ об отзыве из отпуска" и "Приказ о замене части отпуска работнику денежной компенсации", принимаемых из АСУП.
      DirRX.NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.RecallAndCompensationVacationDocumentExtension.Name, true, "doc",
                                                                   Constants.Module.ConstantsSetting.RecallAndCompensationVacationDocumentExtension.Guid,
                                                                   Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Количество обрабатываемых записей в АО "Сформировать листы ознакомления" в одной итерации.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.CountCreateAcquiantanceListsInIteration.Name, true,
                                                             Constants.Module.ConstantsDefaultValues.CountCreateAcquiantanceListsInIteration,
                                                             Constants.Module.ConstantsSetting.CountCreateAcquiantanceListsInIteration.Guid,
                                                             Constants.Module.ConstantsGroup.MaintenanceOfVacationSchedule.Guid);
      
      // Отпуска. Количество обрабатываемых записей в АО "Создать и запустить задачу Ведение графика отпусков" в одной итерации.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.CountCreateVacationInfoAndStartTaskInIteration.Name, true,
                                                             Constants.Module.ConstantsDefaultValues.CountCreateVacationInfoAndStartTaskInIteration,
                                                             Constants.Module.ConstantsSetting.CountCreateVacationInfoAndStartTaskInIteration.Guid,
                                                             Constants.Module.ConstantsGroup.MaintenanceOfVacationSchedule.Guid);
      
      // Отпуска. Текст инструкции по прикреплению документов.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.InstructionForAttachingDocuments.Name, true,
                                                             Constants.Module.ConstantsDefaultValues.InstructionForAttachingDocuments,
                                                             Constants.Module.ConstantsSetting.InstructionForAttachingDocuments.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Коды плановых отпусков.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.PlannedVacationsCodes.Name, true, string.Empty,
                                                             Constants.Module.ConstantsSetting.PlannedVacationsCodes.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Коды зарегистрированных отпусков.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.RegisteredVacationsCodes.Name, true, string.Empty,
                                                             Constants.Module.ConstantsSetting.RegisteredVacationsCodes.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Коды ежегодных отпусков.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.AnnualVacationsCodes.Name, true, string.Empty,
                                                             Constants.Module.ConstantsSetting.AnnualVacationsCodes.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Коды плановых отпусков.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.PlannedVacationsCodes.Name, true, string.Empty,
                                                             Constants.Module.ConstantsSetting.PlannedVacationsCodes.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Коды зарегистрированных отпусков.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.RegisteredVacationsCodes.Name, true, string.Empty,
                                                             Constants.Module.ConstantsSetting.RegisteredVacationsCodes.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. ИД варианта процесса для уведомления ответственных за ошибки настройки сроков согласования отпусков.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.VacationApprovalDateNotificationProcessId.Name, true, (long)0,
                                                             Constants.Module.ConstantsSetting.VacationApprovalDateNotificationProcessId.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
      
      // Отпуска. Дополнительные выходные дни донорам.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.DonorVacationsCodes.Name, true, string.Empty,
                                                             Constants.Module.ConstantsSetting.DonorVacationsCodes.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
    }
    
    
    /// <summary>
    /// Создать группы констант.
    /// </summary>
    public virtual void CreateConstantsGroup()
    {
      InitializationLogger.Debug("Init NNCommons module: create new Constants Group.");
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.MaintenanceOfVacationSchedule.Name,
                                                            Constants.Module.ConstantsGroup.MaintenanceOfVacationSchedule.Guid);
      
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.RegistrationOfVacations.Name,
                                                            Constants.Module.ConstantsGroup.RegistrationOfVacations.Guid);
    }
    
    #region Выдача прав доступа.
    
    /// <summary>
    /// Выдать права на папки.
    /// </summary>
    public static void GrantRightsOnFolders()
    {
      InitializationLogger.Debug("Init Vacations module: Grant rights on folders.");
      
      SpecialFolders.VacationScheduleAcquaintanceLists.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      SpecialFolders.VacationScheduleAcquaintanceLists.AccessRights.Save();
    }
    
    /// <summary>
    /// Выдать права на отчеты.
    /// </summary>
    public static void GrantRightsOnReports()
    {
      InitializationLogger.Debug("Init Vacations module: Grant rights on reports.");
      
      Reports.AccessRights.Grant(Reports.GetGroupAcquiantanceVacScheduleReport().Info, Roles.AllUsers, DefaultReportAccessRightsTypes.Execute);
      Reports.AccessRights.Grant(Reports.GetAcquiantanceVacScheduleReport().Info, Roles.AllUsers, DefaultReportAccessRightsTypes.Execute);
    }
    
    /// <summary>
    /// Выдать права на справочники.
    /// </summary>
    public static void GrantRightsOnDataBooks()
    {
      InitializationLogger.Debug("Init Vacations module: Grant rights on databooks.");
      
      // Cправочник "Причины отпуска".
      NNVacations.VacationReasonses.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      NNVacations.VacationReasonses.AccessRights.Save();
      
      // Cправочник "Срок согласования отпусков".
      NNVacations.VacationsApprovalPeriods.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      NNVacations.VacationsApprovalPeriods.AccessRights.Save();
    }
    
    #endregion
    
    /// <summary>
    /// Создать таблицы отчётов.
    /// </summary>
    public static void CreateReportTables()
    {
      Sungero.Docflow.PublicFunctions.Module.DropReportTempTables(new []
                                                                  {
                                                                    Constants.AcquiantanceVacScheduleReport.SourceTableName
                                                                  });
      
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.AcquiantanceVacScheduleReport.CreateDepartmentsTempTable,
                                                                     new [] { Constants.AcquiantanceVacScheduleReport.SourceTableName });
    }
    
    #region Заполнение свойства Бизнес-процесс.
    
    /// <summary>
    /// Заполнить свойство Бизнес-процесс в записях справочника Виды документов.
    /// </summary>
    public static void FillBusinessProcessInDocKinds()
    {
      var businessProcessKEDO = NNCommons.PublicFunctions.Module.CreateBusinessProcess(NNCommons.Resources.BusinessProcessKEDO, null);
      var businessProcessVacations = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessVacations, businessProcessKEDO);
      
      #region Ведение графика отпусков.
      
      var vacationScheduleManagement = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessVacationScheduleManagement, businessProcessVacations);
      // График отпусков.
      FillBusinessProcessInDocKind(Constants.Module.DocumentKind.VacationScheduleKind, vacationScheduleManagement);
      
      #endregion
      
      #region Отзыв из отпуска.
      
      var recallVacation = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessRecallVacation, businessProcessVacations);
      // Заявление на досрочное прерывание отпуска.
      FillBusinessProcessInDocKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationSelfRecallStatementKind, recallVacation);
      // Согласие на отзыв из отпуска.
      FillBusinessProcessInDocKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallConsentKind, recallVacation);
      // Приказ об отзыве из отпуска.
      FillBusinessProcessInDocKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallOrderKind, recallVacation);
      
      #endregion
      
      #region Регистрация ежегодного отпуска.
      
      var registrationAnnualVacation = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessRegistrationAnnualVacation, businessProcessVacations);
      // Простой документ.
      FillBusinessProcessInDocKind(Sungero.Docflow.PublicConstants.Module.Initialize.SimpleDocumentKind, registrationAnnualVacation);
      
      #endregion
    }
    
    /// <summary>
    /// Заполнить свойство Бизнес-процесс в записи справочника Вид документа.
    /// </summary>
    /// <param name="docKindGuid">Guid вида документа.</param>
    /// <param name="businessProcess">Бизнес-процесс.</param>
    public static void FillBusinessProcessInDocKind(Guid docKindGuid, NNCommons.IBusinessProcess businessProcess)
    {
      if (businessProcess == null)
        return;
      
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(docKindGuid);
      var overrideDocKind = DirRX.OverrideSolution.DocumentKinds.As(docKind);
      if (overrideDocKind == null)
        return;
      
      if (!Equals(overrideDocKind.BusinessProcessDirRX, businessProcess))
      {
        overrideDocKind.BusinessProcessDirRX = businessProcess;
        overrideDocKind.Save();
      }
      NNCommons.PublicFunctions.Module.CreateBusinessRole(NNCommons.BusinessRole.RoleType.ByKind, null, overrideDocKind, true);
      NNCommons.PublicFunctions.Module.CreateBusinessRole(NNCommons.BusinessRole.RoleType.ByKind, null, overrideDocKind, false);
    }
    
    #endregion
  }
}
