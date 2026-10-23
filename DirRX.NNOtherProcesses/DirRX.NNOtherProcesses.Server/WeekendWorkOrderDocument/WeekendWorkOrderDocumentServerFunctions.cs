using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.WeekendWorkOrderDocument;

namespace DirRX.NNOtherProcesses.Server
{
  partial class WeekendWorkOrderDocumentFunctions
  {

    /// <summary>
    /// Заполнить свойства и тело приказа (распоряжение) о работе в ВД/ПД/СР.
    /// </summary>
    /// <param name="structure">Структура входящего запроса на создание приказа.</param>
    /// <param name="statement">Заявка.</param>
    /// <param name="canceledOrder">Отменяемый приказ.</param>
    /// <param name="ourSignatory">Подписал.</param>
    /// <param name="preparedBy">Подготовил.</param>
    public string FillWeekendWorkOrderProperties(NNIntegration.Structures.Module.ICreateWeekendWorkOrderStructure structure,
                                                 IWeekendWorkStatementDocument statement, IWeekendWorkOrderDocument canceledOrder,
                                                 OverrideSolution.IEmployee ourSignatory, OverrideSolution.IEmployee preparedBy)
    {
      _obj.DocumentKind = Functions.Module.GetDocumentKindForWeekendWorkOrder(structure.Kind);
      _obj.IsGroup = statement.IsGroup;
      _obj.IsNGDDirRX = statement.IsNGDDirRX;
      _obj.BaseOrder = canceledOrder;
      _obj.WeekendWorkStatement = statement;
      _obj.BusinessUnit = statement.BusinessUnit;
      _obj.Department = statement.Department;
      _obj.PersonnelSectionDirRX = statement.PersonnelSectionDirRX;
      _obj.OurSignatory = ourSignatory;
      _obj.PreparedBy = preparedBy;
      _obj.RegistrationNumber = structure.RegNumber;
      _obj.RegistrationDate = structure.Date;
      _obj.SAPEventDateDirRX = structure.DateEvent;
      _obj.ExternalId = structure.SAPID;
      _obj.DueTo = statement.DueTo;
      _obj.ExecutionControl = statement.ExecutionControl;
      
      // В HR св-во HRLifeCycleStateHRPro продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно"
      // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
      // Используется в HRLifeCycleStateHRProChanged официального документа.
      _obj.HRLifeCycleStateHRPro = DirRX.NNOtherProcesses.WeekendWorkOrderDocument.HRLifeCycleStateHRPro.Draft;
      
      var extension = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.WeekendWorkOrderExtension.Guid);

      if (string.IsNullOrEmpty(extension))
        return string.Format("Прикладная константа '{0}' не заполнена.", Constants.Module.ConstantsSetting.WeekendWorkOrderExtension.Guid);
      
      // Документ сохраняется во время создания новой версии.
      var error = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.CreateVersionFromBase64(_obj, structure.Body, extension);
      return error;
    }
    
    /// <summary>
    /// Проверить, является ли вид документа - "Приказ (распоряжение) об отмене работы по приказу (распоряжению)".
    /// </summary>
    /// <param name="documentKind">Вид документа.</param>
    /// <returns>Результат проверки.</returns>
    [Remote(IsPure = true)]
    public static bool IsCancelWork(Sungero.Docflow.IDocumentKind documentKind)
    {
      var link = Sungero.Docflow.PublicFunctions.Module.GetExternalLinks(documentKind).FirstOrDefault();
      return link?.ExternalEntityId == Constants.Module.DocumentKind.WeekendWorkCancelationOrderDocument.ToString();
    }
  }
}