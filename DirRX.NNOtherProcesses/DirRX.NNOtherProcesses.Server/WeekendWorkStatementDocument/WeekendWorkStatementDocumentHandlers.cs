using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkStatementDocument;
using DirRX.OverrideSolution;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkStatementDocumentCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      base.CreatingFrom(e);
      e.Without(_info.Properties.Versions);
      e.Without(_info.Properties.HasVersions);
    }
  }
  
  partial class WeekendWorkStatementDocumentEmployeesCompensationPropertyFilteringServerHandler<T>
  {
    public virtual IQueryable<T> EmployeesCompensationFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var docKind = _root.DocumentKind;
      return query.Where(x => Equals(x.DocumentKind, docKind));
    }
  }

  partial class WeekendWorkStatementDocumentEmployeesEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> EmployeesEmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      List<long> selectedEmployees = _obj.WeekendWorkStatementDocument.Employees.Where(d => d.Employee != null).Select(d => d.Employee.Id).ToList();
      
      // Фильтрация по состоянию, уже выбранным сотрудникам в ТЧ, согласию КЭДО, НОР и признаку ТОП.
      return query
        .Where(d => d.Status == OverrideSolution.Employee.Status.Active && !selectedEmployees.Contains(d.Id) &&
               d.ConsentHRPro == OverrideSolution.Employee.ConsentHRPro.Signed &&
               OverrideSolution.BusinessUnits.Equals(d.BusinessUnit, _obj.WeekendWorkStatementDocument.PreparedBy.Department.BusinessUnit) &&
               (_obj.WeekendWorkStatementDocument.TopDirRX == true ? d.IsNGDDirRX.Value : !d.IsNGDDirRX.Value));
    }
  }

  partial class WeekendWorkStatementDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.CheckOvertimeWorkDocKindByExternalLink(_obj.DocumentKind);
      
      #region Общие проверки валидации данных
      
      // Валидация дат работы.
      if (_obj.WorkDateFrom.Value.Date != _obj.WorkDateTo.Value.Date)
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WorkDateOneDayErrorFormat(_obj.DocumentKind.Name));
      
      // Валидация времени периода работы.
      if (isOvertimeWork != true && _obj.WorkDateFrom.Value.TimeOfDay >= _obj.WorkDateTo.Value.TimeOfDay)
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WorkTimeValidationError);
      
      // Валидация группы НГД.
      if (_obj.Employees.Any(x => x.Employee.IsNGDDirRX.Value) && _obj.Employees.Any(x => !x.Employee.IsNGDDirRX.Value))
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesNGDGroupValidationError);
      
      // Валидация даты в ТЧ и в карточке для полей перода работы.
      var errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkPeriodDates(_obj);
      if (errorRecords.Any())
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WorkDateValidationError, _obj.Info.Actions.ValidateWeekendWorkPeriodDates);

      // Валидация дат доп. отдыха для компенсации типа предоставление другого дня отдыха.
      errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDates(_obj);
      if (errorRecords.Any())
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources
                   .CompensationValidationErrorFormat(_obj.Employees.FirstOrDefault(x => x.Compensation.NeedRestDate == true).Compensation.Name),
                   _obj.Info.Actions.ValidateWeekendWorkRestDates);

      // Валидация Даты доп. отдыха.
      errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDateFrom(_obj);
      if (errorRecords.Any())
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateError, _obj.Info.Actions.ValidateWeekendWorkRestDateFrom);

      // Валидация перерывов.
      var validateResult = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkBreakPeriods(_obj);
      if (!string.IsNullOrEmpty(validateResult.Error))
        e.AddError(validateResult.Error, _obj.Info.Actions.ValidateWeekendWorkBreakPeriods);
      
      #endregion
      
      #region Проверки валидации для работы в сверхурочное время
      if (isOvertimeWork == true)
      {
        // Проверка на различие дат RestDateTo и RestDateFrom.
        errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDateOneDay(_obj);
        if (errorRecords.Any())
          e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateOneDayError, _obj.Info.Actions.ValidateWeekendWorkRestDateOneDay);

        // Проверка на то, что RestDateTo меньше RestDateFrom по времени.
        errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDateTo(_obj);
        if (errorRecords.Any())
          e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateToError, _obj.Info.Actions.ValidateWeekendWorkRestDateTo);
      }
      #endregion

      #region Проверки валидации для работы ВД/ПД
      if (isOvertimeWork != true)
      {
        // Валидация периода доп. отдыха для заявки на работу в выходной/праздничный день.
        errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDateEqual(_obj);
        if (errorRecords.Any())
          e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateEqualError, _obj.Info.Actions.ValidateWeekendWorkRestDateEqual);
        
        // Валидация времени в ТЧ и в карточке для полей периода работы.
        errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkPeriodTime(_obj);
        if (errorRecords.Any())
          e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WorkTimeValidationError, _obj.Info.Actions.ValidateWeekendWorkPeriodTime);
        
        // Валидация количества рабочих часов.
        var businessUnit = BusinessUnits.As(_obj.BusinessUnit);
        errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkHoursExceeded(_obj);
        if (errorRecords.Any())
        {
          var workingHoursWithoutBreak = businessUnit.WorkingHoursWithoutBreakDirRX.Value;
          var errorWeekendWorkHoursExceeded = Resources.ErrorWeekendWorkHoursExceededFormat(errorRecords.Count > 1 ? Resources.ErrorWeekendWorkHoursExceededEmployees : Resources.ErrorWeekendWorkHoursExceededEmployee,
                                                                                            string.Join(", ", errorRecords.Select(x => x.Employee.Name)),
                                                                                            workingHoursWithoutBreak,
                                                                                            NNCommons.PublicFunctions.Module.GetHoursDeclension(workingHoursWithoutBreak));
          e.AddError(errorWeekendWorkHoursExceeded, _obj.Info.Actions.ValidateWeekendWorkHoursExceeded);
        }
      }
      #endregion
      
      // Заполнить поле Кому линейными руководителями сотрудников.
      var lineManagers = _obj.Employees.Where(x => x.Employee.LineManagerDirRX != null &&
                                              x.Employee.LineManagerDirRX.Status == OverrideSolution.Employee.Status.Active)
        .Select(x => x.Employee.LineManagerDirRX).Distinct();
      _obj.LineManagers = lineManagers.Any() ? string.Join(", ", lineManagers.Select(x => x.Person.ShortName)) : string.Empty;
      
      // Очистить даты перерывов сотрудников для сверхурочной работы.
      Functions.WeekendWorkStatementDocument.ClearBreakPeriodForOvertimeWork(_obj);
      
      // Убрать выделение цветом данных по ошибкам в табличной части заявки ВД/ПД/СР.
      Functions.WeekendWorkStatementDocument.ClearHighlightingEmployeesDataErrors(_obj);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      // Сделано для копирования, т.к. в base.Created происходит изменение свойства TopDirRX.
      var top = _obj.TopDirRX;
      
      base.Created(e);
      
      _obj.RegistrationDate = Calendar.UserNow;
      if (_obj.State.IsCopied)
      {
        _obj.TopDirRX = top;
        foreach (var employee in _obj.Employees)
        {
          employee.RestDateFrom = null;
          employee.RestDateTo = null;
          employee.FirstBreakFrom = null;
          employee.FirstBreakTo = null;
          employee.SecondBreakFrom = null;
          employee.SecondBreakTo = null;
          employee.ChecksPassed = null;
          employee.ChecksResult = null;
          employee.RejectedByApproval = null;
          employee.RejectedByOrder = null;
        }
      }
      else
      {
        _obj.IsGroup = false;
      }
    }
  }

}