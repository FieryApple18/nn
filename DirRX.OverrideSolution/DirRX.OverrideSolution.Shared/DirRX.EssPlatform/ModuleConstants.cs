using System;
using Sungero.Core;

namespace DirRX.OverrideSolution.Module.EssPlatform.Constants
{
  public static class Module
  {

    /// <summary>
    /// Допустимое количество попыток обновить данные сотрудника. После превышения пишем ошибку.
    /// </summary>
    [Public]
    public const int SynchronizeEmployee = 20;

    /// <summary>
    /// Константы с параметрами для запроса в Message Broker.
    /// </summary>
    public static class MessageBrokerRequestConstants
    {
      /// <summary>
      /// Тип учётных данных.
      /// </summary>
      public const string CredentialType = "PHONE";
      
      /// <summary>
      /// Тип доставки.
      /// </summary>
      public const string DeliveryProxy = "Sms";
    }

    /// <summary>
    /// Идентификаторы ролей.
    /// </summary>
    [Public]
    public static class RolesGuid
    {
      /// <summary>
      /// Видимость плитки в ЛК "Отпуска".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileMyVacations = Guid.Parse("6AE7E206-0EEB-4CA9-8FFC-4AC289288FD6");
      
      /// <summary>
      /// Видимость плитки в ЛК "Ежегодный отпуск".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileGetAnnualVacation = Guid.Parse("7D154A6E-50AF-41BB-98EA-0DC916571044");
      
      /// <summary>
      /// Видимость плитки в ЛК "Отпуск без сохранения заработной платы".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileGetVacationWithoutPay = Guid.Parse("834A6129-6BFC-43FF-AD78-1BA9DECD688A");
      
      /// <summary>
      /// Видимость плитки в ЛК "Остальные отпуска".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileGetOtherVacation = Guid.Parse("0CFE5645-660F-4836-8F2A-AD84E96C5517");
      
      /// <summary>
      /// Видимость плитки в ЛК "Перенос отпуска".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileNNShiftVacationByStatement = Guid.Parse("BB856C9E-AB90-4515-9709-1464F39EB18F");
      
      /// <summary>
      /// Видимость плитки в ЛК "Компенсация отпуска".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileCompensationVacation = Guid.Parse("7DE7660F-6943-42F2-8007-E8F772F9CA16");
      
      /// <summary>
      /// Видимость плитки в ЛК "Отзыв сотрудника из отпуска".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileNNRecallVacationEmployee = Guid.Parse("CE762776-9D83-44B1-BE6C-16DF02FFABED");
      
      /// <summary>
      /// Видимость плитки в ЛК "Прерывание отпуска".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileNNSelfRecallVacationEmployee = Guid.Parse("0E49B038-A6DA-43F3-B15C-9720A1874550");
      
      /// <summary>
      /// Видимость плитки в ЛК "Заявления".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileStatements = Guid.Parse("B2143D93-8D82-4516-AEA5-80B082AB85DE");
      
      /// <summary>
      /// Видимость плитки в ЛК "Заявка на работу в выходной/праздничный день".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileWeekendWork = Guid.Parse("3BFCBE2B-2463-4737-A849-B815D6597C7F");
      
      /// <summary>
      /// Видимость плитки в ЛК "Заявка на работу в сверхурочное время".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileOvertimeWork = Guid.Parse("7B70AD53-B7A1-487A-9662-45358963B4B7");
      
      /// <summary>
      /// Видимость плитки в ЛК "Заявление на выплату по ЛНА".
      /// </summary>
      [Public]
      public static readonly Guid ShowTileGetLrdPayment = Guid.Parse("7F6A42E1-F1C1-42A4-82B5-1F99BE1C34C4");
    }

  }
}