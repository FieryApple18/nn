using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNVacations.Structures.Module
{
  
  /// <summary>
  /// Период отпуска.
  /// </summary>
  partial class VacationPeriod
  {
    // Дата начала отпуска.
    public DateTime? StartDate { get; set; }
    // Дата окончания отпуска.
    public DateTime? EndDate { get; set; }
  }
  
  #region IF-S-25553. Передача статусов отправки уведомлений об отпуске из КЭДО в АСУП (КЭДО -> АСУП).
  
  /// <summary>
  /// Информация по отпуску сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfoIFS25553
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
    
    /// <summary>
    /// Результат обработки.
    /// </summary>
    public string Result { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  #endregion
  
  #region IF-S-6373. Регистрация ежегодного отпуска (по графику отпусков) из КЭДО в АСУП (КЭДО -> АСУП).
  
  /// <summary>
  /// Информация по отпускам сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class RegistrationAnnualVacationBySchedule
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
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// Информация по отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IVacationInfoIFS6373> ListVacations { get; set; }
  }
  
  /// <summary>
  /// Информация об отпуске.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfoIFS6373
  {
    /// <summary>
    /// Дата начала.
    /// </summary>
    public string DateStart { get; set; }
    
    /// <summary>
    /// Вид планового отпуска.
    /// </summary>
    public string TypeVacation { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DayCount { get; set; }
  }
  
  #endregion
  
  #region IF-S-6376. Регистрация прочего отпуска из КЭДО в АСУП (КЭДО -> АСУП).
  
  /// <summary>
  /// Информация по отпуску сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class RegistrationOtherVacation
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
    /// Тип отпуска.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Причина отпуска.
    /// </summary>
    public string Reason { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public string DateStart { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DaysCount { get; set; }
    
    /// <summary>
    /// Аспирант.
    /// </summary>
    public bool? IsStudent { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
  }
  
  #endregion
  
  #region IF-S-6372. Регистрация ежегодного отпуска (по заявлению) из КЭДО в АСУП (КЭДО -> АСУП).
  
  /// <summary>
  /// Информация по отпускам сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class RegistrationAnnualVacationByStatement
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
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// Информация по отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IVacationInfoIFS6372> ListVacations { get; set; }
  }
  
  /// <summary>
  /// Информация об отпуске.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfoIFS6372
  {
    /// <summary>
    /// Дата начала.
    /// </summary>
    public string DateStart { get; set; }
    
    /// <summary>
    /// Тип отпуска.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DayCount { get; set; }
    
    /// <summary>
    /// Причина отпуска.
    /// </summary>
    public string Reason { get; set; }
  }
  
  #endregion
  
  #region IF-S-6369. Автоматическое создание мероприятия «Компенсация части ежегодного оплачиваемого отпуска» из КЭДО в АСУП.
  
  /// <summary>
  /// Информация по отпускам сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class CompensationForPartOfAnnualPaidLeave
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
    /// Выбрано дней.
    /// </summary>
    public int DaysSelected { get; set; }
    
    /// <summary>
    /// Дата документа.
    /// </summary>
    public string Date { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
  }
  
  #endregion
  
  #region IF-S-6361. Автоматическое создание мероприятия "Отзыв из отпуска" из КЭДО в АСУП.
  
  /// <summary>
  /// Информация по отпускам сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class RegistrationRecallVacation
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
    /// Мероприятие.
    /// </summary>
    public string EventSAP { get; set; }
    
    /// <summary>
    /// Дата начала текущего отпуска.
    /// </summary>
    public string CurrentDateStart { get; set; }
    
    /// <summary>
    /// Дата окончания текущего отпуска.
    /// </summary>
    public string CurrentDateEnd { get; set; }
    
    /// <summary>
    /// Дата начала периода прерывания/отзыва из отпуска.
    /// </summary>
    public string PresenceDateStart { get; set; }
    
    /// <summary>
    /// Дата окончания периода прерывания/отзыва из отпуска.
    /// </summary>
    public string PresenceDateEnd { get; set; }
    
    /// <summary>
    /// Тип отпуска.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Количество дней для компенсации.
    /// </summary>
    public int DayCountCompensation { get; set; }
    
    /// <summary>
    /// Дата переноса дней отпуска.
    /// </summary>
    public string DateTransferStart { get; set; }
    
    /// <summary>
    /// Дата окончания переноса дней отпуска.
    /// </summary>
    public string DateTransferEnd { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DayCount { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
  }
  
  #endregion
  
  #region Виджет "Мои отпуска".
  
  /// <summary>
  /// Данные по отпускам сотрудника для виджета "Мои отпуска".
  /// </summary>
  [Public(Isolated = true)]
  partial class EmployeeMyVacation
  {
    // ИД персоны.
    public long PersonId { get; set; }
    
    // Данные по отпускам сотрудника по годам.
    public List<DirRX.NNVacations.Structures.Module.IEmployeeVacationByYearData> EmployeeVacationByYearData { get; set; }
    
    // Текст ошибки.
    public string ErrorText { get; set; }
    
    // Время последнего обновления.
    public string UpdateTime { get; set; }
  }
  
  /// <summary>
  /// Данные по отпускам сотрудника по годам.
  /// </summary>
  [Public(Isolated = true)]
  partial class EmployeeVacationByYearData
  {
    // Год планируемых отпусков.
    public string Year { get; set; }
    
    // Данные по отпускам сотрудника.
    public List<DirRX.NNVacations.Structures.Module.IEmployeeVacationData> EmployeeVacationData { get; set; }
  }
  
  /// <summary>
  /// Данные по отпускам сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class EmployeeVacationData
  {
    // Должность сотрудника. Заполняется только при наличии совмещения.
    public string EmployeeJobTitile { get; set; }
    
    // Подраздление сотрудника. Заполняется только при наличии совмещения.
    public string EmployeeDepartment { get; set; }
    
    // Организация сотрудника. Заполняется только при наличии совмещения.
    public string EmployeeBusinessUnit { get; set; }
    
    // Признак оплаты.
    public bool Paid { get; set; }
    
    // Детализация отпуска по статусу, видам и датам.
    public List<DirRX.NNVacations.Structures.Module.IVacationStatusDetail> VacationStatusDetail { get; set; }
    
    // Текст при отсутствии данных.
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Детализация отпусков сотрудника по видам и датам.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationStatusDetail
  {
    // Статус отпуска.
    public string VacationStatus { get; set; }
    
    // Детализация по группам.
    public List<DirRX.NNVacations.Structures.Module.IVacationGroupDetail> VacationGroupDetail { get; set; }
    
    // Текст при отсутствии данных.
    public string NoDataText { get; set; }
  }
  
  /// <summary>
  /// Детализация отпусков сотрудника по группам.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationGroupDetail
  {
    // Группа лимитов.
    public string Group { get; set; }
    
    // Приоритет сортировки.
    public int Priority { get; set; }
    
    // Детализация по видам и датам.
    public List<DirRX.NNVacations.Structures.Module.IVacationDataDetail> VacationDetail { get; set; }
  }
  
  /// <summary>
  /// Детализация отпусков сотрудника по видам и датам.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationDataDetail
  {
    // Вид отпуска.
    public string VacationKind { get; set; }
    
    // Детализация по датам.
    public List<string> Dates { get; set; }
    
    // Кол-во дней.
    public string NumberOfDays { get; set; }
    
    // Приоритет сортировки.
    public int Priority { get; set; }
  }
  
  #endregion
  
  #region IF-S-10536. Формирование проводки на перенос отпуска из КЭДО в АСУП (КЭДО -> АСУП).
  
  /// <summary>
  /// Информация по переносу отпуска сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class RegistrationShiftVacation
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
    /// Информация по старым отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IOldVacationInfoIFS10536> OldVacations { get; set; }
    
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long TaskID { get; set; }
    
    /// <summary>
    /// Информация по новым отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IVacationInfoIFS10536> Vacations { get; set; }
  }
  
  /// <summary>
  /// Информация о старом отпуске.
  /// </summary>
  [Public(Isolated = true)]
  partial class OldVacationInfoIFS10536
  {
    /// <summary>
    /// Идентификатор старого отпуска.
    /// </summary>
    public string VacationCode { get; set; }
    
    /// <summary>
    /// Вид старого отпуска.
    /// </summary>
    public string TypeOldVacation { get; set; }
    
    /// <summary>
    /// Старый отпуск с.
    /// </summary>
    public string OldDateStart { get; set; }
    
    /// <summary>
    /// Старый отпуск по.
    /// </summary>
    public string OldDateEnd { get; set; }
  }
  
  /// <summary>
  /// Информация об отпуске.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfoIFS10536
  {
    /// <summary>
    /// Идентификатор старого отпуска.
    /// </summary>
    public string VacationCode { get; set; }
    
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
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DaysCount { get; set; }
  }
  
  #endregion
  
  #region IF-S-10392. Получение данных о текущем отпуске сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
  
  /// <summary>
  /// Структура для получения данных о текущем отпуске сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class CurrentVacationData
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
  
  /// <summary>
  /// Структура для получения данных о текущем отпуске сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class CurrentVacationDataAnswer
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
    /// Информация по текущим отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IVacationInfoIFS10392> VacationData { get; set; }
  }
  
  /// <summary>
  /// Информация об отпуске.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfoIFS10392
  {
    /// <summary>
    /// Тип отпуска.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Дата начала текущего отпуска.
    /// </summary>
    public string CurrentDateStart { get; set; }
    
    /// <summary>
    /// Дата окончания текущего отпуска.
    /// </summary>
    public string CurrentDateEnd { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DayCount { get; set; }
  }
  
  /// <summary>
  /// Структура для получения данных о текущем отпуске сотрудника.
  /// </summary>
  [Public]
  partial class CurrentVacationDataResult
  {
    /// <summary>
    /// Полученные данные от SAP о текущем отпуске сотрудника.
    /// </summary>
    public DirRX.NNVacations.Structures.Module.ICurrentVacationDataAnswer CurrentVacationDataAnswer { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Структура для получения данных о текущем отпуске сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class CurrentVacationDataForRecall
  {
    /// <summary>
    /// Период отпуска.
    /// </summary>
    public string VacationPeriod { get; set; }
    
    /// <summary>
    /// Дата начала текущего отпуска.
    /// </summary>
    public DateTime CurrentDateStart { get; set; }
    
    /// <summary>
    /// Дата окончания текущего отпуска.
    /// </summary>
    public DateTime CurrentDateEnd { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DayCount { get; set; }
    
    /// <summary>
    /// Информация по текущим отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IVacationInfoIFS10392> CurrentVacations { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  #endregion
  
  #region IF-S-10384. Получение данных о доступном количестве дней на компенсацию из АСУП в КЭДО (АСУП -> КЭДО).
  
  /// <summary>
  /// Структура для получения данных о доступном количестве дней на компенсацию.
  /// </summary>
  [Public(Isolated = true)]
  partial class CompensationAvailableDays
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
  
  /// <summary>
  /// Структура для получения данных о доступном количестве дней на компенсацию.
  /// </summary>
  [Public(Isolated = true)]
  partial class CompensationAvailableDaysResult
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
    /// Доступно дней.
    /// </summary>
    public int DayCount { get; set; }
    
    /// <summary>
    /// Доступно дней.
    /// </summary>
    public string DayCountString { get; set; }
    
    /// <summary>
    /// Дней на согласовании.
    /// </summary>
    public int DaysOnApproving { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  #endregion
  
  #region IF-S-10409. Проверка заявления на предоставление прочего отпуска (АСУП -> КЭДО).
  
  /// <summary>
  /// Информация для проверки данных по заявлению на прочий отпуск.
  /// </summary>
  [Public(Isolated = true)]
  partial class EmployeeOtherVacationsValidation
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
    /// Тип отпуска.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Начало.
    /// </summary>
    public string DateStart { get; set; }
    
    /// <summary>
    /// Окончание.
    /// </summary>
    public string DateEnd { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DaysCount { get; set; }
  }
  
  #endregion
  
  #region IF-S-10406. Проверка заявления на перенос отпуска (АСУП -> КЭДО).
  
  /// <summary>
  /// Информация для проверки данных по заявлению на перенос отпуска.
  /// </summary>
  [Public(Isolated = true)]
  partial class EmployeeShiftVacationsValidation
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
    /// Информация по старым отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IOldVacationInfoIFS10536> OldVacations { get; set; }
    
    /// <summary>
    /// Информация по новым отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IVacationInfoIFS10406> Vacations { get; set; }
  }
  
  /// <summary>
  /// Информация об отпуске.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationInfoIFS10406
  {
    /// <summary>
    /// Идентификатор старого отпуска.
    /// </summary>
    public string VacationCode { get; set; }
    
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
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DaysCount { get; set; }
  }
  
  #endregion
  
  #region IF-S-10394. Проверка данных в АСУП для оформления отзыва из отпуска (АСУП -> КЭДО).
  
  /// <summary>
  /// Структура для проверки заявления на компенсацию части ежегодного отпуска.
  /// </summary>
  [Public(Isolated = true)]
  partial class ValidateVacationRecall
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
    /// Дата начала отзыва.
    /// </summary>
    public string PresenceDateStart { get; set; }
    
    /// <summary>
    /// Дата окончания отзыва.
    /// </summary>
    public string PresenceDateEnd { get; set; }
    
    /// <summary>
    /// Дата переноса дней отпуска.
    /// </summary>
    public string DateTransferStart { get; set; }
    
    /// <summary>
    /// Дата окончания переноса дней отпуска.
    /// </summary>
    public string DateTransferEnd { get; set; }
    
    /// <summary>
    /// Количество дней компенсации.
    /// </summary>
    public int DayCountCompensation { get; set; }
  }
  
  #endregion
  
  #region Перенос отпуска по заявлению.
  
  /// <summary>
  /// Информация об отпуске для доработки переноса по заявлению.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationDataForReworkShiftByStatement
  {
    /// <summary>
    /// ИД исполнителя задания.
    /// </summary>
    public long PerformerId { get; set; }
    
    /// <summary>
    /// Информация об исполнителе.
    /// </summary>
    public string PerformerInfo { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime BeginDate { get; set; }
    
    /// <summary>
    /// Кол-во дней.
    /// </summary>
    public int Duration { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// ИД заявления на перенос отпуска.
    /// </summary>
    public long StatementId { get; set; }
    
    /// <summary>
    /// ИД задачи на перенос отпуска.
    /// </summary>
    public long TaskId { get; set; }
    
    /// <summary>
    /// Вид переноса.
    /// </summary>
    public string ShiftKind { get; set; }
    
    /// <summary>
    /// Период отпуска.
    /// </summary>
    public string ShiftPeriod { get; set; }
    
    /// <summary>
    /// Подробная информация о периоде отпуска.
    /// </summary>
    public string ShiftPeriodInfo { get; set; }
    
    /// <summary>
    /// Причина.
    /// </summary>
    public string Reason { get; set; }
    
    /// <summary>
    /// Коды типов отпусков.
    /// </summary>
    public List<string> Types { get; set; }
    
    /// <summary>
    /// Признак оплаты.
    /// </summary>
    public bool Paid { get; set; }
    
    /// <summary>
    /// Табличные данные старых отпусков.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IFormattedVacationForShiftByStatement> OldVacations { get; set; }
    
    /// <summary>
    /// Табличные данные новых отпусков.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IAdditionalVacationData> NewVacations { get; set; }
  }
  
  /// <summary>
  /// Данные о доступных отпусках для переноса по заявлению.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationDataForShiftByStatement
  {
    /// <summary>
    /// Порядковый номер периода отпуска.
    /// </summary>
    public int Number { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime DateStart { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime DateEnd { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DaysCount { get; set; }
    
    /// <summary>
    /// Период отпуска.
    /// </summary>
    public string VacationPeriod { get; set; }
    
    /// <summary>
    /// Подробная информация о периоде отпуска.
    /// </summary>
    public string VacationPeriodInfo { get; set; }
    
    /// <summary>
    /// Признак оплаты.
    /// </summary>
    public bool Paid { get; set; }
    
    /// <summary>
    /// Плановый отпуск.
    /// </summary>
    public bool IsPlanned { get; set; }
    
    /// <summary>
    /// Информация по отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IFormattedVacationForShiftByStatement> Vacations { get; set; }
  }
  
  /// <summary>
  /// Информация об одном отпуске сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class FormattedVacationForShiftByStatement
  {
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime DateStart { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime DateEnd { get; set; }
    
    /// <summary>
    /// ИД типа отпуска.
    /// </summary>
    public long VacationTypeId { get; set; }
    
    /// <summary>
    /// Тип отпуска.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DaysCount { get; set; }
    
    /// <summary>
    /// Признак оплаты.
    /// </summary>
    public bool Paid { get; set; }
  }
  
  #endregion
  
  /// <summary>
  /// Информация об остатках отпуска сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class FormattedVacationAvailableLimits
  {
    /// <summary>
    /// ИД сотрудника.
    /// </summary>
    public long employeeId { get; set; }
    
    /// <summary>
    /// Информация по отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IFormattedVacationData> Vacations { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Информация об одном отпуске сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class FormattedVacationData
  {
    /// <summary>
    /// Имя лимита.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Группа лимита.
    /// </summary>
    public string Group { get; set; }
    
    /// <summary>
    /// Приоритет группы.
    /// </summary>
    public int GroupPriority { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime DateStart { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime DateEnd { get; set; }
    
    /// <summary>
    /// Тип отпуска.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DaysCount { get; set; }
    
    /// <summary>
    /// Признак оплаты.
    /// </summary>
    public bool Paid { get; set; }
    
    /// <summary>
    /// Дата начала цепочки отпусков.
    /// </summary>
    public DateTime? ChainDateStart { get; set; }
  }
  
  /// <summary>
  /// Структура для проверки информации по отпускам сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class EmployeeVacationAvailableLimits
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
  
  /// <summary>
  /// Информация об остатках отпуска сотрудника.
  /// </summary>
  partial class VacationAvailableLimits
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Информация по отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IVacationData> Vacations { get; set; }
  }
  
  #region IF-S-10405. Проверка данных при оформлении заявления на отпуск (АСУП -> КЭДО)
  
  /// <summary>
  /// Структура для проверки данных по заявлению на отпуск.
  /// </summary>
  [Public(Isolated = true)]
  partial class EmployeeVacationsValidation
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
    /// Информация по отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IVacationData> Vacations { get; set; }
  }
  
  /// <summary>
  /// Структура c результатом проверки информации по отпускам сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class EmployeeVacationsValidationResult
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
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
    
    /// <summary>
    /// Дней авансом.
    /// </summary>
    public int? AdvanceDays { get; set; }
  }
  
  #endregion
  
  #region IF-S-11187. Проверка данных в АСУП для заявления на компенсацию части ежегодного отпуска (АСУП -> КЭДО)
  
  /// <summary>
  /// Структура для проверки заявления на компенсацию части ежегодного отпуска.
  /// </summary>
  [Public(Isolated = true)]
  partial class ValidateVacationCompensation
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
    /// Кол-во дней.
    /// </summary>
    public int Numbr { get; set; }
  }
  
  #endregion
  
  /// <summary>
  /// Структура для результата проверки информации по отпускам сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class ValidateEmployeeVacationsResult
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
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Информация об одном отпуске сотрудника.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationData
  {
    /// <summary>
    /// Дата начала.
    /// </summary>
    public string DateStart { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public string DateEnd { get; set; }
    
    /// <summary>
    /// Тип отпуска.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DaysCount { get; set; }
  }
  
  /// <summary>
  /// Информация об отпуске для доработки переноса.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationDataForReworkShift
  {
    /// <summary>
    /// ИД исполнителя задания.
    /// </summary>
    public long PerformerId { get; set; }
    
    /// <summary>
    /// ИД заявления на перенос отпуска.
    /// </summary>
    public long StatementId { get; set; }
    
    /// <summary>
    /// Причина.
    /// </summary>
    public string Reason { get; set; }
    
    /// <summary>
    /// ИД задачи на перенос отпуска.
    /// </summary>
    public long TaskId { get; set; }
    
    /// <summary>
    /// Данные об отпуске для переноса.
    /// </summary>
    public DirRX.NNVacations.Structures.Module.ICurrentVacationInfoForShift VacationInfo { get; set; }
    
    /// <summary>
    /// Табличные данные дополнительных отпусков.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IAdditionalVacationData> AdditionalVacationData { get; set; }
  }
  
  /// <summary>
  /// Данные для доработки заявления на прочий отпуск.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationDataForReworkOther
  {
    /// <summary>
    /// ИД исполнителя.
    /// </summary>
    public long InitiatorId { get; set; }
    
    /// <summary>
    /// ИД заявления.
    /// </summary>
    public long StatementId { get; set; }
    
    /// <summary>
    /// ИД типа отпуска.
    /// </summary>
    public long VacationTypeId { get; set; }
    
    /// <summary>
    /// ИД причины отпуска.
    /// </summary>
    public long VacationReasonId { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime BeginDate { get; set; }
    
    /// <summary>
    /// Кол-во дней.
    /// </summary>
    public int Duration { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Вид отпуска.
    /// </summary>
    public string VacationKind { get; set; }
    
    /// <summary>
    /// Аспирант.
    /// </summary>
    public bool IsAspirant { get; set; }
    
    /// <summary>
    /// Информация об исполнителе.
    /// </summary>
    public string PerformerInfo { get; set; }
  }
  
  /// <summary>
  /// Данные для доработки заявления на ежегодный отпуск.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationDataForReworkAnnual
  {
    /// <summary>
    /// ИД исполнителя.
    /// </summary>
    public long InitiatorId { get; set; }
    
    /// <summary>
    /// ИД заявления.
    /// </summary>
    public long StatementId { get; set; }
    
    /// <summary>
    /// ИД типа отпуска.
    /// </summary>
    public long VacationTypeId { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime BeginDate { get; set; }
    
    /// <summary>
    /// Кол-во дней.
    /// </summary>
    public int Duration { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Информация об исполнителе.
    /// </summary>
    public string PerformerInfo { get; set; }
    
    /// <summary>
    /// Табличные данные дополнительных отпусков.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IAdditionalVacationData> AdditionalVacationData { get; set; }
  }
  
  /// <summary>
  /// Данные для доработки заявления на ежегодный отпуск.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationDataForReworkRecall
  {
    /// <summary>
    /// ИД исполнителя.
    /// </summary>
    public long InitiatorId { get; set; }
    
    /// <summary>
    /// ИД заявления.
    /// </summary>
    public long StatementId { get; set; }
    
    /// <summary>
    /// ИД типа отпуска.
    /// </summary>
    public long VacationTypeId { get; set; }
    
    /// <summary>
    /// ИД отзываемого сотрудника.
    /// </summary>
    public long EmployeeId { get; set; }
    
    /// <summary>
    /// Информация об исполнителе.
    /// </summary>
    public string PerformerInfo { get; set; }
    
    /// <summary>
    /// Период отпуска.
    /// </summary>
    public string VacationPeriod { get; set; }
    
    /// <summary>
    /// Дата начала текущего отпуска.
    /// </summary>
    public DateTime BeginDateCurrent { get; set; }
    
    /// <summary>
    /// Кол-во дней текущего отпуска.
    /// </summary>
    public int DurationCurrent { get; set; }
    
    /// <summary>
    /// Дата окончания текущего отпуска.
    /// </summary>
    public DateTime EndDateCurrent { get; set; }
    
    /// <summary>
    /// Дата начала отзыва из отпуска.
    /// </summary>
    public DateTime BeginDateRecall { get; set; }
    
    /// <summary>
    /// Кол-во дней отзыва из отпуска.
    /// </summary>
    public int DurationRecall { get; set; }
    
    /// <summary>
    /// Дата окончания отзыва из отпуска.
    /// </summary>
    public DateTime EndDateRecall { get; set; }
    
    /// <summary>
    /// Дата начала переноса отпуска.
    /// </summary>
    public DateTime BeginDateShift { get; set; }
    
    /// <summary>
    /// Кол-во дней переноса отпуска.
    /// </summary>
    public int DurationShift { get; set; }
    
    /// <summary>
    /// Дата окончания переноса отпуска.
    /// </summary>
    public DateTime EndDateShift { get; set; }
    
    /// <summary>
    /// Вид компенсации.
    /// </summary>
    public string CompensationKind { get; set; }
    
    /// <summary>
    /// Причина прерывания.
    /// </summary>
    public string Reason { get; set; }
    
    /// <summary>
    /// Информация по текущим отпускам.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IVacationInfoIFS10392> CurrentVacations { get; set; }
  }
  
  /// <summary>
  /// Данные для доработки заявления на компенсацию.
  /// </summary>
  [Public(Isolated = true)]
  partial class StatementDataForReworkCompensation
  {
    /// <summary>
    /// ИД исполнителя.
    /// </summary>
    public long InitiatorId { get; set; }
    
    /// <summary>
    /// ИД заявления.
    /// </summary>
    public long StatementId { get; set; }
    
    /// <summary>
    /// ИД вида документа.
    /// </summary>
    public long DocumentKind { get; set; }
    
    /// <summary>
    /// Информация об исполнителе.
    /// </summary>
    public string PerformerInfo { get; set; }
    
    /// <summary>
    /// Выбрано дней.
    /// </summary>
    public int DaysSelected { get; set; }
  }
  
  /// <summary>
  /// Данные о дополнительном отпуске.
  /// </summary>
  [Public(Isolated = true)]
  partial class AdditionalVacationData
  {
    /// <summary>
    /// ИД типа отпуска.
    /// </summary>
    public long VacationTypeId { get; set; }
    
    /// <summary>
    /// ИД причины отпуска.
    /// </summary>
    public long VacationReasonId { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime BeginDate { get; set; }
    
    /// <summary>
    /// Кол-во дней.
    /// </summary>
    public int Duration { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime EndDate { get; set; }
  }
  
  /// <summary>
  /// Данные об отпуске.
  /// </summary>
  [Public]
  partial class VerificationVacationData
  {
    /// <summary>
    /// Тип отпуска.
    /// </summary>
    public DirRX.OverrideSolution.IVacationKind VacationType { get; set; }
    
    /// <summary>
    /// Причина отпуска.
    /// </summary>
    public DirRX.NNVacations.IVacationReasons VacationReason { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime BeginDate { get; set; }
    
    /// <summary>
    /// Кол-во дней.
    /// </summary>
    public int Duration { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime EndDate { get; set; }
  }
  
  /// <summary>
  /// Параметры для обновления в заявлении на отпуск.
  /// </summary>
  [Public(Isolated = true)]
  partial class ValidateVacationResult
  {
    /// <summary>
    /// Кол-во дней превышения.
    /// </summary>
    public int VacationDaysExcess { get; set; }
    
    /// <summary>
    /// Отсутствие отпуска в 14 дней.
    /// </summary>
    public string IsVacationPeriodBroken { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorText { get; set; }
  }
  
  /// <summary>
  /// Данные о документе из ЛК.
  /// </summary>
  [Public(Isolated = true)]
  partial class DocumentInfoFromLK
  {
    /// <summary>
    /// ИД документа.
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Имя документа.
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Имя целевой системы.
    /// </summary>
    public string System { get; set; }
    
    /// <summary>
    /// Тип документа.
    /// </summary>
    public string Type { get; set; }
  }
  
  /// <summary>
  /// Данные об отпуске для переноса.
  /// </summary>
  [Public(Isolated = true)]
  partial class CurrentVacationInfoForShift
  {
    /// <summary>
    /// ИД уведомления о начале отпуска.
    /// </summary>
    public long NoticeId { get; set; }
    
    /// <summary>
    /// Период отпуска.
    /// </summary>
    public string VacationPeriod { get; set; }
    
    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime BeginDate { get; set; }
    
    /// <summary>
    /// Кол-во дней.
    /// </summary>
    public int Duration { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Информация об исполнителе.
    /// </summary>
    public string PerformerInfo { get; set; }
    
    /// <summary>
    /// Табличные данные текущих отпусков.
    /// </summary>
    public List<DirRX.NNVacations.Structures.Module.IAdditionalVacationData> Vacations { get; set; }
  }
  
  /// <summary>
  /// Данные о сотруднике.
  /// </summary>
  [Public(Isolated = true)]
  partial class EmployeeInfo
  {
    /// <summary>
    /// ИД сотрудника.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// ФИО сотрудника.
    /// </summary>
    public string Name { get; set; }
  }
  
  /// <summary>
  /// Данные о типе отпуска.
  /// </summary>
  partial class VacationTypeInfo
  {
    /// <summary>
    /// ИД типа отпуска.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Наименование типа отпуска.
    /// </summary>
    public string Name { get; set; }
  }
  
  /// <summary>
  /// Данные о причине отпуска.
  /// </summary>
  partial class VacationReasonInfo
  {
    /// <summary>
    /// ИД причины отпуска.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Наименование причины отпуска.
    /// </summary>
    public string Name { get; set; }
  }
  
  /// <summary>
  /// Данные по шаблону заявлений на отпуск/перенос отпуска/вызов из отпуска.
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationStatementData
  {
    /// <summary>
    /// ИД шаблона заявления.
    /// </summary>
    public long TemplateId { get; set; }
    
    /// <summary>
    /// ИД вида документа.
    /// </summary>
    public long DocumentKindId { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string Error { get; set; }
  }
  
  /// <summary>
  /// Структура для вида документа.
  /// </summary>
  [Public(Isolated = true)]
  partial class DocumentKind
  {
    /// <summary>
    /// Ид вида документа.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Наименование вида документа.
    /// </summary>
    public string Name { get; set; }
  }
  
  
  /// <summary>
  /// Информацией о сроке согласования отпуска
  /// </summary>
  [Public(Isolated = true)]
  partial class VacationApprovalDateInfo
  {
    /// <summary>
    /// Дней до даты начала нового отпуска.
    /// </summary>
    public int NextVacationDays { get; set; }
    
    /// <summary>
    /// Дней до даты начала старого отпуска.
    /// </summary>
    public int OldVacationDays { get; set; }
  }
}