using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationStatementDocument;

namespace DirRX.OverrideSolution
{
  partial class VacationStatementDocumentAdditionalVacationDirRXSharedHandlers
  {

    public virtual void AdditionalVacationDirRXDaysChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      Functions.VacationStatementDocument.SetAnnualVacDeviationDaysDirRX(_obj.VacationStatementDocument);
    }
  }

  partial class VacationStatementDocumentVacationDataDirRXSharedHandlers
  {

    public virtual void VacationDataDirRXDaysChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      Functions.VacationStatementDocument.SetAnnualVacDeviationDaysDirRX(_obj.VacationStatementDocument);
    }
  }

  partial class VacationStatementDocumentSharedHandlers
  {

    public virtual void VacationDataDirRXChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
    }

    public virtual void AdditionalVacationDirRXChanged(Sungero.Domain.Shared.CollectionPropertyChangedEventArgs e)
    {
    }
    
    public virtual void ApplicationDateDirRXChanged(Sungero.Domain.Shared.DateTimePropertyChangedEventArgs e)
    {
      this.FillName();
    }

    public override void EmployeeChanged(HRPro.Vacations.Shared.VacationPersonalDocumentBaseEmployeeChangedEventArgs e)
    {
      base.EmployeeChanged(e);
      
      if (e.NewValue != null)
      {
        _obj.Department = e.NewValue?.Department;
        _obj.BusinessUnit = e.NewValue?.Department?.BusinessUnit;
      }
    }

  }
}