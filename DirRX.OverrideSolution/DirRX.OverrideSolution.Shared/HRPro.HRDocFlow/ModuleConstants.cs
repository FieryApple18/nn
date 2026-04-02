using System;
using Sungero.Core;

namespace DirRX.OverrideSolution.Module.HRDocFlow.Constants
{
  public static class Module
  {

    /// <summary>
    /// Комментарий для создания замещения по виртуальному сотруднику.
    /// </summary>
    public const string CreatedVirtualEmployeeDescription = "Не удалять! Замещение виртуального сотрудника. Служебная запись. Создана автоматически.";

    /// <summary>
    /// Ключи параметров сущностей.
    /// </summary>
    [Public]
    public static class ParamKeys
    {
      /// Название параметра для действия конкурентной работы.
      [Public]
      public const string CompetitionWorkActionsParamName = "Действие конкурентной работы";
    }

    /// <summary>
    /// Список имен объектов личного кабинета сотрудника.
    /// </summary>
    [Public]
    public static class ObjectCardTypeNames
    {
      /// <summary>
      /// Задание на доработку заявления.
      /// </summary>
      [Public]
      public const string NNStatementReworkAssignment = "INNStatementReworkAssignments";
    }

  }
}