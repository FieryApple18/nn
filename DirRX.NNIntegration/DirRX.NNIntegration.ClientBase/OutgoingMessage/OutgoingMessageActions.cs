using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.OutgoingMessage;

namespace DirRX.NNIntegration.Client
{
  partial class OutgoingMessageActions
  {
    public virtual void MessageText(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var message = DirRX.NNIntegration.PublicFunctions.OutgoingMessage.GetOutgoingMessageText(_obj);
      
      var dialog = Dialogs.CreateInputDialog(DirRX.NNIntegration.IncomingMessages.Resources.TextOfTheMessage);
      var messageControl = dialog.AddMultilineString(string.Empty, false, message);
      messageControl.IsEnabled = false;
      messageControl.WithRowsCount(15);
      dialog.Width = 600;
      dialog.Show();
    }

    public virtual bool CanMessageText(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.ExistsMessageText == true;
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

  partial class OutgoingMessageCollectionActions
  {

    public virtual bool CanSendMessage(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _objs.All(m => m.StatusMessage.HasValue && m.StatusMessage.Value == OutgoingMessage.StatusMessage.Error);
    }

    public virtual void SendMessage(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var text = _objs.Count() == 1 ? NNIntegration.OutgoingMessages.Resources.RetrySendMessage : NNIntegration.OutgoingMessages.Resources.RetrySendMessages;
      var dialog = Dialogs.CreateTaskDialog(text, MessageType.Question);
      
      var resendButton = dialog.Buttons.AddCustom(NNIntegration.OutgoingMessages.Resources.RetrySending);
      var upgradeResendButton = dialog.Buttons.AddCustom(NNIntegration.OutgoingMessages.Resources.OverwriteAndSend);
      var cancelButton = dialog.Buttons.AddCancel();
      var result = dialog.Show();
      var clearTextMessage = result == upgradeResendButton;
      if (result == resendButton || clearTextMessage)
      {
        foreach (var message in _objs)
        {
          PublicFunctions.OutgoingMessage.Remote.ChangePropertiesResendMessage(message, clearTextMessage);
          
          var handler = NNIntegration.AsyncHandlers.SendOutgoingMessage.Create();
          handler.OutgoingMessageId = message.Id;
          handler.ExecuteAsync();
        }
        
      }
    }
  }

}