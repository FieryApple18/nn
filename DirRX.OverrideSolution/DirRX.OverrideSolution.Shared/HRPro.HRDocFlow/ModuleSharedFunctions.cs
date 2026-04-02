using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.HRDocFlow.Shared
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Проверить доступность базовых результатов выполнения.
    /// </summary>
    /// <param name="isCompetitionWorkOld">Признак конкурентной работы (устар.)</param>
    /// <param name="performer">Исполнитель.</param>
    /// <param name="assignment">Задание.</param>
    /// <returns>Признак доступности базовых результатов выполнения.</returns>
    /// <remarks>Признак isCompetitionWorkOld оставлен для совместимости для успешного завершения стартованных заданий.</remarks>
    [Public]
    public bool CheckBaseExecutionResults(bool? isCompetitionWorkOld, IEmployee performer, Sungero.Workflow.IAssignment assignment)
    {
      // Старая логика
      if (isCompetitionWorkOld.HasValue)
        return isCompetitionWorkOld.Value != true || performer?.IsVirtualDirRX != true;
      
      // Новая логика
      if (assignment?.IsCompetitive == true)
        return Sungero.Docflow.PublicFunctions.Module.IsWorkStarted(assignment);
      
      return true;
    }

    /// <summary>
    /// Проверить доступность действий конкурентной работы.
    /// </summary>
    /// <param name="isCompetitionWork">Признак конкурентной работы.</param>
    /// <param name="performer">Исполнитель.</param>
    /// <param name="status">Статус.</param>
    /// <param name="isTakeToWork">Признак действия "Взять в работу" в рамках конкурентной работы.</param>
    /// <returns>Признак доступности действия конкурентной работы.</returns>
    [Public]
    public bool CheckCompetitiveWorkActions(bool? isCompetitionWork, IEmployee performer, Enumeration? status, bool isTakeToWork)
    {
      var isActive = isCompetitionWork == true && status == Sungero.Workflow.AssignmentBase.Status.InProcess;
      var isVirtualPerformer = performer.IsVirtualDirRX == true;
      
      return isActive && isVirtualPerformer == isTakeToWork;
    }

  }
}