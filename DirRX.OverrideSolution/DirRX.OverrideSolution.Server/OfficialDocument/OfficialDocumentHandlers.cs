using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.OfficialDocument;

namespace DirRX.OverrideSolution
{
  partial class OfficialDocumentServerHandlers
  {

    public override void BeforeSigning(Sungero.Domain.BeforeSigningEventArgs e)
    {
      base.BeforeSigning(e);
      
      if (DirRX.NNCommons.PublicFunctions.Module.IsSimpleCertificate(e.Certificate))
      {
        e.SignatureTargetFormat = SignatureAutoImproveTargetFormat.None;
        
        Logger.Debug(DirRX.OverrideSolution.OfficialDocuments.Resources.SignatureImprovementSuppressedFormat(e.Certificate?.Issuer ?? "NULL", _obj.Id));
      }
    }

  }
}