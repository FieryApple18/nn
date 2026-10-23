using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.StatementReworkAssignment;

namespace DirRX.NNCommons.Client
{
  partial class StatementReworkAssignmentActions
  {
    public virtual void Abort(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanAbort(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return (_obj.Performer.Equals(Users.Current) ||
              (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowed.HasValue && _obj.IsSubstitutionAllowed.Value == true));
    }

    public virtual void ForReapproving(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      
      // Если вложение - заявка на ВД/ПД/СР, то выполнить проверку в САП.
      if(NNOtherProcesses.WeekendWorkStatementDocuments.Is(document))
      {
        var error = NNOtherProcesses.PublicFunctions.WeekendWorkStatementDocument.Remote.ValidateStatementInExternalSystem(NNOtherProcesses.WeekendWorkStatementDocuments.As(document));
        if (!string.IsNullOrEmpty(error))
        {
          e.AddError(error);
          return;
        }
      }
    }

    public virtual bool CanForReapproving(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return (_obj.Performer.Equals(Users.Current) ||
              (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowed.HasValue && _obj.IsSubstitutionAllowed.Value == true));
    }

  }

}