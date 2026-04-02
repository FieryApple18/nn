using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNVacations.Structures.AcquiantanceVacScheduleReport
{

  /// <summary>
  /// Строка таблицы отчета.
  /// </summary>
  partial class TableLine
  {
    /// <summary>
    /// ID сессии формирования отчёта.
    /// </summary>
    public string ReportSessionId { get; set; }
    
    /// <summary>
    /// ФИО сотрудника.
    /// </summary>
    public string FullName { get; set; }
    
    /// <summary>
    /// Табельный номер сотрудника.
    /// </summary>
    public string PersonnelNumber { get; set; }
    
    /// <summary>
    /// Должность сотрудника.
    /// </summary>
    public string JobTitle { get; set; }
    
    /// <summary>
    /// НОР сотрудника.
    /// </summary>
    public string BusinessUnit { get; set; }
    
    /// <summary>
    /// Подразделение 1.
    /// </summary>
    public string FirstDepartment { get; set; }
    
    /// <summary>
    /// Подразделение 2.
    /// </summary>
    public string SecondDepartment { get; set; }
    
    /// <summary>
    /// Подразделение 3.
    /// </summary>
    public string ThirdDepartment { get; set; }
    
    /// <summary>
    /// Подразделение 4.
    /// </summary>
    public string FourthDepartment { get; set; }
    
    /// <summary>
    /// Статус ознакомления.
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Дата ознакомления.
    /// </summary>
    public string Date { get; set; }
  }

}