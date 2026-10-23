using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Aspose.Cells;
using HRPro.HRDocFlow.Server;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Domain.Shared;
using Newtonsoft.Json;
using Sungero.Workflow.Server;
using Sungero.Workflow.Interfaces;
using Sungero.Workflow.Interfaces.ProcessKind;
using Sungero.Workflow.Interfaces.WorkflowComponents;
using Sungero.Workflow.Shared;
using Sungero.Domain;
using Sungero.Domain.SessionExtensions;
using Sungero.Workflow;
using Sungero.Workflow.Server.NoCode;
using Sungero.Workflow.Server.PublishedScheme;

namespace DirRX.NNCommons.Server
{
  public partial class ModuleFunctions
  {

    #region Логика по формированию отчета проверки корректности маршрутов.
    
    /// <summary>
    /// Формирует предсказнную схему для отчета.
    /// </summary>
    /// <param name="requests">Данные для формирования задачи и документа.</param>
    /// <returns>Данные для формирования отчета.</returns>
    [Public]
    public virtual List<Structures.Module.IProcessKindSchemeResult> ExportPredictedSchemes(List<Structures.Module.IExportPredictedSchemeRequest> requests)
    {
      if (requests == null || requests.Count == 0)
        throw AppliedCodeException.Create("Тело запроса пустое или не содержит элементов.");

      var createdTasks = new List<Sungero.Workflow.ITask>();
      var createdDocuments = new List<Sungero.Docflow.IOfficialDocument>();
      var result = new List<Structures.Module.IProcessKindSchemeResult>();

      // Документ и задача всегда одного типа: создаются один раз по данным
      // первого запроса и переиспользуются для всего пакета.
      var firstRequest = requests[0];
      var taskTypeName = string.IsNullOrWhiteSpace(firstRequest.TaskTypeName)
        ? Constants.Module.PredctedSchemeReport.TaskType.ActionItemExecutionTask
        : firstRequest.TaskTypeName;

      try
      {
        // --- 1. Создание тестового документа и задачи один раз (по первому запросу) ---
        // Ошибка создания ресурсов не прерывает обработку пакета:
        // ошибка фиксируется и выдаётся в результатах всех запросов.
        var assignedByfirst = Sungero.Company.Employees.Get(firstRequest.AssignedById);
        if (assignedByfirst == null)
          throw AppliedCodeException.Create($"Сотрудник (AssignedBy) с Id={firstRequest.AssignedById} не найден.");

        var assigneefirst = Sungero.Company.Employees.Get(firstRequest.AssigneeId);
        
        if (assigneefirst == null)
          throw AppliedCodeException.Create($"Сотрудник (Assignee) с Id={firstRequest.AssigneeId} не найден.");

        // Создаём тестовый документ
        var testDocument = CreateTestDocument(firstRequest);
        // Создаём тестовую задачу - отключено, см. ниже обяснение.
        //var task = CreateTestTask(testDocument, assignedByfirst, assigneefirst, taskTypeName);

        // --- 2. Обработка каждого элемента пакета с переиспользованием ---
        foreach (var req in requests)
        {
          var itemResult = Structures.Module.ProcessKindSchemeResult.Create();

          try
          {
            var assignedBy = OverrideSolution.Employees.Get(req.AssignedById);
            if (assignedBy == null)
              throw AppliedCodeException.Create($"Сотрудник (AssignedBy) с Id={req.AssignedById} не найден.");

            var assignee = OverrideSolution.Employees.Get(req.AssigneeId);
            if (assignee == null)
              throw AppliedCodeException.Create($"Сотрудник (Assignee) с Id={req.AssigneeId} не найден.");
            
            // Обновить поля существующего тестового документа на основании параметров запроса.
            UpdateTestDocument(testDocument, req);

            // Убираем документ из вложения задачи и добавляем его обратно:
            // при сохранении задачи перегенерируется предсказанная схема.
            //ReplaceTaskDocument(task, testDocument);

            // Обновляем исполнителей на переиспользуемой задаче
            //UpdateTaskAssignees(task, assignedBy, assignee, taskTypeName);

            // HACK: временное решение, баг https://rxtfs.directum.ru/is-builder8/Kotlin/_workitems/edit/444216
            var task = CreateTestTask(testDocument, assignedBy, assignee, taskTypeName);

            var processKind = ProcessKinds.GetAllMatches(task).FirstOrDefault();
            task.ProcessKind = processKind;
            
            itemResult.ProcessKindId = processKind != null ? processKind.Id : 0;
            itemResult.ProcessKindName = processKind != null ? processKind.Name : string.Empty;
            
            task.Save();
            
            // Собираем информацию о схеме
            itemResult.Scheme = BuildSchemeInfo(task, assignedBy);

            // --- Заполнение данных из карточки сотрудника (assignee) ---
            itemResult.OrganizationName = assignee.Department != null && assignee.Department.BusinessUnit != null
              ? assignee.Department.BusinessUnit.Name
              : "";

            if (assignee != null && assignee.PersonnelSectionDirRX != null)
            {
              itemResult.PersonnelSectionId = assignee.PersonnelSectionDirRX.Id;
              itemResult.PersonnelSectionName = assignee.PersonnelSectionDirRX.Name;
            }

            itemResult.PersonnelNumber = assignee.PersonnelNumber ?? string.Empty;
            itemResult.EmployeeName = assignee.Person != null ? assignee.Person.Name : string.Empty;
            itemResult.SapIdJobTitle = OverrideSolution.JobTitles.As(assignee.JobTitle)?.SapIdDirRX;
            itemResult.JobTitleName = assignee.JobTitle != null ? assignee.JobTitle.Name : string.Empty;
            itemResult.SapIdDepartment = OverrideSolution.Departments.As(assignee.Department)?.SapIdDirRX;
            itemResult.DepartmentName = assignee.Department != null ? assignee.Department.Name : string.Empty;

            // Формируем строки маршрута согласования и пропущенных этапов
            BuildRouteStrings(itemResult);

            // Если вариант процесса не подобрался, схема будет пустой
            if (itemResult.Scheme.Blocks.Count == 0)
              itemResult.Error = DirRX.NNCommons.Resources.SchemeNotFound;
            try
            {
              Sungero.Workflow.Tasks.Delete(task);
            }
            catch (AppliedCodeException ex)
            {
              Logger.ErrorFormat("Не удалось удалить тестовую задачу {0}.", ex, task.Id);
            }
          }
          catch (AppliedCodeException ex)
          {
            itemResult.Error = ex.Message;
          }

          result.Add(itemResult);
        }
        
        if (result.Any())
          result = result.OrderBy(r => r.EmployeeName).ToList();

        try
        {
          Sungero.Docflow.OfficialDocuments.Delete(testDocument);
        }
        catch (AppliedCodeException ex)
        {
          Logger.ErrorFormat("Не удалось удалить тестовый документ {0}.", ex, testDocument.Id);
        }

        return result;
      }
      catch (AppliedCodeException ex)
      {
        Logger.Error($"Не удалось сформировать схему", ex);
      }
      
      return result;
    }

    // =================================================================================
    // Вспомогательные методы
    // =================================================================================



    /// <summary>
    /// Формирует Excel-отчёт на основе шаблона.
    /// Первая строка — заголовки (сохраняется без изменений).
    /// Данные заполняются начиная со второй строки.
    /// Колонки: A=Организация, B=Раздел персонала, C=Табельный номер, D=Сотрудник,
    /// E=ID должности, F=Должность, G=ID подразделения, H=Подразделение,
    /// I=Вариант процесса, J=Маршрут согласования, K=Пропущенные этапы.
    /// </summary>
    /// <param name="results">Данные для формирования отчета.</param>
    /// <returns>Поток сформированного отчета.</returns>
    public System.IO.MemoryStream GetExcelReport(
      List<Structures.Module.IProcessKindSchemeResult> results)
    {
      
      // Получаем шаблон документа по ID
      var reportTemplateId = NNCommons.PublicFunctions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.PredctedSchemeReportTemplate.Guid);
      
      var template = Sungero.Content.ElectronicDocumentTemplates.GetAll(e => e.Id == reportTemplateId).FirstOrDefault();
      if (template == null || template.LastVersion == null)
        throw AppliedCodeException.Create($"Шаблон отчёта с Id={reportTemplateId} не найден или не имеет версий.");

      var templateBody = template.LastVersion.Body;
      if (templateBody == null)
        throw AppliedCodeException.Create($"Тело шаблона отчёта с Id={reportTemplateId} пустое.");

      var templateBytes = templateBody.Read();
      var report = IsolatedFunctions.OpenXML.BuildExcelReport(results, templateBytes);
      
      var memoryStream = new MemoryStream();

      report.CopyTo(memoryStream);
      memoryStream.Position = 0;
      
      return memoryStream;
    }
    
    /// <summary>
    /// Создание тестового документа для предсказания схемы.
    /// </summary>
    /// <param name="documentParameters">Параметры документа.</param>
    /// <returns>Документ.</returns>
    private Sungero.Docflow.IOfficialDocument CreateTestDocument(Structures.Module.IExportPredictedSchemeRequest documentParameters)
    {
      if (documentParameters == null || !documentParameters.DocDocumentKind.HasValue)
        throw AppliedCodeException.Create("Параметр DocDocumentKind не указан в documentParameters.");

      long documentKindId = documentParameters.DocDocumentKind.Value;
      var documentKind = Sungero.Docflow.DocumentKinds.Get(documentKindId);
      if (documentKind == null)
        throw AppliedCodeException.Create($"Вид документа с Id={documentKindId} не найден.");

      // Определяем конкретный тип документа по сущности вида (DocumentTypeGuid)
      // и сопоставляем его с обработчиком создания.
      var typeGuid = documentKind.DocumentType != null
        ? documentKind.DocumentType.DocumentTypeGuid
        : null;
      if (typeGuid == null)
        throw AppliedCodeException.Create($"У вида документа с Id={documentKindId} не задан тип документа.");

      IOfficialDocument document = null;
      
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.SimpleDocument)
        document = CreateSimpleDocument(documentKind, documentParameters);
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.ContractualDocument)
        document = CreateContractualDocument(documentKind, documentParameters);
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.Memo)
        document = CreateMemoDocument(documentKind, documentParameters);
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.StatementDocument)
        document = CreateStatementDocument(documentKind, documentParameters);
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.VacationStatementDocument)
        document = CreateVacationStatementDocument(documentKind, documentParameters);
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.LrdPaymentStatementDocument)
        document = CreateLrdPaymentStatementDocument(documentKind, documentParameters);
      if (typeGuid == Constants.Module.PredctedSchemeReport.DocumentTypeGuid.WeekendWorkStatementDocument)
        document = CreateWeekendWorkStatementDocument(documentKind, documentParameters);
      
      if (document == null)
        throw AppliedCodeException.Create($"Неподдерживаемый тип документа: {typeGuid}.");
      
      // Отключаем АО по выдаче прав, чтобы не сработала блокировка в момент удаления.
      var documentParams = ((Sungero.Domain.Shared.IExtendedEntity)document).Params;
      documentParams.Remove(Sungero.Docflow.PublicConstants.OfficialDocument.GrantAccessRightsToDocumentAsync);
      document.Save();
      
      return document;
    }

    /// <summary>
    /// Обновляет поля существующего тестового документа из параметров запроса
    /// и пересохраняет его.
    /// </summary>
    private void UpdateTestDocument(Sungero.Docflow.IOfficialDocument document, Structures.Module.IExportPredictedSchemeRequest documentParameters)
    {
      if (Sungero.Docflow.Memos.Is(document))
      {
        var memo = Sungero.Docflow.Memos.As(document);
        ApplyMemoParameters(memo, documentParameters);
      }

      if (HRPro.HRDocFlow.StatementDocuments.Is(document))
      {
        var statement = HRPro.HRDocFlow.StatementDocuments.As(document);
        ApplyStatementParameters(statement, documentParameters);
      }
      
      if (OverrideSolution.VacationStatementDocuments.Is(document))
      {
        var vacationStatementDocument = OverrideSolution.VacationStatementDocuments.As(document);
        ApplyVacationStatementDocumentParameters(vacationStatementDocument, documentParameters);
      }
      
      if (NNOtherProcesses.LrdPaymentStatementDocuments.Is(document))
      {
        var lrdPaymentStatementDocument = NNOtherProcesses.LrdPaymentStatementDocuments.As(document);
        ApplyLrdPaymentStatementDocumentParameters(lrdPaymentStatementDocument, documentParameters);
      }
      
      if (NNOtherProcesses.WeekendWorkStatementDocuments.Is(document))
      {
        var weekendWorkStatementDocument = NNOtherProcesses.WeekendWorkStatementDocuments.As(document);
        ApplyWeekendWorkStatementDocumentParameters(weekendWorkStatementDocument, documentParameters);
      }

      // Отключаем АО по выдаче прав, чтобы не сработала блокировка в момент удаления.
      var documentParams = ((Sungero.Domain.Shared.IExtendedEntity)document).Params;
      documentParams.Remove(Sungero.Docflow.PublicConstants.OfficialDocument.GrantAccessRightsToDocumentAsync);
      document.Save();
    }

    /// <summary>
    /// Создаёт тестовый документ SimpleDocument.
    /// </summary>
    /// <param name="documentKind">Вид документа</param>
    /// <param name="documentParameters">Параметры для документа.</param>
    /// <returns>Простой документ.</returns>
    private Sungero.Docflow.IOfficialDocument CreateSimpleDocument(
      Sungero.Docflow.IDocumentKind documentKind,
      Structures.Module.IExportPredictedSchemeRequest documentParameters)
    {
      var document = Sungero.Docflow.SimpleDocuments.Create();
      document.DocumentKind = documentKind;
      document.Name = string.Format(PublicConstants.Module.PredctedSchemeReport.NameDocumentTest, Guid.NewGuid().ToString("N")).Substring(0, 30);

      return document;
    }


    /// <summary>
    /// Создаёт тестовый документ Договорной документ.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="documentParameters">Параметры для документа.</param>
    /// <returns>Договорной документ.</returns>
    private Sungero.Docflow.IOfficialDocument CreateContractualDocument(
      Sungero.Docflow.IDocumentKind documentKind,
      Structures.Module.IExportPredictedSchemeRequest documentParameters)
    {
      var document = Sungero.Contracts.ContractualDocuments.Create();
      document.DocumentKind = documentKind;
      document.Name = string.Format(PublicConstants.Module.PredctedSchemeReport.NameDocumentTest, Guid.NewGuid().ToString("N")).Substring(0, 30);

      return document;
    }

    /// <summary>
    /// Создаёт тестовый документ Служебная записка с заполнением специфичных полей.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="documentParameters">Параметры для документа.</param>
    /// <returns>Служебная записка.</returns>
    private Sungero.Docflow.IOfficialDocument CreateMemoDocument(
      Sungero.Docflow.IDocumentKind documentKind,
      Structures.Module.IExportPredictedSchemeRequest documentParameters)
    {
      var document = Sungero.Docflow.Memos.Create();
      document.DocumentKind = documentKind;
      document.Name = string.Format(PublicConstants.Module.PredctedSchemeReport.NameDocumentTest, Guid.NewGuid().ToString("N")).Substring(0, 30);
      document.Subject = PublicConstants.Module.PredctedSchemeReport.SubjectTest;

      ApplyMemoParameters(document, documentParameters);

      return document;
    }
    
    /// <summary>
    /// Создаёт тестовый документ Заявление и согласие по отпуску.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="documentParameters">Параметры для документа.</param>
    /// <returns>Заявление и согласие по отпуску.</returns>
    private OverrideSolution.IVacationStatementDocument CreateVacationStatementDocument(
      Sungero.Docflow.IDocumentKind documentKind,
      Structures.Module.IExportPredictedSchemeRequest documentParameters)
    {
      var document = OverrideSolution.VacationStatementDocuments.Create();
      
      document.DocumentKind = documentKind;
      document.Name = string.Format(PublicConstants.Module.PredctedSchemeReport.NameDocumentTest, Guid.NewGuid().ToString("N")).Substring(0, 30);
      document.Subject = PublicConstants.Module.PredctedSchemeReport.SubjectTest;
      
      ApplyVacationStatementDocumentParameters(document, documentParameters);
      
      return document;
    }
    
    /// <summary>
    /// Создает тестовый документ Заявление на выплату по ЛНА.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="documentParameters">Параметры для документа.</param>
    /// <returns>Заявление на выплату по ЛНА.</returns>
    private NNOtherProcesses.ILrdPaymentStatementDocument CreateLrdPaymentStatementDocument(
      Sungero.Docflow.IDocumentKind documentKind,
      Structures.Module.IExportPredictedSchemeRequest documentParameters)
    {
      var document = NNOtherProcesses.LrdPaymentStatementDocuments.Create();
      
      document.DocumentKind = documentKind;
      document.Name = string.Format(PublicConstants.Module.PredctedSchemeReport.NameDocumentTest, Guid.NewGuid().ToString("N")).Substring(0, 30);
      document.Subject = PublicConstants.Module.PredctedSchemeReport.SubjectTest;
      
      ApplyLrdPaymentStatementDocumentParameters(document, documentParameters);
      
      return document;
    }
    
    /// <summary>
    /// Создает тестовый документ Заявка на привлечение к работе в ВД/ПД/СР.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="documentParameters">Параметры для документа.</param>
    /// <returns>Заявка на привлечение к работе в ВД/ПД/СР</returns>
    private NNOtherProcesses.IWeekendWorkStatementDocument CreateWeekendWorkStatementDocument(
      Sungero.Docflow.IDocumentKind documentKind,
      Structures.Module.IExportPredictedSchemeRequest documentParameters)
    {
      var document = NNOtherProcesses.WeekendWorkStatementDocuments.Create();
      
      document.DocumentKind = documentKind;
      document.Name = string.Format(PublicConstants.Module.PredctedSchemeReport.NameDocumentTest, Guid.NewGuid().ToString("N")).Substring(0, 30);
      document.Subject = PublicConstants.Module.PredctedSchemeReport.SubjectTest;
      
      ApplyWeekendWorkStatementDocumentParameters(document, documentParameters);
      
      return document;
    }
    
    /// <summary>
    /// Заполнение полей для документа Заявка на привлечение к работе в ВД/ПД/СР.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="dp">Данные для документа.</param>
    private void ApplyWeekendWorkStatementDocumentParameters(
      NNOtherProcesses.IWeekendWorkStatementDocument document,
      Structures.Module.IExportPredictedSchemeRequest dp)
    {
      var employee = OverrideSolution.Employees.GetAll(e => e.Id == dp.AssignedById).FirstOrDefault();
      document.PreparedBy = employee;
      document.Department = employee.Department;
      document.BusinessUnit = employee.BusinessUnit;
      document.PersonnelSectionDirRX = employee.PersonnelSectionDirRX;
      
      var workDate = Calendar.BeginningOfDay(Calendar.EndOfWeek(Calendar.Now));
      document.WorkDateFrom = workDate.AddHours(8);
      document.WorkDateTo = workDate.AddHours(16);
      document.Reason = NNOtherProcesses.WeekendWorkReasons.GetAll(w => w.Status == NNOtherProcesses.WeekendWorkReason.Status.Active).FirstOrDefault();
      
      document.Employees.Clear();
      
      var record = document.Employees.AddNew();
      record.Employee = employee;
      record.Compensation = NNOtherProcesses.WeekendWorkCompensationKinds.GetAll(w => Equals(w.DocumentKind, document.DocumentKind)).FirstOrDefault();
      record.WorkDateFrom = document.WorkDateFrom;
      record.WorkDateTo = document.WorkDateTo;
      record.FirstBreakFrom = workDate.AddHours(12);
      record.FirstBreakTo = workDate.AddHours(13);
      record.PaidBreak = false;
    }
    
    /// <summary>
    /// Заполнение полей для документа Заявление на выплату по ЛНА.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="dp">Данные для документа.</param>
    private void ApplyLrdPaymentStatementDocumentParameters(
      NNOtherProcesses.ILrdPaymentStatementDocument document,
      Structures.Module.IExportPredictedSchemeRequest dp)
    {
      var employee = OverrideSolution.Employees.GetAll(e => e.Id == dp.AssignedById).FirstOrDefault();
      document.EmployeeDirRX = employee;
      document.PreparedBy = employee;
    }
    
    /// <summary>
    /// Заполнение полей для документа Заявление и согласие по отпуску
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="dp">Данные для документа.</param>
    private void ApplyVacationStatementDocumentParameters(
      OverrideSolution.IVacationStatementDocument document,
      Structures.Module.IExportPredictedSchemeRequest dp)
    {
      var employee = OverrideSolution.Employees.GetAll(e => e.Id == dp.AssignedById).FirstOrDefault();
      document.Employee = employee;
      document.PersonnelNumberDirRX = !string.IsNullOrEmpty(employee.PersonnelNumber) ? employee.PersonnelNumber : "1";
      document.IsShiftByStatementDirRX = true;
    }

    /// <summary>
    /// Заполняет специфичные поля Memo из параметров.
    /// </summary>
    private void ApplyMemoParameters(
      Sungero.Docflow.IMemo memo,
      Structures.Module.IExportPredictedSchemeRequest dp)
    {
      // Устанавливаем Addressee из параметра "Адресат" (ID сотрудника)
      SetEmployeeParameter(dp.AssignedById, (employee) => { memo.Addressee = employee; });

      // Устанавливаем BusinessUnit из параметра "Наша организация" (ID BusinessUnit)
      SetBusinessUnitParameter(dp.DocBusinessUnit, (businessUnit) => { memo.BusinessUnit = businessUnit; });

      // Устанавливаем OurSignatory из параметра "Подписал" (ID сотрудника)
      SetEmployeeParameter(dp.DocSignatory, (employee) => { memo.OurSignatory = employee; });

      // Устанавливаем PreparedBy из параметра "Подготовил" (ID сотрудника)
      SetEmployeeParameter(dp.DocPreparedBy, (employee) => { memo.PreparedBy = employee; });

      // Устанавливаем Department из параметра "Подразделение" (ID подразделения)
      SetDepartmentParameter(dp.DocDepartment, (department) => { memo.Department = department; });
    }

    /// <summary>
    /// Создаёт тестовый документ StatementDocument с заполнением обязательных и специфичных полей.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="documentParameters">Параметры для документа.</param>
    /// <returns>Документ.</returns>
    private Sungero.Docflow.IOfficialDocument CreateStatementDocument(
      Sungero.Docflow.IDocumentKind documentKind,
      Structures.Module.IExportPredictedSchemeRequest documentParameters)
    {
      var document = HRPro.HRDocFlow.StatementDocuments.Create();
      document.DocumentKind = documentKind;
      document.Name = string.Format(PublicConstants.Module.PredctedSchemeReport.NameDocumentTest, Guid.NewGuid().ToString("N")).Substring(0, 30);

      ApplyStatementParameters(document, documentParameters);

      document.Save();
      return document;
    }

    /// <summary>
    /// Заполняет специфичные поля для Заявления из параметров.
    /// </summary>
    /// <param name="statement">Заявление</param>
    /// <param name="dp"></param>
    private void ApplyStatementParameters(
      HRPro.HRDocFlow.IStatementDocument statement,
      Structures.Module.IExportPredictedSchemeRequest dp)
    {
      // Обязательное поле: Author (Автор) — сотрудник
      SetEmployeeParameter(dp.AssignedById, (employee) => { statement.Author = employee; });

      // Опциональные поля из параметров
      SetEmployeeParameter(dp.AssignedById, (employee) => { statement.PreparedBy = employee; });
      SetBusinessUnitParameter(dp.DocBusinessUnit, (businessUnit) => { statement.BusinessUnit = businessUnit; });
      SetEmployeeParameter(dp.DocSignatory, (employee) => { statement.OurSignatory = employee; });
      SetDepartmentParameter(dp.DocDepartment, (department) => { statement.Department = department; });
    }

    /// <summary>
    /// Получает сотрудника по Id и применяет действие, если сотрудник найден.
    /// </summary>
    private void SetEmployeeParameter(
      long? employeeId,
      Action<Sungero.Company.IEmployee> setter)
    {
      if (!employeeId.HasValue)
        return;

      var employee = Sungero.Company.Employees.Get(employeeId.Value);
      if (employee != null)
        setter(employee);
      else
        Logger.Debug($"Сотрудник с Id={employeeId.Value} не найден.");
    }

    /// <summary>
    /// Получает BusinessUnit по Id и применяет действие, если организация найдена.
    /// </summary>
    /// <param name="businessUnitId">Id нашей орагнизации.</param>
    /// <param name="setter">Делегат для заполнения св-ва</param>
    private void SetBusinessUnitParameter(
      long? businessUnitId,
      Action<Sungero.Company.IBusinessUnit> setter)
    {
      if (!businessUnitId.HasValue)
        return;

      var businessUnit = Sungero.Company.BusinessUnits.Get(businessUnitId.Value);
      if (businessUnit != null)
        setter(businessUnit);
      else
        Logger.Debug($"Наша организация с Id={businessUnitId.Value} не найдена.");
    }

    /// <summary>
    /// Получает Department по Id и применяет действие, если подразделение найдено.
    /// </summary>
    /// <param name="businessUnitId">Id нашей орагнизации.</param>
    /// <param name="setter">Делегат для заполнения св-ва.</param>
    private void SetDepartmentParameter(
      long? departmentId,
      Action<Sungero.Company.IDepartment> setter)
    {
      if (!departmentId.HasValue)
        return;

      var department = Sungero.Company.Departments.Get(departmentId.Value);
      if (department != null)
        setter(department);
      else
        Logger.Debug($"Подразделение с Id={departmentId.Value} не найдено.");
    }

    /// <summary>
    /// Создает тестовую задачу для предсказания маршрута.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="assignedBy">Выдал.</param>
    /// <param name="assignee">Исполнитель.</param>
    /// <param name="taskTypeName">Тип задачи.</param>
    /// <returns>Задача.</returns>
    private Sungero.Workflow.ITask CreateTestTask(
      Sungero.Docflow.IOfficialDocument document,
      Sungero.Company.IEmployee assignedBy,
      Sungero.Company.IEmployee assignee,
      string taskTypeName = Constants.Module.PredctedSchemeReport.TaskType.ActionItemExecutionTask)
    {
      switch (taskTypeName)
      {
        case Constants.Module.PredctedSchemeReport.TaskType.StatementTask:
          {
            var statementTask = OverrideSolution.StatementTasks.Create();

            statementTask.Subject = DirRX.NNCommons.Resources.TestTaskSubject;
            statementTask.ActiveText = DirRX.NNCommons.Resources.TestTaskSubject;
            statementTask.Author = assignedBy;

            // Привязка документа StatementDocument к задаче
            if (document != null && !statementTask.DocumentGroup.StatementDocuments.Any())
              statementTask.DocumentGroup.DocRelatedToWorkBases.Add(OverrideSolution.DocRelatedToWorkBases.As(document));
            return Sungero.Workflow.Tasks.As(statementTask);
          }
        case Constants.Module.PredctedSchemeReport.TaskType.WeekendWorkTask:
          {
            var weekendTask = NNOtherProcesses.WeekendWorkTasks.Create();
            
            weekendTask.Subject = DirRX.NNCommons.Resources.TestTaskSubject;
            weekendTask.ActiveText = DirRX.NNCommons.Resources.TestTaskSubject;
            weekendTask.Author = assignedBy;
            
            weekendTask.DocumentGroup.OfficialDocuments.Add(OverrideSolution.OfficialDocuments.As(document));
            
            return Sungero.Workflow.Tasks.As(weekendTask);
          }
        default:
          {
            var actionItemTask = Sungero.RecordManagement.ActionItemExecutionTasks.Create();

            actionItemTask.ActionItem = DirRX.NNCommons.Resources.TestActionItemTaskSubject;
            actionItemTask.Assignee = assignee;
            actionItemTask.AssignedBy = assignedBy;
            actionItemTask.HasIndefiniteDeadline = false;
            actionItemTask.Deadline = Calendar.Now.AddWorkingDays(5);
            actionItemTask.ActiveText = DirRX.NNCommons.Resources.TestActionItemTaskSubject;

            if (document != null && !actionItemTask.DocumentsGroup.OfficialDocuments.Any())
              actionItemTask.DocumentsGroup.OfficialDocuments.Add(document);

            return Sungero.Workflow.Tasks.As(actionItemTask);
          }
      }
    }

    /// <summary>
    /// Обновляет исполнителей на существующей задаче без создания новой.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="assignedBy">Выдал.</param>
    /// <param name="assignee">Исполнитель.</param>
    /// <param name="taskTypeName">Тип задачи.</param>
    private void UpdateTaskAssignees(
      Sungero.Workflow.ITask task,
      Sungero.Company.IEmployee assignedBy,
      Sungero.Company.IEmployee assignee,
      string taskTypeName)
    {
      task.Author = assignedBy;

      if (HRPro.HRDocFlow.StatementTasks.Is(task))
      {
        var statementTask = HRPro.HRDocFlow.StatementTasks.As(task);
        if (statementTask != null)
          statementTask.Author = assignedBy;
      }

      if (Sungero.RecordManagement.ActionItemExecutionTasks.Is(task))
      {
        var actionItemTask = Sungero.RecordManagement.ActionItemExecutionTasks.As(task);
        if (actionItemTask != null)
        {
          actionItemTask.Assignee = assignee;
          actionItemTask.AssignedBy = assignedBy;
        }
      }
    }

    /// <summary>
    /// Убирает документ из вложения задачи и добавляет его обратно.
    /// При последующем сохранении задачи перегенерируется предсказанная схема.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="document">Документ.</param>
    private void ReplaceTaskDocument(
      Sungero.Workflow.ITask task,
      Sungero.Docflow.IOfficialDocument document)
    {
      if (HRPro.HRDocFlow.StatementTasks.Is(task))
      {
        var statementTask = HRPro.HRDocFlow.StatementTasks.As(task);
        var statementDocument = HRPro.HRDocFlow.StatementDocuments.As(document);
        if (statementTask != null && statementDocument != null)
        {
          statementTask.DocumentGroup.All.Clear();
          statementTask.Save();
          statementTask.DocumentGroup.StatementDocuments.Add(statementDocument);
          statementTask.Save();
        }
      }

      if (Sungero.RecordManagement.ActionItemExecutionTasks.Is(task))
      {
        var actionItemTask = Sungero.RecordManagement.ActionItemExecutionTasks.As(task);
        if (actionItemTask != null)
        {
          actionItemTask.DocumentsGroup.All.Clear();
          actionItemTask.Save();
          actionItemTask.DocumentsGroup.OfficialDocuments.Add(document);
          actionItemTask.Save();
        }
      }
    }
    
    /// <summary>
    /// Получаем информацию о исполнителях из блоков заданий и уведомлений.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="assignedBy">Выдал.</param>
    /// <returns>Данные по блокам.</returns>
    private Structures.Module.ISchemeExportInfo BuildSchemeInfo(Sungero.Workflow.ITask task, Sungero.Company.IEmployee assignedBy)
    {
      var schemeInfo = Structures.Module.SchemeExportInfo.Create();
      schemeInfo.TaskId = task.Id;
      schemeInfo.Blocks = new List<Structures.Module.ISchemeBlockInfo>();

      var predictedScheme = task.PredictedScheme;
      if (predictedScheme == null)
        return schemeInfo;

      var scheme = task.Scheme;
      
      // Получаем вычисленные блоки по предполагаемому маршруту.
      var blocks = Sungero.Core.SchemeBlocks.GetAll(predictedScheme);

      // Обрабатываем только блоки типа задание и уведомление и их наследников.
      foreach (var block in blocks.Where(x => Sungero.Core.AssignmentSchemeBlocks.Is(x) || Sungero.Core.NoticeSchemeBlocks.Is(x)))
      {
        // Собираем всех исполнителей блока.
        List<string> performerNames = null;

        if (Sungero.Core.AssignmentSchemeBlocks.Is(block))
        {
          var assignmentBlock = Sungero.Core.AssignmentSchemeBlocks.As(block);
          if (assignmentBlock.Performers != null && assignmentBlock.Performers.Any())
            performerNames = GetPerformerNames(assignmentBlock.Performers, assignedBy);
        }
        else if (Sungero.Core.NoticeSchemeBlocks.Is(block))
        {
          var noticeBlock = Sungero.Core.NoticeSchemeBlocks.As(block);
          if (noticeBlock.Performers != null && noticeBlock.Performers.Any())
            performerNames = GetPerformerNames(noticeBlock.Performers, assignedBy);
        }

        // Если по блоку не удалось установить исполнителей, фиксируем блок с пустым списком.
        if (performerNames == null || performerNames.Count == 0)
          performerNames = new List<string>();

        var blockInfo = Structures.Module.SchemeBlockInfo.Create();
        blockInfo.Id = block.Id;
        blockInfo.Title = block.Title;
        blockInfo.BlockType = block.GetType().Name;
        blockInfo.Description = block.Description;
        blockInfo.PredictedDeadline = block.PredictedDeadline.Absolute;
        blockInfo.PredictedPerformers = performerNames;
        blockInfo.OutputEdges = new List<Structures.Module.ISchemeEdgeInfo>();

        if (scheme != null)
        {
          // Получаем "тот же" блок, но уже из обычной схемы, чтобы у него был доступен
          // GetOutputEdges.
          var schemeBlock = Sungero.Core.SchemeBlocks.Get(scheme, block.Id);
          if (schemeBlock != null)
          {
            foreach (var edge in scheme.GetOutputEdges(schemeBlock))
            {
              var edgeInfo = Structures.Module.SchemeEdgeInfo.Create();
              edgeInfo.Unconditional = edge.Unconditional;
              edgeInfo.Another = edge.Else;
              edgeInfo.Value = edge.Value.ToString();

              blockInfo.OutputEdges.Add(edgeInfo);
            }
          }
        }

        schemeInfo.Blocks.Add(blockInfo);
      }

      return schemeInfo;
    }

    /// <summary>
    /// Получает список имён исполнителей из перформеров блока схемы.
    /// </summary>
    /// <param name="performers">Исполнители.</param>
    /// <param name="assignedBy">Выдал.</param>
    /// <returns>Список имен.</returns>
    private List<string> GetPerformerNames(IEnumerable<Sungero.CoreEntities.IRecipient> performers, Sungero.Company.IEmployee assignedBy)
    {
      var names = new List<string>();

      foreach (var performer in performers)
      {
        // Пытаемся привести перформера к сотруднику.
        var employee = Sungero.Company.Employees.As(performer);
        if (employee == null)
        {
          // Если перформер — не сотрудник (например, роль или группа), используем его имя.
          names.Add(performer.Name);
          continue;
        }

        names.Add(GetEmployeeDisplayName(employee));
      }

      // Заменяем все вхождения "Administrator" на имя assignedBy.
      return ReplaceAdministratorNames(names, assignedBy);
    }

    /// <summary>
    /// Возвращает отображаемое имя сотрудника. Если у сотрудника на текущую дату
    /// есть активное замещение (Substitution, где сотрудник — замещаемый),
    /// к имени добавляется в скобках имя замещающего:
    /// "Иванов Иван Иванович (замещает Петров Петр Петрович)".
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Инфорация по замещению.</returns>
    private string GetEmployeeDisplayName(Sungero.Company.IEmployee employee)
    {
      var name = employee.Name;

      var user = Sungero.CoreEntities.Users.As(employee);
      if (user == null)
        return name;

      // Ищем замещение, активное на текущую дату, где сотрудник — замещаемый (User).
      // Системные замещения (IsSystem) не учитываем — по конвенции проекта.
      var substitution = Sungero.CoreEntities.Substitutions.GetAll(s =>
                                                                   Sungero.CoreEntities.Users.Equals(s.User, user) &&
                                                                   s.IsSystem != true &&
                                                                   s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active &&
                                                                   (s.StartDate == null || s.StartDate.Value <= Calendar.Today) &&
                                                                   (s.EndDate == null || s.EndDate.Value >= Calendar.Today))
        .FirstOrDefault();

      if (substitution == null || substitution.Substitute == null)
        return name;

      var substituteEmployee = Sungero.Company.Employees.As(substitution.Substitute);
      var substituteName = substituteEmployee != null ? substituteEmployee.Name : substitution.Substitute.Name;

      return $"{name} (замещает {substituteName})";
    }

    /// <summary>
    /// Заменяет все вхождения имени "Administrator" на имя сотрудника assignedBy.
    /// </summary>
    /// <param name="names">Имена.</param>
    /// <param name="assignedBy">Выдал.</param>
    /// <returns>Имена.</returns>
    private List<string> ReplaceAdministratorNames(List<string> names, Sungero.Company.IEmployee assignedBy)
    {
      var assignedByName = assignedBy != null ? assignedBy.Name : null;
      if (assignedByName == null)
        return names;

      for (int i = 0; i < names.Count; i++)
      {
        if (names[i] == "Administrator" || names[i] == "Service User" || names[i] == "Integration Service")
          names[i] = assignedByName;
      }

      return names;
    }

    /// <summary>
    /// Возвращает виды документов поддерживаемых типов:
    /// </summary>
    [Remote(IsPure = true)]
    public IQueryable<Sungero.Docflow.IDocumentKind> GetSupportedDocumentKinds()
    {
      var vacationStatementDocument = Constants.Module.PredctedSchemeReport.DocumentTypeGuid.VacationStatementDocument;
      var lrdPaymentStatementDocument = Constants.Module.PredctedSchemeReport.DocumentTypeGuid.LrdPaymentStatementDocument;
      var weekendWorkStatementDocument = Constants.Module.PredctedSchemeReport.DocumentTypeGuid.WeekendWorkStatementDocument;
      
      return Sungero.Docflow.DocumentKinds.GetAll(k => k.Status == Sungero.Docflow.DocumentKind.Status.Active &&
                                                  k.DocumentType != null &&
                                                  (k.DocumentType.DocumentTypeGuid == vacationStatementDocument ||
                                                   k.DocumentType.DocumentTypeGuid == lrdPaymentStatementDocument ||
                                                   k.DocumentType.DocumentTypeGuid == weekendWorkStatementDocument));
    }
    
    /// <summary>
    /// Возвращает разделы персон.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Разделы персон.</returns>
    [Remote(IsPure = true)]
    public IQueryable<NNCommons.IPersonnelSection> GetFilteredPersonalSection(Sungero.Company.IBusinessUnit businessUnit)
    {
      return NNCommons.PersonnelSections.GetAll(p => Equals(p.BusinessUnit, businessUnit));
    }

    /// <summary>
    /// Фильтрация сотрудников для диалога.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <param name="personnelSections">Раздел персонала.</param>
    /// <returns>Сотрудники.</returns>
    [Remote(IsPure = true)]
    public IQueryable<OverrideSolution.IEmployee> GetFilteredEmployees(
      Sungero.Company.IBusinessUnit businessUnit,
      List<long> personnelSectionIds)
    {
      var employees = OverrideSolution.Employees.GetAll(e => e.Status == OverrideSolution.Employee.Status.Active);
      
      if (businessUnit == null)
        return employees.Where(e => false);

      if (personnelSectionIds.Any())
      {
        return employees.Where(e => e.PersonnelSectionDirRX != null
                               && personnelSectionIds.Contains(e.PersonnelSectionDirRX.Id));
      }


      return employees.Where(e => OverrideSolution.BusinessUnits.Equals(e.BusinessUnit, businessUnit));
    }
    
    /// <summary>
    /// Фильтрация сотрудников для диалога.
    /// </summary>
    /// <param name="businessUnit">Наша организация.</param>
    /// <param name="personnelSections">Раздел персонала.</param>
    /// <returns>Сотрудники.</returns>
    [Remote(IsPure = true)]
    public List<long> GetFilteredEmployeeIds(
      Sungero.Company.IBusinessUnit businessUnit,
      List<long> personnelSectionIds)
    {
      return GetFilteredEmployees(businessUnit, personnelSectionIds).Select(c => c.Id).ToList();
    }
    
    /// <summary>
    /// Возвращает имя типа документа по сущности вида документа.
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <returns>Тип документа.</returns>
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
    
    /// <summary>
    /// Подготовка данных для формирования тестовых задач и документов.
    /// </summary>
    /// <param name="taskTypeName">Тип задачи.</param>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="organization">Наша организация.</param>
    /// <param name="employees">Сотрудники.</param>
    /// <returns>Данные для формирования задачи и документа.</returns>
    public List<Structures.Module.IExportPredictedSchemeRequest> BuildRequests(
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

      foreach (var emp in employees)
      {
        var request = Structures.Module.ExportPredictedSchemeRequest.Create();
        request.AssignedById = emp.Id; // INFO: Учитывать при доработке логики, если потребуется добавить в диалог дополнительное поле для инициатора и исполнителя.
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
    /// Создает АО для формирования отчета.
    /// </summary>
    /// <param name="taskTypeName">Тип задачи.</param>
    /// <param name="documentKind">Вид документа.</param>
    /// <param name="organization">Наша организация.</param>
    /// <param name="employees">Сотрудники.</param>
    [Remote]
    public void CreatePredctedSchemeReport(string taskTypeName,
                                           string taskTypeLocalizeName,
                                           Sungero.Docflow.IDocumentKind documentKind,
                                           Sungero.Company.IBusinessUnit organization,
                                           List<long> employees)
    {

      var batchSize = NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.PredctedSchemeReportSizeBatch.Guid);
      
      if (batchSize == null || batchSize == 0)
      {
        Logger.ErrorFormat("Установите значение в константе Размер обрабатываемых сотрудников");
        return;
      }
      
      var totalBatches = (int)Math.Ceiling(employees.Count / (double)batchSize.Value);

      // Обрабатываем порцонно сотрудников, чтобы не выгружать в память большое кол-во сущностей.
      for (int i = 0; i < employees.Count; i += batchSize.Value)
      {
        var batch = employees.Skip(i).Take(batchSize.Value).ToList();
        var batchNumber = (i / batchSize.Value) + 1;

        Logger.DebugFormat(
          "CreatePredctedSchemeReport. Запуск АО. Блок {0}/{1}, записей: {2}.",
          batchNumber,
          totalBatches,
          batch.Count);

        // для определения, что отчет сформируется только 1. Исходя из этого в АО скорректируется наименование отчета.
        if (batchSize >= employees.Count)
          batchNumber = 0;
        
        var asyncHandler = DirRX.NNCommons.AsyncHandlers.CreatePredctedSchemeReportAsync.Create();
        asyncHandler.CurrentUserId = Users.Current != null ? Users.Current.Id : 0;
        asyncHandler.BusinessUnitId = organization.Id;
        asyncHandler.EmployeeIds = string.Join(",", batch);
        asyncHandler.DocumentKindId = documentKind.Id;
        asyncHandler.TaskTypeName = taskTypeName;
        asyncHandler.NumberReport = batchNumber;
        asyncHandler.TaskTypeLocalizeName = taskTypeLocalizeName;
        asyncHandler.ExecuteAsync();
      }

      Logger.DebugFormat(
        "CreatePredctedSchemeReport. Итого запущено АО: {0}.",
        totalBatches);
    }

    /// <summary>
    /// Формирует строки маршрута согласования (этапы с исполнителями) и пропущенных этапов (без исполнителей).
    /// </summary>
    /// <param name="itemResult">Данные по схеме.</param>
    private void BuildRouteStrings(Structures.Module.IProcessKindSchemeResult itemResult)
    {
      if (itemResult.Scheme == null || itemResult.Scheme.Blocks == null || itemResult.Scheme.Blocks.Count == 0)
      {
        itemResult.ApprovalRoute = "";
        itemResult.MissingSteps = "";
        return;
      }

      var approvalLines = new List<string>();
      var missingLines = new List<string>();

      // формируем строки для отчета в формате Название блока - Исполнители.
      // если исполнителя нет, то просто название блока.
      foreach (var block in itemResult.Scheme.Blocks)
      {
        var title = block.Title ?? "";
        if (block.PredictedPerformers != null && block.PredictedPerformers.Count > 0)
        {
          approvalLines.Add($"{title} — {string.Join(", ", block.PredictedPerformers)}");
        }
        else
        {
          missingLines.Add(title);
        }
      }

      itemResult.ApprovalRoute = approvalLines.Count > 0 ? string.Join("\n", approvalLines) : string.Empty;
      itemResult.MissingSteps = missingLines.Count > 0 ? string.Join("\n", missingLines) : string.Empty;
    }

    #endregion
    
    /// <summary>
    /// Парсит строку c идентификаторами в список идентификаторов.
    /// </summary>
    /// <param name="input">Строка с идентификаторами.</param>
    /// <returns>Список с идентификаторами.</returns>
    [Public]
    public List<long> ParseStringToListLong(string input)
    {
      if (!string.IsNullOrEmpty(input))
        return input.Trim().Split(',').Select(c => long.Parse(c)).ToList();
      
      return new List<long>();
    }
    
    /// <summary>
    /// Возвращает Нашe организации у сотрудника
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>НОР.</returns>
    [Public]
    public OverrideSolution.IBusinessUnit GetBusinessUnitEmployee(OverrideSolution.IEmployee employee)
    {
      if (employee != null)
        return OverrideSolution.BusinessUnits.As(employee.BusinessUnit);
      
      return null;
    }
    
    /// <summary>
    /// Возвращает Нашe организацию у сотрудника.
    /// </summary>
    /// <param name="employee">Ид сотрудника.</param>
    /// <returns>НОР.</returns>
    [Public]
    public OverrideSolution.IBusinessUnit GetBusinessUnitEmployee(long id)
    {
      var employee = OverrideSolution.Employees.GetAll(e => e.Id == id).FirstOrDefault();
      return GetBusinessUnitEmployee(employee);
    }
    
    /// <summary>
    /// Создает и отправляет простую задачу.
    /// </summary>
    /// <param name="performer">Исполнитель.</param>
    /// <param name="subject">Тема.</param>
    /// <param name="activeText">Текст задачи.</param>
    [Public]
    public void SendSimpleTask(IGroup performer, string subject, string activeText)
    {
      var task = Sungero.Workflow.SimpleTasks.Create(subject, performer);
      task.ActiveText = activeText;
      
      task.Start();
    }
    
    /// <summary>
    /// Запустить задачи на выпуск сертификатов.
    /// </summary>
    [Remote]
    public void MassIssueCertificates()
    {
      var employeesIds = OverrideSolution.Employees.GetAll(x => x.Status == OverrideSolution.Employee.Status.Active &&
                                                           x.ConsentHRPro == OverrideSolution.Employee.ConsentHRPro.Signed &&
                                                           x.EmploymentType == OverrideSolution.Employee.EmploymentType.MainPlace).Select(x => x.Id);
      var async = AsyncHandlers.MassIssueCertificates.Create();
      async.EmployeesIds = string.Join(Constants.Module.Separator.ToString(), employeesIds.Select(id => id.ToString()));
      async.ExecuteAsync(DirRX.NNCommons.Resources.MassIssueCertificatesStarted, DirRX.NNCommons.Resources.MassIssueCertificatesCompleted);
    }
    
    /// <summary>
    /// Закрыть сертификаты ПЭП.
    /// </summary>
    [Remote]
    public void DisableSimpleCertificates()
    {
      var issueConst = Functions.Module.GetStringValue(Constants.Module.ConstantsSetting.IssuerNames.Guid);
      if (string.IsNullOrEmpty(issueConst))
      {
        Logger.DebugFormat("DisableSimpleCertificates. Прикладная константа '{1}' не заполнена.", Constants.Module.ConstantsSetting.IssuerNames.Name);
        return;
      }
      var issueNames = issueConst.Trim().Split(Constants.Module.Separator);
      var certificatesIds = Certificates.GetAll(x => x.Enabled == true && issueNames.Contains(x.Issuer)).Select(x => x.Id);
      var async = AsyncHandlers.DisableSimpleCertificates.Create();
      async.CertificatesIds = string.Join(Constants.Module.Separator.ToString(), certificatesIds.Select(id => id.ToString()));
      async.ExecuteAsync(DirRX.NNCommons.Resources.DisableSimpleCertificatesStarted, DirRX.NNCommons.Resources.DisableSimpleCertificatesCompleted);
    }
    
    /// <summary>
    /// Прибавить число к указанному.
    /// </summary>
    /// <param name="firstArgument">Первый аргумент.</param>
    /// <param name="secondArgument">Второй аргумент.</param>
    /// <returns>Сумма аргументов.</returns>
    [ExpressionElement("ExpFunc_Add_Name", "ExpFunc_Add_Description", "", "ExpFunc_Add_SecondArgument"), Obsolete("Используйте коробочную функцию \"Увеличить на\".")]
    public int Add(int firstArgument, int secondArgument)
    {
      return firstArgument + secondArgument;
    }
    
    /// <summary>
    /// Получить ссылки на активные задания в ЛК по задаче.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="performer">Исполнитель задания.</param>
    /// <returns>Ссылки на активные задания в ЛК.</returns>
    [ExpressionElement("ExpFunc_GetHyperLinkToAssignmentsInSelfOffice_Name", "ExpFunc_GetHyperLinkToAssignmentsInSelfOffice_Desc", "", "ExpFunc_GetHyperLinkToAssignmentsInSelfOffice_Param_Performer")]
    public static string GetHyperLinkToAssignmentsInSelfOffice(Sungero.Workflow.ITask task, IUser performer)
    {
      var assignments = Sungero.Workflow.Assignments.GetAll(a => Equals(a.Task, task) && Equals(a.Performer, performer) &&
                                                            a.Status == Sungero.Workflow.Assignment.Status.InProcess);
      var hyperLinks = new List<string>();
      foreach (var assignment in assignments)
      {
        var assignmentMetadata = assignment.GetEntityMetadata();
        if (!assignmentMetadata.CanBeUsedInIntegration)
          continue;
        
        var objectTypeName = string.Format("I{0}s", assignmentMetadata.IntegrationServiceName);
        var hyperLink = DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.CreateHyperLinkToSelfOffice(HRPro.HRProSolution.PublicConstants.Module.TargetNameSpace,
                                                                                                                     EssPlatform.PublicConstants.Module.SelfOfficeObjectTypes.WorkItems,
                                                                                                                     objectTypeName, assignment.Id.ToString());
        hyperLinks.Add(string.Format("<a href=\"{0}\">{1}</a>", hyperLink, assignment.Subject));
      }
      return string.Join("<br>", hyperLinks);
    }
    
    #region Построение отчёта SLA

    /// <summary>
    /// Определить маппинг колонок SQL -> Excel.
    /// </summary>
    /// <returns>Таблица маппинга.</returns>
    /// <remarks>Ключ - номер колонки в Excel, значение - название столбца в результирующей выборке SQL запроса.</remarks>
    public static System.Collections.Generic.Dictionary<int, string> GetMappingReportSLA()
    {
      var processingTimeIndex = Constants.Module.ReportSLA.ProcessingTimeIndex;
      var exceedingDeadlineIndex = Constants.Module.ReportSLA.ExceedingDeadlineIndex;
      
      // Ключ - номер колонки в Excel, значение - название столбца в результирующей выборке SQL запроса.
      var columnMapping = new Dictionary<int, string>
      {
        // Для колонок "Время обработки" и "Превышение срока" реализуется своя логика маппинга при обработке результирующей выборки SQL запроса.
        {0, "docKindName"},
        {1, "taskSubject"},
        {2, "taskHyperlink"},
        {3, "authorName"},
        {4, "authorPersNumber"},
        {5, "taskCreated"},
        {6, "taskStatus"},
        {7, "employeeName"},
        {8, "employeePersNumber"},
        {9, "performerName"},
        {10, "performerPersNumber"},
        {11, "assignmentCreated"},
        {12, "assignmentDeadline"},
        {13, "assignmentCompleted"},
        {processingTimeIndex, ""},
        {exceedingDeadlineIndex, ""},
        {16, "assignmentResult"},
        {17, "assignmentText"},
        {18, "assignmentSubject"},
        {19, "assignmentHyperlink"}
      };
      
      return columnMapping;
    }

    /// <summary>
    /// Получение даты истечения сертификата.
    /// </summary>
    /// <param name="signatureInfo">Информация о подписи.</param>
    /// <returns>Дата истечения сертификата.</returns>
    [Public]
    public static DateTime GetCertificateExpirationDateInSignature(Sungero.Core.ISignatureInfo signatureInfo)
    {
      var expirationDate = Calendar.SqlMinValue;
      switch (signatureInfo.SignatureFormat)
      {
        case SignatureFormat.CadesBes:
          {
            var cadesBesInfo = signatureInfo.AsCadesBesSignatureInfo();
            expirationDate = cadesBesInfo.CertificateInfo.NotAfter;
            break;
          }
        case SignatureFormat.CadesT:
          {
            var cadesTInfo = signatureInfo.AsCadesBesSignatureInfo();
            expirationDate = cadesTInfo.CertificateInfo.NotAfter;
            break;
          }
        case SignatureFormat.CadesXL:
          {
            var cadesXLInfo = signatureInfo.AsCadesXLSignatureInfo();
            expirationDate = cadesXLInfo.EvidenceTimestamp.Certificate.NotAfter;
            break;
          }
        case SignatureFormat.CadesA:
          {
            var cadesAInfo = signatureInfo.AsCadesASignatureInfo();
            expirationDate = cadesAInfo.ArchiveTimestamps.Last().Certificate.NotAfter;
            break;
          }
      }
      return expirationDate;
    }
    
    /// <summary>
    /// Сформировать отчет SLA.
    /// </summary>
    /// <param name="documentKindsIds">ИД видов документов.</param>
    /// <param name="businessUnitsIds">ИД наших организаций.</param>
    /// <param name="employeesIds">ИД сотрудников.</param>
    /// <param name="performersIds">ИД исполнителей заданий.</param>
    /// <param name="beginDate">Период с.</param>
    /// <param name="endDate">Период по.</param>
    /// <param name="initiator">Сотрудник, формирующий отчет.</param>
    /// <param name="assignmentType">Тип заданий.</param>
    /// <returns>Путь к файлу отчета.</returns>
    // TODO: Seleznyov_DV. По хорошему нужно вынести работу с Aspose в изолированную область.
    [Public, Remote]
    public static string GenerateReportSLA(List<long> documentKindsIds, List<long> businessUnitsIds, List<long> employeesIds, List<long> performersIds,
                                           DateTime beginDate, DateTime endDate, OverrideSolution.IEmployee initiator, string assignmentType)
    {
      Logger.DebugFormat("GenerateReportSLA. Начало построения отчета.");
      var documentPath = string.Empty;
      var isManualHoldingEvent = assignmentType == Resources.ReportSLADialogAssignmentTypeEvent;
      var docKindsInfo = Functions.DocumentsVisabilityLimitation.GetAvailableDocumentKindsInfo(initiator)
        .Where(x => documentKindsIds.Contains(x.DocumentKindId))
        .Select(x => string.Format("({0}, {1})", x.DocumentKindId, x.isTop));
      var employeesIdsForReadAccess = Substitutions.ActiveSubstitutedUsers.Select(x => x.Id).ToList();
      employeesIdsForReadAccess.Add(initiator.Id);
      
      // Получить доступные разделы персонала для сотрудника.
      var currentEmployee = DirRX.OverrideSolution.Employees.Current;
      var availiableSectionsIds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation.GetAvailablePersonnelSections(currentEmployee, businessUnitsIds);
      
      var businessUnitsWithoutGeneralLimitationIds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
        .GetBusinessUnitsWithoutGeneralLimitation(currentEmployee, businessUnitsIds);
      
      var businessUnitsWithGeneralLimitation = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
        .GetBusinessUnitsWithGeneralLimitation(currentEmployee, businessUnitsIds);
      
      // Для формирования гиперссылок получить хост.
      var task = Sungero.Workflow.Tasks.GetAll().FirstOrDefault();
      if (task == null)
        throw new AppliedCodeException(DirRX.NNCommons.Resources.ReportSLANoTasksError);
      
      var hostName = Hyperlinks.Get(task);
      hostName = hostName.Substring(0, hostName.IndexOf('?'));
      var templateId = Functions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.ReportSLATemplateId.Guid).GetValueOrDefault();
      if (templateId == 0)
        throw new AppliedCodeException(DirRX.NNCommons.Resources.ReportSLATemplateError);
      
      var reportTemplate = Sungero.Content.ElectronicDocuments.GetAll(t => t.Id == templateId).FirstOrDefault();
      var templateVersion = reportTemplate.LastVersion;
      var memoryStream = new System.IO.MemoryStream ();
      templateVersion.Body.Read().CopyTo(memoryStream);
      var excelDocument = new Aspose.Cells.Workbook(memoryStream);
      var workSheet = excelDocument.Worksheets.FirstOrDefault();
      
      // Заполнить дату формирования и ФИО сформировавшего отчет сотрудника.
      workSheet.Cells[1, 1].Value = Calendar.UserNow.ToString("g");
      workSheet.Cells[2, 1].Value = initiator.Name;
      
      // Определим маппинг колонок SQL -> Excel.
      var columnMapping = GetMappingReportSLA();
      
      // Координаты для вставки данных в Excel.
      var firstRowIndex = Constants.Module.ReportSLA.FirstRowIndex;
      var firstColumnIndex = Constants.Module.ReportSLA.FirstColumnIndex;
      
      // Индекс последней колонки в Excel для вставки данных.
      var lastColumnIndex = firstColumnIndex + columnMapping.Count;
      var rowIndex = firstRowIndex;
      
      var processingTimeIndex = Constants.Module.ReportSLA.ProcessingTimeIndex;
      var exceedingDeadlineIndex = Constants.Module.ReportSLA.ExceedingDeadlineIndex;
      
      // Учёт смещения часового пояса пользователя относительно веб сервера.
      var timeZoneOffset = Users.UtcOffsetOfCurrent - Calendar.UtcOffset;
      var offsetHours = (int)timeZoneOffset.TotalHours;
      
      using (var connection = SQL.CreateConnection())
      {
        using (var command = connection.CreateCommand())
        {
          command.CommandText = string.Format(Queries.Module.SelectDataForSLA, hostName,
                                              string.Join(Constants.Module.Separator.ToString(), employeesIdsForReadAccess),
                                              string.Join(Constants.Module.Separator.ToString(), docKindsInfo),
                                              string.Join(Constants.Module.Separator.ToString(), businessUnitsIds),
                                              string.Join(Constants.Module.Separator.ToString(), employeesIds),
                                              string.Join(Constants.Module.Separator.ToString(), performersIds),
                                              beginDate.ToString(Constants.Module.SQLDateTimeFormat),
                                              endDate.EndOfDay().ToString(Constants.Module.SQLDateTimeFormat),
                                              isManualHoldingEvent, offsetHours,
                                              string.Join(Constants.Module.Separator.ToString(), availiableSectionsIds),
                                              string.Join(Constants.Module.Separator.ToString(), businessUnitsWithoutGeneralLimitationIds),
                                              string.Join(Constants.Module.Separator.ToString(), businessUnitsWithGeneralLimitation));
          Logger.DebugFormat("GenerateReportSLA. Запрос на получение данных: {0}", command.CommandText);
          using(var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              for (int columnIndex = firstColumnIndex; columnIndex < lastColumnIndex; columnIndex++)
              {
                // Номер колонки в excel в таблице с данными.
                var columnNumber = columnIndex - firstColumnIndex;
                
                // Колонка "Время обработки".
                if (columnNumber == processingTimeIndex)
                {
                  var created = reader["processingTimeStart"] is DBNull ? default(DateTime) : (DateTime)reader["processingTimeStart"];
                  var completed = reader["processingTimeEnd"] is DBNull ? default(DateTime) : (DateTime)reader["processingTimeEnd"];
                  if (created != default(DateTime) && completed != default(DateTime))
                  {
                    var processingTime = WorkingTime.GetDurationInWorkingHours(created, completed);
                    workSheet.Cells[rowIndex, columnIndex].Value = processingTime >= 0 ? processingTime : 0;
                  }
                  else
                    workSheet.Cells[rowIndex, columnIndex].Value = 0;
                }
                // Колонка "Превышение срока".
                else if (columnNumber == exceedingDeadlineIndex)
                {
                  var deadline = reader["exceedingDeadlineStart"] is DBNull ? default(DateTime) : (DateTime)reader["exceedingDeadlineStart"];
                  var completed = reader["exceedingDeadlineEnd"] is DBNull ? default(DateTime) : (DateTime)reader["exceedingDeadlineEnd"];
                  if (deadline != default(DateTime) && completed != default(DateTime))
                  {
                    var exceedingDeadline = WorkingTime.GetDurationInWorkingHours(deadline, completed);
                    workSheet.Cells[rowIndex, columnIndex].Value = exceedingDeadline > 0 ? exceedingDeadline : 0;
                  }
                  else
                    workSheet.Cells[rowIndex, columnIndex].Value = 0;
                }
                // Остальные колонки. Заполняются по маппингу.
                else if (columnMapping.ContainsKey(columnNumber))
                {
                  var columnName = columnMapping[columnNumber];
                  workSheet.Cells[rowIndex, columnIndex].Value = reader[columnName] is DBNull ? null : reader[columnName];
                }
              }
              rowIndex++;
            }
          }
        }
      };
      
      if (rowIndex > firstRowIndex)
      {
        workSheet.AutoFitColumns();
        documentPath = string.Format("{0}{1}.{2}", System.IO.Path.GetTempPath(), "SLA", HRPro.HRDocFlow.PublicConstants.Module.ExcelFormatXLSX);
        excelDocument.Save(documentPath);
      }
      memoryStream.Flush();
      Logger.DebugFormat("GenerateReportSLA. Конец построения отчета.");
      return documentPath;
    }
    
    #endregion

    /// <summary>
    /// Сформировать отчет "Реестр кадровых документов".
    /// </summary>
    /// <param name="documentKindsIds">ИД видов документов.</param>
    /// <param name="businessUnitsIds">ИД наших организаций.</param>
    /// <param name="employeesIds">ИД сотрудников.</param>
    /// <param name="beginDate">Дата создания документа с.</param>
    /// <param name="endDate">Дата создания документа по.</param>
    /// <param name="initiator">Сотрудник, формирующий отчет.</param>
    /// <returns>Путь к файлу отчета.</returns>
    // TODO: Seleznyov_DV. По хорошему нужно вынести работу с Aspose в изолированную область.
    [Public, Remote]
    public static string GeneratePeronnelDocumentsRegister(List<long> documentKindsIds, List<long> businessUnitsIds, List<long> employeesIds, DateTime beginDate, DateTime endDate, OverrideSolution.IEmployee initiator)
    {
      var documentPath = string.Empty;
      var docKindsInfo = Functions.DocumentsVisabilityLimitation.GetAvailableDocumentKindsInfo(initiator)
        .Where(x => documentKindsIds.Contains(x.DocumentKindId))
        .Select(x => string.Format("({0}, {1})", x.DocumentKindId, x.isTop));
      
      // Для формирования гиперссылок получить хост.
      var task = Sungero.Workflow.Tasks.GetAll().FirstOrDefault();
      if (task == null)
        throw new AppliedCodeException(DirRX.NNCommons.Resources.ReportSLANoTasksError);
      
      var hostName = Hyperlinks.Get(task);
      hostName = hostName.Substring(0, hostName.IndexOf('?'));
      // Получить шаблон реестра кадровых документов.
      var templateId = Functions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.PersonelDocumentsRegisterReportTemplateId.Guid).Value;
      if (templateId == (long)0)
        throw new AppliedCodeException(Resources.PersonnelDocumentsRegisterTemplateError);
      
      var reportTemplate = Sungero.Content.ElectronicDocuments.GetAll(t => t.Id == templateId).FirstOrDefault();
      var templateVersion = reportTemplate.LastVersion;
      var memoryStream = new System.IO.MemoryStream ();
      templateVersion.Body.Read().CopyTo(memoryStream);
      var excelDocument = new Aspose.Cells.Workbook(memoryStream);
      var workSheet = excelDocument.Worksheets.FirstOrDefault();
      
      // Получить доступные разделы персонала для сотрудника.
      var currentEmployee = DirRX.OverrideSolution.Employees.Current;
      var availiableSectionsIds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation.GetAvailablePersonnelSections(currentEmployee, businessUnitsIds);
      
      var businessUnitsWithoutGeneralLimitationIds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
        .GetBusinessUnitsWithoutGeneralLimitation(currentEmployee, businessUnitsIds);
      
      var businessUnitsWithGeneralLimitation = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
        .GetBusinessUnitsWithGeneralLimitation(currentEmployee, businessUnitsIds);
      
      // Заполнить дату формирования и ФИО сформировавшего отчет сотрудника.
      workSheet.Cells[1, 1].Value = string.Format("{0} {1}", Calendar.UserNow.ToShortDateString(), Calendar.UserNow.ToShortTimeString());
      workSheet.Cells[2, 1].Value = initiator.Name;
      var firstRowIndex = Constants.Module.PersonnelDocumentRegisterReport.FirstRowIndex;
      var rowIndex = firstRowIndex;
      var lastColumnIndex = Constants.Module.PersonnelDocumentRegisterReport.ColumnsCount;
      
      // Учёт смещения часового пояса пользователя относительно веб сервера.
      var timeZoneOffset = Users.UtcOffsetOfCurrent - Calendar.UtcOffset;
      var offsetHours = (int)timeZoneOffset.TotalHours;
      
      // Получить данные для отчета и записать их в Excel-документ.
      using (var connection = SQL.CreateConnection())
      {
        using (var command = connection.CreateCommand())
        {
          command.CommandText = string.Format(Queries.Module.PersonnelDocumentsRegister, hostName,
                                              string.Join(NNCommons.PublicConstants.Module.Separator.ToString(), businessUnitsIds),
                                              string.Join(NNCommons.PublicConstants.Module.Separator.ToString(), docKindsInfo),
                                              string.Join(NNCommons.PublicConstants.Module.Separator.ToString(), employeesIds),
                                              beginDate.ToString(Constants.Module.SQLDateTimeFormat),
                                              endDate.EndOfDay().ToString(Constants.Module.SQLDateTimeFormat),
                                              offsetHours,
                                              string.Join(Constants.Module.Separator.ToString(), availiableSectionsIds),
                                              string.Join(Constants.Module.Separator.ToString(), businessUnitsWithoutGeneralLimitationIds),
                                              string.Join(Constants.Module.Separator.ToString(), businessUnitsWithGeneralLimitation));
          
          using(var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              var columnIndex = 0;
              while (columnIndex < lastColumnIndex)
              {
                // Преобразования к часовому поясу пользователя.
                workSheet.Cells[rowIndex, columnIndex].Value = reader[columnIndex];
                columnIndex++;
              }
              rowIndex++;
            }
          }
        };
      }
      // Установка стилей для таблицы если найдены данные.
      if (rowIndex > Constants.Module.PersonnelDocumentRegisterReport.FirstRowIndex)
      {
        var cellStyle = excelDocument.CreateStyle();
        cellStyle.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
        cellStyle.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
        cellStyle.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
        cellStyle.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
        var range = workSheet.Cells.CreateRange(firstRowIndex, 0, rowIndex - firstRowIndex, lastColumnIndex);
        range.SetStyle(cellStyle);
        workSheet.AutoFitColumns();
        documentPath = string.Format("{0}{1}.{2}", System.IO.Path.GetTempPath(), Resources.PersonnelDocumentsRegisterReportTitle, HRPro.HRDocFlow.PublicConstants.Module.ExcelFormatXLSX);
        excelDocument.Save(documentPath);
      }
      memoryStream.Flush();
      return documentPath;
    }

    /// <summary>
    /// Получить список сотрудников.
    /// </summary>
    /// <param name="businessUnits">Список НОР.</param>
    /// <returns>Список сотрудников.</returns>
    [Public, Remote(IsPure = true)]
    public IQueryable<DirRX.OverrideSolution.IEmployee> GetAvailableEmployees(List<DirRX.OverrideSolution.IBusinessUnit> businessUnits)
    {
      var businessUnitIds = businessUnits.Select(b => b.Id);
      return OverrideSolution.Employees.GetAll(emp => emp.BusinessUnit != null && businessUnitIds.Contains(emp.BusinessUnit.Id));
    }
    
    /// <summary>
    /// Получить список исполнителей.
    /// </summary>
    /// <returns>Список сотрудников.</returns>
    [Public, Remote(IsPure = true)]
    public IQueryable<DirRX.OverrideSolution.IEmployee> GetAvailablePerformers()
    {
      return OverrideSolution.Employees.GetAll(emp => emp.BusinessUnit != null);
    }

    /// <summary>
    /// Сравнить две версии документа.
    /// </summary>
    /// <param name="docID">Ид документа.</param>
    /// <param name="ver1Id">Выбранная версия.</param>
    /// <param name="ver2Id">Версия для сравнения.</param>
    /// <param name="author">Автор.</param>
    /// <returns>Документ с результатами сравнения, не сохраненный в БД.</returns>
    [Public, Remote]
    public IOfficialDocument CompareVersionsDocument(long docID, int selectedVesrionId, int comparingVersionId, string author)
    {
      Logger.DebugFormat("CompareVersionsDocument. Ид документа: {0}. Выбранная версия: {1}. Версия для сравнения: {2}.", docID, selectedVesrionId, comparingVersionId);
      
      var resultDoc = SimpleDocuments.Create();
      
      // Получить тело документа.
      var sourceDocument = DirRX.OverrideSolution.DocRelatedToWorkBases.Get(docID);
      var ver1 = sourceDocument.Versions.Where(x => x.Number == comparingVersionId).FirstOrDefault();
      var ver2 = sourceDocument.Versions.Where(x => x.Number == selectedVesrionId).FirstOrDefault();
      
      var ver1Stream = new System.IO.MemoryStream();
      ver1.Body.Read().CopyTo(ver1Stream);
      
      var ver2Stream = new System.IO.MemoryStream();
      ver2.Body.Read().CopyTo(ver2Stream);
      
      var extension = NNCommons.PublicConstants.Module.ExtensionsForComparison.Pdf;
      var streamResult = NNCommons.IsolatedFunctions.ComparingDocumentVersions.GetResultComparingVersions(ver1Stream, ver2Stream, author, Calendar.Now);

      var docResult =  HRPro.HRCommons.IsolatedFunctions.PdfConverter.GeneratePdfA(streamResult, NNCommons.PublicConstants.Module.ExtensionsForComparison.Docx, NNCommons.PublicConstants.Module.PdfAType.V1A);
      
      if(string.IsNullOrEmpty(docResult.ExeptionCode) && docResult.DocStream != null)
      {
        resultDoc.CreateVersion();
        var lastVersion = resultDoc.LastVersion;
        lastVersion.PublicBody.Write(docResult.DocStream);
        lastVersion.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(NNCommons.PublicConstants.Module.ExtensionsForComparison.Pdf);
        resultDoc.Name = DirRX.NNCommons.Resources.CompareVersionsDocumentResult;
      }
      
      return resultDoc;
    }

    /// <summary>
    /// Добавить календарные дни.
    /// </summary>
    /// <param name="date">Дата.</param>
    /// <param name="days">Календарные дни.</param>
    /// <returns>Новая дата.</returns>
    [ExpressionElement("ExpFunc_AddCalendarDays_Name", "ExpFunc_AddCalendarDays_Desc"), Obsolete("Используйте коробочные функции \"Добавить дни\" и \"Вычесть дни\".")]
    public static DateTime AddCalendarDays(DateTime date, int days)
    {
      return date.AddDays(days);
    }

    #region Функции для работы с ЛК.

    #region Виджет "Отправленные заявления" в ЛК.

    /// <summary>
    /// Сборка данных для виджета "Отправленные заявления".
    /// </summary>
    /// <param name="userId">Ид сотрудника.</param>
    /// <param name="isVacation">Заявление по отпуску?.</param>
    /// <returns>Данные для виджета "Отправленные заявления".</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetNNStatementTasksDataForWidget(long userId, bool isVacation)
    {
      var employee = OverrideSolution.Employees.GetAll(d => d.Id == userId).FirstOrDefault();
      if (employee == null)
        return "[]";
      
      var personEmployeeIds = DirRX.EssPlatform.PublicFunctions.Module.GetPersonEmployeeIds(employee.Person.Id);
      var taskDataList = new List<Structures.Module.ITaskDataForWidget>();
      var dayYearAgo = Calendar.Today.AddYears(-1);
      var tasks = Sungero.Workflow.Tasks.GetAll().Where(t => t.Status != Sungero.Workflow.Task.Status.Draft && personEmployeeIds.Contains(t.Author.Id) &&
                                                        (t.Created.HasValue && t.Created.Value >= dayYearAgo));
      if (isVacation)
        tasks = tasks.Where(t => OverrideSolution.StatementTasks.Is(t) && t.AttachmentDetails.Any(a => a.AttachmentTypeGuid == HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument));
      else
        tasks = tasks.Where(t => NNOtherProcesses.WeekendWorkTasks.Is(t) ||
                            (OverrideSolution.StatementTasks.Is(t) && !t.AttachmentDetails.Any(a => a.AttachmentTypeGuid == HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument)));
      foreach (var task in tasks)
      {
        var taskData = Structures.Module.TaskDataForWidget.Create();
        taskData.Id = task.Id;
        taskData.ProcessName = string.Format("I{0}s", task.GetEntityMetadata().IntegrationServiceName);
        taskData.Subject = task.Subject;
        taskData.Created = task.Created.Value;
        var state = HRPro.HRCommons.PublicFunctions.Module.GetTaskExecutionState(task);
        taskData.Status = state != null ? state.Name : task.Status.Value.ToString();
        taskDataList.Add(taskData);
      }
      return IsolatedFunctions.Json.SerializeInJson(taskDataList.OrderByDescending(t => t.Created).ToList());
    }

    #endregion
    
    /// <summary>
    /// Проверяет, что сотрудник работает по основному месту работы.
    /// </summary>
    /// <param name="employeeId">Ид сотрудника.</param>
    /// <returns>Текст ошибки, если работает не по основному месту работы, иначе пустая строка.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string CheckEmploymentTypeInitiator(long employeeId)
    {
      Logger.DebugFormat("CheckEmploymentTypeInitiator. EmployeeId: {0}", employeeId);
      
      var employee = OverrideSolution.Employees.GetAll(e => e.Id == employeeId).FirstOrDefault();
      
      if (employee != null)
      {
        if (employee.EmploymentType != OverrideSolution.Employee.EmploymentType.MainPlace)
        {
          var errorMessage = NNCommons.PublicFunctions.Module.GetStringValue(NNOtherProcesses.PublicConstants.Module.ConstantsSetting.RegistrationStatementMainPlace.Guid);

          if (!string.IsNullOrEmpty(errorMessage))
            return errorMessage;
          else
          {
            Logger.Debug("CheckEmploymentTypeInitiator. Константа не заполнена. Использовано значение по умолчанию");
            return NNOtherProcesses.PublicConstants.Module.ConstantsDefaultValues.RegistrationStatementMainPlace;
          }
        }
      }
      else
        Logger.DebugFormat("CheckEmploymentTypeInitiator. Сотрудник с ИД: {0} не найден", employeeId);
      
      return string.Empty;
    }
    
    /// <summary>
    /// Получить информацию о НОР и ТОП сотрудника.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Строка в формате json. Два признака IsTop, IsGo.</returns>
    /// <remarks>IsTop - наличие у сотрудника привилегий ТОП.
    /// IsGo - принадлежность сотрудника к ГО Норникеля.</remarks>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetTopAndGoValue(long employeeId)
    {
      Logger.DebugFormat("GetTopAndGoValue. EmployeeId: '{0}'. Получен запрос для получения информации о НОР и ТОП сотрудника.", employeeId);
      var result = string.Empty;
      
      try
      {
        var employee = DirRX.OverrideSolution.Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
        if (employee == null)
          Logger.ErrorFormat("GetTopAndGoValue. EmployeeId: '{0}'. Не найден сотрудник по ИД.", employeeId);
        
        var goBusinessUnitSapId = Functions.Module.GetStringValue(PublicConstants.Module.ConstantsSetting.GoSapCode.Guid);
        if (string.IsNullOrEmpty(goBusinessUnitSapId))
          Logger.ErrorFormat("GetTopAndGoValue. EmployeeId: '{0}'. Не заполнена/не найдена прикладная константа {1} по Guid: {2}.", employeeId,
                             PublicConstants.Module.ConstantsSetting.GoSapCode.Name,
                             PublicConstants.Module.ConstantsSetting.GoSapCode.Guid);
        
        var employeeGoAndTopInfo = Structures.Module.EmployeeGoAndTopInfo.Create();
        employeeGoAndTopInfo.IsGo = OverrideSolution.BusinessUnits.As(employee.BusinessUnit)?.SapLegalEntityCodeDirRX == goBusinessUnitSapId;
        employeeGoAndTopInfo.IsTop = OverrideSolution.JobTitles.As(employee.JobTitle)?.IsTopDirRX == true;
        
        result = IsolatedFunctions.Json.SerializeInJson(employeeGoAndTopInfo);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetTopAndGoValue. EmployeeId: '{0}'. Ошибка: '{1}'. StackTrace: '{2}'.", employeeId, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    /// <summary>
    /// Получение ИД Нашей организации сотрудника.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>ИД Нашей организации сотрудника.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public long GetBusinessUnitId(long employeeId)
    {
      Logger.DebugFormat("GetBusinessUnitId. EmployeeId: '{0}'. Получен запрос из ЛК для получения информации о НОР сотрудника.", employeeId);
      var result = 0;
      
      try
      {
        var employee = DirRX.OverrideSolution.Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
        if (employee == null)
        {
          Logger.ErrorFormat("GetBusinessUnitId. EmployeeId: '{0}'. Не найден сотрудник по ИД.", employeeId);
          return result;
        }
        
        if (employee.BusinessUnit == null)
        {
          Logger.ErrorFormat("GetBusinessUnitId. EmployeeId: '{0}'. В карточке сотрудника не заполнено поле 'Наша организация'.", employeeId);
          return result;
        }
        
        return employee.BusinessUnit.Id;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetBusinessUnitId. EmployeeId: '{0}'. Ошибка: '{1}'. StackTrace: '{2}'.", employeeId, ex.Message, ex.StackTrace);
      }
      
      return result;
    }
    
    /// <summary>
    /// Получить список доступных сотрудников с учетом настроек ограничения видимости орг. структуры.
    /// </summary>
    /// <param name="queryName">Условие для поиска по имени сотрудника.</param>
    /// <param name="queryIds">Условие для поиска по ИД сотрудника.</param>
    /// <param name="takeCount">Количество записей, которое необходимо вернуть. Если передано 0, то возвращаются все доступные записи.</param>
    /// <param name="skipCount">Количество записей, которое необходимо пропустить.</param>
    /// <returns>Cписок доступных сотрудников.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public List<Structures.Module.IEmployeeInfo> GetVisibleEmployees(string queryByName, string queryByIds, int takeCount, int skipCount)
    {
      var employees = OverrideSolution.Employees.GetAll().Where(e => e.Status != null && e.Status == OverrideSolution.Employee.Status.Active);
      var employeesInfo = new List<Structures.Module.IEmployeeInfo>();
      
      if (Sungero.Company.PublicFunctions.Module.Remote.IsRecipientRestrict())
      {
        var visibleEmployeesIds = Sungero.Company.PublicFunctions.Module.Remote.GetVisibleRecipientIds(Constants.Module.EmployeeTypeGuid);
        employees = employees.Where(e => visibleEmployeesIds.Contains(e.Id));
      }
      
      if (!string.IsNullOrEmpty(queryByName))
        employees = employees.Where(e => e.Name != null && e.Name.ToLower().Contains(queryByName.ToLower()));
      
      if (!string.IsNullOrEmpty(queryByIds))
      {
        var idsForFilter = queryByIds.Split(',').ToList();
        employees = employees.Where(e => idsForFilter.Contains(e.Id.ToString()));
      }
      
      if (takeCount <= 0)
        takeCount = Constants.Module.DefaultRecordCountForOpeningEmployees;
      employees = employees.Skip(skipCount).Take(takeCount);
      
      foreach (var empl in employees)
      {
        var emplInfo = Structures.Module.EmployeeInfo.Create();
        emplInfo.Id = empl.Id;
        emplInfo.Name = empl.Name;
        employeesInfo.Add(emplInfo);
      }

      return employeesInfo;
    }
    
    #endregion

    /// <summary>
    /// Получить шаблон по виду документа.
    /// </summary>
    /// <param name="docKind">Вид документа.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Шаблон.</returns>
    [Public, Remote(IsPure = true)]
    public static Sungero.Content.IElectronicDocumentTemplate GetTemplateByKind(Sungero.Docflow.IDocumentKind docKind, Sungero.Company.IBusinessUnit businessUnit)
    {
      var docKindHR = OverrideSolution.DocumentKinds.As(docKind);
      if (docKindHR.IsNeedTemplatesRXHRPro != true)
        return null;
      
      // Если есть шаблон для конкретной НОР, то использовать его, иначе - шаблон по умолчанию.
      var docKindBUTemplate = docKindHR.TemplatesToBusinessUnitHRPro.Where(t => Sungero.Company.BusinessUnits.Equals(t.BusinessUnit, businessUnit));
      if (!docKindBUTemplate.Any())
        docKindBUTemplate = docKindHR.TemplatesToBusinessUnitHRPro.Where(t => t.BusinessUnit == null);
      
      return Sungero.Content.ElectronicDocumentTemplates.As(docKindBUTemplate.Select(t => t.TemplateRX).FirstOrDefault());
    }

    /// <summary>
    /// Проверить есть ли у сотрудника стартованные задачи на выпуск сертификата.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>True если задачи найдены, иначе false.</returns>
    [Public, Remote]
    public bool EmployeeHasActiveCertificateIssueTask(DirRX.EssPlatformSolution.IEmployee employee, NNCommons.ICertificateIssueTask fromTask)
    {
      return NNCommons.CertificateIssueTasks.GetAll(c => !Equals(fromTask, c) && Equals(c.Employee, employee) && c.Status.HasValue &&
                                                    c.Status.Value == Sungero.Workflow.Task.Status.InProcess).Any();
    }

    /// <summary>
    /// Получить список подразделений, доступных пользователю.
    /// </summary>
    /// <param name="businessUnitIds">Список НОР.</param>
    /// <returns>Список подразделений.</returns>
    [Public, Remote(IsPure=true)]
    public IQueryable<DirRX.OverrideSolution.IDepartment> GetAvailableDepartments(List<DirRX.OverrideSolution.IBusinessUnit> businessUnits)
    {
      var departmentsIds = DirRX.OverrideSolution.Departments.GetAll(d => d.Status == DirRX.OverrideSolution.Department.Status.Active &&
                                                                     businessUnits.Contains(DirRX.OverrideSolution.BusinessUnits.As(d.BusinessUnit))).Select(d => d.Id);
      return DirRX.OverrideSolution.Departments.GetAll(d => departmentsIds.Where(dep => dep == d.Id).Any());
    }

    /// <summary>
    /// Получить список доступных сотруднику НОР из справочника "Ограничения видимости документов".
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Список НОР доступных сотруднику.</returns>
    [Public, Remote]
    public List<DirRX.OverrideSolution.IBusinessUnit> GetAvailableBusinessUnits(DirRX.OverrideSolution.IEmployee employee)
    {
      var limitations = DocumentsVisabilityLimitations.GetAll().Where(x => x.Status == NNCommons.DocumentsVisabilityLimitation.Status.Active &&
                                                                      x.Employees.Any(emp => DirRX.OverrideSolution.Employees.Equals(emp.Employee, employee)));
      var businessUnits = limitations.Select(l => l.BusinessUnit).Distinct().ToList();
      return businessUnits;
    }

    /// <summary>
    /// Проверить возможность изменить статус документа.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>Текст ошибки.</returns>
    [Public]
    public virtual string CheckPossibilityStatusChange(DirRX.OverrideSolution.IDocRelatedToWorkBase document)
    {
      if (DirRX.OverrideSolution.VacationOrderDocuments.Is(document) || DirRX.OverrideSolution.VacationScheduleDocuments.Is(document) ||
          NNCommons.OrderDocumentBases.Is(document) || DirRX.OverrideSolution.DocRelatedToWorks.Is(document))
      {
        if (Users.Current.IsSystem != true &&
            !document.State.IsInserted &&
            ((document.State.Properties.LifeCycleState.IsChanged && document.LifeCycleState != document.State.Properties.LifeCycleState.OriginalValue) ||
             (document.State.Properties.InternalApprovalState.IsChanged && document.InternalApprovalState != document.State.Properties.InternalApprovalState.OriginalValue)))
        {
          var task = Sungero.Workflow.Tasks.GetAll().FirstOrDefault(x => x.Status != Sungero.Workflow.Task.Status.Aborted &&
                                                                    x.AttachmentDetails.Any(a => a.AttachmentId == document.Id));
          if (task != null)
          {
            if (task.Status == Sungero.Workflow.Task.Status.Draft ||
                task.Status == Sungero.Workflow.Task.Status.InProcess ||
                task.Status == Sungero.Workflow.Task.Status.UnderReview)
              return DirRX.NNCommons.Resources.HaveTasksInProgressError;
            if (task.Status == Sungero.Workflow.Task.Status.Completed ||
                task.Status == Sungero.Workflow.Task.Status.Suspended)
              return DirRX.NNCommons.Resources.ApprovedDocumentCannotBeChangedError;
          }
        }
      }
      return string.Empty;
    }

    /// <summary>
    /// Создать обобщающую роль.
    /// </summary>
    /// <param name="name">Имя обобщающей роли.</param>
    /// <returns>Обобщающая роль.</returns>
    [Public]
    public virtual IGeneralRole CreateGeneralRole(string name)
    {
      var generalRole = GeneralRoles.GetAll(r => r.Name == name).FirstOrDefault();
      if (generalRole == null)
      {
        generalRole = GeneralRoles.Create();
        generalRole.Name = name;
        generalRole.Save();
      }
      return generalRole;
    }

    /// <summary>
    /// Создать бизнес-роль.
    /// </summary>
    /// <param name="roleType">Тип роли.</param>
    /// <param name="businessProcess">Бизнес-процесс.</param>
    /// <param name="docKind">Вид документа.</param>
    /// <param name="top">Топ.</param>
    /// <returns>Бизнес-роль.</returns>
    [Public]
    public virtual IBusinessRole CreateBusinessRole(Enumeration roleType, IBusinessProcess businessProcess, OverrideSolution.IDocumentKind docKind, bool top)
    {
      var businessRole = BusinessRoles.GetAll(r => r.RoleType == roleType &&
                                              Equals(r.Process, businessProcess) &&
                                              Equals(r.DocumentKind, docKind) &&
                                              r.Top == top).FirstOrDefault();
      if (businessRole == null)
      {
        businessRole = BusinessRoles.Create();
        businessRole.RoleType = roleType;
        businessRole.Process = businessProcess;
        businessRole.DocumentKind = docKind;
        businessRole.Top = top;
        businessRole.Save();
      }
      return businessRole;
    }

    /// <summary>
    /// Создать бизнес-процесс.
    /// </summary>
    /// <param name="name">Имя бизнес-процесса.</param>
    /// <param name="processGroup">Группа процессов.</param>
    /// <returns>Бизнес-процесс.</returns>
    [Public]
    public virtual IBusinessProcess CreateBusinessProcess(string name, IBusinessProcess processGroup)
    {
      var businessProcess = BusinessProcesses.GetAll(b => b.Name == name).FirstOrDefault();
      if (businessProcess == null)
      {
        businessProcess = BusinessProcesses.Create();
        businessProcess.Name = name;
        businessProcess.ProcessGroup = processGroup;
        businessProcess.Save();
      }
      Functions.Module.CreateBusinessRole(NNCommons.BusinessRole.RoleType.ByProcess, businessProcess, null, true);
      Functions.Module.CreateBusinessRole(NNCommons.BusinessRole.RoleType.ByProcess, businessProcess, null, false);
      
      return businessProcess;
    }

    /// <summary>
    /// Expression-функция сверки всех документов переданному состоянию документа HRPro.
    /// </summary>
    /// <param name="task">Задачи ведения графиков отпусков.</param>
    /// <returns>Список листов ознакомления.</returns>
    [Public, Remote(IsPure = true), ExpressionElement("ExpFunc_VerifyDocumentsForState_Name", "ExpFunc_VerifyDocumentsForState_Desc", "")]
    public static bool VerifyDocumentsForState(List<OverrideSolution.IDocRelatedToWorkBase> documents, string state)
    {
      state = state.Trim();
      return documents.All(x => OverrideSolution.DocRelatedToWorkBases.Info.Properties.LifeCycleState.GetLocalizedValue(x.LifeCycleState) == state);
    }

    /// <summary>
    /// Получить ИД вида документа по guid.
    /// </summary>
    /// <param name="guid">Guid вида документа.</param>
    /// <returns>ИД вида документа.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public long? GetDocumentKindId(string guid)
    {
      var docKindGuid = Guid.Empty;
      if (Guid.TryParse(guid, out docKindGuid))
      {
        var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(docKindGuid);
        if (docKind != null)
          return docKind.Id;
        else
          Logger.ErrorFormat("GetDocumentKindId. guid: {0}. Не найден вид документа.", guid);
      }
      else
      {
        Logger.ErrorFormat("GetDocumentKindId. guid: {0}. Некорректный формат guid.", guid);
      }
      
      return null;
    }

    /// <summary>
    /// Проверить необходимость фильтрации документов по ролевой модели для текущего пользователя.
    /// </summary>
    /// <returns>Признак необходимости фильтрации.</returns>
    [Public]
    public static bool NeedDocumentsFiltration()
    {
      var user = Users.Current;
      var systemRecipientsSid = new List<Guid>();
      systemRecipientsSid.Add(Sungero.Domain.Shared.SystemRoleSid.Administrators);
      systemRecipientsSid.Add(Sungero.Domain.Shared.SystemRoleSid.Auditors);
      systemRecipientsSid.Add(Sungero.Domain.Shared.SystemRoleSid.ServiceUsers);
      
      // Проверить на включение в состав предопределенных ролей «Служебные пользователи», «Администраторы», «Аудиторы».
      var isIncludedInServiceRole = Roles.GetAll(x => x.Sid != null && systemRecipientsSid.Contains(x.Sid.Value)).Any(x => x.RecipientLinks.Any(r => Recipients.Equals(r.Member, user)));
      if (isIncludedInServiceRole)
        return false;
      
      // Проверить на включение в качестве участника хотя бы в одну из ролей справочника Ограничения видимости документов.
      var isIncludedInLimitation = DocumentsVisabilityLimitations.GetAll().Any(x => x.Employees.Any(e => Users.Equals(e.Employee, user)));
      if (isIncludedInLimitation)
        return true;
      
      return false;
    }

    /// <summary>
    /// Проверить наличие прав на просмотр документа по ролевой модели для сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="document">Документ.</param>
    /// <returns>Признак наличия прав. True - если права есть. False - если нет.</returns>
    [Public, Remote(IsPure = true)]
    public virtual bool HasRightsOnEDoc(DirRX.OverrideSolution.IEmployee employee, DirRX.OverrideSolution.IDocRelatedToWorkBase document)
    {
      // Проверить необходимость фильтрации документов по ролевой модели для текущего пользователя.
      var needDocumentFiltration = NeedDocumentsFiltration();
      if (!needDocumentFiltration)
        return true;

      // Наша организация.
      var businessUnit = DirRX.OverrideSolution.BusinessUnits.As(document.BusinessUnit);
      // Раздел персонала.
      var personnelSection = document.PersonnelSectionDirRX;

      // Получить ограничения видимости на сотрудника и НОР документа.
      var limitations = PublicFunctions.DocumentsVisabilityLimitation
        .GetDocumentsVisabilityLimitations(employee, businessUnit);

      // Отфильтровать ограничения видимости по разделу персонала из документа.
      limitations = PublicFunctions.DocumentsVisabilityLimitation
        .FilterLimitationsByPersonnelSection(limitations, personnelSection);

      // Проверка 1: Есть ли ограничение, дающее доступ к этому документу?
      var hasLimitation = limitations.Any();
      
      // Проверка 2: Есть ли экземплярные права на документ?
      var hasInstanceRights = document.AccessRights.Current
        .Any(a => a.Recipient.Id == employee.Id && a.AccessRightsType != DefaultAccessRightsTypes.Forbidden);

      // Проверка 3: Совпадает ли НОР документа с НОР сотрудника?
      var isSameBusinessUnit = OverrideSolution.BusinessUnits.Equals(document.BusinessUnit, employee.BusinessUnit);

      // Доступ есть, если:
      // 1. Есть ограничение, дающее доступ к документу (с учетом раздела) ИЛИ
      // 2. Есть экземплярные права И НОР документа совпадает с НОР сотрудника
      if (hasLimitation || (isSameBusinessUnit && hasInstanceRights))
        return true;

      return false;
    }


    /// <summary>
    /// Проверить доступ к запуску АО Проверка документов в файловом хранилище.
    /// </summary>
    /// <returns>Текст ошибки.</returns>
    [Remote]
    public virtual string CheckAccessVerifyDocuments()
    {
      if (!Sungero.Docflow.PublicFunctions.Module.Remote.IsAdministrator())
        return DirRX.NNCommons.Resources.VerifyDocumentsDialogAccessError;
      var documentConstant = PublicFunctions.Module.Remote.GetConstant(Constants.Module.ConstantsSetting.VerifyDocumentsCountInIteration.Guid);
      if (documentConstant == null)
        return string.Format(DirRX.NNCommons.Resources.ConstantNotFoundError, Constants.Module.ConstantsSetting.VerifyDocumentsCountInIteration.Name,
                             Constants.Module.ConstantsSetting.VerifyDocumentsCountInIteration.Guid);
      
      // Если константа заблокирована не текущим пользователем, тогда считаем что АО уже запущен.
      if (Locks.GetLockInfo(documentConstant).IsLockedByOther)
        return DirRX.NNCommons.Resources.VerifyDocumentsDialogReportLocked;
      return string.Empty;
    }

    /// <summary>
    /// Получить Id провайдера для выпуска сертификата сотруднику при активации УЗ.
    /// </summary>
    /// <returns>Провайдер.</returns>
    [Public]
    public virtual string GetProviderIdIssueCertificate()
    {
      // По умолчанию провайдер = Directum СОП.
      return Constants.Module.DirectumProviderId;
    }

    /// <summary>
    /// Проверить наличие у персоны сертификата на ЭП.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <returns>Результат проверки. True - если сертификат есть. False - если сертификата нет.</returns>
    [Public]
    public bool HasEssCertificate(long personId)
    {
      // Проверить наличие сертификата по параметрам.
      return GetEssCertificates(personId).Any();
    }

    /// <summary>
    /// Получить сертификаты ЭП закреплённые за персоной.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <returns>Сертификаты ЭП.</returns>
    [Public, Remote]
    public IQueryable<ICertificate> GetEssCertificates(long personId)
    {
      var employeeIds = DirRX.EssPlatform.PublicFunctions.Module.GetPersonEmployeeIds(personId);
      
      return Certificates.GetAll()
        .Where(x =>
               employeeIds.Contains(x.Owner.Id) &&
               x.Enabled == true &&
               x.Parameters.Any(p => p.Key == DirRX.SignPlatform.PublicConstants.Module.ProviderIdCertificateKeyParameter) &&
               x.Parameters.Any(p => p.Key == DirRX.SignPlatform.PublicConstants.Module.OwnerIdCertificateKeyParameter && p.Value.Contains(personId.ToString())) &&
               x.NotBefore <= Calendar.Now && Calendar.Now < x.NotAfter);
    }

    #region Константы.

    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    /// <returns>Новая или обновленная константа</returns>
    public virtual IConstantsSetting CreateOrUpdateConstant(string name, bool isSystem, string guid, string groupGuid)
    {
      var logPrefix = string.Format("CreateOrUpdateConstant. Name: {0}. Guid: {1}. GroupGuid: {2}", name, guid, groupGuid);
      Logger.DebugFormat("{0}. Создание/обновление константы.", logPrefix);
      
      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(groupGuid))
      {
        Logger.ErrorFormat("{0}. Не заполнены обязательные поля для константы.", logPrefix);
        return ConstantsSettings.Null;
      }
      
      // Найти константу по guid. Если в константе не заполнены guid и guid группы, то поиск выполняется по имени. Если константа не найдена, то создать новую.
      var constant = ConstantsSettings.GetAll(p => ((p.GUID == null || p.GUID == string.Empty) && p.ConstantsGroup == null && p.Name == name) ||
                                              p.GUID == guid).FirstOrDefault() ??
        ConstantsSettings.Create();
      
      // Указать guid константы.
      if (string.IsNullOrEmpty(constant.GUID))
        constant.GUID = guid;
      
      if (!(constant.IsSystem == isSystem))
        constant.IsSystem = isSystem;
      
      // Указать группу констант.
      if (constant.ConstantsGroup == null)
      {
        var constantGroup = ConstantsGroups.GetAll(c => c.GUID == groupGuid).FirstOrDefault();
        if (constantGroup == null)
        {
          Logger.ErrorFormat("{0}. Не найдена группа констант по guid.", logPrefix);
          return ConstantsSettings.Null;
        }
        else
          constant.ConstantsGroup = constantGroup;
      }
      
      // Если константа не сохранена в БД, заполняем её поля.
      if (constant.State.IsInserted)
      {
        constant.Name = name;
      }
      
      return constant;
    }

    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">Строковое значение.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, string value, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      if (constant.State.IsInserted)
      {
        constant.StringValue = value;
        constant.Type = NNCommons.ConstantsSetting.Type.StringType;
      }
      
      if (constant.State.IsChanged)
        constant.Save();
    }

    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">Числовое значение.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, double value, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      if (constant.State.IsInserted)
      {
        constant.DoubleValue = value;
        constant.Type = NNCommons.ConstantsSetting.Type.DoubleType;
      }
      
      if (constant.State.IsChanged)
        constant.Save();
    }

    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">Значение идентификатора.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, long value, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      if (constant.State.IsInserted)
      {
        constant.IdentifierValue = value;
        constant.Type = NNCommons.ConstantsSetting.Type.IdentifierType;
      }
      
      if (constant.State.IsChanged)
        constant.Save();
    }

    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">Числовое значение.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, int value, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      if (constant.State.IsInserted)
      {
        constant.IntegerValue = value;
        constant.Type = NNCommons.ConstantsSetting.Type.IntegerType;
      }
      
      if (constant.State.IsChanged)
        constant.Save();
    }

    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">Дата.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, DateTime value, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      if (constant.State.IsInserted)
      {
        constant.DateValue = value;
        constant.Type = NNCommons.ConstantsSetting.Type.DateType;
      }
      
      if (constant.State.IsChanged)
        constant.Save();
    }

    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">Строковое значение.</param>
    /// <param name="isPassword">Является паролем.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, string value, bool isPassword, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      if (constant.State.IsInserted)
      {
        constant.PasswordValue = value;
        constant.Type = NNCommons.ConstantsSetting.Type.PasswordType;
      }
      
      if (constant.State.IsChanged)
        constant.Save();
    }

    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">Логическое значение.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, bool value, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      if (constant.State.IsInserted)
      {
        constant.BoolValue = value;
        constant.Type = NNCommons.ConstantsSetting.Type.BoolType;
      }
      
      if (constant.State.IsChanged)
        constant.Save();
    }
    
    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">Должность.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, DirRX.OverrideSolution.IJobTitle value, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      constant.Type = NNCommons.ConstantsSetting.Type.JobTitleType;
      
      if (constant.State.IsInserted && value != null)
        constant.JobTitleValue = value;
      
      if (constant.State.IsChanged)
        constant.Save();
    }
    
    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">Сотрудник.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, DirRX.OverrideSolution.IEmployee value, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      constant.Type = NNCommons.ConstantsSetting.Type.EmployeeType;
      
      if (constant.State.IsInserted && value != null)
        constant.EmployeeValue = value;
      
      if (constant.State.IsChanged)
        constant.Save();
    }
    
    /// <summary>
    /// Создать или обновить прикладную константу, если не заполнено поле GUID.
    /// </summary>
    /// <param name="name">Имя константы.</param>
    /// <param name="isSystem">Системная. Константы, создаваемые в инициализации, должны быть системными.</param>
    /// <param name="value">НОР.</param>
    /// <param name="guid">Guid константы.</param>
    /// <param name="groupGuid">Guid группы константы.</param>
    [Public, Remote]
    public virtual void CreateConstant(string name, bool isSystem, DirRX.OverrideSolution.IBusinessUnit value, string guid, string groupGuid)
    {
      var constant = CreateOrUpdateConstant(name, isSystem, guid, groupGuid);
      if (constant == null)
        return;
      
      constant.Type = NNCommons.ConstantsSetting.Type.BusUnitType;
      
      if (constant.State.IsInserted && value != null)
        constant.BusinessUnitValue = value;
      
      if (constant.State.IsChanged)
        constant.Save();
    }

    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public]
    public virtual object GetValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant == null)
        return null;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.StringType)
        return constant.StringValue;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.IntegerType)
        return constant.IntegerValue;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.DoubleType)
        return constant.DoubleValue;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.IdentifierType)
        return constant.IdentifierValue;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.DateType)
        return constant.DateValue.Value;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.PasswordType)
        return Encryption.Decrypt(constant.Value);
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.BoolType)
        return constant.BoolValue.Value;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.BusUnitType)
        return constant.BusinessUnitValue;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.JobTitleType)
        return constant.JobTitleValue;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.EmployeeType)
        return constant.EmployeeValue;
      
      return constant.Value;
    }

    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public]
    public virtual string GetStringValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant == null)
        return null;
      
      if (constant.Type == NNCommons.ConstantsSetting.Type.StringType)
        return constant.StringValue;

      if (constant.Type == NNCommons.ConstantsSetting.Type.PasswordType)
        return Encryption.Decrypt(constant.Value);

      return constant.Value;
    }

    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public]
    public virtual double? GetDoubleValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant != null && constant.Type == NNCommons.ConstantsSetting.Type.DoubleType)
        return constant.DoubleValue;
      
      return null;
    }

    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public]
    public virtual int? GetIntValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant != null && constant.Type == NNCommons.ConstantsSetting.Type.IntegerType)
        return constant.IntegerValue;
      
      return null;
    }

    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public]
    public virtual long? GetIdentifierValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant == null)
      {
        Logger.ErrorFormat("GetIdentifierValue. name: {0}. Константы с таким именем не существует.", constant.Name);
        return null;
      }
      
      if (constant.Type != NNCommons.ConstantsSetting.Type.IdentifierType)
      {
        Logger.ErrorFormat("GetIdentifierValue. name: {0}. Константа имеет тип отличный от типа Идентификатор.", constant.Name);
        return null;
      }
      
      return constant.IdentifierValue;
    }

    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public]
    public virtual DateTime? GetDateValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant != null && constant.Type == NNCommons.ConstantsSetting.Type.DateType)
        return constant.DateValue.Value;
      
      return null;
    }


    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public, Remote]
    public virtual bool? GetBoolValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant != null && constant.Type == NNCommons.ConstantsSetting.Type.BoolType)
        return constant.BoolValue;
      
      return null;
    }
    
    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public, Remote]
    public virtual DirRX.OverrideSolution.IEmployee GetEmployeeValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant != null && constant.Type == NNCommons.ConstantsSetting.Type.EmployeeType)
        return constant.EmployeeValue;
      
      return null;
    }
    
    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public, Remote]
    public virtual DirRX.OverrideSolution.IBusinessUnit GetBusinessUnitValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant != null && constant.Type == NNCommons.ConstantsSetting.Type.BusUnitType)
        return constant.BusinessUnitValue;
      
      return null;
    }
    
    /// <summary>
    /// Получить значение константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Значение константы.</returns>
    [Public, Remote]
    public virtual DirRX.OverrideSolution.IJobTitle GetJobTitleValue(string guid)
    {
      var constant = ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
      if (constant != null && constant.Type == NNCommons.ConstantsSetting.Type.JobTitleType)
        return constant.JobTitleValue;
      
      return null;
    }
    
    /// <summary>
    /// Получить список Id из константы.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Список Id.</returns>
    [Public]
    public virtual List<long> GetIdsListValue(string guid, string prefix)
    {
      var idsList = new List<long>();
      var constant = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(guid);
      if (string.IsNullOrEmpty(constant))
      {
        Logger.DebugFormat("{0}. Прикладная константа '{1}' не заполнена.", prefix, guid);
        return idsList;
      }
      
      try
      {
        idsList = constant.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => long.Parse(id.Trim())).ToList();
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("{0}. Не удалось извлечь список ИД из прикладной константы: {1}. Текст ошибки: {2}. StackTrace: {3}.",
                           prefix, guid,
                           ex.Message, ex.StackTrace);
        return new List<long>();
      }
      
      return idsList;
    }

    /// <summary>
    /// Получить константу.
    /// </summary>
    /// <param name="guid">Guid константы.</param>
    /// <returns>Константа.</returns>
    [Public, Remote]
    public virtual IConstantsSetting GetConstant(string guid)
    {
      return ConstantsSettings.GetAll(p => p.GUID == guid).FirstOrDefault();
    }

    #endregion

    /// <summary>
    /// Установить свойства Состояние и Согласование в документах.
    /// </summary>
    /// <param name="documents">Документы.</param>
    /// <param name="approvalState">Статус согласования.</param>
    /// <param name="lifeState">Состояние.</param>
    [Public, Remote]
    public void SetLifeAndApprovalState(List<IOfficialDocument> documents,
                                        Enumeration approvalState, Enumeration lifeState)
    {
      foreach (var doc in documents)
      {
        if (HRPro.HRDocFlow.PublicFunctions.Module.IsHrProDocument(doc))
        {
          var hrDoc = DirRX.OverrideSolution.OfficialDocuments.As(doc);
          
          // В HR св-во HRLifeCycleStateHRPro продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
          // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
          // Используется в HRLifeCycleStateHRProChanged официального документа.
          if (!hrDoc.HRLifeCycleStateHRPro.HasValue || hrDoc.HRLifeCycleStateHRPro != lifeState)
            hrDoc.HRLifeCycleStateHRPro = lifeState;
          
          if (!hrDoc.InternalApprovalState.HasValue || hrDoc.InternalApprovalState != approvalState)
            hrDoc.InternalApprovalState = approvalState;
          
          hrDoc.Save();
        }
        else
          Logger.DebugFormat("SetLifeAndApprovalState() Документ с  ИД={0} пропущен - это не документ HR", doc.Id);
      }
    }

    /// <summary>
    /// Запустить АО для выполнения проверки документов в хранилище.
    /// </summary>
    /// <param name="beginDate">Дата начала периода.</param>
    /// <param name="endDate">Дата конца периода.</param>
    /// <param name="employee">Сотрудник, стартовавший задачу.</param>
    /// <param name="inception">Признак первого запуска АО.</param>
    /// <param name="iteration">Номер итерации.</param>
    [Public, Remote]
    public void VerifyDocumentsInStorage(DateTime beginDate, DateTime endDate, long employeeId, bool inception, int iteration)
    {
      var async = NNCommons.AsyncHandlers.VerifyDocumentsInStorage.Create();
      async.beginDate = beginDate;
      async.endDate = endDate;
      async.employeeId = employeeId;
      async.inception = inception;
      async.iteration = iteration;
      async.ExecuteAsync();
    }

    /// <summary>
    /// Проверить документ на чтение.
    /// </summary>
    /// <param name="binaryData">Тело документа.</param>
    /// <returns>Структура с информацией по проверке.</returns>
    public Structures.Module.IVersionVerifyInfo CheckReadingDocumentBody(Sungero.Domain.Shared.IBinaryData binaryData)
    {
      var versionVerifyInfo = NNCommons.Structures.Module.VersionVerifyInfo.Create();
      versionVerifyInfo.IsPublicBody = false;
      try
      {
        var bodyStream = binaryData.Read();
        var textError = DirRX.NNCommons.IsolatedFunctions.VerifyDocumentsInStorage.CheckReadingDocumentBody(bodyStream, binaryData.Size);
        if (textError == string.Empty)
          versionVerifyInfo.Valid = true;
        else
        {
          versionVerifyInfo.Valid = false;
          versionVerifyInfo.Error = textError;
        }
      }
      catch (Exception ex)
      {
        versionVerifyInfo.Valid = false;
        versionVerifyInfo.Error = ex.Message;
      }
      return versionVerifyInfo;
    }

    /// <summary>
    /// Найти и заблокировать обобщённую роль.
    /// </summary>
    /// <param name="logPrefix">Префикс лога.</param>
    /// <param name="roleId">Ид роли.</param>
    /// <param name="isBlocked">Результат блокировки. True - если удалось заблокировать, False - если нет.</param>
    /// <returns>Обобщённая роль.</returns>
    public virtual Structures.Module.IGeneralRoleInfo TryFindAndBlockGeneralRole(string logPrefix, long roleId)
    {
      // Поиск обобщённой роли.
      var result = Structures.Module.GeneralRoleInfo.Create();
      result.GeneralRole = GeneralRoles.GetAll(x => x.Id == roleId).FirstOrDefault();
      result.IsLocked = false;
      if (result.GeneralRole == null)
      {
        Logger.ErrorFormat("{0} Не найдена обобщённая роль по Ид.", logPrefix);
        return result;
      }
      
      // Блокировка роли.
      var roleIsLocked = Locks.TryLock(result.GeneralRole);
      if (!roleIsLocked)
      {
        var lockInfo = Locks.GetLockInfo(result.GeneralRole);
        Logger.DebugFormat("{0} Не удалось установить блокировку на обобщённую роль. Роль заблокирована пользователем: {1}. Время установки блокировки: {2}. Обработка повторится при следующем запуске АО.",
                           logPrefix, lockInfo.OwnerName, lockInfo.LockTime);
        return result;
      }
      
      result.IsLocked = true;
      return result;
    }

    /// <summary>
    /// Expression-функция для вычитания рабочих дней из даты.
    /// </summary>
    /// <param name="task">Задачи ведения графиков отпусков.</param>
    /// <returns>Список сотрудников.</returns>
    [Public, Remote(IsPure = true), ExpressionElement("ExpFunc_SubtractDays_Name", "ExpFunc_SubtractDays_Desc", ""), Obsolete("Используйте коробочные функции \"Добавить рабочие дни\" и \"Вычесть рабочие дни\".")]
    public static DateTime SubtractDays(DateTime date, int days)
    {
      return date.AddWorkingDays(-1 * days);
    }

    /// <summary>
    /// Функция определяет является ли подпись УНЭП.
    /// </summary>
    /// <param name="signature">Электронная подпись.</param>
    /// <returns>True если является, иначе false.</returns>
    [Public]
    public virtual bool IsUnqualifiedSignature(Sungero.Domain.Shared.ISignature signature)
    {
      if (signature?.SignCertificate == null)
        return false;
      
      var issueConst = GetStringValue(Constants.Module.ConstantsSetting.IssuerNamesUnqualifiedSignature.Guid);
      if (string.IsNullOrEmpty(issueConst))
        return false;
      
      var signCertificate = signature.SignCertificate;
      var certificate = Certificates.GetAll(x => x.Thumbprint == signCertificate.Thumbprint).FirstOrDefault();
      var issueNames = issueConst.Trim().Split(',');
      return issueNames.Contains(signCertificate.IssuerName) && certificate?.PluginId == DirRX.SignPlatform.PublicConstants.Module.PluginIdOfCloudCertificates;
    }

    /// <summary>
    /// Функция опеределяет является ли подпись ПЭП.
    /// </summary>
    /// <param name="signature">Электронная подпись.</param>
    /// <returns>True если является, иначе false.</returns>
    [Public]
    public virtual bool IsSimpleSignature(Sungero.Domain.Shared.ISignature signature)
    {
      if (signature?.SignCertificate == null)
        return true;
      
      var issueConst = GetStringValue(Constants.Module.ConstantsSetting.IssuerNames.Guid);
      if (string.IsNullOrEmpty(issueConst))
        return false;
      
      var issueNames = issueConst.Trim().Split(',');
      return issueNames.Contains(signature.SignCertificate.IssuerName);
    }
    
    /// <summary>
    /// Функция определяет является ли Сертификат ПЭП.
    /// </summary>
    /// <param name="certificate">Сертификат.</param>
    /// <returns>True если является сертификатом ПЭП, иначе false.</returns>
    [Public]
    public virtual bool IsSimpleCertificate(Sungero.CoreEntities.ICertificate certificate)
    {
      if (certificate == null)
        return true;
      
      var issueConst = GetStringValue(Constants.Module.ConstantsSetting.IssuerNames.Guid);
      if (string.IsNullOrEmpty(issueConst))
        return false;
      
      var issueNames = issueConst.Trim().Split(',');
      return issueNames.Contains(certificate.Issuer);
    }

    /// <summary>
    /// Функция создает группу констант.
    /// </summary>
    /// <param name="name">Имя группы констант.</param>
    /// <param name="Guid">Guid группы констант.</param>
    [Public]
    public void CreateConstantsGroup(string name, string guid)
    {
      if (ConstantsGroups.GetAll(p => p.GUID == guid).Any())
      {
        var constant = ConstantsGroups.GetAll(p => p.GUID == guid).FirstOrDefault();
        
        if (!string.Equals(constant.Name, name))
        {
          constant.Name = name;
          constant.Save();
        }
        
        return;
      }
      
      var newConstantsGroup = ConstantsGroups.Create();
      newConstantsGroup.Name = name;
      newConstantsGroup.GUID = guid;
      newConstantsGroup.Save();
    }

    /// <summary>
    /// Проверить, что итерация повтора асинхронного обработчика - последняя.
    /// </summary>
    /// <param name="currentIteration">Текущая итерация.</param>
    /// <returns>True, если повтор - последний, иначе False.</returns>
    [Public]
    public bool IsLastAsyncHandlerRetry(int currentIteration)
    {
      var maxRetry = GetIntValue(NNIntegration.PublicConstants.Module.ConstantsSetting.SendNotificationToSupernikaAsyncHandlerMaxRetry.Guid);
      if (maxRetry == null)
      {
        Logger.ErrorFormat("IsLastAsyncHandlerRetry. Не удалось определить максимальное число повторов АО. Не заполнена константа: {0}.",
                           NNIntegration.PublicConstants.Module.ConstantsSetting.SendNotificationToSupernikaAsyncHandlerMaxRetry.Name);
        return true;
      }
      
      return currentIteration >= maxRetry;
    }

    #region Усовершенствование ЭП до CAdES-XL.

    /// <summary>
    /// Получение списка документов и их версий с ЭП уровня CadesBes и CadesT.
    /// </summary>
    /// <param name="documents">Список документов.</param>
    /// <returns>Отфильтрованный словарь документов и их версий.</returns>
    public static System.Collections.Generic.Dictionary<Sungero.Content.IElectronicDocumentVersions, Sungero.Content.IElectronicDocument> GetDocumentAndVersionsWithSignature(List<Sungero.Content.IElectronicDocument> documents)
    {
      var docWithVersions = new Dictionary<Sungero.Content.IElectronicDocumentVersions, Sungero.Content.IElectronicDocument>();
      var findFormats = new List<Sungero.Core.SignatureFormat>{SignatureFormat.CadesBes, SignatureFormat.CadesT};
      
      foreach (var document in documents)
      {
        foreach (var version in document.Versions)
        {
          if (Signatures.Get(version).Where(s => findFormats.Contains(Signatures.GetSignatureInfo(s).SignatureFormat)).Any())
            docWithVersions.Add(version, document);
        }
      }
      return docWithVersions;
    }

    /// <summary>
    /// Получить значение параметра "Дата обновления подписи".
    /// </summary>
    /// <returns>Значение найденного параметра, если он существует и значение одноимённой константы, если параметр не существует.</returns>
    public static int GetSignatureUpdateDate()
    {
      Sungero.Docflow.PublicFunctions.Module.InsertDocflowParam(HRPro.HRCommons.PublicConstants.Module.DocflowParams.SignatureUpdateDateKey, Constants.Module.SignatureUpdateDate.ToString());
      var parameter = Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(HRPro.HRCommons.PublicConstants.Module.DocflowParams.SignatureUpdateDateKey);
      return parameter == null ? Constants.Module.SignatureUpdateDate : int.Parse(parameter.ToString());
    }

    #endregion

    #region Интеграционный сценарий "Синхронизация календарей рабочего времени".
    
    #region IF-S-28097. Синхронизация календарей рабочего времени из АСУП в КЭДО (АСУП -> КЭДО).
    
    [Public]
    public NNIntegration.Structures.Module.IProcessIncomingMessageResult UpdateOrCreateWorkingTimeCalendar(long idIncomingMessage)
    {
      var logPrefix = string.Format("UpdateOrCreateWorkingTimeCalendar. Ид входящего сообщения: {0}.", idIncomingMessage);
      Logger.Debug(logPrefix);
      
      var result = NNIntegration.Structures.Module.ProcessIncomingMessageResult.Create();
      result.Retry = false;
      
      var message = NNIntegration.IncomingMessages.GetAll(x => x.Id == idIncomingMessage).FirstOrDefault();
      if (message == null)
      {
        result.ErrorText = "Не найдена запись справочника Входящие интеграционные сообщения.";
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        result.Retry = true;
        return result;
      }
      
      var jsonMessage = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(message);
      
      if (string.IsNullOrWhiteSpace(jsonMessage))
      {
        result.ErrorText = "Не удалось получить тело запроса.";
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        result.Retry = true;
        return result;
      }
      
      var model = NNIntegration.Structures.Module.WorkingTimeCalendar.Create();
      try
      {
        model = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Ошибка десериализации в структуру IncomingWorkingTimeCalendar.";
        Logger.ErrorFormat("{0} {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, result.ErrorText, ex.Message, ex.StackTrace);
        result.Retry = true;
        return result;
      }
      
      var employeeResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Employee);
      
      if (!string.IsNullOrWhiteSpace(employeeResult.ErrorMessage))
      {
        result.ErrorText = string.Format("EmployeeError: {0}. ", employeeResult.ErrorMessage);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var employee = employeeResult.Employee;
      if (employee.Status == OverrideSolution.Employee.Status.Closed)
      {
        result.ErrorText = string.Format("Календари синхронизируются только для действующих сотрудников.");
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var year = model.Year;
      var userCalendar = PrivateWorkingTimeCalendars.Null;
      
      try
      {
        var daysCountOfTargetYear = Calendar.GetDate(year, 1, 1).EndOfYear().DayOfYear;
        var isAllDaysInModel = model.DayDisc.Select(d => d.Date).Distinct().Count() == daysCountOfTargetYear;
        userCalendar = GetOrCreatePrivateWorkingTimeCalendar(year, employee, !isAllDaysInModel, logPrefix);

        if (!userCalendar.State.IsInserted && !Locks.TryLock(userCalendar))
        {
          result.ErrorText = "Не удалось поставить блокировку на частный календарь сотрудника.";
          Logger.ErrorFormat("{0} {1}.", logPrefix, result.ErrorText);
          result.Retry = true;
          return result;
        }
        
        foreach (var dayModel in model.DayDisc)
        {
          var errorText = UpdateWorkingTimeCalendarByDayModel(userCalendar, dayModel);
          
          if (!string.IsNullOrEmpty(errorText))
            throw AppliedCodeException.Create(errorText);
        }
        
        userCalendar.Save();
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки календаря сотрудника с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", employee.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      finally
      {
        if (userCalendar != null && Locks.GetLockInfo(userCalendar).IsLockedByMe)
          Locks.Unlock(userCalendar);
      }
      
      return result;
    }
    
    /// <summary>
    /// Получить существующий или создать новый частный календарь рабочего времени сотрудника.
    /// </summary>
    /// <param name="year">Год.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="needFilledCalendar">Признак необходимости вернуть календарь с заполненными днями.</param>
    /// <param name="logPrefix">Префикс для записи логов.</param>
    /// <returns>Существующий или созданный календарь.</returns>
    public IPrivateWorkingTimeCalendar GetOrCreatePrivateWorkingTimeCalendar(int year, OverrideSolution.IEmployee employee, bool needFilledCalendar, string logPrefix)
    {
      var userCalendar = GetPrivateWorkingCalendar(year, employee);
      
      if (userCalendar == null)
      {
        Logger.DebugFormat("{0} {1}", logPrefix, "Частный календарь сотрудника не найден, будет создан новый.");
        
        userCalendar = PrivateWorkingTimeCalendars.Create();
        userCalendar.Name = string.Format("{0}. {1} ТН {2}", year, Sungero.Company.PublicFunctions.UserBase.GetUserShortName(employee, false), employee.PersonnelNumber);
        userCalendar.Year = year;
        
        var calendarRecipient = userCalendar.Recipients.AddNew();
        calendarRecipient.Recipient = employee;
        
        if(needFilledCalendar)
        {
          // Ищем частный календарь по НОР.
          var globalUserCalendar = WorkingTime.Null;
          globalUserCalendar = GetPrivateWorkingCalendar(year, employee.BusinessUnit);
          
          // Ищем общий календарь.
          if (globalUserCalendar == null)
          {
            globalUserCalendar = WorkingTime.GetAll(c => !PrivateWorkingTimeCalendars.Is(c) && c.Year.HasValue && c.Year.Value == year).FirstOrDefault();
            Logger.DebugFormat("{0} {1}", logPrefix, "Частный календарь сотрудника будет основан на общем календаре.");
          }
          else
            Logger.DebugFormat("{0} {1}", logPrefix, "Частный календарь сотрудника будет основан на календаре НОР.");
          
          foreach (var day in globalUserCalendar.Day)
            userCalendar.Day.AddCopy(day);
        }
      }
      else
      {
        if (!needFilledCalendar)
          userCalendar.Day.Clear();
      }
      
      return userCalendar;
    }
    
    /// <summary>
    /// Обновить календарь в соответствии с переданной моделью дня.
    /// </summary>
    /// <param name="calendar">Обновляемый календарь.</param>
    /// <param name="dayModel">Модель дня.</param>
    /// <returns>Пустая строка при успешном выполнении, иначе текст ошибки.</returns>
    public string UpdateWorkingTimeCalendarByDayModel(IWorkingTimeCalendar calendar,
                                                      NNIntegration.Structures.Module.IWorkingDayDescription dayModel)
    {
      var targetDate = dayModel.Date;
      var calendarDay = GetOrCreateWorkingTimeDay(calendar, targetDate);
      
      if (calendarDay == null)
        return String.Format("Не удалось обработать дату {0}.", targetDate.Date);
      
      var dayBeginning = dayModel.DayStart;
      var dayEnding = dayModel.DayEnd.HasValue ? NormalizeRightBoundaryOfTime(dayModel.DayEnd.Value) : dayModel.DayEnd;
      var lunchBreakBeginning = dayModel.BrStart;
      var lunchBreakEnding = dayModel.BrEnd.HasValue ? NormalizeRightBoundaryOfTime(dayModel.BrEnd.Value) : dayModel.BrEnd;
      
      var previousCalendarDay = GetOrCreateWorkingTimeDay(calendar, targetDate.AddDays(-1));
      var previousCalendarDayIsTransit = false;
      if (previousCalendarDay != null)
        previousCalendarDayIsTransit = previousCalendarDay.DayEnding == Constants.Module.CalendarDayEnding &&
          calendarDay.DayBeginning == Constants.Module.CalendarDayBeginning;
      
      var nextCalendarDay = GetOrCreateWorkingTimeDay(calendar, targetDate.AddDays(1));
      var calendarDayIsTransit = nextCalendarDay != null &&
        calendarDay.DayEnding == Constants.Module.CalendarDayEnding &&
        nextCalendarDay.DayBeginning == Constants.Module.CalendarDayBeginning;
      var dayModelIsTransit = dayBeginning >= dayEnding;
      
      #region Тип дня.
      
      var dayModelKind = DefineWorkingTimeCalendarDayKind(dayModel.Type);
      
      // Если день будет нерабочим.
      if (dayModelKind != null)
      {
        // Если предыдущий день переходящий и текущий день будет выходным (а не праздничным).
        if (previousCalendarDayIsTransit && dayModelKind == Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Weekend)
        {
          // Если текущий день совмещает две смены.
          if (calendarDay.LunchBreakBeginning != null)
          {
            calendarDay.DayEnding = calendarDay.LunchBreakBeginning;
            calendarDay.LunchBreakBeginning = null;
            calendarDay.LunchBreakEnding = null;
          }
        }
        else
          calendarDay.Kind = dayModelKind;
        
        // Если текущий день переходящий.
        if (calendarDayIsTransit && nextCalendarDay != null)
        {
          // Если следующий день совмещает две смены.
          if (nextCalendarDay.LunchBreakBeginning != null)
          {
            nextCalendarDay.DayBeginning = nextCalendarDay.LunchBreakEnding;
            nextCalendarDay.LunchBreakBeginning = null;
            nextCalendarDay.LunchBreakEnding = null;
          }
          else
            nextCalendarDay.Kind = Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Weekend;
        }
        
        return string.Empty;
      }
      else
        calendarDay.Kind = dayModelKind;
      
      #endregion
      
      #region Переходность предыдущего дня.
      
      if (previousCalendarDayIsTransit)
      {
        // Если текущий день не совмещал смен.
        if (calendarDay.LunchBreakBeginning == null)
          calendarDay.LunchBreakBeginning = calendarDay.DayEnding;
        
        calendarDay.LunchBreakEnding = dayBeginning;
        
        NormalizeWorkingTimeCalendarDay(calendarDay);
      }
      else
      {
        calendarDay.DayBeginning = dayBeginning;
        calendarDay.LunchBreakBeginning = dayModelIsTransit ? null : lunchBreakBeginning;
        calendarDay.LunchBreakEnding = dayModelIsTransit ? null : lunchBreakEnding;
      }
      
      #endregion
      
      #region Переходность текущего дня.
      
      // Если день будет переходящий.
      if (dayModelIsTransit)
      {
        calendarDay.DayEnding = Constants.Module.CalendarDayEnding;
        
        if (nextCalendarDay == null)
          return string.Empty;
        
        // Если день уже переходящий.
        if (calendarDayIsTransit)
        {
          // Если следующий день не совмещал смен.
          if (nextCalendarDay.LunchBreakBeginning == null)
            nextCalendarDay.DayEnding = dayEnding;
          else
          {
            nextCalendarDay.LunchBreakBeginning = dayEnding;
            
            NormalizeWorkingTimeCalendarDay(nextCalendarDay);
          }
        }
        else
        {
          if (nextCalendarDay.Kind == Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Holiday)
            return string.Empty;
          
          if (nextCalendarDay.Kind == Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Weekend)
          {
            nextCalendarDay.Kind = null;
            nextCalendarDay.LunchBreakBeginning = null;
            nextCalendarDay.LunchBreakEnding = null;
            nextCalendarDay.DayEnding = dayEnding;
          }
          else
          {
            nextCalendarDay.LunchBreakBeginning = dayEnding;
            nextCalendarDay.LunchBreakEnding = nextCalendarDay.DayBeginning;
            
            NormalizeWorkingTimeCalendarDay(nextCalendarDay);
          }
          
          nextCalendarDay.DayBeginning = Constants.Module.CalendarDayBeginning;
        }
      }
      else
      {
        calendarDay.DayEnding = dayEnding;
        
        // Если текущий день переходящий.
        if (calendarDayIsTransit && nextCalendarDay != null)
        {
          // Если следующий день совмещает две смены.
          if (nextCalendarDay.LunchBreakBeginning != null)
          {
            nextCalendarDay.DayBeginning = nextCalendarDay.LunchBreakEnding;
            nextCalendarDay.LunchBreakBeginning = null;
            nextCalendarDay.LunchBreakEnding = null;
          }
          else
            nextCalendarDay.Kind = Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Weekend;
        }
      }
      
      #endregion
      
      return string.Empty;
    }
    
    /// <summary>
    /// Получить или создать день календаря рабочего времени.
    /// </summary>
    /// <param name="calendar">Календарь рабочего времени.</param>
    /// <param name="date">Дата, соответствующая дню календаря.</param>
    /// <returns>День календаря или null, если такого дня в указанном календаре быть не может.</returns>
    public IWorkingTimeCalendarDay GetOrCreateWorkingTimeDay(IWorkingTimeCalendar calendar, DateTime date)
    {
      if (date.Year != calendar.Year.Value)
        return null;
      
      var calendarDay = calendar.Day.Where(d => d.Day.Date.Equals(date.Date)).FirstOrDefault();
      if (calendarDay == null)
      {
        calendarDay = calendar.Day.AddNew();
        calendarDay.Day = date;
      }
      
      return calendarDay;
    }
    
    /// <summary>
    /// Получить частный календарь рабочего времени субъекта прав.
    /// </summary>
    /// <param name="year">Год</param>
    /// <param name="recipient">Субъект прав.</param>
    /// <returns>Частный календарь.</returns>
    public IPrivateWorkingTimeCalendar GetPrivateWorkingCalendar(int year, IRecipient recipient)
    {
      return PrivateWorkingTimeCalendars.GetAll(c => c.Year.HasValue && c.Year.Value == year &&
                                                c.Recipients.Select(r => r.Recipient).Contains(recipient)).FirstOrDefault();
    }
    
    /// <summary>
    /// Определить тип дня календаря рабочего времени.
    /// </summary>
    /// <param name="type">Целочисленное представление типа календарного дня.</param>
    /// <returns>Значение типа календарного дня.</returns>
    public Nullable<Enumeration> DefineWorkingTimeCalendarDayKind(int type)
    {
      switch (type)
      {
        case 1:
          return Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Weekend;
        case 2:
          return Sungero.CoreEntities.WorkingTimeCalendarDay.Kind.Holiday;
        default:
          return null;
      }
    }
    
    /// <summary>
    /// Нормализация дня календаря рабочего времени.
    /// </summary>
    /// <param name="calendarDay">День календаря рабочего времени.</param>
    public void NormalizeWorkingTimeCalendarDay(IWorkingTimeCalendarDay calendarDay)
    {
      if (calendarDay.LunchBreakBeginning >= calendarDay.LunchBreakEnding)
        calendarDay.LunchBreakEnding = calendarDay.LunchBreakBeginning + Constants.Module.CalendarDayMinute;
    }
    
    /// <summary>
    /// Нормализация правой границы времени.
    /// </summary>
    /// <param name="time">Время.</param>
    /// <returns>Нормализованная правая граница времени.</returns>
    public double NormalizeRightBoundaryOfTime(double time)
    {
      if (time == Constants.Module.CalendarDayBeginning || time == Constants.Module.CalendarDayEnding24)
        time = Constants.Module.CalendarDayEnding;
      
      return time;
    }
    
    /// <summary>
    /// Выполнить измерение скорости расчёта времени с учётом календарей рабочего времени.
    /// </summary>
    /// <param name="employeeCount">Количество случайных сотрудников в выборке.</param>
    /// <param name="shiftDays">Cмещение.</param>
    /// <param name="shiftHours">True для смещения дней, иначе часов.</param>
    /// <returns> Результат замера.</returns>
    [Public, Remote(IsPure=true)]
    public string WorkingTimeCalculationSpeedTest(int employeeCount, int shift, bool shiftDays)
    {
      var totalEmployee = OverrideSolution.Employees.GetAll().Where(e => e.Status == OverrideSolution.Employee.Status.Active &&
                                                                    e.IsVirtualDirRX.HasValue && e.IsVirtualDirRX.Value == false);
      var totalEmployeeCountLong = totalEmployee.Select(e => e.Id).LongCount();
      var totalEmployeeCount = totalEmployeeCountLong > int.MaxValue ? int.MaxValue : (int)totalEmployeeCountLong;
      var random = new System.Random();
      var calculationTimes = new List<double>();
      var stopwatch = new System.Diagnostics.Stopwatch();
      
      for (var i = 0; i < employeeCount; i++)
      {
        var skip = random.Next(totalEmployeeCount);
        var employee = totalEmployee.Skip(skip).FirstOrDefault();
        
        stopwatch.Restart();
        
        if (shiftDays)
          Calendar.Now.AddWorkingDays(employee, shift);
        else
          Calendar.Now.AddWorkingHours(employee, shift);
        
        stopwatch.Stop();
        calculationTimes.Add(Math.Round(stopwatch.Elapsed.TotalSeconds, 5));
      }
      
      var avarageTimes = calculationTimes.Sum() / calculationTimes.Count;
      
      return DirRX.NNCommons.Resources.WorkingTimeCalculationSpeedtestResultFormat(
        Math.Round(avarageTimes, 5),
        string.Join(Environment.NewLine, calculationTimes));
    }
    
    #endregion
    
    #endregion
    
    #region Работа с часовыми поясами.
    
    /// <summary>
    /// Синхронизировать часовой пояс сотрудника с одноимённым справочником.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Пустая строка при успешном выполнении. Иначе текст ошибки.</returns>
    [Public]
    public string SynchronizationTimeZone(OverrideSolution.IEmployee employee)
    {
      if (employee.Status == OverrideSolution.Employee.Status.Closed)
      {
        ExcludeEmployeeFromTimeZone(employee);
        return string.Empty;
      }
      
      var businessUnitUtcOffset = TimeZones.GetAll()
        .Where(z => z.Recipients.Any(r => r.Recipient.Equals(employee.BusinessUnit)))
        .Select(z => z.UtcOffset)
        .FirstOrDefault();
      
      if (businessUnitUtcOffset == null)
        return "Не удалось определить часовой пояс Нашей организации сотрудника";
      
      var equalsBusinessUnitTimeZone = businessUnitUtcOffset == employee.UtcOffsetDirRX;
      var employeeTimeZone = TimeZones.GetAll(z => z.Recipients.Any(r => r.Recipient.Equals(employee))).FirstOrDefault();
      var needSyncTimeZone = false;
      
      try
      {
        if (!equalsBusinessUnitTimeZone)
        {
          if (employeeTimeZone != null)
          {
            if (employeeTimeZone.UtcOffset != employee.UtcOffsetDirRX)
            {
              ExcludeEmployeeFromTimeZone(employeeTimeZone, employee);
              needSyncTimeZone = true;
            }
          }
          else
            needSyncTimeZone = true;
          
          if (needSyncTimeZone)
          {
            var targetTimeZone = TimeZones.GetAll(z => z.UtcOffset == employee.UtcOffsetDirRX).FirstOrDefault();
            if (targetTimeZone == null)
            {
              targetTimeZone = TimeZones.Create();
              targetTimeZone.Name = DirRX.NNCommons.Resources.TimeZoneNameTemplateFormat(employee.UtcOffsetDirRX);
              targetTimeZone.UtcOffset = employee.UtcOffsetDirRX;
            }
            
            var recipient = targetTimeZone.Recipients.AddNew();
            recipient.Recipient = employee;
            targetTimeZone.Save();
          }
        }
        else
        {
          if (employeeTimeZone != null)
            ExcludeEmployeeFromTimeZone(employeeTimeZone, employee);
        }
      }
      catch (Exception ex)
      {
        var error = string.Format("Не удалось синхронизировать часовой пояс. Ошибка: {0}.", ex);
        Logger.ErrorFormat("{0}. ИД Сотрудника {1}. {2}.", nameof(SynchronizationTimeZone), employee.Id, error);
        return error;
      }
      
      return string.Empty;
    }
    
    /// <summary>
    /// Исключает сотрудника из часового пояса.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <remarks>Если в часовом поясе был указан только один сотрудник, то пояс удаляется.</remarks>
    [Public]
    public void ExcludeEmployeeFromTimeZone(OverrideSolution.IEmployee employee)
    {
      var employeeTimeZone = TimeZones.GetAll(z => z.Recipients.Any(r => r.Recipient.Equals(employee))).FirstOrDefault();
      if (employeeTimeZone != null)
      {
        ExcludeEmployeeFromTimeZone(employeeTimeZone, employee);
      }
    }
    
    /// <summary>
    /// Исключает сотрудника из часового пояса.
    /// </summary>
    /// <param name="timeZone">Часовой пояс.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <remarks>Если в часовом поясе был указан только один сотрудник, то пояс удаляется.</remarks>
    [Public]
    public void ExcludeEmployeeFromTimeZone(Sungero.CoreEntities.ITimeZone timeZone, OverrideSolution.IEmployee employee)
    {
      if (timeZone.Recipients.Any(r => r.Recipient.Equals(employee)))
      {
        if (timeZone.Recipients.Count == 1)
          TimeZones.Delete(timeZone);
        else
        {
          var recipient = timeZone.Recipients.FirstOrDefault(r => r.Recipient.Equals(employee));
          timeZone.Recipients.Remove(recipient);
          timeZone.Save();
        }
      }
    }
    
    #endregion
    
    /// <summary>
    /// Возврат GUID-ов индивидуальных типов кадровых документов.
    /// </summary>
    /// <returns>Список Guid-ов типов документов.</returns>
    public List<Guid> GetHRDocumentTypes()
    {
      var hrDocumentTypes = new List<Guid>()
      {
        // Заявление на удержание (программа личных пожертвований).
        NNOtherProcesses.PublicConstants.Module.DocumentType.NNRetentionStatementDocument,
        // Простой кадровый документ.
        NNOtherProcesses.PublicConstants.Module.DocumentType.HRSimpleDocument,
        // Приказ на выплату по ЛНА.
        NNOtherProcesses.PublicConstants.Module.DocumentType.LrdPaymentOrderDocument,
        // Согласие на работу в ВД/ПД/СР.
        NNOtherProcesses.PublicConstants.Module.DocumentType.WeekendWorkAgreementDocument,
        // Заявление на выплату по ЛНА
        NNOtherProcesses.PublicConstants.Module.DocumentType.LrdPaymentStatementDocument,
        // Приказ (распоряжение) о работе в ВД/ПД/СР.
        NNOtherProcesses.PublicConstants.Module.DocumentType.WeekendWorkOrderDocument,
        // НН. Приказ на командировку.
        NNOtherProcesses.PublicConstants.Module.DocumentType.NNBusinessTripOrderDocument,
        // Документ, связанный с работой, коробочный guid.
        HRPro.HRDocFlow.PublicConstants.Module.EntityTypeGuid.DocRelatedToWork,
        // Извещение об отпуске, коробочный guid.
        HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationNoticeDocument,
        // Приказ об отпуске, коробочный guid.
        HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationOrderDocument,
        // Заявление и согласие по отпуску, коробочный guid.
        HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationStatementDocument,
        // Согласие сотрудника.
        HRPro.HRDocFlow.PublicConstants.Module.EntityTypeGuid.ConsentDocument,
        // Извещение работника работодателем при выплате заработной платы.
        HRPro.HRDocFlow.PublicConstants.Module.EntityTypeGuid.SalarySlip,
        // Заявление сотрудника.
        HRPro.HRDocFlow.PublicConstants.Module.EntityTypeGuid.StatementDocument,
        // Индивидуальный лист ознакомления.
        HRPro.Vacations.PublicConstants.Module.EntityTypeGuid.VacationIndividualAcquaintanceDocument,
        // Приказ по командировке.
        HRPro.BusinessTrips.PublicConstants.Module.EntityTypeGuid.BusinessTripOrder
      };

      return hrDocumentTypes;
    }
  }
}