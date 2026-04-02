using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.JobTitleQueue;

namespace DirRX.NNIntegration.Server
{
  partial class JobTitleQueueFunctions
  {
    /// <summary>
    /// Установить значение перечисления Атрибут "Группа оценки" по его строковому представлению.
    /// </summary>
    /// <param name="evaluationGroup">Атрибут "Группа оценки".</param>
    /// <returns>Результат. True - удалось установить значение. False - не удалось установить значение.</returns>
    public bool TryParseJobTitleEvalutionGroup(string evalutionGroup)
    {
      if (evalutionGroup == Constants.Module.EvaluationGroupCodes.Top100)
        _obj.EvaluationGroup = NNIntegration.JobTitleQueue.EvaluationGroup.Top100;
      else if (evalutionGroup == Constants.Module.EvaluationGroupCodes.Top10)
        _obj.EvaluationGroup = NNIntegration.JobTitleQueue.EvaluationGroup.Top10;
      else if (evalutionGroup == Constants.Module.EvaluationGroupCodes.OutOfCategories)
        _obj.EvaluationGroup = NNIntegration.JobTitleQueue.EvaluationGroup.OutOfCategories;
      else
        return false;
      
      return true;
    }
  }
}