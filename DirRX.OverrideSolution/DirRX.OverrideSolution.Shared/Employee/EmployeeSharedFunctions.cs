using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Employee;

namespace DirRX.OverrideSolution.Shared
{
  partial class EmployeeFunctions
  {

    /// <summary>
    /// Установить значение параметра-признака временного отключения сотрудника от ЛК.
    /// </summary>
    public virtual void SetIsTempDisconnectParam()
    {
      // HACK Если сохранение выполняется в невизуальном режиме, то автоматически заполнять параметр временного отключения значением false,
      // иначе при программном сохранении сотрудника после отключения пользователя, возникает ошибка о необходимости отключения сотрудника от ЛК.
      var entityParams = ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params;
      var isVisualMode = entityParams.ContainsKey(DirRX.OverrideSolution.PublicConstants.Docflow.DocumentKind.IsVisualModeParamName);
      if (!isVisualMode)
      {
        var key = EssPlatform.Resources.ParameterIsTempDisconnectFormat(_obj.Id);
        if (entityParams.ContainsKey(key))
          entityParams[key] = false;
        else
          entityParams.Add(key, false);
      }
    }

  }
}