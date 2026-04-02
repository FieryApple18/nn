using System;
using Sungero.Core;

namespace DirRX.NNOtherProcesses.Constants
{
  public static class LrdPaymentPurpose
  {
    /// <summary>
    /// Названия записей справочника по умолчанию.
    /// </summary>
    public static class DefaultNames
    {
      // Смерть члена семьи.
      [Public]
      public const string FamilyMemberDeath = "Смерть члена семьи";
      
      // Пожар, кража, авария, стихийное бедствие, иные ЧС.
      [Public]
      public const string Emergency = "Пожар, кража, авария, стихийное бедствие, иные ЧС";
      
      // Материальная помощь работнику, имеющему ребенка-инвалида в возрасте до 18 лет.
      [Public]
      public const string DisabledChild = "Материальная помощь работнику, имеющему ребенка-инвалида в возрасте до 18 лет";
      
      // Материальная помощь работнику-инвалиду.
      [Public]
      public const string DisabledEmployee = "Материальная помощь работнику-инвалиду";
      
      // Иное.
      [Public]
      public const string Other = "Иное";
    }
    
    /// <summary>
    /// Коды записей справочника по умолчанию.
    /// </summary>
    public static class DefaultCodes
    {
      // Смерть члена семьи.
      [Public]
      public const string FamilyMemberDeath = "01";
      
      // Пожар, кража, авария, стихийное бедствие, иные ЧС.
      [Public]
      public const string Emergency = "02";
      
      // Материальная помощь работнику, имеющему ребенка-инвалида в возрасте до 18 лет.
      [Public]
      public const string DisabledChild = "02";
      
      // Материальная помощь работнику-инвалиду.
      [Public]
      public const string DisabledEmployee = "01";
      
      // Иное.
      [Public]
      public const string Other = "04";
    }
  }
}