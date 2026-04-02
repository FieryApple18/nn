using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.StatementTask;

namespace DirRX.OverrideSolution
{
  partial class StatementTaskSharedHandlers
  {

    public override void DocumentGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      var document = OfficialDocuments.As(e.Attachment);
      _obj.Subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(StatementTasks.Resources.StatementTaskSubject, document.Name);
    }

  }
}