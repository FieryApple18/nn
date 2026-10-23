using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.PersonnelSubSection;

namespace DirRX.NNCommons.Client
{
  partial class PersonnelSubSectionActions
  {
    public virtual void ShowDuplicates(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var duplicates = PublicFunctions.PersonnelSubSection.Remote.GetDuplicates(_obj);
      duplicates.Show();
    }

    public virtual bool CanShowDuplicates(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }


}