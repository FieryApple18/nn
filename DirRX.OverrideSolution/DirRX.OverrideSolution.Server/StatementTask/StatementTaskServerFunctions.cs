using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.StatementTask;

namespace DirRX.OverrideSolution.Server
{
  partial class StatementTaskFunctions
  {

    // HACK: Копирование базовой функции StatementInstruction, с добавлением работы с новой группой вложения VacationStatementDocuments.
    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public Sungero.Core.StateView StatementVacationInstruction()
    {
      var instruction = string.Empty;
      var needAttachments = false;
      
      if (!_obj.DocumentGroup.DocRelatedToWorkBases.Any())
      {
        var statement = _obj.DocumentGroup.StatementDocuments.FirstOrDefault();
        var setting = HRPro.HRDocFlow.PublicFunctions.Module.Remote.GetStatementSetting(statement);
        
        needAttachments = HRPro.HRDocFlow.PublicFunctions.Module.Remote.CheckSettingForStatementAttachments(setting);
      }
      
      instruction = needAttachments ? HRPro.HRDocFlow.Resources.StatementTaskInstructionWithAddendum :
        HRPro.HRDocFlow.Resources.StatementTaskInstructionWithoutAddendum;
      
      return DirRX.OverrideSolution.Module.HRDocFlow.Functions.Module.GetAssignmentStateView(instruction);
    }

  }
}