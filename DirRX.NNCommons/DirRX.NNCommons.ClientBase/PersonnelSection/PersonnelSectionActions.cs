using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.PersonnelSection;

namespace DirRX.NNCommons.Client
{
  partial class PersonnelSectionActions
  {
    public virtual void ShowDuplicates(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var duplicates = PublicFunctions.PersonnelSection.Remote.GetDuplicates(_obj);
      duplicates.Show();
    }

    public virtual bool CanShowDuplicates(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}