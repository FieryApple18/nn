using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.DocumentsVisabilityLimitation;

namespace DirRX.NNCommons
{
  partial class DocumentsVisabilityLimitationRolePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> RoleFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(r => r.Status == DocumentsVisabilityLimitation.Status.Active);
    }
  }


  partial class DocumentsVisabilityLimitationServerHandlers
  {

    public override void AfterDelete(Sungero.Domain.AfterDeleteEventArgs e)
    {
      // Запуск АО для обновления обобщённой роли после удаления записи.
      var employeesIds = string.Join(",", _obj.Employees.Select(x => x.Employee.Id));
      if (!string.IsNullOrEmpty(employeesIds))
      {
        var asyncHandler = NNCommons.AsyncHandlers.UpdateGeneralRoleAfterDelete.Create();
        asyncHandler.GeneralRoleId = _obj.Role.Id;
        asyncHandler.EmployeesIds = employeesIds;
        asyncHandler.ExecuteAsync();
      }
    }

    public override void AfterSave(Sungero.Domain.AfterSaveEventArgs e)
    {
      var employeesIsChanged = false;
      var statusIsChanged = false;
      long originalRoleId;
      var originalEmployeesIds = string.Empty;
      
      e.Params.TryGetValue(Constants.DocumentsVisabilityLimitation.EmployeesIsChanged, out employeesIsChanged);
      e.Params.TryGetValue(Constants.DocumentsVisabilityLimitation.StatusIsChanged, out statusIsChanged);
      var roleIsChanged = e.Params.TryGetValue(Constants.DocumentsVisabilityLimitation.GeneralRoleIsChanged, out originalRoleId);
      var hasEmployeesIds = e.Params.TryGetValue(Constants.DocumentsVisabilityLimitation.OriginalEmployeesIds, out originalEmployeesIds);
      
      // Запуск АО для обновления состава обобщённой роли при изменении ограничения.
      if (employeesIsChanged || statusIsChanged || roleIsChanged)
      {
        var asyncHandler = NNCommons.AsyncHandlers.UpdateGeneralRoleAfterChange.Create();
        asyncHandler.DocumentsVisabilityLimitationId = _obj.Id;
        
        // Обновить участников, если изменилась роль или состав участников.
        asyncHandler.EmployeesIsChanged = roleIsChanged || employeesIsChanged;
        asyncHandler.StatusIsChanged = statusIsChanged;
        asyncHandler.ExecuteAsync();
      }
      
      // Запустить АО для очистки состава первоначальной обобщённой роли, если изменилось значение обобщённой роли ограничения.
      if (roleIsChanged && hasEmployeesIds && !string.IsNullOrEmpty(originalEmployeesIds))
      {
        var asyncHandler = NNCommons.AsyncHandlers.UpdateGeneralRoleAfterDelete.Create();
        
        // Первоначальная обобщённая роль.
        asyncHandler.GeneralRoleId = originalRoleId;
        
        // Первоначальный состав ограничения на момент открытия карточки или после последнего сохранения.
        asyncHandler.EmployeesIds = originalEmployeesIds;
        asyncHandler.ExecuteAsync();
        e.Params.Remove(Constants.DocumentsVisabilityLimitation.GeneralRoleIsChanged);
      }
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // Заполнение имени.
      PublicFunctions.DocumentsVisabilityLimitation.FillName(_obj);
      
      // Проверка на наличие дублей.
      if (Functions.DocumentsVisabilityLimitation.HaveDuplicates(_obj))
        e.AddError(DirRX.NNCommons.DocumentsVisabilityLimitations.Resources.DuplicatesDetected, _obj.Info.Actions.ShowDuplicates);
      
      // Изменился состав ограничения видимости документов.
      var employeeIsChanged = _obj.State.Properties.Employees.IsChanged || _obj.Employees.Any(x => x.State.Properties.Employee.IsChanged);
      e.Params.AddOrUpdate(Constants.DocumentsVisabilityLimitation.EmployeesIsChanged, employeeIsChanged);
      
      // Изменилось состояние ограничения видимости документов.
      var statusIsChanged = _obj.Status != _obj.State.Properties.Status.OriginalValue;
      e.Params.AddOrUpdate(Constants.DocumentsVisabilityLimitation.StatusIsChanged, statusIsChanged);
      
      // Изменилась ссылка на обобщённую роль. Необходимо синхронизировать первоначальную обощённую роль и новую.
      if(_obj.State.Properties.Role.OriginalValue != null && !GeneralRoles.Equals(_obj.Role, _obj.State.Properties.Role.OriginalValue))
      {
        e.Params.AddOrUpdate(Constants.DocumentsVisabilityLimitation.GeneralRoleIsChanged, _obj.State.Properties.Role.OriginalValue.Id);
        var oldEmployees = _obj.Employees
          .Where(x => !_obj.State.Properties.Employees.Added.Any(a => x.Id == a.Id) && !_obj.State.Properties.Employees.Changed.Any(c => c.Id == x.Id))
          .Select(x => x.Employee.Id.ToString()).ToList();
        var deletedEmployees = _obj.State.Properties.Employees.Deleted.Select(x => x.Employee.Id.ToString()).ToList();
        var changedEmployees = _obj.State.Properties.Employees.Changed.Select(x => x.State.Properties.Employee.OriginalValue.Id.ToString()).ToList();
        var originalEmployeesIds = string.Join(",", oldEmployees.Concat(deletedEmployees).Concat(changedEmployees));
        e.Params.AddOrUpdate(Constants.DocumentsVisabilityLimitation.OriginalEmployeesIds, originalEmployeesIds);
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      PublicFunctions.DocumentsVisabilityLimitation.FillName(_obj);
    }
  }

}