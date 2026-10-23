using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkRestStatementDocument;
using DirRX.OverrideSolution;

namespace DirRX.NNOtherProcesses.Server
{
  partial class WeekendWorkRestStatementDocumentFunctions
  {

    /// <summary>
    /// Получить заявление на доп день отдыха для сотрудника.
    /// </summary>
    /// <param name="sourceDocument">Заявка на привлечение к работе в ВД/ПД/СР.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Заявление на доп день отдыха.</returns>
    [Public]
    public static IWeekendWorkRestStatementDocument GetRestStatement(IWeekendWorkStatementDocument sourceDocument, IEmployee employee)
    {
      return WeekendWorkRestStatementDocuments.GetAll()
        .Where(x =>
               WeekendWorkStatementDocuments.Equals(x.SourceDocument, sourceDocument) &&
               Employees.Equals(x.EmployeeDirRX, employee))
        .FirstOrDefault();
    }
  }
}