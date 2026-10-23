using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.CompanyQueueBase;

namespace DirRX.NNIntegration.Shared
{
  partial class CompanyQueueBaseFunctions
  {

    /// <summary>
    /// Заполнить наименование записи очереди.
    /// </summary>
    public virtual void FillRecordName()
    {
      _obj.RecordName = _obj.Id.ToString();
    }
    
    /// <summary>
    /// Получение имени для записи очереди с учетом длины строки.
    /// </summary>
    /// <param name="id">ИД сущности.</param>
    /// <param name="name">Имя сущности.</param>
    /// <param name="date">Дата получения записи.</param>
    /// <returns>Имя записи очереди.</returns>
    /// <remarks>Если имя записи очереди превышает максимум, то режется название, ИД и дата остаются.</remarks>
    public virtual string GetRecordName(string id, string name, DateTime? date)
    {
      var receivedDate = date?.ToString(Constants.CompanyQueueBase.DateFormat);
      var recordName = string.Join(" ", id, name, receivedDate);
      
      if (recordName.Length > _obj.Info.Properties.RecordName.Length)
      {
        var count = recordName.Length - _obj.Info.Properties.RecordName.Length;
        recordName = string.Join(" ", id, name.Substring(0, name.Length - count), receivedDate);
      }
      
      return recordName;
    }

  }
}