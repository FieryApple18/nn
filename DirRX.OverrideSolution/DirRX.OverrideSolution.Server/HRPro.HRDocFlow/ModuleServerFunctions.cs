using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.LinqExpressions;

namespace DirRX.OverrideSolution.Module.HRDocFlow.Server
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Разворачивает виртуальных сотрудников в списке участников.
    /// Для каждого участника: если он виртуальный - заменяется на список его исполнителей.
    /// Иначе - остается без изменений.
    /// </summary>
    /// <param name="recipients">Список участников, в котором могут содержатся виртуальные сотрудники.</param>
    /// <returns>Список реальных участников, где виртуальные сотрудники развернуты до их исполнителей.</returns>
    [ExpressionElement("ExpFunc_ExpandVirtualEmployeesInList_Name", "ExpFunc_ExpandVirtualEmployeesInList_Desc")]
    public static IEnumerable<Sungero.CoreEntities.IRecipient> ExpandVirtualEmployeesInList(IEnumerable<Sungero.CoreEntities.IRecipient> recipients)
    {
      var result = new List<Sungero.CoreEntities.IRecipient>();
      
      if (recipients == null)
        return result;
      
      foreach (var recipient in recipients)
      {
        if (DirRX.OverrideSolution.Employees.Is(recipient))
        {
          var employee = DirRX.OverrideSolution.Employees.As(recipient);
          
          if (employee.IsVirtualDirRX == true)
            result.AddRange(employee.PerformersDirRX.Select(e => Sungero.CoreEntities.Recipients.As(e.Performer)).ToList());
        }
        else
          result.Add(recipient);
      }
      
      return result.Distinct();
    }

    /// <summary>
    /// Отправить уведомление СОК об автоматическом создании замещений по сотрудникам персоны.
    /// </summary>
    /// <param name="substitute">Замещающий.</param>
    /// <param name="substitution">Замещаемый.</param>
    [Public]
    public virtual void StartAutoSubstitutionNotice(HRPro.HRProSolution.IEmployee substitute, List<HRPro.HRProSolution.IEmployee> substitutions)
    {
      base.StartAutoSubstitutionNotice(substitute, substitutions);
    }

    /// <summary>
    /// Получить ИД просроченных заданий, по которым надо сделать рассылку совместителям.
    /// </summary>
    /// <param name="previousRun">Предыдущий запуск.</param>
    /// <param name="notificationDate">Текущий запуск.</param>
    /// <returns>ИД заданий, по которым будет выполнена рассылка.</returns>
    public override List<long> GetExpiredAssignmentIds(DateTime previousRun, DateTime notificationDate)
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
                !(DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.Is(a) && DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.As(a).ShowInSelfServiceOffice == true))
        .Select(a => a.Id)
        .ToList();
    }

    /// <summary>
    /// Получить ИД новых заданий, по которым надо сделать рассылку.
    /// </summary>
    /// <param name="previousRun">Предыдущий запуск.</param>
    /// <param name="notificationDate">Текущий запуск.</param>
    /// <returns>ИД заданий, по которым будет выполнена рассылка.</returns>
    public override List<long> GetNewAssignmentIds(DateTime previousRun, DateTime notificationDate)
    {
      return Sungero.Workflow.Assignments
        .GetAll(a => ((previousRun <= a.Created && a.Created < notificationDate) || (a.IsCompetitive == true && previousRun <= a.Returned && a.Returned < notificationDate)) &&
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
                !(DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.Is(a) && DirRX.SignPlatform.CertificateIssueStatementAcceptAssignments.As(a).ShowInSelfServiceOffice == true))
        .Select(a => a.Id)
        .ToList();
    }

    // HACK: Метод для использования непубличной коробочной функции.
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <param name="text">Текст инструкции.</param>
    /// <returns>Модель состояния.</returns>
    [Public]
    public StateView GetAssignmentStateView(string text)
    {
      return base.GetAssignmentStateView(text);
    }

    /// <summary>
    /// Получить гуиды типов HR-документов модуля HRDocFlow.
    /// </summary>
    /// <returns>Список гуидов типов HR документов.</returns>
    [Public, Remote(IsPure = true)]
    public override List<Guid> GetHRDocumentTypesGuids()
    {
      var guids = base.GetHRDocumentTypesGuids();
      guids.AddRange(new[]
                     {
                       DirRX.NNOtherProcesses.PublicConstants.Module.DocumentType.NNRetentionStatementDocument,
                       DirRX.NNOtherProcesses.PublicConstants.Module.DocumentType.NNBusinessTripOrderDocument,
                       DirRX.NNOtherProcesses.PublicConstants.Module.DocumentType.LrdPaymentOrderDocument,
                       DirRX.NNOtherProcesses.PublicConstants.Module.DocumentType.LrdPaymentStatementDocument,
                       DirRX.NNOtherProcesses.PublicConstants.Module.DocumentType.WeekendWorkAgreementDocument,
                       DirRX.NNOtherProcesses.PublicConstants.Module.DocumentType.WeekendWorkOrderDocument,
                       DirRX.NNOtherProcesses.PublicConstants.Module.DocumentType.WeekendWorkStatementDocument,
                       HRPro.HRDocFlow.PublicConstants.Module.EntityTypeGuid.DocRelatedToWork
                     });

      return guids;
    }

    #region Конкурентная работа.

    /// <summary>
    /// Установить результат выполнения для действия конкурентной работы.
    /// </summary>
    /// <param name="e">Параметры события "До выполнения".</param>
    public void SetCompetitionWorkActionResult(Sungero.Workflow.Server.BeforeCompleteEventArgs e)
    {
      var isTakeToWork = false;
      
      if (e.Params.TryGetValue(PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName, out isTakeToWork))
      {
        e.Result = isTakeToWork ? Resources.TakeToWorkMessage : Resources.ReturnToQueueMessage;
        e.Params.Remove(PublicConstants.Module.ParamKeys.CompetitionWorkActionsParamName);
      }
    }

    #endregion

    /// <summary>
    /// Получить значение макропеременной для заявления на выплату по ЛНА.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <param name="fieldName">Макропеременная.</param>
    /// <returns>Значение макропеременной.</returns>
    [Public(WebApiRequestType = RequestType.Get)]
    public string GetLrdPaymentStatementDocumentFieldValue(long employeeId, string fieldName)
    {
      var employee = HRPro.HRProSolution.Employees.Get(employeeId);
      var businessUnit = employee.Department.BusinessUnit;
      var buisenessUnitConstant = DirRX.NNCommons.PublicFunctions.Module.Remote.GetBusinessUnitValue(DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.LrdPaymentStatementBusinessUnit.Guid);
      var CEOJobTitleConstant = DirRX.NNCommons.PublicFunctions.Module.Remote.GetJobTitleValue(DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.LrdPaymentStatementCEOJobTitle.Guid);
      var CEOConstant = DirRX.NNCommons.PublicFunctions.Module.Remote.GetEmployeeValue(DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.LrdPaymentStatementCEO.Guid);

      switch (fieldName)
      {
        case "CEO_JOB_TITLE_DATIVE":
          if (businessUnit != null && employee.BusinessUnit != null)
          {
            if (employee.BusinessUnit.Equals(buisenessUnitConstant) && CEOJobTitleConstant != null)
              return CaseConverter.ConvertJobTitleToTargetDeclension(CEOJobTitleConstant.Name, DeclensionCase.Dative);
            return Sungero.Company.PublicFunctions.Employee.GetJobTitle(businessUnit.CEO, DeclensionCase.Dative);
          }
          return string.Empty;
        case "CEO_FULLNAME_DATIVE":
          if (businessUnit != null && employee.BusinessUnit != null)
          {
            if (employee.BusinessUnit.Equals(buisenessUnitConstant) && CEOConstant != null)
              return this.GetFullNameInDeclension(CEOConstant, DeclensionCase.Dative);
            return this.GetFullNameInDeclension(businessUnit.CEO, DeclensionCase.Dative);
          }
          return string.Empty;
        default:
          return string.Empty;
      }
    }

  }
}