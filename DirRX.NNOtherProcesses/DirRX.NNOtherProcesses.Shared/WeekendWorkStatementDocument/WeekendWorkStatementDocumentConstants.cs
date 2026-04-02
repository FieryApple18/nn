using System;
using Sungero.Core;

namespace DirRX.NNOtherProcesses.Constants
{
  public static class WeekendWorkStatementDocument
  {
    
    /// HEX цвета RGB хинта с ошибкой валидации.
    public const string ErrorHintColorHEX = "FFFAC6B6";
    
    /// <summary>
    /// Наименование параметра для кэширования записией сотрудников, по которым не проходит валидация.
    /// </summary>
    public const string EmployeeErrorRecordsIdsKey = "EmployeeErrorRecordsIds";
    
    /// <summary>
    /// Тег для вставки табличной части в тело документа.
    /// </summary>
    public const string InsertEmployeesInfoTag = "Table";

    /// <summary>
    /// Наименование параметра для вида документа.
    /// </summary>
    public const string DocKindParamName = "DocKind:{0}";
    
    #region Диалог заполнения сотрудников в заявке ВД/ПД/СР.
    
    // Высота контрола заполнения сотрудников.
    [Sungero.Core.Public]
    public const int EmployeesTextRowsCount = 3;
    
    // Максимальное число сотрудников в заявке.
    [Sungero.Core.Public]
    public const int MaxEmployeesCount = 100;
    
    #endregion

  }
}