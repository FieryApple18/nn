using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.NNVacations.VacationScheduleManagementTask;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations.Server.VacationScheduleManagementTaskBlocks
{
  partial class SetDocumentDetailsOnTextMarksScriptBlockHandlers
  {

    public virtual void SetDocumentDetailsOnTextMarksScriptBlockExecute()
    {
      var document = _obj.DocumentGroup.VacationScheduleDocuments.FirstOrDefault();
      
      if (document == null || !document.HasVersions)
      {
        throw AppliedCodeException.Create("SetDocumentDetailsOnTextMarks. Документ не найден или не содержит версию.");
      }
      
      var version = document.Versions.FirstOrDefault();
      
      try
      {
        if (!Locks.TryLock(document) || !Locks.TryLock(version.Body))
        {
          Logger.Debug("SetDocumentDetailsOnTextMarks. Документ или его версия заблокирована.");
          _block.OutProperties.ExecutionResult = ExecutionResult.LockError;
          return;
        }

        var textReplacements = OverrideSolution.PublicFunctions.VacationScheduleDocument.PrepareDataForInsertingIntoDocument(document);
        using (var documentStream = version.Body.Read())
        {
          using(var resultDocumentStream = NNCommons.IsolatedFunctions.SetTextIntoDocument.SetTextIntoPdfDocument(documentStream, textReplacements))
          {
            document.CreateVersionFrom(resultDocumentStream, "pdf");
          }
        }
        
        document.Save();
      }
      catch (Exception ex)
      {
        throw AppliedCodeException.Create("SetDocumentDetailsOnTextMarks. Возникла непредвиденная ошибка.", ex);
      }
      finally
      {
        if (Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
        
        if (Locks.GetLockInfo(version.Body).IsLockedByMe)
          Locks.Unlock(version.Body);
      }
    }

  }

  partial class InitiatingApprovalBlockHandlers
  {

    public virtual void InitiatingApprovalBlockEnd(System.Collections.Generic.IEnumerable<DirRX.NNVacations.IInitiatingApprovalAssignment> createdAssignments)
    {
      _obj.LastInitiatingApprovalAssignment = createdAssignments.FirstOrDefault(a => a.Status == NNVacations.InitiatingApprovalAssignment.Status.Completed);
    }

    public virtual void InitiatingApprovalBlockCompleteAssignment(DirRX.NNVacations.IInitiatingApprovalAssignment assignment)
    {
      if (assignment.Result == NNVacations.InitiatingApprovalAssignment.Result.Forward)
      {
        var completedAssignment = InitiatingApprovalAssignments.As(assignment);
        _block.Performers.Remove(completedAssignment.Performer);
        Logger.DebugFormat("InitiatingApprovalBlockCompleteAssignment. TaskId: {0}, BlockUid: {1}, AssignmentId: {2}. Переадресация задания. ИД нового исполнителя: {3}, ИД предыдущего исполнителя: {4}.",
                           _obj.Id, _block.Id, assignment.Id, completedAssignment.Addressee.Id, completedAssignment.CompletedBy.Id);
        assignment.Forward(completedAssignment.Addressee, _block.AbsoluteDeadline);
      }
      else if (_obj.DocumentGroup.VacationScheduleDocuments.Any())
      {
        _obj.DaysToRemind = assignment.DaysToRemind;
        _obj.EmployeeAcquaintancePeriod = assignment.EmployeeAcquaintancePeriod;
        _obj.Department = assignment.Department;
        
        var doc = _obj.DocumentGroup.VacationScheduleDocuments.FirstOrDefault();
        doc.StatementNumberDirRX = assignment.StatementNumber;
        doc.StatementDateDirRX = assignment.StatementDate;
        doc.Department = assignment.Department;
        doc.RegistrationNumber = assignment.RegistrationNumber;
        doc.RegistrationDate = assignment.RegistrationDate;
        
        // Заполнение ТЧ Мнение профсоюзов в документе.
        var opinionIds = new List<long>();
        foreach (var opinion in assignment.TradeUnionOpinion)
        {
          var oldOpinion = doc.TradeUnionOpinionDirRX.Where(x => x.Kind.Equals(opinion.Kind) && x.StatementNumber.Equals(opinion.StatementNumber) && x.StatementDate.Equals(opinion.StatementDate)).FirstOrDefault();
          if (oldOpinion == null)
          {
            var newOpinion = doc.TradeUnionOpinionDirRX.AddNew();
            newOpinion.Kind = opinion.Kind;
            newOpinion.StatementNumber = opinion.StatementNumber;
            newOpinion.StatementDate = opinion.StatementDate;
            opinionIds.Add(newOpinion.Id);
          }
          else
            opinionIds.Add(oldOpinion.Id);
        }
        var removeOpinions = doc.TradeUnionOpinionDirRX.Where(x => !opinionIds.Contains(x.Id)).ToList();
        foreach (var removeOpinion in removeOpinions)
          doc.TradeUnionOpinionDirRX.Remove(removeOpinion);

        doc.Save();
      }
    }

    public virtual void InitiatingApprovalBlockStart()
    {
      if (_block.CompetitionWork == true && _block.VirtualEmployee == null)
      {
        var performer = Employees.As(_block.Performers.FirstOrDefault());
        _block.VirtualEmployee = _block.CompetitionWork == true && performer?.IsVirtualDirRX == true ? performer : Employees.Null;
      }
      
      var performers = DirRX.NNVacations.Functions.InitiatingApprovalAssignment.GetLastPerformers(_obj.Id, _block.Id, _obj.StartId);
      if (performers.Any())
      {
        Logger.DebugFormat("InitiatingApprovalBlockStart. TaskId: {0}, BlockUid: {1}. Изменение исполнителей блока. Всего последних исполнителей: {2}.",
                           _obj.Id, _block.Id, performers.Count());
        _block.Performers.Clear();
        foreach (var lastPerformer in performers)
          _block.Performers.Add(lastPerformer);
      }
    }

    public virtual void InitiatingApprovalBlockStartAssignment(DirRX.NNVacations.IInitiatingApprovalAssignment assignment)
    {
      var assignmentHR = InitiatingApprovalAssignments.As(assignment);
      
      // TODO Sannikova_YS. Устаревшие св-ва VirtualEmployee и IsCompetitionWork.
      // Удалить после полного завершения стартованных задач до обновления на 25.2/2.7.
      assignmentHR.VirtualEmployee = _block.VirtualEmployee;
      assignmentHR.IsCompetitionWork = _block.CompetitionWork;
      
      assignmentHR.CanComplete = _block.CanComplete;
      assignmentHR.CanCorrected = _block.CanCorrected;
      assignmentHR.CanForward = _block.CanForward;
      assignmentHR.BusinessUnit = _obj.BusinessUnit;
      assignmentHR.Year = _obj.Year?.Year;
      assignmentHR.LoadingErrorMessage = _obj.LoadingErrorMessage;
      
      var vacationSchedule = _obj.DocumentGroup.VacationScheduleDocuments.FirstOrDefault();
      assignmentHR.ScheduleResponsible = vacationSchedule?.ScheduleResponsibleDirRX ?? _obj.BusinessUnit?.VacationScheduleResponsibleDirRX;
      
      var previousAssigment = _obj.LastInitiatingApprovalAssignment;
      if (previousAssigment != null)
      {
        assignmentHR.Department = previousAssigment.Department;
        assignmentHR.StatementNumber = previousAssigment.StatementNumber;
        assignmentHR.StatementDate = previousAssigment.StatementDate;
        assignmentHR.RegistrationNumber = previousAssigment.RegistrationNumber;
        assignmentHR.RegistrationDate = previousAssigment.RegistrationDate;
        assignmentHR.ApprovalDeadline = previousAssigment.ApprovalDeadline;
        foreach (var previosApprover in previousAssigment.Approvers.Select(x => x.Approver))
        {
          var row = assignmentHR.Approvers.AddNew();
          row.Approver = previosApprover;
          row.ApproverJobTitle = JobTitles.As(previosApprover.JobTitle);
        }
        assignmentHR.AssertingApprover = previousAssigment.AssertingApprover;
        assignmentHR.AssertingApproverJobTitle = previousAssigment.AssertingApproverJobTitle;
        assignmentHR.AssertingApprovalDeadline = previousAssigment.AssertingApprovalDeadline;
        assignmentHR.EmployeeAcquaintancePeriod = previousAssigment.EmployeeAcquaintancePeriod;
        assignmentHR.DaysToRemind = previousAssigment.DaysToRemind;
        foreach (var unionOpinion in previousAssigment.TradeUnionOpinion)
        {
          var row = assignmentHR.TradeUnionOpinion.AddNew();
          row.Kind = unionOpinion.Kind;
          row.StatementNumber = unionOpinion.StatementNumber;
          row.StatementDate = unionOpinion.StatementDate;
        }
      }
    }

  }
  
  partial class SignAcquaintanceListAssignmentBlockHandlers
  {

    public virtual void SignAcquaintanceListAssignmentBlockCompleteAssignment(DirRX.NNVacations.ISignAcquaintanceListAssignment assignment)
    {
      if (assignment.Result == DirRX.NNVacations.SignAcquaintanceListAssignment.Result.Forward)
      {
        _block.Performers.Remove(assignment.Performer);
        Logger.DebugFormat("SignAcquaintanceListAssignmentBlockCompleteAssignment. TaskId: {0}, BlockUid: {1}. Переадресация задания. ИД нового исполнителя: {2}, ИД предыдущего исполнителя: {3}.",
                           _obj.Id, _block.Id, assignment.Addressee.Id, assignment.CompletedBy.Id);
        assignment.Forward(assignment.Addressee, _block.AbsoluteDeadline);
        return;
      }
    }

    public virtual void SignAcquaintanceListAssignmentBlockStartAssignment(DirRX.NNVacations.ISignAcquaintanceListAssignment assignment)
    {
      assignment.ShowInSelfServiceOffice = _block.ShowInSelfServiceOffice.HasValue ? _block.ShowInSelfServiceOffice.Value : false;
      assignment.CanComplete = _block.CanComplete.HasValue ? _block.CanComplete.Value : false;
      assignment.CanForRevision = _block.CanForRevision.HasValue ? _block.CanForRevision.Value : false;
      assignment.CanApprove = _block.CanApprove.HasValue ? _block.CanApprove.Value : false;
      assignment.CanRefuse = _block.CanRefuse.HasValue ? _block.CanRefuse.Value : false;
      assignment.CanTerminate = _block.CanTerminate.HasValue ? _block.CanTerminate.Value : false;
      assignment.SubstituteCanComplete = _block.SubstituteCanComplete.HasValue ? _block.SubstituteCanComplete.Value : false;
      
      // TODO Sannikova_YS. Устаревшие св-ва VirtualEmployee и IsCompetitionWork.
      // Удалить после полного завершения стартованных задач до обновления на 25.2/2.7.
      assignment.VirtualEmployee = _block.VirtualEmployee;
      assignment.IsCompetitionWork = _block.CompetitionWork;
      
      assignment.CanForward = _block.CanForward;
      
      assignment.IsElectronic = _block.IsElectronic;
      
      // Отправить сообщение о новом задании.
      if (_block.NeedNotification.HasValue && _block.NeedNotification.Value == true)
      {
        DirRX.EssPlatform.PublicFunctions.Module.SendNewNotification(HRPro.HRProSolution.Employees.As(assignment.Performer), false, HRPro.HRProSolution.PublicConstants.Module.TargetNameSpace,
                                                                     DirRX.EssPlatform.PublicConstants.Module.SelfOfficeObjectTypes.WorkItems,
                                                                     HRPro.HRDocFlow.PublicConstants.Module.ObjectCardTypeNames.HRAssignment, assignment.Id.ToString(),
                                                                     assignment.ShowInSelfServiceOffice ?? false);
      }
    }

    public virtual void SignAcquaintanceListAssignmentBlockStart()
    {
      if (_block.CompetitionWork == true && _block.VirtualEmployee == null)
      {
        var performer = Employees.As(_block.Performers.FirstOrDefault());
        _block.VirtualEmployee = _block.CompetitionWork == true && performer?.IsVirtualDirRX == true ? performer : null;
      }

      var performers = Functions.SignAcquaintanceListAssignment.GetLastPerformers(_obj.Id, _block.Id, _obj.StartId);
      if (performers.Any())
      {
        Logger.DebugFormat("SignAcquaintanceListAssignmentBlockStart. TaskId: {0}, BlockUid: {1}. Изменение исполнителей блока. Всего последних исполнителей: {2}.",
                           _obj.Id, _block.Id, performers.Count());
        _block.Performers.Clear();
        foreach (var lastPerformer in performers)
          _block.Performers.Add(lastPerformer);
      }
    }

  }

  partial class CreateIndividualAcquiantanceListTasksBlockHandlers
  {

    public virtual void CreateIndividualAcquiantanceListTasksBlockExecute()
    {
      var employeesIds = Functions.VacationScheduleManagementTask.GetEmployeesWithConsent(_obj).Select(e => e.Id);
      var step = NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.EmployeesAmountForIndividualAcquaintanceList.Guid) ??
        Constants.Module.ConstantsDefaultValues.EmployeesAmountForIndividualAcquiantanceList;
      for (var i = 0; i < employeesIds.Count(); i += step)
      {
        var partEmployeesIds = employeesIds.Skip(i).Take(step).ToList();
        var async = AsyncHandlers.CreateIndividualAcquiantanceListTasks.Create();
        async.VacationScheduleManagementTaskId = _obj.Id;
        async.EmployeesIds = string.Join(Constants.Module.ListIdsDelimiter.ToString(), partEmployeesIds);
        async.ExecuteAsync();
      }
    }

  }
}