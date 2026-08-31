using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.EssPlatform.Structures.Module
{

  /// <summary>
  /// Атрибуты задания/уведомления в Супернику.
  /// </summary>
  [Public]
  partial class SupernikaNotifiactionParams
  {
    /// <summary>
    /// Тема.
    /// </summary>
    public string Subject { get; set; }
    /// <summary>
    /// Текст.
    /// </summary>
    public string Text { get; set; }
    /// <summary>
    /// Уведомлять по email.
    /// </summary>
    public bool NeedEmailNotification { get; set; }
    /// <summary>
    /// Доступные действия.
    /// </summary>
    public List<DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.ISupernikaAction> Actions { get; set; }
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime? Created { get; set; }
  }

  /// <summary>
  /// Обновить данные сотрудника.
  /// </summary>
  [Public]
  partial class UpdateEssUserJson
  {
    /// <summary>
    /// Номер телефона.
    /// </summary>
    public string phoneNumber { get; set; }
    /// <summary>
    /// Данные для обновления.
    /// </summary>
    public DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.IClaim claims { get; set; }
  }

  /// <summary>
  /// Дополнительный параметр.
  /// </summary>
  [Public]
  partial class Claim
  {
    /// <summary>
    /// ИД персоны.
    /// </summary>
    public long DirectumRX_PersonId { get; set; }
    
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string DirectumRX_Username { get; set; }
    
    /// <summary>
    /// Телефон.
    /// </summary>
    public string phone { get; set; }
    
  }

  #region IS-F-25401. Передача результата нажатия на действие в центре уведомлений ПМД (ПМД → КЭДО).

  /// <summary>
  /// Уведомление в MessageBroker.
  /// </summary>
  [Public]
  partial class MessageBrokerNotification
  {
    public DirRX.EssPlatform.Structures.Module.IMessageBrokerNotificationIdentity Identity { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public int Priority { get; set; }

    public int DeliveryMethod { get; set; }
    
    public string DeliveryProxy { get; set; }
    
    public string ExpirationPeriod { get; set; }

    public System.Collections.Generic.Dictionary<string, string> Properties { get; set; }
    
    public List<DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.ISupernikaAction> Actions { get; set; }

    public List<DirRX.EssPlatform.Structures.Module.IMessageBrokerNotificationAttachment> Attachments { get; set; }
  }

  /// <summary>
  /// Действие в Супернике.
  /// </summary>
  [Public]
  partial class SupernikaAction
  {
    public string Name { get; set; }

    public string Title { get; set; }
    
    public DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.ISupernikaActionParameter Parameters { get; set; }
  }

  /// <summary>
  /// Параметры действия в Суперники.
  /// </summary>
  [Public]
  partial class SupernikaActionParameter
  {
    public bool Comment_required { get; set; }

    public string Button_type { get; set; }
  }

  #endregion

  /// <summary>
  /// Структура для создания пользователя в ЛК.
  /// </summary>
  [Public]
  partial class EssUserActivateJson
  {
    public string userName { get; set; }
    
    public DirRX.EssPlatform.Structures.Module.IPersonData person { get; set; }
    
    public DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.IClaim identities { get; set; }
    
    public DirRX.EssPlatform.Structures.Module.IAuthentication authentication { get; set; }
    
    public string inviteToResource {get; set; }
  }

}