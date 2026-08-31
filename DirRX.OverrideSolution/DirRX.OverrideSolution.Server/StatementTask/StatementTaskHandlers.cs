using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.StatementTask;

namespace DirRX.OverrideSolution
{
  partial class StatementTaskServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (_obj.State.IsInserted)
        DirRX.OverrideSolution.Functions.StatementTask.AddSubstitutions(_obj);
    }

    // HACK: Копирование базового метода BeforeStart, с добавлением работы с новой группой вложения VacationStatementDocuments.
    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      var isVacationDoc = _obj.DocumentGroup.DocRelatedToWorkBases.Any();
      
      var statement = isVacationDoc ? OfficialDocuments.As(_obj.DocumentGroup.DocRelatedToWorkBases.FirstOrDefault()) :
        OfficialDocuments.As(_obj.DocumentGroup.StatementDocuments.FirstOrDefault());
      
      _obj.Author = statement.Author;
      
      if (!HRPro.HRDocFlow.PublicFunctions.Module.Remote.CheckEmployeeConsent(HRPro.HRProSolution.Employees.As(_obj.Author)))
        e.AddError(HRPro.HRDocFlow.Resources.EmployeeConsentError);
      
      var mainDocs = isVacationDoc ? _obj.DocumentGroup.DocRelatedToWorkBases.Select(OfficialDocuments.As) :
        _obj.DocumentGroup.StatementDocuments.Select(OfficialDocuments.As);
      
      HRPro.HRProSolution.Module.HRWred.PublicFunctions.Module.CreateSigningOperation(mainDocs, new List<Sungero.Docflow.IOfficialDocument>(), Sungero.Company.Employees.As(_obj.Author), false);
    }

  }
}