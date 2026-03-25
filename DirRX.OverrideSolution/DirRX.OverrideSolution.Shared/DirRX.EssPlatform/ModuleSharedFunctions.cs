using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.EssPlatform.Shared
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Изменить статус подключения к личному кабинету для сотрудника.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника</param>
    /// <param name="essStatus">Статус ЛК.</param>
    // HACK: Функция дублирует коробочную функцию EssPlatform.PublicFunctions.Module.ChangeEmployeeStatusConnectionToEss, добавлена обработка блокировок.
    // В коробке эта функция - разделяемая, поэтому пришлось оставить сохранение здесь. В коробке есть её вызов из клиентского действия сотрудника CreateESSUserDirRX.
    [Public]
    public override void ChangeEmployeeStatusConnectionToEss(long employeeId, Sungero.Core.Enumeration essStatus)
    {
      Logger.DebugFormat("ChangeEmployeeConnectionToEssStatus. ИД сотрудника: {0}. Начало изменения статуса подключения к ЛК.", employeeId);
      var employee = EssPlatformSolution.Employees.GetAll(e => e.Id == employeeId).FirstOrDefault();
      
      if (employee == null)
      {
        Logger.ErrorFormat("ChangeEmployeeConnectionToEssStatus. ИД сотрудника: {0}. Сотрудник не найден.", employeeId);
        return;
      }
      
      var lockInfo = Locks.GetLockInfo(employee);
      
      // При блокировке проверяем заблокирован ли уже сотрудник. Если заблокирован ранее, то блокировка должна остатья по итогам выполнения функции.
      var isAlreadyLockedByMe = lockInfo.IsLockedByMe;
      if (!isAlreadyLockedByMe && !Locks.TryLock(employee))
      {
        Logger.DebugFormat("ChangeEmployeeConnectionToEssStatus. ИД сотрудника: {0}. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}.",
                           employeeId, lockInfo.OwnerName, lockInfo.LockTime);
        return;
      }
      
      try
      {
        var localizedStatus = Employees.Info.Properties.PersonalAccountStatusDirRX.GetLocalizedValue(essStatus);
        if (employee.PersonalAccountStatusDirRX != essStatus)
        {
          employee.PersonalAccountStatusDirRX = essStatus;
          Logger.DebugFormat("ChangeEmployeeConnectionToEssStatus. ИД сотрудника: {0}. Установлен новый статус подключения: {1}.", employee.Id, localizedStatus);
          employee.PersonalAccountStatusChangedDirRX = Calendar.Now;
          employee.Save();
          var operation = new Enumeration(DirRX.EssPlatform.Constants.Module.EmployeeStatChanged);
          employee.History.Write(operation, operation, string.Format("Статус: {0}. Изменено от имени: {1}", localizedStatus, Users.Current.Login.ToString()));
        }
        else
          Logger.DebugFormat("ChangeEmployeeConnectionToEssStatus. ИД сотрудника: {0}. Статус подключения к ЛК не имзенился. Текущий статус: {1}.", employee.Id, localizedStatus);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("ChangeEmployeeConnectionToEssStatus. ИД сотрудника: {0}. Ошибка при обновлении статуса. Текст ошибки: {1}. StackTrace: {2}.",
                           employee.Id, ex.Message, ex.StackTrace);
      }
      finally
      {
        // Проверить наличие блокировки, если блокировка была установлена ранее до выполнения функции, то её снимать не нужно.
        lockInfo = Locks.GetLockInfo(employee);
        if (lockInfo.IsLockedByMe && !isAlreadyLockedByMe)
          Locks.Unlock(employee);
      }
    }

  }
}