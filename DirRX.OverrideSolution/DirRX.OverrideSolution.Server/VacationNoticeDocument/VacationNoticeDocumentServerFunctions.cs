using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationNoticeDocument;
using TableDataKeys = DirRX.NNVacations.PublicConstants.Module.TableDataKeys;

namespace DirRX.OverrideSolution.Server
{
  partial class VacationNoticeDocumentFunctions
  {

    /// <summary>
    /// Заполнить таблицу отпусков в шаблоне документа.
    /// </summary>
    /// <param name="logPrefix">Префикс сообщения для логирования.</param>
    [Public]
    public void FillVacationDataInTemplate(string logPrefix)
    {
      var vacationInfoList = _obj.VacationDataDirRX.OrderBy(v => v.StartDate.Value);
      var tableData = NNCommons.Structures.Module.TableData.Create();
      tableData.NumberKey = TableDataKeys.Table;
      tableData.Values = new List<Dictionary<string, string>>();
      foreach (var vacationInfo in vacationInfoList)
      {
        var tableLineDictionary = new Dictionary<string, string>();
        tableLineDictionary.Add(TableDataKeys.VacationTypeName, vacationInfo.VacationType.Name);
        tableLineDictionary.Add(TableDataKeys.DaysAmount, vacationInfo.Days.Value.ToString());
        tableLineDictionary.Add(TableDataKeys.BeginDate, vacationInfo.StartDate.Value.ToShortDateString());
        tableLineDictionary.Add(TableDataKeys.EndDate, vacationInfo.EndDate.Value.ToShortDateString());
        tableData.Values.Add(tableLineDictionary);
      }
      
      using (var memory = new System.IO.MemoryStream())
      {
        try
        {
          using (Sungero.Domain.Shared.HistoryLocker.WithoutWriteHistory(_obj.LastVersion.ElectronicDocument))
          {
            _obj.LastVersion.Body.Read().CopyTo(memory);
          }
          var documentStream = NNCommons.IsolatedFunctions.WordInsertTable.WordInsertTableInTag(logPrefix, memory, tableData);
          using (var stream = new System.IO.MemoryStream())
          {
            documentStream.CopyTo(stream);
            _obj.LastVersion.Body.Write(stream);
            _obj.Save();
          }
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. Заполнить таблицу отпусков. Ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        }
      }
    }

  }
}