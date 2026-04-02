using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.SignPlatform.Server
{
  partial class ModuleJobs
  {

    /// <summary>
    /// Запустить задачу на перевыпуск сертификатов.
    /// </summary>
    public override void CloudCertificatesReissueTasks()
    {
      bool sendResult = false;
      var previousRun = Calendar.Today;
      try
      {
        var settings = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings();
        var daysToWarningCertificate = settings.DaysToWarnCert ?? DirRX.EssPlatform.PublicConstants.Module.DefaultDaysToWarningCertificate;
        var result = Sungero.Docflow.PublicFunctions.Module.Remote.GetDocflowParamsStringValue(NNCommons.PublicConstants.Module.LastTaskOfCloudCertificatesExpiration);
        Logger.DebugFormat("Jobs CloudCertificatesReissueTasks: Last notification date of cloud certificates reissue in DB is {0} (UTC)", result);
        previousRun = result == null ? Calendar.Today.AddDays(Convert.ToDouble(-daysToWarningCertificate)) : Calendar.FromUtcTime(DateTime.Parse(result, null, System.Globalization.DateTimeStyles.AdjustToUniversal));
      }
      catch (Exception ex)
      {
        Logger.Error(DirRX.SignPlatform.Resources.ErrorTryGetLastCloudCertificatesExpirationTaskDate, ex);
        throw new Exception(DirRX.SignPlatform.Resources.ErrorTryGetLastCloudCertificatesExpirationTaskDate, ex);
      }
      var currentRun = Calendar.Now;
      try
      {
        var settings = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings();
        var daysToWarningCertificate = settings.DaysToWarnCert ?? DirRX.EssPlatform.PublicConstants.Module.DefaultDaysToWarningCertificate;
        var expiringCloudCertificates = Sungero.CoreEntities.Certificates.GetAll(c => c.NotAfter != null &&
                                                                                 c.Enabled != null && c.Enabled.Value == true &&
                                                                                 previousRun <= c.NotAfter.Value.AddDays(Convert.ToDouble(-daysToWarningCertificate)) &&
                                                                                 currentRun > c.NotAfter.Value.AddDays(Convert.ToDouble(-daysToWarningCertificate)) &&
                                                                                 c.PluginId == DirRX.SignPlatform.PublicConstants.Module.PluginIdOfCloudCertificates).ToList();
        
        sendResult = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.TrySendCertificateIssueTask(expiringCloudCertificates);
      }
      finally
      {
        if (sendResult)
        {
          var newDate = currentRun.Add(-Calendar.UtcOffset).ToString("yyyy-MM-ddTHH:mm:ss.ffff+0");
          Sungero.Docflow.PublicFunctions.Module.InsertOrUpdateDocflowParam(NNCommons.PublicConstants.Module.LastTaskOfCloudCertificatesExpiration, newDate);
          Logger.DebugFormat("Jobs CloudCertificatesReissueTasks: Last notification date of cloud certificates expiration is set to {0} (UTC)", newDate);
        }
        else
          Logger.Debug("Jobs CloudCertificatesReissueTasks: Last check date of cloud certificates reissue hasn't been changed");
      }
    }

    /// <summary>
    /// Мониторинг задач на выпуск сертификатов.
    /// </summary>
    public override void CertificateIssueTaskMonitoring()
    {
      Logger.Debug("CertificateIssue. Jobs CertificateIssueTaskMonitoring. Started.");
      var requestEntries = DirRX.SignPlatform.CertificateRequests.GetAll(x => x.Executor == DirRX.SignPlatform.CertificateRequest.Executor.BckgrndProcess)
        .Where(x => x.IssueStatus != DirRX.SignPlatform.CertificateRequest.IssueStatus.Error)
        .Where(x => x.IssueStatus != DirRX.SignPlatform.CertificateRequest.IssueStatus.NeedConfirm)
        .Where(x => x.IssueStatus != DirRX.SignPlatform.CertificateRequest.IssueStatus.CertCreated);
      
      foreach (var requestEntry in requestEntries)
      {
        Logger.DebugFormat(
          "CertificateIssue. Jobs CertificateIssueTaskMonitoring. requestEntry.RequestId={0} requestEntry.ProviderId={1} IssueStatus={2} start",
          requestEntry.RequestId,
          requestEntry.ProviderId,
          requestEntry.IssueStatus);

        // Сначала проверим на NeedConfirmEsia
        // Вдруг повезет, и если в ESIA уже подписали, то может уже и сертификат успеет выпуститься и следующеее условие это проверит
        if (requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.NeedConfirmEsia)
          if (DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.CheckCertificateConfirmByESIA(requestEntry))
            if (requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.IssVerification ||
                requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.DocVerification ||
                requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.Error) {
          // Потрогать задачу за активные блоки мониторинга, пусть проверит - не пора ли двигаться дальше
          NNCommons.CertificateIssueTasks.Get(requestEntry.TaskId.Value).Blocks.ExecuteAllMonitoringBlocks();
          continue;
        }
        
        // этап сохранения сертификата в справочник "Цифровые сертификаты"
        if (requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.CertCreated)
        {
          if (Functions.Module.SaveCertificate(requestEntry))
            if (requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.CertRegistered ||
                requestEntry.IssueStatus == DirRX.SignPlatform.CertificateRequest.IssueStatus.Error) {
            // Потрогать задачу за активные блоки мониторинга, пусть проверит - не пора ли двигаться дальше
            NNCommons.CertificateIssueTasks.Get(requestEntry.TaskId.Value).Blocks.ExecuteAllMonitoringBlocks();
            continue;
          }
        }
      }
      Logger.Debug("CertificateIssue. Jobs CertificateIssueTaskMonitoring. Finished.");
    }

  }
}