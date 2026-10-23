using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNVacations.VacationScheduleManagementTask;

namespace DirRX.NNVacations
{
  partial class VacationScheduleManagementTaskServerHandlers
  {

    public override void BeforeAbort(Sungero.Workflow.Server.BeforeAbortEventArgs e)
    {
      base.BeforeAbort(e);
      
      if (e.IsValid)
      {
        var document = _obj.DocumentGroup.VacationScheduleDocuments.FirstOrDefault();
        
        if (document != null)
        {
          Functions.Module.VacationScheduleManagementTaskAbortHandler(document);
          NNIntegration.PublicFunctions.Module.StartCreatedOutgoingMessages(NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.VacationScheduleProcessStatus,
                                                                            new List<long> {document.Id});
        }
      }
    }
  }
}