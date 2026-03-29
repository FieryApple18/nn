using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.SubstitutionSystem;

namespace DirRX.NNOtherProcesses.Server
{
  partial class SubstitutionSystemFunctions
  {

    /// <summary>
    /// Получить запись справочника по умолчанию (КЭДО, ИД системы="IS0000098").
    /// </summary>
    /// <returns>Запись справочника.</returns>
    [Public]
    public static DirRX.NNOtherProcesses.ISubstitutionSystem GetDefaultRecord()
    {
      var kedoSystemIdConstant = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(DirRX.NNOtherProcesses.Constants.Module.ConstantsSetting.SubstitutionSystemKedoSystemId.Guid);
      var kedoSystemId = DirRX.NNOtherProcesses.Constants.Module.ConstantsDefaultValues.SubstitutionSystemKedoSystemId;

      if (!string.IsNullOrEmpty(kedoSystemIdConstant) && !string.IsNullOrWhiteSpace(kedoSystemIdConstant))
        kedoSystemId = kedoSystemIdConstant;

      return DirRX.NNOtherProcesses.SubstitutionSystems
        .GetAll(s => string.Equals(s.SystemId, kedoSystemId, StringComparison.OrdinalIgnoreCase))
        .FirstOrDefault();
    }

    /// <summary>
    /// Создать запись справочника.
    /// </summary>
    /// <param name="name">Наименование.</param>
    /// <param name="shortName">Краткое наименование.</param>
    /// <param name="systemId">ИД системы.</param>
    /// <param name="functions">Функции системы.</param>
    /// <returns>Ошибка. Пустая строка, если ошибок нет.</returns>
    public static string CreateRecord(string name, string shortName, string systemId, string functions)
    {
      if (DirRX.NNOtherProcesses.SubstitutionSystems.GetAll(s => s.SystemId == systemId).Any())
      {
        return string.Empty;
      }

      var error = string.Empty;
      var record = DirRX.NNOtherProcesses.SubstitutionSystems.Create();
      
      record.Name = name;
      record.ShortName = shortName;
      record.SystemId = systemId;
      record.Functions = functions;

      try
      {
        record.Save();
      }
      catch (Exception ex)
      {
        error = string.Format("Ошибка при сохранении записи справочника {0} с SystemId={1}: {2}",
                              DirRX.NNOtherProcesses.SubstitutionSystems.Info.LocalizedName,
                              systemId,
                              ex.Message);
        Logger.DebugFormat("SubstitutionSystemFunctions.CreateRecord. {0}", error);
      }

      return error;
    }

  }
}