using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace DirRX.OverrideSolution.Module.HRDocFlow.Server.HRDocFlowBlocks
{
  partial class ESignAssignmentBlockHandlers
  {

    public override void ESignAssignmentBlockCompleteAssignment(HRPro.HRDocFlow.IESignAssignment assignment)
    {
      base.ESignAssignmentBlockCompleteAssignment(assignment);
      
      if (assignment.Result == ESignAssignment.Result.Sign &&
          (NNVacations.VacationScheduleManagementTasks.Is(_obj) || NNVacations.IndividualVacationScheduleAcquaintanceTasks.Is(_obj)))
      {
        var doc = assignment.DocumentGroup.OfficialDocuments.FirstOrDefault(d => VacationScheduleDocuments.Is(d));
        var vacationSchedule = VacationScheduleDocuments.As(doc);
        
        if (vacationSchedule != null)
        {
          vacationSchedule.ApprovalDateDirRX = Calendar.Today;
          vacationSchedule.Save();
        }
      }
    }

    public override void ESignAssignmentBlockStartAssignment(HRPro.HRDocFlow.IESignAssignment assignment)
    {
      var overrideAssignment = DirRX.OverrideSolution.ESignAssignments.As(assignment);
      
      overrideAssignment.MassSigningDirRX = _block.MassSigningDirRX;
      overrideAssignment.SummaryDirRX = _block.SummaryDirRX;
      overrideAssignment.SupernikaSubjectDirRX = _block.SupernikaSubjectDirRX;
      overrideAssignment.SupernikaTextDirRX = _block.SupernikaTextDirRX;
      overrideAssignment.IsSubstitutionAllowedDirRX = _block.IsSubstitutionAllowedDirRX;
      overrideAssignment.IsSubstitutionVisibleDirRX = _block.IsSubstitutionVisibleDirRX;
      
      base.ESignAssignmentBlockStartAssignment(assignment);
    }

  }

  partial class HRAssignmentBlockHandlers
  {

    public override void HRAssignmentBlockCompleteAssignment(HRPro.HRDocFlow.IHRAssignment assignment)
    {
      if (assignment.Result == HRAssignment.Result.Forward)
      {
        var completedAssignment = HRAssignments.As(assignment);
        _block.Performers.Remove(completedAssignment.Performer);
        Logger.DebugFormat("HRAssignmentBlockCompleteAssignment. TaskId: {0}, BlockUid: {1}. Переадресация задания. ИД нового исполнителя: {2}, ИД предыдущего исполнителя: {3}.",
                           _obj.Id, _block.Id, completedAssignment.AddresseeDirRX.Id, completedAssignment.CompletedBy.Id);
        assignment.Forward(completedAssignment.AddresseeDirRX, _block.AbsoluteDeadline);
        return;
      }
      
      base.HRAssignmentBlockCompleteAssignment(assignment);
    }

    public override void HRAssignmentBlockStart()
    {
      base.HRAssignmentBlockStart();
      
      if (_block.CompetitionWorkDirRX == true && _block.VirtualEmployeeDirRX == null)
      {
        var performer = Employees.As(_block.Performers.FirstOrDefault());
        _block.VirtualEmployeeDirRX = _block.CompetitionWorkDirRX == true && performer?.IsVirtualDirRX == true ? performer : Employees.Null;
      }
      
      if (_block.CompetitionWorkDirRX == true && _block.CanForwardDirRX == true)
      {
        // Подменить исполнителей блока на тех, кто выполнили задания по блоку в рамках предыдущей итерации блока с результатом не равным  Переадресовать.
        var performers = DirRX.OverrideSolution.PublicFunctions.HRAssignment.GetLastPerformers(_obj.Id, _block.Id, _obj.StartId);
        if (performers.Any())
        {
          Logger.DebugFormat("HRAssignmentBlockStart. TaskId: {0}, BlockUid: {1}. Изменение исполнителей блока. Всего последних исполнителей: {2}.",
                             _obj.Id, _block.Id, performers.Count());
          _block.Performers.Clear();
          foreach (var lastPerformer in performers)
            _block.Performers.Add(lastPerformer);
        }
      }
    }

    public override void HRAssignmentBlockStartAssignment(HRPro.HRDocFlow.IHRAssignment assignment)
    {
      base.HRAssignmentBlockStartAssignment(assignment);
      
      var assignmentHR = HRAssignments.As(assignment);
      assignmentHR.CanNotedDirRX = _block.CanNotedDirRX.HasValue ? _block.CanNotedDirRX.Value : false;
      
      // TODO Sannikova_YS. Устаревшие св-ва VirtualEmployee и IsCompetitionWork.
      // Удалить после полного завершения уже стартованных задач до обновления на 25.2/2.7.
      assignmentHR.VirtualEmployeeDirRX = _block.VirtualEmployeeDirRX;
      assignmentHR.IsCompetitionWorkDirRX = _block.CompetitionWorkDirRX;
      
      assignmentHR.CanForwardDirRX = _block.CanForwardDirRX;
      assignmentHR.NeedActiveTextDirRX = _block.NeedActiveTextDirRX;
      assignmentHR.IsManualHoldingEventDirRX = _block.IsManualHoldingEventDirRX;
      assignmentHR.SupernikaSubjectDirRX = _block.SupernikaSubjectDirRX;
      assignmentHR.SupernikaTextDirRX = _block.SupernikaTextDirRX;
      
      // Добавить дейcтвия push сообщений в Супернику для задания.
      if (_block.SupernikaActionsDirRX.Any())
        foreach (var action in _block.SupernikaActionsDirRX)
          assignmentHR.SupernikaActionsDirRX.AddNew().Action = action;
    }

  }

  partial class HRNoticeBlockHandlers
  {

    public override void HRNoticeBlockEnd(System.Collections.Generic.IEnumerable<HRPro.HRDocFlow.IHRNotice> createdNotices)
    {
      base.HRNoticeBlockEnd(createdNotices);
      
      // Отправить уведомление на почту специалистам ДКП.
      var specialistsDKP = _block.DKPSpecialistDirRX.ToList();
      if (specialistsDKP.Any())
      {
        var subject = _block.Subject;
        var text = _block.DKPTextDirRX;
        var employeesDKP = Sungero.Company.PublicFunctions.Module.GetEmployeesFromRecipients(specialistsDKP).Where(x => !string.IsNullOrEmpty(x.Email)).Distinct();
        foreach (var employeeDKP in employeesDKP)
          OverrideSolution.Module.EssPlatform.Functions.Module.SendSupernikaPushOnEmail(employeeDKP.Email, subject, text);
      }
    }

    public override void HRNoticeBlockStartNotice(HRPro.HRDocFlow.IHRNotice notice)
    {
      base.HRNoticeBlockStartNotice(notice);
      
      var noticeHR = HRNotices.As(notice);
      noticeHR.SupernikaSubjectDirRX = _block.SupernikaSubjectDirRX;
      noticeHR.SupernikaTextDirRX = _block.SupernikaTextDirRX;
      noticeHR.NeedEmailNotificationDirRX = _block.NeedEmailNotificationDirRX;
      
      // Добавить дейcтвия push сообщений в Супернику для задания.
      if (_block.SupernikaActionsDirRX.Any())
        foreach (var action in _block.SupernikaActionsDirRX)
          noticeHR.SupernikaActionsDirRX.AddNew().Action = action;
    }

  }

  partial class BindEmployeesAndDocumentsHandlersHandlers
  {

  }

  partial class DocumentApprovalAssignmentBlockHandlers
  {

    public override void DocumentApprovalAssignmentBlockCompleteAssignment(HRPro.HRDocFlow.IDocumentApprovalAssignment assignment)
    {
      var completedAssignment = DocumentApprovalAssignments.As(assignment);
      
      if (assignment.Result == DocumentApprovalAssignment.Result.Forward)
      {
        _block.Performers.Remove(completedAssignment.Performer);
        Logger.DebugFormat("DocumentApprovalAssignmentBlockCompleteAssignment. TaskId: {0}, BlockUid: {1}. Переадресация задания. ИД нового исполнителя: {2}, ИД предыдущего исполнителя: {3}.",
                           _obj.Id, _block.Id, completedAssignment.AddresseeDirRX.Id, completedAssignment.CompletedBy.Id);
        assignment.Forward(completedAssignment.AddresseeDirRX, _block.AbsoluteDeadline);
        return;
      }
      
      if (completedAssignment.Result == DocumentApprovalAssignment.Result.Approved && completedAssignment.NeedLrdPaymentInfoDirRX == true)
      {
        var doc = completedAssignment.Task.Attachments.Where(x => DirRX.NNOtherProcesses.LrdPaymentStatementDocuments.Is(x)).FirstOrDefault();
        if (doc != null)
        {
          var statement = DirRX.NNOtherProcesses.LrdPaymentStatementDocuments.As(doc);
          if (statement.ApprovedAmount != completedAssignment.LrdPaymentAmountDirRX)
            statement.ApprovedAmount = completedAssignment.LrdPaymentAmountDirRX;
          if (statement.PayDate != completedAssignment.LrdPaymentDateDirRX)
            statement.PayDate = completedAssignment.LrdPaymentDateDirRX;
        }
        else
          Logger.ErrorFormat("DocumentApprovalAssignmentBlockCompleteAssignment. TaskId: {0}, BlockUid: {1}. CompletedAssignmentId: {2}. В задачу не вложено заявление на вплату ЛНА.",
                             _obj.Id, _block.Id, completedAssignment.Id);
      }
      
      base.DocumentApprovalAssignmentBlockCompleteAssignment(assignment);
    }

    public override void DocumentApprovalAssignmentBlockStart()
    {
      base.DocumentApprovalAssignmentBlockStart();
      
      if (_block.CompetitionWorkDirRX == true && _block.VirtualEmployeeDirRX == null)
      {
        var performer = Employees.As(_block.Performers.FirstOrDefault());
        _block.VirtualEmployeeDirRX = _block.CompetitionWorkDirRX == true && performer?.IsVirtualDirRX == true ? performer : Employees.Null;
      }
      
      if (_block.CompetitionWorkDirRX == true && _block.CanForwardDirRX == true)
      {
        // Подменить исполнителей блока на тех, кто выполнили задания по блоку в рамках предыдущей итерации блока с результатом не равным  Переадресовать.
        var performers = DirRX.OverrideSolution.PublicFunctions.DocumentApprovalAssignment.GetLastPerformers(_obj.Id, _block.Id, _obj.StartId);
        if (performers.Any())
        {
          Logger.DebugFormat("DocumentApprovalAssignmentBlockStart. TaskId: {0}, BlockUid: {1}. Изменение исполнителей блока. Всего последних исполнителей: {2}.",
                             _obj.Id, _block.Id, performers.Count());
          _block.Performers.Clear();
          foreach (var lastPerformer in performers)
            _block.Performers.Add(lastPerformer);
        }
      }
    }

    public override void DocumentApprovalAssignmentBlockStartAssignment(HRPro.HRDocFlow.IDocumentApprovalAssignment assignment)
    {
      base.DocumentApprovalAssignmentBlockStartAssignment(assignment);
      
      var assignmentHR = DocumentApprovalAssignments.As(assignment);
      
      // TODO Sannikova_YS. Устаревшие св-ва VirtualEmployee и IsCompetitionWork.
      // Удалить после полного завершения уже стартованных задач до обновления на 25.2/2.7.
      assignmentHR.VirtualEmployeeDirRX = _block.VirtualEmployeeDirRX;
      assignmentHR.IsCompetitionWorkDirRX = _block.CompetitionWorkDirRX == true;
      
      assignmentHR.CanForwardDirRX = _block.CanForwardDirRX == true;
      assignmentHR.AllowOpinionDirRX = _block.AllowOpinionDirRX == true;
      assignmentHR.AddApproverDirRX = _block.AddApproverDirRX == true;
      assignmentHR.IsParallelDirRX = _block.IsParallel == true;
      assignmentHR.SupernikaSubjectDirRX = _block.SupernikaSubjectDirRX;
      assignmentHR.SupernikaTextDirRX = _block.SupernikaTextDirRX;
      assignmentHR.NeedEmailNotificationDirRX = _block.NeedEmailNotificationDirRX;
      assignmentHR.NeedLrdPaymentInfoDirRX = _block.NeedLrdPaymentInfoDirRX == true;
      
      var summaryList = new List<string>();
      if (!string.IsNullOrWhiteSpace(_block.SummaryDirRX))
        summaryList.Add(_block.SummaryDirRX);
      
      var document = assignmentHR.DocumentGroup.OfficialDocuments.FirstOrDefault(d => VacationStatementDocuments.Is(d));
      var statement = VacationStatementDocuments.As(document);
      if (statement != null)
      {
        // TODO: SeleznyovDV. Перевести с AsupCheckResultDirRX на поле SapCommentText типа документа DocRelatedToWork. 
        if (_block.ShowSapCommentTextDirRX.GetValueOrDefault() && !string.IsNullOrEmpty(statement.AsupCheckResultDirRX))
          summaryList.Add(statement.AsupCheckResultDirRX);
        
        if (_block.ShowVacationAdvancesDirRX.GetValueOrDefault())
        {
          var vacationDaysExcess = statement.VacationDaysExcessDirRX ?? 0;
          if (vacationDaysExcess > 0)
            summaryList.Add(Resources.VacationDaysExcessWarningFormat(vacationDaysExcess));
        }
      }
      
      var summary = string.Join(Environment.NewLine + Environment.NewLine, summaryList);
      var summaryLength = assignmentHR.Info.Properties.SummaryDirRX.Length;
      assignmentHR.SummaryDirRX = summary.Length > summaryLength ? summary.Substring(0, summaryLength) : summary;
      
      // Добавить дейcтвия push сообщений в Супернику для задания.
      if (_block.SupernikaActionsDirRX.Any())
        foreach (var action in _block.SupernikaActionsDirRX)
          assignmentHR.SupernikaActionsDirRX.AddNew().Action = action;
    }

  }

  partial class DocumentReworkAssignmentHandlers
  {

    public override void DocumentReworkAssignmentStartAssignment(HRPro.HRDocFlow.IDocumentReworkAssignment assignment)
    {
      base.DocumentReworkAssignmentStartAssignment(assignment);
      
      var assignmentHR = DocumentReworkAssignments.As(assignment);
      assignmentHR.ShowInSelfServiceOfficeDirRX = _block.ShowInSelfServiceOfficeDirRX.HasValue ? _block.ShowInSelfServiceOfficeDirRX.Value : false;
      assignmentHR.SupernikaSubjectDirRX = _block.SupernikaSubjectDirRX;
      assignmentHR.SupernikaTextDirRX = _block.SupernikaTextDirRX;
    }

  }

  partial class PAcquaintanceAssignmentBlockHandlers
  {

  }

  partial class PSignAssignmentBlockHandlers
  {

  }

  partial class EAcquaintanceAssignmentBlockHandlers
  {

  }

  partial class RefusalAcqAssignmentBlockHandlers
  {

  }

  partial class PrepareRefuseActAssignmentBlockHandlers
  {

  }

  partial class WaitingDocumentHandlers
  {

  }

  partial class StartHRApprovalTaskBlockHandlers
  {

  }

  partial class StartHRSimpleTaskBlockHandlers
  {

  }

  partial class EAcquitanceAssignmentOverdueBlockHandlers
  {

  }

  partial class StartHRProcessTaskBlockHandlers
  {

  }

  partial class StartConsentTaskBlockHandlers
  {

  }

  partial class SetHRLifeCycleStateBlockHandlers
  {

    public override void SetHRLifeCycleStateBlockExecute()
    {
      // TODO Sannikova_YS. Св-во HRLifeCycleStateHRPro устарело и не должно использоваться.
      // В HR св-во продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
      // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
      // Используется в HRLifeCycleStateHRProChanged официального документа.
      
      #region Копипаст базового функциоанала блока SetHRLifeCycleStateBlock в HRDocFlow
      
      Enumeration lifeState = DirRX.OverrideSolution.OfficialDocument.HRLifeCycleStateHRPro.Draft;
      if (_block.HRLifeCycleState == HRDocFlow.Server.HRDocFlowBlocks.SetHRLifeCycleStateBlockHandlers.HRLifeCycleState.PendingSign)
        lifeState = OfficialDocument.HRLifeCycleStateHRPro.PendingSign;
      if (_block.HRLifeCycleState == HRDocFlow.Server.HRDocFlowBlocks.SetHRLifeCycleStateBlockHandlers.HRLifeCycleState.Active)
        lifeState = OfficialDocument.HRLifeCycleStateHRPro.Active;
      if (_block.HRLifeCycleState == HRDocFlow.Server.HRDocFlowBlocks.SetHRLifeCycleStateBlockHandlers.HRLifeCycleState.Obsolete)
        lifeState = OfficialDocument.HRLifeCycleStateHRPro.Obsolete;
      if (_block.HRLifeCycleState == HRDocFlow.Server.HRDocFlowBlocks.SetHRLifeCycleStateBlockHandlers.HRLifeCycleState.OnDestruction)
        lifeState = OfficialDocument.HRLifeCycleStateHRPro.OnDestruction;
      
      #endregion
      
      var approvalState = DirRX.OverrideSolution.OfficialDocument.InternalApprovalState.OnApproval;
      if (_block.ApprovalStateDirRX == ApprovalStateDirRX.OnRework)
        approvalState = DirRX.OverrideSolution.OfficialDocument.InternalApprovalState.OnRework;
      if (_block.ApprovalStateDirRX == ApprovalStateDirRX.PendingSign)
        approvalState = DirRX.OverrideSolution.OfficialDocument.InternalApprovalState.PendingSign;
      if (_block.ApprovalStateDirRX == ApprovalStateDirRX.Signed)
        approvalState = DirRX.OverrideSolution.OfficialDocument.InternalApprovalState.Signed;
      if (_block.ApprovalStateDirRX == ApprovalStateDirRX.Aborted)
        approvalState = DirRX.OverrideSolution.OfficialDocument.InternalApprovalState.Aborted;
      if (_block.ApprovalStateDirRX == ApprovalStateDirRX.Approved)
        approvalState = DirRX.OverrideSolution.OfficialDocument.InternalApprovalState.Approved;
      
      DirRX.NNCommons.PublicFunctions.Module.Remote.SetLifeAndApprovalState(_block.Documents.ToList(), approvalState, lifeState);
    }

  }
}