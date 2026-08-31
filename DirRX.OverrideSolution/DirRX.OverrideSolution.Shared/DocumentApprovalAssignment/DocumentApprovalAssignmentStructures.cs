using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Structures.HRDocFlow.DocumentApprovalAssignment
{

  /// <summary>
  /// Структура, содержащая информацию по подзадачи для запроса мнения.
  /// </summary>
  partial class OpinionTaskInfoStructure
  {
    /// <summary>
    /// Тема.
    /// </summary>
    public string Subject { get; set; }
    
    /// <summary>
    /// Копия.
    /// </summary>
    public IEnumerable<IEmployee> CopyTo { get; set; }
    
    /// <summary>
    /// Исполнители.
    /// </summary>
    public IEnumerable<IEmployee> Performers { get; set; }
  }

}