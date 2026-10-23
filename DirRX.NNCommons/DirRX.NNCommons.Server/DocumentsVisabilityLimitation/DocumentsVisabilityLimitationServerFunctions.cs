using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;
using DirRX.NNCommons.DocumentsVisabilityLimitation;

namespace DirRX.NNCommons.Server
{
  partial class DocumentsVisabilityLimitationFunctions
  {

    /// <summary>
    /// Получить список действующих ограничений видимости документов для сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Список ограничений видимости документов.</returns>
    [Public, Remote]
    public static IQueryable<IDocumentsVisabilityLimitation> ActionGetDocumentsVisabilityLimitations(IEmployee employee)
    {
      return Functions.DocumentsVisabilityLimitation.GetDocumentsVisabilityLimitations(employee, BusinessUnits.Null);
    }
    
    /// <summary>
    /// Получить список доступных видов документов.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Список видов документов.</returns>
    [Public, Remote(IsPure = true)]
    public static IQueryable<IDocumentKind> GetAvailableDocumentKinds(DirRX.OverrideSolution.IEmployee employee)
    {
      var documentKindsIds = GetAvailableDocumentKindsInfo(employee).Select(d => d.DocumentKindId);
      return DocumentKinds.GetAll(d => documentKindsIds.Contains(d.Id));
    }
    
    /// <summary>
    /// Сформировать список ИД доступных видов документов и их признака "НГД".
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Информацию по ИД видов документов и признака "НГД".</returns>
    [Public]
    public static List<Structures.DocumentsVisabilityLimitation.IDocumentKindsInfo> GetAvailableDocumentKindsInfo(DirRX.OverrideSolution.IEmployee employee)
    {
      var availibleDocumentKindsInfoList = new List<NNCommons.Structures.DocumentsVisabilityLimitation.IDocumentKindsInfo> ();
      var generalRolesList = PublicFunctions.DocumentsVisabilityLimitation.GetDocumentsVisabilityLimitations(employee, BusinessUnits.Null)
        .Select(l => l.Role)
        .ToList();
      
      var businessRolesByKind = BusinessRoles.GetAll(b => b.Status == NNCommons.BusinessRole.Status.Active &&
                                                     b.RoleType == NNCommons.BusinessRole.RoleType.ByKind &&
                                                     b.RecipientLinks.Any(r => generalRolesList.Contains(GeneralRoles.As(r.Member))));
      
      var businessRolesByProcess = BusinessRoles.GetAll(b => b.Status == NNCommons.BusinessRole.Status.Active &&
                                                        b.RoleType == NNCommons.BusinessRole.RoleType.ByProcess &&
                                                        b.RecipientLinks.Any(r => generalRolesList.Contains(GeneralRoles.As(r.Member)))).ToList();
      
      if (businessRolesByKind.Count() == 0 && businessRolesByProcess.Count() == 0)
      {
        return availibleDocumentKindsInfoList;
      }
      
      foreach (var roles in businessRolesByProcess.GroupBy(x => x.Top))
      {
        var top = roles.First().Top.Value;
        var processes = roles.Select(x => x.Process).ToList();
        processes.AddRange(Functions.BusinessProcess.GetSubBusinessProcesses(processes));
        var docKindsInfo = DocumentKinds.GetAll(d => d.Status == DirRX.OverrideSolution.DocumentKind.Status.Active && processes.Contains(d.BusinessProcessDirRX))
          .Select(d => Structures.DocumentsVisabilityLimitation.DocumentKindsInfo.Create(d.Id, top));
        if (docKindsInfo.Any())
          availibleDocumentKindsInfoList.AddRange(docKindsInfo);
      }
      
      var documentKindsInfo = businessRolesByKind.Select(x => Structures.DocumentsVisabilityLimitation.DocumentKindsInfo.Create(x.DocumentKind.Id, x.Top.Value));
      if (documentKindsInfo.Any())
        availibleDocumentKindsInfoList.AddRange(documentKindsInfo);
      
      return availibleDocumentKindsInfoList.Distinct().ToList();
    }
    
    /// <summary>
    /// Получить список доступных разделов персонала для сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="availableBusinessUnits">Список ИД доступных НОР.</param>
    /// <returns>Список ИД доступных разделов персонала.</returns>
    [Public]
    public static List<long> GetAvailablePersonnelSections(DirRX.OverrideSolution.IEmployee employee, List<long> availableBusinessUnits)
    {
      return PublicFunctions.DocumentsVisabilityLimitation
        .GetDocumentsVisabilityLimitations(employee, DirRX.OverrideSolution.BusinessUnits.Null)
        .Where(x => x.PersonnelSection != null && availableBusinessUnits.Contains(x.BusinessUnit.Id))
        .Select(x => x.PersonnelSection.Id)
        .Distinct()
        .ToList();
    }
    
    /// <summary>
    /// Получить список ИД НОР, для которых настроено общее ограничение (без раздела персонала).
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="availableBusinessUnits">Список ИД доступных НОР.</param>
    /// <returns>Список ИД НОР с общим ограничением.</returns>
    [Public]
    public static List<long> GetBusinessUnitsWithGeneralLimitation(IEmployee employee, List<long> availableBusinessUnits)
    {
      var allLimitations = PublicFunctions.DocumentsVisabilityLimitation.GetDocumentsVisabilityLimitations(employee, BusinessUnits.Null);
      
      return allLimitations
        .Where(x => x.PersonnelSection == null && availableBusinessUnits.Contains(x.BusinessUnit.Id))
        .Select(x => x.BusinessUnit.Id)
        .Distinct()
        .ToList();
    }
    
    /// <summary>
    /// Получить список ИД НОР, для которых настроено ограничение (c разделом персонала).
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="availableBusinessUnits">Список ИД доступных НОР.</param>
    /// <returns>Список ИД НОР с общим ограничением.</returns>
    [Public]
    public static List<long> GetBusinessUnitsWithoutGeneralLimitation(IEmployee employee, List<long> availableBusinessUnits)
    {
      var allLimitations = PublicFunctions.DocumentsVisabilityLimitation.GetDocumentsVisabilityLimitations(employee, BusinessUnits.Null);
      Logger.DebugFormat("С разделом: {0}", string.Join(",", allLimitations.Select(a => a.Id)));
      return allLimitations
        .Where(x => x.PersonnelSection != null && availableBusinessUnits.Contains(x.BusinessUnit.Id))
        .Select(x => x.BusinessUnit.Id)
        .Distinct()
        .ToList();
    }

    /// <summary>
    /// Получить список ИД разделов персонала, доступных для сотрудника.
    /// Исключаются НОР, для которых есть общее ограничение (без раздела).
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="availableBusinessUnits">Список ИД доступных НОР.</param>
    /// <param name="businessUnitsWithGeneralLimitation">Список ИД НОР с общим ограничением.</param>
    /// <returns>Список ИД разрешенных разделов персонала.</returns>
    [Public]
    public static List<long> GetAllowedPersonnelSectionIds(IEmployee employee, List<long> availableBusinessUnits, List<long> businessUnitsWithGeneralLimitation)
    {
      var allLimitations = PublicFunctions.DocumentsVisabilityLimitation.GetDocumentsVisabilityLimitations(employee, BusinessUnits.Null);
      
      return allLimitations
        .Where(x => x.PersonnelSection != null &&
               availableBusinessUnits.Contains(x.BusinessUnit.Id) &&
               !businessUnitsWithGeneralLimitation.Contains(x.BusinessUnit.Id))
        .Select(x => x.PersonnelSection.Id)
        .Distinct()
        .ToList();
    }

    
    /// <summary>
    /// Получить список доступных НОР для сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Список записей справочника Наши организации, доступных сотруднику.</returns>
    [Public]
    public static List<long> GetAvailableBusinessUnits(DirRX.OverrideSolution.IEmployee employee)
    {
      return GetAvailableBusinessUnits(employee, DirRX.OverrideSolution.BusinessUnits.Null).Select(x => x.Id).ToList();
    }
    
    
    
    /// <summary>
    /// Получить список доступных НОР для сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="businessUnit">Наша организация, для которой нужно проверить доступность.</param>
    /// <returns>Список записей справочника Наши организации, доступных сотруднику.</returns>
    [Public]
    public static IQueryable<IBusinessUnit> GetAvailableBusinessUnits(DirRX.OverrideSolution.IEmployee employee, DirRX.OverrideSolution.IBusinessUnit businessUnit)
    {
      var limitations = PublicFunctions.DocumentsVisabilityLimitation.GetDocumentsVisabilityLimitations(employee, businessUnit);
      if (!limitations.Any())
        return Enumerable.Empty<IBusinessUnit>().AsQueryable();
      
      var businessUnits = limitations.GroupBy(x => x.BusinessUnit).Select(x => x.Key);
      return businessUnits;
    }
    
    /// <summary>
    /// Получить список действующих ограничений видимости документов для сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="businessUnit">Наша организация, для которой нужно получить ограничения видимости у сотрудника.</param>
    /// <returns>Список ограничений видимости документов.</returns>
    /// <remarks>Если параметр businessUnit не указан (равен null), соответствующие условия фильтрации не применяются.
    /// Указание конкретного значения фильтрует ограничения только для этой организации.</remarks>
    [Public]
    public static IQueryable<IDocumentsVisabilityLimitation> GetDocumentsVisabilityLimitations(IEmployee employee, IBusinessUnit businessUnit)
    {
      return DocumentsVisabilityLimitations.GetAll().Where(x => x.Status == NNCommons.DocumentsVisabilityLimitation.Status.Active &&
                                                           x.Employees.Any(e => Employees.Equals(e.Employee, employee)) &&
                                                           (businessUnit == BusinessUnits.Null || BusinessUnits.Equals(businessUnit, x.BusinessUnit)));
    }
    
    /// Отфильтровать список ограничений видимости документов по разделу персонала.
    /// </summary>
    /// <param name="limitations">Ограничения видимости документов.</param>
    /// <param name="personnelSection">Раздел персонала.</param>
    /// <returns>Список ограничений видимости документов.</returns>
    [Public]
    public static IQueryable<IDocumentsVisabilityLimitation> FilterLimitationsByPersonnelSection(IQueryable<IDocumentsVisabilityLimitation> limitations, IPersonnelSection personnelSection)
    {
      var activeLimitations = limitations.Where(x => x.Status == NNCommons.DocumentsVisabilityLimitation.Status.Active);
      
      // Раздел персонала не задан, то подбираются ограничения, у которых раздел персонала не заполнен.
      if (personnelSection == null)
        return activeLimitations.Where(x => x.PersonnelSection == null);
      // Раздел персонала задан, то подбираются ограничения, у которых раздел персонала не заполнен ИЛИ он совпадает с заданным.
      else
        return activeLimitations.Where(x => x.PersonnelSection == null || PersonnelSections.Equals(x.PersonnelSection, personnelSection));
    }
    
    /// <summary>
    /// Получить дубли ограничения видимости документов.
    /// </summary>
    /// <returns>Список ограничений видимости документов.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<IDocumentsVisabilityLimitation> GetDuplicates()
    {
      return DocumentsVisabilityLimitations.GetAll(x =>
                                                   x.Id != _obj.Id &&
                                                   GeneralRoles.Equals(_obj.Role, x.Role) &&
                                                   BusinessUnits.Equals(_obj.BusinessUnit, x.BusinessUnit) &&
                                                   PersonnelSections.Equals(_obj.PersonnelSection, x.PersonnelSection));
    }

    /// <summary>
    /// Получить список сотрудников, которые не указаны в текущей настройке ограничения видимости.
    /// </summary>
    /// <returns>Список сотрудников.</returns>
    [Remote(IsPure = true)]
    public virtual IQueryable<IEmployee> GetEmployeesList()
    {
      var employeesIds = _obj.Employees.Select(x => x.Employee.Id).ToList();
      return Employees.GetAll(e => !employeesIds.Contains(e.Id));
    }

  }
}