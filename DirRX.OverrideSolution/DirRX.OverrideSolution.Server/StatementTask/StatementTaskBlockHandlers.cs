using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.OverrideSolution.StatementTask;

namespace DirRX.OverrideSolution.Server.StatementTaskBlocks
{
  partial class StatementReworkBlockDirRXHandlers
  {

    public virtual void StatementReworkBlockDirRXStartAssignment(DirRX.OverrideSolution.INNStatementReworkAssignment assignment)
    {
      assignment.ShowInSelfServiceOffice = _block.ShowInSelfServiceOfficeDirRX;
      assignment.Facility = _block.FacilityDirRX;
      assignment.Remark = _block.RemarkDirRX;
      
      // Отправить сообщение о новом задании.
      if (_block.NeedNotificationDirRX.HasValue && _block.NeedNotificationDirRX.Value == true)
      {
        DirRX.EssPlatform.PublicFunctions.Module.SendNewNotification(Employees.As(assignment.Performer), false,
                                                                     HRPro.HRProSolution.PublicConstants.Module.TargetNameSpace,
                                                                     DirRX.EssPlatform.PublicConstants.Module.SelfOfficeObjectTypes.WorkItems,
                                                                     OverrideSolution.Module.HRDocFlow.Constants.Module.ObjectCardTypeNames.NNStatementReworkAssignment,
                                                                     assignment.Id.ToString(),
                                                                     assignment.ShowInSelfServiceOffice ?? false);
      }
    }

  }
}