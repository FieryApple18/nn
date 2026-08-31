using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationConfirmationAssignment;

namespace DirRX.NNVacations
{
  partial class VacationConfirmationAssignmentSubstitutePropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> SubstituteFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      var employee = Sungero.Company.Employees.As(_obj.Performer);
      if (employee == null)
        return query.Take(0);
      return query.Where(l => !l.Equals(employee));
    }
  }

}