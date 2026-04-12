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

    /// <summary>
    /// Добавить замещения.
    /// </summary>       
    public void AddSubstitutions()
    {
      var substituteIds = !string.IsNullOrEmpty(_obj.SubstituteIdsDirRX) ? _obj.SubstituteIdsDirRX.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList() : new List<long>();
      var systemIds = !string.IsNullOrEmpty(_obj.SystemIdsDirRX) ? _obj.SystemIdsDirRX.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList() : new List<long>();
      var startDates = !string.IsNullOrEmpty(_obj.StartDatesDirRX) ? _obj.StartDatesDirRX.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(DateTime.Parse).ToList() : new List<DateTime>();
      var endDates = !string.IsNullOrEmpty(_obj.EndDatesDirRX) ? _obj.EndDatesDirRX.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(DateTime.Parse).ToList() : new List<DateTime>();
      
      for (var i = 0; i < substituteIds.Count(); i++)
      {
        var item = _obj.SubstitutionsDirRX.AddNew();
        
        item.Substitute = DirRX.OverrideSolution.Employees.GetAll(r => r.Id == substituteIds[i]).FirstOrDefault();
        item.SubstitutionSystem = DirRX.NNOtherProcesses.SubstitutionSystems.GetAll(s => s.Id == systemIds[i]).FirstOrDefault();
        item.StartDate = startDates[i];
        item.EndDate = endDates[i];
      }
    }

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