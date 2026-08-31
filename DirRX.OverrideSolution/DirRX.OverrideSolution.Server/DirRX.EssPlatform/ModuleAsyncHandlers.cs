using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.EssPlatform.Server
{
  partial class ModuleAsyncHandlers
  {

    /// <summary>
    /// Синхронизация сотрудников одной персоны.
    /// </summary>
    /// <param name="args">Аргументы АО./param>
    // HACK: Перекрывает коробочный АО. Доработаны блокировки и логирование.
    public override void SynchronizeEmployee(DirRX.EssPlatform.Server.AsyncHandlerInvokeArgs.SynchronizeEmployeeInvokeArgs args)
    {
      var logPrefix = string.Format("SynchronizeEmployee. RetryIteration: {0}. ИД основного сотрудника: {1}", args.RetryIteration, args.mainEmployeeId);
      Logger.DebugFormat("{0}. Старт асинхронного обработчика. Параметры: changeTime: {1}.", logPrefix, args.changeTime.ToString("dd.MM.yyyy hh:mm:ss"));
      
      var mainEmployee = EssPlatformSolution.Employees.Get(args.mainEmployeeId);
      
      // Блокировка основного сотрудника.
      var isMainEmployeeLocked = Locks.TryLock(mainEmployee);
      if (!isMainEmployeeLocked)
      {
        var mainEmployeeLockInfo = Locks.GetLockInfo(mainEmployee);
        Logger.DebugFormat("{0}. Основной сотрудник заблокирован пользователем: {1}. Время установки блокировки: {2}. LockedByAsync: {3}.",
                           logPrefix, mainEmployeeLockInfo.OwnerName, mainEmployeeLockInfo.LockTime, mainEmployee.LockedByAsync == true);
        args.Retry = true;
        return;
      }
      
      try
      {
        var partTimeEmployeeIds = EssPlatformSolution.Employees.GetAll().Where(a => a.Status == Sungero.Company.Employee.Status.Active && a.Person.Id == mainEmployee.Person.Id &&
                                                                               a.Id != mainEmployee.Id).Select(a => a.Id);
        var exceptionExist = false;
        var addedMinutes = DirRX.EssPlatform.PublicFunctions.Module.GetMinutesToNextAsyncExecute(args.RetryIteration, Constants.Module.SynchronizeEmployee);
        args.NextRetryTime = Calendar.Now.AddMinutes(addedMinutes);
        var changeTime = args.changeTime;
        
        // Если статус подключения изменился уже после того, как было запрошено текущее изменение. То текущее изменение отменяем.
        if (changeTime != null && mainEmployee.SynchronizationPropertiesChangedDirRX != null && mainEmployee.SynchronizationPropertiesChangedDirRX > changeTime)
        {
          Logger.DebugFormat("{0}. Cтатус подключения изменился уже после того, как было запрошено текущее изменение. changeTime: {1}", logPrefix, args.changeTime.ToString("dd.MM.yyyy hh:mm:ss"));
          return;
        }
        
        if (args.RetryIteration > DirRX.EssPlatform.Constants.Module.SynchronizeEmployee)
        {
          Logger.ErrorFormat("{0}. Превышено максимальное число запусков АО. ИД совместителей: {1}.", logPrefix, string.Join(",", partTimeEmployeeIds));
          args.Retry = false;
          
          // Изменение признака LockedByAsync и сохранение основного сотрудника.
          try
          {
            mainEmployee.LockedByAsync = false;
            mainEmployee.Save();
          }
          catch (Exception ex)
          {
            Logger.DebugFormat("{0}. Ошибка при сохранении основного сотрудника. Значение признака LockedByAsync не изменилось: {1}. Ошибка: {2}. StackTrace: {3}.",
                               logPrefix, mainEmployee.LockedByAsync == true, ex.Message, ex.StackTrace);
          }
          
          return;
        }
        
        var personalPhone = mainEmployee.PersonalPhoneDirRX;
        var needNotifyNewHRAssignmentDirRX = mainEmployee.NeedNotifyNewHRAssignmentDirRX.HasValue ? mainEmployee.NeedNotifyNewHRAssignmentDirRX.Value.ToString() : string.Empty;
        var needNotifyExpiredHRAssignmentDirRX = mainEmployee.NeedNotifyExpiredHRAssignmentDirRX.HasValue ? mainEmployee.NeedNotifyExpiredHRAssignmentDirRX.Value.ToString() : string.Empty;
        var needNotifyHRRepeatDirRX = mainEmployee.NeedNotifyHRRepeatDirRX.HasValue ? mainEmployee.NeedNotifyHRRepeatDirRX.Value.ToString() : string.Empty;
        var confirmationTypeDirRX  = mainEmployee.ConfirmationTypeDirRX.HasValue ? mainEmployee.ConfirmationTypeDirRX.Value.ToString() : string.Empty;
        var messagesEmail = mainEmployee.MessagesEmailDirRX;
        var inheritFromBusinessUnitDirRX = mainEmployee.InheritFromBusinessUnitDirRX.Value;
        var personalAccountStatus = mainEmployee.PersonalAccountStatusDirRX;
        
        foreach (var partTimeEmployeeId in partTimeEmployeeIds)
        {
          #region Синхронизация совместителя
          
          var employee = EssPlatformSolution.Employees.GetAll(x => x.Id == partTimeEmployeeId).FirstOrDefault();
          if (employee == null)
          {
            Logger.DebugFormat("{0}. Не найден совместитель (ИД: {1}).", logPrefix, partTimeEmployeeId);
            continue;
          }
          
          // Блокировка соместителя при обновлении.
          var lockInfo = Locks.GetLockInfo(employee);
          var isLockedPreviously = lockInfo.IsLockedByMe;
          var isLockedNow = false;
          if (!isLockedPreviously)
            isLockedNow = Locks.TryLock(employee);
          
          if (!isLockedPreviously && !isLockedNow)
          {
            Logger.DebugFormat("{0}. Совместитель (ИД: {1}) заблокирован пользователем: {1}. Время установки блокировки: {2}.", logPrefix, employee.Id, lockInfo.OwnerName, lockInfo.LockTime);
            args.Retry = true;
            continue;
          }
          
          try
          {
            if (employee.InheritFromBusinessUnitDirRX != inheritFromBusinessUnitDirRX)
              employee.InheritFromBusinessUnitDirRX = employee.Department.BusinessUnit == null ? false : inheritFromBusinessUnitDirRX;
            if (employee.PersonalPhoneDirRX != personalPhone)
              employee.PersonalPhoneDirRX = personalPhone;
            if (!employee.NeedNotifyNewHRAssignmentDirRX.HasValue ||
                (employee.NeedNotifyNewHRAssignmentDirRX.HasValue && employee.NeedNotifyNewHRAssignmentDirRX.Value.ToString() != needNotifyNewHRAssignmentDirRX))
              employee.NeedNotifyNewHRAssignmentDirRX = DirRX.EssPlatform.PublicFunctions.Module.GetNeedNotifyNewHRAssignmentDirRXEnum(needNotifyNewHRAssignmentDirRX);
            if (!employee.NeedNotifyExpiredHRAssignmentDirRX.HasValue ||
                (employee.NeedNotifyExpiredHRAssignmentDirRX.HasValue && employee.NeedNotifyExpiredHRAssignmentDirRX.Value.ToString() != needNotifyExpiredHRAssignmentDirRX))
              employee.NeedNotifyExpiredHRAssignmentDirRX = DirRX.EssPlatform.PublicFunctions.Module.GetNeedNotifyExpiredHRAssignmentDirRXEnum(needNotifyExpiredHRAssignmentDirRX);
            if (!employee.NeedNotifyHRRepeatDirRX.HasValue ||
                (employee.NeedNotifyHRRepeatDirRX.HasValue && employee.NeedNotifyHRRepeatDirRX.Value.ToString() != needNotifyHRRepeatDirRX))
              employee.NeedNotifyHRRepeatDirRX = DirRX.EssPlatform.PublicFunctions.Module.GetNeedNotifyHRRepeatDirRXEnum(needNotifyHRRepeatDirRX);
            if(!employee.ConfirmationTypeDirRX.HasValue ||
               (employee.ConfirmationTypeDirRX.HasValue && employee.ConfirmationTypeDirRX.Value.ToString() != confirmationTypeDirRX))
              employee.ConfirmationTypeDirRX = DirRX.EssPlatform.PublicFunctions.Module.GetConfirmationTypeDirRXEnum(confirmationTypeDirRX);
            if (employee.MessagesEmailDirRX != messagesEmail)
              employee.MessagesEmailDirRX = messagesEmail;
            if (employee.PersonalAccountStatusDirRX != personalAccountStatus)
              employee.PersonalAccountStatusDirRX = personalAccountStatus;
            if (employee.State.IsChanged)
            {
              employee.Save();
              Logger.DebugFormat("{0}. Совместитель (ИД: {1}) успешно сохранён.", logPrefix, employee.Id);
            }
          }
          catch (Exception ex)
          {
            Logger.ErrorFormat("{0}. Ошибка при обновлении совместителя (ИД: {1}). Ошибка: {2}. StackTrace: {3}.", logPrefix, employee.Id, ex.Message, ex.StackTrace);
            args.Retry = true;
            exceptionExist = true;
          }
          finally
          {
            if (isLockedNow)
              Locks.Unlock(employee);
          }
          
          #endregion
        }

        if (!exceptionExist)
        {
          mainEmployee.LockedByAsync = false;
          mainEmployee.SynchronizationPropertiesChangedDirRX = changeTime;
          mainEmployee.Save();
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при выполнении АО. Ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        args.Retry = true;
      }
      finally
      {
        Locks.Unlock(mainEmployee);
      }
      
      Logger.DebugFormat("{0}. Конец асинхронного обработчика. Повторить обработку: {1}.", logPrefix, args.Retry);
    }

    /// <summary>
    /// Асинхронное формирование записей справочника "Push-уведомление в Супернику".
    /// </summary>
    /// <param name="args">Список параметров.</param>
    public virtual void CreateNotificationToSupernikaDirRX(DirRX.OverrideSolution.Module.EssPlatform.Server.AsyncHandlerInvokeArgs.CreateNotificationToSupernikaDirRXInvokeArgs args)
    {
      Logger.DebugFormat("CreateNotificationToSupernikaDirRX. Начало выполнения АО. Параметры: EmployeeID: {0}, Id: {1}, IsNotice: {2}, IsSendToMobile: {3}, NextRetryTime: {4}, ObjectTypeName: {5}, RetryIteration: {6}, SelfOfficeObjectType: {7}, TargetSystemName: {8}.", args.EmployeeID, args.Id, args.IsNotice, args.IsSendToMobile, args.NextRetryTime, args.ObjectTypeName, args.RetryIteration, args.SelfOfficeObjectType, args.TargetSystemName);
      var logPrefix = string.Format("CreateNotificationToSupernikaDirRX. ИД сотрудника: {0}, ИД задания/уведомления: {1}.", args.EmployeeID, args.Id);
      
      var isLastRetry = NNCommons.PublicFunctions.Module.IsLastAsyncHandlerRetry(args.RetryIteration);
      
      // Проверить наличие задания в БД.
      var jobExists = args.IsNotice ?
        Sungero.Workflow.Notices.GetAll(x => x.Id == args.Id).Any() :
        Sungero.Workflow.Assignments.GetAll(x => x.Id == args.Id).Any();
      
      // Если задания еще нет, тогда откладываем обработку АО.
      if(!jobExists)
      {
        if (isLastRetry)
        {
          Logger.DebugFormat("{0} Не удалось найти задание. Достигнуто максимальное количество повторных запусков АО. Будет выполнена отправка уведомления ответственному за поток.",
                             logPrefix);
          NNIntegration.PublicFunctions.Module.SendNotificationAboutErrors(DirRX.OverrideSolution.Module.EssPlatform.Resources.SupernikaNotificationErrorSubject,
                                                                           DirRX.OverrideSolution.Module.EssPlatform.Resources.SupernikaNotificationAsyncHandlerErrorTextFormat(args.Id),
                                                                           NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.GetSupernikaActionResult);
        }
        else
          Logger.DebugFormat("{0} Не удалось найти задание. Обработка АО будет отложена.", logPrefix);
        
        args.Retry = !isLastRetry;
      }
      else
      {
        Logger.DebugFormat("{0} Задание/уведомление успешно найдено.", logPrefix);
        var employee = Employees.GetAll(x => x.Id == args.EmployeeID).FirstOrDefault();
        
        // Содержание уведомления для Суперники (ПМД).
        var notificationParams = Functions.Module.GetSupernikaPushContent(args.Id, args.IsNotice);
        
        // Заполнение атрибута content для отображения текста уведомления отображения в центре уведомлений ПМД.
        // К тексту уведомления добавляется ссылка на задание/уведомление в ЛК по формату: [<Отображаемое имя>;<Название SmartApp>;<Относительный адрес внутри SmartApp>].
        var supernikaHyperLink = DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.CreateHyperLinkToSupernika(args.TargetSystemName, args.SelfOfficeObjectType, args.ObjectTypeName, args.Id.ToString());
        var content = string.Format("{0}{1}<br>{2}", notificationParams.Text, Environment.NewLine, supernikaHyperLink);
        
        // Расчитать плановую дату отправки.
        var isWorkHoursNotification = DirRX.OverrideSolution.PublicFunctions.Employee.IsWorkHoursNotification(employee);
        var plannedShipDate = Calendar.Now;
        
        // Если пользователю нужно отправлять сообщение только в рабочее время, тогда надо расчитать ближайшее рабочее время.
        if(isWorkHoursNotification)
          plannedShipDate = Calendar.AddWorkingHours(plannedShipDate, employee, 0);
        
        var record = NNIntegration.SupernikaPushNotifications.Create();
        record.EntityId = args.Id;
        record.IsNotification = args.IsNotice;
        record.MessageText = content;
        record.Employee = employee;
        record.EmployeeId = args.EmployeeID;
        record.Phone = employee.PersonalPhoneDirRX;
        record.PlannedShipDate = plannedShipDate;
        record.Save();
        
        Logger.DebugFormat("{0} Запись справочника успешно создана. Ид записи: {1}.", logPrefix, record.Id);
      }
      
      Logger.DebugFormat("{0} Окончания выполнения АО.", logPrefix);
    }

  }
}