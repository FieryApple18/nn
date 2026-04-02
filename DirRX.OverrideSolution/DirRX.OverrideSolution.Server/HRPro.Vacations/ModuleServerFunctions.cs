using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.Vacations.Server
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Получить варианты процессов.
    /// </summary>
    /// <returns>Список вариантов процессов.</returns>
    [Remote]
    public override List<Sungero.Workflow.IProcessKind> GetVacationProcessKind()
    {
      var processKinds = base.GetVacationProcessKind();
      var nnVacationsTaskTypesGuid = new HashSet<string>() {"2ed31a9c-1c09-419b-8b46-29127562d8de",
        "89391326-efe5-4944-a7b9-cf30b9347bc3",
        "b15f387b-fd6f-4ddb-a54f-e36a1e9094fc"};
      var nnVacationsProcessKinds = Sungero.Workflow.ProcessKinds.GetAll(pk => nnVacationsTaskTypesGuid.Contains(pk.TaskType)).ToList();
      processKinds.AddRange(nnVacationsProcessKinds);
      
      return processKinds;
    }

    /// <summary>
    /// Получить гуиды типов HR-документов модуля Vacation.
    /// </summary>
    /// <returns>Список гуидов типов HR документов.</returns>
    [Public, Remote(IsPure = true)]
    public override List<Guid> GetHRDocumentTypesGuids()
    {
      var documentTypesGuids = base.GetHRDocumentTypesGuids();
      documentTypesGuids.AddRange(new[]
                                  {
                                    NNVacations.PublicConstants.Module.DocumentTypes.VacationNoticeDocument,
                                    NNVacations.PublicConstants.Module.DocumentTypes.VacationOrderDocument,
                                    NNVacations.PublicConstants.Module.DocumentTypes.VacationScheduleDocument,
                                    NNVacations.PublicConstants.Module.DocumentTypes.VacationStatementDocument
                                  });
      
      return documentTypesGuids;
    }

  }
}