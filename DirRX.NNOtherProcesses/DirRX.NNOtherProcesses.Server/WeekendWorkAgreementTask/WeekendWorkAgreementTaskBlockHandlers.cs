using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.NNOtherProcesses.WeekendWorkAgreementTask;

namespace DirRX.NNOtherProcesses.Server.WeekendWorkAgreementTaskBlocks
{
  partial class SetRejectedStatusForEmployeeBlockHandlers
  {

    public virtual void SetRejectedStatusForEmployeeBlockExecute()
    {
      var employeeInfo = _obj.WeekendWorkStatement.Employees.Where(x => OverrideSolution.Employees.Equals(x.Employee, _obj.Employee)).FirstOrDefault();
      employeeInfo.RejectedByApproval = true;
      
      try
      {
        _obj.WeekendWorkStatement.Save();
        Logger.DebugFormat("SetRejectedStatusForEmployee. ИД заявки ВД/ПД/СР: {0}. Таб. номмер сотрудника: {1}. Признак \"Отклонено\" успешно установлен в ТЧ \"Данные по сотрудникам\" заявки.",
                           _obj.WeekendWorkStatement.Id, _obj.Employee.PersonnelNumber);
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("SetRejectedStatusForEmployee. ИД заявки ВД/ПД/СР: {0}. Таб. номмер сотрудника: {1}. Ошиба при установке признака \"Отклонено\" в ТЧ \"Данные по сотрудникам\" заявки. Текст ошибки: {2}. StackTrace: {3}.",
                           _obj.WeekendWorkStatement.Id, _obj.Employee.PersonnelNumber, ex.Message, ex.StackTrace);
      }
    }
  }

  partial class CreateWeekendWorkConsentHandlers
  {

    public virtual void CreateWeekendWorkConsentExecute()
    {
      var statement = _block.WeekendWorkStatement;
      var agreement = Functions.WeekendWorkAgreementDocument.CreateWeekendWorkAgreement(_obj.Employee, statement);
      _obj.DocumentGroup.WeekendWorkAgreementDocuments.Add(agreement);
    }
  }

}