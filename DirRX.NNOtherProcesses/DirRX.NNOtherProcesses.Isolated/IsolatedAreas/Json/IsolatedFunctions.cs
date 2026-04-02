using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using DirRX.NNOtherProcesses.Structures.Module;

namespace DirRX.NNOtherProcesses.Isolated.Json
{
  public partial class IsolatedFunctions
  {
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(List<ICompensationKindStructure> structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IStatementDataForReworkWeekendWork structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
  }
}