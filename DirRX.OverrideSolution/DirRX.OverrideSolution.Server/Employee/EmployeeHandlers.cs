using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Employee;
using Sungero.Domain.Shared;

namespace DirRX.OverrideSolution
{
  partial class EmployeeServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.IsVirtualDirRX = false;
      _obj.NeedCreateSubstitutionsDirRX = false;
      _obj.NeedClearPhoneDirRX = false;
      _obj.IsNGDDirRX = false;
      _obj.SendPushDuringWorkHoursDirRX = false;
    }

    public override void AfterSave(Sungero.Domain.AfterSaveEventArgs e)
    {
      base.AfterSave(e);

      var needStartAsyncHandlers = false;
      var needExcludeEmployeeFromTimeZone = false;
      
      if (e.Params.TryGetValue(Constants.HRProSolution.Employee.NeedStartUpdateSubstitutionsForVirtualEmployeeDirRX, out needStartAsyncHandlers) && needStartAsyncHandlers)
      {
        var asyncHandler = OverrideSolution.Module.HRDocFlow.AsyncHandlers.UpdateSubstitutionsForVirtualEmployeeDirRX.Create();
        asyncHandler.EmployeeID = _obj.Id;
        asyncHandler.ExecuteAsync();
        
        e.Params.Remove(Constants.HRProSolution.Employee.NeedStartUpdateSubstitutionsForVirtualEmployeeDirRX);
      }
      
      if (e.Params.TryGetValue(Constants.HRProSolution.Employee.NeedExcludeEmployeeFromTimeZone, out needExcludeEmployeeFromTimeZone) && needExcludeEmployeeFromTimeZone)
      {
        NNCommons.PublicFunctions.Module.ExcludeEmployeeFromTimeZone(_obj);
      }
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // HACK Отключено предупреждение о перевыпуске сертификата при смене номера.
      /*var entityParams = ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params;
      var key = EssPlatform.PublicConstants.Module.PhoneEmailChangedParamName;
      if (entityParams.ContainsKey(key))
        entityParams[key] = false;
      else
        entityParams.Add(key, false);*/
      
      Functions.Employee.SetIsTempDisconnectParam(_obj);
      base.BeforeSave(e);

      // Если изменился признак "Создать замещения" и новое значение отличается от старого.
      var needStartAsyncHandlersPart1 = _obj.State.Properties.NeedCreateSubstitutionsDirRX.IsChanged &&
        _obj.State.Properties.NeedCreateSubstitutionsDirRX.OriginalValue.HasValue &&
        _obj.State.Properties.NeedCreateSubstitutionsDirRX.OriginalValue.Value != _obj.NeedCreateSubstitutionsDirRX.Value;
      
      // Если изменились "Исполнители" и установлен признак "Создать замещения".
      var needStartAsyncHandlersPart2 = _obj.NeedCreateSubstitutionsDirRX.HasValue &&
        _obj.NeedCreateSubstitutionsDirRX.Value &&
        _obj.State.Properties.PerformersDirRX.IsChanged;
      
      if (needStartAsyncHandlersPart1 || needStartAsyncHandlersPart2)
        e.Params.AddOrUpdate(Constants.HRProSolution.Employee.NeedStartUpdateSubstitutionsForVirtualEmployeeDirRX, true);
      
      // Если пользователя отключили от ЛК и установлен признак очистки номера телефона, то выпонить её.
      if (_obj.State.Properties.PersonalAccountStatusDirRX.IsChanged &&
          _obj.PersonalAccountStatusDirRX == Employee.PersonalAccountStatusDirRX.InviteIsNotSent &&
          _obj.NeedClearPhoneDirRX == true)
      {
        _obj.NeedClearPhoneDirRX = false;
        _obj.PersonalPhoneDirRX = string.Empty;
      }
      
      // Вычислить необходимость синхронизации сотрудников персоны.
      Functions.Employee.NeedUpdatePersonEmployees(_obj);
      
      // Исключение сотрудника из часового пояса, если запись сотрудника закрывается.
      if (!_obj.State.IsInserted && _obj.State.Properties.Status.IsChanged && _obj.Status == Employee.Status.Closed)
        e.Params.AddOrUpdate(Constants.HRProSolution.Employee.NeedExcludeEmployeeFromTimeZone, true);
    }

  }
}