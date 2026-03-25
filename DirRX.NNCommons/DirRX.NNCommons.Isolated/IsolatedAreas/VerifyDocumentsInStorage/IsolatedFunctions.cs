using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Sungero.Core;
using Aspose.Cells;
using DirRX.NNCommons.Structures.Module;

namespace DirRX.NNCommons.Isolated.VerifyDocumentsInStorage
{
  public partial class IsolatedFunctions
  {
    #region Проверка документов в файловом хранилище
    
    /// <summary>
    /// Проверить возможность прочитать тело документа.
    /// </summary>
    /// <param name="bodyStream">Поток тела документа.</param>
    /// <param name="size">Размер тела документа.</param>
    /// <returns>Текст ошибки.</returns>
    [Public]
    public virtual string CheckReadingDocumentBody(Stream bodyStream, long size)
    {
      var bodySize = 0;
      try
      {
        using (var memory = new MemoryStream())
        {
          bodyStream.CopyTo(memory);
          bodySize = memory.ToArray().Length;
        }
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
      
      if (bodySize == 0 || size == 0)
        return "Пустой документ";
      if (bodySize != size)
        return string.Format("Различие в размере файла ({0} байт в системе; {1} байт при чтении)", size, bodySize);
      
      return string.Empty;
    }
    
    /// <summary>
    /// Сформировать отчет формата xlsx по проверке документов в файловом хранилище.
    /// </summary>
    /// <param name="stream">Поток тела документа.</param>
    /// <param name="documentsVerifyInfo">Результаты проверки.</param>
    /// <param name="needCreate">Необходимость создания листа.</param>
    /// <param name="title">Заголовок для отображения в отчете.</param>
    /// <param name="documentCount">Количество найденных документов.</param>
    /// <returns>Поток с отчетом.</returns>
    [Public]
    public virtual Stream BuildVerifyDocumentsReport(Stream stream, List<DirRX.NNCommons.Structures.Module.IDocumentVerifyInfo> documentsVerifyInfo, bool needCreate, string title, int documentCount)
    {
      if (needCreate)
      {
        using (var workbook = new Workbook())
        {
          workbook.Worksheets.Clear();
          this.AddTotalResultSheet(workbook, documentsVerifyInfo, null, title, documentCount);
          workbook.Save(stream, SaveFormat.Xlsx);
        }
      }
      else
      {
        using (var workbook = new Workbook(stream))
        {
          var worksheet = workbook.Worksheets.FirstOrDefault();
          this.AddTotalResultSheet(workbook, documentsVerifyInfo, worksheet, title, documentCount);
          workbook.Save(stream, SaveFormat.Xlsx);
        }
      }
      return stream;
    }
    
    /// <summary>
    /// Добавить страницу с общими результатами проверки.
    /// </summary>
    /// <param name="workbook">Книга.</param>
    /// <param name="documentsVerifyInfo">Результаты проверки.</param>
    /// <param name="worksheet">Файл для формирования.</param>
    /// <param name="title">Заголовок для отображения в отчете.</param>
    /// <param name="documentCount">Количество найденных документов.</param>
    public void AddTotalResultSheet(Aspose.Cells.Workbook workbook, List<DirRX.NNCommons.Structures.Module.IDocumentVerifyInfo> documentsVerifyInfo, Worksheet worksheet, string title, int documentCount)
    {
      var columnDocId = 0;
      var columnDocName = 1;
      var columnVersion = 2;
      var columnError = 3;
      var rowTitleNumber = 0;
      var rowDocumentCountNumber = 1;
      var rowHeaderNumber = 3;
      
      // Задать стиль по умолчанию.
      if (worksheet == null)
      {
        Style wbStyle = workbook.CreateStyle();
        wbStyle.Font.Name = "Arial";
        wbStyle.Font.Size = 11;
        wbStyle.HorizontalAlignment = TextAlignmentType.Center;
        wbStyle.VerticalAlignment = TextAlignmentType.Center;
        
        Style wbStyleTitle = workbook.CreateStyle();
        wbStyleTitle.Font.Name = "Arial";
        wbStyleTitle.Font.Size = 11;
        wbStyleTitle.Font.IsBold = true;
        wbStyleTitle.HorizontalAlignment = TextAlignmentType.Left;
        wbStyleTitle.VerticalAlignment = TextAlignmentType.Center;
        
        workbook.DefaultStyle = wbStyle;
        var worksheetName = "Стр 1.";
        worksheet = workbook.Worksheets.Add(worksheetName);
        
        // Заголовки.
        worksheet.Cells[rowHeaderNumber, columnDocId].Value = "ИД";
        worksheet.Cells[rowHeaderNumber, columnDocName].Value = "Название документа";
        worksheet.Cells[rowHeaderNumber, columnVersion].Value = "Версия";
        worksheet.Cells[rowHeaderNumber, columnError].Value = "Текст ошибки";

        // Установить ширину столбцов.
        worksheet.Cells.CreateRange(rowHeaderNumber, columnDocId, 1, 1).ColumnWidth = 8;
        worksheet.Cells.CreateRange(rowHeaderNumber, columnDocName, 1, 1).ColumnWidth = 80;
        worksheet.Cells.CreateRange(rowHeaderNumber, columnVersion, 1, 1).ColumnWidth = 15;
        worksheet.Cells.CreateRange(rowHeaderNumber, columnError, 1, 1).ColumnWidth = 100;
        
        // Стиль для шапки
        worksheet.Cells[rowTitleNumber, 0].SetStyle(wbStyleTitle);
        worksheet.Cells[rowDocumentCountNumber, 0].SetStyle(wbStyleTitle);
        
      }
      
      worksheet.Cells[rowTitleNumber, 0].Value = title;
      worksheet.Cells[rowDocumentCountNumber, 0].Value = string.Format("Найдено проблемных документов: {0}", documentCount);
      
      // Стиль с выравниванием по левому краю.
      var leftStyle = workbook.CreateStyle();
      leftStyle.HorizontalAlignment = TextAlignmentType.Left;
      
      // Начальная строка в текущей итерации.
      var rowNumber = worksheet.Cells.MaxRow == 0 ? rowHeaderNumber + 1 : worksheet.Cells.MaxRow + 1;
      
      foreach (var document in documentsVerifyInfo)
      {
        // Строка документа.
        var docRowNumber = rowNumber;
        // ИД.
        worksheet.Cells[rowNumber, columnDocId].Value = document.DocumentId;
        // Имя.
        worksheet.Cells[rowNumber, columnDocName].Value = document.DocumentName;
        worksheet.Cells[rowNumber, columnDocName].SetStyle(leftStyle);
        worksheet.Hyperlinks.Add(rowNumber,  columnDocName, 1, 1, document.Hyperlink);
        // Номер версии.
        worksheet.Cells[rowNumber, columnVersion].Value = document.Versions;
        // Текст ошибки.
        worksheet.Cells[rowNumber, columnError].Value = document.Error;
        worksheet.Cells[rowNumber, columnError].SetStyle(leftStyle);
        rowNumber++;
      }
    }
    
    #endregion
  }
}