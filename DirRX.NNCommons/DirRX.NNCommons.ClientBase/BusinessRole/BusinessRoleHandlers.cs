using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.BusinessRole;

namespace DirRX.NNCommons
{
  partial class BusinessRoleClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      Functions.BusinessRole.SetStatePropertyDependOnRoleType(_obj);
      _obj.State.Properties.Name.IsEnabled = false;
    }
  }

}