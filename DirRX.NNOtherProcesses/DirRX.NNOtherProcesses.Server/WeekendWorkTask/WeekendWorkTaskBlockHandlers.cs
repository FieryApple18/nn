using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.NNOtherProcesses.WeekendWorkTask;

namespace DirRX.NNOtherProcesses.Server.WeekendWorkTaskBlocks
{
  partial class SetRejectedStatusForEmployeesBlockHandlers
  {

    public virtual void SetRejectedStatusForEmployeesBlockExecute()
    {
      var statement = _block.WeekendWorkStatement;
      foreach (var employeeInfo in statement.Employees)
        employeeInfo.RejectedByApproval = true;
      
      try
      {
        statement.Save();
        Logger.DebugFormat("SetRejectedStatusForEmployees. ИД заявки ВД/ПД/СР: {0}. Признак \"Отклонено\" успешно установлен в ТЧ \"Данные по сотрудникам\" заявки.", statement.Id);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("SetRejectedStatusForEmployees. ИД заявки ВД/ПД/СР: {0}. Ошибка при установке признака \"Отклонено\" в ТЧ \"Данные по сотрудникам\" заявки. Текст ошибки: {1}. StackTrace: {2}.",
                           statement.Id, ex.Message, ex.StackTrace);
      }
    }
  }

  partial class CreateAgreementTasksBlockHandlers
  {

    public virtual void CreateAgreementTasksBlockStartTask(DirRX.NNOtherProcesses.IWeekendWorkAgreementTask task, DirRX.NNOtherProcesses.IWeekendWorkTaskEmployees item)
    {
      task.Employee = item.Employee;
      task.Subject = _block.Subject;
      task.ActiveText = _block.Text;
      task.Author = _block.Author;
      task.WeekendWorkStatement = _block.WeekendWorkStatement;
    }

    public virtual void CreateAgreementTasksBlockStart()
    {
      _obj.Employees.Clear();
      var employees = _block.WeekendWorkStatement.Employees.Where(x => x.RejectedByApproval != true).Select(x => x.Employee);
      foreach (var employee in employees)
        _obj.Employees.AddNew().Employee = employee;
    }
  }


  partial class RegenerateStatementBodyBlockHandlers
  {

    public virtual void RegenerateStatementBodyBlockExecute()
    {
      var statement = _block.WeekendWorkStatement;
      Functions.WeekendWorkStatementDocument.RegenerateWeekendWorkStatementBody(statement);
    }
  }

}