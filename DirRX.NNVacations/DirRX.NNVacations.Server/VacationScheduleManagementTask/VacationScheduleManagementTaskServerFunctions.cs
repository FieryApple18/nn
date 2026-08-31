using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationScheduleManagementTask;
using DirRX.OverrideSolution;
using DocKind = DirRX.NNVacations.Constants.Module.DocumentKind;

namespace DirRX.NNVacations.Server
{
  partial class VacationScheduleManagementTaskFunctions
  {
    #region Expression-функции.
    
    /// <summary>
    /// Expression-функция для получения сотрудников, подписавших согласие на КЭДО.
    /// </summary>
    /// <param name="task">Задачи ведения графиков отпусков.</param>
    /// <returns>Список сотрудников.</returns>
    [Public, Remote(IsPure = true), ExpressionElement("ExpFunc_GetEmployeesWithConsent_Name", "ExpFunc_GetEmployeesWithConsent_Desc", "")]
    public static List<IEmployee> GetEmployeesWithConsent(IVacationScheduleManagementTask task)
    {
      return VacationsInformations.GetAll().Where(v => v.Year == task.Year)
        .Select(v => v.Employee)
        .Where(x => x.Status == DirRX.OverrideSolution.Employee.Status.Active
               && x.ConsentHRPro == DirRX.OverrideSolution.Employee.ConsentHRPro.Signed
               && Equals(x.BusinessUnit, task.BusinessUnit)
               && (task.Department == null || Equals(x.Department, task.Department)))
        .Distinct()
        .ToList();
    }
    
    /// <summary>
    /// Expression-функция для получения сотрудников, не подписавших согласие на КЭДО.
    /// </summary>
    /// <param name="task">Задачи ведения графиков отпусков.</param>
    /// <returns>Список сотрудников.</returns>
    [Public, Remote(IsPure = true), ExpressionElement("ExpFunc_GetEmployeesWithoutConsent_Name", "ExpFunc_GetEmployeesWithoutConsent_Desc", "")]
    public static List<IEmployee> GetEmployeesWithoutConsent(IVacationScheduleManagementTask task)
    {
      return VacationsInformations.GetAll().Where(v => v.Year == task.Year)
        .Select(v => v.Employee)
        .Where(x => x.Status == DirRX.OverrideSolution.Employee.Status.Active
               && x.ConsentHRPro != DirRX.OverrideSolution.Employee.ConsentHRPro.Signed
               && Equals(x.BusinessUnit, task.BusinessUnit)
               && (task.Department == null || Equals(x.Department, task.Department)))
        .Distinct()
        .ToList();
    }
    
    /// <summary>
    /// Expression-функция для получения кол-ва Индивидуальных листов ознакомления,
    /// для которых ознакомление в электронном виде было просрочено.
    /// </summary>
    /// <param name="task">Задачи ведения графиков отпусков.</param>
    /// <returns>Список листов ознакомления.</returns>
    [Public, Remote(IsPure = true), ExpressionElement("ExpFunc_GetExpiredVacationScheduleDocuments_Name", "ExpFunc_GetExpiredVacationScheduleDocuments_Desc", "")]
    public static IQueryable<IVacationScheduleDocument> GetExpiredVacationScheduleDocuments(IVacationScheduleManagementTask task)
    {
      var abortedStatus = OverrideSolution.VacationScheduleDocument.InternalApprovalState.Aborted;
      var tasks = IndividualVacationScheduleAcquaintanceTasks.GetAll()
        .Where(x => x.VacationScheduleManagementTaskId == task.Id).ToList();
      return tasks.SelectMany(t => t.DocumentGroup.VacationScheduleDocuments.Where(g => g.InternalApprovalState == abortedStatus).Select(d => d)).AsQueryable();
    }
    
    #endregion
  }
}