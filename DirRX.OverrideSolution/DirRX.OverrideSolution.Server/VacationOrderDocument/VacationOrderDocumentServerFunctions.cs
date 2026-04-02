using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationOrderDocument;

namespace DirRX.OverrideSolution.Server
{
  partial class VacationOrderDocumentFunctions
  {

    /// <summary>
    /// Создать приказ об отпуске и заполнить карточку по сотруднику.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="documentKind">Вид документа.</param>
    /// <returns>Приказ об отпуске.</returns>
    [Public]
    public static IVacationOrderDocument CreateVacationDocumentWithEmployee(IEmployee employee, IDocumentKind documentKind)
    {
      var document = VacationOrderDocuments.Create();
      document.DocumentKind = documentKind;
      document.BusinessUnit = employee.BusinessUnit;
      document.Department = employee.Department;
      document.Employee = employee;
      
      return document;
    }

    /// <summary>
    /// Создать приказ об отпуске и заполнить карточку по сотруднику.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="documentKind">Guid вида документа.</param>
    /// <returns>Приказ об отпуске.</returns>
    [Public]
    public static IVacationOrderDocument CreateVacationDocumentWithEmployee(IEmployee employee, Guid documentKindGuid)
    {
      var document = VacationOrderDocuments.Create();
      document.DocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(documentKindGuid);
      document.BusinessUnit = employee.BusinessUnit;
      document.Department = employee.Department;
      document.Employee = employee;
      
      return document;
    }

    /// <summary>
    /// Получить приказ об отпуске по SAP ID.
    /// </summary>
    /// <param name="SAPID">SAP ID.</param>
    /// <returns>Приказ об отпуске.</returns>
    [Public]
    public static IVacationOrderDocument GetVacationOrderDocumentBySAPID(string SAPID)
    {
      return VacationOrderDocuments.GetAll(x => x.ExternalId == SAPID).FirstOrDefault();
    }

  }
}