using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.DocumentsVisabilityLimitation;

namespace DirRX.NNCommons
{
  partial class DocumentsVisabilityLimitationSharedHandlers
  {

    public virtual void BusinessUnitChanged(DirRX.NNCommons.Shared.DocumentsVisabilityLimitationBusinessUnitChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        PublicFunctions.DocumentsVisabilityLimitation.FillName(_obj);
    }

    public virtual void RoleChanged(DirRX.NNCommons.Shared.DocumentsVisabilityLimitationRoleChangedEventArgs e)
    {
      if (e.NewValue != e.OldValue)
        PublicFunctions.DocumentsVisabilityLimitation.FillName(_obj);
    }

  }
}