using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationStatementDocument;

namespace DirRX.OverrideSolution.Shared
{
  partial class VacationStatementDocumentFunctions
  {
    /// <summary>
    /// Заполнение поля Отклонение по дням ежегодного оплачиваемого отпуска.
    /// </summary>
    public void SetAnnualVacDeviationDaysDirRX()
    {
      var annualVacationsConstantValue = NNCommons.PublicFunctions.Module.GetStringValue(NNVacations.PublicConstants.Module.ConstantsSetting.AnnualVacationsCodes.Guid);
      var annualVacationsTypes = annualVacationsConstantValue
        .Split(',')
        .Select(x => x.Trim())
        .ToList();
      
      if (annualVacationsTypes.Any() && _obj.VacationDataDirRX.Any() && _obj.AdditionalVacationDirRX.Any())
      {
        var vacationDuration = _obj.VacationDataDirRX
          .Where(v => v.Days.HasValue &&
                 (annualVacationsTypes.Contains(v.VacationType.PlannedAbsenceDirRX) ||
                  annualVacationsTypes.Contains(v.VacationType.PlannedAbsenceShiftDirRX) ||
                  annualVacationsTypes.Contains(v.VacationType.SAPIdDirRX)))
          .Select(c => c.Days.Value)
          .Sum();
        
        var additionalVacationDuration = _obj.AdditionalVacationDirRX
          .Where(v => v.Days.HasValue &&
                 (annualVacationsTypes.Contains(v.VacationType.PlannedAbsenceDirRX) ||
                  annualVacationsTypes.Contains(v.VacationType.PlannedAbsenceShiftDirRX) ||
                  annualVacationsTypes.Contains(v.VacationType.SAPIdDirRX)))
          .Select(c => c.Days.Value)
          .Sum();
        
        _obj.AnnualVacDeviationDaysDirRX = additionalVacationDuration - vacationDuration;
      }
    }
    
    /// <summary>
    /// Сформировать имя документа.
    /// </summary>
    public override void FillName()
    {
      if (_obj.DocumentKind != null && !_obj.DocumentKind.GenerateDocumentName.Value && _obj.Name == Sungero.Docflow.OfficialDocuments.Resources.DocumentNameAutotext)
        _obj.Name = string.Empty;
      
      if (_obj.DocumentKind == null || !_obj.DocumentKind.GenerateDocumentName.Value)
        return;
      
      var name = string.Empty;
      using (TenantInfo.Culture.SwitchTo())
      {
        name += string.Format("{0} ", _obj.DocumentKind.ShortName);
        
        // <Вид документа> <ФИО Работника> от <Дата подачи>.
        name += string.Format(" {0}", _obj.Employee != null ? Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employee, DeclensionCase.Genitive, false)
                              : HRPro.Vacations.VacationDocumentBases.Resources.EmployeeTemplate);
        
        if (_obj.ApplicationDateDirRX != null)
          name += HRPro.Vacations.VacationDocumentBases.Resources.DateFrom + _obj.ApplicationDateDirRX.Value.ToString("d");
      }
      
      _obj.Name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
    }

    /// <summary>
    /// Изменить отображение панели регистрации.
    /// </summary>
    /// <param name="needShow">Признак отображения.</param>
    /// <param name="repeatRegister">Признак повторной регистрации\изменения реквизитов.</param>
    public override void ChangeRegistrationPaneVisibility(bool needShow, bool repeatRegister)
    {
      base.ChangeRegistrationPaneVisibility(true, true);
    }

    /// <summary>
    /// Сменить доступность реквизитов документа.
    /// </summary>
    /// <param name="isEnabled">True, если свойства должны быть доступны.</param>
    /// <param name="repeatRegister">Перерегистрация.</param>
    public override void ChangeDocumentPropertiesAccess(bool isEnabled, bool repeatRegister)
    {
      base.ChangeDocumentPropertiesAccess(true, true);
      
      _obj.State.Properties.RegistrationDate.IsEnabled = false;
      _obj.State.Properties.DocumentKind.IsEnabled = true;
    }

  }
}