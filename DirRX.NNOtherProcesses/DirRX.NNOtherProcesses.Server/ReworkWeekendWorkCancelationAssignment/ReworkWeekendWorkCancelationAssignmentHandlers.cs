using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.ReworkWeekendWorkCancelationAssignment;

namespace DirRX.NNOtherProcesses
{
  partial class ReworkWeekendWorkCancelationAssignmentEmployeesEmployeePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> EmployeesEmployeeFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {      
      var assignment = _obj.ReworkWeekendWorkCancelationAssignment;
      var task = WeekendWorkCancelationTasks.As(assignment.Task);      
      var employeesInfo = task?.WeekendWorkOrder?.WeekendWorkStatement?.Employees;
      if(employeesInfo == null)
        return null;
      
      var employeesIds = employeesInfo.Select(emp => emp.Employee.Id);
      var selectedEmployeesIds = assignment.Employees.Where(x => x.Employee != null).Select(x => x.Employee.Id);
      employeesIds = employeesIds.Except(selectedEmployeesIds);
      return query.Where(emp => employeesIds.Contains(emp.Id));
    }
  }

}