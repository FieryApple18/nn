using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkCancelationTask;

namespace DirRX.NNOtherProcesses.Server
{
  partial class WeekendWorkCancelationTaskFunctions
  {

    /// <summary>
    /// Создать задачу "Оформление отмены работы в ВД/ПД/СР" на основе приказа.
    /// </summary>
    /// <returns>Задача.</returns>
    [Remote]
    public static IWeekendWorkCancelationTask CreateTaskByOrder(IWeekendWorkOrderDocument order)
    {
      var cancelationTask = WeekendWorkCancelationTasks.Create();
      cancelationTask.WeekendWorkOrder = order;
      return cancelationTask;
    }

  }
}