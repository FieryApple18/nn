using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.OverrideSolution;

namespace DirRX.NNIntegration.Shared
{
  public partial class ModuleFunctions
  {

    #region Интеграционный сценарий "Синхронизация организационной структуры".
    
    /// <summary>
    /// Сформировать текст по ошибкам обработки записи очереди синхронизации орг. структуры.
    /// </summary>
    /// <param name="errors">Список записей очередей орг. структуры, в которых возникла ошибка обработки.</param>
    /// <returns>Текст.</returns>
    public string FormCompanySynchronizationErrorsText(List<ICompanyQueueBase> errors)
    {
      var departmentErrors = errors.Where(e => DepartmentQueues.Is(e)).ToList();
      var jobTitleErrors = errors.Where(e => JobTitleQueues.Is(e)).ToList();
      var employeeErrors = errors.Where(e => EmployeeQueues.Is(e)).ToList();
      
      var text = string.Format("{0}{1}", DirRX.NNIntegration.Resources.CompanySynchronizationTextTitle, Environment.NewLine);
      if (departmentErrors.Any())
        text += FormErrorsTextByQueueType(DepartmentQueues.Info.LocalizedName, departmentErrors);
      if (jobTitleErrors.Any())
        text += FormErrorsTextByQueueType(JobTitleQueues.Info.LocalizedName, jobTitleErrors);
      if (employeeErrors.Any())
        text += FormErrorsTextByQueueType(EmployeeQueues.Info.LocalizedName, employeeErrors);
      
      return text;
    }
    
    /// <summary>
    /// Сформировать текст для ошибок по типу очереди.
    /// </summary>
    /// <param name="queueTypeName">Название типа очереди.</param>
    /// <param name="errors">Список записей очередей орг. структуры, в которых возникла ошибка обработки.</param>
    /// <returns>Текст.</returns>
    public string FormErrorsTextByQueueType(string queueTypeName, List<ICompanyQueueBase> errors)
    {
      var text = string.Format("{0}:{1}", queueTypeName, Environment.NewLine);
      var number = 1;
      foreach (var error in errors)
      {
        var errorMessage = DirRX.NNIntegration.Resources.CompanySynchronizationErrorTextFormat(error.ErrorMessage);
        var entityLink = string.Empty;
        if (DepartmentQueues.As(error)?.Department != null)
          entityLink = DirRX.NNIntegration.Resources.CompanySynchronizationErrorEntityLinkFormat(Departments.Info.LocalizedName, Hyperlinks.Get(DepartmentQueues.As(error).Department));
        if (JobTitleQueues.As(error)?.JobTitle != null)
          entityLink = DirRX.NNIntegration.Resources.CompanySynchronizationErrorEntityLinkFormat(JobTitles.Info.LocalizedName, Hyperlinks.Get(JobTitleQueues.As(error).JobTitle));
        if (EmployeeQueues.As(error)?.Employee != null)
          entityLink = DirRX.NNIntegration.Resources.CompanySynchronizationErrorEntityLinkFormat(Employees.Info.LocalizedName, Hyperlinks.Get(EmployeeQueues.As(error).Employee));
        
        text += string.Format("{0}. {1}. {2}{3}{4}", number, Hyperlinks.Get(error), entityLink, errorMessage, Environment.NewLine);
        number++;
      }
      
      return text;
    }
    
    #endregion
    
  }
}