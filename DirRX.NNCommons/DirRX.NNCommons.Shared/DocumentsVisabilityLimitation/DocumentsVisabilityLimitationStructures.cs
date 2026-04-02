using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNCommons.Structures.DocumentsVisabilityLimitation
{

  /// <summary>
  /// Данные по виду докумнета.
  /// </summary>
  [Public]
  partial class DocumentKindsInfo
  {
    /// <summary>
    /// ИД вида документа.
    /// </summary>
    public long DocumentKindId { get; set; }
    
    /// <summary>
    /// Признак "Топ 10/100".
    /// </summary>
    public bool isTop { get; set; }
  }

}