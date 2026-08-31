using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkTask;

namespace DirRX.NNOtherProcesses.Shared
{
  partial class WeekendWorkTaskFunctions
  {

    /// <summary>
    /// Заполнить тему задачи.
    /// </summary>
    public void FillSubject()
    {
      if (_obj.DocumentGroup.OfficialDocuments.Any())
        _obj.Subject = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault().Name;
      else
        _obj.Subject = string.Empty;
    }

  }
}