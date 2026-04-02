using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Employee;

namespace DirRX.OverrideSolution.Client
{
  partial class EmployeeFunctions
  {

    /// <summary>
    /// Изменение доступности свойств и вкладок на форме.
    /// </summary>       
    public void UpdateForm()
    {
      var isVirtual = _obj.IsVirtualDirRX == true;
      _obj.State.Pages.Performers.IsVisible = isVirtual;
      _obj.State.Properties.NeedCreateSubstitutionsDirRX.IsVisible = isVirtual;
      
      // Действие "Отправлять Push-уведомления в рабочее время" показывать только если не используются настройки организации.
      var isVisible = _obj.InheritFromBusinessUnitDirRX == false;
      _obj.State.Properties.SendPushDuringWorkHoursDirRX.IsVisible = isVisible;
    }

  }
}