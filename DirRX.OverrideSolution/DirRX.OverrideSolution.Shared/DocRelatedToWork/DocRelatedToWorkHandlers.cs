using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocRelatedToWork;

namespace DirRX.OverrideSolution
{
  partial class DocRelatedToWorkSharedHandlers
  {

    public override void EmployeeDirRXChanged(DirRX.OverrideSolution.Shared.DocRelatedToWorkBaseEmployeeDirRXChangedEventArgs e)
    {
      base.EmployeeDirRXChanged(e);
      
      this.FillName();
    }

  }
}