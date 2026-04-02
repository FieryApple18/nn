using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkStatementDocument;

namespace DirRX.NNOtherProcesses.Shared
{
  partial class WeekendWorkStatementDocumentFunctions
  {
    
    #region Выделение цветом данных по ошибкам в табличной части заявки ВД/ПД/СР

    /// <summary>
    /// Обновить выделение цветом данных по ошибкам в табличной части заявки ВД/ПД/СР.
    /// </summary>
    [Public]
    public virtual void RefreshHighlightingEmployeesDataErrors()
    {
      List<long> employeeErrorRecordsIds;
      object employeeErrorRecordsIdsObj;
      var entityParams = ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params;
      var result = entityParams.TryGetValue(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, out employeeErrorRecordsIdsObj);
      if (result && employeeErrorRecordsIdsObj.GetType() == typeof(List<long>))
      {
        employeeErrorRecordsIds = (List<long>)employeeErrorRecordsIdsObj;
        Functions.WeekendWorkStatementDocument.HighlightEmployeesDataErrors(_obj, employeeErrorRecordsIds);
        entityParams.Remove(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey);
      }
    }
    
    /// <summary>
    /// Убрать выделение цветом данных по ошибкам в табличной части заявки ВД/ПД/СР.
    /// </summary>
    [Public]
    public virtual void ClearHighlightingEmployeesDataErrors()
    {
      foreach (var record in _obj.Employees)
        HighlightEmployeeRecord(record, Colors.Empty);
    }
    
    /// <summary>
    /// Выделить цветом данные по ошибкам в табличной части заявки ВД/ПД/СР.
    /// </summary>
    /// <param name="employeeErrorRecordsIds">Список ИД записей ТЧ, в которых присутствуют ошибки.</param>
    [Public]
    public virtual void HighlightEmployeesDataErrors(List<long> employeeErrorRecordsIds)
    {
      var errorColor = Colors.FromUInt32(Convert.ToUInt32(Constants.WeekendWorkStatementDocument.ErrorHintColorHEX, 16));
      foreach (var record in _obj.Employees)
      {
        if (employeeErrorRecordsIds.Contains(record.Id))
          HighlightEmployeeRecord(record, errorColor);
        else
          HighlightEmployeeRecord(record, Colors.Empty);
      }
    }
    
    /// <summary>
    /// Выделить цветом данные по ошибкам в табличной части заявки ВД/ПД/СР.
    /// </summary>
    /// <param name="record">Строка в табличной части.</param>
    /// <param name="color">Цвет.</param>
    public static void HighlightEmployeeRecord(IWeekendWorkStatementDocumentEmployees record, Color color)
    {
      if (record.State.Properties.Employee.HighlightColor != color)
      {
        record.State.Properties.Employee.HighlightColor = color;
        record.State.Properties.PersonnelNumber.HighlightColor = color;
        record.State.Properties.Department.HighlightColor = color;
        record.State.Properties.JobTitle.HighlightColor = color;
        record.State.Properties.WorkDateFrom.HighlightColor = color;
        record.State.Properties.WorkDateTo.HighlightColor = color;
        record.State.Properties.FirstBreakFrom.HighlightColor = color;
        record.State.Properties.FirstBreakTo.HighlightColor = color;
        record.State.Properties.SecondBreakFrom.HighlightColor = color;
        record.State.Properties.SecondBreakTo.HighlightColor = color;
        record.State.Properties.Compensation.HighlightColor = color;
        record.State.Properties.RestDateFrom.HighlightColor = color;
        record.State.Properties.RestDateTo.HighlightColor = color;
        record.State.Properties.RejectedByApproval.HighlightColor = color;
        record.State.Properties.RejectedByOrder.HighlightColor = color;
        record.State.Properties.ChecksPassed.HighlightColor = color;
        record.State.Properties.ChecksResult.HighlightColor = color;
      }
    }
    
    #endregion
    
    #region Логика валидации табличной части Работники.
    
    /// <summary>
    /// Валидация количества рабочих часов.
    /// </summary>
    /// <returns>Записи ТЧ, которе не прошли валидацию.</returns>
    public virtual List<IWeekendWorkStatementDocumentEmployees> ValidateWeekendWorkHoursExceeded()
    {
      var errorRecords = new List<IWeekendWorkStatementDocumentEmployees>();
      var businessUnit = OverrideSolution.BusinessUnits.As(_obj.BusinessUnit);
      foreach (var employee in _obj.Employees)
      {
        var workDateFrom = employee.WorkDateFrom.HasValue ? employee.WorkDateFrom.Value : _obj.WorkDateFrom.Value;
        var workDateTo = employee.WorkDateTo.HasValue ? employee.WorkDateTo.Value : _obj.WorkDateTo.Value;
        var isWorkingHoursExceeded = Functions.WeekendWorkStatementDocument.Remote.IsWorkingHoursExceeded(businessUnit, workDateFrom, workDateTo,
                                                                                                          employee.FirstBreakFrom, employee.SecondBreakFrom);
        if (isWorkingHoursExceeded)
          errorRecords.Add(employee);
      }
      return errorRecords;
    }
    
    /// <summary>
    /// Валидация даты в ТЧ и в карточке для полей периода работы.
    /// </summary>
    /// <returns>Записи ТЧ, которе не прошли валидацию.</returns>
    public virtual List<IWeekendWorkStatementDocumentEmployees> ValidateWeekendWorkPeriodDates()
    {
      return _obj.Employees.Where(x =>
                                  (_obj.WorkDateTo.HasValue && x.WorkDateTo.HasValue && x.WorkDateTo.Value.Date != _obj.WorkDateTo.Value.Date) ||
                                  (_obj.WorkDateFrom.HasValue && x.WorkDateFrom.HasValue && x.WorkDateFrom.Value.Date != _obj.WorkDateFrom.Value.Date))
        .ToList();
    }
    
    /// <summary>
    /// Валидация времени в ТЧ и в карточке для полей периода работы.
    /// </summary>
    /// <returns>Записи ТЧ, которе не прошли валидацию.</returns>
    public virtual List<IWeekendWorkStatementDocumentEmployees> ValidateWeekendWorkPeriodTime()
    {
      return _obj.Employees.Where(x => x.WorkDateFrom.HasValue && x.WorkDateTo.HasValue && x.WorkDateFrom.Value.TimeOfDay >= x.WorkDateTo.Value.TimeOfDay).ToList();
    }
    
    /// <summary>
    /// Валидация дат доп. отдыха для компенсации типа предоставление другого дня отдыха.
    /// </summary>
    /// <returns>Записи ТЧ, которе не прошли валидацию.</returns>
    public virtual List<IWeekendWorkStatementDocumentEmployees> ValidateWeekendWorkRestDates()
    {
      return _obj.Employees.Where(x => x.Compensation.NeedRestDate == true && x.Compensation.IsRestDateRequired == true &&
                                  (!x.RestDateFrom.HasValue || !x.RestDateTo.HasValue)).ToList();
    }
    
    /// <summary>
    /// Валидация "Даты доп. отдыха с".
    /// </summary>
    /// <returns>Записи ТЧ, которе не прошли валидацию.</returns>
    public virtual List<IWeekendWorkStatementDocumentEmployees> ValidateWeekendWorkRestDateFrom()
    {
      return _obj.Employees.Where(x =>
                                  x.Compensation.NeedRestDate == true &&
                                  x.RestDateFrom.HasValue &&
                                  x.RestDateFrom.Value.Date <= _obj.WorkDateFrom.Value.Date).ToList();
    }
    
    /// <summary>
    /// Проверка на различие дат полей "Даты доп. отдыха с" и "Даты доп. отдыха по".
    /// </summary>
    /// <returns>Записи ТЧ, которе не прошли валидацию.</returns>
    public virtual List<IWeekendWorkStatementDocumentEmployees> ValidateWeekendWorkRestDateOneDay()
    {
      return _obj.Employees.Where(x =>
                                  x.Compensation.NeedRestDate == true &&
                                  x.RestDateTo.HasValue &&
                                  x.RestDateFrom.HasValue &&
                                  x.RestDateTo.Value.Date != x.RestDateFrom.Value.Date).ToList();
    }
    
    /// <summary>
    /// Проверка на то, что "Даты доп. отдыха по" меньше "Даты доп. отдыха" с по времени.
    /// </summary>
    /// <returns>Записи ТЧ, которе не прошли валидацию.</returns>
    public virtual List<IWeekendWorkStatementDocumentEmployees> ValidateWeekendWorkRestDateTo()
    {
      return _obj.Employees.Where(x =>
                                  x.Compensation.NeedRestDate == true &&
                                  x.RestDateTo.HasValue &&
                                  x.RestDateFrom.HasValue &&
                                  x.RestDateTo.Value.TimeOfDay < x.RestDateFrom.Value.TimeOfDay).ToList();
    }
    
    /// <summary>
    /// Валидация периода доп. отдыха для заявки на работу в выходной/праздничный день.
    /// </summary>
    /// <returns>Записи ТЧ, которе не прошли валидацию.</returns>
    public virtual List<IWeekendWorkStatementDocumentEmployees> ValidateWeekendWorkRestDateEqual()
    {
      return _obj.Employees.Where(x =>
                                  x.Compensation.NeedRestDate == true &&
                                  x.RestDateTo.HasValue &&
                                  x.RestDateFrom.HasValue &&
                                  x.RestDateTo != x.RestDateFrom).ToList();
    }
    
    /// <summary>
    /// Валидация перерывов.
    /// </summary>
    /// <returns>Результат валидации.</returns>
    public virtual DirRX.NNOtherProcesses.Structures.WeekendWorkStatementDocument.ValidateEmployeesTableResult ValidateWeekendWorkBreakPeriods()
    {
      var result = Structures.WeekendWorkStatementDocument.ValidateEmployeesTableResult.Create();
      var errorRecords = new List<IWeekendWorkStatementDocumentEmployees>();
      
      // Валидация перерывов
      errorRecords = _obj.Employees.Where(x =>
                                          (x.FirstBreakFrom.HasValue && !x.FirstBreakTo.HasValue) ||
                                          (x.FirstBreakTo.HasValue && !x.FirstBreakFrom.HasValue) ||
                                          (x.SecondBreakFrom.HasValue && !x.SecondBreakTo.HasValue) ||
                                          (x.SecondBreakTo.HasValue && !x.SecondBreakFrom.HasValue)).ToList();
      if (errorRecords.Any())
      {
        result.ErrorRecords = errorRecords;
        result.Error = WeekendWorkStatementDocuments.Resources.BreakIncompleteValidationError;
        return result;
      }
      
      // Валидация перерывов относительно рабочего времени.
      errorRecords = _obj.Employees.Where(x =>
                                          (x.WorkDateTo < x.FirstBreakFrom || x.FirstBreakFrom < x.WorkDateFrom) ||
                                          (x.WorkDateTo < x.FirstBreakTo || x.FirstBreakTo < x.WorkDateFrom) ||
                                          (x.WorkDateTo < x.SecondBreakFrom || x.SecondBreakFrom < x.WorkDateFrom) ||
                                          (x.WorkDateTo < x.SecondBreakTo || x.SecondBreakTo < x.WorkDateFrom)).ToList();
      if (errorRecords.Any())
      {
        result.ErrorRecords = errorRecords;
        result.Error = WeekendWorkStatementDocuments.Resources.FillEmployeesDialogDatesError;
        return result;
      }
      
      return result;
    }
    
    #endregion
    
    /// <summary>
    /// Проверить, что вид документа - сверхурочная работа.
    /// </summary>
    /// <returns>Результат. True - если вид документа - сверхурочная работа. False - если нет. Null - если не удалось извлечь параметр для вида документа.</returns>
    public bool? IsOvertimeWorkDocKind()
    {
      object paramValue;
      if (!((Sungero.Domain.Shared.IExtendedEntity)_obj).Params.TryGetValue(string.Format(Constants.WeekendWorkStatementDocument.DocKindParamName, _obj.Id), out paramValue))
      {
        Logger.ErrorFormat("IsOvertimeWorkDocKind. Ид документа: {0}. Не удалось извлечь параметр вида документа.", _obj.Id);
        return null;
      }
      
      return (bool)paramValue;
    }
    
    /// <summary>
    /// Добавить сотрудников в табличную часть.
    /// </summary>
    /// <param name="employeesInfo">Список сотрудников.</param>
    /// <param name="compensation">Компенсация.</param>
    /// <param name="workDateFrom">Дата работы с.</param>
    /// <param name="workDateTo">Дата работы по.</param>
    /// <param name="restDateFrom">Дата доп. отдыха с.</param>
    /// <param name="restDateTo">Дата доп. отдыха по.</param>
    /// <param name="firstBreakFrom">Первый перерыв с.</param>
    /// <param name="firstBreakTo">Первый перерыв по.</param>
    /// <param name="secondBreakFrom">Второй перерыв с.</param>
    /// <param name="secondBreakTo">Второй перерыв по.</param>
    public virtual void AddEmployes(List<Structures.WeekendWorkStatementDocument.EmployeeInfo> employeesInfo, IWeekendWorkCompensationKind compensation, DateTime? workDateFrom, DateTime? workDateTo,
                                    DateTime? restDateFrom, DateTime? restDateTo, DateTime? firstBreakFrom, DateTime? firstBreakTo,
                                    DateTime? secondBreakFrom, DateTime? secondBreakTo)
    {
      var employees = Functions.WeekendWorkStatementDocument.Remote.GetEmployees(employeesInfo);
      foreach (var employee in employees)
      {
        var record = _obj.Employees.AddNew();
        record.Employee = employee;
        record.Compensation = compensation;
        record.WorkDateFrom = workDateFrom;
        record.WorkDateTo = workDateTo;
        record.RestDateFrom = restDateFrom;
        record.RestDateTo = restDateTo;
        record.FirstBreakFrom = firstBreakFrom;
        record.FirstBreakTo = firstBreakTo;
        record.SecondBreakFrom = secondBreakFrom;
        record.SecondBreakTo = secondBreakTo;
      }
    }
    
    /// <summary>
    /// Сформировать строку с именами сотрудников.
    /// </summary>
    /// <param name="employeesInfo">Список сотрудников.</param>
    /// <returns>Строка.</returns>
    public static string FormEmployeesNamesString(List<Structures.WeekendWorkStatementDocument.EmployeeInfo> employeesInfo)
    {
      return string.Join(", ", employeesInfo.Select(x => x.Name).ToList());
    }
    
    /// <summary>
    /// Заполнить имя документа.
    /// </summary>
    [Public]
    public override void FillName()
    {
      var documentKind = _obj.DocumentKind;
      if (documentKind == null || !documentKind.GenerateDocumentName.Value)
        return;
      
      _obj.Name = this.GetGeneratedDocumentName();
    }
    
    /// <summary>
    /// Получить автоматически сформированное имя документа.
    /// </summary>
    /// <returns>Имя документа.</returns>
    [Public]
    public override string GetGeneratedDocumentName()
    {
      var documentKind = _obj.DocumentKind;
      var name = string.Empty;
      
      /* Имя в формате:
        <Вид документа> <ФИО> от <дата>.
       */
      using (TenantInfo.Culture.SwitchTo())
      {
        if (_obj.Employees.Count == 1 && _obj.Employees.FirstOrDefault().Employee != null)
          name += string.Format(" {0}", Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employees.FirstOrDefault().Employee, DeclensionCase.Genitive, false));
        
        if (_obj.RegistrationDate != null)
          name += Sungero.Docflow.OfficialDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("dd.MM.yyyy");
      }
      
      if (string.IsNullOrWhiteSpace(name))
      {
        if (_obj.VerificationState == null)
          name = GetAutogeneratedName();
        else
          name = _obj.DocumentKind.ShortName;
      }
      else if (documentKind != null)
      {
        name = documentKind.ShortName + name;
      }
      
      name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
      return Sungero.Docflow.PublicFunctions.OfficialDocument.AddClosingQuote(name, _obj);
    }
    
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      
      // Настройка доступности для редактирвоания полей заявки.
      Functions.WeekendWorkStatementDocument.SetEnabledProperties(_obj);
      
      // Настройка видимости и обязательности полей.
      _obj.State.Properties.RegistrationDate.IsVisible = true;
      _obj.State.Properties.InternalApprovalState.IsVisible = true;
      
      object paramValue;
      var entityParams = ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params;
      var paramName = string.Format(Constants.WeekendWorkStatementDocument.DocKindParamName, _obj.Id);
      if (entityParams.TryGetValue(paramName, out paramValue))
      {
        // Вид документа - Сверхурочная работа.
        var isOvertimeWork = (bool)paramValue;
        _obj.State.Properties.Reason.IsVisible = !isOvertimeWork;
        _obj.State.Properties.Reason.IsRequired = !isOvertimeWork;
        _obj.State.Properties.Employees.Properties.FirstBreakFrom.IsVisible = !isOvertimeWork;
        _obj.State.Properties.Employees.Properties.FirstBreakTo.IsVisible = !isOvertimeWork;
        _obj.State.Properties.Employees.Properties.SecondBreakFrom.IsVisible = !isOvertimeWork;
        _obj.State.Properties.Employees.Properties.SecondBreakTo.IsVisible = !isOvertimeWork;
      }
    }
    
    /// <summary>
    /// Установить доступность полей для пользователя.
    /// </summary>
    public virtual void SetEnabledProperties()
    {
      // Доступность полей группы "Жизненный цикл".
      _obj.State.Properties.InternalApprovalState.IsEnabled = false;
      _obj.State.Properties.LifeCycleState.IsEnabled = false;
      
      // Заявка - согласована.
      var isApproved = StatementIsApproved();
      
      // Общая логика доступности внесения изменений в заявку.
      _obj.State.IsEnabled = _obj.InternalApprovalState == null ||
        _obj.InternalApprovalState == NNOtherProcesses.WeekendWorkStatementDocument.InternalApprovalState.OnRework ||
        isApproved;
      
      #region Логика доступности отдельных полей заявки
      
      // Вид документа.
      _obj.State.Properties.DocumentKind.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // ТОП-10/100.
      _obj.State.Properties.TopDirRX.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Основание.
      _obj.State.Properties.ReasonDescription.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Уточнение причины.
      _obj.State.Properties.Reason.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Дата работы с.
      _obj.State.Properties.WorkDateFrom.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Дата работы по.
      _obj.State.Properties.WorkDateTo.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Доп. согласующий
      _obj.State.Properties.AdditionalApprover.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Наша орг.
      _obj.State.Properties.BusinessUnit.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Подразделение.
      _obj.State.Properties.Department.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Подготовил.
      _obj.State.Properties.PreparedBy.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Примечание.
      _obj.State.Properties.Note.IsEnabled = _obj.State.IsEnabled && !isApproved;
      
      
      // Поля ТЧ Работники.
      // ФИО.
      _obj.State.Properties.Employees.Properties.Employee.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Даты работы.
      _obj.State.Properties.Employees.Properties.WorkDateFrom.IsEnabled = _obj.State.IsEnabled && !isApproved;
      _obj.State.Properties.Employees.Properties.WorkDateTo.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Перерывы.
      _obj.State.Properties.Employees.Properties.FirstBreakFrom.IsEnabled = _obj.State.IsEnabled && !isApproved;
      _obj.State.Properties.Employees.Properties.FirstBreakTo.IsEnabled = _obj.State.IsEnabled && !isApproved;
      _obj.State.Properties.Employees.Properties.SecondBreakFrom.IsEnabled = _obj.State.IsEnabled && !isApproved;
      _obj.State.Properties.Employees.Properties.SecondBreakTo.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Компенсация.
      _obj.State.Properties.Employees.Properties.Compensation.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Даты доп. отдыха.
      _obj.State.Properties.Employees.Properties.RestDateFrom.IsEnabled = _obj.State.IsEnabled && !isApproved;
      _obj.State.Properties.Employees.Properties.RestDateTo.IsEnabled = _obj.State.IsEnabled && !isApproved;
      // Отклонено. Поле дооступно для изменения при условии, что заявка - cогласована.после.
      _obj.State.Properties.Employees.Properties.RejectedByApproval.IsEnabled = _obj.State.IsEnabled && isApproved;
      
      #endregion
      
      // Доступность действий над записями в ТЧ Работники.
      _obj.State.Properties.Employees.CanAdd = _obj.State.IsEnabled && !isApproved;
      _obj.State.Properties.Employees.CanDelete = _obj.State.IsEnabled && !isApproved;
      _obj.State.Properties.Employees.CanCopy = _obj.State.IsEnabled && !isApproved;
    }
    
    /// <summary>
    /// Проверить, что заявка находится на этапе - Согласована.
    /// </summary>
    /// <returns>Результат проверки.</returns>
    public virtual bool StatementIsApproved()
    {
      return _obj.InternalApprovalState == NNOtherProcesses.WeekendWorkStatementDocument.InternalApprovalState.Approved &&
        _obj.LifeCycleState == NNOtherProcesses.WeekendWorkStatementDocument.LifeCycleState.Active;
    }
  }
}