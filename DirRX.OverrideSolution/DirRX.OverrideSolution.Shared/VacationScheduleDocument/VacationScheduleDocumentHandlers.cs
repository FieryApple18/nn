using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationScheduleDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationScheduleDocumentSharedHandlers
  {

    public override void EmployeeDirRXChanged(DirRX.OverrideSolution.Shared.DocRelatedToWorkBaseEmployeeDirRXChangedEventArgs e)
    {
      // Не должно заполняться св-во ТОП.
      var top = _obj.IsTopDirRX;
      base.EmployeeDirRXChanged(e);
      if (_obj.IsTopDirRX != top)
        _obj.IsTopDirRX = top;
      
      this.FillName();
    }

    public override void DepartmentChanged(Sungero.Docflow.Shared.OfficialDocumentDepartmentChangedEventArgs e)
    {
      base.DepartmentChanged(e);
      this.FillName();
      
      if (e.NewValue != e.OldValue)
        _obj.StructuralDepartmentDirRX = NNVacations.PublicFunctions.Module.Remote.GetDepartmentsStructure(e.NewValue);
    }

    public override void YearChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      base.YearChanged(e);
      this.FillName();
    }

  }
}