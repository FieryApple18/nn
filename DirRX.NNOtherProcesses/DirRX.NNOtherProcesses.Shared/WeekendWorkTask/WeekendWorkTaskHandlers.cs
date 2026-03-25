using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkTask;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkTaskSharedHandlers
  {

    public virtual void DocumentGroupDeleted(Sungero.Workflow.Interfaces.AttachmentDeletedEventArgs e)
    {
      Functions.WeekendWorkTask.FillSubject(_obj);
    }

    public virtual void DocumentGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      Functions.WeekendWorkTask.FillSubject(_obj);
    }

  }
}