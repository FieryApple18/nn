using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using DirRX.NNCommons.Structures.Module;

namespace DirRX.NNCommons.Isolated.Json
{
  public partial class IsolatedFunctions
  {
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IStatementData structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(List<ITaskDataForWidget> structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(List<IDocumentInfoFromLK> structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
    
    /// <summary>
    /// Сериализация структуры в json строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <returns>Json строка.</returns>
    [Public]
    public static string SerializeInJson(IEmployeeGoAndTopInfo structure)
    {
      return JsonConvert.SerializeObject(structure);
    }
  }
}