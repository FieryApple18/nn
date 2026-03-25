using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkAgreementDocument;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkAgreementDocumentSharedHandlers
  {

    public override void EmployeeDirRXChanged(DirRX.OverrideSolution.Shared.DocRelatedToWorkBaseEmployeeDirRXChangedEventArgs e)
    {
      base.EmployeeDirRXChanged(e);
      Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
    }

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      var regData = _obj.RegistrationDate;
      base.DocumentKindChanged(e);
      Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
      _obj.RegistrationDate = regData;
    }

  }
}