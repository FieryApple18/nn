using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.ExternalSystem;

namespace DirRX.NNIntegration.Client
{
  partial class ExternalSystemActions
  {
    public virtual void SetPassword(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var dialog = Dialogs.CreateInputDialog(DirRX.NNIntegration.ExternalSystems.Resources.DialogPassword);
      var password = dialog.AddPasswordString(DirRX.NNIntegration.ExternalSystems.Resources.DialogPassword, true);
      dialog.Buttons.AddOkCancel();
      dialog.Buttons.Default = DialogButtons.Ok;
      dialog.SetOnButtonClick(a =>
                              {
                                if (a.Button == DialogButtons.Ok)
                                  _obj.Password = password.Value;
                              });
      dialog.Show();
    }

    public virtual bool CanSetPassword(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}