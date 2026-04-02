using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Sungero.Core;
using Sungero.CoreEntities;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DirRX.OverrideSolution.Module.EssPlatform.Server
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Определить отношение указанной персоны к зарегистрированным ролям личного кабинета.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <returns>Список зарегистрированных ролей, у которых будут указаны списки акторов, имеющих отношение к персоне.</returns>
    /// <remarks>Добавлена обработка ролей для скрытия плиток в ЛК.</remarks>
    [Public]
    public override List<DirRX.EssPlatform.Structures.Module.IRoleActors> GetEssRolesActors(long personId)
    {
      var result = new List<DirRX.EssPlatform.Structures.Module.IRoleActors>();
      
      var roles = base.GetEssRolesActors(personId);
      
      foreach (var role in roles)
        if (role.actorIds.Any())
          result.Add(role);
      
      var employees = Employees.GetAll(x => x.Status == Employee.Status.Active && x.Person.Id == personId).ToList();
      var employeeIds = employees.Select(x => x.Id).ToList();
      var rolesShowTile = new List<Guid>
      {
        Constants.Module.RolesGuid.ShowTileMyVacations,
        Constants.Module.RolesGuid.ShowTileGetAnnualVacation,
        Constants.Module.RolesGuid.ShowTileGetVacationWithoutPay,
        Constants.Module.RolesGuid.ShowTileGetOtherVacation,
        Constants.Module.RolesGuid.ShowTileNNShiftVacationByStatement,
        Constants.Module.RolesGuid.ShowTileCompensationVacation,
        Constants.Module.RolesGuid.ShowTileNNRecallVacationEmployee,
        Constants.Module.RolesGuid.ShowTileNNSelfRecallVacationEmployee,
        Constants.Module.RolesGuid.ShowTileStatements,
        Constants.Module.RolesGuid.ShowTileWeekendWork,
        Constants.Module.RolesGuid.ShowTileOvertimeWork,
        Constants.Module.RolesGuid.ShowTileGetLrdPayment
      };
      
      foreach (var role in rolesShowTile)
      {
        if (!employees.Any(x => x.IncludedIn(role)))
        {
          var roleActors = DirRX.EssPlatform.Structures.Module.RoleActors.Create(role.ToString(), employeeIds);
          result.Add(roleActors);
        }
      }
      
      return result;
    }

    /// <summary>
    /// Изменить или создать новую учетную запись для сотрудника.
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <returns>Учетная запись.</returns>
    /// <remarks>Дублирует коробочную логику, но добавлено подробное логирование.</remarks>
    [Public]
    public override Sungero.CoreEntities.ILogin ChangeOrCreateMainEmployeesAccount(long employeeId)
    {
      var logPrefix = string.Format("EssPlatform.ModuleServerFunctions.ChangeOrCreateMainEmployeesAccount(). Ид сотрудника: {0}", employeeId);
      Logger.DebugFormat("{0}. Старт выполнения функции.", logPrefix);
      
      var employee = DirRX.OverrideSolution.Employees.GetAll(x => x.Id == employeeId).FirstOrDefault();
      if (employee == null)
      {
        var error = "Не найден сотрудник по ИД";
        Logger.ErrorFormat("{0}. Ошибка: {1}.", logPrefix, error);
        throw AppliedCodeException.Create(error);
      }
      
      var employeeTemplateAccountName = GetEmployeeAccountNameWithTemplate(employee.Person.Id);
      
      // Если у сотрудника уже есть УЗ, она действующая или сформирована по шаблону HR, то используем её.
      if (employee.Login != null && (employee.Login.Status == Sungero.CoreEntities.DatabookEntry.Status.Active ||
                                     employee.Login.LoginName == employeeTemplateAccountName))
      {
        Logger.DebugFormat("{0}. Для сотрудника будет использована его текущая УЗ. ИД УЗ: {1}. Логин: {2}.", logPrefix, employee.Login.Id, employee.Login.LoginName);
        ActivateExistingAccount(employee.Login);
        return employee.Login;
      }
      
      // Поиск шаблонной УЗ сотрудника.
      var templateAccount = GetExistingAccount(employeeTemplateAccountName);
      
      try
      {
        // Если УЗ, сформированой по шаблону HR нет в RX, то создаём новую.
        if (templateAccount == null)
        {
          Logger.DebugFormat("{0}. Для сотрудника будет создана новая шаблонная УЗ. Логин: {1}.", logPrefix, employeeTemplateAccountName);
          var newAccount = DirRX.EssPlatform.PublicFunctions.Module.CreateNewAccount(employeeTemplateAccountName);
          Logger.DebugFormat("{0}. Cоздана новая УЗ. ИД УЗ: {1}. Логин: {2}.", logPrefix,newAccount.Id, newAccount.LoginName);
          return newAccount;
        }
        
        // Поиск другого сотрудника, у которого указана такая же УЗ.
        var employeeWithSameLogin = Employees.GetAll(a => a.Login != null && a.Id != employee.Id && Logins.Equals(a.Login, templateAccount)).FirstOrDefault();
        
        // Если такого сотрудника нет в RX, то используем найденную шаблонную УЗ.
        if (employeeWithSameLogin == null)
        {
          Logger.DebugFormat("{0}. Для сотрудника будет использована существующая шаблонная УЗ. ИД УЗ: {1}. Логин: {2}.", logPrefix, templateAccount.Id, templateAccount.LoginName);
          ActivateExistingAccount(templateAccount);
          return templateAccount;
        }
        
        // Если нашли другого сотрудника, он действующий И обрабатываемый сотрудник не основной, то возвращаем null, перепривязывать УЗ к обрабатываемому сотруднику не требуется.
        if (employeeWithSameLogin.Status == DirRX.OverrideSolution.Employee.Status.Active && employee.EmploymentType != DirRX.OverrideSolution.Employee.EmploymentType.MainPlace)
        {
          Logger.DebugFormat("{0}. УЗ уже привязана к другому сотруднику. Перепривязка УЗ не требуется. ИД УЗ: {1}. Логин: {2}. ИД другого сотрудника: {3}. ИД персоны сотрудника: {4}. Вид занятости: {5}. Состояние: {6}",
                             logPrefix, templateAccount.Id, templateAccount.LoginName, employeeWithSameLogin.Id, employeeWithSameLogin.Person.Id,
                             DirRX.OverrideSolution.Employees.Info.Properties.EmploymentType.GetLocalizedValue(employeeWithSameLogin.EmploymentType),
                             DirRX.OverrideSolution.Employees.Info.Properties.Status.GetLocalizedValue(employeeWithSameLogin.Status));
          return Sungero.CoreEntities.Logins.Null;
        }

        // Если нашли не действующего сотрудника или обрабатываемый сотрудник основной, то отвязываем у другого сотрудника УЗ и используем эту УЗ для обрабатываемый сотрудника.
        Logger.DebugFormat("{0}. УЗ уже привязана к другому сотруднику. УЗ будет привязана к новому сотруднику. ИД УЗ: {1}. Логин: {2}. ИД другого сотрудника: {3}. ИД персоны сотрудника: {4}. Вид занятости: {5}. Состояние: {6}",
                           logPrefix, templateAccount.Id, templateAccount.LoginName, employeeWithSameLogin.Id, employeeWithSameLogin.Person.Id,
                           DirRX.OverrideSolution.Employees.Info.Properties.EmploymentType.GetLocalizedValue(employeeWithSameLogin.EmploymentType),
                           DirRX.OverrideSolution.Employees.Info.Properties.Status.GetLocalizedValue(employeeWithSameLogin.Status));
        employeeWithSameLogin.Login = Logins.Null;
        employeeWithSameLogin.Save();
        ActivateExistingAccount(templateAccount);
        return templateAccount;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Возникла ошибка при обновлении/создании УЗ KEDO в RX. Ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        throw ex;
      }
      finally
      {
        Logger.DebugFormat("{0}. Конец выполнения функции.", logPrefix);
      }
    }

    /// <summary>
    /// Активировать в RX УЗ сотрудника, созданный по шаблону КЭДО.
    /// </summary>
    /// <param name="templateAccount">Шаблонная УЗ.</param>
    /// <returns>Действующая шаблонная УЗ.</returns>
    public virtual Sungero.CoreEntities.ILogin ActivateExistingAccount(Sungero.CoreEntities.ILogin templateAccount)
    {
      if (templateAccount.Status != Sungero.CoreEntities.DatabookEntry.Status.Active)
      {
        Logger.DebugFormat("ActivateExistingAccount. ИД учётной записи: {0}. УЗ - закрытая. Она будет переведена в действующее состояние.", templateAccount.Id);
        templateAccount.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
        templateAccount.Save();
      }
      else
        Logger.DebugFormat("ActivateExistingAccount. ИД учётной записи: {0}. УЗ уже действующая.", templateAccount.Id);
      
      return templateAccount;
    }

    /// <summary>
    /// Заполнить учетную запись основному сотруднику и включить его в роль "Неинтерактивные пользователи".
    /// </summary>
    /// <param name="employeeId">ИД сотрудника.</param>
    /// <remarks>Копипаст коробочной функции, включение сотрудника в роль "Неинтерактивные пользователи" вынесено в АО.</remarks>
    [Public, Remote]
    public override void FillAccountAndIncludeNonInteractiveUsersRoles(long employeeId)
    {
      AccessRights.AllowRead(
        () =>
        {
          var newAccount = DirRX.EssPlatform.PublicFunctions.Module.ChangeOrCreateMainEmployeesAccount(employeeId);
          if (newAccount != null)
          {
            var employee = EssPlatformSolution.Employees.Get(employeeId);
            
            employee.Login = newAccount;
            employee.Save();

            // Добавить сотрудника в роль "Неинтерактивные пользователи".
            var NonInteractiveUsersRoles = Sungero.CoreEntities.Roles.GetAll(a => a.Sid == Sungero.Domain.Shared.SystemRoleSid.NonInteractiveUsers).FirstOrDefault();
            if (!employee.IncludedIn(NonInteractiveUsersRoles))
            {
              var asyncHandler = NNCommons.AsyncHandlers.AddEmployeeToRole.Create();
              asyncHandler.EmployeeId = employeeId;
              asyncHandler.RoleId = NonInteractiveUsersRoles.Id;
              asyncHandler.ExecuteAsync();
            }
          }
        });
    }

    /// <summary>
    /// Отправить дубль Push-уведомления в Супернику на почту сотрудника.
    /// </summary>
    /// <param name="email">Почта.</param>
    /// <param name="title">Тема письма.</param>
    /// <param name="text">Текст письма.</param>
    /// <returns>Ошибка. Если ошибки нет, то возвращается пустая строка.</returns>
    [Public]
    public string SendSupernikaPushOnEmail(string email, string title, string text)
    {
      var error = string.Empty;
      try
      {
        var message = Mail.CreateMailMessage();
        message.Subject = title;
        message.Body = text;
        message.IsBodyHtml = true;
        message.Priority = MailPriority.Normal;
        message.To.Add(email);
        Mail.Send(message);
        Logger.Debug("SendSupernikaPushOnEmail. Сообщение успешно отправлено на почту сотрудника.");
      }
      catch (Exception ex)
      {
        error = ex.Message;
        Logger.ErrorFormat("SendSupernikaPushOnEmail. Ошибка при отправки письма на почту. Текст ошибки: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
      }
      return error;
    }

    /// <summary>
    /// Получить текст из html разметки.
    /// </summary>
    /// <param name="html">Текст с html разметкой.</param>
    /// <returns>Текст без разметки.</returns>
    [Public]
    public static string ExtractTextFromHtml(string html)
    {
      if (string.IsNullOrEmpty(html))
        return html;

      // Сначала заменяем теги переносов на временные маркеры
      html = Regex.Replace(html, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
      html = Regex.Replace(html, "</p>", "\n", RegexOptions.IgnoreCase);
      html = Regex.Replace(html, "</div>", "\n", RegexOptions.IgnoreCase);
      html = Regex.Replace(html, "</tr>", "\n", RegexOptions.IgnoreCase);
      html = Regex.Replace(html, "</li>", "\n", RegexOptions.IgnoreCase);
      
      // Удаляем все остальные HTML-теги
      string textOnly = Regex.Replace(html, "<[^>]+>", " ");
      
      // Декодируем HTML-сущности
      textOnly = System.Net.WebUtility.HtmlDecode(textOnly);
      
      // Удаляем лишние пробелы (но сохраняем переносы)
      textOnly = Regex.Replace(textOnly, @"[ \t]+", " ");
      textOnly = Regex.Replace(textOnly, @"[ \t]+\n", "\n");
      textOnly = Regex.Replace(textOnly, @"\n[ \t]+", "\n");
      
      // Удаляем множественные переносы строк
      textOnly = Regex.Replace(textOnly, @"\n{3,}", "\n\n");
      
      return textOnly.Trim();
    }

    /// <summary>
    /// Получить содержание push-сообщения о задании/уведомлении.
    /// </summary>
    /// <param name="entityId">Ид задания/уведомления.</param>
    /// <param name="isNotice">Признак того, что отправляется уведомление.</param>
    /// <returns>Структура, с содержанием push-сообщения.</returns>
    [Public]
    public Structures.Module.ISupernikaNotifiactionParams GetSupernikaPushContent(long entityId, bool isNotice)
    {
      Structures.Module.ISupernikaNotifiactionParams notifiactionParams;
      if (isNotice)
      {
        var notice = Sungero.Workflow.Notices.GetAll(x => x.Id == entityId).FirstOrDefault();
        notifiactionParams = Functions.Module.GetSupernikaNotificatioParams(notice);
        notifiactionParams.Subject = FormSupernikaSubject(notifiactionParams.Subject, isNotice);
        notifiactionParams.Actions = PublicFunctions.Module.GetNotificationActionsForMessageBroker(notice);
        return notifiactionParams;
      }
      else
      {
        var assignment = Sungero.Workflow.Assignments.GetAll(x => x.Id == entityId).FirstOrDefault();
        notifiactionParams = Functions.Module.GetSupernikaNotificatioParams(assignment);
        notifiactionParams.Subject = FormSupernikaSubject(notifiactionParams.Subject, isNotice);
        notifiactionParams.Actions = PublicFunctions.Module.GetNotificationActionsForMessageBroker(assignment);
        return notifiactionParams;
      }
    }

    /// <summary>
    /// Сформировать тему уведомления/задания в Супернику.
    /// </summary>
    /// <param name="subject">Тема push-сообщения в Супернику из свойства задания/уведомления.</param>
    /// <param name="subject">Тема по умолчанию.</param>
    /// <returns>Итоговая тема push-сообщения.</returns>
    [Public]
    public string FormSupernikaSubject(string subject, bool isNotice)
    {
      if (string.IsNullOrEmpty(subject))
      {
        var constGuid = isNotice ? NNIntegration.PublicConstants.Module.ConstantsSetting.SupernikaPushNotificationsNoticeName.Guid : NNIntegration.PublicConstants.Module.ConstantsSetting.SupernikaPushNotificationsAssignmentName.Guid;
        var constName = isNotice ? NNIntegration.PublicConstants.Module.ConstantsSetting.SupernikaPushNotificationsNoticeName.Name : NNIntegration.PublicConstants.Module.ConstantsSetting.SupernikaPushNotificationsAssignmentName.Name;
        subject = NNCommons.PublicFunctions.Module.GetStringValue(constGuid);
        if (string.IsNullOrEmpty(subject))
        {
          Logger.DebugFormat("FormSupernikaAssignmentTitle. Не заполнена прикладная константа: {0}. Будет использовано значение по умолчанию.", constName);
          subject = isNotice ? NNIntegration.PublicConstants.Module.ConstantsDefaultValues.SupernikaPushNotificationsNoticeName : NNIntegration.PublicConstants.Module.ConstantsDefaultValues.SupernikaPushNotificationsAssignmentName;;
        }
        else
          Logger.DebugFormat("FormSupernikaAssignmentTitle. Для темы будет использовано значение из прикладной константы {0}.", constName);
      }
      
      return subject;
    }

    /// <summary>
    /// Получить атрибуты уведомления в Супернику.
    /// </summary>
    /// <param name="notice">Уведомление.</param>
    /// <returns>Структура с атрибутами уведомления в Супернику.</returns>
    public Structures.Module.ISupernikaNotifiactionParams GetSupernikaNotificatioParams(Sungero.Domain.Shared.IEntity assignment)
    {
      var notificationParams = Structures.Module.SupernikaNotifiactionParams.Create();
      
      // Задание HR Pro.
      if (HRAssignments.Is(assignment))
      {
        var hrAssignment = HRAssignments.As(assignment);
        notificationParams.Subject = hrAssignment.SupernikaSubjectDirRX;
        notificationParams.Text = hrAssignment.SupernikaTextDirRX;
        notificationParams.Created = hrAssignment.Created;
      }
      // Согласование документов.
      else if (DocumentApprovalAssignments.Is(assignment))
      {
        var documentApprovalAssignment = DocumentApprovalAssignments.As(assignment);
        notificationParams.Subject = documentApprovalAssignment.SupernikaSubjectDirRX;
        notificationParams.Text = documentApprovalAssignment.SupernikaTextDirRX;
        notificationParams.NeedEmailNotification = documentApprovalAssignment.NeedEmailNotificationDirRX == true;
        notificationParams.Created = documentApprovalAssignment.Created;
      }
      // Доработка документов.
      else if (DocumentReworkAssignments.Is(assignment))
      {
        var documentReworkAssignment = DocumentReworkAssignments.As(assignment);
        notificationParams.Subject = documentReworkAssignment.SupernikaSubjectDirRX;
        notificationParams.Text = documentReworkAssignment.SupernikaTextDirRX;
        notificationParams.Created = documentReworkAssignment.Created;
      }
      // Задание на доработку заявления
      else if (NNCommons.StatementReworkAssignments.Is(assignment))
      {
        var statementReworkAssignment = NNCommons.StatementReworkAssignments.As(assignment);
        notificationParams.Subject = statementReworkAssignment.SupernikaSubjectDirRX;
        notificationParams.Text = statementReworkAssignment.SupernikaTextDirRX;
        notificationParams.Created = statementReworkAssignment.Created;
      }
      // Подписание утверждающей подписью.
      else if (ESignAssignments.Is(assignment))
      {
        var eESignAssignment = ESignAssignments.As(assignment);
        notificationParams.Subject = eESignAssignment.SupernikaSubjectDirRX;
        notificationParams.Text = eESignAssignment.SupernikaTextDirRX;
        notificationParams.Created = eESignAssignment.Created;
      }
      // Задание на подтверждение дат отпуска.
      else if (NNVacations.VacationConfirmationAssignments.Is(assignment))
      {
        var confirmationAssignment = NNVacations.VacationConfirmationAssignments.As(assignment);
        notificationParams.Subject = confirmationAssignment.SupernikaSubject;
        notificationParams.Text = confirmationAssignment.SupernikaText;
        notificationParams.Created = confirmationAssignment.Created;
      }
      // Уведомление HR Pro.
      else if (HRNotices.Is(assignment))
      {
        var hrNotice = HRNotices.As(assignment);
        notificationParams.Subject = hrNotice.SupernikaSubjectDirRX;
        notificationParams.Text = hrNotice.SupernikaTextDirRX;
        notificationParams.NeedEmailNotification = hrNotice.NeedEmailNotificationDirRX == true;
        notificationParams.Created = hrNotice.Created;
      }

      // Если необходимо добавить дату и время формирования сообщения, тогда добавим в самом конце теста.
      var isTimestampRequiredConstant = NNCommons.PublicFunctions.Module.Remote.GetConstant(DirRX.NNIntegration.PublicConstants.Module.ConstantsSetting.SupernikaPushNotificationsIsTimestampRequired.Guid);
      
      if(isTimestampRequiredConstant != null && isTimestampRequiredConstant.BoolValue == true)
        notificationParams.Text = string.Format("{0}{1}<br>Сформировано: {2:dd.MM.yyyy HH:mm}", notificationParams.Text, Environment.NewLine, notificationParams.Created);
      
      return notificationParams;
    }

    /// <summary>
    /// Отправить сообщение/уведомление в Супернику через Message broker.
    /// </summary>
    /// <param name="assignmentId">Ид задания/уведомления.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="title">Заголовок сообщения.</param>
    /// <param name="content">Контент сообщения.</param>
    /// <param name="priority">Приоритет.</param>
    /// <param name="properties">Свойства.</param>
    /// <param name="actions">Действия в уведомлениях.</param>
    /// <param name="attachments">Вложения.</param>
    /// <returns>Guid push-уведомления.</returns>
    /// <remarks>Если не настроено подключение, то возвращается пустая строка. Если не удалось выполнить отправку, то пробрасывается ошибка для запуска повторной отправки в АО.</remarks>
    [Public]
    public string SendNotificationToSupernika(long assignmentId, IEmployee employee, string title, string content, int priority,
                                              System.Collections.Generic.Dictionary<string, string> properties,
                                              List<DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.ISupernikaAction> actions,
                                              System.Collections.Generic.Dictionary<string, long> attachments)
    {
      // Если отключена настройка "Подключить сервис идентификации", то отправка СМС выполняться не будет.
      if (!DirRX.EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
        return string.Empty;
      
      // Создать сообщение для сотрудника.
      var message = CreateEmptyEssNotification(employee);
      
      message.Title = title;
      message.Content = content;
      message.Priority = priority;
      
      if (properties != null)
        foreach(var p in properties)
          message.Properties.Add(p.Key, p.Value);
      
      // Действия.
      message.Actions = actions;

      if (attachments != null)
        foreach(var a in attachments)
          message.Attachments.Add(DirRX.EssPlatform.PublicFunctions.Module.CreateAttachmentNotification(a.Key, a.Value));
      
      // Сериализация сообщения.
      var messageJson = JsonConvert.SerializeObject(message);
      
      try
      {
        var token = GetAnAuthenticationTokenMessageBroker();
        var messageBrokerUrl = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings().MessageBrokerAddress;
        var requestApi = NNIntegration.PublicConstants.Module.MessageBrokerNotificationParameters.RequestApi;
        var requestResult = RunPostRequest(messageBrokerUrl, requestApi, messageJson, token);
        var statusCode = requestResult.StatusCode;
        if (statusCode == (int)HttpStatusCode.OK)
        {
          Logger.DebugFormat("SendNewNotification. Ид задания: {0}. Сообщение успешно отправлено в Message broker. Ответ: {1}.", assignmentId, requestResult.RequestMessage);
          return requestResult.RequestMessage;
        }
        else
        {
          // Ответы, отличные от 200 считать ошибками и пробрасывать дальше.
          Logger.ErrorFormat("SendNewNotification. Ид задания: {0}. Ошибка при отправке сообщения в Message broker. StatusCode: {1}. RequestMessage: {2}.",
                             assignmentId, statusCode, requestResult.RequestMessage);
          throw new Exception(DirRX.EssPlatform.Resources.SendSMSErrorFormat(statusCode, requestResult.RequestMessage));
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("SendNewNotification. Ид задания: {0}. Ошибка при отправке сообщения в Message broker. Текст ошибки: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
        throw ex;
      }
    }

    /// <summary>
    /// Подготовка структуры с описанием сообщения-колокольчика в ЛК.
    /// </summary>
    /// <param name="employee">Сотрудник, которому надо отправить сообщение.</param>
    /// <returns>Структура с описанием сообщения.</returns>
    [Public]
    public virtual DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.IMessageBrokerNotification CreateEmptyEssNotification(IEmployee employee)
    {
      try
      {
        // Структура для уведомления.
        var result = DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.MessageBrokerNotification.Create();
        result.Properties = new Dictionary<string, string>();
        result.Attachments = new List<DirRX.EssPlatform.Structures.Module.IMessageBrokerNotificationAttachment>();
        
        // Сотрудник, которому отправляется сообщение.
        var identity = DirRX.EssPlatform.Structures.Module.MessageBrokerNotificationIdentity.Create();
        var phoneNumber = DirRX.EssPlatform.PublicFunctions.Module.GetSignificantSymbolsFromPhoneNumber(employee.PersonalPhoneDirRX);
        identity.CredentialValue = phoneNumber;
        identity.CredentialType = Constants.Module.MessageBrokerRequestConstants.CredentialType;
        result.Identity = identity;
        
        // Дефолтные параметры доставки.
        result.DeliveryMethod = NNIntegration.PublicConstants.Module.MessageBrokerNotificationParameters.DeliveryMethod.Push;
        result.Priority = NNIntegration.PublicConstants.Module.MessageBrokerNotificationParameters.Priority.High;
        result.DeliveryProxy =  Constants.Module.MessageBrokerRequestConstants.DeliveryProxy;
        var expirationPeriod = new TimeSpan(0, 5, 0);
        result.ExpirationPeriod = string.Format("{0}:{1}:{2}", expirationPeriod.Hours, expirationPeriod.Minutes, expirationPeriod.Seconds);

        return result;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.Message);
        throw ex;
      }
    }

    /// <summary>
    /// Сформировать структуру с действиями уведомлений Суперники для отправки в MessageBroker.
    /// </summary>
    /// <param name="assignment">Задание/уведомление.</param>
    /// <returns>Список действий.</returns>
    /// <remarks>Передаётся как IEntity, т.к. на прикладном уровне нет общего наследника для заданий и уведомлений.</remarks>
    [Public]
    public List<Structures.Module.ISupernikaAction> GetNotificationActionsForMessageBroker(Sungero.Domain.Shared.IEntity assignment)
    {
      var actions = NNIntegration.PublicFunctions.SupernikaPushNotificationAction.GetActions(assignment);
      var actionList = new List<DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.ISupernikaAction>();
      var number = 0;
      foreach (var action in actions)
      {
        number ++;
        var actionStruct = DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.SupernikaAction.Create();
        actionStruct.Name = NNIntegration.PublicConstants.Module.MessageBrokerNotificationParameters.ActionParams.Button;
        var label = NNIntegration.PublicConstants.Module.MessageBrokerNotificationParameters.ActionParams.Label;
        actionStruct.Title = action.Name;
        actionStruct.Parameters = DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.SupernikaActionParameter.Create(
          action.NeedComment == true,
          action.ActionType);
        
        actionList.Add(actionStruct);
      }
      return actionList;
    }

    /// <summary>
    /// Сформировать ссылку на объект для приложения суперника.
    /// </summary>
    /// <param name="targetSystem">Имя целевой системы.</param>
    /// <param name="selfOfficeObjectType">Рабочий элменет/Услуга/Группа услуг.</param>
    /// <param name="objectType">Имя типа карточки.</param>
    /// <param name="id">Идентификатор.</param>
    /// <returns>Ссылка на сущность в личном кабинете.</returns>
    [Public]
    public virtual string CreateHyperLinkToSupernika(string targetSystemName, string selfOfficeObjectType, string objectTypeName, string id)
    {
      var selfOfficeUrl = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings().ESSAddress.TrimEnd('/');
      var resultUrl = base.CreateHyperLinkToSelfOffice(targetSystemName, selfOfficeObjectType, objectTypeName, id);
      var textForHyperlinkTemplate = NNCommons.PublicFunctions.Module.GetStringValue(NNIntegration.PublicConstants.Module.ConstantsSetting.TextForHyperlink.Guid);
      var homescreenExpressTemplate = NNCommons.PublicFunctions.Module.GetStringValue(NNIntegration.PublicConstants.Module.ConstantsSetting.HomescreenExpress.Guid);
      
      if (!string.IsNullOrEmpty(resultUrl))
      {
        resultUrl = resultUrl.Replace(selfOfficeUrl, "");
        resultUrl = resultUrl.TrimEnd('/');
        resultUrl = string.Format("{0}{1}", resultUrl, NNIntegration.PublicConstants.Module.ViewMobile.Replace('&', '?'));
      }
      
      return string.Format("[{0};{1};{2}]", textForHyperlinkTemplate, homescreenExpressTemplate, resultUrl);
    }

    /// <summary>
    /// Отправить сообщение сотруднику о новом задании/уведомлении.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="isNotice">True - уведомление, false - задание.</param>
    /// <param name="targetSystemName">Имя целевой системы.</param>
    /// <param name="selfOfficeObjectType">Тип объекта Личного кабинета.</param>
    /// <param name="objectTypeName">Объект личного кабинета сотрудника.</param>
    /// <param name="id">ИД объекта.</param>
    /// <param name="isSendToMobile">Может отправляться по СМС или Viber.</param>
    /// <remarks>Копипаста коробочного метода, изменена ссылка для уведомления по СМС.</remarks>
    [Public]
    public override void SendNewNotification(IUser employee, bool isNotice, string targetSystemName, string selfOfficeObjectType, string objectTypeName, string id, bool isSendToMobile)
    {
      if (employee == null)
      {
        Logger.DebugFormat("SendNewNotification. Не заполнен пользователь.");
        return;
      }
      
      Logger.DebugFormat("SendNewNotification. Пользователь: {0}. Начало выполнения функции. Id сущности: {1}, isSendToMobile: {2}.", employee.Id, id, isSendToMobile);
      
      if (Sungero.Company.Employees.Is(employee))
      {
        var employeeO = EssPlatformSolution.Employees.As(employee);
        
        // Определить способы оповещения. Если необходимо наследовать от НОР - взять настройки из НОР, иначе из Сотрудника.
        var sendSms = false;
        var sendEmail = false;
        var sendMessageToViber = false;
        var connectingESS = employeeO.PersonalAccountStatusDirRX.HasValue && employeeO.PersonalAccountStatusDirRX.Value == EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted;
        if (employeeO.InheritFromBusinessUnitDirRX.HasValue && employeeO.InheritFromBusinessUnitDirRX.Value)
        {
          if (employeeO.BusinessUnit != null)
          {
            var businessUnitO = EssPlatformSolution.BusinessUnits.As(employeeO.BusinessUnit);
            sendSms = businessUnitO.NeedNotifyNewHRAssignmentDirRXSungero.HasValue &&
              businessUnitO.NeedNotifyNewHRAssignmentDirRXSungero.Value == EssPlatformSolution.BusinessUnit.NeedNotifyNewHRAssignmentDirRXSungero.SMS && connectingESS;
            sendEmail = businessUnitO.NeedNotifyNewHRAssignmentDirRXSungero.HasValue &&
              (businessUnitO.NeedNotifyNewHRAssignmentDirRXSungero.Value == EssPlatformSolution.BusinessUnit.NeedNotifyNewHRAssignmentDirRXSungero.Email ||
               businessUnitO.NeedNotifyNewHRAssignmentDirRXSungero.Value == EssPlatformSolution.BusinessUnit.NeedNotifyNewHRAssignmentDirRXSungero.EmailPersonal);
            sendMessageToViber = businessUnitO.NeedNotifyNewHRAssignmentDirRXSungero.HasValue &&
              businessUnitO.NeedNotifyNewHRAssignmentDirRXSungero.Value == EssPlatformSolution.BusinessUnit.NeedNotifyNewHRAssignmentDirRXSungero.Viber && connectingESS;
          }
        }
        else
        {
          sendSms = employeeO.NeedNotifyNewHRAssignmentDirRX.HasValue &&
            employeeO.NeedNotifyNewHRAssignmentDirRX.Value == EssPlatformSolution.Employee.NeedNotifyNewHRAssignmentDirRX.SMS && connectingESS;
          sendEmail = employeeO.NeedNotifyNewHRAssignmentDirRX.HasValue &&
            (employeeO.NeedNotifyNewHRAssignmentDirRX.Value == EssPlatformSolution.Employee.NeedNotifyNewHRAssignmentDirRX.Email ||
             employeeO.NeedNotifyNewHRAssignmentDirRX.Value == EssPlatformSolution.Employee.NeedNotifyNewHRAssignmentDirRX.EmailPersonal);
          sendMessageToViber = employeeO.NeedNotifyNewHRAssignmentDirRX.HasValue &&
            employeeO.NeedNotifyNewHRAssignmentDirRX.Value == EssPlatformSolution.Employee.NeedNotifyNewHRAssignmentDirRX.Viber && connectingESS;
        }
        
        // Если необходимо отправить смс, тогда вызываем АО для отправки уведомления в Супернику (ПМД).
        if (sendSms && !string.IsNullOrEmpty(objectTypeName) && isSendToMobile)
        {
          Logger.DebugFormat("SendNewNotification. Пользователь: {0}. Старт АО для поставноки сообщения в очередь.", employee.Id);
          
          var asyncHandler = EssPlatform.AsyncHandlers.CreateNotificationToSupernikaDirRX.Create();
          asyncHandler.EmployeeID = employee.Id;
          asyncHandler.IsNotice = isNotice;
          asyncHandler.TargetSystemName = targetSystemName;
          asyncHandler.SelfOfficeObjectType = selfOfficeObjectType;
          asyncHandler.ObjectTypeName = objectTypeName;
          asyncHandler.Id = long.Parse(id);
          asyncHandler.IsSendToMobile = isSendToMobile;
          asyncHandler.ExecuteAsync();
        }
        else
        {
          Logger.DebugFormat("SendNewNotification. Пользователь: {0}. Вызов базового обработчика по отправки сообщения/уведомления.", employee.Id);
          base.SendNewNotification(employee, isNotice, targetSystemName, selfOfficeObjectType, objectTypeName, id, isSendToMobile);
        }
      }
      Logger.DebugFormat("SendNewNotification. Пользователь: {0}. Окончание выполнения функции.", employee.Id);
    }

    /// <summary>
    /// Сформировать ссылку на объект в личном кабинете пользователя.
    /// </summary>
    /// <param name="targetSystem">Имя целевой системы.</param>
    /// <param name="selfOfficeObjectType">Рабочий элемент/Услуга/Группа услуг.</param>
    /// <param name="objectType">Имя типа карточки.</param>
    /// <param name="id">Идентификатор.</param>
    /// <returns>Ссылка на сущность в личном кабинете.</returns>
    /// <remarks>Ссылка формируется для открытия в Супернике. Шаблон гиперссылки задается в константе HyperLinkTemplateToSmartAppKedo.</remarks>
    [Public]
    public override string CreateHyperLinkToSelfOffice(string targetSystemName, string selfOfficeObjectType, string objectTypeName, string id)
    {
      var resultUrl = base.CreateHyperLinkToSelfOffice(targetSystemName, selfOfficeObjectType, objectTypeName, id);
      var hyperLinkTemplate = NNCommons.PublicFunctions.Module.GetStringValue(NNIntegration.PublicConstants.Module.ConstantsSetting.HyperLinkTemplateToSmartAppKedo.Guid);
      if (!string.IsNullOrEmpty(hyperLinkTemplate))
      {
        // Получить относительный адрес.
        var selfOfficeUrl = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings().ESSAddress;
        resultUrl = resultUrl.Replace(selfOfficeUrl, "");
        resultUrl = resultUrl.StartsWith("/") ? resultUrl : "/" + resultUrl;
        
        resultUrl = System.Net.WebUtility.UrlEncode(resultUrl + NNIntegration.PublicConstants.Module.ViewMobile);
        resultUrl = string.Format(hyperLinkTemplate, resultUrl);
      }
      return resultUrl;
    }

    /// <summary>
    /// Получить список сертификатов по персоне сотрудника.
    /// </summary>
    /// <param name="employeeId">Cотрудник.</param>
    /// <returns>Список сертификатов по персоне сотрудника.</returns>
    [Public]
    public override IQueryable<ICertificate> GetEmployeeCertificateEnities(Sungero.Company.IEmployee employee)
    {
      var certificates = base.GetEmployeeCertificateEnities(employee);
      var pluginId = DirRX.SignPlatform.PublicConstants.Module.PluginIdOfCloudCertificates;
      
      return certificates.Where(x => x.PluginId == pluginId);
    }

    /// <summary>
    /// Вложить в задание/задачу документы.
    /// </summary>
    /// <param name="entityType">Тип сущности (задача/задание).</param>
    /// <param name="entityId">ИД сущности.</param>
    /// <param name="documents">Список приложений в формате json, в которой есть ИД и имя группы, в которую надо вкладывать.</param>
    /// <remarks>Пример структуры вложений: "documents": "[{\"EntityId\":\"260\",\"Group\":\"AddendaGroup\"}]".
    /// Сейчас функция добавляет вложение в группу не смотря на то, какой тип указан в этой группе.</remarks>
    [Remote,Public(WebApiRequestType = RequestType.Post)]
    public override void AttachDocuments(string entityType, long entityId, string documents)
    {
      if (!string.IsNullOrEmpty(entityType))
      {
        Sungero.Domain.Shared.IEntity entity = null;
        if (entityType.ToLower().Contains("task"))
          entity = Sungero.Workflow.Tasks.GetAll(t => t.Id == entityId).FirstOrDefault();
        else if (entityType.ToLower().Contains("assignment"))
        {
          entity = Sungero.Workflow.Assignments.GetAll(a => a.Id == entityId).FirstOrDefault();
        }
        
        if (entity != null)
        {
          var documentsList = JsonConvert.DeserializeObject<List<DirRX.EssPlatform.Structures.Module.AttachDocumentsInfo>>(documents);
          
          foreach (var document in documentsList)
          {
            var attachment = Sungero.Content.ElectronicDocuments.GetAll(x => x.Id == long.Parse(document.EntityId)).FirstOrDefault();
            // HACK: обходное решение для добавления документов в SimpleTask.
            if (!string.IsNullOrEmpty(document.Group))
            {
              var attachmentGroupePropertyInfo = entity.GetType().GetProperties().FirstOrDefault(p => p.Name == document.Group);
              var attachmentGroupeValue = attachmentGroupePropertyInfo?.GetValue(entity);
              var attachmentsTypePropertyInfo = attachmentGroupeValue?.GetType().GetProperty("All");
              var attachmentsTypeValue = attachmentsTypePropertyInfo?.GetValue(attachmentGroupeValue);
              var addAttachmentMethod = attachmentsTypePropertyInfo?.PropertyType.GetMethod("Add", new[] { attachment.GetType() });
              addAttachmentMethod?.Invoke(attachmentsTypeValue, new object[] { attachment });
            }
            else
            {
              if (Sungero.Workflow.Tasks.Is(entity))
                Sungero.Workflow.Tasks.As(entity).Attachments.Add(attachment);
              else if (Sungero.Workflow.Assignments.Is(entity))
                Sungero.Workflow.Assignments.As(entity).Attachments.Add(attachment);
            }
          }
          entity.Save();
        }
      }
    }

    /// <summary>
    /// Заполнить данные пользователя в json-строку.
    /// </summary>
    /// <param name="person">Персона.</param>
    /// <param name="user">Пользователь.</param>
    /// <param name="phone">Номер телефона.</param>
    /// <returns>Сериализованная строка.</returns>
    /// <remarks>Измененная коробочная функция HRPro 2.7 для доступа в ЛК по токену.</remarks>
    [Remote, Public]
    public override string CreateUserStructure(Sungero.Parties.IPerson person, IUser user, string phone)
    {
      var personUid = GetUidPerson(person);
      
      var personData = DirRX.EssPlatform.Structures.Module.PersonData.Create();
      personData.givenName = person.FirstName;
      personData.patronym = person.MiddleName;
      personData.phone = phone;
      personData.surname = person.LastName;
      
      var userLogin = GetUserLogin(person.Id, user);
      
      // Норникель. Не передаём СНИЛС.
      var identites = Structures.Module.Claim.Create();
      identites.DirectumRX_PersonId = person.Id;
      identites.DirectumRX_Username = userLogin;
      identites.phone = phone;
      
      var authentication = DirRX.EssPlatform.Structures.Module.Authentication.Create();
      // Убираем передачу типа аутентификации, так как пользователь теперь всегда будет создаваться без аутентификации.
      authentication.provider = string.Empty;
      
      var userEssJson = Structures.Module.EssUserActivateJson.Create();
      userEssJson.userName = personUid;
      userEssJson.person = personData;
      userEssJson.identities = identites;
      userEssJson.authentication = authentication;
      userEssJson.inviteToResource = DirRX.EssPlatform.Constants.Module.Audiences.EssSite;
      
      // Норникель. Не передаём СНИЛС.
      return SerializedToJson(userEssJson)
        .Replace("DirectumRX_PersonId", "DirectumRX/PersonId")
        .Replace("DirectumRX_Username", "DirectumRX/Username");
    }

    /// <summary>
    /// Отключить пользователя личного кабинета.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <remarks>Убрана отправка СМС из коробочной реализации HRPro 2.7.</remarks>
    [Public, Remote]
    public override void DisconnectEssUser(EssPlatformSolution.IEmployee employee)
    {
      var personUid = GetUidPerson(employee.Person);
      Logger.DebugFormat("EssPlatform.ModuleServerFunctions.DisconnectESSUser(): user {0} disconnected started.", personUid);
      
      try
      {
        DeleteESSUser(employee);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.DisconnectESSUser(). An error occurred while diconnecting: {0}.", ex.Message);
        throw AppliedCodeException.Create(DirRX.EssPlatform.Resources.DisconnectEssUserError, ex);
      }
      
      Logger.DebugFormat("EssPlatform.ModuleServerFunctions.DisconnectESSUser(): user {0} disconnected.", personUid);
      
      // HACK: Seleznyov_DV. Заполнить необходимые после отключения чекбоксы у сотрудника (коробочныя логика).
      OverrideSolution.Functions.Employee.FillParametersAfterDissconect(OverrideSolution.Employees.As(employee));
      
      DirRX.EssPlatform.PublicFunctions.Module.ChangeEmployeeStatusConnectionToEss(employee.Id, EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent);
    }

    /// <summary>
    /// Обновить пользователя личного кабинета.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <remarks>- Если необходимо поменять номер, но сотрудник не включен в НОР или НОР не подключена к ЛК, СМС отправлены не будут, информация о пользователе не обновится.
    /// - Если было необходимо поменять номер, но процедура отправки СМС прошла неуспешно, информация о пользователе не обновится.</remarks>
    /// <remarks>Убрана отправка СМС коробочная реализация HRPro 2.7.</remarks>
    [Public]
    public override void UpdateEssUser(EssPlatformSolution.IEmployee employee, string oldPhoneNumber)
    {
      var uid = GetUidPerson(employee.Person);
      Logger.DebugFormat("EssPlatform.ModuleServerFunctions.UpdateEssUser: user {0} update started.", uid);
      try
      {
        DirRX.EssPlatform.PublicFunctions.Module.Remote.PatchEssUser(employee);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.UpdateEssUser: could not update EssUser. Employee id: {0}. Error: {1}.", employee.Id, ex.Message);
        throw AppliedCodeException.Create(DirRX.EssPlatform.Resources.UpdateEssUserError);
      }
    }

    /// <summary>
    /// Отправить запрос на активацию пользователя.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <param name="person">Персона.</param>
    /// <param name="phone">Номер телефона.</param>
    /// <param name="businessUnit">Организация.</param>
    /// <remarks>Убрана отправка СМС из коробочной реализации HRPro 2.7.</remarks>
    [Public, Remote]
    public override void ActivateESSUser(IUser user, Sungero.Parties.IPerson person, string phone, Sungero.Company.IBusinessUnit businessUnit)
    {
      try
      {
        var personUid = GetUidPerson(person);
        
        Logger.DebugFormat("EssPlatform.ModuleServerFunctions.ActivateESSUser(). Activate Ess user {0}.", personUid);
        
        var baseUri = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings().EssServiceAddress;
        var token = GetAnAuthenticationToken(DirRX.EssPlatform.Constants.Module.Audiences.EssService);
        // Если пользователя с таким логином не нашли (не было или безвозвратно отключили) - создать его с передачей данных, иначе активировать существующего.
        var jsonString = string.Empty;
        var requestUri = string.Empty;
        if (ExistsIdsUser(personUid))
          requestUri = string.Format(DirRX.EssPlatform.Constants.Module.RequestApiIdentity.ActivateDisabledEssUser, personUid);
        else
        {
          requestUri = DirRX.EssPlatform.Constants.Module.RequestApiIdentity.ActivateUser;
          
          jsonString = DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.Remote.CreateUserStructure(person, user, phone);
        }
        
        // Отправить запрос на активацию пользователя.
        var result = RunPostRequest(baseUri, requestUri, jsonString, token);
        // При первичном создании возвращается код 200.
        if (result.StatusCode == 200)
        {
          Logger.Debug("EssPlatform.ModuleServerFunctions.ActivateESSUser(). User created.");
          return;
        }
        // При активации ранее отключенного - код 204.
        else if (result.StatusCode == 204)
        {
          Logger.Debug("EssPlatform.ModuleServerFunctions.ActivateESSUser(). User activated.");
          ModifyEssUserAccount(
            person,
            DirRX.EssPlatform.Constants.Module.UserIdentityAttribute.Inila,
            person.INILA);
          return;
        }
        else
          throw new Exception(DirRX.EssPlatform.Resources.ActivateESSUserExceptionFormat(result.StatusCode, result.RequestMessage));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("EssPlatform.ModuleServerFunctions.ActivateESSUser(). An error occurred while sending the request: {0}.", ex.Message);
        throw new Exception(ex.Message);
      }
    }

  }
}