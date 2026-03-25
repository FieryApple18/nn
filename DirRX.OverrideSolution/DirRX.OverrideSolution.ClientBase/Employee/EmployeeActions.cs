using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Employee;
using DirRX.NNIntegration;

namespace DirRX.OverrideSolution.Client
{
  partial class EmployeeActions
  {

    public virtual void ShowDocumentsVisabilityLimitationDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var records = DirRX.NNCommons.PublicFunctions.DocumentsVisabilityLimitation.Remote.ActionGetDocumentsVisabilityLimitations(_obj);
      records.ShowModal();
    }

    public virtual bool CanShowDocumentsVisabilityLimitationDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return DirRX.NNCommons.DocumentsVisabilityLimitations.AccessRights.CanRead(Users.Current) ||
        DirRX.NNCommons.DocumentsVisabilityLimitations.AccessRights.CanCreate(Users.Current);
    }

    public override void ShowLinkedDocsHRPro(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var employeeDocuments = Functions.Employee.Remote.GetEmployeeDocuments(_obj);
      employeeDocuments.ShowModal();
    }

    public override bool CanShowLinkedDocsHRPro(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanShowLinkedDocsHRPro(e);
    }

    public override void CreateESSUserDirRX(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if(_obj.State.IsChanged)
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.SaveBeforeCreateESSUser);
        return;
      }
      
      var mainEmployee = EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(_obj.Person.Id, null);
      if (mainEmployee != null && ((mainEmployee.LockedByAsync.HasValue && mainEmployee.LockedByAsync.Value) || Locks.GetLockInfo(mainEmployee).IsLockedByOther))
      {
        e.AddError(EssPlatform.Resources.EmployeeCardIsLockedFormat(mainEmployee.Name));
        return;
      }
      
      if (mainEmployee != null && _obj.Id != mainEmployee.Id)
      {
        e.AddWarning(DirRX.EssPlatformSolution.Employees.Resources.OpenTheMainEmployeeCardToConnectToESS, _obj.Info.Actions.ShowMainEmployeeSungero);
        return;
      }
      
      // Отправить приглашение сотруднику можно только при закрытой карточке настроек личного кабинета.
      
      var canCreateEssUser = false;
      var businessUnit = _obj.Department.BusinessUnit;
      if (businessUnit != null && businessUnit.Status.Value == Sungero.Company.BusinessUnit.Status.Active)
      {
        var businessUnitO = EssPlatformSolution.BusinessUnits.As(businessUnit);
        canCreateEssUser = businessUnitO.UseESSDirRX.HasValue && businessUnitO.UseESSDirRX.Value;
      }
      if (!canCreateEssUser)
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.BusinessUnitOfEmployeeIsNotUseEssError);
        return;
      }
      // Проверить, что указан Личный телефон.
      if (string.IsNullOrEmpty(_obj.PersonalPhoneDirRX))
      {
        e.AddError(DirRX.EssPlatformSolution.Employees.Resources.PersonalPhoneRequired);
        return;
      }
      
      var availableServicesMessage = EssPlatform.PublicFunctions.Module.Remote.CheckAvailableServices();
      if (!string.IsNullOrEmpty(availableServicesMessage))
      {
        e.AddError(availableServicesMessage);
        return;
      }
      
      var employeeErrors = SignPlatform.PublicFunctions.Module.Remote.ValidateEmployeeRequiredFields(_obj);
      var personErrors = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.Remote.ValidatePersonRequiredFields(_obj.Person);
      
      if (employeeErrors.Any() || personErrors.Any())
      {
        employeeErrors.ForEach(e.AddError);
        personErrors.ForEach(error => e.AddError(error, _obj.Info.Actions.ShowPerson));
        return;
      }
      
      var blockingErrors = DirRX.OverrideSolution.PublicFunctions.Employee.IsItPossibleToChangeEntities(_obj, true);
      if (blockingErrors.Any())
      {
        foreach (var error in blockingErrors)
          e.AddError(error);
        return;
      }
      
      try
      {
        if (string.IsNullOrEmpty(_obj.PersonnelNumber))
          e.AddError(OverrideSolution.Employees.Resources.PersonnelNumberEmptyError);
        if (string.IsNullOrEmpty(_obj.PersonalPhoneDirRX))
          e.AddError(OverrideSolution.Employees.Resources.PersonnelPhoneNumberEmptyError);

        var structure = NNIntegration.Structures.Module.CreateAccountStruct.Create();
        structure.Employee = _obj.PersonnelNumber;
        structure.Login = _obj.PersonalPhoneDirRX;
        structure.Type = NNIntegration.PublicConstants.Module.CreateAccount.AccountType.Kedo;
        structure.IsKEDO = DirRX.OverrideSolution.PublicFunctions.Employee.Remote.CheckConsentSigned(_obj, false);
        structure.Provider = NNIntegration.PublicConstants.Module.CreateAccount.Providers.Keycloak;
        
        var errors = NNIntegration.PublicFunctions.Module.KedoAccountProcessing(structure, _obj);
        if (!string.IsNullOrEmpty(errors.ErrorText))
          e.AddError(errors.ErrorText);
        
        if (DirRX.OverrideSolution.PublicFunctions.Employee.Remote.HasActiveCertificateIssueTasks(_obj))
        {
          var dialog = Dialogs.CreateTaskDialog(DirRX.EssPlatformSolution.Employees.Resources.CertificateIssueTaskAlreadyExist, MessageType.Question);
          dialog.Buttons.AddOkCancel();
          dialog.Buttons.Default = DialogButtons.Ok;
          
          if (dialog.Show() != DialogButtons.Ok)
            return;
        }
        
        var userConnectionParams = SignPlatform.PublicFunctions.Module.SetActivatingEssUserParameters(_obj);
        if (userConnectionParams.IsSet == true)
        {
          SignPlatform.PublicFunctions.Module.Remote.IssueCertificate(_obj, Users.Current, userConnectionParams);
          Dialogs.NotifyMessage(DirRX.EssPlatformSolution.Employees.Resources.IssueCertificateTaskSent);
        }
      }
      catch (Exception ex)
      {
        throw AppliedCodeException.Create(EssPlatform.Resources.ActivateEssUserErrorFormat(ex.Message));
      }
    }

    public override bool CanCreateESSUserDirRX(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateESSUserDirRX(e);
    }

    public override void CreateCertificateIssueTaskDirRx(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var mainEmployee = EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(_obj.Person.Id, null);
      
      var isCustomCertificateIssueProcess = NNCommons.PublicFunctions.Module.Remote.GetBoolValue(NNCommons.PublicConstants.Module.ConstantsSetting.CertificateIssueProcess.Guid).Value;
      var startCertificateIssueTaskResult = isCustomCertificateIssueProcess ? DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.StartCertificateIssueTask(mainEmployee, null) :
        DirRX.SignPlatform.PublicFunctions.Module.StartCertificateIssueTask(mainEmployee, null);
      if (startCertificateIssueTaskResult.IsStarted)
        Dialogs.NotifyMessage(DirRX.EssPlatformSolution.Employees.Resources.IssueCertificateTaskSent);
      else if (string.IsNullOrEmpty(startCertificateIssueTaskResult.StartTaskError) && !startCertificateIssueTaskResult.DataErrorList.Any())
        return;
      else
      {
        if (startCertificateIssueTaskResult.DataErrorList.Any())
        {
          foreach (var error in startCertificateIssueTaskResult.DataErrorList)
          {
            if (error.ShowPersonAction)
              e.AddError(error.Error, _obj.Info.Actions.ShowPerson);
            else
              e.AddError(error.Error);
          }
          return;
        }
        if (!string.IsNullOrEmpty(startCertificateIssueTaskResult.StartTaskError))
          Dialogs.CreateTaskDialog(DirRX.EssPlatformSolution.Employees.Resources.BeforeCertificateIssueTaskErrorFormat(startCertificateIssueTaskResult.StartTaskError), MessageType.Error).Show();
      }
    }

    public override bool CanCreateCertificateIssueTaskDirRx(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateCertificateIssueTaskDirRx(e);
    }

  }
}