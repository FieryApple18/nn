using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using DirRX.NNCommons.Structures.Module;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace DirRX.NNCommons.Isolated.WordInsertTable
{
  public partial class IsolatedFunctions
  {
    /// <summary>
    /// Заполнить табличную часть в документе по шаблону.
    /// </summary>
    /// <param name="logPrefix">Префикс сообщения для логирования.</param>
    /// <param name="stream">Поток с документом.</param>
    /// <param name="tableData">Табличные данные.</param>
    /// <returns>Поток с документом, в котором добавлена таблица.</returns>
    [Public]
    public static System.IO.Stream WordInsertTableInTag(string logPrefix, System.IO.Stream stream, NNCommons.Structures.Module.ITableData tableData)
    {
      DocumentFormat.OpenXml.Packaging.WordprocessingDocument wordDocument = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(stream, true);
      
      var body = wordDocument.MainDocumentPart.Document.Body;

      // Найти строку по тегу для замены.
      var rows = body.Descendants<TableRow>().Where(x => x.Descendants<SdtProperties>().Any(q => q.Elements<Tag>().Any(t => t.Val == tableData.NumberKey)));
      if (!rows.Any())
      {
        Logger.ErrorFormat("{0}. Заполнить таблицу документа. Строка с тэгом {1} отсутствует.", logPrefix, tableData.NumberKey);
        return stream;
      }

      TableRow baseRow = rows.First();
      DocumentFormat.OpenXml.Wordprocessing.Table table = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>().Where(x => x.Descendants<TableRow>().Contains(baseRow)).First();

      TableRow prevRow = null;
      var replaceNumberText = string.Empty;
      foreach (var line in tableData.Values)
      {
        var newRow = new TableRow(baseRow.OuterXml);

        // Заполнить ячейки значениями.
        foreach (var item in line)
        {
          var sdtCell = newRow.Descendants<SdtCell>().Where(x => x.Descendants<Tag>().Any(t => t.Val == item.Key)).FirstOrDefault();
          if (sdtCell != null)
            newRow.ReplaceChild(FillCell(sdtCell, item.Value), sdtCell);
        }

        table.InsertAfter(newRow, prevRow ?? baseRow);
        prevRow = newRow;
      }

      table.RemoveChild(baseRow);
      
      wordDocument.MainDocumentPart.Document.Save();
      
      var memoryStream = new System.IO.MemoryStream();
      wordDocument.Clone(memoryStream);
      
      wordDocument.Dispose();
      
      return memoryStream;
    }

    /// <summary>
    /// Заполнить текст ячейки с сохранением стилей.
    /// </summary>
    /// <param name="sdtCell">Ячейка.</param>
    /// <param name="valueCell">Значение для заполнения.</param>
    /// <returns>Измененный элемент табличной строки.</returns>
    public static DocumentFormat.OpenXml.Wordprocessing.SdtCell FillCell(DocumentFormat.OpenXml.Wordprocessing.SdtCell sdtCell, string valueCell)
    {
      if (string.IsNullOrEmpty(valueCell))
        valueCell = string.Empty;
      
      var run = sdtCell.Descendants<DocumentFormat.OpenXml.Wordprocessing.Run>().FirstOrDefault();
      if (run == null)
        run =  new DocumentFormat.OpenXml.Wordprocessing.Run();
      run.RemoveAllChildren<DocumentFormat.OpenXml.Wordprocessing.Text>();
      
      // Чтобы сохранить перносы строк нужно вставлять break.
      string[ ] newlineArray = { Environment.NewLine, "\n", "\r\n", "\n\r" };
      string[ ] textArray = valueCell.Split(newlineArray, StringSplitOptions.None);
      var first = true;
      
      foreach (string line in textArray )
      {
        if (!first )
        {
          run.Append(new DocumentFormat.OpenXml.Wordprocessing.Break());
        }
        first = false;
        run.Append(new DocumentFormat.OpenXml.Wordprocessing.Text(line));
      }

      var paragraph = sdtCell.Descendants<Paragraph>().FirstOrDefault();
      if (paragraph == null)
        paragraph =  new Paragraph();
      paragraph.RemoveAllChildren<DocumentFormat.OpenXml.Wordprocessing.Run>();
      paragraph.Append(new DocumentFormat.OpenXml.Wordprocessing.Run(run.OuterXml));

      var cell = sdtCell.Descendants<TableCell>().FirstOrDefault();
      if (cell == null)
        cell = new TableCell();
      cell.RemoveAllChildren<Paragraph>();
      cell.Append(new Paragraph(paragraph.OuterXml));

      sdtCell.SdtContentCell.ReplaceChild(new TableCell(cell.OuterXml), sdtCell.Descendants<TableCell>().FirstOrDefault());

      return new SdtCell(sdtCell.OuterXml);
    }
  }
}