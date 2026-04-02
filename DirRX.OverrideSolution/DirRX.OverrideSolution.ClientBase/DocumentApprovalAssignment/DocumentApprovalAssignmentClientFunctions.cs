using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocumentApprovalAssignment;

namespace DirRX.OverrideSolution.Client
{
  partial class DocumentApprovalAssignmentFunctions
  {

    /// <summary>
    /// Диалог для запроса информации по подзадаче для запроса мнения.
    /// </summary>
    public OverrideSolution.Structures.HRDocFlow.DocumentApprovalAssignment.OpinionTaskInfoStructure ShowLrdPaymentOpinionDialog()
    {
      var dialog = Dialogs.CreateInputDialog(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.LrdPaymentOpinionDialogTitle);
      var subject = dialog.AddString(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.LrdPaymentOpinionDialogSubjectFieldName, true);
      var performers = dialog.AddSelectMany(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.LrdPaymentOpinionDialogPerformersFieldName, true, Employees.Null);
      var copyTo = dialog.AddSelectMany(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.LrdPaymentOpinionDialogObserversFieldName, false, Employees.Null);
      dialog.Buttons.AddOkCancel();
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var structure = OverrideSolution.Structures.HRDocFlow.DocumentApprovalAssignment.OpinionTaskInfoStructure.Create
          (subject.Value,
           copyTo.Value,
           performers.Value);
        
        return structure;
      }
      else
        return null;
    }

    /// <summary>
    /// Диалог для запроса информации по выплате ЛНА.
    /// </summary>
    /// <returns>Результат выполнения. Если пользователь нажал Ок, то возвращается - True, иначе - False.</returns>
    public bool ShowLrdPaymentInfoDialog()
    {
      var doc = _obj.Task.Attachments.Where(x => DirRX.NNOtherProcesses.LrdPaymentStatementDocuments.Is(x)).FirstOrDefault();
      var statement = DirRX.NNOtherProcesses.LrdPaymentStatementDocuments.As(doc);
      if (statement == null)
        return false;
      
      var dialog = Dialogs.CreateInputDialog(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.LrdPaymentInfoDialogTitle);
      dialog.Width = 350;
      var paymentDate = dialog.AddDate(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.LrdPaymentInfoDialogPaymentDateFieldName, false, statement.PayDate);
      var amount = dialog.AddDouble(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.LrdPaymentInfoDialogAmountFieldName, true, statement.ApprovedAmount);
      dialog.Buttons.AddOkCancel();
      
      dialog.SetOnRefresh((er) => {
                            if (paymentDate.Value.HasValue && paymentDate.Value.Value < Calendar.Today)
                              er.AddError(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.DialogLrdPaymentInfoDateError);
                          });
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        _obj.LrdPaymentAmountDirRX = amount.Value;
        _obj.LrdPaymentDateDirRX = paymentDate.Value;
        return true;
      }
      
      return false;
    }

    /// <summary>
    /// Диалог для переадресации задания дополнительному согласующему.
    /// </summary>
    public virtual void ShowAddApproverDialog()
    {
      var dialog = Dialogs.CreateInputDialog(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.AddingApproverDialogTitle);
      var employee = dialog.AddSelect<IEmployee>(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.ApproverDialogFieldName, true, null)
        .Where(x => x.Status != OverrideSolution.Employee.Status.Closed);
      var addButton = dialog.Buttons.AddCustom(Sungero.Docflow.FreeApprovalTasks.Resources.Add);
      dialog.Buttons.AddCancel();
      dialog.SetOnButtonClick(a =>
                              {
                                if (a.Button == addButton && !Functions.DocumentApprovalAssignment.Remote.CanForwardTo(_obj, employee.Value))
                                  a.AddError(DirRX.OverrideSolution.DocumentApprovalAssignments.Resources.ApproverAlreadyExistsFormat(employee.Value.Person.ShortName));
                              });
      
      if (dialog.Show() == addButton)
        Functions.DocumentApprovalAssignment.Remote.ForwardToNewApprover(_obj, employee.Value);
    }

  }
}