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
    public static IQueryable<NNVacations.IVacationsInformation> GetVacationInfo(IVacationScheduleDocument vacationSchedule)
    {
      return NNVacations.VacationsInformations.GetAll(v => v.Employee.Equals(vacationSchedule.EmployeeDirRX) && v.Year == vacationSchedule.Year
                                                      && v.Department.Equals(vacationSchedule.Department) && v.BusinessUnit.Equals(vacationSchedule.BusinessUnit));
    }

  }
}