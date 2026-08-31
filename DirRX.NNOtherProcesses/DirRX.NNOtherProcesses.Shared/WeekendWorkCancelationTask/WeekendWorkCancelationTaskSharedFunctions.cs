using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkCancelationTask;

namespace DirRX.NNOtherProcesses.Shared
{
  partial class WeekendWorkCancelationTaskFunctions
  {
    /// <summary>
    /// Заполнить тему задачи.
    /// </summary>
    public void FillSubject()
    {
      var document = _obj.WeekendWorkOrder;
      if (document != null)
      {
        var subject = DirRX.NNOtherProcesses.WeekendWorkCancelationTasks.Resources.WeekendWorkCancelationTaskSubjectTextFormat(document.Name);
        _obj.Subject =  Sungero.Docflow.PublicFunctions.Module.CutText(subject, _obj.Info.Properties.Subject.Length);
      }
      else
        _obj.Subject = string.Empty;
    }
    
    /// <summary>
    /// Заполнить ТЧ работники на основании заявки для приказа ВД/ПД/СР.
    /// </summary>
    /// <param name="order">Приказ ВД/ПД/СР.</param>
    public void FillEmployeesByOrder(IWeekendWorkOrderDocument order)
    {
      var statement = order?.WeekendWorkStatement;
      if (statement == null)
        return;
      
      _obj.Employees.Clear();
      var activeEmployees = statement.Employees.Where(x => x.RejectedByOrder != true && x.RejectedByApproval != true);
      foreach(var employeeInfo in activeEmployees)
        _obj.Employees.AddNew().Employee = employeeInfo.Employee;
    }
  }
}
