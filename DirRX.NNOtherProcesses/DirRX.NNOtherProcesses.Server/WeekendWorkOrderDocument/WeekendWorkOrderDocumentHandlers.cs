using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkOrderDocument;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkOrderDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      Functions.WeekendWorkOrderDocument.FillName(_obj);
      base.BeforeSave(e);
    }
  }

}