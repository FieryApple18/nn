using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkStatementDocument;

namespace DirRX.NNOtherProcesses.Client
{
  partial class WeekendWorkStatementDocumentFunctions
  {

    /// <summary>
    /// Изменить данные исполнителей в табличной части.
    /// </summary>
    public void EditEmployee()
    {
      var employee = _obj.Employees.FirstOrDefault();
      
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWorkDocKind(_obj);
      if (!isOvertimeWork.HasValue)
        return;
      
      var businessUnit = OverrideSolution.BusinessUnits.As(_obj.BusinessUnit);
      
      var dialog = Dialogs.CreateInputDialog(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.EditEmployeeDialogTitle);
      
      #region Поля диалога.
      
      // Компенсация
      var compensation = dialog.AddSelect(_obj.Info.Properties.Employees.Properties.Compensation.LocalizedName, true, employee.Compensation)
        .Where(k => k.Status == NNOtherProcesses.WeekendWorkCompensationKind.Status.Active && Equals(k.DocumentKind, _obj.DocumentKind));
      
      // Даты работы
      var workDateFrom = dialog.AddDate(_obj.Info.Properties.WorkDateFrom.LocalizedName, true, employee.WorkDateFrom).AsDateTime();
      var workDateTo = dialog.AddDate(_obj.Info.Properties.WorkDateTo.LocalizedName, true, employee.WorkDateTo).AsDateTime();
      workDateFrom.IsEnabled = false;
      workDateTo.IsEnabled = false;
      
      // Даты перерывов.
      var firstBreakFrom = dialog.AddDate(_obj.Info.Properties.Employees.Properties.FirstBreakFrom.LocalizedName, false, employee.FirstBreakFrom).AsDateTime();
      var firstBreakTo = dialog.AddDate(_obj.Info.Properties.Employees.Properties.FirstBreakTo.LocalizedName, false, employee.FirstBreakTo).AsDateTime();
      var secondBreakFrom = dialog.AddDate(_obj.Info.Properties.Employees.Properties.SecondBreakFrom.LocalizedName, false, employee.SecondBreakFrom).AsDateTime();
      var secondBreakTo = dialog.AddDate(_obj.Info.Properties.Employees.Properties.SecondBreakTo.LocalizedName, false, employee.SecondBreakTo).AsDateTime();
      firstBreakFrom.IsVisible = !isOvertimeWork.Value;
      firstBreakTo.IsVisible = !isOvertimeWork.Value;
      secondBreakFrom.IsVisible = !isOvertimeWork.Value;
      secondBreakTo.IsVisible = !isOvertimeWork.Value;
      
      // Даты доп. отдыха
      var restDate = dialog.AddDate(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateTitle, false, (!isOvertimeWork.Value && employee.RestDateFrom.HasValue) ? employee.RestDateFrom.Value.Date : (DateTime?)null);
      var restDateFrom = dialog.AddDate(_obj.Info.Properties.Employees.Properties.RestDateFrom.LocalizedName, false, isOvertimeWork.Value ? employee.RestDateFrom : null).AsDateTime();
      var restDateTo = dialog.AddDate(_obj.Info.Properties.Employees.Properties.RestDateTo.LocalizedName, false, isOvertimeWork.Value ? employee.RestDateTo : null).AsDateTime();
      var isVisibleRestDate = employee.Compensation?.NeedRestDate == true;
      restDate.IsVisible = isVisibleRestDate && !isOvertimeWork.Value;
      restDateFrom.IsVisible = isVisibleRestDate && isOvertimeWork.Value;
      restDateTo.IsVisible = isVisibleRestDate && isOvertimeWork.Value;
      
      #endregion
      
      // Кнопки диалога
      var okButton = dialog.Buttons.AddOk();
      dialog.Buttons.AddCancel();
      
      #region Обработчики диалога.
      
      // Обработка клика на кнопку "ОК"
      dialog.SetOnButtonClick(h =>
                              {
                                if (h.Button == okButton)
                                {
                                  // Валидация Даты доп. отдыха.
                                  if (restDate.Value.HasValue && restDate.Value < _obj.WorkDateFrom)
                                    h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateError);
                                  
                                  // Валидация Даты доп. отдыха с.
                                  if (restDateFrom.Value.HasValue && restDateFrom.Value.Value.Date <= _obj.WorkDateFrom.Value.Date)
                                    h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateFromError);
                                  
                                  // Валидация Дат доп. отдыха.
                                  if (restDateFrom.Value.HasValue && restDateTo.Value.HasValue)
                                  {
                                    if (restDateFrom.Value.Value.Date != restDateTo.Value.Value.Date)
                                      h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateOneDayError);
                                    else if (restDateTo.Value < restDateFrom.Value)
                                      h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateToError);
                                  }
                                  
                                  // Валидация перерывов.
                                  if (isOvertimeWork != true &&
                                      ((firstBreakFrom.Value.HasValue && !firstBreakTo.Value.HasValue) ||
                                       (firstBreakTo.Value.HasValue && !firstBreakFrom.Value.HasValue) ||
                                       (secondBreakFrom.Value.HasValue && !secondBreakTo.Value.HasValue) ||
                                       (secondBreakTo.Value.HasValue && !secondBreakFrom.Value.HasValue)))
                                    h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.BreakIncompleteValidationError);
                                  
                                  // Валидация перерывов относительно рабочего времени.
                                  if (isOvertimeWork != true &&
                                      (!workDateFrom.Value.HasValue || !workDateTo.Value.HasValue || workDateFrom.Value > workDateTo.Value ||
                                       workDateFrom.Value.Value.Date != _obj.WorkDateFrom.Value.Date || workDateTo.Value.Value.Date != _obj.WorkDateTo.Value.Date ||
                                       (workDateTo.Value < firstBreakFrom.Value || firstBreakFrom.Value < workDateFrom.Value) ||
                                       (workDateTo.Value < firstBreakTo.Value || firstBreakTo.Value < workDateFrom.Value) ||
                                       (workDateTo.Value < secondBreakFrom.Value || secondBreakFrom.Value < workDateFrom.Value) ||
                                       (workDateTo.Value < secondBreakTo.Value || secondBreakTo.Value < workDateFrom.Value)))
                                    h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesDialogDatesError);
                                  
                                  // Валидация количества рабочих часов.
                                  if (isOvertimeWork != true)
                                  {
                                    var isWorkingHoursExceeded = Functions.WeekendWorkStatementDocument.Remote.IsWorkingHoursExceeded(businessUnit, workDateFrom.Value.Value, workDateTo.Value.Value,
                                                                                                                                      firstBreakFrom.Value, secondBreakFrom.Value);
                                    if (isWorkingHoursExceeded)
                                    {
                                      var workingHoursWithoutBreak = businessUnit.WorkingHoursWithoutBreakDirRX.Value;
                                      h.AddError(Resources.ErrorCheckWorkingHoursExceededFormat(workingHoursWithoutBreak, NNCommons.PublicFunctions.Module.GetHoursDeclension(workingHoursWithoutBreak)));
                                    }
                                  }
                                }
                              });
      
      // Обработчик изменения значения поля Компенсация.
      compensation.SetOnValueChanged(args => {
                                       var isVisible = compensation.Value?.NeedRestDate == true;
                                       restDate.IsVisible = isVisible && !isOvertimeWork.Value;
                                       restDateFrom.IsVisible = isVisible && isOvertimeWork.Value;
                                       restDateTo.IsVisible = isVisible && isOvertimeWork.Value;
                                       if (!isVisible)
                                       {
                                         restDate.Value = null;
                                         restDateFrom.Value = null;
                                         restDateTo.Value = null;
                                       }
                                     });
      
      #endregion
      
      if (dialog.Show() == okButton)
      {
        foreach (var record in _obj.Employees)
        {
          record.Compensation = compensation.Value;
          record.WorkDateFrom = workDateFrom.Value;
          record.WorkDateTo = workDateTo.Value;
          record.RestDateFrom = restDateFrom.Value ?? restDate.Value;
          record.RestDateTo = restDateTo.Value ?? restDate.Value;
          record.FirstBreakFrom = firstBreakFrom.Value;
          record.FirstBreakTo = firstBreakTo.Value;
          record.SecondBreakFrom = secondBreakFrom.Value;
          record.SecondBreakTo = secondBreakTo.Value;
        }
      }
    }

    /// <summary>
    /// Добавить исполнителей в табличную часть.
    /// </summary>
    public void FillEmployees()
    {
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWorkDocKind(_obj);
      if (!isOvertimeWork.HasValue)
        return;
      
      var businessUnit = OverrideSolution.BusinessUnits.As(_obj.BusinessUnit);
      
      var dialog = Dialogs.CreateInputDialog(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesDialogTitle);
      
      #region Поля диалога.
      
      // Компенсация
      var compensation = dialog.AddSelect(_obj.Info.Properties.Employees.Properties.Compensation.LocalizedName, true, WeekendWorkCompensationKinds.Null)
        .Where(k => k.Status == NNOtherProcesses.WeekendWorkCompensationKind.Status.Active && Equals(k.DocumentKind, _obj.DocumentKind));
      
      // Сотрудники
      var employeesInfo = new List<Structures.WeekendWorkStatementDocument.EmployeeInfo>();
      var employeesText = dialog
        .AddMultilineString(_obj.Info.Properties.Employees.LocalizedName, true, string.Empty)
        .WithRowsCount(PublicConstants.WeekendWorkStatementDocument.EmployeesTextRowsCount);
      employeesText.IsEnabled = false;
      
      // Гиперссылки
      var addEmployees = dialog.AddHyperlink(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesAddHyperlinkName);
      var deleteEmployees = dialog.AddHyperlink(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesDeleteHyperlinkName);
      
      // Даты работы
      var workDateFrom = dialog.AddDate(_obj.Info.Properties.WorkDateFrom.LocalizedName, true, _obj.WorkDateFrom).AsDateTime();
      var workDateTo = dialog.AddDate(_obj.Info.Properties.WorkDateTo.LocalizedName, true, _obj.WorkDateTo).AsDateTime();
      workDateFrom.IsEnabled = false;
      workDateTo.IsEnabled = false;
      
      // Даты перерывов.
      var firstBreakFrom = dialog.AddDate(_obj.Info.Properties.Employees.Properties.FirstBreakFrom.LocalizedName, false, null).AsDateTime();
      var firstBreakTo = dialog.AddDate(_obj.Info.Properties.Employees.Properties.FirstBreakTo.LocalizedName, false, null).AsDateTime();
      var secondBreakFrom = dialog.AddDate(_obj.Info.Properties.Employees.Properties.SecondBreakFrom.LocalizedName, false, null).AsDateTime();
      var secondBreakTo = dialog.AddDate(_obj.Info.Properties.Employees.Properties.SecondBreakTo.LocalizedName, false, null).AsDateTime();
      firstBreakFrom.IsVisible = !isOvertimeWork.Value;
      firstBreakTo.IsVisible = !isOvertimeWork.Value;
      secondBreakFrom.IsVisible = !isOvertimeWork.Value;
      secondBreakTo.IsVisible = !isOvertimeWork.Value;
      
      // Даты доп. отдыха
      var restDate = dialog.AddDate(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateTitle, false, null);
      var restDateFrom = dialog.AddDate(_obj.Info.Properties.Employees.Properties.RestDateFrom.LocalizedName, false, null).AsDateTime();
      var restDateTo = dialog.AddDate(_obj.Info.Properties.Employees.Properties.RestDateTo.LocalizedName, false, null).AsDateTime();
      restDate.IsVisible = false;
      restDateFrom.IsVisible = false;
      restDateTo.IsVisible = false;
      
      #endregion
      
      // Кнопки диалога
      var addButton = dialog.Buttons.AddCustom(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesAddButtonName);
      dialog.Buttons.AddCancel();
      
      #region Обработчики диалога.
      
      // Обработка клика на кнопку "Добавить"
      dialog.SetOnButtonClick(h =>
                              {
                                if (h.Button == addButton)
                                {
                                  // Валидация количества записей сотрудников.
                                  if (_obj.Employees.Count + employeesInfo.Count() > PublicConstants.WeekendWorkStatementDocument.MaxEmployeesCount)
                                    h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesEmployeesAmounError);
                                  
                                  // Валидация Даты доп. отдыха.
                                  if (restDate.Value.HasValue && restDate.Value < _obj.WorkDateFrom)
                                    h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateError);
                                  
                                  // Валидация Даты доп. отдыха с.
                                  if (restDateFrom.Value.HasValue && restDateFrom.Value.Value.Date <= _obj.WorkDateFrom.Value.Date)
                                    h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateFromError);
                                  
                                  // Валидация Дат доп. отдыха.
                                  if (restDateFrom.Value.HasValue && restDateTo.Value.HasValue)
                                  {
                                    if (restDateFrom.Value.Value.Date != restDateTo.Value.Value.Date)
                                      h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateOneDayError);
                                    else if (restDateTo.Value < restDateFrom.Value)
                                      h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesRestDateToError);
                                  }
                                  
                                  // Валидация перерывов.
                                  if (isOvertimeWork != true &&
                                      ((firstBreakFrom.Value.HasValue && !firstBreakTo.Value.HasValue) ||
                                       (firstBreakTo.Value.HasValue && !firstBreakFrom.Value.HasValue) ||
                                       (secondBreakFrom.Value.HasValue && !secondBreakTo.Value.HasValue) ||
                                       (secondBreakTo.Value.HasValue && !secondBreakFrom.Value.HasValue)))
                                    h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.BreakIncompleteValidationError);
                                  
                                  // Валидация перерывов относительно рабочего времени.
                                  if (isOvertimeWork != true &&
                                      (!workDateFrom.Value.HasValue || !workDateTo.Value.HasValue || workDateFrom.Value > workDateTo.Value ||
                                       workDateFrom.Value.Value.Date != _obj.WorkDateFrom.Value.Date || workDateTo.Value.Value.Date != _obj.WorkDateTo.Value.Date ||
                                       (workDateTo.Value < firstBreakFrom.Value || firstBreakFrom.Value < workDateFrom.Value) ||
                                       (workDateTo.Value < firstBreakTo.Value || firstBreakTo.Value < workDateFrom.Value) ||
                                       (workDateTo.Value < secondBreakFrom.Value || secondBreakFrom.Value < workDateFrom.Value) ||
                                       (workDateTo.Value < secondBreakTo.Value || secondBreakTo.Value < workDateFrom.Value)))
                                    h.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesDialogDatesError);
                                  
                                  // Валидация количества рабочих часов.
                                  if (isOvertimeWork != true)
                                  {
                                    var isWorkingHoursExceeded = Functions.WeekendWorkStatementDocument.Remote.IsWorkingHoursExceeded(businessUnit, workDateFrom.Value.Value, workDateTo.Value.Value,
                                                                                                                                      firstBreakFrom.Value, secondBreakFrom.Value);
                                    if (isWorkingHoursExceeded)
                                    {
                                      var workingHoursWithoutBreak = businessUnit.WorkingHoursWithoutBreakDirRX.Value;
                                      h.AddError(Resources.ErrorCheckWorkingHoursExceededFormat(workingHoursWithoutBreak, NNCommons.PublicFunctions.Module.GetHoursDeclension(workingHoursWithoutBreak)));
                                    }
                                  }
                                }
                              });
      
      // Обработчик изменения значения поля Компенсация.
      compensation.SetOnValueChanged(args => {
                                       var isVisible = compensation.Value?.NeedRestDate == true;
                                       restDate.IsVisible = isVisible && !isOvertimeWork.Value;
                                       restDateFrom.IsVisible = isVisible && isOvertimeWork.Value;
                                       restDateTo.IsVisible = isVisible && isOvertimeWork.Value;
                                       if (!isVisible)
                                       {
                                         restDate.Value = null;
                                         restDateFrom.Value = null;
                                         restDateTo.Value = null;
                                       }
                                     });
      
      // Обработчик добавление сотрудников при нажатии на гиперссылку.
      addEmployees.SetOnExecute(() =>
                                {
                                  List<long> employeesIds = employeesInfo.Select(e => e.Id).ToList();
                                  var selectedEmployees = Functions.WeekendWorkStatementDocument.Remote.GetEmployeesForFilling(_obj)
                                    .ShowSelectMany(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesAddHyperlinkListTitle);
                                  
                                  if (selectedEmployees != null && selectedEmployees.Any())
                                  {
                                    employeesInfo.AddRange(selectedEmployees
                                                           .Where(x => !employeesIds.Contains(x.Id))
                                                           .Select(x => Structures.WeekendWorkStatementDocument.EmployeeInfo.Create(x.Id, x.Person.ShortName)));
                                    employeesText.Value = Functions.WeekendWorkStatementDocument.FormEmployeesNamesString(employeesInfo);
                                  }
                                });
      
      // Обработчик исключения сотрудников при нажатии на гиперссылку.
      deleteEmployees.SetOnExecute(() =>
                                   {
                                     var selectedEmployees = Functions.WeekendWorkStatementDocument.Remote.GetEmployees(employeesInfo)
                                       .ShowSelectMany(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.FillEmployeesDeleteHyperlinkListTitle);
                                     
                                     if (selectedEmployees != null && selectedEmployees.Any())
                                     {
                                       foreach (var employee in selectedEmployees)
                                       {
                                         var employeeInfo = employeesInfo.Where(x => x.Id == employee.Id).FirstOrDefault();
                                         if (employeeInfo != null)
                                           employeesInfo.Remove(employeeInfo);
                                       }
                                       employeesText.Value = Functions.WeekendWorkStatementDocument.FormEmployeesNamesString(employeesInfo);
                                     }
                                   });
      
      #endregion
      
      if (dialog.Show() == addButton)
        Functions.WeekendWorkStatementDocument.AddEmployes(_obj, employeesInfo, compensation.Value, workDateFrom.Value, workDateTo.Value,
                                                           restDateFrom.Value ?? restDate.Value, restDateTo.Value ?? restDate.Value,
                                                           firstBreakFrom.Value, firstBreakTo.Value, secondBreakFrom.Value, secondBreakTo.Value);
    }

  }
}