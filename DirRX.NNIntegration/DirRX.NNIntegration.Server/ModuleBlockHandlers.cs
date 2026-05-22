using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace DirRX.NNIntegration.Server.NNIntegrationBlocks
{
  /// <summary>
  /// Отправка документов на проштамповку.
  /// </summary>
  partial class SendToStampingBlockHandlers
  {

    public virtual void SendToStampingBlockExecute()
    {
      Logger.DebugFormat("SendToStampingBlockExecute. ИД задачи: {0}. Старт выполнения блока \"Отправка документов на проштамповку\".", _obj.Id);
      
      if (!_block.Documents.Any())
      {
        Logger.DebugFormat("SendToStampingBlockExecute. ИД задачи: {0}. Не обнаружены документы для перештамповки в свойстве блока.", _obj.Id);
        return;
      }
      
      var externalSystemFlow = NNIntegration.PublicFunctions.ExternalSystemFlow.GetExternalSystemFlow(NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.SendDocumentToStamping);
      if (externalSystemFlow == null)
      {
        Logger.ErrorFormat("SendToStampingBlockExecute. ИД задачи: {0}. Не найден поток внешней системы по гуид: {1}.", _obj.Id,
                           NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.SendDocumentToStamping);
        return;
      }
      
      var rewritePublicBody = _block.RewritePublicBody == true;
      // Определение типа подписи. По умолчанию - пусто.
      bool? isSimpleSignature = null;
      // ПЭП.
      if (_block.SignatureType == NNIntegration.Server.NNIntegrationBlocks.SendToStampingBlockHandlers.SignatureType.Simple)
        isSimpleSignature = true;
      // УКЭП/УНЭП.
      else if (_block.SignatureType == NNIntegration.Server.NNIntegrationBlocks.SendToStampingBlockHandlers.SignatureType.Qualified)
        isSimpleSignature = false;

      Logger.DebugFormat("SendToStampingBlockExecute. ИД задачи: {0}. Всего документов: {1}. rewritePublicBody: {2}. isSimpleSignature: {3}.", 
                         _obj.Id, _block.Documents.Count(), rewritePublicBody, isSimpleSignature);
      
      foreach (var document in _block.Documents)
      {
        var version = document.LastVersion;
        if (version == null)
        {
          Logger.ErrorFormat("SendToStampingBlockExecute. ИД задачи: {0}. ИД документа: {1}. У документа нет последней версии.", _obj.Id, document.Id);
          return;
        }

        // Сформировать текст исходящего сообщения для отправки документа в сервис проштамповки.
        // Указываем тип ЭП и признак для перезаписывания PublicBody из настройки блока.
        var messageGuid = Guid.NewGuid().ToString();
        var resultGenerateText = Functions.Module.GetTextForSendToStampingDocument(document, version, messageGuid, rewritePublicBody, isSimpleSignature);
        if (!string.IsNullOrEmpty(resultGenerateText.ErrorText))
        {
          Logger.ErrorFormat("SendToStampingBlockExecute.  ИД задачи: {0}. ИД документа: {1}. Ошибка при формировании текста сообщения: {2}", _obj.Id, document.Id,
                             resultGenerateText.ErrorText);
          return;
        }
        
        Logger.DebugFormat("SendToStampingBlockExecute. ИД задачи: {0}. ИД документа: {1}. ИД версии: {2}. Сформирован текст исходящего сообщения.", _obj.Id, document.Id, version.Id);
        
        // Создать исходящее сообщение.
        var message = OutgoingMessages.Create();
        message.EntityId = document.Id;
        message.VersionId = version.Id;
        message.ExternalSystemFlow = externalSystemFlow;
        message.GuidMessage = messageGuid;
        
        // Заполнить текст сообщения и сохранить исходящее сообщение.
        PublicFunctions.OutgoingMessage.SetOutgoingMessageTextAndSaveRecord(message, resultGenerateText.RequestText);
        
        Logger.DebugFormat("SendToStampingBlockExecute. ИД задачи: {0}. ИД документа: {1}. ИД версии: {2}. Запуск АО для отправки исходящего сообщения.", _obj.Id, document.Id, version.Id);
        
        var handler = NNIntegration.AsyncHandlers.SendOutgoingMessage.Create();
        handler.OutgoingMessageId = message.Id;
        handler.ExecuteAsync();
      }
      
      Logger.DebugFormat("SendToStampingBlockExecute. ИД задачи: {0}. Завершение выполнения блока \"Отправка документов на проштамповку\".", _obj.Id);
    }
  }

  /// <summary>
  /// Передача данных во внешнюю систему.
  /// </summary>
  partial class SendingDataToExternalSystemBlockHandlers
  {

    public virtual void SendingDataToExternalSystemBlockStartTask(DirRX.NNIntegration.IExternalSystemDataExchangeTask task)
    {
      // Сформировать тему задачи.
      if (string.IsNullOrEmpty(_block.Subject))
      {
        var taskId = task.ParentTask?.Id ?? task.Id;
        task.Subject = DirRX.NNIntegration.ExternalSystemDataExchangeTasks.Resources.TaskDefaultSubjectFormat(taskId);
      }
      
      // Добавить документы в задачу.
      foreach (var document in _block.Documents)
        task.Documents.AddNew().Document = document;
    }

    public virtual void SendingDataToExternalSystemBlockStart()
    {
      _block.IsSubtask = true;
      _block.NeedsReview = false;
    }
  }

  /// <summary>
  /// Отправка документов в ЛК SAP.
  /// </summary>
  partial class SendDocumentsToSapHandlers
  {
    public virtual void SendDocumentsToSapExecute()
    {
      var ids = _block.Documents.Select(c => c.Id).ToList();
      
      if (!ids.Any())
      {
        Logger.ErrorFormat("SendDocumentsToSapExecute. Отсутствуют документы в свойствах блока.");
        return;
      }
      
      PublicFunctions.Module.StartCreatedOutgoingMessages(Constants.Module.ExternalSystemsFlowGuids.SapFioriOutgoing, ids);
    }
  }
}