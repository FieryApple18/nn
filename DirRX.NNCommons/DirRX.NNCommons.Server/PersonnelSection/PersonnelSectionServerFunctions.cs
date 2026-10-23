using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.PersonnelSection;

namespace DirRX.NNCommons.Server
{
  partial class PersonnelSectionFunctions
  {
    /// <summary>
    /// Получить Раздел персонала по коду.
    /// </summary>
    /// <param name="code">Код.</param>
    /// <returns>Раздел персонала.</returns>
    [Public, Remote(IsPure = true)]
    public static IPersonnelSection GetPersonnelSectionByCode(string code)
    {
      return PersonnelSections.GetAll(ps => ps.Code == code).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить список дублирующихся записей.
    /// </summary>
    /// <returns>Список дублей.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<IPersonnelSection> GetDuplicates()
    {
      return PersonnelSections.GetAll(ps => ps.Id != _obj.Id && ps.Code == _obj.Code);
    }
  }
}