using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.ESSUserAccountQueue;

namespace DirRX.NNIntegration.Shared
{
  partial class ESSUserAccountQueueFunctions
  {
    /// <summary>
    /// Заполнить наименование записи очереди.
    /// </summary>
    public override void FillRecordName()
    {
      _obj.RecordName = Functions.CompanyQueueBase.GetRecordName(_obj, _obj.PersonnelNumber, string.Empty, _obj.ReceivedDate);
    }
  }
}