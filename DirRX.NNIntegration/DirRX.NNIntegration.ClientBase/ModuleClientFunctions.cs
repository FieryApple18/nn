using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNIntegration.Client
{
  public partial class ModuleFunctions
  {
    /// <summary>
    /// Сформировать отчет об отправке Push-уведомлений в Супернику.
    /// </summary>
    [LocalizeFunction("SupernikaPushNotificationReportFuncName", "")]
    public void CreateSupernikaPushNotificationReport()
    {
      var dialog = Dialogs.CreateInputDialog(Resources.SupernikaPushNotificationReportTitle);
      var personalPhone = dialog.AddString(Resources.SupernikaPushNotificationReportPersonalPhone, true);
      var beginDate = dialog.AddDate(Resources.SupernikaPushNotificationReportBeginDate, true);
      var endDate = dialog.AddDate(Resources.SupernikaPushNotificationReportEndDate, true);
      
      dialog.SetOnRefresh((er) => {
                            if (!string.IsNullOrWhiteSpace(personalPhone.Value))
                            {
                              if (!EssPlatform.PublicFunctions.Module.PhoneNumberLengthIsValid(personalPhone.Value, false))
                                er.AddError(DirRX.EssPlatformSolution.Employees.Resources.PersonalPhoneNumbersCountError);
                              else
                                personalPhone.Value = EssPlatform.PublicFunctions.Module.PhoneNumberToUniversalFormat(personalPhone.Value);
                            }
                            if (beginDate.Value.HasValue && endDate.Value.HasValue)
                            {
                              if (endDate.Value.Value < beginDate.Value.Value)
                                er.AddError(DirRX.NNCommons.Resources.ReportSLADialogEndDateError);
                              if (beginDate.Value.Value.AddMonths(NNCommons.PublicConstants.Module.ReportSLA.DateDiffInMonths) < endDate.Value.Value)
                                er.AddError(DirRX.NNCommons.Resources.ReportSLADialogPeriodError);
                            }
                          });
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        var report = Reports.GetSupernikaPushNotificationReport();
        report.BeginDate = beginDate.Value;
        report.EndDate = endDate.Value.Value.EndOfDay();
        report.PersonalPhone = personalPhone.Value;
        report.Open();
      }
    }
  }
}