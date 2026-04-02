using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkCancelationTask;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkCancelationTaskSharedHandlers
  {

    public virtual void WeekendWorkOrderChanged(DirRX.NNOtherProcesses.Shared.WeekendWorkCancelationTaskWeekendWorkOrderChangedEventArgs e)
    {
      if (!WeekendWorkOrderDocuments.Equals(e.NewValue, e.OldValue))
      {
        var attachment = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
        if (!WeekendWorkOrderDocuments.Equals(e.NewValue, attachment))
        {
          _obj.DocumentGroup.OfficialDocuments.Clear();
          if (e.NewValue != null)
          {
            _obj.DocumentGroup.OfficialDocuments.Add(e.NewValue);
            
            // Заполнить работников на основе приказа.
            Functions.WeekendWorkCancelationTask.FillEmployeesByOrder(_obj, e.NewValue);
            // Заполнить имя задачи.
            Functions.WeekendWorkCancelationTask.FillSubject(_obj);
          }
        }
      }
    }

  }
}