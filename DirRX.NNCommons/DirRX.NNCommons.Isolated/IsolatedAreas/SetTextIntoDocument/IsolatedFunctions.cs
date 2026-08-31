using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using DirRX.NNCommons.Structures.Module;

namespace DirRX.NNCommons.Isolated.SetTextIntoDocument
{
  public partial class IsolatedFunctions
  {

    /// <summary>
    /// Устанавливает текст на метки в PDF документе.
    /// </summary>
    /// <param name="replacements">Структура с информацией о замене текста по меткам.</param>
    /// <returns>Документ с проставленным текстом.</returns>
    [Public]
    public Stream SetTextIntoPdfDocument(Stream inputStream, List<NNCommons.Structures.Module.ITextReplacement> replacements)
    {
      var resultStream = new MemoryStream();
      
      try
      {
        using (var document = new Aspose.Pdf.Document(inputStream))
        {
          var absorber = new Aspose.Pdf.Text.TextFragmentAbsorber();
          
          foreach (var replacement in replacements)
          {
            absorber.Phrase = replacement.OldValue;
            document.Pages.Accept(absorber);

            foreach (var textFragment in absorber.TextFragments)
            {
              if (replacement.IsNewParagraph)
              {
                textFragment.Text = string.Empty;
                
                var newTextFragment = new Aspose.Pdf.Text.TextFragment(replacement.NewValue.Replace(@"\n", Environment.NewLine));
                newTextFragment.TextState.FontSize = textFragment.TextState.FontSize;
                newTextFragment.TextState.Font = textFragment.TextState.Font;
                newTextFragment.TextState.ForegroundColor = textFragment.TextState.ForegroundColor;
                if (textFragment.TextState.BackgroundColor != null)
                  newTextFragment.TextState.BackgroundColor = textFragment.TextState.BackgroundColor;
                
                var paragraph = new Aspose.Pdf.Text.TextParagraph();
                paragraph.AppendLine(newTextFragment);
                paragraph.Position = textFragment.Position;
                
                var textBuilder = new Aspose.Pdf.Text.TextBuilder(textFragment.Page);
                textBuilder.AppendParagraph(paragraph);
              }
              else
              {
                textFragment.Text = replacement.NewValue;
              }
            }
          }
          
          document.Save(resultStream);
        }
      }
      catch (Exception ex)
      {
        resultStream.Close();
        Logger.Error("Не удалось выполнить замену текста в документе.", ex);
        throw AppliedCodeException.Create("Не удалось выполнить замену текста в документе.");
      }
      
      return resultStream;
    }

  }
}