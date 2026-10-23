using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.ESignAssignment;
using Status = HRPro.HRDocFlow.ESignAssignment.Status;

namespace DirRX.OverrideSolution.Client
{
  partial class ESignAssignmentActions
  {
    public override void ForRevision(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.ForRevision(e);
    }

    public override bool CanForRevision(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return (_obj.Performer.Equals(Users.Current) ||
              (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowedDirRX.HasValue && _obj.IsSubstitutionAllowedDirRX.Value == true)) &&
        base.CanForRevision(e);
    }

    public override void Refuse(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Refuse(e);
    }

    public override bool CanRefuse(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return (_obj.Performer.Equals(Users.Current) ||
              (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowedDirRX.HasValue && _obj.IsSubstitutionAllowedDirRX.Value == true)) &&
        base.CanRefuse(e);
    }

    public override void Sign(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Sign(e);
    }

    public override bool CanSign(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return (_obj.Performer.Equals(Users.Current) ||
              (HRPro.HRDocFlow.PublicFunctions.Module.Remote.IsPerformer(_obj.Performer) && _obj.IsSubstitutionAllowedDirRX.HasValue && _obj.IsSubstitutionAllowedDirRX.Value == true)) &&
        base.CanSign(e);
    }

  }

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