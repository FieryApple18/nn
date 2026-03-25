using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace DirRX.NNOtherProcesses.Server.NNOtherProcessesBlocks
{
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