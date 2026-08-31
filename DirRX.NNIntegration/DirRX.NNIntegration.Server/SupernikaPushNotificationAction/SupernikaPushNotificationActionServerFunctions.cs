using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.SupernikaPushNotificationAction;

namespace DirRX.NNIntegration.Server
{
  partial class SupernikaPushNotificationActionFunctions
  {
    /// <summary>
    /// Выполнить обработку задания/уведомления по действию в Супернике.
    /// </summary>
    /// <param name="assignment">Задание/уведомление.</param>
    /// <returns>Ошибка. Если ошибки нет - пусая строка.</returns>
    public virtual string CompleteAssignment(Sungero.Domain.Shared.IEntity assignment)
    {
      var blockRx = GetBlockName(assignment);
      
      switch (blockRx)
      {
          // Простое задание HR Pro.
        case Constants.Module.SupernikaActions.BlockNamesRx.HRAssignment:
          return Functions.SupernikaPushNotificationAction.CompleteHRAssignment(_obj, DirRX.OverrideSolution.HRAssignments.As(assignment));
          // Согласование документов.
        case Constants.Module.SupernikaActions.BlockNamesRx.DocumentApprovalAssignment:
          return Functions.SupernikaPushNotificationAction.CompleteDocumentApprovalAssignment(_obj, DirRX.OverrideSolution.DocumentApprovalAssignments.As(assignment));
          // Уведомление HR Pro.
        case Constants.Module.SupernikaActions.BlockNamesRx.HRNotice:
          return Functions.SupernikaPushNotificationAction.ReadHRNotice(_obj, DirRX.OverrideSolution.HRNotices.As(assignment));
          // Иной тип сущности.
        default:
          var error = string.Format("Не удалось выполнить логику обработки задания/уведомления, т.к. не определён тип блока для задания/уведомления: {0}", blockRx);
          return error;
      }
    }
    
    /// <summary>
    /// Выполнить обработку простого задания HR Pro по действию в Супернике.
    /// </summary>
    /// <param name="assignment">Простое задание HR Pro.</param>
    /// <returns>Ошибка. Если ошибки нет - пусая строка.</returns>
    public virtual string CompleteHRAssignment(DirRX.OverrideSolution.IHRAssignment assignment)
    {
      if (assignment.Status != OverrideSolution.HRAssignment.Status.InProcess)
        return DirRX.NNIntegration.SupernikaPushNotificationActions.Resources.AssignmentNotInProgress;
      
      switch (_obj.ResultRx)
      {
        case Constants.Module.SupernikaActions.ResultNamesRx.Complete:
          assignment.Complete(DirRX.OverrideSolution.HRAssignment.Result.Complete);
          break;
        case Constants.Module.SupernikaActions.ResultNamesRx.Approved:
          assignment.Complete(DirRX.OverrideSolution.HRAssignment.Result.Approved);
          break;
        case Constants.Module.SupernikaActions.ResultNamesRx.Noted:
          assignment.Complete(DirRX.OverrideSolution.HRAssignment.Result.Noted);
          break;
        case Constants.Module.SupernikaActions.ResultNamesRx.Refuse:
          assignment.Complete(DirRX.OverrideSolution.HRAssignment.Result.Reject);
          break;
        default:
          return "Не удалось определить вариант выполнения для простого задания HR Pro по действию в Супернике";
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Выполнить обработку задания на согласование документов по действию в Супернике.
    /// </summary>
    /// <param name="assignment">Задание на согласование документов.</param>
    /// <returns>Ошибка. Если ошибки нет - пусая строка.</returns>
    public virtual string CompleteDocumentApprovalAssignment(DirRX.OverrideSolution.IDocumentApprovalAssignment assignment)
    {
      if (assignment.Status != OverrideSolution.DocumentApprovalAssignment.Status.InProcess)
        return DirRX.NNIntegration.SupernikaPushNotificationActions.Resources.AssignmentNotInProgress;
      
      switch (_obj.ResultRx)
      {
        case Constants.Module.SupernikaActions.ResultNamesRx.Approved:
          assignment.Complete(DirRX.OverrideSolution.DocumentApprovalAssignment.Result.Approved);
          break;
        case Constants.Module.SupernikaActions.ResultNamesRx.ForRework:
          assignment.Complete(DirRX.OverrideSolution.DocumentApprovalAssignment.Result.ForRework);
          break;
        case Constants.Module.SupernikaActions.ResultNamesRx.Refuse:
          assignment.Complete(DirRX.OverrideSolution.DocumentApprovalAssignment.Result.Refuse);
          break;
        default:
          return "Не удалось определить вариант выполнения для простого задания HR Pro по действию в Супернике";
      }
      return string.Empty;
    }
    
    /// <summary>
    /// Выполнить обработку уведомления HR Pro по действию в Супернике.
    /// </summary>
    /// <param name=" notice">Уведомление HR Pro.</param>
    /// <returns>Ошибка. Если ошибки нет - пусая строка.</returns>
    public virtual string ReadHRNotice(DirRX.OverrideSolution.IHRNotice notice)
    {
      if (notice.IsRead == true)
      {
        Logger.DebugFormat("ReadHRNotice. Ид уведомления: {0}. Уведомление уже прочитано.", notice.Id);
        return string.Empty;
      }
      
      switch (_obj.ResultRx)
      {
        case Constants.Module.SupernikaActions.ResultNamesRx.MarkRead:
          try
          {
            notice.IsRead = true;
            notice.Save();
          }
          catch (Exception ex)
          {
            Logger.ErrorFormat("ReadHRNotice. Ид уведомления: {0}. Не удалось пометить уведомление прочитанным и сохрнить. Ошибка: {1}. StackTrace: {2}.", notice.Id, ex.Message, ex.StackTrace);
          }
          break;
        default:
          return "Не удалось определить логику обработки уведомления HR Pro по действию в Супернике";
      }
      
      return string.Empty;
    }
    
    /// <summary>
    /// Получить имя блока в RX по заданию/уведомлению.
    /// </summary>
    /// <returns>Задание/уведомление.</returns>
    public static string GetBlockName(Sungero.Domain.Shared.IEntity assignment)
    {
      return PublicFunctions.Module.GetBlockNameByAssignemnt(assignment);
    }
    
    /// <summary>
    /// Получить действие в Супернике.
    /// </summary>
    /// <param name="assignment">Задание/уведомление.</param>
    /// <param name="actionType">Тип действия в Супернике.</param>
    /// <returns>Действие в супернике.</returns>
    public static ISupernikaPushNotificationAction GetAction(Sungero.Domain.Shared.IEntity assignment, string actionType)
    {
      var blockRx = GetBlockName(assignment);
      return GetAction(blockRx, actionType);
    }
    
    /// <summary>
    /// Получить действие в Супернике.
    /// </summary>
    /// <param name="blockRxName">Наименование блока в RX.</param>
    /// <param name="actionType">Тип действия в Супернике.</param>
    /// <returns>Действие в супернике.</returns>
    public static ISupernikaPushNotificationAction GetAction(string blockRxName, string actionType)
    {
      return SupernikaPushNotificationActions.GetAll()
        .Where(x => x.BlockName == blockRxName && x.ActionType == actionType)
        .FirstOrDefault();
    }

    /// <summary>
    /// Получить доступные действия Суперники в задании.
    /// </summary>
    /// <param name="assignment">Задание/уведомление.</param>
    /// <returns>Список действий.</returns>
    /// <remarks>Передаётся как IEntity, т.к. на прикладном уровне нет общего наследника для заданий и уведомлений.</remarks>
    [Public]
    public static List<ISupernikaPushNotificationAction> GetActions(Sungero.Domain.Shared.IEntity assignment)
    {
      return PublicFunctions.Module.GetActions(assignment);
    }
    
  }
}