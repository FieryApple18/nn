using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.NNCommons.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      // HACK: Seleznyov_DV. Дублирование методов инициализации Sungero.Docflow для первой инициализации пустой базы.
      EmptyDatabaseFirstInitializing();

      this.CreateRoles();
      this.CreateGeneralRoles();
      CreateDocumentTypes();
      CreateDocumentKinds();
      GrantRightsOnDatabooks();
      CreateConstantsGroup();
      this.CreateConstants();
      GrantRightsOnFolder();
      GrantRightsOnDocument();
      AddMembersToRole();
      CreateIndexes();
    }
    
    #region Инициализация для пустой БД.
    
    /// <summary>
    /// Инициализация для пустой БД.
    /// </summary>
    public static void EmptyDatabaseFirstInitializing()
    {
      InitializationLogger.Debug("Init: First database initializing.");
      // Создание типа прав для видов документов.
      if (!AccessRightsTypes.GetAll().Where(x => Equals(x.EntityTypeGuid, Sungero.Docflow.Server.DocumentKind.ClassTypeGuid) && Equals(x.Sid, Sungero.Docflow.Constants.DocumentKind.DocumentKindChoiseAccessRightType)).Any())
        CreateDocflowDocumentKindAccessRights();
      
      CreateDocflowDocumentSendActions();
      CreateDocflowDocumentTypes();
      CreateDocflowDocumentKinds();
      GrantRightsOnDocflowDocuments();
    }
    
    /// <summary>
    /// Создать типы прав для работы с видами документов.
    /// </summary>
    public static void CreateDocflowDocumentKindAccessRights()
    {
      InitializationLogger.Debug("Init: First database initializing. Create access rights for document kind");
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateAccessRightsType(Sungero.Docflow.Server.DocumentKind.ClassTypeGuid.ToString(),
                                                                                  Sungero.Docflow.Resources.SelectInDocumentRightTypeName.ToString(),
                                                                                  Sungero.Docflow.DocumentKindOperations.SelectInDocument,
                                                                                  Sungero.Docflow.DocumentKindOperations.SelectInDocument, Sungero.CoreEntities.AccessRightsType.AccessRightsTypeArea.Both,
                                                                                  Sungero.Docflow.Constants.DocumentKind.DocumentKindChoiseAccessRightType, false, string.Empty);
    }
    
    /// <summary>
    /// Создать действия отправки.
    /// </summary>
    public static void CreateDocflowDocumentSendActions()
    {
      InitializationLogger.Debug("Init: First database initializing. Create document send actions");

      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentSendAction(Sungero.Docflow.OfficialDocuments.Info.Actions.SendForReview, Sungero.Docflow.Resources.SendForReviewActionName);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentSendAction(Sungero.Docflow.OfficialDocuments.Info.Actions.SendActionItem, Sungero.Docflow.Resources.SendActionItemActionName);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentSendAction(Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval, Sungero.Docflow.Resources.SendForFreeApprovalActionName);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentSendAction(Sungero.Docflow.OfficialDocuments.Info.Actions.SendForApproval, Sungero.Docflow.Resources.SendForApprovalActionName);
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentSendAction(Sungero.Docflow.OfficialDocuments.Info.Actions.SendForAcquaintance, Sungero.Docflow.Resources.SendForAcquaintanceActionName);
    }
    
    /// <summary>
    /// Создать типы документов для документооборота.
    /// </summary>
    public static void CreateDocflowDocumentTypes()
    {
      InitializationLogger.Debug("Init: First database initializing. Create document types");
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(Sungero.Docflow.Resources.SimpleDocumentTypeName,
                                                                              Sungero.Docflow.Server.SimpleDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, true);
    }
    
    /// <summary>
    /// Создать виды документов для документооборота.
    /// </summary>
    public static void CreateDocflowDocumentKinds()
    {
      InitializationLogger.Debug("Init: First database initializing. Create document kinds.");
      
      var notNumerable = Sungero.Docflow.DocumentKind.NumberingType.NotNumerable;
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(Sungero.Docflow.Resources.SimpleDocumentKindName, Sungero.Docflow.Resources.SimpleDocumentKindShortName,
                                                                              notNumerable, Sungero.Docflow.DocumentType.DocumentFlow.Inner, false, false, Sungero.Docflow.Server.SimpleDocument.ClassTypeGuid,
                                                                              new Sungero.Domain.Shared.IActionInfo[]
                                                                              {
                                                                                Sungero.Docflow.OfficialDocuments.Info.Actions.SendActionItem,
                                                                                Sungero.Docflow.OfficialDocuments.Info.Actions.SendForFreeApproval,
                                                                                Sungero.Docflow.OfficialDocuments.Info.Actions.SendForAcquaintance
                                                                              },
                                                                              Sungero.Docflow.PublicConstants.Module.Initialize.SimpleDocumentKind);
    }
    
    /// <summary>
    /// Выдать права всем пользователям на документы.
    /// </summary>
    public static void GrantRightsOnDocflowDocuments()
    {
      InitializationLogger.Debug("Init: First database initializing. Grant rights on documents to all users.");
      
      Sungero.Docflow.SimpleDocuments.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Create);
    }
    
    #endregion
    
    /// <summary>
    /// Создать индексы на таблицах БД.
    /// </summary>
    public void CreateIndexes()
    {
      // Индекс по полю "SAP ID" в документах.
      var tableName = "sungero_content_edoc";
      var indexName = "idx_edoc_externalid";
      var indexQuery = string.Format(Queries.Module.NNDocRelatedToWorkBaseIndexQuery, tableName, indexName);
      Sungero.Docflow.PublicFunctions.Module.CreateIndexOnTable(tableName, indexName, indexQuery);
      
      // Индекс по полю "Табельный номер" в справочнике "Сотрудники".
      tableName = "sungero_core_recipient";
      indexName = "idx_recipient_persnumber";
      indexQuery = string.Format(Queries.Module.NNEmployeeIndexQuery, tableName, indexName);
      Sungero.Docflow.PublicFunctions.Module.CreateIndexOnTable(tableName, indexName, indexQuery);

      // Индекс по полю "Сотрудник" в персональных документах по отпускам.
      tableName = "sungero_content_edoc";
      indexName = "idx_doc_employee_vacation_hpro";
      indexQuery = string.Format(Queries.Module.CreateVacationPersonalDocumentBaseIndex, tableName, indexName);
      Sungero.Docflow.PublicFunctions.Module.CreateIndexOnTable(tableName, indexName, indexQuery);
    }
    
    /// <summary>
    /// Добавить субъекты в роли.
    /// </summary>
    public void AddMembersToRole()
    {
      var admin = Roles.Administrators;
      
      AddMemberToRole(admin, HRPro.HRProSolution.PublicConstants.Module.HRProRoles.HRAdministrator);
      AddMemberToRole(admin, HRPro.HRDocFlow.PublicConstants.Module.Roles.HRSpecialist);
      AddMemberToRole(admin, DirRX.EssPlatform.PublicConstants.Module.AdminElEmployeeInteractionSystem);
      AddMemberToRole(admin, HRPro.HRCommons.PublicConstants.Module.Roles.SignatureControlUser);
    }
    
    /// <summary>
    /// Добавить субъект прав в роль.
    /// </summary>
    /// <param name="member">Субъект.</param>
    /// <param name="sid">Guid роли.</param>
    public void AddMemberToRole(IRecipient member, Guid sid)
    {
      InitializationLogger.Debug("Init NNCommons module: Add member to role");
      
      var role = Roles.GetAll().Where(r => r.Sid == sid).FirstOrDefault();
      
      if (role != null && !member.IncludedIn(role))
      {
        var recipient = role.RecipientLinks.AddNew();
        recipient.Member = member;
        role.Save();
      }
    }
    
    /// <summary>
    /// Создать предопределенные роли.
    /// </summary>
    public void CreateRoles()
    {
      InitializationLogger.Debug("Init NNCommons module: Create Roles.");
      // Линейные руководители.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.NNCommons.Resources.LineManagerRoleName,
                                                                      DirRX.NNCommons.Resources.LineManagerRoleDescription,
                                                                      Constants.Module.RolesGuid.LineManager);
      
      // Специалисты ДКП.
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.NNCommons.Resources.SpecialistDKPRoleName,
                                                                      DirRX.NNCommons.Resources.SpecialistDKPRoleDescription,
                                                                      Constants.Module.RolesGuid.SpecialistDKP);
    }
    
    /// <summary>
    /// Создать обобщающие роли.
    /// </summary>
    public void CreateGeneralRoles()
    {
      InitializationLogger.Debug("Init NNCommons module: Create GeneralRoles.");
      
      string[] generalRoleNames = {"КЭДО. Специалисты ОКА",
        "КЭДО. Специалисты ОКА. ТОП",
        "КЭДО. Специалисты ОТиЗ",
        "КЭДО. Специалисты ОТиЗ. ТОП",
        "КЭДО. Специалисты УЭД",
        "КЭДО. Специалисты УЭД. ТОП",
        "КЭДО. Операторы группы КА",
        "КЭДО. Операторы группы КА. ТОП",
        "КЭДО. Операторы группы РсП",
        "КЭДО. Операторы группы РсП. ТОП"};
      
      foreach (var generalRoleName in generalRoleNames)
        Functions.Module.CreateGeneralRole(generalRoleName);
    }
    
    /// <summary>
    /// Выдача прав на документы.
    /// </summary>
    public static void GrantRightsOnDocument()
    {
      InitializationLogger.Debug("Init NNCommons module: Grant rights on document.");
      
      var allUsers = Roles.AllUsers;
      
      DirRX.NNCommons.HRSimpleDocuments.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Create);
      DirRX.NNCommons.HRSimpleDocuments.AccessRights.Save();
    }
    
    /// <summary>
    /// Выдать права на справочники.
    /// </summary>
    public static void GrantRightsOnDatabooks()
    {
      InitializationLogger.Debug("Init NNCommons module: Grant rights on databooks.");
      
      BusinessRoles.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      BusinessRoles.AccessRights.Save();
      
      BusinessProcesses.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      BusinessProcesses.AccessRights.Save();
      
      DocumentsVisabilityLimitations.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      DocumentsVisabilityLimitations.AccessRights.Save();
      
      ConstantsSettings.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      ConstantsSettings.AccessRights.Save();
      
      ConstantsGroups.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      ConstantsGroups.AccessRights.Save();
    }
    
    /// <summary>
    /// Выдача прав на папки.
    /// </summary>
    public void GrantRightsOnFolder()
    {
      InitializationLogger.Debug("Init NNCommons module: Grant rights on folder.");

      var allUsers = Roles.AllUsers;

      DirRX.NNCommons.SpecialFolders.MassApprove.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
      DirRX.NNCommons.SpecialFolders.MassApprove.AccessRights.Save();
    }
    
    /// <summary>
    /// Создать прикладные константы.
    /// </summary>
    public virtual void CreateConstants()
    {
      InitializationLogger.Debug("Init NNCommons module: Create constants.");
      
      // Общее. ИД автора задачи на выпуск сертификата ЭП.
      var serviceUser = Users.GetAll().Where(x => x.IsSystem == true && x.Name == DirRX.NNCommons.PublicConstants.Module.ServiceUserName).FirstOrDefault();
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.CertificateTaskAuthorId.Name, true, serviceUser.Id,
                                      Constants.Module.ConstantsSetting.CertificateTaskAuthorId.Guid,
                                      Constants.Module.ConstantsGroup.IssueOfEPCertificates.Guid);
      
      // Общее. Количество обрабатываемых записей в АО "Проверка документов в файловом хранилище" в одной итерации.
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.VerifyDocumentsCountInIteration.Name, true,
                                      Constants.Module.DefaultDocsCountInIteration,
                                      Constants.Module.ConstantsSetting.VerifyDocumentsCountInIteration.Guid,
                                      Constants.Module.ConstantsGroup.FileStorage.Guid);
      
      // Общее. Имена издателей ЭП, УНЭП которых определяются как ПЭП (указываются через ',').
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.IssuerNames.Name, true, "",
                                      Constants.Module.ConstantsSetting.IssuerNames.Guid,
                                      Constants.Module.ConstantsGroup.IntegrationWithKss.Guid);

      // Общее. Официальное наименование провайдера.
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.OfficialProviderName.Name, true, Constants.Module.OfficialProviderNameDefaultValue,
                                      Constants.Module.ConstantsSetting.OfficialProviderName.Guid,
                                      Constants.Module.ConstantsGroup.IssueOfEPCertificates.Guid);

      // Константы для выпуска сертификатов.
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.GeneralInn.Name, true, "780204893183",
                                      Constants.Module.ConstantsSetting.GeneralInn.Guid,
                                      Constants.Module.ConstantsGroup.IssueOfEPCertificates.Guid);
      
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.PersonelDocumentsRegisterReportTemplateId.Name, true, (long)0,
                                      Constants.Module.ConstantsSetting.PersonelDocumentsRegisterReportTemplateId.Guid,
                                      Constants.Module.ConstantsGroup.Reports.Guid);
      
      // Общее. Сертификаты. Наименование организации-издателя для указания табельного номера в качестве идентифицирующего документа.
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.PersNumIssuedOrganisation.Name, true, "ПАО \"ГМК \"Норильский Никель\"",
                                      Constants.Module.ConstantsSetting.PersNumIssuedOrganisation.Guid,
                                      Constants.Module.ConstantsGroup.IssueOfEPCertificates.Guid);
      
      // Общее. Сертификаты. Выпуск сертификата УНЭП без заявления сотрудника.
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.CertificateIssueProcess.Name, true, true,
                                      Constants.Module.ConstantsSetting.CertificateIssueProcess.Guid,
                                      Constants.Module.ConstantsGroup.IssueOfEPCertificates.Guid);
      
      // Общее. Сертификаты. Эл. почта для получения сертификатов.
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.CertificateEmail.Name, true, string.Empty,
                                      Constants.Module.ConstantsSetting.CertificateEmail.Guid,
                                      Constants.Module.ConstantsGroup.IssueOfEPCertificates.Guid);
      
      // Общее. Отчеты. ИД шаблона для отчета "SLA".
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.ReportSLATemplateId.Name, true, (long)0,
                                      Constants.Module.ConstantsSetting.ReportSLATemplateId.Guid,
                                      Constants.Module.ConstantsGroup.Reports.Guid);
      
      // Кол-во дней за которые требуется уведомлять об окончании ЭП.
      Functions.Module.CreateConstant(Constants.Module.ConstantsSetting.NumberDaysAdvancerequiredNotifyTerminationEP.Name, true, string.Empty,
                                      Constants.Module.ConstantsSetting.NumberDaysAdvancerequiredNotifyTerminationEP.Guid,
                                      Constants.Module.ConstantsGroup.IssueOfEPCertificates.Guid);
    }
    
    /// <summary>
    /// Создать группы констант.
    /// </summary>
    public virtual void CreateConstantsGroup()
    {
      InitializationLogger.Debug("Init NNCommons module: create new Constants Group.");
      Functions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.IntegrationWithKss.Name, Constants.Module.ConstantsGroup.IntegrationWithKss.Guid);
      Functions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.FileStorage.Name, Constants.Module.ConstantsGroup.FileStorage.Guid);
      Functions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.IssueOfEPCertificates.Name, Constants.Module.ConstantsGroup.IssueOfEPCertificates.Guid);
      Functions.Module.CreateConstantsGroup(Constants.Module.ConstantsGroup.Reports.Name, Constants.Module.ConstantsGroup.Reports.Guid);
    }
    
    /// <summary>
    /// Создать вид HR документа и обновить его.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="shortName">Сокращенное имя.</param>
    /// <param name="numerationType">Нумерация.</param>
    /// <param name="direction">Документопоток.</param>
    /// <param name="autoFormattedName">Признак автоформирования имени.</param>
    /// <param name="autoNumerable">Признак автонумерации.</param>
    /// <param name="typeGuid">Доступный тип документа.</param>
    /// <param name="actions">Действия отправки по умолчанию.</param>
    /// <param name="initID">ИД инициализации.</param>
    /// <param name="isDefault">Признак вида документа по умолчанию.</param>
    /// <param name="approver">Документ утверждает.</param>
    /// <param name="daysForSignEmployee">Дней на подписание сотрудником.</param>
    /// <param name="daysForSignManager">Дней на подписание работодателем.</param>
    /// <param name="documentKindsCodes">Код Минтруда.</param>
    /// <param name="isNeedTemplatesRXHRPro">Признак шаблона для RX.</param>
    /// <param name="isNeedTemplatesLKHRPro">Признак шаблона для ЛК.</param>
    /// <param name="needCancelAction">Признак автоматической отмены мероприятия.</param>
    /// <param name="businessProcess">Бизнес-процесс.</param>
    /// <param name="transferBody">Передавать тело документа.</param>
    /// <param name="sapKindCode">Код вида SAP.</param>
    [Public]
    public static void CreateHRDocumentKindNN(string name, string shortName, Enumeration numerationType, Enumeration direction, bool autoFormattedName,
                                              bool autoNumerable, Guid typeGuid, Sungero.Domain.Shared.IActionInfo[] actions, Guid initID, bool isDefault,
                                              Enumeration approver, int? daysForSignEmployee, int? daysForSignManager, string documentKindsCodes,
                                              bool isNeedTemplatesRXHRPro, bool isNeedTemplatesLKHRPro, bool needCancelAction, IBusinessProcess businessProcess, bool transferBody, string sapKindCode)
    {
      InitializationLogger.DebugFormat("Init: Создание вида документа: {0}.", name);
      HRPro.HRDocFlow.PublicInitializationFunctions.Module.CreateHRDocumentKind(name, shortName, numerationType, direction, autoFormattedName,
                                                                                autoNumerable, typeGuid, actions, initID, isDefault, approver,
                                                                                daysForSignEmployee, daysForSignManager, documentKindsCodes,
                                                                                isNeedTemplatesRXHRPro, isNeedTemplatesLKHRPro);
      
      try
      {
        var link = Sungero.Docflow.PublicFunctions.Module.GetExternalLink(Constants.Module.DataBooks.DocumentKindTypeGuid, initID);
        if (link != null)
        {
          var documentKind = DirRX.OverrideSolution.DocumentKinds.GetAll(r => r.Id == link.EntityId).FirstOrDefault();
          documentKind.NeedCancelActionDirRX = needCancelAction;
          documentKind.BusinessProcessDirRX = businessProcess;
          documentKind.TransferBody = transferBody;
          documentKind.SAPCodeDirRX = sapKindCode;
          documentKind.Save();
          
          Functions.Module.CreateBusinessRole(NNCommons.BusinessRole.RoleType.ByKind, null, documentKind, true);
          Functions.Module.CreateBusinessRole(NNCommons.BusinessRole.RoleType.ByKind, null, documentKind, false);
        }
      }
      catch (Exception ex)
      {
        InitializationLogger.DebugFormat("Init: Не удалось обновить вид документа, либо создать роли. Ошибка: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
      }
    }
    
    /// <summary>
    /// Создание типов документов.
    /// </summary>
    private void CreateDocumentTypes()
    {
      InitializationLogger.Debug("Init NNCommons module: Create document types.");
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentType(DirRX.NNCommons.Resources.HRSimpleDocumentDocType,
                                                                              HRSimpleDocument.ClassTypeGuid,
                                                                              Sungero.Docflow.DocumentType.DocumentFlow.Inner, false);
    }
    
    /// <summary>
    /// Создание видов документов.
    /// </summary>
    private static void CreateDocumentKinds()
    {
      InitializationLogger.Debug("Init NNCommons module: Create document kinds.");
      
      #region Бизнес-процессы.
      
      // КЭДО.
      var businessProcessKEDO = Functions.Module.CreateBusinessProcess(Resources.BusinessProcessKEDO, null);
      // Отпуска.
      var businessProcessVacations = Functions.Module.CreateBusinessProcess(NNVacations.Resources.BusinessProcessVacations, businessProcessKEDO);
      // Регистрация ежегодного отпуска.
      var registrationAnnualVacation = NNCommons.PublicFunctions.Module.CreateBusinessProcess(NNVacations.Resources.BusinessProcessRegistrationAnnualVacation, businessProcessVacations);
      // Регистрация прочих отпусков.
      var registrationOtherVacation = NNCommons.PublicFunctions.Module.CreateBusinessProcess(NNVacations.Resources.BusinessProcessRegistrationOtherVacation, businessProcessVacations);
      // Регистрация работы в ВД/ПД/СР.
      var registrationWork = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessRegistrationWork, businessProcessKEDO);
      // Регистрация выплат по ЛНА.
      var registrationPaymentsLNA = NNCommons.PublicFunctions.Module.CreateBusinessProcess(Resources.BusinessProcessRegistrationPaymentsLNA, businessProcessKEDO);
      
      #endregion
      
      #region Простой кадровый документ.
      
      // Мнение профсоюзной организации
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.TradeUnionOrganizationOpinionDocKind,
                                                                            DirRX.NNCommons.Resources.TradeUnionOrganizationOpinionDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, false, false,
                                                                            HRSimpleDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.TradeUnionOrganizationOpinion, true,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationWork, false,
                                                                            Constants.Module.SapKindsCodes.TradeUnionOrganizationOpinionDocKind);
      // Подтверждающий документ к заявлению на выплату по ЛНА
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.SupportingDocumentForLNADocKind,
                                                                            DirRX.NNCommons.Resources.SupportingDocumentForLNADocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, false, false,
                                                                            HRSimpleDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.SupportingDocumentForLNA, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationPaymentsLNA, false,
                                                                            Constants.Module.SapKindsCodes.SupportingDocumentForLNADocKind);
      
      // Подтверждающий документ к заявлению на ежегодный отпуск
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.SupportingDocumentForAnnualVacationDocKind,
                                                                            DirRX.NNCommons.Resources.SupportingDocumentForAnnualVacationDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, false, false,
                                                                            HRSimpleDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.SupportingDocumentForAnnualVacation, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationAnnualVacation, false,
                                                                            Constants.Module.SapKindsCodes.SupportingDocumentForAnnualVacationDocKind);
      
      // Подтверждающий документ к заявлению на прочий отпуск
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.SupportingDocumentForOtherVacationDocKind,
                                                                            DirRX.NNCommons.Resources.SupportingDocumentForOtherVacationDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, false, false,
                                                                            HRSimpleDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.SupportingDocumentForOtherVacation, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationOtherVacation, false,
                                                                            Constants.Module.SapKindsCodes.SupportingDocumentForOtherVacationDocKind);
      
      // Письмо в авиакомпанию
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.AirlineLetterDocKind,
                                                                            DirRX.NNCommons.Resources.AirlineLetterDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            HRSimpleDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.AirlineLetter, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationAnnualVacation, false,
                                                                            Constants.Module.SapKindsCodes.AirlineLetterDocKind);
      
      // Письмо в ОВД
      NNCommons.PublicInitializationFunctions.Module.CreateHRDocumentKindNN(DirRX.NNCommons.Resources.OVDLetterDocKind,
                                                                            DirRX.NNCommons.Resources.OVDLetterDocKind,
                                                                            Sungero.Docflow.DocumentKind.NumberingType.NotNumerable,
                                                                            Sungero.Docflow.DocumentType.DocumentFlow.Inner, true, false,
                                                                            HRSimpleDocument.ClassTypeGuid,
                                                                            new Sungero.Domain.Shared.IActionInfo[] {  },
                                                                            Constants.Module.DocumentKind.OVDLetter, false,
                                                                            HRPro.HRProSolution.DocumentKind.ApproverHRPro.NotRequired, 1, 1,
                                                                            string.Empty, false, false, false, registrationAnnualVacation, false,
                                                                            Constants.Module.SapKindsCodes.OVDLetterDocKind);
      #endregion
      
    }
  }
}
