using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkCancelationTask;

namespace DirRX.NNOtherProcesses
{

  partial class WeekendWorkCancelationTaskEmployeesEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> EmployeesEmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var employeesInfo = _obj.WeekendWorkCancelationTask?.WeekendWorkOrder?.WeekendWorkStatement.Employees;
      if (employeesInfo == null)
        return null;
      
      List<long> employeesIds = employeesInfo.Where(x => x.RejectedByOrder != true).Select(x => x.Employee.Id).ToList();
      List<long> selectedEmployeesIds = _obj.WeekendWorkCancelationTask.Employees.Where(x => x.Employee != null).Select(x => x.Employee.Id).ToList();
      employeesIds = employeesIds.Except(selectedEmployeesIds).ToList();
      
      return query.Where(x => employeesIds.Contains(x.Id));
    }
  }

}