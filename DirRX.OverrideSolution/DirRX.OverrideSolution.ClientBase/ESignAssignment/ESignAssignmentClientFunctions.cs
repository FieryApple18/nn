using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.ESignAssignment;
using DirRX.OverrideSolution.Structures.HRDocFlow.ESignAssignment;
using Sungero.Content;
using Sungero.Docflow;
using Sungero.Domain.Client;

namespace DirRX.OverrideSolution.Client
{
  partial class ESignAssignmentFunctions
  {

    #region Массовое подписание.

    /// <summary>
    /// Обработать список заданий на подписание.
    /// </summary>
    /// <param name="objs">Список заданий.</param>
    /// <param name="certificate">Сертификат.</param>
    /// <returns>Структура результата обработки.</returns>
    [Public]
    public static IMassApproveSignStruct ProcessSignTasks(List<IESignAssignment> objs,
                                                          ICertificate certificate)
    {
      var signStruct = MassApproveSignStruct.Create();
      signStruct.SuccessIdList = new List<long>();
      signStruct.ErrorIdList = new List<long>();
      
      Logger.DebugFormat("ProcessSignTasks. Ид пользователя: {0}. Начало подписания документов в заданиях. Количество заданий на подписание: {1}. Сертификат для подписания: {2}.",
                         Users.Current.Id, objs.Count(), certificate.Id);
      
      foreach (var assignment in objs)
      {
        var isCompleted = false;
        
        try
        {
          if (ProcessAssignment(assignment, certificate))
            isCompleted = true;
        }
        catch(Exception ex)
        {
          Logger.ErrorFormat("ProcessSignTasks. Ид пользователя: {0}. Ошибка при обработке задания с ИД: {1}. Текст ошибки: {2}. StackTrace: {3}.",
                             Users.Current.Id, assignment.Id, ex.Message, ex.StackTrace);
        }
        finally
        {
          if (isCompleted)
            signStruct.SuccessIdList.Add(assignment.Id);
          else
            signStruct.ErrorIdList.Add(assignment.Id);
        }
      }
      
      Logger.DebugFormat("ProcessSignTasks. Ид пользователя: {0}. Окончание подписания документов в заданиях. Успешно выполнено заданий: {1}. Заданий с ошибками: {2}.",
                         Users.Current.Id, signStruct.SuccessIdList.Count(), signStruct.ErrorIdList.Count());
      
      return signStruct;
    }

    /// <summary>
    /// Обработать задание на подписание.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <param name="certificate">Сертификат.</param>
    /// <returns>True если обработка завершилоась успешно, иначе false.</returns>
    private static bool ProcessAssignment(IESignAssignment assignment, ICertificate certificate)
    {
      // Получить документ из задания, проверяется на наличие и блокировки.
      var document = GetDocument(assignment);
      
      if (document == null)
        return false;

      // Проверка возможности подписания документа выбранным сертификатом.
      if (!CheckSelectedCertificate(certificate, document))
        return false;

      // Подписать документ.
      if (!SignDocument(assignment, certificate, document))
        return false;
      
      // Выполнить задание.
      if (!CompleteAssignment(assignment))
        return false;

      return true;
    }

    /// <summary>
    /// Выполнить задание.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <returns>True если выполнение задание завершилоась успешно, иначе false.</returns>
    private static bool CompleteAssignment(IESignAssignment assignment)
    {
      try
      {
        assignment.Complete(Result.Sign);

        return true;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CompleteTask. Ошибка выполнения задания с ИД: {0}. Выполнялась пользоватлем c ИД: {1}. Текст ошибки: {2} StackTrace: {3}.",
                           assignment.Id, Users.Current.Id, ex.Message, ex.StackTrace);
      }

      return false;
    }

    /// <summary>
    /// Проверить возможность подписание выбранным сертификатом.
    /// </summary>
    /// <param name="certificate">Сертификат.</param>
    /// <param name="document">Документ.</param>
    /// <returns>True если выбранный сертификат подходит для подписания, иначе false.</returns>
    private static bool CheckSelectedCertificate(ICertificate certificate,
                                                 DirRX.OverrideSolution.IOfficialDocument document)
    {
      // Проверить что выбранный сертификат подходит под настроенные права подписи.
      var certificatesByRule = Sungero.Docflow.PublicFunctions.Module.Remote.GetCertificates(document);
      var response = certificatesByRule.Contains(certificate);

      if (!response)
        Logger.ErrorFormat("CheckSelectedCertificate. Ид пользователя: {0}. Выбранный сертификат ИД: {1} не подходит для подписания документа с ИД: {2}. Попытка подписи пользователя с ИД: {3}.",
                           Users.Current.Id, certificate.Id, document.Id, Users.Current.Id);

      return response;
    }

    /// <summary>
    /// Получить документы из задания и проверить на блокировки.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <returns>Документ.</returns>
    private static IOfficialDocument GetDocument(IESignAssignment assignment)
    {
      IOfficialDocument document = null;

      if (!assignment.DocumentGroup.OfficialDocuments.Any())
      {
        Logger.ErrorFormat("GetDocument. Ид пользователя: {0}. Не найден документ в задании с ИД: {1} для подписания или на него отсутствуют права. Выполнялась пользователем c ИД: {2}.",
                           Users.Current.Id, assignment.Id, Users.Current.Id);
        return null;
      }
      
      document = DirRX.OverrideSolution.OfficialDocuments.As(assignment.DocumentGroup.OfficialDocuments.FirstOrDefault());
      // Проверить блокировку до подписания документа.
      var lockMessage = DirRX.OverrideSolution.PublicFunctions.Module.Remote.CheckDocumentLock(document);

      if (!string.IsNullOrEmpty(lockMessage))
      {
        Logger.ErrorFormat("GetDocument. Ид пользователя: {0}. {1}. Задание с ИД: {2}.", Users.Current.Id, lockMessage, assignment.Id);
        return null;
      }

      return document;
    }

    /// <summary>
    /// Проверить, что у всех заданий одинаковый исполнитель.
    /// </summary>
    /// <param name="objs">Задания.</param>
    /// <returns>True исполнитель одинаковый, иначе false.</returns>
    public static bool TaskHaveSinglePerformer(System.Collections.Generic.IEnumerable<IESignAssignment> objs)
    {
      var groupedPerformers = objs.GroupBy(x => x.Performer);
      return groupedPerformers.Count() > 1;
    }

    /// <summary>
    /// Проверить наличие замещений для возможности выполнения заданий.
    /// </summary>
    /// <param name="performers">Исполнители заданий.</param>
    /// <param name="operationName">Имя операции.</param>
    /// <returns>Текст исключения, при отсутствии замещений.</returns>
    /// <remarks>HACK Вариант реализации платформенной проверки "ZZZ не имеет прав на выполнение операции "Согласование" за YYY".</remarks>
    public static string CheckExecuteAccessRight(List<IUser> performers, string operationName)
    {
      var currentUser = Users.Current;
      var otherPerformers = performers.Where(x => !x.Equals(currentUser));
      
      if (!otherPerformers.Any())
        return string.Empty;
      
      var substituteUsers = PublicFunctions.Module.Remote.GetCurrentUserActiveSubstitutions();
      var substitutePerformers = performers.Except(substituteUsers)
        .Select(x => x.Name);
      
      if (substitutePerformers.Any())
        return DirRX.OverrideSolution.ESignAssignments.Resources.NoRightsMessageFormat(
          currentUser.Name, string.Join(",", substitutePerformers.Distinct()), operationName);
      
      return string.Empty;
    }

    /// <summary>
    /// Выбор сертификатов.
    /// </summary>
    /// <returns>Выбранные сертификаты.</returns>
    public static Sungero.CoreEntities.ICertificate SelectSignCertificate()
    {
      var certificates = Functions.Module.Remote.GetCertificates();
      
      // Если сертификатов нет, то не прерываем обработку сразу же, чтобы дать возможность выполнить другие задания. Проверка сертификата будет при выполнении задания.
      if (certificates.Any())
      {
        var selectedCertificate = certificates.Count() > 1 ?
          certificates.ShowSelectCertificate() :
          certificates.FirstOrDefault();
        
        // Сертификат выбираем без учета настроенных прав подписи. Проверка настройки будут выполнятся для каждого задания отдельно.
        if (selectedCertificate != null)
          return selectedCertificate;
        
        Dialogs.ShowMessage(DirRX.OverrideSolution.ESignAssignments.Resources.NotSelectSertMassApprove, MessageType.Warning);
      }
      
      return Certificates.Null;
    }

    /// <summary>
    /// Диалог с результатами массового выполнения задания.
    /// </summary>
    /// <param name="signStruct">Результаты массового подписания.</param>
    public static void ShowMassActionResult(IMassApproveSignStruct signStruct)
    {
      var dialog = Dialogs.CreateInputDialog(DirRX.OverrideSolution.ESignAssignments.Resources.TitleShowMassDialog);
      dialog.Text = DirRX.OverrideSolution.ESignAssignments.Resources.TextShowMassDialogFormat(signStruct.SuccessIdList.Count, signStruct.ErrorIdList.Count);
      dialog.Buttons.AddCustom(DirRX.OverrideSolution.ESignAssignments.Resources.SuccessfulButtonShowMassDialog);
      dialog.Buttons.AddCustom(DirRX.OverrideSolution.ESignAssignments.Resources.NotCompletedButtonShowMassDialog);
      dialog.Width = 200;
      dialog.SetOnButtonClick((a) => ShowCompleteResult(a, signStruct));
      dialog.Show();
    }

    /// <summary>
    /// Обработчик нажатий кнопок.
    /// </summary>
    /// <param name="obj">Событие.</param>
    /// <param name="signStruct">Результаты массового подписания.</param>
    private static void ShowCompleteResult(InputDialogButtonClickEventArgs obj, IMassApproveSignStruct signStruct)
    {
      var successButtonName = ESignAssignments.Resources.SuccessfulButtonShowMassDialog;
      var errorButtonName = ESignAssignments.Resources.NotCompletedButtonShowMassDialog;

      if (obj.Button.Name == successButtonName)
      {
        if (!signStruct.SuccessIdList.Any())
        {
          Dialogs.ShowMessage(DirRX.OverrideSolution.ESignAssignments.Resources.SuccessNotFoundMassApprove, MessageType.Information);
          return;
        }
        
        ShowTasksList(signStruct.SuccessIdList);
      }

      if (obj.Button.Name == errorButtonName)
      {
        if (!signStruct.ErrorIdList.Any())
        {
          Dialogs.ShowMessage(DirRX.OverrideSolution.ESignAssignments.Resources.ErrorNotFoundMassApprove, MessageType.Information);
          return;
        }
        
        ShowTasksList(signStruct.ErrorIdList);
      }
    }

    /// <summary>
    /// Отобразить список заданий.
    /// </summary>
    /// <param name="ids">Ид заданий.</param>
    private static void ShowTasksList(List<long> ids)
    {
      var tasks = DirRX.OverrideSolution.Functions.ESignAssignment.Remote.GetSignAssignments(ids);
      
      if (!tasks.Any())
      {
        Dialogs.ShowMessage(DirRX.OverrideSolution.ESignAssignments.Resources.TaskNotFoundMassApprove, MessageType.Error);
        return;
      }
      
      tasks.Show();
    }

    /// <summary>
    /// Подписать документ.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <param name="certificate">Сертификат.</param>
    /// <param name="document">Документ.</param>
    /// <returns>True если подписание завершилоась успешно, иначе false.</returns>
    private static bool SignDocument(IESignAssignment assignment, ICertificate certificate, DirRX.OverrideSolution.IOfficialDocument document)
    {
      var comment = string.IsNullOrWhiteSpace(assignment.ActiveText) ? string.Empty : assignment.ActiveText;
      
      try
      {
        return Sungero.Core.Signatures.Approve(document.LastVersion, certificate, comment);
      }
      // Копия кода catch из функции EndorseDocument - клиентская функция модуля Docflow. Версия прикладной 4.1.24.
      catch (CommonLibrary.Exceptions.PlatformException ex)
      {
        Logger.ErrorFormat("SignDocument. Ид пользователя: {0}. Ошибка подписания документа с ИД: {1}. Сертификат ИД: {2}. Текст ошибки: {3} StackTrace: {4}",
                           Users.Current.Id, document.Id, certificate.Id, ex.Message, ex.StackTrace);
      }

      return false;
    }

    #endregion

  }
}