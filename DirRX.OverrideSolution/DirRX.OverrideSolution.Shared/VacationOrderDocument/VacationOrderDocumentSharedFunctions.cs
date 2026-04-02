using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationOrderDocument;

namespace DirRX.OverrideSolution.Shared
{
  partial class VacationOrderDocumentFunctions
  {

    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public override void FillName()
    {
      if (_obj.DocumentKind != null && !_obj.DocumentKind.GenerateDocumentName.Value && _obj.Name == Sungero.Docflow.OfficialDocuments.Resources.DocumentNameAutotext)
        _obj.Name = string.Empty;
      
      if (_obj.DocumentKind == null || !_obj.DocumentKind.GenerateDocumentName.Value)
        return;
      
      var name = string.Empty;
      
      // Имя в формате: <Вид документа> <Сотрудник> от <Дата создания документа>.
      using (TenantInfo.Culture.SwitchTo())
      {
        name += string.Format("{0} ", _obj.DocumentKind.ShortName);
        name += _obj.Employee != null ? Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employee, DeclensionCase.Genitive, false) : NNVacations.Resources.Employee;
        
        if (_obj.RegistrationDate != null)
          name += NNVacations.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("d");
      }
      _obj.Name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
    }

  }
}