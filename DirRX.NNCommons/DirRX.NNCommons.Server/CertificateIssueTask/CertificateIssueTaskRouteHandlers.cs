using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.NNCommons.CertificateIssueTask;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DirRX.OverrideSolution.Module.SignPlatform;

namespace DirRX.NNCommons.Server
{
  partial class CertificateIssueTaskRouteHandlers
  {

    public virtual void StartAssignment12(DirRX.NNCommons.ICertificateIssueErrorAssignment assignment, DirRX.NNCommons.Server.CertificateIssueErrorAssignmentArguments e)
    {
      // Отправить сообщение о новом задании.
      EssPlatform.PublicFunctions.Module.SendNewNotification(Sungero.Company.Employees.As(assignment.Performer), false, DirRX.EssPlatform.PublicConstants.Module.TargetNameSpaces.EssNameSpace,
                                                             EssPlatform.PublicConstants.Module.SelfOfficeObjectTypes.WorkItems,
                                                             EssPlatform.PublicConstants.Module.ObjectCardTypeNames.CertificateIssueErrorAssignment, assignment.Id.ToString(), true);
    }

    public virtual void StartBlock12(DirRX.NNCommons.Server.CertificateIssueErrorAssignmentArguments e)
    {
      if (string.IsNullOrEmpty(_obj.ErrorAssignmentSubject))
      {
        var subject = DirRX.SignPlatform.Resources.CertificateIssueErrorAssignmentSubjectFormat(_obj.RequestId, _obj.Employee).ToString();
        var subjectPropertyLength = CertificateIssueErrorAssignments.Info.Properties.Subject.Length;
        e.Block.Subject = DirRX.SignPlatform.PublicFunctions.Module.GetLimitStringWithEllipsis(subject, subjectPropertyLength);
      }
      else
        e.Block.Subject = _obj.ErrorAssignmentSubject;
      var adminElEmployeeInteractionSystem = Roles.GetAll(r => r.Name == DirRX.EssPlatform.Resources.RoleName_AdminElEmployeeInteractionSystem).Single();
      e.Block.Performers.Add(adminElEmployeeInteractionSystem);
    }

    public virtual void Script25Execute()
    {
      if (_obj.MonitoringTask != null)
      {
        _obj.IsCompleting = true;
        _obj.MonitoringTask.Blocks.ExecuteAllMonitoringBlocks();
      }
    }

    public virtual void Script24Execute()
    {
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        requestEntry.Executor = SignPlatform.CertificateRequest.Executor.TaskScheme;
        requestEntry.Save();
      }
      
      DirRX.SignPlatform.PublicFunctions.Module.AfterIssueCertificate(_obj.Employee, _obj.Author, _obj.ProviderType.ToString());
    }

    public virtual bool Decision23Result()
    {
      if(_obj.RequestId.HasValue)
      {
        var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
        if (requestEntry != null)
        {
          if(requestEntry.CertificateID.HasValue && Sungero.CoreEntities.Certificates.GetAll(x => x.Id == requestEntry.CertificateID.Value).FirstOrDefault() != null)
            return true;
          else
            _obj.ErrorAssignmentSubject = DirRX.SignPlatform.CertificateIssueTasks.Resources.SaveCertificateErrorFormat(_obj.RequestId.Value, _obj.Employee);
        }
        else
          _obj.ErrorAssignmentSubject = DirRX.SignPlatform.CertificateIssueTasks.Resources.GetRequestByIdErrorFormat(_obj.RequestId.Value, _obj.Employee);
      }
      else
        _obj.ErrorAssignmentSubject = DirRX.SignPlatform.CertificateIssueTasks.Resources.GetRequestErrorFormat(_obj.Employee);
      
      return false;
    }

    /// <summary>
    /// Медленный мониторинг сохранения облачного сертификата в справочник "Цифровые сертификаты"
    /// Предполагается, что запрос на выпуск сертификата дойдет до блока в состоянии:
    ///   - (CertCreated) - тогда задача остается на этом же блоке
    ///   - (CertRegistered, Error) - тогда задача двигается дальше
    /// </summary>
    public virtual bool Monitoring21Result()
    {
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertCreated)
          return false;  // остаться на месте
        else if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertRegistered ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.Error)
          return true; // двинуть задачу дальше
        else
          // неожиданное для блока текущее значение статуса запроса на выпуск сертификата
          throw AppliedCodeException.Create(
            string.Format(
              "CertificateIssue. Monitoring36Result. Unexpected requestEntry.IssueStatus={0} for block RequestId={1} ProviderId={2} in Monitoring36",
              requestEntry.IssueStatus,
              requestEntry.RequestId,
              requestEntry.ProviderId));
      }
      // Особый случай выхода - не найдена запись справочника CertificateRequest
      // Будет обработано в задании CertificateIssueErrorAssignment
      return true;
    }

    public virtual void StartBlock21(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      e.Block.Period = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskMonitoringBackgroundProcessKey).ToString()));
      e.Block.RelativeDeadline = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskTimeoutBackgroundProcessKey).ToString()));
      
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        requestEntry.Executor = SignPlatform.CertificateRequest.Executor.BckgrndProcess;
        requestEntry.Save();
      }
    }

    /// <summary>
    /// Быстрый мониторинг сохранения облачного сертификата в справочник "Цифровые сертификаты"
    /// Предполагается, что запрос на выпуск сертификата дойдет до блока в состоянии:
    /// 1. (CertCreated). Тогда в ходе выполнения блока
    ///   - задача останется на текущем блоке
    ///   - либо перейдет в Error или CertRegistered и тогда задачу надо двинуть дальше
    /// 2. (CertRegistered, Error). Тогда в ходе выполнения блока
    ///   - и тогда задачу надо двинуть дальше
    /// </summary>
    public virtual bool Monitoring20Result()
    {
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertCreated)
        {
          DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.SaveCertificate(requestEntry);
          if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertCreated)
            return false; // остаться на блоке
          else if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertRegistered ||
                   requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.Error)
            return true; // двинуть задачу дальше
          else
            // неожиданное для блока текущее значение статуса запроса на выпуск сертификата
            throw AppliedCodeException.Create(
              string.Format(
                "CertificateIssue. Unexpected requestEntry.IssueStatus={0} for block RequestId={1} ProviderId={2} in Monitoring35-1",
                requestEntry.IssueStatus,
                requestEntry.RequestId,
                requestEntry.ProviderId));
        }
        else if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertRegistered ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.Error)
          return true; // двинуть задачу дальше
        else
          // неожиданное для блока текущее значение статуса запроса на выпуск сертификата
          throw AppliedCodeException.Create(
            string.Format(
              "CertificateIssue. Unexpected requestEntry.IssueStatus={0} for block RequestId={1} ProviderId={2} in Monitoring35-2",
              requestEntry.IssueStatus,
              requestEntry.RequestId,
              requestEntry.ProviderId));
      }
      // Особый случай выхода - не найдена запись справочника CertificateRequest
      // Будет обработано в задании CertificateIssueErrorAssignment
      return true;
    }

    public virtual void StartBlock20(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      e.Block.Period = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskMonitoringKey).ToString()));
      e.Block.RelativeDeadline = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskTimeoutKey).ToString()));
      
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        requestEntry.Executor = SignPlatform.CertificateRequest.Executor.TaskScheme;
        requestEntry.Save();
      }
    }

    public virtual bool Decision19Result()
    {
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        // Ожидается, что до блока заявка дойдет либо в состоянии CertCreated, либо Error
        if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertCreated)
          return true;
        else if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.Error)
          return false;
        else
          // неожиданное для блока текущее значение статуса заявки
          throw AppliedCodeException.Create(
            string.Format(
              "CertificateIssue. Unexpected requestEntry.IssueStatus={0} for block RequestId={1} ProviderId={2} in Decision24",
              requestEntry.IssueStatus,
              requestEntry.RequestId,
              requestEntry.ProviderId));
      }
      // Особый случай выхода - не найдена запись справочника CertificateRequest
      // Будет обработано в задании CertificateIssueErrorAssignment
      return false;
    }

    /// <summary>
    /// Медленный мониторинг проверки документов
    /// Предполагается, что запрос на выпуск сертификата дойдет до блока в состоянии:
    ///   - IssVerification или DocVerification - тогда задача остается на этом же блоке
    ///   - (CertCreated, CertRegistered, Error) - тогда задача двигается дальше
    /// </summary>
    public virtual bool Monitoring17Result()
    {
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.IssVerification ||
            requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.InProgress)
          return false;  // остаться на месте
        else if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertCreated ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertRegistered ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.Error)
          return true; // двинуть задачу дальше
        else
          // неожиданное для блока текущее значение статуса запроса на выпуск сертификата
          throw AppliedCodeException.Create(
            string.Format(
              "CertificateIssue. Monitoring8Result. Unexpected requestEntry.IssueStatus={0} for block RequestId={1} ProviderId={2} in Monitoring8",
              requestEntry.IssueStatus,
              requestEntry.RequestId,
              requestEntry.ProviderId));
      }
      // Особый случай выхода - не найдена запись справочника CertificateRequest
      // Будет обработано в задании CertificateIssueErrorAssignment
      return true;
    }

    public virtual void StartBlock17(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      e.Block.Period = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskMonitoringBackgroundProcessKey).ToString()));
      e.Block.RelativeDeadline = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskTimeoutBackgroundProcessKey).ToString()));
      
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        requestEntry.Executor = SignPlatform.CertificateRequest.Executor.BckgrndProcess;
        requestEntry.Save();
      }
    }

    public virtual bool Decision16Result()
    {
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.IssVerification ||
            requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.InProgress ||
            requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertCreated ||
            requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertRegistered)
          return true;
        else if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.Error)
          return false;
        else
          // неожиданное для блока текущее значение статуса заявки
          throw AppliedCodeException.Create(
            string.Format(
              "CertificateIssue. Unexpected requestEntry.IssueStatus={0} for block RequestId={1} ProviderId={2} in Decision33",
              requestEntry.IssueStatus,
              requestEntry.RequestId,
              requestEntry.ProviderId));
      }
      // Особый случай выхода - не найдена запись справочника CertificateRequest
      // Будет обработано в задании CertificateIssueErrorAssignment
      return false;
    }

    /// <summary>
    /// Обработать ситуации, когда из медленных мониторингов вышли по таймауту
    /// Нужно изменить статус запроса на выпуск сертификата на Error и указать текст ошибки
    /// </summary>
    public virtual void Script15Execute()
    {
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        requestEntry.Errors = DirRX.SignPlatform.Resources.SignServiceNotAnswer;
        
        if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.DocVerification)
        {
          requestEntry.Errors = SignPlatform.CertificateIssueTasks.Resources.ErrorTimeoutDocVerification;
        }
        else if(requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.InProgress)
        {
          requestEntry.Errors = SignPlatform.CertificateIssueTasks.Resources.ErrorTimeoutCertificateIssued;
        }
        else if(requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.NeedConfirmEsia)
        {
          requestEntry.Errors = SignPlatform.CertificateIssueTasks.Resources.ErrorTimeoutSignStatementESIA;
        }
        requestEntry.IssueStatus = SignPlatform.CertificateRequest.IssueStatus.Error;
        requestEntry.Executor = SignPlatform.CertificateRequest.Executor.TaskScheme;
        requestEntry.Save();
      }
    }

    /// <summary>
    /// Медленный мониторинг ожидания подписания в ЕСИА
    /// Предполагается, что запрос на выпуск сертификата дойдет до блока в состоянии:
    /// 1. NeedConfirmEsia. Задача останется на текущем блоке
    /// 2. (IssVerification, InProgress, CertCreated, CertRegistered, Error). Тогда в ходе выполнения блока
    ///   - и тогда задача останется на текущем блоке
    ///   - либо перейдет в Error или CertCreated и тогда задачу надо двинуть дальше
    /// </summary>
    public virtual bool Monitoring13Result()
    {
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.NeedConfirmEsia)
          return false;
        else if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.IssVerification ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.InProgress ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertCreated ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertRegistered ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.Error)
          return true;
        else
          // неожиданное для блока текущее значение статуса запроса на выпуск сертификата
          throw AppliedCodeException.Create(string.Format(
            "CertificateIssue. Unexpected requestEntry.IssueStatus={0} for block RequestId={1} ProviderId={2} in Monitoring32",
            requestEntry.IssueStatus,
            requestEntry.RequestId,
            requestEntry.ProviderId));
      }
      // Особый случай выхода - не найдена запись справочника CertificateRequest
      // Будет обработано в задании CertificateIssueErrorAssignment
      return true;
    }

    public virtual void StartBlock13(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      e.Block.Period = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskMonitoringBackgroundProcessKey).ToString()));
      e.Block.RelativeDeadline = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskTimeoutBackgroundProcessKey).ToString()));
      
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        requestEntry.Executor = SignPlatform.CertificateRequest.Executor.BckgrndProcess;
        requestEntry.Save();
      }
    }

    /// <summary>
    /// Быстрый мониторинг ожидания подписания в ЕСИА и выпуска сертификата
    /// Предполагается, что запрос на выпуск сертификата дойдет до блока в состоянии:
    /// 1. (IssVerification, InProgress). Тогда в ходе выполнения блока
    ///   - задача останется на текущем блоке
    ///   - либо перейдет в Error или CertCreated и тогда задачу надо двинуть дальше
    /// 2. (CertCreated, CertRegistered, Error). Тогда в ходе выполнения блока
    ///   - и тогда задачу надо двинуть дальше
    /// </summary>
    public virtual bool Monitoring11Result()
    {
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.IssVerification ||
            requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.InProgress)
        {
          DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.CheckCertificateStatus(requestEntry);
          if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.IssVerification ||
              requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.InProgress)
            return false; // остаться на блоке
          else if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertCreated ||
                   requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.Error)
            return true; // двинуть задачу дальше
          else
            // неожиданное для блока текущее значение статуса запроса на выпуск сертификата
            throw AppliedCodeException.Create(
              string.Format(
                "CertificateIssue. Unexpected requestEntry.IssueStatus={0} for block RequestId={1} ProviderId={2} in Monitoring11",
                requestEntry.IssueStatus,
                requestEntry.RequestId,
                requestEntry.ProviderId));
        }
        else if (requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertCreated ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.CertRegistered ||
                 requestEntry.IssueStatus == SignPlatform.CertificateRequest.IssueStatus.Error)
          return true; // двинуть задачу дальше
        else
          // неожиданное для блока текущее значение статуса запроса на выпуск сертификата
          throw AppliedCodeException.Create(
            string.Format(
              "CertificateIssue. Unexpected requestEntry.IssueStatus={0} for block RequestId={1} ProviderId={2} in Monitoring11",
              requestEntry.IssueStatus,
              requestEntry.RequestId,
              requestEntry.ProviderId));
      }
      // Особый случай выхода - не найдена запись справочника CertificateRequest
      // Будет обработано в задании CertificateIssueErrorAssignment
      return true;
    }

    public virtual void StartBlock11(Sungero.Workflow.Server.Route.MonitoringStartBlockEventArguments e)
    {
      e.Block.Period = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskMonitoringKey).ToString()));
      e.Block.RelativeDeadline = TimeSpan.FromMinutes(double.Parse(Sungero.Docflow.PublicFunctions.Module.GetDocflowParamsValue(DirRX.SignPlatform.PublicConstants.Module.ParamKey.CertificateIssueTaskTimeoutKey).ToString()));

      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry != null)
      {
        requestEntry.Executor = SignPlatform.CertificateRequest.Executor.TaskScheme;
        requestEntry.Save();
      }
    }

    public virtual bool Decision10Result()
    {
      return _obj.IdentificationType == SignPlatform.CertificateIssueTask.IdentificationType.Personal ? true : false;
    }
    
    /// <summary>
    /// Выпуск сертификата.
    /// </summary>
    public virtual void Script9Execute()
    {
      if (!DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.Remote.ExistsCertificateRequest(_obj))
      {
        Logger.DebugFormat(
          "CertificateIssue. Script9. TaskID={0} RequestId={1} ProviderId={2} not found in CertificateRequest",
          _obj.Id,
          _obj.RequestId,
          _obj.ProviderId);
      }
      
      Logger.DebugFormat(
        "CertificateIssue. Script9. TaskID={0} RequestId={1} ProviderId={2} started",
        _obj.Id,
        _obj.RequestId,
        _obj.ProviderId);
      
      // Получить номер телефона для выпуска сертификата для сотрудника.
      var phoneNumber = DirRX.OverrideSolution.PublicFunctions.Employee.GetCertificatePhoneNumber(OverrideSolution.Employees.As(_obj.Employee));
      
      if (_obj.IdentificationType == NNCommons.CertificateIssueTask.IdentificationType.Personal)
      {
        var result = SignPlatform.PublicFunctions.Module.SendCertificateIssueConfirmation(_obj.RequestId.Value, _obj.ProviderId);
        if (result.ResultCode == NNCommons.PublicConstants.Module.ActionResultCodes.Ok)
        {
          return;
        }
        else
          Logger.DebugFormat(
            "CertificateIssue. Script9. TaskID={0} RequestId={1} ProviderId={2} error {3}",
            _obj.Id,
            _obj.RequestId,
            _obj.ProviderId,
            result.Error.Message);
      }
      
      var requestEntry = DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateRequest(_obj);
      if (requestEntry == null)
        Logger.DebugFormat(
          "CertificateIssue. Script9. TaskID={0}. RequestId={1} ProviderId={2} not found in CertificateRequest",
          _obj.Id,
          requestEntry.RequestId,
          requestEntry.ProviderId);
      if (_obj.IdentificationType == SignPlatform.CertificateIssueTask.IdentificationType.Personal)
      {
        // При личной идентификации после успешного подписания заявку на сервисе переходит в статус IssueVerification
        // и у нас надо перевести в это же состояние
        requestEntry.IssueStatus = SignPlatform.CertificateRequest.IssueStatus.IssVerification;
        requestEntry.Save();
      } else if (_obj.IdentificationType == SignPlatform.CertificateIssueTask.IdentificationType.Esia)
      {
        // При идентификации через ЕСИА заявка на сервисе остается в статусе NeedConfirm
        // а у нас надо перевести в состояние NeedConfirmEsia
        requestEntry.IssueStatus = SignPlatform.CertificateRequest.IssueStatus.NeedConfirmEsia;
        requestEntry.Save();
      }
      else
        Logger.DebugFormat(
          "CertificateIssue. Script9. TaskID={0} RequestId={1} ProviderId={2}. Identification type '{3}' not supported for certificates issue",
          _obj.Id,
          requestEntry.RequestId,
          requestEntry.ProviderId,
          _obj.IdentificationType);
    }

    public virtual bool Decision8Result()
    {
      return string.IsNullOrEmpty(_obj.ErrorAssignmentSubject);
    }

    /// <summary>
    /// Создание запроса на выдачу сертификата.
    /// </summary>
    public virtual void Script7Execute()
    {
      var response = DirRX.SignPlatform.PublicFunctions.Module.SendCertificateIssueRequest(_obj.Employee, _obj.ProviderId, _obj.IdentificationType.Value.Value);
      if (!string.IsNullOrEmpty(response.Error))
      {
        var errorSubjectPropertyLength = _obj.Info.Properties.ErrorAssignmentSubject.Length;
        _obj.ErrorAssignmentSubject = DirRX.SignPlatform.PublicFunctions.Module.GetLimitStringWithEllipsis(response.Error, errorSubjectPropertyLength);
        _obj.ActiveText = response.Error;
        _obj.Save();
        return;
      }
      _obj.RequestId = response.RequestId;
      _obj.Save();
      
      var certificateRequestEntry = DirRX.SignPlatform.CertificateRequests.Create();
      certificateRequestEntry.RequestId = _obj.RequestId;
      certificateRequestEntry.TaskId = _obj.Id;
      certificateRequestEntry.CreateDate = Calendar.Now;
      certificateRequestEntry.Employee = _obj.Employee;
      certificateRequestEntry.IssueStatus = SignPlatform.CertificateRequest.IssueStatus.IssVerification;
      certificateRequestEntry.Executor = SignPlatform.CertificateRequest.Executor.TaskScheme;
      certificateRequestEntry.IdentificationType = _obj.IdentificationType;
      var name = DirRX.SignPlatform.Resources.CertificateRequestEntryNameFormat(certificateRequestEntry.Id, certificateRequestEntry.Employee.DisplayValue).ToString();
      var namePropertyLength = certificateRequestEntry.Info.Properties.Name.Length;
      certificateRequestEntry.Name = DirRX.SignPlatform.PublicFunctions.Module.GetLimitStringWithEllipsis(name, namePropertyLength);
      certificateRequestEntry.ProviderId = _obj.ProviderId;
      certificateRequestEntry.ProviderType = _obj.ProviderType.Value.Value;
      certificateRequestEntry.Save();

      _obj.CertificateRequestAttachmentGroup.CertificateRequests.Add(certificateRequestEntry);
      _obj.Save();
    }

    public virtual void Script6Execute()
    {
      var docflowParamsCertificate = Sungero.Docflow.PublicFunctions.Module
        .GetDocflowParamsValue(NNCommons.PublicConstants.Module.ParamKey.GoskeyTechnologicalCertificate)
        .ToString();
      var certificate = !string.IsNullOrEmpty(docflowParamsCertificate)
        ? JsonConvert.DeserializeObject<SignPlatform.Structures.Module.Certificate>(docflowParamsCertificate)
        : JsonConvert.DeserializeObject<SignPlatform.Structures.Module.Certificate>(Constants.CertificateIssueTask.jsonCertificate);
      var mainEmployee = DirRX.EssPlatform.PublicFunctions.Module.Remote.GetPersonMainEmployee(_obj.Employee.Person.Id, null);
      
      var digitalCertificate = Sungero.CoreEntities.Certificates.Create();
      digitalCertificate.Owner = mainEmployee;
      digitalCertificate.Description = DirRX.SignPlatform.CertificateIssueTasks.Resources.GoskeyCertificateDescription;
      digitalCertificate.Subject = mainEmployee.Person.Name;
      digitalCertificate.Issuer = certificate.IssuerCommonName;
      digitalCertificate.NotBefore = certificate.ValidFrom;
      digitalCertificate.NotAfter = certificate.ValidTo;
      digitalCertificate.Thumbprint = certificate.Thumbprint;
      digitalCertificate.X509Certificate = System.Convert.FromBase64String(certificate.Content);
      digitalCertificate.PluginId = DirRX.SignPlatform.PublicConstants.Module.PluginIdOfCloudCertificates;
      
      var providerIdParameter = digitalCertificate.Parameters.AddNew();
      providerIdParameter.Key = DirRX.SignPlatform.PublicConstants.Module.ProviderIdCertificateKeyParameter;
      providerIdParameter.Value = _obj.ProviderId;
      
      var ownerIdParameter = digitalCertificate.Parameters.AddNew();
      ownerIdParameter.Key = DirRX.SignPlatform.PublicConstants.Module.OwnerIdCertificateKeyParameter;
      ownerIdParameter.Value = EssPlatform.PublicFunctions.Module.Remote.GetUidPerson(mainEmployee.Person);
      
      var inilaParameter = digitalCertificate.Parameters.AddNew();
      inilaParameter.Key = DirRX.SignPlatform.PublicConstants.Module.OwnerInilaCertificateKeyParameter;
      inilaParameter.Value = mainEmployee.Person.INILA;
      digitalCertificate.Save();
      
      _obj.IssuedCertificate = digitalCertificate;
      _obj.Save();
      
      Logger.DebugFormat("CertificateIssue. Saved goskey certificate for employeeId={0}",
                         _obj.Employee.Id);
      
      DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.DisablePrevCloudCertificates(_obj.Employee.Person.Id, _obj.ProviderId);
    }

    public virtual bool Decision5Result()
    {
      return string.IsNullOrEmpty(_obj.ErrorAssignmentSubject);
    }

    public virtual void Script4Execute()
    {
      if (_obj.Employee.PersonalAccountStatusDirRX == DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteAccepted
          || _obj.Employee.PersonalAccountStatusDirRX == DirRX.EssPlatformSolution.Employee.PersonalAccountStatusDirRX.InviteSent)
      {
        try
        {
          DirRX.EssPlatform.PublicFunctions.Module.Remote.ModifyEssUserAccount(
            _obj.Employee.Person,
            EssPlatform.PublicConstants.Module.UserIdentityAttribute.Inila,
            _obj.Employee.Person.INILA);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("DirRX.SignPlatform.Server.CertificateIssueTaskRouteHandlers: could not update EssUser. Employee id: {0}. Error: {1}.", _obj.Employee, ex.Message);
          _obj.ErrorAssignmentSubject = DirRX.EssPlatform.Resources.UpdateEssUserError;
          _obj.Save();
        }
      }
    }

    public virtual bool Decision3Result()
    {
      return _obj.ProviderType == ProviderType.Goskey;
    }

  }
}