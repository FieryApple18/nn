using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.OutgoingMessage;

namespace DirRX.NNIntegration
{
  partial class OutgoingMessageFilteringServerHandler<T>
  {
    public virtual IQueryable<DirRX.NNIntegration.IExternalSystemFlow> SystemFlowFiltering(IQueryable<DirRX.NNIntegration.IExternalSystemFlow> query, Sungero.Domain.FilteringEventArgs e)
    {
      return query.Where(x => x.Direction == ExternalSystemFlow.Direction.Outgoing);
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
      
      query = query.Where(m => !m.SendingDate.HasValue || m.SendingDate.Between(periodBegin, periodEnd));
      
      // Фильтр по потоку.
      if(_filter.SystemFlow != null)
        query = query.Where(m => m.ExternalSystemFlow.Equals(_filter.SystemFlow));
      
      // Фильтр по статусу сообщения.
      if (_filter.Canceled || _filter.Canceled || _filter.Error || _filter.Recieved || _filter.Sending)
        query = query.Where(m => ((_filter.Created && m.StatusMessage == OutgoingMessage.StatusMessage.Created) ||
                                  (_filter.Canceled && m.StatusMessage == OutgoingMessage.StatusMessage.Canceled) ||
                                  (_filter.Error && m.StatusMessage == OutgoingMessage.StatusMessage.Error) ||
                                  (_filter.Recieved && m.StatusMessage == OutgoingMessage.StatusMessage.Received) ||
                                  (_filter.Sending && m.StatusMessage == OutgoingMessage.StatusMessage.Sended)));
      
      return query;
    }
  }

  partial class OutgoingMessageServerHandlers
  {
    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (string.IsNullOrEmpty(_obj.Name) || _obj.State.Properties.ProcessingDate.OriginalValue != _obj.ProcessingDate)
        _obj.Name = NNIntegration.Resources.OutgoingMessageNameFormat(_obj.Id.ToString(), _obj.ProcessingDate != null ? _obj.ProcessingDate.Value.ToShortDateString() : string.Empty);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.StatusMessage = NNIntegration.OutgoingMessage.StatusMessage.Created;
      _obj.GuidMessage = Guid.NewGuid().ToString();
      _obj.ExistsMessageText = false;
    }
  }
}