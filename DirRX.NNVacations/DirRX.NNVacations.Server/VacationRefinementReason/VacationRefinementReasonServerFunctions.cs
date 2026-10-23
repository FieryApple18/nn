using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationRefinementReason;

namespace DirRX.NNVacations.Server
{
  partial class VacationRefinementReasonFunctions
  {
    /// <summary>
    /// Проверить на дубли.
    /// </summary>
    public virtual bool CheackDuplicateVacationRefinementReason()
    {
      return VacationRefinementReasons.GetAll(v => !Equals(v, _obj) &&
                                                                 _obj.Name == v.Name &&
                                                                 _obj.Code == v.Code &&
                                                                 v.Status == NNVacations.VacationRefinementReason.Status.Active).Any();
    }
  }
}