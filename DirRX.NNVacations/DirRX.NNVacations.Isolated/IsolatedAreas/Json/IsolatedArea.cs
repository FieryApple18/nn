using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sungero.Core;
using DirRX.NNVacations.Structures.Module;

namespace DirRX.NNVacations.Isolated.Json
{
  
  #region IF-S-10403. Передача данных о доступных остатках отпуска сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
  
  /// <summary>
  /// Информация об остатках сотрудника.
  /// </summary>
  public class VacationAvailableLimits
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public string RequestCode { get; set; }
    
    /// <summary>
    /// Информация по отпускам.
    /// </summary>
    public List<VacationInfo> Vacations { get; set; }
  }
  
  /// <summary>
  /// Информация об одном отпуске сотрудника.
  /// </summary>
  public class VacationInfo
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
    public string DateStart { get; set; }
    
    /// <summary>
    /// Дата окончания.
    /// </summary>
    public string DateEnd { get; set; }
    
    /// <summary>
    /// Количество дней.
    /// </summary>
    public int DaysCount { get; set; }
    
    /// <summary>
    /// Тип отпуска.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Признак оплаты.
    /// </summary>
    public bool Paid { get; set; }
    
    /// <summary>
    /// Дата начала цепочки отпусков.
    /// </summary>
    public string ChainDateStart { get; set; }
  }
  
  #endregion
  
  #region IF-S-10392. Получение данных о текущем отпуске сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
  
  /// <summary>
  /// Конвертер для сериализации/десериализации структуры CurrentVacationDataAnswer.
  /// </summary>
  public class CurrentVacationDataAnswerConverter : JsonConverter<CurrentVacationDataAnswer>
  {
    public override CurrentVacationDataAnswer ReadJson(JsonReader reader, Type objectType, CurrentVacationDataAnswer existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      var response = new CurrentVacationDataAnswer();
      JObject jsonObject = JObject.Load(reader);
      response.RequestCode = jsonObject["RequestCode"]?.ToString() ?? string.Empty;
      response.Employee = jsonObject["Employee"]?.ToString() ?? string.Empty;
      var currentVacations = jsonObject["VacationData"]?.ToObject<List<VacationInfoIFS10392>>(serializer) ?? new List<VacationInfoIFS10392>();
      response.VacationData = currentVacations.Cast<IVacationInfoIFS10392>().ToList();
      
      return response;
    }
    
    public override void WriteJson(JsonWriter writer, CurrentVacationDataAnswer value, JsonSerializer serializer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("RequestCode");
      writer.WriteValue(value.RequestCode);
      writer.WritePropertyName("Employee");
      writer.WriteValue(value.Employee);
      writer.WritePropertyName("VacationData");
      serializer.Serialize(writer, value.VacationData);
      writer.WriteEndObject();
    }
  }
  
  #endregion
  
  #region IF-S-28542 Данные по оплатам отсутствий из АСУП в КЭДО (АСУП → КЭДО).
  
  /// <summary>
  /// Конвертер для сериализации/десериализации структуры EmployeeVacationsIFS28542.
  /// </summary>
  public class EmployeeVacationsIFS28542Converter : JsonConverter<EmployeeVacationsIFS28542>
  {
    public override EmployeeVacationsIFS28542 ReadJson(JsonReader reader, Type objectType, EmployeeVacationsIFS28542 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      var response = new EmployeeVacationsIFS28542();
      JObject jsonObject = JObject.Load(reader);
      response.RequestCode = jsonObject["RequestCode"]?.ToString() ?? string.Empty;
      response.Employee = jsonObject["Employee"]?.ToString() ?? string.Empty;
      var vacations = jsonObject["Vacations"]?.ToObject<List<VacationInfoIFS28542>>(serializer) ?? new List<VacationInfoIFS28542>();
      response.Vacations = vacations.Cast<IVacationInfoIFS28542>().ToList();
      
      return response;
    }
    
    public override void WriteJson(JsonWriter writer, EmployeeVacationsIFS28542 value, JsonSerializer serializer)
    {
      writer.WriteStartObject();
      writer.WritePropertyName("RequestCode");
      writer.WriteValue(value.RequestCode);
      writer.WritePropertyName("Employee");
      writer.WriteValue(value.Employee);
      writer.WritePropertyName("Vacations");
      serializer.Serialize(writer, value.Vacations);
      writer.WriteEndObject();
    }
  }
  
  #endregion
  
}