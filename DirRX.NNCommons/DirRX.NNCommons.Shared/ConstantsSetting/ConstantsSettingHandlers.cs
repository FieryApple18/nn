using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.ConstantsSetting;

namespace DirRX.NNCommons
{
  partial class ConstantsSettingSharedHandlers
  {

    public virtual void PasswordValueChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      _obj.PasswordTemp = e.NewValue;
    }

    public virtual void TypeChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        if (e.NewValue != NNCommons.ConstantsSetting.Type.BoolType && _obj.BoolValue != false)
          _obj.BoolValue = false;
        if (e.NewValue != NNCommons.ConstantsSetting.Type.StringType && _obj.StringValue != null)
          _obj.StringValue = string.Empty;
        if (e.NewValue != NNCommons.ConstantsSetting.Type.DateType && _obj.DateValue != null)
          _obj.DateValue = null;
        if (e.NewValue != NNCommons.ConstantsSetting.Type.DoubleType && _obj.DoubleValue != null)
          _obj.DoubleValue = null;
        if (e.NewValue != NNCommons.ConstantsSetting.Type.IdentifierType && _obj.IdentifierValue != null)
          _obj.IdentifierValue = null;
        if (e.NewValue != NNCommons.ConstantsSetting.Type.IntegerType && _obj.IntegerValue != null)
          _obj.IntegerValue = null;
        if (e.NewValue != NNCommons.ConstantsSetting.Type.PasswordType && _obj.PasswordValue != null)
          _obj.PasswordValue = null;
        if (e.NewValue != NNCommons.ConstantsSetting.Type.BusUnitType && _obj.BusinessUnitValue != null)
          _obj.BusinessUnitValue = null;
        if (e.NewValue != NNCommons.ConstantsSetting.Type.EmployeeType && _obj.EmployeeValue != null)
          _obj.EmployeeValue = null;
        if (e.NewValue != NNCommons.ConstantsSetting.Type.JobTitleType && _obj.JobTitleValue != null)
         _obj.JobTitleValue = null;
      }
      
      Functions.ConstantsSetting.SetPropertiesState(_obj);
    }

  }
}