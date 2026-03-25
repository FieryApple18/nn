using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.StatementDocumentBase;

namespace DirRX.NNCommons
{
  partial class StatementDocumentBaseServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.PreparationDate = Calendar.UserNow;
    }
  }

}