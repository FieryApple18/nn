using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.PersonnelSection;

namespace DirRX.NNCommons
{
  partial class PersonnelSectionServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.PersonnelSection.Remote.GetDuplicates(_obj).Any())
        e.AddError(DirRX.NNCommons.PersonnelSections.Resources.DuplicatesError, _obj.Info.Actions.ShowDuplicates);
    }
  }

}