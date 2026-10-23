using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkAgreementDocument;

namespace DirRX.NNOtherProcesses.Server
{
  partial class WeekendWorkAgreementDocumentFunctions
  {

    /// <summary>
    /// Создать согласие на привлечение к работе в ВД/ПД/СР.
    /// </summary>
    /// <param name="employee">Сотрудник, который будет подписывать согласие.</param>
    /// <param name="statement">Заявка на привлечение к работе ВД/ПД/СР.</param>
    /// <returns>Согласие.</returns>
    public static IWeekendWorkAgreementDocument CreateWeekendWorkAgreement(OverrideSolution.IEmployee employee, IWeekendWorkStatementDocument statement)
    {
      var agreement = WeekendWorkAgreementDocuments.Create();
      var agreementEmployeeInfo = statement.Employees.Where(x => OverrideSolution.Employees.Equals(x.Employee, employee)).FirstOrDefault();
      var documentKindId = Functions.WeekendWorkStatementDocument.IsOvertimeWork(statement.DocumentKind) ?
        Sungero.Docflow.PublicFunctions.Module.GetDocumentKindIdByGuid(WeekendWorkAgreementDocument.ClassTypeGuid,
                                                                       Constants.Module.DocumentKind.OvertimeWorkAgreementDocument) :
        Sungero.Docflow.PublicFunctions.Module.GetDocumentKindIdByGuid(WeekendWorkAgreementDocument.ClassTypeGuid,
                                                                       Constants.Module.DocumentKind.WeekendWorkAgreementDocument);
      // HACK: Обход коробочного ограничения для корректного отображения времени в документах.
      // При передаче в SAP необходимо привести время к формату часового пояса инициатора на момент заполнения им карточки.
      var initiatorTimeZoneDiff = DirRX.NNOtherProcesses.PublicFunctions.WeekendWorkStatementDocument.Remote.GetInitiatorTimeZoneDiff(statement);
      
      agreement.DocumentKind = OverrideSolution.DocumentKinds.Get(documentKindId);
      agreement.EmployeeDirRX = employee;
      agreement.OurSignatory = employee;
      agreement.RegistrationDate = Calendar.Now;
      agreement.PreparedBy = statement.PreparedBy;
      agreement.BaseDocument = statement;
      agreement.Relations.AddFrom(Sungero.Docflow.PublicConstants.Module.AddendumRelationName, statement);
      agreement.WorkDate = agreementEmployeeInfo.WorkDateFrom.HasValue? agreementEmployeeInfo.WorkDateFrom.Value.ToString("dd.MM.yyyy") : string.Empty;
      agreement.WorkDateFrom = agreementEmployeeInfo.WorkDateFrom.HasValue? agreementEmployeeInfo.WorkDateFrom.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm") : string.Empty;
      agreement.WorkDateTo = agreementEmployeeInfo.WorkDateTo.HasValue? agreementEmployeeInfo.WorkDateTo.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm") : string.Empty;
      agreement.FirstBreakFrom = agreementEmployeeInfo.FirstBreakFrom.HasValue? agreementEmployeeInfo.FirstBreakFrom.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm"): string.Empty;
      agreement.FirstBreakTo =  agreementEmployeeInfo.FirstBreakTo.HasValue? agreementEmployeeInfo.FirstBreakTo.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm") : string.Empty;
      agreement.SecondBreakFrom = agreementEmployeeInfo.SecondBreakFrom.HasValue? agreementEmployeeInfo.SecondBreakFrom.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm") : string.Empty;
      agreement.SecondBreakTo = agreementEmployeeInfo.SecondBreakTo.HasValue? agreementEmployeeInfo.SecondBreakTo.Value.AddHours(initiatorTimeZoneDiff).ToString("HH:mm") : string.Empty;
      agreement.Compensation = agreementEmployeeInfo.Compensation;
      agreement.RestDateFrom = agreementEmployeeInfo.RestDateFrom.HasValue? agreementEmployeeInfo.RestDateFrom.Value.ToString("dd.MM.yyyy") : string.Empty;
      
      // Получаем текст условия оплачиваемого перерыва из константы "Оплачиваемый перерыв".
      var paidBreakConstantValue = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.PaidBreak.Guid);
      if (string.IsNullOrEmpty(paidBreakConstantValue))
        paidBreakConstantValue = Constants.Module.ConstantsDefaultValues.PaidBreak;
      
      agreement.PaidBreakConditions = agreementEmployeeInfo.PaidBreak == true ? paidBreakConstantValue : string.Empty;
      
      var template = NNCommons.PublicFunctions.Module.Remote.GetTemplateByKind(agreement.DocumentKind, agreement.BusinessUnit);
      if (template == null)
      {
        var error = string.Format("Не найден шаблон для документа. ИД вида документа: {0}. ИД НОР: {1}.", agreement.DocumentKind, agreement.BusinessUnit);
        Logger.ErrorFormat("CreateWeekendWorkAgreement. ИД заявления: {0}. Таб. номер сотрудника: {1}. {2}", statement.Id, employee.PersonnelNumber, agreement.DocumentKind, agreement.BusinessUnit, error);
        throw AppliedCodeException.Create(error);
      }
      try
      {
        using (var body = template.LastVersion.Body.Read())
        {
          var newVersion = agreement.CreateVersionFrom(body, template.AssociatedApplication.Extension);
          var internalEntity = (Sungero.Domain.Shared.IExtendedEntity)agreement;
          internalEntity.Params[Sungero.Content.Shared.ElectronicDocumentUtils.FromTemplateIdKey] = template.Id;
          agreement.Save();
        }
        Logger.DebugFormat("CreateWeekendWorkAgreement. ИД заявления: {0}. Таб. номер сотрудника: {1}. Согласие на привлечение к работе ВД/ПД/СР успешно создано. ИД согласия: {2}.", statement.Id, employee.PersonnelNumber, agreement.Id);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CreateWeekendWorkAgreement. ИД заявления: {0}. Таб. номер сотрудника: {1}. Ошибка при создании согалсия на привлечение к работе ВД/ПД/СР. Текст ошибки: {2}, StackTrace: {3}.", statement.Id, employee.PersonnelNumber, ex.Message, ex.StackTrace);
      }
      
      return agreement;
    }

  }
}