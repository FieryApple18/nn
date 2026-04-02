using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkStatementDocument;
using DirRX.OverrideSolution;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkStatementDocumentEmployeesSharedCollectionHandlers
  {

    public virtual void EmployeesAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      _added.ChecksPassed = false;
      _added.RejectedByApproval = false;
    }
  }

  partial class WeekendWorkStatementDocumentEmployeesSharedHandlers
  {

    public virtual void EmployeesCompensationChanged(DirRX.NNOtherProcesses.Shared.WeekendWorkStatementDocumentEmployeesCompensationChangedEventArgs e)
    {
      if (e.NewValue == e.OldValue)
        return;
      if (e.NewValue?.NeedRestDate == false)
        _obj.RestDateFrom = null; 
    }

    public virtual void EmployeesRestDateFromChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue == e.OldValue)
        return;       
      if (_obj.Compensation?.NeedRestDate == false)
      {
        using (EntityEvents.Disable(WeekendWorkStatementDocuments.Info.Properties.Employees.Properties.RestDateFrom.Events.Changed))
          _obj.RestDateTo = _obj.RestDateFrom = null;
        return;
      }            
      _obj.RestDateTo = e.NewValue;
    }

    public virtual void EmployeesEmployeeChanged(DirRX.NNOtherProcesses.Shared.WeekendWorkStatementDocumentEmployeesEmployeeChangedEventArgs e)
    {
      _obj.PersonnelNumber = e.NewValue?.PersonnelNumber;
      _obj.Department = Departments.As(e.NewValue?.Department);
      _obj.JobTitle = JobTitles.As(e.NewValue?.JobTitle);
    }
  }

  partial class WeekendWorkStatementDocumentSharedHandlers
  {

    public virtual void WorkDateToChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue == null || e.NewValue == e.OldValue)
        return;
      foreach (var employee in _obj.Employees)
        employee.WorkDateTo = e.NewValue;
    }

    public virtual void WorkDateFromChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      if (e.NewValue == null || e.NewValue == e.OldValue)
        return;
      foreach (var employee in _obj.Employees)
        employee.WorkDateFrom = e.NewValue;
      if (!_obj.WorkDateTo.HasValue || _obj.WorkDateTo.Value.Date != e.NewValue.Value.Date)
        _obj.WorkDateTo = e.NewValue.Value.Date.AddHours(18);
    }

    public virtual void EmployeesChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
      _obj.IsGroup = _obj.Employees.Count > 1;
      Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
    }

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      // Обновить параметр для вида документа при его изменении.
      if (!DocumentKinds.Equals(e.NewValue, e.OldValue) && e.NewValue != null)
      {
        var isOvertimeWork = Functions.WeekendWorkStatementDocument.Remote.IsOvertimeWork(e.NewValue);
        var paramName = string.Format(Constants.WeekendWorkStatementDocument.DocKindParamName, _obj.Id);
        e.Params.AddOrUpdate(paramName, isOvertimeWork);
        
        if (isOvertimeWork && _obj.Reason != null)
          _obj.Reason = null;
        
        // Очистить значение поля Компенсация, т.к.разным видам документов соответствуют разные компенсации.
        foreach (var employee in _obj.Employees)
          employee.Compensation = null;
      }
      
      base.DocumentKindChanged(e);
    }

  }
}