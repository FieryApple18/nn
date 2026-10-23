using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.JobTitle;

namespace DirRX.OverrideSolution.Server
{
  partial class JobTitleFunctions
  {

    /// <summary>
    /// Получить дубли должности.
    /// </summary>
    /// <returns>Список дублей должности.</returns>
    [Remote(IsPure = true)]
    public override List<Sungero.Company.IJobTitle> GetDuplicates()
    {
      return Sungero.Company.JobTitles.GetAll()
        .Where(x => (x.Name.Trim() == _obj.Name.Trim()) && Sungero.Company.Departments.Equals(x.Department, _obj.Department) &&
               JobTitles.As(x).SapIdDirRX == _obj.SapIdDirRX && !Sungero.Company.JobTitles.Equals(x, _obj))
        .ToList();
    }

    /// <summary>
    /// Получить сотрудников с такой же должностью, но другим подразделением.
    /// </summary>
    /// <returns>Список сотрудников с такой же должностью, но другим подразделением.</returns>
    [Remote(IsPure = true)]
    public override List<Sungero.Company.IEmployee> GetEmployeesWithSameJobTitle()
    {
      var employees = Sungero.Company.Employees.GetAll().Where(x => Equals(x.JobTitle, _obj) && !Equals(x.Department, _obj.Department)).ToList();
      
      if (employees.Any())
      {
        var employeesIds = string.Join(", ", employees.Select(x => x.Id.ToString()));
        
        Logger.DebugFormat("GetEmployeesWithSameJobTitle. ИД должности: {0}. Список ИД сотрудников с такой же должностью, но другим подразделением: {1}.", _obj.Id,
                           employeesIds);
      }
      
      return employees;
    }

    /// <summary>
    /// Получить значение перечисления "Уровень штатной должности" по его коду.
    /// </summary>
    /// <param name="positionLevel">Код уровня штатной должности.</param>
    /// <returns>Значение перечисления "Уровень штатной должности".</returns>
    [Public]
    public Sungero.Core.Enumeration GetJobTitlePositionLevel(string positionLevel)
    {
      var result = PositionLevelDirRX.RCCN2Lower;
      
      if (positionLevel == Constants.Company.JobTitle.PositionLevelCodes.N)
        result = PositionLevelDirRX.RCCN;
      else if (positionLevel == Constants.Company.JobTitle.PositionLevelCodes.N1)
        result = PositionLevelDirRX.RCCN1;
      else if (positionLevel == Constants.Company.JobTitle.PositionLevelCodes.N2)
        result = PositionLevelDirRX.RCCN2;
      
      return result;
    }
  }
}