using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentMPGroup;

namespace DirRX.NNOtherProcesses.Server
{
  partial class LrdPaymentMPGroupFunctions
  {

    /// <summary>
    /// Получить список дублирующихся записей.
    /// </summary>
    /// <returns>Список дублей.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<ILrdPaymentMPGroup> GetDuplicates()
    {
      return LrdPaymentMPGroups.GetAll().Where(x => x.Id != _obj.Id && x.Code == _obj.Code);
    }

  }
}