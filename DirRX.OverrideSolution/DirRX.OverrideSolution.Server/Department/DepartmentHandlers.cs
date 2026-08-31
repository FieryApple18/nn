using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Department;

namespace DirRX.OverrideSolution
{
  partial class DepartmentServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.IsHeadquartersDirRX = false;
    }

  }
}