using System;
using System.Collections.Generic;
using System.Linq;
using DirRX.NNIntegration.ExternalSystemFlow;
using HRPro.HRProSolution;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.CoreEntities.Server;
using Sungero.Domain.Initialization;

namespace DirRX.NNIntegration.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateExternalSystemRecord("КСИП", NNIntegration.Constants.Module.ExternalSystemsGuids.KSIP);
      CreateExternalSystemFlowRecords();
      CreateConstantsGroup();
      CreateConstants();
      CreateSupernikaActions();
      GrantRightsOnDataBooks();
    }
    
    /// <summary>
    /// Создать записи справочника Потоки внешних систем.
    /// </summary>
    public virtual void CreateExternalSystemFlowRecords()
    {
      InitializationLogger.Debug("Init Integration module: Create External systems flows.");
      
      // Система КСИП.
      var ksip = NNIntegration.ExternalSystems.GetAll(s => s.Guid == Constants.Module.ExternalSystemsGuids.KSIP).FirstOrDefault();
      
      // IF-S-9310, IF-S-9331, IF-S-9330. Синхронизация организационной структуры.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CompanySynchronization, Constants.Module.ExternalSystemsFlowDescriptions.CompanySynchronization,
                                     Constants.Module.ExternalSystemsFlowGuids.CompanySynchronization, NNIntegration.ExternalSystemFlow.Direction.Incoming,
                                     ksip, string.Empty, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic,
                                     null, null);
      
      // IF-S-10397. Автоматическое создание карточки документа из АСУП в КЭДО.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CreateRetentionStatementDocument, Constants.Module.ExternalSystemsFlowDescriptions.CreateRetentionStatementDocument,
                                     Constants.Module.ExternalSystemsFlowGuids.CreateRetentionStatementDocument, Direction.Incoming, ksip,
                                     string.Empty, false, FlowKind.Asynchronic,
                                     null, null);
      
      // IF-S-11622. Получение результата асинхронной отправки данных из КСИП в АСУП.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.GetSendingAsyncDataResultFromKSIP, Constants.Module.ExternalSystemsFlowDescriptions.GetSendingAsyncDataResultFromKSIP,
                                     Constants.Module.ExternalSystemsFlowGuids.GetSendingAsyncDataResultFromKSIP, NNIntegration.ExternalSystemFlow.Direction.Incoming,
                                     ksip, string.Empty, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Synchronic,
                                     null, null);
      
      // IF-S-9401. Передача информации о документе из КЭДО в АСУП (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.SendDocumentInSapFiori, Constants.Module.ExternalSystemsFlowNames.SendDocumentInSapFiori,
                                     Constants.Module.ExternalSystemsFlowGuids.SapFioriOutgoing, Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false, FlowKind.Asynchronic,
                                     null, null);
      
      // IF-S-9336. Передача уведомления об отпуске из АСУП в КЭДО.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.SendingVacationNotice, Constants.Module.ExternalSystemsFlowNames.SendingVacationNotice,
                                     Constants.Module.ExternalSystemsFlowGuids.SendingVacationNotice, NNIntegration.ExternalSystemFlow.Direction.Incoming,
                                     ksip, string.Empty, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic,
                                     null, null);
      
      // IF-S-10532. Передача статуса приказа (КЭДО -> АСУП).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.SendStatusDocumentInSap, Constants.Module.ExternalSystemsFlowDescriptions.SendStatusDocumentInSap,
                                     Constants.Module.ExternalSystemsFlowGuids.StatusDocumentInSap, Direction.Outgoing,
                                     ksip, "SendStatusDocumentInSap", false, FlowKind.Asynchronic,
                                     null, null, true);
      
      // IF-S-10408. Передача приказа на предоставление отпуска из АСУП в КЭДО (АСУП -> КЭДО).
      var annualVacationDocKinds = new List<IDocumentKind>();
      var annualVacationKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationOrderKind);
      if (annualVacationKind != null)
        annualVacationDocKinds.Add(DocumentKinds.As(annualVacationKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CreateAnnualVacationOrder, Constants.Module.ExternalSystemsFlowDescriptions.CreateAnnualVacationOrder,
                                     Constants.Module.ExternalSystemsFlowGuids.CreateAnnualVacationOrder, Direction.Incoming, ksip,
                                     string.Empty, false, FlowKind.Asynchronic,
                                     null, annualVacationDocKinds);
      
      // IF-S-10411. Передача приказа на предоставление прочего отпуска из АСУП в КЭДО (АСУП -> КЭДО).
      var otherVacationDocKinds = new List<IDocumentKind>();
      var otherVacationKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNVacations.PublicConstants.Module.DocumentKind.OtherVacationOrderKind);
      if (otherVacationKind != null)
        otherVacationDocKinds.Add(DocumentKinds.As(otherVacationKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CreateOtherVacationOrder, Constants.Module.ExternalSystemsFlowDescriptions.CreateOtherVacationOrder,
                                     Constants.Module.ExternalSystemsFlowGuids.CreateOtherVacationOrder, Direction.Incoming, ksip,
                                     string.Empty, false, FlowKind.Asynchronic,
                                     null, otherVacationDocKinds);
      
      // IF-S-10396. Передача приказа на компенсацию части ежегодного оплачиваемого отпуска из АСУП в КЭДО (АСУП -> КЭДО).
      var vacationCompemsationDocKinds = new List<IDocumentKind>();
      var vacationCompemsationDocKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNVacations.PublicConstants.Module.DocumentKind.VacationCompensationOrderKind);
      if (vacationCompemsationDocKind != null)
        vacationCompemsationDocKinds.Add(DocumentKinds.As(vacationCompemsationDocKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CreateVacationCompensationOrder, Constants.Module.ExternalSystemsFlowDescriptions.CreateVacationCompensationOrder,
                                     Constants.Module.ExternalSystemsFlowGuids.CreateVacationCompensationOrder, Direction.Incoming, ksip,
                                     string.Empty, false, FlowKind.Asynchronic,
                                     null, vacationCompemsationDocKinds);
      
      // IF-S-10391. Передача приказа об отзыве из отпуска из АСУП в КЭДО (АСУП -> КЭДО).
      var recallVacationDocKinds = new List<IDocumentKind>();
      var recallVacationKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallOrderKind);
      if (recallVacationKind != null)
        recallVacationDocKinds.Add(DocumentKinds.As(recallVacationKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CreateRecallVacationOrder, Constants.Module.ExternalSystemsFlowDescriptions.CreateRecallVacationOrder,
                                     Constants.Module.ExternalSystemsFlowGuids.CreateRecallVacationOrder, Direction.Incoming, ksip,
                                     string.Empty, false, FlowKind.Asynchronic,
                                     null, recallVacationDocKinds);
      
      // IF-S-10398. Автоматическое формирование задания на ручное разблокирование ИТ0015 в АСУП (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.ManualHoldingEvent, Constants.Module.ExternalSystemsFlowDescriptions.ManualHoldingEvent,
                                     Constants.Module.ExternalSystemsFlowGuids.ManualHoldingEvent, NNIntegration.ExternalSystemFlow.Direction.Incoming,
                                     ksip, string.Empty, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic, OverrideSolution.Employees.Null,
                                     null);
      
      // IF-S-6365. Создание карточки согласования графика отпусков из АСУП в КЭДО.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CreateVacationScheduleDocument, Constants.Module.ExternalSystemsFlowDescriptions.CreateVacationScheduleDocument,
                                     Constants.Module.ExternalSystemsFlowGuids.CreateVacationScheduleDocument, Direction.Incoming, ksip,
                                     string.Empty, false, FlowKind.Asynchronic,
                                     null, null);
      
      // IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CreateBusinessTripOrder, Constants.Module.ExternalSystemsFlowDescriptions.CreateBusinessTripOrder,
                                     Constants.Module.ExternalSystemsFlowGuids.CreateBusinessTripOrder, NNIntegration.ExternalSystemFlow.Direction.Incoming,
                                     ksip, string.Empty, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic, OverrideSolution.Employees.Null,
                                     null);

      // IF-S-10403. Передача данных о доступных остатках отпуска сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.GetVacationAvailableLimits, Constants.Module.ExternalSystemsFlowDescriptions.GetVacationAvailableLimits,
                                     Constants.Module.ExternalSystemsFlowGuids.GetVacationAvailableLimits, NNIntegration.ExternalSystemFlow.Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Synchronic, OverrideSolution.Employees.Null,
                                     null, true);
      
      // IF-S-10384. Получение данных о доступном количестве дней на компенсацию из АСУП в КЭДО (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.GetCompensationAvailableDays, Constants.Module.ExternalSystemsFlowDescriptions.GetCompensationAvailableDays,
                                     Constants.Module.ExternalSystemsFlowGuids.GetCompensationAvailableDays, NNIntegration.ExternalSystemFlow.Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Synchronic, OverrideSolution.Employees.Null,
                                     null, true);
      
      // IF-S-10392. Получение данных о текущем отпуске сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.GetCurrentVacationData, Constants.Module.ExternalSystemsFlowDescriptions.GetCurrentVacationData,
                                     Constants.Module.ExternalSystemsFlowGuids.GetCurrentVacationData, NNIntegration.ExternalSystemFlow.Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Synchronic, OverrideSolution.Employees.Null,
                                     null, true);
      
      // IF-S-10405. Проверка данных при оформлении заявления на отпуск (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.ValidateEmployeeVacations, Constants.Module.ExternalSystemsFlowDescriptions.ValidateEmployeeVacations,
                                     Constants.Module.ExternalSystemsFlowGuids.ValidateEmployeeVacations, NNIntegration.ExternalSystemFlow.Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Synchronic, OverrideSolution.Employees.Null,
                                     null, true);
      
      // IF-S-10394. Проверка данных в АСУП для оформления отзыва из отпуска (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.ValidateVacationRecall, Constants.Module.ExternalSystemsFlowDescriptions.ValidateVacationRecall,
                                     Constants.Module.ExternalSystemsFlowGuids.ValidateVacationRecall, NNIntegration.ExternalSystemFlow.Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Synchronic, OverrideSolution.Employees.Null,
                                     null, true);
      
      // IF-S-11187. Проверка данных в АСУП для заявления на компенсацию части ежегодного отпуска (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.ValidateVacationCompensation, Constants.Module.ExternalSystemsFlowDescriptions.ValidateVacationCompensation,
                                     Constants.Module.ExternalSystemsFlowGuids.ValidateVacationCompensation, NNIntegration.ExternalSystemFlow.Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Synchronic, OverrideSolution.Employees.Null,
                                     null, true);
      
      // IF-S-10409. Проверка заявления на предоставление прочего отпуска (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.ValidateEmployeeOtherVacations, Constants.Module.ExternalSystemsFlowDescriptions.ValidateEmployeeOtherVacations,
                                     Constants.Module.ExternalSystemsFlowGuids.ValidateEmployeeOtherVacations, NNIntegration.ExternalSystemFlow.Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Synchronic, OverrideSolution.Employees.Null,
                                     null, true);
      
      // IF-S-10406. Проверка заявления на перенос отпуска (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.ValidateEmployeeShiftVacations, Constants.Module.ExternalSystemsFlowDescriptions.ValidateEmployeeShiftVacations,
                                     Constants.Module.ExternalSystemsFlowGuids.ValidateEmployeeShiftVacations, NNIntegration.ExternalSystemFlow.Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Synchronic, OverrideSolution.Employees.Null,
                                     null, true);
      
      // IF-S-7601. Передача документа на проштамповку.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.SendDocumentToStamping, Constants.Module.ExternalSystemsFlowDescriptions.SendDocumentToStamping,
                                     Constants.Module.ExternalSystemsFlowGuids.SendDocumentToStamping, NNIntegration.ExternalSystemFlow.Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic, OverrideSolution.Employees.Null,
                                     null, false, true);
      
      // IF-S-7538. Получение результата проштамповки.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.GetResultStamping, Constants.Module.ExternalSystemsFlowDescriptions.GetResultStamping,
                                     Constants.Module.ExternalSystemsFlowGuids.GetResultStamping, NNIntegration.ExternalSystemFlow.Direction.Incoming,
                                     ksip, string.Empty, false,
                                     NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic, OverrideSolution.Employees.Null,
                                     null);
      
      // IF-S-10536. Формирование проводки на перенос отпуска из КЭДО в АСУП (КЭДО -> АСУП).
      var vacationShiftStatementDocKinds = new List<IDocumentKind>();
      var vacationShiftStatementKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationShiftStatementKind);
      if (vacationShiftStatementKind != null)
        vacationShiftStatementDocKinds.Add(DocumentKinds.As(vacationShiftStatementKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.RegistrationShiftVacation, Constants.Module.ExternalSystemsFlowDescriptions.RegistrationShiftVacation,
                                     Constants.Module.ExternalSystemsFlowGuids.RegistrationShiftVacation, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, FlowKind.Asynchronic,
                                     null, vacationShiftStatementDocKinds, true);
      
      // IF-S-6372. Регистрация ежегодного отпуска (по заявлению).
      var vacationStatementDocKinds = new List<IDocumentKind>();
      var vacationStatementKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationStatementKind);
      if (vacationStatementKind != null)
        vacationStatementDocKinds.Add(DocumentKinds.As(vacationStatementKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.RegistrationAnnualVacationByStatement, Constants.Module.ExternalSystemsFlowDescriptions.RegistrationAnnualVacationByStatement,
                                     Constants.Module.ExternalSystemsFlowGuids.RegistrationAnnualVacationByStatement, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, FlowKind.Asynchronic,
                                     null, vacationStatementDocKinds, true);
      
      // IF-S-6376. Регистрация прочего отпуска.
      var otherVacationStatementDocKinds = new List<IDocumentKind>();
      var otherVacationStatementKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNVacations.PublicConstants.Module.DocumentKind.OtherVacationKind);
      if (otherVacationStatementKind != null)
        otherVacationStatementDocKinds.Add(DocumentKinds.As(otherVacationStatementKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.RegistrationOtherVacation, Constants.Module.ExternalSystemsFlowDescriptions.RegistrationOtherVacation,
                                     Constants.Module.ExternalSystemsFlowGuids.RegistrationOtherVacation, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, FlowKind.Asynchronic,
                                     null, otherVacationStatementDocKinds, true);
      
      // IF-S-6373. Регистрация ежегодного отпуска (по графику отпусков).
      var vacationBeginningNoticeDocKinds = new List<IDocumentKind>();
      var vacationBeginningNoticeKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationBeginningNoticeKind);
      if (vacationBeginningNoticeKind != null)
        vacationBeginningNoticeDocKinds.Add(DocumentKinds.As(vacationBeginningNoticeKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.RegistrationAnnualVacationBySchedule, Constants.Module.ExternalSystemsFlowDescriptions.RegistrationAnnualVacationBySchedule,
                                     Constants.Module.ExternalSystemsFlowGuids.RegistrationAnnualVacationBySchedule, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, FlowKind.Asynchronic,
                                     null, vacationBeginningNoticeDocKinds, true);
      
      // IF-S-25553. Передача статусов отправки уведомлений об отпуске из КЭДО в АСУП (КЭДО -> АСУП).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.SendStatusVacationNoticeInSap, Constants.Module.ExternalSystemsFlowNames.SendStatusVacationNoticeInSap,
                                     Constants.Module.ExternalSystemsFlowGuids.SendStatusVacationNoticeInSap, Direction.Outgoing,
                                     ksip, Resources.NeedFillActionName, false, FlowKind.Synchronic,
                                     null, null);
      // IF-S-25401. Передача результата нажатия на действие в центре уведомлений ПМД (ПМД → КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.GetSupernikaActionResult, Constants.Module.ExternalSystemsFlowDescriptions.GetSupernikaActionResult,
                                     Constants.Module.ExternalSystemsFlowGuids.GetSupernikaActionResult, Direction.Incoming, ksip,
                                     string.Empty, false, FlowKind.Asynchronic,
                                     null, null);
      
      // IF-S-6369. Автоматическое создание мероприятия «Компенсация части ежегодного оплачиваемого отпуска» из КЭДО в АСУП.
      var compensationVacationKinds = new List<IDocumentKind>();
      var compensationVacationKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNVacations.PublicConstants.Module.DocumentKind.CompensationVacationKind);
      var compensationAdditionalVacationKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNVacations.PublicConstants.Module.DocumentKind.CompensationAdditionalVacationKind);
      if (compensationVacationKind != null)
        compensationVacationKinds.Add(DocumentKinds.As(compensationVacationKind));
      if (compensationAdditionalVacationKind != null)
        compensationVacationKinds.Add(DocumentKinds.As(compensationAdditionalVacationKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CompensationForPartOfAnnualPaidLeave, Constants.Module.ExternalSystemsFlowDescriptions.CompensationForPartOfAnnualPaidLeave,
                                     Constants.Module.ExternalSystemsFlowGuids.CompensationForPartOfAnnualPaidLeave, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, FlowKind.Asynchronic,
                                     null, compensationVacationKinds, true);
      
      // IF-S-6361. Автоматическое создание мероприятия "Отзыв из отпуска" из КЭДО в АСУП.
      var recallVacationStatementDocKinds = new List<IDocumentKind>();
      var recallVacationStatementKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallConsentKind);
      var selfRecallVacationStatementKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationSelfRecallStatementKind);
      if (recallVacationStatementKind != null)
        recallVacationStatementDocKinds.Add(DocumentKinds.As(recallVacationStatementKind));
      if (selfRecallVacationStatementKind != null)
        recallVacationStatementDocKinds.Add(DocumentKinds.As(selfRecallVacationStatementKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.RegistrationRecallVacation, Constants.Module.ExternalSystemsFlowDescriptions.RegistrationRecallVacation,
                                     Constants.Module.ExternalSystemsFlowGuids.RegistrationRecallVacation, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, FlowKind.Asynchronic,
                                     null, recallVacationStatementDocKinds, true);
      
      // IF-S-10412. Автоматическая проверка заявления на работу в выходной и/или праздничный день из КЭДО в АСУП.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.ValidateWeekendWorkStatement, Constants.Module.ExternalSystemsFlowDescriptions.ValidateWeekendWorkStatement,
                                     Constants.Module.ExternalSystemsFlowGuids.ValidateWeekendWorkStatement, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, NNIntegration.ExternalSystemFlow.FlowKind.Synchronic,
                                     OverrideSolution.Employees.Null, null, false);
      
      // IF-S-10413. Передача приказа на выход сотрудника в выходной и/или праздничный день из АСУП в КЭДО (АСУП -> КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.CreateWeekendWorkOrderDocument, Constants.Module.ExternalSystemsFlowDescriptions.CreateWeekendWorkOrderDocument,
                                     Constants.Module.ExternalSystemsFlowGuids.CreateWeekendWorkOrderDocument, Direction.Incoming, ksip,
                                     Resources.NeedFillActionName, false, NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic,
                                     OverrideSolution.Employees.Null, null, false);
      
      // IF-S-6363. Автоматическое создание мероприятия «Регистрация работы в выходной и/или праздничный день» из КЭДО в АСУП (КЭДО -> АСУП).
      var weekendWorkStatementKinds = new List<IDocumentKind>();
      var weekendWorkStatementKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNOtherProcesses.PublicConstants.Module.DocumentKind.WeekendWorkStatementDocument);
      var overtimeWorkStatementKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNOtherProcesses.PublicConstants.Module.DocumentKind.OvertimeWorkStatementDocument);
      if (weekendWorkStatementKind != null)
        weekendWorkStatementKinds.Add(DocumentKinds.As(weekendWorkStatementKind));
      if (overtimeWorkStatementKind != null)
        weekendWorkStatementKinds.Add(DocumentKinds.As(overtimeWorkStatementKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.WeekendWorkEvent, Constants.Module.ExternalSystemsFlowDescriptions.WeekendWorkEvent,
                                     Constants.Module.ExternalSystemsFlowGuids.WeekendWorkEvent, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic,
                                     OverrideSolution.Employees.Null, weekendWorkStatementKinds, true);
      
      // IF-S-6362. Автоматическое внесение изменений в действующий приказ из КЭДО в АСУП (КЭДО -> АСУП).
      var weekendWorkOrderKinds = new List<IDocumentKind>();
      var weekendWorkOrderKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNOtherProcesses.PublicConstants.Module.DocumentKind.WeekendWorkOrderDocument);
      var overtimeWorkOrderKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNOtherProcesses.PublicConstants.Module.DocumentKind.OvertimeWorkOrderDocument);
      if (weekendWorkOrderKind != null)
        weekendWorkOrderKinds.Add(DocumentKinds.As(weekendWorkOrderKind));
      if (overtimeWorkOrderKind != null)
        weekendWorkOrderKinds.Add(DocumentKinds.As(overtimeWorkOrderKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.WeekendWorkCancelationEvent, Constants.Module.ExternalSystemsFlowDescriptions.WeekendWorkCancelationEvent,
                                     Constants.Module.ExternalSystemsFlowGuids.WeekendWorkCancelationEvent, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic,
                                     OverrideSolution.Employees.Null, weekendWorkOrderKinds, true);
      
      // IF-S-6366. Автоматическое создание мероприятия «Регистрация выплат ЛНА» из КЭДО в АСУП (КЭДО → АСУП).
      var lrdPaymentKinds = new List<IDocumentKind>();
      var lrdPaymentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNOtherProcesses.PublicConstants.Module.DocumentKind.LrdPaymentStatementDocument);
      if (lrdPaymentKind != null)
        lrdPaymentKinds.Add(DocumentKinds.As(lrdPaymentKind));
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.LrdPaymentEvent, Constants.Module.ExternalSystemsFlowDescriptions.LrdPaymentEvent,
                                     Constants.Module.ExternalSystemsFlowGuids.LrdPaymentEvent, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic,
                                     OverrideSolution.Employees.Null, lrdPaymentKinds, true);
      
      // IF-S-10400. Проверка данных по заявлению на выплату ЛНА (АСУП → КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.LrdPaymentCheck, Constants.Module.ExternalSystemsFlowDescriptions.LrdPaymentCheck,
                                     Constants.Module.ExternalSystemsFlowGuids.LrdPaymentCheck, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, false, NNIntegration.ExternalSystemFlow.FlowKind.Synchronic,
                                     OverrideSolution.Employees.Null, null, false);
      
      // IF-S-25360. Получение списка детей работника. (АСУП → КЭДО).
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.LrdPaymentChildren, Constants.Module.ExternalSystemsFlowDescriptions.LrdPaymentChildren,
                                     Constants.Module.ExternalSystemsFlowGuids.LrdPaymentChildren, Direction.Outgoing, ksip,
                                     Resources.NeedFillActionName, true, NNIntegration.ExternalSystemFlow.FlowKind.Synchronic,
                                     OverrideSolution.Employees.Null, null, false);
      
      // IF-S-10399. Передача приказа на выплату по ЛНА из АСУП в КЭДО.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.LrdPaymentOrder, Constants.Module.ExternalSystemsFlowDescriptions.LrdPaymentOrder,
                                     Constants.Module.ExternalSystemsFlowGuids.LrdPaymentOrder, Direction.Incoming, ksip,
                                     string.Empty, false, NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic,
                                     OverrideSolution.Employees.Null, null, false);
      
      // IF-S-28097. Синхронизация календарей рабочего времени из АСУП в КЭДО.
      CreateExternalSystemFlowRecord(Constants.Module.ExternalSystemsFlowNames.UpdateOrCreateWorkingTimeCalendar, Constants.Module.ExternalSystemsFlowDescriptions.UpdateOrCreateWorkingTimeCalendar,
                                     Constants.Module.ExternalSystemsFlowGuids.UpdateOrCreateWorkingTimeCalendar, Direction.Incoming, ksip,
                                     string.Empty, false, NNIntegration.ExternalSystemFlow.FlowKind.Asynchronic,
                                     OverrideSolution.Employees.Null, null, false);
    }
    
    /// <summary>
    /// Создать запись справочника Внешняя система.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="guid">Guid записи.</param>
    [Public]
    public virtual void CreateExternalSystemRecord(string name, string guid)
    {
      InitializationLogger.Debug("Init Integration module: Create External system.");
      
      if (!NNIntegration.ExternalSystems.GetAll().Any(s => s.Guid == guid))
      {
        var externalSystem = NNIntegration.ExternalSystems.Create();
        externalSystem.Name = name;
        externalSystem.Guid = guid;
        externalSystem.Save();
      }
    }
    
    /// <summary>
    /// Создать запись справочника Потоки внешних систем.
    /// </summary>
    /// <param name="name">Имя - содержит код и имя потока, например "IF-S-9310. Синхронизация справочника Подразделения".</param>
    /// <param name="description">Описание.</param>
    /// <param name="flowGuid">Guid потока.</param>
    /// <param name="direction">Направление.</param>
    /// <param name="externalSystem">Внешняя система.</param>
    /// <param name="actionName">Имя действия.</param>
    /// <param name="clearAfterProcessing">Очищать после обработки.</param>
    /// <param name="flowKind">Вид потока.</param>
    /// <param name="responsibleForErrors">Ответственный за ошибки.</param>
    /// <param name="documentKinds">Виды документов.</param>
    /// <param name="waitResponse">Ожидать ответа внешней системы.</param>
    /// <param name="IsMultipartFormData">Многокомпонентная отправка сообщения.</param>
    [Public]
    public virtual void CreateExternalSystemFlowRecord(string name, string description, string flowGuid, Nullable<Enumeration> direction,
                                                       IExternalSystem externalSystem, string actionName, bool clearAfterProcessing,
                                                       Nullable<Enumeration> flowKind, OverrideSolution.IEmployee responsibleForErrors,
                                                       System.Collections.Generic.IEnumerable<HRPro.HRProSolution.IDocumentKind> documentKinds,
                                                       bool waitResponse = false, bool IsMultipartFormData = false)
    {
      if (!NNIntegration.ExternalSystemFlows.GetAll().Any(s => s.FlowGuid == flowGuid))
      {
        var flowexternalSystem = NNIntegration.ExternalSystemFlows.Create();
        flowexternalSystem.Name = name;
        flowexternalSystem.Description = description;
        flowexternalSystem.FlowGuid = flowGuid;
        flowexternalSystem.Direction = direction;
        flowexternalSystem.ExternalSystem = externalSystem;
        flowexternalSystem.ActionName = actionName;
        flowexternalSystem.ClearAfterProcessing = clearAfterProcessing;
        flowexternalSystem.FlowKind = flowKind;
        flowexternalSystem.ResponsibleForErrors = responsibleForErrors;
        flowexternalSystem.WaitResponse = waitResponse;
        flowexternalSystem.IsMultipartFormData = IsMultipartFormData;
        if (documentKinds != null && documentKinds.Any())
        {
          foreach (var docKind in documentKinds)
            flowexternalSystem.DocumentKinds.AddNew().DocumentKind = docKind;
        }
        flowexternalSystem.Save();
      }
    }
    
    /// <summary>
    /// Создать записи справочника Прикладные константы.
    /// </summary>
    public virtual void CreateConstants()
    {
      InitializationLogger.Debug("Init Integration module: Create constants.");
      
      // Интеграция. Количество обрабатываемых записей в АО "Создать и отправить исходящие сообщения" в одной итерации.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.CountCreateOutgoingMessageInIteration.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.CountCreateOutgoingMessageInIteration,
                                                             Constants.Module.ConstantsSetting.CountCreateOutgoingMessageInIteration.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. Количество дней для хранения записей в справочниках "Очереди синхронизации орг. структуры".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.CompanyQueuesClearingJobDays.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.CompanyQueuesClearingJobDays,
                                                             Constants.Module.ConstantsSetting.CompanyQueuesClearingJobDays.Guid,
                                                             Constants.Module.ConstantsGroup.SynchronizationOfOrganizationalStructure.Guid);
      
      // Интеграция. Оповещать о задержках синхронизации орг. структуры в рабочих днях.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.UnmodifiedBusinessUnitDays.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.UnmodifiedBusinessUnitDays,
                                                             Constants.Module.ConstantsSetting.UnmodifiedBusinessUnitDays.Guid,
                                                             Constants.Module.ConstantsGroup.SynchronizationOfOrganizationalStructure.Guid);
      
      // Интеграция. Фиксировать текст запроса в лог-файлах.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.NeedLogKSIPMessagesDetails.Name,
                                                             true,
                                                             true,
                                                             Constants.Module.ConstantsSetting.NeedLogKSIPMessagesDetails.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. ИД группы ответственных за интеграцию.
      var idDefaultGroupResponsible = Roles.Administrators.Id;
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.IdGroupResponibleForIntegrationKSIP.Name,
                                                             true,
                                                             idDefaultGroupResponsible,
                                                             Constants.Module.ConstantsSetting.IdGroupResponibleForIntegrationKSIP.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. Срок хранения интеграционного сообщения.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.IntegrationMessagesCleaningJobDays.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.IntegrationMessagesCleaningJobDays,
                                                             Constants.Module.ConstantsSetting.IntegrationMessagesCleaningJobDays.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. Количество записей для удаления за одну итерацию в ФП "Очистка реестра интеграционных сообщений".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.IntegrationMessagesCleaningJobCount.Name, true,
                                                             Constants.Module.ConstantsDefaultValues.IntegrationMessagesCleaningJobCount,
                                                             Constants.Module.ConstantsSetting.IntegrationMessagesCleaningJobCount.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. Использовать сервис проштамповки по умолчанию.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.IsUseStampingService.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.IsUseStampingService,
                                                             Constants.Module.ConstantsSetting.IsUseStampingService.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithKss.Guid);
      
      // Интеграция. Идентификатор шаблона для проштамповки ПЭП сервиса проштамповки.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.TemplateIdSimpleSignature.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.TemplateIdSimpleSignature,
                                                             Constants.Module.ConstantsSetting.TemplateIdSimpleSignature.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithKss.Guid);
      
      // Интеграция. Идентификатор шаблона для проштамповки УКЭП сервиса проштамповки.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.TemplateIdQualifiedSignature.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.TemplateIdQualifiedSignature,
                                                             Constants.Module.ConstantsSetting.TemplateIdQualifiedSignature.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithKss.Guid);
      
      // Интеграция. Проштамповка. Список ИД вариантов процесса для отправки документов на повторную проштамповку.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.RestampingProcessKindsIds.Name,
                                                             true,
                                                             string.Empty,
                                                             Constants.Module.ConstantsSetting.RestampingProcessKindsIds.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithKss.Guid);
      
      // Интеграция. ИД варианта процесса для уведомления ответственных за ошибки интеграции.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.IntegrationNotificationProcessId.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.IntegrationNotificationProcessId,
                                                             Constants.Module.ConstantsSetting.IntegrationNotificationProcessId.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. Суперника. Шаблон гиперссылки для формирования ссылки на SmartApp КЭДО. Значение "{0}" заменяется на ссылку на объект в ЛК HR Pro.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.HyperLinkTemplateToSmartAppKedo.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.HyperLinkTemplateToSmartAppKedo,
                                                             Constants.Module.ConstantsSetting.HyperLinkTemplateToSmartAppKedo.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Код вида документа АСУП. Заявление о предоставлении учебного отпуска.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.StudyVacationDocKindSAPCode.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.HyperLinkTemplateToSmartAppKedo,
                                                             Constants.Module.ConstantsSetting.StudyVacationDocKindSAPCode.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. Суперника. Шаблон app_id для гиперссылки на SmartApp КЭДО.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.HomescreenExpress.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.KedoSmartappBot,
                                                             Constants.Module.ConstantsSetting.HomescreenExpress.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid
                                                            );
      
      // Интеграция. Суперника. Шаблон текста для гиперссылки на SmartApp КЭДО.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.TextForHyperlink.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.TextTemplateToSmartAppKedo,
                                                             Constants.Module.ConstantsSetting.TextForHyperlink.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);

      // Интеграция. Суперника. Максимальное количество повторов асинхронного обработчика "Асинхронное формирование сообщения через Сервис сообщений".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SendNotificationToSupernikaAsyncHandlerMaxRetry.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.SendNotificationToSupernikaAsyncHandlerMaxRetry,
                                                             Constants.Module.ConstantsSetting.SendNotificationToSupernikaAsyncHandlerMaxRetry.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Число push-уведомлений, которые удаляются в рамках одного запуска фонового процесса.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaNotificationsCleaningNumber.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.SupernikaNotificationsCleaningNumberDefault,
                                                             Constants.Module.ConstantsSetting.SupernikaNotificationsCleaningNumber.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Количество дней для хранения push-уведомлений в Супернику.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsCleaningJobDays.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.SupernikaPushNotificationsCleaningJobDaysDefault,
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsCleaningJobDays.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      

      // Интеграция. Суперника. Адрес сервера СУБД с базой MessageBroker.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBServer.Name,
                                                             true, string.Empty,
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBServer.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Порт сервера СУБД с базой MessageBroker.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBPort.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.SupernikaPushNotificationsDBPort,
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBPort.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Имя пользователя для подключения к серверу СУБД с базой MessageBroker.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBUserName.Name,
                                                             true, "",
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBUserName.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Пароль для подключения к серверу СУБД с базой MessageBroker.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBPassword.Name,
                                                             true, "", true,
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBPassword.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Имя базы данных MessageBroker.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBName.Name,
                                                             true, "",
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsDBName.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Тема задания по умолчанию для push-сообщения.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsAssignmentName.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.SupernikaPushNotificationsAssignmentName,
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsAssignmentName.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Тема уведомления по умолчанию для push-сообщения.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsNoticeName.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.SupernikaPushNotificationsNoticeName,
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsNoticeName.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Количество сообщений обрабатываемых ФП "Отправка сообщений из справочника Push-уведомления в Супернику".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsSendMessageCount.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.SupernikaPushNotificationsSendMessageCount,
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsSendMessageCount.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Суперника. Добавлять дату и время формирования задания в текст Push-уведомления.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SupernikaPushNotificationsIsTimestampRequired.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.SupernikaPushNotificationsIsTimestampRequired,
                                                             Constants.Module.ConstantsSetting.SupernikaPushNotificationsIsTimestampRequired.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      
      // Интеграция. Перевыпуск сертификата УНЭП при синхронизации орг. структуры.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.ReissueCertificateOnOrgStructureSync.Name,
                                                             true, false,
                                                             Constants.Module.ConstantsSetting.ReissueCertificateOnOrgStructureSync.Guid,
                                                             Constants.Module.ConstantsGroup.SynchronizationOfOrganizationalStructure.Guid);
      
      // Интеграция. Проштамповка. Идентификатор шаблона штампа для УНЭП.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.TemplateIdUnqualifiedSignature.Name,
                                                             true, string.Empty,
                                                             Constants.Module.ConstantsSetting.TemplateIdUnqualifiedSignature.Guid,
                                                             Constants.Module.ConstantsGroup.IntegrationWithKss.Guid);
      
      #region Коды видов документов SAP.

      // Интеграция. Код вида документа АСУП. Заявление о предоставлении учебного отпуска.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.StudyVacationDocKindSAPCode.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.StudyVacationDocKindSAPCode,
                                                             Constants.Module.ConstantsSetting.StudyVacationDocKindSAPCode.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. Код вида документа АСУП. Заявление на отпуск (по прочим основаниям).
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.OtherVacationDocKindSAPCode.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.OtherVacationDocKindSAPCode,
                                                             Constants.Module.ConstantsSetting.OtherVacationDocKindSAPCode.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. Код вида документа АСУП. Заявление на предоставление отпуска по КД (работник-инвалид).
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.VacationDisabledEmployeeDocKindSAPCode.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.VacationDisabledEmployeeDocKindSAPCode,
                                                             Constants.Module.ConstantsSetting.VacationDisabledEmployeeDocKindSAPCode.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      
      // Интеграция. Код вида документа АСУП. Заявление на дополнительный отпуск на обустройство по программе Релокация.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.VacationByRelocationDocKindSAPCode.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.VacationByRelocationDocKindSAPCode,
                                                             Constants.Module.ConstantsSetting.VacationByRelocationDocKindSAPCode.Guid,
                                                             Constants.Module.ConstantsGroup.Integration.Guid);
      #endregion
    }
    
    /// <summary>
    /// Создать группы констант.
    /// </summary>
    public virtual void CreateConstantsGroup()
    {
      InitializationLogger.Debug("Init NNCommons module: create new Constants Group.");
      
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.Integration.Name, Constants.Module.ConstantsGroup.Integration.Guid);
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.IntegrationWithKss.Name, Constants.Module.ConstantsGroup.IntegrationWithKss.Guid);
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.IntegrationWithSupernika.Name, Constants.Module.ConstantsGroup.IntegrationWithSupernika.Guid);
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.OtherProcesses.Name, Constants.Module.ConstantsGroup.OtherProcesses.Guid);
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.SynchronizationOfOrganizationalStructure.Name,
                                                            Constants.Module.ConstantsGroup.SynchronizationOfOrganizationalStructure.Guid);
    }
    
    /// <summary>
    /// Выдать права на справочники.
    /// </summary>
    public static void GrantRightsOnDataBooks()
    {
      InitializationLogger.Debug("Init Vacations module: Grant rights on databooks.");
      
      NNIntegration.ExternalSystemFlows.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      NNIntegration.ExternalSystemFlows.AccessRights.Save();
      
      NNIntegration.SupernikaPushNotificationActions.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      NNIntegration.SupernikaPushNotificationActions.AccessRights.Save();
    }
    
    /// <summary>
    /// Создать записи справочника "Действия в push-уведомлениях Суперники".
    /// </summary>
    public static void CreateSupernikaActions()
    {
      InitializationLogger.Debug("Init: Создание записей справочника \"Действия в push-уведомлениях Суперники\".");
      
      // Пометить как прочитанное.
      CreateSupernikaAction(Constants.Module.SupernikaActions.ActionNames.MarkRead, Constants.Module.SupernikaActions.ActionTypes.Other, Constants.Module.SupernikaActions.ActionGuids.MarkRead,
                            false, Constants.Module.SupernikaActions.BlockNamesRx.HRNotice, Constants.Module.SupernikaActions.ResultNamesRx.MarkRead);
      
      // Выполнить.
      CreateSupernikaAction(Constants.Module.SupernikaActions.ActionNames.Complete, Constants.Module.SupernikaActions.ActionTypes.Approve, Constants.Module.SupernikaActions.ActionGuids.Complete,
                            false, Constants.Module.SupernikaActions.BlockNamesRx.HRAssignment, Constants.Module.SupernikaActions.ResultNamesRx.Complete);
      
      // Согласовать.
      CreateSupernikaAction(Constants.Module.SupernikaActions.ActionNames.Approved, Constants.Module.SupernikaActions.ActionTypes.Approve, Constants.Module.SupernikaActions.ActionGuids.Approved,
                            false, Constants.Module.SupernikaActions.BlockNamesRx.DocumentApprovalAssignment, Constants.Module.SupernikaActions.ResultNamesRx.Approved);
      
      // На доработку.
      CreateSupernikaAction(Constants.Module.SupernikaActions.ActionNames.ForRework, Constants.Module.SupernikaActions.ActionTypes.Other, Constants.Module.SupernikaActions.ActionGuids.ForRework,
                            true, Constants.Module.SupernikaActions.BlockNamesRx.DocumentApprovalAssignment, Constants.Module.SupernikaActions.ResultNamesRx.ForRework);
      
      // Отказать.
      CreateSupernikaAction(Constants.Module.SupernikaActions.ActionNames.Refuse, Constants.Module.SupernikaActions.ActionTypes.Reject, Constants.Module.SupernikaActions.ActionGuids.Refuse,
                            true, Constants.Module.SupernikaActions.BlockNamesRx.DocumentApprovalAssignment, Constants.Module.SupernikaActions.ResultNamesRx.Refuse);
      
      // Принято к сведению.
      CreateSupernikaAction(Constants.Module.SupernikaActions.ActionNames.Noted, Constants.Module.SupernikaActions.ActionTypes.Other, Constants.Module.SupernikaActions.ActionGuids.Noted,
                            false, Constants.Module.SupernikaActions.BlockNamesRx.HRAssignment, Constants.Module.SupernikaActions.ResultNamesRx.Noted);
    }
    
    /// <summary>
    /// Создать запись справочника "Действия в push-уведомлениях Суперники".
    /// </summary>
    /// <param name="name">Заголовок действия.</param>
    /// <param name="actionType">Тип действия.</param>
    /// <param name="actionGuid">Guid действия.</param>
    /// <param name="needComment">Требуется комментарий.</param>
    /// <param name="blockNameRx">Наименование блока в RX.</param>
    /// <param name="resultNameRx">Имя результата выполнения в RX.</param>
    [Public]
    public static void CreateSupernikaAction(string name, string actionType, Guid actionGuid, bool needComment, string blockNameRx, string resultNameRx)
    {
      var externalLink = Sungero.Docflow.PublicFunctions.Module.GetExternalLink(SupernikaPushNotificationAction.ClassTypeGuid, actionGuid);
      
      // Может быть кейс, когда запись удилили руками, но это не правильно, нужно пересоздать такую запись, а вместо удаления оперировать Состоянием записи.
      if (externalLink != null)
      {
        var recordExists = SupernikaPushNotificationActions.GetAll(x => x.Id == externalLink.EntityId).Any();
        
        if(recordExists)
        {
          InitializationLogger.DebugFormat("Init: Запись с именем {0} была создана ранее.", name);
          return;
        }
        else
        {
          InitializationLogger.DebugFormat("Init: Обнаружено ручное удаление записи: {0}. Запись будет пересоздана.", name);
          Sungero.Domain.ModuleFunctions.DeleteExternalLinks(externalLink.ExternalEntityTypeId, new List<string>() { externalLink.ExternalEntityId }, externalLink.ExternalSystemId);
        }
      }
      
      InitializationLogger.DebugFormat("Init: Создание записи: {0}", name);
      
      try
      {
        var newAction = SupernikaPushNotificationActions.Create();
        newAction.Name = name;
        newAction.ActionType = actionType;
        newAction.NeedComment = needComment;
        newAction.BlockName = blockNameRx;
        newAction.ResultRx = resultNameRx;
        newAction.Save();
        Sungero.Docflow.PublicFunctions.Module.CreateExternalLink(newAction, actionGuid);
      }
      catch (Exception ex)
      {
        InitializationLogger.DebugFormat("Init: Ошибка создания записи с именем: {0}. Текст ошибки: {1}", name, ex.Message);
      }
    }
  }
}