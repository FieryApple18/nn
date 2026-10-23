using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.Shell.Server
{
  partial class OnEventHoldingDirRXFolderHandlers
  {
    public virtual IQueryable<DirRX.OverrideSolution.IHRAssignment> OnEventHoldingDirRXDataQuery(IQueryable<DirRX.OverrideSolution.IHRAssignment> query)
    {
      query = query.Where(t => t.IsManualHoldingEventDirRX == true && t.DocForManualHoldingEventDirRX != null);
      
      if (_filter == null)
        return query;
      
       // Фильтры по статусу и периоду.
       var result = query.Select(q => Sungero.Workflow.AssignmentBases.As(q));
       result = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result, _filter.InProcess,
                                                                                            _filter.Last30Days, _filter.Last90Days, _filter.Last180Days, false);
       query = result.Select(q => HRAssignments.As(q));
      
      if (_filter.DocumentKindDirRX != null)
        query = query.Where(t => DocumentKinds.Equals(t.DocForManualHoldingEventDirRX.DocumentKind, _filter.DocumentKindDirRX));
      
      if (_filter.BusinessUnitDirRX != null)
        query = query.Where(t => BusinessUnits.Equals(t.DocForManualHoldingEventDirRX.BusinessUnit, _filter.BusinessUnitDirRX));
         
      if (_filter.PersonnelSectionDirRX != null)
        query = query.Where(t => NNCommons.PersonnelSections.Equals(t.DocForManualHoldingEventDirRX.PersonnelSectionDirRX, _filter.PersonnelSectionDirRX));

      if (_filter.BusinessProcessDirRX != null)
        query = query.Where(t => NNCommons.BusinessProcesses.Equals(DocumentKinds.As(t.DocForManualHoldingEventDirRX.DocumentKind).BusinessProcessDirRX, _filter.BusinessProcessDirRX));
      
      if (_filter.NGDDirRX)
        query = query.Where(t => t.DocForManualHoldingEventDirRX.IsNGDDirRX == true);
      
      if (_filter.NotNGDDirRX)
        query = query.Where(t => t.DocForManualHoldingEventDirRX.IsNGDDirRX != true);

      return query;
    }
  }

  partial class OnApprovalFolderHandlers
  {

    public override IQueryable<Sungero.Workflow.IAssignmentBase> OnApprovalDataQuery(IQueryable<Sungero.Workflow.IAssignmentBase> query)
    {
      query = base.OnApprovalDataQuery(query);
      if (_filter == null)
        return query;
      
      // Новыми фильтрами фильтруются только задания на согласование документов, связанных с работой.
      var approvalAssignments = query.Where(t => DocumentApprovalAssignments.Is(t)).Select(ap => DocumentApprovalAssignments.As(ap))
                                     .Where(t => t.ApprovalDocumentDirRX != null);
      
      // В списке должны остаться остальные задания на согласование без учета новых фильтров.
      var approvalAssignmentsIds = approvalAssignments.Select(t => t.Id);
      var otherAssignmentsId = query.Where(t => !approvalAssignmentsIds.Any(ap => ap == t.Id)).Select(t => t.Id).ToList();
      
      if (_filter.DocumentKindDirRX != null)
        approvalAssignments = approvalAssignments.Where(t => DocumentKinds.Equals(t.ApprovalDocumentDirRX.DocumentKind, _filter.DocumentKindDirRX));
      
      if (_filter.BusinessUnitDirRX != null)
        approvalAssignments = approvalAssignments.Where(t => BusinessUnits.Equals(t.ApprovalDocumentDirRX.BusinessUnit, _filter.BusinessUnitDirRX));
     
      
      if (_filter.PersonnelSectionDirRX != null)
        approvalAssignments = approvalAssignments.Where(t => NNCommons.PersonnelSections.Equals(t.ApprovalDocumentDirRX.PersonnelSectionDirRX, _filter.PersonnelSectionDirRX));
      
      if (_filter.BusinessProcessDirRX != null)
        approvalAssignments = approvalAssignments.Where(t => NNCommons.BusinessProcesses.Equals(DocumentKinds.As(t.ApprovalDocumentDirRX.DocumentKind).BusinessProcessDirRX, _filter.BusinessProcessDirRX));
      
      if (_filter.NGDDirRX)
        approvalAssignments = approvalAssignments.Where(t => t.ApprovalDocumentDirRX.IsNGDDirRX == true);
      
      if (_filter.NotNGDDirRX)
        approvalAssignments = approvalAssignments.Where(t => t.ApprovalDocumentDirRX.IsNGDDirRX != true);
      
      approvalAssignmentsIds = approvalAssignments.Select(t => t.Id);
      query = query.Where(t => approvalAssignmentsIds.Any(ap => ap == t.Id) || otherAssignmentsId.Contains(t.Id));
      
      return query;
    }
  }

  partial class ShellHandlers
  {
  }
}