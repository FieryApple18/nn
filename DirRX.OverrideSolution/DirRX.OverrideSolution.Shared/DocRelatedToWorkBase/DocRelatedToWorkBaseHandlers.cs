using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocRelatedToWorkBase;

namespace DirRX.OverrideSolution
{
  partial class DocRelatedToWorkBaseSharedHandlers
  {

    public virtual void PersonnelNumberDirRXChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      // Очистить сотрудника при изменении таб. номера.
      if (e.NewValue != e.OldValue && _obj.EmployeeDirRX != null && _obj.EmployeeDirRX.PersonnelNumber != e.NewValue)
        _obj.EmployeeDirRX = null;
    }

    public override void BusinessUnitChanged(Sungero.Docflow.Shared.OfficialDocumentBusinessUnitChangedEventArgs e)
    {
      base.BusinessUnitChanged(e);
      
      // Очистить cотрудника при изменении НОР.
      if (!BusinessUnits.Equals(e.NewValue, e.OldValue) && _obj.EmployeeDirRX != null && !BusinessUnits.Equals(_obj.EmployeeDirRX.BusinessUnit, e.NewValue))
      {
        _obj.EmployeeDirRX = null;
        
        if (e.NewValue == null && e.OldValue != null)
          Logger.DebugFormat("DocRelatedToWorkBase. BusinessUnitChanged. Выполнилась очистка НОР. Ид документа: {0}.", _obj.Id);
      }
    }

    public override void DocumentKindChanged(Sungero.Docflow.Shared.OfficialDocumentDocumentKindChangedEventArgs e)
    {
      var date = _obj.RegistrationDate;
      
      base.DocumentKindChanged(e);
      
      // HACK: Seleznyov_DV. Обход очистки поля Дата документа при смене вида документа.
      if (!_obj.RegistrationDate.HasValue)
        _obj.RegistrationDate = date;
    }

    public virtual void EmployeeDirRXChanged(DirRX.OverrideSolution.Shared.DocRelatedToWorkBaseEmployeeDirRXChangedEventArgs e)
    {
      if (!Employees.Equals(e.NewValue, e.OldValue))
      {
        if (e.NewValue != null)
        {
          _obj.PersonnelNumberDirRX = e.NewValue.PersonnelNumber;
          _obj.BusinessUnit = e.NewValue.BusinessUnit;
          _obj.IsNGDDirRX = e.NewValue.IsNGDDirRX;
          _obj.IsTopDirRX = OverrideSolution.JobTitles.As(e.NewValue.JobTitle)?.IsTopDirRX;
          _obj.PersonnelSectionDirRX = e.NewValue.PersonnelSectionDirRX;
        }
      }
    }

  }
}