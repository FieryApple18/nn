using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNCommons.Client
{
  public partial class ModuleFunctions
  {
    /// <summary>
    /// Сформировать отчет SLA.
    /// </summary>
    public void CreateReportSLA()
    {
      var currentEmployee = DirRX.OverrideSolution.Employees.Current;
      var dialog = Dialogs.CreateInputDialog("SLA");
      
      // Виды документов.
      var documentKindsString = dialog.AddString(DirRX.NNCommons.Resources.ReportSLADialogDocKinds, true);
      documentKindsString.IsEnabled = false;
      var selectDocumentKinds = dialog.AddHyperlink(DirRX.NNCommons.Resources.ReportSLADialogSelectDocKinds);
      var selectedDocumentKinds = new List<DirRX.OverrideSolution.IDocumentKind>();
      
      // Организации.
      var businessUnitsString = dialog.AddString(DirRX.NNCommons.Resources.ReportSLADialogBusinessUnits, true);
      businessUnitsString.IsEnabled = false;
      var selectBusinessUnits = dialog.AddHyperlink(DirRX.NNCommons.Resources.ReportSLADialogSelectBusinessUnits);
      var selectedBusinessUnits = new List<DirRX.OverrideSolution.IBusinessUnit>();
      
      // Сотрудники.
      var employeesString = dialog.AddString(DirRX.NNCommons.Resources.ReportSLADialogEmployees, false);
      employeesString.IsEnabled = false;
      var selectEmployees = dialog.AddHyperlink(DirRX.NNCommons.Resources.ReportSLADialogSelectEmployees);
      var selectedEmployees = new List<DirRX.OverrideSolution.IEmployee>();
      
      // Исполнители заданий.
      var performersString = dialog.AddString(DirRX.NNCommons.Resources.ReportSLADialogPerformers, false);
      performersString.IsEnabled = false;
      var selectPerformers = dialog.AddHyperlink(DirRX.NNCommons.Resources.ReportSLADialogSelectPerformers);
      var selectedPerformers = new List<DirRX.OverrideSolution.IEmployee>();
      
      // Тип заданий.
      var assignmentType = dialog.AddSelect(Resources.ReportSLADialogAssignmentType, true).From(Resources.ReportSLADialogAssignmentTypeAll, Resources.ReportSLADialogAssignmentTypeEvent);
      
      // Диапозон дат.
      var beginDate = dialog.AddDate(DirRX.NNCommons.Resources.ReportSLADialogBeginDate, true, Calendar.BeginningOfMonth(Calendar.Today));
      var endDate = dialog.AddDate(DirRX.NNCommons.Resources.ReportSLADialogEndDate, true, Calendar.Today);
      
      // Получить доступные для выбора записи справочников.
      var availableBusinessUnits = NNCommons.PublicFunctions.Module.Remote.GetAvailableBusinessUnits(currentEmployee);
      var availiableEmployees = NNCommons.PublicFunctions.Module.Remote.GetAvailableEmployees(availableBusinessUnits);
      var availiablePerformers = NNCommons.PublicFunctions.Module.Remote.GetAvailablePerformers();
      var availableDocumentKinds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation.Remote.GetAvailableDocumentKinds(currentEmployee);
      
      if (availableBusinessUnits.Count() == 1)
      {
        businessUnitsString.Value = availableBusinessUnits.First().Name;
        selectedBusinessUnits.Add(availableBusinessUnits.FirstOrDefault());
      }
      
      // Обработчик клика на ссылку "Выбрать виды документов".
      selectDocumentKinds.SetOnExecute(() =>
                                       {
                                         selectedDocumentKinds = availableDocumentKinds.ShowSelectMany(DirRX.NNCommons.Resources.ReportSLADialogDocKinds).ToList();
                                         if (selectedDocumentKinds.Any())
                                         {
                                           documentKindsString.Value = string.Join("; ", selectedDocumentKinds.Select(s => s.Name));
                                         }
                                         else
                                         {
                                           documentKindsString.Value = string.Empty;
                                         }
                                       });

      // Обработчик клика на ссылку "Выбрать организации".
      selectBusinessUnits.SetOnExecute(() =>
                                       {
                                         selectedBusinessUnits = availableBusinessUnits.ShowSelectMany(DirRX.NNCommons.Resources.ReportSLADialogBusinessUnits).ToList();
                                         if (selectedBusinessUnits.Any())
                                         {
                                           businessUnitsString.Value = string.Join("; ", selectedBusinessUnits.Select(s => s.Name));
                                           availiableEmployees = NNCommons.PublicFunctions.Module.Remote.GetAvailableEmployees(selectedBusinessUnits);
                                         }
                                         else
                                         {
                                           businessUnitsString.Value = string.Empty;
                                           availiableEmployees = NNCommons.PublicFunctions.Module.Remote.GetAvailableEmployees(availableBusinessUnits);
                                         }
                                         employeesString.Value = string.Empty;
                                         performersString.Value = string.Empty;
                                         selectedEmployees.Clear();
                                         selectedPerformers.Clear();
                                       });
      
      // Обработчик клика на ссылку "Выбрать сотрудников".
      selectEmployees.SetOnExecute(() =>
                                   {
                                     selectedEmployees = availiableEmployees.ToList().ShowSelectMany(DirRX.NNCommons.Resources.ReportSLADialogEmployees).ToList();
                                     if (selectedEmployees.Any())
                                     {
                                       employeesString.Value = string.Join("; ", selectedEmployees.Select(s => s.Name));
                                       if (!selectedBusinessUnits.Any())
                                       {
                                         selectedBusinessUnits = selectedEmployees.Where(d => d.BusinessUnit != null)
                                           .Select(x => DirRX.OverrideSolution.BusinessUnits.As(x.BusinessUnit))
                                           .Distinct()
                                           .ToList();
                                         businessUnitsString.Value = string.Join("; ", selectedBusinessUnits.Select(s => s.Name));
                                       }
                                     }
                                     else
                                     {
                                       employeesString.Value = string.Empty;
                                     }
                                   });
      
      // Обработчик клика на ссылку "Выбрать исполнителей заданий".
      selectPerformers.SetOnExecute(() =>
                                    {
                                      selectedPerformers = availiablePerformers.ShowSelectMany(DirRX.NNCommons.Resources.ReportSLADialogPerformers).ToList();
                                      if (selectedPerformers.Any())
                                      {
                                        performersString.Value = string.Join("; ", selectedPerformers.Select(s => s.Name));
                                        if (!selectedBusinessUnits.Any())
                                        {
                                          selectedBusinessUnits = selectedPerformers.Where(d => d.BusinessUnit != null)
                                            .Select(x => DirRX.OverrideSolution.BusinessUnits.As(x.BusinessUnit))
                                            .Distinct()
                                            .ToList();
                                          businessUnitsString.Value = string.Join("; ", selectedBusinessUnits.Select(s => s.Name));
                                        }
                                      }
                                      else
                                      {
                                        performersString.Value = string.Empty;
                                      }
                                    });
      
      dialog.SetOnRefresh((er) => {
                            if (beginDate.Value.HasValue && endDate.Value.HasValue)
                            {
                              if (endDate.Value.Value < beginDate.Value.Value)
                                er.AddError(DirRX.NNCommons.Resources.ReportSLADialogEndDateError);
                              if (beginDate.Value.Value.AddMonths(Constants.Module.ReportSLA.DateDiffInMonths) < endDate.Value.Value)
                                er.AddError(DirRX.NNCommons.Resources.ReportSLADialogPeriodError);
                            }
                          });
      
      if (dialog.Show() != DialogButtons.Ok)
      {
        return;
      }
      
      var docKindsIds = selectedDocumentKinds.Select(d => d.Id).ToList();
      var businessUnitsIds = selectedBusinessUnits.Select(d => d.Id).ToList();
      var employeesIds = selectedEmployees.Select(d => d.Id).ToList();
      var performersIds = selectedPerformers.Select(d => d.Id).ToList();
      var reportPath = PublicFunctions.Module.Remote.GenerateReportSLA(docKindsIds, businessUnitsIds, employeesIds, performersIds, beginDate.Value.Value, endDate.Value.Value, currentEmployee, assignmentType.Value);
      if (!string.IsNullOrEmpty(reportPath))
        Sungero.Core.EntityDataPropertyExtensions.Open(System.IO.File.ReadAllBytes(reportPath), reportPath);
      else
        Dialogs.ShowMessage(Resources.PersonnelDocumentsRegisterDialogNoData);
    }

    /// <summary>
    /// Сформировать отчет "Реестр кадровых документов".
    /// </summary>
    public void CreatePersonnelDocumentsRegister()
    {
      var currentEmployee = DirRX.OverrideSolution.Employees.Current;
      var dialog = Dialogs.CreateInputDialog(Resources.PersonnelDocumentsRegisterReportTitle);
      
      // Виды документов.
      var documentKindsString = dialog.AddString(Resources.PersonnelDocumentsRegisterDialogDocKinds, true);
      documentKindsString.IsEnabled = false;
      var selectDocumentKinds = dialog.AddHyperlink(Resources.PersonnelDocumentsRegisterDialogSelectDocKinds);
      var selectedDocumentKinds = new List<DirRX.OverrideSolution.IDocumentKind>();

      // Организации.
      var businessUnitsString = dialog.AddString(Resources.PersonnelDocumentsRegisterDialogBusinessUnits, true);
      businessUnitsString.IsEnabled = false;
      var selectBusinessUnits = dialog.AddHyperlink(Resources.PersonnelDocumentsRegisterDialogSelectBusUnits);
      var selectedBusinessUnits = new List<DirRX.OverrideSolution.IBusinessUnit>();
      
      // Сотрудники.
      var employeesString = dialog.AddString(Resources.PersonnelDocumentsRegisterDialogEmployees, false);
      employeesString.IsEnabled = false;
      var selectEmployees = dialog.AddHyperlink(Resources.PersonnelDocumentsRegisterDialogSelectEmployees);
      var selectedEmployees = new List<DirRX.OverrideSolution.IEmployee>();
      
      // Диапозон дат.
      var beginDate = dialog.AddDate(Resources.PersonnelDocumentsRegisterDialogPeriodFrom, true, Calendar.BeginningOfMonth(Calendar.Today));
      var endDate = dialog.AddDate(Resources.PersonnelDocumentsRegisterDialogPeriodTo, true, Calendar.Today);
      
      // Получить доступные для выбора записи справочников.
      var availableBusinessUnits = NNCommons.PublicFunctions.Module.Remote.GetAvailableBusinessUnits(currentEmployee);
      var availiableEmployees = NNCommons.PublicFunctions.Module.Remote.GetAvailableEmployees(availableBusinessUnits);
      var availableDocumentKinds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation.Remote.GetAvailableDocumentKinds(currentEmployee);
      
      if (availableBusinessUnits.Count() == 1)
      {
        businessUnitsString.Value = availableBusinessUnits.First().Name;
        selectedBusinessUnits.Add(availableBusinessUnits.FirstOrDefault());
      }
      
      // Обработчик клика на ссылку "Выбрать виды документов".
      selectDocumentKinds.SetOnExecute(() =>
                                       {
                                         selectedDocumentKinds = availableDocumentKinds.ShowSelectMany(Resources.PersonnelDocumentsRegisterDialogDocKinds).ToList();
                                         if (selectedDocumentKinds.Any())
                                         {
                                           documentKindsString.Value = string.Join("; ", selectedDocumentKinds.Select(s => s.Name));
                                         }
                                         else
                                         {
                                           documentKindsString.Value = string.Empty;
                                         }
                                       });

      // Обработчик клика на ссылку "Выбрать организации".
      selectBusinessUnits.SetOnExecute(() =>
                                       {
                                         selectedBusinessUnits = availableBusinessUnits.ShowSelectMany(Resources.PersonnelDocumentsRegisterDialogBusinessUnits).ToList();
                                         if (selectedBusinessUnits.Any())
                                         {
                                           businessUnitsString.Value = string.Join("; ", selectedBusinessUnits.Select(s => s.Name));
                                           availiableEmployees = NNCommons.PublicFunctions.Module.Remote.GetAvailableEmployees(selectedBusinessUnits);
                                         }
                                         else
                                         {
                                           businessUnitsString.Value = string.Empty;
                                           availiableEmployees = NNCommons.PublicFunctions.Module.Remote.GetAvailableEmployees(availableBusinessUnits);
                                         }
                                         employeesString.Value = string.Empty;
                                         selectedEmployees.Clear();
                                       });
      
      // Обработчик клика на ссылку "Выбрать сотрудников".
      selectEmployees.SetOnExecute(() =>
                                   {
                                     selectedEmployees = availiableEmployees.ShowSelectMany(Resources.PersonnelDocumentsRegisterDialogEmployees).ToList();
                                     if (selectedEmployees.Any())
                                     {
                                       employeesString.Value = string.Join("; ", selectedEmployees.Select(s => s.Name));
                                       if (!selectedBusinessUnits.Any())
                                       {
                                         selectedBusinessUnits = selectedEmployees.Where(d => d.BusinessUnit != null)
                                           .Select(x => DirRX.OverrideSolution.BusinessUnits.As(x.BusinessUnit))
                                           .Distinct()
                                           .ToList();
                                         businessUnitsString.Value = string.Join("; ", selectedBusinessUnits.Select(s => s.Name));
                                       }
                                     }
                                     else
                                     {
                                       employeesString.Value = string.Empty;
                                     }
                                   });
      dialog.SetOnRefresh((er) => {
                            if (beginDate.Value.HasValue && endDate.Value.HasValue)
                            {
                              if (endDate.Value.Value < beginDate.Value.Value)
                                er.AddError(DirRX.NNCommons.Resources.ReportSLADialogEndDateError);
                              if (beginDate.Value.Value.AddMonths(Constants.Module.PersonnelDocumentRegisterReport.DateDiffInMonths) < endDate.Value.Value)
                                er.AddError(DirRX.NNCommons.Resources.PersonnelDocumentsRegisterDialogPeriodError);
                            }
                          });
      
      if (dialog.Show() != DialogButtons.Ok)
      {
        return;
      }
      
      var docKindsIds = selectedDocumentKinds.Select(d => d.Id).ToList();
      var businessUnitsIds = selectedBusinessUnits.Select(d => d.Id).ToList();
      var employeesIds = selectedEmployees.Select(d => d.Id).ToList();
      var reportPath = PublicFunctions.Module.Remote.GeneratePeronnelDocumentsRegister(docKindsIds, businessUnitsIds, employeesIds, beginDate.Value.Value, endDate.Value.Value, currentEmployee);
      if (!string.IsNullOrEmpty(reportPath))
        Sungero.Core.EntityDataPropertyExtensions.Open(System.IO.File.ReadAllBytes(reportPath), reportPath);
      else
        Dialogs.ShowMessage(Resources.PersonnelDocumentsRegisterDialogNoData);
    }

    /// <summary>
    /// Показать диалог запуска проверки документов в файловом хранилище.
    /// </summary>
    public virtual void VerifyDocumentsDialog()
    {
      if (Sungero.Company.Employees.Current == null)
      {
        Dialogs.ShowMessage("Формирование отчета под служебной учетной записью недоступно.", MessageType.Error);
        return;
      }
      
      var errorMessage = Functions.Module.Remote.CheckAccessVerifyDocuments();
      if (!string.IsNullOrEmpty(errorMessage))
      {
        Dialogs.ShowMessage(errorMessage, MessageType.Error);
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog(DirRX.NNCommons.Resources.VerifyDocumentsDialogName);
      var calendarNow = Calendar.Now;
      var beginDateControl = dialog.AddDate(DirRX.NNCommons.Resources.VerifyDocumentsDialogPeriodFrom, true, calendarNow.AddDays(-13).BeginningOfDay()).AsDateTime();
      var endDateControl = dialog.AddDate(DirRX.NNCommons.Resources.VerifyDocumentsDialogPeriodTo, true, calendarNow.EndOfDay()).AsDateTime();
      
      dialog.SetOnRefresh((h) => {
                            if (endDateControl.Value != null && beginDateControl.Value != null)
                            {
                              var totalDays = (endDateControl.Value.Value - beginDateControl.Value.Value).TotalDays;
                              if (totalDays > 14)
                                h.AddError(DirRX.NNCommons.Resources.VerifyDocumentsDialogLongPeriodError);
                              else if (totalDays <= 0)
                                h.AddError(DirRX.NNCommons.Resources.VerifyDocumentsDialogShortPeriodError);
                            }
                          });
      if (dialog.Show() == DialogButtons.Ok)
      {
        NNCommons.PublicFunctions.Module.Remote.VerifyDocumentsInStorage(beginDateControl.Value.Value,
                                                                         endDateControl.Value.Value,
                                                                         Sungero.Company.Employees.Current.Id,
                                                                         true,
                                                                         0);
        Dialogs.NotifyMessage(Resources.VerifyDocumentsInStorageNotificationStarted);
      }
    }
  }
}