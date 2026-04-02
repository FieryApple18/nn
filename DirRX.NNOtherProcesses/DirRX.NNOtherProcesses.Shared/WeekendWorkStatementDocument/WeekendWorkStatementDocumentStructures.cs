using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNOtherProcesses.Structures.WeekendWorkStatementDocument
{

  /// <summary>
  /// Результат валидации таблицы Сотрудники.
  /// </summary>
  partial class ValidateEmployeesTableResult
  {
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string Error { get; set; }
    
    /// <summary>
    /// Некорректные записи таблицы сотрудников.
    /// </summary>
    public List<IWeekendWorkStatementDocumentEmployees> ErrorRecords { get; set; }
  }

  /// <summary>
  /// Информация по сотруднику.
  /// </summary>
  partial class EmployeeInfo
  {
    /// <summary>
    /// Ид.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Имя.
    /// </summary>
    public string Name { get ; set; }
  }

}