using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentKind;

namespace DirRX.OverrideSolution
{
  partial class DocumentKindClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params[DirRX.OverrideSolution.PublicConstants.Docflow.DocumentKind.IsVisualModeParamName] = true;
      _obj.State.Pages.HRProPage.IsVisible = true;
    }

  }
}