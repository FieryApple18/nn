using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.IncomingMessage;

namespace DirRX.NNIntegration.Server
{
  partial class IncomingMessageFunctions
  {
    /// <summary>
    /// Получить текст сообщения.
    /// </summary>
    /// <returns>Текст сообщения.</returns>
    [Public, Remote(IsPure = true)]
    public virtual string GetIncomingMessageText()
    {
      return PublicFunctions.Module.DecodeMessageText(_obj.MessageText.Read(), _obj.Id, true);
    }
    
    /// <summary>
    /// Установить текст сообщения и сохранить запись справочника.
    /// </summary>
    /// <param name="messageText">Текст сообщения.</param>
    /// <returns>Текст ошибки, если она возникла.</returns>
    [Public]
    public virtual string SetIncomingMessageTextAndSaveRecord(string messageText)
    {
      var result = string.Empty;
      var stream = PublicFunctions.Module.EncodingMessageText(messageText, _obj.Id, true);
      try
      {
        _obj.MessageText.Write(stream);
        _obj.Save();
      }
      catch (Exception e)
      {
        Logger.ErrorFormat("SetIncomingMessageTextAndSaveRecord. Ид сообщения: {0}. При установке текста сообщения произошла ошибка: {1}. StackTrace:{2}.",
                           _obj.Id, e.Message, e.StackTrace);
        result = e.Message;
      }
      finally
      {
        stream.Close();
      }
      return result;
    }
  }
}