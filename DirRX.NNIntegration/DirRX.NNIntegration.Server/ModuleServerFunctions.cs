using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Data;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using DirRX.NNIntegration.Structures.Module;
using System.Security.Cryptography;
using DirRX.NNOtherProcesses;
using DirRX.NNOtherProcesses.NNRetentionStatementDocument;
using DirRX.OverrideSolution;
using Sungero.Domain.Shared;
using Newtonsoft.Json;
using Sungero.CoreEntities.Login;
using DirRX.OverrideSolution.Employee;
using DirRX.NNOtherProcesses.NNBusinessTripOrderDocument;
using System.Text.RegularExpressions;

namespace DirRX.NNIntegration.Server
{
  public partial class ModuleFunctions
  {

    #region Интеграционный сценарий "Синхронизация организационной структуры".
    
    #region Post методы создания записей очередей синхронизации орг. структуры.
    
    /// <summary>
    /// Создать запись справочника "Очередь сообщений справочника Подразделения".
    /// </summary>
    /// <param name="OHRP1000STEXT">Наименование.</param>
    /// <param name="OHRP1000OBJID">SAP IP объекта.</param>
    /// <param name="OHRP1000SHORT">Краткое наименование.</param>
    /// <param name="Headquarters">Признак «Штаб».</param>
    /// <param name="HeadOffice">Головное подразделение.</param>
    /// <param name="Manager">Руководитель.</param>
    /// <param name="OHRP1000ENDDA">Дата окончания действия записи.</param>
    /// <param name="OHRP1000BEGDA">Дата начала действия записи.</param>
    /// <param name="BusinessUnit">Организация.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateDepartmentQueueRecord(string OHRP1000STEXT, string OHRP1000OBJID, string OHRP1000SHORT, bool? Headquarters, string HeadOffice, string Manager, string OHRP1000ENDDA, string OHRP1000BEGDA, string BusinessUnit)
    {
      Logger.DebugFormat("CreateDepartmentQueueRecord. SAP ID: {0}. Получен запрос на создание записи очереди справочника Подразделения.", OHRP1000OBJID);
      var queueRecord = DepartmentQueues.Create();
      queueRecord.Name = OHRP1000STEXT;
      queueRecord.SapId = OHRP1000OBJID;
      queueRecord.ShortName = OHRP1000SHORT;
      queueRecord.IsHeadquarters = Headquarters ?? false;
      queueRecord.HeadOffice = HeadOffice;
      queueRecord.Manager = Manager;
      queueRecord.BusinessUnit = BusinessUnit;
      
      // Проверка периода действия записи.
      var error = Functions.CompanyQueueBase.SetRecordDatePeriod(queueRecord, OHRP1000BEGDA, OHRP1000ENDDA);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateDepartmentQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", OHRP1000OBJID, error);
        Logger.ErrorFormat("CreateDepartmentQueueRecord. SAP ID: {0}. Запись справочника \"Очередь сообщений справочника Подразделения\" не создана из-за ошибок валидации.", OHRP1000OBJID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, error);
      }
      
      try
      {
        queueRecord.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CreateDepartmentQueueRecord. SAP ID подразделения: {0}. Ошибка при сохранении записи справочника \"Очередь сообщений справочника Подразделения\". Текст ошибки: {1}, StackTrace: {2}.",
                           queueRecord.SapId, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, ex.Message);
      }
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
    }
    
    
    /// <summary>
    /// Создать запись справочника "Очередь сообщений справочника Должности".
    /// </summary>
    /// <param name="SHRP1000STEXT">Наименование.</param>
    /// <param name="SHRP1000OBJID">SAP ID.</param>
    /// <param name="Division">Подразделение.</param>
    /// <param name="SHRP1001BEGDA">Дата начала действия записи.</param>
    /// <param name="SHRP1001ENDDA">Дата окончания действия записи.</param>
    /// <param name="IsEmployeeExists">Есть сотрудники на должности.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateJobTitleQueueRecord(string SHRP1000STEXT, string SHRP1000OBJID, string Division, string SHRP1001BEGDA, string SHRP1001ENDDA, bool IsEmployeeExists)
    {
      Logger.DebugFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Получен запрос на создание записи очереди справочника Должности.", SHRP1000OBJID);
      var queueRecord = JobTitleQueues.Create();
      queueRecord.Name = SHRP1000STEXT;
      queueRecord.SapId = SHRP1000OBJID;
      queueRecord.DepartmentCode = Division;
      queueRecord.IsEmployeeExists = IsEmployeeExists;
      
      var validationErrors = string.Empty;
      
      // Проверка периода действия записи.
      var error = Functions.CompanyQueueBase.SetRecordDatePeriod(queueRecord, SHRP1001BEGDA, SHRP1001ENDDA);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", SHRP1000OBJID, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Вернуть ошибки валидации запроса в КСИП.
      if (!string.IsNullOrEmpty(validationErrors))
      {
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Запись справочника \"Очередь сообщений справочника Должности\" не создана из-за ошибок валидации.", SHRP1000OBJID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, validationErrors);
      }
      
      try
      {
        queueRecord.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID подразделения: {0}. Ошибка при сохранении записи справочника \"Очередь сообщений справочника Должности\". Текст ошибки: {1}, StackTrace: {2}.",
                           queueRecord.SapId, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, ex.Message);
      }
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
    }
    
    /// <summary>
    /// Создать запись справочника "Очередь сообщений справочника Должности".
    /// </summary>
    /// <param name="SHRP1000STEXT">Наименование.</param>
    /// <param name="SHRP1000OBJID">SAP ID.</param>
    /// <param name="Division">Подразделение.</param>
    /// <param name="SHRP1001BEGDA">Дата начала действия записи.</param>
    /// <param name="SHRP1001ENDDA">Дата окончания действия записи.</param>
    /// <param name="SHRP1222ZATTRIB">Атрибут «Группа оценки».</param>
    /// <param name="IsEmployeeExists">Есть сотрудники на должности.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateJobTitleQueueRecordV2(string SHRP1000STEXT, string SHRP1000OBJID, string Division, string SHRP1001BEGDA, string SHRP1001ENDDA, string SHRP1222ZATTRIB, bool IsEmployeeExists)
    {
      Logger.DebugFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Получен запрос на создание записи очереди справочника Должности.", SHRP1000OBJID);
      var queueRecord = JobTitleQueues.Create();
      queueRecord.Name = SHRP1000STEXT;
      queueRecord.SapId = SHRP1000OBJID;
      queueRecord.DepartmentCode = Division;
      queueRecord.IsEmployeeExists = IsEmployeeExists;
      
      var validationErrors = string.Empty;
      
      // Проверка периода действия записи.
      var error = Functions.CompanyQueueBase.SetRecordDatePeriod(queueRecord, SHRP1001BEGDA, SHRP1001ENDDA);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", SHRP1000OBJID, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка поля Атрибут "Группа оценки".
      if (!Functions.JobTitleQueue.TryParseJobTitleEvalutionGroup(queueRecord, SHRP1222ZATTRIB))
      {
        error = DirRX.NNIntegration.Resources.EvalutionGroupParseErrorFormat(SHRP1222ZATTRIB);
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", SHRP1000OBJID, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Вернуть ошибки валидации запроса в КСИП.
      if (!string.IsNullOrEmpty(validationErrors))
      {
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Запись справочника \"Очередь сообщений справочника Должности\" не создана из-за ошибок валидации.", SHRP1000OBJID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, validationErrors);
      }
      
      try
      {
        queueRecord.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID подразделения: {0}. Ошибка при сохранении записи справочника \"Очередь сообщений справочника Должности\". Текст ошибки: {1}, StackTrace: {2}.",
                           queueRecord.SapId, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, ex.Message);
      }
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
    }
    
    /// <summary>
    /// Создать запись справочника "Очередь сообщений справочника Должности".
    /// </summary>
    /// <param name="SHRP1000STEXT">Наименование.</param>
    /// <param name="SHRP1000OBJID">SAP ID.</param>
    /// <param name="Division">Подразделение.</param>
    /// <param name="SHRP1001BEGDA">Дата начала действия записи.</param>
    /// <param name="SHRP1001ENDDA">Дата окончания действия записи.</param>
    /// <param name="PP0001ZZPERNR_TOP">ТОП.</param>
    /// <param name="IsEmployeeExists">Есть сотрудники на должности.</param>
    /// <param name="Z_POS_LVL">Уровень штатной должности.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateJobTitleQueueRecordV3(string SHRP1000STEXT, string SHRP1000OBJID, string Division, string SHRP1001BEGDA,
                                                                                string SHRP1001ENDDA, bool PP0001ZZPERNR_TOP, bool IsEmployeeExists, string Z_POS_LVL)
    {
      Logger.DebugFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Получен запрос на создание записи очереди справочника Должности.", SHRP1000OBJID);
      var queueRecord = JobTitleQueues.Create();
      queueRecord.Name = SHRP1000STEXT;
      queueRecord.SapId = SHRP1000OBJID;
      queueRecord.DepartmentCode = Division;
      queueRecord.IsEmployeeExists = IsEmployeeExists;
      queueRecord.IsTop = PP0001ZZPERNR_TOP;
      queueRecord.PositionLevel = Z_POS_LVL;
      
      var validationErrors = string.Empty;
      
      // Проверка периода действия записи.
      var error = Functions.CompanyQueueBase.SetRecordDatePeriod(queueRecord, SHRP1001BEGDA, SHRP1001ENDDA);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", SHRP1000OBJID, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Вернуть ошибки валидации запроса в КСИП.
      if (!string.IsNullOrEmpty(validationErrors))
      {
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID: {0}. Запись справочника \"Очередь сообщений справочника Должности\" не создана из-за ошибок валидации.", SHRP1000OBJID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, validationErrors);
      }
      
      try
      {
        queueRecord.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CreateJobTitleQueueRecord. SAP ID подразделения: {0}. Ошибка при сохранении записи справочника \"Очередь сообщений справочника Должности\". Текст ошибки: {1}, StackTrace: {2}.",
                           queueRecord.SapId, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, ex.Message);
      }
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
    }
    
    /// <summary>
    /// Создать запись справочника "Очередь сообщений справочника Сотрудники".
    /// </summary>
    /// <param name="PP0002NACHN">Фамилия.</param>
    /// <param name="PP0002VORNA">Имя.</param>
    /// <param name="PP0002MIDNM">Отчество.</param>
    /// <param name="PP0002GBDAT">Дата рождения.</param>
    /// <param name="PP0290ICCD">СНИЛС.</param>
    /// <param name="PP0002GESCH">Пол.</param>
    /// <param name="Region">Регион.</param>
    /// <param name="PP0006ORT01">Населенный пункт.</param>
    /// <param name="RegistrationAddress">Адрес регистрации.</param>
    /// <param name="PostalAddress">Почтовый адрес.</param>
    /// <param name="Phones">Телефоны.</param>
    /// <param name="Division">Подразделение.</param>
    /// <param name="PP0001PLANS">Должность.</param>
    /// <param name="PP0001ZZPERNR_MNG">Линейный руководитель.</param>
    /// <param name="PP0001PERSG">Вид занятости.</param>
    /// <param name="PP0000PERNR">Табельный номер.</param>
    /// <param name="Email">Эл. почта (рабочая).</param>
    /// <param name="PP0000BEGDA">Дата начала действия записи.</param>
    /// <param name="PP0000ENDDA">Дата окончания действия записи.</param>
    /// <param name="MPGroup">Группа МР.</param>
    /// <param name="PP0001VDSK1">НГД.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateEmployeeQueueRecord(string PP0002NACHN, string PP0002VORNA, string PP0002MIDNM, string PP0002GBDAT, string PP0290ICCD, string PP0002GESCH, string Region,
                                                                              string PP0006ORT01, string RegistrationAddress, string PostalAddress, string Phones, string Division, string PP0001PLANS, string PP0001ZZPERNR_MNG,
                                                                              string PP0001PERSG, string PP0000PERNR, string Email, string PP0000BEGDA, string PP0000ENDDA,
                                                                              string MPGroup, string PP0001VDSK1)
    {
      Logger.DebugFormat("CreateEmployeeQueueRecord. Табельный номер: {0}. Получен запрос на создание записи очереди справочника Сотрудники.", PP0000PERNR);
      var queueRecord = EmployeeQueues.Create();
      queueRecord.LastName = PP0002NACHN;
      queueRecord.FirstName = PP0002VORNA;
      queueRecord.MiddleName = PP0002MIDNM;
      queueRecord.INILA = PP0290ICCD;
      queueRecord.Region = Region;
      queueRecord.City = PP0006ORT01;
      queueRecord.LegalAddress = RegistrationAddress;
      queueRecord.PostalAddress = PostalAddress;
      queueRecord.Phones = Phones;
      queueRecord.DepartmentCode = Division;
      queueRecord.JobTitleCode = PP0001PLANS;
      queueRecord.LineManagerCode = PP0001ZZPERNR_MNG;
      queueRecord.EmploymentType = PP0001PERSG;
      queueRecord.PersonnelNumber = PP0000PERNR;
      queueRecord.Email = Email;
      queueRecord.MPGroup = MPGroup;
      
      var validationErrors = string.Empty;
      
      // Проверка периода действия записи.
      var error = Functions.CompanyQueueBase.SetRecordDatePeriod(queueRecord, PP0000BEGDA, PP0000ENDDA);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка СНИЛС.
      error = Functions.EmployeeQueue.CheckInila(queueRecord);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1} СНИЛС: {2}.", PP0000PERNR, error, queueRecord.INILA);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка кода вида занятости сотрудника.
      if (!Functions.EmployeeQueue.CheckEmployemntType(queueRecord.EmploymentType))
      {
        error = DirRX.NNIntegration.Resources.EmployeeEmployemntTypeParseErrorFormat(queueRecord.EmploymentType);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка кода пола персоны.
      if (!Functions.EmployeeQueue.TryParsePersonSex(queueRecord, PP0002GESCH))
      {
        error = DirRX.NNIntegration.Resources.SexParseErrorFormat(PP0002GESCH);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка Группы МР.
      error = Functions.EmployeeQueue.CheckMPGroup(queueRecord);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      //Проверка признака НГД.
      if (!Functions.EmployeeQueue.TrySetNGD(queueRecord, PP0001VDSK1))
      {
        error = DirRX.NNIntegration.Resources.NGDParseErrorFormat(PP0001VDSK1);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      var culture = new System.Globalization.CultureInfo("ru-RU");
      if (!string.IsNullOrEmpty(PP0002GBDAT))
        queueRecord.DateOfBirth = DateTime.Parse(PP0002GBDAT, culture);
      
      // Вернуть ошибки валидации запроса в КСИП.
      if (!string.IsNullOrEmpty(validationErrors))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. Таб. номер сотрудника: {0}. Запись справочника \"Очередь сообщений справочника Сотрудники\" не создана из-за ошибок валидации.",
                           queueRecord.PersonnelNumber);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, validationErrors);
      }
      
      try
      {
        queueRecord.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. Таб. номер сотрудника: {0}. Ошибка при сохранении записи справочника \"Очередь сообщений справочника Сотрудники\". Текст ошибки: {1}, StackTrace: {2}.",
                           queueRecord.PersonnelNumber, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, ex.Message);
      }
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
    }
    
    /// <summary>
    /// Создать запись справочника "Очередь сообщений справочника Сотрудники".
    /// </summary>
    /// <param name="PP0002NACHN">Фамилия.</param>
    /// <param name="PP0002VORNA">Имя.</param>
    /// <param name="PP0002MIDNM">Отчество.</param>
    /// <param name="PP0002GBDAT">Дата рождения.</param>
    /// <param name="PP0290ICCD">СНИЛС.</param>
    /// <param name="PP0002GESCH">Пол.</param>
    /// <param name="Region">Регион.</param>
    /// <param name="PP0006ORT01">Населенный пункт.</param>
    /// <param name="RegistrationAddress">Адрес регистрации.</param>
    /// <param name="PostalAddress">Почтовый адрес.</param>
    /// <param name="Phones">Телефоны.</param>
    /// <param name="Division">Подразделение.</param>
    /// <param name="PP0001PLANS">Должность.</param>
    /// <param name="PP0001ZZPERNR_MNG">Линейный руководитель.</param>
    /// <param name="PP0001PERSG">Вид занятости.</param>
    /// <param name="PP0000PERNR">Табельный номер.</param>
    /// <param name="Email">Эл. почта (рабочая).</param>
    /// <param name="PP0000BEGDA">Дата начала действия записи.</param>
    /// <param name="PP0000ENDDA">Дата окончания действия записи.</param>
    /// <param name="MPGroup">Группа МР.</param>
    /// <param name="PP0001VDSK1">НГД.</param>
    /// <param name="TIN">ИНН.</param>
    /// <param name="IdentityDocumentKind">Вид документа.</param>
    /// <param name="IdentityDocumentSeries">Вид документа.</param>
    /// <param name="IdentityDocumentNumber">Номер.</param>
    /// <param name="IdentityDocumentIssueDate">Дата выдачи.</param>
    /// <param name="IdentityDocumentIssuedBy">Кем выдан.</param>
    /// <param name="IdentityDocumentIssuerID">Код подразделения.</param>
    /// <param name="IdentityDocumentBirthPlace">Место рождения.</param>
    /// <param name="IdentityDocumentExpirationDate">Срок действия.</param>
    /// <param name="UtcOffset">Часовой пояс.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateEmployeeQueueRecordV2(string PP0002NACHN, string PP0002VORNA, string PP0002MIDNM, string PP0002GBDAT, string PP0290ICCD, string PP0002GESCH, string Region,
                                                                                string PP0006ORT01, string RegistrationAddress, string PostalAddress, string Phones, string Division, string PP0001PLANS, string PP0001ZZPERNR_MNG,
                                                                                string PP0001PERSG, string PP0000PERNR, string Email, string PP0000BEGDA, string PP0000ENDDA,
                                                                                string MPGroup, string PP0001VDSK1,
                                                                                string TIN, string IdentityDocumentKind, string IdentityDocumentSeries, string IdentityDocumentNumber, string IdentityDocumentIssueDate,
                                                                                string IdentityDocumentIssuedBy, string IdentityDocumentIssuerID, string IdentityDocumentBirthPlace, string IdentityDocumentExpirationDate,
                                                                                string UtcOffset)
    {
      Logger.DebugFormat("CreateEmployeeQueueRecord. Табельный номер: {0}. Получен запрос на создание записи очереди справочника Сотрудники.", PP0000PERNR);
      var queueRecord = EmployeeQueues.Create();
      queueRecord.LastName = PP0002NACHN;
      queueRecord.FirstName = PP0002VORNA;
      queueRecord.MiddleName = PP0002MIDNM;
      queueRecord.INILA = PP0290ICCD;
      queueRecord.Region = Region;
      queueRecord.City = PP0006ORT01;
      queueRecord.LegalAddress = RegistrationAddress;
      queueRecord.PostalAddress = PostalAddress;
      queueRecord.Phones = Phones;
      queueRecord.DepartmentCode = Division;
      queueRecord.JobTitleCode = PP0001PLANS;
      queueRecord.LineManagerCode = PP0001ZZPERNR_MNG;
      queueRecord.EmploymentType = PP0001PERSG;
      queueRecord.PersonnelNumber = PP0000PERNR;
      queueRecord.Email = Email;
      queueRecord.MPGroup = MPGroup;
      queueRecord.TIN = TIN;
      queueRecord.IdentityDocumentSeries = IdentityDocumentSeries;
      queueRecord.IdentityDocumentNumber = IdentityDocumentNumber;
      queueRecord.IdentityDocumentIssuedBy = IdentityDocumentIssuedBy;
      queueRecord.IdentityDocumentIssuerID = IdentityDocumentIssuerID?.Replace("-", string.Empty);
      queueRecord.IdentityDocumentBirthPlace = IdentityDocumentBirthPlace;
      
      var validationErrors = string.Empty;
      
      // Проверка периода действия записи.
      var error = Functions.CompanyQueueBase.SetRecordDatePeriod(queueRecord, PP0000BEGDA, PP0000ENDDA);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка СНИЛС.
      error = Functions.EmployeeQueue.CheckInila(queueRecord);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1} СНИЛС: {2}.", PP0000PERNR, error, queueRecord.INILA);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка кода вида занятости сотрудника.
      if (!Functions.EmployeeQueue.CheckEmployemntType(queueRecord.EmploymentType))
      {
        error = DirRX.NNIntegration.Resources.EmployeeEmployemntTypeParseErrorFormat(queueRecord.EmploymentType);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка кода пола персоны.
      if (!Functions.EmployeeQueue.TryParsePersonSex(queueRecord, PP0002GESCH))
      {
        error = DirRX.NNIntegration.Resources.SexParseErrorFormat(PP0002GESCH);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка Группы МР.
      error = Functions.EmployeeQueue.CheckMPGroup(queueRecord);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      //Проверка признака НГД.
      if (!Functions.EmployeeQueue.TrySetNGD(queueRecord, PP0001VDSK1))
      {
        error = DirRX.NNIntegration.Resources.NGDParseErrorFormat(PP0001VDSK1);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка вида документа, удостоверяющего личность.
      if (!string.IsNullOrEmpty(IdentityDocumentKind) && !Functions.EmployeeQueue.TryParseIdentityDocumentKind(queueRecord, IdentityDocumentKind))
      {
        error = DirRX.NNIntegration.Resources.IdentityDocumentKindParseErrorFormat(IdentityDocumentKind);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      var culture = new System.Globalization.CultureInfo("ru-RU");
      if (!string.IsNullOrEmpty(PP0002GBDAT))
        queueRecord.DateOfBirth = DateTime.Parse(PP0002GBDAT, culture);
      if (!string.IsNullOrEmpty(IdentityDocumentIssueDate))
        queueRecord.IdentityDocumentIssueDate = DateTime.Parse(IdentityDocumentIssueDate, culture);
      if (!string.IsNullOrEmpty(IdentityDocumentExpirationDate))
        queueRecord.IdentityDocumentExpirationDate = DateTime.Parse(IdentityDocumentExpirationDate, culture);
      
      // Проверка формата часового пояса.
      var utcOffset = 0.0;
      if (!double.TryParse(UtcOffset, out utcOffset))
      {
        error = DirRX.NNIntegration.Resources.UtcOffsetParseErrorFormat(UtcOffset);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      else
        queueRecord.UtcOffset = utcOffset;
      
      // Вернуть ошибки валидации запроса в КСИП.
      if (!string.IsNullOrEmpty(validationErrors))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. Таб. номер сотрудника: {0}. Запись справочника \"Очередь сообщений справочника Сотрудники\" не создана из-за ошибок валидации.",
                           queueRecord.PersonnelNumber);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, validationErrors);
      }
      
      try
      {
        queueRecord.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. Таб. номер сотрудника: {0}. Ошибка при сохранении записи справочника \"Очередь сообщений справочника Сотрудники\". Текст ошибки: {1}, StackTrace: {2}.",
                           queueRecord.PersonnelNumber, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, ex.Message);
      }
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
    }
    
    /// <summary>
    /// Создать запись справочника "Очередь сообщений справочника Сотрудники".
    /// </summary>
    /// <param name="PP0002NACHN">Фамилия.</param>
    /// <param name="PP0002VORNA">Имя.</param>
    /// <param name="PP0002MIDNM">Отчество.</param>
    /// <param name="PP0002GBDAT">Дата рождения.</param>
    /// <param name="PP0290ICCD">СНИЛС.</param>
    /// <param name="PP0002GESCH">Пол.</param>
    /// <param name="Region">Регион.</param>
    /// <param name="PP0006ORT01">Населенный пункт.</param>
    /// <param name="RegistrationAddress">Адрес регистрации.</param>
    /// <param name="PostalAddress">Почтовый адрес.</param>
    /// <param name="Phones">Телефоны.</param>
    /// <param name="Division">Подразделение.</param>
    /// <param name="PP0001PLANS">Должность.</param>
    /// <param name="PP0001ZZPERNR_MNG">Линейный руководитель.</param>
    /// <param name="PP0001PERSG">Вид занятости.</param>
    /// <param name="PP0000PERNR">Табельный номер.</param>
    /// <param name="Email">Эл. почта (рабочая).</param>
    /// <param name="PP0000BEGDA">Дата начала действия записи.</param>
    /// <param name="PP0000ENDDA">Дата окончания действия записи.</param>
    /// <param name="MPGroup">Группа МР.</param>
    /// <param name="PP0001VDSK1">НГД.</param>
    /// <param name="TIN">ИНН.</param>
    /// <param name="IdentityDocumentKind">Вид документа.</param>
    /// <param name="IdentityDocumentSeries">Серия.</param>
    /// <param name="IdentityDocumentNumber">Номер.</param>
    /// <param name="IdentityDocumentIssueDate">Дата выдачи.</param>
    /// <param name="IdentityDocumentIssuedBy">Кем выдан.</param>
    /// <param name="IdentityDocumentIssuerID">Код подразделения.</param>
    /// <param name="IdentityDocumentBirthPlace">Место рождения.</param>
    /// <param name="IdentityDocumentExpirationDate">Срок действия.</param>
    /// <param name="UtcOffset">Часовой пояс.</param>
    /// <param name="WERKS">Код раздела персонала..</param>
    /// <param name="BTRTL">Код подраздела персонала.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateEmployeeQueueRecordV3(string PP0002NACHN, string PP0002VORNA, string PP0002MIDNM, string PP0002GBDAT, string PP0290ICCD, string PP0002GESCH, string Region,
                                                                                string PP0006ORT01, string RegistrationAddress, string PostalAddress, string Phones, string Division, string PP0001PLANS, string PP0001ZZPERNR_MNG,
                                                                                string PP0001PERSG, string PP0000PERNR, string Email, string PP0000BEGDA, string PP0000ENDDA,
                                                                                string MPGroup, string PP0001VDSK1,
                                                                                string TIN, string IdentityDocumentKind, string IdentityDocumentSeries, string IdentityDocumentNumber, string IdentityDocumentIssueDate,
                                                                                string IdentityDocumentIssuedBy, string IdentityDocumentIssuerID, string IdentityDocumentBirthPlace, string IdentityDocumentExpirationDate,
                                                                                string UtcOffset, string WERKS, string BTRTL,
                                                                                string InfoManager, string ManagerLevel2, string VSPManager, string SPManager, string PERSK)
    {
      Logger.DebugFormat("CreateEmployeeQueueRecord. Табельный номер: {0}. Получен запрос на создание записи очереди справочника Сотрудники.", PP0000PERNR);
      var queueRecord = EmployeeQueues.Create();
      queueRecord.LastName = PP0002NACHN;
      queueRecord.FirstName = PP0002VORNA;
      queueRecord.MiddleName = PP0002MIDNM;
      queueRecord.INILA = PP0290ICCD;
      queueRecord.Region = Region;
      queueRecord.City = PP0006ORT01;
      queueRecord.LegalAddress = RegistrationAddress;
      queueRecord.PostalAddress = PostalAddress;
      queueRecord.Phones = Phones;
      queueRecord.DepartmentCode = Division;
      queueRecord.JobTitleCode = PP0001PLANS;
      queueRecord.LineManagerCode = PP0001ZZPERNR_MNG;
      queueRecord.EmploymentType = PP0001PERSG;
      queueRecord.PersonnelNumber = PP0000PERNR;
      queueRecord.PersonnelSectionCode = WERKS;
      queueRecord.PersonnelSubSectionCode = BTRTL;
      queueRecord.Email = Email;
      queueRecord.MPGroup = MPGroup;
      queueRecord.TIN = TIN;
      queueRecord.IdentityDocumentSeries = IdentityDocumentSeries;
      queueRecord.IdentityDocumentNumber = IdentityDocumentNumber;
      queueRecord.IdentityDocumentIssuedBy = IdentityDocumentIssuedBy;
      queueRecord.IdentityDocumentIssuerID = Regex.Replace(IdentityDocumentIssuerID ?? string.Empty, @"[^\d]", string.Empty);
      queueRecord.IdentityDocumentBirthPlace = IdentityDocumentBirthPlace;
      queueRecord.InfoManager = InfoManager;
      queueRecord.ManagerLevel2 = ManagerLevel2;
      queueRecord.VSPManager = VSPManager;
      queueRecord.SPManager = SPManager;
      queueRecord.PersCategory = PERSK;
      
      var validationErrors = string.Empty;
      
      // Проверка периода действия записи.
      var error = Functions.CompanyQueueBase.SetRecordDatePeriod(queueRecord, PP0000BEGDA, PP0000ENDDA);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка СНИЛС.
      error = Functions.EmployeeQueue.CheckInila(queueRecord);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1} СНИЛС: {2}.", PP0000PERNR, error, queueRecord.INILA);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка кода вида занятости сотрудника.
      if (!Functions.EmployeeQueue.CheckEmployemntType(queueRecord.EmploymentType))
      {
        error = DirRX.NNIntegration.Resources.EmployeeEmployemntTypeParseErrorFormat(queueRecord.EmploymentType);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка кода пола персоны.
      if (!Functions.EmployeeQueue.TryParsePersonSex(queueRecord, PP0002GESCH))
      {
        error = DirRX.NNIntegration.Resources.SexParseErrorFormat(PP0002GESCH);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка Группы МР.
      error = Functions.EmployeeQueue.CheckMPGroup(queueRecord);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      //Проверка признака НГД.
      if (!Functions.EmployeeQueue.TrySetNGD(queueRecord, PP0001VDSK1))
      {
        error = DirRX.NNIntegration.Resources.NGDParseErrorFormat(PP0001VDSK1);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка вида документа, удостоверяющего личность.
      if (!string.IsNullOrEmpty(IdentityDocumentKind) && !Functions.EmployeeQueue.TryParseIdentityDocumentKind(queueRecord, IdentityDocumentKind))
      {
        error = DirRX.NNIntegration.Resources.IdentityDocumentKindParseErrorFormat(IdentityDocumentKind);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка раздела персонала.
      if (string.IsNullOrEmpty(queueRecord.PersonnelSectionCode))
      {
        error = DirRX.NNIntegration.EmployeeQueues.Resources.PersonnelSectionIsEmptyError;
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Проверка подраздела персонала.
      if (string.IsNullOrEmpty(queueRecord.PersonnelSubSectionCode))
      {
        error = DirRX.NNIntegration.EmployeeQueues.Resources.PersonnelSubSectionIsEmptyError;
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      var culture = new System.Globalization.CultureInfo("ru-RU");
      if (!string.IsNullOrEmpty(PP0002GBDAT))
        queueRecord.DateOfBirth = DateTime.Parse(PP0002GBDAT, culture);
      if (!string.IsNullOrEmpty(IdentityDocumentIssueDate))
        queueRecord.IdentityDocumentIssueDate = DateTime.Parse(IdentityDocumentIssueDate, culture);
      if (!string.IsNullOrEmpty(IdentityDocumentExpirationDate))
        queueRecord.IdentityDocumentExpirationDate = DateTime.Parse(IdentityDocumentExpirationDate, culture);
      
      // Проверка формата часового пояса.
      var utcOffset = 0.0;
      if (!double.TryParse(UtcOffset, out utcOffset))
      {
        error = DirRX.NNIntegration.Resources.UtcOffsetParseErrorFormat(UtcOffset);
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PP0000PERNR, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      else
        queueRecord.UtcOffset = utcOffset;
      
      // Проверка категории сотрудника.
      error = Functions.EmployeeQueue.CheckEmployeeCategory(queueRecord);
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. SAP ID: {0}. Ошибка валидации запроса: {1}", PERSK, error);
        validationErrors = string.Join(" ", error, validationErrors);
      }
      
      // Вернуть ошибки валидации запроса в КСИП.
      if (!string.IsNullOrEmpty(validationErrors))
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. Таб. номер сотрудника: {0}. Запись справочника \"Очередь сообщений справочника Сотрудники\" не создана из-за ошибок валидации.",
                           queueRecord.PersonnelNumber);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, validationErrors);
      }
      
      try
      {
        queueRecord.Save();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CreateEmployeeQueueRecord. Таб. номер сотрудника: {0}. Ошибка при сохранении записи справочника \"Очередь сообщений справочника Сотрудники\". Текст ошибки: {1}, StackTrace: {2}.",
                           queueRecord.PersonnelNumber, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, ex.Message);
      }
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
    }
    #endregion
    
    #region Функции синхронизации справочников орг. структуры.
    
    /// <summary>
    /// Обработка устаревших записей очередей орг. структуры.
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    /// <returns>Устаревшие записи.</returns>
    public IQueryable<ICompanyQueueBase> CompanyQueueOldRecordsProcessing(string logPrefix)
    {
      var oldRecords = Functions.Module.GetOldCompanyQueueRecords();
      Logger.DebugFormat("{0}. Всего устаревших записей: {1}.", logPrefix, oldRecords.Count());
      foreach (var record in oldRecords)
      {
        Logger.DebugFormat("{0}. ID записи очереди: {1}. Старт обработки устаревшей записи.", logPrefix, record.Id);
        var isLocked = Locks.TryLock(record);
        if (isLocked)
        {
          record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Old;
          
          if (record.LastProcessingDate != Calendar.Now)
            record.LastProcessingDate = Calendar.Now;
          
          if (!Functions.Module.CompanyQueueRecordSave(record, logPrefix))
            Logger.ErrorFormat("{0}. {1}. ID записи очереди: {2}. Cтатус записи не был изменён на \"{3}\" из-за ошибки сохранения.",
                               logPrefix, DepartmentQueues.Info.LocalizedName, record.Id,
                               CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
          Locks.Unlock(record);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(record);
          Logger.DebugFormat("{0}. ID записи очереди: {1}. Не удалось установить блокировку. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.", logPrefix, record.Id,
                             lockInfo.OwnerName, lockInfo.LockTime);
        }
      }
      return oldRecords;
    }
    
    /// <summary>
    /// Обработка очереди справочника Подразделения.
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    /// <param name="oldRecords">Устаревшие записи.</param>
    /// <param name="errors">Ошибки обработки.</param>
    public void DepartmentQueueProcessing(string logPrefix, IQueryable<ICompanyQueueBase> oldRecords, List<ICompanyQueueBase> errors)
    {
      Logger.DebugFormat("{0}. {1}. Начало обработки очереди.", logPrefix, DepartmentQueues.Info.LocalizedName);
      
      // Поиск записей очереди справочника Подразделения.
      var departmentQueueRecords = DepartmentQueues.GetAll(x => !oldRecords.Any(o => CompanyQueueBases.Equals(o, x)))
        .Where(x =>
               x.ProcessingStatus == DirRX.NNIntegration.DepartmentQueue.ProcessingStatus.Recent ||
               x.ProcessingStatus == DirRX.NNIntegration.DepartmentQueue.ProcessingStatus.Retry)
        .OrderBy(x => x.ProcessingStatus);
      
      Logger.DebugFormat("{0}. {1}. Начало обработки. Всего новых записей: {2}. Всего записей на повторной обработке: {3}.", logPrefix, DepartmentQueues.Info.LocalizedName,
                         departmentQueueRecords.Where(x => x.ProcessingStatus == DirRX.NNIntegration.JobTitleQueue.ProcessingStatus.Recent).Count(),
                         departmentQueueRecords.Where(x => x.ProcessingStatus == DirRX.NNIntegration.JobTitleQueue.ProcessingStatus.Retry).Count());
      
      // Обработка записей очереди справочника Подразделения.
      foreach (var record in departmentQueueRecords)
      {
        Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Начало обработки записи очереди. Текущий статус обработки: {4}.", logPrefix, DepartmentQueues.Info.LocalizedName,
                           record.Id, record.SapId, CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
        
        var jobState = NNIntegration.Structures.Module.CompanySynchJobState.Create();
        
        // Блокировка записи очереди.
        var isRecordLocked = Locks.TryLock(record);
        if (!isRecordLocked)
        {
          var lockInfo = Locks.GetLockInfo(record);
          Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {4}. Время установки блокировки: {5}. Обработка повторится при следующем запуске фонового процесса.", logPrefix,
                             DepartmentQueues.Info.LocalizedName, record.Id, record.SapId, lockInfo.OwnerName, lockInfo.LockTime);
          continue;
        }
        
        // Зафиксировать дату обработки записи.
        if (record.LastProcessingDate != Calendar.Now)
          record.LastProcessingDate = Calendar.Now;
        
        // Если SAP ID подразделения равен SAP ID ОЕ любой НОР, то не создаём подразделение в системе.
        var isBusinessUnit = Functions.Module.CheckBusinessUnitOUSapId(record.SapId);
        if (isBusinessUnit)
        {
          Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Подразделение не будет создано, т.к. оно соответсвует НОР в SAP.", logPrefix,
                             DepartmentQueues.Info.LocalizedName, record.Id, record.SapId);
        }
        else
        {
          // Проверка существования НОР.
          var businessUnit = Functions.Module.GetBusinessUnitBySAPId(record.BusinessUnit);
          if (businessUnit == null)
          {
            jobState.ErrorMessage = DirRX.NNIntegration.Resources.BusinessUnitNotFoundErrorFormat(record.BusinessUnit);
            Logger.ErrorFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Не найдена запись справочника \"Наши организации\" для подразделения. SAP ID НОР: {4}.",
                               logPrefix, DepartmentQueues.Info.LocalizedName, record.Id, record.SapId, record.BusinessUnit);
          }
          
          #region Создание/обновление подразделения.
          
          var executionResult = false;
          var department = Departments.Null;
          
          // Поиск/создание подразделения.
          if (string.IsNullOrEmpty(jobState.ErrorMessage))
          {
            executionResult =
              Transactions.Execute(() =>
                                   {
                                     department = record.Department ?? Functions.Module.GetDepartmentBySAPId(record.SapId) ?? Departments.Create();
                                     Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. ID подразделения: {4}. Создание/обновление подразделения.", logPrefix, DepartmentQueues.Info.LocalizedName,
                                                        record.Id, record.SapId, department.Id);
                                     
                                     // Блокировка подразделения.
                                     var entityIsLocked = Locks.TryLock(department);
                                     if (!entityIsLocked)
                                     {
                                       var lockInfo = Locks.GetLockInfo(department);
                                       record.ProcessingStatus = NNIntegration.CompanyQueueBase.ProcessingStatus.Retry;
                                       jobState.RetryMessage = string.Format("ID подразделения: {0}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}",
                                                                             department.Id, lockInfo.OwnerName, lockInfo.LockTime);
                                     }
                                     
                                     try
                                     {
                                       // Обновление полей подразделения.
                                       if (string.IsNullOrEmpty(jobState.RetryMessage))
                                       {
                                         if (department.State.IsInserted || department.Name != record.Name)
                                           department.Name = record.Name;
                                         if (department.State.IsInserted || department.ShortName != record.ShortName)
                                           department.ShortName = record.ShortName;
                                         if (department.State.IsInserted || department.IsHeadquartersDirRX != record.IsHeadquarters)
                                           department.IsHeadquartersDirRX = record.IsHeadquarters;
                                         if (department.State.IsInserted || !BusinessUnits.Equals(department.BusinessUnit, businessUnit))
                                           department.BusinessUnit = businessUnit;
                                         if (department.State.IsInserted || department.RecordStartDateDirRX != record.RecordStartDate)
                                           department.RecordStartDateDirRX = record.RecordStartDate;
                                         if (department.State.IsInserted || department.RecordExpirationDateDirRX != record.RecordEndDate)
                                           department.RecordExpirationDateDirRX = record.RecordEndDate;
                                         if (string.IsNullOrEmpty(department.SapIdDirRX))
                                           department.SapIdDirRX = record.SapId;
                                         
                                         var status = GetStatusByRecordDate(record);
                                         if (department.Status != status)
                                           department.Status = status;
                                         
                                         #region Изменение руководителя подразделения.
                                         
                                         // Всего две причины для изменения поля в справочнике.
                                         // Первый случай. Пришёл не пустой SAP ID должности руководителя.
                                         if (!string.IsNullOrEmpty(record.Manager))
                                         {
                                           var prefix = string.Format("{0}. {1}. ID записи: {2}. SAP ID: {3}. ID подразделения: {4}. SAP ID должности руководителя: {5}.", logPrefix, DepartmentQueues.Info.LocalizedName, record.Id, record.SapId, department.Id, department.ManagerJobTitleSapIdDirRX);
                                           if (department?.ManagerJobTitleSapIdDirRX != record.Manager)
                                           {
                                             Logger.DebugFormat("{0}. Изменилось поле SAP ID должности руководителя.", prefix);
                                             department.ManagerJobTitleSapIdDirRX = record.Manager;
                                           }
                                           else
                                             Logger.DebugFormat("{0}. Поле SAP ID должности руководителя не изменилось.", prefix);
                                           
                                           // Если за должностью есть закреплённые сотрудники, то выполнить поиск руководителя.
                                           var isEmployeeExists = CheckJobTitleEmployeeExist(record.Manager);
                                           if (isEmployeeExists == true)
                                           {
                                             Logger.DebugFormat("{0}. За должностью есть закреплённые сотрудники в SAP.", prefix);
                                             
                                             // Найти и заполнить руководителя. Если руководитель - не найден, то запускается повторная обработка.
                                             SetDepartmentManager(prefix, department, jobState);
                                           }
                                           // Если за должностью нет закреплённых сотрудников, то при необходимости очистить руководителя у подразделения.
                                           else if (isEmployeeExists == false)
                                           {
                                             Logger.DebugFormat("{0}. Нет закреплённых сотрудников за должностью.", prefix);
                                             if (department.Manager != null)
                                               department.Manager = Employees.Null;
                                           }
                                           // Если не найдена должность руководителя по SAP ID, то отправить запись на повторную обработку.
                                           else if (isEmployeeExists == null)
                                             jobState.RetryMessage = string.Format("Не найдена должность руководителя по SAP ID: {0}.", record.Manager);
                                         }
                                         // Второй случай. Пришёл пустой SAP ID должности руководителя.
                                         else if (string.IsNullOrEmpty(record.Manager))
                                         {
                                           if (!string.IsNullOrEmpty(department?.ManagerJobTitleSapIdDirRX))
                                             department.ManagerJobTitleSapIdDirRX = string.Empty;
                                           
                                           if (department?.Manager != null)
                                             department.Manager = Employees.Null;
                                         }
                                         
                                         #endregion
                                         
                                         #region Изменение головного подразделения.
                                         // Всего две причины для изменения поля в справочнике.
                                         // Первый случай. Пришёл не пустой SAP ID головного подразделения, и он не совпадает с указанным в записи SAP ID головного подразделения.
                                         if (!string.IsNullOrEmpty(record.HeadOffice) && Departments.As(department.HeadOffice)?.SapIdDirRX != record.HeadOffice)
                                         {
                                           // Если SAP ID головного подразделения соответствует SAP ID ОЕ любой НОР, то у подразделения не заполняется головное подразделение.
                                           var headOfficeIsBU = Functions.Module.CheckBusinessUnitOUSapId(record.HeadOffice);
                                           if (headOfficeIsBU)
                                           {
                                             Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. ID подразделения: {4}. Головное подразделение соответствует НОР с SAP ID ОЕ: {5}.",
                                                                logPrefix, DepartmentQueues.Info.LocalizedName, record.Id, record.SapId, department.Id, record.HeadOffice);
                                             
                                             if (department.HeadOffice != null)
                                               department.HeadOffice = Departments.Null;
                                           }
                                           else
                                           {
                                             // Поиск подразделения.
                                             var headOffice = Functions.Module.GetDepartmentBySAPId(record.HeadOffice);
                                             
                                             // Если не найдено головное подразделение, то отправляем запись подразделения на повторную обработку.
                                             if (headOffice == null)
                                               jobState.RetryMessage = string.Format("ID подразделения: {0}. Не найдено головное подразделение: {1}", department.Id, record.HeadOffice);
                                             else
                                               department.HeadOffice = headOffice;
                                           }
                                         }
                                         // Второй случай. Пришёл пустой SAP ID, и в подразделении уже заполнено головное подразделение.
                                         else if (string.IsNullOrEmpty(record.HeadOffice) && department.HeadOffice != null)
                                           department.HeadOffice = Departments.Null;
                                         #endregion
                                         
                                         // Сохранение подразделения.
                                         var changesSaved = department.State.IsInserted == false && department.State.IsChanged == false;
                                         if (!changesSaved)
                                         {
                                           department.Save();
                                           changesSaved = true;
                                           Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. ID подразделения: {4}. Запись справочника Подразделения успешно сохранена.",
                                                              logPrefix, DepartmentQueues.Info.LocalizedName, record.Id, record.SapId, department.Id);
                                         }
                                       }
                                     }
                                     catch (Exception ex)
                                     {
                                       jobState.ErrorMessage = DirRX.NNIntegration.Resources.CompanySynchronizationDepartmentNotSavedErrorFormat(ex.Message);
                                       Logger.ErrorFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. ID подразделения: {4}. Не удалось сохранить запись справочника Подразделения. Ошибка: {5}. StackTrace: {6}.",
                                                          logPrefix, DepartmentQueues.Info.LocalizedName, record.Id, record.SapId, department.Id, ex.Message, ex.StackTrace);
                                       throw ex;
                                     }
                                     finally
                                     {
                                       Locks.Unlock(department);
                                     }
                                   });
          }
          
          #endregion
          
          #region Обработка результатов сохранения.
          // Если транзакция завершилась с ошибкой, то выполняем поиск подразделения.
          if (!executionResult || jobState.ResetChanges || (department != null && department.State.IsInserted))
            record.Department = department == null || department.State.IsInserted ? Departments.Null : Functions.Module.GetDepartmentBySAPId(record.SapId);
          else if (!Departments.Equals(record.Department, department) && department != null && !department.State.IsInserted)
            record.Department = department;
        } // if (businessUnit == null)
        
        // Если во время обработки возникла ошибока, то добавить запись в список ошибок и установить статус - Ошибка обработки.
        if (!string.IsNullOrEmpty(jobState.ErrorMessage))
        {
          record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Error;
          record.ErrorMessage = jobState.ErrorMessage;
          errors.Add(record);
        }
        // Если требуется повторная обработка, то установить статус - Повторная обработка.
        else if (!string.IsNullOrEmpty(jobState.RetryMessage))
        {
          if (record.ProcessingStatus != DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Retry)
            record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Retry;
          if (record.RetryReason != jobState.RetryMessage)
            record.RetryReason = jobState.RetryMessage;
          
          Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. {4}. Обработка повторится при следующем запуске фонового процесса.", logPrefix, DepartmentQueues.Info.LocalizedName, record.Id, record.SapId, jobState.RetryMessage);
        }
        // Иначе установить статус - Обработано.
        else
          record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Processed;
        #endregion
        
        // Сохранение записи очереди.
        record.RetryReason = jobState.RetryMessage;
        if (!Functions.Module.CompanyQueueRecordSave(record, logPrefix))
          Logger.ErrorFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Cтатус записи не был изменён на \"{4}\" из-за ошибки сохранения.",
                             logPrefix, DepartmentQueues.Info.LocalizedName, record.Id, record.SapId,
                             CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
        Locks.Unlock(record);
        Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Наименование: {4}. Статус обработки: {5}. Конец обработки записи очереди.", logPrefix, DepartmentQueues.Info.LocalizedName,
                           record.Id, record.SapId, record.RecordName, CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
      }
      
      Logger.DebugFormat("{0}. {1}. На повторную обработку отправлено {2} записей.", logPrefix, DepartmentQueues.Info.LocalizedName,
                         departmentQueueRecords.Where(x => x.ProcessingStatus == DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Retry).Count());
      
      Logger.DebugFormat("{0}. {1}. Конец обработки очереди.", logPrefix, DepartmentQueues.Info.LocalizedName);
    }
    
    /// <summary>
    /// Обработка очереди справочника Должности.
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    /// <param name="oldRecords">Устаревшие записи.</param>
    /// <param name="errors">Ошибки обработки.</param>
    public void JobTitleQueueProcessing(string logPrefix, IQueryable<ICompanyQueueBase> oldRecords, List<ICompanyQueueBase> errors)
    {
      Logger.DebugFormat("{0}. {1}. Начало обработки очереди.", logPrefix, JobTitleQueues.Info.LocalizedName);
      
      // Поиск записей очереди справочника Должности.
      var jobTitleQueueRecords = JobTitleQueues.GetAll(x => !oldRecords.Any(o => CompanyQueueBases.Equals(o, x)))
        .Where(x => x.ProcessingStatus == DirRX.NNIntegration.JobTitleQueue.ProcessingStatus.Recent ||
               x.ProcessingStatus == DirRX.NNIntegration.JobTitleQueue.ProcessingStatus.Retry)
        .OrderBy(x => x.ProcessingStatus);
      
      Logger.DebugFormat("{0}. {1}. Начало обработки. Всего новых записей: {2}. Всего записей на повторной обработке: {3}.", logPrefix, JobTitleQueues.Info.LocalizedName,
                         jobTitleQueueRecords.Where(x => x.ProcessingStatus == DirRX.NNIntegration.JobTitleQueue.ProcessingStatus.Recent).Count(),
                         jobTitleQueueRecords.Where(x => x.ProcessingStatus == DirRX.NNIntegration.JobTitleQueue.ProcessingStatus.Retry).Count());
      
      // Обработка записей очереди справочника Должности.
      foreach (var record in jobTitleQueueRecords)
      {
        Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Начало обработки записи очереди. Текущий статус обработки: {4}.", logPrefix, JobTitleQueues.Info.LocalizedName,
                           record.Id, record.SapId, CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
        
        var jobState = NNIntegration.Structures.Module.CompanySynchJobState.Create();
        
        // Блокировка записи очереди справочника.
        var isRecordLocked = Locks.TryLock(record);
        if (!isRecordLocked)
        {
          var lockInfo = Locks.GetLockInfo(record);
          Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {4}. Время установки блокировки: {5}. Обработка повторится при следующем запуске фонового процесса.", logPrefix,
                             JobTitleQueues.Info.LocalizedName, record.Id, record.SapId, lockInfo.OwnerName, lockInfo.LockTime);
          continue;
        }

        // Зафиксировать дату обработки записи.
        if (record.LastProcessingDate != Calendar.Now)
          record.LastProcessingDate = Calendar.Now;
        
        #region Создание/обновление должности.
        
        var executionResult = false;
        var jobTitle = JobTitles.Null;
        
        executionResult =
          Transactions.Execute(() =>
                               {
                                 
                                 // Поиск/создание должности.
                                 jobTitle = record.JobTitle ?? Functions.Module.GetJobTitleBySAPId(record.SapId) ?? JobTitles.Create();
                                 Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. ID должности: {4}. Создание/обновление должности.", logPrefix, JobTitleQueues.Info.LocalizedName,
                                                    record.Id, record.SapId, jobTitle.Id);
                                 
                                 // Блокировка должности.
                                 var entityIsLocked = Locks.TryLock(jobTitle);
                                 if (!entityIsLocked)
                                 {
                                   var lockInfo = Locks.GetLockInfo(jobTitle);
                                   jobState.RetryMessage = string.Format("ID должности: {0}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}",
                                                                         jobTitle.Id, lockInfo.OwnerName, lockInfo.LockTime);
                                 }
                                 
                                 try
                                 {
                                   // Обновление полей должности.
                                   if (string.IsNullOrEmpty(jobState.RetryMessage))
                                   {
                                     if (jobTitle.State.IsInserted || jobTitle.Name != record.Name)
                                       jobTitle.Name = record.Name;
                                     if (jobTitle.State.IsInserted || jobTitle.RecordStartDateDirRX != record.RecordStartDate)
                                       jobTitle.RecordStartDateDirRX = record.RecordStartDate;
                                     if (jobTitle.State.IsInserted || jobTitle.RecordExpirationDateDirRX != record.RecordEndDate)
                                       jobTitle.RecordExpirationDateDirRX = record.RecordEndDate;
                                     
                                     // Свойство устарело, теперь используется свойство IsTopDirRX.
                                     if (jobTitle.State.IsInserted || jobTitle.EvaluationGroupDirRX != record.EvaluationGroup)
                                       jobTitle.EvaluationGroupDirRX = record.EvaluationGroup;
                                     
                                     if (string.IsNullOrEmpty(jobTitle.SapIdDirRX))
                                       jobTitle.SapIdDirRX = record.SapId;
                                     if (jobTitle.State.IsInserted || jobTitle.IsTopDirRX != record.IsTop)
                                       jobTitle.IsTopDirRX = record.IsTop;
                                     
                                     var positionLevel = OverrideSolution.PublicFunctions.JobTitle.GetJobTitlePositionLevel(jobTitle, record.PositionLevel);
                                     if (jobTitle.State.IsInserted || !jobTitle.PositionLevelDirRX.Equals(positionLevel))
                                       jobTitle.PositionLevelDirRX = positionLevel;
                                     
                                     #region Изменение признака наличия сотрудников на должности.
                                     // Если изменился признак или запись попала в повторную обработку.
                                     if (record.ProcessingStatus == NNIntegration.JobTitleQueue.ProcessingStatus.Retry || jobTitle.IsEmployeeExistsDirRX != record.IsEmployeeExists)
                                     {
                                       var prefix = string.Format("{0}. {1}. ID записи: {2}. SAP ID: {3}. ID должности: {4}", logPrefix, JobTitleQueues.Info.LocalizedName, record.Id, record.SapId, jobTitle.Id);
                                       if (jobTitle.IsEmployeeExistsDirRX != record.IsEmployeeExists)
                                       {
                                         Logger.DebugFormat("{0}. Изменился признак наличия сотрудников на должности. Значение: {1}.", prefix, record.IsEmployeeExists);
                                         jobTitle.IsEmployeeExistsDirRX = record.IsEmployeeExists;
                                       }
                                       
                                       var department = GetDepartmentByManagerJobTitleSAPId(jobTitle.SapIdDirRX);
                                       
                                       // Если подразделение, к которому привязана эта должность руководителя найдено, то необходимо выполнить обработку руководителя.
                                       if (department != null)
                                       {
                                         Logger.DebugFormat("{0}. Должность является руководящей для подразделения с SAP ID: {1}.", prefix, department.SapIdDirRX);
                                         
                                         // Блокировка подразделения.
                                         var departmentIsLocked = Locks.TryLock(department);
                                         if (departmentIsLocked)
                                         {
                                           try
                                           {
                                             // Первый случай. Изменение признака на значение true.
                                             if (record.IsEmployeeExists == true)
                                             {
                                               Logger.DebugFormat("{0}. На должности теперь есть сотрудники в SAP. Необходимо заполнить руководителя в подразделении с SAP ID: {1}.", prefix, department.SapIdDirRX);
                                               
                                               // Найти и заполнить руководителя. Если руководитель - не найден, то запускается повторная обработка.
                                               SetDepartmentManager(prefix, department, jobState);
                                             }
                                             // Второй случай. Изменение признака на значение false.
                                             else if (record.IsEmployeeExists == false)
                                             {
                                               Logger.DebugFormat("{0}. На должности больше нет сотрудников в SAP. Необходимо очистить руководителя подразделения c SAP ID: {1}.", prefix, department.SapIdDirRX);
                                               department.Manager = Employees.Null;
                                             }
                                             
                                             department.Save();
                                           }
                                           catch (Exception ex)
                                           {
                                             jobState.ErrorMessage = DirRX.NNIntegration.Resources.CompanySynchronizationJobTitleNotSavedErrorFormat(ex.Message);
                                             Logger.ErrorFormat("{0}. SAP ID подразделения: {1}. Не удалось сохранить подразделение при обработке его руководителя. Ошибка: {2}. StackTrace: {3}.",
                                                                prefix, department.SapIdDirRX, ex.Message, ex.StackTrace);
                                             throw ex;
                                           }
                                           finally
                                           {
                                             Locks.Unlock(department);
                                           }
                                         }
                                         else
                                         {
                                           var lockInfo = Locks.GetLockInfo(department);
                                           jobState.RetryMessage = string.Format("ID подразделения: {0}. Не удалось установить блокировку на подразделение. Запись заблокирована пользователем: {1}. Время установки блокировки: {2}",
                                                                                 department.Id, lockInfo.OwnerName, lockInfo.LockTime);
                                         }
                                       }
                                       else
                                         Logger.DebugFormat("{0}. Должность не является руководящей ни для одного подразделения.", prefix);
                                     }
                                     #endregion
                                     
                                     var status = GetStatusByRecordDate(record);
                                     if (status != jobTitle.Status)
                                       jobTitle.Status = status;
                                     
                                     #region Изменение подразделения.
                                     // Всего две причины для изменения поля в справочнике.
                                     // Первый случай. Пришёл не пустой SAP ID подразделения, и он не совпадает с указанным у текущей должности.
                                     if (!string.IsNullOrEmpty(record.DepartmentCode) && Departments.As(jobTitle.Department)?.SapIdDirRX != record.DepartmentCode)
                                     {
                                       // Поиск подразделения.
                                       var department = Functions.Module.GetDepartmentBySAPId(record.DepartmentCode);
                                       
                                       // Если не найдено подразделение, то отправляем запись должности на повторную обработку.
                                       if (department == null)
                                         jobState.RetryMessage = string.Format("ID должности: {0}. Не найдено подразделение: {1}", jobTitle.Id, record.DepartmentCode);
                                       else
                                         jobTitle.Department = department;
                                     }
                                     // Второй случай. Пришёл пустой SAP ID, и в должносте уже заполнено подразделение.
                                     else if (string.IsNullOrEmpty(record.DepartmentCode) && jobTitle.Department != null)
                                       jobTitle.Department = Departments.Null;
                                     #endregion
                                     
                                     // Сохранение записи должности.
                                     var changesSaved = jobTitle.State.IsInserted == false && jobTitle.State.IsChanged == false;
                                     if (!changesSaved)
                                     {
                                       jobTitle.Save();
                                       changesSaved = true;
                                       Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. ID должности: {4}. Запись справочника Должности успешно сохранена.",
                                                          logPrefix, JobTitleQueues.Info.LocalizedName, record.Id, record.SapId, jobTitle.Id);
                                     }
                                   }
                                 }
                                 catch (Exception ex)
                                 {
                                   jobState.ErrorMessage = DirRX.NNIntegration.Resources.CompanySynchronizationJobTitleNotSavedErrorFormat(ex.Message);
                                   Logger.ErrorFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. ID должности: {4}. Не удалось сохранить запись справочника Должности. Ошибка: {5}. StackTrace: {6}.",
                                                      logPrefix, JobTitleQueues.Info.LocalizedName, record.Id, record.SapId, jobTitle.Id, ex.Message, ex.StackTrace);
                                   throw ex;
                                 }
                                 finally
                                 {
                                   Locks.Unlock(jobTitle);
                                 }
                               });
        
        #endregion
        
        #region Обработка результатов сохранения.
        // Если транзакция завершилась с ошибкой, то выполняем поиск должности.
        if (!executionResult || jobState.ResetChanges || (jobTitle != null && jobTitle.State.IsInserted))
          record.JobTitle = jobTitle == null || jobTitle.State.IsInserted ? JobTitles.Null : Functions.Module.GetJobTitleBySAPId(record.SapId);
        else if (!JobTitles.Equals(record.JobTitle, jobTitle) && jobTitle != null && !jobTitle.State.IsInserted)
          record.JobTitle = jobTitle;
        
        // Если во время обработки возникла ошибока, то добавить запись в список ошибок и установить статус - Ошибка обработки.
        if (!string.IsNullOrEmpty(jobState.ErrorMessage))
        {
          record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Error;
          record.ErrorMessage = jobState.ErrorMessage;
          errors.Add(record);
        }
        // Если требуется повторная обработка, то установить статус - Повторная обработка.
        else if (!string.IsNullOrEmpty(jobState.RetryMessage))
        {
          if (record.ProcessingStatus != DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Retry)
            record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Retry;
          if (record.RetryReason != jobState.RetryMessage)
            record.RetryReason = jobState.RetryMessage;
          
          Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. {4}. Обработка повторится при следующем запуске фонового процесса.", logPrefix, JobTitleQueues.Info.LocalizedName, record.Id, record.SapId, jobState.RetryMessage);
        }
        // Иначе установить статус - Обработано.
        else
          record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Processed;
        #endregion
        
        // Сохранение записи очереди.
        if (!Functions.Module.CompanyQueueRecordSave(record, logPrefix))
          Logger.ErrorFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Cтатус записи не был изменён на \"{4}\" из-за ошибки сохранения.",
                             logPrefix, JobTitleQueues.Info.LocalizedName, record.Id, record.SapId,
                             CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
        
        Locks.Unlock(record);
        Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Наименование: {4}. Статус обработки: {5}. Конец обработки записи очереди.", logPrefix, JobTitleQueues.Info.LocalizedName,
                           record.Id, record.SapId, record.RecordName, CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
      }
      
      Logger.DebugFormat("{0}. {1}. На повторную обработку отправлено {2} записей.", logPrefix, JobTitleQueues.Info.LocalizedName,
                         jobTitleQueueRecords.Where(x => x.ProcessingStatus == DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Retry).Count());
      Logger.DebugFormat("{0}. {1}. Конец обработки очереди.", logPrefix, JobTitleQueues.Info.LocalizedName);
    }
    
    /// <summary>
    /// Обработка очереди справочника Сотрудники.
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    /// <param name="oldRecords">Устаревшие записи.</param>
    /// <param name="errors">Ошибки обработки.</param>
    public void EmployeeQueueProcessing(string logPrefix, IQueryable<ICompanyQueueBase> oldRecords, List<ICompanyQueueBase> errors)
    {
      Logger.DebugFormat("{0}. {1}. Начало обработки очереди.", logPrefix, EmployeeQueues.Info.LocalizedName);
      
      // Поиск записей очереди справочника Сотрудники.
      var employeeQueueRecords = EmployeeQueues.GetAll(x => !oldRecords.Any(o => CompanyQueueBases.Equals(o, x)))
        .Where(x => x.ProcessingStatus == DirRX.NNIntegration.EmployeeQueue.ProcessingStatus.Recent ||
               x.ProcessingStatus == DirRX.NNIntegration.EmployeeQueue.ProcessingStatus.Retry)
        .OrderBy(x => x.ProcessingStatus);
      // Получение ответственного из справочника "Потоки внешних систем" по guid потока.
      var responsible = Functions.Module.GetFlowResponsible(Constants.Module.ExternalSystemsFlowGuids.CompanySynchronization);
      
      Logger.DebugFormat("{0}. {1}. Начало обработки. Всего новых записей: {2}. Всего записей на повторной обработке: {3}.", logPrefix, EmployeeQueues.Info.LocalizedName,
                         employeeQueueRecords.Where(x => x.ProcessingStatus == DirRX.NNIntegration.EmployeeQueue.ProcessingStatus.Recent).Count(),
                         employeeQueueRecords.Where(x => x.ProcessingStatus == DirRX.NNIntegration.EmployeeQueue.ProcessingStatus.Retry).Count());
      
      // Обработка записей очереди справочника Сотрудники.
      foreach (var record in employeeQueueRecords)
      {
        Logger.DebugFormat("{0}. {1}. ID записи: {2}. Таб. номер: {3}. Начало обработки записи очереди. Текущий статус обработки: {4}.", logPrefix, EmployeeQueues.Info.LocalizedName,
                           record.Id, record.PersonnelNumber, CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
        
        var jobState = NNIntegration.Structures.Module.CompanySynchJobState.Create();
        
        // Блокировка записи очереди справочника.
        var isRecordLocked = Locks.TryLock(record);
        if (!isRecordLocked)
        {
          var lockInfo = Locks.GetLockInfo(record);
          Logger.DebugFormat("{0}. {1}. ID записи: {2}. Таб. номер: {3}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {4}. Время установки блокировки: {5}. Обработка повторится при следующем запуске фонового процесса.", logPrefix,
                             EmployeeQueues.Info.LocalizedName, record.Id, record.PersonnelNumber, lockInfo.OwnerName, lockInfo.LockTime);
          continue;
        }
        
        // Зафиксировать дату обработки записи
        if (record.LastProcessingDate != Calendar.Now)
          record.LastProcessingDate = Calendar.Now;
        
        var entityIsLocked = false;
        var person = People.Null;
        var personalPhone = record.Phones.Split(',').FirstOrDefault();
        var personalPhoneIsEmpty = string.IsNullOrEmpty(personalPhone);
        var employeeStatus = GetStatusByRecordDate(record);
        var needIssueCertificate = false;
        
        // Если личный телефон - не пустой, то необходимо его провалидировать до обновления персоны и сотрудника.
        if (!personalPhoneIsEmpty && !DirRX.EssPlatform.PublicFunctions.Module.PhoneNumberLengthIsValid(personalPhone, false))
        {
          jobState.ErrorMessage = DirRX.NNIntegration.Resources.PersonalPhoneValidationErrorFormat(personalPhone);
          Logger.ErrorFormat("{0}. {1}. ID записи: {2}. Таб. номер: {3}. {4}",
                             logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, record.PersonnelNumber, jobState.ErrorMessage);
        }
        
        #region Создание/обновление персоны.
        
        var personExecutionResult = false;
        if (string.IsNullOrEmpty(jobState.ErrorMessage))
        {
          personExecutionResult =
            Transactions.Execute(() =>
                                 {
                                   // Поиск или создание персоны.
                                   person = record.Person ?? DirRX.OverrideSolution.PublicFunctions.Person.Remote.GetPersonByINILA(record.INILA) ?? People.Create();
                                   
                                   // Обновляем персону в зависимости от состояния сотрудников по ней.
                                   var employeesByPerson = Employees.GetAll(e => People.Equals(e.Person, person));
                                   if (person.State.IsInserted == true || employeeStatus == Status.Active || !employeesByPerson.Select(e => e.Status).Any(s => s == Status.Active))
                                   {
                                     // Блокировка персоны.
                                     entityIsLocked = Locks.TryLock(person);
                                     if (!entityIsLocked)
                                     {
                                       var lockInfo = Locks.GetLockInfo(person);
                                       jobState.RetryMessage = string.Format("ID персоны: {0}. Таб. номер: {1}. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}. Не удалось установить блокировку на запись.",
                                                                             person.Id, record.PersonnelNumber, lockInfo.OwnerName, lockInfo.LockTime);
                                     }
                                     
                                     // Обновление полей Персоны.
                                     try
                                     {
                                       if (string.IsNullOrEmpty(jobState.RetryMessage))
                                       {
                                         if (!person.State.IsInserted && (person.FirstName != record.FirstName || person.LastName != record.LastName || person.MiddleName != record.MiddleName))
                                           needIssueCertificate = true;
                                         
                                         if (person.State.IsInserted || person.FirstName != record.FirstName)
                                           person.FirstName = record.FirstName;
                                         if (person.State.IsInserted || person.LastName != record.LastName)
                                           person.LastName = record.LastName;
                                         if (person.State.IsInserted || person.MiddleName != record.MiddleName)
                                           person.MiddleName = record.MiddleName;
                                         if (person.State.IsInserted || person.LegalAddress != record.LegalAddress)
                                           person.LegalAddress = record.LegalAddress;
                                         if (person.State.IsInserted || person.PostalAddress != record.PostalAddress)
                                           person.PostalAddress = record.PostalAddress;
                                         if (person.State.IsInserted || person.DateOfBirth != record.DateOfBirth)
                                           person.DateOfBirth = record.DateOfBirth;
                                         if (person.State.IsInserted || person.Phones != record.Phones)
                                           person.Phones = record.Phones;
                                         if (person.State.IsInserted || person.Sex != record.Sex)
                                           person.Sex = record.Sex;
                                         if (string.IsNullOrEmpty(person.INILA))
                                           person.INILA = record.INILA;
                                         if (person.Status != DirRX.OverrideSolution.Person.Status.Active)
                                           person.Status = DirRX.OverrideSolution.Person.Status.Active;
                                         
                                         #region ИНН.
                                         if (person.State.IsInserted || person.TIN != record.TIN)
                                         {
                                           var errorTin = Sungero.Parties.PublicFunctions.Counterparty.CheckTin(record.TIN, false);
                                           if (!string.IsNullOrEmpty(errorTin))
                                           {
                                             Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID персоны: {3}. Таб. номер: {4}. Ошибка: {5}.",
                                                                logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, person.Id, record.PersonnelNumber, errorTin);
                                             if (responsible != null)
                                               SendNotificationToResponsible(responsible, Resources.CompanySynchronizationErrorsNotificationSubjectFormat(Calendar.Now), string.Format("{0}. {1}", Hyperlinks.Get(record), errorTin));
                                             person.TIN = null;
                                           }
                                           else
                                           {
                                             person.TIN = record.TIN;
                                           }
                                         }
                                         #endregion
                                         
                                         #region Реквизиты документа, удостоверяющего личность.
                                         
                                         // Проверка реквизитов документа, удостоверяющего личность.
                                         var errorIdentityDocument = string.Join(" ", Functions.EmployeeQueue.CheckPersonIdentityDocument(record));
                                         if (!string.IsNullOrEmpty(errorIdentityDocument))
                                         {
                                           Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID персоны: {3}. Таб. номер: {4}. Ошибка: {5}.",
                                                              logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, person.Id, record.PersonnelNumber, errorIdentityDocument);
                                           if (responsible != null)
                                             SendNotificationToResponsible(responsible, Resources.CompanySynchronizationErrorsNotificationSubjectFormat(Calendar.Now), string.Format("{0}. {1}", Hyperlinks.Get(record), errorIdentityDocument));
                                           person.IdentityDocumentKind = null;
                                           person.IdentityDocumentSeries = null;
                                           person.IdentityDocumentNumber = null;
                                           person.IdentityDocumentIssueDate = null;
                                           person.IdentityDocumentIssuedBy = null;
                                           person.IdentityDocumentIssuerID = null;
                                           person.IdentityDocumentBirthPlaceDirRX = null;
                                           person.IdentityDocumentExpirationDate = null;
                                         }
                                         else
                                         {
                                           if (person.State.IsInserted || person.IdentityDocumentKind != record.IdentityDocumentKind)
                                             person.IdentityDocumentKind = record.IdentityDocumentKind;
                                           if (person.State.IsInserted || person.IdentityDocumentSeries != record.IdentityDocumentSeries)
                                             person.IdentityDocumentSeries = record.IdentityDocumentSeries;
                                           if (person.State.IsInserted || person.IdentityDocumentNumber != record.IdentityDocumentNumber)
                                             person.IdentityDocumentNumber = record.IdentityDocumentNumber;
                                           if (person.State.IsInserted || person.IdentityDocumentIssueDate != record.IdentityDocumentIssueDate)
                                             person.IdentityDocumentIssueDate = record.IdentityDocumentIssueDate;
                                           if (person.State.IsInserted || person.IdentityDocumentIssuedBy != record.IdentityDocumentIssuedBy)
                                             person.IdentityDocumentIssuedBy = record.IdentityDocumentIssuedBy;
                                           if (person.State.IsInserted || person.IdentityDocumentIssuerID != record.IdentityDocumentIssuerID)
                                             person.IdentityDocumentIssuerID = record.IdentityDocumentIssuerID;
                                           if (person.State.IsInserted || person.IdentityDocumentBirthPlaceDirRX != record.IdentityDocumentBirthPlace)
                                             person.IdentityDocumentBirthPlaceDirRX = record.IdentityDocumentBirthPlace;
                                           if (person.State.IsInserted || person.IdentityDocumentExpirationDate != record.IdentityDocumentExpirationDate)
                                             person.IdentityDocumentExpirationDate = record.IdentityDocumentExpirationDate;
                                         }
                                         
                                         #endregion
                                         
                                         #region Регион.
                                         // Заполнение региона и населённого пункта должно выполняться с учётом логики ведущий-ведомый.
                                         // Поиск/создание региона.
                                         var region = Sungero.Commons.Regions.Null;

                                         // Если код региона не совпадает с тем, который указан в персоне, то выполняем поиск или создание региона.
                                         if (person.Region?.Code != record.Region)
                                         {
                                           // Поиск/создание региона.
                                           region = Functions.Module.GetOrCreateRegion(record.Region);
                                           // Если не удалось найти/создать регион, то указываем регион по умолчанию.
                                           if (region == null)
                                           {
                                             region = Functions.Module.GetOrCreateDefaultRegion();
                                             Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID персоны: {3}. Таб. номер: {4}. Код региона: {5}. Не удалось создать регион. Будет указан регион по умолчанию.", logPrefix, EmployeeQueues.Info.LocalizedName,
                                                                region.Id, person.Id, record.PersonnelNumber, record.Region);
                                           }
                                         }
                                         // Если код региона совпадает, то записываем его в переменную для определения населённого пункта.
                                         else
                                           region = person.Region;
                                         #endregion

                                         #region Населенный пункт.
                                         // Всего две причины для изменения поля в справочнике.
                                         // Первый случай. Пришёл не пустой населённый пункт, и его имя и код региона не совпадает с тем, который уже указан в записи.
                                         if (!string.IsNullOrEmpty(record.City) && (person.City?.Name != record.City || person.Region?.Code != region.Code))
                                         {
                                           // Поиск/создание населённого пункта.
                                           var city = Functions.Module.GetOrCreateCity(record.City, region);
                                           if (city == null)
                                           {
                                             jobState.ErrorMessage = DirRX.NNIntegration.Resources.CompanySynchronizationCreateCityError;
                                             Logger.ErrorFormat("{0}. {1}. ID записи: {2}. ID персоны: {3}. Таб. номер: {4}. Населённый пункт: {5}. Не удалось создать населённый пункт.", logPrefix, EmployeeQueues.Info.LocalizedName,
                                                                record.Id, person.Id, record.PersonnelNumber, record.City);
                                           }
                                           else
                                             person.City = city;
                                         }
                                         // Второй случай. Пришёл пустой населённый пункт, а в персоне населённый пункт указан. Тогда необходимо очистить поле населённый пункт.
                                         // При очистке необходимо указать найденный регион, т.к. он автоматически очищается вместе с населённым пунктом.
                                         else if (string.IsNullOrEmpty(record.City) && person.City != null)
                                         {
                                           person.City = Sungero.Commons.Cities.Null;
                                           person.Region = region;
                                         }
                                         #endregion
                                         
                                         // Сохранение персоны.
                                         var changesSaved = person.State.IsInserted == false && person.State.IsChanged == false;
                                         if (string.IsNullOrEmpty(jobState.ErrorMessage) && !changesSaved)
                                         {
                                           person.Save();
                                           changesSaved = true;
                                           Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID персоны: {3}. Таб. номер: {4}. Запись справочника Персоны успешно сохранена.",
                                                              logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, person.Id, record.PersonnelNumber);
                                         }
                                       }
                                     }
                                     catch (Exception ex)
                                     {
                                       jobState.ErrorMessage = DirRX.NNIntegration.Resources.CompanySynchronizationPersonNotSavedErrorFormat(ex.Message);
                                       Logger.ErrorFormat("{0}. {1}. ID записи: {2}. ID персоны: {3}. Таб. номер: {4}. Не удалось сохранить запись справочника Персоны. Ошибка: {5}. StackTrace: {6}.",
                                                          logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, person.Id, record.PersonnelNumber, ex.Message, ex.StackTrace);
                                       throw ex;
                                     }
                                     finally
                                     {
                                       Locks.Unlock(person);
                                     }
                                   }
                                   else
                                     Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID персоны: {3}. Таб. номер: {4}. Запись справочника Персоны не требует обновления, так как по ней найдено несколько сотрудников, часть из которых в состоянии Действующая.",
                                                        logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, person.Id, record.PersonnelNumber);
                                 });
          
          // Если транзакция завершилась с ошибкой, то выполняем поиск персоны.
          if (!personExecutionResult || (person != null && person.State.IsInserted))
            record.Person = person == null || person.State.IsInserted ? People.Null : DirRX.OverrideSolution.PublicFunctions.Person.Remote.GetPersonByINILA(record.INILA);
          else if (!People.Equals(record.Person, person))
            record.Person = person;
        }
        #endregion
        
        #region Создание/обновление сотрудника.
        
        var employeeExecutionResult = false;
        var oldDepartmentIsLocked = false;
        var newDepartmentIsLocked = false;
        var department = Departments.Null;
        var oldDepartment = Sungero.Company.Departments.Null;
        var employee = Employees.Null;
        var employeeIsInserted = false;
        var employeePreviousStatus = OverrideSolution.Employee.Status.Active;
        
        if (string.IsNullOrEmpty(jobState.ErrorMessage) && string.IsNullOrEmpty(jobState.RetryMessage))
        {
          var oldJobTitle = OverrideSolution.JobTitles.Null;
          var isUpdateJobTitle = false;
          
          employeeExecutionResult =
            Transactions.Execute(() =>
                                 {
                                   // Поиск или создание сотрудника.
                                   employee = record.Employee ?? GetEmployeeByPersonnelNumber(record.PersonnelNumber) ?? Employees.Create();
                                   employeePreviousStatus = employee.Status.Value;
                                   oldDepartment = employee?.Department;
                                   var needUpdateEmployeeCertificatePhone = false;
                                   employeeIsInserted = employee.State.IsInserted;
                                   
                                   #region Блокировки.
                                   
                                   // Блокировка сотрудника.
                                   entityIsLocked = Locks.TryLock(employee);
                                   if (!entityIsLocked)
                                   {
                                     var lockInfo = Locks.GetLockInfo(employee);
                                     jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}",
                                                                           employee.Id, record.PersonnelNumber, lockInfo.OwnerName, lockInfo.LockTime);
                                   }
                                   
                                   // Поиск подразделения.
                                   if (entityIsLocked && Departments.As(employee.Department)?.SapIdDirRX != record.DepartmentCode)
                                   {
                                     department = GetDepartmentBySAPId(record.DepartmentCode);
                                     
                                     // Не найдено подразделение.
                                     if (department == null)
                                       jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не найдено подразделение по SAP ID: {2}", employee.Id, record.PersonnelNumber, record.DepartmentCode);
                                     
                                     if (string.IsNullOrEmpty(jobState.RetryMessage) && oldDepartment != null)
                                     {
                                       // Не заблокировано старое подразделение.
                                       oldDepartmentIsLocked = Locks.TryLock(oldDepartment);
                                       if (!oldDepartmentIsLocked)
                                       {
                                         var lockInfo = Locks.GetLockInfo(department);
                                         jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. ID подразделения: {2}. Не удалось установить блокировку на старое подразделение сотрудника. Запись заблокирована пользователем: {3}. Время установки блокировки: {4}",
                                                                               employee.Id, record.PersonnelNumber, employee.Department.Id, lockInfo.OwnerName, lockInfo.LockTime);
                                       }
                                     }
                                     
                                     if (string.IsNullOrEmpty(jobState.RetryMessage) && (oldDepartment == null || oldDepartmentIsLocked))
                                     {
                                       // Не заблокировано новое подразделение.
                                       newDepartmentIsLocked = Locks.TryLock(department);
                                       if (!newDepartmentIsLocked)
                                       {
                                         var lockInfo = Locks.GetLockInfo(department);
                                         jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. ID подразделения: {2}. Не удалось установить блокировку на новое подразделение сотрудника. Запись заблокирована пользователем: {3}. Время установки блокировки: {4}",
                                                                               employee.Id, record.PersonnelNumber, department.Id, lockInfo.OwnerName, lockInfo.LockTime);
                                       }
                                       else
                                         employee.Department = department;
                                     }
                                   }
                                   
                                   #endregion
                                   
                                   try
                                   {
                                     // Обновление полей сотрудника.
                                     if (string.IsNullOrEmpty(jobState.RetryMessage))
                                     {
                                       var personalPhoneFormatted = !personalPhoneIsEmpty ? EssPlatform.PublicFunctions.Module.PhoneNumberToUniversalFormat(personalPhone) : string.Empty;
                                       
                                       // Персона
                                       if (employee.State.IsInserted || !People.Equals(employee.Person, person))
                                         employee.Person = person;
                                       
                                       // Таб. номер сотрудника.
                                       if (string.IsNullOrEmpty(employee.PersonnelNumber))
                                         employee.PersonnelNumber = record.PersonnelNumber;
                                       
                                       #region Раздел и подраздел персонала.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage))
                                       {
                                         var personnelSection = NNCommons.PublicFunctions.PersonnelSection.Remote.GetPersonnelSectionByCode(record.PersonnelSectionCode);
                                         if (personnelSection == null)
                                           jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. {2}", employee.Id, record.PersonnelNumber,
                                                                                 EmployeeQueues.Resources.PersonnelSectionNotFoundErrorFormat(record.PersonnelSectionCode));
                                         else if (employee.State.IsInserted || !NNCommons.PersonnelSections.Equals(employee.PersonnelSectionDirRX, personnelSection))
                                           employee.PersonnelSectionDirRX = personnelSection;
                                         
                                         var personnelSubSection = NNCommons.PublicFunctions.PersonnelSubSection.Remote.GetPersonnelSubSectionByCode(record.PersonnelSubSectionCode);
                                         if (personnelSubSection == null)
                                           jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. {2}", employee.Id, record.PersonnelNumber,
                                                                                 EmployeeQueues.Resources.PersonnelSubSectionNotFoundErrorFormat(record.PersonnelSubSectionCode));
                                         else if (employee.State.IsInserted || !NNCommons.PersonnelSubSections.Equals(employee.PersonnelSubSectionDirRX, personnelSubSection))
                                           employee.PersonnelSubSectionDirRX = personnelSubSection;
                                       }
                                       #endregion
                                       
                                       #region Email.
                                       if (employee.State.IsInserted || employee.Email != record.Email)
                                       {
                                         employee.Email = record.Email;
                                         if (!employee.State.IsInserted && employee.PersonalAccountStatusDirRX == OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted)
                                           needIssueCertificate = true;
                                       }
                                       #endregion
                                       
                                       #region Даты действия сотрудника.
                                       if (employee.State.IsInserted || employee.RecordStartDateDirRX != record.RecordStartDate)
                                         employee.RecordStartDateDirRX = record.RecordStartDate;
                                       if (employee.State.IsInserted || employee.RecordExpirationDateDirRX != record.RecordEndDate)
                                         employee.RecordExpirationDateDirRX = record.RecordEndDate;
                                       #endregion
                                       
                                       #region Обработка состояния действия сотрудника.
                                       
                                       // Если сотрудник уже сохранён в системе, он преводится в закрытое состояние и дата начала действия меньше текущей даты,
                                       // то закрытие будет выполнено в блоке ФП "Изменение состояния записей справочника "Сотрудники". Установка статуса "Закрытая"".
                                       // При необходимости для сотрудника выполнится отключения от ЛК и очистится его УЗ.
                                       if (!employee.State.IsInserted && employee.Status == OverrideSolution.Employee.Status.Active
                                           && employeeStatus == OverrideSolution.Employee.Status.Closed && employee.RecordStartDateDirRX < Calendar.Now)
                                       {
                                         Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Сотрудник остаётся в действующем состоянии. Закрытие выполнится далее в последующей обработке.",
                                                            logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber);
                                         employeeStatus = OverrideSolution.Employee.Status.Active;
                                       }
                                       
                                       if (employee.Status != employeeStatus)
                                         employee.Status = employeeStatus;
                                       #endregion
                                       
                                       #region Группа МП.
                                       var groupMP = GetActiveMPGroups(record.MPGroup).FirstOrDefault();
                                       if (!NNOtherProcesses.LrdPaymentMPGroups.Equals(groupMP, employee.MPGroupDirRX))
                                         employee.MPGroupDirRX = groupMP;
                                       #endregion
                                       
                                       #region Обработка вида занятости сотрудника.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage) && !DirRX.OverrideSolution.PublicFunctions.Employee.TryParseEmployemntType(employee, record.EmploymentType))
                                       {
                                         Logger.ErrorFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Не удалось установить вид занятости сотрудника. Код вида занятости: {5}.",
                                                            logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber, record.EmploymentType);
                                         jobState.ErrorMessage = DirRX.NNIntegration.Resources.EmployeeEmployemntTypeParseErrorFormat(record.EmploymentType);
                                       }
                                       
                                       // Проверить существование действующих сотрудников с основным местом работы, у которых вид занятости - основное место работы.
                                       // Если такие записи есть, то необходимо сбросить изменения и отправить на повторную обработку запись.
                                       if (employee.State.IsChanged && employee.Status == Sungero.Company.Employee.Status.Active &&
                                           employee.EmploymentType == EmploymentType.MainPlace && HasActiveEmployeesWithMainPlace(employee))
                                       {
                                         jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Найдены другие действующие сотрудники персоны с видом занятости - основное место работы",
                                                                               employee.Id, record.PersonnelNumber);
                                         // Сбросить текущие изменения сотрудника и прервать транзакцию, т.к. такой сотрудник не сохранится из-за ошибки валидации в HR Pro.
                                         jobState.ResetChanges = true;
                                         return;
                                       }
                                       
                                       #endregion
                                       
                                       #region Доступ в ЛК.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage) && (employee.State.IsInserted || employee.PersonalPhoneDirRX != personalPhoneFormatted))
                                       {
                                         Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Обнаружено изменение личного телефона сотрудника.",
                                                            logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber);
                                         
                                         var mainEmployee = Employees.As(EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(employee.Person.Id, null));
                                         
                                         // Если сотрудник или его основной сотрудник подключён к личному кабинету.
                                         if (employee.PersonalAccountStatusDirRX != OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent ||
                                             (mainEmployee != null && mainEmployee.PersonalAccountStatusDirRX != OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent))
                                         {
                                           Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Обработка изменения номера телефона. Сотрудник - подключён к ЛК.",
                                                              logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber);
                                           
                                           // Если очистился личный телефон, то отключить сотрудника от ЛК и проставить признак для отчистки телефона после отключения.
                                           if (personalPhoneIsEmpty)
                                           {
                                             Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Обработка изменения номера телефона. У сотрудника очистился личный телефон. Запуск отключения сотрудника от ЛК.",
                                                                logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber);
                                             try
                                             {
                                               // HACK: Нельзя очистить номер телефона, пока сотрудник подключен к ЛК. Очистка телефона выполняется в событии "До сохранения"
                                               // вместе с изменением статуса после отключения сотрудника от ЛК. Если сотрудник совместитель, то установить этот признак главному сотруднику.
                                               if (mainEmployee != null && !Employees.Equals(mainEmployee, employee))
                                               {
                                                 Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Обработка изменения номера телефона. Сотрудник не является основным сотрудником персоны. Отключение будет выполнятся для основного сотрудника с ИД: {5}.",
                                                                    logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber, mainEmployee.Id);
                                                 
                                                 mainEmployee.NeedClearPhoneDirRX = true;
                                                 mainEmployee.Save();
                                                 DirRX.OverrideSolution.PublicFunctions.Employee.DisconnectEssEmployee(mainEmployee);
                                               }
                                               else
                                               {
                                                 Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Обработка изменения номера телефона. Сотрудник является основным сотрудником персоны. Отключение будет выполнятся для этого сотрудника.",
                                                                    logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber);
                                                 employee.NeedClearPhoneDirRX = true;
                                                 DirRX.OverrideSolution.PublicFunctions.Employee.DisconnectEssEmployee(employee);
                                               }
                                             }
                                             catch (Exception ex)
                                             {
                                               var errorMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не удалось отключить сотрудника от ЛК. Текст ошибки: {2}.", employee.Id, record.PersonnelNumber, ex.Message);
                                               jobState.RetryMessage = string.Format("{0}  StackTrace: {1}", errorMessage, ex.StackTrace);
                                               
                                               if (responsible != null)
                                                 DirRX.NNIntegration.PublicFunctions.Module.SendNotificationToResponsible(responsible, errorMessage, errorMessage);
                                             }
                                           }
                                           // Если обновился личный телефон, то выполнить обновление номера в ЛК.
                                           else
                                           {
                                             Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Обработка изменения номера телефона. У сотрудника обновился личный телефон. Запуск обновления телефона в ЛК.",
                                                                logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber);
                                             
                                             employee.PersonalPhoneDirRX = personalPhoneFormatted;
                                             // Отправить запрос в ЛК для смены телефона.
                                             var phoneChangeError = ChangeEmployeePhoneNumber(logPrefix, employee, Constants.Module.ExternalSystemsFlowGuids.CompanySynchronization);
                                             if (!string.IsNullOrEmpty(phoneChangeError))
                                               jobState.RetryMessage = phoneChangeError;
                                             else
                                               needIssueCertificate = true;
                                           }

                                         }
                                         // Если сотрудник не подключён к ЛК, меняем номер телефона
                                         else
                                         {
                                           Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Обработка изменения номера телефона. Сотрудник не подключён к ЛК. Телефон будет обновлён.",
                                                              logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber);
                                           employee.PersonalPhoneDirRX = personalPhoneFormatted;
                                         }
                                         
                                         // Если у сотрудника изменился личный телефон, то после сохранения необходимо запустить АО по обновлению потенциального фиктивного телефона у другого сотрудника.
                                         needUpdateEmployeeCertificatePhone = employee.State.Properties.PersonalPhoneDirRX.IsChanged && !string.IsNullOrEmpty(employee.PersonalPhoneDirRX);
                                       }
                                       #endregion
                                       
                                       #region Признаки для рассылки уведомлений.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage))
                                       {
                                         if (employee.State.IsInserted || string.IsNullOrEmpty(employee.Email))
                                         {
                                           if (employee.NeedNotifyAssignmentsSummary != false)
                                             employee.NeedNotifyAssignmentsSummary = false;
                                           if (employee.NeedNotifyExpiredAssignments != false)
                                             employee.NeedNotifyExpiredAssignments = false;
                                           if (employee.NeedNotifyNewAssignments != false)
                                             employee.NeedNotifyNewAssignments = false;
                                         }
                                       }
                                       #endregion
                                       
                                       #region Должности.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage))
                                       {
                                         // Изменение должности. Существует две причины для изменения поля в справочнике.
                                         // Первый случай. Пришёл SAP ID должности, который не совпадает с кодом должности у сотрудника.
                                         if (!string.IsNullOrEmpty(record.JobTitleCode) && JobTitles.As(employee.JobTitle)?.SapIdDirRX != record.JobTitleCode)
                                         {
                                           // Поиск должности.
                                           var jobTitle = GetJobTitleBySAPId(record.JobTitleCode);
                                           
                                           // Если не найдена должность, то отправляем запись на повторную обработку.
                                           if (jobTitle == null)
                                             jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не найдена должность по SAP ID: {2}", employee.Id, record.PersonnelNumber,  record.JobTitleCode);
                                           else
                                           {
                                             oldJobTitle = OverrideSolution.JobTitles.As(employee.JobTitle);
                                             employee.JobTitle = jobTitle;
                                             isUpdateJobTitle = true;
                                           }
                                         }
                                         // Второй случай. Пришёл пустой SAP ID должности, а у сотрудника заполнена должность.
                                         else if (string.IsNullOrEmpty(record.JobTitleCode) && employee.JobTitle != null)
                                         {
                                           oldJobTitle = OverrideSolution.JobTitles.As(employee.JobTitle);
                                           employee.JobTitle = JobTitles.Null;
                                           isUpdateJobTitle = true;
                                         }
                                         
                                       }
                                       #endregion
                                       
                                       #region Линейный руководитель.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage))
                                       {
                                         // Изменение лин. руководителя. Всего две причины для изменения поля в справочнике.
                                         // Первый случай. Пришёл не пустой SAP ID лин. руководителя, и он не совпадает с тем, который уже указан в записи сотрудника.
                                         if (!string.IsNullOrEmpty(record.LineManagerCode) && employee.LineManagerDirRX?.PersonnelNumber != record.LineManagerCode)
                                         {
                                           // Поиск лин. руководителя.
                                           var manager = GetEmployeeByPersonnelNumber(record.LineManagerCode);
                                           // Если не найден лин. руководитель, то отправляем запись на повторную обработку.
                                           if (manager == null)
                                             jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не найден лин. руководитель по таб. номеру: {2}", employee.Id, record.PersonnelNumber, record.LineManagerCode);
                                           else
                                             employee.LineManagerDirRX = manager;
                                         }
                                         // Второй случай. Пришёл пустой SAP ID лин. руководителя, а у сотрудника заполнен лин. руководитель.
                                         else if (string.IsNullOrEmpty(record.LineManagerCode) && employee.LineManagerDirRX != null)
                                           employee.LineManagerDirRX = Employees.Null;
                                       }
                                       #endregion
                                       
                                       #region НГД.
                                       if (employee.IsNGDDirRX != record.IsNGD)
                                         employee.IsNGDDirRX = record.IsNGD;
                                       #endregion
                                       
                                       #region Часовой пояс
                                       if (record.UtcOffset != employee.UtcOffsetDirRX)
                                         employee.UtcOffsetDirRX = record.UtcOffset;
                                       #endregion
                                       
                                       #region Линейный руководитель для информирования.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage))
                                       {
                                         // Изменение лин. руководителя для информирования. Всего две причины для изменения поля в справочнике.
                                         // Первый случай. Пришёл не пустой SAP ID лин. руководителя для информирования, и он не совпадает с тем, который уже указан в записи сотрудника.
                                         if (!string.IsNullOrEmpty(record.InfoManager) && employee.InfoManagerDirRX?.PersonnelNumber != record.InfoManager)
                                         {
                                           var infoManager = GetEmployeeByPersonnelNumber(record.InfoManager);
                                           // Если не найден лин. руководитель для информирования, то отправляем запись на повторную обработку.
                                           if (infoManager == null)
                                             jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не найден лин. руководитель для информирования по таб. номеру: {2}", employee.Id, record.PersonnelNumber, record.InfoManager);
                                           else
                                             employee.InfoManagerDirRX = infoManager;
                                         }
                                         // Второй случай. Пришёл пустой SAP ID лин. руководителя для информирования, а у сотрудника заполнен лин. руководитель.
                                         else if (string.IsNullOrEmpty(record.InfoManager) && employee.InfoManagerDirRX != null)
                                           employee.InfoManagerDirRX = Employees.Null;
                                       }
                                       #endregion
                                       
                                       #region Линейный руководитель(2 уровень).
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage))
                                       {
                                         // Изменение лин. руководителя (2 уровня). Всего две причины для изменения поля в справочнике.
                                         // Первый случай. Пришёл не пустой SAP ID лин. руководителя (2 уровня) для информирования, и он не совпадает с тем, который уже указан в записи сотрудника.
                                         if (!string.IsNullOrEmpty(record.ManagerLevel2) && employee.ManagerLevel2DirRX?.PersonnelNumber != record.ManagerLevel2)
                                         {
                                           var managerLevel2 = GetEmployeeByPersonnelNumber(record.ManagerLevel2);
                                           // Если не найден лин. руководитель (2 уровень), то отправляем запись на повторную обработку.
                                           if (managerLevel2 == null)
                                             jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не найден лин. руководитель (2 уровень) по таб. номеру: {2}",
                                                                                   employee.Id, record.PersonnelNumber, record.ManagerLevel2);
                                           else
                                             employee.ManagerLevel2DirRX = managerLevel2;
                                         }
                                         // Второй случай. Пришёл пустой SAP ID лин. руководителя (2 уровень), а у сотрудника заполнен лин. руководитель.
                                         else if (string.IsNullOrEmpty(record.ManagerLevel2) && employee.ManagerLevel2DirRX != null)
                                           employee.ManagerLevel2DirRX = Employees.Null;
                                       }
                                       #endregion
                                       
                                       #region Руководитель ВСП.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage))
                                       {
                                         // Изменение Руководителя ВСП. Всего две причины для изменения поля в справочнике.
                                         // Первый случай. Пришёл не пустой SAP ID Руководителя ВСП, и он не совпадает с тем, который уже указан в записи сотрудника.
                                         if (!string.IsNullOrEmpty(record.VSPManager) && employee.VSPManagerDirRX?.PersonnelNumber != record.VSPManager)
                                         {
                                           var vspManager = GetEmployeeByPersonnelNumber(record.VSPManager);
                                           // Если не найден лин. руководитель для информирования, то отправляем запись на повторную обработку.
                                           if (vspManager == null)
                                             jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не найден Руководителя ВСП по таб. номеру: {2}",
                                                                                   employee.Id, record.PersonnelNumber, record.VSPManager);
                                           else
                                             employee.VSPManagerDirRX = vspManager;
                                         }
                                         // Второй случай. Пришёл пустой SAP ID Руководителя ВСП, а у сотрудника заполнен лин. руководитель.
                                         else if (string.IsNullOrEmpty(record.VSPManager) && employee.VSPManagerDirRX != null)
                                           employee.VSPManagerDirRX = Employees.Null;
                                       }
                                       #endregion
                                       
                                       #region Руководитель СП.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage))
                                       {
                                         // Изменение Руководителя СП. Всего две причины для изменения поля в справочнике.
                                         // Первый случай. Пришёл не пустой SAP ID Руководителя СП, и он не совпадает с тем, который уже указан в записи сотрудника.
                                         if (!string.IsNullOrEmpty(record.SPManager) && employee.SPManagerDirRX?.PersonnelNumber != record.SPManager)
                                         {
                                           var spManager = GetEmployeeByPersonnelNumber(record.SPManager);
                                           // Если не найден лин. руководитель для информирования, то отправляем запись на повторную обработку.
                                           if (spManager == null)
                                             jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не найден Руководителя СП по таб. номеру: {2}",
                                                                                   employee.Id, record.PersonnelNumber, record.SPManager);
                                           else
                                             employee.SPManagerDirRX = spManager;
                                         }
                                         // Второй случай. Пришёл пустой SAP ID Руководителя ВСП, а у сотрудника заполнен лин. руководитель.
                                         else if (string.IsNullOrEmpty(record.SPManager) && employee.SPManagerDirRX != null)
                                           employee.SPManagerDirRX = Employees.Null;
                                       }
                                       #endregion
                                       
                                       #region Категория сотрудника.
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage))
                                       {
                                         var category = Functions.EmployeeQueue.GetEmployeeCategory(record, record.PersCategory);
                                         if (!employee.CategoryDirRX.Equals(category))
                                           employee.CategoryDirRX = category;
                                       }
                                       #endregion
                                       
                                       Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Итоговый статус действия сотрудника перед сохранением: {5}.",
                                                          logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber,
                                                          Employees.Info.Properties.Status.GetLocalizedValue(employee.Status));
                                       
                                       // Сохранение сотрудника.
                                       var changesSaved = employee.State.IsInserted == false && employee.State.IsChanged == false;
                                       if (string.IsNullOrEmpty(jobState.ErrorMessage) && !changesSaved)
                                       {
                                         employee.Save();
                                         changesSaved = true;
                                         Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Запись справочника Сотрудники успешно сохранена.",
                                                            logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber);
                                         
                                         // Если сотрудник сохранился и у него изменился личный телефон, то проверяем, что такой номер не используется в качестве фиктивного номера.
                                         if (needUpdateEmployeeCertificatePhone)
                                         {
                                           Logger.DebugFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. У сотрудника изменился личный телефон, будет выполнена проверка, что такой номер не используется в качестве фиктивного номера в сертификате другого сотрудника.",
                                                              logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber);
                                           DirRX.OverrideSolution.PublicFunctions.Employee.CheckEmployeesCertificatePhone(employee);
                                         }
                                       }
                                     }
                                   }
                                   catch (Exception ex)
                                   {
                                     jobState.ErrorMessage = DirRX.NNIntegration.Resources.CompanySynchronizationEmployeeNotSavedErrorFormat(ex.Message);
                                     Logger.ErrorFormat("{0}. {1}. ID записи: {2}. ID сотрудника: {3}. Таб. номер: {4}. Не удалось сохранить запись справочника Сотрудники. Ошибка: {5}. StackTrace: {6}.",
                                                        logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, employee.Id, record.PersonnelNumber, ex.Message, ex.StackTrace);
                                     throw ex;
                                   }
                                   finally
                                   {
                                     Locks.Unlock(employee);
                                   }
                                 });
          
          // Если должность поменялась, нужно проверить, руководящая ли она, и обновить руководителей подразделений.
          if (employeeExecutionResult && isUpdateJobTitle)
            Functions.Module.UpdateDepartmentManagers(logPrefix, employee, oldJobTitle);
          
          if (oldDepartmentIsLocked && Locks.GetLockInfo(oldDepartment).IsLockedByMe)
            Locks.Unlock(oldDepartment);
          
          if (newDepartmentIsLocked && department != null && !department.State.IsInserted && Locks.GetLockInfo(department).IsLockedByMe)
            Locks.Unlock(department);
          
          // Если транзакция завершилась с ошибкой, то выполняем поиск сотрудника.
          if (!employeeExecutionResult || jobState.ResetChanges || (employee != null && employee.State.IsInserted))
            record.Employee = employee == null || employee.State.IsInserted ? Employees.Null : GetEmployeeByPersonnelNumber(record.PersonnelNumber);
          else
          {
            var isReissueCertificateOnOrgStructureSync = NNCommons.PublicFunctions.Module.Remote.GetBoolValue(Constants.Module.ConstantsSetting.ReissueCertificateOnOrgStructureSync.Guid);
            var isAccountConnected = employee.PersonalAccountStatusDirRX == OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted &&
              employee.ConsentHRPro == OverrideSolution.Employee.ConsentHRPro.Signed;
            // Выпустить сертификат, если изменился номер телефона или адрес эл. почты или ФИО, а сам сотрудник подключен к ЛК и у него подписано согласие.
            if ((isReissueCertificateOnOrgStructureSync == true) && isAccountConnected && needIssueCertificate && string.IsNullOrEmpty(jobState.ErrorMessage))
              DirRX.OverrideSolution.PublicFunctions.Employee.ReissueCertificate(employee);
            
            if (!Employees.Equals(record.Employee, employee))
              record.Employee = employee;
          }
        }
        
        #endregion
        
        #region Создание замещений для внутреннего совместителя.
        if (string.IsNullOrEmpty(jobState.ErrorMessage) && string.IsNullOrEmpty(jobState.RetryMessage))
        {
          // Если сотрудник - действующий совместитель, то необходимо проверить наличие или создать замещение.
          if (record.Employee.EmploymentType == OverrideSolution.Employee.EmploymentType.InternalConcurr &&
              record.Employee.Status == OverrideSolution.Employee.Status.Active)
          {
            var substitutionInfo = FindSubstitutionInfo(logPrefix, record.Employee);
            // Если найден замещающий (сотрудник с основным местом работы) и в системе нет настроенного замещения, то создать замещение.
            if (substitutionInfo.Substitute != null && !substitutionInfo.HasSubstitution)
              CreateSubstitution(logPrefix, substitutionInfo);
          }
          
          // Если сотрудник - действующий сотрудник с основным местом работы, и он был только что создан или его предыдущий статус был закрытый,
          // то необходимо проверить наличие замещений для его совместителей.
          if (record.Employee.EmploymentType == OverrideSolution.Employee.EmploymentType.MainPlace &&
              record.Employee.Status == OverrideSolution.Employee.Status.Active &&
              (employeeIsInserted || employeePreviousStatus == OverrideSolution.Employee.Status.Closed))
          {
            UpdateSubstitutionsForMainEmployee(logPrefix, record.Employee);
          }
          
        }
        #endregion
        
        #region Синхронизация часового пояса.
        if (string.IsNullOrEmpty(jobState.ErrorMessage) && string.IsNullOrEmpty(jobState.RetryMessage))
        {
          var syncTimeZoneError = NNCommons.PublicFunctions.Module.SynchronizationTimeZone(employee);
          if (!string.IsNullOrEmpty(syncTimeZoneError))
            jobState.RetryMessage = string.Format("ID сотрудника: {0}. Таб. номер: {1}. Не удалось синхронизировать часовой пояс сотрудника: {2}", employee.Id, record.PersonnelNumber,  syncTimeZoneError);
        }
        #endregion
        
        // Если во время обработки возникла ошибка, то добавить запись в список ошибок и установить статус - Ошибка обработки.
        if (!string.IsNullOrEmpty(jobState.ErrorMessage))
        {
          record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Error;
          record.ErrorMessage = jobState.ErrorMessage;
          errors.Add(record);
        }
        // Если требуется повторная обработка, то установить статус - Повторная обработка.
        else if (!string.IsNullOrEmpty(jobState.RetryMessage))
        {
          if (record.ProcessingStatus != DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Retry)
            record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Retry;
          if (record.RetryReason != jobState.RetryMessage)
            record.RetryReason = jobState.RetryMessage;
          
          Logger.DebugFormat("{0}. {1}. ID записи: {2}. Таб. номер: {3}. {4}. Обработка повторится при следующем запуске фонового процесса.", logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, record.PersonnelNumber, jobState.RetryMessage);
        }
        // Иначе установить статус - Обработано.
        else
          record.ProcessingStatus = DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Processed;

        // Сохранение записи очереди.
        if (!Functions.Module.CompanyQueueRecordSave(record, logPrefix))
          Logger.DebugFormat("{0}. {1}. ID записи: {2}. Таб. номер: {3}. Cтатус записи не был изменён на \"{4}\" из-за ошибки сохранения.",
                             logPrefix, EmployeeQueues.Info.LocalizedName, record.Id, record.PersonnelNumber,
                             CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
        
        Locks.Unlock(record);
        Logger.DebugFormat("{0}. {1}. ID записи: {2}. Таб. номер: {3}. Наименование: {4}. Статус обработки: {5}. Конец обработки записи очереди.", logPrefix, EmployeeQueues.Info.LocalizedName,
                           record.Id, record.PersonnelNumber, record.RecordName, CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
      }
      
      Logger.DebugFormat("{0}. {1}. Конец обработки. На повторную обработку отправлено {2} записей.", logPrefix, EmployeeQueues.Info.LocalizedName,
                         employeeQueueRecords.Where(x => x.ProcessingStatus == DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Retry).Count());
      Logger.DebugFormat("{0}. {1}. Конец обработки очереди.", logPrefix, EmployeeQueues.Info.LocalizedName);
    }

    /// <summary>
    /// Обработка линейных руководителей.
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    /// <param name="responsible">Ответственный.</param>
    public virtual void LineManagersProccessing(string logPrefix, IEmployee responsible)
    {
      // Получить роль "Линейные руководители".
      var roleGuid = DirRX.NNCommons.PublicConstants.Module.RolesGuid.LineManager;
      var managerRole = Roles.GetAll(x => x.Sid == roleGuid).FirstOrDefault();
      
      Logger.DebugFormat("{0}. Начало обработки линейных руководителей.", logPrefix);
      
      if (managerRole == null)
      {
        var message = string.Format("Не удалось получить роль \"Линейные руководители\" по Guid. Необходимо выполнить инициализацию системы для создания роли. Значение Guid роли из константы: {0}.", roleGuid);
        Logger.ErrorFormat("{0}. {1}.", logPrefix, message);
        SendNotificationToResponsible(responsible, message, string.Empty);
        return;
      }
      
      if (!Locks.TryLock(managerRole))
      {
        var lockInfo = Locks.GetLockInfo(managerRole);
        Logger.DebugFormat("{0}. Не удалось установить блокировку на роль \"{1}\". Запись заблокирована пользователем: {2}. Время установки блокировки: {3}. Обновление роли повторится при следующем запуске ФП.", logPrefix,
                           managerRole.Name, lockInfo.OwnerName, lockInfo.LockTime);
        return;
      }
      
      // Список ЛР указанный в справочнике "Сотрудники".
      var managersInEmployees = Employees.GetAll()
        .Where(x =>
               x.Status == DirRX.OverrideSolution.Employee.Status.Active
               && x.LineManagerDirRX != null
              )
        .Select(x => x.LineManagerDirRX);
      
      // Получить недействительных руководителей, которые стали не действующими или
      // у которых в настройках НОР признаки Подключить или Автоматически подключать линейных руководителей к ЛК указано значение не "Да".
      var closedManagers = managerRole.RecipientLinks
        .Where(x => Employees.Is(x.Member))
        .Where(x => x.Member.Status == Sungero.Company.Employee.Status.Closed ||
               ((Employees.As(x.Member).RecordExpirationDateDirRX.HasValue &&
                 (Employees.As(x.Member).RecordExpirationDateDirRX <= Calendar.Now && x.Member.Status == DirRX.OverrideSolution.Employee.Status.Active))) ||
               OverrideSolution.BusinessUnits.As(Employees.As(x.Member).BusinessUnit)?.UseESSDirRX != true ||
               OverrideSolution.BusinessUnits.As(Employees.As(x.Member).BusinessUnit)?.ActivateLineManagersDirRX != true)
        .ToList();
      
      foreach (var recipient in closedManagers)
      {
        Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотрудник - недействителен. Он будет исключён из роли \"{2}\".", logPrefix, recipient.Member.Id, managerRole.Name);
        managerRole.RecipientLinks.Remove(recipient);
      }

      // Получить список участников роли "Линейные руководители" для исключения, которые не указаны в поле "Линейный руководитель" ни у одного сотрудника.
      var notManagers = managerRole.RecipientLinks
        .Where(x => !managersInEmployees.Contains(Employees.As(x.Member)))
        .ToList();
      
      foreach (var recipient in notManagers)
      {
        Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотрудник - не линейный руководитель. Он будет исключён из роли \"{2}\".", logPrefix, recipient.Member.Id, managerRole.Name);
        managerRole.RecipientLinks.Remove(recipient);
      }
      
      // Итоговый список сотрудников, которых нужно исключить из роли "Линейные руководители".
      notManagers.AddRange(closedManagers);
      
      // Получить список сотрудников, которых нужно добавить в роль "Линейные руководители", и которые ещё не включены в роль
      // и у которых заполнены личный телефон и в настройках НОР признаки Подключить и Автоматически подключать линейных руководителей к ЛК указано значение "Да".
      var managers = managerRole.RecipientLinks.Select(y => y.Member).AsEnumerable();
      IEnumerable<IEmployee> newManagers = managersInEmployees.Where(x => !managers.Contains(x) && x.Status == DirRX.OverrideSolution.Employee.Status.Active).ToList().Distinct();
      newManagers = newManagers.Where(x =>
                                      x.PersonalPhoneDirRX != null && x.PersonalPhoneDirRX != string.Empty &&
                                      OverrideSolution.BusinessUnits.As(x.BusinessUnit)?.UseESSDirRX == true &&
                                      OverrideSolution.BusinessUnits.As(x.BusinessUnit)?.ActivateLineManagersDirRX == true);
      
      foreach (var employee in newManagers)
      {
        Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотрудник - линейный руководитель. Он будет включён в роль \"{2}\".", logPrefix, employee.Id, managerRole.Name);
        managerRole.RecipientLinks.AddNew().Member = employee;
      }
      
      #region Отключение руководителей от ЛК.
      Logger.DebugFormat("{0}. Начало отключения сотрудников от личного кабинета. Необходимо отключить: {1}.", logPrefix, notManagers.Count());
      foreach (var recipient in notManagers)
      {
        var employee = Employees.As(recipient.Member);
        Logger.DebugFormat("{0}. ID сотрудника: {1}. Согласие на КЭДО: {2}. Начало обработки устаревшего руководителя.", logPrefix, employee.Id,
                           DirRX.OverrideSolution.Employees.Info.Properties.ConsentHRPro.GetLocalizedValue(employee.ConsentHRPro));
        var isLocked = Locks.TryLock(employee);
        if (isLocked)
        {
          var personLineManagersNumber = managerRole.RecipientLinks.Where(c => Employees.Is(c.Member) && People.Equals(Employees.As(c.Member).Person, employee.Person)).Count();
          
          // Если у сотрудника или у его совместителей не подписано согласие на КЭДО и ни один совместитель не является ЛРом, необходимо отключить сотрудника от ЛК.
          if (!DirRX.OverrideSolution.PublicFunctions.Employee.Remote.CheckConsentSigned(employee, true) && personLineManagersNumber == 0)
          {
            Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотрудник выполнял функции только одного линейного руководителя, и у него не подписано согласие на КЭДО. Сейчас будет произведено его отключение от ЛК.",
                               logPrefix, employee.Id);
            
            var mainEmployee = Employees.As(DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(employee.Person.Id, null)) ?? employee;
            
            // Отключить сотрудника от ЛК.
            try
            {
              DirRX.OverrideSolution.PublicFunctions.Employee.DisconnectEssEmployee(mainEmployee);
            }
            catch (Exception ex)
            {
              Logger.ErrorFormat("{0}. ID сотрудника: {1}. Не удалось отключить сотрудника от ЛК. Ошибка: {2}. StackTrace: {3}.",
                                 logPrefix, employee.Id, ex.Message, ex.StackTrace);
              managerRole.RecipientLinks.AddNew().Member = employee;
              Locks.Unlock(employee);
              continue;
            }
          }
          else
          {
            Logger.DebugFormat("{0}. ID сотрудника: {1}. У руководителя или его совместителей подписано согласие на КЭДО. Отключение от ЛК не будет произведено.",
                               logPrefix, employee.Id);
            // Выпуск сертификата.
            if (DirRX.OverrideSolution.PublicFunctions.Employee.NeedIssueCertificate(employee))
            {
              try
              {
                DirRX.OverrideSolution.PublicFunctions.Employee.IssueCertificate(employee);
              }
              catch (Exception ex)
              {
                Logger.ErrorFormat("{0}. ID сотрудника: {1}. Не удалось выпустить сертификат ЭП для сотрудника. Ошибка: {2}. StackTrace: {3}.", logPrefix, employee.Id, ex.Message, ex.StackTrace);
              }
            }
          }
          
          Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотрудник исключён из роли \"{2}\".", logPrefix, employee.Id, managerRole.Name);
          Locks.Unlock(employee);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(employee);
          Logger.DebugFormat("{0}. ID сотрудника: {1}. Не удалось установить блокировку. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.", logPrefix, employee.Id,
                             lockInfo.OwnerName, lockInfo.LockTime);
          managerRole.RecipientLinks.AddNew().Member = employee;
          continue;
        }
        Logger.DebugFormat("{0}. ID сотрудника: {1}. Конец обработки устаревшего руководителя.", logPrefix, employee.Id);
      }
      #endregion
      
      #region Подключение руководителей к ЛК.
      
      // Подключаются только линейные руководители, у которых в настройках НОР включен признак автоматического подключения.
      Logger.DebugFormat("{0}. Начало подключения сотрудников к личному кабинету. Необходимо подключить: {1}.", logPrefix, newManagers.Count());
      foreach (var employee in newManagers)
      {
        Logger.DebugFormat("{0}. ID сотрудника: {1}. Согласие на КЭДО: {2}. Начало обработки сотрудника.", logPrefix, employee.Id,
                           DirRX.OverrideSolution.Employees.Info.Properties.ConsentHRPro.GetLocalizedValue(employee.ConsentHRPro));
        
        
        var isLocked = Locks.TryLock(employee);
        if (isLocked)
        {
          // Активация пользователей в ЛК.
          try
          {
            if (NeedConnectLineManager(employee))
            {
              Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотрудник не подключён к личному кабинету. Старт подключения.", logPrefix, employee.Id);
              
              var isConsentSigned = DirRX.OverrideSolution.PublicFunctions.Employee.Remote.CheckConsentSigned(employee, false);
              var processingError = ProcessingErrorStruct.Create();
              var model = ProcessIncomingData(employee.PersonnelNumber, employee.PersonalPhoneDirRX, Constants.Module.CreateAccount.AccountType.Kedo, isConsentSigned, Constants.Module.CreateAccount.Providers.Keycloak, processingError);
              var mainEmployee = Employees.As(DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(employee.Person.Id, null)) ?? employee;
              
              if (mainEmployee.EmploymentType == DirRX.OverrideSolution.Employee.EmploymentType.InternalConcurr)
              {
                Logger.ErrorFormat("{0}. ID сотрудника: {1}. Основной сотрудник (ID: {2}) персоны этого сотрудника является внутренним совместителем." +
                                   " Подключение к ЛК возможно только при наличии сотрудника с видом занятости \"Основное место работы\" или \"Внешнее совместительство\".", logPrefix, employee.Id, mainEmployee.Id);
                managerRole.RecipientLinks.Remove(managerRole.RecipientLinks.Where(x => Recipients.Equals(x.Member, employee)).FirstOrDefault());
              }
              else
                KedoAccountProcessing(model, mainEmployee);
            }
            else
              Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотрудник уже подключён к личному кабинету.", logPrefix, employee.Id);
            
            Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотрудник включён в роль \"{2}\".", logPrefix, employee.Id, managerRole.Name);
          }
          catch (Exception ex)
          {
            Logger.ErrorFormat("{0}. ID сотрудника: {1}. Ошибка при подключении сотрудника к ЛК. Текст ошибки: {2}. StackTrace: {3}.", logPrefix, employee.Id, ex.Message, ex.StackTrace);
            managerRole.RecipientLinks.Remove(managerRole.RecipientLinks.Where(x => Recipients.Equals(x.Member, employee)).FirstOrDefault());
          }
          
          Locks.Unlock(employee);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(employee);
          Logger.DebugFormat("{0}. ID сотрудника: {1}. Не удалось установить блокировку. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.",
                             logPrefix, employee.Id, lockInfo.OwnerName, lockInfo.LockTime);
          
          managerRole.RecipientLinks.Remove(managerRole.RecipientLinks.Where(x => Recipients.Equals(x.Member, employee)).FirstOrDefault());
          continue;
        }
      }
      
      #endregion
      
      // Сохранение роли.
      if (managerRole.State.IsChanged)
      {
        try
        {
          managerRole.Save();
          Logger.DebugFormat("{0}. Роль \"{1}\" успешно сохранена.", logPrefix, managerRole.Name);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. Не удалось сохранить роль \"{1}\". Текст ошибки: {2}. StackTrace: {3}.", logPrefix, managerRole.Name, ex.Message, ex.StackTrace);
        }
      }
      
      Locks.Unlock(managerRole);
      Logger.DebugFormat("{0}. Конец обработки линейных руководителей.", logPrefix);
    }

    /// <summary>
    /// Обновить номер телефона сотрудника через api Ids.
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="flowGuid">Гуид потока.</param>
    /// <returns>Ошибка. Возвращается пустая строка, если ошибок нет.</returns>
    public string ChangeEmployeePhoneNumber(string logPrefix, IEmployee employee, string flowGuid)
    {
      var errorText = string.Empty;
      Logger.DebugFormat("{0}. ID сотрудника: {1}. Начало смены номера телефона сотрудника в ЛК.", logPrefix, employee.Id);
      try
      {
        var personUid = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetUidPerson(employee.Person);
        var idSUrl = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings().IdentityServiceAddress;
        var token = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetAnAuthenticationTokenCurrentSystem();
        // Cоздать структуру для формирования json-контента.
        var updatePhoneJson = DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.UpdateEssUserJson.Create();
        var userClaimEssJson = DirRX.OverrideSolution.Module.EssPlatform.Structures.Module.Claim.Create();
        userClaimEssJson.DirectumRX_PersonId = employee.Person.Id;
        userClaimEssJson.DirectumRX_Username = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetUserLogin(employee.Person.Id, Users.As(employee));
        userClaimEssJson.phone = employee.PersonalPhoneDirRX;
        updatePhoneJson.phoneNumber = employee.PersonalPhoneDirRX;
        updatePhoneJson.claims = userClaimEssJson;
        var requestResult = DirRX.EssPlatform.PublicFunctions.Module.Remote.RunPatchRequest(idSUrl, String.Format(Constants.Module.PatchEssUser, personUid),
                                                                                            DirRX.EssPlatform.PublicFunctions.Module.SerializedToJson(updatePhoneJson)
                                                                                            .Replace("DirectumRX_PersonId", "DirectumRX/PersonId")
                                                                                            .Replace("DirectumRX_Username", "DirectumRX/Username"), token);
        var statusCode = requestResult.StatusCode;
        if (statusCode == (int)HttpStatusCode.NoContent)
        {
          Logger.DebugFormat("{0}. ID сотрудника: {1}. Завершение смены номера телефона сотрудника в ЛК.", logPrefix, employee.Id);
          return errorText;
        }
        else
          errorText = DirRX.NNIntegration.Resources.UpdateESSPhoneNumberExceptionFormat(statusCode, requestResult.RequestMessage);
      }
      catch (Exception ex)
      {
        errorText = string.Format("Текст ошибки: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
      }
      
      if (!string.IsNullOrEmpty(errorText))
      {
        Logger.ErrorFormat("{0}. ID сотрудника: {1}. Не удалось изменить номер телефона сотрудника в ЛК. {2}", logPrefix, employee.Id, errorText);
        
        if (!string.IsNullOrEmpty(flowGuid))
        {
          var responsible = NNIntegration.PublicFunctions.Module.GetFlowResponsible(flowGuid);
          DirRX.NNIntegration.PublicFunctions.Module.SendNotificationToResponsible(responsible,
                                                                                   DirRX.NNIntegration.Resources.UpdateESSPhoneNumberExceptionNotificationSubjectFormat(employee.Name),
                                                                                   errorText);
        }
      }
      
      return errorText;
    }
    #endregion

    #region Получение записей справочников орг. структуры по SAP ID.

    /// <summary>
    /// Получить НОР по SAP ID.
    /// </summary>
    /// <param name="sapId">SAP ID.</param>
    /// <returns>НОР.</returns>
    [Public]
    public IBusinessUnit GetBusinessUnitBySAPId(string sapId)
    {
      return BusinessUnits.GetAll(x => (x.SapOrganizationTypeDirRX + x.SapLegalEntityCodeDirRX) == sapId).FirstOrDefault();
    }

    /// <summary>
    /// Получить подразделение по SAP ID.
    /// </summary>
    /// <param name="sapId">SAP ID.</param>
    /// <returns>Подразделение.</returns>
    public IDepartment GetDepartmentBySAPId(string sapId)
    {
      return Departments.GetAll(x => x.SapIdDirRX == sapId).FirstOrDefault();
    }

    /// <summary>
    /// Получить руководителя подразделения по SAP ID должности.
    /// </summary>
    /// <param name="sapId">SAP ID должности руководителя.</param>
    /// <returns>Руководитель.</returns>
    public IEmployee GetDepartmentManagerBySAPId(string sapId)
    {
      return Employees.GetAll(x => x.JobTitle != null &&
                              JobTitles.As(x.JobTitle).SapIdDirRX == sapId &&
                              x.Status == OverrideSolution.Employee.Status.Active)
        .FirstOrDefault();
    }
    
    /// <summary>
    /// Получить действующее подразделение, к которому привязана должность руководителя.
    /// </summary>
    /// <param name="sapId">SAP ID должности руководителя.</param>
    /// <returns>Подразделение.</returns>
    public IDepartment GetDepartmentByManagerJobTitleSAPId(string sapId)
    {
      return Departments.GetAll(x => x.ManagerJobTitleSapIdDirRX == sapId && x.Status == OverrideSolution.JobTitle.Status.Active).FirstOrDefault();
    }
    
    /// <summary>
    /// Проверить наличие закрепленных сотрудников за должностью.
    /// </summary>
    /// <param name="sapId">SAP ID должности.</param>
    /// <returns>Результат проверки. True - если есть сотрудники, false - если нет, null - если должность не найдена по SAP ID.</returns>
    /// <remarks>Проверка выполняется по признаку "Есть сотрудники на должности" в справочнике Должности.</remarks>
    public bool? CheckJobTitleEmployeeExist(string sapId)
    {
      var isExists = JobTitles.GetAll(x => x.SapIdDirRX == sapId)
        .Select(x => x.IsEmployeeExistsDirRX)
        .FirstOrDefault();

      return isExists == default ? null : isExists;
    }

    /// <summary>
    /// Получить должность по SAP ID.
    /// </summary>
    /// <param name="sapId">SAP ID.</param>
    /// <returns>Подразделение.</returns>
    public IJobTitle GetJobTitleBySAPId(string sapId)
    {
      return JobTitles.GetAll(x => x.SapIdDirRX == sapId).FirstOrDefault();
    }

    /// <summary>
    /// Получить сотрудника по табельному номеру.
    /// </summary>
    /// <param name="personnelNumber">Таб. номер.</param>
    /// <returns>Сотрудник.</returns>
    public IEmployee GetEmployeeByPersonnelNumber(string personnelNumber)
    {
      return Employees.GetAll(x => x.PersonnelNumber == personnelNumber).FirstOrDefault();
    }

    #endregion

    #region Изменение состояния записей справочников орг. структуры.

    /// <summary>
    /// Изменение состояния для действующих записей справочника "Сотрудники".
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    public void UpdateActiveEmployeeRecords(string logPrefix)
    {
      #region Активация сотрудников
      var activeEmployees = Employees.GetAll().Where(e => e.RecordStartDateDirRX.HasValue && Calendar.Now.Date >= e.RecordStartDateDirRX &&
                                                     e.RecordExpirationDateDirRX.HasValue && Calendar.Now.Date <= e.RecordExpirationDateDirRX &&
                                                     e.Status == DirRX.OverrideSolution.Employee.Status.Closed);
      Logger.DebugFormat("{0}. Активация сотрудников. Всего необходимо перевести сотрудников в состояние \"Действующий\": {1}.", logPrefix, activeEmployees.Count());
      foreach (var employee in activeEmployees)
      {
        Logger.DebugFormat("{0}. Активация сотрудников. ID сотрудника: {1}. Старт обработки записи. Дата начала действия: {2}. Дата окончания действия: {3}.",
                           logPrefix, employee.Id, employee.RecordStartDateDirRX.ToString(), employee.RecordExpirationDateDirRX.ToString());
        
        // Проверить для сотрудника существование других действующих сотрудников с основным местом работы.
        if(employee.EmploymentType == EmploymentType.MainPlace && HasActiveEmployeesWithMainPlace(employee))
        {
          Logger.ErrorFormat("{0}. Активация сотрудников. ID сотрудника: {1}. У сотрудника обнаружены другие действующие сотрудники с видом занятости - основное место работы. Он не будет переведён в состояние \"Действующий\".",
                             logPrefix, employee.Id);
          continue;
        }
        
        
        var skipProcessing = false;
        
        // Транзакция для активации сотрудника.
        var executionResult = Transactions.Execute(() =>
                                                   {
                                                     var isLocked = Locks.TryLock(employee);
                                                     if (!isLocked)
                                                     {
                                                       var lockInfo = Locks.GetLockInfo(employee);
                                                       Logger.DebugFormat("{0}. Активация сотрудников. ID сотрудника: {1}. Не удалось установить блокировку. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.", logPrefix, employee.Id,
                                                                          lockInfo.OwnerName, lockInfo.LockTime);
                                                       skipProcessing = true;
                                                     }
                                                     
                                                     if (!skipProcessing)
                                                     {
                                                       try
                                                       {
                                                         employee.Status = DirRX.OverrideSolution.Employee.Status.Active;
                                                         
                                                         // Если сотрудник - внутренний совместитель, то необходимо проверить наличие замещения для основного сотрудника.
                                                         if (employee.EmploymentType == OverrideSolution.Employee.EmploymentType.InternalConcurr)
                                                         {
                                                           // Поиск информации по замещению на внутреннего совместителя.
                                                           var substitutionInfo = FindSubstitutionInfo(logPrefix, employee);
                                                           
                                                           // Если сотрудник - внутренний совместитель и для него в системе нет действующего сотрудника с основным местом работы, то оставляем совместителя закрытым.
                                                           if (substitutionInfo.Substitute == null)
                                                           {
                                                             Logger.DebugFormat("{0}. Активация сотрудников. ID сотрудника: {1}. Сотрудник - внутренний совместитель и для него в системе нет действующего сотрудника с основным местом работы.", logPrefix, employee.Id);
                                                             skipProcessing = true;
                                                           }
                                                           
                                                           // Если сотрудник - внутренний совместитель, для которого существует действующий сотрудник с основным местом работы, и в системе нет замещения, то его необходимо создать.
                                                           if (substitutionInfo.Substitute != null && !substitutionInfo.HasSubstitution)
                                                           {
                                                             Logger.DebugFormat("{0}. Активация сотрудников. ID сотрудника: {1}. Сотрудник - внутренний совместитель и для него в системе не настроено замещение. Будет создано замещение на основного сотрудника (ID: {2}).",
                                                                                logPrefix, employee.Id, substitutionInfo.Substitute.Id);
                                                             
                                                             CreateSubstitution(logPrefix, substitutionInfo);
                                                           }
                                                         }
                                                         
                                                         // Сохранение сотрудника.
                                                         if (!skipProcessing)
                                                           employee.Save();
                                                       }
                                                       catch (Exception ex)
                                                       {
                                                         Logger.ErrorFormat("{0}. Активация сотрудников. ID сотрудника: {1}. Ошибка при выполнении транзакции для активации сотрудника. Ошибка: {2}. StackTrace: {3}.",
                                                                            logPrefix, employee.Id, ex.Message, ex.StackTrace);
                                                         throw ex;
                                                       }
                                                       finally
                                                       {
                                                         Locks.Unlock(employee);
                                                       }
                                                     }
                                                   });
        if (!skipProcessing)
        {
          if (executionResult)
          {
            Logger.DebugFormat("{0}. Активация сотрудников. ID сотрудника: {1}. Изменился статус. Старый статус: {2}. Новый статус: {3}", logPrefix, employee.Id,
                               Employees.Info.Properties.Status.GetLocalizedValue(OverrideSolution.Employee.Status.Closed),
                               Employees.Info.Properties.Status.GetLocalizedValue(employee.Status));
            
            // Если сотрудник - сотрудник с основным местом работы, то необходимо проверить наличие замещений для его совместителей.
            if (employee.EmploymentType == OverrideSolution.Employee.EmploymentType.MainPlace)
              UpdateSubstitutionsForMainEmployee(logPrefix, employee);
          }
          else
            Logger.ErrorFormat("{0}. Активация сотрудников. ID сотрудника: {1}. Не удалось выполнить транзакцию для активации сотрудника.", logPrefix, employee.Id);
        }
      }
      #endregion
    }

    /// <summary>
    /// Изменение состояния для закрытых записей справочника "Сотрудники".
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    public void UpdateClosedEmployeeRecords(string logPrefix)
    {
      #region Закрытие сотрудников
      var closedEmployees = Employees.GetAll().Where(e => e.RecordExpirationDateDirRX.HasValue && e.RecordExpirationDateDirRX < Calendar.Now.Date && e.Status == DirRX.OverrideSolution.Employee.Status.Active);
      Logger.DebugFormat("{0}. Закрытие сотрудников. Всего необходимо закрыть сотрудников: {1}.", logPrefix, closedEmployees.Count());
      foreach (var employee in closedEmployees)
      {
        var skipProcessing = false;
        Logger.DebugFormat("{0}. Закрытие сотрудников. ID сотрудника: {1}. Старт обработки записи. Дата начала действия: {2}. Дата окончания действия: {3}.",
                           logPrefix, employee.Id, employee.RecordStartDateDirRX.ToString(), employee.RecordExpirationDateDirRX.ToString());
        
        var isLocked = Locks.TryLock(employee);
        if (!isLocked)
        {
          var lockInfo = Locks.GetLockInfo(employee);
          Logger.DebugFormat("{0}. Закрытие сотрудников. ID сотрудника: {1}. Не удалось установить блокировку. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.", logPrefix, employee.Id,
                             lockInfo.OwnerName, lockInfo.LockTime);
          skipProcessing = true;
        }
        
        if (!skipProcessing)
        {
          var isDisconected = employee.PersonalAccountStatusDirRX == OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent;
          var mainEmployee = EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(employee.Person.Id, null);
          var isMainPlace = Employees.Equals(mainEmployee, employee);
          
          // Если сотрудник - основной сотрудник, который подключён ЛК.
          if (!isDisconected && isMainPlace)
          {
            Logger.DebugFormat("{0}. Закрытие сотрудников. ID сотрудника: {1}. Сотрудник является основным и подключён к ЛК. Будет выполнено его отключение от ЛК.", logPrefix, employee.Id);
            try
            {
              employee.NeedClearPhoneDirRX = true;
              DirRX.OverrideSolution.PublicFunctions.Employee.DisconnectEssEmployee(employee);
              isDisconected = true;
            }
            catch (Exception ex)
            {
              Logger.ErrorFormat("{0}. Закрытие сотрудников. ID сотрудника: {1}. Не удалось отключить сотрудника от ЛК. Ошибка: {2}. StackTrace: {3}.",
                                 logPrefix, employee.Id, ex.Message, ex.StackTrace);
            }
          }
          // Если сотрудник - сотрудник-совместитель, который подключён ЛК.
          else if (!isDisconected && !isMainPlace)
            Logger.DebugFormat("{0}. Закрытие сотрудников. ID сотрудника: {1}. Сотрудник является совместителем и подключён к ЛК. Перед его закрытием отключение от ЛК не требуется.", logPrefix, employee.Id);
          // Если сотрудник - сотрудник не подключён к ЛК.
          else if (isDisconected)
            Logger.DebugFormat("{0}. Закрытие сотрудников. ID сотрудника: {1}. Сотрудник не подключён к ЛК. Перед его закрытием отключение от ЛК не требуется.", logPrefix, employee.Id);
          
          // Закрыть сотрудника возможно, если он - совместитель или если он отключён от ЛК.
          try
          {
            var canClose = !isMainPlace || isDisconected;
            if (canClose)
            {
              employee.Status = DirRX.OverrideSolution.Employee.Status.Closed;
              employee.Save();
            }
          }
          catch (Exception ex)
          {
            Logger.ErrorFormat("{0}. Закрытие сотрудников. ID сотрудника: {1}. Ошибка при сохранении сотрудника. Ошибка: {2}. StackTrace: {3}.",
                               logPrefix, employee.Id, ex.Message, ex.StackTrace);
          }
          finally
          {
            Locks.Unlock(employee);
            Logger.DebugFormat("{0}. Закрытие сотрудников. ID сотрудника: {1}. Конец обработки сотрудника. Старый статус: {2}. Новый статус: {3}", logPrefix, employee.Id,
                               Employees.Info.Properties.Status.GetLocalizedValue(OverrideSolution.Employee.Status.Active),
                               Employees.Info.Properties.Status.GetLocalizedValue(employee.Status));
          }
        }
      }
      #endregion
      
      #region Актуализация замещений
      // Удаление замещений для совместителей после актуализиции состояний записи сотрудников.
      var substitutions = Sungero.CoreEntities.Substitutions
        .GetAll().Where(s => (s.Substitute.Status == OverrideSolution.Employee.Status.Closed ||
                              s.User.Status == OverrideSolution.Employee.Status.Closed) &&
                        s.Comment == PublicConstants.Module.CommentSubstitute);
      
      // Блокировка замещений.
      Logger.DebugFormat("{0}. Удаление устаревших замещений. Всего устаревших замещений: {1}.", logPrefix, substitutions.Count());
      foreach (var substitution in substitutions)
      {
        var isLocked = Locks.TryLock(substitution);
        if (!isLocked)
        {
          var lockInfo = Locks.GetLockInfo(substitution);
          Logger.DebugFormat("{0}. Удаление устаревших замещений. ID замещения: {1}. Не удалось установить блокировку. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.", logPrefix, substitution.Id,
                             lockInfo.OwnerName, lockInfo.LockTime);
          continue;
        }
        
        try
        {
          Sungero.CoreEntities.Substitutions.Delete(substitution);
          Logger.DebugFormat("{0}. Удаление устаревших замещений. ID замещения: {1}. Замещение успешно удалено. Основной сотрудник (ИД: {2}, Состояние: {3}). Совместитель (ИД: {4}, Состояние: {5}).",
                             logPrefix, substitution.Id, substitution.Substitute.Id, Users.Info.Properties.Status.GetLocalizedValue(substitution.Substitute.Status),
                             substitution.User.Id, Users.Info.Properties.Status.GetLocalizedValue(substitution.User.Status));
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. Удаление устаревших замещений. ID замещения: {1}. Не удалось удалить замещение. Текст ошибки: {2}. StackTrace: {3}.",
                             logPrefix, substitution.Id, ex.Message, ex.StackTrace);
        }
        finally
        {
          Locks.Unlock(substitution);
        }
      }
      #endregion
    }

    /// <summary>
    /// Изменение состояния для закрытых записей справочника "Подразделения".
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    public void UpdateClosedDepartmentRecords(string logPrefix)
    {
      var closedDepartments = Departments.GetAll().Where(e => e.RecordExpirationDateDirRX.HasValue && e.RecordExpirationDateDirRX < Calendar.Now.Date && e.Status == DirRX.OverrideSolution.Department.Status.Active);
      Logger.DebugFormat("{0}. Закрытие подразделений. Всего необходимо закрыть подразделений: {1}.", logPrefix, closedDepartments.Count());
      foreach (var department in closedDepartments)
      {
        Logger.DebugFormat("{0}. Закрытие подразделений. ID подразделения: {1}. Старт обработки записи. Дата начала действия: {2}. Дата окончания действия: {3}.",
                           logPrefix, department.Id, department.RecordStartDateDirRX.ToString(), department.RecordExpirationDateDirRX.ToString());
        
        var isLocked = Locks.TryLock(department);
        if (isLocked)
        {
          department.Status = DirRX.OverrideSolution.Department.Status.Closed;
          var result = Transactions.Execute(() =>
                                            {
                                              department.Save();
                                            });
          if (result)
            Logger.DebugFormat("{0}. Закрытие подразделений. ID подразделения: {1}. Изменился статус. Старый статус: {2}. Новый статус: {3}", logPrefix, department.Id,
                               Departments.Info.Properties.Status.GetLocalizedValue(OverrideSolution.Department.Status.Active),
                               Departments.Info.Properties.Status.GetLocalizedValue(department.Status));
          else
            Logger.ErrorFormat("{0}. Закрытие подразделений. ID подразделения: {1}. Не удалось сохранить запись справочника Подразделения.",
                               logPrefix, department.Id);
          Locks.Unlock(department);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(department);
          Logger.DebugFormat("{0}. Закрытие подразделений. ID подразделения: {1}. Не удалось установить блокировку. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.", logPrefix, department.Id,
                             lockInfo.OwnerName, lockInfo.LockTime);
        }
      }
    }

    /// <summary>
    /// Изменение состояния для закрытых записей справочника "Должности".
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    public void UpdateClosedJobTitleRecords(string logPrefix)
    {
      var closedJobTitles = JobTitles.GetAll().Where(e => e.RecordExpirationDateDirRX.HasValue && e.RecordExpirationDateDirRX < Calendar.Now.Date && e.Status == DirRX.OverrideSolution.JobTitle.Status.Active);
      Logger.DebugFormat("{0}. Закрытие должностей. Всего необходимо закрыть должностей: {1}.", logPrefix, closedJobTitles.Count());
      foreach (var jobTitle in closedJobTitles)
      {
        Logger.DebugFormat("{0}. Закрытие должностей. ID должности: {1}. Старт обработки записи. Дата начала действия: {2}. Дата окончания действия: {3}.",
                           logPrefix, jobTitle.Id, jobTitle.RecordStartDateDirRX.ToString(), jobTitle.RecordExpirationDateDirRX.ToString());
        
        var isLocked = Locks.TryLock(jobTitle);
        if (isLocked)
        {
          jobTitle.Status = DirRX.OverrideSolution.JobTitle.Status.Closed;
          var result = Transactions.Execute(() =>
                                            {
                                              jobTitle.Save();
                                            });
          if (result)
            Logger.DebugFormat("{0}. Закрытие должностей. ID должности: {1}. Изменился статус. Старый статус: {2}. Новый статус: {3}", logPrefix, jobTitle.Id,
                               JobTitles.Info.Properties.Status.GetLocalizedValue(OverrideSolution.JobTitle.Status.Active),
                               JobTitles.Info.Properties.Status.GetLocalizedValue(jobTitle.Status));
          else
            Logger.ErrorFormat("{0}. Закрытие должностей. ID должности: {1}. Не удалось сохранить запись справочника Должности.",
                               logPrefix, jobTitle.Id);
          Locks.Unlock(jobTitle);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(jobTitle);
          Logger.DebugFormat("{0}. Закрытие должностей. ID должности: {1}. Не удалось установить блокировку. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.", logPrefix, jobTitle.Id,
                             lockInfo.OwnerName, lockInfo.LockTime);
        }
      }
    }

    #endregion

    #region Другие функции ФП "Обновление организационной структуры".
    
    /// <summary>
    /// Получить список действующих записей справочника Группы МП.
    /// </summary>
    /// <param name="code">Код группы МП.</param>
    [Public]
    public virtual IQueryable<NNOtherProcesses.ILrdPaymentMPGroup> GetActiveMPGroups(string code)
    {
      return NNOtherProcesses.LrdPaymentMPGroups.GetAll(x => x.Code == code && x.Status == NNOtherProcesses.LrdPaymentMPGroup.Status.Active);
    }
    
    /// <summary>
    /// Проверить для сотрудника существование других действующих сотрудников с основным местом работы, у которых вид занятости - основное место работы.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Результат проверки.</returns>
    public virtual bool HasActiveEmployeesWithMainPlace(IEmployee employee)
    {
      // Найти других действующих сотрудников персоны, у которых вид занятости - основное место работы.
      var employeesWithMainPlace = Employees.GetAll(w =>
                                                    w.Status == OverrideSolution.Employee.Status.Active &&
                                                    w.Person.Id == employee.Person.Id &&
                                                    w.Id != employee.Id &&
                                                    w.EmploymentType == EmploymentType.MainPlace);
      
      return employeesWithMainPlace.Any();
    }
    
    /// <summary>
    /// Проверить, нужно ли подключить ЛР к личному кабинету.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Реузльтат проверки.</returns>
    public virtual bool NeedConnectLineManager(IEmployee employee)
    {
      var isConnected = employee.PersonalAccountStatusDirRX == DirRX.OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted;
      var isMainEmployeeWithoutLogin = employee.EmploymentType == DirRX.OverrideSolution.Employee.EmploymentType.MainPlace && employee.Login == null;
      var isPartTimeWithoutLogin = employee.EmploymentType != DirRX.OverrideSolution.Employee.EmploymentType.MainPlace &&
        Employees.GetAll(x => x.Person.Id == employee.Person.Id && x.Status == DirRX.OverrideSolution.Employee.Status.Active)
        .All(x => x.Login == null || x.Login.Status == Sungero.CoreEntities.DatabookEntry.Status.Closed);
      var hasClosedLogin = employee.Login?.Status == Sungero.CoreEntities.DatabookEntry.Status.Closed;
      
      Logger.DebugFormat("NeedConnectLineManager. ИД линейного руководителя: {0}. ЛР подключён к ЛК: {1}. ЛР - основной сотрудник без УЗ: {2}. ЛР - совместитель без УЗ (учитываются в том числе другие действующие сотрудники персоны): {3}. УЗ есть и закрыта: {4}.",
                         employee.Id, isConnected, isMainEmployeeWithoutLogin, isPartTimeWithoutLogin, hasClosedLogin);
      
      // ЛР не подключен к ЛК ИЛИ (у ЛР нет УЗ и он явялется основным) ИЛИ (ЛР - совместитель и нет других сотрудников его персоны с УЗ) ИЛИ (УЗ есть и она - закрытая)
      return !isConnected || isMainEmployeeWithoutLogin || isPartTimeWithoutLogin || hasClosedLogin;
    }

    /// <summary>
    /// Найти и заполнить руководителя подразделения.
    /// </summary>
    /// <param name="prefix">Префикс.</param>
    /// <param name="department">Подразделение</param>
    /// <param name="jobState">Состояние ФП "Обновление орг. структуры."</param>
    /// <remarks>Если руководитель - не найден, то запускается повторная обработка.</remarks>
    public virtual void SetDepartmentManager(string prefix, IDepartment department, NNIntegration.Structures.Module.ICompanySynchJobState jobState)
    {
      var manager = Functions.Module.GetDepartmentManagerBySAPId(department.ManagerJobTitleSapIdDirRX);
      if (!Employees.Equals(department.Manager, manager))
      {
        Logger.DebugFormat("{0}. Изменился руководитель подразделения (SAP ID: {1}), закреплённый за должностью.", prefix, department.SapIdDirRX);
        department.Manager = manager;
      }
      
      // Если руководитель не найден, то отправить запись на повторную обработку.
      if (manager == null)
        jobState.RetryMessage = string.Format("Не найден действующий руководитель с должностью SAP ID: {0}", department.ManagerJobTitleSapIdDirRX);
      else
        Logger.DebugFormat("{0}. Таб. номер нового руководителя подразделения (SAP ID: {1}): {2}.", prefix, department.SapIdDirRX, manager.PersonnelNumber);
    }
    
    /// <summary>
    /// Проверить что, что SAP ID является OUSAPID действующей нашей организации.
    /// </summary>
    /// <param name="sapId">SAP ID</param>
    /// <returns>Признак того, что SAP ID является OUSAPID НОР.</returns>
    public bool CheckBusinessUnitOUSapId(string sapId)
    {
      return BusinessUnits.GetAll().Where(x => x.Status == OverrideSolution.BusinessUnit.Status.Active && x.OUSAPIDDirRX == sapId).Any();
    }
    
    /// <summary>
    /// Получить список действующих совместителей для сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник с основным местом работы.</param>
    public IQueryable<IEmployee> GetInnerWorkersForEmployee(IEmployee employee)
    {
      return Employees.GetAll().Where(x => x.Status == OverrideSolution.Employee.Status.Active &&
                                      x.EmploymentType == OverrideSolution.Employee.EmploymentType.InternalConcurr &&
                                      People.Equals(employee.Person, x.Person));
    }

    /// <summary>
    /// Обновить замещения для совместителей сотрудника с основным местом работы.
    /// </summary>
    /// <param name="logPrefix">Префикс лога.</param>
    /// <param name="employee">Сотрудник с основным местом работы.</param>
    public void UpdateSubstitutionsForMainEmployee(string logPrefix, IEmployee employee)
    {
      var innerWorkers = GetInnerWorkersForEmployee(employee);
      
      Logger.DebugFormat("{0}. ID сотрудника: {1}. Всего действующих совместителей для сотрудника: {2}.", logPrefix, employee.Id, innerWorkers.Count());
      foreach (var worker in innerWorkers)
      {
        // Поиск информации по замещению на внутреннего совместителя.
        var substitutionInfo = FindSubstitutionInfo(logPrefix, worker);
        if (!substitutionInfo.HasSubstitution)
          CreateSubstitution(logPrefix, substitutionInfo);
      }
    }

    /// <summary>
    /// Поиск информации по замещению внутреннего совместителя.
    /// </summary>
    /// <param name="logPrefix">Лог префикс.</param>
    /// <param name="record">Сотрудник.</param>
    public NNIntegration.Structures.Module.IEmployeeSubstitutionStructure FindSubstitutionInfo(string logPrefix, IEmployee employee)
    {
      var substitutionInfo = Structures.Module.EmployeeSubstitutionStructure.Create();
      if (employee.Status == NNIntegration.EmployeeQueue.Status.Active &&
          employee.EmploymentType == OverrideSolution.Employee.EmploymentType.InternalConcurr)
      {
        substitutionInfo.InternalWorker = employee;
        Logger.DebugFormat("{0} ID сотрудника: {1}. Поиск замещения для внутреннего совместителя.", logPrefix, employee.Id);

        // Поиск сотрудника с основным местом работы для внутреннего совместителя.
        var substitute = Employees
          .GetAll()
          .Where(e => e.Status == DirRX.OverrideSolution.Employee.Status.Active &&
                 e.EmploymentType == DirRX.OverrideSolution.Employee.EmploymentType.MainPlace &&
                 People.Equals(e.Person, employee.Person)).FirstOrDefault();
        
        if (substitute == null)
        {
          Logger.DebugFormat("{0} ID сотрудника: {1}. Не найден сотрудник с основным местом работы для внутреннего совместителя.", logPrefix, employee.Id);
          return substitutionInfo;
        }
        
        substitutionInfo.Substitute = substitute;
        
        // Проверить наличие замещения.
        var hasSubstitution = Sungero.CoreEntities.Substitutions
          .GetAll()
          .Where(s => Employees.Equals(s.Substitute, substitute) &&
                 Users.Equals(s.User, employee) &&
                 s.Comment == PublicConstants.Module.CommentSubstitute).Any();
        
        if (hasSubstitution)
          Logger.DebugFormat("{0} ID внутреннего совместителя: {1}. ID сотрудника с основным местом работы: {2}. Замещение найдено.", logPrefix, employee.Id, substitute.Id);
        else
          Logger.DebugFormat("{0} ID внутреннего совместителя: {1}. ID сотрудника с основным местом работы: {2}. Замещение не найдено.", logPrefix, employee.Id, substitute.Id);
        
        substitutionInfo.HasSubstitution = hasSubstitution;
      }
      
      return substitutionInfo;
    }

    /// <summary>
    /// Создание замещения для сотрудника с основным местом работы.
    /// </summary>
    /// <param name="record">Запись очереди сотрудников.</param>
    /// <param name="logPrefix">Лог префикс.</param>
    public void CreateSubstitution(string logPrefix, NNIntegration.Structures.Module.IEmployeeSubstitutionStructure substitutionInfo)
    {
      // Сотрудник не является внутренним совместителем.
      if (substitutionInfo.InternalWorker == null)
        return;
      
      // Замещение для сотрудника с основным местом работы уже создано.
      if (substitutionInfo.Substitute != null && substitutionInfo.HasSubstitution)
      {
        Logger.DebugFormat("{0} ID внутреннего совместителя: {1}. ID сотрудника с основным местом работы: {2}. Замещение уже существует в системе.",
                           logPrefix, substitutionInfo.InternalWorker.Id, substitutionInfo.Substitute.Id);
        return;
      }
      
      var newSubstitute =  Sungero.CoreEntities.Substitutions.Create();
      newSubstitute.Substitute = substitutionInfo.Substitute;
      newSubstitute.User = substitutionInfo.InternalWorker;
      newSubstitute.Comment = PublicConstants.Module.CommentSubstitute;
      
      try
      {
        newSubstitute.Save();
        Logger.DebugFormat("{0} ID внутреннего совместителя: {1}. ID сотрудника с основным местом работы: {2}. Замещение успешно создано в системе.",
                           logPrefix, substitutionInfo.InternalWorker.Id, substitutionInfo.Substitute.Id);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0} ID внутреннего совместителя: {1}. ID сотрудника с основным местом работы: {2}. Ошибка при создании замещения. Текст ошибки: {3}. StackTrace: {4}.",
                           logPrefix, substitutionInfo.InternalWorker.Id, substitutionInfo.Substitute.Id, ex.Message, ex.StackTrace);
      }
    }

    /// <summary>
    /// Получить ответственного за ошибки для потока.
    /// </summary>
    /// <param name="flowGuid">Guid потока.</param>
    /// <returns>Ответственный.</returns>
    [Public]
    public IEmployee GetFlowResponsible(string flowGuid)
    {
      var flow = ExternalSystemFlows.GetAll(x => x.FlowGuid == flowGuid).FirstOrDefault();
      if (flow == null)
        Logger.ErrorFormat("Не найден интеграционный поток. Guid потока: {0}", flowGuid);
      
      return flow?.ResponsibleForErrors;
    }

    /// <summary>
    /// Отправить уведомление об ошибках обработки ответственному за cинхронизацию орг. структуры.
    /// </summary>
    /// <param name="responsible">Ответственный.</param>
    /// <param name="logPrefix">Префикс.</param>
    /// <param name="errors">Список записей очередей орг. структуры, в которых возникла ошибка обработки.</param>
    public void SendCompanySynchronizationErrorsNotification(IEmployee responsible, string logPrefix, List<ICompanyQueueBase> errors)
    {
      if (!errors.Where(x => x.ProcessingStatus == DirRX.NNIntegration.CompanyQueueBase.ProcessingStatus.Error).Any())
      {
        Logger.ErrorFormat("{0}. В списке нет записей очереди орг. структуры с ошибкой обработки. Уведомление об ошибках - не отправлено.", logPrefix);
        return;
      }
      
      if (responsible == null)
      {
        Logger.ErrorFormat("{0}. Не заполнен ответственный за синхронизации орг. структуры. Уведомление об ошибках - не отправлено.", logPrefix);
        return;
      }
      
      try
      {
        var notification = SendNotificationToResponsible(responsible,
                                                         DirRX.NNIntegration.Resources.CompanySynchronizationErrorsNotificationSubjectFormat(Calendar.Now),
                                                         Functions.Module.FormCompanySynchronizationErrorsText(errors));
        Logger.DebugFormat("{0}. NotificationId: {1}. Уведомление пользователю успешно отправлено.",
                           logPrefix, notification.Id);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. Ошибка при отправке уведомления. Message: {1}. StackTrace: {2}.",
                           logPrefix, ex.Message, ex.StackTrace);
      }
    }

    /// <summary>
    /// Получить список Id устаревших записей очереди орг. структуры.
    /// </summary>
    /// <param name="dbColumnName">Название столбца, в котором хранится SAP ID записи очереди.</param>
    /// <returns>Список Id устаревших записей.</returns>
    public static List<long> GetOldCompanyQueueRecordsId(string dbColumnName)
    {
      var idsList = new List<long>();
      using (var connection = SQL.CreateConnection())
      {
        var command = connection.CreateCommand();
        command.CommandText = string.Format(Queries.Module.GetOldCompanyQueueRecordsId, dbColumnName);
        using (var rds = command.ExecuteReader())
        {
          if (rds != null)
            while (rds.Read())
          {
            idsList.Add(rds.GetInt64(0));
          }
          
          rds.Close();
        };
      };
      
      return idsList;
    }

    /// <summary>
    /// Получить устаревшие записи очереди справочников орг. структуры.
    /// </summary>
    /// <returns>Записи очереди справочника Сотрудники.</returns>
    public IQueryable<ICompanyQueueBase> GetOldCompanyQueueRecords()
    {
      var departmentQueueRecordsId = GetOldCompanyQueueRecordsId(DepartmentQueues.Info.Properties.SapId.DBColumnName);
      var jobTitleQueueRecordsId = GetOldCompanyQueueRecordsId(JobTitleQueues.Info.Properties.SapId.DBColumnName);
      var employeeQueueRecordsId = GetOldCompanyQueueRecordsId(EmployeeQueues.Info.Properties.PersonnelNumber.DBColumnName);
      return CompanyQueueBases.GetAll().Where(x=>
                                              departmentQueueRecordsId.Contains(x.Id) ||
                                              jobTitleQueueRecordsId.Contains(x.Id) ||
                                              employeeQueueRecordsId.Contains(x.Id));
    }

    /// <summary>
    /// Сохранить запись очереди справочника орг. структуры.
    /// </summary>
    /// <param name="record">Запись очереди.</param>
    /// <param name="logPrefix">Префикс.</param>
    /// <returns>Результат сохранения. True - если запись сохранена. False - если запись не сохранена.</returns>
    public bool CompanyQueueRecordSave(ICompanyQueueBase record, string logPrefix)
    {
      
      var queueName = string.Empty;
      var sapId = string.Empty;
      if (DepartmentQueues.Is(record))
      {
        queueName = DepartmentQueues.Info.LocalizedName;
        sapId = DepartmentQueues.As(record).SapId;
      }
      if (JobTitleQueues.Is(record))
      {
        queueName = JobTitleQueues.Info.LocalizedName;
        sapId = JobTitleQueues.As(record).SapId;
      }
      if (EmployeeQueues.Is(record))
      {
        queueName = EmployeeQueues.Info.LocalizedName;
        sapId = EmployeeQueues.As(record).PersonnelNumber;
      }
      
      if (!record.State.IsChanged)
      {
        Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Запись очереди не была изменена. Текущий статус обработки: {4}.",
                           logPrefix, queueName, record.Id, sapId,
                           CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
        return true;
      }
      
      var originalStatus = CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.State.Properties.ProcessingStatus.OriginalValue);
      var result = Transactions.Execute(() =>
                                        {
                                          record.Save();
                                        });
      if (result)
        Logger.DebugFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Запись очереди - успешно сохранена. Предыдущий статус обработки: {4}. Новый статус обработки: {5}.",
                           logPrefix, queueName, record.Id, sapId,
                           originalStatus,
                           CompanyQueueBases.Info.Properties.ProcessingStatus.GetLocalizedValue(record.ProcessingStatus));
      else
        Logger.ErrorFormat("{0}. {1}. ID записи: {2}. SAP ID: {3}. Не удалось сохранить запись очереди.",
                           logPrefix, queueName, record.Id, sapId);
      
      return result;
    }

    /// <summary>
    /// Получить статус записи справочника по дате начала действия и дате окончания действия записи очереди орг. структуры.
    /// </summary>
    /// <param name="record">Запись очереди.</param>
    [Public]
    public Enumeration GetStatusByRecordDate(ICompanyQueueBase record)
    {
      var today = Calendar.Now.Date;
      var status = Sungero.CoreEntities.DatabookEntry.Status.Active;
      if (today >= record.RecordStartDate && (!record.RecordEndDate.HasValue || today <= record.RecordEndDate))
        status = Sungero.CoreEntities.DatabookEntry.Status.Active;
      else
        status = Sungero.CoreEntities.DatabookEntry.Status.Closed;
      
      Logger.DebugFormat("GetStatusByRecordDate. ID записи: {0}. Период действия сотрудника: {1} - {2}. Статус записи: {3}.",
                         record.Id,
                         record.RecordStartDate.HasValue ? record.RecordStartDate.Value.ToString("dd.MM.yyyy") : string.Empty,
                         record.RecordEndDate.HasValue ? record.RecordEndDate.Value.ToString("dd.MM.yyyy") : string.Empty,
                         status);
      return status;
    }

    /// <summary>
    /// Получить или создать страну по умолчанию.
    /// </summary>
    /// <returns>Страна.</returns>
    public Sungero.Commons.ICountry GetOrCreateDefaultCountry()
    {
      var country = Sungero.Commons.Countries.GetAll(x => x.Name == Constants.Module.DefaultCountryName &&
                                                     x.Code == Constants.Module.DefaultCountryCode).FirstOrDefault();
      if (country != null)
        return country;
      
      try
      {
        country = Sungero.Commons.Countries.Create();
        country.Name = Constants.Module.DefaultCountryName;
        country.Code = Constants.Module.DefaultCountryCode;
        country.Save();
        Logger.Debug("GetOrCreateDefaultCountry. Создана страна по умолчанию.");
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetOrCreateDefaultCountry. Не удалось создать страну по умолчанию. Текст ошибки: {0}, StackTrace: {1}.", ex.Message, ex.StackTrace);
      }
      
      return country;
    }

    /// <summary>
    /// Получить или создать регион по умолчанию.
    /// </summary>
    /// <returns>Регион.</returns>
    public Sungero.Commons.IRegion GetOrCreateDefaultRegion()
    {
      var region = Sungero.Commons.Regions.GetAll(x => x.Name == Constants.Module.DefaultRegionName).FirstOrDefault();
      if (region != null)
        return region;
      
      try
      {
        region = Sungero.Commons.Regions.Create();
        region.Name = Constants.Module.DefaultRegionName;
        region.Country = GetOrCreateDefaultCountry();
        region.Save();
        Logger.Debug("GetOrCreateDefaultRegion. Создан регион по умолчанию.");
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetOrCreateDefaultRegion. Не удалось создать регион по умолчанию. Текст ошибки: {0}, StackTrace: {1}.", ex.Message, ex.StackTrace);
      }
      
      return region;
    }

    /// <summary>
    /// Получить или создать регион.
    /// </summary>
    /// <param name="code">Код региона.</param>
    /// <returns>Регион.</returns>
    public Sungero.Commons.IRegion GetOrCreateRegion(string code)
    {
      if (string.IsNullOrEmpty(code))
        return Sungero.Commons.Regions.Null;
      
      var region = Sungero.Commons.Regions.GetAll(x => x.Code == code).FirstOrDefault();
      if (region != null)
        return region;
      
      try
      {
        region = Sungero.Commons.Regions.Create();
        region.Name = code;
        region.Code = code;
        region.Country = Sungero.Commons.Countries.GetAll(x => x.Code == Constants.Module.RussianFederationCode).FirstOrDefault();
        region.Save();
        Logger.DebugFormat("GetOrCreateRegion. Код региона: {0}. Создан новый регион.", code);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetOrCreateRegion. Код региона: {0}. Не удалось создать регион. Текст ошибки: {1}, StackTrace: {2}.", code, ex.Message, ex.StackTrace);
      }
      
      return region;
    }

    /// <summary>
    /// Cоздать населённый пункт.
    /// </summary>
    /// <param name="name">Наименование.</param>
    /// <param name="region">Регион.</param>
    /// <returns>Населённый пункт.</returns>
    public Sungero.Commons.ICity GetOrCreateCity(string name, Sungero.Commons.IRegion region)
    {
      if (string.IsNullOrEmpty(name))
      {
        Logger.DebugFormat("GetOrCreateRegion. Наименование: {0}. Не заполнено наименование населённого пункта.", name);
        return Sungero.Commons.Cities.Null;
      }
      
      if (region == null)
      {
        Logger.DebugFormat("GetOrCreateRegion. Наименование: {0}. Не заполнен регион населённого пункта.", name);
        return Sungero.Commons.Cities.Null;
      }
      
      var city = Sungero.Commons.Cities.GetAll(x => x.Name == name && Sungero.Commons.Regions.Equals(x.Region, region)).FirstOrDefault();
      if (city != null)
        return city;
      
      
      try
      {
        city = Sungero.Commons.Cities.Create();
        city.Name = name;
        city.Region = region;
        city.Save();
        Logger.DebugFormat("GetOrCreateRegion. Наименование: {0}. Создан новый населённый пункт.", name);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetOrCreateCity. Наименование: {0}. Не удалось создать населённый пункт. Текст ошибки: {1}, StackTrace: {2}.", name, ex.Message, ex.StackTrace);
      }
      
      return city;
    }
    
    /// <summary>
    /// Обновление руководителей подразделений при смене должности сотрудника.
    /// </summary>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="oldJobTitle">Должность сотрудника до обновления.</param>
    /// <remarks>Если должность сотрудника поменялась, и он был/будет руководителем, то нужно обновить руководителя в соответствующем подразделении.</remarks>
    public void UpdateDepartmentManagers(string logPrefix, OverrideSolution.IEmployee employee, OverrideSolution.IJobTitle oldJobTitle)
    {
      if (oldJobTitle != null)
        SetDepartmentManagerByJobTitle(logPrefix, null, oldJobTitle.SapIdDirRX);
      
      var newJobTitle = OverrideSolution.JobTitles.As(employee.JobTitle);
      if(newJobTitle != null)
        SetDepartmentManagerByJobTitle(logPrefix, employee, newJobTitle.SapIdDirRX);
    }
    
    /// <summary>
    /// Установка нового руководителя подразделения по SapID руководящей должности.
    /// </summary>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <param name="newManager">Новый руководитель (если известен).</param>
    /// <param name="jobTitleSapID">SapID должности.</param>
    public void SetDepartmentManagerByJobTitle(string logPrefix, OverrideSolution.IEmployee newManager, string jobTitleSapID)
    {
      var jobTitleDepartment = Functions.Module.GetDepartmentByManagerJobTitleSAPId(jobTitleSapID);
      if (jobTitleDepartment != null)
      {
        Logger.DebugFormat("{0}. Изменение руководителя подразделения с SapID: {1}.", logPrefix, jobTitleDepartment.SapIdDirRX);
        
        if (newManager == null)
          newManager = Functions.Module.GetDepartmentManagerBySAPId(jobTitleSapID);
        
        var oldManager = jobTitleDepartment.Manager;
        if (!Equals(oldManager, newManager))
        {
          try
          {
            jobTitleDepartment.Manager = newManager;
            jobTitleDepartment.Save();
            Logger.DebugFormat("{0}. Руководитель изменен. Таб. номер предыдущего руководителя: {1}. Таб. номер нового руководителя: {2}",
                               logPrefix, oldManager?.PersonnelNumber, newManager?.PersonnelNumber);
          }
          catch (Exception ex)
          {
            Logger.ErrorFormat("{0}. Не удалось сохранить запись справочника Подразделения. SapID: {1}. Ошибка: {2}. StackTrace: {3}.",
                               logPrefix, jobTitleDepartment.SapIdDirRX, ex.Message, ex.StackTrace);
          }
        }
      }
    }
    
    #endregion
    
    #region Обработка УЗ.
    
    /// <summary>
    /// Обработать УЗ после синхронизации орг. структуры.
    /// </summary>
    /// <param name="logPrefx">Префикс лога.</param>
    public void ProcessEmployeesLogin(string logPrefix)
    {
      Logger.DebugFormat("{0}. Начало обработки УЗ сотрудников.", logPrefix);
      
      #region Очистка шаблонных УЗ у сотрудников.
      
      var hrLoginPrefix = DirRX.EssPlatform.PublicConstants.Module.TemplateAccountName
        .Substring(0, DirRX.EssPlatform.PublicConstants.Module.TemplateAccountName.IndexOf('{')).Trim();
      
      var clearLoginEmployees = Employees.GetAll().Where(x =>
                                                         BusinessUnits.As(x.BusinessUnit).UseESSDirRX != true &&
                                                         (x.PersonalAccountStatusDirRX != OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted ||
                                                          (x.PersonalAccountStatusDirRX == OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted &&
                                                           x.EmploymentType == OverrideSolution.Employee.EmploymentType.ExternalConcurr)) &&
                                                         x.Login != null && x.Person != null &&
                                                         x.Login.LoginName == hrLoginPrefix + x.Person.Id.ToString());
      
      foreach (var employee in clearLoginEmployees)
      {
        
        // Установить блокировку.
        Locks.TryLock(employee);
        if (!Locks.GetLockInfo(employee).IsLockedByMe)
        {
          var lockInfo = Locks.GetLockInfo(employee);
          Logger.DebugFormat("{0}. ИД сотрудника: {1}. Не удалось установить блокировку. Сотрудник заблокирован пользователем: {2}. Время установки блокировки: {3}.",
                             logPrefix, employee.Id, lockInfo.OwnerName, lockInfo.LockTime);
          continue;
        }
        
        try
        {
          employee.Login = Logins.Null;
          var result = Transactions.Execute(() =>
                                            {
                                              employee.Save();
                                            });
          if (result)
            Logger.DebugFormat("{0}. ID сотрудника: {1}. Выполнилась очистка УЗ у сотрудника.", logPrefix, employee.Id);
          else
            Logger.ErrorFormat("{0}. ID сотрудника: {1}. Не удалось сохранить сотрудника при очистки УЗ.", logPrefix, employee.Id);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. ID сотрудника: {1}.  Ошибка при очистки УЗ сотрудника. Ошибка: {2}. StackTrace: {3}.", logPrefix, employee.Id, ex.Message, ex.StackTrace);
        }
        finally
        {
          Locks.Unlock(employee);
        }
      }

      #endregion
      
      #region Присвоение шаблонных учетных записей сотрудникам
      var addLoginEmployees = Employees.GetAll().Where(x =>
                                                       x.PersonalAccountStatusDirRX == OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted &&
                                                       x.ConsentHRPro == OverrideSolution.Employee.ConsentHRPro.Signed &&
                                                       BusinessUnits.As(x.BusinessUnit).UseESSDirRX == true &&
                                                       x.EmploymentType == OverrideSolution.Employee.EmploymentType.MainPlace &&
                                                       x.Login == null);
      foreach (var employee in addLoginEmployees)
      {
        // Установить блокировку.
        Locks.TryLock(employee);
        if (!Locks.GetLockInfo(employee).IsLockedByMe)
        {
          var lockInfo = Locks.GetLockInfo(employee);
          Logger.DebugFormat("{0}. ИД сотрудника: {1}. Не удалось установить блокировку. Сотрудник заблокирован пользователем: {2}. Время установки блокировки: {3}.",
                             logPrefix, employee.Id, lockInfo.OwnerName, lockInfo.LockTime);
          continue;
        }
        
        try
        {
          employee.Login = EssPlatform.PublicFunctions.Module.ChangeOrCreateMainEmployeesAccount(employee.Id);
          
          var result = Transactions.Execute(() =>
                                            {
                                              employee.Save();
                                            });
          if (result)
          {
            Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотруднику присвоена УЗ по шаблону для ЛК.", logPrefix, employee.Id);
            
            // Выпуск сертификата.
            if (OverrideSolution.PublicFunctions.Employee.NeedIssueCertificate(employee))
              OverrideSolution.PublicFunctions.Employee.IssueCertificate(employee);
          }
          else
            Logger.ErrorFormat("{0}. ID сотрудника: {1}. Не удалось сохранить сотрудника при присвоении ему УЗ ЛК.", logPrefix, employee.Id);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. ID сотрудника: {1}.  Ошибка при присвоении УЗ сотруднику. Ошибка: {2}. StackTrace: {3}.", logPrefix, employee.Id, ex.Message, ex.StackTrace);
        }
        finally
        {
          Locks.Unlock(employee);
        }
      }
      
      #endregion
      
      #region Исключить из роли неинтерактивных пользователей сотрудников, которые не подключены к ЛК и у сотрудника пустой логин.
      
      var nonInteractiveUsersRole = Sungero.CoreEntities.Roles.GetAll(a => a.Sid == Sungero.Domain.Shared.SystemRoleSid.NonInteractiveUsers).FirstOrDefault();
      var interactiveEmployees = nonInteractiveUsersRole.RecipientLinks.Where(x =>
                                                                              Employees.Is(x.Member) &&
                                                                              Employees.As(x.Member).PersonalAccountStatusDirRX != OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted &&
                                                                              Employees.As(x.Member).Login == null).ToList();
      
      foreach (var employee in interactiveEmployees)
      {
        nonInteractiveUsersRole.RecipientLinks.Remove(employee);
        Logger.DebugFormat("{0}. ID сотрудника: {1}. Сотрудник исключён из роли \"Неинтерактивные пользователи\".", logPrefix, employee.Id);
      }
      
      if (nonInteractiveUsersRole.State.Properties.RecipientLinks.IsChanged)
      {
        try
        {
          nonInteractiveUsersRole.Save();
          Logger.DebugFormat("{0}. Роль \"Неинтерактивные пользователи\" успешно сохранена.", logPrefix);
        }
        catch (Exception ex)
        {
          Logger.DebugFormat("{0}. Роль \"Неинтерактивные пользователи\" не сохранена. Ошибка: {1}. StackTrace: {2}.", logPrefix, ex.Message, ex.StackTrace);
        }
      }
      
      #endregion
      
      Logger.DebugFormat("{0}. Конец обработки УЗ сотрудников.", logPrefix);
    }
    
    #endregion

    #region Функции ФП "Очистка очередей организационной структуры".

    /// <summary>
    /// Получить список НОР, для которых изменения организационной структуры не производились n-дней.
    /// </summary>
    /// <param name="days">Количество рабочих дней.</param>
    /// <returns>Список НОР.</returns>
    public static IQueryable<IBusinessUnit> GetUnmodifiedBusinessUnitIdsByDays(int days)
    {
      var idsList = new List<long>();
      var date = Calendar.Now.AddWorkingDays(-days);
      using (var connection = SQL.CreateConnection())
      {
        var command = connection.CreateCommand();
        var str = string.Format(Queries.Module.GetUnmodifiedBUByDays, date.ToString("yyyy-MM-dd"));
        command.CommandText = string.Format(Queries.Module.GetUnmodifiedBUByDays, date.ToString("yyyy-MM-dd"));
        using (var rds = command.ExecuteReader())
        {
          if (rds != null)
            while (rds.Read())
          {
            idsList.Add(rds.GetInt64(0));
          }
          
          rds.Close();
        };
      };
      
      return BusinessUnits.GetAll(x => idsList.Contains(x.Id));
    }

    /// <summary>
    /// Очистка устаревших записей организационной структуры.
    /// </summary>
    /// <param name="logPrefix">Префикс.</param>
    [Public]
    public virtual void DeleteOldCompanyQueueRecords(string logPrefix)
    {
      // Получить количество дней из прикладной константы, по истечению которых необходимо удалить запись.
      var days = DirRX.NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.CompanyQueuesClearingJobDays.Guid);
      if (days == null || days.Value < 0)
      {
        var error = DirRX.NNIntegration.Resources.CompanyQueueClearingJobDaysErrorFormat(days);
        Logger.ErrorFormat("{0}. {1}.", logPrefix, error);
        throw AppliedCodeException.Create(error);
      }
      
      // Удаление устаревших записей.
      var overdueDate = Calendar.Now.AddDays(-days.Value);
      var oldRecords = CompanyQueueBases.GetAll(x => x.LastProcessingDate < overdueDate);
      Logger.DebugFormat("{0}. Начало удаления устаревших записей. Всего записей: {1}. Дата просрочки записей: {2}.", logPrefix,
                         oldRecords.Count(), overdueDate.ToShortDateString());
      foreach (var record in oldRecords)
      {
        Logger.DebugFormat("{0}. ID записи очереди: {1}. Старт обработки устаревшей записи.", logPrefix, record.Id);
        var isLocked = Locks.TryLock(record);
        if (isLocked)
        {
          var result = Transactions.Execute(() =>
                                            {
                                              CompanyQueueBases.Delete(record);
                                            });
          if (result)
            Logger.DebugFormat("{0}. ID записи очереди: {1}. Запись очереди справочников орг. структуры - успешно удалена.", logPrefix, record.Id);
          else
            Logger.ErrorFormat("{0}. ID записи очереди: {1}. Не удалось удалить запись очереди справочников орг. структуры.",
                               logPrefix, record.Id);
          Locks.Unlock(record);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(record);
          Logger.DebugFormat("{0}. ID записи очереди: {1}. Не удалось установить блокировку. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.", logPrefix, record.Id,
                             lockInfo.OwnerName, lockInfo.LockTime);
        }
        Logger.DebugFormat("{0}. ID записи очереди: {1}. Конец обработки устаревшей записи.", logPrefix, record.Id);
      }
      Logger.DebugFormat("{0}. Конец удаления устаревших записей.", logPrefix);
    }

    /// <summary>
    /// Отправить уведомления о НОР, для которых изменения организационной структуры не производились n-дней.
    /// </summary>
    /// <param name="logPrefix"></param>
    /// <param name="responsible"></param>
    [Public]
    public virtual void SendNotificationAboutUnmodifiedBusinessUnits(string logPrefix, IUser responsible)
    {
      // Получить количество дней из прикладной константы, по истечению которых необходимо оповещать о задержках синхронизации организационной структуры.
      var workingDays = DirRX.NNCommons.PublicFunctions.Module.GetIntValue(Constants.Module.ConstantsSetting.UnmodifiedBusinessUnitDays.Guid);
      if (workingDays == null || workingDays.Value < 0)
      {
        var error = DirRX.NNIntegration.Resources.UnmodifiedBusinessUnitDaysErrorFormat(workingDays);
        Logger.ErrorFormat("{0}. {1}.", logPrefix, error);
        throw AppliedCodeException.Create(error);
      }
      
      // Получить список НОР, для которых изменения организационной структуры не производились n-дней.
      var businessUnits = GetUnmodifiedBusinessUnitIdsByDays(workingDays.Value);
      Logger.DebugFormat("{0}. Всего найдено НОР, для которых изменения организационной структуры не производились {1} дней: {2}.", logPrefix, workingDays,
                         businessUnits.Count());
      
      if (businessUnits.Any() && responsible != null)
      {
        Logger.DebugFormat("{0}. Отправка уведомления ответственному о найденных НОР.", logPrefix);
        var subject = DirRX.NNIntegration.Resources.UnmodifiedBusinessUnitDataNotificationSubjectFormat(workingDays);
        var notification = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, responsible);
        notification.ActiveText = DirRX.NNIntegration.Resources.UnmodifiedBusinessUnitDataNotificationTextFormat(
          string.Join(", ", businessUnits.Select(x => Hyperlinks.Get(x))));
        notification.Save();
        notification.Start();
      }
    }

    /// <summary>
    /// Отправить уведомление ответственному о работе ФП.
    /// </summary>
    /// <param name="responsible">Ответственный.</param>
    /// <param name="subject">Тема уведомления.</param>
    /// <param name="activeText">Текст уведомления</param>
    /// <returns>Уведомление.</returns>
    [Public]
    public Sungero.Workflow.ISimpleTask SendNotificationToResponsible(IUser responsible, string subject, string text)
    {
      var notification = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, responsible);
      notification.ActiveText = text;
      notification.Save();
      notification.Start();
      return notification;
    }

    #endregion

    #endregion
    
    #region Общие функции интеграции.

    #region Исходящий поток.

    /// <summary>
    /// Запустить процесс отправки Исходящих сообщений во внешнюю систему.
    /// </summary>
    /// <param name="flowGuid">Guid потока.</param>
    /// <param name="entityIds">Список id записей.</param>
    [Public]
    public virtual void StartCreatedOutgoingMessages(string flowGuid, List<long> entityIds)
    {
      StartCreatedOutgoingMessages(flowGuid, entityIds, null);
    }

    /// <summary>
    /// Запустить процесс отправки исходящих сообщений во внешнюю систему.
    /// </summary>
    /// <param name="externalSystemFlow">Поток внешней системы.</param>
    /// <param name="docsIds">Список id документов.</param>
    /// <param name="versionsIds">Список id документов.</param>
    [Public]
    public virtual void StartCreatedOutgoingMessages(string flowGuid, List<long> docsIds, List<long> versionsIds)
    {
      var logPrefix = "StartCreatedOutgoingMessages";
      if (docsIds.Any() && (versionsIds == null || (versionsIds.Any() && docsIds.Count() == versionsIds.Count())) && NNIntegration.ExternalSystemFlows.GetAll().Any(f => f.FlowGuid == flowGuid))
      {
        var versionIdsString = versionsIds == null ? string.Empty : string.Join(Constants.Module.ListIdsDelimiter.ToString(), versionsIds);
        var docIdsString = string.Join(Constants.Module.ListIdsDelimiter.ToString(), docsIds);
        Logger.DebugFormat("{0}. Количество ИД документов: {1}, ИД документов: {2}, ИД версий документов: {3} FlowGuid: {4}. Старт процесса отправки Исходящих сообщений.",
                           logPrefix, docsIds.Count(), docIdsString, versionIdsString, flowGuid);
        var handler = NNIntegration.AsyncHandlers.CreateOutgoingMessages.Create();
        handler.FlowGuid = flowGuid;
        handler.EntitiesIds = docIdsString;
        handler.VersionsIds = versionIdsString;
        handler.ExecuteAsync();
      }
      else
      {
        var messageText = docsIds.Any() ?
          string.Format("{0}. Поток внешних систем с flowguid: {1} не найден.", logPrefix, flowGuid) :
          string.Format("{0}. Список id записей пустой.", logPrefix);

        Logger.ErrorFormat(messageText);
        throw new Exception(messageText);
      }
    }

    /// <summary>
    /// Создать запись справочника Исходящие информационные сообщения.
    /// </summary>
    /// <param name="externalSystemFlow">Поток внешней системы.</param>
    /// <param name="entityId">Id сущности.</param>
    /// <param name="versionId">Id версии.</param>
    /// <returns>True - запись создана, False - иначе.</returns>
    public virtual string CreateOutgoingMessage(IExternalSystemFlow externalSystemFlow, string entityId, string versionId)
    {
      var result = string.Empty;
      
      try
      {
        var message = OutgoingMessages.Create();
        long entityIdLong;
        if (Int64.TryParse(entityId, out entityIdLong))
          message.EntityId = entityIdLong;
        else
          result = Resources.IncorrectEntityIdFormat(entityId);
        message.ExternalSystemFlow = externalSystemFlow;
        if (!string.IsNullOrEmpty(versionId))
        {
          long versionIdLong;
          if (Int64.TryParse(versionId, out versionIdLong))
            message.VersionId = versionIdLong;
          else
            result = Resources.IncorrectVersionIdFormat(versionId);
        }
        message.Save();
        
        var handler = NNIntegration.AsyncHandlers.SendOutgoingMessage.Create();
        handler.OutgoingMessageId = message.Id;
        handler.ExecuteAsync();
      }
      catch (Exception ex)
      {
        result = ex.Message;
      }
      
      return result;
    }

    /// <summary>
    /// Case-функция получения текста исходящего сообщения из функции потока.
    /// </summary>
    /// <param name="flowGuid">Guid потока.</param>
    /// <param name="entityId">Id сущности.</param>
    /// <param name="guidMessage">Guid сообщения.</param>
    /// <returns>Результат.</returns>
    public NNIntegration.Structures.Module.IResultGenerateRequestText GetMessageText(string flowGuid, long entityId, string guidMessage)
    {
      var result = NNIntegration.Structures.Module.ResultGenerateRequestText.Create();
      switch (flowGuid)
      {
        case "0": // Пример использования Case-функции.
          result.RequestText = string.Empty; // Вызов функции потока.
          break;
          // IF-S-9401. Передача информации о документе из КЭДО в АСУП (АСУП -> КЭДО).
        case Constants.Module.ExternalSystemsFlowGuids.SapFioriOutgoing:
          result = GenerateStructForSendingDocToSapFiori(entityId);
          break;
          // IF-S-10532. Передача статуса приказа (КЭДО -> АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.StatusDocumentInSap:
          result = GenerateStructForSendingStatusDocToSap(entityId, guidMessage);
          break;
          // IF-S-7601. Передача документа на проштамповку.
        case Constants.Module.ExternalSystemsFlowGuids.SendDocumentToStamping:
          result = GetTextForSendToStampingDocument(entityId, guidMessage);
          break;
          // IF-S-10536. Формирование проводки на перенос отпуска из КЭДО в АСУП (КЭДО -> АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.RegistrationShiftVacation:
          result = NNVacations.PublicFunctions.Module.CreateRegistrationShiftVacation(entityId, guidMessage);
          break;
          // IF-S-6372. Регистрация ежегодного отпуска (по заявлению) из КЭДО в АСУП (КЭДО -> АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.RegistrationAnnualVacationByStatement:
          result = NNVacations.PublicFunctions.Module.CreateRegistrationAnnualVacationByStatement(entityId, guidMessage);
          break;
          // IF-S-6376. Регистрация прочего отпуска из КЭДО в АСУП (КЭДО -> АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.RegistrationOtherVacation:
          result = NNVacations.PublicFunctions.Module.CreateRegistrationOtherVacation(entityId, guidMessage);
          break;
          // IF-S-6373. Регистрация ежегодного отпуска (по графику отпусков) из КЭДО в АСУП (КЭДО -> АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.RegistrationAnnualVacationBySchedule:
          result = NNVacations.PublicFunctions.Module.CreateRegistrationAnnualVacationBySchedule(entityId, guidMessage);
          break;
          // IF-S-6369. Автоматическое создание мероприятия «Компенсация части ежегодного оплачиваемого отпуска» из КЭДО в АСУП.
        case Constants.Module.ExternalSystemsFlowGuids.CompensationForPartOfAnnualPaidLeave:
          result = NNVacations.PublicFunctions.Module.CreateCompensationForPartOfAnnualPaidLeave(entityId, guidMessage);
          break;
          // IF-S-6361. Автоматическое создание мероприятия "Отзыв из отпуска" из КЭДО в АСУП (КЭДО -> АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.RegistrationRecallVacation:
          result = NNVacations.PublicFunctions.Module.CreateRegistrationRecallVacation(entityId, guidMessage);
          break;
          // IF-S-6363. Автоматическое создание мероприятия «Регистрация работы в выходной и/или праздничный день» из КЭДО в АСУП (КЭДО -> АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.WeekendWorkEvent:
          result = NNOtherProcesses.PublicFunctions.Module.CreateWeekendWorkEvent(entityId, guidMessage);
          break;
          // IF-S-6362. Автоматическое внесение изменений в действующий приказ из КЭДО в АСУП (КЭДО -> АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.WeekendWorkCancelationEvent:
          result = NNOtherProcesses.PublicFunctions.Module.CreateWeekendWorkCancelationEvent(entityId, guidMessage);
          break;
          // IF-S-6366. Автоматическое создание мероприятия «Регистрация выплат ЛНА» из КЭДО в АСУП (КЭДО -> АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.LrdPaymentEvent:
          result = NNOtherProcesses.PublicFunctions.Module.CreateLrdPaymentEvent(entityId, guidMessage);
          break;
          // IF-S-28522 Проверка оплаты отсутствий (КЭДО → АСУП).
        case Constants.Module.ExternalSystemsFlowGuids.SendVacationsForPaymentChecking:
          result = NNVacations.PublicFunctions.Module.GenerateStructForSendingVacationsForPaymentChecking(entityId, guidMessage);
          break;
          // IF-S-29000. Передача статуса по процессу «Ведение графика отпусков» из КЭДО в АСУП.
        case Constants.Module.ExternalSystemsFlowGuids.VacationScheduleProcessStatus:
          result = NNVacations.PublicFunctions.Module.GenerateStructForSendingStatusVacationScheduleDoc(entityId, guidMessage);
          break;
        default:
          result.ErrorText = "Поток с переданным guid не найден.";
          break;
      }
      return result;
    }

    /// <summary>
    /// Отправить сообщение во внешнюю систему.
    /// </summary>
    /// <param name="outgoingMessage">Запись справочника Исходящие информационные сообщения.</param>
    /// <param name="messageText">Текст сообщения.</param>
    /// <returns>Структура - Код результата, Текст ответа, Текст ошибки.</returns>
    [Public]
    public virtual Structures.Module.IResultSendingMessage SendMessageToExternalSystem(NNIntegration.IOutgoingMessage outgoingMessage, string messageText)
    {
      if (outgoingMessage.ExternalSystemFlow.IsMultipartFormData == true)
        return SendMultipartMessageToExternalSystem(messageText, outgoingMessage);
      else
        return SendMessageToExternalSystem(messageText, outgoingMessage.ExternalSystemFlow.FlowGuid, outgoingMessage.EntityId.Value, outgoingMessage.GuidMessage);
    }

    /// <summary>
    /// Отправить сообщение во внешнюю систему.
    /// </summary>
    /// <param name="messageText">Текст запроса.</param>
    /// <param name="flowGuid">Guid-потока.</param>
    /// <param name="entityId">Id сущности.</param>
    /// <returns>Структура - Код результата, Текст ответа, Текст ошибки.</returns>
    [Public]
    public virtual Structures.Module.IResultSendingMessage SendMessageToExternalSystem(string messageText, string flowGuid, long entityId)
    {
      var guid = Guid.NewGuid().ToString();
      return SendMessageToExternalSystem(messageText, flowGuid, entityId, guid);
    }

    /// <summary>
    /// Отправить сообщение во внешнюю систему.
    /// </summary>
    /// <param name="messageText">Текст запроса.</param>
    /// <param name="flowGuid">Guid-потока.</param>
    /// <param name="entityId">Id сущности.</param>
    /// <param name="guidOutgoingMessage">guid исходящего сообщения.</param>
    /// <returns>Структура - Код результата, Текст ответа, Текст ошибки.</returns>
    public virtual Structures.Module.IResultSendingMessage SendMessageToExternalSystem(string messageText, string flowGuid, long entityId, string guidOutgoingMessage)
    {
      var result = Structures.Module.ResultSendingMessage.Create();
      var logPrefix = string.Format("SendMessageToExternalSystem. Guid-запроса: {0}. Guid-потока: {1}. Id сущности: {2}", guidOutgoingMessage, flowGuid, entityId);
      Logger.DebugFormat("{0}. Стартовал процесс отправки исходящего сообщения.", logPrefix);
      if (string.IsNullOrEmpty(messageText))
      {
        result.ErrorText = "Не передан текст сообщения";
        Logger.ErrorFormat("{0}. Процесс отправки исходящего сообщения завершился с ошибкой: {1}", logPrefix, result.ErrorText);
        return result;
      }
      var externalSystemFlow = PublicFunctions.ExternalSystemFlow.GetExternalSystemFlow(flowGuid);
      if (externalSystemFlow == null)
      {
        result.ErrorText = string.Format("Не найден поток внешней системы с ИД: {0}", flowGuid);
        Logger.ErrorFormat("{0}. Процесс отправки исходящего сообщения завершился с ошибкой: {1}", logPrefix, result.ErrorText);
        return result;
      }
      var errorTextValidateData = ValidateConnectionDataExternalSystem(externalSystemFlow.ActionName, externalSystemFlow.ExternalSystem.Address, externalSystemFlow.ExternalSystem.Port,
                                                                       externalSystemFlow.ExternalSystem.Login, externalSystemFlow.ExternalSystem.Password);
      if (!string.IsNullOrEmpty(errorTextValidateData))
      {
        result.ErrorText = string.Format("При валидации данных для подключения к внешней системе произошла ошибка: {0}", errorTextValidateData);
        return result;
      }
      
      var needAuthorizationHeader = externalSystemFlow.ExternalSystem.NeedAuthorization == true;
      var password = Sungero.Core.Encryption.Decrypt(externalSystemFlow.ExternalSystem.Password);
      
      result = IsolatedFunctions.SendOutgoingMessage.ExecuteRequestToExternalSystem(messageText, externalSystemFlow.ActionName, externalSystemFlow.ExternalSystem.Address, externalSystemFlow.ExternalSystem.Port.ToString(),
                                                                                    externalSystemFlow.ExternalSystem.Login, password, result, needAuthorizationHeader);
      if (string.IsNullOrEmpty(result.ErrorText) && result.Code == Constants.Module.Request.ResponseCode.SuccessfullProcessing)
        Logger.DebugFormat("{0}. Процесс отправки исходящего сообщения завершился успешно.", logPrefix);
      else
        Logger.ErrorFormat("{0}. Процесс отправки исходящего сообщения завершился с ошибкой: {1}", logPrefix, result.ErrorText);
      return result;
    }

    /// <summary>
    /// Отправить многокомпонентное сообщение во внешнюю систему.
    /// </summary>
    /// <param name="messageText">Текст сообщения.</param>
    /// <param name="outgoingMessage">Исходящее сообщение.</param>
    /// <returns>Структура - Код результата, Текст ответа, Текст ошибки.</returns>
    public virtual Structures.Module.IResultSendingMessage SendMultipartMessageToExternalSystem(string messageText, NNIntegration.IOutgoingMessage outgoingMessage)
    {
      var result = Structures.Module.ResultSendingMessage.Create();
      var logPrefix = string.Format("SendMultipartMessageToExternalSystem. Guid исходящего сообщения: {0}", outgoingMessage.GuidMessage);
      Logger.DebugFormat("{0}. Стартовал процесс отправки исходящего сообщения.", logPrefix);
      var externalSystemFlow = outgoingMessage.ExternalSystemFlow;
      var errorTextValidateData = ValidateConnectionDataExternalSystem(externalSystemFlow.ActionName, externalSystemFlow.ExternalSystem.Address, externalSystemFlow.ExternalSystem.Port,
                                                                       externalSystemFlow.ExternalSystem.Login, externalSystemFlow.ExternalSystem.Password);
      if (!string.IsNullOrEmpty(errorTextValidateData))
      {
        result.ErrorText = string.Format("При валидации данных для подключения к внешней системе произошла ошибка: {0}", errorTextValidateData);
        return result;
      }
      result = SendMultipartDataContent(outgoingMessage, messageText, result);
      if (string.IsNullOrEmpty(result.ErrorText) && result.Code == Constants.Module.Request.ResponseCode.SuccessfullProcessing)
        Logger.DebugFormat("{0}. Процесс отправки исходящего сообщения завершился успешно.", logPrefix);
      else
        Logger.ErrorFormat("{0}. Процесс отправки исходящего сообщения завершился с ошибкой: {1}", logPrefix, result.ErrorText);
      return result;
    }

    /// <summary>
    /// Проверить данные для подключения к внешней системе.
    /// </summary>
    /// <param name="actionName">Имя действия.</param>
    /// <param name="address">Адрес внешней системы.</param>
    /// <param name="port">Порт.</param>
    /// <param name="login">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    /// <returns>Текст ошибки.</returns>
    private string ValidateConnectionDataExternalSystem(string actionName, string address, Nullable<int> port, string login, string password)
    {
      var textError = string.Empty;
      if (string.IsNullOrEmpty(actionName))
        textError += "Не заполнено поле \"Имя действия\".";
      if (string.IsNullOrEmpty(address))
        textError += "Не заполнено поле \"Адрес подключения\".";
      if (port == null)
        textError += "Не заполнено поле \"Порт\".";
      if (string.IsNullOrEmpty(login))
        textError += "Не заполнено поле \"Логин\".";
      if (string.IsNullOrEmpty(password))
        textError += "Не заполнено поле \"Пароль\".";
      return textError;
    }


    /// <summary>
    /// Case-функция отправки Multipart запроса.
    /// </summary>
    /// <param name="outgoingMessage">Исходящее письмо.</param>
    /// <param name="messageText">Текст сообщения.</param>
    /// <param name="result">Структура - Код результата, Текст ответа, Текст ошибки.</param>
    /// <returns>Структура - Код результата, Текст ответа, Текст ошибки.</returns>
    public Structures.Module.IResultSendingMessage SendMultipartDataContent(NNIntegration.IOutgoingMessage outgoingMessage, string messageText, Structures.Module.IResultSendingMessage result)
    {
      switch (outgoingMessage.ExternalSystemFlow.FlowGuid)
      {
          // IF-S-7601. Передача документа на проштамповку.
        case Constants.Module.ExternalSystemsFlowGuids.SendDocumentToStamping:
          {
            result = SendMultipartContentStampingDocument(outgoingMessage, messageText, result);
            break;
          };
        default:
          Logger.ErrorFormat("SendMultipartDataContent. Guid потока {0}. Поток с переданным guid не найден.", outgoingMessage.ExternalSystemFlow.FlowGuid);
          break;
      }
      return result;
    }

    /// <summary>
    /// Уведомить ответственного за ошибки.
    /// </summary>
    /// <param name="subject">Тема.</param>
    /// <param name="text">Текст задания.</param>
    /// <param name="isNoticeForFlowResponsible">Уведомление для ответственного за ошибки?</param>
    /// <param name="message">Запись справочника, которая будет вложена в уведомление.</param>
    /// <returns>Сообщение в случае возникновения ошибок, иначе - пустая строка.</returns>
    [Public]
    public virtual string SendNotificationAboutErrors(string subject, string text, bool isNoticeForFlowResponsible, IDatabookEntry message)
    {
      var idResponibleForIntegrationKSIP = NNCommons.PublicFunctions.Module.GetIdentifierValue(
        NNIntegration.PublicConstants.Module.ConstantsSetting.IdGroupResponibleForIntegrationKSIP.Guid);
      var responibleForIntegrationKSIP = Sungero.CoreEntities.Recipients.GetAll().Where(r => r.Id == idResponibleForIntegrationKSIP).FirstOrDefault();
      Sungero.CoreEntities.IRecipient performer = responibleForIntegrationKSIP;
      if (isNoticeForFlowResponsible)
      {
        if (NNIntegration.OutgoingMessages.Is(message))
          performer = NNIntegration.OutgoingMessages.As(message).ExternalSystemFlow.ResponsibleForErrors;
        else if (NNIntegration.IncomingMessages.Is(message))
          performer = NNIntegration.IncomingMessages.As(message).ExternalSystemFlow.ResponsibleForErrors;
        if (performer == null)
        {
          performer = responibleForIntegrationKSIP;
          text = Resources.NotFoundResponsibleForErrors + text;
        }
      }
      if (performer == null)
        return "Не удалось определить ответственного за ошибки.";
      var result = string.Empty;
      try
      {
        var notice = HRPro.HRDocFlow.HRSimpleTasks.Create();
        notice.Subject = subject;
        var processKindId = NNCommons.PublicFunctions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.IntegrationNotificationProcessId.Guid);
        notice.ProcessKind = Sungero.Workflow.ProcessKinds.GetAll(p => p.Id == processKindId).FirstOrDefault();
        var performers = notice.Performers.AddNew();
        performers.Performer = performer;
        notice.ActiveText = text;
        notice.OtherGroup.All.Add(message);
        notice.Start();
      }
      catch (Exception ex)
      {
        result = string.Format("Отправка уведомления завершилась с ошибкой: {0}. StackTrace: {1}", ex.Message, ex.StackTrace);
      }
      return result;
    }
    
    /// <summary>
    /// Уведомить ответственного за ошибки.
    /// </summary>
    /// <param name="subject">Тема.</param>
    /// <param name="text">Текст задания.</param>
    /// <param name="isNoticeForFlowResponsible">Уведомление для ответственного за ошибки?</param>
    /// <param name="flowGuid">Guid потока.</param>
    /// <returns>Сообщение в случае возникновения ошибок, иначе - пустая строка.</returns>
    [Public]
    public virtual string SendNotificationAboutErrors(string subject, string text, string flowGuid)
    {
      var performer = Sungero.CoreEntities.Recipients.Null;
      var flow = ExternalSystemFlows.GetAll(x => x.FlowGuid == flowGuid).FirstOrDefault();
      if (flow != null)
        performer = flow.ResponsibleForErrors;
      else
        Logger.ErrorFormat("SendNotificationAboutErrors. Не найден поток по guid: {0}.", flowGuid);
      
      if (performer == null)
      {
        var idResponibleForIntegrationKSIP = NNCommons.PublicFunctions.Module
          .GetIdentifierValue(NNIntegration.PublicConstants.Module.ConstantsSetting.IdGroupResponibleForIntegrationKSIP.Guid);
        performer = Sungero.CoreEntities.Recipients.GetAll().Where(r => r.Id == idResponibleForIntegrationKSIP).FirstOrDefault();
        text = Resources.NotFoundResponsibleForErrors + text;
      }
      
      if (performer == null)
        return "Не удалось определить ответственного за ошибки.";
      
      var result = string.Empty;
      
      try
      {
        var notice = HRPro.HRDocFlow.HRSimpleTasks.Create();
        notice.Subject = subject;
        var processKindId = NNCommons.PublicFunctions.Module.GetIdentifierValue(Constants.Module.ConstantsSetting.IntegrationNotificationProcessId.Guid);
        notice.ProcessKind = Sungero.Workflow.ProcessKinds.GetAll(p => p.Id == processKindId).FirstOrDefault();
        var performers = notice.Performers.AddNew();
        performers.Performer = performer;
        notice.ActiveText = text;
        notice.Start();
      }
      catch (Exception ex)
      {
        result = string.Format("Отправка уведомления завершилась с ошибкой: {0}. StackTrace: {1}", ex.Message, ex.StackTrace);
      }
      return result;
    }

    #endregion

    #region Входящий поток.

    /// <summary>
    /// Создать запись Входящее информационное сообщение и запустить процесс его обработки.
    /// </summary>
    /// <param name="flowGuid">Guid потока.</param>
    /// <param name="apiMethodName">Наименование функции сервиса интеграции.</param>
    /// <param name="requestText">Текст запроса.</param>
    /// <param name="guidOutgoingMessage">Guid исходящего сообщения.</param>
    /// <param name="guidIncomingMessage">Guid входящего сообщения.</param>
    /// <returns>Ошибка.</returns>
    [Public]
    public string CreateIncomingMessage(string flowGuid, string apiMethodName, string responseBody, string guidOutgoingMessage, string guidIncomingMessage = "")
    {
      if (string.IsNullOrEmpty(guidIncomingMessage))
        guidIncomingMessage = Guid.NewGuid().ToString();

      // Логирование входящего запроса.
      LoggerIncomingMessage(flowGuid, apiMethodName, string.Empty, "Старт обработки входящего сообщения.", false, guidIncomingMessage, guidOutgoingMessage);
      
      // Поиск потока по guid.
      var flow = ExternalSystemFlows.GetAll(f => f.FlowGuid == flowGuid).FirstOrDefault();
      var logPrefix = "CreateIncomingMessage";
      var error = string.Empty;
      if (flow == null)
      {
        LoggerIncomingMessage(flowGuid, apiMethodName, string.Empty, DirRX.NNIntegration.Resources.FlowNotFoundByGuidErrorFormat(flowGuid), false, guidIncomingMessage, guidOutgoingMessage);
        return DirRX.NNIntegration.Resources.FlowNotFoundByGuidErrorFormat(flowGuid);
      }
      
      var message = IncomingMessages.Create();
      message.ExternalSystemFlow = flow;
      message.GuidIncomingMessage = guidIncomingMessage;
      
      if (!string.IsNullOrEmpty(guidOutgoingMessage))
      {
        var messagesNumber = OutgoingMessages.GetAll().Where(x => x.GuidMessage == guidOutgoingMessage).Count();
        if (messagesNumber > 1)
          error = string.Format("{0}. Найдено более одного исходящего сообщения по Guid: {1}. Всего сообщений: {2}.", logPrefix, guidOutgoingMessage, messagesNumber);
        else if (messagesNumber == 0)
          error = string.Format("{0}. Не найдено исходящее сообщение по Guid: {1}.", logPrefix, guidOutgoingMessage);
        message.GuidOutgoingMessage = guidOutgoingMessage;
      }
      
      if (!string.IsNullOrEmpty(error))
      {
        message.ErrorText = error;
        message.ProcessingDate = Calendar.Now;
        message.StatusMessage = NNIntegration.IncomingMessage.StatusMessage.Error;
      }
      
      // Сохранение сообщения вместе с текстом запроса.
      error = PublicFunctions.IncomingMessage.SetIncomingMessageTextAndSaveRecord(message, responseBody);
      
      var logBodyText = flow.ClearAfterProcessing == true ? string.Empty : responseBody;
      if (!string.IsNullOrEmpty(error))
        LoggerIncomingMessage(flowGuid, apiMethodName, logBodyText, string.Format("Ошибка при обработке входящего сообщения. {0}", error), true, guidIncomingMessage, guidOutgoingMessage);
      else
        LoggerIncomingMessage(flowGuid, apiMethodName, logBodyText, "Обработка входящего сообщения успешно завершена.", false, guidIncomingMessage, guidOutgoingMessage);

      if (string.IsNullOrEmpty(error))
      {
        var handler = NNIntegration.AsyncHandlers.ProcessingIncomingMessages.Create();
        handler.IncomingMessageId = message.Id;
        handler.ExecuteAsync();
      }
      
      return error;
    }

    /// <summary>
    /// Case-функция обработки входящего сообщения.
    /// </summary>
    /// <param name="flowGuid">Guid потока</param>
    /// <param name="idIncomingMessage">Id записи справочника Входящее информационное сообщение.</param>
    /// <returns>Результат обработки.</returns>
    public Structures.Module.IProcessIncomingMessageResult ProcessIncomingMessage(string flowGuid, long idIncomingMessage)
    {
      var result = Structures.Module.ProcessIncomingMessageResult.Create();
      
      switch (flowGuid)
      {
        case "0": // Пример использования Case-функции. При необходимости установите признак для переповтора АО (result.Retry = true).
          result.ErrorText = string.Empty; // Вызов функции потока.
          break;
          // IF-S-6365. Создание карточки согласования графика отпусков из АСУП в КЭДО.
        case Constants.Module.ExternalSystemsFlowGuids.CreateVacationScheduleDocument:
          result = NNVacations.PublicFunctions.Module.ProcessingCreateVacationScheduleDocument(idIncomingMessage);
          break;
          // IF-S-9336. Передача уведомления об отпуске из АСУП в КЭДО (АСУП-> КЭДО).
        case Constants.Module.ExternalSystemsFlowGuids.SendingVacationNotice:
          result = NNVacations.PublicFunctions.Module.CreateVacationNoticesAndStartTasks(idIncomingMessage);
          break;
          // IF-S-10397. Автоматическое создание карточки документа из АСУП в КЭДО.
        case Constants.Module.ExternalSystemsFlowGuids.CreateRetentionStatementDocument:
          result = DirRX.NNOtherProcesses.PublicFunctions.Module.ProcessingCreateRetentionStatementDocument(idIncomingMessage);
          break;
          // IF-S-10408. Передача приказа на предоставление отпуска из АСУП в КЭДО (АСУП -> КЭДО).
        case Constants.Module.ExternalSystemsFlowGuids.CreateAnnualVacationOrder:
          result = NNVacations.PublicFunctions.Module.UpdateOrCreateAnnualVacationOrder(idIncomingMessage);
          break;
          // IF-S-10411. Передача приказа на предоставление прочего отпуска из АСУП в КЭДО (АСУП -> КЭДО).
        case Constants.Module.ExternalSystemsFlowGuids.CreateOtherVacationOrder:
          result = NNVacations.PublicFunctions.Module.UpdateOrCreateOtherVacationOrder(idIncomingMessage);
          break;
          // IF-S-10396. Передача приказа на компенсацию части ежегодного оплачиваемого отпуска из АСУП в КЭДО (АСУП -> КЭДО).
        case Constants.Module.ExternalSystemsFlowGuids.CreateVacationCompensationOrder:
          result = NNVacations.PublicFunctions.Module.UpdateOrCreateVacationCompensationOrder(idIncomingMessage);
          break;
          // IF-S-10391. Передача приказа об отзыве из отпуска из АСУП в КЭДО (АСУП -> КЭДО).
        case Constants.Module.ExternalSystemsFlowGuids.CreateRecallVacationOrder:
          result = NNVacations.PublicFunctions.Module.UpdateOrCreateRecallVacationOrder(idIncomingMessage);
          break;
          // IF-S-10398. Автоматическое формирование задания на ручное разблокирование ИТ0015 в АСУП (АСУП -> КЭДО).
        case Constants.Module.ExternalSystemsFlowGuids.ManualHoldingEvent:
          result = DirRX.NNOtherProcesses.PublicFunctions.Module.ProcessManualHoldingEvent(idIncomingMessage);
          break;
          // IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП.
        case Constants.Module.ExternalSystemsFlowGuids.CreateBusinessTripOrder:
          result = DirRX.NNOtherProcesses.PublicFunctions.Module.ProcessCreateBusinessTripOrder(idIncomingMessage);
          break;
          // IF-S-7578. Получение результата проштамповки.
        case Constants.Module.ExternalSystemsFlowGuids.GetResultStamping:
          result = ProcessingResultStamping(idIncomingMessage);
          break;
          // IF-S-11622. Получение результата асинхронной отправки данных из КСИП в АСУП.
        case Constants.Module.ExternalSystemsFlowGuids.GetSendingAsyncDataResultFromKSIP:
          result = ProcessingMessageSendingResult(idIncomingMessage);
          break;
          // IF-S-10413. Передача приказа на выход сотрудника в выходной и/или праздничный день.
        case Constants.Module.ExternalSystemsFlowGuids.CreateWeekendWorkOrderDocument:
          result = DirRX.NNOtherProcesses.PublicFunctions.Module.CreateWeekendWorkOrderDocumentHandler(idIncomingMessage);
          break;
          // IF-S-25401. Получение результата нажатия на действие в ПМД.
        case Constants.Module.ExternalSystemsFlowGuids.GetSupernikaActionResult:
          result = ProcessManualHoldingEvent(idIncomingMessage);
          break;
        case Constants.Module.ExternalSystemsFlowGuids.LrdPaymentOrder:
          result = DirRX.NNOtherProcesses.PublicFunctions.Module.UpdateOrCreateLrdPaymentOrder(idIncomingMessage);
          break;
          // IF-S-28097. Синхронизация календарей рабочего времени.
        case Constants.Module.ExternalSystemsFlowGuids.UpdateOrCreateWorkingTimeCalendar:
          result = DirRX.NNCommons.PublicFunctions.Module.UpdateOrCreateWorkingTimeCalendar(idIncomingMessage);
          break;
          // IF-S-29002. Передача приказа на дополнительный день отдыха за РВД из АСУП в КЭДО (АСУП -> КЭДО).
        case Constants.Module.ExternalSystemsFlowGuids.CreateWeekendWorkRestOrderDocument:
          result = DirRX.NNOtherProcesses.PublicFunctions.Module.CreateWeekendWorkRestOrderDocumentHandler(idIncomingMessage);
          break;
          // IF-S-28542 Данные по оплатам отсутствий из АСУП в КЭДО (АСУП → КЭДО).
        case Constants.Module.ExternalSystemsFlowGuids.VacationsPaymentInfo:
          result = DirRX.NNVacations.PublicFunctions.Module.ValidationVacationsPaymentInfo(idIncomingMessage);
          break;
        case Constants.Module.ExternalSystemsFlowGuids.EmploymentContractAdditionalAgreement:
          result = DirRX.NNOtherProcesses.PublicFunctions.Module.ProcessEmploymentContractAdditionalAgreement(idIncomingMessage);
          break;
        default:
          result.ErrorText = string.Format("Поток с переданным guid не найден. Guid: {0}", flowGuid);
          break;
      }
      
      return result;
    }

    /// <summary>
    /// Логирование получения входящего сообщения.
    /// </summary>
    /// <param name="flowGuid">Guid-потока.</param>
    /// <param name="apiMethodName">Наименование функции сервиса интеграции.</param>
    /// <param name="responseBody">Тело запроса. Для конфиденцальных данных необходимо передавать пустую строку.</param>
    /// <param name="text">Текст сообщения лога.</param>
    /// <param name="isError">Признак ошибки. True - если сообщение типа Error. False - если сообщение типа Debug.</param>
    /// <param name="guidIncomingMessage">Guid входящего запроса.</param>
    /// <param name="guidOutgoingMessage">Guid исходящего запроса.</param>
    [Public]
    public virtual void LoggerIncomingMessage(string flowGuid, string apiMethodName, string responseBody, string text, bool isError, string guidIncomingMessage, string guidOutgoingMessage)
    {
      var messageInfo = string.Format("Guid входящего запроса: {0}.", guidIncomingMessage);
      if (!string.IsNullOrEmpty(guidOutgoingMessage))
        messageInfo += string.Format(" Guid исходящего запроса: {0}.", guidOutgoingMessage);
      
      var logMessage = string.Format("LoggerIncomingMessage. Имя метода: {0}. Guid-потока: {1}. {2} Сообщение: {3}.", apiMethodName, flowGuid, messageInfo, text);
      
      var needLogBody = NNCommons.PublicFunctions.Module.Remote.GetBoolValue(PublicConstants.Module.ConstantsSetting.NeedLogKSIPMessagesDetails.Guid);
      if (needLogBody == true && !string.IsNullOrEmpty(responseBody))
        logMessage += string.Format(" Тело запроса: {0}.", responseBody);
      
      if (isError)
        Logger.Error(logMessage);
      else
        Logger.Debug(logMessage);
    }

    #endregion

    #region Заполнение/извлечение текста сообщения.

    /// <summary>
    /// Раскодировать текст сообщения.
    /// </summary>
    /// <param name="codedMessageText">Закодированный текст сообщения.</param>
    /// <param name="idMessage">Ид сообщения.</param>
    /// <param name="isIncomingMessage">Признак что сообщение входящее, иначе исходящее.</param>
    /// <returns>Текст сообщения.</returns>
    [Public]
    public virtual string DecodeMessageText(System.IO.Stream codedMessageText, long idMessage, bool isIncomingMessage)
    {
      string result = string.Empty;
      try
      {
        using (var reader = new StreamReader(codedMessageText, System.Text.Encoding.UTF8))
          result = reader.ReadToEnd();
      }
      catch (Exception e)
      {
        var typeMessage = isIncomingMessage ? "входящих" : "исходящих";
        Logger.ErrorFormat("DecodeMessageText. Ид сообщения: {0}. Справочник {1} сообщений. При раскодировании текста сообщения произошла ошибка: {2}. StackTrace:{3}.",
                           idMessage, typeMessage, e.Message, e.StackTrace);
      }
      return result;
    }

    /// <summary>
    /// Закодировать текст сообщения.
    /// </summary>
    /// <param name="messageText">Текст сообщения.</param>
    /// <param name="idMessage">Ид сообщения.</param>
    /// <param name="isIncomingMessage">Признак что сообщение входящее, иначе исходящее.</param>
    /// <returns>Закодированный текст сообщения.</returns>
    [Public]
    public virtual System.IO.Stream EncodingMessageText(string messageText, long idMessage, bool isIncomingMessage)
    {
      var stream = new MemoryStream();
      var typeMessage = isIncomingMessage ? "входящих" : "исходящих";
      try
      {
        if (messageText != null)
        {
          var messageTextBytes = System.Text.Encoding.UTF8.GetBytes(messageText);
          stream.Write(messageTextBytes, 0, messageTextBytes.Length);
        }
        else
          Logger.DebugFormat("EncodingMessageText. Ид сообщения: {0}. Справочник {1} сообщений. Текст сообщения - null.", idMessage, typeMessage);
      }
      catch (Exception e)
      {
        Logger.ErrorFormat("EncodingMessageText. Ид сообщения: {0}. Справочник {1} сообщений. При кодировании текста сообщения произошла ошибка: {2}. StackTrace:{3}.",
                           idMessage, typeMessage, e.Message, e.StackTrace);
      }
      return stream;
    }

    /// <summary>
    /// Конвертировать бинарные данные в base64.
    /// </summary>
    /// <param name="data">Бинарные данные.</param>
    /// <param name="error">Ошибка.</param>
    /// <returns>Преобразованная строка.</returns>
    private string Converter(IBinaryData data, out string error)
    {
      error = string.Empty;
      
      if (data == null)
        return string.Empty;

      try
      {
        using (var memory = new MemoryStream())
        {
          data.Read().CopyTo(memory);
          return System.Convert.ToBase64String(memory.ToArray());
        }
      }
      catch (Exception e)
      {
        error = string.Format("Произошла ошибка чтения тела {0}. StackTrace: {1}.", e.Message, e.StackTrace);
        return string.Empty;
      }
    }

    /// <summary>
    /// Произвести очистку интеграционных сообщений.
    /// </summary>
    /// <param name="messages">Интеграционные сообщения.</param>
    /// <param name="logPrefix">Префикс.</param>
    public virtual void IntegrationMessagesCleaning(string logPrefix, IQueryable<Sungero.CoreEntities.IDatabookEntry> messages)
    {
      foreach (var message in messages)
      {
        var isIncomingMessage = IncomingMessages.Is(message);
        var typePrefix = string.Empty;
        if (isIncomingMessage)
          typePrefix = string.Format("{0}. Ид сообщения: {1}", IncomingMessages.Info.LocalizedName, message.Id);
        else
          typePrefix = string.Format("{0}. Ид сообщения: {1}", OutgoingMessages.Info.LocalizedName, message.Id);
        
        Logger.DebugFormat("{0}. {1}. Старт обработки сообщения.", logPrefix, typePrefix);
        var isLocked = Locks.TryLock(message);
        if (isLocked)
        {
          var result = Transactions.Execute(() =>
                                            {
                                              if (isIncomingMessage)
                                                IncomingMessages.Delete(IncomingMessages.As(message));
                                              else
                                                OutgoingMessages.Delete(OutgoingMessages.As(message));
                                            });
          if (result)
            Logger.DebugFormat("{0}. {1}. Сообщение - успешно удалено.", logPrefix, typePrefix);
          else
            Logger.ErrorFormat("{0}. {1}. Не удалось удалить Сообщение.", logPrefix, typePrefix);
          Locks.Unlock(message);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(message);
          Logger.DebugFormat("{0}. {1}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.",
                             logPrefix, typePrefix, lockInfo.OwnerName, lockInfo.LockTime);
        }
      }
    }

    #endregion

    #region Работа с XML.

    /// <summary>
    /// Сериализация структуры в xml строку.
    /// </summary>
    /// <param name="structure">Структура.</param>
    /// <param name="errorMessage">Ошибка.</param>
    /// <typeparam name="T">Тип структуры.</typeparam>
    /// <returns>Строка xml.</returns>
    private string SerializeInXml<T>(T structure, out string errorMessage)
    {
      errorMessage = string.Empty;
      try
      {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using (var stringWriter = new StringWriter())
        {
          using (var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = System.Xml.Formatting.Indented })
          {
            xmlSerializer.Serialize(xmlTextWriter, (T)structure);
            return stringWriter.ToString();
          }
        }
      }
      catch (Exception e)
      {
        errorMessage = "Произошла ошибка сериализации структуры в xml строку.";
        Logger.ErrorFormat("SerializeInXml. {0} Ошибка: {1}. StackTrace: {2}.", errorMessage, e.Message, e.StackTrace);
      }

      return string.Empty;
    }

    #endregion

    #region Работа с DateTime

    /// <summary>
    /// Парсинг строки в дату с учетом культуры "ru-RU".
    /// </summary>
    /// <param name="date">Дата.</param>
    /// <returns>Если парсинг успешен DateTime, иначе null.</returns>
    [Public]
    public DateTime? ParseStringToDate(string date)
    {
      try
      {
        return DateTime.Parse(date, new System.Globalization.CultureInfo("ru-RU"));
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("ParseStringToDate. Ошибка при попытке парсинга строки '{0}' в дату. Текст ошибки: {1} StackTrace: {2}", date, ex.Message, ex.StackTrace);
        return null;
      }
    }

    #endregion

    /// <summary>
    /// Проверить id текущей итерации на максимальное значение числа повторов АО.
    /// </summary>
    /// <param name="currentIter">Текущая итерация АО.</param>
    /// <returns>Результат. True - если достигнуто максимальное значение, False - если нет.</returns>
    [Public]
    public bool IsLastAsyncHandlerRetry(int currentIterId)
    {
      return currentIterId == Constants.Module.MaxRetry;
    }

    /// <summary>
    /// Создать ответ на запрос внешней системы в Json.
    /// </summary>
    /// <param name="result">Результат. Передется одно из значений констант NNIntegration.Constants.Module.Request.ResultCode.</param>
    /// <param name="error">Текст ошибки.</param>
    /// <returns>Ответ в Json.</returns>
    private IResponseToExternalSystemRequest CreateResponseToExternalSystem(string result, string errorMessage)
    {
      if (!string.IsNullOrEmpty(errorMessage))
        throw AppliedCodeException.Create(errorMessage);
      
      var response = Structures.Module.ResponseToExternalSystemRequest.Create();
      response.Result = result;
      
      return response;
    }

    /// <summary>
    /// Получить код SAP из вида документа.
    /// </summary>
    /// <param name="doc">Документ.</param>
    /// <returns>Код SAP из вида документа.</returns>
    [Public]
    public string GetSAPCodeByDocKind(IDocRelatedToWorkBase doc)
    {
      if (VacationStatementDocuments.Is(doc))
      {
        var statement = VacationStatementDocuments.As(doc);
        var otherDocKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNVacations.PublicConstants.Module.DocumentKind.OtherVacationKind);
        if (Equals(statement.DocumentKind, otherDocKind) && statement.VacationTypeDirRX != null)
        {
          // Заявление на учебный отпуск.
          if (NNVacations.PublicFunctions.Module.CheckForStudyVacationType(statement.VacationTypeDirRX.Id))
            return NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.StudyVacationDocKindSAPCode.Guid);
          
          var actualAbsence = statement.VacationTypeDirRX.ActualAbsenceDirRX.FirstOrDefault(x => Equals(x.VacationReason, statement.VacationReasonDirRX))?.ActualAbsence;
          // Заявление на отпуск (по прочим основаниям).
          if (actualAbsence == Constants.Module.ActualAbsence.OtherVacation)
            return NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.OtherVacationDocKindSAPCode.Guid);
          // Заявление на предоставление отпуска по КД (работник-инвалид).
          if (actualAbsence == Constants.Module.ActualAbsence.VacationDisabledEmployee)
            return NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.VacationDisabledEmployeeDocKindSAPCode.Guid);
          // Заявление на дополнительный отпуск на обустройство по программе Релокация.
          if (actualAbsence == Constants.Module.ActualAbsence.VacationByRelocation)
            return NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.VacationByRelocationDocKindSAPCode.Guid);
        }
      }
      return DocumentKinds.As(doc.DocumentKind).SAPCodeDirRX;
    }

    #endregion

    #region Интеграционный сценарий "Хранение документа в ЛК SAP".

    #region IF-S-6385. Передача документа из КЭДО в АСУП.

    /// <summary>
    /// Получить тело документа.
    /// </summary>
    /// <param name="ID">ИД документа.</param>
    /// <returns>Тело документа в base64.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public string GetDocument(string ID, string IdentityType)
    {
      var logPrefix = string.Format("GetDocument. ID документа: '{0}'. IdentityType: '{1}'. Пользователь: '{2}'", ID, IdentityType, Users.Current.Id);
      
      Logger.DebugFormat("{0}. Запрос тела документа.", logPrefix);
      var document = DirRX.OverrideSolution.OfficialDocuments.Null;
      long convertId = 0;
      
      if (IdentityType == NNIntegration.PublicConstants.Module.IdentityTypes.RX && long.TryParse(ID, out convertId))
        document = DirRX.OverrideSolution.OfficialDocuments.GetAll(c => c.Id == convertId).FirstOrDefault();

      if (IdentityType == NNIntegration.PublicConstants.Module.IdentityTypes.SAP)
        document = DirRX.OverrideSolution.OfficialDocuments.GetAll(c => c.ExternalId == ID).FirstOrDefault();

      if (document == null || !document.HasVersions)
      {
        var errorMessage = DirRX.NNIntegration.Resources.GetDocumentNotFoundError;
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        throw AppliedCodeException.Create(errorMessage);
      }

      var response = string.Empty;
      var error = string.Empty;

      response = document.HasPublicBody ? Converter(document.LastVersion.PublicBody, out error) : Converter(document.LastVersion.Body, out error);
      
      if (!string.IsNullOrEmpty(error))
      {
        Logger.ErrorFormat("{0}. {1}.", logPrefix, error);
        throw AppliedCodeException.Create(error);
      }

      return response;
    }

    #endregion

    #region IF-S-9401. Передача информации о документе из КЭДО в АСУП.

    /// <summary>
    /// Создание структуры для отправки документа в SAP Fiori.
    /// </summary>
    /// <param name="documentId">Ид документа.</param>
    /// <returns>Структура для передачи данных в SAP Fiori.</returns>
    private IResultGenerateRequestText GenerateStructForSendingDocToSapFiori(long documentId)
    {
      var result = ResultGenerateRequestText.Create();
      Logger.DebugFormat("GenerateStructForSendingDocToSapFiori. Ид документа: {0}. Запуск создания структуры для отправки документа в SAP Fiori.", documentId);
      var doc = DirRX.OverrideSolution.DocRelatedToWorkBases.GetAll(c => c.Id == documentId).FirstOrDefault();

      if (doc == null)
      {
        result.ErrorText = "Документ не найден";
        Logger.ErrorFormat("GenerateStructForSendingDocToSAPFiori. Ид документа: {0}. {1}.", documentId, result.ErrorText);
        return result;
      }
      
      var docKindSAPCode = GetSAPCodeByDocKind(doc);
      if (string.IsNullOrEmpty(docKindSAPCode))
      {
        result.ErrorText = string.Format("Для вида документа {0} не заполнено значение поля Код SAP. Для передачи данных в SAP заполните поле Код SAP в записи справочника Виды документов", doc.DocumentKind.Id);
        Logger.ErrorFormat("GenerateStructForSendingDocToSapFiori. Ид документа: {0}. {1}.", documentId, result.ErrorText);
        return result;
      }

      var model = DirRX.NNIntegration.Structures.Module.DocToSAPFioriInfo.Create(
        doc.ExternalId,
        doc.InternalApprovalState.HasValue ? doc.InternalApprovalState?.Value : string.Empty,
        docKindSAPCode,
        doc.Name,
        doc.RegistrationDate.HasValue ? doc.RegistrationDate?.ToString("dd.MM.yyyy") : string.Empty,
        doc.EmployeeDirRX == null ? string.Empty : doc.EmployeeDirRX.PersonnelNumber,
        doc.Id,
        doc.TypeDiscriminator.ToString()
       );
      
      try
      {
        result.RequestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(model);
        Logger.DebugFormat("GenerateStructForSendingDocToSAPFiori. Ид документа: {0}. Завершено создание структуры для отправки документа в SAP Fiori.",
                           documentId);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("GenerateStructForSendingDocToSapFiori. При создании структуры для отправки документа в SAP Fiori возникла ошибка: {0}. StackTrace: {1}.",
                           ex.Message, ex.StackTrace);
      }
      
      return result;
    }

    #endregion

    #region IF-S-10532. Передача статуса приказа (КЭДО -> АСУП).

    /// <summary>
    /// Создание структуры для отправки статуса документа в SAP.
    /// </summary>
    /// <param name="documentId">ID основной задачи.</param>
    /// <returns>Структура для передачи данных в SAP.</returns>
    private IResultGenerateRequestText GenerateStructForSendingStatusDocToSap(long mainTaskId, string messageGuid)
    {
      var result = ResultGenerateRequestText.Create();
      Logger.DebugFormat("GenerateStructForSendingStatusDocToSap. Ид основной задачи: {0}. Запуск создания структуры для отправки статуса документа в SAP.", mainTaskId);
      var doc = DirRX.OverrideSolution.DocRelatedToWorkBases.As(GetDocumentForSendingToExternalSystem(mainTaskId));
      
      if (doc == null)
      {
        result.ErrorText = "Документ не найден или не того типа";
        Logger.ErrorFormat("GenerateStructForSendingStatusDocToSap. Ид основной задачи: {0}. {1}.", mainTaskId, result.ErrorText);
        return result;
      }
      
      var docKindSAPCode = GetSAPCodeByDocKind(doc);
      if (string.IsNullOrEmpty(docKindSAPCode))
      {
        result.ErrorText = string.Format("Для вида документа {0} не заполнено значение поля Код SAP. Для передачи данных в SAP заполните поле Код SAP в записи справочника Виды документов", doc.DocumentKind.Id);
        Logger.ErrorFormat("GenerateStructForSendingStatusDocToSap. Ид основной задачи: {0}. {1}.", mainTaskId, result.ErrorText);
        return result;
      }
      
      var docKind = DirRX.OverrideSolution.DocumentKinds.As(doc.DocumentKind);
      
      var approval = Constants.Module.ApprovalStatusDoc.Reject;
      if (doc.InternalApprovalState == Sungero.Docflow.OfficialDocument.InternalApprovalState.Signed &&
          doc.LifeCycleState == HRPro.HRProSolution.OfficialDocument.LifeCycleState.Active)
        approval = Constants.Module.ApprovalStatusDoc.Sign;
      
      var grantRestOrderDocumentKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(NNOtherProcesses.PublicConstants.Module.DocumentKind.GrantRestOrderDocument);
      
      var needCancelAction = true;
      
      if (DocumentKinds.Equals(doc.DocumentKind, grantRestOrderDocumentKind))
      {
        var order = NNOtherProcesses.WeekendWorkRestOrderDocuments.As(doc)?.Statement;
        var statement = NNOtherProcesses.WeekendWorkRestStatementDocuments.As(order);
        
        if (statement != null &&
            statement.SourceDocument != null &&
            statement.SourceDocument.Employees != null)
        {
          var employeeInfo = statement.SourceDocument.Employees
            .Where(em => Equals(em.Employee, doc.EmployeeDirRX))
            .FirstOrDefault();
          
          if (employeeInfo != null)
            needCancelAction = !(employeeInfo.RejectedByOrder == true);
        }
      }
      else
        needCancelAction = docKind.NeedCancelActionDirRX == true;
      
      var model = DirRX.NNIntegration.Structures.Module.StatusDocToSAPInfo.Create(
        messageGuid,
        doc.EmployeeDirRX != null ? doc.EmployeeDirRX.PersonnelNumber : string.Empty,
        doc.RegistrationNumber,
        doc.DocumentDate.HasValue ? doc.DocumentDate.Value.ToString("dd.MM.yyyy") : string.Empty,
        approval,
        doc.Id,
        needCancelAction,
        docKindSAPCode,
        doc.TypeDiscriminator.ToString(),
        doc.ExternalId,
        string.Empty
       );
      
      var errorConvert = string.Empty;
      
      if (docKind.TransferBody == true && doc.HasVersions)
        model.Body = doc.HasPublicBody ? Converter(doc.LastVersion.PublicBody, out errorConvert) : Converter(doc.LastVersion.Body, out errorConvert);
      
      if (!string.IsNullOrEmpty(errorConvert))
      {
        result.ErrorText = errorConvert;
        Logger.ErrorFormat("GenerateStructForSendingStatusDocToSap. Ид основной задачи: {0}. При создании структуры для отправки статуса документа в SAP возникла ошибка: {1}.",
                           mainTaskId, errorConvert);
      }
      
      try
      {
        result.RequestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(model);
        Logger.DebugFormat("GenerateStructForSendingStatusDocToSap. Ид основной задачи: {0}. Завершено создание структуры для отправки статуса документа в SAP.", mainTaskId);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Произошла ошибка сериализации структуры в json строку.";
        Logger.ErrorFormat("GenerateStructForSendingStatusDocToSap. Ид основной задачи: {0}. При создании структуры для отправки статуса документа в SAP возникла ошибка: {1}.",
                           mainTaskId, ex.Message);
      }
      
      return result;
    }

    #endregion

    #endregion

    #region Интеграционный сценарий "Оформление ежегодного отпуска по графику".

    #region IF-S-10535. Передача уведомления об отпуске из АСУП в КЭДО (АСУП-> КЭДО).

    /// <summary>
    /// Передать уведомление об отпуске.
    /// </summary>
    /// <param name="Employees">Информация об отпусках.</param>
    /// <param name="Emails">Адреса эл. почты.</param>
    /// <returns>Ответ в Json.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest SendVacationNotice(List<DirRX.NNIntegration.Structures.Module.IVacationInfo> Employees, List<DirRX.NNIntegration.Structures.Module.IEmails> Emails)
    {
      var errorMessage = string.Empty;
      if (Employees == null)
      {
        errorMessage = "Передано пустое значение.";
        Logger.ErrorFormat("SendVacationNotice. {0}", errorMessage);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      var model = Structures.Module.IncomingVacationsInfo.Create(Employees, Emails);
      foreach (var vacationInfo in model.EmployeesVacationsInfo)
      {
        vacationInfo.Employee = vacationInfo.Employee.Trim().TrimStart('0');
      }
      
      var responseBody = string.Empty;
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("SendVacationNotice. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.SendingVacationNotice, "SendVacationNotice", responseBody, string.Empty);
      
      return string.IsNullOrEmpty(errorMessage)
        ? CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty)
        : CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
    }

    #endregion

    #endregion

    #region Интеграционный сценарий "Проведение мероприятий".

    /// <summary>
    /// Получить подзадачу типа "Обмен данными с внешней системой" по ИД основной задачи.
    /// </summary>
    /// <param name="mainTaskId">ID основной задачи.</param>
    /// <returns>Задача.</returns>
    [Public]
    public static IExternalSystemDataExchangeTask GetExternalSystemDataExchangeSubTask(long mainTaskId)
    {
      var mainTask = Sungero.Workflow.Tasks.GetAll(x => x.Id == mainTaskId).FirstOrDefault();
      if (mainTask == null)
        Logger.DebugFormat("GetExternalSystemDataExchangeSubTask. Ид основной задачи: {0}. Не найдена основная задача.", mainTaskId);
      
      var subTask = mainTask?.Subtasks
        .Where(x => ExternalSystemDataExchangeTasks.Is(x))
        .OrderByDescending(x => x.Id)
        .FirstOrDefault();
      
      if (subTask == null)
        Logger.ErrorFormat("GetExternalSystemDataExchangeSubTask. Ид основной задачи: {0}. Не найдена действующая подзадача типа \"Обмен данными с внешней системой\" у основной задачи.", mainTaskId);
      else
        Logger.DebugFormat("GetExternalSystemDataExchangeSubTask. Ид основной задачи: {0}. Ид подзадачи: {1}. Подзадача - найдена.", mainTaskId, subTask.Id);
      
      return ExternalSystemDataExchangeTasks.As(subTask);
    }

    /// <summary>
    /// Получить документ для передачи во внешнюю систему.
    /// </summary>
    /// <param name="mainTaskId">ID основной задачи.</param>
    /// <returns>Документ.</returns>
    [Public]
    public static IOfficialDocument GetDocumentForSendingToExternalSystem(long mainTaskId)
    {
      var subTask = GetExternalSystemDataExchangeSubTask(mainTaskId);
      var document = subTask?.Documents.FirstOrDefault()?.Document;
      if (document == null)
        Logger.ErrorFormat("GetDocumentForSendingToExternalSystem. Ид основной задачи: {0}. Не найден документ для передачи во внешнюю систему по основной задаче.", mainTaskId);
      else
        Logger.DebugFormat("GetDocumentForSendingToExternalSystem. Ид основной задачи: {0}. Ид документа: {1}. Документ для передачи во внешнюю систему - найден.", mainTaskId, document.Id);
      
      return document;
    }

    /// <summary>
    /// Получить отправленное сообщение по задаче.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <returns>Исходящее сообщение.</returns>
    [ExpressionElement("ExpressionFuncName_GetMessageSentByTask", "ExpressionFuncDesc_GetMessageSentByTask")]
    public static IOutgoingMessage GetMessageSentByTask(Sungero.Workflow.ITask task)
    {
      var messages = OutgoingMessages.GetAll(x => x.EntityId == task.Id).ToList();
      var entityId = messages.ToDictionary(x => x.Id, x => GetCreateDateDatabookEntry(x)).OrderByDescending(x => x.Value).FirstOrDefault().Key;
      
      return messages.Where(x => x.Id == entityId).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить дату создания записи справочника.
    /// </summary>
    /// <param name="record">Запись справочника.</param>
    /// <returns>Дата создания.</returns>
    [Public]
    public static DateTime? GetCreateDateDatabookEntry(Sungero.CoreEntities.IDatabookEntry databookEntry)
    {
      return databookEntry.History.GetAll().OrderBy(h => h.HistoryDate)
        .Where(h => h.Action == Sungero.CoreEntities.History.Action.Create)
        .Select(h => h.HistoryDate)
        .FirstOrDefault();
    }

    /// <summary>
    /// Получить входящее сообщение на основании исходящего.
    /// </summary>
    /// <param name="message">Исходящее сообщение.</param>
    /// <returns>Входящее сообщение.</returns>
    [ExpressionElement("ExpressionFuncName_GetIncomingMessageByOutgoingMessage", "ExpressionFuncDesc_GetIncomingMessageByOutgoingMessage")]
    public static IIncomingMessage GetIncomingMessageByOutgoingMessage(IOutgoingMessage message)
    {
      return message != null ? IncomingMessages.GetAll(x => x.GuidOutgoingMessage == message.GuidMessage).OrderByDescending(x => x.Id).FirstOrDefault() : IncomingMessages.Null;
    }

    /// <summary>
    /// Получить код ошибки.
    /// </summary>
    /// <param name="message">Входящее сообщение.</param>
    /// <returns>Код ошибки.</returns>
    [ExpressionElement("ExpressionFuncName_GetCodeError", "ExpressionFuncDesc_GetCodeError")]
    public static string GetCodeError(IOutgoingMessage message)
    {
      if (message == null)
        return string.Empty;
      else
        return !string.IsNullOrEmpty(message?.ErrorText) && message.ErrorText.IndexOf(' ') > -1 ? message.ErrorText.Substring(0, message.ErrorText.IndexOf(' ')) : string.Empty;
    }

    #endregion

    #region Интеграционный сценарий "Передача документа на проштамповку".

    /// <summary>
    /// Сформировать текст исходящего сообщения для отправки документа в сервис проштамповки.
    /// </summary>
    /// <param name="doc">Документ.</param>
    /// <param name="version">Версия документа.</param>
    /// <param name="outgoingMessage">Исходящее сообщение.</param>
    /// <param name="overwritePublicBody">Признак того, что нужно перезаписать PublicBody.</param>
    /// <param name="isSimpleSignature">Тип ЭП.</param>
    /// <returns>Результат.</returns>
    public IResultGenerateRequestText GetTextForSendToStampingDocument(DirRX.OverrideSolution.IOfficialDocument doc, Sungero.Content.IElectronicDocumentVersions version,
                                                                       string messageGuid, bool overwritePublicBody, bool? isSimpleSignature)
    {
      var logPrefix = "GetTextForSendToStampingDocument";
      var result = ResultGenerateRequestText.Create();
      
      if (doc == null)
      {
        result.ErrorText = "Документ - не найден.";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      if (version == null)
      {
        result.ErrorText = "Не указана версия документа.";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      // Подпись.
      var signature = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.GetSignatureForMarkBySignatureType(doc, version.Id, isSimpleSignature);
      if (signature == null)
      {
        var signatureTypeString = string.Empty;
        if (isSimpleSignature.HasValue)
          signatureTypeString = isSimpleSignature.Value ? " с типом ПЭП" : " с типом УКЭП/УНЭП";
        
        result.ErrorText = string.Format("На версии не найдено подписей{0}.", signatureTypeString);
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      // Если тип подписи не указан, то выполнить проверку для определеие типа по подписи.
      if (!isSimpleSignature.HasValue)
        isSimpleSignature = NNCommons.PublicFunctions.Module.IsSimpleSignature(signature);
      
      Logger.DebugFormat("{0}. Реквизиты подписи для установки штампа. ИД документа: {1}. ИД версии: {2}. ИД подписи: {3}. Простая подпись: {4}.",
                         logPrefix, doc.Id, version.Id, signature.Id, isSimpleSignature);
      
      // Структура для передачи документа на проштамповку.
      var sendDocumentToStampingModel = DirRX.NNIntegration.Structures.Module.SendDocumentToStampingModel.Create();
      try
      {
        var attributes = DirRX.NNIntegration.Structures.Module.Attributes.Create();
        
        // Получить Guid шаблона штампа в зависимости от его типа.
        var templateId = string.Empty;
        // ПЭП.
        if (isSimpleSignature == true)
          templateId = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.TemplateIdSimpleSignature.Guid);
        // УНЭП.
        else if (NNCommons.PublicFunctions.Module.IsUnqualifiedSignature(signature))
          templateId = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.TemplateIdUnqualifiedSignature.Guid);
        // УКЭП.
        else
          templateId = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.TemplateIdQualifiedSignature.Guid);
        
        // Структура для парметров документа.
        var document = DirRX.NNIntegration.Structures.Module.DocumentForStamping.Create();
        // Владелец сертификата.
        document.certificate_owner = signature.SignatoryFullName;
        // Организация.
        document.organization = doc.BusinessUnit != null ? doc.BusinessUnit.Name : string.Empty;
        // Ключ проверки.
        document.verification_key = signature.Signatory.Id.ToString();
        // Отпечаток сертификата.
        document.certificate = isSimpleSignature == true ? string.Empty : signature.SignCertificate.Thumbprint;
        // Начало срока действия сертификата.
        document.certificate_valid_period_start = isSimpleSignature == true ? string.Empty : signature.SignCertificate.NotBefore.ToString();
        // Конец срока действия сертификата.
        document.certificate_valid_period_end = isSimpleSignature == true  ? string.Empty : signature.SignCertificate.NotAfter.ToString();
        // Дата подписания.
        document.signed_date_time = GetSigningDateForStampingService(signature.SigningDate);
        
        attributes.document = document;
        var attributesString = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(attributes);
        
        sendDocumentToStampingModel.template_id = templateId;
        sendDocumentToStampingModel.attributes = attributesString;
        sendDocumentToStampingModel.request_id = messageGuid;
        sendDocumentToStampingModel.OverwritePublicBodyFromVersionBody = overwritePublicBody;
        
        result.RequestText = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(sendDocumentToStampingModel);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("При заполнении тела сообщения произошла ошибка: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1}. StackTrace: {2}", logPrefix, result.ErrorText, ex.StackTrace);
        return result;
      }
      
      return result;
    }
    
    /// <summary>
    /// Сформировать текст исходящего сообщения для отправки документа в сервис проштамповки.
    /// </summary>
    /// <param name="docId">ИД документа.</param>
    /// <param name="guidMessage">Гуид исходящего сообщения.</param>
    /// <returns>Результат.</returns>
    /// <remarks>Перегрузка для запуска из case - функции.</remarks>
    public IResultGenerateRequestText GetTextForSendToStampingDocument(long docId, string guidMessage)
    {
      var doc = OverrideSolution.OfficialDocuments.GetAll(d => d.Id == docId).FirstOrDefault();
      
      // Исходящее сообщение.
      var outgoingMessage = OutgoingMessages.GetAll(x => x.GuidMessage == guidMessage).FirstOrDefault();
      if (outgoingMessage == null)
      {
        var result = ResultGenerateRequestText.Create();
        result.ErrorText = string.Format("Не найдено исходящее сообщение по guid: {0}", guidMessage);
        Logger.ErrorFormat("GetTextForSendToStampingDocument. {0}.", result.ErrorText);
        return result;
      }
      
      var version = doc?.Versions.FirstOrDefault(v => v.Id == outgoingMessage.VersionId);
      
      // Вызов основной логики. Не перезаписываем PublicBody, тип подписи не указываем, он определиться по подписи.
      return GetTextForSendToStampingDocument(doc, version, guidMessage, false, null);
    }

    /// <summary>
    /// Использовать сервис проштамповки?
    /// </summary>
    /// <returns>True - Сервис проштамповки используется, иначе не используется.</returns>
    [Public]
    public bool IsUseStampingService()
    {
      return NNCommons.PublicFunctions.Module.Remote.GetBoolValue(NNIntegration.PublicConstants.Module.ConstantsSetting.IsUseStampingService.Guid) == true;
    }

    /// <summary>
    /// Отправить многокомпонентный запрос в сервис проштамповки.
    /// </summary>
    /// <param name="outgoingMessage">Структура - Код результата, Текст ответа, Текст ошибки.</param>
    /// <param name="messageText">Текст сообщения.</param>
    /// <param name="result">Структура - Код результата, Текст ответа, Текст ошибки.</param>
    /// <returns>Структура - Код результата, Текст ответа, Текст ошибки.</returns>
    public Structures.Module.IResultSendingMessage SendMultipartContentStampingDocument(NNIntegration.IOutgoingMessage outgoingMessage, string messageText, Structures.Module.IResultSendingMessage result)
    {
      var logPrefix = string.Format("SendMultipartContentStampingDocument. Guid исх. сообщения: {0}", outgoingMessage.GuidMessage);
      var docId = outgoingMessage.EntityId;
      if (outgoingMessage.VersionId == null)
      {
        result.ErrorText = "Не заполнено поле \"Id версии\".";
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      var versionId = outgoingMessage.VersionId.Value;
      var doc = OverrideSolution.OfficialDocuments.GetAll().FirstOrDefault(d => d.Id == docId);
      if (doc == null)
      {
        result.ErrorText = string.Format("Не найден документ с ИД: {0}", docId);
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      var version = doc.Versions.FirstOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        result.ErrorText = string.Format("Не найдена версия документа с ИД: {0}", versionId);
        Logger.ErrorFormat("{0}. {1}.", logPrefix, result.ErrorText);
        return result;
      }
      
      ISendDocumentToStampingModel model = null;
      model = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(messageText, model);
      var needAuthorizationHeader = outgoingMessage.ExternalSystemFlow.ExternalSystem.NeedAuthorization == true;
      var password = Sungero.Core.Encryption.Decrypt(outgoingMessage.ExternalSystemFlow.ExternalSystem.Password);
      var body = !model.OverwritePublicBodyFromVersionBody && version.PublicBody != null && version.PublicBody.Size > 0 ? version.PublicBody : version.Body;
      var docName = string.Format("{0}_v{1}.pdf", doc.Id, version.Number);
      
      Logger.DebugFormat("{0}. Передавать заголовок Authorization: {1}. Перезаписать PublicBody: {2}.", logPrefix, needAuthorizationHeader, model.OverwritePublicBodyFromVersionBody);
      using (var versionBody = body.Read())
      {
        result = IsolatedFunctions.SendOutgoingMessage.SendDocumentStampingToExternalSystem(outgoingMessage.ExternalSystemFlow.ActionName, outgoingMessage.ExternalSystemFlow.ExternalSystem.Address,
                                                                                            outgoingMessage.ExternalSystemFlow.ExternalSystem.Port.ToString(), outgoingMessage.ExternalSystemFlow.ExternalSystem.Login,
                                                                                            password, versionBody, model, docName, result, needAuthorizationHeader);
      }
      return result;
    }

    /// <summary>
    /// Получить дату и время сервера для простановки отметки в сервисе проштамповки.
    /// </summary>
    /// <param name="signingDate">Дата подписания.</param>
    [Public]
    public virtual string GetSigningDateForStampingService(DateTime signingDate)
    {
      var signingDateLabel = string.Empty;
      using (Sungero.Core.CultureInfoExtensions.SwitchTo(TenantInfo.Culture))
      {
        var utcOffset = Calendar.UtcOffset.TotalHours;
        var utcOffsetLabel = utcOffset >= 0 ? "+" + utcOffset.ToString() : utcOffset.ToString();
        signingDateLabel = Sungero.Docflow.PublicFunctions.Module.ToShortDateShortTime(signingDate.AddHours(utcOffset));
        signingDateLabel = string.Format("{0:g} (UTC{1})", signingDateLabel, utcOffsetLabel);
      }
      return signingDateLabel;
    }

    #endregion

    #region Интеграционный сценарий "Получение результата асинхронной отправки данных из КСИП в АСУП".

    #region IF-S-11622. Получение результата асинхронной отправки данных из КСИП в КЭДО (КСИП → КЭДО).
    
    /// <summary>
    /// Запись передачи результата асинхронной отправки сообщения из КСИП в АСУП.
    /// </summary>
    /// <param name="RequestCode">Идентификатор запроса.</param>
    /// <param name="Result">Результат обработки запроса.</param>
    /// <param name="IsSuccessfullyProcessed">Результат проведения мероприятия в АСУП.</param>
    /// <param name="ErrorText">Текст ошибки.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest UpdateMessageSendingAsyncResult(string RequestCode, string Result, bool IsSuccessfullyProcessed, string ErrorText)
    {
      Logger.DebugFormat("UpdateMessageSendingAsyncResult. Start. RequestCode: {0}. Result: {1}. ErrorText: {2}. IsSuccessfullyProcessed: {3}", RequestCode, Result, ErrorText, IsSuccessfullyProcessed.ToString());
      var incomingMessageGuid = Guid.NewGuid();
      var bodyStructure = Structures.Module.MessageSendingAsyncResult.Create(RequestCode, Result, IsSuccessfullyProcessed, ErrorText);
      var requestBody = NNIntegration.IsolatedFunctions.Convert.SerializeInJson(bodyStructure);
      LoggerIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.GetSendingAsyncDataResultFromKSIP, Constants.Module.ExternalSystemsFlowActionNames.GetSendingAsyncDataResultFromKSIP,
                            requestBody, "Получено входящее сообщение.", false, incomingMessageGuid.ToString(), RequestCode);
      
      // Поиск исходящего сообщения.
      var outgoingMessage = OutgoingMessages.GetAll(x => x.GuidMessage == RequestCode).FirstOrDefault();
      var error = outgoingMessage == null ? string.Format("Не найдено исходящее сообщение по Guid: {0}.", RequestCode) : string.Empty;
      
      if (string.IsNullOrEmpty(error) && !ValidateMessageSendingAsyncResul(Result))
        error = string.Format("Некорректный результат отправки сообщения из КСИП в АСУП. Результат: {0}.", Result);
      
      if (string.IsNullOrEmpty(error))
        error = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.GetSendingAsyncDataResultFromKSIP,
                                      "GetSendingAsyncDataResultFromKSIP", requestBody, RequestCode);
      
      if (string.IsNullOrEmpty(error))
      {
        LoggerIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.GetSendingAsyncDataResultFromKSIP, Constants.Module.ExternalSystemsFlowActionNames.GetSendingAsyncDataResultFromKSIP,
                              string.Empty, "Входящее сообщение - обработано.", false, incomingMessageGuid.ToString(), RequestCode);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
      }
      else
      {
        LoggerIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.GetSendingAsyncDataResultFromKSIP, Constants.Module.ExternalSystemsFlowActionNames.GetSendingAsyncDataResultFromKSIP,
                              string.Empty, string.Format("Ошибка: {0}", error), true, incomingMessageGuid.ToString(), RequestCode);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, error);
      }
    }

    /// <summary>
    /// Проверить входящий результат обработки запроса на валидность.
    /// </summary>
    /// <param name="result">Результат проверки. True - если результат валиден. False - если результат не валиден.</param>
    public bool ValidateMessageSendingAsyncResul(string result)
    {
      return result == Constants.Module.SendingAsyncDataResultFromKSIP.Successfully ||
        result == Constants.Module.SendingAsyncDataResultFromKSIP.Error;
    }

    /// <summary>
    /// Обрботка результата асинхронной отправки данных из КСИП в АСУП.
    /// </summary>
    /// <param name="idIncommingMessage">ИД записи справочника Входящее информационное сообщение.</param>
    /// <returns>Результат обработки.</returns>
    public Structures.Module.IProcessIncomingMessageResult ProcessingMessageSendingResult(long idIncomingMessage)
    {
      var logPrefix = string.Format("ProcessingMessageSendingResult. Ид входящего сообщения: {0}.", idIncomingMessage);
      Logger.DebugFormat("{0} Начало обработки результата асинхронной отправки данных из КСИП в АСУП.", logPrefix);
      
      var result = NNIntegration.Structures.Module.ProcessIncomingMessageResult.Create();
      result.Retry = false;
      var errorMessage = string.Empty;
      
      // Поиск входящего сообщения.
      var incomingMessage = NNIntegration.IncomingMessages.GetAll(x => x.Id == idIncomingMessage).FirstOrDefault();
      if (incomingMessage == null)
      {
        result.ErrorText = "Не найдена запись справочника Входящие интеграционные сообщения.";
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      // Поиск связанного исходящего сообщения.
      var outgoingMessage = NNIntegration.OutgoingMessages.GetAll(x => x.GuidMessage == incomingMessage.GuidOutgoingMessage).FirstOrDefault();
      if (outgoingMessage == null)
      {
        result.ErrorText = string.Format("Не найдена связанная запись справочника Исходящие интеграционные сообщения. Guid исходящего сообщения: {0}.", incomingMessage.GuidOutgoingMessage);
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var jsonMessage = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      
      if (string.IsNullOrWhiteSpace(jsonMessage))
      {
        result.ErrorText = "Не удалось получить тело запроса.";
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var model = NNIntegration.Structures.Module.MessageSendingAsyncResult.Create();
      try
      {
        model = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Ошибка десериализации в структуру MessageSendingAsyncResult";
        Logger.ErrorFormat("{0} {1} Текст ошибки: {2}. StackTrace: {3}.", logPrefix, result.ErrorText, ex.Message, ex.StackTrace);
        return result;
      }
      
      // Установить блокировку на исходящее сообщение.
      var isLocked = Locks.TryLock(outgoingMessage);
      if (!Locks.GetLockInfo(outgoingMessage).IsLockedByMe && !isLocked)
      {
        var lockInfo = Locks.GetLockInfo(outgoingMessage);
        Logger.DebugFormat("{0} Не удалось установить блокировку. Исходящее сообщение заблокировано пользователем: {1}. Время установки блокировки: {2}. Будет запущена повторная обработка входящего сообщения.",
                           logPrefix, lockInfo.OwnerName, lockInfo.LockTime);
        result.Retry = true;
        return result;
      }
      
      // Обработка результата отправки исходящего сообщения и фиксация результата во входящем.
      if (model.Result == Constants.Module.SendingAsyncDataResultFromKSIP.Successfully && model.IsSuccessfullyProcessed == true)
      {
        outgoingMessage.StatusMessage = NNIntegration.OutgoingMessage.StatusMessage.Received;
        Logger.DebugFormat("{0} Сообщение успешно отправлено из КСИП в АСУП. Результат передачи КСИП-АСУП: {1}. Мероприятие проведено в АСУП: {2}. Новый статус исходящего сообщения: {3}.",
                           logPrefix, model.Result, model.IsSuccessfullyProcessed, NNIntegration.OutgoingMessages.Info.Properties.StatusMessage.GetLocalizedValue(outgoingMessage.StatusMessage));
      }
      else
      {
        outgoingMessage.StatusMessage = NNIntegration.OutgoingMessage.StatusMessage.Error;
        outgoingMessage.ErrorText = model.ErrorText;
        Logger.DebugFormat("{0} Ошибка при отправке сообщения из КСИП в АСУП. Результат передачи КСИП-АСУП: {1}. Мероприятие проведено в АСУП: {2}. Новый статус исходящего сообщения: {3}. Ошибка: {4}.",
                           logPrefix, model.Result, model.IsSuccessfullyProcessed, NNIntegration.OutgoingMessages.Info.Properties.StatusMessage.GetLocalizedValue(outgoingMessage.StatusMessage), outgoingMessage.ErrorText);
      }
      
      try
      {
        using (var session = Sungero.Domain.Session.CreateIsolatedSession())
        {
          session.Update(outgoingMessage);
          session.SubmitChanges();
        }
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Не удалось обновить сообщение. АО будет запущен повторно. Текст ошибки: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
        result.Retry = true;
        Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
        Locks.Unlock(outgoingMessage);
        return result;
      }
      
      Logger.DebugFormat("{0} Для потока требуется выполнение обработчиков активных блоков мониторинга для задачи: {1}. ИД основной задачи: {2}.",
                         logPrefix, incomingMessage.ExternalSystemFlow.WaitResponse == true, incomingMessage.EntityId);
      
      // Выполнить обработчики активных блоков мониторинга.
      if (outgoingMessage.ExternalSystemFlow.WaitResponse == true && outgoingMessage.EntityId.HasValue)
      {
        var subTask = PublicFunctions.Module.GetExternalSystemDataExchangeSubTask(outgoingMessage.EntityId.Value);
        if (subTask != null)
        {
          subTask.Blocks.ExecuteAllMonitoringBlocks();
          Logger.DebugFormat("{0} Запущено выполнение обработчиков активных блоков мониторинга для задачи. ИД задачи: {1}.", logPrefix, subTask.Id);
        }
        else
          Logger.DebugFormat("{0} Не найдена активная задача типа \"Обмен данными с внешней системой\" для исходящего сообщения. ИД основной задачи: {1}.",
                             logPrefix, outgoingMessage.EntityId);
      }

      Locks.Unlock(outgoingMessage);
      Logger.DebugFormat("{0} Завершение обработки результата асинхронной отправки данных из КСИП в АСУП.", logPrefix);
      return result;
    }
    #endregion

    #endregion

    #region Интеграционный сценарий "Компенсация части ежегодного оплачиваемого отпуска".

    #region IF-S-10396. Передача приказа на компенсацию части ежегодного оплачиваемого отпуска из АСУП в КЭДО.

    /// <summary>
    /// Создать приказ на компенсацию части ежегодного оплачиваемого отпуска.
    /// </summary>
    /// <param name="Signed">Подписал.</param>
    /// <param name="Prepared">Подготовил.</param>
    /// <param name="RegNumber">Регистрационный номер.</param>
    /// <param name="Date">Дата документа.</param>
    /// <param name="DateEvent">Дата мероприятия.</param>
    /// <param name="Body">Тело документа.</param>
    /// <param name="Employee">Сотрудник.</param>
    /// <param name="TaskID">ИД задачи.</param>
    /// <param name="SAPID">SAP ID</param>
    /// <returns>Результат обработки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateVacationCompensationOrderCard(string Signed, string Prepared, string RegNumber, string Date, string DateEvent, string Body, string Employee, long TaskID, string SAPID)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrWhiteSpace(Signed))
        errorMessage += "Не заполнено поле Подписал. ";
      if (string.IsNullOrWhiteSpace(Prepared))
        errorMessage += "Не заполнено поле Подготовил. ";
      if (string.IsNullOrWhiteSpace(RegNumber))
        errorMessage += "Не заполнен Регистрационный номер. ";
      if (string.IsNullOrWhiteSpace(Date))
        errorMessage += "Не заполнена Дата документа. ";
      if (string.IsNullOrWhiteSpace(DateEvent))
        errorMessage += "Не заполнена Дата мероприятия. ";
      if (string.IsNullOrWhiteSpace(Body))
        errorMessage += "Не заполнено содержимое документа в формате base64. ";
      if (string.IsNullOrWhiteSpace(Employee))
        errorMessage += "Не заполнен Сотрудник. ";
      if (string.IsNullOrWhiteSpace(SAPID))
        errorMessage += "Не заполнен SAPID. ";
      
      var date = ParseStringToDate(Date);
      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату документа: {0}", Date);
      var dateEvent = ParseStringToDate(DateEvent);
      if (!dateEvent.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату мероприятия: {0}", DateEvent);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        Logger.ErrorFormat("CreateVacationCompensationOrderCard. {0}. SAPID: {1}.", errorMessage, SAPID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      var model = Structures.Module.VacationCompensationOrderCard.Create(Signed.Trim().TrimStart('0'), Prepared.Trim().TrimStart('0'), RegNumber, date.Value, dateEvent.Value, Body, Employee.Trim().TrimStart('0'), TaskID, SAPID);
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateVacationCompensationOrderCard. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.CreateVacationCompensationOrder, "CreateVacationCompensationOrderCard", responseBody, string.Empty);
      
      if (!string.IsNullOrWhiteSpace(errorMessage))
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, errorMessage);
    }
    #endregion

    #endregion

    #region Интеграционный сценарий "Участие в благотворительности".

    #region IF-S-10397. Автоматическое создание карточки документа из АСУП в КЭДО (АСУП -> КЭДО).

    /// <summary>
    /// Обновить или создать "Заявление на удержание (программа личных пожертвований)".
    /// </summary>
    /// <param name="ProgramID">ID программы.</param>
    /// <param name="ProgramName">Наименование программы.</param>
    /// <param name="Employee">Табельный номер сотрудника.</param>
    /// <param name="Date">Дата подачи.</param>
    /// <param name="Body">Содержимое документа в формате base64.</param>
    /// <param name="Login">Логин.</param>
    /// <param name="DateSigned">Дата подписания.</param>
    /// <param name="SAPID">SAP ID.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest CreateCardRetentionStatementDocument(string ProgramID, string ProgramName, string Employee, string Date,
                                                                                 string Body, string Login, string DateSigned, string SAPID)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrEmpty(ProgramID))
        errorMessage += "Не заполнено ID программы.";
      if (string.IsNullOrEmpty(ProgramName))
        errorMessage += "Не заполнено наименование программы. ";
      if (string.IsNullOrEmpty(Employee))
        errorMessage += "Не заполнен табельный номер сотрудника. ";
      if (string.IsNullOrEmpty(Body))
        errorMessage += "Не заполнено cодержимое документа в формате base64. ";
      if (string.IsNullOrEmpty(Login))
        errorMessage += "Не заполнен логин. ";
      if (string.IsNullOrEmpty(SAPID))
        errorMessage += "Не заполнен SAPID. ";

      var date = ParseStringToDate(Date);
      var dateSigned = ParseStringToDate(DateSigned);
      
      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", Date);
      if (!dateSigned.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", DateSigned);
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("CreateCardRetentionStatementDocument. {0} SAP ID: '{1}'.", errorMessage, SAPID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }

      var model = RetentionStatementDocumentCard.Create();
      model.ProgramID = ProgramID;
      model.ProgramName = ProgramName;
      model.Employee = Employee.Trim();
      model.Date = date.Value;
      model.Body = Body;
      model.Login = Login;
      model.DateSigned = dateSigned.Value;
      model.SAPID = SAPID;
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateCardRetentionStatementDocument. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.CreateRetentionStatementDocument,
                                           "CreateCardRetentionStatementDocument", responseBody, string.Empty);
      
      return string.IsNullOrEmpty(errorMessage)
        ? CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty)
        : CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
    }

    #endregion

    #region IF-S-10398. Автоматическое формирование задания на ручное разблокирование ИТ0015 в АСУП (АСУП -> КЭДО).

    /// <summary>
    /// Получить структуру из записи 'Входящие информационные сообщения'.
    /// </summary>
    /// <param name="SAPID">Идентификатор документа, переданный в потоке.</param>
    /// <returns>Стандартный ответ системы КЭДО.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest CreateTaskForManualHoldingEvent(string SAPID)
    {
      Logger.Debug("CreateTaskForManualHoldingEvent. Получен входящий запрос. SAPID: {0}", SAPID);
      
      var errorMessage = string.Empty;
      if (string.IsNullOrEmpty(SAPID))
      {
        errorMessage = "Передано пустое значение SAPID.";
        Logger.ErrorFormat("CreateTaskForManualHoldingEvent. {0}", errorMessage);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }

      var structure = ManualHoldingEventStruct.Create();
      structure.SAPID = SAPID;
      var responseBody = string.Empty;
      
      try
      {
        responseBody = DirRX.NNIntegration.IsolatedFunctions.Convert.SerializeInJson(structure);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateTaskForManualHoldingEvent. {0} Текст ошибки: {1} StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.ManualHoldingEvent, "CreateTaskForManualHoldingEvent", responseBody, string.Empty);
      
      return string.IsNullOrEmpty(errorMessage)
        ? CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty)
        : CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
    }
    #endregion

    #endregion

    #region Интеграционный сценарий "Оформление отпуска по заявлению".

    #region IF-S-10408. Передача приказа на предоставление отпуска из АСУП в КЭДО (АСУП -> КЭДО).

    /// <summary>
    /// Создать приказ на ежегодный отпуск.
    /// </summary>
    /// <param name="Signed">Подписал.</param>
    /// <param name="Prepared">Подготовил.</param>
    /// <param name="RegNumber">Регистрацинный номер.</param>
    /// <param name="Date">Дата документа.</param>
    /// <param name="BodyAvia">Шаблон письма в авиакомпанию.</param>
    /// <param name="BodyOVD">Шаблон письма в ОВД.</param>
    /// <param name="Body">Шаблон приказа.</param>
    /// <param name="Employee">Сотрудник.</param>
    /// <param name="TaskID">ИД задачи.</param>
    /// <param name="SAPID">SAP ID.</param>
    /// <param name="Kind">Вид документа.</param>
    /// <returns>Ответ в виде json строки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateAnnualVacationOrder(string Signed, string Prepared, string RegNumber, string Date, string BodyAvia,
                                                                              string BodyOVD, string Body, string Employee, long TaskID, string SAPID, string Kind)
    {
      var errorMessage = string.Empty;
      
      if (string.IsNullOrWhiteSpace(Signed))
        errorMessage += "Не заполнено поле Подписал. ";
      if (string.IsNullOrWhiteSpace(Prepared))
        errorMessage += "Не заполнено поле Подготовил. ";
      if (string.IsNullOrWhiteSpace(RegNumber))
        errorMessage += "Не заполнен Регистрационный номер. ";
      if (string.IsNullOrWhiteSpace(Date))
        errorMessage += "Не заполнена Дата документа. ";
      if (string.IsNullOrWhiteSpace(Body))
        errorMessage += "Не заполнено содержимое шаблона приказа в формате base64. ";
      if (string.IsNullOrWhiteSpace(Employee))
        errorMessage += "Не заполнен Сотрудник. ";
      if (string.IsNullOrWhiteSpace(SAPID))
        errorMessage += "Не заполнен SAPID. ";
      if (string.IsNullOrWhiteSpace(Kind))
        errorMessage += "Не заполнен Вид документа. ";
      
      var date = ParseStringToDate(Date);
      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", Date);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        Logger.ErrorFormat("CreateAnnualVacationOrder. {0}. SAPID: {1}.", errorMessage, SAPID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      var model = Structures.Module.AnnualVacationOrderCard.Create(Signed.Trim().TrimStart('0'), Prepared.Trim().TrimStart('0'), RegNumber, date.Value, BodyAvia,
                                                                   BodyOVD, Body, Employee.Trim().TrimStart('0'), TaskID, SAPID, Kind);
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch(Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateAnnualVacationOrder. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.CreateAnnualVacationOrder, "CreateAnnualVacationOrder", responseBody, string.Empty);
      
      if (!string.IsNullOrWhiteSpace(errorMessage))
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, errorMessage);
    }

    #endregion

    #endregion

    #region Интеграционный сценарий "Регистрация прочих отпусков."

    #region IF-S-10411. Передача приказа на предоставление прочего отпуска из АСУП в КЭДО (АСУП -> КЭДО).

    /// <summary>
    /// Создать приказ на прочий отпуск.
    /// </summary>
    /// <param name="Signed">Подписал.</param>
    /// <param name="Prepared">Подготовил.</param>
    /// <param name="RegNumber">Регистрационный номер.</param>
    /// <param name="Date">Дата документа.</param>
    /// <param name="Body">Тело документа.</param>
    /// <param name="Employee">Сотрудник.</param>
    /// <param name="TaskID">ИД задачи.</param>
    /// <param name="SAPID">SAP ID.</param>
    /// <param name="Kind">Код вида документа.</param>
    /// <returns>Ответ в виде json строки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateOtherVacationOrder(string Signed, string Prepared, string RegNumber, string Date, string Body, string Employee, long TaskID, string SAPID, string Kind)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrWhiteSpace(Signed))
        errorMessage += "Не заполнено поле Подписал. ";
      if (string.IsNullOrWhiteSpace(Prepared))
        errorMessage += "Не заполнено поле Подготовил. ";
      if (string.IsNullOrWhiteSpace(RegNumber))
        errorMessage += "Не заполнен Регистрационный номер. ";
      if (string.IsNullOrWhiteSpace(Date))
        errorMessage += "Не заполнена Дата документа. ";
      if (string.IsNullOrWhiteSpace(Body))
        errorMessage += "Не заполнено содержимое документа в формате base64. ";
      if (string.IsNullOrWhiteSpace(Employee))
        errorMessage += "Не заполнен Сотрудник. ";
      if (string.IsNullOrWhiteSpace(SAPID))
        errorMessage += "Не заполнен SAPID. ";
      if (string.IsNullOrWhiteSpace(Kind))
        errorMessage += "Не заполнен Код вида документа. ";
      
      var date = ParseStringToDate(Date);
      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", Date);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        Logger.ErrorFormat("CreateOtherVacationOrder. {0}. SAPID: {1}.", errorMessage, SAPID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      var model = Structures.Module.OtherVacationOrderCard.Create(Signed.Trim().TrimStart('0'), Prepared.Trim().TrimStart('0'), RegNumber, date.Value, Body, Employee.Trim().TrimStart('0'), TaskID, SAPID, Kind);
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateOtherVacationOrder. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.CreateOtherVacationOrder, "CreateOtherVacationOrder", responseBody, string.Empty);
      
      if (!string.IsNullOrWhiteSpace(errorMessage))
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, errorMessage);
    }

    #endregion

    #endregion
    
    #region Интеграционный сценарий "Отзыв из отпуска."
    
    #region IF-S-10391. Передача приказа об отзыве из отпуска из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Создать приказ об отзыве из отпуска.
    /// </summary>
    /// <param name="Signed">Подписал.</param>
    /// <param name="Prepared">Подготовил.</param>
    /// <param name="RegNumber">Регистрационный номер.</param>
    /// <param name="Date">Дата документа.</param>
    /// <param name="Body">Тело документа.</param>
    /// <param name="Employee">Сотрудник.</param>
    /// <param name="Basis">Основание для приказа.</param>
    /// <param name="TaskID">ИД задачи.</param>
    /// <param name="SAPID">SAP ID.</param>
    /// <returns>Ответ в виде json строки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateRecallVacationOrder(string Signed, string Prepared, string RegNumber, string Date, string Body, string Employee, string Basis, long TaskID, string SAPID)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrWhiteSpace(Signed))
        errorMessage += "Не заполнено поле Подписал. ";
      if (string.IsNullOrWhiteSpace(Prepared))
        errorMessage += "Не заполнено поле Подготовил. ";
      if (string.IsNullOrWhiteSpace(RegNumber))
        errorMessage += "Не заполнен Регистрационный номер. ";
      if (string.IsNullOrWhiteSpace(Date))
        errorMessage += "Не заполнена Дата документа. ";
      if (string.IsNullOrWhiteSpace(Body))
        errorMessage += "Не заполнено содержимое документа в формате base64. ";
      if (string.IsNullOrWhiteSpace(Employee))
        errorMessage += "Не заполнен Сотрудник. ";
      if (string.IsNullOrWhiteSpace(Basis))
        errorMessage += "Не заполнено Основание для приказа. ";
      if (string.IsNullOrWhiteSpace(SAPID))
        errorMessage += "Не заполнен SAPID. ";
      
      var date = ParseStringToDate(Date);
      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", Date);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        Logger.ErrorFormat("CreateRecallVacationOrder. {0}. SAPID: {1}.", errorMessage, SAPID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      var model = Structures.Module.RecallVacationOrderCard.Create(Signed.Trim().TrimStart('0'), Prepared.Trim().TrimStart('0'), RegNumber, date.Value, Body, Employee.Trim().TrimStart('0'), Basis, TaskID, SAPID);
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateRecallVacationOrder. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.CreateRecallVacationOrder, "CreateRecallVacationOrder", responseBody, string.Empty);
      
      if (!string.IsNullOrWhiteSpace(errorMessage))
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, errorMessage);
    }
    
    #endregion
    
    #region IF-S-28542 Данные по оплатам отсутствий из АСУП в КЭДО (АСУП → КЭДО).
    
    /// <summary>
    /// Создать входящее информационное сообщение с информацией по оплаченным отпускам сотрудников.
    /// </summary>
    /// <param name="RequestCode">Идентификатор запроса.</param>
    /// <param name="Employee">Табельный номер сотрудника.</param>
    /// <param name="Vacations">Отпуска</param>
    /// <returns>Результат обработки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateVacationsPaymentInfo(string RequestCode, string Employee, List<DirRX.NNVacations.Structures.Module.IVacationInfoIFS28542> Vacations)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrWhiteSpace(RequestCode))
        errorMessage += "Не заполнен Идентификатор запроса. ";
      if (string.IsNullOrWhiteSpace(Employee))
        errorMessage += "Не заполнен Сотрудник. ";
      
      // Поиск исходящего сообщения.
      var outgoingMessage = OutgoingMessages.GetAll(x => x.GuidMessage == RequestCode).FirstOrDefault();
      if (outgoingMessage == null)
        errorMessage += string.Format("Не найдено исходящее сообщение по GUID: {0}.", RequestCode);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        Logger.ErrorFormat("CreateVacationsPaymentInfo. {0}", errorMessage);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      Employee = Employee.Trim().TrimStart('0');
      var model = DirRX.NNVacations.Structures.Module.EmployeeVacationsIFS28542.Create(RequestCode, Employee, Vacations);
      var responseBody = string.Empty;
      
      try
      {
        responseBody = DirRX.NNVacations.IsolatedFunctions.Json.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateVacationsPaymentInfo. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.VacationsPaymentInfo, "CreateVacationsPaymentInfo", responseBody, RequestCode);
      
      if (!string.IsNullOrWhiteSpace(errorMessage))
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, errorMessage);
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий "Передача графика отпусков".

    #region IF-S-6365. Создание карточки согласования графика отпусков из АСУП в КЭДО.

    /// <summary>
    /// Обновить или создать "График отпусков".
    /// </summary>
    /// <param name="Body">Содержимое документа в формате base64.</param>
    /// <param name="OrgType">SAP Тип организации.</param>
    /// <param name="OrgCode">SAP Код юридического лица.</param>
    /// <param name="Year">На год.</param>
    /// <param name="SAPID">Идентификатор документа в SAP.</param>
    /// <param name="Employees">Список работников.</param>
    /// <param name="RespEmployee">Ответственный за График отпусков.</param>
    /// <returns>Результат обработки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest CreateVacationScheduleDocument(string Body, string OrgType, string OrgCode, string Year, string SAPID, List<Structures.Module.IVacationInfoFromSAP> Employees, string RespEmployee)
    {
      var errorMessage = string.Empty;
      var businessUnit = NNIntegration.PublicFunctions.Module.GetBusinessUnitBySAPId(OrgType + OrgCode);
      if (string.IsNullOrEmpty(Body))
        errorMessage += "Не заполнено Тело документа. ";
      if (string.IsNullOrEmpty(OrgType))
        errorMessage += "Не заполнен Тип организации. ";
      if (string.IsNullOrEmpty(OrgCode))
        errorMessage += "Не заполнен Код юридического лица. ";
      if (string.IsNullOrEmpty(Year))
        errorMessage += "Не заполнено На год. ";
      if (string.IsNullOrEmpty(SAPID))
        errorMessage += "Не заполнен Идентификатор документа в SAP. ";
      if (businessUnit == null)
        errorMessage += "Не найдена запись справочника Наши организации. ";
      if (!Employees.Any())
        errorMessage += "Список работников пустой. ";
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        if (!string.IsNullOrEmpty(OrgCode))
          errorMessage += string.Format("Код юридического лица: {0}.", OrgCode);
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("CreateVacationScheduleDocument. {0}", errorMessage);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      foreach (var employeeInfo in Employees)
      {
        employeeInfo.Employee = employeeInfo.Employee.Trim().TrimStart('0');
      }
      
      var model = Structures.Module.IncomingVacationScheduleDocumentInfoFromSAP.Create();
      model.Body = Body;
      model.OrgType = OrgType;
      model.OrgCode = OrgCode;
      model.Year = Year;
      model.SAPID = SAPID;
      model.Employees = Employees;
      model.Responsible = RespEmployee;
      
      var responseBody = string.Empty;
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateVacationScheduleDocument. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = NNIntegration.PublicFunctions.Module.CreateIncomingMessage(NNIntegration.Constants.Module.ExternalSystemsFlowGuids.CreateVacationScheduleDocument,
                                                                                "CreateVacationScheduleDocument", responseBody, string.Empty, string.Empty);
      return string.IsNullOrEmpty(errorMessage)
        ? CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty)
        : CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
    }

    #endregion
    
    #region IF-S-29001. Проверка УЗ Ответственного за график отпусков в КЭДО.

    /// <summary>
    /// Проверить УЗ Ответственного за график отпусков в КЭДО.
    /// </summary>
    /// <param name="PP0000PERNR">Табельный номер.</param>
    /// <returns>Результат обработки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public ICheckKedoLoginResult CheckKedoLogin(string PP0000PERNR)
    {
      var result = Structures.Module.CheckKedoLoginResult.Create();
      result.Result = false;
      
      var errorMessage = string.Empty; 
      try
      {
        if (string.IsNullOrEmpty(PP0000PERNR))
          Logger.Error("CheckKedoLogin. Не заполнен Табельный номер.");
        else
        {
          var employee = NNIntegration.Functions.Module.GetEmployeeByPersonnelNumber(PP0000PERNR);
          if (employee == null)
            Logger.ErrorFormat("CheckKedoLogin. Не найден Сотрудник по ТН: {0}.", PP0000PERNR);
          else
          {
            var login = employee.Login;
            if (login == null)
              Logger.ErrorFormat("CheckKedoLogin. У сотрудника ИД: {0} не заполнена Учетная запись.", employee.Id);
            else
            {
              if (login.Status != Sungero.CoreEntities.DatabookEntry.Status.Active)
                Logger.ErrorFormat("CheckKedoLogin. Учетная запись ИД: {0} не является действующей.", login.Id);
              else
                result.Result = !login.LoginName.Contains(Constants.Module.HRProLoginPrefix);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CheckKedoLogin. Возникла ошибка при обработке запроса. Текст ошибки: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
      }
      
      return result;
    }

    #endregion

    #endregion

    #region Интеграционный сценарий "Создание учетных записей в КЭДО".

    #region IF-S-6377. Автоматическое создание учетных записей из АСУП в КЭДО.

    /// <summary>
    /// Создать УЗ RX и Лк.
    /// </summary>
    /// <param name="Employee">Табельный номер сотрудника.</param>
    /// <param name="Login">Логин УЗ.</param>
    /// <param name="Type">Тип УЗ.</param>
    /// <param name="IsKEDO">Подписано согласие на КЭДО.</param>
    /// <param name="Provider">Провайдер.</param>
    /// <returns>Ответ в Json.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest CreateAccount(string Employee, string Login, string Type, bool IsKEDO, string Provider)
    {
      var logPrefix = string.Format("CreateAccount. Табельный номер: '{0}'. Тип УЗ '{1}'. Согласие на КЭДО '{2}'. Провайдер '{3}'", Employee, Type, IsKEDO, Provider);
      Employee = Employee.Trim().TrimStart('0');
      
      Logger.DebugFormat("{0}. Получен запрос на создание УЗ.", logPrefix);
      var processingError = ProcessingErrorStruct.Create();
      var model = ProcessIncomingData(Employee, Login, Type, IsKEDO, Provider, processingError);

      if (model == null)
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, processingError.ErrorText);
      
      var employee = DirRX.OverrideSolution.PublicFunctions.Employee.GetAvailabilityEmployee(model.Employee, processingError);

      if (employee == null)
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, processingError.ErrorText);
      
      // Подключаем основного сотрудника и создаём ему УЗ.
      var mainEmployee = Employees.As(DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(employee.Person.Id, null));
      
      if (mainEmployee == null)
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, DirRX.NNIntegration.Resources.EmployeeDoesntHaveMainEmployeeErrorFormat(model.Employee));
      
      Logger.DebugFormat("{0}. Найден сотрудник с ИД={1} в НОР с ИД={2}, основной сотрудник с ИД={3} в НОР с ИД={4}.",
                         logPrefix, employee.Id.ToString(), employee.BusinessUnit.Id.ToString(), mainEmployee.Id.ToString(), mainEmployee.BusinessUnit.Id.ToString());
      
      // Сохраняем ИД НОР сотрудника, по которому пришел запрос из АСУП.
      model.BusinessUnitId = employee.BusinessUnit.Id;
      
      // Анализируем изменение значения св-ва "Согласие на КЭДО" сотрудника, по которому пришёл запрос из АСУП.
      model.ConsentChangeFlag = Constants.Module.CreateAccount.ConsentChangeType.None;

      if (model.IsKEDO && employee.ConsentHRPro != DirRX.OverrideSolution.Employee.ConsentHRPro.Signed)
        model.ConsentChangeFlag = Constants.Module.CreateAccount.ConsentChangeType.BecameSigned;
      else if (!model.IsKEDO && employee.ConsentHRPro == DirRX.OverrideSolution.Employee.ConsentHRPro.Signed)
        model.ConsentChangeFlag = Constants.Module.CreateAccount.ConsentChangeType.BecameUnsigned;
      
      // Анализируем изменение значения св-ва "Согласие на КЭДО" сотрудника, по которому пришёл запрос из АСУП.
      model.ConsentChangeFlag = Constants.Module.CreateAccount.ConsentChangeType.None;

      if (model.IsKEDO && employee.ConsentHRPro != DirRX.OverrideSolution.Employee.ConsentHRPro.Signed)
        model.ConsentChangeFlag = Constants.Module.CreateAccount.ConsentChangeType.BecameSigned;
      else if (!model.IsKEDO && employee.ConsentHRPro == DirRX.OverrideSolution.Employee.ConsentHRPro.Signed)
        model.ConsentChangeFlag = Constants.Module.CreateAccount.ConsentChangeType.BecameUnsigned;
      
      // Согласие меняем у сотрудника, по которому пришёл запрос.
      processingError.ErrorText = DirRX.OverrideSolution.PublicFunctions.Employee.ChangeConsent(employee, model.IsKEDO);
      
      // Обработка создания УЗ.
      if (string.IsNullOrEmpty(processingError?.ErrorText))
      {
        switch (Type)
        {
          case Constants.Module.CreateAccount.AccountType.Rx:
            Logger.DebugFormat("{0}. Запуск создания УЗ RX.", logPrefix);
            processingError = RxAccountProcessing(model, mainEmployee);
            break;
          case Constants.Module.CreateAccount.AccountType.Kedo:
            // Асинхронное создание УЗ через ФП.
            if (NNCommons.PublicFunctions.Module.Remote.GetBoolValue(Constants.Module.ConstantsSetting.AsyncCreateESSUserAccount.Guid) == true)
            {
              Logger.DebugFormat("{0}. Создание записи очереди для создания УЗ ЛК HRPro.", logPrefix);
              var queueItem = ESSUserAccountQueues.Create();
              queueItem.PersonnelNumber = Employee;
              queueItem.Login = Login;
              queueItem.IsKEDO = IsKEDO;
              queueItem.ConsentChangeType = model.ConsentChangeFlag;
              queueItem.Save();
            }
            else
            {
              Logger.DebugFormat("{0}. Запуск создания УЗ ЛК HRPro.", logPrefix);
              processingError = KedoAccountProcessing(model, mainEmployee);
            }
            break;
        }
      }

      if (string.IsNullOrEmpty(processingError?.ErrorText))
      {
        Logger.DebugFormat("{0}. УЗ успешно создана.", logPrefix);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
      }

      Logger.ErrorFormat("{0}. Во время создания УЗ произошла ошибка. Текст ошибки: {1}.", logPrefix, processingError.ErrorText);
      
      return processingError.isBlock ?
        CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Block, processingError.ErrorText) :
        CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, processingError.ErrorText);
    }

    # region Создание УЗ RX.
    /// <summary>
    /// Создание УЗ RX.
    /// </summary>
    /// <param name="model">Данные с интеграции.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Структура ошибок.</returns>
    public virtual IProcessingErrorStruct RxAccountProcessing(ICreateAccountStruct model, IEmployee employee)
    {
      var processingError = ProcessingErrorStruct.Create();

      processingError.ErrorText = CheckLoginAvailability(model.Login);
      
      if (!string.IsNullOrEmpty(processingError.ErrorText))
        return processingError;
      
      if (employee.Login == null || (employee.Login.LoginName != model.Login) ||
          (employee.Login.TypeAuthentication != TypeAuthentication.Windows))
      {
        if (!CreateProcessing(employee, processingError, model))
          return processingError;
      }
      
      if (employee.Login?.Status == Sungero.CoreEntities.DatabookEntry.Status.Closed)
        ChangeLoginStatus(employee.Login, Sungero.CoreEntities.DatabookEntry.Status.Active , processingError);
      
      return processingError;
    }

    /// <summary>
    /// Процесс создания аккаунта RX.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="processingError">Структура ошибок.</param>
    /// <param name="model">Переданные параметры по интеграции.</param>
    /// <returns>True если процесс прошел успеншно, иначе false.</returns>
    private bool CreateProcessing(IEmployee employee, IProcessingErrorStruct processingError, ICreateAccountStruct model)
    {
      var mainEmployee = Employees.Null;

      if (employee.EmploymentType.HasValue && employee.EmploymentType != EmploymentType.MainPlace)
        mainEmployee = DirRX.OverrideSolution.PublicFunctions.Employee.GetMainEmployee(employee);
      
      else if (employee.EmploymentType.HasValue)
        mainEmployee = employee;

      if (mainEmployee == null)
      {
        processingError.ErrorText = string.Format("Сотрудник с табельным номером: '{0}' не имеет совместителей с основным местом работы.", employee.PersonnelNumber);
        return false;
      }
      
      if (mainEmployee.Login != null)
      {
        if (!ChangeLoginStatus(mainEmployee.Login, Sungero.CoreEntities.DatabookEntry.Status.Closed, processingError))
          return false;
      }
      
      try
      {
        // Заблокировать запись.
        if (!string.IsNullOrEmpty(DirRX.OverrideSolution.PublicFunctions.Employee.TryLockEmployee(employee)))
          return false;
        
        // Создать УЗ RX.
        var login = CreateRxAccount(model, processingError);

        if (login == null)
          return false;
        
        // Сменить УЗ у сотрудника.
        processingError.ErrorText = ChangeAccount(employee, login);
        
        if (!string.IsNullOrEmpty(processingError.ErrorText))
          return false;
        // Отчистить УЗ у совместителей.
        ClearAccountInPartTimers(employee);

        return true;
      }
      finally
      {
        if (Locks.GetLockInfo(employee).IsLockedByMe)
          Locks.Unlock(employee);
      }
    }

    /// <summary>
    /// Изменить статус УЗ RX.
    /// </summary>
    /// <param name="login">Учетная запись.</param>
    /// <param name="status">Статус.</param>
    /// <param name="processingError">Ошибки.</param>
    /// <returns>True если изменения прошли успешно, иначе false.</returns>
    private bool ChangeLoginStatus(ILogin login, Enumeration status, IProcessingErrorStruct processingError)
    {
      try
      {
        if (!Locks.TryLock(login))
        {
          processingError.ErrorText = string.Format("Не удалось заблокировать учетную запись с ИД '{0}'.", login.Id);
          processingError.isBlock = true;
          Logger.ErrorFormat("ChangeLoginStatus. {0}", processingError.ErrorText);
          return false;
        }
        
        login.Status = status;
        login.Save();
        
        return true;
      }
      catch (Exception e)
      {
        processingError.ErrorText = string.Format("Не удалось сохранить учетную запись с ИД: '{0}'. Текст ошибки: {1}", login.Id, e.Message);
        Logger.ErrorFormat("TrySaveLogin. {0}. StackTrace: {1}", processingError.ErrorText, e.StackTrace);
        
        return false;
      }
      finally
      {
        if (Locks.GetLockInfo(login).IsLockedByMe)
          Locks.Unlock(login);
      }
    }

    /// <summary>
    /// Создание учетной записи RX.
    /// </summary>
    /// <param name="model">Данные для создания.</param>
    /// <param name="processingError">Ошибки.</param>
    /// <returns>Учетная запись.</returns>
    private ILogin CreateRxAccount(ICreateAccountStruct model, IProcessingErrorStruct processingError)
    {
      try
      {
        var login = Sungero.CoreEntities.Logins.Create();
        login.LoginName = model.Login;
        login.TypeAuthentication = TypeAuthentication.Windows;
        login.Status = Sungero.CoreEntities.DatabookEntry.Status.Active;
        login.Save();
        Logger.DebugFormat("CreateRxAccount. Логин с ИД: {0}. Успешно создан.", login.Id);
        
        return login;
      }
      catch (Exception e)
      {
        processingError.ErrorText = string.Format("Ошибка создания учетной записи. Текст ошибки: {0}.", e.Message);
        Logger.ErrorFormat("CreateRxAccount. {0} StackTrace: {1}.", processingError.ErrorText, e.StackTrace);
        
        return null;
      }
    }

    /// <summary>
    /// Поменять УЗ у сотрудников.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="login">Учетная запись.</param>
    /// <param name="processingError">Ошибки.</param>
    /// <returns>Текст ошибки, иначе string.Empty..</returns>
    private string ChangeAccount(IEmployee employee, ILogin login)
    {
      var logPrefix = string.Format("ChangeAccount. Логин у сотрудника с ИД: {0}.", employee.Id);
      employee.Login = login;
      var processingError = DirRX.OverrideSolution.PublicFunctions.Employee.TrySaveEmployee(employee);

      if (string.IsNullOrEmpty(processingError))
      {
        Logger.DebugFormat("{0}. Успешно изменен.",  logPrefix);
        return string.Empty;
      }
      
      Logger.DebugFormat("{0}. Произошла ошибка.", logPrefix);
      return processingError;
    }

    /// <summary>
    /// Запуск АО "Удаление УЗ у совместителей".
    /// </summary>
    /// <param name="employee">Сотрудник с основным местом работы.</param>
    private void ClearAccountInPartTimers(IEmployee employee)
    {
      var clearAccount = DirRX.NNIntegration.AsyncHandlers.ClearAccountInPartTimersAsync.Create();
      clearAccount.MainEmployeeId = employee.Id;
      clearAccount.ExecuteAsync();
    }

    #endregion

    #region Создание УЗ для ЛК.

    /// <summary>
    /// Создание УЗ для ЛК. Перегрузка с необхоимостью создать УЗ в RX и добавить в роль "Неинтерактивные пользователи".
    /// </summary>
    /// <param name="model">Данные с интеграции.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <returns>Структура ошибок.</returns>
    [Public]
    public virtual IProcessingErrorStruct KedoAccountProcessing(ICreateAccountStruct model, IEmployee employee)
    {
      return KedoAccountProcessing(model, employee, true);
    }
    
    /// <summary>
    /// Создание УЗ для ЛК.
    /// </summary>
    /// <param name="model">Данные с интеграции.</param>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="needIncludeToNonInteractiveUsersRole">Нужно ли создать УЗ в RX и добавить в роль "Неинтерактивные пользователи".</param>
    /// <returns>Структура ошибок.</returns>
    [Public]
    public virtual IProcessingErrorStruct KedoAccountProcessing(ICreateAccountStruct model, IEmployee employee, bool needIncludeToNonInteractiveUsersRole)
    {
      var logPrefix = "KedoAccountProcessing";
      var processingError = ProcessingErrorStruct.Create();
      var needReissueCertificate = false;
      
      // Валидация сотрудника, персоны и номера телефона.
      processingError.ErrorText = OverrideSolution.PublicFunctions.Employee.Validate(employee, model.Login);

      if (!string.IsNullOrEmpty(processingError.ErrorText))
        return processingError;
      
      // Если номер телефона отличается от номера телефона, указанного в сотруднике – то создавать УЗ с номером полученном в запросе.
      var requestPhone = OverrideSolution.PublicFunctions.Employee.GetFormatPhoneNumber(employee, model.Login);
      var phoneIsChanged = employee.PersonalPhoneDirRX != requestPhone;
      
      if (phoneIsChanged)
      {
        Logger.DebugFormat("{0}. ИД cотрудника: {1}. Номер телефона в запросе не соответсвует номеру личного телефона сотрудника. Телефон сотрудника будет обновлён и он будет использоваться при создании УЗ.", logPrefix, employee.Id);
        // Если номер телефона был изменён, то в событии "После сохранения" в справочнике Сотрудники будет запущен АО, который синхронизирует новый номер в совместителей.
        employee.PersonalPhoneDirRX = requestPhone;
      }
      else
        Logger.DebugFormat("{0}. ИД cотрудника: {1}. Номер телефона в запросе соответсвует номеру личного телефона сотрудника.", logPrefix, employee.Id);

      if (!string.IsNullOrEmpty(processingError.ErrorText))
        return processingError;
      
      // Если сотрудник не был сохранён в рамках смены статуса согласия, то сохранить изменения телефона.
      if (employee.State.IsChanged)
      {
        try
        {
          employee.Save();
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("{0}. ИД cотрудника: {1}. Не удалось сохранить сотрудника. Ошибка: {2}. StackTrace: {3}.", logPrefix, employee.Id, ex.Message, ex.StackTrace);
          processingError.ErrorText = ex.Message;
          return processingError;
        }
      }

      // Обновление фиктивного номера телефона у сотрудника, у которого номер для выпуска сертификата совпадает с новым личным телефоном.
      if (phoneIsChanged)
        DirRX.OverrideSolution.PublicFunctions.Employee.CheckEmployeesCertificatePhone(employee);
      
      var isConsentSigned = DirRX.OverrideSolution.PublicFunctions.Employee.Remote.CheckConsentSigned(employee, true);
      var hasModelData = model.ConsentChangeFlag != null && model.BusinessUnitId != null;
      var isConsentUnchangedAndSigned = model.ConsentChangeFlag == Constants.Module.CreateAccount.ConsentChangeType.None && isConsentSigned;
      var isConsentBecameSigned = model.ConsentChangeFlag == Constants.Module.CreateAccount.ConsentChangeType.BecameSigned;
      var isConsentBecameUnsigned = model.ConsentChangeFlag == Constants.Module.CreateAccount.ConsentChangeType.BecameUnsigned;
      
      try
      {
        // Если сотрудник не подключен к ЛК, то создаём УЗ и активируем её в ЛК.
        if (employee.PersonalAccountStatusDirRX == OverrideSolution.Employee.PersonalAccountStatusDirRX.InviteIsNotSent)
        {
          // Создание УЗ в RX и добавление в роль "Неинтерактивные пользователи".
          // Если Асинхронное создание УЗ через ФП, то это делается в ФП. 
          if (needIncludeToNonInteractiveUsersRole)
            DirRX.EssPlatform.PublicFunctions.Module.Remote.FillAccountAndIncludeNonInteractiveUsersRoles(employee.Id);
          // Создание и активация УЗ в ЛК.
          DirRX.OverrideSolution.PublicFunctions.Employee.ActivateEssEmployee(employee);
          Logger.DebugFormat("{0}. ИД cотрудника: {1}. Сотрудник не был подключен к ЛК. Учётная запись сотрудника ЛК успешно создана.", logPrefix, employee.Id);
        }
        // Если сотрудник подключен к ЛК и изменился телефон, то необходимо его обновить в ЛК.
        else if (phoneIsChanged)
        {
          processingError.ErrorText = ChangeEmployeePhoneNumber(logPrefix, employee, string.Empty);
          
          if (!string.IsNullOrEmpty(processingError.ErrorText))
          {
            Logger.ErrorFormat("{0}. ИД cотрудника {1}. {2}.", logPrefix, employee.Id, processingError.ErrorText);
            return processingError;
          }
          // Перевыпуск при смене номера возможен только если согласие подписано:
          // 1. Из CreateAccount: изменения не было, но есть хотя бы один сотрудник с подписанным согласием.
          // 2. Из CreateAccount: изменение было с "Не подписано" на "Подписано".
          // 3. Из CreateAccount: изменение было с "Подписано" на "Не подписано", но есть хотя бы один внешний сотрудник с подписанным согласием.
          // 4. Не из CreateAccount - есть хотя бы один сотрудник с подписанным согласием.
          if (hasModelData && (isConsentUnchangedAndSigned || isConsentBecameSigned || (isConsentBecameUnsigned && HasSignedConsentExcludingBusinessUnit(employee.Person.Id, model.BusinessUnitId.Value))) || isConsentSigned)
          {
            needReissueCertificate = true;
            Logger.DebugFormat("{0}. ИД cотрудника: {1}. У сотрудника изменился телефон и подписано согласие на КЭДО. Необходимо перевыпустить сертификат.", logPrefix, employee.Id);
          }
          // Необходимо прекратить задачу и закрыть сертификат из CreateAccount при условии что
          // изменение было с "Подписано" на "Не подписано", и нет ни одного внешнего сотрудника с подписанным согласием.
          else if (hasModelData && (isConsentBecameUnsigned && !HasSignedConsentExcludingBusinessUnit(employee.Person.Id, model.BusinessUnitId.Value)))
          {
            Logger.DebugFormat("{0}. ИД cотрудника: {1}. У сотрудника изменился телефон и изменилось согласие на КЭДО с Подписано на Не подписано. Необходимо прекратить действующую задачу и закрыть сертификат.", logPrefix, employee.Id);
            
            AbortActiveCertificateIssueTaskForPerson(employee.Person.Id, logPrefix);
            DisableCertificateForPerson(employee.Person.Id, logPrefix);
          }
        }
        // Если сотрудник подключен к ЛК и телефон не изменился, но изменилось согласие c Не подписано на Подписано,
        // то необходимо перевыпустить сертификат, если действующей задачи на выпуск или самого сертификата еще нет.
        else if (hasModelData && isConsentBecameSigned && !HasActiveCertificateIssueTaskOrCertificateForPerson(employee.Person.Id))
        {
          Logger.DebugFormat("{0}. ИД cотрудника: {1}. У сотрудника изменилось согласие на КЭДО с Не подписано на Подписано. Необходимо перевыпустить сертификат.", logPrefix, employee.Id);
          needReissueCertificate = true;
        }
        // Если сотрудник подключен к ЛК и телефон не изменился, но изменилось согласие c Подписано на Не подписано,
        // то необходимо прекратить действующую задачу на выпуск, закрыть сертификат, если больше нет сотрудников, у которых согл. подписано.
        else if (hasModelData && isConsentBecameUnsigned && !HasSignedConsentExcludingBusinessUnit(employee.Person.Id, model.BusinessUnitId.Value))
        {
          Logger.DebugFormat("{0}. ИД cотрудника: {1}. У сотрудника изменилось согласие на КЭДО с Подписано на Не подписано. Необходимо прекратить действующую задачу и закрыть сертификат.", logPrefix, employee.Id);
          
          AbortActiveCertificateIssueTaskForPerson(employee.Person.Id, logPrefix);
          DisableCertificateForPerson(employee.Person.Id, logPrefix);
        }
        // Тут окажемся в рамках вызова из ФП при обработке Линейных руководителей, либо подключении к Лк из карточки сотрудника,
        // поэтому если признак изменения не пришел, идем просто перевыпускать серт при подписанном согласии.
        else if (!hasModelData && isConsentSigned)
        {
          Logger.DebugFormat("{0}. ИД cотрудника: {1}. У сотрудника согласие на КЭДО Подписано. Необходимо перевыпустить сертификат.", logPrefix, employee.Id);
          needReissueCertificate = true;
        }
        else
          Logger.DebugFormat("{0}. ИД cотрудника: {1}. У сотрудника уже создана УЗ для ЛК.", logPrefix, employee.Id);
        
        if (needReissueCertificate)
        {
          Logger.DebugFormat("{0}. ИД cотрудника: {1}. Запущен АО ReissueCertificateAsync.", logPrefix, employee.Id);
          var asyncHandler = DirRX.NNIntegration.AsyncHandlers.ReissueCertificateAsync.Create();
          asyncHandler.EmployeeId = employee.Id;
          asyncHandler.ExecuteAsync();
        }
      }
      catch (Exception ex)
      {
        processingError.ErrorText = string.Format("Ошибка создания и активации УЗ для ЛК. Текст ошибки: {0}.", ex.Message);
        Logger.ErrorFormat("{0}. ИД cотрудника {1}. {2} StackTrace: {3}.", logPrefix, employee.Id, processingError.ErrorText, ex.StackTrace);
        return processingError;
      }

      // Изменить статус УЗ на "Подключен".
      try
      {
        DirRX.EssPlatform.PublicFunctions.Module.ChangePersonalAccountStatusToInviteAccepted(employee.Id);
      }
      catch (Exception ex)
      {
        processingError.ErrorText = string.Format("Ошибка изменения статуса УЗ для ЛК. Текст ошибки: {0}.", ex.Message);
        Logger.ErrorFormat("{0}. ИД cотрудника: {1}. {2} StackTrace: {3}.", logPrefix, employee.Id, processingError.ErrorText, ex.StackTrace);
        return processingError;
      }
      
      return processingError;
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Обработать входные параметры.
    /// </summary>
    /// <param name="employee">Табельный номер сотрудника.</param>
    /// <param name="login">Логин УЗ.</param>
    /// <param name="type">Тип УЗ.</param>
    /// <param name="isKedo">Согласие на КЭДО.</param>
    /// <param name="provider">Провайдер.</param>
    /// <param name="errorStruct">Ошибки.</param>
    /// <returns>Структура входных данных.</returns>
    private ICreateAccountStruct ProcessIncomingData(string employee, string login, string type, bool isKedo,
                                                     string provider, IProcessingErrorStruct errorStruct)
    {
      if (string.IsNullOrEmpty(employee))
        errorStruct.ErrorText += "Не задан табельный номер сотрудника. ";

      if (string.IsNullOrEmpty(login))
        errorStruct.ErrorText += "Не задан логин. ";

      if (string.IsNullOrEmpty(type))
        errorStruct.ErrorText += "Не задан тип. ";
      else
      {
        if ((type != Constants.Module.CreateAccount.AccountType.Rx) &&
            (type != Constants.Module.CreateAccount.AccountType.Kedo))
          errorStruct.ErrorText += "Передан не правильный тип УЗ. ";
      }

      if (string.IsNullOrEmpty(provider))
        errorStruct.ErrorText += "Не задан провайдер. ";
      else
      {
        if (provider != Constants.Module.CreateAccount.Providers.Ad &&
            provider != Constants.Module.CreateAccount.Providers.Keycloak)
          errorStruct.ErrorText += "Передан не правильный провайдер.";
      }

      if (string.IsNullOrEmpty(errorStruct.ErrorText))
      {
        var structure = CreateAccountStruct.Create();
        structure.Employee = employee;
        structure.Login = login;
        structure.Type = type;
        structure.IsKEDO = isKedo;
        structure.Provider = provider;
        structure.ConsentChangeFlag = null;
        structure.BusinessUnitId = null;

        return structure;
      }

      errorStruct.ErrorText = errorStruct.ErrorText.Trim();
      Logger.ErrorFormat("ProcessIncomingData. {0}", errorStruct.ErrorText);
      return null;
    }

    /// <summary>
    /// Проверка занятости логина.
    /// </summary>
    /// <param name="login">Имя УЗ.</param>
    /// <returns>Текст ошибки, иначе string.Empty.</returns>
    private string CheckLoginAvailability(string login)
    {
      if (!Logins.GetAll(c => c.LoginName.Equals(login)).Any())
        return string.Empty;

      var errorMessage = "Логин используется другим сотрудником";
      Logger.ErrorFormat("CheckLoginAvailability. {0}.", errorMessage);
      return errorMessage;
    }
    
    /// <summary>
    /// Сравнить первые цифры номера телефона с шаблонным значением для личных телефонов для выпуска сертификатов.
    /// </summary>
    /// <param name="phone">Номер телефона.</param>
    /// <returns>Результат проверки. True - в случе совпадения с маской, false - если нет, null - если номер не валиден.</returns>
    /// <remarks>Принимает на вход телефон, состоящий только из цифр без специальных символов.</remarks>
    [Public]
    public virtual bool? MatchesPersonalPhoneMask(string phone)
    {
      // Проверить номер телефона.
      if (!ValidatePhoneNumber(phone))
        return null;
      
      // Извлечь первые цифры номера.
      var firstDigits = phone.Substring(0, PublicConstants.Module.PersonalPhoneFirstDigits.Length);
      return firstDigits == PublicConstants.Module.PersonalPhoneFirstDigits;
    }
    
    /// <summary>
    /// Сформировать фиктивный номер телефона для выпуска сертификата.
    /// </summary>
    /// <param name="phone">Телефон.</param>
    /// <param name="personId">Ид персоны.</param>
    /// <returns>Фиктивный номер телефона.</returns>
    /// <remarks>Принимает на вход телефон, состоящий только из цифр без специальных символов.</remarks>
    [Public]
    public virtual string FormCerificatePhoneNumber(string phone, long personId)
    {
      // Проверить номер телефона.
      if (!ValidatePhoneNumber(phone))
        throw AppliedCodeException.Create("Номер телефона не прошёл валидацию.");
      
      var firstDigitsCount = PublicConstants.Module.PersonalPhoneFirstDigits.Length;
      var potentialNumber = PublicConstants.Module.PersonalPhoneFirstDigits + phone.Substring(firstDigitsCount);
      
      // Проверяем уникальность номера.
      if (IsPhoneUnique(potentialNumber, personId))
        return potentialNumber;
      
      // Перебор 3-й цифры для нахождения уникального номера.
      for (int increment = 0; increment <= 9; increment++)
      {
        potentialNumber = PublicConstants.Module.PersonalPhoneFirstDigits + increment.ToString() + potentialNumber.Substring(firstDigitsCount + 1);
        
        if (IsPhoneUnique(potentialNumber, personId))
          return potentialNumber;
      }
      
      throw AppliedCodeException.Create("Не подобран уникальный фиктивный номер телефона.");
    }
    
    /// <summary>
    /// Проверить номер телефона на уникальность по справочнику Сотрудники.
    /// </summary>
    /// <param name="phone">Телефон.</param>
    /// <param name="personId">ИД персоны.</param>
    /// <returns>Признак того, что номер телефона - уникален.</returns>
    public virtual bool IsPhoneUnique(string phone, long personId)
    {
      var universalPhone = DirRX.EssPlatform.PublicFunctions.Module.PhoneNumberToUniversalFormat(phone);
      return !Employees.GetAll().Where(x =>
                                       x.Person.Id != personId &&
                                       (x.PersonalPhoneDirRX == universalPhone || x.CertificatePhoneNumberDirRX == universalPhone)).Any();
    }
    
    /// <summary>
    /// Проверить номер телефона на валидность.
    /// </summary>
    /// <param name="phone">Номер телефона</param>
    /// <returns>Результат проверки.</returns>
    public virtual bool ValidatePhoneNumber(string phone)
    {
      // Проверить, что длина номера телефона не превышает заданное значение.
      if (phone.Length < 11)
      {
        Logger.ErrorFormat("ValidatePhoneNumber. Не корректная длинна номера телефона. Всего чисел: {0}.", phone.Length);
        return false;
      }
      
      // Проверить, что номер телефона содержит только цифры.
      if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d+$"))
      {
        Logger.Error("ValidatePhoneNumber. Номер телефона содержит не только цифры, но и другие символы.");
        return false;
      }
      
      return true;
    }

    /// <summary>
    /// Проверить наличие действующей задачи на выпуск сертификата или действующего сертификата по персоне.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <returns>True - есть действующая задача или сертификат, иначе - нет.</returns>
    public bool HasActiveCertificateIssueTaskOrCertificateForPerson(long personId)
    {
      var task = GetActiveCertificateIssueTaskForPerson(personId);
      var certificate = DirRX.NNCommons.PublicFunctions.Module.Remote.GetEssCertificates(personId).FirstOrDefault();
      
      if (task != null || certificate != null)
        return true;
      
      return false;
    }
    
    /// <summary>
    /// Получить действующую задачу на выпуск сертификата электронной подписи по персоне.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <returns>Задача.</returns>
    public Sungero.Workflow.ITask GetActiveCertificateIssueTaskForPerson(long personId)
    {
      var task = Sungero.Workflow.Tasks.Null;
      
      var isCustomCertificateIssueProcess = NNCommons.PublicFunctions.Module.Remote.GetBoolValue(NNCommons.PublicConstants.Module.ConstantsSetting.CertificateIssueProcess.Guid).Value;
      
      if (isCustomCertificateIssueProcess)
        task = NNCommons.CertificateIssueTasks.GetAll(c => c.Employee.Person.Id == personId && c.Status == Sungero.Workflow.Task.Status.InProcess).FirstOrDefault();
      else
        task = DirRX.SignPlatform.CertificateIssueTasks.GetAll(c => c.Employee.Person.Id == personId && c.Status == Sungero.Workflow.Task.Status.InProcess).FirstOrDefault();
      
      return task;
    }
    
    /// <summary>
    /// Прекратить активную задачу на выпуск сертификата для персоны.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <returns>Пустая строка если нет ошибок, иначе текст ошибки.</returns>
    [Public]
    public string AbortActiveCertificateIssueTaskForPerson(long personId, string logPrefix)
    {
      var result = string.Empty;
      var task = GetActiveCertificateIssueTaskForPerson(personId);
      
      if (task != null)
      {
        try
        {
          Logger.DebugFormat("{0}. AbortActiveCertificateIssueTaskForPerson. Найдена устаревшая задача для выпуска сертификата ИД={1} для персоны ИД={2}. Задача будет прекращена.", logPrefix, task.Id, personId.ToString());
          task.Abort();
        }
        catch (Exception ex)
        {
          result = ex.Message;
          Logger.ErrorFormat("{0}. AbortActiveCertificateIssueTaskForPerson. Произошла ошибка при прекращении задачи {0}: {1}", logPrefix, task.Id, ex.Message);
        }
      }
      
      return result;
    }

    /// <summary>
    /// Закрыть сертификат для персоны.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <param name="logPrefix">Префикс для логирования.</param>
    /// <returns>Пустая строка если нет ошибок, иначе текст ошибки.</returns>
    [Public]
    public string DisableCertificateForPerson(long personId, string logPrefix)
    {
      var result = string.Empty;
      
      // Найти устаревший сертификат
      var certificate = DirRX.NNCommons.PublicFunctions.Module.Remote.GetEssCertificates(personId).FirstOrDefault();
      
      if (certificate != null)
      {
        try
        {
          certificate.Enabled = false;
          Logger.DebugFormat("{0}. DisableCertificateForPerson. Найден устаревший сертификат ИД={1} для персоны ИД={2}. Сертификат будет закрыт.", logPrefix, personId.ToString(), certificate.Id);
          certificate.Save();
        }
        catch (Exception ex)
        {
          result = ex.Message;
          Logger.ErrorFormat("{0}. DisableCertificateForPerson. Произошла ошибка при закрытии сертификата ИД={1}: {2}", logPrefix, certificate.Id, ex.Message);
        }
      }
      
      return result;
    }

    /// <summary>
    /// Проверить, есть ли у персоны сотрудник с подписанным Согласие на КЭДО в любой НОР, кроме указанной.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <param name="bUnitId">ИД НОР, которую необходимо исключить из поиска.</param>
    /// <returns>True - есть есть сотрудник с согласием в другой НОР, иначе - нет.</returns>
    public bool HasSignedConsentExcludingBusinessUnit(long personId, long bUnitId)
    {
      return DirRX.OverrideSolution.Employees.GetAll().Any(e => e.Person.Id == personId &&
                                                           e.Status == Sungero.Company.Employee.Status.Active &&
                                                           e.BusinessUnit.Id != bUnitId &&
                                                           e.ConsentHRPro == DirRX.OverrideSolution.Employee.ConsentHRPro.Signed);
    }

    #endregion

    #endregion

    #endregion

    #region Интеграционный сценарий «Оформление командировки».

    #region IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП.

    /// <summary>
    /// Создание приказа на командировку.
    /// </summary>
    /// <param name="RegNumber">Регистрационный номер.</param>
    /// <param name="Date">Дата документа.</param>
    /// <param name="Kind">Вид документа.</param>
    /// <param name="Request">Номер заявки.</param>
    /// <param name="Employee">Табельный номер сотрудника.</param>
    /// <param name="Supervisor">Табельный номер работодателя.</param>
    /// <param name="Prepared">Табельный номер сотрудника подготовившего документ.</param>
    /// <param name="Destination">Место назначения.</param>
    /// <param name="Days">Календарные дни.</param>
    /// <param name="DateStart">Дата начала.</param>
    /// <param name="DateEnd">Дата окончания.</param>
    /// <param name="Body">Тело документа.</param>
    /// <param name="SAPID">Идентификатор документа в SAP.</param>
    /// <param name="MainOrderRegNumber">Регистрационный номер ведущего документа.</param>
    /// <param name="MainOrderDate">Дата ведущего документа.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest CreateBusinessTripOrder(string RegNumber, string Date, string Kind, string Request, string Employee,
                                                                    string Supervisor, string Prepared, string Destination, int Days, string DateStart, string DateEnd, string Body, string SAPID, string MainOrderRegNumber, string MainOrderDate)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrEmpty(RegNumber))
        errorMessage += "Не заполнен регистрационный номер. ";
      if (string.IsNullOrEmpty(Date))
        errorMessage += "Не заполнена дата документа. ";
      if (string.IsNullOrEmpty(Kind))
        errorMessage += "Не заполнен вид документа. ";
      if (string.IsNullOrEmpty(Request))
        errorMessage += "Не заполнен номер заявки. ";
      if (string.IsNullOrEmpty(Employee))
        errorMessage += "Не заполнен табельный номер сотрудника. ";
      if (string.IsNullOrEmpty(Supervisor))
        errorMessage += "Не заполнен табельный номер работодателя. ";
      if (string.IsNullOrEmpty(Prepared))
        errorMessage += "Не заполнен табельный номер сотрудника подготовившего документ. ";
      if (string.IsNullOrEmpty(Destination))
        errorMessage += "Не заполнено место назначения. ";
      if (string.IsNullOrEmpty(DateStart))
        errorMessage += "Не заполнена дата начала. ";
      if (string.IsNullOrEmpty(DateEnd))
        errorMessage += "Не заполнена дата окончания. ";
      if (string.IsNullOrEmpty(Body))
        errorMessage += "Не заполнено тело документа. ";
      if (string.IsNullOrEmpty(SAPID))
        errorMessage += "Не заполнен идентификатор документа в SAP. ";

      var date = ParseStringToDate(Date);
      var dateStart = ParseStringToDate(DateStart);
      var dateEnd = ParseStringToDate(DateEnd);
      DateTime? mainDate = null;

      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", Date);
      if (!dateStart.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", DateStart);
      if (!dateEnd.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", DateEnd);
      
      if (!string.IsNullOrEmpty(MainOrderDate))
      {
        mainDate = ParseStringToDate(MainOrderDate);
        if (!mainDate.HasValue)
          errorMessage += string.Format("Не удалось обработать переданную дату: {0}", MainOrderDate);
      }

      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("CreateBusinessTripOrder. {0}", errorMessage);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }

      var structure = DirRX.NNIntegration.Structures.Module.CreateBusinessTripOrder.Create();
      structure.RegNumber = RegNumber;
      structure.Date = date.Value;
      structure.Kind = Kind;
      structure.Request = Request;
      structure.Employee = Employee.Trim();
      structure.Prepared = Prepared.Trim();
      structure.Supervisor = Supervisor.Trim();
      structure.Destination = Destination;
      structure.Days = Days;
      structure.DateStart = dateStart.Value;
      structure.DateEnd = dateEnd.Value;
      structure.Body = Body;
      structure.SAPID = SAPID;
      structure.MainOrderRegNumber = MainOrderRegNumber;
      structure.MainOrderDate = mainDate;
      
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(structure);
      }
      catch (Exception ex)
      {
        errorMessage = string.Format("Ошибка сериализации в Json строку. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("CreateCardRetentionStatementDocument. {0} StackTrace: {1}.", errorMessage, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.CreateBusinessTripOrder, "CreateBusinessTripOrder", responseBody, string.Empty);
      
      return string.IsNullOrEmpty(errorMessage)
        ? CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty)
        : CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
    }

    #endregion

    #endregion

    #region Интеграционный сценарий «Получение результата проштамповки».

    #region IF-S-7578. Получение результата проштамповки.
    
    /// <summary>
    /// Получение результата проштамповки.
    /// </summary>
    /// <param name="request_id">Внешний идентификатор запроса.</param>
    /// <param name="status">Статус выполнения.</param>
    /// <param name="messages">Сообщения об ошибках.</param>
    /// <param name="file">Обработанный документ.</param>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest GetResultStamping(string request_id, string status, List<string> messages, string file)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrEmpty(request_id))
        errorMessage += "Не заполнен внешний идентификатор запроса. ";
      if (string.IsNullOrEmpty(status))
        errorMessage += "Не заполнен статус выполнения. ";
      if (status.ToUpper() == Constants.Module.StatusResultStamping.Success && string.IsNullOrEmpty(file))
        errorMessage += "Не передан обработанный документ. ";
      
      var guidIncomingMessage = Guid.NewGuid().ToString();
      var logPrefix = string.Format("GetResultStamping. request_id= {0}. Guid входящего сообщения= {1}", request_id, guidIncomingMessage);
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }

      var model = ResultStampingDocument.Create();
      model.request_id = request_id;
      model.status = status;
      model.messages = messages;
      model.file = file;
      
      var responseBody = string.Empty;
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = string.Format("Ошибка сериализации в Json строку. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1}. StackTrace: {2}.", logPrefix, errorMessage, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.GetResultStamping,
                                           "GetResultStamping", responseBody, request_id, guidIncomingMessage);
      
      return string.IsNullOrEmpty(errorMessage)
        ? CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty)
        : CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
    }

    /// <summary>
    /// Обработать результат проштамповки.
    /// </summary>
    /// <returns>Результат обработки.</returns>
    public IProcessIncomingMessageResult ProcessingResultStamping(long idIncomingMessage)
    {
      var result = ProcessIncomingMessageResult.Create();
      var logPrefix = string.Format("ProcessingResultStamping. Ид входящего сообщения = {0}", idIncomingMessage);
      result.Retry = false;
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == idIncomingMessage).FirstOrDefault();

      Logger.DebugFormat("{0} Стартовал процесс обработки результата проштамповки.", logPrefix);
      // Поиск входящего сообщения.
      if (incomingMessage == null)
      {
        result.ErrorText = string.Format("Не найдена запись в справочнике 'Входящие информационные сообщения' c ID {0}.", idIncomingMessage);
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      // Получить тело результата проштамповки.
      var jsonMessage = DirRX.NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);

      IResultStampingDocument model = null;
      
      try
      {
        model = IsolatedFunctions.Convert.DeserializeInStruct(jsonMessage, model);
      }
      catch (Exception ex)
      {
        result.ErrorText = "Ошибка десериализации.";
        Logger.ErrorFormat("{0}. {1} Текст ошибки: {2}.", logPrefix, result.ErrorText, ex.Message);
        return result;
      }

      // Определение статуса результата проштамповки.
      if (model.status.ToUpper() == Constants.Module.StatusResultStamping.Fail)
      {
        result.ErrorText = string.Format("Проштамповка на стороне внешней системы произошла с ошибкой: {0}.", string.Join("; ", model.messages));
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      else if (model.status.ToUpper() == Constants.Module.StatusResultStamping.Success)
      {
        // Поиск исходящего сообщения.
        var outgoingMessage = OutgoingMessages.GetAll(c => c.GuidMessage == model.request_id).FirstOrDefault();

        if (outgoingMessage == null)
        {
          result.ErrorText = string.Format("Не найдена запись в справочнике 'Исходящие информационные сообщения' c GuidMessage: {0}.", model.request_id);
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;
        }
        if (outgoingMessage.EntityId == null)
        {
          result.ErrorText = "Не удалось определить документ для занесения тела, не заполнено свойство ИД сущности " +
            "в справочнике 'Исходящие информационные сообщения'.";
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;
        }
        if (outgoingMessage.VersionId == null)
        {
          result.ErrorText = "У исходящего сообщения не заполнено свойство versionId";
          Logger.ErrorFormat("{0} {1}.", logPrefix, result.ErrorText);
          return result;
        }

        // Поиск документа.
        var document = OfficialDocuments.GetAll().FirstOrDefault(d => d.Id == outgoingMessage.EntityId);
        if (document == null)
        {
          result.ErrorText = string.Format("Не найден документ с ИД: {0}.", outgoingMessage.EntityId);
          Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
          return result;
        }
        
        if (!Locks.GetLockInfo(document).IsLockedByMe && !Locks.TryLock(document))
        {
          var locksInfo = Locks.GetLockInfo(document);
          result.ErrorText = string.Format("Не удалось заблокировать документ ID: '{0}'. Заблокирован: {1}. Время установки блокировки: {2}",
                                           document.Id, locksInfo.OwnerName, locksInfo.LockTime);
          result.Retry = true;
          Logger.DebugFormat("{0} {1}.", logPrefix, result.ErrorText);
          return result;
        }

        result.EntityId = document.Id;
        
        try
        {
          // Документ сохраняется при перезаписи publicbody.
          result.ErrorText = DirRX.OverrideSolution.PublicFunctions.OfficialDocument.RewritePublicBodyForVersionDocument(document, outgoingMessage.VersionId.Value, model.file);
          if (!string.IsNullOrEmpty(result.ErrorText))
          {
            result.Retry = true;
            Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
            return result;
          }
        }
        catch (Exception ex)
        {
          result.ErrorText = string.Format("Произошла ошибка обработки документа с ID = '{0}. Текст ошибки: {1}. StackTrace: {2}.'", document.Id, ex.Message, ex.StackTrace);
          result.Retry = true;
          Logger.ErrorFormat("{0} {1}", logPrefix, result.ErrorText);
          return result;
        }
        finally
        {
          if (Locks.GetLockInfo(document).IsLockedByMe)
            Locks.Unlock(document);
        }
      }
      else
      {
        Logger.ErrorFormat("{0}. Статус результата проштамповки: {1}. Не удалось определить статус результата проштамповки.", logPrefix, model.status);
        return result;
      }
      Logger.DebugFormat("{0} Тело проштампованного документа успешно занесено", logPrefix);
      return result;
    }
    #endregion

    #endregion
    
    #region  Интеграционный сценарий «Регистрация работы в выходной и/или праздничный день».
    
    #region IF-S-10413. Передача приказа на выход сотрудников в выходной и/или праздничный день из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Создать приказ на выход сотрудников в выходной и/или праздничный день.
    /// </summary>
    /// <param name="Kind">Вид документа.</param>
    /// <param name="Signed">Подписал.</param>
    /// <param name="Prepared">Подготовил.</param>
    /// <param name="CancelsNumber">Номер отменяемого приказа.</param>
    /// <param name="CancelsDate">Дата отменяемого приказа.</param>
    /// <param name="RegNumber">Регистрационный номер.</param>
    /// <param name="Date">Дата документа.</param>
    /// <param name="DateEvent">Дата мероприятия.</param>
    /// <param name="Body">Тело документа.</param>
    /// <param name="TaskId">ИД задачи.</param>
    /// <param name="SAPID">SAP ID</param>
    /// <returns>Результат обработки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateWeekendWorkOrderCard(string Kind, string Signed, string Prepared, string CancelsNumber, string CancelsDate, string RegNumber, string Date, string DateEvent, string Body, long TaskID, string SAPID)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrWhiteSpace(Kind))
        errorMessage += "Не заполнено поле Вид документа. ";
      if (string.IsNullOrWhiteSpace(Signed))
        errorMessage += "Не заполнено поле Подписал. ";
      if (string.IsNullOrWhiteSpace(Prepared))
        errorMessage += "Не заполнено поле Подготовил. ";
      if (string.IsNullOrWhiteSpace(RegNumber))
        errorMessage += "Не заполнен Регистрационный номер. ";
      if (string.IsNullOrWhiteSpace(Date))
        errorMessage += "Не заполнена Дата документа. ";
      if (string.IsNullOrWhiteSpace(DateEvent))
        errorMessage += "Не заполнена Дата мероприятия. ";
      if (string.IsNullOrWhiteSpace(Body))
        errorMessage += "Не заполнено содержимое документа в формате base64. ";
      
      if (Kind != Constants.Module.WeekendWorkOrderKindCodes.WeekendWork && Kind != Constants.Module.WeekendWorkOrderKindCodes.OvertimeWork &&
          Kind != Constants.Module.WeekendWorkOrderKindCodes.Cancelation)
        errorMessage += "Не удалось определить вид документа по переданному коду. ";
      
      // Для приказа на отмену должны быть заполнены Номер отменяемого приказа и Дата отменяемого приказа.
      DateTime? cancelsDate = null;
      if (Kind == Constants.Module.WeekendWorkOrderKindCodes.Cancelation)
      {
        if (string.IsNullOrWhiteSpace(CancelsNumber))
          errorMessage += "Не заполнен Номер отменяемого приказа. ";
        if (!string.IsNullOrWhiteSpace(CancelsDate))
        {
          cancelsDate = ParseStringToDate(CancelsDate);
          if (!cancelsDate.HasValue)
            errorMessage += string.Format("Не удалось обработать переданную дату отменяемого приказа: {0}", Date);
        }
        else
          errorMessage += "Не заполнена Дата отменяемого приказа. ";
      }
      
      var date = ParseStringToDate(Date);
      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", Date);
      
      var dateEvent = ParseStringToDate(DateEvent);
      if (!dateEvent.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату мероприятия: {0}", DateEvent);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        Logger.ErrorFormat("CreateWeekendWorkOrderCard. {0}. SAPID: {1}.", errorMessage, SAPID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      var model = Structures.Module.CreateWeekendWorkOrderStructure.Create(Kind, Signed, Prepared, CancelsNumber, cancelsDate, RegNumber,
                                                                           date.Value, dateEvent.Value, Body, TaskID, SAPID);
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateWeekendWorkOrderCard. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.CreateWeekendWorkOrderDocument, "CreateWeekendWorkOrderCard", responseBody, string.Empty);
      
      if (!string.IsNullOrWhiteSpace(errorMessage))
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, errorMessage);
    }
    
    #endregion
    
    #region IF-S-29002. Передача приказа на дополнительный день отдыха за РВД из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Создать приказ на дополнительный день отдыха за РВД.
    /// </summary>
    /// <param name="Employee">Сотрудник.</param>
    /// <param name="Kind">Вид документа.</param>
    /// <param name="Signed">Подписал.</param>
    /// <param name="Prepared">Подготовил.</param>
    /// <param name="CancelsNumber">Номер отменяемого приказа.</param>
    /// <param name="CancelsDate">Дата отменяемого приказа.</param>
    /// <param name="RegNumber">Регистрационный номер.</param>
    /// <param name="Date">Дата документа.</param>
    /// <param name="DateEvent">Дата мероприятия.</param>
    /// <param name="Body">Тело документа.</param>
    /// <param name="SAPID">SAP ID</param>
    /// <param name="TaskId">ИД задачи.</param>
    /// <returns>Результат обработки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateWeekendWorkRestOrderCard( string Employee, string Kind, string Signed, string Prepared, string CancelsNumber, string CancelsDate,
                                                                                   string RegNumber, string Date, string DateEvent, string Body, string SAPID, long TaskID)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrWhiteSpace(Kind))
        errorMessage += "Не заполнено поле Вид документа. ";
      if (string.IsNullOrWhiteSpace(Signed))
        errorMessage += "Не заполнено поле Подписал. ";
      if (string.IsNullOrWhiteSpace(Prepared))
        errorMessage += "Не заполнено поле Подготовил. ";
      if (string.IsNullOrWhiteSpace(RegNumber))
        errorMessage += "Не заполнен Регистрационный номер. ";
      if (string.IsNullOrWhiteSpace(Date))
        errorMessage += "Не заполнена Дата документа. ";
      if (string.IsNullOrWhiteSpace(DateEvent))
        errorMessage += "Не заполнена Дата мероприятия. ";
      if (string.IsNullOrWhiteSpace(Body))
        errorMessage += "Не заполнено содержимое документа в формате base64. ";
      
      if (!IsWeekendWorkRestOrderKindValid(Kind))
        errorMessage += "Не удалось определить вид документа по переданному коду. ";
      
      // Для приказа на отмену должны быть заполнены Номер отменяемого приказа и Дата отменяемого приказа.
      DateTime? cancelsDate = null;
      if (Kind == Constants.Module.WeekendWorkRestOrderKindCodes.CancelationRestOrder)
      {
        if (string.IsNullOrWhiteSpace(CancelsNumber))
          errorMessage += "Не заполнен Номер отменяемого приказа. ";
        if (!string.IsNullOrWhiteSpace(CancelsDate))
        {
          cancelsDate = ParseStringToDate(CancelsDate);
          if (!cancelsDate.HasValue)
            errorMessage += string.Format("Не удалось обработать переданную дату отменяемого приказа: {0}", Date);
        }
        else
          errorMessage += "Не заполнена Дата отменяемого приказа. ";
      }
      
      var date = ParseStringToDate(Date);
      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", Date);
      
      var dateEvent = ParseStringToDate(DateEvent);
      if (!dateEvent.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату мероприятия: {0}", DateEvent);
      
      if (string.IsNullOrWhiteSpace(Employee))
        errorMessage += "Не заполнен Сотрудник. ";
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        Logger.ErrorFormat("CreateWeekendWorkRestOrderCard. {0}. SAPID: {1}.", errorMessage, SAPID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      var model = Structures.Module.CreateWeekendWorkRestOrderStructure.Create(Kind, Signed, Prepared, CancelsNumber, cancelsDate, RegNumber,
                                                                               date.Value, dateEvent.Value, Body, TaskID, SAPID, Employee);
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateWeekendWorkRestOrderCard. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.CreateWeekendWorkRestOrderDocument, "CreateWeekendWorkRestOrderCard", responseBody, string.Empty);
      
      if (!string.IsNullOrWhiteSpace(errorMessage))
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty);
    }
    
    /// <summary>
    /// Валидация кода вида приказа на дополнительный день отдыха за РВД.
    /// </summary>
    /// <param name="kind">Код вида приказа.</param>
    /// <returns>True - данный вид входит в список возможных значений. False - неизвестный вид приказа.</returns>
    public static bool IsWeekendWorkRestOrderKindValid(string kind)
    {
      return kind == Constants.Module.WeekendWorkRestOrderKindCodes.GrantRestOrder || kind == Constants.Module.WeekendWorkRestOrderKindCodes.CancelationRestOrder;
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий "Выплаты по ЛНА".
    
    #region IF-S-10399. Передача приказа на выплату по ЛНА из АСУП в КЭДО.

    /// <summary>
    /// Создать приказ на на выплату по ЛНА.
    /// </summary>
    /// <param name="Employee">Сотрудник.</param>
    /// <param name="Signed">Подписал.</param>
    /// <param name="Prepared">Подготовил.</param>
    /// <param name="RegNumber">Регистрационный номер.</param>
    /// <param name="Date">Дата документа.</param>
    /// <param name="Body">Тело документа.</param>
    /// <param name="TaskID">ИД задачи.</param>
    /// <param name="SAPID">SAP ID</param>
    /// <returns>Результат обработки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public virtual IResponseToExternalSystemRequest CreateLrdPaymentOrderCard(string Employee, string Signed, string Prepared, string RegNumber, string Date, string Body, long TaskID, string SAPID)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrWhiteSpace(Employee))
        errorMessage += "Не заполнен Сотрудник. ";
      if (string.IsNullOrWhiteSpace(Signed))
        errorMessage += "Не заполнено поле Подписал. ";
      if (string.IsNullOrWhiteSpace(Prepared))
        errorMessage += "Не заполнено поле Подготовил. ";
      if (string.IsNullOrWhiteSpace(RegNumber))
        errorMessage += "Не заполнен Регистрационный номер. ";
      if (string.IsNullOrWhiteSpace(Date))
        errorMessage += "Не заполнена Дата документа. ";
      if (string.IsNullOrWhiteSpace(Body))
        errorMessage += "Не заполнено содержимое документа в формате base64. ";
      if (string.IsNullOrWhiteSpace(SAPID))
        errorMessage += "Не заполнен SAPID. ";
      
      Employee = Employee.Trim().TrimStart('0');
      Signed = Signed.Trim().TrimStart('0');
      Prepared = Prepared.Trim().TrimStart('0');
      
      var date = ParseStringToDate(Date);
      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату документа: {0}", Date);
      
      if (!string.IsNullOrWhiteSpace(errorMessage.Trim()))
      {
        Logger.ErrorFormat("CreateLrdPaymentOrderCard. {0}. SAPID: {1}.", errorMessage, SAPID);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      var model = Structures.Module.LrdPaymentOrderRequest.Create(Employee, Signed, Prepared, RegNumber, date.Value, Body, TaskID, SAPID);
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("CreateLrdPaymentOrderCard. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.LrdPaymentOrder, "CreateLrdPaymentOrderCard", responseBody, string.Empty);
      
      if (!string.IsNullOrWhiteSpace(errorMessage))
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      
      return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, errorMessage);
    }
    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий «Массовое формирование и подписание ДС о пересмотре ЗП».
    
    #region IF-S-28562. Создание карточки дополнительного соглашения к трудовому договору в КЭДО из АСУП (АСУП → КЭДО).
    
    /// <summary>
    /// Создание карточки дополнительного соглашения к трудовому договору.
    /// </summary>
    /// <param name="RegNumber">Регистрационный номер.</param>
    /// <param name="Date">Дата документа.</param>
    /// <param name="Kind">Вид документа.</param>
    /// <param name="Employee">Табельный номер сотрудника.</param>
    /// <param name="Supervisor">Табельный номер работодателя.</param>
    /// <param name="Prepared">Табельный номер сотрудника подготовившего документ.</param>
    /// <param name="Body">Тело документа.</param>
    /// <param name="SAPID">Идентификатор документа в SAP.</param>
    /// <returns>Результат.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest CreateEmploymentContractAdditionalAgreement(string RegNumber, string Date, string Kind, string Employee,
                                                                                        string Supervisor, string Prepared, string Body, string SAPID)
    {
      var errorMessage = string.Empty;
      if (string.IsNullOrEmpty(Date))
        errorMessage += "Не заполнена дата документа. ";
      if (string.IsNullOrEmpty(Kind))
        errorMessage += "Не заполнен вид документа. ";
      if (string.IsNullOrEmpty(Employee))
        errorMessage += "Не заполнен табельный номер сотрудника. ";
      if (string.IsNullOrEmpty(Supervisor))
        errorMessage += "Не заполнен табельный номер работодателя. ";
      if (string.IsNullOrEmpty(Prepared))
        errorMessage += "Не заполнен табельный номер сотрудника подготовившего документ. ";
      if (string.IsNullOrEmpty(Body))
        errorMessage += "Не заполнено тело документа. ";
      if (string.IsNullOrEmpty(SAPID))
        errorMessage += "Не заполнен идентификатор документа в SAP. ";

      var date = ParseStringToDate(Date);
      DateTime? mainDate = null;

      if (!date.HasValue)
        errorMessage += string.Format("Не удалось обработать переданную дату: {0}", Date);

      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("CreateEmploymentContractAdditionalAgreement. {0}", errorMessage);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }

      var structure = DirRX.NNIntegration.Structures.Module.EmploymentContractAdditionalAgreement.Create();
      structure.RegNumber = RegNumber;
      structure.Date = date.Value;
      structure.Kind = Kind;
      structure.Employee = Employee.Trim();
      structure.Prepared = Prepared.Trim();
      structure.Supervisor = Supervisor.Trim();
      structure.Body = Body;
      structure.SAPID = SAPID;
      
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(structure);
      }
      catch (Exception ex)
      {
        errorMessage = string.Format("Ошибка сериализации в Json строку. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("CreateCardRetentionStatementDocument. {0} StackTrace: {1}.", errorMessage, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.EmploymentContractAdditionalAgreement, "CreateCardRetentionStatementDocument", responseBody, string.Empty);
      
      return string.IsNullOrEmpty(errorMessage)
        ? CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty)
        : CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
    }

    
    #endregion
    
    #endregion
    
    #region Интеграционный сценарий "Синхронизация календарей рабочего времени".
    
    #region IF-S-28097. Синхронизация календарей рабочего времени из АСУП в КЭДО (АСУП -> КЭДО).
    
    /// <summary>
    /// Синхронизация календаря рабочего времени.
    /// </summary>
    /// <param name="Employee">Сотрудник.</param>
    /// <param name="Year">Год.</param>
    /// <param name="DayDisc">Описание дней.</param>
    /// <returns>Результат обработки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest UpdateOrCreateWorkingTimeCalendar(string Employee, string Year, List<Structures.Module.IIncomingWorkingDayDescription> DayDisc)
    {
      var errorMessage = string.Empty;

      if (string.IsNullOrEmpty(Employee))
        errorMessage += "Не заполнен Сотрудник. ";
      
      var yearNumber = 0;
      if (string.IsNullOrEmpty(Year))
        errorMessage += "Не заполнен Год. ";
      else
      {
        if (int.TryParse(Year, out yearNumber))
        {
          if (yearNumber < Calendar.Today.Year)
            errorMessage += "Переданный год меньше текущего. ";
        }
        else
          errorMessage += "Некорректный формат года. ";
      }

      if (!DayDisc.Any())
        errorMessage += "Список передаваемых дней пуст. ";
      
      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("UpdateOrCreateWorkingTimeCalendar. {0}", errorMessage);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      Employee = Employee.Trim().TrimStart('0');
      
      var model = Structures.Module.IncomingWorkingTimeCalendar.Create(Employee, yearNumber, DayDisc);
      var responseBody = string.Empty;
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(model);
      }
      catch (Exception ex)
      {
        errorMessage = "Ошибка сериализации в Json строку.";
        Logger.ErrorFormat("UpdateOrCreateWorkingTimeCalendar. {0} Текст ошибки: {1}. StackTrace: {2}.", errorMessage, ex.Message, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = NNIntegration.PublicFunctions.Module.CreateIncomingMessage(NNIntegration.Constants.Module.ExternalSystemsFlowGuids.UpdateOrCreateWorkingTimeCalendar,
                                                                                "UpdateOrCreateWorkingTimeCalendar", responseBody, string.Empty, string.Empty);
      return string.IsNullOrEmpty(errorMessage)
        ? CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty)
        : CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
    }
    
    #endregion
    
    #endregion
    
    #region ПМД (Суперника)
    
    #region IF-S-25401. Передача результата нажатия на действие в центре уведомлений ПМД (ПМД → КЭДО).
    
    /// <summary>
    /// Передать результат нажатия на действие в центре уведомлений ПМД.
    /// </summary>
    /// <param name="App_id">App_id приложение, отправившего уведомления.</param>
    /// <param name="notif_send_status">Объект с результатами обработки каждого уведомления.</param>
    /// <returns>Результат обработки.</returns>
    [Public(WebApiRequestType = RequestType.Post)]
    public IResponseToExternalSystemRequest GetSupernikaActionResult(string app_id, DirRX.NNIntegration.Structures.Module.ISupernikaNotificationStatus notif_send_status)
    {
      var homescreenExpressAppId = NNCommons.PublicFunctions.Module.GetStringValue(NNIntegration.PublicConstants.Module.ConstantsSetting.HomescreenExpress.Guid);
      
      var errorMessage = string.Empty;
      if (string.IsNullOrEmpty(app_id) && app_id != homescreenExpressAppId)
        errorMessage += string.Format("Не совпадает app id со значением из константы {0}. Полученное значение: {1}. Значение из константы: {2}. ",
                                      NNIntegration.PublicConstants.Module.ConstantsSetting.HomescreenExpress.Name, app_id, homescreenExpressAppId);

      if (!string.IsNullOrEmpty(errorMessage))
      {
        errorMessage = errorMessage.Trim();
        Logger.ErrorFormat("SendSupernikaActionResult. {0}", errorMessage);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      var responseBody = string.Empty;
      
      try
      {
        responseBody = IsolatedFunctions.Convert.SerializeInJson(notif_send_status);
      }
      catch (Exception ex)
      {
        errorMessage = string.Format("Ошибка сериализации в Json строку. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("SendSupernikaActionResult. {0} StackTrace: {1}.", errorMessage, ex.StackTrace);
        return CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
      }
      
      errorMessage = CreateIncomingMessage(Constants.Module.ExternalSystemsFlowGuids.GetSupernikaActionResult, "SendSupernikaActionResult", responseBody, string.Empty);
      
      return string.IsNullOrEmpty(errorMessage)
        ? CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Successfully, string.Empty)
        : CreateResponseToExternalSystem(Constants.Module.Request.ResultCode.Error, errorMessage);
    }
    
    /// <summary>
    /// Обработка результата нажатия на действие в центре уведомлений ПМД (ПМД → КЭДО).
    /// </summary>
    /// <param name="incomingMessageId">ИД записи входящих сообщений.</param>
    /// <returns>Структура ответа.</returns>
    [Public]
    public IProcessIncomingMessageResult ProcessManualHoldingEvent(long incomingMessageId)
    {
      var processingErrors = string.Empty;
      var baseSeparator = "\n";
      var logPrefix = string.Format("ProcessManualHoldingEvent. ИД записи справочника 'Входящие информационные сообщения': {0}", incomingMessageId);
      var result = ProcessIncomingMessageResult.Create();
      var incomingMessage = IncomingMessages.GetAll(c => c.Id == incomingMessageId).FirstOrDefault();
      
      if (incomingMessage == null)
      {
        result.ErrorText = "Не найдена запись в справочнике 'Входящие информационные сообщения'.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var message = NNIntegration.PublicFunctions.IncomingMessage.Remote.GetIncomingMessageText(incomingMessage);
      ISupernikaNotificationStatus structure = null;
      
      try
      {
        structure = NNIntegration.IsolatedFunctions.Convert.DeserializeInStruct(message, structure);
      }
      catch (Exception ex)
      {
        result.ErrorText = string.Format("Ошибка десериализации. Текст ошибки: {0}", ex.Message);
        Logger.ErrorFormat("{0}. {1} StackTrace: {2}.", logPrefix, result.ErrorText,  ex.StackTrace);
        return result;
      }
      
      if (!structure.notifications.Any())
      {
        result.ErrorText = "Не заполнен массив результатов отправки push-уведолмений (атрибут notifications в тексте вх. сообщения).";
        Logger.ErrorFormat("{0}. {1}", logPrefix, result.ErrorText);
        return result;
      }
      
      var completedButtonStatus = NNIntegration.PublicConstants.Module.MessageBrokerNotificationParameters.CompletedButtonStatusCode;
      
      #region Сохранение статусов по push-уведомлениям в БД
      
      foreach (var notification in structure.notifications)
      {
        var selectedButtons = notification.buttons.Where(x => x.button_status == completedButtonStatus);
        Logger.DebugFormat("{0}. MBMessageId: {1}. Запуск АО для сохранение статуса сообщения полученного из Суперники.", logPrefix, notification.unique_id);
        
        // АО для сохранение статуса сообщения полученного из Суперники.
        var asyncHandler = NNIntegration.AsyncHandlers.SaveStatusSupernikaPushNotification.Create();
        asyncHandler.RXIncomingMessageId = incomingMessageId;
        asyncHandler.MBMessageId = notification.unique_id;
        asyncHandler.DateTime = incomingMessage.ReceivedDate.Value;
        asyncHandler.Status = notification.status;
        asyncHandler.SelectedButton = string.Join("; ", selectedButtons.Select(x => x.button_type));
        asyncHandler.Comment = string.Join("; ", selectedButtons.Select(x => x.comment));
        asyncHandler.ErrorMessage = notification.message;
        
        asyncHandler.ExecuteAsync();
      }
      
      #endregion
      
      // Если в сообщении нет статусов у действий, то такое сообщение считаем статусом самого сообщения.
      if(!structure.notifications.Where(s => s.buttons.Any(w => w.button_status != null && w.button_status != "")).Any())
      {
        Logger.DebugFormat("{0}. Не найдены статусы действий.", logPrefix);
        result.Retry = false;
        return result;
      }
      
      #region Связывание с отправленным push-уведомлением в Супренику
      
      foreach (var notification in structure.notifications.Where(s => s.buttons.Any(w => w.button_status != null && w.button_status != "")))
      {
        var push = NNIntegration.PublicFunctions.SupernikaPushNotification.GetSendedMessage(notification.unique_id);
        
        // Если уведомление не найдено.
        if (push == null)
        {
          var error = string.Format("Не найдено отправленное push-уведомление в справочнике по guid: {0}.", notification.unique_id);
          Logger.ErrorFormat("{0}. {1}.", logPrefix, error);
          var separator = string.IsNullOrEmpty(processingErrors) ? "" : baseSeparator;
          processingErrors = string.Join(separator, processingErrors, error);
          continue;
        }
        // Если по сообщению уже получен ответ, то пропускаем обработку.
        else if (push.ProcessingStatus != NNIntegration.SupernikaPushNotification.ProcessingStatus.Sended && push.ProcessingStatus != NNIntegration.SupernikaPushNotification.ProcessingStatus.Error)
        {
          Logger.DebugFormat("{0}. По push-уведомлению c guid {1} ответ уже получен. Текущий статус push-уведомления: {2}.", logPrefix, push.NotificationGuid,
                             SupernikaPushNotifications.Info.Properties.ProcessingStatus.GetLocalizedValue(push.ProcessingStatus));
          continue;
        }
        
        // Установить блокировку на push - уведомление.
        var isLocked = Locks.TryLock(push);
        if (!Locks.GetLockInfo(push).IsLockedByMe && !isLocked)
        {
          var lockInfo = Locks.GetLockInfo(push);
          Logger.DebugFormat("{0} Не удалось установить блокировку. Push-уведомление заблокировано пользователем: {1}. Время установки блокировки: {2}. Будет запущена повторная обработка входящего сообщения.",
                             logPrefix, lockInfo.OwnerName, lockInfo.LockTime);
          result.Retry = true;
          continue;
        }
        
        // Обновить и сохранить push-уведомление.
        try
        {
          push.IncomingMessageId = incomingMessageId;
          push.ReceivedDate = incomingMessage.ReceivedDate;
          
          var completedButtons = notification.buttons.Where(x => x.button_status == completedButtonStatus);
          if (!completedButtons.Any())
          {
            var error = string.Format("Не удалось определить нажатое действие в push-уведомлении с guid: {0}. Не найдено ни одного выполненного действия со статус-кодом: {1}.", notification.unique_id, completedButtonStatus);
            Logger.ErrorFormat("{0}. {1}", logPrefix, error);
            var separator = string.IsNullOrEmpty(processingErrors) ? "" : baseSeparator;
            processingErrors = string.Join(separator, processingErrors, error);
            push.ErrorText = error;
            push.ProcessingDate = Calendar.Now;
            push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Error;
            continue;
          }
          else if (completedButtons.Count() > 1)
          {
            var error = string.Format("Не удалось определить нажатое действие в push-уведомлении с guid: {0}. Найдено несколько выполненных действий со статус-кодом: {1}.", notification.unique_id, completedButtonStatus);
            Logger.ErrorFormat("{0}. {1}", logPrefix, error);
            var separator = string.IsNullOrEmpty(processingErrors) ? "" : baseSeparator;
            processingErrors = string.Join(separator, processingErrors, error);
            push.ErrorText = error;
            push.ProcessingDate = Calendar.Now;
            push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Error;
            continue;
          }
          
          var button = completedButtons.FirstOrDefault();
          push.Comment = button.comment;
          
          if (!GetSupernikaActionTypes().Contains(button.button_type))
          {
            var error = string.Format("Не удалось определить нажатое действие в push-уведомлении с guid: {0}. Имя типа нажатого действия не найдено в КЭДО. Полученное имя из ПМД: {1}.", notification.unique_id, button.button_type);
            Logger.ErrorFormat("{0}. {1}", logPrefix, error);
            var separator = string.IsNullOrEmpty(processingErrors) ? "" : baseSeparator;
            processingErrors = string.Join(separator, processingErrors, error);
            push.ErrorText = error;
            push.ProcessingDate = Calendar.Now;
            push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Error;
            continue;
          }
          
          push.ActionName = button.button_type;
          push.ProcessingStatus = NNIntegration.SupernikaPushNotification.ProcessingStatus.Received;
        }
        catch (Exception ex)
        {
          var error = string.Format("Ошибка при обработке push-уведомления с guid: {0}. Подробные сведения об ошибке зафиксированы в лог сообщениях.", push.NotificationGuid);
          Logger.ErrorFormat("{0}. Ошибка при обработке push-уведомления с guid: {1}. Будет запущена повторная обработка. Ошибка: {2}. StackTrace: {3}.", logPrefix, push.NotificationGuid, ex.Message, ex.StackTrace);
          var separator = string.IsNullOrEmpty(processingErrors) ? "" : baseSeparator;
          processingErrors = string.Join(separator, processingErrors, error);
          push.ErrorText = error;
          push.ProcessingDate = Calendar.Now;
          result.Retry = true;
        }
        finally
        {
          // Сохранение push-уведомления.
          var saveError = PublicFunctions.SupernikaPushNotification.SaveMessage(push, logPrefix);
          var separator = string.IsNullOrEmpty(processingErrors) ? "" : baseSeparator;
          if (!string.IsNullOrEmpty(saveError))
            processingErrors = string.Join(separator, processingErrors, saveError);
          
          Locks.Unlock(push);
        }
      }
      
      #endregion
      
      result.ErrorText = processingErrors;
      if (!string.IsNullOrEmpty(result.ErrorText))
        Logger.DebugFormat("{0}. Возникли ошибки при обработке входящего сообщения.", logPrefix);
      else
        Logger.DebugFormat("{0}. Успешно обработан.", logPrefix);
      
      return result;
    }
    
    /// <summary>
    /// Получить список типов действий в Супернике из констант.
    /// </summary>
    /// <returns>Список гуидов видов документов.</returns>
    [Public]
    public List<string> GetSupernikaActionTypes()
    {
      var actionTypesList = new List<string>();
      
      // Типы действий в Супернике.
      actionTypesList = typeof(Constants.Module.SupernikaActions.ActionTypes)
        .GetFields()
        .Where(field => field.FieldType == typeof(string))
        .Select(field => (string)field.GetValue(null))
        .ToList();
      
      return actionTypesList;
    }
    
    #endregion
    
    #region Вспомогательные функции для отправки уведомления в Супернику.
    
    /// <summary>
    /// Получить доступные действия Суперники в задании.
    /// </summary>
    /// <param name="assignment">Задание/уведомление.</param>
    /// <returns>Список действий.</returns>
    /// <remarks>Передаётся как IEntity, т.к. на прикладном уровне нет общего наследника для заданий и уведомлений.</remarks>
    [Public]
    public virtual List<ISupernikaPushNotificationAction> GetActions(Sungero.Domain.Shared.IEntity assignment)
    {
      var actions = new List<ISupernikaPushNotificationAction>();
      if (assignment == null)
        return actions;
      
      // Простое задание HR Pro.
      if (DirRX.OverrideSolution.HRAssignments.Is(assignment))
        actions = DirRX.OverrideSolution.HRAssignments.As(assignment).SupernikaActionsDirRX.Select(x => x.Action).ToList();
      // Согласование документов.
      else if (DirRX.OverrideSolution.DocumentApprovalAssignments.Is(assignment))
        actions = DirRX.OverrideSolution.DocumentApprovalAssignments.As(assignment).SupernikaActionsDirRX.Select(x => x.Action).ToList();
      // Уведомление HR Pro.
      else if (DirRX.OverrideSolution.HRNotices.Is(assignment))
        actions = DirRX.OverrideSolution.HRNotices.As(assignment).SupernikaActionsDirRX.Select(x => x.Action).ToList();
      // Иной тип сущности.
      else
        Logger.DebugFormat("GetActions. Передан некорректный тип сущности: {0}.", assignment.GetType().Name);
      
      return actions;
    }
    
    /// <summary>
    /// Получить имя блока в RX по заданию/уведомлению.
    /// </summary>
    /// <returns>Задание/уведомление.</returns>
    [Public]
    public virtual string GetBlockNameByAssignemnt(Sungero.Domain.Shared.IEntity assignment)
    {
      // Простое задание HR Pro.
      if (DirRX.OverrideSolution.HRAssignments.Is(assignment))
        return Constants.Module.SupernikaActions.BlockNamesRx.HRAssignment;
      // Согласование документов.
      else if (DirRX.OverrideSolution.DocumentApprovalAssignments.Is(assignment))
        return Constants.Module.SupernikaActions.BlockNamesRx.DocumentApprovalAssignment;
      // Уведомление HR Pro.
      else if (DirRX.OverrideSolution.HRNotices.Is(assignment))
        return Constants.Module.SupernikaActions.BlockNamesRx.HRNotice;
      // Иной тип сущности.
      else
        Logger.DebugFormat("GetBlockNameByAssignemnt. Не удалось определить наименование блока по заданию/уведомлению. Тип задания/уведомления: {0}.", assignment.GetType().Name);
      
      return string.Empty;
    }
    
    /// <summary>
    /// Удалить push-уведомления.
    /// </summary>
    /// <param name="logPrefix">Префикс лога.</param>
    /// <param name="pushes">Push-уведомления.</param>
    /// <param name="count">Максимальное число записей для удаления.</param>
    public void SupernikaNotificationsCleaning(string logPrefix, IQueryable<ISupernikaPushNotification> pushes, int count)
    {
      var prefix = string.Format("{0} - SupernikaNotificationsCleaning", logPrefix);
      Logger.DebugFormat("{0}. Размер порции для удаления: {1}.", prefix, count);
      foreach (var push in pushes.Take(count))
      {
        Logger.DebugFormat("{0}. ИД: {1}. Начало удаления записи.", prefix, push.Id);
        var isLocked = Locks.TryLock(push);
        if (isLocked)
        {
          var result = Transactions.Execute(() =>
                                            {
                                              SupernikaPushNotifications.Delete(push);
                                            });
          if (result)
            Logger.DebugFormat("{0}. ИД: {1}. Запись удалена.", prefix, push.Id);
          else
            Logger.ErrorFormat("{0}. ИД: {1}. Не удалось удалить запись.", prefix, push.Id);
          
          Locks.Unlock(push);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(push);
          Logger.DebugFormat("{0}. ИД: {1}. Не удалось установить блокировку на запись. Запись заблокирована пользователем: {2}. Время установки блокировки: {3}.",
                             prefix, push.Id, lockInfo.OwnerName, lockInfo.LockTime);
        }
      }
    }
    
    /// <summary>
    /// Проверить задачу на выполнение или уведомление на прочитанность.
    /// </summary>
    /// <param name="id">Id задачи.</param>
    /// <param name="isNotification">Признак уведомления.</param>
    /// <returns>True - задание выполенно, уведомление прочитано.</returns>
    public bool VerifyTaskCompletion(long id, bool isNotification)
    {
      var result = false;
      if(isNotification)
        result = Sungero.Workflow.Notices.GetAll(x => x.Id == id).Select(x => x.IsRead).FirstOrDefault() == true;
      else
        result = Sungero.Workflow.Assignments.GetAll(x => x.Id == id).Select(x => x.Status).FirstOrDefault() != Sungero.Workflow.Assignment.Status.InProcess;
      
      return result;
    }
    
    #endregion
    
    #endregion
    
    #region Взаимодействие с ЛК.
    
    /// <summary>
    /// Получить профиль пользователя ЛК из БД Ess.
    /// </summary>
    /// <param name="personId">Ид персоны.</param>
    /// <returns>Данные фильтра из профиля пользователя ЛК.</returns>
    [Public]
    public NNIntegration.Structures.Module.IEssCurrentPeriod GetPersonEssCurrentPeriod(long personId)
    {
      // Параметры подключения к базе данных.
      var server = NNCommons.PublicFunctions.Module.GetStringValue(NNCommons.PublicConstants.Module.ConstantsSetting.EssDatabaseAddress.Guid);
      var port = NNCommons.PublicFunctions.Module.GetStringValue(NNCommons.PublicConstants.Module.ConstantsSetting.EssDatabasePort.Guid);
      var database = NNCommons.PublicFunctions.Module.GetStringValue(NNCommons.PublicConstants.Module.ConstantsSetting.EssDatabaseName.Guid);
      var user = NNCommons.PublicFunctions.Module.GetStringValue(NNCommons.PublicConstants.Module.ConstantsSetting.EssDatabaseUserName.Guid);
      var password = NNCommons.PublicFunctions.Module.GetStringValue(NNCommons.PublicConstants.Module.ConstantsSetting.EssDatabaseUserPassword.Guid);
      if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
      {
        Logger.ErrorFormat("GetPerosnEssCurrentPeriod. PersonId: {0}. Не заполнены константы для строки подключения к БД Ess.", personId);
        return null;
      }
      
      var connectionString = string.Format("server={0};port={1};database={2};user id={3};password={4}", server, port, database, user, password);
      var personUid = DirRX.OverrideSolution.Module.EssPlatform.PublicFunctions.Module.Remote.GetUidPerson(personId);
      var essCurrentPeriod = NNIntegration.IsolatedFunctions.EssDatabase.GetEssUserProfileCurrentPeriod(Queries.Module.SelectEssUserProfile, connectionString, personUid);
      if (essCurrentPeriod == null)
      {
        Logger.DebugFormat("GetPerosnEssCurrentPeriod. PersonId: {0}. Не удалось получить данные фильтра из профиля пользователя в Ess. Подробности в лог файле изолированной области модуля. Период фильтрации будет составлять 30 дней.", personId);
        essCurrentPeriod = DirRX.NNIntegration.Structures.Module.EssCurrentPeriod.Create();
        essCurrentPeriod.Length = 0;
      }
      
      return essCurrentPeriod;
    }
    
    #endregion
  }
}