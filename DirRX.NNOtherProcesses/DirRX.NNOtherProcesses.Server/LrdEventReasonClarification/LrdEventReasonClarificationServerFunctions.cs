using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdEventReasonClarification;

namespace DirRX.NNOtherProcesses.Server
{
  partial class LrdEventReasonClarificationFunctions
  {
    /// <summary>
    /// Получить список дублирующихся записей.
    /// </summary>
    /// <returns>Список дублей.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<ILrdEventReasonClarification> GetDuplicates()
    {
      return LrdEventReasonClarifications.GetAll().Where(x => x.Id != _obj.Id && x.Code == _obj.Code);
    }
    
    /// <summary>
    /// Создать запись справочника.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="code">Код.</param>
    /// <returns>Ошибка. Пустая строка, если ошибок нет.</returns>
    public static string CreateRecord(string name, string code)
    {
      if (LrdEventReasonClarifications.GetAll(x => x.Name == name && x.Code == code).Any())
        return string.Empty;
      
      var error = string.Empty;
      var record = LrdEventReasonClarifications.Create();
      record.Name = name;
      record.Code = code;
      
      try
      {
        record.Save();
      }
      catch (Exception ex)
      {
        error = string.Format("Ошибка при сохранении уточнения причины мероприятия \"{0}\": {1}.", name, ex.Message);
        Logger.DebugFormat("CreateRecord. {0}", error);
      }
      
      return error;
    }
  }
}