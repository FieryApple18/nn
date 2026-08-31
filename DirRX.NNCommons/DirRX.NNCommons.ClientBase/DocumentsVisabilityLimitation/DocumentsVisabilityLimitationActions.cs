using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.DocumentsVisabilityLimitation;

namespace DirRX.NNCommons.Client
{
  partial class DocumentsVisabilityLimitationActions
  {
    public virtual void ShowDuplicates(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var duplicates = Functions.DocumentsVisabilityLimitation.Remote.GetDuplicates(_obj);
      if (duplicates.Any())
        duplicates.Show();
      else
        Dialogs.NotifyMessage(Sungero.Commons.Resources.DuplicateNotFound);
    }

    public virtual bool CanShowDuplicates(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

  internal static class DocumentsVisabilityLimitationEmployeesStaticActions
  {

    public static bool CanFillEmployees(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public static void FillEmployees(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      // Занесение сотрудников в табличную часть Состав.
      var record = DocumentsVisabilityLimitations.As(e.Entity);
      var selectedEmployees = Functions.DocumentsVisabilityLimitation.Remote.GetEmployeesList(record).ShowSelectMany().ToList();
      Functions.DocumentsVisabilityLimitation.AddEmployees(record, selectedEmployees);
    }
  }

}