using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DirRX.NNCommons.Structures.Module;

namespace DirRX.NNCommons.Isolated.OpenXML
{
  public partial class IsolatedFunctions
  {
    /// <summary>
    /// Формирует Excel-отчёт на основе шаблона.
    /// Первая строка — заголовки (сохраняется без изменений).
    /// Данные заполняются начиная со второй строки.
    /// Колонки: A=Организация, B=Раздел персонала, C=Табельный номер, D=Сотрудник,
    /// E=ID должности, F=Должность, G=ID подразделения, H=Подразделение,
    /// I=Вариант процесса, J=Маршрут согласования, K=Пропущенные этапы.
    /// </summary>
    /// <param name="results">Данные для формирования отчета.</param>
    /// <param name="bytes">Тело шаблона в виде потока.</param>
    /// <returns>Отчет в виде потока.</returns>
    [Public]
    public System.IO.Stream BuildExcelReport(
      List<Structures.Module.IProcessKindSchemeResult> results,
      System.IO.Stream bytes)
    {
      var templateBytes = new MemoryStream();
      bytes.CopyTo(templateBytes);
      templateBytes.Position = 0;
      
      using (var spreadsheet = SpreadsheetDocument.Open(templateBytes, true))
      {
        var workbookPart = spreadsheet.WorkbookPart;
        if (workbookPart == null)
          throw AppliedCodeException.Create("WorkbookPart шаблона отчёта пустой.");

        var worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
        if (worksheetPart == null)
          throw AppliedCodeException.Create("В шаблоне отчёта не найден лист.");

        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        if (sheetData == null)
          throw AppliedCodeException.Create("SheetData в шаблоне отчёта пустой.");

        // Удаляем все строки кроме первой (заголовки)
        var rows = sheetData.Elements<Row>().ToList();
        for (int i = rows.Count - 1; i >= 1; i--)
          rows[i].Remove();

        var workbookStylesPart = workbookPart.WorkbookStylesPart;
        if (workbookStylesPart == null)
        {
          workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
          workbookStylesPart.Stylesheet = new Stylesheet();
        }
        var stylesheet = workbookStylesPart.Stylesheet;

        // Один раз создаём стиль с переносом текста
        uint wrapStyleIndex = GetOrCreateWrapTextStyleIndex(stylesheet);

        // Заполняем данные начиная со второй строки
        for (int i = 0; i < results.Count; i++)
        {
          var result = results[i];
          var row = new Row();

          // A: Наша организация
          row.Append(CreateInlineCell(result.OrganizationName ?? ""));

          // B: Раздел персонала
          row.Append(CreateInlineCell(result.PersonnelSectionName ?? ""));

          // C: Табельный номер
          row.Append(CreateInlineCell(result.PersonnelNumber ?? ""));

          // D: Сотрудник (ФИО)
          row.Append(CreateInlineCell(result.EmployeeName ?? ""));

          // E: ID должности
          row.Append(CreateInlineCell(result.SapIdJobTitle ?? ""));

          // F: Должность
          row.Append(CreateInlineCell(result.JobTitleName ?? ""));

          // G: ID подразделения
          row.Append(CreateInlineCell(result.SapIdDepartment ?? ""));

          // H: Подразделение
          row.Append(CreateInlineCell(result.DepartmentName ?? ""));

          // I: Вариант процесса
          string processKindName = string.IsNullOrWhiteSpace(result.ProcessKindName)
            ? "Не подобран"
            : result.ProcessKindName;
          row.Append(CreateInlineCell(processKindName));

          // J: Маршрут согласования
          row.Append(CreateMultiLineCell(result.ApprovalRoute ?? "", wrapStyleIndex));

          // K: Пропущенные этапы
          row.Append(CreateMultiLineCell(result.MissingSteps ?? "", wrapStyleIndex));

          sheetData.Append(row);
        }

        worksheetPart.Worksheet.Save();
        workbookStylesPart.Stylesheet.Save();
      }

      // Документ сохранён in-place в templateBytes — возвращаем его
      // (после Save позиция в конце, сбрасываем в начало).
      templateBytes.Position = 0;
      return templateBytes;
    }

    /// <summary>
    /// Создаёт ячейку с inline-строкой (одна строка текста).
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <returns>Ячейка.</returns>
    private Cell CreateInlineCell(string text)
    {
      return new Cell
      {
        DataType = CellValues.InlineString,
        InlineString = new InlineString(new Run(new Text(text ?? "")))
      };
    }
    
    /// <summary>
    /// Создаёт ячейку с inline-строкой и переносами строк (Break между Run).
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="styleIndex">Индекс стиля.</param>
    /// <returns>Ячейка.</returns>
    private Cell CreateMultiLineCell(string text, uint styleIndex)
    {
      var run = new Run(
        new RunProperties(), // при необходимости
        new Text(text ?? "")
        {
          Space = SpaceProcessingModeValues.Preserve
        }
       );

      var inlineString = new InlineString(run);

      return new Cell
      {
        DataType = CellValues.InlineString,
        InlineString = inlineString,
        StyleIndex = styleIndex // <-- индекс стиля с WrapText = true
      };
    }
    
    /// <summary>
    /// Добавляет новый стиль ячейки с переносом тектса и белой заливкой.
    /// </summary>
    /// <param name="stylesheet">Объект стилей.</param>
    /// <returns>Индекс формата ячейки.</returns>
    private uint GetOrCreateWrapTextStyleIndex(Stylesheet stylesheet)
    {
      if (stylesheet == null)
        throw AppliedCodeException.Create("Stylesheet шаблона отчёта пустой.");

      // --- Шрифт без жирности ---
      var fonts = stylesheet.Fonts;
      if (fonts == null)
      {
        fonts = new Fonts(new Font());
        stylesheet.InsertAt(fonts, 0);
      }

      // Ищем существующий шрифт без Bold, чтобы не плодить дубликаты.
      uint regularFontId = 0;
      bool foundRegular = false;
      uint fontIndex = 0;
      foreach (var f in fonts.Elements<Font>())
      {
        var bold = f.GetFirstChild<Bold>();
        bool isBold = bold != null &&
          (bold.Val == null || bold.Val.Value);
        if (!isBold)
        {
          regularFontId = fontIndex;
          foundRegular = true;
          break;
        }
        fontIndex++;
      }

      if (!foundRegular)
      {
        var regularFont = new Font(
          new FontSize { Val = 11 },
          new FontName { Val = "Calibri" }
         );
        fonts.Append(regularFont);
        fonts.Count = (uint)fonts.ChildElements.Count;
        regularFontId = (uint)(fonts.ChildElements.Count - 1);
      }

      // --- Заливка: белая, solid ---
      var fills = stylesheet.Fills;
      if (fills == null)
      {
        fills = new Fills(
          new Fill(new PatternFill { PatternType = PatternValues.None }),
          new Fill(new PatternFill { PatternType = PatternValues.Gray125 })
         );
        stylesheet.InsertAt(fills, 1);
      }

      var whiteFill = new Fill(
        new PatternFill(new ForegroundColor { Rgb = "FFFFFFFF" })
        {
          PatternType = PatternValues.Solid
        });
      fills.Append(whiteFill);
      fills.Count = (uint)fills.ChildElements.Count;
      uint newFillId = (uint)(fills.ChildElements.Count - 1);
            
      var tinBoarderId = GetTinBoarderId(stylesheet);
      
      // --- Формат ячейки ---
      var cellFormats = stylesheet.CellFormats;
      if (cellFormats == null)
      {
        cellFormats = new CellFormats(new CellFormat());
        stylesheet.Append(cellFormats);
      }

      var newFormat = new CellFormat
      {
        Alignment = new Alignment
        {
          WrapText = true,
          Horizontal = HorizontalAlignmentValues.Left,
          Vertical = VerticalAlignmentValues.Top
        },
        ApplyAlignment = true,
        FillId = newFillId,
        ApplyFill = true,
        BorderId = tinBoarderId,
        ApplyBorder = true,
        FontId = regularFontId, // <-- ключевой момент: не-жирный шрифт
        ApplyFont = true,
        FormatId = 0
      };

      cellFormats.Append(newFormat);
      cellFormats.Count = (uint)cellFormats.ChildElements.Count;

      return (uint)(cellFormats.ChildElements.Count - 1);
    }

    /// <summary>
    /// Добавляет границы для ячейки или использует найденную.
    /// </summary>
    /// <param name="stylesheet">Объект стилей.</param>
    /// <returns>Индекс формата ячейки.</returns>
    private uint GetTinBoarderId(Stylesheet stylesheet)
    {
      // --- Границы: тонкие со всех сторон ---
      var borders = stylesheet.Borders;
      if (borders == null)
      {
        borders = new Borders(new Border());
        stylesheet.InsertAt(borders, 2);
      }

      // Ищем уже существующий Border с тонкими линиями со всех сторон.
      uint thinBorderId = 0;
      bool foundBorder = false;
      uint borderIndex = 0;
      foreach (var b in borders.Elements<Border>())
      {
        var allThin = b.LeftBorder?.Style?.Value == BorderStyleValues.Thin &&
          b.RightBorder?.Style?.Value == BorderStyleValues.Thin &&
          b.TopBorder?.Style?.Value == BorderStyleValues.Thin &&
          b.BottomBorder?.Style?.Value == BorderStyleValues.Thin;
        
        if (allThin)
        {
          thinBorderId = borderIndex;
          foundBorder = true;
          break;
        }
        borderIndex++;
      }

      if (!foundBorder)
      {
        var thinBorder = new Border(
          new LeftBorder { Style = BorderStyleValues.Thin },
          new RightBorder { Style = BorderStyleValues.Thin },
          new TopBorder { Style = BorderStyleValues.Thin },
          new BottomBorder { Style = BorderStyleValues.Thin },
          new DiagonalBorder()
         );
        borders.Append(thinBorder);
        borders.Count = (uint)borders.ChildElements.Count;
        thinBorderId = (uint)(borders.ChildElements.Count - 1);
      }
      
      return thinBorderId;
    }
  }
}