using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.IncomingMessage;

namespace DirRX.NNIntegration
{
  partial class IncomingMessageFilteringServerHandler<T>
  {
    public virtual IQueryable<DirRX.NNIntegration.IExternalSystemFlow> SystemFlowFiltering(IQueryable<DirRX.NNIntegration.IExternalSystemFlow> query, Sungero.Domain.FilteringEventArgs e)
    {
      return query.Where(x => x.Direction == ExternalSystemFlow.Direction.Incoming);
    }
    
    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      // Проверка того, что панель фильтрации включена.
      if (_filter == null)
        return query;
      
      // Фильтр по периоду.
      var periodBegin = Calendar.SqlMinValue;
      var periodEnd   = Calendar.SqlMaxValue;
      
      if (_filter.Today == true)
        periodBegin = Calendar.UserToday;
      
      if (_filter.LastWeek == true)
        periodBegin = Calendar.UserToday.AddDays(-7);
      
      if (_filter.LastMonth == true)
        periodBegin = Calendar.UserToday.AddDays(-30);
      
      if (_filter.ManualPeriod == true)
      {
        periodBegin = _filter.DateRangeFrom ?? Calendar.SqlMinValue;
        periodEnd = _filter.DateRangeTo ?? Calendar.SqlMaxValue;
      }
      
      query = query.Where(m => !m.ReceivedDate.HasValue || m.ReceivedDate.Between(periodBegin, periodEnd));
      
      // Фильтр по потоку.
      if(_filter.SystemFlow != null)
        query = query.Where(m => m.ExternalSystemFlow.Equals(_filter.SystemFlow));
      
      // Фильтр по статусу сообщения.
      if (_filter.NeedProcessing || _filter.Error || _filter.Processing)
        query = query.Where(m => ((_filter.NeedProcessing && m.StatusMessage == IncomingMessage.StatusMessage.NeedProcessing) ||
                                  (_filter.Error && m.StatusMessage == IncomingMessage.StatusMessage.Error) ||
                                  (_filter.Processing && m.StatusMessage ==  IncomingMessage.StatusMessage.Processed)));
      
      return query;
    }
  }

  partial class IncomingMessageServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (string.IsNullOrEmpty(_obj.Name) || _obj.State.Properties.ReceivedDate.OriginalValue != _obj.ReceivedDate)
        _obj.Name = NNIntegration.Resources.IncomingMessageNameFormat(_obj.Id.ToString(), _obj.ReceivedDate.Value.ToShortDateString());
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.StatusMessage = NNIntegration.IncomingMessage.StatusMessage.NeedProcessing;
      _obj.ReceivedDate = Calendar.Now;
    }
  }
}