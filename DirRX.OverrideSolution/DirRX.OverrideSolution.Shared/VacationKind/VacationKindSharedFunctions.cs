using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationKind;

namespace DirRX.OverrideSolution.Shared
{
  partial class VacationKindFunctions
  {

    /// <summary>
    /// Заполнение Правила переноса типа отпуска (Рабочие).
    /// </summary>
    /// <param name="transfer">Значение переноса</param>
    public void SetTransferringRulesRSS(Enumeration? transfer)
    {
      if (transfer == OverrideSolution.VacationKind.TransferringRulesEmployeesDirRX.Fewer)
        _obj.TransferringRulesRSSDirRXDirRX = OverrideSolution.VacationKind.TransferringRulesRSSDirRXDirRX.Fewer;
      else if (transfer == OverrideSolution.VacationKind.TransferringRulesEmployeesDirRX.FewerOrMore)
        _obj.TransferringRulesRSSDirRXDirRX = OverrideSolution.VacationKind.TransferringRulesRSSDirRXDirRX.FewerOrMore;
      else if (transfer == OverrideSolution.VacationKind.TransferringRulesEmployeesDirRX.MatchCount)
        _obj.TransferringRulesRSSDirRXDirRX = OverrideSolution.VacationKind.TransferringRulesRSSDirRXDirRX.MatchCount;
      else if (transfer == OverrideSolution.VacationKind.TransferringRulesEmployeesDirRX.More)
        _obj.TransferringRulesRSSDirRXDirRX = OverrideSolution.VacationKind.TransferringRulesRSSDirRXDirRX.More;
      else if (transfer == null)
        _obj.TransferringRulesRSSDirRXDirRX = null;
    }

  }
}