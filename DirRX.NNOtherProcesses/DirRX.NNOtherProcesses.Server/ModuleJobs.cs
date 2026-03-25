using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNOtherProcesses.Server
{
  public partial class ModuleJobs
  {

    /// <summary>
    /// ФП по удалению устаревших записей справочника "Замещение ЛК" по истечении срока хранения.
    /// </summary>
    public virtual void CleanSubstitutionPersonalAccountJob()
    {
      var retentionConstant = DirRX.NNCommons.PublicFunctions.Module.GetIntValue(DirRX.NNOtherProcesses.Constants.Module.ConstantsSetting.SubstitutionPersonalAccountRetentionDays.Guid);
      var retentionDays = DirRX.NNOtherProcesses.Constants.Module.ConstantsDefaultValues.SubstitutionPersonalAccountRetentionDays;

      if (retentionConstant != null && retentionConstant.HasValue)
        retentionDays = retentionConstant.Value;

      var borderDate = Calendar.Today.AddDays(-retentionDays);

      Logger.DebugFormat("CleanSubstitutionPersonalAccountJob. Start. RetentionDays = {0}, borderDate = {1}.", retentionDays, borderDate);
      
      var deletedCount = 0;
      var errorsCount = 0;
      
      var records = DirRX.NNOtherProcesses.SubstitutionPersonalAccounts.GetAll(r => r.EndDate.HasValue && r.EndDate < borderDate).ToList();
      
      foreach (var record in records)
      {
        try
        {
          Transactions.Execute(() => {
                                 SubstitutionPersonalAccounts.Delete(record);
                               });
          deletedCount++;
        }
        catch (Exception ex)
        {
          errorsCount++;
          Logger.ErrorFormat("CleanSubstitutionPersonalAccountJob. Error while deleting recordId={0}, {1}.", record.Id, ex.Message);
        }
      }

      Logger.DebugFormat("CleanSubstitutionPersonalAccountJob. Finished. Deleted {0}, errors {1}.", deletedCount, errorsCount);
    }

    /// <summary>
    /// ФП по актуализации признака "TOP-10/100" в документах.
    /// </summary>
    public virtual void UpdateTopAttributeInDocumentsJob()
    {
      var logPrefix = "UpdateTopAttributeInDocumentsJob";
      
      Logger.DebugFormat("{0}. Старт фонового процесса \"Актуализация признака \"TOP-10/100\" в документах\".", logPrefix);
      
      // Иключаем из выборки WeekendWorkStatementDocuments, так как в нём признак "TOP-10/100" выставляется вручную пользователем.
      var documents = DirRX.OverrideSolution.DocRelatedToWorkBases.GetAll()
        .Where(d => !NNOtherProcesses.WeekendWorkStatementDocuments.Is(d) &&
               (d.EmployeeDirRX != null && d.TopDirRX != d.EmployeeDirRX.IsNGDDirRX));
      
      if (!documents.Select(d => d.Id).Any())
      {
        Logger.DebugFormat("{0}. Не найдены документы, требующие актуализации. Отправка уведомления администратору.", logPrefix);
        
        var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(Resources.UpdateTopAttributeInDocumentsJobNoticeSubject,
                                                                    Roles.Administrators);
        notice.ActiveText = Resources.UpdateTopAttributeInDocumentsJobNoticeText;
        notice.Start();
      }
      
      foreach (var doc in documents.Take(Constants.Module.UpdateTopAttributeInDocumentsJobBatchSize))
      {
        Logger.DebugFormat("{0}. Начало обработки документа. ИД: {1}.", logPrefix, doc.Id);
        
        var isLocked = false;
        
        try
        {
          isLocked = Locks.TryLock(doc);
          
          if (isLocked)
          {
            doc.TopDirRX = doc.EmployeeDirRX.IsNGDDirRX;
            doc.Save();
            
            Logger.DebugFormat("{0}. Документ обработан. ИД: {1}.", logPrefix, doc.Id);
          }
          else
          {
            Logger.DebugFormat("{0}. Документ заблокирован, обработка документа пропущена. ИД: {1}.", logPrefix, doc.Id);
            continue;
          }
        }
        catch (Exception ex)
        {
          Logger.DebugFormat("{0}. При обработке возникла ошибка, обработка документа пропущена. ИД: {1}. Ошибка: {2}", logPrefix, doc.Id, ex);
        }
        finally
        {
          if (isLocked)
            Locks.Unlock(doc);
        }
      }
      
      Logger.DebugFormat("{0}. Завершение фонового процесса \"Актуализация признака \"TOP-10/100\" в документах\".", logPrefix);
    }

  }
}