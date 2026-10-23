using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace DirRX.NNOtherProcesses.Server.NNOtherProcessesBlocks
{
  partial class WeekendWorkStatementSetRestDateCanceledBlockHandlers
  {

    public virtual void WeekendWorkStatementSetRestDateCanceledBlockExecute()
    {
      var logPrefix = string.Format("WeekendWorkStatementSetRestDateCanceled. ИД задачи: {0}", _obj.Id);
      Logger.DebugFormat("{0}. Начало выполнения блока.", logPrefix);
      
      var restOrder = WeekendWorkRestOrderDocuments.As(HRPro.HRDocFlow.HRProcessTasks.As(_obj).DocumentGroup.OfficialDocuments.FirstOrDefault());
      if (restOrder == null)
      {
        Logger.DebugFormat("{0}. Во вложениях задачи отсутствует Приказ на доп. день отдыха.", logPrefix);
        return;
      }
      
      var restStatement = WeekendWorkRestStatementDocuments.As(restOrder.Statement);
      if (restStatement == null)
      {
        Logger.DebugFormat("{0}. Не найдено заявление на доп. день отдыха.", logPrefix);
        return;
      }
      
      var statement = restStatement.SourceDocument;
      if (statement == null)
      {
        Logger.DebugFormat("{0}. Не найдена заявка на привлечение к работе в ВД/ПД/СР.", logPrefix);
        return;
      }
      
      logPrefix = string.Format("{0}. ИД заявления на доп. день отдыха: {1}. ИД заявки ВД/ПД/СР: {2}", logPrefix, restStatement.Id, statement.Id);
      Logger.DebugFormat("{0}. Заполнение признака в документе.", logPrefix);
      
      // Установить блокировку на запись.
      var isLocked = Locks.TryLock(statement);
      if (!Locks.GetLockInfo(statement).IsLockedByMe && !isLocked)
      {
        var lockInfo = Locks.GetLockInfo(statement);
        Logger.DebugFormat("{0}. Не удалось установить блокировку. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}.",
                           logPrefix, lockInfo.OwnerName, lockInfo.LockTime);
        if (_block.RetrySettings.RetryIteration < NNCommons.Constants.Module.DefaultBlockRetryCount)
        {
          _block.RetrySettings.Retry = true;
          _block.RetrySettings.NextRetryTime = Calendar.Now.AddMinutes(NNCommons.Constants.Module.DefaultBlockNextRetryTime);
        }
        return;
      }
      
      var employeeInfo = statement.Employees.Where(x => DirRX.OverrideSolution.Employees.Equals(x.Employee, restOrder.EmployeeDirRX)).FirstOrDefault();
      if (employeeInfo == null)
      {
        Logger.DebugFormat("{0}. Сотрудник ИД: {1} не найден в табличной части заявки ВД/ПД/СР: {2}.", logPrefix, restOrder.EmployeeDirRX.Id, statement.Id);
        return;
      }
      
      employeeInfo.RestDateCanceled = true;
      
      try
      {
        statement.Save();
        Logger.DebugFormat("{0}. Признак \"Доп.день отдыха отменен\" успешно установлен в ТЧ \"Данные по сотрудникам\" заявки.",
                           logPrefix);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при установке признака \"Доп.день отдыха отменен\" в ТЧ \"Данные по сотрудникам\" заявки. Текст ошибки: {1}. StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
      }
      finally
      {
        if(Locks.GetLockInfo(statement).IsLockedByMe)
          Locks.Unlock(statement);
      }
    }
  }

  partial class SetWeekendWorkOrderDocumentTextMarksBlockHandlers
  {

    public virtual void SetWeekendWorkOrderDocumentTextMarksBlockExecute()
    {
      var logPrefix = string.Format("SetWeekendWorkOrderDocumentTextMarks. ИД задачи: {0}", _obj.Id);
      Logger.DebugFormat("{0}. Начало выполнения блока.", logPrefix);
      
      var document = _obj.Attachments.Where(d => WeekendWorkOrderDocuments.Is(d)).FirstOrDefault();
      var order = WeekendWorkOrderDocuments.As(document);
      
      if (order == null || !order.HasVersions)
      {
        throw AppliedCodeException.Create(string.Format("{0}. Документ не найден или не содержит версию.", logPrefix));
      }
       
      var version = order.Versions.Last();
      try
      {
        if (!Locks.TryLock(order) || !Locks.TryLock(version.Body))
        {
          Logger.Debug("SetWeekendWorkOrderDocumentTextMarks. Документ или его версия заблокирована.");
          _block.OutProperties.ExecutionResult = ExecutionResult.LockError;
          return;
        }

        var execControl = order.ExecutionControl != null ? Sungero.Company.PublicFunctions.UserBase.GetUserShortName(order.ExecutionControl, DeclensionCase.Accusative, false) : string.Empty;
        var textReplacements = new List<NNCommons.Structures.Module.ITextReplacement>();
        textReplacements.Add(NNCommons.Structures.Module.TextReplacement.Create(DirRX.NNOtherProcesses.Resources.TextMark_DueTo, order.DueTo ?? string.Empty, false));
        textReplacements.Add(NNCommons.Structures.Module.TextReplacement.Create(DirRX.NNOtherProcesses.Resources.TextMark_ExecControl, execControl, false));
        using (var documentStream = version.Body.Read())
        {
          using(var resultDocumentStream = NNCommons.IsolatedFunctions.SetTextIntoDocument.SetTextIntoPdfDocument(documentStream, textReplacements))
          {
            order.CreateVersionFrom(resultDocumentStream, "pdf");
          }
        }
        
        order.Save();
      }
      catch (Exception ex)
      {
        throw AppliedCodeException.Create(string.Format("{0}. Возникла непредвиденная ошибка.", logPrefix), ex);
      }
      finally
      {
        if (Locks.GetLockInfo(order).IsLockedByMe)
          Locks.Unlock(order);
        
        if (Locks.GetLockInfo(version.Body).IsLockedByMe)
          Locks.Unlock(version.Body);
      }
    }
  }
  
  partial class CreateWeekendWorkRestStatementBlockHandlers
  {

    public virtual void CreateWeekendWorkRestStatementBlockExecute()
    {
      var logPrefix = string.Format("CreateWeekendWorkRestStatement. ИД задачи: {0}", _obj.Id);
      Logger.DebugFormat("{0}. Начало выполнения блока.", logPrefix);
      
      var task = WeekendWorkAgreementTasks.As(_obj);
      if (task == null)
      {
        Logger.DebugFormat("{0}. Задача ИД: {1} не является задачей на создание согласия на привлечение к работе.", logPrefix, _obj.Id);
        return;
      }
      
      var statement = _block.WeekendWorkStatement;
      var employee = statement.Employees.Where(x => x.Employee.Id == _block.Employee.Id).FirstOrDefault();
      
      if (employee == null)
        throw AppliedCodeException.Create(string.Format("Сотрудник ИД: {0} не указан в заявке.", _block.Employee.Id));
      
      var compensations = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.WeekendWorkCompensationKindsForRestStatement.Guid).Split(',');
      if (employee.Compensation == null || !compensations.Contains(employee.Compensation.Id.ToString()))
      {
        Logger.DebugFormat("{0}. Вид компенсации сотрудника ИД: {1} не входит в список видов, для которых необходимо создавать заявление на доп. день отдыха.", logPrefix, _block.Employee.Id);
        return;
      }
      
      if (employee.RestDateFrom == null)
      {
        Logger.DebugFormat("{0}. У сотрудника ИД: {1} не указана Дата доп. дня отдыха.", logPrefix, _block.Employee.Id);
        return;
      }
      
      var restStatement = WeekendWorkRestStatementDocuments.Create();
      
      restStatement.IsNGD = employee.Employee.IsNGDDirRX;
      restStatement.SourceDocument = statement;
      restStatement.RestDate = employee.RestDateFrom;
      restStatement.OurSignatory = employee.Employee;
      restStatement.EmployeeDirRX = employee.Employee;
      restStatement.IsTopDirRX = OverrideSolution.JobTitles.As(employee.Employee.JobTitle)?.IsTopDirRX;
      
      var template = NNCommons.PublicFunctions.Module.Remote.GetTemplateByKind(restStatement.DocumentKind,  restStatement.BusinessUnit);
      if (template == null)
      {
        var error = string.Format("Не найден шаблон для документа. ИД вида документа: {0}. ИД НОР: {1}.",  restStatement.DocumentKind,  restStatement.BusinessUnit);
        Logger.ErrorFormat("{0}. ИД заявления: {1}. Таб. номер сотрудника: {2}. {3}",
                           logPrefix, restStatement.Id, employee.PersonnelNumber, error);
        throw AppliedCodeException.Create(error);
      }
      
      try
      {
        using (var body = template.LastVersion.Body.Read())
        {
          var newVersion = restStatement.CreateVersionFrom(body, template.AssociatedApplication.Extension);
          var internalEntity = (Sungero.Domain.Shared.IExtendedEntity)restStatement;
          internalEntity.Params[Sungero.Content.Shared.ElectronicDocumentUtils.FromTemplateIdKey] = template.Id;
          restStatement.Save();
        }
        task.AddendaGroup.OfficialDocuments.Add(restStatement);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при создании заявления. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        throw AppliedCodeException.Create(ex.Message);
      }
    }
  }

  partial class SetRejectedOrderStatusForEmployeesBlockHandlers
  {
    public virtual void SetRejectedOrderStatusForEmployeesBlockExecute()
    {
      var logPrefix = string.Format("SetRejectedOrderStatusForEmployees. ИД задачи: {0}", _obj.Id);
      Logger.DebugFormat("{0}. Начало выполнения блока.", logPrefix);
      
      var task = WeekendWorkCancelationTasks.GetAll(t => t.Id == _obj.MainTaskId).FirstOrDefault();
      if (task != null)
      {
        var statement = task.WeekendWorkOrder?.WeekendWorkStatement;
        var employees = task.Employees.Select(x => x.Employee).ToList();
        
        logPrefix = string.Format("{0}. ИД задачи на оформление отмены работы в ВД/ПД/СР: {1}. ИД заявки ВД/ПД/СР: {2}", logPrefix, task.Id, statement?.Id);
        Logger.DebugFormat("{0}. Заполнение признака в документе.", logPrefix);
        
        foreach (var employeeInfo in statement.Employees.Where(x => employees.Contains(x.Employee)))
          employeeInfo.RejectedByOrder = true;
        
        try
        {
          statement.Save();
          Logger.DebugFormat("{0}. Признак \"Выход отменен\" успешно установлен в ТЧ \"Данные по сотрудникам\" заявки.",
                             logPrefix);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. Ошибка при установке признака \"Выход отменен\" в ТЧ \"Данные по сотрудникам\" заявки. Текст ошибки: {1}. StackTrace: {2}.",
                             logPrefix, ex.Message, ex.StackTrace);
        }
      }
      else
      {
        Logger.DebugFormat("{0}. Не найдена ведущая задача на оформление отмены работы в ВД/ПД/СР.", logPrefix);
        
        var currTask = HRPro.HRDocFlow.HRProcessTasks.As(_obj);
        if (currTask != null)
        {
          if (currTask.DocumentGroup.OfficialDocuments.Any(d => WeekendWorkOrderDocuments.Is(d)))
          {
            var statement = WeekendWorkOrderDocuments.As(currTask.DocumentGroup.OfficialDocuments.FirstOrDefault(d => WeekendWorkOrderDocuments.Is(d)))?.WeekendWorkStatement;
            logPrefix = string.Format("{0}. ИД заявки ВД/ПД/СР: {1}", logPrefix, statement?.Id);
            Logger.DebugFormat("{0}. Заполнение признака в документе.", logPrefix);
            
            foreach (var employeeInfo in statement.Employees)
              employeeInfo.RejectedByOrder = true;
            
            try
            {
              statement.Save();
              Logger.DebugFormat("{0}. Признак \"Выход отменен\" успешно установлен в ТЧ \"Данные по сотрудникам\" заявки.",
                                 logPrefix);
            }
            catch (Exception ex)
            {
              Logger.ErrorFormat("{0}. Ошибка при установке признака \"Выход отменен\" в ТЧ \"Данные по сотрудникам\" заявки. Текст ошибки: {1}. StackTrace: {2}.",
                                 logPrefix, ex.Message, ex.StackTrace);
            }
          }
          else
            Logger.DebugFormat("{0}. Во вложениях текущей задачи не найдено документов с типом \"Приказ (распоряжение) о работе в ВД/ПД/СР\".", logPrefix);
        }
        else
          Logger.DebugFormat("{0}. Текущая задача не является задачей на согласование документов, связанных с работой.", logPrefix);
      }
    }
  }
}