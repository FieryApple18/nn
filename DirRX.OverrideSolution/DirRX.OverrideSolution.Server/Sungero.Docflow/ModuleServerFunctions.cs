using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Docflow;
using Sungero.Domain.LinqExpressions;

namespace DirRX.OverrideSolution.Module.Docflow.Server
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Получить задания для указанных сотрудников, которые были созданы и должны быть выполнены в указанный период (без HR уведомлений).
    /// </summary>
    /// <param name="employeeIds">Список ИД сотрудников.</param>
    /// <param name="periodFirstDay">Минимально возможная дата создания задания.</param>
    /// <param name="periodLastDay">Максимально возможный срок задания.</param>
    /// <returns>Задания.</returns>
    public override IQueryable<Sungero.Workflow.IAssignment> GetEmployeeAssignmentsInPeriod(List<long> employeeIds, DateTime periodFirstDay, DateTime periodLastDay)
    {
      return Sungero.Workflow.Assignments
        .GetAll(a => a.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                (a.Deadline.HasValue && a.Deadline <= periodLastDay || a.IsRead == false && a.Created >= periodFirstDay) &&
                a.Performer != null &&
                employeeIds.Contains(a.Performer.Id) &&
                !(HRPro.HRDocFlow.EAcquaintanceAssignments.Is(a) && HRPro.HRDocFlow.EAcquaintanceAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.HRDocFlow.DocumentApprovalAssignments.Is(a) && HRPro.HRDocFlow.DocumentApprovalAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.ESignAssignments.Is(a) && DirRX.OverrideSolution.ESignAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.DocumentReworkAssignments.Is(a) && DirRX.OverrideSolution.DocumentReworkAssignments.As(a).ShowInSelfServiceOfficeDirRX == true) &&
                !(DirRX.OverrideSolution.HRAssignments.Is(a) && DirRX.OverrideSolution.HRAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.NNStatementReworkAssignments.Is(a) && DirRX.OverrideSolution.NNStatementReworkAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.DocumentApprovalAssignments.Is(a) && DirRX.OverrideSolution.DocumentApprovalAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(NNVacations.VacationConfirmationAssignments.Is(a) && NNVacations.VacationConfirmationAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(NNVacations.SignAcquaintanceListAssignments.Is(a) && NNVacations.SignAcquaintanceListAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(NNCommons.StatementReworkAssignments.Is(a) && NNCommons.StatementReworkAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.HRDocFlow.ConsentAssignments.Is(a) && HRPro.HRDocFlow.ConsentAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.Is(a) && DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.HRDocFlow.AgreementAssignments.Is(a) &&  HRPro.HRDocFlow.AgreementAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.HRDocFlow.PAcquaintanceAssignments.Is(a) &&
                !HRPro.HRDocFlow.PrepareRefuseActAssignments.Is(a) &&
                !HRPro.HRDocFlow.PSignAssignments.Is(a) &&
                !HRPro.HRDocFlow.RefusalAcqAssignments.Is(a) &&
                !HRPro.HRCommons.ManualConvertToPDFAAssignments.Is(a) &&
                !DirRX.SignPlatform.CertificateIssueErrorAssignments.Is(a) &&
                !HRPro.HRWred.ExportResultsAssignments.Is(a));
    }

    /// <summary>
    /// Получить уведомления для указанных сотрудников, созданных после указанной даты (без HR уведомлений).
    /// </summary>
    /// <param name="employeeIds">Список ИД сотрудников.</param>
    /// <param name="periodFirstDay">Минимально возможная дата создания уведомления.</param>
    /// <returns>Уведомления.</returns>
    public override IQueryable<Sungero.Workflow.INotice> GetEmployeeNoticesInPeriod(List<long> employeeIds, DateTime periodFirstDay)
    {
      return Sungero.Workflow.Notices
        .GetAll(a => a.IsRead == false && a.Created >= periodFirstDay &&
                a.Performer != null &&
                employeeIds.Contains(a.Performer.Id) &&
                !(HRPro.HRDocFlow.HRNotices.Is(a) && HRPro.HRDocFlow.HRNotices.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.SignPlatform.CertificateIssueNotices.Is(a) && DirRX.SignPlatform.CertificateIssueNotices.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.HRWred.ExportResultsAssignments.Is(a));
    }

    /// <summary>
    /// Отправить письма со сводками по заданиям.
    /// </summary>
    /// <param name="messages">Список писем со сводками.</param>
    /// <remarks>Коробка RX + доп. логирование.</remarks>
    public override void SendSummaryMailNotificationMessages(List<Sungero.Core.IEmailMessage> messages)
    {
      Logger.DebugFormat("SendSummaryMailNotificationMessages. Начало отправки писем. Количество сообщений для отправки: {0}.", messages.Count());
      try
      {
        Mail.Send(messages);
        Logger.Debug("SendSummaryMailNotificationMessages. Сообщения успешно отправлены.");
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("SendSummaryMailNotificationMessages. Не удалось отправить сообщения электронной почты. Будет предпринята попытка отправить сообщения по одному. Ошибка: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
        
        this.SummaryMailLogError("Не удалось отправить сообщения электронной почты. Будет предпринята попытка отправить сообщения по одному.", ex);
        foreach (var mail in messages)
        {
          try
          {
            // Переполучить сообщение для корректной отправки.
            Mail.Send(this.GetSummaryMailNotificationMessage(mail.Subject, mail.To.ToList(), mail.CC.ToList(), mail.Body));
            Logger.DebugFormat("SendSummaryMailNotificationMessages. Сообщение успешно отправлено. To: {0}. CC: {1}.",
                               string.Join(", ", mail.To.Select(e => MaskEmail(e)).ToList()),
                               string.Join(", ", mail.CC.Select(e => MaskEmail(e)).ToList()));
          }
          catch (Exception exception)
          {
            var mailTo = string.Join(", ", mail.To.Select(e => MaskEmail(e)).ToList());
            this.SummaryMailLogError(string.Format("Не удалось отправить электронное сообщение на адрес {0}.", mailTo), exception);
          }
        }
      }
      
      Logger.Debug("SendSummaryMailNotificationMessages. Отправка писем закончена.");
    }

    /// <summary>
    /// Скрытие адреса электронной почты, для логирования.
    /// </summary>
    /// <param name="email">Исходный адрес электронной почты.</param>
    /// <returns>Скрытый адрес электронной почты.</returns>
    [Public]
    static string MaskEmail(string email)
    {
      var atIndex = email.IndexOf('@');

      if (atIndex == -1)
        return email;

      var localPart = email.Substring(0, atIndex);
      var domainPart = email.Substring(atIndex);
      var maskedLocalPart = localPart.Length > 3
        ? localPart.Substring(0, 3) + new string('*', localPart.Length - 3)
        : "***";

      var maskedDomainPart = domainPart.Length > 3
        ? new string('*', domainPart.Length - 4) + domainPart.Substring(domainPart.Length - 3, 3)
        : "*.ru";
      
      return maskedLocalPart + "@" + maskedDomainPart;
    }

    /// <summary>
    /// Получить новые задания и уведомления для рассылки, созданные в указанном периоде, без учета HR-заданий.
    /// </summary>
    /// <param name="previousRun">Предыдущий запуск.</param>
    /// <param name="notificationDate">Текущий запуск.</param>
    /// <returns>Новые задания и уведомления, по которым будет выполнена рассылка.</returns>
    public override IQueryable<Sungero.Workflow.IAssignmentBase> GetNewAssignmentsAndNoticesInPeriod(DateTime previousRun, DateTime notificationDate)
    {
      return Sungero.Workflow.AssignmentBases
        .GetAll(a => (previousRun <= a.Created && a.Created < notificationDate ||
                      Sungero.Workflow.Assignments.Is(a) && Sungero.Workflow.Assignments.As(a).IsCompetitive == true &&
                      previousRun <= Sungero.Workflow.Assignments.As(a).Returned &&
                      Sungero.Workflow.Assignments.As(a).Returned < notificationDate) &&
                a.IsRead == false && a.Status != Sungero.Workflow.AssignmentBase.Status.Aborted &&
                !(HRPro.HRDocFlow.EAcquaintanceAssignments.Is(a) && HRPro.HRDocFlow.EAcquaintanceAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.HRDocFlow.DocumentApprovalAssignments.Is(a) && HRPro.HRDocFlow.DocumentApprovalAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.ESignAssignments.Is(a) && DirRX.OverrideSolution.ESignAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.DocumentReworkAssignments.Is(a) && DirRX.OverrideSolution.DocumentReworkAssignments.As(a).ShowInSelfServiceOfficeDirRX == true) &&
                !(DirRX.OverrideSolution.HRAssignments.Is(a) && DirRX.OverrideSolution.HRAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.NNStatementReworkAssignments.Is(a) && DirRX.OverrideSolution.NNStatementReworkAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.DocumentApprovalAssignments.Is(a) && DirRX.OverrideSolution.DocumentApprovalAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(NNVacations.VacationConfirmationAssignments.Is(a) && NNVacations.VacationConfirmationAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(NNVacations.SignAcquaintanceListAssignments.Is(a) && NNVacations.SignAcquaintanceListAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(NNCommons.StatementReworkAssignments.Is(a) && NNCommons.StatementReworkAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.HRDocFlow.ConsentAssignments.Is(a) && HRPro.HRDocFlow.ConsentAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.Is(a) && DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.HRDocFlow.PAcquaintanceAssignments.Is(a) &&
                !HRPro.HRDocFlow.PrepareRefuseActAssignments.Is(a) &&
                !HRPro.HRDocFlow.PSignAssignments.Is(a) &&
                !HRPro.HRDocFlow.RefusalAcqAssignments.Is(a) &&
                !(HRPro.HRDocFlow.AgreementAssignments.Is(a) && HRPro.HRDocFlow.AgreementAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.HRCommons.ManualConvertToPDFAAssignments.Is(a) &&
                !DirRX.SignPlatform.CertificateIssueErrorAssignments.Is(a) &&
                !(HRPro.HRDocFlow.HRNotices.Is(a) && HRPro.HRDocFlow.HRNotices.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.SignPlatform.CertificateIssueNotices.Is(a) && DirRX.SignPlatform.CertificateIssueNotices.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.HRWred.ExportResultsAssignments.Is(a) &&
                !(HRPro.Vacations.VacationAlignDateAssignments.Is(a) && HRPro.Vacations.VacationAlignDateAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.Vacations.VacationConfirmationAssignments.Is(a) && HRPro.Vacations.VacationConfirmationAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.Vacations.VacationApprovalAssignments.Is(a) && HRPro.Vacations.VacationApprovalAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.Vacations.VacationReworkAssignments.Is(a) && HRPro.Vacations.VacationReworkAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.Vacations.VacationChangeApprovalAssignments.Is(a) && HRPro.Vacations.VacationChangeApprovalAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.Vacations.VacationChangeReworkAssignments.Is(a) && HRPro.Vacations.VacationChangeReworkAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.Vacations.VacationChangeSignAssignments.Is(a) && HRPro.Vacations.VacationChangeSignAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.Vacations.VacationEmployeeSchedulingAssignments.Is(a) && HRPro.Vacations.VacationEmployeeSchedulingAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.Vacations.VacationManagerCheckAssignments.Is(a) && HRPro.Vacations.VacationManagerCheckAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.Vacations.CheckAndCorrectVacationAssignments.Is(a) &&
                !HRPro.Vacations.PrepareDocsAssignments.Is(a) &&
                !(HRPro.Hiring.SigningDocumentsForEmploymentAssignments.Is(a) && HRPro.Hiring.SigningDocumentsForEmploymentAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.Hiring.CandidateDocumentsUploadingAssignments.Is(a) && HRPro.Hiring.CandidateDocumentsUploadingAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.Hiring.ReworkDocumentsUploadAssignments.Is(a) &&
                !HRPro.BusinessTrips.BusinessTripReWorkAssignments.Is(a) &&
                !HRPro.BusinessTrips.BusinessTripPrepareReportAssignments.Is(a) &&
                !(HRPro.ExpenseReports.ExpenseReportPrepareAssignments.Is(a) && HRPro.ExpenseReports.ExpenseReportPrepareAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.ExpenseReports.ExpenseReportEmployeeSignAssignments.Is(a) &&
                !(HRPro.ExpenseReports.ExpenseReportSimpleAssignments.Is(a) && HRPro.ExpenseReports.ExpenseReportSimpleAssignments.As(a).ShowInSelfServiceOffice == true));
    }

    /// <summary>
    /// Получить задания для рассылки, срок которых истек в указанный период, без учета HR-заданий.
    /// </summary>
    /// <param name="previousRun">Предыдущий запуск.</param>
    /// <param name="notificationDate">Текущий запуск.</param>
    /// <returns>Просроченные задания, по которым будет выполнена рассылка.</returns>
    public override IQueryable<Sungero.Workflow.IAssignment> GetExpiredAssignmentsInPeriod(DateTime previousRun, DateTime notificationDate)
    {
      return Sungero.Workflow.Assignments
        .GetAll(a => a.Status == Sungero.Workflow.AssignmentBase.Status.InProcess  &&
                (a.Deadline.HasValue && a.Deadline.Value.HasTime() &&
                 previousRun <= a.Deadline && a.Deadline < notificationDate ||
                 a.Deadline.HasValue && !a.Deadline.Value.HasTime() &&
                 previousRun <= a.Deadline.Value.AddDays(1) && a.Deadline.Value.AddDays(1) < notificationDate) &&
                !(HRPro.HRDocFlow.EAcquaintanceAssignments.Is(a) && HRPro.HRDocFlow.EAcquaintanceAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.HRDocFlow.DocumentApprovalAssignments.Is(a) && HRPro.HRDocFlow.DocumentApprovalAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.ESignAssignments.Is(a) && DirRX.OverrideSolution.ESignAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.DocumentReworkAssignments.Is(a) && DirRX.OverrideSolution.DocumentReworkAssignments.As(a).ShowInSelfServiceOfficeDirRX == true) &&
                !(DirRX.OverrideSolution.HRAssignments.Is(a) && DirRX.OverrideSolution.HRAssignments.As(a).ShowInSelfServiceOffice != true) &&
                !(DirRX.OverrideSolution.NNStatementReworkAssignments.Is(a) && DirRX.OverrideSolution.NNStatementReworkAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.OverrideSolution.DocumentApprovalAssignments.Is(a) && DirRX.OverrideSolution.DocumentApprovalAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(NNVacations.VacationConfirmationAssignments.Is(a) && NNVacations.VacationConfirmationAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(NNVacations.SignAcquaintanceListAssignments.Is(a) && NNVacations.SignAcquaintanceListAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(NNCommons.StatementReworkAssignments.Is(a) && NNCommons.StatementReworkAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(HRPro.HRDocFlow.ConsentAssignments.Is(a) && HRPro.HRDocFlow.ConsentAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !(DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.Is(a) && DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.HRDocFlow.PAcquaintanceAssignments.Is(a) &&
                !HRPro.HRDocFlow.PrepareRefuseActAssignments.Is(a) &&
                !HRPro.HRDocFlow.PSignAssignments.Is(a) &&
                !HRPro.HRDocFlow.RefusalAcqAssignments.Is(a) &&
                !(HRPro.HRDocFlow.AgreementAssignments.Is(a) &&  HRPro.HRDocFlow.AgreementAssignments.As(a).ShowInSelfServiceOffice == true) &&
                !HRPro.HRCommons.ManualConvertToPDFAAssignments.Is(a) &&
                !DirRX.SignPlatform.CertificateIssueErrorAssignments.Is(a) &&
                !HRPro.HRWred.ExportResultsAssignments.Is(a));
    }

  }
}