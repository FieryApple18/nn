using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.DepartmentQueue;

namespace DirRX.NNIntegration.Shared
{
  partial class DepartmentQueueFunctions
  {
    /// <summary>
    /// Заполнить наименование записи очереди.
    /// </summary>
    public override void FillRecordName()
    {
      _obj.RecordName = Functions.CompanyQueueBase.GetRecordName(_obj, _obj.SapId, _obj.Name, _obj.ReceivedDate);
    }

  }
}