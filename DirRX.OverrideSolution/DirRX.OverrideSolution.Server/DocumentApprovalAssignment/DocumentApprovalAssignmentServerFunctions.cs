using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentApprovalAssignment;

namespace DirRX.OverrideSolution.Server
{
  partial class DocumentApprovalAssignmentFunctions
  {

    /// <summary>
    /// Является ли исполнитель задания доп. согласущим?
    /// </summary>
    [Remote]
    public bool IsPerformerAdditional()
    {
      var performerId = _obj.Performer.Id.ToString();
      
      // Если добавили в Задаче на обработку документов, связанных с работой.
      var task = HRPro.HRDocFlow.HRProcessTasks.As(_obj.Task);
      if (task != null)
      {
        var approvers = task.AdditionalApprovers.Select(a => a.AdditionalApprover.Id.ToString());
        if (approvers.Contains(performerId))
          return true;
      }
      
      // Если добавляли в заданиях.
      var assignmentsIds = DocumentApprovalAssignments.GetAll()
        .Where(a => Sungero.Workflow.Tasks.Equals(a.Task, _obj.Task))
        .Where(a => a.TaskStartId == _obj.TaskStartId && a.IterationId == _obj.IterationId)
        .Where(a => !Equals(_obj, a))
        .Select(a => a.Id);
      if (!assignmentsIds.Any())
        return false;
      
      var operation = new Enumeration(Sungero.Docflow.PublicConstants.ApprovalAssignment.AddApprover);
      var histories = Sungero.Workflow.WorkflowHistories.GetAll()
        .Where(h => h.EntityType == HRPro.HRDocFlow.PublicConstants.Module.EntityTypeGuid.DocumentApprovalAssignment)
        .Where(h => h.EntityId != null && assignmentsIds.Contains(h.EntityId.Value))
        .Where(h => h.Operation == operation && h.OperationDetailed == operation);
      
      foreach (var h in histories)
      {
        if (string.IsNullOrEmpty(h.Comment))
          continue;
        
        var splittedComment = h.Comment.Split('|');
        if (splittedComment.Count() != 2)
          continue;
        
        if (performerId == splittedComment[1])
          return true;
      }
      
      return false;
    }

    /// <summary>
    /// Построить модель состояния сводки.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote]
    public StateView GetDocumentApprovalAssignmentSummary()
    {
      // TODO: Заменить на вызов коробочной функции после добавления возможности.
      return DirRX.OverrideSolution.Module.HRDocFlow.Functions.Module.GetAssignmentStateView(_obj.SummaryDirRX);
    }

    /// <summary>
    /// Получить последних исполнителей, выполнивших задание не с результатом Переадресовать.
    /// </summary>
    /// <param name="taskId">ИД задачи.</param>
    /// <param name="blockUid">ИД блока.</param>
    /// <param name="startId">Количество стартов задачи.</param>
    /// <returns>Последние исполнители.</returns>
    [Public]
    public static IQueryable<IUser> GetLastPerformers(long taskId, string blockUid, int? startId)
    {
      var lastIter = DocumentApprovalAssignments.GetAll()
        .Where(a => a.Task.Id == taskId && a.BlockUid == blockUid && a.TaskStartId == startId)
        .OrderByDescending(a => a.IterationId)
        .Select(a => a.IterationId)
        .FirstOrDefault();
      
      if (lastIter != null)
        Logger.DebugFormat("GetLastPerformers. TaskId: {0}, BlockUid: {1}, StartId: {2}. Задание на согласование документов. Итерация: {3}", taskId, blockUid, startId, lastIter);
      
      return DocumentApprovalAssignments.GetAll()
        .Where(a => a.Task.Id == taskId &&
               a.BlockUid == blockUid &&
               a.IterationId == lastIter &&
               a.TaskStartId == startId &&
               a.Result != DirRX.OverrideSolution.HRAssignment.Result.Forward)
        .Select(a => a.CompletedBy);
    }

    /// <summary>
    /// Проверить, можно ли добавить сотрудника в процесс согласования.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>True, если сотрудника можно добавлять.</returns>
    [Remote(IsPure = true)]
    public virtual bool CanForwardTo(IEmployee employee)
    {
      if (employee == null && Employees.Equals(Employees.Current, employee))
        return false;
      
      var assignments = DocumentApprovalAssignments.GetAll(a => Sungero.Workflow.Tasks.Equals(a.Task, _obj.Task) &&
                                                           a.TaskStartId == _obj.TaskStartId &&
                                                           a.IterationId == _obj.IterationId);
      
      // Если у сотрудника есть задание в работе, то не создам дубль.
      var hasInProcess = assignments.Where(a => a.Status == OverrideSolution.DocumentApprovalAssignment.Status.InProcess &&
                                           Employees.Equals(a.Performer, employee)).Any();
      
      if (hasInProcess)
        return false;
      
      // Если у сотрудника нет заданий в работе, проверяем, все ли его задания созданы.
      var assignmentList = assignments.ToList();
      foreach (var assignment in assignmentList)
      {
        var added = assignment.ForwardedTo.Count(u => Users.Equals(u, employee));
        var created = assignmentList
          .Where(a => Users.Equals(a.Performer, employee) &&
                 Sungero.Workflow.Assignments.Equals(a.ForwardedFrom, assignment) &&
                 a.Status == OverrideSolution.DocumentApprovalAssignment.Status.InProcess)
          .Count();
        
        if (added < created)
          return false;
      }
      
      return true;
    }

    /// <summary>
    /// Переадресовать задание сотруднику.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    [Remote]
    public void ForwardToNewApprover(IEmployee employee)
    {
      var isParallel = _obj.IsParallelDirRX == true;
      try
      {
        if (isParallel)
          _obj.Forward(employee);
        else
          _obj.Forward(employee, ForwardingLocation.Next);
        
        // Добавить информацию в историю.
        var operation = new Enumeration(Sungero.Docflow.PublicConstants.ApprovalAssignment.AddApprover);
        var comment = string.Join("|", Sungero.Company.PublicFunctions.Employee.GetShortName(employee, false), employee.Id);
        _obj.History.Write(operation, operation, comment);
        
        Logger.DebugFormat("ForwardToNewApprover. ИД задания: {0}. Паралелльные задания: {1}. Задание отправлено дополнительному согласующему с ИД: {2}.",
                           _obj.Id, isParallel, employee.Id);
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("ForwardToNewApprover. ИД задания: {0}. Паралелльные задания: {1}. Ошибка при переадресации дополнительному согласующему с ИД: {2}. Текст ошибки: {3}. StackTrace: {4}.",
                           _obj.Id, isParallel, employee.Id, ex.Message, ex.StackTrace);
      }
    }

    /// <summary>
    /// Создать задачу для запроса мнения.
    /// </summary>
    /// <remarks>Созданная и несохраненная задача.</remarks>
    [Remote]
    public HRPro.HRDocFlow.IHRSimpleTask CreateOpinionSubTask()
    {
      var processId = DirRX.NNCommons.PublicFunctions.Module.GetIdentifierValue(NNOtherProcesses.PublicConstants.Module.ConstantsSetting.OpinionTask.Guid);
      if (processId == null)
      {
        Logger.ErrorFormat("StartOpinionSubTask. Не заполнена константа \"{0}\".", NNOtherProcesses.PublicConstants.Module.ConstantsSetting.OpinionTask.Guid);
        return HRPro.HRDocFlow.HRSimpleTasks.Null;
      }
      
      var processKind = Sungero.Workflow.ProcessKinds.GetAll(x => x.Id == processId.Value).FirstOrDefault();
      if (processKind == null)
      {
        Logger.ErrorFormat("StartOpinionSubTask. Не найден вариант процесса с ИД: {0}.", processId.Value);
        return HRPro.HRDocFlow.HRSimpleTasks.Null;
      }
      
      var subTask = HRPro.HRDocFlow.HRSimpleTasks.CreateAsSubtask(_obj.Task);
      subTask.Subject = string.Format("{0}{1}", ">>", _obj.Subject);
      subTask.ProcessKind = processKind;
      subTask.State.Attachments.AddendaGroup.IsVisible = false;
      subTask.State.Attachments.OtherGroup.IsVisible = false;
      subTask.State.Properties.ProcessKind.IsEnabled = false;
      
      foreach (var doc in _obj.DocumentGroup.OfficialDocuments)
        subTask.DocumentGroup.OfficialDocuments.Add(doc);
      
      foreach (var doc in _obj.AddendaGroup.OfficialDocuments)
        subTask.AddendaGroup.OfficialDocuments.Add(doc);
      
      foreach (var doc in _obj.OtherGroup.All)
      {
        var document = Sungero.Docflow.OfficialDocuments.As(doc);
        if (document != null)
          subTask.OtherGroup.All.Add(document);
      }
      
      return subTask;
    }

  }
}