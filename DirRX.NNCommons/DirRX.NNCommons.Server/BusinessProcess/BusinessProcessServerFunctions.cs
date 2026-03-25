using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.BusinessProcess;
using DirRX.OverrideSolution;

namespace DirRX.NNCommons.Server
{
  partial class BusinessProcessFunctions
  {
    /// <summary>
    /// Получить подчиненные бизнес-процессы.
    /// </summary>
    /// <param name="businessProcess">Бизнес-процессы.</param>
    /// <returns>Подчиненные бизнес-процессы.</returns>
    [Public]
    public static List<IBusinessProcess> GetSubBusinessProcesses(List<IBusinessProcess> headBusinessProcess)
    {
      var subBusinessProcesses = BusinessProcesses.GetAll().Where(x => x.Status == DirRX.OverrideSolution.Department.Status.Active && headBusinessProcess.Contains(x.ProcessGroup)).ToList();
      if (subBusinessProcesses.Any())
        subBusinessProcesses.AddRange(GetSubBusinessProcesses(subBusinessProcesses));
      
      return subBusinessProcesses.Distinct().ToList();
    }
    
    /// <summary>
    /// Получить связанные справочники.
    /// </summary>
    /// <returns>Связанные справочники.</returns>
    [Remote(IsPure = true)]
    public List<Sungero.CoreEntities.IDatabookEntry> GetBusinessProcessLinks()
    {
      var docKinds = DocumentKinds.GetAll(x => x.Status == OverrideSolution.DocumentKind.Status.Active && Equals(x.BusinessProcessDirRX, _obj));
      var businessProcesses = BusinessProcesses.GetAll(x => x.Status == NNCommons.BusinessProcess.Status.Active && Equals(x.ProcessGroup, _obj));
      var businessRoles = BusinessRoles.GetAll(x => x.Status == NNCommons.BusinessRole.Status.Active && Equals(x.Process, _obj));
      var records = new List<Sungero.CoreEntities.IDatabookEntry>();
      records.AddRange(docKinds);
      records.AddRange(businessProcesses);
      records.AddRange(businessRoles);
      return records;
    }
  }
}