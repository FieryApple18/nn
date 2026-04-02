using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNVacations
{
  partial class GroupAcquiantanceVacScheduleReportServerHandlers
  {
    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.IndividualAcquiantanceVacScheduleListKind);
      var businessUnit = DirRX.OverrideSolution.BusinessUnits.GetAll(d => d.Id == GroupAcquiantanceVacScheduleReport.BusinessUnitId).FirstOrDefault();
      GroupAcquiantanceVacScheduleReport.StructuralDepartment = businessUnit.NameInGenitiveCaseDirRX;
      GroupAcquiantanceVacScheduleReport.DocKindId = docKind != null ? docKind.Id : 0;
    }
  }
}