using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationScheduleDocument;

namespace DirRX.OverrideSolution.Shared
{
  partial class VacationScheduleDocumentFunctions
  {

    /// <summary>
    /// Заполнить имя документа.
    /// </summary>
    public override void FillName()
    {
      if (_obj.DocumentKind != null && !_obj.DocumentKind.GenerateDocumentName.Value && _obj.Name == Sungero.Docflow.OfficialDocuments.Resources.DocumentNameAutotext)
        _obj.Name = string.Empty;
      
      if (_obj.DocumentKind == null || !_obj.DocumentKind.GenerateDocumentName.Value)
        return;
      
      var name = string.Empty;
      var individualScheduleKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNVacations.PublicConstants.Module.DocumentKind.IndividualAcquiantanceVacScheduleListKind);
      
      using (TenantInfo.Culture.SwitchTo())
      {
        if (_obj.DocumentKind != null)
        {
          name += _obj.DocumentKind.ShortName;
          
          if (_obj.DocumentKind.Equals(individualScheduleKind))
          {
            // <Вид документа> <На год> <ФИО работника>
            name += _obj.Year != null ? NNVacations.Resources.VacationScheduleTemplateNameFormat(_obj.Year.Value.Year.ToString()) : string.Empty;
            name += _obj.EmployeeDirRX != null ? string.Format(" {0}", Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.EmployeeDirRX, DeclensionCase.Genitive, false))
              : string.Format(" {0}", HRPro.Vacations.VacationDocumentBases.Resources.EmployeeTemplate);
          }
          else
          {
            // <Вид документа> <На год> <Структурное подразделение>
            name += _obj.Year != null ? NNVacations.Resources.VacationScheduleTemplateNameFormat(_obj.Year.Value.Year.ToString()) : string.Empty;
            name += _obj.Department != null ? string.Format(" {0}", _obj.Department.Name) : string.Empty;
          }
        }
      }
      _obj.Name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
    }

    /// <summary>
    /// Установить обязательность свойств в зависимости от заполненных данных.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      
      _obj.State.Properties.BusinessUnit.IsRequired = false;
      _obj.State.Properties.Department.IsRequired = false;
    }

    /// <summary>
    /// Обновить карточку документа.
    /// </summary>
    public override void RefreshDocumentForm()
    {
      base.RefreshDocumentForm();
      
      _obj.State.Properties.Department.IsVisible = true;
    }

    /// <summary>
    /// Признак необходимости отображения панели регистрации.
    /// </summary>
    /// <param name="additionalCondition">Дополнительное условие при наследовании.</param>
    /// <returns>True, если надо показать панель.</returns>
    public override bool NeedShowRegistrationPane(bool additionalCondition)
    {
      return true;
    }

  }
}