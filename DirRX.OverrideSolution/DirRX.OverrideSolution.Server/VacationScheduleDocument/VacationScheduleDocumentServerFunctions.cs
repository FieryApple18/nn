using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationScheduleDocument;
using TableDataKeys = DirRX.NNVacations.PublicConstants.Module.TableDataKeys;

namespace DirRX.OverrideSolution.Server
{
  partial class VacationScheduleDocumentFunctions
  {

    /// <summary>
    /// Подготовка данных для замены меток в теле документа.
    /// </summary>
    /// <returns>Список структур для замены меток в документе.</returns>
    [Public]
    public List<NNCommons.Structures.Module.ITextReplacement> PrepareDataForInsertingIntoDocument()
    {
      var mounthGenetiveNamesCulture = (System.Globalization.CultureInfo) TenantInfo.Culture.Clone();
      mounthGenetiveNamesCulture.DateTimeFormat.MonthNames = mounthGenetiveNamesCulture.DateTimeFormat.MonthGenitiveNames;
      mounthGenetiveNamesCulture.DateTimeFormat.AbbreviatedMonthNames = mounthGenetiveNamesCulture.DateTimeFormat.AbbreviatedMonthGenitiveNames;
      
      #region Номер документа.
      
      var registrationNumber = _obj.RegistrationNumber ?? string.Empty;
      
      #endregion
      
      #region Дата регистрации.
      
      var registrationDate = _obj.RegistrationDate.HasValue ? _obj.RegistrationDate.Value.ToString("d MMMM") : string.Empty;
      
      #endregion
      
      #region Мнение профсоюзного органа.
      
      var opinionDay = _obj.StatementDateDirRX.HasValue ? _obj.StatementDateDirRX.Value.Day.ToString() : string.Empty;
      var opinionMouthYear = _obj.StatementDateDirRX.HasValue ? _obj.StatementDateDirRX.Value.ToString("MMMM yyyyг.", mounthGenetiveNamesCulture) : string.Empty;
      var opinionNumber = !string.IsNullOrEmpty(_obj.StatementNumberDirRX) ? _obj.StatementNumberDirRX : string.Empty;
      var opinion = !string.IsNullOrEmpty(opinionDay) && !string.IsNullOrEmpty(opinionMouthYear) && !string.IsNullOrEmpty(opinionNumber)
        ? VacationScheduleDocuments.Resources.Template_UnionOpinionFormat(opinionDay, opinionMouthYear, opinionNumber)
        : string.Empty;
      
      #endregion
      
      var result = new List<NNCommons.Structures.Module.ITextReplacement>();
      
      result.Add(NNCommons.Structures.Module.TextReplacement.Create(VacationScheduleDocuments.Resources.TextMark_RegistrationNumber,
                                                                    registrationNumber,
                                                                    false));
      result.Add(NNCommons.Structures.Module.TextReplacement.Create(VacationScheduleDocuments.Resources.TextMark_RegistrationDate,
                                                                    registrationDate,
                                                                    false));
      result.Add(NNCommons.Structures.Module.TextReplacement.Create(VacationScheduleDocuments.Resources.TextMark_UnionOpinion,
                                                                    opinion,
                                                                    true));
      
      return result;
    }

    /// <summary>
    /// Создать документ по шаблону.
    /// </summary>
    /// <param name="template">Шаблон.</param>
    [Public, Remote]
    public void CreateFromTemplate(string logPrefix)
    {
      var vacationInfoList = NNVacations.PublicFunctions.VacationsInformation.GetVacationInfo(_obj).OrderBy(v => v.StartDate.Value);
      var tableData = NNCommons.Structures.Module.TableData.Create();
      tableData.NumberKey = TableDataKeys.Table;
      tableData.Values = new List<Dictionary<string, string>>();
      foreach (var vacationInfo in vacationInfoList)
      {
        var tableLineDictionary = new Dictionary<string, string>();
        tableLineDictionary.Add(TableDataKeys.VacationTypeName, vacationInfo.VacationType.Name);
        tableLineDictionary.Add(TableDataKeys.DaysAmount, vacationInfo.DaysAmount.Value.ToString());
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
          Logger.ErrorFormat("{0}. Заполнить таблицу графика отпусков. Ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        }
      }
    }

  }
}