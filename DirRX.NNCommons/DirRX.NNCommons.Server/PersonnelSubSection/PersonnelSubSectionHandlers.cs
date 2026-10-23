using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.PersonnelSubSection;

namespace DirRX.NNCommons
{
  partial class PersonnelSubSectionServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (PublicFunctions.PersonnelSubSection.Remote.GetDuplicates(_obj).Any())
        e.AddError(DirRX.NNCommons.PersonnelSubSections.Resources.DuplicatesError, _obj.Info.Actions.ShowDuplicates);
    }
  }

}