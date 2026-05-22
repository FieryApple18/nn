using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sungero.Core;
using DirRX.NNIntegration.Structures.Module;

namespace DirRX.NNIntegration.Isolated.Convert
{
  public partial class IsolatedFunctions
  {
    #region Работа с датой и временем
    
    /// <summary>
    /// Парсинг строки в дату с учетом культуры "ru-RU".
    /// </summary>
    /// <param name="date">Дата.</param>
    /// <returns>Если парсинг успешен DateTime, иначе null.</returns>
    public static DateTime? ParseStringToDate(string date)
    {
      try
      {
        return DateTime.Parse(date, new System.Globalization.CultureInfo("ru-RU"));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("ParseStringToDate. Ошибка при попытке парсинга строки '{0}' в дату. Текст ошибки: {1} StackTrace: {2}", date, ex.Message, ex.StackTrace);
        return null;
      }
    }
    
    /// <summary>
    /// Парсинг строки с временем в число с плавающей точкой.
    /// </summary>
    /// <param name="time">Время в формате MM:SS.</param>
    /// <returns>Если парсинг успешен double, иначе null.</returns>
    public static double? ParseTimeToDouble(string time)
    {
      if (string.IsNullOrEmpty(time))
        return null;
      
      try
      {
        var timeByParts = time.Split(':');
        var minutes = int.Parse(timeByParts[0]);
        var seconds = int.Parse(timeByParts[1]);
        return minutes + seconds / 60.0;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("ParseTimeToDouble. Ошибка при попытке парсинга строки с временем '{0}' в число с плавающей точкой. Текст ошибки: {1} StackTrace: {2}", time, ex.Message, ex.StackTrace);
        return null;
      }
    }
    
    #endregion
    
    #region IF-S-9312. Автоматическое создание карточки документа из АСУП в КЭДО.
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Объект структуры для доступности перегрузки.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IRetentionStatementDocumentCard DeserializeInStruct(string json, IRetentionStatementDocumentCard structure)
    {
      return JsonConvert.DeserializeObject<RetentionStatementDocumentCard>(json);
    }

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IRetentionStatementDocumentCard structure)
    {
      return JsonConvert.SerializeObject(structure);
    }

    #endregion
    
    #region IF-S-10408. Передача приказа на предоставление отпуска из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="model">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IAnnualVacationOrderCard DeserializeInStruct(string json, IAnnualVacationOrderCard model)
    {
      model = JsonConvert.DeserializeObject<AnnualVacationOrderCard>(json);
      return model;
    }

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IAnnualVacationOrderCard structure)
    {
      return JsonConvert.SerializeObject(structure);
    }

    #endregion
    
    #region IF-S-10411. Передача приказа на предоставление прочего отпуска из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="model">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IOtherVacationOrderCard DeserializeInStruct(string json, IOtherVacationOrderCard model)
    {
      model = JsonConvert.DeserializeObject<OtherVacationOrderCard>(json);
      return model;
    }

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IOtherVacationOrderCard structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10396. Передача приказа на компенсацию части ежегодного оплачиваемого отпуска из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="model">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IVacationCompensationOrderCard DeserializeInStruct(string json, IVacationCompensationOrderCard model)
    {
      model = JsonConvert.DeserializeObject<VacationCompensationOrderCard>(json);
      return model;
    }

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IVacationCompensationOrderCard structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10391. Передача приказа об отзыве из отпуска из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="model">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IRecallVacationOrderCard DeserializeInStruct(string json, IRecallVacationOrderCard model)
    {
      model = JsonConvert.DeserializeObject<RecallVacationOrderCard>(json);
      return model;
    }

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IRecallVacationOrderCard structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region Ответ из системы АСУП на асинхронный исходящий запрос.
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Структура.</param>
    /// <returns>Структура с данными.</returns>
    [Public]
    public static IAnswerOnOutgoingMessage DeserializeInStruct(string json, IAnswerOnOutgoingMessage structure)
    {
      return JsonConvert.DeserializeObject<AnswerOnOutgoingMessage>(json);
    }
    
    #endregion
    
    #region IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП.

    /// <summary>
    /// Десериализация строки json в структуру "CreateBusinessTripOrder" для согласования приказа на командировку.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Объект структуры для доступности перегрузки.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static ICreateBusinessTripOrder DeserializeInStruct(string json, ICreateBusinessTripOrder structure)
    {
      return JsonConvert.DeserializeObject<CreateBusinessTripOrder>(json);
    }
    
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICreateBusinessTripOrder structure)
    {
      return JsonConvert.SerializeObject(structure);
    }

    #endregion
    
    #region Создание ответа на запрос внешней системы.
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IResponseToExternalSystemRequest structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-9313. Автоматическое формирование задания на ручное проведения мероприятия в АСУП (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IManualHoldingEventStruct DeserializeInStruct(string json, IManualHoldingEventStruct structure)
    {
      return JsonConvert.DeserializeObject<ManualHoldingEventStruct>(json);
    }

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IManualHoldingEventStruct structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-6365. Создание карточки согласования графика отпусков из АСУП в КЭДО.
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IVacationScheduleDocumentInfoFromSAP DeserializeInStruct(string messageText, IVacationScheduleDocumentInfoFromSAP structure)
    {
      var vacScheduleDocInfoIsolated = JsonConvert.DeserializeObject<VacationScheduleDocumentInfoFromSAP>(messageText);
      var vacScheduleDocInfo = Structures.Module.VacationScheduleDocumentInfoFromSAP.Create();
      vacScheduleDocInfo.Body = vacScheduleDocInfoIsolated.Body;
      vacScheduleDocInfo.OrgCode = vacScheduleDocInfoIsolated.OrgCode;
      vacScheduleDocInfo.OrgType = vacScheduleDocInfoIsolated.OrgType;
      vacScheduleDocInfo.SAPID = vacScheduleDocInfoIsolated.SAPID;
      vacScheduleDocInfo.Year = vacScheduleDocInfoIsolated.Year;
      vacScheduleDocInfo.Employees = new List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTimeFromSAP>();
      foreach (var employeeInfo in vacScheduleDocInfoIsolated.Employees)
      {
        var employee = Structures.Module.VacationInfoDateTimeFromSAP.Create();
        employee.DateStart = ParseStringToDate(employeeInfo.DateStart);
        employee.DateEnd = ParseStringToDate(employeeInfo.DateEnd);
        employee.DayCount = employeeInfo.DayCount;
        employee.EmplPersNum = employeeInfo.Employee;
        employee.TypeVacation = employeeInfo.TypeVacation;
        vacScheduleDocInfo.Employees.Add(employee);
      }
      return vacScheduleDocInfo;
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IIncomingVacationScheduleDocumentInfoFromSAP structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-9336. Передача уведомления об отпуске из АСУП в КЭДО.
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IIncomingVacationsInfo structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IVacationsInfo DeserializeInStruct(string json, IVacationsInfo structure)
    {
      var vacationInfoIsolated = JsonConvert.DeserializeObject<DirRX.NNIntegration.Isolated.Convert.VacationsInfo>(json);
      var vacationsInfoList = DirRX.NNIntegration.Structures.Module.VacationsInfo.Create();
      var vacationInfoList = new List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTime>();
      var culture = new System.Globalization.CultureInfo("ru-RU");
      var errors = new List<string>();
      foreach (var employee in vacationInfoIsolated.EmployeesVacationsInfo)
      {
        var vacationInfo = DirRX.NNIntegration.Structures.Module.VacationInfoDateTime.Create();
        vacationInfo.PersonnelNumber = employee.Employee;
        vacationInfo.TypeVacation = employee.TypeVacation;
        var dateStart = ParseStringToDate(employee.DateStart);
        var dateEnd = ParseStringToDate(employee.DateEnd);
        if (!dateStart.HasValue || !dateEnd.HasValue)
          errors.Add(employee.Employee);
        else
        {
          vacationInfo.DateStart = dateStart.Value;
          vacationInfo.DateEnd = dateEnd.Value;
          vacationInfoList.Add(vacationInfo);
        }
      }
      vacationsInfoList.EmployeesVacationsInfo = vacationInfoList;
      vacationsInfoList.Email = vacationInfoIsolated.Email;
      if (errors.Any())
      {
        var personnelNumbers = string.Join("," + Environment.NewLine, errors);
        vacationsInfoList.ErrorText = string.Format("Сотрудники с данными табельными номерами не прошли валидацию дат отпусков: {0}", personnelNumbers);
      }
      
      return vacationsInfoList;
    }
    
    #endregion
    
    #region IF-S-7578. Получение результата проштамповки
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IResultStampingDocument DeserializeInStruct(string json, IResultStampingDocument structure)
    {
      return JsonConvert.DeserializeObject<ResultStampingDocument>(json);
    }

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IResultStampingDocument structure)
    {
      return JsonConvert.SerializeObject(structure);
    }

    #endregion
    
    #region IF-S-7601. Передача документа на проштамповку.

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IAttributes structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ISendDocumentToStampingModel structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static ISendDocumentToStampingModel DeserializeInStruct(string json, ISendDocumentToStampingModel structure)
    {
      return JsonConvert.DeserializeObject<SendDocumentToStampingModel>(json);
    }
    
    #endregion
    
    #region IF-S-9401. Передача информации о документе из КЭДО в АСУП (АСУП -> КЭДО).

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IDocToSAPFioriInfo structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10532. Передача статуса приказа (КЭДО -> АСУП).

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IStatusDocToSAPInfo structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-11622. Получение результата асинхронной отправки данных из КСИП в АСУП.
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IMessageSendingAsyncResult DeserializeInStruct(string json, IMessageSendingAsyncResult structure)
    {
      return JsonConvert.DeserializeObject<MessageSendingAsyncResult>(json);
    }

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IMessageSendingAsyncResult structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    #endregion

    #region IF-S-10412. Автоматическая проверка заявления на работу в выходной и/или праздничный день из КЭДО в АСУП.

    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Объект структуры для доступности перегрузки.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IValidateWeekendWorkStatementResponse DeserializeInStruct(string json, IValidateWeekendWorkStatementResponse structure)
    {
      var settings = new JsonSerializerSettings();
      settings.Converters.Add(new ValidateWeekendWorkStatementResponseConverter());
      
      // HACK: Seleznyov_DV. Реализован конвертер с логикой десириализации Json в структуру ответа для устранения следующей ошибки.
      // В изолированной области могут использоваться только публичные структуры, в которых вложенные структуры должны быть публичными и указываются в виде интерфейсов.
      // В структуре ответа от внешней системы, есть вложенная структура со списком данных о сотруднике. Возникает следующая ошибка при автоматической десириализации:
      // Could not create an instance of type DirRX.NNIntegration.Structures.Module.IValidateWeekendWorkStatementEmployeeInfoResponse.
      // Type is an interface or abstract class and cannot be instantiated.
      return JsonConvert.DeserializeObject<ValidateWeekendWorkStatementResponse>(json, settings);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IValidateWeekendWorkStatementRequest structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10413. Передача приказа на выход сотрудника в выходной и/или праздничный день из АСУП в КЭДО (АСУП -> КЭДО).

    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Объект структуры для доступности перегрузки.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static ICreateWeekendWorkOrderStructure DeserializeInStruct(string json, ICreateWeekendWorkOrderStructure structure)
    {
      structure = JsonConvert.DeserializeObject<CreateWeekendWorkOrderStructure>(json);
      return structure;
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICreateWeekendWorkOrderStructure structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-6363. Автоматическое создание мероприятия «Регистрация работы в выходной и/или праздничный день» из КЭДО в АСУП (КЭДО -> АСУП)
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICreateWeekendWorkEvent structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-6362. Автоматическое внесение изменений в действующий приказ из КЭДО в АСУП (КЭДО -> АСУП)
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICreateWeekendWorkCancelationEvent structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-6366. Автоматическое создание мероприятия «Регистрация выплат ЛНА» из КЭДО в АСУП (КЭДО → АСУП)
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICreateLrdPaymentEvent structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10400. Проверка данных по заявлению на выплату ЛНА (АСУП → КЭДО)
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ILrdPaymentCheckRequest structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static ILrdPaymentCheckResponse DeserializeInStruct(string json, ILrdPaymentCheckResponse structure)
    {
      return JsonConvert.DeserializeObject<LrdPaymentCheckResponse>(json);
    }
    
    #endregion
    
    #region IF-S-25360. Получение списка детей работника (АСУП → КЭДО).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ILrdPaymentChildrenRequest structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static ILrdPaymentChildrenResponse DeserializeInStruct(string json, ILrdPaymentChildrenResponse structure)
    {
      var settings = new JsonSerializerSettings();
      settings.Converters.Add(new LrdPaymentChildrenResponseConverter());
      
      // HACK: Seleznyov_DV. Реализован конвертер с логикой десириализации Json в структуру ответа для устранения следующей ошибки.
      // В изолированной области могут использоваться только публичные структуры, в которых вложенные структуры должны быть публичными и указываются в виде интерфейсов.
      // В структуре ответа от внешней системы, есть вложенная структура со списком данных о сотруднике. Возникает следующая ошибка при автоматической десириализации:
      // Could not create an instance of type DirRX.NNIntegration.Structures.Module.ILrdPaymentChildInfoResponse.
      // Type is an interface or abstract class and cannot be instantiated.
      return JsonConvert.DeserializeObject<LrdPaymentChildrenResponse>(json, settings);
    }
    
    #endregion
    
    #region IF-S-10399. Передача приказа на выплату по ЛНА из АСУП в КЭДО.
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ILrdPaymentOrderRequest structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Объект структуры для доступности перегрузки.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static ILrdPaymentOrderRequest DeserializeInStruct(string json, ILrdPaymentOrderRequest structure)
    {
      structure = JsonConvert.DeserializeObject<LrdPaymentOrderRequest>(json);
      return structure;
    }
    
    #endregion
    
    #region Выплаты по ЛНА. Взаимодействие с ЛК.
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(List<IPaymentTypeStructure> structures)
    {
      return JsonConvert.SerializeObject(structures);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IPaymentTypeSettingStructure structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ILrdPaymentAssignmentInfo structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-25401. Передача результата нажатия на действие в центре уведомлений ПМД (ПМД → КЭДО).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ISupernikaNotificationStatus structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="model">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static ISupernikaNotificationStatus DeserializeInStruct(string json, ISupernikaNotificationStatus model)
    {
      var settings = new JsonSerializerSettings();
      settings.Converters.Add(new SupernikaNotificationStatusConverter());
      
      // HACK: Seleznyov_DV. Реализован конвертер с логикой десириализации Json в структуру ответа для устранения следующей ошибки.
      // В изолированной области могут использоваться только публичные структуры, в которых вложенные структуры должны быть публичными и указываются в виде интерфейсов.
      // В структуре ответа от внешней системы, есть вложенная структура со списком данных о сотруднике. Возникает следующая ошибка при автоматической десириализации:
      // Could not create an instance of type DirRX.NNIntegration.Structures.Module.ISupernikaNotificationStatus.
      // Type is an interface or abstract class and cannot be instantiated.
      return JsonConvert.DeserializeObject<SupernikaNotificationStatus>(json, settings);
    }

    
    #endregion
    
    #region IF-S-28097. Синхронизация календарей рабочего времени из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка структуры IncomingWorkingTimeCalendar.</param>
    /// <returns>Структура IWorkingTimeCalendar.</returns>
    /// <remarks>Помимо десериализации происходит преобразование типов свойств, поэтому выходная структура отличается от входящей сериализованной.</remarks>
    [Public]
    public static IWorkingTimeCalendar DeserializeInStruct(string json, IWorkingTimeCalendar structure)
    {
      var isolatedCalendarStructure = JsonConvert.DeserializeObject<IncomingWorkingTimeCalendar>(json);
      var calendarStructure = NNIntegration.Structures.Module.WorkingTimeCalendar.Create();
      structure.Employee = isolatedCalendarStructure.Employee;
      structure.Year = isolatedCalendarStructure.Year;
      structure.DayDisc = new List<NNIntegration.Structures.Module.IWorkingDayDescription>();
      foreach (var isolatedDayStructure in isolatedCalendarStructure.DayDisc)
      {
        var dayStructure = NNIntegration.Structures.Module.WorkingDayDescription.Create();
        dayStructure.Date = ParseStringToDate(isolatedDayStructure.Date).Value;
        dayStructure.DayStart = ParseTimeToDouble(isolatedDayStructure.DayStart);
        dayStructure.DayEnd = ParseTimeToDouble(isolatedDayStructure.DayEnd);
        dayStructure.BrStart = ParseTimeToDouble(isolatedDayStructure.BrStart);
        dayStructure.BrEnd = ParseTimeToDouble(isolatedDayStructure.BrEnd);
        dayStructure.Type = isolatedDayStructure.Type;
        structure.DayDisc.Add(dayStructure);
      }
      return structure;
    }

    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IIncomingWorkingTimeCalendar structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
  }
}