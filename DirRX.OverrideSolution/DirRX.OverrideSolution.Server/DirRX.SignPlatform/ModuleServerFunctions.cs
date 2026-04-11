using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DirRX.OverrideSolution.Module.SignPlatform.Server
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Проверить есть ли у сотрудника действующий сертификат УНЭП.
    /// </summary>
    /// <param name="IEmployee">Сотрудник.</param>
    /// <returns>True если есть сертификат, иначе false.</returns>
    [Public, Remote]
    public bool EmployeeHasCloudCertificate(EssPlatformSolution.IEmployee employee)
    {
      return Sungero.CoreEntities.Certificates.GetAll(c => Equals(c.Owner, employee) && c.Enabled == true &&
                                                      c.PluginId == DirRX.SignPlatform.PublicConstants.Module.PluginIdOfCloudCertificates).Any();
    }

    public override string GetErrorMessage(string errorMain, DirRX.SignPlatform.Structures.Module.HttpRequestDetailedErrorResult errorDetail)
    {
      return base.GetErrorMessage(errorMain, errorDetail);
    }

    public virtual string GetErrorMessage(string errorMain, DirRX.OverrideSolution.Module.SignPlatform.Structures.Module.HttpRequestErrorResult errorDetail)
    {
      var errorMessage = errorDetail.Message;
      
      if (!string.IsNullOrEmpty(errorMain))
        errorMessage = string.Format("{0}{1}{2}", errorMain, Environment.NewLine, errorDetail.Message);
      
      if (errorDetail.Details != null)
        foreach (var detail in errorDetail.Details)
      {
        errorMessage += string.Format("{0}{1}Field: {2}, message: {3}\r\n", errorMessage, Environment.NewLine, detail.Field, detail.Message);
      }
      
      return errorMessage;
    }

    /// <summary>
    /// Проверить подписано ли заявление в ЕСИА
    /// </summary>
    /// <param name="requestEntry">Запись из справочника заявок на выпуск УНЭП.</param>
    /// <returns>Признак, изменен ли статус в записи.</returns>
    [Public]
    public override bool CheckCertificateConfirmByESIA(DirRX.SignPlatform.ICertificateRequest requestEntry)
    {
      return base.CheckCertificateConfirmByESIA(requestEntry);
    }

    /// <summary>
    /// Запустить новый выпуск, если истекает срока действия облачных сертификатов.
    /// </summary>
    /// <param name="certificates">Сертификаты, по которым будет выполнен выпуск.</param>
    /// <returns>True, если нет ошибок или хотя бы один выпуск сертификата был стартован, иначе - false.</returns>
    [Public]
    public override bool TrySendCertificateIssueTask(List<Sungero.CoreEntities.ICertificate> certificates)
    {
      Logger.Debug("Jobs CloudCertificatesReissueTasks: Checking cloud certificates");
      var hasErrors = false;
      var anyIssueCreate = false;

      foreach (var certificate in certificates)
      {
        try
        {
          var employee = EssPlatformSolution.Employees.As(certificate.Owner);
          
          if (employee == null || employee.Status == Sungero.CoreEntities.DatabookEntry.Status.Closed)
          {
            Logger.DebugFormat("Jobs CloudCertificatesReissueTasks: employee is not found. CertificateId='{0}'", certificate.Id);
            continue;
          }
          
          var author = EssPlatformSolution.Employees
            .GetAll(e => Equals(e.BusinessUnit, employee.BusinessUnit) && e.Status == Sungero.Company.Employee.Status.Active)
            .Select(s => Sungero.CoreEntities.Users.As(s)).ToList()
            .Where(u => u.IncludedIn(DirRX.EssPlatform.PublicConstants.Module.AdminElEmployeeInteractionSystem))
            .FirstOrDefault();
          
          if (author == null)
          {
            hasErrors = true;
            
            Logger.ErrorFormat(
              "Jobs CloudCertificatesReissueTasks: author for CertificateIssueTask is not found. EmployeeId='{0}', CertificateId='{1}'",
              employee.Id,
              certificate.Id);
            
            continue;
          }
          
          var certificateRequest = DirRX.SignPlatform.CertificateRequests.GetAll().First(p => p.CertificateID == certificate.Id);
          var userConnectionParams = DirRX.SignPlatform.Structures.Module.UserConnectionParams.Create(
            certificateRequest.ProviderId,
            certificateRequest.ProviderType,
            certificateRequest.IdentificationType.Value.Value,
            true);

          DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.Remote.IssueCertificate(employee, author, userConnectionParams);
          anyIssueCreate = true;
        }
        catch (Exception ex)
        {
          hasErrors = true;
          Logger.Error("Jobs CloudCertificatesReissueTasks: Error while issue certificate date about cloud certificates expiration", ex);
        }
      }

      if (!certificates.Any())
        Logger.Debug("Jobs CloudCertificatesReissueTasks: No expiring cloud certificates for issue certificate");
      else if (!anyIssueCreate && !hasErrors)
        Logger.Debug("Jobs CloudCertificatesReissueTasks: No employees for expiring cloud certificates for issue certificate");

      return anyIssueCreate || !hasErrors;
    }

    /// <summary>
    /// Деактивировать ранее выпущенные "облачные" сертификаты персоны. Оставить только последний выпущенный.
    /// </summary>
    /// <param name="personId">ИД персоны.</param>
    /// <param name="providerId">ИД провайдера.</param>
    [Public]
    public override void DisablePrevCloudCertificates(long personId, string providerId)
    {
      var mainEmployee = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(personId, null);
      
      var certificates = Certificates
        .GetAll()
        .Where(c => c.Owner.Equals(mainEmployee)
               && c.Enabled == true
               && c.Parameters.Any(p => Equals(p.Value, providerId))
               && c.PluginId.ToLower().Equals(DirRX.SignPlatform.PublicConstants.Module.PluginIdOfCloudCertificates))
        .OrderByDescending(o => o.Id).ToList();
      
      var mainCert = certificates.FirstOrDefault();
      if (mainCert != null)
        certificates.Remove(mainCert);
      
      foreach(var cert in certificates)
      {
        try
        {
          Logger.DebugFormat("CertificateIssue. DisablePrevCloudCertificates(). Try disable certificate with id {0} and thumbprint {1} by {2}", cert.Id, cert.Thumbprint, cert.Owner.Name);
          cert.Enabled = false;
          cert.Save();
        }
        catch(Exception ex)
        {
          Logger.DebugFormat("CertificateIssue. DisablePrevCloudCertificates(). Error when disable cert id {0}. {1}", cert.Id, ex.StackTrace);
        }
      }
    }

    /// <summary>
    /// Выпустить сертификат сотруднику.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="author">Автор.</param>
    /// <param name="userConnectionParams">Параметры для выпуска сертификата.</param>
    [Public, Remote]
    public override void IssueCertificate(
      EssPlatformSolution.IEmployee employee,
      IUser author,
      DirRX.SignPlatform.Structures.Module.IUserConnectionParams userConnectionParams)
    {
      var isCustomCertificateIssueProcess = NNCommons.PublicFunctions.Module.Remote.GetBoolValue(NNCommons.PublicConstants.Module.ConstantsSetting.CertificateIssueProcess.Guid).Value;
      if (isCustomCertificateIssueProcess)
      {
        var task = NNCommons.CertificateIssueTasks.Create();
        task.Author = author;
        task.Employee = employee;
        task.IdentificationType = GetIdentificationTypeEnumeration(userConnectionParams.IdentificationType);
        task.ProviderType = GetProviderTypeEnumeration(userConnectionParams.ProviderType);
        task.ProviderId = userConnectionParams.ProviderId;
        task.Start();
      }
      else
      {
        base.IssueCertificate(employee, author, userConnectionParams);
      }
    }

    /// <summary>
    /// Выпустить сертификат сотруднику.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="author">Автор.</param>
    /// <param name="userConnectionParams">Параметры для выпуска сертификата.</param>
    /// <param name="fromTask">Задача на выпуск сертификата.</param>
    [Public, Remote]
    public virtual void IssueCertificate(DirRX.EssPlatformSolution.IEmployee employee, IUser author, DirRX.SignPlatform.Structures.Module.IUserConnectionParams userConnectionParams, DirRX.NNCommons.ICertificateIssueTask fromTask)
    {
      var task = NNCommons.CertificateIssueTasks.Create();
      task.Author = author;
      task.Employee = employee;
      task.IdentificationType = GetIdentificationTypeEnumeration(userConnectionParams.IdentificationType);
      task.ProviderType = GetProviderTypeEnumeration(userConnectionParams.ProviderType);
      task.ProviderId = userConnectionParams.ProviderId;
      if (fromTask != null && fromTask.MonitoringTask != null)
        task.MonitoringTask = fromTask.MonitoringTask;
      task.Start();
    }

    /// <summary>
    /// Сохранить облачный сертификат в справочник "Цифровые сертификаты".
    /// </summary>
    /// <param name="requestEntry">Запись из справочника заявок на выпуск УНЭП.</param>
    /// <returns>Признак, изменен ли статус в записи.</returns>
    [Public]
    public override bool SaveCertificate(DirRX.SignPlatform.ICertificateRequest requestEntry)
    {
      Logger.DebugFormat(
        "CertificateIssue. SaveCertificate(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2} start",
        requestEntry.RequestId,
        requestEntry.ProviderId,
        requestEntry.IssueStatus);
      
      Enumeration? newIssueStatus = null;
      string errorMessage = string.Empty;
      long? certificateID = null;
      
      if(requestEntry.CloudCertificateID.HasValue)
      {
        var oldIssueStatus = requestEntry.IssueStatus;
        var baseUri = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings().SignServiceAddress;
        var requestApi = string.Format(NNCommons.PublicConstants.Module.RequestApiSignService.GetCertificate, requestEntry.ProviderId, requestEntry.CloudCertificateID.Value);
        var token = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetAnAuthenticationToken(NNCommons.PublicConstants.Module.SignServiceAudience);
        
        Logger.DebugFormat(
          "CertificateIssue. SaveCertificate(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2} token received",
          requestEntry.RequestId,
          requestEntry.ProviderId,
          requestEntry.IssueStatus);
        
        try
        {
          var response = DirRX.EssPlatform.PublicFunctions.Module.RunGetRequest(baseUri, requestApi, token);
          
          Logger.DebugFormat(
            "CertificateIssue. SaveCertificate(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2}, urlapi='/{3}' response.StatusCode={4}",
            requestEntry.RequestId,
            requestEntry.ProviderId,
            requestEntry.IssueStatus,
            requestApi, response.StatusCode);
          
          if (response.StatusCode == (int)HttpStatusCode.OK)
          {
            var certificate = JsonConvert.DeserializeObject<DirRX.SignPlatform.Structures.Module.Certificate>(response.RequestMessage);
            var mainEmployee = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(requestEntry.Employee.Person.Id, null);
            
            if (string.IsNullOrEmpty(requestEntry.ProviderType))
            {
              var providersInfo = GetCloudSignProviderInfo();
              requestEntry.ProviderType = providersInfo.First(info => info.ProviderId == requestEntry.ProviderId).ProviderType;
              requestEntry.Save();
            }
            
            var officialProviderName = NNCommons.PublicFunctions.Module.GetStringValue(NNCommons.PublicConstants.Module.ConstantsSetting.OfficialProviderName.Guid);
            
            var digitalCertificate = Sungero.CoreEntities.Certificates.Create();
            digitalCertificate.Owner = mainEmployee;
            var description = DirRX.SignPlatform.CertificateIssueTasks.Resources.CertificateDescriptionFormat(officialProviderName,
                                                                                                              requestEntry.Employee.Person.ShortName,
                                                                                                              certificate.ValidTo.ToShortDateString());
            var descriptionPropetyLength = digitalCertificate.Info.Properties.Description.Length;
            digitalCertificate.Description = DirRX.SignPlatform.PublicFunctions.Module.GetLimitStringWithEllipsis(description, descriptionPropetyLength);
            digitalCertificate.Subject = certificate.OwnerCommonName;
            digitalCertificate.Issuer = certificate.IssuerCommonName;
            digitalCertificate.NotBefore = certificate.ValidFrom;
            digitalCertificate.NotAfter = certificate.ValidTo;
            digitalCertificate.Thumbprint = certificate.Thumbprint;
            digitalCertificate.X509Certificate = System.Convert.FromBase64String(certificate.Content);
            digitalCertificate.PluginId = DirRX.SignPlatform.Constants.Module.PluginIdOfCloudCertificates;
            // Заполнение providerId
            var providerIdParameter = digitalCertificate.Parameters.AddNew();
            providerIdParameter.Key = DirRX.SignPlatform.PublicConstants.Module.ProviderIdCertificateKeyParameter;
            providerIdParameter.Value = certificate.ProviderId;
            // Заполнение ownerId
            var ownerIdParameter = digitalCertificate.Parameters.AddNew();
            ownerIdParameter.Key = DirRX.SignPlatform.PublicConstants.Module.OwnerIdCertificateKeyParameter;
            ownerIdParameter.Value = certificate.OwnerId;
            digitalCertificate.Save();
            
            Logger.DebugFormat(
              "CertificateIssue. SaveCertificate(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2}, saved certificate for employeeId={3}",
              requestEntry.RequestId,
              requestEntry.ProviderId,
              requestEntry.IssueStatus,
              mainEmployee.Id);
            
            certificateID = digitalCertificate.Id;
            newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.CertRegistered;
            
            this.DisablePrevCloudCertificates(requestEntry.Employee.Person.Id, certificate.ProviderId);
          }
          else if (response.StatusCode == (int)HttpStatusCode.InternalServerError || response.StatusCode == (int)HttpStatusCode.NotFound)
          {
            var result = JsonConvert.DeserializeObject<DirRX.OverrideSolution.Module.SignPlatform.Structures.Module.HttpRequestErrorResult>(response.RequestMessage);
            
            Logger.DebugFormat(
              "CertificateIssue. SaveCertificate(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2} resultError.Code={3}",
              requestEntry.RequestId,
              requestEntry.ProviderId,
              requestEntry.IssueStatus,
              result.Code);
            
            if (result.Code != NNCommons.PublicConstants.Module.SignServiceResponseCodes.TemporaryUnavailableError)
            {
              var mainErrorText = Resources.Error500SignServiceRequest;
              if (response.StatusCode == (int)HttpStatusCode.NotFound)
                mainErrorText = Resources.Error404SignServiceRequest;
              errorMessage = this.GetErrorMessage(mainErrorText, result);
              newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
            }
          }
          else
          {
            errorMessage = DirRX.SignPlatform.Resources.UnknownResponseCodeFormat(response.StatusCode);
            newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
          }
        }
        // перехватить ошибку "хост не найдет", которая будет признаком того, что пропал канал связи с сервисом
        catch (System.AggregateException ex)
        {
          if(ex.InnerException.GetType() == typeof(System.Net.Http.HttpRequestException))
          {
            Logger.DebugFormat(
              "CertificateIssue. SaveCertificate(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2} host {3} not found",
              requestEntry.RequestId,
              requestEntry.ProviderId,
              requestEntry.IssueStatus,
              baseUri);
            
            return false;
          }
          else
          {
            throw ex;
          }
        }
      }
      else
      {
        // ошибка обработки заявки - не найден ИД облачного сертификата
        errorMessage = DirRX.SignPlatform.Resources.CloudCertificateIDNotFoundFormat(requestEntry.RequestId);
        newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
      }
      
      // обновить запись в CertificateRequest если что-то поменялось
      if (newIssueStatus != null)
      {
        Logger.DebugFormat(
          "CertificateIssue. SaveCertificate(). requestEntry.RequestId={0} requestEntry.ProviderId={1} change IssueStatus={2}->{3}",
          requestEntry.RequestId,
          requestEntry.ProviderId,
          requestEntry.IssueStatus,
          newIssueStatus);
        
        if(certificateID.HasValue)
          requestEntry.CertificateID = certificateID.Value;
        requestEntry.IssueStatus = newIssueStatus;
        requestEntry.LastStatusChange = Calendar.Now;
        if (!string.IsNullOrEmpty(errorMessage))
        {
          requestEntry.Errors = errorMessage;
        }
        requestEntry.Save();
        return true;
      }
      // ничего не изменилось
      Logger.DebugFormat(
        "CertificateIssue. SaveCertificate(). requestEntry.RequestId={0} requestEntry.ProviderId={1} no change IssueStatus={2}",
        requestEntry.RequestId,
        requestEntry.ProviderId,
        requestEntry.IssueStatus);
      
      return false;
    }

    /// <summary>
    /// Проверить статус заявки на выпуск УНЭП.
    /// </summary>
    /// <param name="requestEntry">Запись из справочника заявок на выпуск УНЭП.</param>
    /// <returns>Признак, изменен ли статус в записи.</returns>
    [Public]
    public override bool CheckCertificateStatus(DirRX.SignPlatform.ICertificateRequest requestEntry)
    {
      Logger.DebugFormat(
        "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2} start",
        requestEntry.RequestId,
        requestEntry.ProviderId,
        requestEntry.IssueStatus);
      
      var oldIssueStatus = requestEntry.IssueStatus;
      var baseUri = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings().SignServiceAddress;
      var token = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetAnAuthenticationToken(DirRX.SignPlatform.Constants.Module.SignServiceAudience);
      
      Logger.DebugFormat(
        "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2} token received",
        requestEntry.RequestId,
        requestEntry.ProviderId,
        requestEntry.IssueStatus);
      
      try
      {
        var requestApi = string.Format(NNCommons.PublicConstants.Module.RequestApiSignService.GetStatementStatus, requestEntry.ProviderId, requestEntry.RequestId.Value);
        var response = DirRX.EssPlatform.PublicFunctions.Module.RunGetRequest(baseUri, requestApi, token);
        Enumeration? newIssueStatus = null;
        string errorMessage = string.Empty;
        
        Logger.DebugFormat(
          "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2}, urlapi='/{3}' response.StatusCode={4}",
          requestEntry.RequestId,
          requestEntry.ProviderId,
          requestEntry.IssueStatus,
          requestApi,
          response.StatusCode);
        
        if (response.StatusCode == 200)
        {
          var result = JsonConvert.DeserializeObject<DirRX.SignPlatform.Structures.Module.HttpRequestResult>(response.RequestMessage);
          Logger.DebugFormat(
            "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2}, result.Status={3}",
            requestEntry.RequestId,
            requestEntry.ProviderId,
            requestEntry.IssueStatus,
            result.Status);
          
          if (requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.DocVerification)
          {
            switch (result.Status)
            {
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.DocumentsVerification:
                // остаться на этом же шаге
                break;
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.NeedConfirm:
                // перейти на этап скачивания тела заявления
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.NeedDownloadSt;
                break;
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.Error:
                // ошибка обработки заявки
                errorMessage = result.Error;
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
                break;
              default:
                // ошибка "неожижанное значение статуса"
                Logger.DebugFormat(
                  "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestnEntry.ProviderId={1} IssueStatus={2}, unexpected result.Status={3}",
                  requestEntry.RequestId,
                  requestEntry.ProviderId,
                  requestEntry.IssueStatus,
                  result.Status);
                
                errorMessage = Resources.UnexpectedStatusErrorFormat(result.Status);
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
                break;
            }
          }
          else if (requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.IssVerification)
          {
            switch (result.Status)
            {
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.IssueVerification:
                // остаться на этом же шаге
                break;
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.InProgress:
                // остаться на этом же шаге, но сменить статус заявки
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.InProgress;
                break;
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.Success:
                // сменить статус заявки - сертификат готов
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.CertCreated;
                break;
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.Error:
                // ошибка обработки заявки
                errorMessage = result.Error;
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
                break;
              default:
                Logger.DebugFormat(
                  "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2}, unexpected result.Status={3}",
                  requestEntry.RequestId,
                  requestEntry.ProviderId,
                  requestEntry.IssueStatus,
                  result.Status);
                
                // ошибка "неожижанное значение статуса"
                errorMessage = Resources.UnexpectedStatusErrorFormat(result.Status);
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
                break;
            }
          }
          else if (requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.InProgress)
          {
            switch (result.Status) {
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.InProgress:
                // остаться на этом же шаге
                break;
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.Success:
                // перейти на этап скачивания сертификата
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.CertCreated;
                break;
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.Error:
                // ошибка обработки заявки
                errorMessage = result.Error;
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
                break;
              default:
                Logger.DebugFormat(
                  "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2}, unexpected result.Status={3}",
                  requestEntry.RequestId,
                  requestEntry.ProviderId,
                  requestEntry.IssueStatus,
                  result.Status);
                
                // ошибка "неожижанное значение статуса"
                errorMessage = DirRX.SignPlatform.Resources.UnexpectedStatusErrorFormat(result.Status);
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
                break;
            }
          }
          else if (requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.CertCreated)
          {
            switch (result.Status) {
              case DirRX.OverrideSolution.Module.SignPlatform.Constants.Module.Response.Success:
                // остаться на этом же шаге
                break;
              default:
                Logger.DebugFormat(
                  "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2}, unexpected result.Status={3}",
                  requestEntry.RequestId,
                  requestEntry.ProviderId,
                  requestEntry.IssueStatus, result.Status);
                
                // ошибка "неожижанное значение статуса"
                errorMessage = DirRX.SignPlatform.Resources.UnexpectedStatusErrorFormat(result.Status);
                newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
                break;
            }
          }
          else
          {
            // ошибка
            Logger.DebugFormat(
              "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} unecpected IssueStatus={2}",
              requestEntry.RequestId,
              requestEntry.ProviderId,
              requestEntry.IssueStatus);
            
            errorMessage = DirRX.SignPlatform.Resources.UnexpectedRequestStatusErrorFormat(requestEntry.IssueStatus.Value);
            newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
          }
        }
        else if (response.StatusCode == (int)HttpStatusCode.InternalServerError || response.StatusCode == (int)HttpStatusCode.NotFound)
        {
          var result = JsonConvert.DeserializeObject<DirRX.OverrideSolution.Module.SignPlatform.Structures.Module.HttpRequestErrorResult>(response.RequestMessage);
          
          Logger.DebugFormat(
            "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2} resultError.Code={3}",
            requestEntry.RequestId,
            requestEntry.ProviderId,
            requestEntry.IssueStatus,
            result.Code);
          
          if (result.Code != NNCommons.PublicConstants.Module.SignServiceResponseCodes.TemporaryUnavailableError)
          {
            var mainErrorText = Resources.Error500SignServiceRequest;
            if (response.StatusCode == (int)HttpStatusCode.NotFound)
              mainErrorText = Resources.Error404SignServiceRequest;
            errorMessage = this.GetErrorMessage(mainErrorText, result);
            newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
          }
        }
        else
        {
          errorMessage = DirRX.SignPlatform.Resources.UnknownResponseCodeFormat(response.StatusCode);
          newIssueStatus = DirRX.SignPlatform.CertificateRequest.IssueStatus.Error;
        }
        // обновить запись в CertificateRequest если что-то поменялось
        if (newIssueStatus != null)
        {
          Logger.DebugFormat(
            "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} change IssueStatus={2}->{3}",
            requestEntry.RequestId,
            requestEntry.ProviderId,
            requestEntry.IssueStatus,
            newIssueStatus);
          
          if(newIssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.CertCreated && response.StatusCode == (int)HttpStatusCode.OK)
            requestEntry.CloudCertificateID = JsonConvert.DeserializeObject<DirRX.SignPlatform.Structures.Module.HttpRequestResult>(response.RequestMessage).CertificateId;
          requestEntry.IssueStatus = newIssueStatus;
          requestEntry.LastStatusChange = Calendar.Now;
          if (!string.IsNullOrEmpty(errorMessage))
          {
            requestEntry.Errors = errorMessage;
          }
          requestEntry.Save();
          return true;
        }
        // ничего не изменилось
        Logger.DebugFormat(
          "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} no change IssueStatus={2}",
          requestEntry.RequestId,
          requestEntry.ProviderId,
          requestEntry.IssueStatus);
        
        return false;
      }
      // перехватить ошибку "хост не найдет", которая будет признаком того, что пропал канал связи с сервисом
      catch (System.AggregateException ex)
      {
        if(ex.InnerException.GetType() == typeof(System.Net.Http.HttpRequestException))
        {
          Logger.DebugFormat(
            "CertificateIssue. CheckCertificateStatus(). requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2} host {3} not found",
            requestEntry.RequestId,
            requestEntry.ProviderId,
            requestEntry.IssueStatus,
            baseUri);
          
          return false;
        }
        else
        {
          throw ex;
        }
      }
    }

    /// <summary>
    /// Проверить наличие записи с указанным RequestId в справочнике Запросы на выдачу сертификата электронной подписи.
    /// </summary>
    /// <param name="requestId">ИД запроса на выдачу сертификата электронной подписи.</param>
    /// <param name="providerId">ИД провайдера.</param>
    /// <returns>True, если запрос существует, иначе false.</returns>
    /// <remarks>Наличе записи проверять с правами на просмотр, так как важно именно наличие записи без учета прав у текущего пользователя.</remarks>
    [Public, Remote]
    public static bool ExistsCertificateRequest(DirRX.NNCommons.ICertificateIssueTask certificateIssueTask)
    {
      var existsRequest = false;
      AccessRights.AllowRead(
        () =>
        {
          existsRequest = GetCertificateRequest(certificateIssueTask) != null;
        });
      return existsRequest;
    }

    /// <summary>
    /// Возвращает запрос на выпуск сертификата, на основе параметров задачи.
    /// </summary>
    /// <param name="certificateIssueTask">Задача на выпуск сертификата.</param>
    /// <returns>Запрос на выпуск сертификата.</returns>
    [Public]
    public static DirRX.SignPlatform.ICertificateRequest GetCertificateRequest(DirRX.NNCommons.ICertificateIssueTask certificateIssueTask)
    {
      return DirRX.SignPlatform.CertificateRequests
        .GetAll(x => x.RequestId == certificateIssueTask.RequestId && x.ProviderId == certificateIssueTask.ProviderId)
        .FirstOrDefault();
    }

    /// <summary>
    /// Создать структуру для запроса на инициирование процесса выпуска сертификата - /CertificateIssues
    /// </summary>
    /// <param name="employee">Сотрудник, которому необходимо выпустить сертификат.</param>
    /// <param name="identificationType">Тип идентификации.</param>
    /// <returns>Структура запроса на выпуск сертификата.</returns>
    public override DirRX.SignPlatform.Structures.Module.CertificateIssueRequest CreateCertificateIssueRequest(EssPlatformSolution.IEmployee employee, string identificationType)
    {
      var certificateIssueRequest = base.CreateCertificateIssueRequest(employee, identificationType);
      certificateIssueRequest.PhoneNumber = DirRX.OverrideSolution.PublicFunctions.Employee.GetCertificatePhoneNumber(Employees.As(employee));
      certificateIssueRequest.Email = string.IsNullOrEmpty(certificateIssueRequest.Email) ?
        NNCommons.PublicFunctions.Module.GetStringValue(NNCommons.PublicConstants.Module.ConstantsSetting.CertificateEmail.Guid) :
        certificateIssueRequest.Email;
      
      return certificateIssueRequest;
    }

    /// <summary>
    /// Получить документы удостоверяющие личность.
    /// </summary>
    /// <param name="person">Персона.</param>
    /// <returns>Список документов удостоверяющих личность.</returns>
    public override List<DirRX.SignPlatform.Structures.Module.IdentityDocument> GetIdentityDocuments(EssPlatformSolution.IPerson person)
    {
      
      //TODO: Seleznyov_DV. Подмена документа удостовреяющего личность.
      if (person.IdentityDocumentKind != null)
        return base.GetIdentityDocuments(person);
      
      var identityDocuments = new List<DirRX.SignPlatform.Structures.Module.IdentityDocument>();
      var identityDocument = DirRX.SignPlatform.Structures.Module.IdentityDocument.Create();
      identityDocument.IdentityDocumentType = DirRX.SignPlatform.Constants.Module.IdentityDocumentType.OtherIdentity;
      identityDocument.Number = person.INILA;
      identityDocument.IssueDate = Calendar.Today;
      identityDocument.IssuedOrganization = "Норникель";
      identityDocuments.Add(identityDocument);
      
      // СНИЛС
      var INILA = DirRX.SignPlatform.Structures.Module.IdentityDocument.Create();
      INILA.IdentityDocumentType = "Snils";
      INILA.Number = person.INILA;
      INILA.BirthDate = person.DateOfBirth.Value;
      identityDocuments.Add(INILA);
      
      return identityDocuments;
    }

    /// <summary>
    /// Создать структуру для запроса на формирование текста заявления - /CertificateIssues/{requestId}/statement.
    /// </summary>
    /// <param name="employee">Сотрудник, которому необходимо выпустить сертификат.</param>
    /// <returns>Структура запроса на создание заявления на выпуск сертификата.</returns>
    public override DirRX.SignPlatform.Structures.Module.CertificateIssueStatementRequest CreateCertificateIssueStatementRequest(DirRX.SignPlatform.ICertificateRequest requestEntry)
    {
      var person = DirRX.EssPlatformSolution.People.Get(requestEntry.Employee.Person.Id);

      if (!ValidateInputData(requestEntry, person))
        return null;
      
      var certificateIssueRequest = DirRX.SignPlatform.Structures.Module.CertificateIssueStatementRequest.Create();
      certificateIssueRequest.Surname = person.LastName;
      certificateIssueRequest.GivenName = person.FirstName;
      certificateIssueRequest.Patronym = person.MiddleName;
      certificateIssueRequest.BirthDate = person.DateOfBirth.Value;
      var inn = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(DirRX.NNCommons.PublicConstants.Module.ConstantsSetting.GeneralInn.Guid);

      if (string.IsNullOrEmpty(inn))
      {
        Logger.ErrorFormat("CreateCertificateIssueStatementRequest. Не заполнена прикладная константа. Имя константы: {0}. Guid: {1}.",
                           DirRX.NNCommons.PublicConstants.Module.ConstantsSetting.GeneralInn.Name,
                           DirRX.NNCommons.PublicConstants.Module.ConstantsSetting.GeneralInn.Guid);
        return null;
      }
      
      certificateIssueRequest.Inn = inn;
      certificateIssueRequest.IdentificationType = requestEntry.IdentificationType.Value.Value;
      certificateIssueRequest.IdentityDocuments = this.GetIdentityDocumentsForPersonnelNumber(requestEntry.Employee.PersonnelNumber);
      certificateIssueRequest.PhoneNumber = DirRX.OverrideSolution.PublicFunctions.Employee.GetCertificatePhoneNumber(Employees.As(requestEntry.Employee));
      certificateIssueRequest.Email = GetCertificateEmail(requestEntry.Employee);
      
      return certificateIssueRequest;
    }

    /// <summary>
    /// Проверить заполненность полей у входных данных.
    /// </summary>
    /// <param name="request">Запрос на выпуск сертификата.</param>
    /// <param name="person">Персона.</param>
    /// <returns>True - если валидация пройдена, иначе false.</returns>
    public virtual bool ValidateInputData(DirRX.SignPlatform.ICertificateRequest request, DirRX.EssPlatformSolution.IPerson person)
    {
      var errors = new List<string> { };
      var logPrefix = string.Format("ValidateInputData. ИД: {0} персоны", person.Id);
      
      if (!person.DateOfBirth.HasValue)
        errors.Add(string.Format("{0}. Не заполнена дата рождения.", logPrefix));
      
      if (string.IsNullOrEmpty(request.Employee.PersonnelNumber))
        errors.Add(string.Format("{0}. Не заполнен табельный номер.", logPrefix));
      
      if (string.IsNullOrEmpty(request.Employee.PersonalPhoneDirRX))
        errors.Add(string.Format("{0}. Не заполнен номер телефона.", logPrefix));

      if (!errors.Any())
        return true;
      
      foreach (var error in errors)
      {
        Logger.Error(error);
      }

      return false;
    }

    /// <summary>
    /// Получить документы удостоверяющие личность с табельным номером.
    /// </summary>
    /// <param name="personnelNumber">Табельный номер.</param>
    /// <returns>Список документов удостоверяющих личность.</returns>
    public List<DirRX.SignPlatform.Structures.Module.IdentityDocument> GetIdentityDocumentsForPersonnelNumber(string personnelNumber)
    {
      var identityDocument = DirRX.SignPlatform.Structures.Module.IdentityDocument.Create();
      identityDocument.IdentityDocumentType = DirRX.SignPlatform.Constants.Module.IdentityDocumentType.OtherIdentity;
      identityDocument.Number = personnelNumber;
      identityDocument.IssueDate = Calendar.Today;
      
      var issuedOrg = DirRX.NNCommons.PublicFunctions.Module.GetStringValue(DirRX.NNCommons.PublicConstants.Module.ConstantsSetting.PersNumIssuedOrganisation.Guid);
      if (string.IsNullOrEmpty(issuedOrg))
      {
        Logger.ErrorFormat("GetIdentityDocumentsForPersonnelNumber. Не заполнена прикладная константа. Имя константы: {0}. Guid: {1}.",
                           DirRX.NNCommons.PublicConstants.Module.ConstantsSetting.PersNumIssuedOrganisation.Name,
                           DirRX.NNCommons.PublicConstants.Module.ConstantsSetting.PersNumIssuedOrganisation.Guid);
        return null;
      }
      identityDocument.IssuedOrganization = issuedOrg;
      
      return new List<DirRX.SignPlatform.Structures.Module.IdentityDocument>(){identityDocument};
    }

    /// <summary>
    /// Валидировать обязательные поля карточки Персоны.
    /// </summary>
    /// <param name="basePerson">Персона.</param>
    /// <returns>Список ошибок валидации.</returns>
    [Public, Remote]
    public override List<string> ValidatePersonRequiredFields(Sungero.Parties.IPerson basePerson)
    {
      var errors = new List<string>();
      
      var person = EssPlatformSolution.People.GetAll(currentPerson => Equals(currentPerson.Id, basePerson.Id)).First();
      
      // Проверить, что в карточке Персоны заполнен "СНИЛС"
      if (string.IsNullOrWhiteSpace(person.INILA))
      {
        errors.Add(SignPlatform.Resources.InilaRequired);
      }
      
      // Проверить, что в карточке Персоны заполнена "Дата рождения"
      if (person.DateOfBirth == null)
      {
        errors.Add(SignPlatform.Resources.DateOfBirthRequired);
      }
      
      // Проверить, что в карточке Персоны заполнен "Пол"
      if (person.Sex == null)
      {
        errors.Add(SignPlatform.Resources.GenderError);
      }
      
      return errors;
    }

    /// <summary>
    /// Извлечь тексты ошибок из ответа запроса
    /// </summary>
    /// <param name="errorMain">Основной текст ошибки.</param>
    /// <param name="errorDetail">Распарсенный json с описанием ответа с ошибками.</param>
    [Public]
    public StateView GetCertificateIssueErrorAssignmentStateView(string text)
    {
      var stateView =  base.GetAssignmentStateView(text);
      return stateView;
    }

    /// <summary>
    /// Получить скрытый номер телефона.
    /// </summary>
    /// <param name="phone">Номер телефона.</param>
    /// <returns>Скрытый номер телефона.</returns>
    [Public, Remote]
    public override string GetHiddenPhone(string phone)
    {
      try
      {
        phone = System.Text.RegularExpressions.Regex.Replace(phone, "[^0-9]", "");
        
        // Проверка на минимальную длину номера.
        if (phone.Length < DirRX.SignPlatform.Constants.Module.HiddenPhone.FirstPartLength +
            DirRX.SignPlatform.Constants.Module.HiddenPhone.LastPartLength)
          throw AppliedCodeException.Create("Телефонный номер слишком короткий для скрытия.");

        var hiddenLength = phone.Length - DirRX.SignPlatform.Constants.Module.HiddenPhone.FirstPartLength -
          DirRX.SignPlatform.Constants.Module.HiddenPhone.LastPartLength;
        
        if (hiddenLength < 0)
          throw AppliedCodeException.Create("Недостаточно цифр для скрытия.");
        
        var firstPart = phone.Substring(0, DirRX.SignPlatform.Constants.Module.HiddenPhone.FirstPartLength);
        var hiddenPart = phone.Substring(DirRX.SignPlatform.Constants.Module.HiddenPhone.FirstPartLength, hiddenLength);
        var lastPart = phone.Substring(DirRX.SignPlatform.Constants.Module.HiddenPhone.FirstPartLength + hiddenLength, DirRX.SignPlatform.Constants.Module.HiddenPhone.LastPartLength);
        var hiddenPhone = firstPart + System.Text.RegularExpressions.Regex.Replace(hiddenPart ,@"\d", "X") + lastPart;
        return hiddenPhone;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetHiddenPhone. Ошибка: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
        throw;
      }
    }

  }
}