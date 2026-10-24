using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.NNOtherProcesses.Server
{
  public partial class ModuleInitializer
  {
    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateDocumentTypes();
      CreateDocumentKinds();
      CreateConstantsGroup();
      CreateConstants();
      CreateDatabooksRecords();
      CreateRoles();
      GrantRightsOnDocument();
      GrantRightsOnDatabooks();
      GrantRightsOnReports();
      GrantRightsOnTasks();
      UpdateHRProDocumentKinds();
    }

    /// <summary>
    /// Создание прикладных констант.
    /// </summary>
    private void CreateConstants()
    {
      // Формат файла благотворительность.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.RetentionStatementDocumentExtension.Name, true, "pdf",
                                                             Constants.Module.ConstantsSetting.RetentionStatementDocumentExtension.Guid,
                                                             Constants.Module.ConstantsGroup.PersonalDonationProgram.Guid);
      
      // Ид варианта процесса "Отправка статуса документа "Заявление на удержание" в АСУП".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SendRetentionStatementStatusProcessKindId.Name, true, (long)0,
                                                             Constants.Module.ConstantsSetting.SendRetentionStatementStatusProcessKindId.Guid,
                                                             Constants.Module.ConstantsGroup.PersonalDonationProgram.Guid);

      // Ид варианта процесса "Задача на ручное разблокирование ИТ0015".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.ManualHoldingEventProcessKindId.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.ManualHoldingEventProcessKindId,
                                                             Constants.Module.ConstantsSetting.ManualHoldingEventProcessKindId.Guid,
                                                             Constants.Module.ConstantsGroup.PersonalDonationProgram.Guid);
      
      // Приказ о направлении в командировку/служебную поездку.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.BusinessTripOrder.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.BusinessTripOrder,
                                                             Constants.Module.ConstantsSetting.BusinessTripOrder.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfBusinessTrips.Guid);
      // Приказ об изменении командировки/служебной поездки.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.ChangeBusinessTripOrder.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.ChangeBusinessTripOrder,
                                                             Constants.Module.ConstantsSetting.ChangeBusinessTripOrder.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfBusinessTrips.Guid);

      // Приказ об отмене командировки.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.CancelBusinessTripOrder.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.CancelBusinessTripOrder,
                                                             Constants.Module.ConstantsSetting.CancelBusinessTripOrder.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfBusinessTrips.Guid);
      // Формат документа "Приказ на командировку".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.BusinessTripOrderExtension.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.BusinessTripOrderExtension,
                                                             Constants.Module.ConstantsSetting.BusinessTripOrderExtension.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfBusinessTrips.Guid);
      // Приказы на командировку. Требуется подписание?
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.BusinessTripOrderNeedApprove.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.BusinessTripOrderNeedApprove,
                                                             Constants.Module.ConstantsSetting.BusinessTripOrderNeedApprove.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfBusinessTrips.Guid);
      
      // Приказы на командировку. Требуется ознакомление сотрудников?
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.BusinessTripOrderNeedAcquaintance.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.BusinessTripOrderNeedAcquaintance,
                                                             Constants.Module.ConstantsSetting.BusinessTripOrderNeedAcquaintance.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfBusinessTrips.Guid);
      // Формат файла приказа ВД/ПД/СР.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.WeekendWorkOrderExtension.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.WeekendWorkOrderExtension,
                                                             Constants.Module.ConstantsSetting.WeekendWorkOrderExtension.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfWorkVDPDSR.Guid);
      // Максимальное количество доп. согласующих.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.MaxNumberAdditionalMatchingElement.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.MaxNumberAdditionalMatchingElement,
                                                             Constants.Module.ConstantsSetting.MaxNumberAdditionalMatchingElement.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfWorkVDPDSR.Guid);
      
      var childSupportTypesIds = LrdPaymentTypes.GetAll(x => x.Name == Constants.LrdPaymentType.DefaultNames.ChildBirth ||
                                                        x.Name == Constants.LrdPaymentType.DefaultNames.DisabledChild).Select(x => x.Id);
      
      var childSupportTypesIdsString = string.Join(", ", childSupportTypesIds);
      
      var disabledChildPaymentTypeId = LrdPaymentTypes.GetAll(x => x.Name == Constants.LrdPaymentType.DefaultNames.DisabledChild).FirstOrDefault()?.Id;
      var childBirthPaymentTypeId = LrdPaymentTypes.GetAll(x => x.Name == Constants.LrdPaymentType.DefaultNames.ChildBirth).FirstOrDefault()?.Id;
      
      var retirementPaymentTypeIds = LrdPaymentTypes.GetAll(x => x.Name == Constants.LrdPaymentType.DefaultNames.Retirement).Select(x => x.Id);
      var retirementPaymentTypeIdsString = string.Join(", ", retirementPaymentTypeIds);
      
      
      // Список ИД типов выплат, для которых в заявлении необходимо указывать ребёнка сотрудника.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.ChildSupportPaymentTypesIds.Name,
                                                             true, childSupportTypesIdsString,
                                                             Constants.Module.ConstantsSetting.ChildSupportPaymentTypesIds.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // ИД типа выплаты, для которого в заявлении необходимо указывать ребёнка-инвалида.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.DisabledChildPaymentTypeId.Name,
                                                             true, disabledChildPaymentTypeId ?? (long)0,
                                                             Constants.Module.ConstantsSetting.DisabledChildPaymentTypeId.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // Список ИД типов выплат, для которых в заявлении необходимо указывать дату увольнения.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.RetirementPaymentTypeIds.Name,
                                                             true, retirementPaymentTypeIdsString,
                                                             Constants.Module.ConstantsSetting.RetirementPaymentTypeIds.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // ИД типа выплаты, на рождение ребенка, а также усыновление детей-сирот и детей, оставшихся без попечения родителей.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.ChildBirthPaymentTypeId.Name,
                                                             true, childBirthPaymentTypeId ?? (long)0,
                                                             Constants.Module.ConstantsSetting.ChildBirthPaymentTypeId.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // Ид варианта процесса для запроса мнений.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.OpinionTask.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.OpinionTask,
                                                             Constants.Module.ConstantsSetting.OpinionTask.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // Время кэширования  минутах для запроса во внешнюю систему для получения информации о детях сотрудника.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.LrdPaymentChildrenCacheTime.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.LrdPaymentChildrenCacheTime,
                                                             Constants.Module.ConstantsSetting.LrdPaymentChildrenCacheTime.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // Формат файла приказа на выплату по ЛНА.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.LrdPaymentOrderExtension.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.LrdPaymentOrderExtension,
                                                             Constants.Module.ConstantsSetting.LrdPaymentOrderExtension.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // Должность руководителя организации в "Заявление на выплату по ЛНА", если организация сотрудника "ПАО 'ГМК Норильский Никель'".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.LrdPaymentStatementCEOJobTitle.Name,
                                                             true, DirRX.OverrideSolution.JobTitles.Null,
                                                             Constants.Module.ConstantsSetting.LrdPaymentStatementCEOJobTitle.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // Руководитель организации в "Заявление на выплату по ЛНА", если организация сотрудника "ПАО 'ГМК Норильский Никель'".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.LrdPaymentStatementCEO.Name,
                                                             true, DirRX.OverrideSolution.Employees.Null,
                                                             Constants.Module.ConstantsSetting.LrdPaymentStatementCEO.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // НОР "ПАО ГМК Норильский Никель" для проверки организации сотрудника в "Заявление на выплату по ЛНА".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.LrdPaymentStatementBusinessUnit.Name,
                                                             true, DirRX.OverrideSolution.BusinessUnits.Null,
                                                             Constants.Module.ConstantsSetting.LrdPaymentStatementBusinessUnit.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      // Количество дней хранения записей "Замещение ЛК".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SubstitutionPersonalAccountRetentionDays.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.SubstitutionPersonalAccountRetentionDays,
                                                             Constants.Module.ConstantsSetting.SubstitutionPersonalAccountRetentionDays.Guid,
                                                             Constants.Module.ConstantsGroup.Substitution.Guid);
      
      // Идентификатор системы КЭДО по умолчанию в справочнике "Список систем для замещения".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SubstitutionSystemKedoSystemId.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.SubstitutionSystemKedoSystemId,
                                                             Constants.Module.ConstantsSetting.SubstitutionSystemKedoSystemId.Guid,
                                                             Constants.Module.ConstantsGroup.Substitution.Guid);

      // Формат документа "Дополнительное соглашение к трудовому договору".
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.EmploymentContractAdditionalAgreementExtension.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.EmploymentContractAdditionalAgreementExtension,
                                                             Constants.Module.ConstantsSetting.EmploymentContractAdditionalAgreementExtension.Guid,
                                                             Constants.Module.ConstantsGroup.SigningEmploymentContractAgreement.Guid);
      
      // Оплачиваемый перерыв.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.PaidBreak.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.PaidBreak,
                                                             Constants.Module.ConstantsSetting.PaidBreak.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfWorkVDPDSR.Guid);

      // ИД видов компенсации, для которых необходимо создавать заявление на доп. день отдыха.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.WeekendWorkCompensationKindsForRestStatement.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.WeekendWorkCompensationKindsForRestStatement,
                                                             Constants.Module.ConstantsSetting.WeekendWorkCompensationKindsForRestStatement.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfWorkVDPDSR.Guid);
      
      // Формат файла приказа на доп. день отдыха.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.WeekendWorkRestOrderExtension.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.WeekendWorkRestOrderExtension,
                                                             Constants.Module.ConstantsSetting.WeekendWorkRestOrderExtension.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationOfWorkVDPDSR.Guid);
      // ДС. Тема письма Специалистам ОКА.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.NotSignedAdditionalAgreementLetterSubject.Name,
                                                             true, Constants.Module.ConstantsDefaultValues.NotSignedAdditionalAgreementLetterSubject,
                                                             Constants.Module.ConstantsSetting.NotSignedAdditionalAgreementLetterSubject.Guid,
                                                             Constants.Module.ConstantsGroup.SigningEmploymentContractAgreement.Guid);
      
      // Общее. Оформление заявления по основному месту работы.
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.RegistrationStatementMainPlace.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.RegistrationStatementMainPlace,
                                                             Constants.Module.ConstantsSetting.RegistrationStatementMainPlace.Guid,
                                                             Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      
      //TODO Возможно стоит переименовать, вдруг этих кодов будет больше чем один, или сделать строку с разделителями, и уже в коде парсить. Пока так.
      // Оформление переводов. Код из АСУП для причины "на период временного отсутствия основного работника". 
      NNCommons.PublicFunctions.Module.Remote.CreateConstant(Constants.Module.ConstantsSetting.SAPIdTransferRequest.Name,
                                                             true,
                                                             Constants.Module.ConstantsDefaultValues.SAPIdTransferRequest,
                                                             Constants.Module.ConstantsSetting.SAPIdTransferRequest.Guid,
                                                             Constants.Module.ConstantsGroup.TransferProcess.Guid);
    }
    
    /// <summary>
    /// Создание типов документов.
    /// </summary>
    private void CreateDocumentTypes()
    {
      InitializationLogger.Debug("Init Other processes module: Create document types.");
      
      // Приказ на командировку.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.Resources.BusinessTripOrder,
                                                                              Constants.Module.DocumentType.NNBusinessTripOrderDocument,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner,true);
      
      // Заявление на удержание.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.Resources.RetentionStatement,
                                                                              Constants.Module.DocumentType.NNRetentionStatementDocument,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner,true);
      
      // Заявка на привлечение к работе в ВД/ПД/СР.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.Resources.WeekendWorkStatementDocument,
                                                                              WeekendWorkStatementDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner,false);
      
      // Согласие на привлечение к работе ВД/ПД/СР.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.Resources.WeekendWorkAgreement,
                                                                              WeekendWorkAgreementDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner,true);
      // Приказ (распоряжение) на привлечение к ВД/ПД/СР.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.Resources.WeekendWorkOrder,
                                                                              WeekendWorkOrderDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner,true);
      
      // Заявление на выплату по ЛНА.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.Resources.LrdPaymentStatement,
                                                                              LrdPaymentStatementDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner,true);
      
      // Приказ на выплату по ЛНА.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.Resources.LrdOrderDocument,
                                                                              LrdPaymentOrderDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner,true);
      
      // Заявление на доп. день отдыха.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.Resources.WeekendWorkRestStatementDocument,
                                                                              WeekendWorkRestStatementDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner,false);
      
      // Приказ на доп. день отдыха.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.Resources.WeekendWorkRestOrderDocument,
                                                                              WeekendWorkRestOrderDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner,false);
      
      #region Оформление переводов.
      
      // Заявка на перевод.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.TransferRequestDocuments.Info.LocalizedName,
                                                                              TransferRequestDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, false);
      // Заявление о переводе.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.TransferStatementDocuments.Info.LocalizedName,
                                                                              TransferStatementDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, false);
      // Декларация о конфликте интересов.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.TransferDeclarationDocuments.Info.LocalizedName,
                                                                              TransferDeclarationDocuments.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, false);
      // Приказ о переводе.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNOtherProcesses.TransferOrderDocuments.Info.LocalizedName,
                                                                              TransferOrderDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, false);
      
      #endregion
    }
    
    /// <summary>
    /// Создать группы констант.
    /// </summary>
    public virtual void CreateConstantsGroup()
    {
      InitializationLogger.Debug("Init NNCommons module: create new Constants Group.");
      
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.RegistrationOfBusinessTrips.Name, Constants.Module.ConstantsGroup.RegistrationOfBusinessTrips.Guid);
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.PersonalDonationProgram.Name, Constants.Module.ConstantsGroup.PersonalDonationProgram.Guid);
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.RegistrationOfWorkVDPDSR.Name, Constants.Module.ConstantsGroup.RegistrationOfWorkVDPDSR.Guid);
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Name, Constants.Module.ConstantsGroup.RegistrationPaymentsOfLNA.Guid);
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.Substitution.Name, Constants.Module.ConstantsGroup.Substitution.Guid);
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.SigningEmploymentContractAgreement.Name, Constants.Module.ConstantsGroup.SigningEmploymentContractAgreement.Guid);
      
      NNCommons.PublicFunctions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.TransferProcess.Name, Constants.Module.ConstantsGroup.TransferProcess.Guid);
    }
    
    /// <summary>
    /// Создание видов документов.
    /// </summary>
    private static void CreateDocumentKinds()
    {
      InitializationLogger.Debug("Init Other processes module: Create document kinds.");
      
      #region Бизнес-процессы.
      // КЭДО.
      var businessProcessKEDO = NNCommons.PublicFunctions.Module.CreateBusinessProcess(NNCommons.Resources.BusinessProcessKEDO, null);

      // Регистрация работы в ВД/ПД/СР.
      var registrationWork = NNCommons.PublicFunctions.Module.CreateBusinessProcess(NNCommons.Resources.BusinessProcessRegistrationWork, businessProcessKEDO);
      // Регистрация выплат по ЛНА.
      var registrationPaymentsLNA = NNCommons.PublicFunctions.Module.CreateBusinessProcess(NNCommons.Resources.BusinessProcessRegistrationPaymentsLNA, businessProcessKEDO);
      
      // Регистрация командировок.
      var registrationBusinessTrips = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessRegistrationBusinessTrips, businessProcessKEDO);
      // Программа личных пожертвований.
      var personalDonationProgram =  NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessPersonalDonationProgram, businessProcessKEDO);
      // Изменение условий трудового договора.
      var changeTermsEmplContract = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessChangeTermsEmplContract, businessProcessKEDO);
      
      #endregion
      
      #region Простой кадровый документ.
      
      // Мнение профсоюзной организации
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.TradeUnionOrganizationOpinionDocKind,
                                                                            DirRX.NNCommons.Resources.TradeUnionOrganizationOpinionDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, false, false,
                                                                            PublicConstants.Module.DocumentType.HRSimpleDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            NNCommons.PublicConstants.Module.DocumentKind.TradeUnionOrganizationOpinion, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationWork, false,
                                                                            PublicConstants.Module.SapKindsCodes.TradeUnionOrganizationOpinionDocKind);
      // Подтверждающий документ к заявлению на выплату по ЛНА
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.SupportingDocumentForLNADocKind,
                                                                            DirRX.NNCommons.Resources.SupportingDocumentForLNADocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, false, false,
                                                                            PublicConstants.Module.DocumentType.HRSimpleDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            NNCommons.PublicConstants.Module.DocumentKind.SupportingDocumentForLNA, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationPaymentsLNA, false,
                                                                            PublicConstants.Module.SapKindsCodes.SupportingDocumentForLNADocKind);
      
      #endregion
      
      // Приказ о направлении в командировку.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.BusinessTripOrderDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.BusinessTripOrderDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.Registrable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            Constants.Module.DocumentType.NNBusinessTripOrderDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.BusinessTripOrder, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 1, 1,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, false, false, false, registrationBusinessTrips, true,
                                                                            Constants.Module.SapKindsCodes.BusinessTripOrderDocKind);

      // Приказ об отмене командировки.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.CancelBusinessTripOrder,
                                                                            DirRX.NNOtherProcesses.Resources.CancelBusinessTripOrder,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.Registrable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            Constants.Module.DocumentType.NNBusinessTripOrderDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.CancelBusinessTripOrder, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 1, 1,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, false, false, false, registrationBusinessTrips, true,
                                                                            Constants.Module.SapKindsCodes.CancelBusinessTripOrder);
      
      // Приказ об изменении командировки.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.ChangeBusinessTripOrder,
                                                                            DirRX.NNOtherProcesses.Resources.ChangeBusinessTripOrder,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.Registrable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            Constants.Module.DocumentType.NNBusinessTripOrderDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.ChangeBusinessTripOrder, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.BUnit, 1, 1,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, false, false, false, registrationBusinessTrips, true,
                                                                            Constants.Module.SapKindsCodes.ChangeBusinessTripOrder);
      
      // Заявление на удержание.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.RetentionStatementDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.RetentionStatementDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.Registrable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            Constants.Module.DocumentType.NNRetentionStatementDocument,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.RetentionStatementDocument, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, null,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, false, false, false, personalDonationProgram, false,
                                                                            Constants.Module.SapKindsCodes.RetentionStatementDocKind);
      // Заявка на привлечение к работе в выходные или/и праздничные дни.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.WeekendWorkStatementDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.WeekendWorkStatementDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkStatementDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.WeekendWorkStatementDocument, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, null,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, true, true, false, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.WeekendWorkStatementDocKind);

      // Заявка на привлечение к работе в сверхурочное время.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.OvertimeWorkStatementDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.OvertimeWorkStatementDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkStatementDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.OvertimeWorkStatementDocument, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, null,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, true, true, false, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.OvertimeWorkStatementDocKind);
      
      // Согласие на привлечение к работе в выходной и/или праздничный день
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.WeekendWorkAgreementDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.WeekendWorkAgreementDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkAgreementDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.WeekendWorkAgreementDocument, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, true, true, false, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.WeekendWorkAgreementDocKind);
      
      // Согласие на привлечение к работе в сверхурочное время
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.OvertimeWorkAgreementDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.OvertimeWorkAgreementDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkAgreementDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.OvertimeWorkAgreementDocument, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, true, true, false, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.OvertimeWorkAgreementDocKind);
      
      // Приказ (распоряжение) о работе в выходные/нерабочие праздничные дни
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.WeekendWorkOrderDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.WeekendWorkOrderDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkOrderDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.WeekendWorkOrderDocument, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, false, false, true, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.WeekendWorkOrderDocKind);
      
      // Приказ (распоряжение) о сверхурочной работе
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.OvertimeWorkOrderDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.OvertimeWorkOrderDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkOrderDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.OvertimeWorkOrderDocument, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, false, false, true, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.OvertimeWorkOrderDocKind);
      
      // Приказ (распоряжение) об отмене работы по приказу (распоряжению)
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.WeekendWorkCancelationOrderDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.WeekendWorkCancelationOrderDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkOrderDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.WeekendWorkCancelationOrderDocument, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, false, false, false, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.WeekendWorkCancelationOrderDocKind);
      
      // Заявление на выплату по ЛНА.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.LrdPaymentStatementDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.LrdPaymentStatementDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            LrdPaymentStatementDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.LrdPaymentStatementDocument, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, null,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, true, true, false, registrationPaymentsLNA, false,
                                                                            Constants.Module.SapKindsCodes.LrdPaymentStatementDocKind);
      
      // Приказ об оформлении выплаты по ЛНА.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.LrdPaymentOrderDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.LrdPaymentOrderDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.Registrable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            LrdPaymentOrderDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.LrdPaymentOrderDocument, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, null,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, false, false, false, registrationPaymentsLNA, false,
                                                                            Constants.Module.SapKindsCodes.LrdPaymentOrderDocKind);
      
      // Приказ на предоставление доп. дня отдыха.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.GrantRestOrderDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.GrantRestOrderDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkRestOrderDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.GrantRestOrderDocument, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, null,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, true, true, true, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.GrantRestOrderDocKind);
      
      // Заявление на доп. день отдыха за РВД.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.WeekendWorkRestStatementDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.WeekendWorkRestStatementDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkRestStatementDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.WeekendWorkRestStatementDocument, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.Employee, 1, null,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, true, true, false, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.WeekendWorkRestStatementDocKind);
      
      // Приказ об отмене доп. дня отдыха.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.Resources.CancelationRestOrderDocKind,
                                                                            DirRX.NNOtherProcesses.Resources.CancelationRestOrderDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            WeekendWorkRestOrderDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.CancelationRestOrderDocument, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, null,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, true, true, false, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.CancelationRestOrderDocKind);
      
      #region Оформление переводов.
      
      // Заявка на перевод.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNOtherProcesses.TransferRequestDocuments.Info.LocalizedName,
                                                                            DirRX.NNOtherProcesses.TransferRequestDocuments.Info.LocalizedName,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            TransferRequestDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.CancelationRestOrderDocument, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, null, null,
                                                                            Constants.Module.CodeMinistryLabor.OtherDocuments, true, true, false, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.CancelationRestOrderDocKind
                                                                           );
      
      // Заявление о переводе.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN();
      
      // Декларация о конфликте интересов.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN();
      
      // Приказ о переводе.
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN();
      
      #endregion
    }
    
    /// <summary>
    /// Создать предопределенные роли.
    /// </summary>
    public void CreateRoles()
    {
      InitializationLogger.Debug("Init NNOtherProcesses module: Create Roles.");
      // Линейные руководители.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.NNOtherProcesses.Resources.ResponsibleWeekendWorkRoleName,
                                                                      DirRX.NNOtherProcesses.Resources.ResponsibleWeekendWorkRoleDescription,
                                                                      Constants.Module.RolesGuid.ResponsibleWeekendWork);
      // TODO Почему тут два раза одну и ту же роль создаем? Косяк слияния?
      // Ответственный за РВ и СУ.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.NNOtherProcesses.Resources.ResponsibleWeekendWorkRoleName,
                                                                      DirRX.NNOtherProcesses.Resources.ResponsibleWeekendWorkRoleDescription,
                                                                      Constants.Module.RolesGuid.ResponsibleWeekendWork);
      // TODO Пересмотреть название роли, доработать ресурсы с именем и описанием роли.
      // Предлагаю: Имя - Ответственный за инициацию переводов, Описание - пользователи, ответственные за иницацию переводов сотрудников.
      // Инициатор перевода.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.NNOtherProcesses.Resources.ResponsibleTransferProcessRoleName,
                                                                      DirRX.NNOtherProcesses.Resources.ResponsibleTransferProcessRoleDescription,
                                                                      Constants.Module.RolesGuid.ResponsibleTransferProcess);
    }
    
    /// <summary>
    /// Выдача прав на документы.
    /// </summary>
    public static void GrantRightsOnDocument()
    {
      InitializationLogger.Debug("Init Other processes module: Grant rights on document.");
      
      NNOtherProcesses.WeekendWorkStatementDocuments.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
      NNOtherProcesses.WeekendWorkStatementDocuments.AccessRights.Save();
    }
    
    /// <summary>
    /// Создание записей по умолчанию для справочников.
    /// </summary>
    private static void CreateDatabooksRecords()
    {
      InitializationLogger.Debug("Init Other processes module: Create databooks records.");
      var error = string.Empty;
      
      #region Выплаты по ЛНА.
      
      // Причины мероприятий.
      error += Functions.LrdEventReason.CreateRecord(PublicConstants.LrdEventReason.DefaultNames.MaterialSupport, PublicConstants.LrdEventReason.DefaultCodes.MaterialSupport);
      error += Functions.LrdEventReason.CreateRecord(PublicConstants.LrdEventReason.DefaultNames.Compensation, PublicConstants.LrdEventReason.DefaultCodes.Compensation);
      
      // Уточнения причин мероприятий.
      error += Functions.LrdEventReasonClarification.CreateRecord(PublicConstants.LrdEventReasonClarification.DefaultNames.ChildBirth, PublicConstants.LrdEventReasonClarification.DefaultCodes.ChildBirth);
      error += Functions.LrdEventReasonClarification.CreateRecord(PublicConstants.LrdEventReasonClarification.DefaultNames.FamilyMemberDeath, PublicConstants.LrdEventReasonClarification.DefaultCodes.FamilyMemberDeath);
      error += Functions.LrdEventReasonClarification.CreateRecord(PublicConstants.LrdEventReasonClarification.DefaultNames.EmployeesMaterialSupport, PublicConstants.LrdEventReasonClarification.DefaultCodes.EmployeesMaterialSupport);
      error += Functions.LrdEventReasonClarification.CreateRecord(PublicConstants.LrdEventReasonClarification.DefaultNames.LifeOneTimePayment, PublicConstants.LrdEventReasonClarification.DefaultCodes.LifeOneTimePayment);
      error += Functions.LrdEventReasonClarification.CreateRecord(PublicConstants.LrdEventReasonClarification.DefaultNames.RetirementOneTimePayment, PublicConstants.LrdEventReasonClarification.DefaultCodes.RetirementOneTimePayment);
      error += Functions.LrdEventReasonClarification.CreateRecord(PublicConstants.LrdEventReasonClarification.DefaultNames.MutilationMaterialSupport, PublicConstants.LrdEventReasonClarification.DefaultCodes.MutilationMaterialSupport);
      error += Functions.LrdEventReasonClarification.CreateRecord(PublicConstants.LrdEventReasonClarification.DefaultNames.DisabledChild, PublicConstants.LrdEventReasonClarification.DefaultCodes.DisabledChild);
      error += Functions.LrdEventReasonClarification.CreateRecord(PublicConstants.LrdEventReasonClarification.DefaultNames.DisabledEmployee, PublicConstants.LrdEventReasonClarification.DefaultCodes.DisabledEmployee);
      error += Functions.LrdEventReasonClarification.CreateRecord(PublicConstants.LrdEventReasonClarification.DefaultNames.EmployeeMaterialSupport3324, PublicConstants.LrdEventReasonClarification.DefaultCodes.EmployeeMaterialSupport3324);

      // Назначения платежа.
      error += Functions.LrdPaymentPurpose.CreateRecord(PublicConstants.LrdPaymentPurpose.DefaultNames.FamilyMemberDeath, PublicConstants.LrdPaymentPurpose.DefaultCodes.FamilyMemberDeath);
      error += Functions.LrdPaymentPurpose.CreateRecord(PublicConstants.LrdPaymentPurpose.DefaultNames.Other, PublicConstants.LrdPaymentPurpose.DefaultCodes.Other);
      error += Functions.LrdPaymentPurpose.CreateRecord(PublicConstants.LrdPaymentPurpose.DefaultNames.DisabledChild, PublicConstants.LrdPaymentPurpose.DefaultCodes.DisabledChild);
      error += Functions.LrdPaymentPurpose.CreateRecord(PublicConstants.LrdPaymentPurpose.DefaultNames.DisabledEmployee, PublicConstants.LrdPaymentPurpose.DefaultCodes.DisabledEmployee);
      error += Functions.LrdPaymentPurpose.CreateRecord(PublicConstants.LrdPaymentPurpose.DefaultNames.Emergency, PublicConstants.LrdPaymentPurpose.DefaultCodes.Emergency);
      
      // Типы выплат.
      
      // Материальная помощь на рождение ребенка, а также усыновление детей-сирот и детей, оставшихся без попечения родителей.
      error += Functions.LrdPaymentType.CreateRecord(PublicConstants.LrdPaymentType.DefaultNames.ChildBirth,
                                                     PublicConstants.LrdEventReason.DefaultCodes.MaterialSupport,
                                                     PublicConstants.LrdEventReasonClarification.DefaultNames.ChildBirth,
                                                     string.Empty,
                                                     PublicConstants.LrdPaymentType.CodeVO.ChildBirth);
      
      // Материальная помощь работнику в связи со смертью члена семьи.
      error += Functions.LrdPaymentType.CreateRecord(PublicConstants.LrdPaymentType.DefaultNames.FamilyMemberDeath,
                                                     PublicConstants.LrdEventReason.DefaultCodes.MaterialSupport,
                                                     PublicConstants.LrdEventReasonClarification.DefaultNames.FamilyMemberDeath,
                                                     PublicConstants.LrdPaymentPurpose.DefaultNames.FamilyMemberDeath,
                                                     PublicConstants.LrdPaymentType.CodeVO.FamilyMemberDeath);
      
      // Материальная помощь работнику при материальном ущербе в связи с экстремальной ситуацией (стихийное бедствие, пожар, авария, кража и т.п.).
      error += Functions.LrdPaymentType.CreateRecord(PublicConstants.LrdPaymentType.DefaultNames.Emergency,
                                                     PublicConstants.LrdEventReason.DefaultCodes.MaterialSupport,
                                                     PublicConstants.LrdEventReasonClarification.DefaultNames.EmployeeMaterialSupport3324,
                                                     PublicConstants.LrdPaymentPurpose.DefaultNames.Emergency,
                                                     PublicConstants.LrdPaymentType.CodeVO.Emergency);
      
      // Единовременная выплата в связи с трудной жизненной ситуацией.
      error += Functions.LrdPaymentType.CreateRecord(PublicConstants.LrdPaymentType.DefaultNames.LifeOneTimePayment,
                                                     PublicConstants.LrdEventReason.DefaultCodes.MaterialSupport,
                                                     PublicConstants.LrdEventReasonClarification.DefaultNames.LifeOneTimePayment,
                                                     string.Empty,
                                                     PublicConstants.LrdPaymentType.CodeVO.LifeOneTimePayment);
      
      // Единовременная материальная помощь по уходу на пенсию.
      error += Functions.LrdPaymentType.CreateRecord(PublicConstants.LrdPaymentType.DefaultNames.Retirement,
                                                     PublicConstants.LrdEventReason.DefaultCodes.MaterialSupport,
                                                     PublicConstants.LrdEventReasonClarification.DefaultNames.RetirementOneTimePayment,
                                                     string.Empty,
                                                     PublicConstants.LrdPaymentType.CodeVO.Retirement);
      
      // Материальная помощь работнику, имеющему ребенка-инвалида в возрасте до 18 лет.
      error += Functions.LrdPaymentType.CreateRecord(PublicConstants.LrdPaymentType.DefaultNames.DisabledChild,
                                                     PublicConstants.LrdEventReason.DefaultCodes.MaterialSupport,
                                                     PublicConstants.LrdEventReasonClarification.DefaultNames.DisabledChild,
                                                     PublicConstants.LrdPaymentPurpose.DefaultNames.DisabledChild,
                                                     PublicConstants.LrdPaymentType.CodeVO.DisabledChild);
      
      // Материальная помощь работнику-инвалиду.
      error += Functions.LrdPaymentType.CreateRecord(PublicConstants.LrdPaymentType.DefaultNames.DisabledEmployee,
                                                     PublicConstants.LrdEventReason.DefaultCodes.MaterialSupport,
                                                     PublicConstants.LrdEventReasonClarification.DefaultNames.DisabledEmployee,
                                                     PublicConstants.LrdPaymentPurpose.DefaultNames.DisabledEmployee,
                                                     PublicConstants.LrdPaymentType.CodeVO.DisabledEmployee);
      
      // Материальная помощь в исключительных случаях.
      error += Functions.LrdPaymentType.CreateRecord(PublicConstants.LrdPaymentType.DefaultNames.Other,
                                                     PublicConstants.LrdEventReason.DefaultCodes.MaterialSupport,
                                                     PublicConstants.LrdEventReasonClarification.DefaultNames.EmployeesMaterialSupport,
                                                     PublicConstants.LrdPaymentPurpose.DefaultNames.Other,
                                                     PublicConstants.LrdPaymentType.CodeVO.Other);
      
      // Материальная помощь (ежемесячная и единовременная) работникам, получившим увечье.
      error += Functions.LrdPaymentType.CreateRecord(PublicConstants.LrdPaymentType.DefaultNames.Mutilation,
                                                     PublicConstants.LrdEventReason.DefaultCodes.Compensation,
                                                     PublicConstants.LrdEventReasonClarification.DefaultNames.MutilationMaterialSupport,
                                                     string.Empty,
                                                     PublicConstants.LrdPaymentType.CodeVO.Mutilation);
      #endregion
      
      #region Список систем для замещения.
      
      error += Functions.SubstitutionSystem.CreateRecord(DirRX.NNOtherProcesses.Resources.DefaultSubstitutionSystem_Name,
                                                         DirRX.NNOtherProcesses.Resources.DefaultSubstitutionSystem_Name,
                                                         DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsDefaultValues.SubstitutionSystemKedoSystemId,
                                                         DirRX.NNOtherProcesses.Resources.DefaultSubstitutionSystem_Name);
      
      #endregion
      
      if (!string.IsNullOrEmpty(error))
        InitializationLogger.Debug(error);
    }
    
    /// <summary>
    /// Выдать права на справочники.
    /// </summary>
    private static void GrantRightsOnDatabooks()
    {
      InitializationLogger.Debug("Init Other processes module: Grant rights on databooks.");
      
      NNOtherProcesses.WeekendWorkCompensationKinds.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      NNOtherProcesses.WeekendWorkCompensationKinds.AccessRights.Save();
      
      NNOtherProcesses.WeekendWorkReasons.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      NNOtherProcesses.WeekendWorkReasons.AccessRights.Save();
      
      NNOtherProcesses.SubstitutionSystems.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      NNOtherProcesses.SubstitutionSystems.AccessRights.Save();
      
      NNOtherProcesses.SubstitutionPersonalAccounts.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      NNOtherProcesses.SubstitutionPersonalAccounts.AccessRights.Save();
      
      NNOtherProcesses.WeekendWorkCancelationReasons.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      NNOtherProcesses.WeekendWorkCancelationReasons.AccessRights.Save();
      
      var hrUsers = Roles.GetAll(x => x.Sid == HRPro.HRDocFlow.PublicConstants.Module.Roles.HRDocFlowUsers).FirstOrDefault();
      if (hrUsers != null)
      {
        NNOtherProcesses.LrdPaymentMPGroups.AccessRights.Grant(hrUsers, DefaultAccessRightsTypes.Read);
        NNOtherProcesses.LrdPaymentMPGroups.AccessRights.Save();
      }
      else
        InitializationLogger.Debug("Не найдена роль \"Пользователи модуля Кадровый ЭДО\"");
      
      //NNOtherProcesses.TransferReasons.AccessRights.Grant(Roles.Administrators, DefaultAccessRightsTypes.FullAccess);
      //NNOtherProcesses.TransferReasons.AccessRights.Save();
      
      var responsibleTransferProcess = Roles.GetAll(r => r.Sid == Constants.Module.RolesGuid.ResponsibleTransferProcess).FirstOrDefault();
      
      if (responsibleTransferProcess != null)
      {
        NNOtherProcesses.TransferReasons.AccessRights.Grant(responsibleTransferProcess, DefaultAccessRightsTypes.Read);
        NNOtherProcesses.TransferReasons.AccessRights.Save();
      }
    }
    
    /// <summary>
    /// Выдать права на задачи.
    /// </summary>
    public static void GrantRightsOnTasks()
    {
      InitializationLogger.Debug("Init: Grant rights on tasks.");
      
      // Выдать права роли "Линейные руководители".
      var managerRole = Roles.GetAll(x => x.Sid == DirRX.NNCommons.PublicConstants.Module.RolesGuid.LineManager).FirstOrDefault();
      // Роль может быть еще не создана, если нарушился порядок инициализации.
      if (managerRole != null)
      {
        NNOtherProcesses.WeekendWorkCancelationTasks.AccessRights.Grant(managerRole, DefaultAccessRightsTypes.Create);
        NNOtherProcesses.WeekendWorkCancelationTasks.AccessRights.Save();
      }
      else
        InitializationLogger.Debug("Не найдена роль \"Линейные руководители\"");
    }

    /// <summary>
    /// Обновить виды документов базового HRPro.
    /// </summary>
    private void UpdateHRProDocumentKinds()
    {
      //Заполнение дополнительных полей вида документа "Дополнительное соглашение к трудовому договору".
      var supAgreement = DirRX.OverrideSolution.DocumentKinds.GetAll().Where(x => x.Name.Equals(HRPro.HRDocFlow.Resources.AdditionalAgreement)).FirstOrDefault();
      if (supAgreement != null)
      {
        var businessProcess = DirRX.NNCommons.BusinessProcesses.GetAll().Where(x => x.Name.Equals(Resources.BusinessProcessChangeTermsEmplContract)).FirstOrDefault();
        if (businessProcess != null && supAgreement.BusinessProcessDirRX == null)
          supAgreement.BusinessProcessDirRX = businessProcess;

        if (supAgreement.SAPCodeDirRX == null)
          supAgreement.SAPCodeDirRX = Constants.Module.SapKindsCodes.AdditionalAgreementDocKind;
        
        if (supAgreement.State.IsChanged)
          supAgreement.Save();
      }
    }
    
    /// <summary>
    /// Выдать права на отчеты.
    /// </summary>
    private static void GrantRightsOnReports()
    {
      InitializationLogger.Debug("Init Other processes module: Grant rights on reports.");
      
      Reports.AccessRights.Grant(Reports.GetNotSignedAdditionalAgreementReport().Info, Roles.AllUsers, DefaultReportAccessRightsTypes.Execute);
    }
  }
}
