using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.SignPlatform.Structures.Module
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