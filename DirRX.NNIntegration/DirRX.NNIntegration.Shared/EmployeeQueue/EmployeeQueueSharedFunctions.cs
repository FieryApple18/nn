using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.EmployeeQueue;

namespace DirRX.NNIntegration.Shared
{
  partial class EmployeeQueueFunctions
  {
    /// <summary>
    /// Заполнить наименование записи очереди.
    /// </summary>   
    public override void FillRecordName()
    {
      _obj.RecordName = string.Join(" ", _obj.PersonnelNumber, _obj.LastName, _obj.FirstName, _obj.MiddleName, _obj.ReceivedDate?.ToString(Constants.CompanyQueueBase.DateFormat));
    }
  }
}