using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.NNRetentionStatementDocument;

namespace DirRX.NNOtherProcesses.Shared
{
  partial class NNRetentionStatementDocumentFunctions
  {
    /// <summary>
    /// Заполнить имя документа.
    /// </summary>
    public override void FillName()
    {
      if (_obj.EmployeeDirRX == null || _obj.Created == null || _obj.DocumentKind == null)
      {
        _obj.Name = Sungero.Docflow.Resources.DocumentNameAutotext;
        return;
      }
      
      var name = string.Empty;
      
      using (TenantInfo.Culture.SwitchTo())
      {
        name = NNRetentionStatementDocuments.Resources.GenerateDocumentNameTemplateFormat(_obj.DocumentKind.ShortName,
          Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.EmployeeDirRX, DeclensionCase.Genitive, false),
          _obj.Created.Value.ToString("d"));
      }
      
      _obj.Name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
    }
  }
}