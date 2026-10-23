using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.BusinessRole;

namespace DirRX.NNCommons.Shared
{
  partial class BusinessRoleFunctions
  {
    /// <summary>
    /// Заполнить имя справочника.
    /// </summary>
    [Public]
    public virtual void FillName()
    {
      var name = string.Empty;
      var ngdName = _obj.Top == true ? DirRX.NNCommons.BusinessRoles.Resources.NGDName : DirRX.NNCommons.BusinessRoles.Resources.NotNGDName;

      if (_obj.Process != null && _obj.DocumentKind == null)
        name = BusinessRoles.Resources.NameTemplateFormat(_obj.Process?.Name, ngdName);
      else if (_obj.DocumentKind != null && _obj.Process == null)
        name = BusinessRoles.Resources.NameTemplateFormat(_obj.DocumentKind?.Name, ngdName);
      else
        name = DirRX.NNCommons.BusinessRoles.Resources.RecordNameAutotext;
      
      _obj.Name = name;
      _obj.State.Properties.Name.IsEnabled = false;
    }

    /// <summary>
    /// Установить состояние свойств "Процесс" и "Вид документа" в зависимости от состояния "Тип роли".
    /// </summary>
    [Public]
    public virtual void SetStatePropertyDependOnRoleType()
    {
      var isByProcess = _obj.RoleType == RoleType.ByProcess;
      _obj.State.Properties.Process.IsEnabled = isByProcess;
      _obj.State.Properties.Process.IsRequired = isByProcess;
      _obj.State.Properties.DocumentKind.IsEnabled = !isByProcess;
      _obj.State.Properties.DocumentKind.IsRequired = !isByProcess;
    }
  }
}