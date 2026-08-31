using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNOtherProcesses.Server
{
  partial class AdditionalAgreementsFolderHandlers
  {

    public virtual IQueryable<DirRX.OverrideSolution.IDocRelatedToWork> AdditionalAgreementsDataQuery(IQueryable<DirRX.OverrideSolution.IDocRelatedToWork> query)
    {
      var additionalAgreementsDocKind = DirRX.OverrideSolution.DocumentKinds.GetAll().Where(x => x.Name.Equals(HRPro.HRDocFlow.Resources.AdditionalAgreement)).FirstOrDefault();
      query = query.Where(x => Equals(x.DocumentKind, additionalAgreementsDocKind));
      
      if (_filter == null)
        return query;

      // Фильтр по периоду.
      var periodBegin = Calendar.SqlMinValue;
      var periodEnd   = Calendar.SqlMaxValue;
      
      if (_filter.LastWeek == true)
        periodBegin = Calendar.UserToday.AddDays(-7);
      
      if (_filter.LastMonth == true)
        periodBegin = Calendar.UserToday.AddDays(-30);
      
      if (_filter.Last90Days == true)
        periodBegin = Calendar.UserToday.AddDays(-90);
      
      if (_filter.ManualPeriod == true)
      {
        periodBegin = _filter.DateRangeFrom ?? Calendar.SqlMinValue;
        periodEnd = _filter.DateRangeTo ?? Calendar.SqlMaxValue;
      }
      
      query = query.Where(m => !m.Created.HasValue || m.Created.Between(periodBegin, periodEnd));
      
      // Фильтр по состоянию.
      if ((_filter.Draft || _filter.Obsolete || _filter.Active) &&
          !(_filter.Draft && _filter.Obsolete && _filter.Active))
        query = query.Where(d => _filter.Draft && d.LifeCycleState == HRPro.HRProSolution.OfficialDocument.LifeCycleState.Draft ||
                            _filter.Obsolete && d.LifeCycleState == HRPro.HRProSolution.OfficialDocument.LifeCycleState.Obsolete  ||
                            _filter.Active && d.LifeCycleState == HRPro.HRProSolution.OfficialDocument.LifeCycleState.Active);

      // Фильтр по НОР
      if (_filter.BusinessUnit != null)
        query = query.Where(d => OverrideSolution.BusinessUnits.Equals(d.BusinessUnit, _filter.BusinessUnit));
      
      // Фильтр по подразделению.
      if (_filter.Department != null)
        query = query.Where(d => OverrideSolution.Departments.Equals(d.Department, _filter.Department));

      return query;
    }
  }

  partial class NNOtherProcessesHandlers
  {
  }
}