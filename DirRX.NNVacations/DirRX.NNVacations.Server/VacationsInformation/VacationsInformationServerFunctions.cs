using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationsInformation;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations.Server
{
  partial class VacationsInformationFunctions
  {
    
    /// <summary>
    /// Сформировать запрос к данным справочника "Информация по отпускам".
    /// </summary>
    /// <param name="vacationSchedule">График отпусков.</param>
    /// <returns>Запрос данных справочника "Информация по отпускам".</returns>
    [Public]
    public static IQueryable<NNVacations.IVacationsInformation> GetVacationInfo(IEmployee employee, IVacationScheduleDocument vacationSchedule)
    {
      if (employee == null || vacationSchedule == null)
        return Enumerable.Empty<NNVacations.IVacationsInformation>().AsQueryable();
        
      return NNVacations.VacationsInformations.GetAll(v => Employees.Equals(v.Employee, employee) &&
                                                      VacationScheduleDocuments.Equals(v.VacationSchedule, vacationSchedule) &&
                                                      v.Status == DirRX.NNVacations.VacationsInformation.Status.Active);
    }

  }
}