using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.SignPlatform.Client
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Стартовать задачу на выпуск сертификата электронной подписи.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="fromTask">Задачa, из которой запускается запрос.</param>
    /// <returns>Структура с результатами старта задачи.</returns>
    [Public]
    public virtual DirRX.SignPlatform.Structures.Module.IStartCertificateIssueTaskResult StartCertificateIssueTask(EssPlatformSolution.IEmployee employee, NNCommons.ICertificateIssueTask fromTask)
    {
      var startTaskResult = DirRX.SignPlatform.Structures.Module.StartCertificateIssueTaskResult.Create();
      startTaskResult.IsStarted = false;
      startTaskResult.StartTaskError = string.Empty;
      startTaskResult.DataErrorList = new List<DirRX.SignPlatform.Structures.Module.IStartCertificateIssueTaskError>();
      
      if (NNCommons.PublicFunctions.Module.Remote.EmployeeHasActiveCertificateIssueTask(employee, fromTask))
      {
        var dialog = Dialogs.CreateTaskDialog(DirRX.EssPlatformSolution.Employees.Resources.CertificateIssueTaskAlreadyExist, MessageType.Question);
        dialog.Buttons.AddOkCancel();
        dialog.Buttons.Default = DialogButtons.Ok;
        
        if (dialog.Show() != DialogButtons.Ok)
        {
          return startTaskResult;
        }
      }
      
      if (Functions.Module.Remote.EmployeeHasCloudCertificate(employee))
      {
        var dialog = Dialogs.CreateTaskDialog(DirRX.EssPlatformSolution.Employees.Resources.CloudCertificateAlreadyExist, MessageType.Question);
        dialog.Buttons.AddOkCancel();
        dialog.Buttons.Default = DialogButtons.Ok;
        
        if (dialog.Show() != DialogButtons.Ok)
        {
          return startTaskResult;
        }
      }
      
      // Если второй фактор или "email" или "по умолчанию" и в общих настройках email.
      var settings = DirRX.EssPlatform.PublicFunctions.EssSetting.GetSettings();
      if (((employee.ConfirmationTypeDirRX == DirRX.EssPlatformSolution.Employee.ConfirmationTypeDirRX.DefaultValue && 
            settings.ConfirmationType == DirRX.EssPlatform.EssSetting.ConfirmationType.Email) ||
            employee.ConfirmationTypeDirRX == DirRX.EssPlatformSolution.Employee.ConfirmationTypeDirRX.Email) &&
          (string.IsNullOrEmpty(employee.Email) && string.IsNullOrEmpty(employee.MessagesEmailDirRX)))
      {
        var error = DirRX.SignPlatform.Structures.Module.StartCertificateIssueTaskError.Create();
        error.Error = DirRX.EssPlatformSolution.Employees.Resources.EmptyPersonalEMailOrWorkEmailError;
        error.ShowPersonAction = false;
        startTaskResult.DataErrorList.Add(error);
        return startTaskResult;
      }
      
      var employeeErrors = DirRX.SignPlatform.PublicFunctions.Module.Remote.ValidateEmployeeRequiredFields(employee);
      foreach (var employeeError in employeeErrors)
      {
        var error = DirRX.SignPlatform.Structures.Module.StartCertificateIssueTaskError.Create();
        error.Error = employeeError;
        error.ShowPersonAction = false;
        startTaskResult.DataErrorList.Add(error);
      }
      var personErrors = SignPlatform.PublicFunctions.Module.Remote.ValidatePersonRequiredFields(employee.Person);
      foreach (var personError in personErrors)
      {
        var error = DirRX.SignPlatform.Structures.Module.StartCertificateIssueTaskError.Create();
        error.Error = personError;
        error.ShowPersonAction = true;
        startTaskResult.DataErrorList.Add(error);
      }
      if (startTaskResult.DataErrorList.Any())
        return startTaskResult;
      
      var userConnectionParams = DirRX.SignPlatform.PublicFunctions.Module.SetActivatingEssUserParameters(employee);
      if (userConnectionParams.IsSet == false)
        return startTaskResult;
      
      DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.Remote.IssueCertificate(employee, Users.Current, userConnectionParams, fromTask);
      startTaskResult.IsStarted = true;
      
      return startTaskResult;
    }

  }
}