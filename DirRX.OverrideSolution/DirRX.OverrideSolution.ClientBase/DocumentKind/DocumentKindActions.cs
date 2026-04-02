using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentKind;

namespace DirRX.OverrideSolution.Client
{
  partial class DocumentKindActions
  {

    public virtual void GetBusinessRolesLinksDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.DocumentKind.Remote.GetBusinessRolesLinks(_obj).ShowModal();
    }

    public virtual bool CanGetBusinessRolesLinksDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }
}