using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNOtherProcesses.Structures.Module
{
  /// <summary>
  /// Результат проверки ранее оформленного заявления.
  /// </summary>
  [Public]
  partial class VerificationResultPayment
  {
    /// <summary>
    /// Результат проверки.
    /// </summary>
    public bool VerificationResult { get; set; }
    
    /// <summary>
    /// Текст замечания.
    /// </summary>
    public string CommentText { get; set; }
  }
  
  /// <summary>
  /// Период заданий/уведомлений по замещению для виджета workItemList.
  /// </summary>
  partial class AssignmentItemPeriod
  {
    /// <summary>
    /// Начало.
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Конец.
    /// </summary>
    public DateTime? EndDate { get; set; }
  }
  
  /// <summary>
  /// Элемент списка заданий/уведомлений по замещению для виджета workItemList.
  /// </summary>
  [Public]
  partial class HistoryAssignmentItem
  {
    //кем выполнено
    /// <summary>
    /// Заголовок задания.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// Имя системы.
    /// </summary>
    public string system { get; set; }

    /// <summary>
    /// Имя типа задания.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Идентификатор задания.
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// Иконка задания.
    /// </summary>
    public string icon { get; set; }

    /// <summary>
    /// Признак прочтения задания.
    /// </summary>
    public bool isRead { get; set; }

    /// <summary>
    /// Признак просроченности задания.
    /// </summary>
    public bool isExpired { get; set; }

    /// <summary>
    /// Срок выполнения задания в формате ISO 8601.
    /// </summary>
    public string deadline { get; set; }
    
    /// <summary>
    /// ФИО замещаемого сотрудника.
    /// </summary>
    public string assignee { get; set; }
    
    /// <summary>
    /// Признак выполнения задания по замещению.
    /// </summary>
    public bool bySubstitution{ get; set; }
    
    /// <summary>
    /// Дата завершения задания в формате ISO 8601.
    /// </summary>
    public string finished { get; set; }
    
    /// <summary>
    /// Состояние задания.
    /// </summary>
    public string status { get; set; }
    
    /// <summary>
    /// Результат выполнения.
    /// </summary>
    public string result { get; set; }
    
    /// <summary>
    /// Имя действия.
    /// </summary>
    public string defaultActionName { get; set; }

    /// <summary>
    /// Заголовок кнопки действия.
    /// </summary>
    public string defaultActionCaption { get; set; }

    /// <summary>
    /// Иконка действия.
    /// </summary>
    public string defaultActionIcon { get; set; }
    
  }

  /// <summary>
  /// Элемент списка заданий/уведомлений по замещению для виджета workItemList.
  /// </summary>
  [Public]
  partial class AssignmentItem
  {
    /// <summary>
    /// Заголовок задания.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// Имя системы.
    /// </summary>
    public string system { get; set; }

    /// <summary>
    /// Имя типа задания.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Идентификатор задания.
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// Иконка задания.
    /// </summary>
    public string icon { get; set; }

    /// <summary>
    /// Признак прочтения задания.
    /// </summary>
    public bool isRead { get; set; }

    /// <summary>
    /// Признак просроченности задания.
    /// </summary>
    public bool isExpired { get; set; }

    /// <summary>
    /// Дата создания задания в формате ISO 8601.
    /// </summary>
    public string created { get; set; }

    /// <summary>
    /// Срок выполнения задания в формате ISO 8601.
    /// </summary>
    public string deadline { get; set; }
    
    /// <summary>
    /// Дата выполнения задания в формате ISO 8601.
    /// </summary>
    public string completed { get; set; }
    
    /// <summary>
    /// Состояние задания.
    /// </summary>
    public string status { get; set; }
    
    /// <summary>
    /// Результат выполнения.
    /// </summary>
    public string result { get; set; }

    /// <summary>
    /// Имя действия.
    /// </summary>
    public string defaultActionName { get; set; }

    /// <summary>
    /// Заголовок кнопки действия.
    /// </summary>
    public string defaultActionCaption { get; set; }

    /// <summary>
    /// Иконка действия.
    /// </summary>
    public string defaultActionIcon { get; set; }
    
  }

  /// <summary>
  /// Элемент списка заданий/уведомлений по замещению для виджета workItemList.
  /// </summary>
  [Public]
  partial class SubstitutionAssignmentItem
  {
    /// <summary>
    /// Заголовок задания.
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// Имя системы.
    /// </summary>
    public string system { get; set; }

    /// <summary>
    /// Имя типа задания.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Идентификатор задания.
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// Иконка задания.
    /// </summary>
    public string icon { get; set; }

    /// <summary>
    /// Признак прочтения задания.
    /// </summary>
    public bool isRead { get; set; }

    /// <summary>
    /// Признак просроченности задания.
    /// </summary>
    public bool isExpired { get; set; }

    /// <summary>
    /// ФИО замещаемого сотрудника.
    /// </summary>
    public string assignee { get; set; }

    /// <summary>
    /// Дата создания задания в формате ISO 8601.
    /// </summary>
    public string created { get; set; }

    /// <summary>
    /// Срок выполнения задания в формате ISO 8601.
    /// </summary>
    public string deadline { get; set; }

    /// <summary>
    /// Признак, разрешено ли выполнять задание по замещению.
    /// </summary>
    public bool isExecutable { get; set; }
  }

  #region Регистрация работы в выходной и/или праздничный день
  
  /// <summary>
  /// Структура с информацией по виду компенсации для ЛК.
  /// </summary>
  [Public (Isolated = true)]
  partial class CompensationKindStructure
  {
    /// <summary>
    /// Ид компенсации.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Запрашивать дату доп. отдыха.
    /// </summary>
    public bool NeedRestDate { get; set; }
    
    /// <summary>
    /// Дата доп. отдыха обязательна для заполнения.
    /// </summary>
    public bool IsRestDateRequired { get; set; }
  }
  
  /// <summary>
  /// Данные для доработки заявления на работу в выходной и/или праздничный день.
  /// </summary>
  [Public(Isolated = true)]
  partial class StatementDataForReworkWeekendWork
  {
    /// <summary>
    /// ИД исполнителя.
    /// </summary>
    public long InitiatorId { get; set; }
    
    /// <summary>
    /// Информация об исполнителе.
    /// </summary>
    public string PerformerInfo { get; set; }
    
    /// <summary>
    /// ИД доп. согласующих через запятую.
    /// </summary>
    public string AdditionalApproverIds { get; set; }
    
    /// <summary>
    /// ИД заявления.
    /// </summary>
    public long StatementId { get; set; }
    
    /// <summary>
    /// Дата работы с.
    /// </summary>
    public DateTime WorkDateFrom { get; set; }
    
    /// <summary>
    /// Дата работы по.
    /// </summary>
    public DateTime WorkDateTo { get; set; }
    
    /// <summary>
    /// Первый перерыв с.
    /// </summary>
    public DateTime? FirstBreakFrom { get; set; }
    
    /// <summary>
    /// Первый перерыв по.
    /// </summary>
    public DateTime? FirstBreakTo { get; set; }
    
    /// <summary>
    /// Второй перерыв с.
    /// </summary>
    public DateTime? SecondBreakFrom { get; set; }
    
    /// <summary>
    /// Второй перерыв по.
    /// </summary>
    public DateTime? SecondBreakTo { get; set; }
    
    /// <summary>
    /// Дата доп. отдыха с.
    /// </summary>
    public DateTime? RestDateFrom { get; set; }
    
    /// <summary>
    /// Дата доп. отдыха по.
    /// </summary>
    public DateTime? RestDateTo { get; set; }
    
    /// <summary>
    /// Компенсация.
    /// </summary>
    public long Compensation { get; set; }
    
    /// <summary>
    /// Причина.
    /// </summary>
    public long Reason { get; set; }
    
    /// <summary>
    /// Основание.
    /// </summary>
    public string ReasonDescription { get; set; }
  }
  
  #endregion
  
}
