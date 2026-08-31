using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.HRDocFlow.Server
{
  partial class ModuleAsyncHandlers
  {

    /// <summary>
    /// Создание замещений для сотрудников персоны.
    /// </summary>
    /// <param name="args.PersonId">ИД персоны.</param>
    public override void CreateSubstitutionsForPerson(HRPro.HRDocFlow.Server.AsyncHandlerInvokeArgs.CreateSubstitutionsForPersonInvokeArgs args)
    {
      long personId = args.personId;
      args.Retry = false;
      
      Logger.DebugFormat("HRDocFlow.AsyncHandlers.CreateSubstitutionsForPerson. iteration: {0}, personId: {1}. Запуск асинхронного обработчика.", args.RetryIteration, personId);
      
      if (args.RetryIteration > HRPro.HRDocFlow.Constants.Module.CreateSubstitutionsForEmployeeRetryCount)
      {
        Logger.ErrorFormat("HRDocFlow.AsyncHandlers.CreateSubstitutionsForPerson. iteration: {0}, personId: {1}. Превышено количество попыток выполнить АО. Замещения - не созданы.", args.RetryIteration, personId);
        args.Retry = false;
        return;
      }
      
      var employees = HRPro.HRProSolution.Employees.GetAll(w => w.Status == HRPro.HRProSolution.Employee.Status.Active && w.Person.Id == personId);
      var substitute = HRPro.HRProSolution.Employees.Null;
      if (employees.Count() > 1)
      {
        var employeesWithLogins = employees.Where(w => w.Login != null);
        if (employeesWithLogins.Any())
        {
          if (employeesWithLogins.Count() > 1)
            substitute = employeesWithLogins.Where(e => e.Id == employeesWithLogins.Select(p => p.Id).Min()).FirstOrDefault();
          
          if (substitute == null)
            substitute = employeesWithLogins.FirstOrDefault();
        }
        else
        {
          substitute = employees.Where(e => e.Id == employees.Select(p => p.Id).Min()).FirstOrDefault();
        }
        
        try
        {
          var substitutions = Substitutions.GetAll(w => Equals(w.Substitute, substitute));
          var isNeedSendNotice = false;
          var employeesForSubstitutions = employees.Where(w => w.BusinessUnit != null && !Equals(w, substitute)).ToList();
          foreach (var employee in employeesForSubstitutions)
          {
            var oldSubstitutions = substitutions.Where(w => w.User.Id == employee.Id).Where(w => !w.EndDate.HasValue || w.EndDate.Value >= Calendar.Today);
            var oldSubstitutionsNumber = oldSubstitutions.Count();
            Logger.DebugFormat("HRDocFlow.AsyncHandlers.CreateSubstitutionsForPerson. iteration: {0}, personId: {1}. employeeId: {2}. Всего устаревших замещений: {3}.", args.RetryIteration, personId, employee.Id, oldSubstitutionsNumber);
            if (oldSubstitutionsNumber > 0)
            {
              foreach (var oldSubstitution in oldSubstitutions)
              {
                if (oldSubstitution.Substitute != substitute)
                {
                  Logger.DebugFormat("HRDocFlow.AsyncHandlers.CreateSubstitutionsForPerson. iteration: {0}, personId: {1}. employeeId: {2}. Удаление устаревшего замещения с ИД: {3}.", args.RetryIteration, personId, employee.Id, oldSubstitution.Id);
                  Substitutions.Delete(oldSubstitution);
                  
                  var substitution = Substitutions.Create();
                  substitution.Substitute = substitute;
                  substitution.User = employee;
                  substitution.Save();
                  isNeedSendNotice = true;
                  Logger.DebugFormat("HRDocFlow.AsyncHandlers.CreateSubstitutionsForPerson. iteration: {0}, personId: {1}. employeeId: {2}. Создано новое замещение с ИД: {3}.", args.RetryIteration, personId, employee.Id, substitution.Id);
                }
              }
            }
            else
            {
              var substitution = Substitutions.Create();
              substitution.Substitute = substitute;
              substitution.User = employee;
              substitution.Save();
              isNeedSendNotice = true;
              Logger.DebugFormat("HRDocFlow.AsyncHandlers.CreateSubstitutionsForPerson. iteration: {0}, personId: {1}. employeeId: {2}. Создано новое замещение с ИД: {3}.", args.RetryIteration, personId, employee.Id, substitution.Id);
            }
          }
          
          if (isNeedSendNotice)
            PublicFunctions.Module.StartAutoSubstitutionNotice(substitute, employeesForSubstitutions);
        }
        catch(Exception ex)
        {
          Logger.ErrorFormat("HRDocFlow.AsyncHandlers.CreateSubstitutionsForPerson. iteration: {0}. personId: {1}. Не удалось создать замещения. Ошибка: {2}. StackTrace: {3}. АО будет перезапущен.", args.RetryIteration ,personId, ex.Message, ex.StackTrace);
          args.Retry = true;
        }
      }
    }

    ///<summary>
    /// Cоздание/обновление замещений для виртуального сотрудника.
    ///</summary>
    public virtual void UpdateSubstitutionsForVirtualEmployeeDirRX(DirRX.OverrideSolution.Module.HRDocFlow.Server.AsyncHandlerInvokeArgs.UpdateSubstitutionsForVirtualEmployeeDirRXInvokeArgs args)
    {
      var virtualEmployeeId = args.EmployeeID;
      Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. RetryIteration: {1}. Начало выполнения асинхронного обработчика.", virtualEmployeeId, args.RetryIteration);
      
      #region Получение виртуального сотрудника.
      var virtualEmployee = Employees.Get(virtualEmployeeId);
      
      if (virtualEmployee == null)
      {
        Logger.ErrorFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. Сотрудник не найден в БД. Окончание выполнения АО.", virtualEmployeeId);
        return ;
      }
      #endregion

      var needCreateSubstitutions = virtualEmployee.NeedCreateSubstitutionsDirRX.Value;
      
      #region Получение замещений.
      
      // Получение замещений, где в качестве сотрудника указан виртуальный сотрудник с переданным ИД, и поле описание заполнено комментарием.
      var oldSubstitutions = Substitutions.GetAll().Where(s => Users.Equals(s.User, Users.As(virtualEmployee)) &&
                                                          s.Comment == Constants.Module.CreatedVirtualEmployeeDescription).ToList();
      
      Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. Найдено замещений: {1}.",
                         virtualEmployeeId, oldSubstitutions.Count());
      #endregion
      
      #region Удаление замещений для сотрудников.
      
      // Список замещений, которые необходимо удалить.
      var deleteSubstitutions = needCreateSubstitutions ?
        oldSubstitutions.Where(s => !virtualEmployee.PerformersDirRX.Any(e => Users.Equals(Users.As(e.Performer), s.Substitute))) :
        oldSubstitutions;
      
      Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. Начало удаления замещений. Будет удалено замещений: {1}.", virtualEmployeeId, deleteSubstitutions.Count());
      
      if (deleteSubstitutions.Any())
      {
        foreach (var substitution in deleteSubstitutions)
        {
          Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. ID замещения: {1}. Начало удаления замещения.", virtualEmployeeId, substitution.Id);
          if (Locks.TryLock(substitution))
          {
            Transactions.Execute(() =>
                                 {
                                   var substitutionId = substitution.Id;
                                   try
                                   {
                                     Substitutions.Delete(substitution);
                                     Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. ID замещения {1}. Замещение удалено.",
                                                        virtualEmployeeId, substitutionId);
                                   }
                                   catch (Exception ex)
                                   {
                                     Logger.ErrorFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. ID замещения {1}. Ошибка при удалении замещения: {2}. StackTrace: {3}.",
                                                        virtualEmployeeId, substitutionId, ex.Message, ex.StackTrace);
                                     args.Retry = true;
                                   }
                                 });
          }
          else
          {
            Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. ID замещения {1}. Не удалось установить блокировку на замещение.",
                               virtualEmployeeId, substitution.Id);
            args.Retry = true;
          }
        }
      }
      
      Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. Окончание удаления замещений.", virtualEmployeeId);
      #endregion
      
      #region Создание новых замещений для сотрудников.
      
      // Список сотрудников, для которых нужно создать замещение виртуального сотрудника.
      var performers = needCreateSubstitutions ?
        virtualEmployee.PerformersDirRX
        .Where(r => !oldSubstitutions.Any(s => Users.Equals(s.Substitute, Users.As(r.Performer))))
        .GroupBy(r => r.Performer.Id)
        .Select(g => g.First().Performer).ToList() :
        new List<DirRX.EssPlatformSolution.IEmployee>();
      
      // Создание новых замещений для сотрудников, которые закреплены за виртуальным сотрудником, но для которых еще не созданы замещения.
      if (performers.Any())
      {
        Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. Начало создания замещений.", virtualEmployeeId);
        
        foreach (var performer in performers)
        {
          Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. ID пользователя: {1}. Создание замещения.",
                             virtualEmployeeId, performer.Id);
          
          Transactions.Execute(() =>
                               {
                                 var newSubstitution = Substitutions.Create();
                                 newSubstitution.User = virtualEmployee;
                                 newSubstitution.Substitute = performer;
                                 newSubstitution.Comment = Constants.Module.CreatedVirtualEmployeeDescription;
                                 
                                 try
                                 {
                                   newSubstitution.Save();
                                   Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. ID пользователя: {1}. Создано замещение.",
                                                      virtualEmployeeId, newSubstitution.Id);
                                 }
                                 catch (Exception ex)
                                 {
                                   Logger.ErrorFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. ID пользователя: {1}. Ошибка при создании замещения: {2}. StackTrace: {3}.",
                                                      virtualEmployeeId, newSubstitution.Id, ex.Message, ex.StackTrace);
                                   args.Retry = true;
                                 }
                               });
        }
        Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. Окончание создания замещений.", virtualEmployeeId);
      }
      else
        Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. Новые записи справочника Замещения не создавались.", virtualEmployeeId);
      #endregion
      
      #region Перезапуск события.
      if (args.Retry)
        Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. RetryIteration: {1}. Асинхронный обработчик будет запущен повторно.",
                           virtualEmployeeId, args.RetryIteration);
      else
        Logger.DebugFormat("UpdateSubstitutionsForVirtualEmployeeDirRX. ID виртуального сотрудника: {0}. RetryIteration: {1}. Окончание выполнения асинхронного обработчика.", virtualEmployeeId, args.RetryIteration);
      #endregion
    }

  }
}