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