using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkCompensationKind;
using DirRX.OverrideSolution;

namespace DirRX.NNOtherProcesses.Server
{
  partial class WeekendWorkCompensationKindFunctions
  {
    /// <summary>
    /// Создать запись справочника.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="code">Код.</param>
    /// <param name="docKind">Вид заявки.</param>
    /// <param name="needRestDate">Запрашивать дату доп. отдыха.</param>
    /// <returns>Ошибка. Пустая строка, если ошибок нет.</returns>
    public static string CreateRecord(string name, string code, Sungero.Docflow.IDocumentKind docKind, bool needRestDate)
    {
      if (WeekendWorkCompensationKinds.GetAll(x => x.Name == name && x.Code == code && Equals(x.DocumentKind, docKind)).Any())
        return string.Empty;
      
      var error = string.Empty;
      var record = WeekendWorkCompensationKinds.Create();
      record.Name = name;
      record.Code = code;
      record.NeedRestDate = needRestDate;
      record.DocumentKind = DocumentKinds.As(docKind);
      
      try
      {
        record.Save();
      }
      catch (Exception ex)
      {
        error = string.Format("Ошибка при сохранении вида компенсации \"{0}\": {1}.", name, ex.Message);
        Logger.DebugFormat("CreateRecord. {0}", error);
      }
      
      return error;
    }
  }
}