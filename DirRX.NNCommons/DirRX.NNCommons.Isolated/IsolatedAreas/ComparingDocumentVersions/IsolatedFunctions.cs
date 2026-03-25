using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using DirRX.NNCommons.Structures.Module;
using Aspose.Words;

namespace DirRX.NNCommons.Isolated.ComparingDocumentVersions
{
  public partial class IsolatedFunctions
  {
    
    /// <summary>
    /// Сравнить две версии документа.
    /// </summary>
    /// <param name="ver1">Первая версия.</param>
    /// <param name="ver2">Вторая версия.</param>
    /// <param name="author">Автор.</param>
    /// <param name="date">Дата.</param>
    /// <returns>Версия с результатами сравнения.</returns>
    [Public]
    public System.IO.Stream GetResultComparingVersions(System.IO.Stream ver1, System.IO.Stream ver2, string author, DateTime date)
    {
      var loadDoc1 = new Document(ver1);
      var loadDoc2 = new Document(ver2);
      
      System.IO.Stream result = new System.IO.MemoryStream();
      
      // Перед сравнением не должно быть ревизий.
      loadDoc1.AcceptAllRevisions();
      loadDoc2.AcceptAllRevisions();
      
      // Удалить макросы, т.к. при сохранении они могут вызывать ошибки.
      loadDoc1.RemoveMacros();
      loadDoc2.RemoveMacros();
      loadDoc1.Compare(loadDoc2, author, date);
      loadDoc1.Save(result, SaveFormat.Docx);
      
      return result;
    }
  }
}