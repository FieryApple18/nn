using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.CompanyQueueBase;

namespace DirRX.NNIntegration
{
  partial class CompanyQueueBaseServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      Functions.CompanyQueueBase.FillRecordName(_obj);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.ProcessingStatus = CompanyQueueBase.ProcessingStatus.Created;
      _obj.ReceivedDate = Calendar.Now;
      _obj.RecordStartDate = Calendar.Now;
    }
  }

}