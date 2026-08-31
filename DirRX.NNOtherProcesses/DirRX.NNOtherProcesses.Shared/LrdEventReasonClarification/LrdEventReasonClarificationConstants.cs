using System;
using Sungero.Core;

namespace DirRX.NNOtherProcesses.Constants
{
  public static class LrdEventReasonClarification
  {
    /// <summary>
    /// Названия записей справочника по умолчанию.
    /// </summary>
    public static class DefaultNames
    {
      // Рождение ребенка.
      [Public]
      public const string ChildBirth = "Рождение ребенка";
      
      // Смерть члена семьи работника.
      [Public]
      public const string FamilyMemberDeath = "Смерть члена семьи работника";
      
      // Материальная поддержка работникам.
      [Public]
      public const string EmployeesMaterialSupport = "Материальная поддержка работникам";
      
      // Единовременная выплата в связи с трудной жизненной ситуацией.
      [Public]
      public const string LifeOneTimePayment = "Единовременная выплата в связи с трудной жизненной ситуацией";
      
      // Единовременная материальная помощь по уходу на пенсию.
      [Public]
      public const string RetirementOneTimePayment = "Единовременная материальная помощь по уходу на пенсию";
      
      // Ребенок инвалид до 18 лет.
      [Public]
      public const string DisabledChild = "Ребенок инвалид до 18 лет";
      
      // В связи с инвалидностью работника.
      [Public]
      public const string DisabledEmployee = "В связи с инвалидностью работника";
      
      // Материальная помощь. Увечье.
      [Public]
      public const string MutilationMaterialSupport = "Материальная помощь. Увечье";
      
      // Материальная поддержка работникам (в/о 3324).
      [Public]
      public const string EmployeeMaterialSupport3324 = "Материальная поддержка работникам (в/о 3324)";
    }
    
    /// <summary>
    /// Коды записей справочника по умолчанию.
    /// </summary>
    public static class DefaultCodes
    {
      // Рождение ребенка.
      [Public]
      public const string ChildBirth = "02";
      
      // Смерть члена семьи работника.
      [Public]
      public const string FamilyMemberDeath = "03";
      
      // Материальная поддержка работникам.
      [Public]
      public const string EmployeesMaterialSupport = "08";
      
      // Единовременная выплата в связи с трудной жизненной ситуацией.
      [Public]
      public const string LifeOneTimePayment = "04";
      
      // Единовременная материальная помощь по уходу на пенсию.
      [Public]
      public const string RetirementOneTimePayment = "05";
      
      // Ребенок инвалид до 18 лет.
      [Public]
      public const string DisabledChild = "17";
      
      // В связи с инвалидностью работника.
      [Public]
      public const string DisabledEmployee = "16";
      
      // Материальная помощь. Увечье.
      [Public]
      public const string MutilationMaterialSupport = "05";
      
      // Материальная поддержка работникам (в/о 3324).
      [Public]
      public const string EmployeeMaterialSupport3324 = "19";
    }
  }
}