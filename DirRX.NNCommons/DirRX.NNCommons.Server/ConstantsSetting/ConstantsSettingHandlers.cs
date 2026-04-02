using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.ConstantsSetting;

namespace DirRX.NNCommons
{
  partial class ConstantsSettingServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.PasswordTemp = null;
      _obj.IsSystem = false;
      _obj.BoolValue = false;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (_obj.Type == NNCommons.ConstantsSetting.Type.StringType)
        _obj.PublicValue = _obj.Value = _obj.StringValue;
      if (_obj.Type == NNCommons.ConstantsSetting.Type.IntegerType)
        _obj.PublicValue = _obj.Value = _obj.IntegerValue.ToString();
      if (_obj.Type == NNCommons.ConstantsSetting.Type.DateType)
        _obj.PublicValue = _obj.Value = _obj.DoubleValue.ToString();
      if (_obj.Type == NNCommons.ConstantsSetting.Type.IdentifierType)
        _obj.PublicValue = _obj.Value = _obj.IdentifierValue.ToString();
      if (_obj.Type == NNCommons.ConstantsSetting.Type.DateType && _obj.DateValue.HasValue)
        _obj.PublicValue = _obj.Value = _obj.DateValue.Value.ToShortDateString();
      if (_obj.Type == NNCommons.ConstantsSetting.Type.BoolType && _obj.BoolValue.HasValue)
        _obj.PublicValue = _obj.Value = _obj.BoolValue.ToString();
      if (_obj.Type ==  NNCommons.ConstantsSetting.Type.PasswordType && !_obj.PasswordTemp.Equals(Constants.ConstantsSetting.HidePasswordValue))
      {
        _obj.Value = Encryption.Encrypt(_obj.PasswordTemp);
        _obj.PasswordTemp = string.Empty;
        _obj.PasswordValue = Constants.ConstantsSetting.HidePasswordValue;
        _obj.PublicValue = Constants.ConstantsSetting.HidePasswordValue;
      } 
      if (_obj.Type == NNCommons.ConstantsSetting.Type.BusUnitType && _obj.BusinessUnitValue != null)
        _obj.PublicValue = _obj.Value = _obj.BusinessUnitValue.ToString();
      if (_obj.Type == NNCommons.ConstantsSetting.Type.EmployeeType && _obj.EmployeeValue != null)
        _obj.PublicValue = _obj.Value = _obj.EmployeeValue.ToString();
      if (_obj.Type == NNCommons.ConstantsSetting.Type.JobTitleType && _obj.JobTitleValue != null)
        _obj.PublicValue = _obj.Value = _obj.JobTitleValue.ToString();
    }
  }

}