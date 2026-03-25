using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.NNVacations.RegistrationOfAnnualLeave;
using DirRX.OverrideSolution;
using DocKindFunctions = Sungero.Docflow.PublicFunctions.DocumentKind;

namespace DirRX.NNVacations.Server.RegistrationOfAnnualLeaveBlocks
{
  partial class FindShiftVacationBlockHandlers
  {

    public virtual void FindShiftVacationBlockExecute()
    {
      Logger.DebugFormat("FindShiftVacationBlock. ИД задачи: {0}. Старт выполнения блока.", _obj.Id);
      
      var statementDocument = DirRX.OverrideSolution.PublicFunctions.VacationStatementDocument.GetDocumentByRegistrationTask(_obj);
      if (statementDocument == null)
      {
        Logger.DebugFormat("FindShiftVacationBlock. ИД задачи: {0}. Документ \"Заявление на перенос отпуска\" не найден по реквизитам задачи.", _obj.Id);
        return;
      }
      
      var task = StatementTasks.GetAll()
        .Where(x => x.Status == OverrideSolution.StatementTask.Status.InProcess &&
               x.AttachmentDetails.Any(d => d.AttachmentId == statementDocument.Id))
        .FirstOrDefault();
      
      if (task == null)
      {
        Logger.DebugFormat("FindShiftVacationBlock. ИД задачи: {0}. Задача \"Задача на рассмотрение заявлений сотрудников\" не найдена по вложеному документу с ИД: {2}.", _obj.Id, statementDocument.Id);
        return;
      }
      
      _obj.TransferTaskId = task.Id;
      Logger.DebugFormat("FindShiftVacationBlock. ИД задачи: {0}. ИД связанной задачи на рассмотрение заявлений сотрудников: {1}. Конец выполнения блока.", _obj.Id, task.Id);
    }
  }

  partial class WaitCompletionShiftTaskMonitoringBlockHandlers
  {

    public virtual bool WaitCompletionShiftTaskMonitoringBlockResult()
    {
      var shiftTask = Functions.RegistrationOfAnnualLeave.GetShiftVacationTask(_obj);
      return shiftTask?.Status == Sungero.Workflow.Task.Status.Completed;
    }
  }

  partial class VacationConfirmationBlockHandlers
  {
    public virtual void VacationConfirmationBlockCompleteAssignment(DirRX.NNVacations.IVacationConfirmationAssignment assignment)
    {
      _obj.ShiftTaskId = assignment.ShiftTaskId;
      _obj.Substitute = assignment.Substitute;
      
      _obj.NotifiableList.Clear();
      foreach (var notifiable in assignment.NotifiableList.Select(p => p.Notifiable))
        _obj.NotifiableList.AddNew().Notifiable = notifiable;
      
      if (assignment.Result == HRPro.Vacations.VacationConfirmationAssignment.Result.Complete)
      {
        // Создать операцию подписания.
        var mainDocs = _obj.DocumentGroup.VacationNoticeDocuments;
        var addendaDocs = Enumerable.Empty<Sungero.Docflow.IOfficialDocument>();
        var completedBy = DirRX.EssPlatformSolution.Employees.As(assignment.CompletedBy);
        var performer = DirRX.EssPlatformSolution.Employees.As(assignment.Performer);
        HRPro.HRProSolution.Module.HRWred.PublicFunctions.Module.CreateSigningOperation(mainDocs, addendaDocs, completedBy, performer, false);
      }
    }

    public virtual void VacationConfirmationBlockStartAssignment(DirRX.NNVacations.IVacationConfirmationAssignment assignment)
    {
      foreach (var person in _obj.NotifiableList)
        assignment.NotifiableList.AddNew().Notifiable = person.Notifiable;

      assignment.ShowInSelfServiceOffice = _block.ShowInSelfServiceOffice.HasValue ? _block.ShowInSelfServiceOffice.Value : false;
      if (_obj.Employee != _block.Substitute)
        assignment.Substitute = _block.Substitute;
      
      assignment.SupernikaSubject = _block.SupernikaSubject;
      assignment.SupernikaText = _block.SupernikaText;
      
      // Отправить уведомление о новом задании.
      DirRX.EssPlatform.PublicFunctions.Module.SendNewNotification(Sungero.Company.Employees.As(assignment.Performer), false,
                                                                   HRPro.HRProSolution.PublicConstants.Module.TargetNameSpace,
                                                                   DirRX.EssPlatform.PublicConstants.Module.SelfOfficeObjectTypes.WorkItems,
                                                                   Constants.Module.ObjectCardTypeNames.VacationConfirmationAssignment,
                                                                   assignment.Id.ToString(),
                                                                   assignment.ShowInSelfServiceOffice ?? false);
    }

  }

  partial class CreateNoticeBlockHandlers
  {

    public virtual void CreateNoticeBlockExecute()
    {
      Logger.Debug("CreateNoticeBlockExecute. Начало выполнения блока.");
      var notice = DirRX.OverrideSolution.VacationNoticeDocuments.Null;
      var docKind = DocKindFunctions.GetNativeDocumentKind(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationBeginningNoticeKind);
      var template = Functions.Module.GetTemplateByKind(docKind, BusinessUnits.As(_obj.Employee?.BusinessUnit));
      if (_block.CreateBody.GetValueOrDefault() && template != null)
        notice = DirRX.OverrideSolution.VacationNoticeDocuments.CreateFrom(template);
      else
        notice = DirRX.OverrideSolution.VacationNoticeDocuments.Create();
      notice.Employee = _obj.Employee;
      notice.StartDateDirRX = _obj.StartDate;
      notice.EndDateDirRX = _obj.EndDate;
      notice.Year = _obj.StartDate.Value.BeginningOfYear();
      foreach (var vacInfo in _obj.VacationData)
      {
        var vacationData = notice.VacationDataDirRX.AddNew();
        vacationData.VacationType = vacInfo.VacationType;
        vacationData.PlannedAbsence = vacInfo.PlannedAbsence;
        vacationData.StartDate = vacInfo.StartDate;
        vacationData.EndDate = vacInfo.EndDate;
        vacationData.Days = vacInfo.Days;
      }
      notice.Save();
      OverrideSolution.PublicFunctions.VacationNoticeDocument.FillVacationDataInTemplate(notice, "CreateNoticeBlockExecute");
      _obj.Notice = notice;
      Logger.Debug("CreateNoticeBlockExecute. Конец выполнения блока.");
    }
  }

}