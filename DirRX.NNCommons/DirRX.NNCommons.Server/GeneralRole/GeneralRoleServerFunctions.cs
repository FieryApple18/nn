using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.GeneralRole;
using DirRX.OverrideSolution;

namespace DirRX.NNCommons.Server
{
  partial class GeneralRoleFunctions
  {
    /// <summary>
    /// Добавить сотрудников из обощённую роли.
    /// </summary>
    /// <param name="employeesIds">Список id сотрудников.</param>
    [Public]
    public virtual void AddEmployees(List<IEmployee> employees)
    {
      // Получить из роли сотрудников по id, которых нет в составе обобщённой роли.
      Logger.DebugFormat("AddEmployees. Ид обобщённой роли: {0}. Всего необходимо включить сотрудников в состав роли: {1}.", _obj.Id, employees.Count());
      foreach (var employee in employees)
      {
        if (!_obj.RecipientLinks.Select(c => c.Member).Contains(employee))
        {
          Logger.DebugFormat("AddEmployees. Ид обобщённой роли: {0}. Ид сотрудника: {1}. Сотрудник будет включён в обобщённую роль.", _obj.Id, employee.Id);
          _obj.RecipientLinks.AddNew().Member = employee;
        }
        else
          Logger.DebugFormat("AddEmployees. Ид обобщённой роли: {0}. Ид сотрудника: {1}. Сотрудник уже включён в обобщённую роль.", _obj.Id, employee.Id);
      }
    }
    
    /// <summary>
    /// Удалить сотрудников из обощённой роли.
    /// </summary>
    /// <param name="employeesIds">Список id сотрудников.</param>
    /// <param name="needCheck">Признак необходимости выполнения проверки перед удалением сотрудника.</param>
    [Public]
    public virtual void RemoveEmployees(List<long> employeesIds, bool needCheck)
    {
      // Получить из роли сотрудников по id, которые есть в составе обобщённой роли.
      var deletedEmployees = _obj.RecipientLinks.Where(e => employeesIds.Contains(e.Member.Id)).ToList();
      Logger.DebugFormat("RemoveEmployees. Ид обобщённой роли: {0}. Всего необходимо обработать сотрудников при удалении из состава роли: {1}.", _obj.Id, deletedEmployees.Count());
      foreach (var item in deletedEmployees)
      {
        // Выполнить проверку перед удалением.
        if (!needCheck || !PublicFunctions.GeneralRole.HasActiveLimitationsForEmployee(_obj, Employees.As(item.Member)))
        {
          Logger.DebugFormat("RemoveEmployees. Ид обобщённой роли: {0}. Ид сотрудника: {1}. Сотрудник будет исключён из обобщённой роли.", _obj.Id, item.Member.Id);
          _obj.RecipientLinks.Remove(item);
        }
        else
          Logger.DebugFormat("RemoveEmployees. Ид обобщённой роли: {0}. Ид сотрудника: {1}. Сотрудник не будет исключён из обобщённой роли, т.к. для него обнаружены активные ограничения видимости.", _obj.Id, item.Member.Id);
      }
    }
    
    /// <summary>
    /// Проверить, что для сотрудника имеются действующие настройки ограничения видимости документов.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Результат проверки. True - если, обнаружены действуюшие настройки. False - если нет.</returns>
    [Public]
    public virtual bool HasActiveLimitationsForEmployee(IEmployee employee)
    {
      return DocumentsVisabilityLimitations.GetAll()
        .Where(x => GeneralRoles.Equals(_obj, x.Role) && x.Status == NNCommons.GeneralRole.Status.Active)
        .SelectMany(c => c.Employees.Select(e => e.Employee))
        .Any(e => Employees.Equals(e, employee));
    }
  }
}