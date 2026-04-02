using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.BusinessRole;

namespace DirRX.NNCommons
{
  partial class BusinessRoleServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.BusinessRole.Remote.GetDuplicates(_obj).Any())
        e.AddError(DirRX.NNCommons.BusinessRoles.Resources.FindDuplicatesError, _obj.Info.Actions.ShowDuplicates);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      PublicFunctions.BusinessRole.FillName(_obj);
      _obj.Status = BusinessRole.Status.Active;
      _obj.RoleType = BusinessRole.RoleType.ByProcess;
      _obj.Top = false;
    }
  }
}