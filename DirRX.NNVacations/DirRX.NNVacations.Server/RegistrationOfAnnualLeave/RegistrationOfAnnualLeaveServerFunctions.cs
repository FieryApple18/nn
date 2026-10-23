using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.RegistrationOfAnnualLeave;

namespace DirRX.NNVacations.Server
{
  partial class RegistrationOfAnnualLeaveFunctions
  {

    /// <summary>
    /// Expression-функция для получения связанной задачи на перенос по заявлению.
    /// </summary>
    /// <param name="task">Задача на подтверждение дат отпуска.</param>
    /// <returns>Задача на перенос отпуска.</returns>
    [Public, Remote(IsPure = true), ExpressionElement("ExpFunc_GetTransferVacationTask_Name", "ExpFunc_GetTransferVacationTask_Desc", "")]
    public static OverrideSolution.IStatementTask GetTransferVacationTask(IRegistrationOfAnnualLeave task)
    {
      return OverrideSolution.StatementTasks.GetAll().FirstOrDefault(x => x.Id == task.TransferTaskId);
    }
    
    /// <summary>
    /// Expression-функция для получения связанной задачи на перенос из уведомления.
    /// </summary>
    /// <param name="task">Задача на подтверждение дат отпуска.</param>
    /// <returns>ИД задачи на перенос из уведомления.</returns>
    [Public, Remote(IsPure = true), ExpressionElement("ExpFunc_GetShiftVacationTask_Name", "ExpFunc_GetShiftVacationTask_Desc", "")]
    public static OverrideSolution.IStatementTask GetShiftVacationTask(IRegistrationOfAnnualLeave task)
    {
      return OverrideSolution.StatementTasks.GetAll().FirstOrDefault(x => x.Id == task.ShiftTaskId);
    }
    
    /// <summary>
    /// Expression-функция для проверки состояние заявления на перенос.
    /// </summary>
    /// <param name="task">Задача на подтверждение отпуска.</param>
    /// <param name="state">Состояние документа.</param>
    /// <returns>True, если документ соответствует переданному состоянию.</returns>
    [Public, Remote(IsPure = true), ExpressionElement("ExpFunc_VerifyShiftVacationDocumentForState_Name", "ExpFunc_VerifyShiftVacationDocumentForState_Desc", "")]
    public static bool VerifyShiftVacationDocumentForState(IRegistrationOfAnnualLeave task, string state)
    {
      var shiftTask = GetShiftVacationTask(task);
      if (shiftTask != null)
      {
        if (shiftTask.DocumentGroup.DocRelatedToWorkBases.Any())
        {
          var statement = shiftTask.DocumentGroup.DocRelatedToWorkBases.FirstOrDefault();
          var localizedState = OverrideSolution.DocRelatedToWorkBases.Info.Properties.InternalApprovalState.GetLocalizedValue(statement.InternalApprovalState);
          return localizedState == state.Trim();
        }
        else
        {
          Logger.Error("VerifyShiftVacationDocumentForState. Не найдено заявление на перенос отпуска.");
        }
      }
      else
      {
        Logger.Error("VerifyShiftVacationDocumentForState. Не найдена задача на перенос отпуска.");
      }
      return false;
    }
    
    /// <summary>
    /// Создание задачи с типом Регистрация ежегодного отпуска.
    /// </summary>
    /// <param name="logPrefix">Префикс логирования.</param>
    /// <param name="vacationsInfo">Структура с информацией по отпуску сотрудника.</param>
    /// <returns>Структура с информацией по отпуску сотрудника.</returns>
    public static List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTime> CreateRegistrationOfAnnualLeaveTask(string logPrefix,
                                                                                                                        List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTime> vacationsInfo)

    {
      var errorVacations = new List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTime>();
      var personnelNumber = vacationsInfo.First().PersonnelNumber;
      var employee = OverrideSolution.Employees.GetAll(x => x.PersonnelNumber == personnelNumber).FirstOrDefault();
      vacationsInfo = vacationsInfo.OrderBy(v => v.DateStart).ToList();
      
      // Сформировать список по группам подряд идущих отпусков.
      var vacationsList = new List<List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTime>>();
      var vacationsGroup = new List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTime>();
      vacationsGroup.Add(vacationsInfo.First());
      foreach (var vacInfo in vacationsInfo.Skip(1))
      {
        if (vacInfo.DateStart != vacationsGroup.OrderByDescending(v => v.DateStart).First().DateEnd.AddDays(1))
        {
          vacationsList.Add(vacationsGroup);
          vacationsGroup = new List<DirRX.NNIntegration.Structures.Module.IVacationInfoDateTime>();
        }
        vacationsGroup.Add(vacInfo);
      }
      vacationsList.Add(vacationsGroup);
      
      foreach (var vacGroup in vacationsList)
      {

        Transactions.Execute(() =>
                             {
                               try
                               {
                                 var task = RegistrationOfAnnualLeaves.Create();
                                 var dateStart = vacGroup.OrderBy(v => v.DateStart).First().DateStart;
                                 var dateEnd = vacGroup.OrderByDescending(v => v.DateEnd).First().DateEnd;
                                 task.Subject = RegistrationOfAnnualLeaves.Resources.RegistrationOfAnnualLeaveSubjectFormat(employee, dateStart.ToShortDateString(), dateEnd.ToShortDateString());
                                 task.Employee = employee;
                                 task.StartDate = dateStart;
                                 task.EndDate = dateEnd;
                                 foreach (var vacInfo in vacGroup)
                                 {
                                   var vacationType = OverrideSolution.VacationKinds.GetAll(x => (x.PlannedAbsenceDirRX == vacInfo.TypeVacation ||
                                                                                                  x.PlannedAbsenceShiftDirRX == vacInfo.TypeVacation) &&
                                                                                                    x.VacationKindDirRX == OverrideSolution.VacationKind.VacationKindDirRX.AnnualLeave &&
                                                                                            Equals(x.BusinessUnitDirRX, employee.BusinessUnit))
                                     .FirstOrDefault();
                                   
                                   var vacationData = task.VacationData.AddNew();
                                   vacationData.VacationType = vacationType;
                                   vacationData.PlannedAbsence = vacInfo.TypeVacation;
                                   vacationData.StartDate = vacInfo.DateStart;
                                   vacationData.EndDate = vacInfo.DateEnd;
                                   vacationData.Days = Functions.Module.DurationInCalendarDaysWithoutHoliday(vacInfo.DateStart, vacInfo.DateEnd, employee, vacationData.VacationType, null);
                                 }
                                 
                                 task.Start();
                                 Logger.DebugFormat("{0}. Табельный номер сотрудника: {1}. Создана и отправлена задача Регистрация ежегодного отпуска с ИД: {2}.",
                                                    logPrefix, personnelNumber, task.Id);
                               }
                               catch (Exception ex)
                               {
                                 Logger.ErrorFormat("{0}. Табельный номер сотрудника: {1}. Ошибка выполнения: {2}. StackStrace: {3}.",
                                                    logPrefix, personnelNumber, ex.Message, ex.StackTrace);
                                 errorVacations.AddRange(vacGroup);
                               }
                             });
      }
      
      return errorVacations;
    }
  }
}