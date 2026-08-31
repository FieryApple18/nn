using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.SignAcquaintanceListAssignment;

namespace DirRX.NNVacations.Client
{
  partial class SignAcquaintanceListAssignmentFunctions
  {
    /// <summary>
    /// Сформировать листы ознакомления.
    /// </summary>
    /// <param name="asyncName">Имя асинхронного обработчика.</param>
    public void CreateAcquiantanceVacScheduleLists(string asyncName)
    {
      var task = VacationScheduleManagementTasks.As(_obj.Task);
      if (asyncName == "CreateGroupAcquiantanceVacScheduleLists")
      {
        var async = AsyncHandlers.CreateGroupAcquiantanceVacScheduleLists.Create();
        async.VacationScheduleManagementTaskId = task.Id;
        async.AcquiantanceOption = DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.InPaper.Value;
        async.Consent = _obj.IsElectronic == true;
        async.PerformerId = Users.Current.Id;
        async.ExecuteAsync(SignAcquaintanceListAssignments.Resources.StartedCreateAcquiantanceVacScheduleLists, SignAcquaintanceListAssignments.Resources.CompletedCreateAcquiantanceVacScheduleLists);
      }
      else
      {
        var async = AsyncHandlers.CreateIndividualAcquiantanceVacScheduleLists.Create();
        async.VacationScheduleManagementTaskId = task.Id;
        async.AcquiantanceOption = DirRX.OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.InPaper.Value;
        async.Consent = _obj.IsElectronic == true;
        async.PerformerId = Users.Current.Id;
        async.ExecuteAsync(SignAcquaintanceListAssignments.Resources.StartedCreateAcquiantanceVacScheduleLists, SignAcquaintanceListAssignments.Resources.CompletedCreateAcquiantanceVacScheduleLists);
      }
      _obj.IsStartedCreateLists = true;
      _obj.Save();
    }
  }
}