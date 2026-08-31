using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationsInformation;

namespace DirRX.NNVacations
{
  partial class VacationsInformationServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (string.IsNullOrEmpty(_obj.Name))
      {
        var vacationTypeName = _obj.VacationType != null ? _obj.VacationType.Name.ToLower() : string.Empty;
        var employeeName = _obj.Employee != null ? _obj.Employee.Name : string.Empty;
        var startDate = _obj.StartDate.HasValue ? _obj.StartDate.Value.ToShortDateString() : string.Empty;
        var endDate = _obj.EndDate.HasValue ? _obj.EndDate.Value.ToShortDateString() : string.Empty;
        _obj.Name = VacationsInformations.Resources.VacationInformationNameFormat(vacationTypeName, employeeName, startDate, endDate);
      }
    }
  }

}