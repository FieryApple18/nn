using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNCommons.Structures.CertificateIssueTask
{

  /// <summary>
  /// Структура с данными о результате http-запроса в случае ошибок.
  /// </summary>
  partial class HttpRequestErrorResult
  {
    /// <summary>
    ///  Код ошибки.
    /// </summary>
    public string Code { get; set; }
    
    /// <summary>
    /// Сообщение.
    /// </summary>
    public string Message { get; set; }
  }

}