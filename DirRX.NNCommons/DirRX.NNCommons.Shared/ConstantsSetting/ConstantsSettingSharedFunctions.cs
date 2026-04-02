using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.ConstantsSetting;

namespace DirRX.NNCommons.Shared
{
  partial class ConstantsSettingFunctions
  {

    /// <summary>
    /// Определить состояния полей в карточке.
    /// </summary>
    public virtual void SetPropertiesState()
    {   
      _obj.State.Properties.StringValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.StringType;
      _obj.State.Properties.IntegerValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.IntegerType;
      _obj.State.Properties.DoubleValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.DoubleType;
      _obj.State.Properties.DateValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.DateType;
      _obj.State.Properties.PasswordValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.PasswordType;
      _obj.State.Properties.IdentifierValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.IdentifierType;
      _obj.State.Properties.BoolValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.BoolType;
      _obj.State.Properties.BusinessUnitValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.BusUnitType;
      _obj.State.Properties.JobTitleValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.JobTitleType;
      _obj.State.Properties.EmployeeValue.IsVisible = _obj.Type == NNCommons.ConstantsSetting.Type.EmployeeType;

      var isSystem = _obj.IsSystem == true;
      _obj.State.Properties.Type.IsEnabled = !isSystem;
      _obj.State.Properties.Name.IsEnabled = !isSystem;
      _obj.State.Properties.ConstantsGroup.IsEnabled = !isSystem;
    }
    
  }
}