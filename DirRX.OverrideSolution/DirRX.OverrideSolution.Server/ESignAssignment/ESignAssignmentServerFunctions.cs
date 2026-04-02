using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.ESignAssignment;
using DirRX.OverrideSolution.Structures.HRDocFlow.ESignAssignment;
using Sungero.Docflow;

namespace DirRX.OverrideSolution.Server
{
  partial class ESignAssignmentFunctions
  {

    /// <summary>
    /// Построить модель состояния сводки.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote]
    public StateView GetESignAssignmentSummary()
    {
      // TODO: Заменить на вызов коробочной функции после добавления возможности.
      return DirRX.OverrideSolution.Module.HRDocFlow.Functions.Module.GetAssignmentStateView(_obj.SummaryDirRX);
    }

    /// <summary>
    /// Получить список активных заданий на подписание.
    /// </summary>
    /// <param name="ids">ИД заданий.</param>
    /// <returns>Список заданий.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IESignAssignment> GetSignAssignments(List<long> ids)
    {
      return ESignAssignments.GetAll().Where(c => ids.Contains(c.Id));
    }

  }
}