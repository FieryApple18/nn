using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations
{
  partial class AcquiantanceVacScheduleReportClientHandlers
  {

    public override void BeforeExecute(Sungero.Reporting.Client.BeforeExecuteEventArgs e)
    {
      var availableBusinessUnits = NNVacations.PublicFunctions.Module.Remote.GetAvailableBusinessUnits(Employees.Current);
      if (availableBusinessUnits.Count() == 0)
        Dialogs.NotifyMessage(NNVacations.Resources.AcquiantanceVacScheduleReportExecutionError);
      var dialog = Dialogs.CreateInputDialog(DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.ReportParameters);
      
      // Организации.
      var businessUnitsString = dialog.AddString(DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.CompanyParameter, true);
      businessUnitsString.IsEnabled = false;
      var selectBusinessUnits = dialog.AddHyperlink(Reports.Resources.AcquiantanceVacScheduleReport.SelectCompanies);
      var selectedBusinessUnits = new List<DirRX.OverrideSolution.IBusinessUnit>();
      
      // Подразделения.
      var departmentsString = dialog.AddString(DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.DepartmentParameter, false);
      departmentsString.IsEnabled = false;
      var selectDepartments = dialog.AddHyperlink(Reports.Resources.AcquiantanceVacScheduleReport.SelectDepartments);
      var selectedDepartments = new List<DirRX.OverrideSolution.IDepartment>();
      
      var subordinateDepartments = dialog.AddBoolean(Reports.Resources.AcquiantanceVacScheduleReport.SelectSubordinateDepartments, false);
      subordinateDepartments.IsEnabled = !string.IsNullOrEmpty(departmentsString.Value);
      
      var acquiantanceStatus = dialog.AddSelect(DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.AcqStatusParameter, false, string.Empty)
        .From(DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.AcquaintedStatus, DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.NotAcquaintedStatus);
      var year = dialog.AddDate(DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.YearParameter, true, Calendar.Now).AsYear();

      var availiableDepartments = NNCommons.PublicFunctions.Module.Remote.GetAvailableDepartments(availableBusinessUnits);
      
      if (availableBusinessUnits.Count() == 1)
      {
        businessUnitsString.Value = string.Join("; ", availableBusinessUnits.Select(s => s.Name));
        selectedBusinessUnits.Add(availableBusinessUnits.FirstOrDefault());
      }
      
      // Обработчик клика на ссылку "Выбрать организации".
      selectBusinessUnits.SetOnExecute(() =>
                                       {
                                         selectedBusinessUnits = availableBusinessUnits.ShowSelectMany(Reports.Resources.AcquiantanceVacScheduleReport.SelectCompanies).ToList();
                                         if (selectedBusinessUnits.Any())
                                         {
                                           businessUnitsString.Value = string.Join("; ", selectedBusinessUnits.Select(s => s.Name));
                                           availiableDepartments = NNCommons.PublicFunctions.Module.Remote.GetAvailableDepartments(selectedBusinessUnits);
                                         }
                                         else
                                         {
                                           businessUnitsString.Value = string.Empty;
                                           availiableDepartments = NNCommons.PublicFunctions.Module.Remote.GetAvailableDepartments(availableBusinessUnits);
                                         }
                                         departmentsString.Value = string.Empty;
                                         subordinateDepartments.Value = false;
                                         subordinateDepartments.IsEnabled = !string.IsNullOrEmpty(departmentsString.Value);
                                         selectedDepartments.Clear();
                                       });
      
      // Обработчик клика на ссылку "Выбрать подразделения".
      selectDepartments.SetOnExecute(() =>
                                     {
                                       selectedDepartments = availiableDepartments.ToList().ShowSelectMany(Reports.Resources.AcquiantanceVacScheduleReport.SelectDepartments).ToList();
                                       if (selectedDepartments.Any())
                                       {
                                         departmentsString.Value = string.Join("; ", selectedDepartments.Select(s => s.Name));
                                         subordinateDepartments.IsEnabled = !string.IsNullOrEmpty(departmentsString.Value);
                                         if (!selectedBusinessUnits.Any())
                                         {
                                           selectedBusinessUnits = selectedDepartments.Where(d => d.BusinessUnit != null)
                                             .Select(x => DirRX.OverrideSolution.BusinessUnits.As(x.BusinessUnit))
                                             .Distinct()
                                             .ToList();
                                           businessUnitsString.Value = string.Join("; ", selectedBusinessUnits.Select(s => s.Name));
                                         }
                                       }
                                       else
                                       {
                                         departmentsString.Value = string.Empty;
                                         subordinateDepartments.Value = false;
                                         subordinateDepartments.IsEnabled = !string.IsNullOrEmpty(departmentsString.Value);
                                       }
                                     });
      
      if (dialog.Show() != DialogButtons.Ok)
      {
        e.Cancel = true;
        return;
      }
      AcquiantanceVacScheduleReport.BusinessUnitName = businessUnitsString.Value;
      AcquiantanceVacScheduleReport.BusinessUnitIds = string.Join(";", selectedBusinessUnits.Select(x => x.Id));
      AcquiantanceVacScheduleReport.DepartmentIds = string.Join(";", selectedDepartments.Select(x => x.Id));
      AcquiantanceVacScheduleReport.StatusAcq = string.IsNullOrWhiteSpace(acquiantanceStatus.Value) ? string.Empty : acquiantanceStatus.Value;
      AcquiantanceVacScheduleReport.DateAcq = year.Value.Value.Year;
      AcquiantanceVacScheduleReport.IncludeHierarchy = subordinateDepartments.Value.Value;
      Logger.DebugFormat("AcquiantanceVacScheduleReport. Данные для отчета из диалога:{0} ИД НОР: {1}{0} ИД подразделений: {2}{0} Год: {3}{0} Учитывать подчиненные подразделения: {4}",
                         Environment.NewLine, AcquiantanceVacScheduleReport.BusinessUnitIds, AcquiantanceVacScheduleReport.DepartmentIds, AcquiantanceVacScheduleReport.DateAcq, AcquiantanceVacScheduleReport.IncludeHierarchy);
    }

  }
}