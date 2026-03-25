using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.NNIntegration.ExternalSystemDataExchangeTask;

namespace DirRX.NNIntegration.Server.ExternalSystemDataExchangeTaskBlocks
{
  partial class SendOrderSigningStatusToSAPHandlers
  {
    /// <summary>
    /// Отправка статуса подписания приказа в АСУП.
    /// </summary>
    public virtual void SendOrderSigningStatusToSAPExecute()
    {
      var parentTaskId = _obj.ParentTask != null ? _obj.ParentTask.Id : _obj.Id;
      var flowGuid = Constants.Module.ExternalSystemsFlowGuids.StatusDocumentInSap;
      Functions.Module.StartCreatedOutgoingMessages(flowGuid, new List<long> { parentTaskId });
    }
  }

  partial class SendDataToExternalSystemBlockHandlers
  {
    /// <summary>
    /// Обмен данными с внешней системой.
    /// </summary>
    public virtual void SendDataToExternalSystemBlockExecute()
    {
      var document = _obj.Documents.FirstOrDefault()?.Document;
      var logPrefix = string.Format("SendDataToExternalSystemBlockExecute. TaskId: {0}", _obj.Id);
      if (document == null)
      {
        Logger.DebugFormat("{0}. Не найден документ в свойстве \"Документы\" задачи.", logPrefix);
        return;
      }
      
      // Определить поток по виду документа.
      var taskId =_obj.ParentTask?.Id ?? _obj.Id;
      var flow = ExternalSystemFlows.GetAll(x => x.DocumentKinds.Any(c => c.DocumentKind.Id == document.DocumentKind.Id)).FirstOrDefault();
      
      if (flow == null)
      {
        Logger.ErrorFormat("{0}. ID документа {1}. Не найден поток по виду документа. ID вида документа: {2}.", logPrefix, document.Id, document.DocumentKind.Id);
        return;
      }
      
      // Запустить процесс отправки Исходящих сообщений во внешнюю систему.
      NNIntegration.PublicFunctions.Module.StartCreatedOutgoingMessages(flow.FlowGuid, new List<long> { taskId });
    }
  }

}