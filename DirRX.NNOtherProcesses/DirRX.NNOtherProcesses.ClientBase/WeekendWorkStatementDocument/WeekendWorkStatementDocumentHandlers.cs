using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkStatementDocument;

namespace DirRX.NNOtherProcesses
{
  partial class WeekendWorkStatementDocumentEmployeesClientHandlers
  {

    public virtual void EmployeesRestDateFromValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      if (e.NewValue == e.OldValue)
        return;
      if (_obj.Compensation?.NeedRestDate == false)
      {
        e.AddInformation(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.RestDateFromIsEnableMessageFormat(_obj.Compensation));
        e.NewValue = null;
      }        
    }
    
    public virtual void EmployeesCompensationValueInput(DirRX.NNOtherProcesses.Client.WeekendWorkStatementDocumentEmployeesCompensationValueInputEventArgs e)
    {
      if (e.NewValue == e.OldValue)
        return;
      if (e.NewValue?.NeedRestDate == false && _obj.RestDateFrom != null)
        e.AddInformation(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.RestDateFromIsEnableMessageFormat(e.NewValue));
    }  
  }


  partial class WeekendWorkStatementDocumentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      // Обновить выделение цветом данных по ошибкам в табличной части заявки ВД/ПД/СР.
      Functions.WeekendWorkStatementDocument.RefreshHighlightingEmployeesDataErrors(_obj);
    }

    public virtual void WorkDateToValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWorkDocKind(_obj);
      if (e.NewValue == null || e.NewValue == e.OldValue || !_obj.WorkDateFrom.HasValue)
        return;
      if (_obj.WorkDateFrom.Value.Date != e.NewValue.Value.Date)
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WorkDateValidationError);
      if (isOvertimeWork != true && _obj.WorkDateFrom.Value.TimeOfDay >= e.NewValue.Value.TimeOfDay)
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WorkTimeValidationError);
    }
    
    public virtual void WorkDateFromValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.IsOvertimeWorkDocKind(_obj);
      if (e.NewValue == null || e.NewValue == e.OldValue)
        return;
      if (e.NewValue.Value.Date < Calendar.Today)
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WorkDateCurrentDayError);
      if (isOvertimeWork != true && _obj.WorkDateTo.HasValue && e.NewValue.Value.TimeOfDay >= _obj.WorkDateTo.Value.TimeOfDay)
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.WorkTimeValidationError);
    }
    
    public override void TopDirRXValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      base.TopDirRXValueInput(e);
      if (e.NewValue != null && _obj.Employees.Any(x => e.NewValue == true ? !x.Employee.IsNGDDirRX.Value : x.Employee.IsNGDDirRX.Value))
        e.AddError(DirRX.NNOtherProcesses.WeekendWorkStatementDocuments.Resources.NGDGroupValidationError);
    }

    public override void Closing(Sungero.Presentation.FormClosingEventArgs e)
    {
      base.Closing(e);
      e.Params.Remove(Constants.WeekendWorkStatementDocument.DocKindParamName);
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.Remote.CheckOvertimeWorkDocKindByExternalLink(_obj.DocumentKind);
      e.Params.AddOrUpdate(string.Format(Constants.WeekendWorkStatementDocument.DocKindParamName, _obj.Id), isOvertimeWork);
      _obj.State.Properties.Employees.CanAdd = false;
    }

  }
}