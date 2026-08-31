using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNVacations
{
  partial class AcquiantanceVacScheduleReportServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.AcquiantanceVacScheduleReport.SourceTableName, AcquiantanceVacScheduleReport.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      Logger.DebugFormat("AcquiantanceVacScheduleReport. Начало построения отчета.");
      AcquiantanceVacScheduleReport.SignedState = OverrideSolution.OfficialDocument.InternalApprovalState.Signed.Value;
      AcquiantanceVacScheduleReport.PendingSignState = OverrideSolution.OfficialDocument.InternalApprovalState.PendingSign.Value;
      AcquiantanceVacScheduleReport.ElectronicAcqOption = OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.Electronic.Value;
      AcquiantanceVacScheduleReport.Acquainted = DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.AcquaintedStatus;
      AcquiantanceVacScheduleReport.NotAcquainted = DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.NotAcquaintedStatus;
      AcquiantanceVacScheduleReport.HasConsent = DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.HasConsent;
      AcquiantanceVacScheduleReport.NoConsent = DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.NoConsent;
      AcquiantanceVacScheduleReport.OutsideConsent = DirRX.NNVacations.Reports.Resources.AcquiantanceVacScheduleReport.OutsideConsent;
      AcquiantanceVacScheduleReport.Signed = OverrideSolution.Employee.ConsentHRPro.Signed.Value;
      AcquiantanceVacScheduleReport.Initiator = Sungero.Company.Employees.Current?.Id;
      
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.IndividualAcquiantanceVacScheduleListKind);
      var reportSessionId = System.Guid.NewGuid().ToString();
      Logger.DebugFormat("AcquiantanceVacScheduleReport. GUID отчета: {0}", reportSessionId);
      AcquiantanceVacScheduleReport.DocTypeGuid = Constants.AcquiantanceVacScheduleReport.VacationScheduleDocumentDiscriminator;
      AcquiantanceVacScheduleReport.DocKindId = docKind != null ? docKind.Id : 0;
      AcquiantanceVacScheduleReport.ReportSessionId = reportSessionId;
      Logger.DebugFormat("AcquiantanceVacScheduleReport. Учитывается иерархия подразделений: {0}", AcquiantanceVacScheduleReport.IncludeHierarchy.Value);
      if (AcquiantanceVacScheduleReport.IncludeHierarchy.Value)
      {
        using (var connection = SQL.CreateConnection())
        {
          using (var command = connection.CreateCommand())
          {
            command.CommandText = string.Format(Queries.AcquiantanceVacScheduleReport.GetSubordinateDepartments,
                                                Constants.AcquiantanceVacScheduleReport.DepartmentDiscriminator,
                                                AcquiantanceVacScheduleReport.DepartmentIds);
            Logger.DebugFormat("AcquiantanceVacScheduleReport. Запрос на получение подчиненных подразделений: {0}", command.CommandText);
            var subDepartmentsIds = command.ExecuteScalar().ToString();
            Logger.DebugFormat("AcquiantanceVacScheduleReport. ИД подчиненных подразделений: {0}", subDepartmentsIds);
            foreach (var id in subDepartmentsIds.Split(';'))
            {
              if (!AcquiantanceVacScheduleReport.DepartmentIds.Contains(id))
                AcquiantanceVacScheduleReport.DepartmentIds += string.Format("{0}{1}", NNVacations.Constants.Module.ListIdsDelimiter, id);
            }
            Logger.DebugFormat("AcquiantanceVacScheduleReport. Значение в параметре DepartmentIds: {0}", AcquiantanceVacScheduleReport.DepartmentIds);
          };
        }
      }
      Logger.DebugFormat("AcquiantanceVacScheduleReport. ИД НОР: {0}", AcquiantanceVacScheduleReport.BusinessUnitIds);
      using (var connection = SQL.CreateConnection())
      {
        using (var command = connection.CreateCommand())
        {
          command.CommandText = string.Format(Queries.AcquiantanceVacScheduleReport.InsertDepartmentsInDepth,
                                              Constants.AcquiantanceVacScheduleReport.SourceTableName,
                                              Constants.AcquiantanceVacScheduleReport.DepartmentDiscriminator,
                                              reportSessionId, Constants.AcquiantanceVacScheduleReport.EmployeeDiscriminator,
                                              AcquiantanceVacScheduleReport.BusinessUnitIds, AcquiantanceVacScheduleReport.DepartmentIds);
          Logger.DebugFormat("AcquiantanceVacScheduleReport. Запрос на получение данных для отчета: {0}", command.CommandText);
          command.ExecuteScalar();
        };
      }
    }

  }
}