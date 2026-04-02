using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentKind;

namespace DirRX.OverrideSolution.Server
{
  partial class DocumentKindFunctions
  {

    /// <summary>
    /// Получить вид документа по коду SAP.
    /// </summary>
    /// <param name="docKindSAPCode">Код SAP вида документа.</param>
    /// <returns>Вид документа.</returns>
    [Public]
    public static IDocumentKind GetDocKindBySAPCode(string docKindSAPCode)
    {
      return DocumentKinds.GetAll(k => k.SAPCodeDirRX == docKindSAPCode).FirstOrDefault();
    }

    /// <summary>
    /// Получить связанные роли.
    /// </summary>
    /// <returns>Роли.</returns>
    [Remote(IsPure = true)]
    public IQueryable<NNCommons.IBusinessRole> GetBusinessRolesLinks()
    {
      return NNCommons.BusinessRoles.GetAll(x => x.Status == OverrideSolution.DocumentKind.Status.Active && Equals(x.DocumentKind, _obj));
    }

    /// <summary>
    /// Получить Guid вида документа.
    /// </summary>
    /// <returns>Guid вида документа.</returns>
    [Public]
    public override string GetDocumentKindGuid()
    {
      // HACK: Seleznyov_DV. Повесил атрибут Public, чтобы можно было пользоваться коробочной фунцией в других модулях.
      return base.GetDocumentKindGuid();
    }

  }
}