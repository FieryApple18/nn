using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Structures.HRDocFlow.ESignAssignment
{

  /// <summary>
  /// Структура для массового подписания.
  /// </summary>
  [Public]
  partial class MassApproveSignStruct
  {
    public List<long> SuccessIdList { get; set; }
    public List<long> ErrorIdList { get; set; }
  }

}