using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkRestOrderDocument;

namespace DirRX.NNOtherProcesses.Server
{
  partial class WeekendWorkRestOrderDocumentFunctions
  {

    /// <summary>
    /// Получить приказ на доп. день отдыха.
    /// </summary>
    /// <param name="restStatement">Заявление на доп день отдыха.</param>
    /// <returns> Приказ на доп. день отдыха</returns>
    [Public]
    public static IWeekendWorkRestOrderDocument GetRestOrder(IWeekendWorkRestStatementDocument restStatement)
    {
      return WeekendWorkRestOrderDocuments
        .GetAll(x => WeekendWorkRestStatementDocuments.Equals(x.Statement, restStatement))
        .FirstOrDefault();
    }
    
    /// <summary>
    /// Заполнить свойства и тело приказа на доп. день отдыха.
    /// </summary>
    /// <param name="structure">Структура входящего запроса на создание приказа.</param>
    /// <param name="statement">Заявка.</param>
    /// <param name="restStatement">Заявление на доп. день отдыха.</param>
    /// <param name="canceledOrder">Отменяемый приказ.</param>
    /// <param name="ourSignatory">Подписал.</param>
    /// <param name="preparedBy">Подготовил.</param>
    /// <param name="employee">Сотрудник.</param>
    public string FillWeekendWorkRestOrderProperties(NNIntegration.Structures.Module.ICreateWeekendWorkRestOrderStructure structure,
                                                     IWeekendWorkStatementDocument statement, IWeekendWorkRestStatementDocument restStatement, IWeekendWorkRestOrderDocument canceledOrder,
                                                     OverrideSolution.IEmployee ourSignatory, OverrideSolution.IEmployee preparedBy, OverrideSolution.IEmployee employee)
    {
      _obj.DocumentKind = Functions.Module.GetDocumentKindForWeekendWorkRestOrder(structure.Kind);
      _obj.LeadingRestOrder = canceledOrder;
      _obj.Statement = restStatement;
      _obj.BusinessUnit = statement.BusinessUnit;
      _obj.Department = statement.Department;
      _obj.OurSignatory = ourSignatory;
      _obj.PreparedBy = preparedBy;
      _obj.RegistrationNumber = structure.RegNumber;
      _obj.RegistrationDate = structure.Date;
      _obj.SAPEventDateDirRX = structure.DateEvent;
      _obj.ExternalId = structure.SAPID;
      _obj.EmployeeDirRX = employee;
      _obj.PersonnelNumberDirRX = employee.PersonnelNumber;
      _obj.IsNGDDirRX = employee.IsNGDDirRX;
      _obj.IsTopDirRX = OverrideSolution.JobTitles.As(employee.JobTitle)?.IsTopDirRX;
      
      _obj.HRLifeCycleStateHRPro = DirRX.NNOtherProcesses.WeekendWorkRestOrderDocument.HRLifeCycleStateHRPro.Draft;
      
      var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.WeekendWorkRestOrderExtension.Guid);

      if (string.IsNullOrEmpty(extension))
        return string.Format("Прикладная константа '{0}' не заполнена.", Constants.Module.ConstantsSetting.WeekendWorkRestOrderExtension.Guid);
      
      // Документ сохраняется во время создания новой версии.
      var error = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(_obj, structure.Body, extension);
      return error;
    }
  }
}