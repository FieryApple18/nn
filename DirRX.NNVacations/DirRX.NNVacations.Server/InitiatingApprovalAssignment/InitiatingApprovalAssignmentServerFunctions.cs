using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.InitiatingApprovalAssignment;

namespace DirRX.NNVacations.Server
{
  partial class InitiatingApprovalAssignmentFunctions
  {
    /// <summary>
    /// Получить последних исполнителей, выполнивших задание не с результатом Переадресовать.
    /// </summary>
    /// <param name="taskId">ИД задачи.</param>
    /// <param name="blockUid">ИД блока.</param>
    /// <param name="startId">Количество стартов задачи.</param>
    /// <returns>Последние исполнители.</returns>
    public static IQueryable<IUser> GetLastPerformers(long taskId, string blockUid, int? startId)
    {
      var lastIter = InitiatingApprovalAssignments.GetAll()
        .Where(a => a.Task.Id == taskId && a.BlockUid == blockUid && a.TaskStartId == startId)
        .OrderByDescending(a => a.IterationId)
        .Select(a => a.IterationId)
        .FirstOrDefault();
      
      if (lastIter != null)
        Logger.DebugFormat("GetLastPerformers. TaskId: {0}, BlockUid: {1}, StartId: {2}. InitiatingApprovalAssignment. Итерация: {3}", taskId, blockUid, startId, lastIter);
      
      return InitiatingApprovalAssignments.GetAll()
        .Where(a => a.Task.Id == taskId &&
               a.BlockUid == blockUid &&
               a.IterationId == lastIter &&
               a.TaskStartId == startId &&
               a.Result != DirRX.NNVacations.InitiatingApprovalAssignment.Result.Forward)
        .Select(a => a.CompletedBy);
    }
  }
}