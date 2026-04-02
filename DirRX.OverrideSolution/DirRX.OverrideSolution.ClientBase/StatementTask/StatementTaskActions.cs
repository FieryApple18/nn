using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.StatementTask;

namespace DirRX.OverrideSolution.Client
{
  partial class StatementTaskActions
  {

    // HACK: Копирование базового действия Start, с добавлением работы с новой группой вложения VacationStatementDocuments.
    public override void Start(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var isVacationDoc = _obj.DocumentGroup.DocRelatedToWorkBases.Any();
      var statement = isVacationDoc ? OfficialDocuments.As(_obj.DocumentGroup.DocRelatedToWorkBases.FirstOrDefault()) :
        OfficialDocuments.As(_obj.DocumentGroup.StatementDocuments.FirstOrDefault());
      
      var lastVersion = statement.LastVersion;
      if (lastVersion == null)
      {
        e.AddError(HRPro.HRDocFlow.StatementTasks.Resources.DocumentHasNotVersions);
        return;
      }
      
      var errorsList = new List<string>();
      if (!isVacationDoc)
      {
        var setting = HRPro.HRDocFlow.PublicFunctions.Module.Remote.GetStatementSetting(HRPro.HRDocFlow.StatementDocuments.As(statement));
        if (setting == null)
        {
          e.AddError(HRPro.HRDocFlow.Resources.FailedGetSettingByDocKind);
          return;
        }
        
        var needAttachments = HRPro.HRDocFlow.PublicFunctions.Module.Remote.CheckSettingForStatementAttachments(setting);
        if (needAttachments && _obj.OtherGroup.All.Count == 0)
        {
          e.AddError(HRPro.HRDocFlow.Resources.NeedAttachmentsForStatementFormat(statement.DocumentKind.Name));
          return;
        }
      }
      
      var isChanged = statement.History.GetAll().Any(h => h.User == Users.Current &&
                                                     h.Action == Sungero.CoreEntities.History.Action.Update &&
                                                     h.Operation == new Enumeration(HRPro.HRDocFlow.Constants.Module.Operation.UpdateVerBody));
      if (!isVacationDoc && !isChanged)
      {
        e.AddError(HRPro.HRDocFlow.Resources.CorrectStatementBeforeSending);
        return;
      }
      
      if (Sungero.Docflow.PublicFunctions.Module.VersionIsLocked(statement.Versions.ToList()))
      {
        e.AddError(HRPro.HRDocFlow.Resources.SaveAndCloseStatementBeforeSending);
        return;
      }
      
      var message = HRPro.HRCommons.PublicFunctions.Module.ConvertToPdfa(statement).ErrorMessage;
      if (!string.IsNullOrEmpty(message))
        errorsList.Add(message);
      
      var validationErrors = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalValidationErrors(statement, true);
      if (validationErrors.Any())
      {
        e.AddError(string.Join(Environment.NewLine, validationErrors));
        return;
      }

      var statementApproved = Sungero.Docflow.PublicFunctions.Module.ApproveWithAddenda(statement, null, null, null, false, true, string.Empty);
      if (!statementApproved)
        return;
      
      if (errorsList.Any())
      {
        e.AddError(string.Join(Environment.NewLine, errorsList));
        return;
      }

      // TODO Sannikova_YS. Св-во HRLifeCycleStateHRPro устарело и не должно использоваться.
      // В HR св-во продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
      // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
      // Используется в HRLifeCycleStateHRProChanged официального документа.
      HRPro.HRDocFlow.PublicFunctions.Module.Remote.SetHRLifeCycleState(new List<Sungero.Docflow.IOfficialDocument>() {statement},
                                                                        HRPro.HRProSolution.OfficialDocument.HRLifeCycleStateHRPro.Active);
      
      _obj.Start();
    }

    public override bool CanStart(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanStart(e);
    }

  }
}