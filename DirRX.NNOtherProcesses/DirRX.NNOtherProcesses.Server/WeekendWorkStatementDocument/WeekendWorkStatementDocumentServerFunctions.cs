using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkStatementDocument;
using Sungero.Docflow;

namespace DirRX.NNOtherProcesses.Server
{
  partial class WeekendWorkStatementDocumentFunctions
  {
    /// <summary>
    /// Проверить наличие превышения количества рабочих часов.
    /// </summary>
    /// <param name="businessUnit">Наша орг.</param>
    /// <param name="workDateFrom">Дата работы с.</param>
    /// <param name="workDateTo">Дата работы по.</param>
    /// <param name="firstBreakFrom">Первый перерыв с.</param>
    /// <param name="secondBreakFrom">Второй перерыв с.</param>
    /// <returns>Результат проверки.</returns>
    [Public, Remote(IsPure = true)]
    public static bool IsWorkingHoursExceeded(OverrideSolution.IBusinessUnit businessUnit,
                                              DateTime workDateFrom, DateTime workDateTo,
                                              DateTime? firstBreakFrom, DateTime? secondBreakFrom)
    {
      var workingHoursWithoutBreak = businessUnit?.WorkingHoursWithoutBreakDirRX ?? 0.0;
      
      // Если не требуется контроль перерывов или не заполнено возможное количество часов без перерыва.
      if (businessUnit?.IsBreakMonitoringRequiredDirRX != true || workingHoursWithoutBreak == 0.0)
        return false;
      
      // Проверка превышения допустимого периода работы.
      if ((workDateTo - workDateFrom).TotalHours <= workingHoursWithoutBreak)
        return false;
      
      // Проверка наличия хотя-бы одного перерыва.
      if (firstBreakFrom.HasValue || secondBreakFrom.HasValue)
        return false;
      
      return true;
    }
    
    /// <summary>
    /// Expression-функция проверяет наличие сотрудников для проведения мероприятия в АСУП.
    /// </summary>
    /// <param name="document">Заявка на привлечение к работе в ВД/ПД/СР.</param>
    /// <returns>Результат проверки.</returns>
    [Public, ExpressionElement("ExpFunc_GetEmployeesWithoutRejected_Name", "ExpFunc_GetEmployeesWithoutRejected_Desc", "")]
    public static bool GetEmployeesWithoutRejected(IWeekendWorkStatementDocument document)
    {
      return document.Employees.Any(x => x.RejectedByApproval != true);
    }
    
    /// <summary>
    /// Получить сотрудников.
    /// </summary>
    /// <param name="employeesInfo">Информация о сотрудниках.</param>
    /// <returns>Список сотрудников.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<OverrideSolution.IEmployee> GetEmployees(List<Structures.WeekendWorkStatementDocument.EmployeeInfo> employeesInfo)
    {
      List<long> employeesIds = employeesInfo.Select(e => e.Id).ToList();
      return DirRX.OverrideSolution.Employees.GetAll().Where(x => employeesIds.Contains(x.Id));
    }
    
    /// <summary>
    /// Очистить результаты предыдущей проверки.
    /// </summary>
    public void ClearChecksResults()
    {
      foreach (var employeeInfo in _obj.Employees)
      {
        employeeInfo.ChecksPassed = false;
        employeeInfo.ChecksResult = string.Empty;
      }
    }
    
    /// <summary>
    /// Проверить заявление ВД/ПД/СР во внешней системе.
    /// </summary>
    /// <returns>Ошибка.</returns>
    [Public, Remote(IsPure = true)]
    public string ValidateStatementInExternalSystem()
    {
      ClearChecksResults();
      
      var response = Functions.Module.ValidateWeekendWorkStatementInExternalSystem(_obj);
      if (response == null || response.Employees.Count != _obj.Employees.Count)
        return DirRX.NNOtherProcesses.Resources.ExternalSystemValidationWeekendWorkError;
      
      // Поиск сотрудников в ТЧ Данные о работниках.
      foreach (var employeeResponse in response.Employees)
      {
        var employeeInfo = _obj.Employees.Where(x => x.Employee.PersonnelNumber == employeeResponse.Employee).FirstOrDefault();
        if (employeeInfo == null)
        {
          Logger.DebugFormat("ValidateStatementInExternalSystem. ИД заявления: {0}. Не найдена строка с информацией по сотруднику. Таб. номер сотрудника: {1}.", _obj.Id, employeeResponse.Employee);
          continue;
        }
        
        employeeInfo.ChecksPassed = employeeResponse.Result;
        employeeInfo.ChecksResult = employeeResponse.ErrorText;
      }
      
      // Для сотрудников не прошедших проверку, по которым не пришла ошибка из внешней системы, необходимо заполнить текст ошибки.
      var hasNoResultEmployees = _obj.Employees.Where(x => x.ChecksPassed != true && string.IsNullOrEmpty(x.ChecksResult));
      foreach (var employeeInfo in hasNoResultEmployees)
        employeeInfo.ChecksResult = DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WeekendWorkStatementValidationNoInfoError;
      
      try
      {
        _obj.Save();
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("ValidateStatementInExternalSystem. ИД заявления: {0}. Не удалось сохранить заявку. Текст ошибки: {1}. StackTrace: {2}.", _obj.Id, ex.Message, ex.StackTrace);
      }
      
      var hasErrors = _obj.Employees.Any(x => x.ChecksPassed != true);
      return hasErrors ? DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WeekendWorkStatementValidationError : string.Empty;
    }
    
    /// <summary>
    /// Проверить, является ли вид документа - "Заявка на привлечение к работе в сверхурочное время".
    /// </summary>
    /// <param name="docuemntKind">Вид документа.</param>
    /// <returns>Результат проверки.</returns>
    [Remote(IsPure = true)]
    public static bool IsOvertimeWork(Sungero.Docflow.IDocumentKind documentKind)
    {
      var link = Sungero.Docflow.PublicFunctions.Module.GetExternalLinks(documentKind).FirstOrDefault();
      return link?.ExternalEntityId == Constants.Module.DocumentKind.OvertimeWorkStatementDocument.ToString();
    }
    
    /// <summary>
    /// Получить сотрудников по критериям для добавления в заявку.
    /// </summary>
    /// <param name="query">Список сотрудников.</param>
    /// <returns>Список сотрудников.</returns>
    [Remote(IsPure = true)]
    public virtual IQueryable<OverrideSolution.IEmployee> GetEmployeesForFilling()
    {
      List<long> selectedEmployees = _obj.Employees.Select(e => e.Employee.Id).ToList();
      
      // Фильтрация по состоянию, уже выбранным сотрудникам в ТЧ, согласию КЭДО, НОР и признаку ТОП.
      return OverrideSolution.Employees.GetAll()
        .Where(e => e.Status == OverrideSolution.Employee.Status.Active && !selectedEmployees.Contains(e.Id) &&
               e.ConsentHRPro == OverrideSolution.Employee.ConsentHRPro.Signed &&
               OverrideSolution.BusinessUnits.Equals(e.BusinessUnit, _obj.PreparedBy.Department.BusinessUnit) &&
               (_obj.TopDirRX == true ? e.IsNGDDirRX.Value : !e.IsNGDDirRX.Value));
    }
    
    /// <summary>
    /// Перегенерировать тело заявки по шаблону вида документа.
    /// </summary>
    public void RegenerateWeekendWorkStatementBody()
    {
      var template = NNCommons.PublicFunctions.Module.Remote.GetTemplateByKind(_obj.DocumentKind, _obj.BusinessUnit);
      if (template == null)
      {
        Logger.ErrorFormat("RegenerateWeekendWorkStatementBody. ИД документа: {0}. Не найден шаблон для документа. ИД вида документа: {1}, ИД НОР: {2}.", _obj.Id, _obj.DocumentKind, _obj.BusinessUnit);
        return;
      }
      
      try
      {
        using (var body = template.LastVersion.Body.Read())
        {
          var newVersion = _obj.CreateVersionFrom(body, template.AssociatedApplication.Extension);
          var internalEntity = (Sungero.Domain.Shared.IExtendedEntity)_obj;
          internalEntity.Params[Sungero.Content.Shared.ElectronicDocumentUtils.FromTemplateIdKey] = template.Id;
          _obj.Save();
        }
        Logger.DebugFormat("RegenerateWeekendWorkStatementBody. ИД документа: {0}. Тело документа успешно создано из шаблона.", _obj.Id);
        InsertEmployeesData();
        Logger.DebugFormat("RegenerateWeekendWorkStatementBody. ИД документа: {0}. Табличная часть с данными по сотрудниками успешно вставлена в тело документа.", _obj.Id);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("RegenerateWeekendWorkStatementBody. ИД документа: {0}. Ошибка при создании документа из шаблона или при вставке табличной части. Текст ошибки: {1}, StackTrace: {2}.", _obj.Id, ex.Message, ex.StackTrace);
      }
    }

    /// <summary>
    /// Вставить в тело заявки табличную часть "Данные о сотрудниках".
    /// </summary>
    public void InsertEmployeesData()
    {
      var tableData = NNCommons.Structures.Module.TableData.Create();
      var logPrefix = string.Format("InsertEmployeesData. WeekendWorkStatementId: {0}.", _obj.Id);
      tableData.NumberKey = Constants.WeekendWorkStatementDocument.InsertEmployeesInfoTag;
      tableData.Values = new List<Dictionary<string, string>>();
      var isOvertimeWork = IsOvertimeWork(_obj.DocumentKind);
      var number = 0;
      foreach (var employeeInfo in _obj.Employees)
      {
        number++;
        if (isOvertimeWork)
          tableData.Values.Add(FormDataForOvertimeWorkStatement(employeeInfo, number));
        else
          tableData.Values.Add(FormDataForWeekendWorkStatement(employeeInfo, number));
      }
      
      using (var memory = new System.IO.MemoryStream())
      {
        try
        {
          using (Sungero.Domain.Shared.HistoryLocker.WithoutWriteHistory(_obj.LastVersion.ElectronicDocument))
          {
            _obj.LastVersion.Body.Read().CopyTo(memory);
          }
          var documentStream = NNCommons.IsolatedFunctions.WordInsertTable.WordInsertTableInTag(logPrefix, memory, tableData);
          using (var stream = new System.IO.MemoryStream())
          {
            documentStream.CopyTo(stream);
            _obj.LastVersion.Body.Write(stream);
            _obj.Save();
          }
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. Вставить табличную часть с данными по сотрудникам. Ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        }
      }
    }
    
    /// <summary>
    /// Сформировать данные по сотруднику для вставки в шаблон для вида документа "Заявка на привлечение к работе в выходные или/и праздничные дни".
    /// </summary>
    /// <param name="IWeekendWorkStatementDocumentEmployeesemployeeInfo">Информация по сотруднику из табличной части.</param>
    /// <returns>Словарь, данные по сотруднику для вставки в шаблон.</returns>
    private Dictionary<string, string> FormDataForWeekendWorkStatement(IWeekendWorkStatementDocumentEmployees employeeInfo, int number)
    {
      var tableLineDictionary = new Dictionary<string, string>();
      var fioWithPersonnelNumber = string.Format("{0},\n{1}", employeeInfo.PersonnelNumber, employeeInfo.Employee);
      
      // Колонки табличной части:
      
      // №.
      tableLineDictionary.Add("Number", number.ToString());
      // Табельный номер.
      tableLineDictionary.Add("PersonnelNumber", employeeInfo.PersonnelNumber);
      // ФИО работника.
      tableLineDictionary.Add("FIO", employeeInfo.Employee.ToString());
      // Должность (специальность, профессия), разряд, класс (категория) квалификация.
      tableLineDictionary.Add("JobTitle", employeeInfo.JobTitle.ToString());
      // Дата работы в выходные и (или) нерабочие праздничные дни.
      tableLineDictionary.Add("WorkDate", employeeInfo.WorkDateFrom.HasValue? employeeInfo.WorkDateFrom.Value.ToString("dd.MM.yyyy") : string.Empty);
      // Период работы с.
      tableLineDictionary.Add("WorkDateFrom", employeeInfo.WorkDateFrom.HasValue? employeeInfo.WorkDateFrom.Value.ToString("HH:mm") : string.Empty);
      // Период работы по.
      tableLineDictionary.Add("WorkDateTo", employeeInfo.WorkDateTo.HasValue? employeeInfo.WorkDateTo.Value.ToString("HH:mm") : string.Empty);
      // Первый перерыв с.
      tableLineDictionary.Add("FirstBreakFrom", employeeInfo.FirstBreakFrom.HasValue? employeeInfo.FirstBreakFrom.Value.ToString("HH:mm"): string.Empty);
      // Первый перерыв по.
      tableLineDictionary.Add("FirstBreakTo", employeeInfo.FirstBreakTo.HasValue? employeeInfo.FirstBreakTo.Value.ToString("HH:mm") : string.Empty);
      // Второй перерыв с.
      tableLineDictionary.Add("SecondBreakFrom", employeeInfo.SecondBreakFrom.HasValue? employeeInfo.SecondBreakFrom.Value.ToString("HH:mm") : string.Empty);
      // Второй перерыв по.
      tableLineDictionary.Add("SecondBreakTo", employeeInfo.SecondBreakTo.HasValue? employeeInfo.SecondBreakTo.Value.ToString("HH:mm") : string.Empty);
      // Вид компенсации.
      tableLineDictionary.Add("Compensation", employeeInfo.Compensation.DisplayValue);
      // Дополнительный день отдыха.
      tableLineDictionary.Add("RestDate", employeeInfo.RestDateFrom.HasValue? employeeInfo.RestDateFrom.Value.ToString("dd.MM.yyyy") : string.Empty);
      
      return tableLineDictionary;
    }
    
    /// <summary>
    /// Сформировать данные по сотруднику для вставки в шаблон для вида документа "Заявка на привлечение к работе в сверхурочное время".
    /// </summary>
    /// <param name="IWeekendWorkStatementDocumentEmployeesemployeeInfo">Информация по сотруднику из табличной части.</param>
    /// <returns>Словарь, данные по сотруднику для вставки в шаблон.</returns>
    private Dictionary<string, string> FormDataForOvertimeWorkStatement(IWeekendWorkStatementDocumentEmployees employeeInfo, int number)
    {
      var tableLineDictionary = new Dictionary<string, string>();
      
      // Колонки табличной части:
      
      // №.
      tableLineDictionary.Add("Number", number.ToString());
      // Табельный номер.
      tableLineDictionary.Add("PersonnelNumber", employeeInfo.PersonnelNumber);
      // ФИО работника.
      tableLineDictionary.Add("FIO", employeeInfo.Employee.ToString());
      // Должность (специальность, профессия), разряд, класс (категория) квалификация.
      tableLineDictionary.Add("JobTitle", employeeInfo.JobTitle.ToString());
      // Дата работы в сверхурочное время.
      tableLineDictionary.Add("WorkDate", employeeInfo.WorkDateFrom.HasValue? employeeInfo.WorkDateFrom.Value.ToString("dd.MM.yyyy") : string.Empty);
      // Период работы с.
      tableLineDictionary.Add("WorkDateFrom", employeeInfo.WorkDateFrom.HasValue? employeeInfo.WorkDateFrom.Value.ToString("HH:mm") : string.Empty);
      // Период работы по.
      tableLineDictionary.Add("WorkDateTo", employeeInfo.WorkDateTo.HasValue? employeeInfo.WorkDateTo.Value.ToString("HH:mm") : string.Empty);
      // Вид компенсации.
      tableLineDictionary.Add("Compensation", employeeInfo.Compensation.DisplayValue);
      
      return tableLineDictionary;
    }

    /// <summary>
    /// Проверить, является ли вид документа - "Заявка на привлечение к работе в сверхурочное время".
    /// </summary>
    /// <returns>Результат проверки.</returns>
    [Remote(IsPure = true)]
    public static bool CheckOvertimeWorkDocKindByExternalLink(Sungero.Docflow.IDocumentKind documentKind)
    {
      var link = Sungero.Docflow.PublicFunctions.Module.GetExternalLinks(documentKind).FirstOrDefault();
      return link?.ExternalEntityId == Constants.Module.DocumentKind.OvertimeWorkStatementDocument.ToString();
    }
    
    /// <summary>
    /// Отчистить даты перерывов сотрудников для сверхурочной работы.
    /// </summary>
    public void ClearBreakPeriodForOvertimeWork()
    {
      var entityParams = ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params;
      var paramName = string.Format(Constants.WeekendWorkStatementDocument.DocKindParamName, _obj.Id);
      object paramValue;
      if (entityParams.TryGetValue(paramName, out paramValue))
      {
        var isOvertimeWork = (bool)paramValue;
        if (isOvertimeWork)
        {
          foreach (var record in _obj.Employees)
          {
            record.FirstBreakFrom = null;
            record.FirstBreakTo = null;
            record.SecondBreakFrom = null;
            record.SecondBreakTo = null;
          }
        }
      }
    }
    
    /// <summary>
    /// Создать заявку на ВД/ПД/СР.
    /// </summary>
    /// <returns>Созданный и несохраненный документ - заявка на ВД/ПД/СР.</returns>
    [Remote]
    public static IWeekendWorkStatementDocument CreateWeekendWorkStatement()
    {
      return WeekendWorkStatementDocuments.Create();
    }
  }
}