using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.JobTitle;

namespace DirRX.OverrideSolution
{
  partial class JobTitleServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.IsEmployeeExistsDirRX = true;
    }

  }
}