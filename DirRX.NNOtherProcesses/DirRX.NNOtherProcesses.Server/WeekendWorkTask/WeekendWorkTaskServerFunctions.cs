using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkTask;

namespace DirRX.NNOtherProcesses.Server
{
  partial class WeekendWorkTaskFunctions
  {

    /// <summary>
    /// Создать задачу оформление работы в ВД/ПД/СР с вложенной заявкой.
    /// </summary>
    /// <param name="statement">Заявка.</param>
    /// <returns>Задача.</returns>
    [Remote]
    public static IWeekendWorkTask CreateTaksWithStatement(IWeekendWorkStatementDocument statement)
    {
      var weekendTask = WeekendWorkTasks.Create();
      weekendTask.DocumentGroup.OfficialDocuments.Add(statement);
      return weekendTask;
    }

  }
}