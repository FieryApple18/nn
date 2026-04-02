using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNOtherProcesses.Structures.Module
{
  
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
    /// ИД доп. согласующего.
    /// </summary>
    public long AdditionalApproverId { get; set; }
    
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