using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.BusinessRole;

namespace DirRX.NNCommons
{
  partial class BusinessRoleSharedHandlers
  {

    public virtual void TopChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        PublicFunctions.BusinessRole.FillName(_obj);
    }

    public virtual void DocumentKindChanged(DirRX.NNCommons.Shared.BusinessRoleDocumentKindChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        PublicFunctions.BusinessRole.FillName(_obj);
    }

    public virtual void ProcessChanged(DirRX.NNCommons.Shared.BusinessRoleProcessChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        PublicFunctions.BusinessRole.FillName(_obj);
    }

    public virtual void RoleTypeChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
      {
        _obj.Process = null;
        _obj.DocumentKind = null;
      }
      
      Functions.BusinessRole.SetStatePropertyDependOnRoleType(_obj);
    }

  }
}