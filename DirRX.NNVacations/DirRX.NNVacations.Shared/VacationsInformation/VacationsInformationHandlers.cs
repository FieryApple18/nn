using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationsInformation;

namespace DirRX.NNVacations
{
  partial class VacationsInformationSharedHandlers
  {
    public virtual void DepartmentChanged(DirRX.NNVacations.Shared.VacationsInformationDepartmentChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        _obj.StructuralDepartment = Functions.Module.Remote.GetDepartmentsStructure(e.NewValue);
    }
  }
}