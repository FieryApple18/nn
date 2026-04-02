using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;

namespace DirRX.NNIntegration.Structures.Module
{

  #region Обновление организационной структуры

  /// <summary>
  /// Структура с информацией о состоянии выполнения фонового процесса "Обновление организационной структуры".
  /// </summary>
  [Public]
  partial class CompanySynchJobState
  {
    // Ошибка обработки.
    public string ErrorMessage { get; set; }
    // Причина повторной обработки.
    public string RetryMessage { get; set; }
    // Признак того, необходимо сбросить изменения синхронизируемой записи.
    public bool ResetChanges { get; set; }
  }
  
  /// <summary>
  /// Структура с информацией о замещении для сотрудника с внутренним совместительством.
  /// </summary>
  [Public]
  partial class EmployeeSubstitutionStructure
  {
    /// <summary>
    /// Сотрудник (Внутренний совместитель)
    /// </summary>
    public IEmployee InternalWorker { get; set; }
    
    /// <summary>
    /// Замещающий (Сотрудник с основным местом работы)
    /// </summary>
    public IEmployee Substitute { get; set; }
    
    /// <summary>
    /// Признак существования замещения.
    /// </summary>
    public bool HasSubstitution { get; set; }
  }
  
  #endregion
  
  #region Создание учётных записей (RX и ЛК)

  /// <summary>
  /// Структура для создания УЗ (RX и ЛК).
  /// </summary>
  [Public]
  partial class CreateAccountStruct
  {
    // Сотрудник.
    public string Employee { get; set; }
    // Логин.
    public string Login { get; set; }
    // Тип учетной записи.
    public string Type { get; set; }
    // Подписано согласие на КЭДО.
    public bool IsKEDO { get; set; }
    // Провайдер.
    public string Provider { get; set; }
  }
  
  /// <summary>
  /// Структура ошибок для процесса создания УЗ.
  /// </summary>
  [Public]
  partial class ProcessingErrorStruct
  {
    public bool isBlock { get; set; }
    public string ErrorText { get; set; }
  }
  
  #endregion
  
  #region Проштамповка документов
  
  #region IF-S-7601. Передача документа на проштамповку.
  
  /// <summary>
  /// Общая структура передачи документа на проштамповку.
  /// </summary>
  [Public (Isolated = true)]
  partial class SendDocumentToStampingModel
  {
    /// <summary>
    /// Параметры задания.
    /// </summary>
    public string attributes { get; set; }
    
    /// <summary>
    /// Идентификатор шаблона.
    /// </summary>
    public string template_id { get; set; }
    
    /// <summary>
    /// Внешний идентификатор запроса.
    /// </summary>
    public string request_id { get; set; }
    
    /// <summary>
    /// Признак того, что при отправке на проштамповку нужно переформировать public body документа.
    /// </summary>
    public bool OverwritePublicBodyFromVersionBody { get; set; }
  }
  
  /// <summary>
  /// Параметры задания.
  /// </summary>
  [Public (Isolated = true)]
  partial class Attributes
  {
    /// <summary>
    /// Параметры, передаваемые в шаблон, при установке штампа.
    /// </summary>
    public DirRX.NNIntegration.Structures.Module.IDocumentForStamping document { get; set; }
  }
  
  /// <summary>
  /// Параметры, передаваемые в шаблон, при установке штампа.
  /// </summary>
  [Public (Isolated = true)]
  partial class DocumentForStamping
  {
    /// <summary>
    /// Имя, фамилия и отчество подписанта ПЭП.
    /// </summary>
    public string certificate_owner { get; set; }
    
    /// <summary>
    /// Передаваемое значение даты и времени подписания.
    /// </summary>
    public string signed_date_time { get; set; }
    
    /// <summary>
    /// Название организации.
    /// </summary>
    public string organization { get; set; }
    
    /// <summary>
    /// Ключ проверки.
    /// </summary>
    public string verification_key { get; set; }
    
    /// <summary>
    /// Сертификат.
    /// </summary>
    public string certificate { get; set; }
    
    /// <summary>
    /// Дата действия сертификата с.
    /// </summary>
    public string certificate_valid_period_start { get; set; }
    
    /// <summary>
    /// Дата действия сертификата по.
    /// </summary>
    public string certificate_valid_period_end { get; set; }
  }
  
  /// <summary>
  /// Результат проштамповки документа.
  /// </summary>
  [Public(Isolated=true)]
  partial class ResultStampingDocument
  {
    /// <summary>
    /// Внешний идентификатор запроса.
    /// </summary>
    public string request_id { get; set; }
    
    /// <summary>
    /// Статус выполнения.
    /// </summary>
    public string status { get; set; }
    
    /// <summary>
    /// Сообщения об ошибках.
    /// </summary>
    public List<string> messages { get; set; }
    
    /// <summary>
    /// Обработанный документ.
    /// </summary>
    public string file { get; set; }
  }
  
  #endregion
  
  #endregion
  
  #region Общие структуры интеграции с КСИП
  
  /// <summary>
  /// Результат отправки сообщения в КСИП.
  /// </summary>
  [Public(Isolated=true)]
  partial class ResultSendingMessage
  {
    /// <summary>
    /// Код результата.
    /// </summary>
    public int Code { get; set; }
    
    /// <summary>
    /// Текст ответа.
    /// </summary>
    public string AnswerText { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Результат Case-функции обработки входящего сообщения.
  /// </summary>
  [Public]
  partial class ProcessIncomingMessageResult
  {
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
    
    /// <summary>
    /// Признак, который определяет, нужно ли повторно выполнять АО после возникновения ошибки.
    /// </summary>
    public bool Retry { get; set; }
    
    /// <summary>
    /// Идентификатор созданой/обновленной сущности RX.
    /// </summary>
    public long? EntityId { get; set; }
  }
  
  /// <summary>
  /// Результат формирования текста исходящего сообщения.
  /// </summary>
  [Public]
  partial class ResultGenerateRequestText
  {
    
    /// <summary>
    /// Текст исходящего сообщения.
    /// </summary>
    public string RequestText { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Результат асинхронной отправки данных из КСИП в АСУП.
  /// </summary>
  [Public(Isolated=true)]
  partial class MessageSendingAsyncResult
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Результат обработки запроса.
    /// </summary>
    public string Result { get; set; }
    
    /// <summary>
    /// Результат проведения мероприятия в АСУП.
    /// </summary>
    public bool IsSuccessfullyProcessed { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Структура для сериализации ответа на входящий запрос из внешней системы.
  /// </summary>
  [Public (Isolated = true)]
  partial class ResponseToExternalSystemRequest
  {
    public string Result { get; set; }
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Структура для сериализации ответа на входящий запрос из внешней системы.
  /// </summary>
  [Public (Isolated = true)]
  partial class Test
  {
    public System.IO.Stream Result { get; set; }
    public Dictionary<string, string> ErrorText { get; set; }
  }
  
  /// <summary>
  /// Ответ из системы АСУП на асинхронный исходящий запрос.
  /// </summary>
  [Public(Isolated=true)]
  partial class AnswerOnOutgoingMessage
  {
    /// <summary>
    /// Результат обработки.
    /// </summary>
    public string Result { get; set; }
    
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Информация о документе для передачи в АСУП.
  /// </summary>
  [Public(Isolated=true)]
  partial class DocToSAPFioriInfo
  {
    /// <summary>
    /// Идентификатор документа в SAP.
    /// </summary>
    public string SAPID { get; set; }
    /// <summary>
    /// Результат согласования.
    /// </summary>
    public string ApprovalState { get; set; }
    
    /// <summary>
    /// Вид Документа.
    /// </summary>
    public string Kind { get; set; }
    
    /// <summary>
    /// Наименование документа.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Дата создания документа.
    /// </summary>
    public string DateCreate { get; set; }
    
    /// <summary>
    /// Табельный номер.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// ИД Документа в Directum RX.
    /// </summary>
    public long RXID { get; set; }
    
    /// <summary>
    /// Идентификатор типа документа.
    /// </summary>
    public string Type { get; set; }
  }
  
  /// <summary>
  /// Структура для создания задания на ручное проведение мероприятия.
  /// </summary>
  [Public(Isolated=true)]
  partial class ManualHoldingEventStruct
  {
    /// <summary>
    /// Идентификатор документа, переданный в потоке.
    /// </summary>
    public string SAPID { get; set; }
  }
  
  #region IF-S-10532. Передача статуса приказа (КЭДО -> АСУП).
  
  /// <summary>
  /// Информация о статусе приказа для передачи из КЭДО в АСУП.
  /// </summary>
  [Public(Isolated=true)]
  partial class StatusDocToSAPInfo
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Рег.№.
    /// </summary>
    public string RegNumber { get; set; }
    
    /// <summary>
    /// Дата документа.
    /// </summary>
    public string Date { get; set; }

    /// <summary>
    /// Cогласование. Возможные значения: Подписан; Отклонен.
    /// </summary>
    public string Approval { get; set; }

    /// <summary>
    /// ИД Документа в Directum RX.
    /// </summary>
    public long RXID	{ get; set; }

    /// <summary>
    /// Признак необходимости автоматической отмены мероприятия.
    /// </summary>
    public bool NeedCancelAction { get; set; }
    
    /// <summary>
    /// Код вида документа.
    /// </summary>
    public string Kind { get; set; }
    
    /// <summary>
    /// Идентификатор типа документа.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// SAP ID документа.
    /// </summary>
    public string SAPID { get; set; }
    
    /// <summary>
    /// Тело документа.
    /// </summary>
    public string Body { get; set; }
  }
  
  #endregion
  
  #endregion
  
  #region Отпуска
  
  #region IF-S-9323. Передача приказа на предоставление отпуска из АСУП в КЭДО (АСУП -> КЭДО)

  /// <summary>
  /// Структура для создания карточки приказа на ежегодный отпуск по заявлению.
  /// </summary>
  [Public(Isolated=true)]
  partial class AnnualVacationOrderCard
  {
    /// <summary>
    /// Подписал.
    /// </summary>
    public string Signed { get; set; }
    
    /// <summary>
    /// Подготовил.
    /// </summary>
    public string Prepared { get; set; }
    
    /// <summary>
    /// Регистрационный номер.
    /// </summary>
    public string RegNumber { get; set; }
    
    /// <summary>
    /// Дата документа.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Шаблон письма в авиакомпанию.
    /// </summary>
    public string BodyAvia { get; set; }
    
    /// <summary>
    /// Шаблон письма в ОВД.
    /// </summary>
    public string BodyOVD { get; set; }
    
    /// <summary>
    /// Шаблон приказа.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// SAP ID.
    /// </summary>
    public string SAPID { get; set; }
    
    /// <summary>
    /// Вид документа.
    /// </summary>
    public string Kind { get; set; }
  }
  
  #endregion
  
  #region IF-S-10411. Передача приказа на предоставление прочего отпуска из АСУП в КЭДО (АСУП -> КЭДО)
  /// <summary>
  /// Структура для создания карточки приказа на прочий отпуск по заявлению.
  /// </summary>
  [Public(Isolated=true)]
  partial class OtherVacationOrderCard
  {
    /// <summary>
    /// Подписал.
    /// </summary>
    public string Signed { get; set; }
    
    /// <summary>
    /// Подготовил.
    /// </summary>
    public string Prepared { get; set; }
    
    /// <summary>
    /// Регистрационный номер.
    /// </summary>
    public string RegNumber { get; set; }
    
    /// <summary>
    /// Дата документа.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Тело документа.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// SAP ID.
    /// </summary>
    public string SAPID { get; set; }
    
    /// <summary>
    /// Код SAP вида документа.
    /// </summary>
    public string Kind { get; set; }
  }
  #endregion
  
  #region IF-S-10396. Передача приказа на компенсацию части ежегодного оплачиваемого отпуска из АСУП в КЭДО (АСУП -> КЭДО)
  /// <summary>
  /// Структура для создания карточки приказа на компенсацию части ежегодного оплачиваемого отпуска.
  /// </summary>
  [Public(Isolated=true)]
  partial class VacationCompensationOrderCard
  {
    /// <summary>
    /// Подписал.
    /// </summary>
    public string Signed { get; set; }
    
    /// <summary>
    /// Подготовил.
    /// </summary>
    public string Prepared { get; set; }
    
    /// <summary>
    /// Регистрационный номер.
    /// </summary>
    public string RegNumber { get; set; }
    
    /// <summary>
    /// Дата документа.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Дата мероприятия.
    /// </summary>
    public DateTime DateEvent { get; set; }
    
    /// <summary>
    /// Тело документа.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// SAP ID.
    /// </summary>
    public string SAPID { get; set; }
  }
  #endregion
  
  #region IF-S-10391. Передача приказа об отзыве из отпуска из АСУП в КЭДО (АСУП -> КЭДО)
  /// <summary>
  /// Структура для создания карточки приказа об отзыве из отпуска.
  /// </summary>
  [Public(Isolated=true)]
  partial class RecallVacationOrderCard
  {
    /// <summary>
    /// Подписал.
    /// </summary>
    public string Signed { get; set; }
    
    /// <summary>
    /// Подготовил.
    /// </summary>
    public string Prepared { get; set; }
    
    /// <summary>
    /// Регистрационный номер.
    /// </summary>
    public string RegNumber { get; set; }
    
    /// <summary>
    /// Дата документа.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Тело документа.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Основание для приказа.
    /// </summary>
    public string Basis { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// SAP ID.
    /// </summary>
    public string SAPID { get; set; }
  }
  #endregion
  
  #region IF-S-9336. Передача уведомления об отпуске из АСУП в КЭДО (АСУП-> КЭДО)
  
  /// <summary>
  /// Информация по отпускам сотрудников.
  /// </summary>
  [Public(Isolated = true)]
  partial class IncomingVacationsInfo
  {
    /// <summary>
    /// Информация по отпускам сотрудников.
    /// </summary>
    public List<DirRX.NNIntegration.Structures.Module.IVacationInfo> EmployeesVacationsInfo { get; set; }
    
    /// <summary>
    /// Адрес эл. почты.
    /// </summary>
    public string Email { get; set; }
  }
  
  /// <summary>
  /// Информация по отпуску сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfo
  {
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public string DateStart { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public string DateEnd { get; set; }
    
    /// <summary>
    /// Вид планового отпуска.
    /// </summary>
    public string TypeVacation { get; set; }
  }
  
  /// <summary>
  /// Информация по отпуску сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfoDateTime
  {
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string PersonnelNumber { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime DateStart { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime DateEnd { get; set; }
    
    /// <summary>
    /// Вид планового отпуска.
    /// </summary>
    public string TypeVacation { get; set; }
  }
  
  
  #endregion
  
  #region IF-S-6365. Создание карточки согласования графика отпусков из АСУП в КЭДО.
  
  /// <summary>
  /// Информация о графике отпусков.
  /// </summary>
  [Public(Isolated = true)]
  partial class IncomingVacationScheduleDocumentInfoFromSAP
  {
    /// <summary>
    /// Содержимое документа в формате base64.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// SAP Тип организации.
    /// </summary>
    public string OrgType { get; set; }
    
    /// <summary>
    /// SAP Код юридического лица.
    /// </summary>
    public string OrgCode { get; set; }
    
    /// <summary>
    /// На год.
    /// </summary>
    public string Year { get; set; }
    
    /// <summary>
    /// Идентификатор документа в SAP.
    /// </summary>
    public string SAPID { get; set; }
    
    /// <summary>
    /// Список работников.
    /// </summary>
    public List<DirRX.NNIntegration.Structures.Module.IVacationInfoFromSAP> Employees { get; set; }
  }
  
  /// <summary>
  /// Информация об отпуске по работнику.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfoFromSAP
  {
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Вид планового отпуска.
    /// </summary>
    public string TypeVacation { get; set; }
    
    /// <summary>
    /// Начало планового отпуска.
    /// </summary>
    public string DateStart { get; set; }
    
    /// <summary>
    /// Окончание планового отпуска.
    /// </summary>
    public string DateEnd { get; set; }
    
    /// <summary>
    /// Кол-во дней планового отпуска.
    /// </summary>
    public int? DayCount { get; set; }
  }
  
  /// <summary>
  /// Информация о графике отпусков.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationScheduleDocumentInfoFromSAP
  {
    /// <summary>
    /// Содержимое документа в формате base64.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// SAP Тип организации.
    /// </summary>
    public string OrgType { get; set; }
    
    /// <summary>
    /// SAP Код юридического лица.
    /// </summary>
    public string OrgCode { get; set; }
    
    /// <summary>
    /// На год.
    /// </summary>
    public string Year { get; set; }
    
    /// <summary>
    /// Идентификатор документа в SAP.
    /// </summary>
    public string SAPID { get; set; }
    
    /// <summary>
    /// Список работников.
    /// </summary>
    public List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTimeFromSAP> Employees { get; set; }
  }
  
  /// <summary>
  /// Информация об отпуске по работнику.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfoDateTimeFromSAP
  {
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string EmplPersNum { get; set; }
    
    /// <summary>
    /// Вид планового отпуска.
    /// </summary>
    public string TypeVacation { get; set; }
    
    /// <summary>
    /// Начало планового отпуска.
    /// </summary>
    public DateTime? DateStart { get; set; }
    
    /// <summary>
    /// Окончание планового отпуска.
    /// </summary>
    public DateTime? DateEnd { get; set; }
    
    /// <summary>
    /// Кол-во дней планового отпуска.
    /// </summary>
    public int? DayCount { get; set; }
  }
  
  #endregion
  
  #region Структуры задачи "Регистрация ежегодного отпуска"
  /// <summary>
  /// Информация по отпускам сотрудников.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationsInfo
  {
    /// <summary>
    /// Информация по отпускам сотрудников.
    /// </summary>
    public List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTime> EmployeesVacationsInfo { get; set; }
    
    /// <summary>
    /// Адрес эл. почты.
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  #endregion
  
  #endregion
  
  #region Прочие процессы
  
  #region Участие в благотворительности
  
  #region IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП (АСУП → КЭДО)
  /// <summary>
  /// Структура для создания карточки заявления на удержание из АСУП.
  /// </summary>
  [Public(Isolated=true)]
  partial class RetentionStatementDocumentCard
  {
    /// <summary>
    /// ID программы.
    /// </summary>
    public string ProgramID { get; set; }
    
    /// <summary>
    /// Наименование программы.
    /// </summary>
    public string ProgramName { get; set; }
    
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Дата подачи.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Содержимое документа в формате base64.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// Логин.
    /// </summary>
    public string Login { get; set; }
    
    /// <summary>
    /// Дата подписания.
    /// </summary>
    public DateTime DateSigned { get; set; }
    
    /// <summary>
    /// SAP ID.
    /// </summary>
    public string SAPID { get; set; }
  }
  
  #endregion
  
  #endregion
  
  #region Командировки
  
  #region IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП (АСУП → КЭДО)
  
  /// <summary>
  /// Атрибутный состав интерфейса "Создание карточки согласования приказа на командировку в КЭДО из АСУП".
  /// </summary>
  [Public (Isolated = true)]
  partial class CreateBusinessTripOrder
  {
    // Регистрационный номер.
    public string RegNumber { get; set; }
    // Дата документа.
    public DateTime Date { get; set; }
    // Вид документа.
    public string Kind { get; set; }
    // Номер заявки.
    public string Request { get; set; }
    // Табельный номер сотрудника.
    public string Employee { get; set; }
    // Табельный номер работодателя.
    public string Supervisor { get; set; }
    // Табельный номер сотрудника подготовившего документ.
    public string Prepared { get; set; }
    // Место назначения.
    public string Destination { get; set; }
    // Календарные дни.
    public int Days { get; set; }
    // Дата начала.
    public DateTime DateStart { get; set; }
    // Дата окончания.
    public DateTime DateEnd { get; set; }
    // Тело документа.
    public string Body { get; set; }
    // Идентификатор документа в SAP.
    public string SAPID { get; set; }
    // Регистрационный номер ведущего документа.
    public string MainOrderRegNumber { get; set; }
    // Дата ведущего документа.
    public DateTime? MainOrderDate { get; set; }
  }
  
  #endregion
  
  #endregion
  
  #region Регистрация работы в выходной и/или праздничный день
  
  #region IF-S-10412. Автоматическая проверка заявления на работу в выходной и/или праздничный день (КЭДО -> АСУП)
  
  #region Запрос в КЭДО.

  /// <summary>
  /// Структура запроса для автоматической проверки заявления на работу в выходной и/или праздничный день.
  /// </summary>
  [Public (Isolated = true)]
  partial class ValidateWeekendWorkStatementRequest
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Вид документа.
    /// </summary>
    public string Kind { get; set; }
    
    /// <summary>
    /// Групповой.
    /// </summary>
    public bool IsGroup { get; set; }
    
    /// <summary>
    /// Список сотрудников.
    /// </summary>
    public List<DirRX.NNIntegration.Structures.Module.IValidateWeekendWorkStatementEmployeeInfoRequest> Employees { get; set; }
  }
  
  /// <summary>
  /// Структура с информацией по сотрунику для запроса для автоматической проверки заявления на работу в выходной и/или праздничный день.
  /// </summary>
  [Public (Isolated = true)]
  partial class ValidateWeekendWorkStatementEmployeeInfoRequest
  {
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Дата работы с.
    /// </summary>
    public string DateStart { get; set; }
    
    /// <summary>
    /// Время работы с.
    /// </summary>
    public string TimeStart { get; set; }
    
    /// <summary>
    /// Дата работы по.
    /// </summary>
    public string DateEnd { get; set; }
    
    /// <summary>
    /// Время работы по.
    /// </summary>
    public string TimeEnd { get; set; }
    
    /// <summary>
    /// Дата доп. отдыха с.
    /// </summary>
    public string DateWeekendStart { get; set; }
    
    /// <summary>
    /// Время доп. отдыха с.
    /// </summary>
    public string TimeWeekendStart { get; set; }
    
    /// <summary>
    /// Дата доп. отдыха по.
    /// </summary>
    public string DateWeekendEnd { get; set; }
    
    /// <summary>
    /// Время доп. отдыха по.
    /// </summary>
    public string TimeWeekendEnd { get; set; }
  }
  #endregion
  
  #region Ответ из КЭДО.

  /// <summary>
  /// Структура запроса для автоматической проверки заявления на работу в выходной и/или праздничный день.
  /// </summary>
  [Public (Isolated = true)]
  partial class ValidateWeekendWorkStatementResponse
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Список сотрудников.
    /// </summary>
    public List<DirRX.NNIntegration.Structures.Module.IValidateWeekendWorkStatementEmployeeInfoResponse> Employees { get; set; }
  }
  
  /// <summary>
  /// Структура с информацией по сотрунику для запроса для автоматической проверки заявления на работу в выходной и/или праздничный день.
  /// </summary>
  [Public (Isolated = true)]
  partial class ValidateWeekendWorkStatementEmployeeInfoResponse
  {
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Результат проверки.
    /// </summary>
    public bool Result { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
    
  }
  #endregion
  
  #endregion
  
  #region IF-S-10413. Передача приказа на выход сотрудника в выходной и/или праздничный день из АСУП в КЭДО (АСУП -> КЭДО).
  
  #region Запрос.
  
  /// <summary>
  /// Структура запроса для создания приказа на выход сотрудника в выходной и/или праздничный день.
  /// </summary>
  [Public(Isolated=true)]
  partial class CreateWeekendWorkOrderStructure
  {
    
    /// <summary>
    /// Вид документа.
    /// </summary>
    public string Kind { get; set; }
    
    /// <summary>
    /// Подписал.
    /// </summary>
    public string Signed { get; set; }
    
    /// <summary>
    /// Подготовил.
    /// </summary>
    public string Prepared { get; set; }
    
    /// <summary>
    /// Номер отменяемого приказа.
    /// </summary>
    public string CancelsNumber { get; set; }
    
    /// <summary>
    /// Дата отменяемого приказа.
    /// </summary>
    public DateTime? CancelsDate { get; set; }
    
    /// <summary>
    /// Рег.№.
    /// </summary>
    public string RegNumber { get; set; }
    
    /// <summary>
    /// Дата документа.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Дата мероприятия.
    /// </summary>
    public DateTime DateEvent { get; set; }
    
    /// <summary>
    /// Документ.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// Идентификатор документа в SAP.
    /// </summary>
    public string SAPID { get; set; }
  }
  
  #endregion
  
  #endregion
  
  #region IF-S-6363. Автоматическое создание мероприятия «Регистрация работы в выходной и/или праздничный день» из КЭДО в АСУП (КЭДО -> АСУП)
  
  #region Запрос.
  /// <summary>
  /// Структура запроса для создания мероприятия «Регистрация работы в выходной и/или праздничный день»
  /// </summary>
  [Public (Isolated = true)]
  partial class CreateWeekendWorkEvent
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Вид документа.
    /// </summary>
    public string Kind { get; set; }
    
    /// <summary>
    /// Групповой.
    /// </summary>
    public bool IsGroup { get; set;}
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// Информация о сотрудниках.
    /// </summary>
    public List<DirRX.NNIntegration.Structures.Module.ICreateWeekendWorkEventEmployeesInfo> Employees { get; set; }
    
    /// <summary>
    /// Уточнение причины.
    /// </summary>
    public string ReasonSpecification { get; set; }
  }
  
  [Public (Isolated = true)]
  partial class CreateWeekendWorkEventEmployeesInfo
  {
    /// <summary>
    /// Таб. номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Дата работы с.
    /// </summary>
    public string DateStart { get; set; }
    
    /// <summary>
    /// Время работы с.
    /// </summary>
    public string TimeStart { get; set; }
    
    /// <summary>
    /// Дата работы по.
    /// </summary>
    public string DateEnd { get; set; }
    
    /// <summary>
    /// Время работы по.
    /// </summary>
    public string TimeEnd { get; set; }
    
    /// <summary>
    /// Дата доп. отдыха с.
    /// </summary>
    public string DateWeekendStart { get; set; }
    
    /// <summary>
    /// Время доп. отдыха с.
    /// </summary>
    public string TimeWeekendStart { get; set; }
    
    /// <summary>
    /// Дата доп. отдыха по.
    /// </summary>
    public string DateWeekendEnd { get; set; }
    
    /// <summary>
    /// Время доп. отдыха по.
    /// </summary>
    public string TimeWeekendEnd { get; set; }
    
    /// <summary>
    /// Чек бокс «Без перерыва».
    /// </summary>
    public bool WithoutABreak { get; set; }
    
    /// <summary>
    /// Первый перерыв с.
    /// </summary>
    public string FirstBreakStart { get; set; }
    
    /// <summary>
    /// Первый перерыв по.
    /// </summary>
    public string FirstBreakEnd { get; set; }
    
    /// <summary>
    /// Второй перерыв с.
    /// </summary>
    public string SecondBreakStart { get; set; }
    
    /// <summary>
    /// Второй перерыв по.
    /// </summary>
    public string SecondBreakEnd { get; set; }
    
    /// <summary>
    /// Компенсация.
    /// </summary>
    public string Compensation { get; set; }
  }
  
  #endregion
  
  #endregion
  
  #region IF-S-6362. Автоматическое внесение изменений в действующий приказ из КЭДО в АСУП (КЭДО -> АСУП)
  
  #region Запрос.
  /// <summary>
  /// Структура запроса для создания мероприятия «IF-S-6362. Автоматическое внесение изменений в действующий приказ»
  /// </summary>
  [Public (Isolated = true)]
  partial class CreateWeekendWorkCancelationEvent
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Вид документа.
    /// </summary>
    public string Kind { get; set; }
    
    /// <summary>
    /// Отменяет действие приказа номер.
    /// </summary>
    public string CancelsNumber { get; set; }
    
    /// <summary>
    /// Отменяет действие приказа дата.
    /// </summary>
    public string CancelsDate { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// Информация о сотрудниках.
    /// </summary>
    public List<DirRX.NNIntegration.Structures.Module.ICreateWeekendWorkCancelationEventEmployeesInfo> Employees { get; set; }
  }
  
  [Public (Isolated = true)]
  partial class CreateWeekendWorkCancelationEventEmployeesInfo
  {
    /// <summary>
    /// Таб. номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
  }
  
  #endregion
  
  #endregion
  
  #endregion
  
  #region Регистрация выплат по ЛНА.
  
  #region IF-S-6366. Автоматическое создание мероприятия «Регистрация выплат ЛНА» из КЭДО в АСУП (КЭДО → АСУП)
  
  #region Запрос.
  
  /// <summary>
  /// Структура запроса для создания мероприятия «Регистрация выплат ЛНА».
  /// </summary>
  [Public (Isolated = true)]
  partial class CreateLrdPaymentEvent
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Сотрудник.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Код причины мероприятия.
    /// </summary>
    public string EventReason { get; set; }
    
    /// <summary>
    /// Код уточнения причины.
    /// </summary>
    public string Refinement { get; set; }
    
    /// <summary>
    /// Код назначения платежа.
    /// </summary>
    public string Purpose { get; set; }
    
    /// <summary>
    /// Дата платежа.
    /// </summary>
    public string PaymentDate { get; set; }
    
    /// <summary>
    /// Сумма утвержденная.
    /// </summary>
    public double Amount { get; set; }
    
    /// <summary>
    /// ФИО ребенка.
    /// </summary>
    public string ChildName { get; set; }
    
    /// <summary>
    /// Дата рождения ребёнка.
    /// </summary>
    public string BirthDate { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
  }
  
  #endregion
  
  #endregion
  
  #region IF-S-10400. Проверка данных по заявлению на выплату ЛНА (АСУП → КЭДО)
  
  #region Запрос.
  
  /// <summary>
  /// Информация об отпуске по работнику.
  /// </summary>
  [Public(Isolated = true)]
  partial class LrdPaymentCheckRequest
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Сотрудник.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Код причины мероприятия.
    /// </summary>
    public string EventReason { get; set; }
    
    /// <summary>
    /// Код уточнения причины.
    /// </summary>
    public string Refinement { get; set; }
    
    /// <summary>
    /// Код назначения платежа.
    /// </summary>
    public string Purpose { get; set; }
    
    /// <summary>
    /// ФИО ребенка.
    /// </summary>
    public string ChildName { get; set; }
    
    /// <summary>
    /// Дата рождения.
    /// </summary>
    public string BirthDate { get; set; }
    
    /// <summary>
    /// Дата подачи.
    /// </summary>
    public string CurrentDate { get; set; }
  }
  
  #endregion
  
  #region Ответ.
  
  /// <summary>
  /// Информация об отпуске по работнику.
  /// </summary>
  [Public(Isolated = true)]
  partial class LrdPaymentCheckResponse
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Сотрудник.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Результат проверки.
    /// </summary>
    public bool Result { get; set; }
    
    /// <summary>
    /// Текст замечания.
    /// </summary>
    public string RemarkText { get; set; }
  }
  #endregion
  
  #endregion
  
  #region IF-S-25360. Получение списка детей работника. (АСУП → КЭДО).
  
  #region Запрос.
  
  /// <summary>
  /// Информация о детях по работнику.
  /// </summary>
  [Public(Isolated = true)]
  partial class LrdPaymentChildrenRequest
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Сотрудник.
    /// </summary>
    public string Employee { get; set; }

  }
  
  #endregion
  
  #region Ответ.
  
  /// <summary>
  /// Информация о детях по работнику.
  /// </summary>
  [Public(Isolated = true)]
  partial class LrdPaymentChildrenResponse
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Дети.
    /// </summary>
    public List<DirRX.NNIntegration.Structures.Module.IChild> Children { get; set; }
  }
  
  /// <summary>
  /// Информация о ребенке.
  /// </summary>
  [Public(Isolated = true)]
  partial class Child
  {
    /// <summary>
    /// ФИО ребенка.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Дата рождения.
    /// </summary>
    public string ChildBirthDate { get; set; }
    
    /// <summary>
    /// Инвалидность.
    /// </summary>
    public bool IsDisabled { get; set; }
  }
  
  #endregion
  
  #endregion
  
  #region IF-S-10399. Передача приказа на выплату по ЛНА из АСУП в КЭДО.
  
  /// <summary>
  /// Структура для создания приказа на выплату по ЛНА.
  /// </summary>
  [Public(Isolated = true)]
  partial class LrdPaymentOrderRequest
  {
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string Employee { get; set; }
    
    /// <summary>
    /// Подписал.
    /// </summary>
    public string Signed { get; set; }
    
    /// <summary>
    /// Подготовил.
    /// </summary>
    public string Prepared { get; set; }
    
    /// <summary>
    /// Регистрационный номер.
    /// </summary>
    public string RegNumber { get; set; }
    
    /// <summary>
    /// Дата документа.
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Тело документа.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// SAP ID.
    /// </summary>
    public string SAPID { get; set; }
  }
  
  #endregion
  
  /// <summary>
  /// Структура с информацией по типу выплаты ЛНА для ЛК.
  /// </summary>
  [Public (Isolated = true)]
  partial class PaymentTypeStructure
  {
    // Ид типа выплаты.
    public long Id { get; set; }
    // Наименование.
    public string Name { get; set; }
  }
  
  /// <summary>
  /// Структура с информацией по настройке типа выплаты ЛНА для ЛК.
  /// </summary>
  [Public (Isolated = true)]
  partial class PaymentTypeSettingStructure
  {
    // Ид типа выплаты.
    public long Id { get; set; }
    // Фиксированная выплата.
    public bool FixedAmount { get; set; }
    // Наименование.
    public string Amount { get; set; }
    // Инструкция.
    public string Instruction { get; set; }
    // Признак того, что тип выплаты относится к материальной помощи по ребенку.
    public bool IsChildSupport { get; set; }
    // Признак того, что сотрудник - мужчина.
    public bool IsMale { get; set; }
  }
  
  /// <summary>
  /// Структура с информацией по заданию доработки заявления для выплаты по ЛНА.
  /// </summary>
  [Public (Isolated = true)]
  partial class LrdPaymentAssignmentInfo
  {
    // ИД исполнителя задания.
    public long EmployeeId { get; set; }
    // ИД заявления.
    public long StatementId { get; set; }
    // ИД типа выплаты.
    public long PaymentTypeId { get; set; }
    // ИД наименование типа выплаты.
    public string PaymentTypeName { get; set; }
    // ИД заявления.
    public string ChildFIO { get; set; }
    // Наименование.
    public DateTime? ChildBirthDate { get; set; }
    // Сумма.
    public string Amount { get; set; }
    // Причина.
    public string Reason { get; set; }
    // Информация об исполнителе.
    public string PerformerInfo { get; set; }
  }
  
  #endregion
  
  #endregion

  #region Отправка Push-уведомлений в ПМД
  
  #region IF-S-25401. Передача результата нажатия на действие в центре уведомлений ПМД (ПМД → КЭДО).
  
  /// <summary>
  /// Объект с результатами обработки каждого уведомления.
  /// </summary>
  [Public(Isolated = true)]
  partial class SupernikaNotificationStatus
  {
    // Массив результатов действий в Супернике.
    public List<DirRX.NNIntegration.Structures.Module.ISupernikaNotification> notifications { get ; set; }
  }
  
  /// <summary>
  /// Результат действия в Супернике.
  /// </summary>
  [Public(Isolated = true)]
  partial class SupernikaNotification
  {
    // Идентификатор уведомления.
    public string unique_id { get ; set; }
    // Статус-код обработки.
    public string status { get ; set; }
    // Сообщение об ошибке.
    public string message { get ; set; }
    // Кнопки.
    public List<DirRX.NNIntegration.Structures.Module.ISupernikaButton> buttons { get ; set; }
  }
  
  /// <summary>
  /// Кнопка.
  /// </summary>
  [Public(Isolated = true)]
  partial class SupernikaButton
  {
    // Тип кнопки.
    public string button_type { get ; set; }
    // Комментарий.
    public string comment { get ; set; }
    // Статус-код нажатия кнопки.
    public string button_status { get ; set; }
  }
  
  
  #endregion
  
  #endregion
  
}