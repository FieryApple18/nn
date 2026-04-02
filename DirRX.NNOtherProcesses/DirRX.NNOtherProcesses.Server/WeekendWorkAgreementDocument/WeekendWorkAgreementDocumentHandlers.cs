using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkAgreementDocument;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkAgreementDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.RegistrationDate = Calendar.Now;
    }
  }

}