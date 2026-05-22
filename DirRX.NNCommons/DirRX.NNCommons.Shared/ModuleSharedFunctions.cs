using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNCommons.Shared
{
  public partial class ModuleFunctions
  {
    
    /// <summary>
    /// Получить правильную форму слова "час" в зависимости от числа.
    /// </summary>
    /// <param name="hours">Число часов.</param>
    /// <returns>Правильная форма слова "час".</returns>
    [Public]
    public static string GetHoursDeclension(double hours)
    {
      var num = (int)hours;
      if (num % 100 >= 11 && num % 100 <= 14)
        return Resources.HoursDeclensionDefault;
      
      switch (num % 10)
      {
        case 1:
          return Resources.HoursDeclensionOne;
        case 2:
        case 3:
        case 4:
          return Resources.HoursDeclensionTwo;
        default:
          return Resources.HoursDeclensionDefault;
      }
    }

    /// <summary>
    /// Получить список гуидов видов документов, созданных в рамках прикладного решения проекта Норникель.
    /// </summary>
    /// <returns>Список гуидов видов документов.</returns>
    [Public]
    public List<Guid> GetNNDocKindsGuidsList()
    {
      var docKindGuidsList = new List<Guid>();
      
      // Виды документов NNCommons.
      var nnCommonsDocKinds = typeof(NNCommons.PublicConstants.Module.DocumentKind).GetProperties()
        .Where(x => x.PropertyType == typeof(Guid)).Select(x => (Guid)x.GetValue(null)).ToList();
      
      // Виды документов NNVacations.
      var nnVacationsDocKinds = typeof(NNVacations.PublicConstants.Module.DocumentKind).GetProperties()
        .Where(x => x.PropertyType == typeof(Guid)).Select(x => (Guid)x.GetValue(null)).ToList();
      
      // Виды документов NNOtherProcesses.
      var nnOtherProcessesDocKinds = typeof(NNOtherProcesses.PublicConstants.Module.DocumentKind).GetProperties()
        .Where(x => x.PropertyType == typeof(Guid)).Select(x => (Guid)x.GetValue(null)).ToList();
      
      docKindGuidsList.AddRange(nnCommonsDocKinds);
      docKindGuidsList.AddRange(nnVacationsDocKinds);
      docKindGuidsList.AddRange(nnOtherProcessesDocKinds);
      
      return docKindGuidsList;
    }
    
    /// <summary>
    /// Получить список базовых гуидов видов документов, которые используются в решении Норникель.
    /// </summary>
    /// <returns>Список гуидов видов документов.</returns>
    [Public]
    public List<Guid> GetBaseDocKindsGuidsList()
    {
      var docKindGuidsList = new List<Guid>();
      // HACK: Seleznyov_DV. Константа с гуидом вида документа График отпусков в модуле Vacations в коробке HRPro - не публичная.
      docKindGuidsList.Add(Guid.Parse("3c13dc09-8153-4b52-85c4-f2fb61a945c4"));
      // Заявление о досрочном прекращении отпуска.
      docKindGuidsList.Add(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationSelfRecallStatementKind);
      // Согласие на отзыв из отпуска.
      docKindGuidsList.Add(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallConsentKind);
      // Приказ об отзыве из отпуска.
      docKindGuidsList.Add(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationRecallOrderKind);
      // Заявление о переносе отпуска.
      docKindGuidsList.Add(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationShiftStatementKind);
      // Заявление на ежегодный отпуск.
      docKindGuidsList.Add(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationStatementKind);
      // Приказ на ежегодный отпуск.
      docKindGuidsList.Add(HRPro.Vacations.PublicConstants.Module.DocumentKind.VacationOrderKind);
      
      return docKindGuidsList;
    }

  }
}