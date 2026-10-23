using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using DirRX.NNIntegration;
using Sungero.Core;
using DirRX.NNOtherProcesses.NNRetentionStatementDocument;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Workflow;
using DirRX.NNIntegration.Structures.Module;
using DirRX.OverrideSolution;
using HRPro.HRDocFlow.Server;
using Sungero.Workflow.Task;
using Sungero.Domain.Shared;

namespace DirRX.NNOtherProcesses.Server
{
  public partial class ModuleFunctions
  {

    /// <summary>
    /// Проверить, что исполнитель задания - текущий пользователь ЛК или его совместитель.
    /// </summary>
    /// <param name="personId">Идентификатор персоны текущего пользователя.</param>
    /// <param name="assignment">Задание.</param>
    /// <returns>Если текущий пользователь исполнитель задания, либо задание выполнено - true, иначе - false.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public static bool CheckIfCurrentUserIsPerformer(long personId, long assignmentId)
    {
      var assignment = Sungero.Workflow.AssignmentBases.GetAll(a => a.Id == assignmentId).FirstOrDefault();
      return HRPro.HRProSolution.Employees.As(assignment.Performer).Person.Id == personId ||
        assignment.Status == Sungero.Workflow.AssignmentBase.Status.Completed;
    }

    /// <summary>
    /// Проверить результат выполнения задания.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <returns>Если свойство найдено - строка с результатом, иначе - пустая строка.</returns>
    [Public]
    public static string CheckAssignmentResult(Sungero.Workflow.IAssignmentBase assignment)
    {
      string assignmentResult;
      try
      {
        assignmentResult = ((Nullable<Enumeration>)(assignment.GetPropertyValue("Result")))?.Value;
      }
      catch
      {
        assignmentResult = string.Empty;
      }
      
      return assignmentResult;
    }

    /// <summary>
    /// Проверить пользователя, который выполнил задание.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <returns>Если свойство найдено - строка с пользователем, иначе - пустая строка.</returns>
    [Public]
    public static long? CheckAssignmentCompletedBy(Sungero.Workflow.IAssignmentBase assignment)
    {
      long? assignmentCompletedBy;
      try
      {
        var user = ((IUser)(assignment.GetPropertyValue("CompletedBy")));
        assignmentCompletedBy = HRPro.HRProSolution.Employees.As(user).Person.Id;
      }
      catch
      {
        assignmentCompletedBy = null;
      }
      
      return assignmentCompletedBy;
    }

    /// <summary>
    /// Проверить дату выполнения задания.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <returns>Если свойство найдено - строка с датой выполнения задания, иначе - пустая строка.</returns>
    [Public]
    public static DateTime? CheckAssignmentCompleted(Sungero.Workflow.IAssignmentBase assignment)
    {
      DateTime? assignmentCompleted;
      try
      {
        assignmentCompleted = ((Nullable<DateTime>)(assignment.GetPropertyValue("Completed")));
      }
      catch
      {
        assignmentCompleted = null;
      }
      
      return assignmentCompleted;
    }

    /// <summary>
    /// Возвращает значение свойства задания.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <param name="propertyName">Свойство.</param>
    /// <param name="defaultValue">Значение по умолчанию.</param>
    /// <returns>Значение свойства, иначе значение по умолчанию.</returns>
    public static bool TryGetPropertyValue(Sungero.Workflow.IAssignmentBase assignment, string propertyName, bool defaultValue)
    {
      bool result;
      try
      {
        result = (bool)assignment.GetPropertyValue(propertyName);
      }
      catch
      {
        result = defaultValue;
      }
      
      return result;
    }

    /// <summary>
    /// Получить даты периода для фильтрации заданий/уведомлений для виджета workItemList.
    /// </summary>
    /// <param name="personId">Ид персоны.</param>
    /// <returns>Период фильтрации.</returns>
    public static Structures.Module.AssignmentItemPeriod GetFilterPeriod(long personId)
    {
      var now = Calendar.Now;
      
      // Получить профиль пользователя ЛК из БД Ess.
      var essUserCurrentPeriod = DirRX.NNIntegration.PublicFunctions.Module.GetPersonEssCurrentPeriod(personId);
      
      if (essUserCurrentPeriod == null)
        return null;

      // Длина периода. Перечисление ЛК.
      // 0 - 30 дней (месяц)
      // 1 - 90 дней (квартал)
      // 2 - 365 дней (год)
      // 3 - произвольный период
      switch (essUserCurrentPeriod.Length)
      {
        case 0:
          return Structures.Module.AssignmentItemPeriod.Create(now.AddDays(-30), now.AddDays(1));
        case 1:
          return Structures.Module.AssignmentItemPeriod.Create(now.AddDays(-90), now.AddDays(1));
        case 2:
          return Structures.Module.AssignmentItemPeriod.Create(now.AddDays(-365), now.AddDays(1));
        case 3:
          return Structures.Module.AssignmentItemPeriod.Create(essUserCurrentPeriod.BeginDate, essUserCurrentPeriod.EndDate);
        default:
          return null;
      }
    }

    /// <summary>
    /// Возвращает список заданий/уведомлений для виджета workItemList.
    /// </summary>
    /// <param name="personId">Идентификатор персоны текущего пользователя.</param>
    /// <returns>Список заданий.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public static List<DirRX.NNOtherProcesses.Structures.Module.IHistoryAssignmentItem> GetHistoryAssignmentsForWorkItemList(long personId)
    {
      var results = new List<DirRX.NNOtherProcesses.Structures.Module.IHistoryAssignmentItem>();
      
      var personIds = new List<long>();

      var employeeIds = HRPro.HRProSolution.Employees.GetAll(e => e.Person.Id == personId &&
                                                             e.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
        .Select(e => e.Id)
        .ToList();
      
      var substitutionPersonIds = Sungero.CoreEntities.Substitutions
        .GetAll(s => employeeIds.Contains(s.Substitute.Id) &&
                s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active &&
                s.IsSystem == false &&
                ((s.StartDate == null || s.StartDate <= Calendar.Now) &&
                 (s.EndDate == null || s.EndDate >= Calendar.Now)))
        .Select(s => HRPro.HRProSolution.Employees.As(s.User).Person.Id)
        .Distinct().ToList();
      
      personIds.AddRange(substitutionPersonIds);
      
      if (!personIds.Contains(personId))
        personIds.Add(personId);
      
      // Получить период для фильтрации заданий/уведомлений для виджета workItemList.
      var filterPeriod = GetFilterPeriod(personId);

      foreach (var id in personIds)
      {
        var assignments = Sungero.Workflow.AssignmentBases.GetAll(a =>
                                                                  HRPro.HRProSolution.Employees.As(a.Performer).Person.Id == id &&
                                                                  (filterPeriod == null || (filterPeriod.StartDate <= a.Created && a.Created <= filterPeriod.EndDate)) &&
                                                                  ((a.Status == Sungero.Workflow.AssignmentBase.Status.Completed && a.IsRead == true) ||
                                                                   a.Status == Sungero.Workflow.AssignmentBase.Status.Aborted));
        foreach (var assignment in assignments)
        {
          if (!(DirRX.EssPlatform.PublicFunctions.Module.CheckAssignmentShowInSelfServiceOffice(assignment)))
            continue;
          
          var completedBy = DirRX.NNOtherProcesses.Server.ModuleFunctions.CheckAssignmentCompletedBy(assignment);
          if (completedBy != null && personId != completedBy)
            continue;
          
          var completed = DirRX.NNOtherProcesses.Server.ModuleFunctions.CheckAssignmentCompleted(assignment);
          var finished = (completed != null && completed.HasValue) ? completed : assignment.Modified;
          
          var assignmentItem = DirRX.NNOtherProcesses.Structures.Module.HistoryAssignmentItem.Create();
          
          assignmentItem.finished = finished?.ToString("yyyy-MM-ddTHH:mm:ss");

          assignmentItem.title = assignment.Subject;
          assignmentItem.system = "HRPro";
          assignmentItem.name = $"I{assignment.GetEntityMetadata().IntegrationServiceName}s";
          assignmentItem.id = assignment.Id;
          assignmentItem.deadline = assignment.Deadline?.ToString("yyyy-MM-ddTHH:mm:ss");
          assignmentItem.isRead = assignment.IsRead ?? false;
          assignmentItem.isExpired = assignment.Deadline.HasValue && assignment.Deadline.Value < Calendar.Now;
          
          assignmentItem.status = assignment.Status?.ToString();
          assignmentItem.assignee = assignment.Performer.Name;
          
          assignmentItem.icon = HRPro.HRDocFlow.HRNotices.Is(assignment) || DirRX.SignPlatform.CertificateIssueNotices.Is(assignment) ? "read-document" : "clock";
          
          assignmentItem.defaultActionCaption = string.Empty;
          assignmentItem.defaultActionIcon = string.Empty;
          assignmentItem.defaultActionName = string.Empty;
          
          var result = DirRX.NNOtherProcesses.Server.ModuleFunctions.CheckAssignmentResult(assignment);
          assignmentItem.bySubstitution = id != personId;
          
          if (!string.IsNullOrEmpty(result))
          {
            if (HRPro.Vacations.VacationEmployeeSchedulingAssignments.Is(assignment))
              assignmentItem.result =
                result == "Complete" || result == "Completed" ? "Выполнено" : string.Empty;
            
            else if (HRPro.Vacations.VacationApprovalAssignments.Is(assignment))
              assignmentItem.result =
                result == "Complete" ? "Согласовано" :
                result == "ForRework" ? "Отправлено на доработку" :
                result == "Deny" ? "Отказано" : string.Empty;
            
            else if (HRPro.Vacations.VacationReworkAssignments.Is(assignment))
              assignmentItem.result =
                result == "ForReapproving" ? "Выполнено" :
                result == "Abort" ? "Прекращено" : string.Empty;
            
            else if (HRPro.Vacations.VacationChangeApprovalAssignments.Is(assignment))
              assignmentItem.result =
                result == "Approve" ? "Согласовано" :
                result == "ForRework" ? "Отправлено на доработку" :
                result == "Deny" ? "Отказано" : string.Empty;
            
            else if (HRPro.Vacations.VacationChangeReworkAssignments.Is(assignment))
              assignmentItem.result =
                result == "ForReapproving" ? "Выполнено" :
                result == "Abort" ? "Прекращено" : string.Empty;
            
            else if (HRPro.Vacations.VacationChangeSignAssignments.Is(assignment))
              assignmentItem.result =
                result == "Sign" ? "Подписано" :
                result == "Refuse" ? "Отказано" : string.Empty;
            
            else if (HRPro.Vacations.VacationConfirmationAssignments.Is(assignment))
              assignmentItem.result =
                result == "Complete" ? "Подтверждено" :
                result == "Shift" ? "Выполнено" : string.Empty;
            
            else if (HRPro.Vacations.VacationAlignDateAssignments.Is(assignment))
              assignmentItem.result =
                result == "GetVacation" ? "Выполнено" :
                result == "Skip" ? "Выполнено" : string.Empty;
            
            else if (DirRX.NNVacations.VacationConfirmationAssignments.Is(assignment))
              assignmentItem.result =
                result == "Confirm" ? "Подтверждено" : string.Empty;
            
            else if (HRPro.HRDocFlow.ConsentAssignments.Is(assignment) || HRPro.HRDocFlow.ESignAssignments.Is(assignment) || HRPro.HRDocFlow.HRAssignments.Is(assignment) ||
                     HRPro.HRDocFlow.EAcquaintanceAssignments.Is(assignment) || HRPro.HRDocFlow.DocumentApprovalAssignments.Is(assignment) || HRPro.HRDocFlow.AgreementAssignments.Is(assignment))
              assignmentItem.result =
                result == "Complete" ? "Выполнено" :
                result == "Approved" ? "Согласовано" :
                result == "ForRework" ? "На доработку" :
                result == "ForRevision" ? "На доработку" :
                result == "Aborted" ? "Прекращено" :
                result == "Terminate" ? "Прекращено" :
                result == "Refuse" ? "Отказано" :
                result == "Reject" ? "Отказано" :
                result == "Sign" ? "Подписано" :
                result == "Accept" ? "Принято" : string.Empty;
            
            else if (DirRX.NNCommons.StatementReworkAssignments.Is(assignment) || DirRX.OverrideSolution.NNStatementReworkAssignments.Is(assignment))
              assignmentItem.result =
                result == "ForReapproving" ? "Выполнено" :
                result == "Abort" ? "Прекращено" : string.Empty;
            
            else if (DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.Is(assignment) || DirRX.SignPlatform.GoskeyInstallAssignments.Is(assignment))
              assignmentItem.result =
                result == "Accept" ? "Подписано" :
                result == "Refuse" ? "Отказано" : string.Empty;
          }

          results.Add(assignmentItem);
        }
      }

      return results.OrderByDescending(a => a.finished).ToList();
    }

    /// <summary>
    /// Возвращает список заданий/уведомлений для виджета workItemList.
    /// </summary>
    /// <param name="personId">Идентификатор персоны текущего пользователя.</param>
    /// <returns>Список заданий.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public static List<DirRX.NNOtherProcesses.Structures.Module.IAssignmentItem> GetAssignmentsForWorkItemList(long personId)
    {
      var results = new List<DirRX.NNOtherProcesses.Structures.Module.IAssignmentItem>();
      var assignments = Sungero.Workflow.AssignmentBases.GetAll(a => HRPro.HRProSolution.Employees.As(a.Performer).Person.Id == personId &&
                                                                (a.Status == Sungero.Workflow.AssignmentBase.Status.InProcess ||
                                                                 (a.Status == Sungero.Workflow.AssignmentBase.Status.Completed &&
                                                                  a.IsRead == false)));
      foreach (var assignment in assignments)
      {
        if (!(DirRX.EssPlatform.PublicFunctions.Module.CheckAssignmentShowInSelfServiceOffice(assignment)))
          continue;

        var assignmentItem = DirRX.NNOtherProcesses.Structures.Module.AssignmentItem.Create();
        
        assignmentItem.title = assignment.Subject;
        assignmentItem.system = "HRPro";
        assignmentItem.name = $"I{assignment.GetEntityMetadata().IntegrationServiceName}s";
        assignmentItem.id = assignment.Id;
        assignmentItem.icon = "clock";
        assignmentItem.created = assignment.Created?.ToString("yyyy-MM-ddTHH:mm:ssZ");
        assignmentItem.deadline = assignment.Deadline?.ToString("yyyy-MM-ddTHH:mm:ssZ");
        assignmentItem.isRead = assignment.IsRead ?? false;
        assignmentItem.isExpired = assignment.Deadline.HasValue && assignment.Deadline.Value < Calendar.Now;
        
        assignmentItem.status = assignment.Info.Properties.Status.GetLocalizedValue(assignment.Status);
        assignmentItem.completed = DirRX.NNOtherProcesses.Server.ModuleFunctions.CheckAssignmentCompleted(assignment)?.ToString("yyyy-MM-ddTHH:mm:ssZ");
        assignmentItem.result = DirRX.NNOtherProcesses.Server.ModuleFunctions.CheckAssignmentResult(assignment);
        
        assignmentItem.defaultActionCaption = string.Empty;
        assignmentItem.defaultActionIcon = string.Empty;
        assignmentItem.defaultActionName = string.Empty;
        
        // Vacations
        if (HRPro.Vacations.VacationEmployeeSchedulingAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Complete";
          assignmentItem.defaultActionCaption = "Спланировать";
          
        }
        else if (HRPro.Vacations.VacationApprovalAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Complete";
          assignmentItem.defaultActionCaption = "Согласовать";
          
        }
        else if (HRPro.Vacations.VacationReworkAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Rework";
          assignmentItem.defaultActionCaption = "Доработать";
        }
        else if (HRPro.Vacations.VacationChangeApprovalAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Approve";
          assignmentItem.defaultActionCaption = "Согласовать";
        }
        else if (HRPro.Vacations.VacationChangeReworkAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "ForReapproving";
          assignmentItem.defaultActionCaption = "Доработать";
        }
        else if (HRPro.Vacations.VacationChangeSignAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Sign";
          assignmentItem.defaultActionCaption = "Подписать";
        }
        else if (HRPro.Vacations.VacationConfirmationAssignments.Is(assignment) || DirRX.NNVacations.VacationConfirmationAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Complete";
          assignmentItem.defaultActionCaption = "Подтвердить";
        }
        else if (HRPro.Vacations.VacationAlignDateAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "GetVacation";
          assignmentItem.defaultActionCaption = "Оформить отпуск";
        }
        
        // HRProStatements
        else if (HRPro.HRDocFlow.AgreementAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Accept";
          assignmentItem.defaultActionCaption = "Принять";
        }
        else if (HRPro.HRDocFlow.HRAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Complete";
          assignmentItem.defaultActionCaption = "Выполнить";
        }
        else if (HRPro.HRDocFlow.DocumentApprovalAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Sign";
          assignmentItem.defaultActionCaption = "Согласовать";
        }
        else if (HRPro.HRDocFlow.ConsentAssignments.Is(assignment) || HRPro.HRDocFlow.ESignAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Sign";
          assignmentItem.defaultActionCaption = "Подписать";
        }
        else if (HRPro.HRDocFlow.EAcquaintanceAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Acquaint";
          assignmentItem.defaultActionCaption = "Ознакомлен";
        }
        else if (HRPro.HRDocFlow.HRNotices.Is(assignment))
        {
          assignmentItem.icon = "read-document";
        }
        else if (DirRX.NNCommons.StatementReworkAssignments.Is(assignment) || DirRX.OverrideSolution.NNStatementReworkAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Rework";
          assignmentItem.defaultActionCaption = "Доработать";
        }
        
        //SignPlatform
        else if (DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Sign";
          assignmentItem.defaultActionCaption = "Подписать";
        }
        else if (DirRX.SignPlatform.CertificateIssueNotices.Is(assignment))
        {
          assignmentItem.icon = "read-document";
        }
        else if (DirRX.SignPlatform.GoskeyInstallAssignments.Is(assignment))
        {
          assignmentItem.defaultActionName = "Complete";
          assignmentItem.defaultActionCaption = "Выполнить";
        }
        
        results.Add(assignmentItem);
      }
      
      return results.OrderByDescending(a => a.created).ToList();
    }

    /// <summary>
    /// Возвращает список заданий/уведомлений по замещению для виджета workItemList.
    /// </summary>
    /// <param name="personId">Идентификатор персоны текущего пользователя.</param>
    /// <returns>Список заданий по замещению.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public static List<DirRX.NNOtherProcesses.Structures.Module.ISubstitutionAssignmentItem> GetSubstitutionAssignmentsForWorkItemList(long personId)
    {
      var results = new List<DirRX.NNOtherProcesses.Structures.Module.ISubstitutionAssignmentItem>();
      
      var employeeIds = HRPro.HRProSolution.Employees.GetAll(e => e.Person.Id == personId &&
                                                             e.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
        .Select(e => e.Id)
        .ToList();
      
      var substitutedUsers = Sungero.CoreEntities.Substitutions.GetAll(s => employeeIds.Contains(s.Substitute.Id) &&
                                                                       s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active &&
                                                                       s.IsSystem == false &&
                                                                       ((s.StartDate == null || s.StartDate <= Calendar.Now) &&
                                                                        (s.EndDate == null || s.EndDate >= Calendar.Now)))
        .Select(s => s.User)
        .Distinct();

      if (!substitutedUsers.Any())
        return results;

      // Получаем задания только в статусе InProcess
      foreach (var user in substitutedUsers)
      {
        var assignments = Sungero.Workflow.AssignmentBases.GetAll(a => a.Performer.Equals(user) &&
                                                                  HRPro.HRProSolution.Employees.As(a.Performer).Person.Id != personId &&
                                                                  (a.Status == Sungero.Workflow.AssignmentBase.Status.InProcess ||
                                                                   (a.Status == Sungero.Workflow.AssignmentBase.Status.Completed &&
                                                                    a.IsRead == false)));
        foreach (var assignment in assignments)
        {
          
          if (!(DirRX.EssPlatform.PublicFunctions.Module.CheckAssignmentShowInSelfServiceOffice(assignment)))
            continue;
          
          var isSpecial = false;
          var isVisible = true;
          var isExecutable = true;

          if (DirRX.OverrideSolution.ESignAssignments.Is(assignment))
          {
            isSpecial = true;
            isVisible = TryGetPropertyValue(assignment, "IsSubstitutionVisibleDirRX", true);
            isExecutable = TryGetPropertyValue(assignment, "IsSubstitutionAllowedDirRX", true);
          }
          else if (DirRX.NNCommons.StatementReworkAssignments.Is(assignment))
          {
            isSpecial = true;
            isVisible = TryGetPropertyValue(assignment, "IsSubstitutionVisible", true);
            isExecutable = TryGetPropertyValue(assignment, "IsSubstitutionAllowed", true);
          }
          else if (DirRX.NNVacations.VacationConfirmationAssignments.Is(assignment))
          {
            isSpecial = true;
            isVisible = TryGetPropertyValue(assignment, "IsSubstitutionVisible", true);
            isExecutable = TryGetPropertyValue(assignment, "IsSubstitutionAllowed", true);
          }

          if (isSpecial && !isVisible)
            continue;

          var substitutionAssignmentItem = DirRX.NNOtherProcesses.Structures.Module.SubstitutionAssignmentItem.Create();
          
          substitutionAssignmentItem.title = assignment.Subject;
          substitutionAssignmentItem.system = "HRPro";
          substitutionAssignmentItem.name = $"I{assignment.GetEntityMetadata().IntegrationServiceName}s";
          substitutionAssignmentItem.id = assignment.Id;
          substitutionAssignmentItem.icon = Sungero.Workflow.Assignments.Is(assignment) ? "clock" : "read-document";
          substitutionAssignmentItem.assignee = assignment.Performer.Name;
          substitutionAssignmentItem.created = assignment.Created?.ToString("yyyy-MM-ddTHH:mm:ssZ");
          substitutionAssignmentItem.deadline = assignment.Deadline?.ToString("yyyy-MM-ddTHH:mm:ssZ");
          substitutionAssignmentItem.isRead = assignment.IsRead ?? false;
          substitutionAssignmentItem.isExpired = assignment.Deadline.HasValue && assignment.Deadline.Value < Calendar.Now;;
          substitutionAssignmentItem.isExecutable = isExecutable;
          
          results.Add(substitutionAssignmentItem);
        }
      }

      return results.OrderByDescending(a => a.created).ToList();
    }

    #region Общие функции.

    #region Работа с XML.

    /// <summary>
    /// Сериализация структуры в xml строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <param name="errorMessage">Ошибка.</param>
    /// <typeparam name="T">Тип структуры.</typeparam>
    /// <returns>Строка xml.</returns>
    private string SerializeInXml<T>(T structure, out string errorMessage)
    {
      errorMessage = string.Empty;
      try
      {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using (var stringWriter = new StringWriter())
        {
          using (var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = System.Xml.Formatting.Indented })
          {
            xmlSerializer.Serialize(xmlTextWriter, (T)structure);
            return stringWriter.ToString();
          }
        }
      }
      catch (Exception e)
      {
        errorMessage = "Произошла ошибка сериализации структуры в xml строку.";
        Logger.ErrorFormat("SerializeInXml. {0} Ошибка: {1}. StackTrace: {2}.", errorMessage, e.Message, e.StackTrace);
      }

      return string.Empty;
    }

    #endregion

    /// <summary>
    /// Создать и отправить подзадачу на подписание документа.
    /// </summary>
    /// <param name="mainTaskID">ID главной задачи.</param>
    /// <param name="employee">Автор задачи.</param>
    /// <param name="document">Документ.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    /// <remarks>Создание и отправка задачи выполняется в транзакции.</remarks>
    private string CreateAndStartNewSignDocumentSubtask(long mainTaskID, OverrideSolution.IEmployee employee, OverrideSolution.IOfficialDocument document)
    {
      var logPrefix = string.Format("CreateAndStartNewSignDocumentSubtask. ИД документа {0}. ИД основной задачи {1}", document.Id, mainTaskID);
      
      var exsitingTaskId = GetExistingHRProcessTaskId(document.Id);
      if (exsitingTaskId > 0)
      {
        Logger.DebugFormat("{0}. Найдена задача на подписание приказа в работе с ИД {1}. Повторная отправка документа на подписание не требуется.", logPrefix, exsitingTaskId);
        return string.Empty;
      }
      
      var executionResult = false;
      var errorMessage = string.Empty;
      executionResult = Transactions.Execute(
        () =>
        {
          var mainTask = Sungero.Workflow.Tasks.GetAll(x => x.Id == mainTaskID).FirstOrDefault();
          if (mainTask == null)
          {
            errorMessage = string.Format("Не найдена ведущая задача по ИД {0}.", mainTaskID);
            return;
          }
          
          try
          {
            var subTask = HRPro.HRDocFlow.HRProcessTasks.CreateAsSubtask(mainTask);
            subTask.Author = employee;
            if (employee.BusinessUnit != null)
              subTask.BusinessUnit = employee.BusinessUnit;
            subTask.NeedApprove = true;
            subTask.NeedAcquaintance = false;
            subTask.DocumentGroup.OfficialDocuments.Add(document);
            
            subTask.Start();

            Logger.DebugFormat("{0}. Отправлена новая задача на подписание документа с ИД: {1}.", logPrefix, subTask.Id);
          }
          catch (Exception e)
          {
            errorMessage = string.Format("Произошла ошибка отправки задачи на подписание приказа. Текст ошибки: {0}.", e.Message);
            Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, errorMessage, e.StackTrace);
            
          }
        });
      
      if (!executionResult || !string.IsNullOrEmpty(errorMessage))
      {
        if (string.IsNullOrEmpty(errorMessage))
        {
          errorMessage = "Произошла ошибка отправки задачи на подписание приказа на командировку. Транзакция не выполнена.";
          Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        }
        return errorMessage;
      }
      else
        return string.Empty;
    }
    
    /// <summary>
    /// Создать и отправить задачу на подписание документа.
    /// </summary>
    /// <param name="employee">Автор задачи.</param>
    /// <param name="document">Документ.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    /// <remarks>Создание и отправка задачи выполняется в транзакции.</remarks>
    private string CreateAndStartNewSignDocumentTask(OverrideSolution.IEmployee employee, OverrideSolution.IOfficialDocument document)
    {
      var logPrefix = string.Format("CreateAndStartNewSignDocumentTask. ИД документа {0}.", document.Id);
      
      var exsitingTaskId = GetExistingHRProcessTaskId(document.Id);
      if (exsitingTaskId > 0)
      {
        Logger.DebugFormat("{0}. Найдена задача на подписание приказа в работе с ИД {1}. Повторная отправка документа на подписание не требуется.", logPrefix, exsitingTaskId);
        return string.Empty;
      }
      
      var executionResult = false;
      var errorMessage = string.Empty;
      executionResult = Transactions.Execute(
        () =>
        {
          try
          {
            var task = HRPro.HRDocFlow.HRProcessTasks.Create();
            task.Author = employee;
            if (employee.BusinessUnit != null)
              task.BusinessUnit = employee.BusinessUnit;
            task.NeedApprove = true;
            task.NeedAcquaintance = false;
            task.DocumentGroup.OfficialDocuments.Add(document);
            
            task.Start();

            Logger.DebugFormat("{0}. Отправлена новая задача на подписание документа с ИД: {1}.", logPrefix, task.Id);
          }
          catch (Exception e)
          {
            errorMessage = string.Format("Произошла ошибка отправки задачи на подписание приказа. Текст ошибки: {0}.", e.Message);
            Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, errorMessage, e.StackTrace);
            
          }
        });
      
      if (!executionResult || !string.IsNullOrEmpty(errorMessage))
      {
        if (string.IsNullOrEmpty(errorMessage))
        {
          errorMessage = "Произошла ошибка отправки задачи на подписание приказа. Транзакция не выполнена.";
          Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        }
        return errorMessage;
      }
      else
        return string.Empty;
    }
    /// <summary>
    /// Получить ИД существующей задачи на обработку документов, связанных с работой.
    /// </summary>
    /// <param name="documentId">ИД Документа.</param>
    /// <returns>ИД Задачи.</returns>
    private long GetExistingHRProcessTaskId(long documentId)
    {
      return HRPro.HRDocFlow.HRProcessTasks.GetAll()
        .Where(x => x.MainDoc.Id == documentId && x.Status == Status.InProcess)
        .Select(x => x.Id)
        .FirstOrDefault();
    }
    
    #endregion

    #region Интеграционный сценарий «Участие в благотворительности».

    #region IF-S-10397. Автоматическое создание карточки документа из АСУП в КЭДО (АСУП -> КЭДО).

    /// <summary>
    /// Обработка создания или обновления карточки "Заявление на удержание (программа личных пожертвований)".
    /// </summary>
    /// <returns>Результат обработки.</returns>
    [Public]
    public IProcessIncomingMessageResult ProcessingCreateRetentionStatementDocument(long idIncomingMessage)
    {
      var result = ProcessIncomingMessageResult.Create();
      var logPrefix = string.Format("ProcessingCreateRetentionStatementDocument. Ид записи: {0}", idIncomingMessage);
      var errorMessage = string.Empty;
      var retry = false;
      DateTime? processingDate = null;
      var model = GetStructureFromIncomingMessage(idIncomingMessage, out errorMessage, out retry, out processingDate);

      if (!string.IsNullOrEmpty(errorMessage))
      {
        result.ErrorText = errorMessage;
        result.Retry = retry;
        return result;
      }
      
      var employee = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(model.Employee)).FirstOrDefault();

      if (employee == null)
      {
        result.ErrorText = string.Format("Не найден сотрудник с табельным номером: {0}.", model.Employee);
        result.Retry = false;
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }

      var document = DirRX.NNOtherProcesses.NNRetentionStatementDocuments.GetAll(c => c.ExternalId.Equals(model.SAPID)).FirstOrDefault();
      document = document ?? NNRetentionStatementDocuments.Create();
      result = UpdateOrCreateRetentionStatementDocument(model, employee, document, processingDate);
      result.EntityId = document?.Id;

      if (string.IsNullOrEmpty(result.ErrorText))
      {
        var task = NNIntegration.ExternalSystemDataExchangeTasks.Create();
        Logger.DebugFormat("{0}. Создана задача с ИД: {1}.", logPrefix, task.Id);
        var processId = DirRX.NNCommons.PublicFunctions.Module.GetIdentifierValue(DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.SendRetentionStatementStatusProcessKindId.Guid);
        var process = ProcessKinds.GetAllMatches(task).FirstOrDefault(c => c.Id == processId);

        if (process == null)
        {
          result.ErrorText = string.Format("Действующий вариант процесса для типа задачи 'Обмен данными с внешней системой' с ИД: {0} не найден. Проверьте значение в константе \"{1}\"",
                                           processId, DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.SendRetentionStatementStatusProcessKindId.Guid);
          result.Retry = false;
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;
        }

        task.Subject = DirRX.NNOtherProcesses.Resources.SubjectTaskSendRetentionStatementStatusFormat(document.Name);
        task.ProcessKind = process;
        task.Documents.AddNew().Document = document;
        try
        {
          task.Start();
          Logger.DebugFormat("{0}. Отправлена задача с ИД: {1}.", logPrefix, task.Id);
        }
        catch (Exception ex)
        {
          result.ErrorText = string.Format("Не удалось запустить задачу на передачу статуса в АСУП. Текст ошибки: {0} StackTrace: {1}.", ex.Message, ex.StackTrace);
          result.Retry = false;
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;;
        }
      }

      if (string.IsNullOrEmpty(result.ErrorText))
      {
        Logger.DebugFormat("{0}. Документ SAPID: {1} успешно обработан.", logPrefix, model.SAPID);
      }
      return result;
    }

    /// <summary>
    /// Получить струткуру из записи 'Входящие информационные сообщения'.
    /// </summary>
    /// <param name="idIncomingMessage">Ид записи 'Входящие информационные сообщения'.</param>
    /// <param name="errorMessage">Ошибка.</param>
    /// <param name="isRestart">True если нужен перезапуск, иначе false.</param>
    /// <param name="processingDate">Дата последней обработки.</param>
    /// <returns>Структура.</returns>
    private IRetentionStatementDocumentCard GetStructureFromIncomingMessage(long idIncomingMessage, out string errorMessage,
                                                                            out bool isRestart, out DateTime? processingDate)
    {
      isRestart = false;
      errorMessage = string.Empty;
      processingDate = null;
      var logPrefix = string.Format("GetStructureFromIncomingMessage. incomingMessageId = {0}", idIncomingMessage);
      var message = IncomingMessages.GetAll(c => c.Id == idIncomingMessage).FirstOrDefault();

      if (message == null)
      {
        errorMessage = string.Format("Не найдена запись в справочнике 'Входящие информационные сообщения' c ID {0}.", idIncomingMessage);
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return null;
      }
      
      processingDate = message.ProcessingDate;
      var jsonMessage = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(message);

      if (string.IsNullOrEmpty(jsonMessage))
      {
        errorMessage = string.Format("Не удалось получить тело запроса записи из справочника 'Входящие информационные сообщения' c ID {0}.", idIncomingMessage);
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        isRestart = true;
        return null;
      }
      
      IRetentionStatementDocumentCard model = null;
      
      try
      {
        model = DirRX.NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      
      catch (Exception ex)
      {
        isRestart = true;
        errorMessage = "Ошибка десериализации.";
        Logger.ErrorFormat("{0}. {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorMessage, ex.Message, ex.StackTrace);
        return null;
      }

      if (model != null)
        return model;
      
      isRestart = true;

      return null;
    }

    /// <summary>
    /// Создать или обновить документ "Заявление на удержание".
    /// </summary>
    /// <param name="model">Структура с данными.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="document">Документ.</param>
    /// <param name="processingDate">Дата последней обработки.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    [Public]
    public IProcessIncomingMessageResult UpdateOrCreateRetentionStatementDocument(DirRX.NNIntegration.Structures.Module.IRetentionStatementDocumentCard model, DirRX.OverrideSolution.IEmployee employee, INNRetentionStatementDocument document, DateTime? processingDate)
    {
      var result = ProcessIncomingMessageResult.Create();
      
      if (!Locks.TryLock(document))
      {
        var locksInfo = Locks.GetLockInfo(document);
        result.ErrorText = string.Format("ИД документа: {0}. Текст ошибки: Не удалось заблокировать документ. Заблокирован: {1}. Время установки блокировки: {2}.", document.Id, locksInfo.OwnerName, locksInfo.LockTime);
        result.Retry = true;
        Logger.ErrorFormat("UpdateOrCreateRetentionStatementDocument. {0}.", result.ErrorText);
        return result;
      }

      try
      {
        if (document.State.IsInserted || !document.HasVersions || processingDate == null || document.LastVersion.Created > processingDate)
          result.ErrorText = FillDocumentProperties(document, model, employee);

        var lastVersionIsSigned = Signatures.Get(document.LastVersion,
                                                 q => q.Where(x => x.Comment == DirRX.NNOtherProcesses.Resources.ApproveComment &&
                                                              x.SignatureType == SignatureType.Approval)).FirstOrDefault() != null;
        if (!lastVersionIsSigned)
        {
          try
          {
            // HACK отключено событие до подписание, так как подписывает служебный пользователь и не нужно заполнять поля.
            using (EntityEvents.Disable(HRPro.HRProSolution.OfficialDocuments.Info.Events.BeforeSigning))
              Signatures.Approve(document.LastVersion, null, DirRX.NNOtherProcesses.Resources.ApproveComment);
          }
          catch (Exception ex)
          {
            result.ErrorText = string.Format("Произошла ошибка подписания документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", document.Id, ex.Message, ex.StackTrace);
            Logger.ErrorFormat("UpdateOrCreateRetentionStatementDocument. {0}", result.ErrorText);
            return result;
          }
        }
        
        var docCollection = new List<Sungero.Docflow.IOfficialDocument>()
        {
          Sungero.Docflow.OfficialDocuments.As(document)
        };
        
        HRPro.HRDocFlow.PublicFunctions.Module.CreateOrUpdateEmployeeDocumentLink(employee, docCollection, HRPro.HRDocFlow.EmployeeDocumentLink.Status.Active);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", document.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("UpdateOrCreateRetentionStatementDocument. {0}", result.ErrorText);
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }

      return result;
    }

    /// <summary>
    /// Заполнить свойства документа.
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="structure">Структура с данными документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    private string FillDocumentProperties(INNRetentionStatementDocument document, IRetentionStatementDocumentCard structure, DirRX.OverrideSolution.IEmployee employee)
    {
      var isInserted = document.State.IsInserted;
      
      if (document.State.IsInserted)
      {
        document.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DirRX.NNOtherProcesses.PublicConstants.Module.DocumentKind.RetentionStatementDocument);
        // В HR св-во HRLifeCycleStateHRPro продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
        // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
        // Используется в HRLifeCycleStateHRProChanged официального документа.
        document.HRLifeCycleStateHRPro = NNOtherProcesses.NNRetentionStatementDocument.HRLifeCycleStateHRPro.Active;
        document.BusinessUnit = employee.BusinessUnit;
        document.Department = employee.Department;
        document.RegistrationDate = Calendar.Today;
        document.InternalApprovalState = InternalApprovalState.Approved;
      }
      
      if (isInserted || !document.ProgramId.Equals(structure.ProgramID))
        document.ProgramId = structure.ProgramID;
      if (isInserted || !document.Program.Equals(structure.ProgramName))
        document.Program = structure.ProgramName;
      if (isInserted || !Employees.Equals(document.EmployeeDirRX, employee))
        document.EmployeeDirRX = employee;
      if (isInserted || document.PreparationDate != structure.Date)
        document.PreparationDate = structure.Date;
      if (isInserted || document.Login.Equals(structure.Login))
        document.Login = structure.Login;
      if (isInserted || document.SignDate != structure.DateSigned)
        document.SignDate = structure.DateSigned;
      if (isInserted || !document.ExternalId.Equals(structure.SAPID))
        document.ExternalId = structure.SAPID;

      var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.RetentionStatementDocumentExtension.Guid);

      if (string.IsNullOrEmpty(extension))
        return "Прикладная константа 'Интеграция. Формат файла для документа \"Заявление на удержание\", принимаемый из АСУП' не заполнена.";
      
      // Документ сохраняется во время создания новой версии.
      var error = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, structure.Body, extension);

      return !string.IsNullOrEmpty(error) ? error : string.Empty;
    }

    #endregion

    #region IF-S-10398. Автоматическое формирование задания на ручное разблокирование ИТ0015 в АСУП (АСУП -> КЭДО).

    /// <summary>
    /// Процесс создания задания на ручное проведение мероприятия.
    /// </summary>
    /// <param name="incomingMessageId">ИД записи входящих сообщений.</param>
    /// <returns>Структура ответа.</returns>
    [Public]
    public IProcessIncomingMessageResult ProcessManualHoldingEvent(long incomingMessageId)
    {
      var logPrefix = string.Format("ProcessManualHoldingEvent. Ид входящей записи: {0}", incomingMessageId);
      var result = ProcessIncomingMessageResult.Create();
      result.Retry = false;
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == incomingMessageId).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.ErrorText = string.Format("Не найдена запись справочника 'Входящие интеграционные сообщения'.");
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      IManualHoldingEventStruct structure = null;
      
      try
      {
        structure = DirRX.NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Ошибка десериализации строки в структуру.";
        Logger.ErrorFormat("{0}. {1} Текст ошибки: {2} StackTrace: {3}.", logPrefix, result.ErrorText, ex.Message, ex.StackTrace);
        return result;
      }
      
      var doc = DirRX.NNOtherProcesses.NNRetentionStatementDocuments.GetAll(c => c.ExternalId.Equals(structure.SAPID)).FirstOrDefault();

      if (doc == null)
      {
        result.ErrorText = string.Format("Не удалось найти документ с SAPID: {0}.", structure.SAPID);
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }

      result.ErrorText = NNOtherProcesses.PublicFunctions.NNRetentionStatementDocument.CreateManualHoldingEvent(doc);
      result.EntityId = doc.Id;
      return result;
    }


    #endregion

    #endregion

    #region Интеграционный сценарий «Оформление командировки».

    #region IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП.

    /// <summary>
    /// Обработка создания приказа на командировку.
    /// </summary>
    /// <param name="incomingMessageId">ИД записи справочника 'Входящие информационные сообщения'.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public IProcessIncomingMessageResult ProcessCreateBusinessTripOrder(long incomingMessageId)
    {
      var logPrefix = string.Format("ProcessCreateBusinessTripOrder. Ид записи справочника 'Входящие информационные сообщения': {0}", incomingMessageId);
      var result = ProcessIncomingMessageResult.Create();
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == incomingMessageId).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.ErrorText = "Не найдена запись в справочнике 'Входящие информационные сообщения'.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      ICreateBusinessTripOrder structure = null;
      
      try
      {
        structure = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Ошибка десериализации. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, result.ErrorText,  ex.StackTrace);
        return result;
      }
      
      var employee = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Employee)).FirstOrDefault();
      var ourSignatory = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Supervisor)).FirstOrDefault();
      var employeeWhoPrepared = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Prepared)).FirstOrDefault();

      if (employee == null || ourSignatory == null || employeeWhoPrepared == null)
      {
        if (employee == null)
          result.ErrorText += string.Format("Не найден сотрудник с табельным номером: {0}. ", structure.Employee);
        if (ourSignatory == null)
          result.ErrorText += string.Format("Не найден подписывающий с табельным номером: {0}. ", structure.Supervisor);
        if (employeeWhoPrepared == null)
          result.ErrorText += string.Format("Не найден сотрудник подготовивший документ с табельным номером: {0}. ", structure.Prepared);

        result.ErrorText = result.ErrorText.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var document = DirRX.NNOtherProcesses.NNBusinessTripOrderDocuments.GetAll(c => c.ExternalId.Equals(structure.SAPID)).FirstOrDefault();
      result.EntityId = document?.Id;
      
      if (document != null)
      {
        var checkResult = OverrideSolution.PublicFunctions.DocRelatedToWorkBase.CanUpdateDocFromASUP(document);
        if (!string.IsNullOrEmpty(checkResult))
        {
          result.ErrorText = string.Format("Версия документа с ID: {0} не создана. {1}.", document.Id, checkResult);
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;
        }
      }
      
      document = document ?? NNBusinessTripOrderDocuments.Create();
      result = UpdateOrCreateBusinessTripOrder(document, structure, employee, ourSignatory, employeeWhoPrepared);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return result;

      result.ErrorText = SendTaskToSignBusinessTripOrder(document, employeeWhoPrepared);
      
      if (string.IsNullOrEmpty(result.ErrorText))
        Logger.DebugFormat("{0}. Успешно обработан.", logPrefix);
      
      result.EntityId = document?.Id;
      return result;
    }

    /// <summary>
    /// Создать или обновить документ "Приказ на командировку".
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="structure">Структура с данными документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="ourSignatory">Подписывающий сотрудник.</param>
    /// <param name="preparedBy">Сотрудник подготовивший документ.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    private IProcessIncomingMessageResult UpdateOrCreateBusinessTripOrder(INNBusinessTripOrderDocument document, ICreateBusinessTripOrder structure, DirRX.OverrideSolution.IEmployee employee,
                                                                          DirRX.OverrideSolution.IEmployee ourSignatory, DirRX.OverrideSolution.IEmployee preparedBy)
    {
      var result = ProcessIncomingMessageResult.Create();
      
      if (!Locks.TryLock(document))
      {
        var locksInfo = Locks.GetLockInfo(document);
        result.ErrorText = string.Format("ИД документа: {0}. Текст ошибки: Не удалось заблокировать документ. Заблокирован: {1}. Время установки блокировки: {2}.", document.Id, locksInfo.OwnerName, locksInfo.LockTime);
        result.Retry = true;
        Logger.ErrorFormat("UpdateOrCreateBusinessTripOrder. {0}", result.ErrorText);
        return result;
      }
      
      try
      {
        result.ErrorText = FillDocumentProperties(document, structure, employee, ourSignatory, preparedBy);
        
        if (!string.IsNullOrEmpty(result.ErrorText))
          return result;
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }
      
      return result;
    }

    /// <summary>
    /// Заполнить свойства документа.
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="structure">Структура с данными документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="ourSignatory">Подписывающий сотрудник.</param>
    /// <param name="preparedBy">Сотрудник подготовивший документ.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    private string FillDocumentProperties(INNBusinessTripOrderDocument document, ICreateBusinessTripOrder structure, DirRX.OverrideSolution.IEmployee employee,
                                          DirRX.OverrideSolution.IEmployee ourSignatory, DirRX.OverrideSolution.IEmployee preparedBy)
    {
      var isInserted = document.State.IsInserted;
      
      if (document.State.IsInserted)
      {
        var documentKind = GetDocumentKindForSignDocumentTask(structure.Kind);

        if (documentKind == null)
          return string.Format("Не удалось найти вид документа для кода вида: {0}", structure.Kind);
        
        document.DocumentKind = documentKind;
        // В HR св-во HRLifeCycleStateHRPro продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
        // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
        // Используется в HRLifeCycleStateHRProChanged официального документа.
        document.HRLifeCycleStateHRPro = NNOtherProcesses.NNBusinessTripOrderDocument.HRLifeCycleStateHRPro.Draft;
        document.BusinessUnit = employee.BusinessUnit;
        document.Department = employee.Department;
      }
      
      if (isInserted || !document.RegistrationNumber.Equals(structure.RegNumber))
        document.RegistrationNumber = structure.RegNumber;
      if (isInserted || document.RegistrationDate != structure.Date)
        document.RegistrationDate = structure.Date;
      if (isInserted || !document.SAPApplicationNumberDirRX.Equals(structure.Request))
        document.SAPApplicationNumberDirRX = structure.Request;
      if (isInserted || !Employees.Equals(document.EmployeeDirRX, employee))
        document.EmployeeDirRX = employee;
      if (isInserted || !Employees.Equals(document.PreparedBy, preparedBy))
        document.PreparedBy = preparedBy;
      if (isInserted || !document.PlaceResidence.Equals(structure.Destination))
        document.PlaceResidence = structure.Destination;
      if (isInserted || document.CalendarDays != structure.Days)
        document.CalendarDays = structure.Days;
      if (isInserted || document.StartDate != structure.DateStart)
        document.StartDate = structure.DateStart;
      if (isInserted || document.EndDate != structure.DateEnd)
        document.EndDate = structure.DateEnd;
      if (isInserted || !document.ExternalId.Equals(structure.SAPID))
        document.ExternalId = structure.SAPID;

      var cancelBusinessKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.CancelBusinessTripOrder);
      var changeBusinessKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.ChangeBusinessTripOrder);
      
      if (document.DocumentKind.Equals(changeBusinessKind) || document.DocumentKind.Equals(cancelBusinessKind))
      {
        var mainOrder = NNBusinessTripOrderDocuments.GetAll(c => c.RegistrationNumber.Equals(structure.MainOrderRegNumber) && c.RegistrationDate == structure.MainOrderDate).FirstOrDefault();

        if (mainOrder != null && (isInserted || document.FondationDocument == null || !document.FondationDocument.Equals(mainOrder)))
          document.FondationDocument = mainOrder;
      }
      
      var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.BusinessTripOrderExtension.Guid);

      if (string.IsNullOrEmpty(extension))
        return "Прикладная константа 'Интеграция. Формат файла для документа \"Приказ на командировку\", принимаемый из АСУП' не заполнена.";
      
      // Документ сохраняется во время создания новой версии.
      var error = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, structure.Body, extension);

      if (!string.IsNullOrEmpty(error))
        return error;

      // HACK Подписывающий может задаваться только после сохранения документа. Иначе на проверки права подписи падает в ошибку.
      document.OurSignatory = ourSignatory;
      document.Save();
      
      return string.Empty;
    }

    /// <summary>
    /// Отправить задачу для подписания приказа на командировку.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="author">Автор.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    /// <remarks>Создание и отправка задачи выполняется в транзакции.</remarks>
    private string SendTaskToSignBusinessTripOrder(INNBusinessTripOrderDocument document, IEmployee author)
    {
      var logPrefix = string.Format("SendTaskToSignBusinessTripOrder. Документ ИД: {0}", document.Id);
      
      var isStarted = HRPro.HRDocFlow.HRProcessTasks.GetAll(c => c.MainDoc.Id == document.Id && c.Status == Status.InProcess).Any();
      if (isStarted)
      {
        Logger.DebugFormat("{0}. Задача на подписание приказа была запущена ранее. Повторная отправка не требуется.", logPrefix);
        return string.Empty;
      }
      
      var needApprove = DirRX.NNCommons.PublicFunctions.Module.Remote.GetBoolValue(Constants.Module.ConstantsSetting.BusinessTripOrderNeedApprove.Guid);
      if (needApprove == null)
        return "Не заполнена прикладная константа 'Интеграция. Приказы на командировку. Значение свойства \"Требуется подписание\" в задаче на обработку документов, связанных с работой, при отправке документов из АСУП'.";
      
      var needAcquaintance = DirRX.NNCommons.PublicFunctions.Module.Remote.GetBoolValue(Constants.Module.ConstantsSetting.BusinessTripOrderNeedAcquaintance.Guid);
      if (needAcquaintance == null)
        return "Не заполнена прикладная константа 'Интеграция. Приказы на командировку. Значение свойства \"Требуется ознакомление сотрудников\" в задаче на обработку документов, связанных с работой, при отправке документов из АСУП'.";
      
      var executionResult = false;
      var errorMessage = string.Empty;
      executionResult = Transactions.Execute(
        () =>
        {
          try
          {
            var task = HRPro.HRDocFlow.PublicFunctions.Module.Remote.CreateNewSignDocumentTask(author, needApprove.Value, needAcquaintance.Value);
            task.DocumentGroup.OfficialDocuments.Add(document);
            
            var processKind = Sungero.Workflow.ProcessKinds.GetAllMatches(task).FirstOrDefault();
            if (processKind == null)
            {
              errorMessage = "Не удалось найти подходящий вариант процесса для отправки задачи на подписание приказа на командировку.";
              return;
            }
            task.Start();
          }
          catch (Exception e)
          {
            errorMessage = string.Format("Произошла ошибка отправки задачи на подписание приказа на командировку. Текст ошибки: {0}", e.Message);
            Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, errorMessage, e.StackTrace);
          }
        });

      if (!executionResult || !string.IsNullOrEmpty(errorMessage))
      {
        if (string.IsNullOrEmpty(errorMessage))
        {
          errorMessage = "Произошла ошибка отправки задачи на подписание приказа на командировку. Транзакция не выполнена.";
          Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        }
        return errorMessage;
      }
      else
        return string.Empty;
    }

    /// <summary>
    /// Получить вид документа по коду из АСУП.
    /// </summary>
    /// <param name="kindCode">Код вида документа из АСУП.</param>
    /// <returns>Вид документа.</returns>
    private Sungero.Docflow.IDocumentKind GetDocumentKindForSignDocumentTask(string kindCode)
    {
      var businessTripOrder = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.BusinessTripOrder.Guid);
      var changeBusinessTripOrder = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.ChangeBusinessTripOrder.Guid);
      var cancelBusinessTripOrder = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.CancelBusinessTripOrder.Guid);
      var kindGuid = Guid.Empty;

      if (businessTripOrder.Contains(kindCode))
        kindGuid = Constants.Module.DocumentKind.BusinessTripOrder;
      else if (changeBusinessTripOrder.Contains(kindCode))
        kindGuid = Constants.Module.DocumentKind.ChangeBusinessTripOrder;
      else if (cancelBusinessTripOrder.Contains(kindCode))
        kindGuid = Constants.Module.DocumentKind.CancelBusinessTripOrder;

      return Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(kindGuid);
    }

    #endregion

    #endregion

    #region Интеграционный сценарий «Регистрация Работы ВД/ПД/СР».

    #region IF-S-10413. Передача приказа на выход сотрудника в выходной и/или праздничный день из АСУП в КЭДО (АСУП -> КЭДО).

    /// <summary>
    /// Обработка создания приказа на выход сотрудника в выходной и/или праздничный день.
    /// </summary>
    /// <param name="incomingMessageId">ИД записи справочника 'Входящие информационные сообщения'.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public IProcessIncomingMessageResult CreateWeekendWorkOrderDocumentHandler(long incomingMessageId)
    {
      var logPrefix = string.Format("ProcessCreateWeekendWorkOrderDocument. Ид записи справочника 'Входящие информационные сообщения': {0}", incomingMessageId);
      var result = ProcessIncomingMessageResult.Create();
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == incomingMessageId).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.ErrorText = "Не найдена запись в справочнике 'Входящие информационные сообщения'.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      ICreateWeekendWorkOrderStructure structure = null;
      
      try
      {
        structure = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Ошибка десериализации. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, result.ErrorText,  ex.StackTrace);
        return result;
      }
      
      var ourSignatory = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Signed)).FirstOrDefault();
      var employeeWhoPrepared = DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(structure.Prepared)).FirstOrDefault();

      if (ourSignatory == null || employeeWhoPrepared == null)
      {
        if (ourSignatory == null)
          result.ErrorText += string.Format("Не найден подписывающий с табельным номером: {0}. ", structure.Signed);
        if (employeeWhoPrepared == null)
          result.ErrorText += string.Format("Не найден сотрудник подготовивший документ с табельным номером: {0}. ", structure.Prepared);

        result.ErrorText = result.ErrorText.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var cancelationTask = WeekendWorkCancelationTasks.Null;
      var canceledOrder = WeekendWorkOrderDocuments.Null;
      var task = WeekendWorkTasks.Null;
      var statement = WeekendWorkStatementDocuments.Null;
      
      // Если вид документа - отмена приказа ВД/ПД/СР.
      if (structure.Kind == NNIntegration.PublicConstants.Module.WeekendWorkOrderKindCodes.Cancelation)
      {
        cancelationTask = WeekendWorkCancelationTasks.GetAll(x => x.Id == structure.TaskID).FirstOrDefault();
        if (cancelationTask == null)
        {
          result.ErrorText = string.Format("Не найдена задача для отмены приказа ВД/ПД/СР по Ид: {0}", structure.TaskID);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          return result;
        }
        
        canceledOrder = cancelationTask.WeekendWorkOrder;
        if (canceledOrder == null)
        {
          result.ErrorText = string.Format("Отсутствует ссылка на приказ в задаче на отмену приказа ВД/ПД/СР. Ид задачи: {0}", structure.TaskID);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          return result;
        }
        
        statement = canceledOrder.WeekendWorkStatement;
      }
      else
      {
        task = WeekendWorkTasks.GetAll(x => x.Id == structure.TaskID).FirstOrDefault();
        if (task == null)
        {
          result.ErrorText = string.Format("Не найдена задача на согласование заявки ВД/ПД/СР по Ид: {0}", structure.TaskID);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          return result;
        }
        statement = WeekendWorkStatementDocuments.As(task.DocumentGroup.OfficialDocuments.FirstOrDefault());
      }
      
      if (statement == null)
      {
        result.ErrorText = string.Format("Отсутствует ссылка на заявку во вложениях задачи или в приказе ВД/ПД/СР");
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var order = WeekendWorkOrderDocuments.GetAll(x => x.ExternalId == structure.SAPID).FirstOrDefault();
      result.EntityId = order?.Id;
      
      if (order != null)
      {
        var checkResult = OverrideSolution.PublicFunctions.DocRelatedToWorkBase.CanUpdateDocFromASUP(order);
        if (!string.IsNullOrEmpty(checkResult))
        {
          result.ErrorText = string.Format("Версия документа с ID: {0} не создана. {1}.", order.Id, checkResult);
          Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
          return result;
        }
      }
      
      order = order ?? WeekendWorkOrderDocuments.Create();
      
      // Блокировка необходима для создания тела документа.
      if (!Locks.GetLockInfo(order).IsLockedByMe && !Locks.TryLock(order))
      {
        var locksInfo = Locks.GetLockInfo(order);
        result.ErrorText = string.Format("ИД документа: {0}. Текст ошибки: Не удалось заблокировать документ. Заблокирован: {1}. Время установки блокировки: {2}.", order.Id, locksInfo.OwnerName, locksInfo.LockTime);
        result.Retry = true;
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      try
      {
        var errorMessage = Functions.WeekendWorkOrderDocument.FillWeekendWorkOrderProperties(order, structure, statement, canceledOrder, ourSignatory, employeeWhoPrepared);
        if (!string.IsNullOrWhiteSpace(errorMessage))
          throw AppliedCodeException.Create(errorMessage);
        
        Logger.DebugFormat("{0} Документ с ИД: {1} был сохранен.", logPrefix, order.Id);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", order.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      finally
      {
        if (Locks.GetLockInfo(order).IsLockedByMe)
          Locks.Unlock(order);
      }
      result.EntityId = order?.Id;
      
      result.ErrorText = this.CreateAndStartNewSignDocumentSubtask(structure.TaskID, employeeWhoPrepared, order);
      if (string.IsNullOrEmpty(result.ErrorText))
        Logger.DebugFormat("{0}. Успешно обработан.", logPrefix);
      
      return result;
    }

    /// <summary>
    /// Получить вид документа по коду из АСУП.
    /// </summary>
    /// <param name="kindCode">Код вида документа из АСУП.</param>
    /// <returns>Вид документа.</returns>
    public Sungero.Docflow.IDocumentKind GetDocumentKindForWeekendWorkOrder(string kindCode)
    {
      var kindGuid = Guid.Empty;
      if (string.IsNullOrEmpty(kindCode))
      {
        Logger.ErrorFormat("GetDocumentKindForWeekendWorkOrder. Код вида документа не может быть null или пустым.");
        return Sungero.Docflow.DocumentKinds.Null;
      }
      
      if (kindCode == NNIntegration.PublicConstants.Module.WeekendWorkOrderKindCodes.Cancelation)
        kindGuid = Constants.Module.DocumentKind.WeekendWorkCancelationOrderDocument;
      else if (kindCode == NNIntegration.PublicConstants.Module.WeekendWorkOrderKindCodes.WeekendWork)
        kindGuid = Constants.Module.DocumentKind.WeekendWorkOrderDocument;
      else if (kindCode == NNIntegration.PublicConstants.Module.WeekendWorkOrderKindCodes.OvertimeWork)
        kindGuid = Constants.Module.DocumentKind.OvertimeWorkOrderDocument;
      else
        Logger.ErrorFormat("GetDocumentKindForWeekendWorkOrder. Не удалось определить вид документа ВД/ПД/СР по его коду. Код: {0}.", kindCode);

      return kindGuid == Guid.Empty ? Sungero.Docflow.DocumentKinds.Null : Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(kindGuid);
    }

    #endregion

    #region IF-S-10412. Автоматическая проверка заявления на работу в выходной и/или праздничный день (АСУП -> КЭДО).

    /// <summary>
    /// Проверить заявление ВД/ПД/СР во внешней системе (базовая перегрузка).
    /// </summary>
    /// <param name="request">Запрос.</param>
    /// <param name="statementId">ИД заявления.</param>
    /// <returns>Ответ.</returns>
    /// <remarks>Если заявление ещё не создано в RX, в параметре ИД заявление передаётся 0.</remarks>
    public IValidateWeekendWorkStatementResponse ValidateWeekendWorkStatementInExternalSystem(NNIntegration.Structures.Module.IValidateWeekendWorkStatementRequest request, long statementId)
    {
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.ValidateWeekendWorkStatement;
      var logPrefix = string.Format("ValidateWeekendWorkStatementInExternalSystem. Гуид потока: {0}. ИД заявления: {1}", flowGuid, statementId);
      Logger.DebugFormat("{0}. Старт синхронной отправки сообщения во внешнюю систему.", logPrefix);
      
      var errorMessage = string.Empty;
      var requestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(request);
      if (!string.IsNullOrEmpty(errorMessage))
        Logger.ErrorFormat("{0}. При формировании текста запроса для проверки заявления ВД/ПД/СР во внешней системе возникла ошибка: {1}.", logPrefix, errorMessage);
      
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(requestText, flowGuid, statementId);
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultSendingMessage.ErrorText);
        return null;
      }
      
      var response = NNIntegration.Structures.Module.ValidateWeekendWorkStatementResponse.Create();
      try
      {
        response = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(resultSendingMessage.AnswerText, response);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        return null;
      }
      
      Logger.DebugFormat("{0}. Конец синхронной отправки сообщения.", logPrefix);
      return response;
    }
    
    /// <summary>
    /// Проверить заявление ВД/ПД/СР во внешней системе (перегрузка для RX).
    /// </summary>
    /// <param name="statement">Заявление ВД/ПД/СР.</param>
    /// <returns>Ответ.</returns>
    public IValidateWeekendWorkStatementResponse ValidateWeekendWorkStatementInExternalSystem(IWeekendWorkStatementDocument statement)
    {
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWorkDocKind(statement);
      if (isOvertimeWork == null)
        isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWork(statement.DocumentKind);
      var kindCode = isOvertimeWork.Value ? NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.OvertimeWork :
        NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.WeekendWork;
      
      // HACK: Обход коробочного ограничения для корректного отображения времени в документах.
      // При передаче в SAP необходимо привести время к формату часового пояса инициатора на момент заполнения им карточки.
      var initiatorTimeZoneDiff = DirRX.NNOtherProcesses.PublicFunctions.WeekendWorkStatementDocument.Remote.GetInitiatorTimeZoneDiff(statement);
      
      // Информация о сотрудниках.
      var employeeInfoList = new List<NNIntegration.Structures.Module.IValidateWeekendWorkStatementEmployeeInfoRequest>();
      foreach (var employeeInfo in statement.Employees)
      {
        var employeeInfoRequest = NNIntegration.Structures.Module.ValidateWeekendWorkStatementEmployeeInfoRequest.Create();
        employeeInfoRequest.Employee = employeeInfo.Employee.PersonnelNumber;
        employeeInfoRequest.DateStart = employeeInfo.WorkDateFrom.Value.ToString("dd.MM.yyyy");
        employeeInfoRequest.TimeStart = employeeInfo.WorkDateFrom.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm");
        employeeInfoRequest.DateEnd = employeeInfo.WorkDateTo.Value.ToString("dd.MM.yyyy");
        employeeInfoRequest.TimeEnd = employeeInfo.WorkDateTo.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm");
        employeeInfoRequest.DateWeekendStart = employeeInfo.RestDateFrom.HasValue ? employeeInfo.RestDateFrom.Value.ToString("dd.MM.yyyy") : string.Empty;
        employeeInfoRequest.DateWeekendEnd = employeeInfo.RestDateTo.HasValue ? employeeInfo.RestDateTo.Value.ToString("dd.MM.yyyy") : string.Empty;
        employeeInfoRequest.TimeWeekendStart = employeeInfo.RestDateFrom.HasValue ? employeeInfo.RestDateFrom.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm") : string.Empty;
        employeeInfoRequest.TimeWeekendEnd = employeeInfo.RestDateTo.HasValue ? employeeInfo.RestDateTo.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm") : string.Empty;
        employeeInfoRequest.Order = employeeInfo.Order;
        employeeInfoRequest.SppElement = employeeInfo.SppElement;
        employeeInfoRequest.OtherRM = employeeInfo.OtherRM;
        employeeInfoList.Add(employeeInfoRequest);
      }
      
      var request = NNIntegration.Structures.Module.ValidateWeekendWorkStatementRequest.Create(
        Guid.NewGuid().ToString(),
        kindCode,
        statement.IsGroup.HasValue && statement.IsGroup.Value,
        employeeInfoList
       );
      
      var response = ValidateWeekendWorkStatementInExternalSystem(request, statement.Id);
      if (response == null || !response.Employees.Any())
        Logger.DebugFormat("ValidateWeekendWorkStatementInExternalSystem. {0} Пустой ответ или нет информации по сотрудникам: {1}.",
                           DirRX.NNOtherProcesses.Resources.ExternalSystemValidationWeekendWorkError, response == null || !response.Employees.Any());
      
      return response;
    }


    /// <summary>
    /// Проверить заявление ВД/ПД/СР во внешней системе (перегрузка для ЛК).
    /// </summary>
    /// <param name="DocKindGuid">Гуид вида документа.</param>
    /// <param name="EmployeeId">ИД сотрудника.</param>
    /// <param name="WorkDateFrom">Дата работы с.</param>
    /// <param name="WorkDateTo">Дата работы по.<</param>
    /// <param name="RestDateFrom">Дата доп. отдыха с.</param>
    /// <param name="RestDateTo">Дата доп. отдыха по.</param>
    /// <param name="Order">Заказ.<</param>
    /// <param name="SppElement">СПП-элемент.</param>
    /// <param name="OtherRM">Другое РМ.</param>
    /// <returns>Ошибка. Если ошибки нет - возвращается пустая строка.</returns>
    /// <remarks>Из ЛК заявление всегда для одного сотрудника.</remarks>
    [Public(WebApiRequestType = RequestType.Post)]
    public string ValidateWeekendWorkStatementInExternalSystem(string DocKindGuid, long EmployeeId, string WorkDateFrom, string WorkDateTo,
                                                               string RestDateFrom, string RestDateTo, string Order, string SppElement, string OtherRM)
    {
      var request = NNIntegration.Structures.Module.ValidateWeekendWorkStatementRequest.Create();
      var docKinId = NNCommons.PublicFunctions.Module.GetDocumentKindId(DocKindGuid);
      var documentKind = OverrideSolution.DocumentKinds.Get(docKinId.Value);
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWork(documentKind);
      var kindCode = isOvertimeWork ? NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.OvertimeWork :
        NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.WeekendWork;
      var employee = OverrideSolution.Employees.Get(EmployeeId);
      
      request.RequestCode = Guid.NewGuid().ToString();
      request.Kind = kindCode;
      request.IsGroup = false;
      request.Employees = new List<NNIntegration.Structures.Module.IValidateWeekendWorkStatementEmployeeInfoRequest>();
      
      var workDateFrom = NNIntegration.PublicFunctions.Module.ParseStringToDate(WorkDateFrom).Value;
      var workDateTo = NNIntegration.PublicFunctions.Module.ParseStringToDate(WorkDateTo).Value;
      
      // Информация о сотруднике.
      var employeeInfoRequest = NNIntegration.Structures.Module.ValidateWeekendWorkStatementEmployeeInfoRequest.Create();
      employeeInfoRequest.Employee = employee.PersonnelNumber;
      employeeInfoRequest.DateStart = workDateFrom.ToString("dd.MM.yyyy");
      employeeInfoRequest.TimeStart = workDateFrom.ToString("HH:mm");
      employeeInfoRequest.DateEnd = workDateTo.ToString("dd.MM.yyyy");
      employeeInfoRequest.TimeEnd = workDateTo.ToString("HH:mm");
      employeeInfoRequest.DateWeekendStart = !string.IsNullOrEmpty(RestDateFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(RestDateFrom).Value.ToString("dd.MM.yyyy") : string.Empty;
      employeeInfoRequest.DateWeekendEnd = !string.IsNullOrEmpty(RestDateTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(RestDateTo).Value.ToString("dd.MM.yyyy") : string.Empty;
      employeeInfoRequest.TimeWeekendStart = !string.IsNullOrEmpty(RestDateFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(RestDateFrom).Value.ToString("HH:mm") : string.Empty;
      employeeInfoRequest.TimeWeekendEnd = !string.IsNullOrEmpty(RestDateTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(RestDateTo).Value.ToString("HH:mm") : string.Empty;
      employeeInfoRequest.Order = Order;
      employeeInfoRequest.SppElement = SppElement;
      employeeInfoRequest.OtherRM = OtherRM;
      request.Employees.Add(employeeInfoRequest);
      
      var response = ValidateWeekendWorkStatementInExternalSystem(request, 0);
      if (response == null || !response.Employees.Any())
      {
        Logger.DebugFormat("ValidateWeekendWorkStatementInExternalSystem. {0} Пустой ответ или нет информации по сотрудникам: {1}.",
                           DirRX.NNOtherProcesses.Resources.ExternalSystemValidationWeekendWorkError, response == null || !response.Employees.Any());
        return DirRX.NNOtherProcesses.Resources.ExternalSystemValidationWeekendWorkError;
      }
      
      return response.Employees.FirstOrDefault().ErrorText;
    }

    #endregion

    #region IF-S-6363. Автоматическое создание мероприятия «Регистрация работы в выходной и/или праздничный день» из КЭДО в АСУП (КЭДО -> АСУП)

    /// <summary>
    /// Процесс создания мероприятия «Регистрация работы в выходной и/или праздничный день».
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="messageGuid">Гуид сообщения.</param>
    /// <returns>Ответ.</returns>
    [Public]
    public IResultGenerateRequestText CreateWeekendWorkEvent(long mainTaskId, string messageGuid)
    {
      var logPrefix = string.Format("CreateWeekendWorkEvent. ИД основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = WeekendWorkStatementDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      logPrefix = string.Format("{0}. ИД документа: {1}", logPrefix, doc.Id);
      
      var agreedEmployees = doc.Employees.Where(e => e.RejectedByApproval != true);
      if (!agreedEmployees.Any())
      {
        result.ErrorText = "В документе не найдено сотрудников, по которым согласован выход";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var errorMessage = string.Empty;
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWork(doc.DocumentKind);
      var kindCode = isOvertimeWork ? NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.OvertimeWork :
        NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.WeekendWork;
      var isGroup = doc.IsGroup == true;
      var reason = doc.Reason != null ? doc.Reason.Code : string.Empty;
      var employeesInfoList = new List<NNIntegration.Structures.Module.ICreateWeekendWorkEventEmployeesInfo>();
      
      // HACK: Обход коробочного ограничения для корректного отображения времени в документах.
      // При передаче в SAP необходимо привести время к формату часового пояса инициатора на момент заполнения им карточки.
      var initiatorTimeZoneDiff = DirRX.NNOtherProcesses.PublicFunctions.WeekendWorkStatementDocument.Remote.GetInitiatorTimeZoneDiff(doc);
      
      var dateFormat = "dd.MM.yyyy";
      var timeFormat = "HH:mm";
      foreach (var employee in agreedEmployees)
      {
        var employeeInfo = NNIntegration.Structures.Module.CreateWeekendWorkEventEmployeesInfo.Create();
        employeeInfo.Employee = employee.PersonnelNumber;
        employeeInfo.DateStart = employee.WorkDateFrom.HasValue? employee.WorkDateFrom.Value.ToString(dateFormat) : string.Empty;
        employeeInfo.DateEnd = employee.WorkDateTo.HasValue? employee.WorkDateTo.Value.ToString(dateFormat) : string.Empty;
        employeeInfo.TimeStart = employee.WorkDateFrom.HasValue? employee.WorkDateFrom.Value.AddHours(initiatorTimeZoneDiff).ToString(timeFormat) : string.Empty;
        employeeInfo.TimeEnd = employee.WorkDateTo.HasValue? employee.WorkDateTo.Value.AddHours(initiatorTimeZoneDiff).ToString(timeFormat) : string.Empty;
        employeeInfo.DateWeekendStart = employee.RestDateFrom.HasValue ? employee.RestDateFrom.Value.ToString(dateFormat) : string.Empty;
        employeeInfo.DateWeekendEnd = employee.RestDateTo.HasValue ? employee.RestDateTo.Value.ToString(dateFormat) : string.Empty;
        employeeInfo.TimeWeekendStart = employee.RestDateFrom.HasValue ? employee.RestDateFrom.Value.AddHours(initiatorTimeZoneDiff).ToString(timeFormat) : string.Empty;
        employeeInfo.TimeWeekendEnd = employee.RestDateTo.HasValue ? employee.RestDateTo.Value.AddHours(initiatorTimeZoneDiff).ToString(timeFormat) : string.Empty;
        employeeInfo.FirstBreakStart = employee.FirstBreakFrom.HasValue? employee.FirstBreakFrom.Value.AddHours(initiatorTimeZoneDiff).ToString(timeFormat) : string.Empty;
        employeeInfo.FirstBreakEnd = employee.FirstBreakTo.HasValue? employee.FirstBreakTo.Value.AddHours(initiatorTimeZoneDiff).ToString(timeFormat) : string.Empty;
        employeeInfo.SecondBreakStart = employee.SecondBreakFrom.HasValue? employee.SecondBreakFrom.Value.AddHours(initiatorTimeZoneDiff).ToString(timeFormat) : string.Empty;
        employeeInfo.SecondBreakEnd = employee.SecondBreakTo.HasValue? employee.SecondBreakTo.Value.AddHours(initiatorTimeZoneDiff).ToString(timeFormat) : string.Empty;
        employeeInfo.WithoutABreak = string.IsNullOrEmpty(employeeInfo.FirstBreakStart) && string.IsNullOrEmpty(employeeInfo.FirstBreakEnd) &&
          string.IsNullOrEmpty(employeeInfo.SecondBreakStart) && string.IsNullOrEmpty(employeeInfo.SecondBreakEnd);
        employeeInfo.Compensation = employee.Compensation.Code;
        employeeInfo.Order = employee.Order;
        employeeInfo.SppElement = employee.SppElement;
        employeeInfo.OtherRM = employee.OtherRM;
        employeesInfoList.Add(employeeInfo);
      }
      
      var model = NNIntegration.Structures.Module.CreateWeekendWorkEvent.Create(messageGuid,
                                                                                kindCode,
                                                                                isGroup,
                                                                                mainTaskId,
                                                                                employeesInfoList,
                                                                                reason);
      
      try
      {
        result.RequestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для проведения мероприятия \"Регистрация работы в выходной и/или праздничный день\".", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для проведения мероприятия \"Регистрация работы в выходной и/или праздничный день\": {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }

    #endregion

    #region IF-S-6362. Автоматическое внесение изменений в действующий приказ из КЭДО в АСУП (КЭДО -> АСУП)

    /// <summary>
    ///  Процесс создания мероприятия «Внесение изменений в действующий приказ ВД/ПД/СР».
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="messageGuid">Гуид сообщения.</param>
    /// <returns>Ответ.</returns>
    [Public]
    public IResultGenerateRequestText CreateWeekendWorkCancelationEvent(long mainTaskId, string messageGuid)
    {
      var logPrefix = string.Format("CreateWeekendWorkCancelationEvent. ИД основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var task = WeekendWorkCancelationTasks.GetAll(x => x.Id == mainTaskId).FirstOrDefault();
      if (task == null)
      {
        result.ErrorText = "Задача типа \"Оформление отмены работы в ВД/ПД/СР\" не найдена";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      if (task.WeekendWorkOrder == null)
      {
        result.ErrorText = "В задачу типа \"Оформление отмены работы в ВД/ПД/СР\" не вложен основной приказ.";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      if (task.WeekendWorkOrder.WeekendWorkStatement == null)
      {
        result.ErrorText = "В приказ (распоряжение) о работе в ВД/ПД/СР не вложена заявка.";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var errorMessage = string.Empty;
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWork(task.WeekendWorkOrder.WeekendWorkStatement.DocumentKind);
      var kindCode = isOvertimeWork ? NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.OvertimeWork :
        NNIntegration.PublicConstants.Module.WeekendWorkStatementKindCodes.WeekendWork;
      var employeesInfoList = new List<NNIntegration.Structures.Module.ICreateWeekendWorkCancelationEventEmployeesInfo>();
      
      var dateFormat = "dd.MM.yyyy";
      var cancelsNumber = task.WeekendWorkOrder.RegistrationNumber;
      var cancelsDate = task.WeekendWorkOrder.RegistrationDate.HasValue ?
        task.WeekendWorkOrder.RegistrationDate.Value.ToString(dateFormat) :
        string.Empty;
      
      var cancelationReason = task.CancelationReason!= null ? task.CancelationReason.Name : string.Empty;
      
      foreach (var employeeData in task.Employees)
      {
        var employeeInfo = NNIntegration.Structures.Module.CreateWeekendWorkCancelationEventEmployeesInfo.Create();
        employeeInfo.Employee = employeeData.Employee.PersonnelNumber;
        employeesInfoList.Add(employeeInfo);
      }
      
      var model = NNIntegration.Structures.Module.CreateWeekendWorkCancelationEvent.Create();
      model.RequestCode = messageGuid;
      model.Kind = kindCode;
      model.CancelsNumber = cancelsNumber;
      model.CancelsDate = cancelsDate;
      model.CancelationReason = cancelationReason;
      model.TaskID = mainTaskId;
      model.Employees = employeesInfoList;
      
      try
      {
        result.RequestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для проведения мероприятия \"Автоматическое внесение изменений в действующий приказ\".", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для проведения мероприятия \"Автоматическое внесение изменений в действующий приказ\": {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }

    #endregion
    
    #region IF-S-29002. Передача приказа на дополнительный день отдыха за РВД из АСУП в КЭДО (АСУП -> КЭДО).

    /// <summary>
    /// Обработка создания приказа на дополнительный день отдыха за РВД.
    /// </summary>
    /// <param name="incomingMessageId">ИД записи справочника 'Входящие информационные сообщения'.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public IProcessIncomingMessageResult CreateWeekendWorkRestOrderDocumentHandler(long incomingMessageId)
    {
      var logPrefix = string.Format("CreateWeekendWorkRestOrderDocument. Ид записи справочника 'Входящие информационные сообщения': {0}", incomingMessageId);
      var result = ProcessIncomingMessageResult.Create();
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == incomingMessageId).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.ErrorText = "Не найдена запись в справочнике 'Входящие информационные сообщения'.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      ICreateWeekendWorkRestOrderStructure structure = null;
      
      try
      {
        structure = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Ошибка десериализации. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, result.ErrorText,  ex.StackTrace);
        return result;
      }
      
      var ourSignatory = DirRX.OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumber(structure.Signed);
      var employeeWhoPrepared = DirRX.OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumber(structure.Prepared);
      var employee = DirRX.OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumber(structure.Employee);

      if (ourSignatory == null || employeeWhoPrepared == null || employee == null)
      {
        if (ourSignatory == null)
          result.ErrorText += string.Format("Не найден подписывающий с табельным номером: {0}. ", structure.Signed);
        if (employeeWhoPrepared == null)
          result.ErrorText += string.Format("Не найден сотрудник подготовивший документ с табельным номером: {0}. ", structure.Prepared);
        if (employee == null)
          result.ErrorText += string.Format("Не найден Cотрудник с табельным номером: {0}. ", structure.Employee);

        result.ErrorText = result.ErrorText.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var cancelationTask = WeekendWorkCancelationTasks.Null;
      var canceledOrder = WeekendWorkRestOrderDocuments.Null;
      var task = WeekendWorkTasks.Null;
      var statement = WeekendWorkStatementDocuments.Null;
      var restStatement = WeekendWorkRestStatementDocuments.Null;
      
      // Если вид документа - отмена приказа.
      if (structure.Kind == NNIntegration.PublicConstants.Module.WeekendWorkRestOrderKindCodes.CancelationRestOrder)
      {
        // Поиск задачи на отмену приказа.
        cancelationTask = WeekendWorkCancelationTasks.GetAll(x => x.Id == structure.TaskID).FirstOrDefault();
        if (cancelationTask == null)
        {
          result.ErrorText = string.Format("Не найдена задача для отмены приказа на доп. день отдыха по ИД: {0}", structure.TaskID);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          return result;
        }
        
        // Поиск основного приказа на работу ВД/ПД/СР (РВД/СУ).
        var mainOrder = cancelationTask.WeekendWorkOrder;
        if (mainOrder == null)
        {
          result.ErrorText = string.Format("Отсутствует ссылка на приказ в задаче на отмену приказа. Ид задачи: {0}", structure.TaskID);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          return result;
        }
        
        // Поиск заявки на работу ВД/ПД/СР (РВД/СУ).
        statement = WeekendWorkStatementDocuments.As(mainOrder.WeekendWorkStatement);
        if (statement != null)
        {
          // Поиск заявления на доп. день отдыха для сотрудника.
          restStatement = PublicFunctions.WeekendWorkRestStatementDocument.GetRestStatement(statement, employee);
          if (restStatement != null)
          {
            // Поиск отменяемого приказа на доп день отдыха.
            canceledOrder = PublicFunctions.WeekendWorkRestOrderDocument.GetRestOrder(restStatement);
            if (canceledOrder == null)
            {
              result.ErrorText = string.Format("Не найден отменяемый приказ по заявлению на доп. день отдыха. ИД заявления: {0}", restStatement.Id);
              Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
              return result;
            }
          }
        }
      }
      else
      {
        // Поиск задачи на оформление работы ВД/ПД/СР (РВД/СУ).
        task = WeekendWorkTasks.GetAll(x => x.Id == structure.TaskID).FirstOrDefault();
        if (task == null)
        {
          result.ErrorText = string.Format("Не найдена задача на согласование заявки ВД/ПД/СР по ИД: {0}", structure.TaskID);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
          return result;
        }
        
        // Поиск заявки на работу ВД/ПД/СР (РВД/СУ).
        statement = WeekendWorkStatementDocuments.As(task.DocumentGroup.OfficialDocuments.FirstOrDefault());
      }
      
      if (statement == null)
      {
        result.ErrorText = string.Format("Отсутствует ссылка на заявку во вложениях задачи или в приказе на доп. день отдыха.");
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      restStatement = PublicFunctions.WeekendWorkRestStatementDocument.GetRestStatement(statement, employee);
      if (restStatement == null)
      {
        result.ErrorText = string.Format("Не найдено заявление на доп. день отдыха, оформленное по заявке ИД: {0} и сотруднику ИД: {1}.", statement.Id, employee.Id);
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      // Поиск приказа на доп день отдыха.
      var order = WeekendWorkRestOrderDocuments.GetAll(x => x.ExternalId == structure.SAPID).FirstOrDefault();
      result.EntityId = order?.Id;
      
      if (order != null)
      {
        var checkResult = OverrideSolution.PublicFunctions.DocRelatedToWorkBase.CanUpdateDocFromASUP(order);
        if (!string.IsNullOrEmpty(checkResult))
        {
          result.ErrorText = string.Format("Версия документа с ID: {0} не создана. {1}.", order.Id, checkResult);
          Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
          return result;
        }
      }
      
      order = order ?? WeekendWorkRestOrderDocuments.Create();
      
      // Блокировка необходима для создания тела документа.
      if (!Locks.GetLockInfo(order).IsLockedByMe && !Locks.TryLock(order))
      {
        var locksInfo = Locks.GetLockInfo(order);
        result.ErrorText = string.Format("ИД документа: {0}. Текст ошибки: Не удалось заблокировать документ. Заблокирован: {1}. Время установки блокировки: {2}.", order.Id, locksInfo.OwnerName, locksInfo.LockTime);
        result.Retry = true;
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      try
      {
        var errorMessage = Functions.WeekendWorkRestOrderDocument.FillWeekendWorkRestOrderProperties(order, structure, statement, restStatement, canceledOrder, ourSignatory, employeeWhoPrepared, employee);
        if (!string.IsNullOrWhiteSpace(errorMessage))
          throw AppliedCodeException.Create(errorMessage);
        
        Logger.DebugFormat("{0} Документ с ИД: {1} был сохранен.", logPrefix, order.Id);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", order.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      finally
      {
        if (Locks.GetLockInfo(order).IsLockedByMe)
          Locks.Unlock(order);
      }
      result.EntityId = order?.Id;
      
      result.ErrorText = this.CreateAndStartNewSignDocumentTask(employeeWhoPrepared, order);
      if (string.IsNullOrEmpty(result.ErrorText))
        Logger.DebugFormat("{0}. Успешно обработан.", logPrefix);
      
      return result;
    }
    
    /// <summary>
    /// Получить вид документа по коду из АСУП.
    /// </summary>
    /// <param name="kindCode">Код вида документа из АСУП.</param>
    /// <returns>Вид документа.</returns>
    public Sungero.Docflow.IDocumentKind GetDocumentKindForWeekendWorkRestOrder(string kindCode)
    {
      var kindGuid = Guid.Empty;
      if (string.IsNullOrEmpty(kindCode))
      {
        Logger.ErrorFormat("GetDocumentKindForWeekendWorkRestOrder. Код вида документа не может быть null или пустым.");
        return Sungero.Docflow.DocumentKinds.Null;
      }
      
      if (kindCode == NNIntegration.PublicConstants.Module.WeekendWorkRestOrderKindCodes.CancelationRestOrder)
        kindGuid = Constants.Module.DocumentKind.CancelationRestOrderDocument;
      else if (kindCode == NNIntegration.PublicConstants.Module.WeekendWorkRestOrderKindCodes.GrantRestOrder)
        kindGuid = Constants.Module.DocumentKind.GrantRestOrderDocument;
      else
        Logger.ErrorFormat("GetDocumentKindForWeekendWorkRestOrder. Не удалось определить вид документа ВД/ПД/СР по его коду. Код: {0}.", kindCode);

      return kindGuid == Guid.Empty ? Sungero.Docflow.DocumentKinds.Null : Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(kindGuid);
    }
    
    #endregion

    #endregion

    #region Функции для работы с личным кабинетом через сервис интеграции.
    
    /// <summary>
    /// Проверяет ранее оформленные Заявления на выплату по ЛНА.
    /// </summary>
    /// <param name="inila">СНИЛС</param>
    /// <param name="paymentTypeId">Тип выплаты.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="statementId">ИД заявления.</param>
    /// <returns>Результат проверки.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public Structures.Module.IVerificationResultPayment CheckLrdPaymentInRX(string inila, long paymentTypeId, long employeeId, long? statementId)
    {
      Logger.DebugFormat("CheckLrdPaymentInRX. ИД Типа выплаты: {0}. Начало выполнения.", paymentTypeId);
      var paymentType = LrdPaymentTypes.GetAll(l => l.Id == paymentTypeId).FirstOrDefault();
      var employee = Employees.GetAll().Where(x => x.Id == employeeId).FirstOrDefault();
      var result = DirRX.NNOtherProcesses.Structures.Module.VerificationResultPayment.Create(true, string.Empty);
      
      // Считаем, что Заявление передают только в услугах на доработку.
      var statement = LrdPaymentStatementDocuments.GetAll(l => l.Id == statementId).FirstOrDefault();
      
      if (CheckRetirementPaymentType(paymentType.Id))
      {
        result = CheckRetirementPayment(employee, paymentType, statement);
        return result;
      }
      
      var hasTypeChildBirth = HasTypeChildBirth(paymentType);
      var hasTypeDisabledChild = HasTypeDisabledChild(paymentType);
      if (hasTypeChildBirth || hasTypeDisabledChild)
        result = AnyLrdPaymentStatementDocumentsForChildren(paymentType, inila, hasTypeChildBirth, hasTypeDisabledChild, statement, employee);
      
      return result;
    }
    
    /// <summary>
    /// Проверяет наличие отправленных заявлений.
    /// </summary>
    /// <param name="paymentType">Тип выплат.</param>
    /// <param name="inila">СНИЛС.</param>
    /// <param name="hasTypeChildBirth">Является типом выплат на мат. помощь на рождение ребенка.</param>
    /// <param name="hasTypeDisabledChild">Является типом выплат мат. помощь ребенку инвалиду.</param>
    /// <param name="statement">Заявление.</param>
    /// <returns>Результат поиска.</returns>
    public Structures.Module.IVerificationResultPayment AnyLrdPaymentStatementDocumentsForChildren(ILrdPaymentType paymentType,
                                                                                                   string inila,
                                                                                                   bool hasTypeChildBirth,
                                                                                                   bool hasTypeDisabledChild,
                                                                                                   ILrdPaymentStatementDocument statement,
                                                                                                   IEmployee employee)
    {
      var result = DirRX.NNOtherProcesses.Structures.Module.VerificationResultPayment.Create();
      
      if (string.IsNullOrEmpty(inila) ||
          PublicConstants.Module.EmptyInilaV1 == inila ||
          PublicConstants.Module.EmptyInilaV2 == inila)
      {
        result.VerificationResult = false;
        result.CommentText = DirRX.NNOtherProcesses.Resources.NoINILAdata;
        
        return result;
      }
      
      ILrdPaymentStatementDocument lrdPaymentStatementDocument = null;
      
      Sungero.Core.AccessRights.AllowRead(
        () =>
        {
          // Создается параметр для отключения фильтрации по ролевой модели, т.к. заявление может быть ранее создано первым родителем, а для второго оно может не попасть в выборку.
          
          if (Employees.Current != null)
            ((Sungero.Domain.Shared.IExtendedEntity)Employees.Current).Params.Add(string.Format(PublicConstants.Module.FilterLrdPaymentInRX, Employees.Current.Id), true);
          
          var lrdPaymentStatementDocuments = LrdPaymentStatementDocuments.GetAll(s => s.INILA == inila &&
                                                                                 Equals(s.PaymentType, paymentType) &&
                                                                                 !Equals(s, statement) &&
                                                                                 (s.InternalApprovalState == NNOtherProcesses.LrdPaymentStatementDocument.InternalApprovalState.Approved ||
                                                                                  s.InternalApprovalState == NNOtherProcesses.LrdPaymentStatementDocument.InternalApprovalState.OnApproval ||
                                                                                  s.InternalApprovalState == NNOtherProcesses.LrdPaymentStatementDocument.InternalApprovalState.OnRework));
          
          lrdPaymentStatementDocument = FilterByPeriodPayment(lrdPaymentStatementDocuments, paymentType, employee).FirstOrDefault();
          
          if (Employees.Current != null)
            ((Sungero.Domain.Shared.IExtendedEntity)Employees.Current).Params.Remove(string.Format(PublicConstants.Module.FilterLrdPaymentInRX, Employees.Current.Id));
        });
      
      if (lrdPaymentStatementDocument != null)
      {
        if (hasTypeChildBirth)
        {
          result.CommentText = DirRX.NNOtherProcesses.Resources.StatementSubmittedInWork;
          result.VerificationResult = false;
          
          return result;
        }
        else if (hasTypeDisabledChild)
        {
          result.CommentText = DirRX.NNOtherProcesses.Resources.StatementSubmittedCurrentPeriod;
          result.VerificationResult = true;
          
          return result;
        }
      }
      
      result.VerificationResult = true;
      result.CommentText = string.Empty;
      
      return result;
    }
    
    /// <summary>
    /// Фильтрация заявлений по периоду Настройки типа заявления.
    /// </summary>
    /// <param name="lrdPaymentStatementDocuments">Заявления.</param>
    /// <param name="lrdPaymentType">Тип выплаты.</param>
    /// <returns>Отфильтрованные заявления.</returns>
    public IQueryable<ILrdPaymentStatementDocument> FilterByPeriodPayment(IQueryable<ILrdPaymentStatementDocument> lrdPaymentStatementDocuments,
                                                                          ILrdPaymentType lrdPaymentType,
                                                                          IEmployee employee)
    {
      // Дата документа для оформляемого заявления проставляется текущая.
      var today = Calendar.Today;
      var lrdPaymentTypeSetting = LrdPaymentTypeSettings.GetAll(s => s.Status == NNOtherProcesses.LrdPaymentTypeSetting.Status.Active &&
                                                                Equals(s.PaymentType, lrdPaymentType) &&
                                                                Equals(s.MPGroup, employee.MPGroupDirRX) &&
                                                                Equals(s.BusinessUnit, employee.BusinessUnit))
        .FirstOrDefault();
      
      if (lrdPaymentTypeSetting != null)
      {
        Logger.DebugFormat("CheckLrdPaymentInRX. ИД Настройки типа выплаты: {0}", lrdPaymentTypeSetting.Id);
        
        if (lrdPaymentTypeSetting.Periodicity == NNOtherProcesses.LrdPaymentTypeSetting.Periodicity.Annually)
        {
          var startYear = Calendar.BeginningOfYear(today);
          var endYear = Calendar.EndOfYear(today);
          
          lrdPaymentStatementDocuments = lrdPaymentStatementDocuments.Where(l => l.RegistrationDate >= startYear && l.RegistrationDate <= endYear);
        }
        else if (lrdPaymentTypeSetting.Periodicity == NNOtherProcesses.LrdPaymentTypeSetting.Periodicity.Quarterly)
        {
          var beginDate = Sungero.Docflow.PublicFunctions.AccountingDocumentBase.BeginningOfQuarter(today);
          var endDate = Sungero.Docflow.PublicFunctions.AccountingDocumentBase.EndOfQuarter(today);
          
          lrdPaymentStatementDocuments = lrdPaymentStatementDocuments.Where(l => l.RegistrationDate >= beginDate && l.RegistrationDate <= endDate);
        }
        else if (lrdPaymentTypeSetting.Periodicity == NNOtherProcesses.LrdPaymentTypeSetting.Periodicity.Monthly)
        {
          var beginDate = Calendar.BeginningOfMonth(today);
          var endDate = Calendar.EndOfMonth(today);
          
          lrdPaymentStatementDocuments = lrdPaymentStatementDocuments.Where(l => l.RegistrationDate >= beginDate && l.RegistrationDate <= endDate);
        }
      }
      
      return lrdPaymentStatementDocuments;
    }
    
    /// <summary>
    /// Проверяет, явялется ли типом выплат "Материальная помощь на рождение ребенка, а также усыновление детей-сирот и детей, оставшихся без попечения родителей".
    /// </summary>
    /// <param name="lrdPaymentType">Тип выплат.</param>
    /// <returns>true - относится к типу выплат мат. помощи ребенку, иначе false.</returns>
    public bool HasTypeChildBirth(ILrdPaymentType lrdPaymentType)
    {
      var childBirthId = DirRX.NNCommons.PublicFunctions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.ChildBirthPaymentTypeId.Guid);
      
      if (childBirthId == null)
        Logger.DebugFormat("CheckLrdPaymentInRX. Прикладная константа '{1}' не заполнена.", Constants.Module.ConstantsSetting.ChildBirthPaymentTypeId.Guid);
      
      var childBirthIdPaymentType = LrdPaymentTypes.GetAll(l => l.Id == childBirthId).FirstOrDefault();
      
      return Equals(childBirthIdPaymentType, lrdPaymentType);
    }
    
    /// <summary>
    /// Проверяет, явялется ли типом выплат "Материальная помощь работнику, имеющему ребенка-инвалида в возрасте до 18 лет".
    /// </summary>
    /// <param name="lrdPaymentType">Тип выплат.</param>
    /// <returns>true - относится к типу выплат мат. помощи ребенку, иначе false.</returns>
    public bool HasTypeDisabledChild(ILrdPaymentType lrdPaymentType)
    {
      var disabledChildId = DirRX.NNCommons.PublicFunctions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.DisabledChildPaymentTypeId.Guid);
      
      if (disabledChildId == null)
        Logger.DebugFormat("CheckLrdPaymentInRX. Прикладная константа '{1}' не заполнена.", Constants.Module.ConstantsSetting.DisabledChildPaymentTypeId.Guid);
      
      var disabledChildPaymentType = LrdPaymentTypes.GetAll(l => l.Id == disabledChildId).FirstOrDefault();
      
      return Equals(disabledChildPaymentType, lrdPaymentType);
    }

    /// <summary>
    /// Возвращает значение из прикладной константы "Максимальное количество доп. согласующих".
    /// </summary>
    /// <returns>Кол-во согласующих.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public int? GetMaxAdditionalApprovers()
    {
      var maxValue = DirRX.NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.MaxNumberAdditionalMatchingElement.Guid);
      
      if (maxValue == null)
      {
        var defaultValue = Constants.Module.ConstantsDefaultValues.MaxNumberAdditionalMatchingElement;
        Logger.DebugFormat("GetMaxAdditionalApprovers. Константа не найдена, будет использоваться значение по умолчанию: {0}.", defaultValue);
        
        return defaultValue;
      }
      
      return maxValue;
    }
    
    #region Регистрация работы в выходной и/или праздничный день

    /// <summary>
    /// Проверить наличие превышения количества рабочих часов.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="workFrom">Дата работы с.</param>
    /// <param name="workTo">Дата работы по.</param>
    /// <param name="firstBreakFrom">Первый перерыв с.</param>
    /// <param name="firstBreakTo">Первый перерыв по.</param>
    /// <param name="secondBreakFrom">Второй перерыв с.</param>
    /// <param name="secondBreakTo">Второй перерыв по.</param>
    /// <returns>Текст ошибки валидации.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string ValidateWeekendWorkStatement(long employeeId,
                                               string workFrom, string workTo,
                                               string firstBreakFrom, string firstBreakTo,
                                               string secondBreakFrom, string secondBreakTo)
    {
      Logger.DebugFormat("ValidateWeekendWorkStatement. ИД сотрудника: {0}. Начало выполнения.", employeeId);
      var employee = Employees.GetAll(emp => emp.Id == employeeId).FirstOrDefault();
      
      var workDateFrom = NNIntegration.PublicFunctions.Module.ParseStringToDate(workFrom).Value;
      var workDateTo = NNIntegration.PublicFunctions.Module.ParseStringToDate(workTo).Value;
      var firstBreakDateFrom = !string.IsNullOrEmpty(firstBreakFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(firstBreakFrom) : null;
      var firstBreakDateTo = !string.IsNullOrEmpty(firstBreakTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(firstBreakTo) : null;
      var secondBreakDateFrom = !string.IsNullOrEmpty(secondBreakFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(secondBreakFrom) : null;
      var secondBreakDateTo = !string.IsNullOrEmpty(secondBreakTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(secondBreakTo) : null;
      
      // Проверка перерывов.
      if ((firstBreakDateFrom.HasValue && !firstBreakDateTo.HasValue) ||
          (firstBreakDateTo.HasValue && !firstBreakDateFrom.HasValue) ||
          (secondBreakDateFrom.HasValue && !secondBreakDateTo.HasValue) ||
          (secondBreakDateTo.HasValue && !secondBreakDateFrom.HasValue))
        return DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.BreakIncompleteValidationError;
      
      // Проверка наличия превышения количества рабочих часов.
      var businessUnit = BusinessUnits.As(employee.BusinessUnit);
      var isWorkingHoursExceeded = Functions.WeekendWorkStatementDocument.IsWorkingHoursExceeded(businessUnit, workDateFrom, workDateTo, firstBreakDateFrom, secondBreakDateFrom, false);
      Logger.DebugFormat("ValidateWeekendWorkStatement. ИД сотрудника: {0}. Результат проверки: {1}. Конец выполнения.", employeeId, isWorkingHoursExceeded);
      if (isWorkingHoursExceeded)
      {
        var workingHoursWithoutBreak = businessUnit.WorkingHoursWithoutBreakDirRX.Value;
        return Resources.ErrorCheckWorkingHoursExceededFormat(workingHoursWithoutBreak, NNCommons.PublicFunctions.Module.GetHoursDeclension(workingHoursWithoutBreak));
      }
      
      return string.Empty;
    }

    /// <summary>
    /// Заполнить данные в заявлении на ВД/ПД/СР.
    /// </summary>
    /// <param name="docId">ИД документа.</param>
    /// <param name="docKindGuid">Гуид вида документа.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="reasonId">ИД причины для работы в ВД/ПД/СР</param>
    /// <param name="compensation">Компенсация.</param>
    /// <param name="description">Основание.</param>
    /// <param name="workFrom">Дата работы с.</param>
    /// <param name="workTo">Дата работы по.</param>
    /// <param name="firstBreakFrom">Первый перерыв с.</param>
    /// <param name="firstBreakTo">Первый перерыв по.</param>
    /// <param name="secondBreakFrom">Второй перерыв с.</param>
    /// <param name="secondBreakTo">Второй перерыв по.</param>
    /// <param name="restFrom">Дата доп. отдыха с.</param>
    /// <param name="restTo">Дата доп. отдыха по.</param>
    /// <param name="additionalApproverIds">ИД доп. согласующих.</param>
    /// <param name="dueTo">В связи с.</param>
    /// <param name="executionControlId">ИД контроля исполнения.</param>
    /// <param name="order">Заказ.</param>
    /// <param name="sppElement">СПП-элемент.</param>
    /// <param name="otherRM">Другое РМ.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public string FillStatementDataForWeekendWork(long? docId, string docKindGuid, long employeeId,
                                                  long reasonId, long compensation, string description,
                                                  string workFrom, string workTo,
                                                  string firstBreakFrom, string firstBreakTo,
                                                  string secondBreakFrom, string secondBreakTo,
                                                  string restFrom, string restTo, string additionalApproverIds,
                                                  string dueTo, long? executionControlId,
                                                  string order, string sppElement, string otherRM)

    {
      Logger.DebugFormat("FillStatementDataForWeekendWork. ИД сотрудника: {0}. ИД документа: {1}. Гуид вида документа: {2}. ИД причины для работы в ВД/ПД/СР: {3}. Компенсация: {4}. Основание: {5}. " +
                         "Дата работы с: {6}. Дата работы по: {7}. Первый перерыв с: {8}. Первый перерыв по: {9}. Второй перерыв с: {10}. Второй перерыв по: {11}. " +
                         "Дата доп. отдыха с: {12}. Дата доп. отдыха по: {13}. ИД доп. согласующих: {14}. Начало выполнения.",
                         employeeId, docId, docKindGuid, reasonId, compensation, description,
                         workFrom, workTo, firstBreakFrom, firstBreakTo, secondBreakFrom, secondBreakTo, restFrom, restTo, additionalApproverIds,
                         dueTo, executionControlId, order, sppElement, otherRM);
      
      var statement = WeekendWorkStatementDocuments.GetAll(d => d.Id == docId).FirstOrDefault();
      if (statement == null)
      {
        var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Guid.Parse(docKindGuid));
        statement = WeekendWorkStatementDocuments.Create();
        statement.DocumentKind = docKind;
        statement.RegistrationDate = Calendar.Today;
      }
      statement.ReasonDescription = description;
      statement.Reason = WeekendWorkReasons.GetAll(r => r.Id == reasonId).FirstOrDefault();
      
      var ids = NNCommons.PublicFunctions.Module.ParseStringToListLong(additionalApproverIds);
      var additionalApprovers = Employees.GetAll(emp => ids.Contains(emp.Id));
      
      if (additionalApprovers.Any())
      {
        statement.AdditionalApprovers.Clear();
        
        foreach (var additionalApprover in additionalApprovers)
          statement.AdditionalApprovers.AddNew().AdditionalApprover = additionalApprover;
      }
      
      var workDateFrom = NNIntegration.PublicFunctions.Module.ParseStringToDate(workFrom);
      var workDateTo = NNIntegration.PublicFunctions.Module.ParseStringToDate(workTo);
      statement.WorkDateFrom = workDateFrom;
      statement.WorkDateTo = workDateTo;
      statement.Employees.Clear();
      var employee = Employees.GetAll(emp => emp.Id == employeeId).FirstOrDefault();
      statement.PersonnelSectionDirRX = employee.PersonnelSectionDirRX;
      
      var line = statement.Employees.AddNew();
      line.Employee = employee;
      line.WorkDateFrom = workDateFrom;
      line.WorkDateTo = workDateTo;
      line.FirstBreakFrom = !string.IsNullOrEmpty(firstBreakFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(firstBreakFrom) : null;
      line.FirstBreakTo = !string.IsNullOrEmpty(firstBreakTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(firstBreakTo) : null;
      line.SecondBreakFrom = !string.IsNullOrEmpty(secondBreakFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(secondBreakFrom) : null;
      line.SecondBreakTo = !string.IsNullOrEmpty(secondBreakTo) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(secondBreakTo) : null;
      line.RestDateFrom = !string.IsNullOrEmpty(restFrom) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(restFrom) : null;
      
      // HACK: Используется только дата доп. отдыха без перерыва. RestDateFrom.
      line.RestDateTo = line.RestDateFrom;
      line.Compensation = WeekendWorkCompensationKinds.GetAll(k => k.Id == compensation).FirstOrDefault();
      line.Order = order;
      line.SppElement = sppElement;
      line.OtherRM = otherRM;
      statement.DueTo = dueTo;
      statement.ExecutionControl = Employees.GetAll(emp => emp.Id == executionControlId).FirstOrDefault();
      
      try
      {
        statement.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("FillStatementDataForWeekendWork. Ошибка: {0}. StackTrace: {1}", ex.Message, ex.StackTrace);
        throw ex;
      }
      
      Logger.DebugFormat("FillStatementDataForWeekendWork. ИД сотрудника: {0}. ИД документа: {1}. Документ успешно сохранен.", employeeId, statement.Id);

      var importedDocuments = new List<NNCommons.Structures.Module.IDocumentInfoFromLK>();
      var importedDocument = NNCommons.Structures.Module.DocumentInfoFromLK.Create();
      importedDocument.Id = statement.Id.ToString();
      importedDocument.Type = string.Format("I{0}s", statement.GetEntityMetadata().IntegrationServiceName);
      importedDocument.Title = statement.Name;
      importedDocument.System = HRPro.HRProSolution.PublicConstants.Module.TargetNameSpace;
      importedDocuments.Add(importedDocument);
      return NNCommons.IsolatedFunctions.Json.SerializeInJson(importedDocuments);
    }

    /// <summary>
    /// Получить данные для доработки заявления на ВД/ПД/СР.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Строка с информацией.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetStatementDataForReworkWeekendWork(long assignmentId)
    {
      Logger.DebugFormat("GetStatementDataForReworkWeekendWork. ИД задания: {0}. Начало выполнения.", assignmentId);
      var assignment = NNCommons.StatementReworkAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      var docRealated = assignment.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var document = WeekendWorkStatementDocuments.As(docRealated);
      if (document == null)
      {
        Logger.ErrorFormat("GetStatementDataForReworkWeekendWork. ИД задания: {0}. Документ не найден.", assignmentId);
        return string.Empty;
      }
      var line = document.Employees.FirstOrDefault();
      var employee = line.Employee;
      var result = Structures.Module.StatementDataForReworkWeekendWork.Create();
      result.InitiatorId = employee.Id;
      result.StatementId = document.Id;
      result.Reason = document.Reason?.Id ?? 0;
      result.ReasonDescription = document.ReasonDescription;
      result.PerformerInfo = string.Join(Environment.NewLine, employee.JobTitle, employee.Department, employee.BusinessUnit);
      result.AdditionalApproverIds = string.Join(",", document.AdditionalApprovers
                                                 .Where(a => a.AdditionalApprover != null)
                                                 .Select(a => a.AdditionalApprover.Id));
      result.WorkDateFrom = line.WorkDateFrom.Value;
      result.WorkDateTo = line.WorkDateTo.Value;
      result.FirstBreakFrom = line.FirstBreakFrom;
      result.FirstBreakTo = line.FirstBreakTo;
      result.SecondBreakFrom = line.SecondBreakFrom;
      result.SecondBreakTo = line.SecondBreakTo;
      result.RestDateFrom = line.RestDateFrom;
      result.RestDateTo = line.RestDateTo;
      result.Compensation = line.Compensation.Id;
      
      Logger.DebugFormat("GetStatementDataForReworkWeekendWork. ИД задания: {0}. Конец выполнения.", assignmentId);
      return IsolatedFunctions.Json.SerializeInJson(result);
    }

    /// <summary>
    /// Получить список видов компенсаций.
    /// </summary>
    /// <param name="docKindGuid">Гуид вида документа.</param>
    /// <returns>Json, содержащий список выплат.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetCompensationKindsByDocKind(string docKindGuid)
    {
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Guid.Parse(docKindGuid));
      if (docKind == null)
      {
        Logger.DebugFormat("GetCompensationKindsByDocKind. Гуид вида документа: {0}. Не найден вид документа.", docKindGuid);
        return "[]";
      }
      var compensationKinds = WeekendWorkCompensationKinds.GetAll(w => w.Status == NNOtherProcesses.WeekendWorkCompensationKind.Status.Active && Equals(w.DocumentKind, docKind));
      var structureList = new List<Structures.Module.ICompensationKindStructure>();
      foreach (var compensationKind in compensationKinds)
      {
        var structure = Structures.Module.CompensationKindStructure.Create(compensationKind.Id, compensationKind.Name,
                                                                           compensationKind.NeedRestDate.GetValueOrDefault(),
                                                                           compensationKind.IsRestDateRequired.GetValueOrDefault());
        structureList.Add(structure);
      }
      
      var result = string.Empty;
      try
      {
        result = IsolatedFunctions.Json.SerializeInJson(structureList);
      }
      catch (Exception ex)
      {
        var errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("GetCompensationKindsByDocKind. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
      }
      
      return result;
    }

    #endregion

    #region Регистрация выплат по ЛНА.

    /// <summary>
    /// Получить список типов выплат доступных сотруднику.
    /// </summary>
    /// <param name="employeeId">ИД сотруднкиа.</param>
    /// <returns>Json, содержащий список выплат.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetPaymentTypesByEmployeId(long employeeId)
    {
      Logger.DebugFormat("GetPaymentTypesByEmployeId. EmployeeID: {0}. Начало работы функции. ", employeeId);
      var employee = Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      if (employee == null)
      {
        Logger.DebugFormat("GetPaymentTypesByEmployeId. EmployeeID: {0}. Не найден сотрудник по ИД.", employeeId);
        return "[]";
      }
      
      var paymentTypeSettings = LrdPaymentTypeSettings.GetAll(x => x.PaymentType != null).Where(x => BusinessUnits.Equals(x.BusinessUnit, employee.BusinessUnit) &&
                                                                                                x.Status == NNOtherProcesses.LrdPaymentTypeSetting.Status.Active &&
                                                                                                NNOtherProcesses.LrdPaymentMPGroups.Equals(x.MPGroup, employee.MPGroupDirRX));
      
      var paymentTypes = LrdPaymentTypes.GetAll(x => x.Status == NNOtherProcesses.LrdPaymentType.Status.Active &&
                                                paymentTypeSettings.Any(s => LrdPaymentTypes.Equals(x, s.PaymentType))).ToList();
      
      return GetJsonPaymentTypes(paymentTypes);
    }

    /// <summary>
    /// Получить Json, содержащий список типов выплат ЛНА для передачи в ЛК.
    /// </summary>
    /// <param name="paymentTypes">Список типов выплат.</param>
    /// <returns>Строка с Json.</returns>
    [Public]
    public string GetJsonPaymentTypes(List<NNOtherProcesses.ILrdPaymentType> paymentTypes)
    {
      var structureList = new List<NNIntegration.Structures.Module.IPaymentTypeStructure>();
      foreach (var paymentType in paymentTypes)
      {
        var structure = NNIntegration.Structures.Module.PaymentTypeStructure.Create(paymentType.Id, paymentType.Name);
        structureList.Add(structure);
      }
      
      var result = string.Empty;
      try
      {
        result = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(structureList);
      }
      catch (Exception ex)
      {
        var errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("GetJsonPaymentTypes. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
      }
      
      return result;
    }

    /// <summary>
    /// Получить настройку типа выплаты для ЛК.
    /// </summary>
    /// <param name="paymentTypeId">ИД типа выплаты.</param>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Json, содержащий настройку типа выплаты.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetPaymentTypeSetting(long paymentTypeId, long employeeId)
    {
      var prefix = string.Format("GetPaymentTypeSetting. PaymentTypeId: {0}. EmployeeId: {1}", paymentTypeId, employeeId);
      Logger.DebugFormat("{0}. Старт функции для поиска настройки типа выплаты.", prefix);
      var employee = Employees.GetAll(x => employeeId.Equals(x.Id)).FirstOrDefault();
      if (employee == null)
      {
        Logger.DebugFormat("{0}. Не найден сотрудник по ИД.", prefix);
        return string.Empty;
      }
      
      var paymentTypeExist = LrdPaymentTypes.GetAll(x => paymentTypeId.Equals(x.Id)).Any();
      if (!paymentTypeExist)
      {
        Logger.DebugFormat("{0}. Не найден тип выплаты по ИД.", prefix);
        return string.Empty;
      }
      
      var paymentTypeSetting = LrdPaymentTypeSettings.GetAll(x =>
                                                             BusinessUnits.Equals(x.BusinessUnit, employee.BusinessUnit) &&
                                                             x.PaymentType != null && x.PaymentType.Id == paymentTypeId &&
                                                             NNOtherProcesses.LrdPaymentMPGroups.Equals(x.MPGroup, employee.MPGroupDirRX)).FirstOrDefault();
      if (paymentTypeSetting == null)
      {
        Logger.DebugFormat("{0}. Не найдена настройка типа выплаты по ИД типа выплаты: {1}, ИД нашей организации сотрудника: {2}.", prefix, paymentTypeId, employee.BusinessUnit.Id);
        return string.Empty;
      }
      
      var constant = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.ChildSupportPaymentTypesIds.Guid);
      if (string.IsNullOrEmpty(constant))
      {
        Logger.DebugFormat("{0}. Прикладная константа '{1}' не заполнена.", prefix, Constants.Module.ConstantsSetting.ChildSupportPaymentTypesIds.Guid);
        return string.Empty;
      }
      
      var childSupportTypesIds = new List<long>();
      try
      {
        childSupportTypesIds = constant.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => long.Parse(id.Trim())).ToList();
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("{0}. Не удалось извлеч список ИД из прикладной константы: {1}. Текст ошибки: {2}. StackTrace: {3}.",
                           prefix, Constants.Module.ConstantsSetting.ChildSupportPaymentTypesIds.Guid,
                           ex.Message, ex.StackTrace);
        return string.Empty;
      }
      
      var isChildSupport = childSupportTypesIds.Contains(paymentTypeSetting.PaymentType.Id);
      
      var structure = NNIntegration.Structures.Module.PaymentTypeSettingStructure.Create(
        paymentTypeSetting.Id,
        paymentTypeSetting.FixedPayment == true,
        paymentTypeSetting.PaymentAmount.ToString(),
        paymentTypeSetting.Instruction,
        isChildSupport,
        employee.Person.Sex == OverrideSolution.Person.Sex.Male);
      
      return NNIntegration.IsolatedFunctions.Convert.SerializeInJson(structure);
    }

    /// <summary>
    /// Получить json строку из ЛК для подстановки в источник данных параметра в ЛК.
    /// </summary>
    /// <param name="json">Json с данными.</param>
    /// <returns>Json с данными.</returns>
    /// <remarks>Функция сделана POST из-за ограничения ЛК на длину параметров в GET-запросах.</remarks>
    [Public(WebApiRequestType = RequestType.Post)]
    public string GetJsonForDataSource(string json)
    {
      return json;
    }

    /// <summary>
    /// Получить информацию о предыдущем заявлении для выплаты ЛНА по заданию на доработку.
    /// </summary>
    /// <param name="assignmentId">ИД задания.</param>
    /// <returns>Json, содержащий настройку типа выплаты.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetLrdPaymentDataForRework(long assignmentId)
    {
      Logger.DebugFormat("GetLnaPaymentDataForRework. ИД задания: {0}. Начало выполнения.", assignmentId);
      var assignment = NNCommons.StatementReworkAssignments.GetAll(x => x.Id == assignmentId).FirstOrDefault();
      var docRelated = assignment.DocumentGroup.OfficialDocuments.FirstOrDefault();
      var statement = LrdPaymentStatementDocuments.As(docRelated);
      if (statement == null)
      {
        Logger.ErrorFormat("GetLnaPaymentDataForRework. ИД задания: {0}. Документ не найден.", assignmentId);
        return string.Empty;
      }
      var employee = statement.EmployeeDirRX;
      var performerInfo = string.Join(Environment.NewLine, employee.BusinessUnit, employee.Department, employee.JobTitle);
      
      var assignmentInfo = NNIntegration.Structures.Module.LrdPaymentAssignmentInfo.Create(
        assignment.Performer.Id,
        statement.Id,
        statement.PaymentType.Id,
        statement.PaymentType.Name,
        statement.ChildName,
        statement.ChildBirthDate,
        statement.RequestedAmount.ToString(),
        statement.Reason,
        performerInfo,
        statement.FireDate);
      
      return NNIntegration.IsolatedFunctions.Convert.SerializeInJson(assignmentInfo);
    }

    /// <summary>
    /// Заполнить данные в заявлении на выплату по ЛНА.
    /// </summary>
    /// <param name="documentId">ИД документа.</param>
    /// <param name="paymentTypeId">ИД типа выплаты.</param>
    /// <param name="childName">ФИО ребенка.</param>
    /// <param name="childBirthDate">Дата рождения.</param>
    /// <param name="amount">Сумма.</param>
    /// <param name="reason">Причина.</param>
    /// <param name="fireDate">Дата увольнения.</param>
    /// <param name="experience">Стаж.</param>
    /// <param name="reason">СНИЛС.</param>
    /// <param name="reason">Текст замечания.</param>
    /// <param name="retiree">Пенсионер.</param>
    /// <param name="disable">Инвалид.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public void FillLrdPaymentData(long documentId, long paymentTypeId, string childName, string childBirthDate, string amount, string reason,
                                   string inila, string commentText, DateTime? fireDate, string experience, bool retiree, bool disable)
    {
      var logPrefix = string.Format("FillLrdPaymentData. ИД документа: {0}", documentId);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      var statement = LrdPaymentStatementDocuments.GetAll(d => d.Id == documentId).FirstOrDefault();
      statement.PaymentType = LrdPaymentTypes.GetAll(p => p.Id == paymentTypeId).FirstOrDefault();
      statement.ChildName = childName;
      statement.RequestedAmount = Double.Parse(amount);
      statement.ApprovedAmount = statement.RequestedAmount;
      statement.Reason = reason;
      statement.INILA = inila;
      statement.SAPCommentTextDirRX = commentText;
      statement.ChildBirthDate = !string.IsNullOrEmpty(childBirthDate) ? NNIntegration.PublicFunctions.Module.ParseStringToDate(childBirthDate) : null;
      statement.FireDate = fireDate;
      statement.Retiree = retiree;
      statement.Disable = disable;
      int experienceInt;
      if (int.TryParse(experience, out experienceInt))
        statement.Experience = experienceInt;
      
      try
      {
        statement.Save();
        Logger.DebugFormat("{0}. Документ успешно сохранен.", logPrefix);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Не удалось сохранить документ. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
    }

    /// <summary>
    /// Получить ИД шаблона заявление на выплату по ЛНА.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>JSON строку с ИД шаблона и ИД вида документа.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public virtual string GetLrdPaymentStatementTemplateId(long employeeId)
    {
      var logPrefix = string.Format("GetLrdPaymentStatementTemplateId. ИД сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      var documentKindGuid = Constants.Module.DocumentKind.LrdPaymentStatementDocument;
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(documentKindGuid);
      var employee = Employees.GetAll(e => e.Id == employeeId).FirstOrDefault();
      var businessUnit = employee.BusinessUnit;
      var settingTemplates = HRPro.HRProSolution.DocumentKinds.As(docKind).TemplatesLKToBusinessUnitHRPro;
      var template = settingTemplates.FirstOrDefault(t => Equals(t.BusinessUnit, businessUnit))?.TemplateLK;
      if (template == null)
        template = settingTemplates.FirstOrDefault(t => t.BusinessUnit == null)?.TemplateLK;
      
      var result = NNCommons.Structures.Module.StatementData.Create();
      if (template != null)
      {
        result.TemplateId = template.Id;
        result.DocumentKindId = docKind.Id;
        result.Error = string.Empty;
      }
      else
      {
        result.TemplateId = 0;
        result.DocumentKindId = 0;
        result.Error = DirRX.NNOtherProcesses.Resources.LrdPaymentTemplateNotFoundErrorFormat(docKind.Name);
      }
      Logger.DebugFormat("{0}. Конец выполнения.", logPrefix);
      return NNCommons.IsolatedFunctions.Json.SerializeInJson(result);
    }

    /// <summary>
    /// Проверить, что тип выплаты - "Материальная помощь работнику, имеющему ребенка-инвалида в возрасте до 18 лет".
    /// </summary>
    /// <param name="paymentTypeId">ИД типа выплаты.</param>
    /// <returns>True, если тип выплаты - "Материальная помощь работнику, имеющему ребенка-инвалида в возрасте до 18 лет".</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public bool CheckForDisabledChildPaymentType(long? paymentTypeId)
    {
      var disabledChildPaymentTypeId = NNCommons.PublicFunctions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.DisabledChildPaymentTypeId.Guid);
      return paymentTypeId == disabledChildPaymentTypeId;
    }

    [Public(WebApiRequestType = RequestType.Get)]
    public string GetPaymentTypeNameById(long? paymentTypeId)
    {
      var paymentType = DirRX.NNOtherProcesses.LrdPaymentTypes.GetAll(x => x.Id == paymentTypeId).FirstOrDefault();
      return paymentType?.Name ?? "";
    }
    
    /// <summary>
    /// Проверить, что тип выплаты - "Единовременная материальная помощь по уходу на пенсию".
    /// </summary>
    /// <param name="paymentTypeId">ИД типа выплаты.</param>
    /// <returns>True, если тип выплаты - "Единовременная материальная помощь по уходу на пенсию".</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public bool CheckRetirementPaymentType(long? paymentTypeId)
    {
      if (paymentTypeId == null)
        return false;
      
      var retirementPaymentTypeIds = NNCommons.PublicFunctions.Module.GetIdsListValue(Constants.Module.ConstantsSetting.RetirementPaymentTypeIds.Guid,
                                                                                      "CheckRetirementPaymentType");
      
      return retirementPaymentTypeIds.Contains(paymentTypeId.Value);
    }

    #endregion

    #endregion

    #region Интеграционный сценарий «Регистрация выплат по ЛНА».

    #region IF-S-6366. Автоматическое создание мероприятия «Регистрация выплат ЛНА» из КЭДО в АСУП (КЭДО → АСУП)

    /// <summary>
    /// Процесс создания мероприятия «Регистрация выплат ЛНА».
    /// </summary>
    /// <param name="mainTaskId">Ид основной задачи.</param>
    /// <param name="messageGuid">Гуид сообщения.</param>
    /// <returns>Ответ.</returns>
    [Public]
    public IResultGenerateRequestText CreateLrdPaymentEvent(long mainTaskId, string messageGuid)
    {
      var logPrefix = string.Format("CreateLrdPaymentEvent. ИД основной задачи: {0}", mainTaskId);
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      
      var doc = LrdPaymentStatementDocuments.As(NNIntegration.PublicFunctions.Module.GetDocumentForSendingToExternalSystem(mainTaskId));
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      logPrefix = string.Format("{0}. ИД документа: {1}", logPrefix, doc.Id);
      
      if (doc.ApprovedAmount == null)
      {
        result.ErrorText = string.Format("Не заполнено поле {0}.", doc.Info.Properties.ApprovedAmount.LocalizedName);
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      var errorMessage = string.Empty;
      var payDate = doc.PayDate.HasValue ? doc.PayDate.Value.ToString("dd.MM.yyyy") : string.Empty;
      var childBirthDate = doc.ChildBirthDate.HasValue ? doc.ChildBirthDate.Value.ToString("dd.MM.yyyy") : string.Empty;
      var purposeCode = doc.PaymentType.PaymentPurpose == null ? string.Empty : doc.PaymentType.PaymentPurpose.Code;
      var model = NNIntegration.Structures.Module.CreateLrdPaymentEvent.Create(messageGuid,
                                                                               doc.EmployeeDirRX.PersonnelNumber,
                                                                               doc.PaymentType.EventReason.Code,
                                                                               doc.PaymentType.EventReasonClarification.Code,
                                                                               purposeCode,
                                                                               payDate,
                                                                               doc.ApprovedAmount.Value,
                                                                               doc.ChildName,
                                                                               childBirthDate,
                                                                               mainTaskId);
      
      try
      {
        result.RequestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(model);
        Logger.DebugFormat("{0}. Завершено создание структуры для проведения мероприятия \"Регистрация выплат по ЛНА\".", logPrefix);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("{0}. При создании структуры для проведения мероприятия \"Регистрация выплат по ЛНА\": {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      return result;
    }

    #endregion

    #region IF-S-10400. Проверка данных по заявлению на выплату ЛНА (АСУП → КЭДО)

    /// <summary>
    /// Проверить заявление на выплату ЛНА во внешней системе (базовая перегрузка).
    /// </summary>
    /// <param name="request">Запрос.</param>
    /// <param name="statementId">ИД заявления.</param>
    /// <returns>Ответ.</returns>
    /// <remarks>Если заявление ещё не создано в RX, в параметре ИД заявление передаётся 0.</remarks>
    public ILrdPaymentCheckResponse CheckLrdPaymentInExternalSystem(NNIntegration.Structures.Module.ILrdPaymentCheckRequest request, long statementId)
    {
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.LrdPaymentCheck;
      var logPrefix = string.Format("CheckLrdPaymentInExternalSystem. GUID потока: {0}. ID заявления: {1}", flowGuid, statementId);
      Logger.DebugFormat("{0}. Старт синхронной отправки сообщения во внешнюю систему.", logPrefix);
      
      var requestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(request);
      
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(requestText, flowGuid, statementId);
      
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0}. Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultSendingMessage.ErrorText);
        return null;
      }
      
      var response = NNIntegration.Structures.Module.LrdPaymentCheckResponse.Create();
      try
      {
        response = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(resultSendingMessage.AnswerText, response);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        return null;
      }
      
      Logger.DebugFormat("{0}. Конец синхронной отправки сообщения.", logPrefix);
      return response;
    }

    /// <summary>
    /// Проверить заявление на выплату ЛНА во внешней системе (перегрузка для ЛК).
    /// </summary>
    /// <param name="EmployeeId">Таб. номер сотрудника.</param>
    /// <param name="PaymentTypeId">ИД типа выплаты.</param>
    /// <param name="Inila">СНИЛС.</param>
    /// <param name="ChildName">ФИО ребенка.</param>
    /// <param name="BirthDate">Дата рождения.</param>
    /// <param name="FireDate">Дата увольнения.</param>
    /// <returns>Результат проверки.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public NNIntegration.Structures.Module.ILrdPaymentCheckResponse CheckLrdPaymentInExternalSystem(long EmployeeId, long PaymentTypeId, string Inila, string ChildName, string BirthDate, DateTime? FireDate)
    {
      Logger.DebugFormat("CheckLrdPaymentInExternalSystem. EmployeeId: {0}. PaymentTypeId: {1}. Cтарт проверки заявления на выплату ЛНА во внешней системе.",
                         EmployeeId, PaymentTypeId);
      
      var response = NNIntegration.Structures.Module.LrdPaymentCheckResponse.Create();
      var employee = Employees.GetAll().Where(x => x.Id == EmployeeId).FirstOrDefault();
      if (employee == null)
      {
        response.Result = false;
        response.RemarkText = DirRX.NNOtherProcesses.Resources.ExternalSystemCheckLrdPaymentError;
        return response;
      }
      
      var paymentType = LrdPaymentTypes.GetAll().Where(x => x.Id == PaymentTypeId).FirstOrDefault();
      if (paymentType == null)
      {
        Logger.ErrorFormat("CheckLrdPaymentInExternalSystem. PaymentTypeId: {0}. Не найден тип выплат по ИД.", PaymentTypeId);
        response.Result = false;
        response.RemarkText = DirRX.NNOtherProcesses.Resources.ExternalSystemCheckLrdPaymentError;
        return response;
      }
      
      var request = NNIntegration.Structures.Module.LrdPaymentCheckRequest.Create();
      request.RequestCode = Guid.NewGuid().ToString();
      request.Employee = employee.PersonnelNumber;
      request.EventReason = paymentType.EventReason.Code;
      request.Refinement = paymentType.EventReasonClarification.Code;
      request.Purpose = paymentType.PaymentPurpose != null ? paymentType.PaymentPurpose.Code : string.Empty;
      request.ChildName = ChildName;
      request.ICCD = Inila;
      request.BirthDate = BirthDate;
      request.CurrentDate = Calendar.Now.ToString("dd.MM.yyyy");
      request.FireDate = FireDate.HasValue ? FireDate.Value.ToString("dd.MM.yyyy") : string.Empty;
      
      var extResponse = CheckLrdPaymentInExternalSystem(request, 0);
      if (extResponse == null)
      {
        Logger.DebugFormat("CheckLrdPaymentInExternalSystem. {0}", DirRX.NNOtherProcesses.Resources.ExternalSystemCheckLrdPaymentError);
        response.Result = false;
        response.RemarkText = DirRX.NNOtherProcesses.Resources.ExternalSystemCheckLrdPaymentError;
        
        return response;
      }
      
      return extResponse;
    }
    
    /// <summary>
    /// Проверить заявление на выплату по уходу на пенсию.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="paymentType">Тип выплаты.</param>
    /// <param name="statement">Заявление.</param>
    /// <returns>Результат проверки.</returns>
    public Structures.Module.IVerificationResultPayment CheckRetirementPayment(OverrideSolution.IEmployee employee, ILrdPaymentType paymentType, ILrdPaymentStatementDocument statement)
    {
      var result = DirRX.NNOtherProcesses.Structures.Module.VerificationResultPayment.Create();
      var statements = LrdPaymentStatementDocuments.GetAll(s => Employees.Equals(s.EmployeeDirRX, employee) &&
                                                           LrdPaymentTypes.Equals(s.PaymentType, paymentType) &&
                                                           !Equals(s, statement) &&
                                                           (s.InternalApprovalState == OverrideSolution.OfficialDocument.InternalApprovalState.OnApproval ||
                                                            s.InternalApprovalState == OverrideSolution.OfficialDocument.InternalApprovalState.OnRework ||
                                                            s.InternalApprovalState == OverrideSolution.OfficialDocument.InternalApprovalState.Approved));
      
      if (statements.Any())
      {
        result.CommentText = DirRX.NNOtherProcesses.Resources.ExistStatementError;
        result.VerificationResult = false;
      }
      else
      {
        result.VerificationResult = true;
        result.CommentText = string.Empty;
      }
      
      return result;
    }
    
    #endregion

    #region IF-S-25360. Получение списка детей работника (АСУП → КЭДО).

    /// <summary>
    /// Получить информацию о детях сотрудника из внешней системы.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Список детей. Если возникла ошибка или у сотрудника нет детей - пустой список.</returns>
    public List<IChild> GetEmployeeChildrenFromExternalSystem(long employeeId)
    {
      var flowGuid = NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.LrdPaymentChildren;
      var logPrefix = string.Format("GetEmployeeChildrenFromExternalSystem. GUID потока: {0}. ИД сотрудника: {1}.", flowGuid, employeeId);
      var children = new List<IChild>();
      
      var employee = Employees.GetAll().Where(x => x.Id == employeeId).FirstOrDefault();
      if (employee == null)
      {
        Logger.ErrorFormat("{0} Не удалось найти сотрудника по ИД.", logPrefix);
        return children;
      }
      
      var request = NNIntegration.Structures.Module.LrdPaymentChildrenRequest.Create(Guid.NewGuid().ToString(),
                                                                                     employee.PersonnelNumber);
      
      Logger.DebugFormat("{0} Старт синхронной отправки сообщения во внешнюю систему.", logPrefix);
      var requestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(request);
      
      // Отправить исходящее сообщение.
      var resultSendingMessage = NNIntegration.PublicFunctions.Module.SendMessageToExternalSystem(requestText, flowGuid, 0);
      
      if (!string.IsNullOrEmpty(resultSendingMessage.ErrorText))
      {
        Logger.ErrorFormat("{0} Ошибка отправки исходящего сообщения. Текст ошибки: {1}.", logPrefix, resultSendingMessage.ErrorText);
        return children;
      }
      
      var response = NNIntegration.Structures.Module.LrdPaymentChildrenResponse.Create();
      try
      {
        response = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(resultSendingMessage.AnswerText, response);
        children = response.Children;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0} Ошибка десериализации в структуру. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        return children;
      }
      
      Logger.DebugFormat("{0} Конец синхронной отправки сообщения.", logPrefix);
      return children;
    }

    /// <summary>
    /// Получить информацию о детях сотрудника (перегрузка для ЛК).
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Текст ошибки. Если ошибок - нет, то возвращается пуствя строка.</returns>
    /// <remarks>Функция выполняет запрос во внешнюю систему для получения списка детей и кэширует его в БД.</remarks>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetChildrenByEmployeeId(long employeeId)
    {
      Logger.DebugFormat("GetChildrenInfoByEmployeeId. EmployeeId: {0}. Получен запрос на предоставление информации о детях сотрудника.", employeeId);
      
      // Поиск кэшированного ответа в БД на исходящий запрос для получения информации о детях сотрудника.
      var cachedResponse = GetCachedChildrenInfoByEmployee(employeeId);
      if (!string.IsNullOrEmpty(cachedResponse))
        return cachedResponse;
      
      // Выполнение запроса во внешнюю систему для получения списка детей.
      var children = GetEmployeeChildrenFromExternalSystem(employeeId);
      var json = EssPlatform.PublicFunctions.Module.SerializedToJson(children);
      
      // Кэширование полученного ответа с информацией о детях сотрудника в БД.
      InsertOrUpdateChildrenResponseCache(employeeId, json);
      return json;
    }

    /// <summary>
    /// Получить информацию о детях сотрудника из кэша (таблица в БД).
    /// </summary>
    /// <param name="employee">ИД сотрудника.</param>
    /// <returns>Json c инфомрацией о детях. Если информации нет - то пустая строка.</returns>
    public string GetCachedChildrenInfoByEmployee(long employeeId)
    {
      var response = string.Empty;
      try
      {
        var key = string.Format(Constants.Module.LrdPaymentChildrenCacheKeyFormat, employeeId);
        if (Cache.TryGetValue(key, out response))
          Logger.DebugFormat("GetCachedChildrenInfoByEmployee. ИД сотрудника: {0}. Кэшированный запрос - обнаружен.", employeeId);
        else
          Logger.DebugFormat("GetCachedChildrenInfoByEmployee. ИД сотрудника: {0}. Кэшированный запрос - не обнаружен.", employeeId);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetCachedChildrenInfoByEmployee. ИД сотрудника: {0}. Ошибка: {1}. StackTrace: {2}.", employeeId, ex.Message, ex.StackTrace);
      }
      
      return response;
    }

    /// <summary>
    /// Обновить информацию о детях сотрудника в кэше (таблица в БД).
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="response">Ответ.</param>
    /// <returns>Json c инфомрацией о детях. Если информации нет - то пустая строка.</returns>
    public void InsertOrUpdateChildrenResponseCache(long employeeId, string response)
    {
      try
      {
        // Получить время в минутах из прикладной константы, по истечению которых необходимо чистить кэшированные запросы.
        var minutes = DirRX.NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.LrdPaymentChildrenCacheTime.Guid);
        if (minutes == null || minutes.Value < 0)
        {
          Logger.ErrorFormat("InsertOrUpdateChildrenResponseCache. Константа: {0}. Значение: {1}. Не удалось определить количество минут, по истечению которых необходо удалить кэш. Будет использовано значение по умолчанию: {2}.",
                             Constants.Module.ConstantsSetting.LrdPaymentChildrenCacheTime.Guid, minutes, Constants.Module.ConstantsDefaultValues.LrdPaymentChildrenCacheTime);
          minutes = Constants.Module.ConstantsDefaultValues.LrdPaymentChildrenCacheTime;
        }
        
        var key = string.Format(Constants.Module.LrdPaymentChildrenCacheKeyFormat, employeeId);
        Cache.AddOrUpdate(key, response, Calendar.Now.AddMinutes(minutes.Value));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("InsertOrUpdateChildrenResponseCache. ИД сотрудника: {0}. Ошибка: {1}, StackTrace: {2}.", employeeId, ex.Message, ex.StackTrace);
      }
    }

    #endregion

    #region IF-S-10399. Передача приказа на выплату по ЛНА из АСУП в КЭДО.

    /// <summary>
    /// Изменить или создать приказ на приказа на выплату по ЛНА.
    /// </summary>
    /// <param name="idIncomingMessage">Id записи справочника Входящее информационное сообщение.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public NNIntegration.Structures.Module.IProcessIncomingMessageResult UpdateOrCreateLrdPaymentOrder(long idIncomingMessage)
    {
      var logPrefix = string.Format("UpdateOrCreateLrdPaymentOrder. Ид входящего сообщения: {0}.", idIncomingMessage);
      Logger.Debug(logPrefix);
      
      var result = NNIntegration.Structures.Module.ProcessIncomingMessageResult.Create();
      result.Retry = false;
      var errorMessage = string.Empty;
      
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
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var model = NNIntegration.Structures.Module.LrdPaymentOrderRequest.Create();
      try
      {
        model = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка десериализации в структуру LrdPaymentOrderRequest.";
        Logger.ErrorFormat("{0} {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, errorMessage, ex.Message, ex.StackTrace);
        result.Retry = true;
        return result;
      }
      
      var employeeResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Employee);
      var signedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Signed);
      var preparedResult = OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumberWithError(model.Prepared);
      
      if (!string.IsNullOrWhiteSpace(employeeResult.ErrorMessage))
        errorMessage += string.Format("EmployeeError: {0}. ", employeeResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(signedResult.ErrorMessage))
        errorMessage += string.Format("SignedError: {0}. ", signedResult.ErrorMessage);
      if (!string.IsNullOrWhiteSpace(preparedResult.ErrorMessage))
        errorMessage += string.Format("PreparedError: {0}.", preparedResult.ErrorMessage);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        result.ErrorText = errorMessage;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var employee = employeeResult.Employee;
      var signed = signedResult.Employee;
      var prepared = preparedResult.Employee;
      
      // Поиск / создание нового документа и заполнение его полей.
      var document = LrdPaymentOrderDocuments.GetAll(x => x.ExternalId == model.SAPID).FirstOrDefault();
      if (document == null)
      {
        Logger.DebugFormat("{0} Документ по SAPID: {1} не найден. Создание нового документа.", logPrefix, model.SAPID);
        document = NNOtherProcesses.PublicFunctions.LrdPaymentOrderDocument.CreateLrdPaymentOrderWithEmployee(employee, NNOtherProcesses.PublicConstants.Module.DocumentKind.LrdPaymentOrderDocument);
        if (document == null)
        {
          result.ErrorText = string.Format("Не удалось создать документ с SAP ID: {0}. Подробные сведения об ошибках в лог файлах.", model.SAPID);
          Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
          return result;
        }
      }
      
      result.EntityId = document?.Id;
      
      var checkResult = OverrideSolution.PublicFunctions.DocRelatedToWorkBase.CanUpdateDocFromASUP(document);
      if (!string.IsNullOrEmpty(checkResult))
      {
        result.ErrorText = string.Format("Версия документа с ID: {0} не создана. {1}.", document.Id, checkResult);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      if (!Locks.GetLockInfo(document).IsLockedByMe && !Locks.TryLock(document))
      {
        var lockInfo = Locks.GetLockInfo(document);
        result.ErrorText = lockInfo.LockedMessage;
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      try
      {
        document.EmployeeDirRX = employee;
        document.OurSignatory = signed;
        document.PreparedBy = prepared;
        document.RegistrationNumber = model.RegNumber;
        document.RegistrationDate = model.Date;
        document.ExternalId = model.SAPID;
        var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.LrdPaymentOrderExtension.Guid);
        
        // Документ сохраняется при создании новой версии.
        errorMessage = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, model.Body, extension);
        if (!string.IsNullOrWhiteSpace(errorMessage))
          throw AppliedCodeException.Create(errorMessage);
        
        Logger.DebugFormat("{0} Документ с ИД: {1} был сохранен.", logPrefix, document.Id);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", document.Id, ex.Message, ex.StackTrace);
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }

      result.ErrorText = this.CreateAndStartNewSignDocumentSubtask(model.TaskID, prepared, document);
      if (string.IsNullOrEmpty(result.ErrorText))
        Logger.DebugFormat("{0}. Успешно обработан.", logPrefix);
      
      return result;
    }

    #endregion

    #endregion
    
    #region Интеграционный сценарий «Массовое формирование и подписание ДС о пересмотре ЗП».
    
    #region IF-S-28562. Создание карточки дополнительного соглашения к трудовому договору в КЭДО из АСУП (АСУП → КЭДО).

    /// <summary>
    /// Обработка создания карточки дополнительного соглашения к трудовому договору.
    /// </summary>
    /// <param name="incomingMessageId">ИД записи справочника 'Входящие информационные сообщения'.</param>
    /// <returns>Результат обработки.</returns>
    [Public]
    public IProcessIncomingMessageResult ProcessEmploymentContractAdditionalAgreement(long incomingMessageId)
    {
      var logPrefix = string.Format("ProcessEmployeeContractSupAgreement. Ид записи справочника 'Входящие информационные сообщения': {0}", incomingMessageId);
      var result = ProcessIncomingMessageResult.Create();
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == incomingMessageId).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.ErrorText = "Не найдена запись в справочнике 'Входящие информационные сообщения'.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      IEmploymentContractAdditionalAgreement structure = null;
      
      try
      {
        structure = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Ошибка десериализации. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, result.ErrorText,  ex.StackTrace);
        return result;
      }
      
      var employee = DirRX.OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumber(structure.Employee);
      var ourSignatory = DirRX.OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumber(structure.Supervisor);
      var employeeWhoPrepared = DirRX.OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumber(structure.Prepared);

      if (employee == null || ourSignatory == null || employeeWhoPrepared == null)
      {
        if (employee == null)
          result.ErrorText += string.Format("Не найден сотрудник с табельным номером: {0}. ", structure.Employee);
        if (ourSignatory == null)
          result.ErrorText += string.Format("Не найден подписывающий с табельным номером: {0}. ", structure.Supervisor);
        if (employeeWhoPrepared == null)
          result.ErrorText += string.Format("Не найден сотрудник подготовивший документ с табельным номером: {0}. ", structure.Prepared);

        result.ErrorText = result.ErrorText.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var document = DirRX.OverrideSolution.DocRelatedToWorks.GetAll(c => c.ExternalId == structure.SAPID).FirstOrDefault();
      result.EntityId = document?.Id;
      
      if (document != null)
      {
        // Обновление возможно только для документов, у которых значения полей жизненного цикла: \"Состояние документа HR Pro\" - \"В разработке\", \"Согласование\" - не заполнено или  \"На доработке\".
        var isValid = document.LifeCycleState == HRPro.HRProSolution.OfficialDocument.LifeCycleState.Draft &&
          (!document.InternalApprovalState.HasValue || document.InternalApprovalState == HRPro.HRProSolution.OfficialDocument.InternalApprovalState.OnRework);
        
        if (!isValid)
        {
          var errorMsg = "Обновление возможно только для документов, у которых значения полей жизненного цикла: \"Состояние документа HR Pro\" - \"В разработке\", \"Согласование\" - не заполнено или \"На доработке\"";
          result.ErrorText = string.Format("Версия документа с ID: {0} не создана. {1}.", document.Id, errorMsg);
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;
        }
      }
      
      document = document ?? DirRX.OverrideSolution.DocRelatedToWorks.Create();
      result = UpdateOrCreateEmploymentContractAdditionalAgreement(document, structure, employee, ourSignatory, employeeWhoPrepared);
      if (!string.IsNullOrEmpty(result.ErrorText))
        return result;

      result.ErrorText = SendTaskToSignEmploymentContractAdditionalAgreement(document, employeeWhoPrepared);
      
      if (string.IsNullOrEmpty(result.ErrorText))
        Logger.DebugFormat("{0}. Успешно обработан.", logPrefix);
      
      result.EntityId = document?.Id;
      return result;
    }
    
    /// <summary>
    /// Создать или обновить документ "Дополнительное соглашение к трудовому договору".
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="structure">Структура с данными документа.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="ourSignatory">Подписал (Работодатель).</param>
    /// <param name="preparedBy">Подготовил.</param>
    /// <returns>Текст ошибки, Пустая строка, если ошибок нет.</returns>
    private IProcessIncomingMessageResult UpdateOrCreateEmploymentContractAdditionalAgreement(DirRX.OverrideSolution.IDocRelatedToWork document, IEmploymentContractAdditionalAgreement structure, DirRX.OverrideSolution.IEmployee employee,
                                                                                              DirRX.OverrideSolution.IEmployee ourSignatory, DirRX.OverrideSolution.IEmployee preparedBy)
    {
      var result = ProcessIncomingMessageResult.Create();
      if (!Locks.TryLock(document))
      {
        var locksInfo = Locks.GetLockInfo(document);
        result.ErrorText = string.Format("ИД документа: {0}. Текст ошибки: Не удалось заблокировать документ. Заблокирован: {1}. Время установки блокировки: {2}.", document.Id, locksInfo.OwnerName, locksInfo.LockTime);
        result.Retry = true;
        Logger.ErrorFormat("UpdateOrCreateEmploymentContractAdditionalAgreement. {0}", result.ErrorText);
        return result;
      }
      
      try
      {
        var isInserted = document.State.IsInserted;
        
        // Вид документа.
        if (isInserted)
        {
          var documentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.AdditionalAgreementKind);
          document.DocumentKind = documentKind;
        }
        
        // Рег. номер.
        if (isInserted || !document.RegistrationNumber.Equals(structure.RegNumber))
          document.RegistrationNumber = structure.RegNumber;
        // Дата докумена.
        if (isInserted || document.RegistrationDate != structure.Date)
          document.RegistrationDate = structure.Date;
        // Сотрудник.
        if (isInserted || !Employees.Equals(document.EmployeeDirRX, employee))
          document.EmployeeDirRX = employee;
        // Подразделение.
        if (isInserted || !Departments.Equals(document.EmployeeDirRX.Department, employee.Department))
          document.Department = employee.Department;
        // Подготовил.
        if (isInserted || !Employees.Equals(document.PreparedBy, preparedBy))
          document.PreparedBy = preparedBy;
        // SAP ID.
        if (isInserted || !document.ExternalId.Equals(structure.SAPID))
          document.ExternalId = structure.SAPID;
        // Подписал.
        if (isInserted || !Employees.Equals(document.OurSignatory, ourSignatory))
          document.OurSignatory = ourSignatory;
        
        var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.EmploymentContractAdditionalAgreementExtension.Guid);
        if (string.IsNullOrEmpty(extension))
        {
          result.ErrorText = string.Format("Прикладная константа '{0}' не заполнена.", Constants.Module.ConstantsSetting.EmploymentContractAdditionalAgreementExtension.Name);
          return result;
        }
        
        // Документ сохраняется во время создания новой версии.
        var error = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(document, structure.Body, extension);
        if (!string.IsNullOrEmpty(error))
          throw AppliedCodeException.Create(error);
        
        document.Save();
      }
      catch (Exception ex)
      {
        var error = string.Format("ИД документа: {0}. Ошибка при сохранении документа. Текст ошибки: {1}. StackTrace: {2}.", document.Id, ex.Message, ex.StackTrace);
        result.ErrorText = error;
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }
      
      return result;
    }

    /// <summary>
    /// Отправить задачу на подписание дополнительного соглашения к трудовому договору.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="author">Автор.</param>
    /// <returns>Текст ошибки, string.Empty если ошибок нет.</returns>
    /// <remarks>Создание и отправка задачи выполняется в транзакции.</remarks>
    private string SendTaskToSignEmploymentContractAdditionalAgreement(DirRX.OverrideSolution.IDocRelatedToWork document, IEmployee author)
    {
      var logPrefix = string.Format("SendTaskToSignBusinessTripOrder. Документ ИД: {0}", document.Id);
      
      var isStarted = HRPro.HRDocFlow.HRProcessTasks.GetAll(t => DirRX.OverrideSolution.OfficialDocuments.Equals(t.MainDoc, document) && t.Status == Status.InProcess).Any();
      if (isStarted)
      {
        Logger.DebugFormat("{0}. Задача на подписание дополнительного соглашения к трудовому договору. Повторная отправка не требуется.", logPrefix);
        return string.Empty;
      }
      
      var executionResult = false;
      var errorMessage = string.Empty;
      executionResult = Transactions.Execute(
        () =>
        {
          try
          {
            // Создание задачу для подписания ДС к трудовому договору.
            var task = HRPro.HRDocFlow.PublicFunctions.Module.Remote.CreateNewSignDocumentTask(author, true, false);
            task.DocumentGroup.OfficialDocuments.Add(document);
            var processKind = Sungero.Workflow.ProcessKinds.GetAllMatches(task).FirstOrDefault();
            if (processKind == null)
            {
              errorMessage = "Не удалось найти подходящий вариант процесса для отправки задачи на подписание дополнительного соглашения к трудовому договору.";
              return;
            }
            
            task.Start();
          }
          catch (Exception ex)
          {
            errorMessage = string.Format("Произошла ошибка отправки задачи на подписание дополнительного соглашения к трудовому договору. Текст ошибки: {0}", ex.Message);
            Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, errorMessage, ex.StackTrace);
            throw ex;
          }
        });

      if (!executionResult && string.IsNullOrEmpty(errorMessage))
        errorMessage = "Произошла ошибка отправки задачи на подписание дополнительного соглашения к трудовому договору. Транзакция не выполнена.";
      
      return errorMessage;
    }
    
    #endregion
    
    #endregion
    
    #region Рассылка отчета «Сотрудники, не подписавшие Дополнительное соглашение к трудовому договору».
    
    /// <summary>
    /// Получение сотрудников для построения отчета. Поиск выполняется через sql запрос.
    /// </summary>
    /// <param name="docKindId">ИД вида документа.</param>
    /// <returns>Сотрудники.</returns>
    public IQueryable<DirRX.OverrideSolution.IEmployee> GetReportEmployees(long docKindId)
    {
      var reportEmployeeIds = new List<long>();
      try
      {
        using (var connection = SQL.CreateConnection())
        {
          using (var command = connection.CreateCommand())
          {
            command.CommandText = string.Format(Queries.Module.GetNotSignedAdditionalAgreementAssigments, docKindId.ToString());
            using(var reader = command.ExecuteReader())
            {
              while (reader.Read())
              {
                if (reader["count"].ToString() != "0")
                {
                  reportEmployeeIds.Add(Int64.Parse(reader["performer"].ToString()));
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetReportEmployees. ИД вида документа: {0}. Ошибка выполнения. Текст ошибки: {1}. StackTrace: {2}.", docKindId, ex.Message, ex.StackTrace);
      }
      
      return DirRX.OverrideSolution.Employees.GetAll().Where(x => reportEmployeeIds.Contains(x.Id));
    }
    
    /// <summary>
    /// Построение отчета.
    /// </summary>
    /// <param name="employee">Сотрудник, по которому строится отчет.</param>
    /// <param name="docKindId">Вид документа.</param>
    /// <returns>Поток данных сформированного отчета.</returns>
    public System.IO.Stream GetNotSignedAdditionalAgreementReport(DirRX.OverrideSolution.IEmployee employee, long docKindId, string hyperlinkTemplate)
    {
      System.IO.Stream reportStream = null; // new System.IO.MemoryStream();
      try
      {
        var report = DirRX.NNOtherProcesses.Reports.GetNotSignedAdditionalAgreementReport();
        report.EmployeeId = employee.Id;
        report.DocKindId = docKindId;
        report.HyperlinkTemplate = hyperlinkTemplate;
        
        reportStream = report.Export();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetNotSignedAdditionalAgreementReport. ИД сотрудника: {0}, ИД вида документа: {1}. Ошибка выполнения. Текст ошибки: {2}. StackTrace: {3}.",
                           employee.Id, docKindId, ex.Message, ex.StackTrace);
      }
      
      return reportStream;
    }
    
    /// <summary>
    /// Отправка сформированного отчета на электронную почту сотрудника.
    /// </summary>
    /// <param name="addressee">Сотрудник, которому отправляется отчет.</param>
    /// <param name="reportSteam">Поток данных сформированного отчета.</param>
    /// <param name="reportName">Наименование сформированного отчета.</param>
    public void SendNotSignedAdditionalAgreementReport(DirRX.OverrideSolution.IEmployee addressee, System.IO.Stream reportSteam, string reportName)
    {
      var logPrefix = string.Format("SendNotSignedAdditionalAgreementReport. ИД сотрудника: {0}", addressee.Id);
      try
      {
        var email = addressee.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
          Logger.ErrorFormat("{0}. Ошибка при отправки письма на почту. В карточке сотрудника с ИД={1} не указан Email.", logPrefix, addressee.Id);
          return;
        }
        
        var subject = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.NotSignedAdditionalAgreementLetterSubject.Guid);
        if (string.IsNullOrEmpty(subject))
        {
          Logger.DebugFormat("{0}. Прикладная константа '{1}' не заполнена.", logPrefix, DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.NotSignedAdditionalAgreementLetterSubject.Guid);
          subject = Constants.Module.ConstantsDefaultValues.NotSignedAdditionalAgreementLetterSubject;
        }
        
        var message = Mail.CreateMailMessage();
        message.Subject = subject;
        message.Body = string.Empty;
        message.IsBodyHtml = false;
        message.Priority = MailPriority.Normal;
        message.To.Add(email);
        message.AddAttachment(reportSteam, reportName);
        
        Mail.Send(message);
        Logger.Debug("{0}. Сообщение успешно отправлено на почту сотрудника.", logPrefix);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при отправки письма на почту. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
    }
    
    #endregion
    
  }
}