using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkOrderDocument;

namespace DirRX.NNOtherProcesses.Client
{
  partial class WeekendWorkOrderDocumentActions
  {
    public virtual void CancelOrder(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var statement = _obj.WeekendWorkStatement;
      if (statement == null)
      {
        Logger.DebugFormat("CancelOrder. ИД приказа: {0}. В приказе ВД/ПД/СР нет ссылки на заявку.", _obj.Id);
        return;
      }
      
      if (statement.WorkDateFrom.Value.Date <= Calendar.UserToday)
      {
        e.AddWarning(DirRX.NNOtherProcesses.WeekendWorkOrderDocuments.Resources.CancelOrderDataWarning);
        return;
      }
      
      if (statement.Employees.All(x => x.RejectedByOrder == true))
      {
        e.AddWarning(DirRX.NNOtherProcesses.WeekendWorkOrderDocuments.Resources.CancelOrderAllEmployeesRejectedWarning);
        return;
      }
      
      Functions.WeekendWorkCancelationTask.Remote.CreateTaskByOrder(_obj).Show();
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanCancelOrder(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Проверить вид документа для определения доступности документа.
      object paramValue;
      var entityParams = ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params;
      var paramName = string.Format(Constants.WeekendWorkOrderDocument.DocKindParamName, _obj.Id);
      var isCancelationOrder = false;
      if (entityParams.TryGetValue(paramName, out paramValue))
        isCancelationOrder = (bool)paramValue;
      
      return !isCancelationOrder;
    }

  }

}