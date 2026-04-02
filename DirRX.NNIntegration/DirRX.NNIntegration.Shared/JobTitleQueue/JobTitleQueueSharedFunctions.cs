using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.JobTitleQueue;

namespace DirRX.NNIntegration.Shared
{
  partial class JobTitleQueueFunctions
  {
    /// <summary>
    /// Заполнить наименование записи очереди.
    /// </summary>   
    public override void FillRecordName()
    {
      _obj.RecordName = string.Join(" ", _obj.SapId, _obj.Name, _obj.ReceivedDate?.ToString(Constants.CompanyQueueBase.DateFormat));
    }
  }
}