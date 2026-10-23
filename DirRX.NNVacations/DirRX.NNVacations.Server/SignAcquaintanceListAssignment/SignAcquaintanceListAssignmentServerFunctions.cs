using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.SignAcquaintanceListAssignment;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations.Server
{
  partial class SignAcquaintanceListAssignmentFunctions
  {
    /// <summary>
    /// Получить последних исполнителей, выполнивших задание не с результатом Переадресовать.
    /// </summary>
    /// <param name="taskId">ИД задачи.</param>
    /// <param name="blockUid">ИД блока.</param>
    /// <param name="taskStartId">Количество стартов задачи.</param>
    /// <returns>Последние исполнители.</returns>
    [Public]
    public static IQueryable<IUser> GetLastPerformers(long taskId, string blockUid, int? taskStartId)
    {
      var lastIter = SignAcquaintanceListAssignments.GetAll()
        .Where(a => a.Task.Id == taskId && a.BlockUid == blockUid && a.TaskStartId == taskStartId)
        .OrderByDescending(a => a.IterationId)
        .Select(a => a.IterationId)
        .FirstOrDefault();
      
      if (lastIter != null)
        Logger.DebugFormat("GetLastPerformers. TaskId: {0}, BlockUid: {1}, TaskStartId: {2}. Задание Подписание листов ознакомления. Итерация: {3}", taskId, blockUid, taskStartId, lastIter);
      
      return SignAcquaintanceListAssignments.GetAll()
        .Where(a => a.Task.Id == taskId &&
               a.BlockUid == blockUid &&
               a.IterationId == lastIter &&
               a.TaskStartId == taskStartId &&
               a.Result != DirRX.NNVacations.SignAcquaintanceListAssignment.Result.Forward)
        .Select(a => a.CompletedBy);
    }
    
    /// <summary>
    /// Получить листы ознакомления сформированные в рамках этого задания.
    /// </summary>
    /// <returns>Листы ознакомления.</returns>
    [Remote]
    public IQueryable<DirRX.OverrideSolution.IVacationScheduleDocument> GetVacationScheduleAcquaintanceLists()
    {
      var task = VacationScheduleManagementTasks.As(_obj.Task);
      var vacationScheduleDocument = task.DocumentGroup.VacationScheduleDocuments.FirstOrDefault();
      var individualAcquiantanceListKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.IndividualAcquiantanceVacScheduleListKind);
      var groupAcquiantanceListKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.GroupAcquiantanceVacScheduleListKind);
      var inPaper = DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.InPaper;
      var isElectronic = _obj.IsElectronic == true;

      var acquiantanceLists = OverrideSolution.VacationScheduleDocuments.GetAll()
        .Where(d => (Equals(d.DocumentKind, individualAcquiantanceListKind) || Equals(d.DocumentKind, groupAcquiantanceListKind)) &&
               OverrideSolution.VacationScheduleDocuments.Equals(vacationScheduleDocument, d.VacationScheduleDocumentDirRX) &&
               d.AcquiantanceOptionDirRX == inPaper && d.ConsentDirRX == isElectronic);
      return acquiantanceLists;
    }
  }
}