using System;
using Sungero.Core;

namespace DirRX.OverrideSolution.Module.SignPlatform.Constants
{
  public static class Module
  {

    /// <summary>
    /// Статусы ответа сервиса подписания.
    /// </summary>
    public static class Response
    {
      /// <summary>
      /// Заявление принято.
      /// </summary>
      public const string Accepted = "Accepted";
      
      /// <summary>
      /// Проверка документов.
      /// </summary>
      public const string DocumentsVerification = "DocumentsVerification";
      
      /// <summary>
      /// Ожидает подтверждения.
      /// </summary>
      public const string NeedConfirm = "NeedConfirm";
      
      /// <summary>
      /// Проверка заявления.
      /// </summary>
      public const string IssueVerification = "IssueVerification";
      
      /// <summary>
      /// В процессе выпуска.
      /// </summary>
      public const string InProgress = "InProgress";
      
      /// <summary>
      /// Сертификат выпущен.
      /// </summary>
      public const string Success = "Success";
      
      /// <summary>
      /// Ошибка.
      /// </summary>
      public const string Error = "Error";
    }

  }
}