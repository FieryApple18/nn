using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.ESignAssignment;
using Status = HRPro.HRDocFlow.ESignAssignment.Status;

namespace DirRX.OverrideSolution.Client
{
  partial class ESignAssignmentCollectionActions
  {

    public virtual bool CanMassApproveSignDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void MassApproveSignDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (Functions.ESignAssignment.TaskHaveSinglePerformer(_objs))
      {
        Dialogs.ShowMessage(DirRX.OverrideSolution.ESignAssignments.Resources.TitleMassApprovePerformet,
                            DirRX.OverrideSolution.ESignAssignments.Resources.DescripionMassApprovePerformer,
                            MessageType.Warning);
        return;
      }

      var performer = _objs.FirstOrDefault()?.Performer;
      var checkSubstitutionErrorMessage = Functions.ESignAssignment.CheckExecuteAccessRight(
        new List<IUser>() { performer },
        DirRX.OverrideSolution.ESignAssignments.Resources.ApproveOperation);
      
      if (!string.IsNullOrEmpty(checkSubstitutionErrorMessage))
      {
        Dialogs.ShowMessage(checkSubstitutionErrorMessage, MessageType.Warning);
        return;
      }
      
      var certificate = Sungero.CoreEntities.Certificates.Null;
      
      if (_objs.Any(c => c.AllowSignSubstitution.HasValue && c.AllowSignSubstitution.Value))
        certificate = Functions.ESignAssignment.SelectSignCertificate();
      else
      {
        Dialogs.ShowMessage(DirRX.OverrideSolution.ESignAssignments.Resources.AssignmentsNotSign);
        return;
      }

      if (certificate == Certificates.Null)
      {
        Dialogs.ShowMessage(DirRX.OverrideSolution.ESignAssignments.Resources.CertificateMissing);
        return;
      }

      var processSignTasks = Functions.ESignAssignment.ProcessSignTasks(_objs.ToList(), certificate);
      Functions.ESignAssignment.ShowMassActionResult(processSignTasks);
    }

  }
}