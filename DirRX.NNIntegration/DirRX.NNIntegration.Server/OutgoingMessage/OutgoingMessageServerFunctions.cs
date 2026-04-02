using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.OutgoingMessage;

namespace DirRX.NNIntegration.Server
{
  partial class OutgoingMessageFunctions
  {
    /// <summary>
    /// Получить текст сообщения.
    /// </summary>
    /// <returns>Текст сообщения.</returns>
    [Public]
    public virtual string GetOutgoingMessageText()
    {
      return PublicFunctions.Module.DecodeMessageText(_obj.MessageText.Read(), _obj.Id, false);
    }
    
    /// <summary>
    /// Установить текст сообщения и сохранить запись справочника.
    /// </summary>
    /// <param name="messageText">Текст сообщения.</param>
    /// <returns>Текст ошибки, если она возникла.</returns>
    [Public]
    public virtual string SetOutgoingMessageTextAndSaveRecord(string messageText)
    {
      var result = string.Empty;
      var stream = PublicFunctions.Module.EncodingMessageText(messageText, _obj.Id, true);
      try
      {
        _obj.MessageText.Write(stream);
        if (!string.IsNullOrEmpty(messageText))
          _obj.ExistsMessageText = true;
        
        _obj.Save();
      }
      catch (Exception e)
      {
        Logger.ErrorFormat("SetOutgoingMessageTextAndSaveRecord. Ид сообщения: {0}. При установке текста сообщения произошла ошибка: {1}. StackTrace:{2}.",
                           _obj.Id, e.Message, e.StackTrace);
        result = e.Message;
      }
      finally
      {
        stream.Close();
      }
      return result;
    }
    
    /// <summary>
    /// Изменить значения свойств при повторной отправке сообщения в КСИП.
    /// </summary>
    /// <param name="isClearMessageText">Требуется очистить текст сообщения?</param>
    [Public, Remote(IsPure = false)]
    public void ChangePropertiesResendMessage(bool isClearMessageText)
    {
      if (!Locks.GetLockInfo(_obj).IsLockedByMe && !Locks.TryLock(_obj))
        return;
      _obj.StatusMessage = NNIntegration.OutgoingMessage.StatusMessage.Created;
      if (isClearMessageText && !string.IsNullOrEmpty(GetOutgoingMessageText()))
        SetOutgoingMessageTextAndSaveRecord(string.Empty);
      else
        _obj.Save();
      Locks.Unlock(_obj);
    }
  }
}