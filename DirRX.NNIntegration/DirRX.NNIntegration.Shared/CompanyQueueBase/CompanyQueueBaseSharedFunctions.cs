using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.CompanyQueueBase;

namespace DirRX.NNIntegration.Shared
{
  partial class CompanyQueueBaseFunctions
  {

    /// <summary>
    /// Заполнить наименование записи очереди.
    /// </summary>
    public virtual void FillRecordName()
    {
      _obj.RecordName = _obj.Id.ToString();
    }

  }
}