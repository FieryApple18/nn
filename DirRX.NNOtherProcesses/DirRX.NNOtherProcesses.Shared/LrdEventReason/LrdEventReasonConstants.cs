using System;
using Sungero.Core;

namespace DirRX.NNOtherProcesses.Constants
{
  public static class LrdEventReason
  {

    /// <summary>
    /// Названия записей справочника по умолчанию.
    /// </summary>
    public static class DefaultNames
    {
      // Материальная поддержка.
      [Public]
      public const string MaterialSupport = "Материальная поддержка";
      
      // Компенсации по обстоятельствам.
      [Public]
      public const string Compensation = "Компенсации по обстоятельствам";
    }
    
    /// <summary>
    /// Коды записей справочника по умолчанию.
    /// </summary>
    public static class DefaultCodes
    {
      // Материальная поддержка.
      [Public]
      public const string MaterialSupport = "03";
      
      // Компенсации по обстоятельствам.
      [Public]
      public const string Compensation = "05";
    }

  }
}