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
      agreement.DocumentKind = OverrideSolution.DocumentKinds.Get(documentKindId);
      agreement.EmployeeDirRX = employee;
      agreement.OurSignatory = employee;
      agreement.RegistrationDate = Calendar.Now;
      agreement.PreparedBy = statement.PreparedBy;
      agreement.BaseDocument = statement;
      agreement.Relations.AddFrom(Sungero.Docflow.PublicConstants.Module.AddendumRelationName, statement);
      agreement.WorkDate = agreementEmployeeInfo.WorkDateFrom.HasValue? agreementEmployeeInfo.WorkDateFrom.Value.ToString("dd.MM.yyyy") : string.Empty;
      agreement.WorkDateFrom = agreementEmployeeInfo.WorkDateFrom.HasValue? agreementEmployeeInfo.WorkDateFrom.Value.ToString("HH:mm") : string.Empty;
      agreement.WorkDateTo = agreementEmployeeInfo.WorkDateTo.HasValue? agreementEmployeeInfo.WorkDateTo.Value.ToString("HH:mm") : string.Empty;
      agreement.FirstBreakFrom = agreementEmployeeInfo.FirstBreakFrom.HasValue? agreementEmployeeInfo.FirstBreakFrom.Value.ToString("HH:mm"): string.Empty;
      agreement.FirstBreakTo =  agreementEmployeeInfo.FirstBreakTo.HasValue? agreementEmployeeInfo.FirstBreakTo.Value.ToString("HH:mm") : string.Empty;
      agreement.SecondBreakFrom = agreementEmployeeInfo.SecondBreakFrom.HasValue? agreementEmployeeInfo.SecondBreakFrom.Value.ToString("HH:mm") : string.Empty;
      agreement.SecondBreakTo = agreementEmployeeInfo.SecondBreakTo.HasValue? agreementEmployeeInfo.SecondBreakTo.Value.ToString("HH:mm") : string.Empty;
      agreement.Compensation = agreementEmployeeInfo.Compensation;
      agreement.RestDateFrom = agreementEmployeeInfo.RestDateFrom.HasValue? agreementEmployeeInfo.RestDateFrom.Value.ToString("dd.MM.yyyy") : string.Empty;
      
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