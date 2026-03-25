using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Structures.Module
{

  /// <summary>
  /// Результат поиска сотрудника.
  /// </summary>
  [Public]
  partial class SearchEmployeeResult
  {

    /// <summary>
    /// Сотрудник.
    /// </summary>
    public IEmployee Employee { get; set; }

    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string ErrorMessage { get; set; }

  }

}