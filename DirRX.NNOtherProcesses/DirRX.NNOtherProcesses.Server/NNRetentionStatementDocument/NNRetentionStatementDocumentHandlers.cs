using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.NNRetentionStatementDocument;

namespace DirRX.NNOtherProcesses
{
  partial class NNRetentionStatementDocumentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      var employee = DirRX.OverrideSolution.Employees.Current;
      if (employee !=  null)
        _obj.EmployeeDirRX = employee;
      
      base.Created(e);
    }
  }
}