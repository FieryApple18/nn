using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.TransferRequestDocument;

namespace DirRX.NNOtherProcesses
{
  partial class TransferRequestDocumentSharedHandlers
  {

    public override void BusinessUnitChanged(Sungero.Docflow.Shared.OfficialDocumentBusinessUnitChangedEventArgs e)
    {
      base.BusinessUnitChanged(e);
      
      if (e.NewValue == null)
        _obj.TransferReason = null;
    }

    public virtual void TransferReasonChanged(DirRX.NNOtherProcesses.Shared.TransferRequestDocumentTransferReasonChangedEventArgs e)
    {
      
    }

    public virtual void TransferTypeChanged(Sungero.Domain.Shared.EnumerationPropertyChangedEventArgs e)
    {
      var needFillTransferEndDate = _obj.TransferType == TransferRequestDocument.TransferType.Temporary;
      
      _obj.State.Properties.TransferEndDate.IsVisible = needFillTransferEndDate;
      _obj.State.Properties.TransferEndDate.IsEnabled = needFillTransferEndDate;
      _obj.State.Properties.TransferEndDate.IsRequired = needFillTransferEndDate;
      
    }

    public override void EmployeeDirRXChanged(DirRX.OverrideSolution.Shared.DocRelatedToWorkBaseEmployeeDirRXChangedEventArgs e)
    {
      // Предзаполняются: Табельный номер, Наша орг., НГД, ТОП (из должности сотр.), Раздел персонала.
      // TODO Что с ТОП? Из АСУП или нет?
      base.EmployeeDirRXChanged(e);
      
      if (!DirRX.OverrideSolution.Employees.Equals(e.NewValue, e.OldValue))
      {
        if (e.NewValue != null)
        {
          _obj.LineManager = e.NewValue.LineManagerDirRX;
          _obj.Department = e.NewValue.Department;
        }
        // TODO Нужна очистка нередактируемых св-в (Лин. рук - редактируемый)?
        else
        {
          _obj.PersonnelNumberDirRX = null;
          _obj.BusinessUnit = null;
          _obj.IsNGDDirRX = null;
          _obj.IsTopDirRX = null;
          _obj.PersonnelSectionDirRX = null;
          _obj.Department = null;
        }
      }
      
      Sungero.Docflow.PublicFunctions.OfficialDocument.FillName(_obj);
    }

  }
}