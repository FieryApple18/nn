using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.NNVacations.IndividualVacationScheduleAcquaintanceTask;
using DirRX.OverrideSolution;

namespace DirRX.NNVacations.Server.IndividualVacationScheduleAcquaintanceTaskBlocks
{
  partial class CreateIndividualScheduleAcquaintanceBlockHandlers
  {
    /// <summary>
    /// Формирование индивидуального листа ознакомления.
    /// </summary>
    public virtual void CreateIndividualScheduleAcquaintanceBlockExecute()
    {
      Logger.Debug("CreateIndividualScheduleAcquaintanceBlockExecute. Начало выполнения блока.");
      var task = VacationScheduleManagementTasks.GetAll().FirstOrDefault(x => x.Id == _obj.VacationScheduleManagementTaskId);
      var vacationScheduleDocument = task.DocumentGroup.VacationScheduleDocuments.FirstOrDefault();
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.IndividualAcquiantanceVacScheduleListKind);
      if (vacationScheduleDocument == null)
        throw AppliedCodeException.Create("Не удалось получить документ График отпусков. ");
      if (docKind == null)
        throw AppliedCodeException.Create("Не удалось получить вид документа Индивидуальный лист ознакомления с графиком отпусков.");
      var template = Functions.Module.GetTemplateByKind(docKind, BusinessUnits.As(_obj.Employee?.BusinessUnit));
      if (template == null)
        throw AppliedCodeException.Create("Не удалось получить шаблон индивидуального листа ознакомления.");
      var doc = OverrideSolution.VacationScheduleDocuments.CreateFrom(template);
      doc.DocumentKind = docKind;
      doc.AcquiantanceOptionDirRX = OverrideSolution.VacationScheduleDocument.AcquiantanceOptionDirRX.Electronic;
      doc.ConsentDirRX = true;
      doc.EmployeeDirRX = _obj.Employee;
      doc.Department = _obj.Employee.Department;
      doc.Year = task.Year;
      doc.RegistrationNumber = vacationScheduleDocument.RegistrationNumber;
      doc.RegistrationDate = vacationScheduleDocument.RegistrationDate;
      doc.UpdateTemplateParameters();
      doc.Save();
      var prefix = IndividualVacationScheduleAcquaintanceTasks.Resources.CreateScheduleAcquaintanceBlockPrefix;
      OverrideSolution.PublicFunctions.VacationScheduleDocument.Remote.CreateFromTemplate(doc, prefix);
      
      var docGroup = _obj.DocumentGroup.VacationScheduleDocuments;
      if (docGroup.Any())
        docGroup.Clear();
      docGroup.Add(doc);
      
      Logger.DebugFormat("CreateIndividualScheduleAcquaintanceBlockExecute. Ид документа: {0}. Конец выполнения блока.", doc.Id);
    }
  }

}