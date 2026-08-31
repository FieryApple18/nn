using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNOtherProcesses.Client
{
  public partial class ModuleFunctions
  {
    #region Функции для обложки модуля
    
    /// <summary>
    /// Создать заявку на работу в ВД/ПД/СР.
    /// </summary>
    public void CreateWeekendWorkStatement()
    {
     Functions.WeekendWorkStatementDocument.Remote.CreateWeekendWorkStatement().Show();
    }
    
    /// <summary>
    /// Открыть папку "Дополнительные соглашения к трудовым договорам".
    /// </summary>
    [LocalizeFunction("ShowAdditionalAgreementListFuncName", "ShowAdditionalAgreementListFuncDesc")]
    public void ShowAdditionalAgreementList()
    {
      SpecialFolders.AdditionalAgreements.Items.Show();
    }
    
    /// <summary>
    /// Сформировать отчет "Сотрудники, не подписавшие Дополнительные соглашения к трудовому договору".
    /// </summary>
    [LocalizeFunction("CreateNotSignedAddtionalAgreementReportFuncName", "CreateNotSignedAddtionalAgreementReportFuncDesc")]
    public void CreateNotSignedAddtionalAgreementReport()
    {
      var currentEmployee = DirRX.OverrideSolution.Employees.Current;
      if (currentEmployee == null)
      {
        Dialogs.ShowMessage(DirRX.OverrideSolution.Module.HRDocFlow.Resources.UserIsSystemErrorMessage, MessageType.Error);
        return;
      }
      
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(Constants.Module.DocumentKind.AdditionalAgreementKind);
      if (docKind == null)
      {
        Dialogs.ShowMessage(DirRX.NNOtherProcesses.Resources.NotSignedAddtionalAgreementReportDocKindNotFound, MessageType.Error);
        return;
      }

      var report = Reports.GetNotSignedAdditionalAgreementReport();
      
      report.EmployeeId = currentEmployee.Id;
      report.DocKindId = docKind.Id;
      
      Logger.DebugFormat("CreateNotSignedAddtionalAgreementReport: ИД сотрудника: {0}, ИД вида документа: {1}", currentEmployee.Id, docKind.Id);
      
      // Получаем ссылку на текущего пользователя и превращаем ее в шаблон ссылки.
      var link = Hyperlinks.Get(currentEmployee);
      report.HyperlinkTemplate = link.Substring(0, link.IndexOf("?type=") + "?type=".Length);
           
      report.Open();
    }
    
    #endregion

  }
}