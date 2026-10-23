using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.PersonnelSubSection;

namespace DirRX.NNCommons.Server
{
  partial class PersonnelSubSectionFunctions
  {
    /// <summary>
    /// Получить Подраздел персонала по коду.
    /// </summary>
    /// <param name="code">Код.</param>
    /// <returns>Подраздел персонала.</returns>
    [Public, Remote(IsPure = true)]
    public static IPersonnelSubSection GetPersonnelSubSectionByCode(string code)
    {
      return PersonnelSubSections.GetAll(ps => ps.Code == code).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить список дублирующихся записей.
    /// </summary>
    /// <returns>Список дублей.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<IPersonnelSubSection> GetDuplicates()
    {
      return PersonnelSubSections.GetAll(ps => ps.Id != _obj.Id && ps.Code == _obj.Code);
    }
  }
}