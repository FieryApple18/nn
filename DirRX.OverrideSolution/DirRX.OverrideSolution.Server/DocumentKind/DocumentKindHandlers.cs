using System;
using System.Collections.Generic;
using System.Linq;
using DirRX.NNCommons;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentKind;
using Sungero.Domain.LinqExpressions;

namespace DirRX.OverrideSolution
{
  partial class DocumentKindBusinessProcessDirRXPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> BusinessProcessDirRXFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(process => !BusinessProcesses.GetAll().Any(child => child.ProcessGroup.Id == process.Id));
    }

  }

  partial class DocumentKindServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (!_obj.State.IsInserted && _obj.Status == DocumentKind.Status.Closed &&
          Functions.DocumentKind.GetBusinessRolesLinks(_obj).Any())
      {
        e.AddError(DocumentKinds.Resources.FindBusinessRolesLinksError, DocumentKinds.Info.Actions.GetBusinessRolesLinksDirRX);
      }

      var isVisualMode = ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params.ContainsKey(DirRX.OverrideSolution.PublicConstants.Docflow.DocumentKind.IsVisualModeParamName) == true;

      if (_obj.BusinessProcessDirRX == null && isVisualMode)
      {
        var docKindGuid = PublicFunctions.DocumentKind.GetDocumentKindGuid(_obj);
        var nnDocKindList = NNCommons.PublicFunctions.Module.GetNNDocKindsGuidsList();
        var baseDocKindList = NNCommons.PublicFunctions.Module.GetBaseDocKindsGuidsList();
        if (!string.IsNullOrEmpty(docKindGuid) && ((nnDocKindList.Contains(Guid.Parse(docKindGuid)) || baseDocKindList.Contains(Guid.Parse(docKindGuid)))))
          e.AddError(DirRX.OverrideSolution.DocumentKinds.Resources.BusinessProcessNotFieldErrorFormat(_obj.Info.Properties.BusinessProcessDirRX.LocalizedName));
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.NeedCancelActionDirRX = false;
      _obj.TransferBody = false;
    }

  }
}
