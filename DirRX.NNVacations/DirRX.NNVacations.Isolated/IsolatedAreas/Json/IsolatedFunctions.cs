using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using DirRX.NNVacations.Structures.Module;

namespace DirRX.NNVacations.Isolated.Json
{
  public partial class IsolatedFunctions
  {
    #region Работа с DateTime
    
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
    
    #endregion
    
    #region IF-S-10406. Проверка заявления на перенос отпуска (АСУП -> КЭДО).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IEmployeeShiftVacationsValidation structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10409. Проверка заявления на предоставление прочего отпуска (АСУП -> КЭДО).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IEmployeeOtherVacationsValidation structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10405. Проверка данных при оформлении заявления на отпуск (АСУП -> КЭДО).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IEmployeeVacationsValidation structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10403. Передача данных о доступных остатках отпуска сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IEmployeeVacationAvailableLimits structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion

    #region IF-S-6373. Регистрация ежегодного отпуска (по графику отпусков) из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IRegistrationAnnualVacationBySchedule structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-6376. Регистрация прочего отпуска из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IRegistrationOtherVacation structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-6372. Регистрация ежегодного отпуска (по заявлению) из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IRegistrationAnnualVacationByStatement structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10536. Формирование проводки на перенос отпуска из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IRegistrationShiftVacation structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-25553. Передача статусов отправки уведомлений об отпуске из КЭДО в АСУП (КЭДО -> АСУП).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IVacationInfoIFS25553 structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-6369. Автоматическое создание мероприятия «Компенсация части ежегодного оплачиваемого отпуска» из КЭДО в АСУП.
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICompensationForPartOfAnnualPaidLeave structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-6361. Автоматическое создание мероприятия "Отзыв из отпуска" из КЭДО в АСУП.
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IRegistrationRecallVacation structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(List<IEmployeeInfo> structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IEmployeeMyVacation structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IVacationDataForReworkShift structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICurrentVacationInfoForShift structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IVacationDataForReworkOther structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IVacationDataForReworkAnnual structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IVacationDataForReworkRecall structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IStatementDataForReworkCompensation structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(List<IDocumentKind> structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IVacationStatementData structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICompensationAvailableDaysResult structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Получить ИД документа из json строки информации о документе в ЛК.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>ИД документа.</returns>
    [Public]
    public long GetDocumentIdFromLK(string document)
    {
      var documentsList = JsonConvert.DeserializeObject<List<DirRX.NNVacations.Structures.Module.DocumentInfoFromLK>>(document);
      var documentId = long.Parse(documentsList[0].Id);
      return documentId;
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IValidateVacationResult structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IVacationApprovalDateInfo structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #region IF-S-10403. Передача данных о доступных остатках отпуска сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IFormattedVacationAvailableLimits DeserializeInStruct(string json, IFormattedVacationAvailableLimits structure)
    {
      var vacationAvailableLimits = JsonConvert.DeserializeObject<DirRX.NNVacations.Isolated.Json.VacationAvailableLimits>(json);
      var result = DirRX.NNVacations.Structures.Module.FormattedVacationAvailableLimits.Create();
      var vacations = new List<DirRX.NNVacations.Structures.Module.IFormattedVacationData>();
      var culture = new System.Globalization.CultureInfo("ru-RU");
      var errors = new List<string>();
      foreach (var vacation in vacationAvailableLimits.Vacations)
      {
        var info = DirRX.NNVacations.Structures.Module.FormattedVacationData.Create();
        var dateStart = ParseStringToDate(vacation.DateStart);
        if (!dateStart.HasValue)
          errors.Add(vacation.DateStart);
        var dateEnd = ParseStringToDate(vacation.DateEnd);
        if (!dateEnd.HasValue)
          errors.Add(vacation.DateEnd);
        
        if (dateStart.HasValue && dateEnd.HasValue)
        {
          info.Name = vacation.Name;
          info.Group = vacation.Group;
          info.GroupPriority = vacation.GroupPriority;
          info.DateStart = dateStart.Value;
          info.DateEnd = dateEnd.Value;
          info.DaysCount = vacation.DaysCount;
          info.Type = vacation.Type;
          info.Paid = vacation.Paid;
          if (!string.IsNullOrEmpty(vacation.ChainDateStart))
            info.ChainDateStart = ParseStringToDate(vacation.ChainDateStart);
          vacations.Add(info);
        }
      }
      
      if (errors.Any())
      {
        var errorDates = string.Join(", ", errors);
        result.ErrorText = string.Format("Следующие даты не прошли валидацию полей: {0}", errorDates);
      }
      
      result.Vacations = vacations;
      return result;
    }
    
    #endregion
    
    #region IF-S-10384. Получение данных о доступном количестве дней на компенсацию из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static ICompensationAvailableDaysResult DeserializeInStruct(string json, ICompensationAvailableDaysResult structure)
    {
      return JsonConvert.DeserializeObject<DirRX.NNVacations.Structures.Module.CompensationAvailableDaysResult>(json);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICompensationAvailableDays structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10392. Получение данных о текущем отпуске сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static ICurrentVacationDataAnswer DeserializeInStruct(string json, ICurrentVacationDataAnswer structure)
    {
      var settings = new JsonSerializerSettings();
      settings.Converters.Add(new CurrentVacationDataAnswerConverter());
      return JsonConvert.DeserializeObject<CurrentVacationDataAnswer>(json, settings);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICurrentVacationDataForRecall structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(ICurrentVacationData structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static List<IVacationInfoIFS10392> DeserializeInStruct(string json, IVacationInfoIFS10392 structure)
    {
      var result = JsonConvert.DeserializeObject<List<DirRX.NNVacations.Structures.Module.VacationInfoIFS10392>>(json) ?? new List<VacationInfoIFS10392>();
      return result.Cast<IVacationInfoIFS10392>().ToList();
    }
    
    #endregion
    
    #region IF-S-10405. Проверка данных при оформлении заявления на отпуск (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <param name="structure">Структура.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IEmployeeVacationsValidationResult DeserializeInStruct(string json, IEmployeeVacationsValidationResult structure)
    {
      return JsonConvert.DeserializeObject<DirRX.NNVacations.Structures.Module.EmployeeVacationsValidationResult>(json);
    }
    
    #endregion
    
    #region IF-S-10406. Проверка заявления на перенос отпуска (АСУП -> КЭДО).
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static IValidateEmployeeVacationsResult DeserializeInStruct(string json, IValidateEmployeeVacationsResult structure)
    {
      return JsonConvert.DeserializeObject<DirRX.NNVacations.Structures.Module.ValidateEmployeeVacationsResult>(json);
    }
    
    #endregion
    
    #region IF-S-11187. Проверка данных в АСУП для заявления на компенсацию части ежегодного отпуска (АСУП -> КЭДО).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IValidateVacationCompensation structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region IF-S-10394. Проверка данных в АСУП для оформления отзыва из отпуска (АСУП -> КЭДО).
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IValidateVacationRecall structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
    
    #region Получение данных об отпуске сотрудника для переноса отпуска по заявлению.
    
    /// <summary>
    /// Десериализация строки json в структуру.
    /// </summary>
    /// <param name="json">Json строка.</param>
    /// <returns>Структура.</returns>
    [Public]
    public static List<IFormattedVacationForShiftByStatement> DeserializeInStruct(string json, IFormattedVacationForShiftByStatement structure)
    {
      var result = JsonConvert.DeserializeObject<List<DirRX.NNVacations.Structures.Module.FormattedVacationForShiftByStatement>>(json) ?? new List<FormattedVacationForShiftByStatement>();
      return result.Cast<IFormattedVacationForShiftByStatement>().ToList();
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(List<Structures.Module.IVacationDataForShiftByStatement> structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IVacationDataForReworkShiftByStatement structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    #endregion
  }
}