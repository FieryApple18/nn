using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.LrdPaymentType;

namespace DirRX.NNOtherProcesses.Server
{
  partial class LrdPaymentTypeFunctions
  {
    /// <summary>
    /// Получить список дублирующихся записей.
    /// </summary>
    /// <returns>Список дублей.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<ILrdPaymentType> GetDuplicates()
    {
      return LrdPaymentTypes.GetAll()
        .Where(x => x.Id != _obj.Id && LrdEventReasons.Equals(x.EventReason, _obj.EventReason) &&
               LrdEventReasonClarifications.Equals(x.EventReasonClarification, _obj.EventReasonClarification) &&
               LrdPaymentPurposes.Equals(x.PaymentPurpose, _obj.PaymentPurpose));
    }
    
    /// <summary>
    /// Создать запись справочника.
    /// </summary>
    /// <param name="name">Наименование.</param>
    /// <param name="eventReasonCode">Код причины мероприятия.</param>
    /// <param name="eventReasonClarificationName">Имя уточнения причины мероприятия.</param>
    /// <param name="paymentPurposeName">Имя назначения платежа.</param>
    /// <returns>Ошибка. Пустая строка, если ошибок нет.</returns>
    public static string CreateRecord(string name, string eventReasonCode, string eventReasonClarificationName, string paymentPurposeName, string codeVO)
    {
      if (string.IsNullOrEmpty(eventReasonCode))
        return "Не передан код причины мероприятия";
      
      if (string.IsNullOrEmpty(eventReasonClarificationName))
        return "Не передано имя уточнения причины мероприятия";
      
      var eventReason = LrdEventReasons.GetAll(x => x.Code == eventReasonCode).FirstOrDefault();
      var eventReasonClarification = LrdEventReasonClarifications.GetAll(x => x.Name == eventReasonClarificationName).FirstOrDefault();
      var paymentPurpose = string.IsNullOrEmpty(paymentPurposeName) ?
        LrdPaymentPurposes.Null : LrdPaymentPurposes.GetAll(x => x.Name == paymentPurposeName).FirstOrDefault();
      
      return CreateRecord(name, eventReason, eventReasonClarification, paymentPurpose, codeVO);
    }
    
    /// <summary>
    /// Создать запись справочника.
    /// </summary>
    /// <param name="name">Наименование.</param>
    /// <param name="eventReason">Причины мероприятия.</param>
    /// <param name="eventReasonClarification">Уточнение причины мероприятия.</param>
    /// <param name="paymentPurpose">Назначения платежа.</param>
    /// <returns>Ошибка. Пустая строка, если ошибок нет.</returns>
    public static string CreateRecord(string name, ILrdEventReason eventReason, ILrdEventReasonClarification eventReasonClarification, ILrdPaymentPurpose paymentPurpose, string codeVO)
    {
      if (string.IsNullOrEmpty(name))
        return "Не заполнено наименование типа выплаты";
      
      if (eventReason == null)
        return "Не заполнена причина мероприятия";
      
      if (eventReasonClarification == null)
        return "Не заполнено уточненние причиня мероприятия";
      
      var isCreated = LrdPaymentTypes.GetAll(x => LrdEventReasons.Equals(x.EventReason, eventReason) &&
                                             LrdEventReasonClarifications.Equals(x.EventReasonClarification, eventReasonClarification) &&
                                             LrdPaymentPurposes.Equals(x.PaymentPurpose, paymentPurpose)).Any();
      if (isCreated)
        return string.Empty;
      
      var paymentType = LrdPaymentTypes.Create();
      var error = string.Empty;
      paymentType.Name = name;
      paymentType.EventReason = eventReason;
      paymentType.EventReasonClarification = eventReasonClarification;
      paymentType.PaymentPurpose = paymentPurpose;
      paymentType.CodeVO = codeVO;
      
      try
      {
        paymentType.Save();
      }
      catch (Exception ex)
      {
        error = string.Format("Ошибка при сохранении типа выплаты \"{0}\": {1}.", name, ex.Message);
        Logger.DebugFormat("CreateRecord. {0}", error);
      }
      
      return error;
    }
  }
}