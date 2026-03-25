using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkStatementDocument;

namespace DirRX.NNOtherProcesses.Client
{

  partial class WeekendWorkStatementDocumentActions
  {
    
    #region Общие действия для выделения ошибок валидации ТЧ
    
    #region Валидация даты в ТЧ и в карточке для полей перода работы
    public virtual void ValidateWeekendWorkPeriodDates(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkPeriodDates(_obj);
      List<long> employeeErrorRecordsIds = errorRecords.Select(x => x.Id).ToList();
      e.Params.AddOrUpdate(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, employeeErrorRecordsIds);
    }

    public virtual bool CanValidateWeekendWorkPeriodDates(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
    #endregion
    
    #region Валидация дат доп. отдыха для компенсации типа предоставление другого дня отдыха
    public virtual void ValidateWeekendWorkRestDates(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDates(_obj);
      List<long> employeeErrorRecordsIds = errorRecords.Select(x => x.Id).ToList();
      e.Params.AddOrUpdate(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, employeeErrorRecordsIds);
    }
    
    public virtual bool CanValidateWeekendWorkRestDates(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
    #endregion
    
    #region Валидация "Даты доп. отдыха с"
    public virtual void ValidateWeekendWorkRestDateFrom(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDateFrom(_obj);
      List<long> employeeErrorRecordsIds = errorRecords.Select(x => x.Id).ToList();
      e.Params.AddOrUpdate(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, employeeErrorRecordsIds);
    }
    
    public virtual bool CanValidateWeekendWorkRestDateFrom(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
    #endregion
    
    #region Валидация перерывов
    public virtual void ValidateWeekendWorkBreakPeriods(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var validateResult = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkBreakPeriods(_obj);
      if (!string.IsNullOrEmpty(validateResult.Error))
      {
        List<long> employeeErrorRecordsIds = validateResult.ErrorRecords.Select(x => x.Id).ToList();
        e.Params.AddOrUpdate(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, employeeErrorRecordsIds);
      }
    }
    
    public virtual bool CanValidateWeekendWorkBreakPeriods(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
    #endregion
    
    #endregion
    
    #region Действия для выделения ошибок валидации ТЧ для работы в сверхурочное время

    #region Проверка на различие дат полей "Даты доп. отдыха с" и "Даты доп. отдыха по"
    public virtual void ValidateWeekendWorkRestDateOneDay(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.Remote.CheckOvertimeWorkDocKindByExternalLink(_obj.DocumentKind);
      var errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDateOneDay(_obj);
      List<long> employeeErrorRecordsIds = isOvertimeWork == true ? errorRecords.Select(x => x.Id).ToList() : new List<long>();
      e.Params.AddOrUpdate(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, employeeErrorRecordsIds);
    }
    
    public virtual bool CanValidateWeekendWorkRestDateOneDay(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
    #endregion

    #region Проверка на то, что "Даты доп. отдыха по" меньше "Даты доп. отдыха" с по времени
    public virtual void ValidateWeekendWorkRestDateTo(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.Remote.CheckOvertimeWorkDocKindByExternalLink(_obj.DocumentKind);
      var errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDateTo(_obj);
      List<long> employeeErrorRecordsIds = isOvertimeWork == true ? errorRecords.Select(x => x.Id).ToList() : new List<long>();
      e.Params.AddOrUpdate(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, employeeErrorRecordsIds);
    }

    public virtual bool CanValidateWeekendWorkRestDateTo(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
    #endregion

    #endregion
    
    #region Действия для выделения ошибок валидации ТЧ для работы ВД/ПД
    
    #region Валидация времени в ТЧ и в карточке для полей периода работы
    public virtual void ValidateWeekendWorkPeriodTime(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.Remote.CheckOvertimeWorkDocKindByExternalLink(_obj.DocumentKind);
      var errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkPeriodTime(_obj);
      List<long> employeeErrorRecordsIds = isOvertimeWork != true ? errorRecords.Select(x => x.Id).ToList() : new List<long>();
      e.Params.AddOrUpdate(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, employeeErrorRecordsIds);
    }

    public virtual bool CanValidateWeekendWorkPeriodTime(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
    #endregion
    
    #region Валидация периода доп. отдыха для заявки на работу в выходной/праздничный день
    public virtual void ValidateWeekendWorkRestDateEqual(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var isOvertimeWork = Functions.WeekendWorkStatementDocument.Remote.CheckOvertimeWorkDocKindByExternalLink(_obj.DocumentKind);
      var errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkRestDateEqual(_obj);
      List<long> employeeErrorRecordsIds = isOvertimeWork != true ? errorRecords.Select(x => x.Id).ToList() : new List<long>();
      e.Params.AddOrUpdate(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, employeeErrorRecordsIds);
    }

    public virtual bool CanValidateWeekendWorkRestDateEqual(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
    #endregion
    
    #region Валидация количества рабочих часов
    public virtual void ValidateWeekendWorkHoursExceeded(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var errorRecords = Functions.WeekendWorkStatementDocument.ValidateWeekendWorkHoursExceeded(_obj);
      List<long> employeeErrorRecordsIds = errorRecords.Select(x => x.Id).ToList();
      e.Params.AddOrUpdate(Constants.WeekendWorkStatementDocument.EmployeeErrorRecordsIdsKey, employeeErrorRecordsIds);
    }

    public virtual bool CanValidateWeekendWorkHoursExceeded(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
    #endregion
    
    #endregion

    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.SendForApproval(e);
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false;
    }

    public virtual void SendStatementForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var error = Functions.WeekendWorkStatementDocument.Remote.ValidateStatementInExternalSystem(_obj);
      if (!string.IsNullOrEmpty(error))
      {
        e.AddError(error);
        return;
      }
      
      // Открыть карточку задачи на согласования заявки ВД/ПД/СР.
      Functions.WeekendWorkTask.Remote.CreateTaksWithStatement(_obj).Show();
      e.CloseFormAfterAction = true;
    }

    public virtual bool CanSendStatementForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.InternalApprovalState == null && _obj.LifeCycleState == WeekendWorkStatementDocument.LifeCycleState.Draft && !_obj.State.IsInserted;
    }

  }

  internal static class WeekendWorkStatementDocumentEmployeesStaticActions
  {

    public static bool CanAddEmployees(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      var statement = WeekendWorkStatementDocuments.As(e.RootEntity);
      var isEnabled = (statement.LifeCycleState == NNOtherProcesses.WeekendWorkStatementDocument.LifeCycleState.Draft ||
                       statement.InternalApprovalState == NNOtherProcesses.WeekendWorkStatementDocument.InternalApprovalState.OnRework) &&
        statement.WorkDateFrom.HasValue && statement.WorkDateTo.HasValue &&
        statement.DocumentKind != null &&
        statement.BusinessUnit != null && statement.PreparedBy != null;
      return isEnabled && statement.Employees.Count < PublicConstants.WeekendWorkStatementDocument.MaxEmployeesCount;
    }

    public static void AddEmployees(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      Functions.WeekendWorkStatementDocument.FillEmployees(WeekendWorkStatementDocuments.As(e.RootEntity));
    }

    public static bool CanEditEmployee(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      var statement = WeekendWorkStatementDocuments.As(e.RootEntity);
      var isEnabled = statement.Employees.Any() &&
        (statement.LifeCycleState == NNOtherProcesses.WeekendWorkStatementDocument.LifeCycleState.Draft ||
         statement.InternalApprovalState == NNOtherProcesses.WeekendWorkStatementDocument.InternalApprovalState.OnRework) &&
        statement.WorkDateFrom.HasValue && statement.WorkDateTo.HasValue &&
        statement.DocumentKind != null &&
        statement.BusinessUnit != null && statement.PreparedBy != null;
      return isEnabled && !statement.State.IsInserted;
    }

    public static void EditEmployee(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      Functions.WeekendWorkStatementDocument.EditEmployee(WeekendWorkStatementDocuments.As(e.RootEntity));
    }
  }

}