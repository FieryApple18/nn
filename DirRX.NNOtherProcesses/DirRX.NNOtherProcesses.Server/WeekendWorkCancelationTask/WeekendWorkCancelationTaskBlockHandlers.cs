using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.NNOtherProcesses.WeekendWorkCancelationTask;

namespace DirRX.NNOtherProcesses.Server.WeekendWorkCancelationTaskBlocks
{
  partial class ReworkWWCancelationAssignmentBlockHandlers
  {

    public virtual void ReworkWWCancelationAssignmentBlockCompleteAssignment(DirRX.NNOtherProcesses.IReworkWeekendWorkCancelationAssignment assignment)
    {
      _obj.Employees.Clear();
      foreach(var employee in assignment.Employees)
      {
        var newEmployee = _obj.Employees.AddNew();
        newEmployee.Employee = employee.Employee;
      } 
    }

    public virtual void ReworkWWCancelationAssignmentBlockStartAssignment(DirRX.NNOtherProcesses.IReworkWeekendWorkCancelationAssignment assignment)
    {
      foreach(var employee in _obj.Employees)
      {
        var newEmployee = assignment.Employees.AddNew();
        newEmployee.Employee = OverrideSolution.Employees.As(employee.Employee);
      }            
    }
  }

}