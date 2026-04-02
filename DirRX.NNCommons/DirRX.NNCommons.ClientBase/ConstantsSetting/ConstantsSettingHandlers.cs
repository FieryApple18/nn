using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.ConstantsSetting;

namespace DirRX.NNCommons
{
  partial class ConstantsSettingClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      Functions.ConstantsSetting.SetPropertiesState(_obj);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      Functions.ConstantsSetting.SetPropertiesState(_obj);
      if (_obj.State.IsInserted && string.IsNullOrEmpty(_obj.GUID))
        _obj.GUID = Guid.NewGuid().ToString();
    }

  }
}