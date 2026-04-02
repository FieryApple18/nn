using System;
using Sungero.Core;

namespace DirRX.NNCommons.Constants
{
  public static class DocumentsVisabilityLimitation
  {

    /// <summary>
    /// Признак изменения табличной части Состав записи.
    /// </summary>
    public const string EmployeesIsChanged = "EmployeesIsChanged";
    
    /// <summary>
    /// Признак изменения состояния записи.
    /// </summary>
    public const string StatusIsChanged = "StatusIsChanged";
    
    /// <summary>
    /// Признак изменения значения поля Обобщённая роль.
    /// </summary>
    public const string GeneralRoleIsChanged = "GeneralRoleIsChanged";
    
    /// <summary>
    /// Параметр для Ид сотрудников, которые были изначально в ограничении для их очистки в старой роли.
    /// </summary>
    public const string OriginalEmployeesIds = "OriginalEmployeesIds";

  }
}