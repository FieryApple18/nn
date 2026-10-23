using System;
using Sungero.Core;

namespace DirRX.NNIntegration.Constants
{
  public static class EmployeeQueue
  {

    /// <summary>
    /// Коды признака НГД.
    /// </summary>
    public static class NGDCodes
    {
      /// <summary>
      /// Константа кода "ОБЩ".
      /// </summary>
      public const string General = "ОБЩ";
      
      /// <summary>
      /// Константа кода "НГД".
      /// </summary>
      public const string NGD = "НГД(СПЕЦ)";
    }
    
    /// <summary>
    /// Регулярное выражения для СНИЛС при получении данных из внешней системы. Пример: 123-456-789 00.
    /// </summary>
    public const string InilaPattern = @"^\d{3}-\d{3}-\d{3} \d{2}$";
    
    /// <summary>
    /// Коды пола персоны.
    /// </summary>
    public static class SexCodes
    {
      /// <summary>
      /// Константа кода для мужского пола.
      /// </summary>
      public const string Male = "М";
      
      /// <summary>
      /// Константа кода для женского пола.
      /// </summary>
      public const string Female = "Ж";
    }
    
    /// <summary>
    /// Коды виды занятости.
    /// </summary>
    [Public]
    public static class EmploymentTypeCodes
    {
      /// <summary>
      /// Константа кода основного места работы.
      /// </summary>
      [Public]
      public const string MainPlace = "1";
      
      /// <summary>
      /// Константа кода для внутреннего совместительства.
      /// </summary>
      [Public]
      public const string InternalConcurr = "2";
      
      /// <summary>
      /// Константа кода для внешнего совместительства.
      /// </summary>
      [Public]
      public const string ExternalConcurr = "3";
    }
    
    /// <summary>
    /// Коды вида документа, удостоверяющего личность.
    /// </summary>
    [Public]
    public static class IdentityDocumentKindCodes
    {
      /// <summary>
      /// Константа кода для паспорта гражданина РФ.
      /// </summary>
      [Public]
      public const string PassportRF = "PassportRF";
      
      /// <summary>
      /// Константа кода для иного документа, удостоверяющего личность.
      /// </summary>
      [Public]
      public const string OtherDocument = "OtherDocument";
      
      /// <summary>
      /// Константа кода для иностранного паспорта.
      /// </summary>
      [Public]
      public const string ForeignPassport = "ForeignPassport";
    }
    
    /// <summary>
    /// Коды категорий сотрудника.
    /// </summary>
    public static class CategoryCodes
    {
      /// <summary>
      /// Руководители.
      /// </summary>
      public const string Manager = "10";
      /// <summary>
      /// Руководители ЧТС.
      /// </summary>
      public const string ManagerCHTS = "11";
      /// <summary>
      /// Специалисты.
      /// </summary>
      public const string Specialist = "20";
      /// <summary>
      /// Специалисты ЧТС.
      /// </summary>
      public const string SpecialistCHTS = "21";
      /// <summary>
      /// Служащие.
      /// </summary>
      public const string Clerk = "30";
      /// <summary>
      /// Служащие ЧТС.
      /// </summary>
      public const string ClerkCHTS = "31";
      /// <summary>
      /// Рабочие.
      /// </summary>
      public const string Worker = "40";
      /// <summary>
      /// Рабочие ЧТС.
      /// </summary>
      public const string WorkerCHTS = "41";
    }
  }
}