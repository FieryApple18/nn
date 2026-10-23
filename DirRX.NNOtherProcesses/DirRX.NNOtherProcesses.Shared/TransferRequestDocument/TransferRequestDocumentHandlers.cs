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