using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.SignPlatform.Structures.Module
{

  /// <summary>
  /// Ошибка при выдаче сертификата.
  /// </summary>
  [Public]
  partial class CertificateIssueError
  {
    /// <summary>
    /// ИД сотрудников, у которых возникла ошибка.
    /// </summary>
    public List<long> EmployeeIds { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string IssueError { get; set; }
  }

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
    /// Связанное с ошибкой поле.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// Сообщение.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Детали.
    /// </summary>
    public List<DirRX.OverrideSolution.Module.SignPlatform.Structures.Module.HttpRequestErrorResult> Details { get; set; }

  }

}