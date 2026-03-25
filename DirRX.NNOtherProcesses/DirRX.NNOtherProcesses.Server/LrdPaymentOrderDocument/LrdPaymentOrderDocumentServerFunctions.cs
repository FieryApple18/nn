using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentOrderDocument;

namespace DirRX.NNOtherProcesses.Server
{
  partial class LrdPaymentOrderDocumentFunctions
  {
    /// <summary>
    /// Создать приказ на выплату по ЛНА.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="documentKind">Guid вида документа.</param>
    /// <returns>Приказ об отпуске.</returns>
    [Public]
    public static ILrdPaymentOrderDocument CreateLrdPaymentOrderWithEmployee(OverrideSolution.IEmployee employee, Guid documentKindGuid)
    {
      var document = LrdPaymentOrderDocuments.Create();
      document.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(documentKindGuid);
      
      if (employee.BusinessUnit == null)
      {
        Logger.ErrorFormat("CreateLrdPaymentOrderWithEmployee. ИД сотрудника: {0}. У сотрудника не заполнена НОР. Документ не будет создан.", employee.Id);
        return LrdPaymentOrderDocuments.Null;
      }
      
      document.BusinessUnit = employee.BusinessUnit;
      document.Department = employee.Department;
      document.EmployeeDirRX = employee;
      
      return document;
    }
  }
}