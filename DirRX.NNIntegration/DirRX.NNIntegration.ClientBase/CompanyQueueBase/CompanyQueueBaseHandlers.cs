using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.CompanyQueueBase;

namespace DirRX.NNIntegration
{
  partial class CompanyQueueBaseClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      _obj.State.Pages.ErrorPage.IsVisible = _obj.ProcessingStatus == CompanyQueueBase.ProcessingStatus.Error;
    }

  }
}