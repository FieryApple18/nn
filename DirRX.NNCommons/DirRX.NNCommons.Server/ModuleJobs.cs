using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution;

namespace DirRX.NNCommons.Server
{
  public partial class ModuleJobs
  {

    /// <summary>
    /// ФП по актуализации признака "TOП" в документах.
    /// </summary>
    public virtual void UpdateTopAttributeJob()
    {
      var logPrefix = "UpdateTopAttributeInDocumentsJob";
      
      Logger.DebugFormat("{0}. Старт фонового процесса \"Актуализация признака \"TOП\"\".", logPrefix);
      
      var hrDocumentTypes = Functions.Module.GetHRDocumentTypes();
      var hrDocumentTypesStr = string.Join(",", hrDocumentTypes.Select(guid => $"'{guid}'"));
      
      var commandText = string.Format(Queries.Module.UpdateTopAttributeQuery, hrDocumentTypesStr);
      using(var command = SQL.GetCurrentConnection().CreateCommand())
      {
        try
        {
          command.CommandText = commandText;
          command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
          Logger.DebugFormat("{0}. При выполнении запроса возникла ошибка. Ошибка: {1}", logPrefix, ex);
        }
      }
      
      Logger.DebugFormat("{0}. Завершение фонового процесса \"Актуализация признака \"TOП\"\".", logPrefix);
    }

    /// <summary>
    /// Норникель. Очистка частных календарей рабочего времени.
    /// </summary>
    public virtual void PrivateWorkingTimeCalendarsCleaningJob()
    {
      var functionName = nameof(PrivateWorkingTimeCalendarsCleaningJob);
      Logger.DebugFormat("{0}. Запущен фоновый процесс.", functionName);
      
      var daysAfterNewYear = Functions.Module.GetIntValue(Constants.Module.ConstantsSetting.CountDaysAfterNewYearToDeleteWorkingTimeCalendars.Guid) ??
        Constants.Module.DefaultCountDaysAfterNewYearToDeleteWorkingTimeCalendars;
      var dateToDeleteOldCalendars = Calendar.Today.BeginningOfYear().AddDays(daysAfterNewYear);
      var needDeleteOldCalendars = dateToDeleteOldCalendars <= Calendar.Today;
      var calendarsToDelete = PrivateWorkingTimeCalendars.GetAll().Where(c => (needDeleteOldCalendars && Calendar.Today.Year > c.Year) ||
                                                                         (c.Recipients.Count() == 1 &&
                                                                          c.Recipients.FirstOrDefault(r => Employees.Is(r.Recipient) &&
                                                                                                      r.Recipient.Status == Sungero.CoreEntities.Recipient.Status.Closed) != null));
      
      var deletedCalendarsCount = 0;
      var calendarsToDeleteCount = calendarsToDelete.Select(c => c.Id).Count();
      if (calendarsToDeleteCount != 0)
      {
        var iterationSize = Functions.Module.GetIntValue(Constants.Module.ConstantsSetting.CountWorkingTimeCalendarsToDeleteInIteration.Guid) ??
          Constants.Module.DefaultCountWorkingTimeCalendarsToDeleteInIteration;
        
        Logger.DebugFormat("{0}. Найдено календарей для удаления: {1}. Максимальное кол-во удаляемых календарей за итерацию ФП: {2}.", functionName, calendarsToDeleteCount, iterationSize);
        
        var calendarsToDeleteIds = calendarsToDelete.Take(iterationSize).Select(c => c.Id).ToList();
        foreach (var calendarId in calendarsToDeleteIds)
        {
          var isSuccess = Transactions.Execute(() => {
                                                 var calendar = PrivateWorkingTimeCalendars.GetAll(c => c.Id == calendarId).FirstOrDefault();
                                                 PrivateWorkingTimeCalendars.Delete(calendar);
                                               });
          if (isSuccess)
            deletedCalendarsCount++;
          else
            Logger.ErrorFormat("{0}. Не удалось удалить календарь c ИД {1}.", functionName, calendarId);
        }
      }
      else
        Logger.DebugFormat("{0}. Не найдены календари для удаления.", functionName);

      Logger.DebugFormat("{0}. Фоновый процесс завершил работу. Удалено календарей: {1}.", functionName, deletedCalendarsCount);
    }
    
    /// <summary>
    /// Уведомление о завершении срока действия УКЭП.
    /// </summary>
    public virtual void NotificationExpirationValidityPeriodSignature()
    {
      var functionName = nameof(NotificationExpirationValidityPeriodSignature);
      Logger.DebugFormat("{0}. Запущен фоновый процесс.", functionName);
      
      
      var constant = Functions.Module.GetStringValue(Constants.Module.ConstantsSetting.NumberDaysAdvancerequiredNotifyTerminationEP.Guid);
      if (string.IsNullOrEmpty(constant))
      {
        Logger.DebugFormat("{0}. Прикладная константа '{1}' не заполнена.", functionName, Constants.Module.ConstantsSetting.NumberDaysAdvancerequiredNotifyTerminationEP.Guid);
        return;
      }
      var daysNotification = new string[] {};
      if (!string.IsNullOrWhiteSpace(constant))
        daysNotification = constant.Trim().Split(',');

      if (!daysNotification.Any())
      {
        Logger.DebugFormat("{0}. Не указано кол-во дней за которое требуется уведомлять об окончании ЭП.", functionName);
        return;
      }
      
      var datesNotification = new List<DateTime>();
      foreach (var dayNotification in daysNotification)
      {
        var day = 0;
        if (int.TryParse(dayNotification, out day))
          datesNotification.Add(Calendar.Today.AddDays(day).Date);
      }
      
      var certificates = Sungero.CoreEntities.Certificates.GetAll(x => x.NotAfter.HasValue &&
                                                                  x.Enabled == true &&
                                                                  datesNotification.Contains(x.NotAfter.Value.Date)).AsEnumerable().Where(x => !Functions.Module.IsSimpleCertificate(x));
      
      Logger.DebugFormat("{0}. Кол-во найденных сертификатов {1}.", functionName, certificates.Count());
      
      foreach (var certificate in certificates)
      {
        var noticePerformer = new List<Sungero.CoreEntities.IRecipient>();

        var ownerCertificate = OverrideSolution.Employees.As(certificate.Owner);
        noticePerformer.Add(ownerCertificate);
        
        var businessUnitCertificate = OverrideSolution.BusinessUnits.As(ownerCertificate.BusinessUnit);
        if (businessUnitCertificate != null)
        {
          var isSendNotificationsCompletionUKEP = businessUnitCertificate.IsNeedNotifyCompletionUKEPDirRX.GetValueOrDefault();
          if (isSendNotificationsCompletionUKEP)
          {
            var recipients = businessUnitCertificate.RecipientsNotificationsUKEPDirRX.Select(x => x.Recipient);
            if (recipients.Any())
              noticePerformer.AddRange(recipients);
          }
        }

        if (noticePerformer.Any())
        {
          var noticeText = NNCommons.Resources.ValidityPeriodUCEPCertificateExpiresFormat(certificate.NotAfter.Value.ToShortDateString());
          var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(noticeText, noticePerformer.ToArray());
          notice.ActiveText = noticeText;
          notice.Attachments.Add(certificate);
          notice.Start();
        }
      }
      
      Logger.DebugFormat("{0}. Завершение работы фонового процесса.", functionName);
    }
    
    /// <summary>
    /// Поиск документов с ЭП формата CadesBes и CadesT в системе и их запись в справочник ImproveSignatureDocument.
    /// </summary>
    public virtual void FindDocumentsForImproveToCAdESXL()
    {
      Logger.Debug("FindDocumentsForImproveToCAdESXL: Начало выполнения.");
      var documentList = HRPro.HRDocFlow.PublicFunctions.Module.GetHRDocuments();
      var docWithVersions = Functions.Module.GetDocumentAndVersionsWithSignature(documentList);
      
      var issueNames = new string[] {};
      var issueConst = NNCommons.PublicFunctions.Module.GetStringValue(Constants.Module.ConstantsSetting.IssuerNames.Guid);
      if(!string.IsNullOrWhiteSpace(issueConst))
        issueNames = issueConst.Trim().Split(',');
      
      if (docWithVersions != null)
      {
        Logger.DebugFormat("FindDocumentsForImproveToCAdESXL: Найдено версий документов с подписями: {0}.", docWithVersions.Count());
        
        foreach (var docWithVersion in docWithVersions)
        {
          Logger.DebugFormat("FindDocumentsForImproveToCAdESXL: Ид документа: {0}, Ид версии: {1}. Начало проверки подписей по версии.", docWithVersion.Value.Id, docWithVersion.Key.Id);
          
          var document = docWithVersion.Value;
          var version = docWithVersion.Key;
          
          if ((!HRPro.HRCommons.ImproveSignatureDocuments.GetAll().Any()
               || !HRPro.HRCommons.ImproveSignatureDocuments.GetAll().Where(l => l.Document.Id.Equals(document.Id) && l.DocumentVersionId.Equals(version.Id)).Any())
              && Signatures.Get(document.Versions.SingleOrDefault(v => v.Id == version.Id)).Where(l => l.SignCertificate != null && !issueNames.Contains(l.SignCertificate.IssuerName)).Any())
          {
            var documentSignatures = Signatures.Get(document.Versions.SingleOrDefault(v => v.Id == version.Id)).Where(l => l.SignCertificate != null && !issueNames.Contains(l.SignCertificate.IssuerName)).ToList();
            if (documentSignatures.Any())
            {
              Logger.DebugFormat("FindDocumentsForImproveToCAdESXL: Ид документа: {0}, Ид версии: {1}. Найдено подписей УКЭП: {2}.", docWithVersion.Value.Id, docWithVersion.Key.Id, documentSignatures.Count());
              
              var entry = HRPro.HRCommons.ImproveSignatureDocuments.Create();
              entry.Name = document.Name;
              entry.Document = document;
              entry.DocumentVersionId = version.Id;
              entry.UpdateDate = HRPro.HRCommons.PublicFunctions.Module.GetMinValidityDate(documentSignatures);
              entry.Status = HRPro.HRCommons.ImproveSignatureDocument.Status.Active;
              foreach (var signature in documentSignatures)
              {
                var signatureInfo = Signatures.GetSignatureInfo(signature);
                var signatureKind = signatureInfo.SignatureFormat.ToString();
                var tableRow = entry.DocumentsSignature.AddNew();
                tableRow.SignatureKind = signatureKind;
                tableRow.CertificateExpirationDate = HRPro.HRCommons.PublicFunctions.Module.GetCertificateExpirationDateInSignature(signatureInfo);
              }
              entry.Save();
              Logger.DebugFormat("FindDocumentsForImproveToCAdESXL: Ид документа: {0}, Ид версии: {1}. Создана запись ImproveSignatureDocuments Id: {2}.", document.Id, version.Id, entry.Id);
            }
          }
          else if (HRPro.HRCommons.ImproveSignatureDocuments.GetAll().Where(l => l.Document.Id.Equals(document.Id) && l.DocumentVersionId.Equals(version.Id)).ToList()
                   .Where(l => l.DocumentsSignature.Count() != Signatures.Get(document.Versions.SingleOrDefault(v => v.Id == version.Id)).Where(u => u.SignCertificate != null && !Functions.Module.IsSimpleSignature(u)).Count()).Any())
          {
            var documentSignatures = Signatures.Get(document.Versions.SingleOrDefault(v => v.Id == version.Id)).Where(l => l.SignCertificate != null && !Functions.Module.IsSimpleSignature(l)).ToList();
            var entry = HRPro.HRCommons.ImproveSignatureDocuments.GetAll().Where(l => l.Document.Id.Equals(document.Id) && l.DocumentVersionId.Equals(version.Id)).FirstOrDefault();
            
            if(Locks.TryLock(entry))
            {
              try
              {
                entry.DocumentsSignature.Clear();
                if (documentSignatures.Count > 0)
                  entry.UpdateDate = HRPro.HRCommons.PublicFunctions.Module.GetMinValidityDate(documentSignatures);
                foreach (var signature in documentSignatures)
                {
                  var signatureInfo = Signatures.GetSignatureInfo(signature);
                  var signatureKind = signatureInfo.SignatureFormat.ToString();
                  var tableRow = entry.DocumentsSignature.AddNew();
                  tableRow.SignatureKind = signatureKind;
                  tableRow.CertificateExpirationDate = HRPro.HRCommons.PublicFunctions.Module.GetCertificateExpirationDateInSignature(signatureInfo);
                }
                
                entry.Save();
                Logger.DebugFormat("FindDocumentsForImproveToCAdESXL: Ид документа: {0}, Ид версии: {1}. Обновлена запись ImproveSignatureDocuments Id: {2}.", document.Id, version.Id, entry.Id);
              }
              catch(Exception error)
              {
                Logger.ErrorFormat("FindDocumentsForImproveToCAdESXL: Ид документа: {0}, Ид версии: {1}. ImproveSignatureDocuments Id: {2}. Произошла ошибка: {3}. StackTrace: {4}",
                                   document.Id, version.Id, entry.Id, error.Message, error.StackTrace);
              }
              finally
              {
                Locks.Unlock(entry);
              }
            }
            else
            {
              var lockInfo = Locks.GetLockInfo(entry);
              Logger.DebugFormat("FindDocumentsForImproveToCAdESXL: Ид документа: {0}, Ид версии: {1}. ImproveSignatureDocuments Id: {2}. Запись заблокирована. Заблокировал: {3} в {4}.",
                                 document.Id, version.Id, entry.Id, lockInfo.OwnerName, lockInfo.LockTime);
            }
          }
        }
      }
      Logger.Debug("FindDocumentsForImproveToCAdESXL: Окончание выполнения.");
    }
    
    /// <summary>
    /// Выбор документов для усовершенствования электронных подписей до формата CAdES-XL.
    /// </summary>
    public virtual void ImproveDocumentSignaturesToCAdESXL()
    {
      Logger.Debug("ImproveDocumentSignaturesToCAdESXL: Начало выполнения ФП.");
      var signatureUpdateDate = Functions.Module.GetSignatureUpdateDate();
      var docList = HRPro.HRCommons.ImproveSignatureDocuments.GetAll().Where(l => l.UpdateDate.Value.AddDays(-signatureUpdateDate) <= Calendar.Today &&
                                                                             l.Status == HRPro.HRCommons.ImproveSignatureDocument.Status.Active);
      Logger.DebugFormat("ImproveDocumentSignaturesToCAdESXL: Поиск записей справочника ImproveSignatureDocuments с подписями за последние {0} дней. Найдено записей: {1}.", signatureUpdateDate, docList.Count());
      foreach (var item in docList)
      {
        if(Locks.TryLock(item))
        {
          if (item.UpdateDate != null)
          {
            item.Status = HRPro.HRCommons.ImproveSignatureDocument.Status.InProcessing;
            item.Save();
            var asyncRightsHandler = NNCommons.AsyncHandlers.ImprovementSignatureToCAdESXL.Create();
            asyncRightsHandler.entryId = item.Id;
            asyncRightsHandler.ExecuteAsync();
            
            Logger.DebugFormat("ImproveDocumentSignaturesToCAdESXL: Поставлен в очередь АО для обогащения подписи для записи ImproveSignatureDocument ID: {0}.", item.Id);
          }
          
          Locks.Unlock(item);
        }
        else
        {
          var lockInfo = Locks.GetLockInfo(item);
          Logger.DebugFormat("ImproveDocumentSignaturesToCAdESXL: Запись ImproveSignatureDocument ID: {0} заблокирована. Заблокировал: {1} в {2}.", item.Id, lockInfo.OwnerName, lockInfo.LockTime);
        }
      }
      
      var errorsDocList = HRPro.HRCommons.ImproveSignatureDocuments.GetAll().Where(l => l.Status.Equals(HRPro.HRCommons.ImproveSignatureDocument.Status.Error));
      if (errorsDocList.Any())
      {
        Logger.Debug("ImproveDocumentSignaturesToCAdESXL: Найдены ошибки обогащения подписей.");
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(HRPro.HRCommons.Resources.NotificationTextOnImprovementError, Roles.Administrators);
        var text = string.Empty;
        foreach (var errorItem in errorsDocList)
        {
          var hypertext = string.Format("{0}: {1}", errorItem.Document.Name, Hyperlinks.Get(errorItem));
          text += hypertext + Environment.NewLine;
        }
        notice.ActiveText = text;
        notice.Start();
        
        Logger.DebugFormat("ImproveDocumentSignaturesToCAdESXL: Отправлена задача с информацией об ошибках. ИД Задачи: {0}.", notice.Id);
      }
      
      Logger.Debug("ImproveDocumentSignaturesToCAdESXL: Окончание выполнения ФП.");
    }
    
    /// <summary>
    /// Закрытие записей справочника Виды документов.
    /// </summary>
    public virtual void ClosingDocumentKinds()
    {
      Logger.Debug("ClosingDocumentKinds. Старт фонового процесса \"Закрытие записей справочника Виды документов\".");
      
      var documentKinds = DocumentKinds.GetAll().Where(d => d.Status == DirRX.OverrideSolution.DocumentKind.Status.Active && d.BusinessProcessDirRX == null);
      foreach (var documentKind in documentKinds)
      {
        Transactions.Execute(() =>
                             {
                               try
                               {
                                 documentKind.IsDefault = false;
                                 documentKind.Status = DirRX.OverrideSolution.DocumentKind.Status.Closed;
                                 documentKind.Save();
                                 Logger.DebugFormat("ClosingDocumentKinds. Ид вида документа: {0}. Успешно обработан.", documentKind.Id);
                               }
                               catch (Exception ex)
                               {
                                 Logger.ErrorFormat("ClosingDocumentKinds. Ид вида документа: {0}. Ошибка при сохранении: {1}. StackTrace: {2}.", documentKind.Id, ex.Message, ex.StackTrace);
                               }
                             });
      }
      
      Logger.Debug("ClosingDocumentKinds. Конец фонового процесса \"Закрытие записей справочника Виды документов\".");
    }
  }
}