using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.SubstitutionPersonalAccount;

namespace DirRX.NNOtherProcesses.Server
{
  partial class SubstitutionPersonalAccountFunctions
  {

    /// <summary>
    /// Создать запись в справочнике "Замещение ЛК".
    /// </summary>
    /// <param name="info">Информация о замещении.</param>
    /// <param name="employee">Замещаемый сотрудник.</param>
    /// <param name="taskId">ИД задачи.</param>
    /// <returns>Запись справочника "Замещение ЛК".</returns>
    [Public]
    public static DirRX.NNOtherProcesses.ISubstitutionPersonalAccount CreateRecord(DirRX.NNOtherProcesses.Structures.SubstitutionPersonalAccount.ISubstitutionInfo info, DirRX.OverrideSolution.IEmployee employee, long taskId)
    {
      var record = DirRX.NNOtherProcesses.SubstitutionPersonalAccounts.Create();
      
      record.Name = DirRX.NNOtherProcesses.SubstitutionPersonalAccounts
        .Resources.PropertyNameFormat(DirRX.NNOtherProcesses.SubstitutionPersonalAccounts.Info.Properties.Substitute.LocalizedName,
                                      info.Substitute.DisplayValue,
                                      employee.DisplayValue,
                                      info.StartDate.ToString("dd.MM.yyyy"),
                                      info.EndDate.ToString("dd.MM.yyyy"),
                                      info.SubstitutionSystem.DisplayValue);
      record.Created = Calendar.Now;
      record.Employee = employee;
      record.Substitute = info.Substitute;
      record.StartDate = info.StartDate;
      record.EndDate = info.EndDate;
      record.SubstitutionSystem = info.SubstitutionSystem;
      record.SourceTask = taskId.ToString();
      
      if (string.Equals(info.SubstitutionSystem.SystemId, DirRX.NNOtherProcesses.PublicConstants.SubstitutionSystem.KedoSystemId, StringComparison.OrdinalIgnoreCase))
        record.SentStatus = DirRX.NNOtherProcesses.SubstitutionPersonalAccount.SentStatus.Assigned;
      
      record.Save();
      
      return record;
    }

  }
}