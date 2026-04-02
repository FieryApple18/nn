using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using DirRX.OverrideSolution.Module.HRCommons.Structures.Module;

namespace DirRX.OverrideSolution.Module.HRCommons.Isolated.PdfConverter
{
  public partial class IsolatedFunctions
  {

    /// <summary>
    /// Преобразовать документ в pdf.
    /// </summary>
    /// <param name="inputStream">Поток с входным документом.</param>
    /// <param name="extension">Расширение входного документа.</param>
    /// <returns>Поток с документом.</returns>
    [Public]
    public new System.IO.Stream GeneratePdf(System.IO.Stream inputStream, string extension)
    {
      var pdfConverter = this.CreatePdfConverter();
      return pdfConverter.GeneratePdf(inputStream, extension);
    }

  }
}