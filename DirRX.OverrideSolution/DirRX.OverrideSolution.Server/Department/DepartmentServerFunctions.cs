using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Department;

namespace DirRX.OverrideSolution.Server
{
  partial class DepartmentFunctions
  {
    #region Функции вычисляемых выражений.
    
    /// <summary>
    /// Expression-функция для проверки, входит ли подразделение в Подразделения инфраструктуры ЗФ.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Результат. True - если входит, False - если нет.</returns>
    [Public, ExpressionElement("ExpFunc_IsIncludedInInfrastructureZF_Name", "ExpFunc_IsIncludedInInfrastructureZF_Desc", "")]
    public static bool IsIncludedInInfrastructureZF(IDepartment department)
    {
      var infrastructureZFSapId = NNCommons.PublicFunctions.Module.GetStringValue(NNCommons.PublicConstants.Module.ConstantsSetting.InfrastructureZFSapCode.Guid);
      if (string.IsNullOrEmpty(infrastructureZFSapId))
        Logger.ErrorFormat("IsIncludedInInfrastructureZF. Не заполнена/не найдена прикладная константа {0} по Guid: {1}.",
                           NNCommons.PublicConstants.Module.ConstantsSetting.InfrastructureZFSapCode.Name,
                           NNCommons.PublicConstants.Module.ConstantsSetting.InfrastructureZFSapCode.Guid);

      var dep = department;
      while (dep != null)
      {
        if (dep.SapIdDirRX == infrastructureZFSapId)
          return true;

        dep = Departments.As(dep.HeadOffice);
      }
      return false;
    }
    
    #endregion
  }
}