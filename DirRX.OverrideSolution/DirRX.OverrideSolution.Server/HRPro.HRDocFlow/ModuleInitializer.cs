using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.OverrideSolution.Module.HRDocFlow.Server
{
  public partial class ModuleInitializer
  {

    /// <summary>
    /// Создать вид HR документа и обновить его.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="shortName">Сокращенное имя.</param>
    /// <param name="numerationType">Нумерация.</param>
    /// <param name="direction">Документопоток.</param>
    /// <param name="autoFormattedName">Признак автоформирования имени.</param>
    /// <param name="autoNumerable">Признак автонумерации.</param>
    /// <param name="typeGuid">Доступный тип документа.</param>
    /// <param name="actions">Действия отправки по умолчанию.</param>
    /// <param name="initID">ИД инициализации.</param>
    /// <param name="isDefault">Признак вида документа по умолчанию.</param>
    /// <param name="approver">Документ утверждает.</param>
    /// <param name="daysForSignEmployee">Дней на подписание сотрудником.</param>
    /// <param name="daysForSignManager">Дней на подписание работодателем.</param>
    /// <param name="documentKindsCodes">Код Минтруда.</param>
    /// <param name="isNeedTemplatesRXHRPro">Признак шаблона для RX.</param>
    /// <param name="isNeedTemplatesLKHRPro">Признак шаблона для ЛК.</param>
    /// <description>HACK: В стандартной поставке HR Pro не учитывается что вид документа могут удалить. После этого повторная инициализация не проходит.</description>
    [Public]
    public override void CreateHRDocumentKind(string name, string shortName, Enumeration numerationType, Enumeration direction, bool autoFormattedName,
                                              bool autoNumerable, Guid typeGuid, Sungero.Domain.Shared.IActionInfo[] actions, Guid initID, bool isDefault,
                                              Enumeration approver, int? daysForSignEmployee, int? daysForSignManager, string documentKindsCodes,
                                              bool isNeedTemplatesRXHRPro, bool isNeedTemplatesLKHRPro)
    {
      InitializationLogger.DebugFormat("CreateHRDocumentKind. Создание вида документа: {0}.", name);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateDocumentKind(name, shortName, numerationType, direction, autoFormattedName, autoNumerable, typeGuid, actions, initID, isDefault);
      
      // Стандартные значения дней на подписание\ознакомление если не были переданы (для заявлений).
      if(daysForSignEmployee.HasValue == false)
        daysForSignEmployee = HRPro.HRDocFlow.Constants.Module.DaysForSigningStatements;
      
      // Добавление доп. полей HR.
      try
      {
        var link = Sungero.Docflow.PublicFunctions.Module.GetExternalLink(Sungero.Docflow.Server.DocumentKind.ClassTypeGuid, initID);
        if (link != null)
        {
          var documentKind =  HRPro.HRProSolution.DocumentKinds.GetAll(r => r.Id == link.EntityId).FirstOrDefault();
          if(documentKind != null)
          {
            documentKind.ApproverHRPro = approver;
            documentKind.DaysForSignEmployeeHRPro = daysForSignEmployee;
            documentKind.DaysForSignManagerHRPro = daysForSignManager;
            documentKind.IsNeedTemplatesRXHRPro = isNeedTemplatesRXHRPro;
            documentKind.IsNeedTemplatesLKHRPro = isNeedTemplatesLKHRPro;
            documentKind.WredcCodeHRPro = documentKindsCodes;
            documentKind.IsHRDocumentHRPro = true;
            documentKind.Save();
          }
          else
            InitializationLogger.DebugFormat("CreateHRDocumentKind. Вид документа с именем '{0}' не найден по LinkEntityId: {1}. Обработка вид документа пропущена.", name, link.EntityId);
        }
      }
      catch (Exception ex)
      {
        InitializationLogger.DebugFormat("CreateHRDocumentKind. Произошла ошибка {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
      }
    }

  }
}
