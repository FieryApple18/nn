using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations.Server
{
  partial class VacationScheduleAcquaintanceListsFolderHandlers
  {
    /// <summary>
    /// Фильтрация индивидуальных и групповых листов ознакомления с графиком отпусков.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <returns>Запрос с фильтрами.</returns>
    public virtual IQueryable<DirRX.OverrideSolution.IVacationScheduleDocument> VacationScheduleAcquaintanceListsDataQuery(IQueryable<DirRX.OverrideSolution.IVacationScheduleDocument> query)
    {
      var groupScheduleAcquaintanceListKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNVacations.PublicConstants.Module.DocumentKind.GroupAcquiantanceVacScheduleListKind);
      var individualScheduleAcquaintanceListKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNVacations.PublicConstants.Module.DocumentKind.IndividualAcquiantanceVacScheduleListKind);
      
      query = query.Where(d => DocumentKinds.Equals(d.DocumentKind, individualScheduleAcquaintanceListKind) || DocumentKinds.Equals(d.DocumentKind, groupScheduleAcquaintanceListKind));
      
      if (_filter == null)
        return query;
      
      // Лист согласования.
      if ((_filter.IndividualList || _filter.GroupList) &&
          !(_filter.IndividualList && _filter.GroupList))
      {
        query = query.Where(d => _filter.IndividualList && DocumentKinds.Equals(d.DocumentKind, individualScheduleAcquaintanceListKind) ||
                            _filter.GroupList && DocumentKinds.Equals(d.DocumentKind, groupScheduleAcquaintanceListKind));
      }
      
      if (_filter.BusinessUnit != null)
        query = query.Where(d => BusinessUnits.Equals(d.BusinessUnit, _filter.BusinessUnit));
      
      if (_filter.Department != null)
        query = query.Where(d => Departments.Equals(d.Department, _filter.Department));
      
      if (_filter.Employee != null)
        query = query.Where(d => Employees.Equals(d.EmployeeDirRX, _filter.Employee));
      
      // Фильтрация по году графика отпусков.
      var periodBegin = Calendar.BeginningOfYear(Calendar.UserToday);
      var periodEnd = Calendar.EndOfYear(Calendar.UserToday);
      if (_filter.PreviousYear)
      {
        periodBegin = Calendar.PreviousYear(Calendar.UserToday);
        periodEnd = Calendar.PreviousYear(Calendar.UserToday);
      }
      if (_filter.NextYear)
      {
        periodBegin = Calendar.NextYear(Calendar.UserToday);
        periodEnd = Calendar.NextYear(Calendar.UserToday);
      }
      if (_filter.Period)
      {
        periodBegin = _filter.DateRangeFrom ?? Calendar.SqlMinValue;
        periodEnd = _filter.DateRangeTo ?? Calendar.SqlMaxValue;
      }
      var serverPeriodBegin = Calendar.SqlMinValue == periodBegin ? periodBegin : Sungero.Docflow.PublicFunctions.Module.Remote.GetTenantDateTimeFromUserDay(periodBegin);
      var serverPeriodEnd = Calendar.SqlMaxValue == periodEnd ? periodEnd : periodEnd.EndOfDay().FromUserTime();
      var clientPeriodEnd = Calendar.SqlMaxValue != periodEnd ? periodEnd.AddDays(1) : Calendar.SqlMaxValue;
      query = query.Where(d => (d.Year.Between(serverPeriodBegin, serverPeriodEnd) ||
                                d.Year == periodBegin) && d.Year != clientPeriodEnd);
      
      // Вариант ознакомления.
      if ((_filter.Paper || _filter.Electronic) &&
          !(_filter.Paper && _filter.Electronic))
      {
        query = query.Where(d => _filter.Paper && d.AcquiantanceOptionDirRX == DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.InPaper ||
                            _filter.Electronic && d.AcquiantanceOptionDirRX == DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.Electronic);
      }
      
      // Согласие на КЭДО.
      if ((_filter.Signed || _filter.NotSigned) &&
          !(_filter.Signed && _filter.NotSigned))
      {
        query = query.Where(d => _filter.Signed && d.ConsentDirRX.GetValueOrDefault() ||
                            _filter.NotSigned && !d.ConsentDirRX.GetValueOrDefault());
      }
      
      // Состояние.
      if ((_filter.Draft || _filter.Obsolete || _filter.Active) &&
          !(_filter.Draft && _filter.Obsolete && _filter.Active))
        query = query.Where(d => _filter.Draft && d.LifeCycleState == HRPro.HRProSolution.OfficialDocument.LifeCycleState.Draft ||
                            _filter.Obsolete && d.LifeCycleState == HRPro.HRProSolution.OfficialDocument.LifeCycleState.Obsolete  ||
                            _filter.Active && d.LifeCycleState == HRPro.HRProSolution.OfficialDocument.LifeCycleState.Active);
      
      return query;
    }
  }

  partial class NNVacationsHandlers
  {
  }
}