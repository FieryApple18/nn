using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.CompanyQueueBase;

namespace DirRX.NNIntegration.Server
{
  partial class CompanyQueueBaseFunctions
  {
    
    /// <summary>
    /// Установить значения для периода действия записи очереди.
    /// </summary>
    /// <param name="startDateString">Дата начала действия записи.</param>
    /// <param name="endDateString">Дата окончания действия записи.</param>
    /// <returns>Ошибка.</returns>
    public string SetRecordDatePeriod(string startDateString, string endDateString)
    {
      DateTime? startDate = null;
      DateTime? endDate = null;
      
      // Валидация даты начала действия записи.
      if (!string.IsNullOrEmpty(startDateString))
      {
        startDate = NNIntegration.Functions.Module.ParseStringToDate(startDateString);
        if (startDate == null)
        {
          var error = DirRX.NNIntegration.CompanyQueueBases.Resources.RecordDatePeriodFormatErrorFormat(_obj.Info.Properties.RecordStartDate.LocalizedName, startDateString);
          Logger.DebugFormat("SetRecordDatePeriod. {0}.", error);
          return error;
        }
      }
      else
        startDate = _obj.ReceivedDate;
      
      // Валидация даты окончания действия записи.
      if (!string.IsNullOrEmpty(endDateString))
      {
        endDate = NNIntegration.Functions.Module.ParseStringToDate(endDateString);
        if (endDate == null)
        {
          var error = DirRX.NNIntegration.CompanyQueueBases.Resources.RecordDatePeriodFormatErrorFormat(_obj.Info.Properties.RecordEndDate.LocalizedName, endDateString);
          Logger.DebugFormat("SetRecordDatePeriod. {0}.", error);
          return error;
        }
      }
      
      if (startDate > endDate)
        return NNIntegration.CompanyQueueBases.Resources.RecordDatePeriodErrorText;
      
      _obj.RecordStartDate = startDate;
      _obj.RecordEndDate = endDate;
      
      return string.Empty;
    }
  }
}