using System;
using Sungero.Core;

namespace DirRX.NNCommons.Constants
{
  public static class Module
  {
    /// <summary>
    /// Количество обрабатываемых записей в АО "Запустить задачи на выпуск сертификатов" в одной итерации.
    /// </summary>
    public const int CountMassIssueCertificatesInIteration = 200;
    
    /// <summary>
    /// Количество обрабатываемых записей в АО "Закрыть сертификаты ПЭП" в одной итерации.
    /// </summary>
    public const int CountDisableSimpleCertificatesInIteration = 200;

    /// <summary>
    /// Кол-во записей справочника Сотрудники, которое возвращается при открытии справочника из ЛК.
    /// </summary>
    public const int DefaultRecordCountForOpeningEmployees = 26;

    /// <summary>
    /// Guid типа реципиента - сотрудники.
    /// </summary>
    /// <remarks>Дублируется константа из модуля Sungero.Company.</remarks>
    public const string EmployeeTypeGuid = "b7905516-2be5-4931-961c-cb38d5677565";
    
    #region Отчеты.
    
    /// <summary>
    /// Константы отчета "Реестр кадровых документов".
    /// </summary>
    [Public]
    public static class PersonnelDocumentRegisterReport
    {
      /// <summary>
      /// Номер первой строки отчета.
      /// </summary>
      [Public]
      public const int FirstRowIndex = 5;
      
      /// <summary>
      /// Количество колонок в отчете.
      /// </summary>
      [Public]
      public const int ColumnsCount = 18;
      
      /// <summary>
      /// Разница между датами начала и окончания создания документов в месяцах.
      /// </summary>
      [Public]
      public const int DateDiffInMonths = 3;
    }
    
    /// <summary>
    /// Константы отчета SLA.
    /// </summary>
    [Public]
    public static class ReportSLA
    {
      /// <summary>
      /// Номер первой строки отчета.
      /// </summary>
      [Public]
      public const int FirstRowIndex = 6;
      
      /// <summary>
      /// Номер первого столбца в отчете.
      /// </summary>
      [Public]
      public const int FirstColumnIndex = 0;
      
      /// <summary>
      /// Номер колонки Документ поступил.
      /// </summary>
      [Public]
      public const int AssignmentCreatedIndex = 9;
      
      /// <summary>
      /// Номер колонки Срок.
      /// </summary>
      [Public]
      public const int AssignmentDeadlineIndex = 10;
      
      /// <summary>
      /// Номер колонки Время обработки.
      /// </summary>
      [Public]
      public const int ProcessingTimeIndex = 14;
      
      /// <summary>
      /// Номер колонки Превышение срока.
      /// </summary>
      [Public]
      public const int ExceedingDeadlineIndex = 15;
      
      /// <summary>
      /// Разница между датами начала и окончания создания документов в месяцах.
      /// </summary>
      [Public]
      public const int DateDiffInMonths = 3;
    }
    
    #endregion
    
    /// <summary>
    /// Константы кодов видов документов в АСУП.
    /// </summary>
    [Public]
    public static class SapKindsCodes
    {
      /// <summary>
      /// Код вида документа "Мнение профсоюзной организации".
      /// </summary>
      [Public]
      public const string TradeUnionOrganizationOpinionDocKind = "";
      /// <summary>
      /// Код вида документа "Подтверждающий документ к заявлению на выплату по ЛНА".
      /// </summary>
      [Public]
      public const string SupportingDocumentForLNADocKind = "";
      /// <summary>
      /// Код вида документа "Подтверждающий документ к заявлению на ежегодный отпуск".
      /// </summary>
      [Public]
      public const string SupportingDocumentForAnnualVacationDocKind = "";
      /// <summary>
      /// Код вида документа "Подтверждающий документ к заявлению на прочий отпуск".
      /// </summary>
      [Public]
      public const string SupportingDocumentForOtherVacationDocKind = "";
      /// <summary>
      /// Код вида документа "Письмо в авиакомпанию".
      /// </summary>
      [Public]
      public const string AirlineLetterDocKind = "";
      /// <summary>
      /// Код вида документа "Письмо в ОВД".
      /// </summary>
      [Public]
      public const string OVDLetterDocKind = "";
    }
    
    #region Ролевая модель
    
    /// <summary>
    /// Константы ролевой модели.
    /// </summary>
    [Public]
    public static class RoleModel
    {
      /// <summary>
      /// Параметр для признака наличия доступа сотрудника к своей НОР.
      /// </summary>
      [Public]
      public const string AccessToBusinesUnitParamName = "AccessToBusinesUnit:{0}";
    }
    
    #endregion
    
    #region Группы констант.
    /// <summary>
    /// Группы констант.
    /// </summary>
    [Public]
    public static class ConstantsGroup
    {
      /// <summary>
      /// Интеграция с КСШ.
      /// </summary>
      [Public]
      public static class IntegrationWithKss
      {
        [Public]
        public const string Guid = "c547fb24-9cdf-42ea-96cd-e6676d845413";
        public const string Name = "Интеграция с КСШ";
      }
      
      /// <summary>
      /// Файловое хранилище.
      /// </summary>
      [Public]
      public static class FileStorage
      {
        [Public]
        public const string Guid = "ad9d2cec-8cdb-4478-a2eb-ef009c61d83e";
        public const string Name = "Файловое хранилище";
      }
      
      /// <summary>
      /// Выпуск сертификатов ЭП.
      /// </summary>
      [Public]
      public static class IssueOfEPCertificates
      {
        [Public]
        public const string Guid = "5747fc2f-7a98-4d69-8c77-3a248c2a942b";
        public const string Name = "Выпуск сертификатов ЭП";
      }
      
      /// <summary>
      /// Отчеты.
      /// </summary>
      [Public]
      public static class Reports
      {
        [Public]
        public const string Guid = "A72475C3-0376-4DD2-8289-1280A8CDCEC3";
        public const string Name = "Отчеты";
      }
      
      /// <summary>
      /// Параметры подключения к БД.
      /// </summary>
      [Public]
      public static class DbConnectionSettings
      {
        [Public]
        public const string Guid = "75886E1B-41A3-46A1-BE54-9FC322BDD454";
        public const string Name = "Параметры подключения к БД";
      }
      /// <summary>
      /// Календари рабочего времени.
      /// </summary>
      [Public]
      public static class WorkingTimeCalendars
      {
        [Public]
        public const string Guid = "937007e4-3469-4c75-9ec1-0cd52db4a1f9";
        public const string Name = "Календари рабочего времени";
      }
      
      /// <summary>
      /// Организационная структура.
      /// </summary>
      [Public]
      public static class OrgStructure
      {
        [Public]
        public const string Guid = "A6747C65-6430-48D0-84A4-951728B6EE44";
        public const string Name = "Организационная структура";
      }
      
    }
    #endregion
    
    #region Прикладные константы.
    
    [Public]
    public static class ConstantsSetting
    {
      /// <summary>
      /// Общее. Сертификаты. Наименование организации-издателя для указания табельного номера в качестве идентифицирующего документа.
      /// </summary>
      [Public]
      public static class PersNumIssuedOrganisation
      {
        [Public]
        public const string Guid = "25887f92-9234-4f11-99e5-05c1a24747d7";
        [Public]
        public const string Name = "Общее. Сертификаты. Наименование организации-издателя для указания табельного номера в качестве идентифицирующего документа";
      }
      
      /// <summary>
      /// Общее. Сертификаты. Фиктивный общий ИНН для выпуска сертификатов ЭП.
      /// </summary>
      [Public]
      public static class GeneralInn
      {
        [Public]
        public const string Guid = "C6F6C779-90AE-4DE2-BC58-DA85B9E867B3";
        [Public]
        public const string Name = "Общее. Сертификаты. Фиктивный общий ИНН для выпуска сертификатов ЭП";
      }

      /// <summary>
      /// Общее. ИД автора задачи на выпуск сертификата УНЭП.
      /// </summary>
      [Public]
      public static class CertificateTaskAuthorId
      {
        [Public]
        public const string Guid = "B6F78F9D-5F83-49E0-8E1C-C128D7D0D1D7";
        [Public]
        public const string Name = "Общее. ИД автора задачи на выпуск сертификата УНЭП";
      }

      /// <summary>
      /// Отпуска. ИД типа отпуска Учебный.
      /// </summary>
      [Public]
      public static class StudyVacationTypeId
      {
        [Public]
        public const string Guid = "D1E7F08D-1A3E-4C28-9C2C-D6BB1E518DE0";
        [Public]
        public const string Name = "Отпуска. ИД типа отпуска Учебный";
      }

      /// <summary>
      /// Общее. Имена издателей ЭП, УНЭП которых определяются как ПЭП (указываются через ',').
      /// </summary>
      [Public]
      public static class IssuerNames
      {
        [Public]
        public const string Guid = "A3F34D4B-4170-4D22-9189-25DA9B2D42B4";
        [Public]
        public const string Name = "Общее. Имена издателей ЭП, УНЭП которых определяются как ПЭП (указываются через ',')";
      }
      
      /// <summary>
      /// Общее. Сертификаты. Выпуск сертификата УНЭП без заявления сотрудника.
      /// </summary>
      [Public]
      public static class CertificateIssueProcess
      {
        [Public]
        public const string Guid = "84EFF25E-E92D-4E4E-A39A-007FE54A68CC";
        [Public]
        public const string Name = "Общее. Сертификаты. Выпуск сертификата УНЭП без заявления сотрудника";
      }
      
      /// <summary>
      /// Общее. Сертификаты. Официальное наименование провайдера.
      /// </summary>
      [Public]
      public static class OfficialProviderName
      {
        [Public]
        public const string Guid = "97bf0aa6-d019-4f6d-8741-5f11eccef622";
        public const string Name = "Общее. Сертификаты. Официальное наименование провайдера";
      }

      /// <summary>
      /// Общее. Количество обрабатываемых записей в АО "Проверка документов в файловом хранилище" в одной итерации.
      /// </summary>
      [Public]
      public static class VerifyDocumentsCountInIteration
      {
        [Public]
        public const string Guid = "2d92a919-4f83-4784-a4d9-9ed158b379aa";
        public const string Name = "Общее. Количество обрабатываемых записей в АО \"Проверка документов в файловом хранилище\" в одной итерации.";
      }
      
      /// <summary>
      /// Общее. Отчеты. ИД шаблона для отчета "Реестр кадровых документов".
      /// </summary>
      [Public]
      public static class PersonelDocumentsRegisterReportTemplateId
      {
        [Public]
        public const string Guid = "9F3994DC-11AB-44D6-AAE3-2BFD079F737D";
        public const string Name = "Общее. Отчеты. ИД шаблона для отчета Реестр кадровых документов";
      }
      
      /// <summary>
      /// Общее. Отчеты. ИД шаблона для отчета "SLA".
      /// </summary>
      [Public]
      public static class ReportSLATemplateId
      {
        [Public]
        public const string Guid = "79BF3E34-3AE5-4BFF-9040-CF8CF6A89066";
        public const string Name = "Общее. Отчеты. ИД шаблона для отчета \"SLA\"";
      }
      
      /// <summary>
      /// Кол-во дней за которые требуется уведомлять об окончании ЭП".
      /// </summary>
      [Public]
      public static class NumberDaysAdvancerequiredNotifyTerminationEP
      {
        [Public]
        public const string Guid = "EC0815DC-73B8-4313-BF0E-738729AECB6A";
        public const string Name = "Кол-во дней за которые требуется уведомлять об окончании ЭП";
      }
      
      /// <summary>
      /// Общее. Сертификаты. Эл. почта для получения сертификатов.
      /// </summary>
      [Public]
      public static class CertificateEmail
      {
        [Public]
        public const string Guid = "4941C1B3-367C-493E-9A82-23B9464D3569";
        public const string Name = "Общее. Сертификаты. Эл. почта для получения сертификатов";
      }
      
      /// <summary>
      /// Общее. Имена издателей УНЭП (указываются через ',').
      /// </summary>
      [Public]
      public static class IssuerNamesUnqualifiedSignature
      {
        [Public]
        public const string Guid = "6A22E99B-2A54-4DC9-B604-694B99701FC7";
        [Public]
        public const string Name = "Общее. Имена издателей УНЭП (указываются через ',')";
      }
      
      /// <summary>
      /// Адрес сервера СУБД Ess.
      /// </summary>
      [Public]
      public static class EssDatabaseAddress
      {
        [Public]
        public const string Guid = "D22369EF-FABF-491C-BD02-207C84534282";
        public const string Name = "Параметры подключения к БД. Адрес сервера СУБД Ess";
        [Public]
        public const string DefaultValue = "";
      }
      
      /// <summary>
      /// Порт сервера СУБД Ess.
      /// </summary>
      [Public]
      public static class EssDatabasePort
      {
        [Public]
        public const string Guid = "9393F40C-E97E-4E11-95CB-6D4167B4C656";
        public const string Name = "Параметры подключения к БД. Порт сервера СУБД Ess";
        [Public]
        public const string DefaultValue = "5432";
      }
      
      /// <summary>
      /// Наименование базы данных Ess.
      /// </summary>
      [Public]
      public static class EssDatabaseName
      {
        [Public]
        public const string Guid = "EA86FE76-09AD-4F51-97B7-C66DBF4DB29A";
        public const string Name = "Параметры подключения к БД. Наименование базы данных Ess";
        [Public]
        public const string DefaultValue = "";
      }
      
      /// <summary>
      /// Имя пользователя для подключения к СУБД Ess.
      /// </summary>
      [Public]
      public static class EssDatabaseUserName
      {
        [Public]
        public const string Guid = "5DFAA580-177E-462F-88F9-059F461E2685";
        public const string Name = "Параметры подключения к БД. Имя пользователя для подключения к СУБД Ess";
        [Public]
        public const string DefaultValue = "";
      }
      
      /// <summary>
      /// Пароль для подключения к СУБД Ess.
      /// </summary>
      [Public]
      public static class EssDatabaseUserPassword
      {
        [Public]
        public const string Guid = "F224C472-D656-4F68-9370-F62496280E4C";
        public const string Name = "Параметры подключения к БД. Пароль для подключения к СУБД Ess";
        [Public]
        public const string DefaultValue = "";
      }
      
      /// <summary>
      /// SAP код ГО.
      /// </summary>
      [Public]
      public static class GoSapCode
      {
        [Public]
        public const string Guid = "7B1134DC-5051-44F3-9065-0E72EA10B85E";
        [Public]
        public const string Name = "SAP код ГО";
      }
      
      /// <summary>
      /// Общее. Количество календарей, удаляемое за один раз ФП Норникель. Очистка частных календарей рабочего времени.
      /// </summary>
      [Public]
      public static class CountWorkingTimeCalendarsToDeleteInIteration
      {
        [Public]
        public const string Guid = "29C012EC-3D6A-48AF-9EFF-53FC5AC894C4";
        public const string Name = "Общее. Количество календарей, удаляемое за один раз ФП \"Норникель. Очистка частных календарей рабочего времени\"";
      }
      
      /// <summary>
      /// Общее. Количество дней для хранения ЧКРВ после Нового года.
      /// </summary>
      [Public]
      public static class CountDaysAfterNewYearToDeleteWorkingTimeCalendars
      {
        [Public]
        public const string Guid = "59286C73-0B96-4899-8EB0-AFE1107690B3";
        public const string Name = "Общее. Количество дней для хранения ЧКРВ после Нового года";
      }
    }
    
    #endregion
    
    /// <summary>
    /// Идентификаторы ролей.
    /// </summary>
    [Public]
    public static class RolesGuid
    {
      /// <summary>
      /// Линейные руководители.
      /// </summary>
      [Public]
      public static readonly Guid LineManager = Guid.Parse("86629D80-CB0D-44BC-8BE5-956299D592D6");
      
      /// <summary>
      /// Специалисты ДКП.
      /// </summary>
      [Public]
      public static readonly Guid SpecialistDKP = Guid.Parse("04D53BC7-75EC-4F34-9C3B-66A39B8D96FA");
    }
    
    /// <summary>
    /// Общее. Сертификаты. Официальное наименование провайдера.
    /// </summary>
    [Public]
    public const string OfficialProviderNameDefaultValue = "ПАО \"ГМК \"Норильский Никель\"";

    /// <summary>
    /// GUID'ы справочников.
    /// </summary>
    public static class DataBooks
    {
      /// <summary>
      /// Guid справочника видов документов.
      /// </summary>
      [Public]
      public static readonly Guid DocumentKindTypeGuid = Guid.Parse("14a59623-89a2-4ea8-b6e9-2ad4365f358c");
    }
    
    /// <summary>
    /// ИД провайдера удостоверяющего центра Directum СОП.
    /// </summary>
    [Public]
    public const string DirectumProviderId = "directum-css";
    
    /// <summary>
    /// Наименование УЗ служебного пользователя.
    /// </summary>
    [Public]
    public const string ServiceUserName = "Service User";
    
    /// <summary>
    /// Guid видов документов.
    /// </summary>
    public static class DocumentKind
    {
      // Мнение профсоюзной организации.
      [Public]
      public static readonly Guid TradeUnionOrganizationOpinion = Guid.Parse("5A7E4A7F-D1B5-49DD-A2C9-19D710003111");
      
      // Подтверждающий документ к заявлению на выплату по ЛНА.
      [Public]
      public static readonly Guid SupportingDocumentForLNA = Guid.Parse("3ED918CF-3D7B-4688-9BAC-6A8BD79E9606");
      
      // Подтверждающий документ к заявлению на ежегодный отпуск.
      [Public]
      public static readonly Guid SupportingDocumentForAnnualVacation = Guid.Parse("2D7C0296-B967-4B3E-BAF2-A0CB89C0D342");
      
      // Подтверждающий документ к заявлению на прочий отпуск.
      [Public]
      public static readonly Guid SupportingDocumentForOtherVacation = Guid.Parse("A3F591BC-918D-43DE-BDC0-0F8D10D83050");
      
      // Письмо в авиакомпанию.
      [Public]
      public static readonly Guid AirlineLetter = Guid.Parse("21A30E74-F281-4190-8CDA-6F031E4A8B29");
      
      // Письмо в ОВД.
      [Public]
      public static readonly Guid OVDLetter = Guid.Parse("E07C92C6-1A4B-423A-8549-6EAD6C6F7B1B");
    }

    /// <summary>
    /// Разделитель-запятая.
    /// </summary>
    [Public]
    public const char Separator = ',';
    
    /// <summary>
    /// Расширение файла для отчета по проверке файлового хранилища.
    /// </summary>
    [Public]
    public const string VerifyRepordDocExtension = "xlsx";
    
    /// <summary>
    /// Количество обрабатываемых документов по умолчанию в АО в одной итерации.
    /// </summary>
    [Public]
    public const int DefaultDocsCountInIteration = 1000;
    
    /// <summary>
    /// Наименование таблицы для АО .
    /// </summary>
    public const string VerifyDocumentsTableName = "dirrx_nncommo_verifydocument";
    
    /// <summary>
    /// Список имен объектов личного кабинета сотрудника.
    /// </summary>
    [Public]
    public static class ObjectCardTypeNames
    {
      /// <summary>
      /// Задание на доработку заявления.
      /// </summary>
      [Public]
      public const string StatementReworkAssignment = "INNCommonsStatementReworkAssignments";
    }
    
    /// <summary>
    /// Количество дней до истечения сертификата, за которые нужно усовершенствовать ЭП.
    /// </summary>
    public const int SignatureUpdateDate = 365;
    
    #region Константы для выпуска сертификата УНЭП.
    /// <summary>
    /// Запросы к SignService.
    /// </summary>
    [Public]
    public static class RequestApiSignService
    {
      /// <summary>
      /// Запрос на проверку подключения сервиса подписания.
      /// </summary>
      [Public]
      public const string SignServiceHealth = "health";
      
      /// <summary>
      /// Запрос на получение информации о облачных провайдерах.
      /// </summary>
      [Public]
      public const string GetCloudSignProviderInfos = "/v2/CloudSignProviders";
      
      /// <summary>
      /// Запрос на создание заявления на выпуск сертификата.
      /// </summary>
      [Public]
      public const string CreateStatement = "/v2/{0}/CertificateIssues/{1}/statement";
      
      /// <summary>
      /// Запрос статуса заявления на выпуск сертификата.
      /// </summary>
      [Public]
      public const string GetStatementStatus = "/v2/{0}/CertificateIssues/{1}/status";
      
      /// <summary>
      /// Запрос на выпуск сертификата.
      /// </summary>
      [Public]
      public const string CreateCertificate = "/v2/{0}/CertificateIssues";
      
      /// <summary>
      /// Запрос на подтверждение выпуска сертификата.
      /// </summary>
      [Public]
      public const string ConfirmationRequest = "/v2/{0}/CertificateIssues/{1}/confirmation-request";
      
      /// <summary>
      /// Запрос на подтверждение выпуска сертификата.
      /// </summary>
      [Public]
      public const string RevocationRequest = "/v2/{0}/Certificate/revocation";

      /// <summary>
      /// Запрос на отправку кода подтверждения для выпуска сертификата.
      /// </summary>
      [Public]
      public const string SendConfirmCode = "/v2/{0}/CertificateIssues/{1}/confirm";
      
      /// <summary>
      /// Запрос на получение сертификата.
      /// </summary>
      [Public]
      public const string GetCertificate = "/v2/{0}/Certificate/get?certificateId={1}";
      
      /// <summary>
      /// Запрос на получение способов подтверждения.
      /// </summary>
      public const string GetConfirmationType = "/v2/SigningConfirmationType/all";
      
      /// <summary>
      /// Запрос на изменение способа подтверждения по умолчанию.
      /// </summary>
      [Public]
      public const string ChangeConfirmationType = "/v2/SigningConfirmationType/default";
      
      /// <summary>
      /// Запрос на изменение способа подтверждения для владельца сертификата.
      /// </summary>
      [Public]
      public const string ChangeConfirmationTypeForCertificateOwner = "/v2/SigningConfirmationType/certificateOwner/{0}";
    }
    
    /// <summary>
    /// Имя сервиса подписания в параметре audience.
    /// </summary>
    [Public]
    public const string SignServiceAudience = "Directum.Core.SignService";
    
    /// <summary>
    /// Коды ответов возвращаемых сервисом подписания.
    /// </summary>
    [Public]
    public static class SignServiceResponseCodes
    {
      /// <summary>
      /// Запрос выполнен успешно.
      /// </summary>
      [Public]
      public const string Success = "Ok";

      /// <summary>
      /// Не найден хост
      /// </summary>
      [Public]
      public const string HostNotFound = "HostNotFound";

      /// <summary>
      /// Запрос на выпуск сертификата не найден.
      /// </summary>
      [Public]
      public const string CertificateIssueRequestNotFoundErrorCode = "CertificateIssueRequestNotFoundError";

      /// <summary>
      /// Произошла временная ошибка.
      /// </summary>
      [Public]
      public const string TemporaryUnavailableError = "TemporaryUnavailableError";

      /// <summary>
      /// Произошла внутренняя ошибка сервиса.
      /// </summary>
      [Public]
      public const string InternalServerError = "InternalServerError";

      /// <summary>
      /// В переданном запросе есть ошибки заполнения данными.
      /// </summary>
      [Public]
      public const string BadRequest = "BadRequest";

      /// <summary>
      /// Слишком частые запросы на подтверждение.
      /// </summary>
      [Public]
      public const string TooManyAttempts = "TooManyAttempts";

      /// <summary>
      /// Время жизни кода подтверждения истекло (для типа идентификации Personal).
      /// </summary>
      [Public]
      public const string ConfirmationCodeExpired = "ConfirmationCodeExpired";

      /// <summary>
      /// Неверный код подтверждения (для типа идентификации Personal).
      /// </summary>
      [Public]
      public const string WrongConfirmationCode = "WrongConfirmationCode";

      /// <summary>
      /// Получатель ещё не подтвердил свою личность (для типа идентификации ESIA)
      /// </summary>
      [Public]
      public const string ConfirmationInProgress = "ConfirmationInProgress";

      /// <summary>
      /// Пользователь отклонил запрос (для типа идентификации ESIA).
      /// </summary>
      [Public]
      public const string ConfirmationRejected = "ConfirmationRejected";

      /// <summary>
      /// Пользователь c подтвержденной учетной записью не найден (для типа идентификации ESIA).
      /// </summary>
      [Public]
      public const string UserNotFoundInEsia = "UserNotFoundInEsia";

      /// <summary>
      /// Не удалось подтвердить личность получателя, подробности во вложенной ошибке (для типа идентификации ESIA).
      /// </summary>
      [Public]
      public const string ConfirmationFailed = "ConfirmationFailed";

      /// <summary>
      /// Время на подтверждение личности истекло (для типа идентификации ESIA).
      /// </summary>
      [Public]
      public const string ConfirmationExpired = "ConfirmationExpired";

      /// <summary>
      /// До повторной отправки кода подтверждения осталось n секунд(ы).
      /// </summary>
      [Public]
      public const string ConfirmationRetryTooFastException = "ConfirmationRetryTooFastException";
    }

    /// <summary>
    /// Результаты действий.
    /// </summary>
    [Public]
    public static class ActionResultCodes
    {
      /// <summary>
      /// Действие успешно выполнено.
      /// </summary>
      [Public]
      public const string Ok = "Ok";
      
      /// <summary>
      /// Действие невыполнено.
      /// </summary>
      [Public]
      public const string Fail = "Fail";
    }
    
    /// <summary>
    /// Ключи параметров Docflow.
    /// </summary>
    public static class ParamKey
    {
      /// <summary>
      /// Ключ технологического сертификата Госключа.
      /// </summary>
      [Public]
      public const string GoskeyTechnologicalCertificate = "DirRX.SignPlatform.GoskeyCertificate";
    }
    
    /// <summary>
    /// Имя параметра с датой последнего запуска ФП "Отправка задач на перевыпуск облачных сертификатов".
    /// </summary>
    [Public]
    public const string LastTaskOfCloudCertificatesExpiration = "LastTaskOfCloudCertificatesExpiration";
    #endregion
    
    #region Константы необходимые для сравнения версий документов.
    
    /// <summary>
    /// Список расширений.
    /// </summary>
    [Public]
    public static class ExtensionsForComparison
    {
      /// <summary>
      /// Doc.
      /// </summary>
      [Public]
      public const string Doc = "doc";
      
      /// <summary>
      /// Docx.
      /// </summary>
      [Public]
      public const string Docx = "docx";
      
      /// <summary>
      /// Pdf.
      /// </summary>
      [Public]
      public const string Pdf = "pdf";
    }
    
    /// <summary>
    /// Версии PDF/A.
    /// </summary>
    [Public]
    public static class PdfAType
    {
      /// <summary>
      /// v1A.
      /// </summary>
      [Public]
      public const string V1A = "v1A";
    }
    
    #endregion
    
    #region Синхронизация календарей рабочего времени.
    
    /// <summary>
    /// Время начала календарного дня рабочего времени.
    /// </summary>
    public const double CalendarDayBeginning = 0.0;
    
    /// <summary>
    /// Время окончания календарного дня рабочего времени.
    /// </summary>
    public const double CalendarDayEnding = 23.98;
    
    /// <summary>
    /// Десятичное представление минуты по отношении к часу.
    /// </summary>
    public const double CalendarDayMinute = 0.02;
    
    /// <summary>
    /// Количество календарей, удаляемое за один раз ФП Норникель. Очистка частных календарей рабочего времени по умолчанию.
    /// </summary>
    public const int DefaultCountWorkingTimeCalendarsToDeleteInIteration = 1000;
    
    /// <summary>
    /// Количество дней для хранения ЧКРВ после Нового года.
    /// </summary>
    public const int DefaultCountDaysAfterNewYearToDeleteWorkingTimeCalendars = 14;
    
    #endregion
  }
}