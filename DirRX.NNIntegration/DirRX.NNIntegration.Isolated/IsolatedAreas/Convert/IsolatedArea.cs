using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sungero.Core;
using DirRX.NNIntegration.Structures.Module;

namespace DirRX.NNIntegration.Isolated.Convert
{
  #region IF-S-9336. Передача уведомления об отпуске из АСУП в КЭДО.
  
  /// <summary>
  /// Информация по отпускам сотрудников.
  /// </summary>
  public class VacationsInfo
  {
    /// <summary>
    /// Информация по отпускам сотрудников.
    /// </summary>
    public List<VacationData> EmployeesVacationsInfo { get; set; }
    
    /// <summary>
    /// Адрес эл. почты.
    /// </summary>
    public string Email { get; set; }
  }
  
  /// <summary>
  /// Информация по отпуску сотрудника.
  /// </summary>
  public class VacationData
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
  
  #endregion
  
  #region IF-S-6365. Создание карточки согласования графика отпусков из АСУП в КЭДО.
  
  /// <summary>
  /// Информация о графике отпусков.
  /// </summary>
  public class VacationScheduleDocumentInfoFromSAP
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
    public List<VacationInfoFromSAP> Employees { get; set; }
  }
  
  /// <summary>
  /// Информация об отпуске по работнику.
  /// </summary>
  public class VacationInfoFromSAP
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
    public int DayCount { get; set; }
  }
  
  #endregion
  
  #region IF-S-10412. Автоматическая проверка заявления на работу в выходной и/или праздничный день из КЭДО в АСУП.
  
  /// <summary>
  /// Конвертер для сериализации/десериализации структуры ValidateWeekendWorkStatementResponse.
  /// </summary>
  public class ValidateWeekendWorkStatementResponseConverter : JsonConverter<ValidateWeekendWorkStatementResponse>
  {
    /// <summary>
    /// Читает данные JSON и десериализует их в объект <see cref="ValidateWeekendWorkStatementResponse"/>.
    /// </summary>
    /// <param name="reader">Объект <see cref="JsonReader"/>, используемый для чтения JSON-данных.</param>
    /// <param name="objectType">Тип объекта, который необходимо создать из JSON. Обычно это <see cref="ValidateWeekendWorkStatementResponse"/>.</param>
    /// <param name="existingValue">Существующее значение объекта, которое может быть обновлено. Если это значение отсутствует, то будет создан новый объект.</param>
    /// <param name="hasExistingValue">Флаг, указывающий, имеет ли существующее значение объект. Если true, <paramref name="existingValue"/> будет использоваться.</param>
    /// <param name="serializer">Объект <see cref="JsonSerializer"/>, который используется для обработки десериализации.</param>
    /// <returns>Экземпляр <see cref="ValidateWeekendWorkStatementResponse"/>, представляющий прочитанные данные из JSON.</returns>
    /// <remarks>
    /// Метод загружает JSON-данные в объект <see cref="JObject"/>, извлекает поле "RequestCode"
    /// и заполняет список сотрудников, преобразовывая их в объекты типа <see cref="IValidateWeekendWorkStatementEmployeeInfoResponse"/>.
    /// </remarks>
    public override ValidateWeekendWorkStatementResponse ReadJson(JsonReader reader, Type objectType, ValidateWeekendWorkStatementResponse existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      var response = new ValidateWeekendWorkStatementResponse();

      JObject jsonObject = JObject.Load(reader);

      response.RequestCode = jsonObject["RequestCode"]?.ToString() ?? string.Empty;
      
      // Получение списка со свойствами IValidateWeekendWorkStatementEmployeeInfoResponse
      var employees = jsonObject["Employees"]?.ToObject<List<ValidateWeekendWorkStatementEmployeeInfoResponse>>(serializer) ?? new List<ValidateWeekendWorkStatementEmployeeInfoResponse>();

      // Преобразование в интерфейс публичной структуры IValidateWeekendWorkStatementEmployeeInfoResponse.
      response.Employees = employees.Cast<IValidateWeekendWorkStatementEmployeeInfoResponse>().ToList();

      return response;
    }
    
    /// <summary>
    /// Сериализует объект <see cref="ValidateWeekendWorkStatementResponse"/> в формат JSON.
    /// </summary>
    /// <param name="writer">Объект <see cref="JsonWriter"/>, используемый для записи JSON-данных.</param>
    /// <param name="value">Объект <see cref="ValidateWeekendWorkStatementResponse"/>, который требуется сериализовать.</param>
    /// <param name="serializer">Объект <see cref="JsonSerializer"/>, который используется для обработки сериализации.</param>
    public override void WriteJson(JsonWriter writer, ValidateWeekendWorkStatementResponse value, JsonSerializer serializer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("RequestCode");
      writer.WriteValue(value.RequestCode);
      writer.WritePropertyName("Employees");
      serializer.Serialize(writer, value.Employees);
      writer.WriteEndObject();
    }
  }
  
  #endregion
  
  #region IF-S-25360. Получение списка детей работника (АСУП → КЭДО). Конвертер для десириализации вложенной структуры.

  /// <summary>
  /// Конвертер для сериализации/десериализации структуры ValidateWeekendWorkStatementResponse.
  /// </summary>
  public class LrdPaymentChildrenResponseConverter : JsonConverter<LrdPaymentChildrenResponse>
  {
    /// <summary>
    /// Читает данные JSON и десериализует их в объект <see cref="ValidateWeekendWorkStatementResponse"/>.
    /// </summary>
    /// <param name="reader">Объект <see cref="JsonReader"/>, используемый для чтения JSON-данных.</param>
    /// <param name="objectType">Тип объекта, который необходимо создать из JSON. Обычно это <see cref="ValidateWeekendWorkStatementResponse"/>.</param>
    /// <param name="existingValue">Существующее значение объекта, которое может быть обновлено. Если это значение отсутствует, то будет создан новый объект.</param>
    /// <param name="hasExistingValue">Флаг, указывающий, имеет ли существующее значение объект. Если true, <paramref name="existingValue"/> будет использоваться.</param>
    /// <param name="serializer">Объект <see cref="JsonSerializer"/>, который используется для обработки десериализации.</param>
    /// <returns>Экземпляр <see cref="ValidateWeekendWorkStatementResponse"/>, представляющий прочитанные данные из JSON.</returns>
    /// <remarks>
    /// Метод загружает JSON-данные в объект <see cref="JObject"/>, извлекает поле "RequestCode"
    /// и заполняет список сотрудников, преобразовывая их в объекты типа <see cref="ILrdPaymentChildrenResponseConverter"/>.
    /// </remarks>
    public override LrdPaymentChildrenResponse ReadJson(JsonReader reader, Type objectType, LrdPaymentChildrenResponse existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      var response = new LrdPaymentChildrenResponse();

      JObject jsonObject = JObject.Load(reader);

      response.RequestCode = jsonObject["RequestCode"]?.ToString() ?? string.Empty;
      
      // Получение списка со свойствами ILrdPaymentChildInfoResponse
      var employees = jsonObject["Children"]?.ToObject<List<Child>>(serializer) ?? new List<Child>();
      
      // Преобразование в интерфейс публичной структуры ILrdPaymentChildInfoResponse.
      response.Children = employees.Cast<IChild>().ToList();

      return response;
    }
    
    /// <summary>
    /// Сериализует объект <see cref="ValidateWeekendWorkStatementResponse"/> в формат JSON.
    /// </summary>
    /// <param name="writer">Объект <see cref="JsonWriter"/>, используемый для записи JSON-данных.</param>
    /// <param name="value">Объект <see cref="ValidateWeekendWorkStatementResponse"/>, который требуется сериализовать.</param>
    /// <param name="serializer">Объект <see cref="JsonSerializer"/>, который используется для обработки сериализации.</param>
    public override void WriteJson(JsonWriter writer, LrdPaymentChildrenResponse value, JsonSerializer serializer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("RequestCode");
      writer.WriteValue(value.RequestCode);
      writer.WritePropertyName("Children");
      serializer.Serialize(writer, value.Children);
      writer.WriteEndObject();
    }
  }
  
  #endregion
  
  #region IF-S-25401. Передача результата нажатия на действие в центре уведомлений ПМД (ПМД → КЭДО). Конвертер для десириализации вложенной структуры.

  /// <summary>
  /// Конвертер для сериализации/десериализации структуры SupernikaNotificationStatus.
  /// </summary>
  public class SupernikaNotificationStatusConverter : JsonConverter<SupernikaNotificationStatus>
  {
    private readonly SupernikaNotificationConverter _notificationConverter = new SupernikaNotificationConverter();

    /// <summary>
    /// Читает данные JSON и десериализует их в объект <see cref="SupernikaNotificationStatus"/>.
    /// </summary>
    public override SupernikaNotificationStatus ReadJson(JsonReader reader, Type objectType, SupernikaNotificationStatus existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      var response = new SupernikaNotificationStatus();
      JObject jsonObject = JObject.Load(reader);
 
      // Получение списка уведомлений.
      var notificationsJson = jsonObject["notifications"]?.Children<JToken>();
      response.notifications = notificationsJson?.Select(n => _notificationConverter.ReadJson(n.CreateReader(), typeof(SupernikaNotification), null, false, serializer)).Cast<ISupernikaNotification>().ToList() ?? new List<ISupernikaNotification>();
      return response;
    }

    /// <summary>
    /// Сериализует экземпляр класса <see cref="SupernikaNotificationStatus"/> в JSON-формат.
    /// </summary>
    public override void WriteJson(JsonWriter writer, SupernikaNotificationStatus value, JsonSerializer serializer)
    {
      // Метод не используется, т.к. серилизиция происходит в функции SerializeInJson(ISupernikaNotificationStatus structure).
      // Метод требуется т.к. необходимо определить метод от абстрактного класса JsonConverter.
    }
  }
  
  /// <summary>
  /// Конвертер для структуры SupernikaNotification.
  /// </summary>
  public class SupernikaNotificationConverter : JsonConverter<SupernikaNotification>
  {

    /// <summary>
    /// Сериализует объект <see cref="SupernikaNotificationStatus"/> в JSON формат.
    /// </summary>
    /// <param name="writer">Объект <see cref="JsonWriter"/>, который используется для записи в JSON формат.</param>
    /// <param name="objectType">Тип объекта, который сериализуется.</param>
    /// <param name="value">Объект <see cref="SupernikaNotificationStatus"/>, который необходимо сериализовать.</param>
    /// <param name="serializer">Объект <see cref="JsonSerializer"/>, используемый для управления сериализацией.</param>
    public override SupernikaNotification ReadJson(JsonReader reader, Type objectType, SupernikaNotification existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      var notification = new SupernikaNotification();
      JObject jsonObject = JObject.Load(reader);
      
      // Считывание свойств уведомления.
      notification.unique_id = jsonObject["unique_id"]?.ToString();
      notification.status = jsonObject["status"]?.ToString();
      notification.message = jsonObject["message"]?.ToString();
      
      // Считывание кнопок.
      var buttons = jsonObject["buttons"]?.ToObject<List<JObject>>() ?? new List<JObject>();
      notification.buttons = buttons.Select(buttonJson => new SupernikaButton
                                            {
                                              button_type = buttonJson["button_type"]?.ToString(),
                                              comment = buttonJson["comment"]?.ToString(),
                                              button_status = buttonJson["button_status"]?.ToString()
                                            }).Cast<ISupernikaButton>().ToList();

      return notification;
    }

    /// <summary>
    /// Метод для сериализации объекта SupernikaNotificationStatus в формат JSON.
    /// </summary>
    /// <param name="writer">JsonWriter, используемый для записи JSON.</param>
    /// <param name="value">Объект SupernikaNotificationStatus, который необходимо сериализовать.</param>
    /// <param name="serializer">JsonSerializer, используемый для сериализации вложенных объектов.</param>
    public override void WriteJson(JsonWriter writer, SupernikaNotification value, JsonSerializer serializer)
    {
      // Метод не используется, т.к. серилизиция происходит в функции SerializeInJson(ISupernikaNotificationStatus structure).
      // Метод требуется т.к. необходимо определить метод от абстрактного класса JsonConverter.
    }
  }

  #endregion
  
}