using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.ExternalSystem;

namespace DirRX.NNIntegration
{
  partial class ExternalSystemServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.NeedAuthorization = false;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // Если изменили пароль, то зашифровать.
      if (_obj.State.Properties.Password.OriginalValue != _obj.Password)
        _obj.Password = Encryption.Encrypt(_obj.Password);
    }
  }


}