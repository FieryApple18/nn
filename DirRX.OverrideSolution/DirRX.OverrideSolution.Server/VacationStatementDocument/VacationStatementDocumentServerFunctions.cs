using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationStatementDocument;

namespace DirRX.OverrideSolution.Server
{
  partial class VacationStatementDocumentFunctions
  {

    /// <summary>
    /// Найти заявление на перенос отпуска по задаче "Задача на подтверждение дат отпуска (по графику)".
    /// </summary>
    /// <param name="task">Задача на подтверждение дат отпуска (по графику).</param>
    /// <returns>Заявление на перенос отпуска.</returns>
    [Public]
    public static IVacationStatementDocument GetDocumentByRegistrationTask(NNVacations.IRegistrationOfAnnualLeave task)
    {
      // Получаем данные о отпусках из задачи.
      var taskVacationData = task.VacationData.ToList();
      
      // Получаем сотрудника из задачи.
      var employee = task.EmployeeAttachmentGroup.Employees.FirstOrDefault();
      if (employee == null)
      {
        Logger.ErrorFormat("GetDocumentByRegistrationTask. ИД задачи: {0}. Не заполнен сотрудник во вложении задачи на подтверждение дат отпуска (по графику).", task.Id);
        return VacationStatementDocuments.Null;
      }
      
      // Ищем документ, у которого VacationDataDirRX полностью совпадает с данными из задачи.
      // HACK: NHibernate не может транслировать подзапрос All(). Добавлено замыкание в список после фильтра по состоянию документа и сотруднику.
      // Таких документов в работе должно быть мало.
      var document = VacationStatementDocuments.GetAll()
        .Where(x =>
               (x.InternalApprovalState == OverrideSolution.VacationStatementDocument.InternalApprovalState.OnApproval ||
                x.InternalApprovalState == OverrideSolution.VacationStatementDocument.InternalApprovalState.OnRework) &&
               Employees.Equals(employee, x.Employee))
        .ToList()
        .Where(x =>
               x.VacationDataDirRX.Count == taskVacationData.Count &&
               x.VacationDataDirRX.All(xd =>
                                       taskVacationData.Any(td =>
                                                            xd.VacationType == td.VacationType &&
                                                            xd.StartDate == td.StartDate &&
                                                            xd.EndDate == td.EndDate &&
                                                            xd.Days == td.Days
                                                           )
                                      )
              )
        .FirstOrDefault();
      
      return document;
    }

  }
}