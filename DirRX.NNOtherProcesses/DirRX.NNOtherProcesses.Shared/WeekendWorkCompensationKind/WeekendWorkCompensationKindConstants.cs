using System;
using Sungero.Core;

namespace DirRX.NNOtherProcesses.Constants
{
  /// <summary>
  /// Виды компенсации.
  /// </summary>
  public static class WeekendWorkCompensationKind
  {
    /// <summary>
    /// Названия записей справочника по умолчанию.
    /// </summary>
    public static class DefaultNames
    {
      // Двойная оплата.
      [Public]
      public const string IncreasedPay = "Двойная оплата";
      
      // Предоставление другого дня отдыха.
      [Public]
      public const string AnotherRestDay = "Предоставление другого дня отдыха";
      
      // Стандартная оплата.
      [Public]
      public const string StandardPay = "Стандартная оплата";
      
      // Оплата с первого часа в двойном размере.
      [Public]
      public const string DualPay = "Оплата с первого часа в двойном размере";
      
      // Предоставление дополнительного времени отдыха.
      [Public]
      public const string AdditionalRestTime = "Предоставление дополнительного времени отдыха";
    }
    
    /// <summary>
    /// Коды записей справочника по умолчанию.
    /// </summary>
    public static class DefaultCodes
    {
      // Повышенная оплата.
      [Public]
      public const string IncreasedPay = "07";
      
      // Предоставление другого дня отдыха.
      [Public]
      public const string AnotherRestDay = "08";
      
      // Стандартная оплата.
      [Public]
      public const string StandardPay = "01";
      
      // Оплата с первого часа в двойном размере.
      [Public]
      public const string DualPay = "02";
      
      // Предоставление дополнительного времени отдыха.
      [Public]
      public const string AdditionalRestTime = "03";
    }
  }
}