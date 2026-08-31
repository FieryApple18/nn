using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNVacations.Server
{
  public partial class ModuleJobs
  {
    /// <summary>
    /// Автоматическое выполнение задания на подтверждение отпуска.
    /// </summary>
    public virtual void AutoExecutionConfirmationAssignment()
    {
      Logger.Debug("AutoExecutionConfirmationAssignment. Начало выполнения фонового процесса.");
      var now = Calendar.Now;
      var assignments = VacationConfirmationAssignments.GetAll().Where(x => x.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                       x.Deadline <= now && x.ShiftTaskId == null);
      
      Logger.DebugFormat("AutoExecutionConfirmationAssignment. Найдено {0} записей.", assignments.Count());
      foreach (var assignment in assignments)
      {
        Transactions.Execute(() => {
                               try
                               {
                                 var lockInfo = Locks.GetLockInfo(assignment);
                                 if (lockInfo.IsLocked)
                                 {
                                   Logger.DebugFormat("AutoExecutionConfirmationAssignment. ИД Задания: {0}. Не удалось выполнить задание: {1}.", assignment.Id, lockInfo.LockedMessage);
                                 }
                                 else
                                 {
                                   assignment.Complete(NNVacations.VacationConfirmationAssignment.Result.Confirm);
                                   Logger.DebugFormat("AutoExecutionConfirmationAssignment. ИД Задания: {0}. Задание успешно выполнено.", assignment.Id);
                                 }
                               }
                               catch (Exception ex)
                               {
                                 Logger.ErrorFormat("AutoExecutionConfirmationAssignment. ИД Задания: {0}. Ошибка выполнения. Текст ошибки: {1}. StackTrace: {2}.",
                                                    assignment.Id, ex.Message, ex.StackTrace);
                               }
                             });
      }
      Logger.Debug("AutoExecutionConfirmationAssignment. Конец выполнения фонового процесса.");
    }
  }
}