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
    /// Отменить записи справочника "Замещения ЛК" по записи справочника "Замещение".
    /// Для найденных записей будет установлен "Статус отправки" = "Отменено".
    /// </summary>
    /// <param name="substitution">Запись справочника "Замещение".</param>
    /// <param name="logPrefix">Префикс для логирования.</param>
    [Public]
    public static void CancelRecords(Sungero.CoreEntities.ISubstitution substitution, string logPrefix)
    {
      Logger.DebugFormat("{0}. CancelRecords. Start.", logPrefix);
      
      var records = GetRecords(substitution, logPrefix);
      if (!records.Any())
      {
        Logger.DebugFormat("{0}. CancelRecords. End. Not found records by criteria.", logPrefix);
        return;
      }
      
      foreach (var record in records)
      {
        record.SentStatus = DirRX.NNOtherProcesses.SubstitutionPersonalAccount.SentStatus.Cancelled;
        record.Save();
        
        Logger.DebugFormat("{0}. CancelRecords. Cancelled recordId={1}.", logPrefix, record.Id);
      }
      
      Logger.DebugFormat("{0}. CancelRecords. End.", logPrefix);
    }

    /// <summary>
    /// Получить записи справочника "Замещение ЛК" по записи справочника "Замещение".
    /// </summary>
    /// <param name="substitution">Запись справочника "Замещение".</param>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <returns>Список записей справочника "Замещение ЛК". </returns>
    [Public]
    public static List<DirRX.NNOtherProcesses.ISubstitutionPersonalAccount> GetRecords(Sungero.CoreEntities.ISubstitution substitution, string logPrefix)
    {
      Logger.DebugFormat("{0}. GetRecords. Start.", logPrefix);
      
      var system = DirRX.NNOtherProcesses.PublicFunctions.SubstitutionSystem.GetDefaultRecord();
      if (system == null)
      {
        Logger.DebugFormat("{0}. GetRecords. End. Not found default record in SubstitutionSystem.", logPrefix);
        return new List<DirRX.NNOtherProcesses.ISubstitutionPersonalAccount>();
      }

      var records = DirRX.NNOtherProcesses.SubstitutionPersonalAccounts
        .GetAll(r => r.Employee.Id == substitution.User.Id &&
                r.Substitute.Id == substitution.Substitute.Id &&
                r.SubstitutionSystem.Id == system.Id &&
                r.StartDate.HasValue && r.EndDate.HasValue &&
                r.StartDate.Value == substitution.StartDate.Value &&
                r.EndDate.Value == substitution.EndDate.Value &&
                r.SentStatus == DirRX.NNOtherProcesses.SubstitutionPersonalAccount.SentStatus.Assigned)
        .ToList();

      Logger.DebugFormat("{0}. GetRecords. End. Found {1} records by criteria.", logPrefix, records.Count);
      
      return records;
    }

    /// <summary>
    /// Создать запись в справочнике "Замещение ЛК".
    /// </summary>
    /// <param name="info">Запись справочника "Замещение".</param>
    /// <param name="taskId">ИД задачи.</param>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <returns>Запись справочника "Замещение ЛК".</returns>
    [Public]
    public static DirRX.NNOtherProcesses.ISubstitutionPersonalAccount CreateRecord(Sungero.CoreEntities.ISubstitution substitution, long taskId, string logPrefix)
    {
      Logger.DebugFormat("{0}. CreateRecord. Start.", logPrefix);
      
      var system = DirRX.NNOtherProcesses.PublicFunctions.SubstitutionSystem.GetDefaultRecord();
      if (system == null)
      {
        Logger.DebugFormat("{0}. CreateRecord. End. Not found default record in SubstitutionSystem.", logPrefix);
        return DirRX.NNOtherProcesses.SubstitutionPersonalAccounts.Null;
      }
      
      var record = DirRX.NNOtherProcesses.SubstitutionPersonalAccounts.Create();
      
      record.Name = DirRX.NNOtherProcesses.SubstitutionPersonalAccounts
        .Resources.PropertyNameFormat(DirRX.NNOtherProcesses.SubstitutionPersonalAccounts.Info.Properties.Substitute.LocalizedName,
                                      substitution.Substitute.DisplayValue,
                                      substitution.User.DisplayValue,
                                      substitution.StartDate.Value.ToString("dd.MM.yyyy"),
                                      substitution.EndDate.Value.ToString("dd.MM.yyyy"),
                                      system.DisplayValue);
      record.Created = DirRX.NNVacations.PublicFunctions.Module.GetCreateDateForSubstitution(substitution);
      record.Employee = DirRX.OverrideSolution.Employees.As(substitution.User);
      record.Substitute = DirRX.OverrideSolution.Employees.As(substitution.Substitute);
      record.StartDate = substitution.StartDate;
      record.EndDate = substitution.EndDate;
      record.SubstitutionSystem = system;
      record.SourceTask = taskId.ToString();
      record.SentStatus = DirRX.NNOtherProcesses.SubstitutionPersonalAccount.SentStatus.Assigned;
      
      record.Save();
      
      Logger.DebugFormat("{0}. CreateRecord. End. Created recordId={1}.", logPrefix, record.Id);
      
      return record;
    }

    /// <summary>
    /// Создать запись в справочнике "Замещение ЛК".
    /// </summary>
    /// <param name="info">Информация о замещении.</param>
    /// <param name="employee">Замещаемый сотрудник.</param>
    /// <param name="taskId">ИД задачи.</param>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <returns>Запись справочника "Замещение ЛК".</returns>
    [Public]
    public static DirRX.NNOtherProcesses.ISubstitutionPersonalAccount CreateRecord(DirRX.NNOtherProcesses.Structures.SubstitutionPersonalAccount.ISubstitutionInfo info, DirRX.OverrideSolution.IEmployee employee, long taskId, string logPrefix)
    {
      Logger.DebugFormat("{0}. CreateRecord. Start.", logPrefix);
      
      var record = DirRX.NNOtherProcesses.SubstitutionPersonalAccounts.Create();
      
      record.Name = DirRX.NNOtherProcesses.SubstitutionPersonalAccounts
        .Resources.PropertyNameFormat(DirRX.NNOtherProcesses.SubstitutionPersonalAccounts.Info.Properties.Substitute.LocalizedName,
                                      info.Substitute.DisplayValue,
                                      employee.DisplayValue,
                                      info.StartDate.ToString("dd.MM.yyyy"),
                                      info.EndDate.ToString("dd.MM.yyyy"),
                                      info.SubstitutionSystem.DisplayValue);
      record.Employee = employee;
      record.Substitute = info.Substitute;
      record.StartDate = info.StartDate;
      record.EndDate = info.EndDate;
      record.SubstitutionSystem = info.SubstitutionSystem;
      record.SourceTask = taskId.ToString();
      
      record.Save();
      
      Logger.DebugFormat("{0}. CreateRecord. End. Created recordId={1}.", logPrefix, record.Id);
      
      return record;
    }

  }
}