using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Employee;
using DirRX.NNIntegration.Structures.Module;
using DirRX.NNCommons;

namespace DirRX.OverrideSolution.Server
{
  partial class EmployeeFunctions
  {
    /// <summary>
    /// Определяет категорию РСС.
    /// </summary>
    /// <returns>true, если относится к РСС, иначе false.</returns>
    [Public]
    public bool IsCategoryRSS()
    {
      return _obj.CategoryDirRX == OverrideSolution.Employee.CategoryDirRX.Manager ||
        _obj.CategoryDirRX == OverrideSolution.Employee.CategoryDirRX.Specialist ||
        _obj.CategoryDirRX == OverrideSolution.Employee.CategoryDirRX.Clerk;
    }
    
    /// <summary>
    /// Добавить данные сотрудника к превью карточки сотрудника.
    /// </summary>
    /// <param name="digest">Превью сотрудника.</param>
    public override void AddEmployeeData(Sungero.Core.IUserDigestModel digest)
    {
      base.AddEmployeeData(digest);
      
      if (!string.IsNullOrEmpty(_obj.PersonnelNumber))
        digest.AddLabel(DirRX.OverrideSolution.Employees.Resources.PersonnelNumberDigestFormat(_obj.PersonnelNumber));
    }

    /// <summary>
    /// Определить необходимость отправлять сообщение пользователю в рабочее время.
    /// </summary>
    /// <returns>True - необходимо отправлять уведомление только в рабочее время.</returns>
    [Public]
    public bool IsWorkHoursNotification()
    {
      var result = false;
      
      // Если у пользователя не заданы индивидуальные настройки, значит нужно использовать настройки Нашей организции.
      if(_obj.InheritFromBusinessUnitDirRX == true)
      {
        if(_obj.BusinessUnit != null)
          result = OverrideSolution.BusinessUnits.As(_obj.BusinessUnit).SendPushDuringWorkHoursDirRX == true;
      }
      else
      {
        result = _obj.SendPushDuringWorkHoursDirRX == true;
      }
      
      return result;
    }
    
    /// <summary>
    /// Получить документы по сотруднику.
    /// </summary>
    /// <returns>Документы по сотруднику.</returns>
    [Public, Remote]
    public IQueryable<Sungero.Docflow.IOfficialDocument> GetEmployeeDocuments()
    {
      var linkedDocuments = HRPro.HRDocFlow.EmployeeDocumentLinks.GetAll(l => Equals(l.Employee, _obj)).Select(d => d.Document);
      return linkedDocuments;
    }
    
    #region Синхронизация данных между сотрудниками одной персоны.

    /// <summary>
    /// Вычислить необходимость синхронизации сотрудников персоны.
    /// </summary>
    /// <remarks>В функции релизована логика заполнения параметра IsNeedUpdateEmployees, который запускает АО в событии После сохранения для синхронизации сотрудников персоны.</remarks>
    public virtual void NeedUpdatePersonEmployees()
    {
      // Если карточка не заблокирована АО,
      // и одно из свойств (Приглашение в ЛК, телефоны, email, каналы доставки) изменилось, то заполнить параметр для обновления данных в других сотрудниках этой персоны.
      // Исключить тот случай, когда изменение поля Приглашение в ЛК идет в рамках закрытия записи.
      var entityParams = ((Sungero.Domain.Shared.IExtendedEntity)_obj).Params;
      var mainEmployee = EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(_obj.Person.Id, null);
      var isMainEmployee = _obj.Id == mainEmployee?.Id;
      var propertiesState = _obj.State.Properties;
      
      // HACK: Seleznyov_DV. Свойство LockedByAsync может быть изменено в событии До сохранения модуля EssPlatform, поэтому необходимо проверять первоначальное значение.
      var isLocked = _obj.State.Properties.LockedByAsync.OriginalValue == true;
      
      // HACK: Seleznyov_DV. Дублирует коробочную логику HR в событии До сохранения модуля EssPlatform.
      var propertiesIsChanged = (propertiesState.PersonalAccountStatusDirRX.IsChanged &&
                                 !propertiesState.Status.IsChanged &&
                                 _obj.Status != Sungero.Company.Employee.Status.Closed) ||
        propertiesState.PersonalPhoneDirRX.IsChanged ||
        propertiesState.NeedNotifyNewHRAssignmentDirRX.IsChanged || propertiesState.NeedNotifyExpiredHRAssignmentDirRX.IsChanged ||
        propertiesState.NeedNotifyHRRepeatDirRX.IsChanged || propertiesState.MessagesEmailDirRX.IsChanged || propertiesState.ConfirmationTypeDirRX.IsChanged ||
        propertiesState.InheritFromBusinessUnitDirRX.IsChanged;
      
      // HACK: Seleznyov_DV. Коробочная логика HR + доп. условие, что если сотрудник - совместитель и у него изменился телефон, то нужно запустить синхронизацию.
      var isNeedUpdateEmployees = !isLocked && mainEmployee != null &&
        ((isMainEmployee && propertiesIsChanged) ||
         (!isMainEmployee && propertiesState.PersonalPhoneDirRX.IsChanged && _obj.PersonalPhoneDirRX != mainEmployee.PersonalPhoneDirRX));
      
      Logger.DebugFormat("NeedUpdatePersonEmployees. ИД сотрудника: {0}. Сотрудник с основным местом работы: {1}. Сотрудник заблокирован АО: {2}. Нужна синхронизация сотрудников: {3}.",
                         _obj.Id, isMainEmployee, isLocked, isNeedUpdateEmployees);
      
      _obj.LockedByAsync = isNeedUpdateEmployees && isMainEmployee;
      
      var key = DirRX.EssPlatformSolution.Employees.Resources.IsNeedUpdateEmployeesFormat(_obj.Id);
      if (entityParams.ContainsKey(key))
        entityParams[key] = isNeedUpdateEmployees;
      else
        entityParams.Add(key, isNeedUpdateEmployees);
    }
    
    #endregion
    
    #region Логика выпуска сертификата ЭП для сотрудников.
    
    #region Входные точки для выпуска/перевыпуска сертификатов.
    
    /// <summary>
    /// Перевыпустить сертификат ЭП для сотрудника.
    /// </summary>
    /// <remarks>Входная функция для перевыпуска сертификата.
    /// При перевыпуске прекращается существующая задача и завершается действие предыдущего сертификата сотрудника.
    /// Новый сертификат будет перевыпущен для основного сотрудника персоны.</remarks>
    [Public]
    public virtual void ReissueCertificate()
    {
      var logPrefix = "ReissueCertificate";
      
      DirRX.NNIntegration.PublicFunctions.Module.AbortActiveCertificateIssueTaskForPerson(_obj.Person.Id, logPrefix);
      DirRX.NNIntegration.PublicFunctions.Module.DisableCertificateForPerson(_obj.Person.Id, logPrefix);
      
      try
      {
        IssueCertificate();
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("{0}. Возникла ошибка при перевыпуске сертификата. Текст ошибки: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        throw ex;
      }
    }
    
    /// <summary>
    /// Выпустить сертификат сотруднику.
    /// </summary>
    /// <remarks>Входная функция для выпуска сертификата. Если сотрудник - не основной, то
    /// выпуск сертификата выполняется асинхронно.</remarks>
    [Public]
    public virtual void IssueCertificate()
    {
      Logger.DebugFormat("IssueCertificate. ИД сотрудника: {0}. Старт выпуска сертификата.", _obj.Id);
      
      // Поиск основного сотрудника персоны. Сертификат должен выпускаться для основного сотрудника персоны.
      var mainEmployee = Employees.As(DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(_obj.Person.Id, null));
      
      // Если текущий сотрудник - основной, то выпускаем ему сертификат.
      if (Employees.Equals(mainEmployee, _obj))
      {
        Logger.DebugFormat("IssueCertificate. ИД сотрудника: {0}. Сотрудник - основной сотрудник. Ему будет выпущен новый сертификат.", _obj.Id);
        IssueCertificateToMainEmployee(_obj.PersonalPhoneDirRX);
      }
      // Если текущий сотрудник - не основной, то запускаем асинхронный выпуск сертификата для основного сотрудника.
      else
      {
        Logger.DebugFormat("IssueCertificate. ИД сотрудника: {0}. Сотрудник - не основной сотрудник. Запуск асинхронного выпуска сертификата основному сотруднику с ИД: {1}.", _obj.Id, mainEmployee.Id);
        var asyncHandler = DirRX.NNIntegration.AsyncHandlers.IssueCertificateToMainEmployeeAsync.Create();
        asyncHandler.MainEmployeeId = mainEmployee.Id;
        asyncHandler.NewPhoneNumber = _obj.PersonalPhoneDirRX;
        asyncHandler.ExecuteAsync();
      }
    }
    
    #endregion
    
    #region Логика подбора фиктивного номера телефона для выпуска сертификата.
    
    /// <summary>
    /// Получить номер телефона для выпуска сертификата.
    /// </summary>
    /// <returns>Номер для выпуска сертификата.</returns>
    [Public]
    public virtual string GetCertificatePhoneNumber()
    {
      return string.IsNullOrEmpty(_obj.CertificatePhoneNumberDirRX) ? _obj.PersonalPhoneDirRX : _obj.CertificatePhoneNumberDirRX;
    }
    
    /// <summary>
    /// Проверить личный телефон сотрудника на совпадение с фиктивным номером у другого сотрудника.
    /// </summary>
    /// <remarks>Если личный телефон сотрудника совпал с фиктивным номером у другого сотрудника, то запаскается АО по смене фиктивного номера и перевыпуску сертификата для другого сотрудника.</remarks>
    [Public]
    public virtual void CheckEmployeesCertificatePhone()
    {
      var employeeWithSamePhone = OverrideSolution.PublicFunctions.Employee.GetEmployeeWithEqualCertificatePhone(_obj);
      if (employeeWithSamePhone != null)
      {
        
        Logger.DebugFormat("CheckEmployeesCertificatePhone. ИД сотрудника: {0}. Таб. номер: {1}. Найден сотрудник (ИД: {2}), у которого фиктивный номер совпадает с личным телефоном этого сотрудника. Для сотрудника с ИД {2} будет подобран новый фиктивный номер и перевыпущен сертификат.",
                           _obj.Id, _obj.PersonnelNumber, employeeWithSamePhone.Id);
        var mainEmployee = Employees.As(DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(employeeWithSamePhone.Person.Id, null)) ?? employeeWithSamePhone;
        var asyncHandler = DirRX.NNIntegration.AsyncHandlers.ReissueCertificateAsync.Create();
        asyncHandler.EmployeeId = mainEmployee.Id;
        asyncHandler.ExecuteAsync();
      }
    }
    
    /// <summary>
    /// Получить сотрудника, у которого номер для выпуска сертификата совпадает с личным телефоном данного сотрудника.
    /// </summary>
    /// <returns>Сотрудник. Если сотрудник не найден - пустое значение.</returns>
    [Public]
    public virtual IEmployee GetEmployeeWithEqualCertificatePhone()
    {
      return Employees.GetAll(x => x.PersonalPhoneDirRX != null &&
                              x.PersonalPhoneDirRX != string.Empty &&
                              x.CertificatePhoneNumberDirRX == _obj.PersonalPhoneDirRX)
        .FirstOrDefault();
    }
    
    /// <summary>
    /// Определить номер телефона сотрудника для выпуска сертификата.
    /// </summary>
    /// <returns>Номер для выпуска сертификата.</returns>
    [Public]
    public virtual void DefineCerificatePhoneNumber(string personalPhone)
    {
      var certificatePhone = _obj.CertificatePhoneNumberDirRX;

      if (string.IsNullOrEmpty(personalPhone))
      {
        Logger.DebugFormat("DefineCerificatePhoneNumber. ИД сотрудника: {0}. Не заполнен личный телефон у сотрудника.", _obj.Id);
        if (!string.IsNullOrEmpty(certificatePhone))
          _obj.CertificatePhoneNumberDirRX = string.Empty;
        
        return;
      }

      // Удалить спец. символы из телефона.
      var phone = System.Text.RegularExpressions.Regex.Replace(personalPhone, "[^0-9]", "");
      
      // Проверить номер телефона на соответствие маске личного телефона сотрудника.
      var isMatchesPersonalPhoneMask = NNIntegration.PublicFunctions.Module.MatchesPersonalPhoneMask(phone);

      if (isMatchesPersonalPhoneMask == null)
      {
        Logger.ErrorFormat("DefineCerificatePhoneNumber. ИД сотрудника: {0}. Номер телефона не валиден.", _obj.Id);
        if (!string.IsNullOrEmpty(certificatePhone))
          _obj.CertificatePhoneNumberDirRX = string.Empty;
        
        return;
      }

      if (isMatchesPersonalPhoneMask == true)
      {
        Logger.DebugFormat("DefineCerificatePhoneNumber. ИД сотрудника: {0}. Номер телефона соответствует маске личного телефона. Будет использоваться этот номер.", _obj.Id);
        if (!string.IsNullOrEmpty(certificatePhone))
          _obj.CertificatePhoneNumberDirRX = string.Empty;
      }
      else
      {
        Logger.DebugFormat("DefineCerificatePhoneNumber. ИД сотрудника: {0}. Номер телефона не соответствует маске личного телефона. Будет использоваться фиктивный номер.", _obj.Id);
        var formedPhoneNumber = NNIntegration.PublicFunctions.Module.FormCerificatePhoneNumber(phone, _obj.Person.Id);
        var newCertificatePhone = DirRX.EssPlatform.PublicFunctions.Module.PhoneNumberToUniversalFormat(formedPhoneNumber);
        if (newCertificatePhone != certificatePhone)
          _obj.CertificatePhoneNumberDirRX = newCertificatePhone;
      }
    }
    
    #endregion
    
    #region Другие функции.
    
    /// <summary>
    /// Выпустить сертификат основному сотруднику.
    /// </summary>
    /// <param name="newPersonalPhone">Новый личный телефон, который используется при выпуске сертификата.</param>
    /// <remarks>При необходимости устанавливается фиктивный номер телефона, который используется для выпуска сертификата в СОП.</remarks>
    [Public]
    public void IssueCertificateToMainEmployee(string newPersonalPhone)
    {
      var logPrefix = string.Format("IssueCertificateToMainEmployee. ID основного сотрудника: {0}", _obj.Id);
      try
      {
        // Проверить личный телефон на уникальность и при необходимости подобрать фиктивный номер для сертификата.
        OverrideSolution.PublicFunctions.Employee.DefineCerificatePhoneNumber(_obj, newPersonalPhone);
        if (_obj.State.IsChanged)
        {
          _obj.Save();
          Logger.DebugFormat("{0}. Изменился фиктивный номер основного сотрудника.", logPrefix);
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при подборе фиктивного номера для выпуска сертификата. Текст ошибки: {1}", logPrefix, ex.Message);
        throw ex;
      }
      
      // Сформировать структуру для выпуска сертификата.
      var cloudSignProviderInfos = DirRX.SignPlatform.PublicFunctions.Module.GetCloudSignProviderInfo();
      var firstCloudSignProviderInfo = cloudSignProviderInfos.FirstOrDefault();
      var userConnectionParams = DirRX.SignPlatform.Structures.Module.UserConnectionParams.Create();
      userConnectionParams.ProviderId = firstCloudSignProviderInfo.ProviderId;
      userConnectionParams.ProviderType = firstCloudSignProviderInfo.ProviderType;
      userConnectionParams.IdentificationType = firstCloudSignProviderInfo.IdentificationTypes.First().Key;;
      userConnectionParams.IsSet = true;
      
      // Получить автора задачи по значению ИД из прикладной константы.
      var authorId = DirRX.NNCommons.PublicFunctions.Module.GetIdentifierValue(DirRX.NNCommons.PublicConstants.Module.ConstantsSetting.CertificateTaskAuthorId.Guid);
      var author = Employees.Current ?? Users.GetAll(x => x.Id == authorId).FirstOrDefault();
      if (author == null)
      {
        Logger.ErrorFormat("{0}. Не найден автор задачи на выпуск сертификата ЭП. ИД автора из константы: {1}.", logPrefix, authorId);
        return;
      }
      
      // Выпустить сертификат сотруднику.
      try
      {
        DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.Remote.IssueCertificate(_obj, author, userConnectionParams);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при старте задачи на выпуск сертификата ЭП. Текст ошибки: {1}", logPrefix, ex.Message);
        DirRX.NNIntegration.PublicFunctions.Module.SendNotificationToResponsible(author,
                                                                                 DirRX.OverrideSolution.Employees.Resources.IssueCertificateErrorNotificationFormat(Hyperlinks.Get(_obj)),
                                                                                 ex.Message);
        throw ex;
      }
      Logger.DebugFormat("{0}. Запущена задача на выпуск сертификата ЭП.", logPrefix);
    }
    
    /// <summary>
    /// Проверить необходимость выпуска сертификата Выпустить сертификат сотруднику.
    /// </summary>
    /// <returns>Результат. True - если необходимо выпустить сертификат. False - если нет.</returns>
    [Public]
    public virtual bool NeedIssueCertificate()
    {
      var result = _obj.ConsentHRPro == DirRX.OverrideSolution.Employee.ConsentHRPro.Signed &&
        !DirRX.NNCommons.PublicFunctions.Module.HasEssCertificate(_obj.Person.Id) && !HasActiveCertificateIssueTasks();
      if (result)
        Logger.DebugFormat("ID сотрудника: {0}. У сотрудника подписано согласие на КЭДО, отсутствует сертификат ЭП и действующая задача на его выпуск. Необходимо стартовать задачу на выпуск сертификата.", _obj.Id);
      else
        Logger.DebugFormat("ID сотрудника: {0}. У сотрудника не подписано согласие на КЭДО, есть сертификат ЭП или действующая задача на его выпуск. Новая задача на выпуск сертификата не будет создана.", _obj.Id);
      
      return result;
    }
    
    /// <summary>
    /// Проверить существование действующей задачи на выпуск сертификата ЭП для сотрудника.
    /// </summary>
    /// <returns>Результат проверки. True - есть задача. False - задачи нет.</returns>
    [Public, Remote]
    public virtual bool HasActiveCertificateIssueTasks()
    {
      var isCustomCertificateIssueProcess = NNCommons.PublicFunctions.Module.Remote.GetBoolValue(NNCommons.PublicConstants.Module.ConstantsSetting.CertificateIssueProcess.Guid).Value;
      if (isCustomCertificateIssueProcess)
        return NNCommons.CertificateIssueTasks.GetAll(c => c.Employee.Id == _obj.Id && c.Status == Sungero.Workflow.Task.Status.InProcess).Any();
      else
        return DirRX.SignPlatform.CertificateIssueTasks.GetAll(c => c.Employee.Id == _obj.Id && c.Status == Sungero.Workflow.Task.Status.InProcess).Any();
    }
    
    #endregion
    
    #endregion
    
    #region Логика подключения/отключения сотрудника от ЛК.
    
    #region Входные точки для подключения/отключения от ЛК.

    /// <summary>
    /// Создать/активировать УЗ сотрудника в ЛК.
    /// </summary>
    [Public]
    public virtual void ActivateEssEmployee()
    {
      var logPrefix = "ActivateEssEmployee";
      Logger.DebugFormat("{0}. ID сотрудника: {1}. Согласие на КЭДО: {2}. Начало создания/активация учётной записи в ЛК.", logPrefix, _obj.Id,
                         DirRX.OverrideSolution.Employees.Info.Properties.ConsentHRPro.GetLocalizedValue(_obj.ConsentHRPro));
      
      // Подключить сотрудника к ЛК.
      try
      {
        DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.Remote.ActivateESSUser(_obj, _obj.Person, _obj.PersonalPhoneDirRX, _obj.BusinessUnit);
        Logger.DebugFormat("{0}. ID сотрудника: {1}. Создана/активирована УЗ сотрудника в ЛК.",
                           logPrefix, _obj.Id);
        
        // Если согласие подписано, то перевыпустить сертификат.
        if (_obj.ConsentHRPro == DirRX.OverrideSolution.Employee.ConsentHRPro.Signed)
          ReissueCertificate();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. ID сотрудника: {1}. Во время активации УЗ в ЛК возникла ошибка. Текст ошибки: {2}. StackTrace: {3}.",
                           logPrefix, _obj.Id, ex.Message, ex.StackTrace);
        throw ex;
      }
    }

    /// <summary>
    /// Отключить сотрудника от ЛК.
    /// </summary>
    [Public]
    public virtual void DisconnectEssEmployee()
    {
      Logger.DebugFormat("DisconnectEssEmployee. ID Сотрудника: {0}. Отключение сотрудника от ЛК.", _obj.Id);
      
      // HACK: Проверить наличие УЗ в ЛК. В коробке HR в функции DisconnectEssUser() проверка на наличие УЗ - не выполняется.
      var personUid = EssPlatform.PublicFunctions.Module.Remote.GetUidPerson(_obj.Person);
      var isUserExist = EssPlatform.PublicFunctions.Module.Remote.ExistsIdsUser(personUid);
      
      // Если УЗ есть в ЛК, то отключить.
      if (isUserExist)
        EssPlatform.PublicFunctions.Module.Remote.DisconnectEssUser(_obj);
    }
    
    #endregion
    
    #region Создание/изменение/очистка УЗ.

    /// <summary>
    /// Валидация сотрудника и персоны перед созданием УЗ для ЛК.
    /// </summary>
    /// <param name="phoneNumber">Номер телефона.</param>
    /// <returns>Ошибка, иначе string.Empty.</returns>
    [Public]
    public virtual string Validate(string phoneNumber)
    {
      var errorMessage = string.Empty;
      var errorList = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.Remote.ValidatePersonRequiredFields(_obj.Person).AsEnumerable();

      var formatFormNumber = GetFormatPhoneNumber(phoneNumber);
      
      if (string.IsNullOrEmpty(formatFormNumber))
        errorList = errorList.Append(string.Format("Переданный номер: '{0}' не соответсвует требуемому формату.", phoneNumber));
      
      if (!errorList.Any())
        return string.Empty;
      
      errorMessage = errorList.Aggregate(errorMessage, (current, message) => current + $"{message}\n");
      return errorMessage;
    }
    
    /// <summary>
    /// Отчистить УЗ у сотрудника.
    /// </summary>
    /// <returns>True если успешно, иначе false.</returns>
    [Public]
    public bool ClearAccountInEmployee()
    {
      var logPrefix = string.Format("ClearAccountInEmployee. Ид сотрудника {0}", _obj.Id);
      var isLock = false;
      try
      {
        if (Locks.TryLock(_obj))
        {
          isLock = true;
          Transactions.Execute((() =>
                                {
                                  _obj.Login = null;
                                  _obj.Save();
                                }));
          Logger.DebugFormat("{0}. Отчистка УЗ прошла успешно.", logPrefix);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(_obj);
          Logger.ErrorFormat("{0}. Запись заблокирована пользователем: {1}. Время установки блокировки: {2} ", logPrefix, lockInfo.OwnerName, lockInfo.LockTime);
          return false;
        }

        return true;
      }
      finally
      {
        if (isLock && Locks.GetLockInfo(_obj).IsLockedByMe)
          Locks.Unlock(_obj);
      }
    }
    
    /// <summary>
    /// Заполнить/очистить параметры при отключении сотрудника от ЛК.
    /// </summary>
    /// <remarks>Функция выполняет в том числе очистку УЗ ЛК в карточке сотрудника при отключении.</remarks>
    public void FillParametersAfterDissconect()
    {
      // Если при отключении указан шаблонный логин для пользователя, то его нужно отчистить.
      var hrLoginTemplateName = DirRX.EssPlatform.PublicFunctions.Module.GetEmployeeAccountNameWithTemplate(_obj.Person.Id);
      if (_obj.Login?.LoginName == hrLoginTemplateName)
        _obj.Login = Logins.Null;
      
      // HACK: Коробочная логика действия Отключить от личного кабинета (DeleteEssUserDirRX).
      // При отключении ставим поля с СМС или Вайбером в емаил если поля с емаил заполнены.
      var hasEmailPersonal = !string.IsNullOrEmpty(_obj.MessagesEmailDirRX);
      var hasEmail = !string.IsNullOrEmpty(_obj.Email);
      if(_obj.NeedNotifyNewHRAssignmentDirRX == DirRX.OverrideSolution.Employee.NeedNotifyNewHRAssignmentDirRX.SMS || _obj.NeedNotifyNewHRAssignmentDirRX == DirRX.OverrideSolution.Employee.NeedNotifyNewHRAssignmentDirRX.Viber)
      {
        _obj.NeedNotifyNewHRAssignmentDirRX = DirRX.OverrideSolution.Employee.NeedNotifyNewHRAssignmentDirRX.No;
        if(hasEmail)
          _obj.NeedNotifyNewHRAssignmentDirRX = DirRX.OverrideSolution.Employee.NeedNotifyNewHRAssignmentDirRX.Email;
        if(hasEmailPersonal)
          _obj.NeedNotifyNewHRAssignmentDirRX = DirRX.OverrideSolution.Employee.NeedNotifyNewHRAssignmentDirRX.EmailPersonal;
      }
      if(_obj.NeedNotifyExpiredHRAssignmentDirRX == DirRX.OverrideSolution.Employee.NeedNotifyExpiredHRAssignmentDirRX.SMS || _obj.NeedNotifyExpiredHRAssignmentDirRX == DirRX.OverrideSolution.Employee.NeedNotifyExpiredHRAssignmentDirRX.Viber)
      {
        _obj.NeedNotifyExpiredHRAssignmentDirRX = DirRX.OverrideSolution.Employee.NeedNotifyExpiredHRAssignmentDirRX.No;
        if(hasEmail)
          _obj.NeedNotifyExpiredHRAssignmentDirRX = DirRX.OverrideSolution.Employee.NeedNotifyExpiredHRAssignmentDirRX.Email;
        if(hasEmailPersonal)
          _obj.NeedNotifyExpiredHRAssignmentDirRX = DirRX.OverrideSolution.Employee.NeedNotifyExpiredHRAssignmentDirRX.EmailPersonal;
      }
      if(_obj.NeedNotifyHRRepeatDirRX == DirRX.OverrideSolution.Employee.NeedNotifyHRRepeatDirRX.SMS || _obj.NeedNotifyHRRepeatDirRX == DirRX.OverrideSolution.Employee.NeedNotifyHRRepeatDirRX.Viber)
      {
        _obj.NeedNotifyHRRepeatDirRX = DirRX.OverrideSolution.Employee.NeedNotifyHRRepeatDirRX.No;
        if(hasEmail)
          _obj.NeedNotifyHRRepeatDirRX = DirRX.OverrideSolution.Employee.NeedNotifyHRRepeatDirRX.Email;
        if(hasEmailPersonal)
          _obj.NeedNotifyHRRepeatDirRX = DirRX.OverrideSolution.Employee.NeedNotifyHRRepeatDirRX.EmailPersonal;
      }
    }
    
    #endregion
    
    #region Функции для выполнения поиска основного сотрудника/совместителей.
    
    /// <summary>
    /// Получить cотрудника с основным местом работы.
    /// </summary>
    /// <returns>Cотрудник с основным местом работы.</returns>
    [Public]
    public IEmployee GetMainEmployee()
    {
      var processedEmployees = new List<IEmployee>(){_obj};
      var employees = FindOtherAccountsEmployee();
      processedEmployees.AddRange(employees);

      var mainEmployee =
        processedEmployees.FirstOrDefault(
          c => c.EmploymentType.HasValue && c.EmploymentType == EmploymentType.MainPlace);

      if (mainEmployee != null)
        return mainEmployee;
      
      Logger.ErrorFormat("GetMainEmployee. Сотрудник с табельным номером: '{0}' не имеет совместителей с основным местом работы.", _obj.PersonnelNumber);
      return null;
    }
    
    /// <summary>
    /// Найти всех совместителей.
    /// </summary>
    /// <returns>Совместители.</returns>
    [Public]
    public List<IEmployee> FindOtherAccountsEmployee()
    {
      if (_obj.Person != null)
        return Employees.GetAll(c => (c.Person.Id == _obj.Person.Id) && (c.Id != _obj.Id) && (c.Status == OverrideSolution.Employee.Status.Active)).ToList();
      
      Logger.ErrorFormat("FindOtherAccountsEmployee. Отсутсвует персона у переданного сотрудника c табельным номером: '{0}'.", _obj.PersonnelNumber);
      return null;
    }
    
    /// <summary>
    /// Получить доступного к обработке сотрудника.
    /// </summary>
    /// <param name="employeePersonnelNumber">Табельный номер.</param>
    /// <param name="errorStruct">Ошибки.</param>
    /// <returns>True если сотрудник найден и не заблокирован, иначе False.</returns>
    [Public]
    public static IEmployee GetAvailabilityEmployee(string employeePersonnelNumber, IProcessingErrorStruct errorStruct)
    {
      var logPrefix = string.Format("GetAvailabilityEmployee. Табельный номер сотрудника: {0}", employeePersonnelNumber);
      var employee = DirRX.OverrideSolution.PublicFunctions.Employee.GetEmployeeByPersNumber(employeePersonnelNumber);
      
      if (employee == null)
      {
        errorStruct.ErrorText = string.Format("Сотрудник с табельным номером: '{0}' не найден", employeePersonnelNumber);
        Logger.ErrorFormat("{0}. Сотрудник не найден.", logPrefix);
        return null;
      }

      if (!Locks.GetLockInfo(employee).IsLocked)
        return employee;
      
      errorStruct.ErrorText = string.Format("Сотрудник с табельным номером: '{0}' заблокирован на изменения.", employeePersonnelNumber);
      errorStruct.isBlock = true;
      var lockInfo = Locks.GetLockInfo(employee);
      Logger.ErrorFormat("{0}. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}.", logPrefix, lockInfo.OwnerName, lockInfo.LockTime);
      return null;
    }
    
    #endregion
    
    #region Другие функции для подключения/отключения сотрудника.
    
    /// <summary>
    /// Попробовать заблокировать сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Ошибка если сущность не сохранена, иначе string.Empty.</returns>
    [Public]
    public string TryLockEmployee()
    {
      if (Locks.TryLock(_obj))
        return string.Empty;
      
      var errorMessage = string.Format("Не удалось заблокировать сотрудника с ИД: '{0}'.", _obj.Id);
      var lockInfo = Locks.GetLockInfo(_obj);
      Logger.ErrorFormat("TryLockEmployees. {0} Запись заблокирована пользователем: {1}. Время установки блокировки: {2}.", errorMessage, lockInfo.OwnerName, lockInfo.LockTime);
      
      return errorMessage;
    }
    
    /// <summary>
    /// Попробовать сохранить сотрудника.
    /// </summary>
    /// <returns>Ошибка если сущность не сохранена, иначе string.Empty.</returns>
    [Public]
    public string TrySaveEmployee()
    {
      var errorMessage = string.Empty;
      try
      {
        _obj.Save();
        return string.Empty;
      }
      catch (Exception e)
      {
        errorMessage = string.Format("Не удалось сохранить сотрудника с ИД '{0}'. Текст ошибки: {1}", _obj.Id, e.Message);
        Logger.ErrorFormat("TrySaveEmployee. {0}. StackTrace: {1}", errorMessage, e.StackTrace);
      }
      return errorMessage;
    }
    
    /// <summary>
    /// Изменить статус согласия КЭДО.
    /// </summary>
    /// <param name="isKedo">Подписано согласие на КЭДО.</param>
    /// <returns>Ошибка, иначе string.Empty.</returns>
    [Public]
    public string ChangeConsent(bool isKedo)
    {
      var errorMessage = string.Empty;
      var newConsent = isKedo ? ConsentHRPro.Signed : ConsentHRPro.NotSent;
      if (_obj.ConsentHRPro != newConsent)
      {
        Logger.DebugFormat("ChangeConsent. ИД сотрудника: {0}. Согласие на КЭДО: {1}.", _obj.Id, _obj.Info.Properties.ConsentHRPro.GetLocalizedValue(newConsent));
        _obj.ConsentHRPro = newConsent;
        errorMessage = TrySaveEmployee();
      }

      return errorMessage;
    }
    
    /// <summary>
    /// Получить форматированный и валидный номер телефона.
    /// </summary>
    /// <param name="phoneNumber">Номер телефона.</param>
    /// <returns>Форматированный номер телефона.</returns>
    [Public]
    public virtual string GetFormatPhoneNumber(string phoneNumber)
    {
      return !DirRX.EssPlatform.PublicFunctions.Module.PhoneNumberLengthIsValid(phoneNumber, false) ?
        string.Empty : DirRX.EssPlatform.PublicFunctions.Module.PhoneNumberToUniversalFormat(phoneNumber);
    }
    
    #endregion
    
    #endregion
    
    #region Функции вычисляемых выражений.
    
    /// <summary>
    /// Expression-функция для проверки наличия совместителей.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Результат. True - если есть совместители. False - если нет.</returns>
    [Public, ExpressionElement("ExpFunc_CheckOtherAccountsEmployee_Name", "ExpFunc_CheckOtherAccountsEmployee_Desc", "")]
    public static bool CheckOtherAccountsEmployee(IEmployee employee)
    {
      var partTimers = OverrideSolution.Functions.Employee.FindOtherAccountsEmployee(employee);
      return partTimers != null && partTimers.Any();
    }
    
    #endregion
    
    #region Остальные функции.
    
    /// <summary>
    /// Определить, можно ли изменять карточку сотрудника, его учётную запись и роль.
    /// </summary>
    /// <param>Если вызов происходит при подключении к личтому кабинету - true, если при отключении - false.</param>
    /// <returns>Список ошибок блокировки.</returns>
    [Public]
    public override List<string> IsItPossibleToChangeEntities(bool isСreating)
    {
      return base.IsItPossibleToChangeEntities(isСreating);
    }
    
    /// <summary>
    /// Заполнить свойства ЛК.
    /// </summary>
    [Remote]
    public override void FillPersonalOfficeProperties()
    {
      var phone = _obj.PersonalPhoneDirRX;
      base.FillPersonalOfficeProperties();
      if (_obj.PersonalPhoneDirRX != phone)
        _obj.PersonalPhoneDirRX = phone;
    }

    /// <summary>
    /// Проверить наличие подписанного согласия на КЭДО у сотрудника.
    /// </summary>
    /// <param name="allPersonEmployees">Признак того, что нужно проверить согласия всех совместителей сотрудника.</param>
    /// <returns>Результат. Если у сотрудника или хотя бы у одного своместителя согласие на КЭДО подписано - true, иначе false.</returns>
    [Public, Remote(IsPure = true)]
    public virtual bool CheckConsentSigned(bool allPersonEmployees)
    {
      var isConsentSigned = false;
      if (allPersonEmployees)
        isConsentSigned = Employees.GetAll(x => x.Person.Id == _obj.Person.Id &&
                                           x.Status == OverrideSolution.Employee.Status.Active &&
                                           x.PersonalAccountStatusDirRX != DirRX.OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent &&
                                           x.ConsentHRPro == DirRX.OverrideSolution.Employee.ConsentHRPro.Signed).Any();
      else
        isConsentSigned = _obj.ConsentHRPro == DirRX.OverrideSolution.Employee.ConsentHRPro.Signed;
      
      Logger.DebugFormat("CheckConsentSigned. ИД сотрудника: {0}. ИД персоны: {1}. Проверить всех сотрудников персоны: {2}. Результат проверки: {3}.",
                         _obj.Id, _obj.Person.Id, allPersonEmployees, isConsentSigned);
      
      return isConsentSigned;
    }
    
    /// <summary>
    /// Установить вид занятости сотрудника по его коду.
    /// </summary>
    /// <param name="empType">Код вида занятости.</param>
    /// <returns>Результат. True - удалось установить вид занятости. False - не удалось установить вид занятости.</returns>
    [Public]
    public bool TryParseEmployemntType(string empType)
    {
      var employmentTypeMapping = new Dictionary<string, Enumeration>()
      {
        { DirRX.NNIntegration.PublicConstants.EmployeeQueue.EmploymentTypeCodes.MainPlace, DirRX.OverrideSolution.Employee.EmploymentType.MainPlace },
        { DirRX.NNIntegration.PublicConstants.EmployeeQueue.EmploymentTypeCodes.InternalConcurr, DirRX.OverrideSolution.Employee.EmploymentType.InternalConcurr },
        { DirRX.NNIntegration.PublicConstants.EmployeeQueue.EmploymentTypeCodes.ExternalConcurr, DirRX.OverrideSolution.Employee.EmploymentType.ExternalConcurr }
      };

      Enumeration newEmploymentType;
      if (employmentTypeMapping.TryGetValue(empType, out newEmploymentType))
      {
        if (_obj.EmploymentType != newEmploymentType)
          _obj.EmploymentType = newEmploymentType;

        return true;
      }

      return false;
    }
    
    /// <summary>
    /// Получить сотрудника по табельному номеру.
    /// </summary>
    /// <param name="personalNumber">Табельный номер.</param>
    /// <returns>Сотрудник.</returns>
    [Public]
    public static DirRX.OverrideSolution.IEmployee GetEmployeeByPersNumber(string personalNumber)
    {
      return DirRX.OverrideSolution.Employees.GetAll(c => c.PersonnelNumber.Equals(personalNumber)).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить сотрудника по табельному номеру.
    /// </summary>
    /// <param name="personalNumber">Табельный номер сотрудника.</param>
    /// <returns>Результат поиска сотрудника.</returns>
    [Public]
    public static Structures.Module.ISearchEmployeeResult GetEmployeeByPersNumberWithError(string personalNumber)
    {
      var errorMessage = string.Empty;
      var employee = GetEmployeeByPersNumber(personalNumber);
      if (employee == null)
      {
        errorMessage = string.Format("Не найден сотрудник с табельным номером: {0}.", personalNumber);
        Logger.ErrorFormat("GetEmployeeByPersNumberWithError. {0}.", errorMessage);
      }
      return Structures.Module.SearchEmployeeResult.Create(employee, errorMessage);
    }
    
    #endregion
    
  }
}
