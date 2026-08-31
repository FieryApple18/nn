using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.CoreEntities.Server;
using Sungero.Workflow;

namespace DirRX.NNOtherProcesses.Server
{
  partial class NNRetentionStatementDocumentFunctions
  {
    
    /// <summary>
    /// Создание задачи для ручного проведения мероприятия.
    /// </summary>
    /// <returns>Текст ошибки или пустая строка если ошибки нет.</returns>
    [Public]
    public string CreateManualHoldingEvent()
    {
      var logPrefix = string.Format("CreateManualHoldingEvent. ID документа: {0}", _obj.Id);
      var errorMessage = string.Empty;

      if (_obj == null)
      {
        errorMessage = "Передана пустая ссылка на документ.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return errorMessage;
      }

      var task = NNIntegration.ExternalSystemDataExchangeTasks.Create();
      Logger.DebugFormat("{0}. Создана задача с ИД: {1}.", logPrefix, task.Id);
      var processId = DirRX.NNCommons.PublicFunctions.Module.GetIdentifierValue(DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.ManualHoldingEventProcessKindId.Guid);
      var process = ProcessKinds.GetAllMatches(task).FirstOrDefault(c => c.Id == processId);

      if (process == null)
      {
        errorMessage = string.Format("Действующий вариант процесса для типа задачи 'Обмен данными с внешней системой' с ИД: {0} не найден. Проверьте значение в константе \"{1}\"",
                                     processId, DirRX.NNOtherProcesses.PublicConstants.Module.ConstantsSetting.ManualHoldingEventProcessKindId.Guid);
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return errorMessage;
      }

      task.Subject = DirRX.NNOtherProcesses.Resources.SubjectTaskManualHoldingEvent;
      task.ProcessKind = process;
      task.Documents.AddNew().Document = _obj;
      try
      {
        task.Start();
        Logger.DebugFormat("{0}. Отправлена задача с ИД: {1}.", logPrefix, task.Id);
      }
      catch (Exception ex)
      {
        errorMessage = "Не удалось запустить задачу.";
        Logger.ErrorFormat("{0}. {1} Текст ошибки: {3} StackTrace: {4}.", logPrefix, errorMessage, ex.Message, ex.StackTrace);
        return errorMessage;
      }
      
      return string.Empty;
    }
  }
}