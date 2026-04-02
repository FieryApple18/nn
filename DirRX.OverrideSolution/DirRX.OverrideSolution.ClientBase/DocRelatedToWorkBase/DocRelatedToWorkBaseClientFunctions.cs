using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocRelatedToWorkBase;
using Sungero.Domain.Client;
using CommonLibrary;

namespace DirRX.OverrideSolution.Client
{
  partial class DocRelatedToWorkBaseFunctions
  {

    /// <summary>
    /// Отобразить диалог для выбора варианта процесса для повторной проштамповки.
    /// </summary>
    /// <param name="processKinds">Доступные для выбора варианты процесса.</param>
    /// <returns>Выбранный вариант процесса.</returns>
    public static Sungero.Workflow.IProcessKind SelectRestampingProcessKind(IQueryable<Sungero.Workflow.IProcessKind> processKinds)
    {
      var dialog = Dialogs.CreateInputDialog(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.RestampingSelectProcessKindDialogTitle);
      var processKindsIds = processKinds.Select(x => x.Id).ToList();
      var processKindProperty = dialog.AddSelect(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.RestampingSelectProcessKindDialogPropertyName, true).From(processKinds.Select(x => x.Name).ToArray());
      
      if (dialog.Show() != DialogButtons.Ok)
        return Sungero.Workflow.ProcessKinds.Null;
      
      var processKindId = processKindsIds.ElementAtOrDefault(processKindProperty.ValueIndex);
      var processKind = processKinds.Where(x => x.Id == processKindId).FirstOrDefault();
      
      return processKind;
    }

    /// <summary>
    /// Проверить права на документ по ролевой модели.
    /// </summary>
    public virtual void CheckRightsOnEDoc(Sungero.Presentation.FormShowingEventArgs e)
    {
      var hasRights = Functions.DocRelatedToWorkBase.Remote.HasRightsOnEDoc(_obj);
      
      if (!hasRights)
        throw AppliedCodeException.Create(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.RoleModelUserHasNoRightsOnEDocMessage);
    }

  }
}