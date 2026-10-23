using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationsApprovalPeriod;

namespace DirRX.NNVacations.Server
{
  partial class VacationsApprovalPeriodFunctions
  {
    
    /// <summary>
    /// Получить срок согласования отпуска.
    /// </summary>
    /// <param name="businessUnitId">ИД нашей организации.</param>
    /// <param name="documentKindId">ИД вида документа.</param>
    /// <param name="vacationType">Тип отпуска.</param>
    /// <param name="paymentCompleted">Отпуск оплачен.</param>
    /// <param name="top">ТОП.</param>
    /// <returns>Срок согласования отпуска.</returns>
    [Public]
    public static IVacationsApprovalPeriod GetVacationsApprovalPeriod(long businessUnitId, long documentKindId, Nullable<Enumeration> vacationType, bool paymentCompleted, bool top)
    {
      return VacationsApprovalPeriods.GetAll(x => x.BusinessUnit.Id == businessUnitId && x.DocumentKind.Id == documentKindId &&
                                             x.VacationType == vacationType && x.PaymentCompleted == paymentCompleted
                                             && x.Top == top).FirstOrDefault();
    }

    /// <summary>
    /// Получить список дублирующихся записей.
    /// </summary>
    /// <returns>Список дублей.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<IVacationsApprovalPeriod> GetDuplicates()
    {
      return VacationsApprovalPeriods.GetAll()
        .Where(x => x.Id != _obj.Id &&
               OverrideSolution.BusinessUnits.Equals(x.BusinessUnit, _obj.BusinessUnit) &&
               OverrideSolution.DocumentKinds.Equals(x.DocumentKind, _obj.DocumentKind) &&
               x.VacationType == _obj.VacationType &&
               x.PaymentCompleted == _obj.PaymentCompleted &&
               x.Top == _obj.Top);
    }

  }
}