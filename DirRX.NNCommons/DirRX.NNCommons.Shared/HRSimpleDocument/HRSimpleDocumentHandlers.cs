using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.HRSimpleDocument;

namespace DirRX.NNCommons
{
  partial class HRSimpleDocumentSharedHandlers
  {

    public override void EmployeeDirRXChanged(DirRX.OverrideSolution.Shared.DocRelatedToWorkBaseEmployeeDirRXChangedEventArgs e)
    {
      base.EmployeeDirRXChanged(e);
      Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
    }

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      // HACK: Seleznyov_DV. Обход отчистки Даты регистрации при изменении вида документа.
      var regData = _obj.RegistrationDate;
      base.DocumentKindChanged(e);
      _obj.RegistrationDate = regData;
      
      if (!OverrideSolution.DocumentKinds.Equals(e.NewValue, e.OldValue))
      {
        if (e.NewValue != null && e.NewValue.GenerateDocumentName == true)
          Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
        else
          _obj.Name = string.Empty;
      }
    }
  }


}