using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNCommons.Client
{
  public partial class ModuleFunctions
  {
    #region Логика по формированию отчета проверки корректности маршрутов.
    
    /// <summary>
    /// Открывает диалог для формирования запроса экспорта предсказанных схем.
    /// Собирает параметры от пользователя и вызывает серверную функцию ExportPredictedSchemes.
    /// </summary>
    [Hyperlink(DisplayNameResource = "GenerateReportApprovalRoutes")]
    public void ShowExportPredictedSchemesDialog()
    {
      var dialog = Dialogs.CreateInputDialog(
        DirRX.NNCommons.Resources.ExportPredictedSchemesDialogTitle);

      // --- 1. Тип задачи (выпадающий список из зафиксированных значений) ---
      var taskTypes = new Dictionary<string, string>
      {
        { Constants.Module.PredctedSchemeReport.TaskType.StatementTask, DirRX.NNCommons.Resources.TaskTypeStatementTask },
        { Constants.Module.PredctedSchemeReport.TaskType.WeekendWorkTask, DirRX.NNCommons.Resources.WeekendWorkTask }
      };
      var taskTypeItems = taskTypes.Values.ToArray();
      var taskTypeControl = dialog.AddSelect(
        DirRX.NNCommons.Resources.ExportPredictedSchemesDialogTaskType,
        true,
        0).From(taskTypeItems);

      // --- 2. Вид документа (справочник «Виды документов», только поддерживаемые типы) ---
      var documentKindControl = dialog.AddSelect(
        DirRX.NNCommons.Resources.ExportPredictedSchemesDialogDocumentKind,
        true,
        Sungero.Docflow.DocumentKinds.Null).From(Functions.Module.Remote.GetSupportedDocumentKinds());

      // --- 3. Наша организация (справочник «Наши организации») ---
      var organizationControl = dialog.AddSelect(
        DirRX.NNCommons.Resources.ExportPredictedSchemesDialogOrganization,
        true,
        Sungero.Company.BusinessUnits.Null);

      var personnelSectionsControl = dialog.AddMultilineString(DirRX.NNCommons.Resources.ExportPredictedSchemesDialogPersonnelSections, false);
      personnelSectionsControl.IsEnabled = false;
      
      var personnelSectionsSelectControl = dialog.AddHyperlink(HRPro.HRWred.Resources.Select);
      personnelSectionsSelectControl.IsEnabled = false;
      
      var personalSectionIds = new List<long>();
      
      var employeesControl = dialog.AddMultilineString(DirRX.NNCommons.Resources.ExportPredictedSchemesDialogEmployees, false);
      employeesControl.IsEnabled = false;
      
      var employeesSelectControl = dialog.AddHyperlink(HRPro.HRWred.Resources.Select);
      employeesSelectControl.IsEnabled = false;
      
      var employeeIds = new List<long>();
      
      personnelSectionsSelectControl.SetOnExecute(
        () =>
        {
          var personalSections = Functions.Module.Remote.GetFilteredPersonalSection(organizationControl.Value).ShowSelectMany();
          personalSectionIds = personalSections.Select(p => p.Id).ToList();
          personnelSectionsControl.Value = string.Join("; ", personalSections.Select(s => s.Name));
        });
      
      employeesSelectControl.SetOnExecute(
        () =>
        {
          var employees = Functions.Module.Remote.GetFilteredEmployees(organizationControl.Value, personalSectionIds).ShowSelectMany();
          employeeIds = employees.Select(p => p.Id).ToList();
          employeesControl.Value = string.Join("; ", employees.Select(s => s.Name));
        });
      
      // --- Обработчик: изменение «Нашей организации» ---
      organizationControl.SetOnValueChanged(args =>
                                            {
                                              if (Equals(args.NewValue, args.OldValue))
                                                return;

                                              personalSectionIds = new List<long>();
                                              personnelSectionsControl.Value = string.Empty;
                                              personnelSectionsSelectControl.IsEnabled = args.NewValue != null;
                                              
                                              employeeIds = new List<long>();
                                              employeesControl.Value = string.Empty;
                                              employeesSelectControl.IsEnabled = args.NewValue != null;
                                            });
      
      personnelSectionsControl.SetOnValueChanged(args =>
                                                 {
                                                   if (Equals(args.NewValue, args.OldValue))
                                                     return;

                                                   employeeIds = new List<long>();
                                                   employeesControl.Value = string.Empty;
                                                 });


      // --- Показ диалога и обработка результата ---
      var okButton = dialog.Buttons.AddOk();
      dialog.Buttons.AddCancel();

      if (dialog.Show() != okButton)
      {
        return;
      }

      var selectedTaskType = taskTypes.FirstOrDefault(
        kv => kv.Value == taskTypeControl.Value);

      try
      {
        if (!employeeIds.Any())
          employeeIds = NNCommons.Functions.Module.Remote.GetFilteredEmployeeIds(organizationControl.Value, personalSectionIds);
        
        Functions.Module.Remote.CreatePredctedSchemeReport(selectedTaskType.Key,
                                                           selectedTaskType.Value,
                                                           documentKindControl.Value,
                                                           organizationControl.Value,
                                                           employeeIds);
        Dialogs.ShowMessage(
          DirRX.NNCommons.Resources.ExportPredictedSchemesDialogSuccess,
          MessageType.Information);
      }
      catch (Exception ex)
      {
        Dialogs.ShowMessage(
          string.Format(DirRX.NNCommons.Resources.ExportPredictedSchemesDialogError,
                        ex.Message),
          MessageType.Error);
      }
    }
    
    /// <summary>
    /// Обновляет фильтрацию доступных разделов персонала.
    /// </summary>
    /// <param name="organization">Наша организация.</param>
    /// <param name="personnelSectionsControl">Контрол разделов персонала.</param>
    private void UpdatePersonalSectionFilter(
      Sungero.Company.IBusinessUnit organization,
      ISelectManyDialogValue<NNCommons.IPersonnelSection> personnelSectionsControl
     )
    {
      if (organization != null)
      {
        personnelSectionsControl.IsEnabled = true;
        personnelSectionsControl.From(NNCommons.PersonnelSections.GetAll(p => Equals(p.BusinessUnit, organization)));
      }
    }

    /// <summary>
    /// Обновляет фильтрацию доступных сотрудников в контроле диалога.
    /// </summary>
    /// <param name="organization">Наша организация.</param>
    /// <param name="personnelSections">Контрол разделов персонала.</param>
    /// <param name="employeesControl">Контрол Сотрудников.</param>
    private void UpdateEmployeesFilter(
      Sungero.Company.IBusinessUnit organization,
      IEnumerable<DirRX.NNCommons.IPersonnelSection> personnelSections,
      ISelectManyDialogValue<Sungero.Company.IEmployee> employeesControl)
    {
      if (organization == null)
      {
        employeesControl.IsEnabled = false;
        employeesControl.Value = new Sungero.Company.IEmployee[0];
        return;
      }

      employeesControl.IsEnabled = true;

      var selectedSections = personnelSections?.ToList()
        ?? System.Linq.Enumerable.Empty<DirRX.NNCommons.IPersonnelSection>().ToList();
//
      //      employeesControl
      //        .From(Functions.Module.Remote.GetFilteredEmployees(organization, selectedSections));
    }

    /// <summary>
    /// Формирует список запросов для серверной функции на основе выбранных параметров.
    /// </summary>
    /// <param name="taskTypeName">Тип задачи.</param>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="organization">Наша орагнизация.</param>
    /// <param name="employees">Сотрудники.</param>
    /// <returns>Данные для подготовки отчета.</returns>
    private List<Structures.Module.IExportPredictedSchemeRequest> BuildRequests(
      string taskTypeName,
      Sungero.Docflow.IDocumentKind documentKind,
      Sungero.Company.IBusinessUnit organization,
      List<Sungero.Company.IEmployee> employees)
    {
      if (employees == null || employees.Count == 0)
        return null;

      if (documentKind == null)
        return null;

      var documentTypeName = GetDocumentTypeName(documentKind);

      var requests = new List<Structures.Module.IExportPredictedSchemeRequest>();

      // Сотрудник будет использоваться в дальнейшем при создании документа и задачи.
      foreach (var emp in employees)
      {
        var request = Structures.Module.ExportPredictedSchemeRequest.Create();
        request.AssignedById = emp.Id;
        request.AssigneeId = emp.Id;
        request.DocumentTypeName = documentTypeName;
        request.DocDocumentKind = documentKind.Id;
        if (organization != null)
          request.DocBusinessUnit = organization.Id;
        if (documentTypeName == Constants.Module.PredctedSchemeReport.DocumentType.StatementDocument)
          request.DocAuthor = emp.Id;
        request.TaskTypeName = taskTypeName;
        requests.Add(request);
      }

      return requests;
    }

    /// <summary>
    /// Возвращает имя типа документа по сущности вида документа.
    /// </summary>
    private string GetDocumentTypeName(Sungero.Docflow.IDocumentKind documentKind)
    {
      var typeGuid = documentKind != null && documentKind.DocumentType != null
        ? documentKind.DocumentType.DocumentTypeGuid
        : null;
      if (typeGuid == null)
        return null;

      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.SimpleDocument)
        return Constants.Module.PredctedSchemeReport.DocumentType.SimpleDocument;
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.Memo)
        return Constants.Module.PredctedSchemeReport.DocumentType.Memo;
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.ContractualDocument)
        return Constants.Module.PredctedSchemeReport.DocumentType.ContractualDocument;
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.StatementDocument)
        return Constants.Module.PredctedSchemeReport.DocumentType.StatementDocument;

      return null;
    }
    
    #endregion
    
    /// <summary>
    /// Запустить задачи на выпуск сертификатов.
    /// </summary>
    public void MassIssueCertificates()
    {
      Functions.Module.Remote.MassIssueCertificates();
    }
    
    /// <summary>
    /// Закрыть сертификаты ПЭП.
    /// </summary>
    public void DisableSimpleCertificates()
    {
      Functions.Module.Remote.DisableSimpleCertificates();
    }
    
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
    
    /// <summary>
    /// Выполнить измерение скорости расчёта времени с учётом календарей рабочего времени.
    /// </summary>
    [LocalizeFunction("WorkingTimeCalculationSpeedTest", "WorkingTimeCalculationSpeedTestDescription")]
    public void WorkingTimeCalculationSpeedTest()
    {
      var dialog = Dialogs.CreateInputDialog(DirRX.NNCommons.Resources.WorkingTimeCalculationSpeedTest);
      var employeeCount = dialog.AddInteger(DirRX.NNCommons.Resources.WorkingTimeCalculationSpeedtestDialog_EmployeeCount, true, 10);
      var shift = dialog.AddInteger(DirRX.NNCommons.Resources.WorkingTimeCalculationSpeedtestDialog_Shift, true);
      var shiftHours = dialog.AddBoolean(DirRX.NNCommons.Resources.WorkingTimeCalculationSpeedtestDialog_HoursShift, true);
      var shiftDays = dialog.AddBoolean(DirRX.NNCommons.Resources.WorkingTimeCalculationSpeedtestDialog_DaysShift, false);
      dialog.Buttons.AddOkCancel();
      
      shiftHours.SetOnValueChanged((h) => {if (h.NewValue.Value == true) shiftDays.Value = false;});
      shiftDays.SetOnValueChanged((h) => {if (h.NewValue.Value == true) shiftHours.Value = false;});
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var result = PublicFunctions.Module.Remote.WorkingTimeCalculationSpeedTest(employeeCount.Value.Value,
                                                                                   shift.Value.Value,
                                                                                   shiftDays.Value.Value);
        
        Dialogs.ShowMessage(result, MessageType.None);
      }
    }
  }
}