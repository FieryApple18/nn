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
  /// ФП по рассылке отчета "Сотрудники, не подписавшие Дополнительные соглашения к трудовому договору".
  /// </summary>
  public virtual void SendNotSignedAdditionalAgreementReportsJob()
  {
    var logPrefix = "SendNotSignedAdditionalAgreementReportsJob";
    
    Logger.DebugFormat("{0}. Старт фонового процесса \"Рассылка отчета \"Сотрудники, не подписавшие Дополнительные соглашения к трудовому договору\".", logPrefix);
    
    var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.AdditionalAgreementKind);
    if (docKind == null)
    {
      Logger.ErrorFormat("{0}. Ошибка. Не найден вид документа c GUID:'{1}'.", logPrefix, Constants.Module.DocumentKind.AdditionalAgreementKind.ToString());
      return;
    }

    // Получаем ссылку на вид документа и превращаем ее в шаблон ссылки.
    var link = Hyperlinks.Get(docKind);
    var hyperlinkTemplate = link.Substring(0, link.IndexOf("?type=") + "?type=".Length);
    
    var reportEmployees = Functions.Module.GetReportEmployees(docKind.Id);
    
    foreach (var reportEmployee in reportEmployees)
    {
      try
      {
        using (var reportStream = Functions.Module.GetNotSignedAdditionalAgreementReport(reportEmployee, docKind.Id, hyperlinkTemplate))
        {
          if (reportStream != null)
            Functions.Module.SendNotSignedAdditionalAgreementReport(reportEmployee, reportStream, DirRX.NNOtherProcesses.Resources.NotSignedAdditionalAgreementReportFileName);
        }
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0}. ИД сотрудника:'{1}'. Ошибка выполнения. Текст ошибки: {2}. StackTrace: {3}.", logPrefix, reportEmployee.Id, ex.Message, ex.StackTrace);
      }
      
    }
    
    Logger.DebugFormat("{0}. Завершение фонового процесса \"Рассылка отчета \"Сотрудники, не подписавшие Дополнительные соглашения к трудовому договору\".", logPrefix);
  }
  
}
}