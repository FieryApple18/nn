using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using DirRX.OverrideSolution.OfficialDocument;
using DirRX.OverrideSolution.Server;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace DirRX.NNCommons.Server.NNCommonsBlocks
{
  partial class ExecuteActiveMonitoringsBlockHandlers
  {

    public virtual void ExecuteActiveMonitoringsBlockExecute()
    {
      var logPrefix = string.Format("ExecuteActiveMonitorings. ИД задачи {0}", _obj.Id);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      
      foreach (var task in _block.Tasks)
      {
        Logger.DebugFormat("{0}. Выполнение блоков мониторинга в задаче ИД: {1}.", logPrefix, task.Id);
        task.Blocks.ExecuteAllMonitoringBlocks();
      }
      
      Logger.DebugFormat("{0}. Конец выполнения.", logPrefix);
    }
  }

  partial class LinkDocumentToTaskBlockHandlers
  {
    public virtual void LinkDocumentToTaskBlockExecute()
    {
      var logPrefix = string.Format("LinkDocumentToTaskBlockExecute. ИД задачи {0}", _obj.Id);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      var errorsExsists = false;
      var separator = NNCommons.PublicConstants.Module.Separator.ToString();
      var docLinkChangeList = _block.DocsForLink.ToList();
      var docDateChangeList = _block.DocsForUpdate.ToList();
      var lockInfo = Locks.GetLockInfo(_obj);
      // Заполнение списка ИД задач в документе.
      if (_block.Tasks.Any())
      {
        var tasksIds = _block.Tasks.Select(t => t.Id).ToList();
        Logger.DebugFormat("{0}. Начало обработки документов с ИД: {1} для записи ИД задач.", logPrefix, string.Join(separator, docLinkChangeList.Select(d => d.Id)));
        for (int i = 0; i < docLinkChangeList.Count; i++)
        {
          var document = docLinkChangeList[i];
          // Проверить блокировку документа.
          if (Locks.TryLock(document))
          {
            var docTaskIds = document.TaskIdsDirRX;
            foreach (var taskId in tasksIds)
            {
              // Записать ИД задачи если его ещё нет в коллекции.
              if (!docTaskIds.Where(t => t.TaskId == taskId).Any())
                docTaskIds.AddNew().TaskId = taskId;
            }
            if (document.State.IsChanged)
            {
              document.Save();
              docLinkChangeList.RemoveAt(i);
            }
            Locks.Unlock(document);
          }
          else
          {
            lockInfo = Locks.GetLockInfo(document);
            Logger.ErrorFormat("{0}. {1}.", logPrefix, lockInfo.LockedMessage);
            errorsExsists = true;
          }
        }
        if (docLinkChangeList.Count > 0)
        {
          Logger.DebugFormat("{0}. ИД документов, которые не получилось обработать: {1} для записи ИД задач.", logPrefix, string.Join(separator, docLinkChangeList.Select(d => d.Id)));
        }
      }
      
      if (_block.SentDate != null)
      {
        // Заполнение даты отправки документа на согласование.
        Logger.DebugFormat("{0}. Начало обработки документов с ИД: {1} для записи даты отправки.", logPrefix, string.Join(separator, docDateChangeList.Select(d => d.Id)));
        for (int index = 0; index < docDateChangeList.Count; index++)
        {
          // Проверить блокировку документа.
          var doc = docDateChangeList[index];
          if (Locks.TryLock(doc))
          {
            if (doc.SentForApprovalDateDirRX.GetValueOrDefault() < _block.SentDate)
            {
              doc.SentForApprovalDateDirRX = _block.SentDate;
              doc.Save();
              docDateChangeList.RemoveAt(index);
            }
            Locks.Unlock(doc);
          }
          else
          {
            lockInfo = Locks.GetLockInfo(doc);
            Logger.ErrorFormat("{0}. {1}.", logPrefix, lockInfo.LockedMessage);
            errorsExsists = true;
          }
        }
        if (docDateChangeList.Count > 0)
        {
          Logger.DebugFormat("{0}. ИД документов, которые не получилось обработать: {1} для записи даты отправки.", logPrefix, string.Join(separator, docDateChangeList.Select(d => d.Id)));
        }
      }
      if (errorsExsists)
      {
        Logger.DebugFormat("{0}. Возникли ошибки. Блок будет выполнен повторно.", logPrefix);
        var locksInfostr = Resources.LinkDocumentToTaskBlockLocksInfoFormat(string.Join(separator, docDateChangeList.Select(d => d.Id)), string.Join(separator, docLinkChangeList.Select(d => d.Id)));
        throw new Sungero.Domain.Shared.Exceptions.RepeatedLockException(true, locksInfostr, lockInfo);
      }
      Logger.DebugFormat("{0}. Конец выполнения.", logPrefix);
    }
  }
  partial class StatementReworkBlockHandlers
  {

    public virtual void StatementReworkBlockCompleteAssignment(DirRX.NNCommons.IStatementReworkAssignment assignment)
    {
      if (assignment.Result == NNCommons.StatementReworkAssignment.Result.ForReapproving)
      {
        var mainDocs = assignment.DocumentGroup.OfficialDocuments;
        var addendaDocs = assignment.AddendaGroup.OfficialDocuments;
        DirRX.OverrideSolution.Module.HRWred.PublicFunctions.Module.CreateSigningOperation(mainDocs, addendaDocs, Sungero.Company.Employees.As(assignment.CompletedBy),
                                                                                           Sungero.Company.Employees.As(assignment.Performer), false);
      }
    }
    public virtual void StatementReworkBlockStartAssignment(DirRX.NNCommons.IStatementReworkAssignment assignment)
    {
      assignment.ShowInSelfServiceOffice = _block.ShowInSelfServiceOffice;
      assignment.Facility = _block.Facility;
      assignment.Remark = _block.Remark;
      assignment.SupernikaSubjectDirRX = _block.SupernikaSubjectDirRX;
      assignment.SupernikaTextDirRX = _block.SupernikaTextDirRX;
      assignment.IsSubstitutionAllowed = _block.IsSubstitutionAllowed ?? false;
      assignment.IsSubstitutionVisible = _block.IsSubstitutionVisible ?? false;
      
      // Отправить сообщение о новом задании.
      if (_block.NeedNotification.GetValueOrDefault())
      {
        DirRX.EssPlatform.PublicFunctions.Module.SendNewNotification(OverrideSolution.Employees.As(assignment.Performer), false,
                                                                     HRPro.HRProSolution.PublicConstants.Module.TargetNameSpace,
                                                                     DirRX.EssPlatform.PublicConstants.Module.SelfOfficeObjectTypes.WorkItems,
                                                                     Constants.Module.ObjectCardTypeNames.StatementReworkAssignment,
                                                                     assignment.Id.ToString(),
                                                                     assignment.ShowInSelfServiceOffice ?? false);
      }
    }
  }

  partial class AbortTaskBlockHandlers
  {

    public virtual void AbortTaskBlockExecute()
    {
      var task = _block.Task;
      if (task != null)
      {
        if (task.Status == Sungero.Workflow.Task.Status.InProcess ||
            task.Status == Sungero.Workflow.Task.Status.Suspended ||
            task.Status == Sungero.Workflow.Task.Status.UnderReview)
        {
          Logger.DebugFormat("AbortTaskBlockExecute. ИД задачи: {0}. Прекращение задачи.", task.Id);
          task.Abort();
        }
        else
        {
          Logger.DebugFormat("AbortTaskBlockExecute. ИД задачи: {0}. Прекращение задачи не требуется.", task.Id);
        }
      }
      else
      {
        Logger.DebugFormat("AbortTaskBlockExecute. Передано пустое значение.");
      }
      
    }
  }

  partial class DeleteDocumentsBlockHandlers
  {

    public virtual void DeleteDocumentsBlockExecute()
    {
      var logPrefix = string.Format("DeleteDocumentsBlockExecute. Документы на удаление: {0}",
                                    string.Join(", ", _block.DocumentsToBeDeleted.Select(x => x.Id)));
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      
      if (!_block.DocumentsToBeDeleted.Any())
        Logger.DebugFormat("{0}. Нет документов на удаление.", logPrefix);
      
      var documents = _block.DocumentsToBeDeleted.Distinct();
      foreach (var document in documents)
      {
        var lockInfo = Locks.GetLockInfo(document);
        if (lockInfo.IsLockedByOther)
        {
          Logger.ErrorFormat("{0}. {1}.", logPrefix, lockInfo.LockedMessage);
          continue;
        }
        
        try
        {
          // Удалить связи.
          var relationsTypes = Sungero.CoreEntities.RelationTypes.GetAll();
          foreach (var relationType in relationsTypes)
          {
            foreach (var relation in document.Relations.GetRelatedFromDocuments(relationType.Name))
              document.Relations.RemoveFrom(relationType.Name, relation);
            foreach (var relation in document.Relations.GetRelatedDocuments(relationType.Name))
              document.Relations.Remove(relationType.Name, relation);
          }
          // Установить статус На удалении.
          // В HR св-во HRLifeCycleStateHRPro продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
          // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
          // Используется в HRLifeCycleStateHRProChanged официального документа.
          document.HRLifeCycleStateHRPro = OverrideSolution.OfficialDocument.HRLifeCycleStateHRPro.OnDestruction;
          // Запретить доступ.
          document.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Forbidden);
          document.AccessRights.Save();
          // Удалить тела.
          while (document.Versions.Any())
            document.DeleteVersion(document.LastVersion);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. Ошибка выполнения: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        }
      }
      
      Logger.DebugFormat("{0}. Конец выполнения.", logPrefix);
    }
  }

  partial class AddDocRelationsBlockHandlers
  {

    public virtual void AddDocRelationsBlockExecute()
    {
      var logPrefix = string.Format("AddDocRelationsBlockExecute. Основной документ: {0}. Связанные документы: {1}. Тип связи: {2}",
                                    _block.MainDocument?.Id,
                                    string.Join(", ", _block.RelatedDocuments.Select(x => x.Id)),
                                    _block.RelationType?.Name);
      Logger.DebugFormat("{0}. Начало выполнения.", logPrefix);
      
      var missingProperties = new List<string>();
      if (_block.MainDocument == null)
        missingProperties.Add("Основной документ");
      if (_block.RelationType == null)
        missingProperties.Add("Тип связи");
      
      if (missingProperties.Any())
      {
        Logger.ErrorFormat("{0}. Ошибка: не заполнены обязательные свойства блока: {1}.", logPrefix, string.Join(", ", missingProperties));
        return;
      }
      else if (!_block.RelatedDocuments.Any())
      {
        Logger.DebugFormat("{0}. Нет связанных документов.", logPrefix);
        return;
      }
      
      try
      {
        foreach (var relatedDocument in _block.RelatedDocuments)
        {
          _block.MainDocument.Relations.Add(_block.RelationType.Name, relatedDocument);
        }
        _block.MainDocument.Relations.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка выполнения: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
      }
      
      Logger.DebugFormat("{0}. Конец выполнения.", logPrefix);
    }
  }

}