using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocRelatedToWorkBase;

namespace DirRX.OverrideSolution.Client
{
  partial class DocRelatedToWorkBaseVersionsActions
  {

    public virtual bool CanComparingVersionsDirRX(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return _obj.AssociatedApplication != null &&
        (_obj.AssociatedApplication.Extension == NNCommons.PublicConstants.Module.ExtensionsForComparison.Pdf || _obj.AssociatedApplication.Extension == NNCommons.PublicConstants.Module.ExtensionsForComparison.Doc || _obj.AssociatedApplication.Extension == NNCommons.PublicConstants.Module.ExtensionsForComparison.Docx);
    }

    public virtual void ComparingVersionsDirRX(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      // Список версий, доступных для сравнения.
      string[] supportedExtensions = {NNCommons.PublicConstants.Module.ExtensionsForComparison.Pdf, NNCommons.PublicConstants.Module.ExtensionsForComparison.Doc, NNCommons.PublicConstants.Module.ExtensionsForComparison.Docx};
      var versions = DocRelatedToWorkBases.As(_obj.RootEntity).Versions
        .Where(x => x.Number != _obj.Number && supportedExtensions.Contains(x.AssociatedApplication.Extension));
      
      // Если нет версий доступных для сравнений, то отобразить ошибку.
      if (!versions.Any())
      {
        Dialogs.ShowMessage(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.ComparingVersionDialogVersionsNotFoundFormat(string.Join(", ", supportedExtensions), MessageType.Information));
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.ComparingVersionsDialogTitle);
      
      // Выбранная версия.
      var selectedVersion = dialog.AddString(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.ComparingVersionsDialogSelectedVersion, true);
      selectedVersion.Value = string.Format("{0}. {1}. Изменена: {2}", _obj.Number, _obj.Note, _obj.Modified.Value.ToString("dd.MM.yyyy"));
      selectedVersion.IsEnabled = false;
      
      // Версия для сравнения.
      string[] versionsList = versions
        .Select(x => string.Format("{0}. {1}. Изменена: {2}", x.Number, x.Note, x.Modified.Value.ToString("dd.MM.yyyy"))).ToArray();
      var comparisonVersion = dialog.AddSelect(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.ComparingVersionsDialogComparingVersion, true, string.Empty).From(versionsList);
      
      // Сравнение.
      if(dialog.Show() == DialogButtons.Ok)
      {
        var comparisonVersionNumber = versions.FirstOrDefault(x => string.Format("{0}. {1}. Изменена: {2}", x.Number, x.Note, x.Modified.Value.ToString("dd.MM.yyyy")) == comparisonVersion.Value).Number;
        var user = Users.Current;
        var doc = DirRX.NNCommons.PublicFunctions.Module.Remote.CompareVersionsDocument(_obj.RootEntity.Id, _obj.Number.Value, comparisonVersionNumber.Value, user.ToString());
        if(doc.Versions.Any())
          doc.LastVersion.Open();
        else
          Dialogs.ShowMessage("Не удалось выполнить сравнение версий.", MessageType.Warning);
      }
    }

  }

  partial class DocRelatedToWorkBaseCollectionActions
  {

    public override void OpenDocumentRead(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var doc = DocRelatedToWorkBases.As(e.Entity);
      var hasRights = Functions.DocRelatedToWorkBase.Remote.HasRightsOnEDoc(doc);
      if (!hasRights)
        throw AppliedCodeException.Create(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.RoleModelUserHasNoRightsOnEDocMessage);
      
      base.OpenDocumentRead(e);
    }

    public override bool CanOpenDocumentRead(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanOpenDocumentRead(e);
    }

    public virtual bool CanSendForRestampingDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Sungero.Docflow.PublicFunctions.Module.Remote.IsAdministrator();
    }

    public virtual void SendForRestampingDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      // Получить информацию по доступным вариантам процесса для прештамповки.
      var processKinds = PublicFunctions.DocRelatedToWorkBase.Remote.GetRestampingProcessKinds();
      if (!processKinds.Any())
      {
        Dialogs.ShowMessage(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.RestampingProcessKindsNotFound);
        return;
      }

      // Выбор варианта процесса.
      var processKind = Functions.DocRelatedToWorkBase.SelectRestampingProcessKind(processKinds);
      if (processKind == null)
        return;
      
      // Старт АО по запуску задач для повторной проштамповки документов.
      Functions.DocRelatedToWorkBase.Remote.StartRestampingAsyncHandler(_objs.Select(x => x.Id).ToList(), processKind.Id);
      Dialogs.ShowMessage(DirRX.OverrideSolution.DocRelatedToWorkBases.Resources.RestampingActionResultInfo);
    }

  }
}