using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.HRSimpleDocument;

namespace DirRX.NNCommons
{
  partial class HRSimpleDocumentServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
      base.BeforeSave(e);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.RegistrationDate = Calendar.Now;
    }
  }

}