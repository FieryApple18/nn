using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;
using DirRX.NNOtherProcesses.LrdPaymentTypeSetting;

namespace DirRX.NNOtherProcesses.Server
{
  partial class LrdPaymentTypeSettingFunctions
  {

    /// <summary>
    /// Получить настройку типа выплаты.
    /// </summary>
    /// <param name="paymentType">Тип выплаты.</param>
    /// <param name="businessUnit">Наша организация.</param>
    /// <returns>Настройка типа выплаты.</returns>
    public static ILrdPaymentTypeSetting GetPaymentTypeSetting(ILrdPaymentType paymentType, IBusinessUnit businessUnit)
    {
      return LrdPaymentTypeSettings.GetAll(x => LrdPaymentTypes.Equals(x.PaymentType, paymentType) &&
                                           BusinessUnits.Equals(x.BusinessUnit, businessUnit)).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить список дублирующихся записей.
    /// </summary>
    /// <returns>Список дублей.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<ILrdPaymentTypeSetting> GetDuplicates()
    {
      return LrdPaymentTypeSettings.GetAll()
        .Where(x => x.Id != _obj.Id && OverrideSolution.BusinessUnits.Equals(x.BusinessUnit, _obj.BusinessUnit) &&
               LrdPaymentTypes.Equals(x.PaymentType, _obj.PaymentType) &&
               LrdPaymentMPGroups.Equals(x.MPGroup, _obj.MPGroup));
    }
  }
}