using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationNoticeDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationNoticeDocumentSharedHandlers
  {

    public override void DepartmentChanged(Sungero.Docflow.Shared.OfficialDocumentDepartmentChangedEventArgs e)
    {
      base.DepartmentChanged(e);
      
      if (e.NewValue != e.OldValue && e.NewValue != null)
        _obj.StructuralDepartmentDirRX = NNVacations.PublicFunctions.Module.Remote.GetDepartmentsStructure(e.NewValue);
    }

  }
}