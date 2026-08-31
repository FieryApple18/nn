using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.IncomingMessage;

namespace DirRX.NNIntegration.Client
{
  partial class IncomingMessageActions
  {
    public virtual void Reprocess(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.StatusMessage = IncomingMessage.StatusMessage.NeedProcessing;
      _obj.Save();
      var async = AsyncHandlers.ProcessingIncomingMessages.Create();
      async.IncomingMessageId = _obj.Id;
      async.ExecuteAsync();
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanReprocess(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.StatusMessage == IncomingMessage.StatusMessage.Error || _obj.StatusMessage == IncomingMessage.StatusMessage.NeedProcessing;
    }

    public virtual void MessageText(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var message = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(_obj);
      
      var dialog = Dialogs.CreateInputDialog(DirRX.NNIntegration.IncomingMessages.Resources.TextOfTheMessage);
      var messageControl = dialog.AddMultilineString(string.Empty, false, message);
      messageControl.IsEnabled = false;
      messageControl.WithRowsCount(15);
      dialog.Width = 600;
      dialog.Show();
    }

    public virtual bool CanMessageText(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public override void CopyEntity(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.CopyEntity(e);
    }

    public override bool CanCopyEntity(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false;
    }

  }

}