using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Sungero.Core;
using System.IO;
using DirRX.NNIntegration;
using DirRX.NNIntegration.Structures.Module;

namespace DirRX.NNIntegration.Isolated.SendOutgoingMessage
{
  public partial class IsolatedFunctions
  {
    /// <summary>
    /// Выполнить обращение к внешней системе.
    /// </summary>
    /// <param name="messageText">Текст сообщения.</param>
    /// <param name="actionName">Имя действия.</param>
    /// <param name="address">Адрес внешней системы.</param>
    /// <param name="port">Порт.</param>
    /// <param name="login">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    /// <param name="result">Структура - Код результата, Текст ответа, Текст ошибки.</param>
    /// <param name="needAuthorizationHeader">Признак необходимости передачи заголовка Authorization.</param>
    /// <returns>Структура - Код результата, Текст ответа, Текст ошибки.</returns>
    /// <remarks>Функция перенесена в изолированные области из-за использования сторонних библиотек и для обхода ограничения по созданию объектов через new.</remarks>
    [Public]
    public virtual Structures.Module.IResultSendingMessage ExecuteRequestToExternalSystem(string messageText, string actionName, string address, string port, string login, string password,
                                                                                          Structures.Module.IResultSendingMessage result, bool needAuthorizationHeader)
    {
      Logger.DebugFormat("ExecuteRequestToExternalSystem. MessageText: {0}", messageText);
      var request = GetWebRequest(actionName, address, port, login, password, "application/json", needAuthorizationHeader);
      
      // Запись тела запроса.
      using (Stream stream = request.GetRequestStream())
      {
        var resultByteArr = System.Text.Encoding.UTF8.GetBytes(messageText);
        stream.Write(resultByteArr, 0, resultByteArr.Length);
      }
      
      return SendDataToExternalSystem(request, result);
    }
    
    /// <summary>
    /// Отправить документа на проштамповку внешней системе.
    /// </summary>
    /// <param name="actionName">Имя действия.</param>
    /// <param name="address">Адрес внешней системы.</param>
    /// <param name="port">Порт.</param>
    /// <param name="login">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    /// <param name="versionBody">Тело версии документа в PDF.</param>
    /// <param name="model">Структура передачи документа на проштамповку.</param>
    /// <param name="docName">Имя документа.</param>
    /// <param name="result">Структура - Код результата, Текст ответа, Текст ошибки.</param>
    /// <param name="needAuthorizationHeader">Признак необходимости передачи заголовка Authorization.</param>
    /// <returns>Структура - Код результата, Текст ответа, Текст ошибки.</returns>
    /// <remarks>Функция перенесена в изолированные области из-за использования сторонних библиотек и для обхода ограничения по созданию объектов через new.</remarks>
    [Public]
    public virtual Structures.Module.IResultSendingMessage SendDocumentStampingToExternalSystem(string actionName, string address, string port, string login, string password,
                                                                                                Stream versionBody, ISendDocumentToStampingModel model, string docName,
                                                                                                Structures.Module.IResultSendingMessage result, bool needAuthorizationHeader)
    {
      var request = GetWebRequest(actionName, address, port, login, password, "multipart/form-data", needAuthorizationHeader);
      
      // HACK: Seleznyov_DV. В запросе для сервиса проштамповки важно соблюдать порядок содержимого.
      // Контент с файлом "form-data" должен передаваться до контента c информацией о ПЭП "attributes".
      using (var content = new MultipartFormDataContent())
      {
        var requestIdContent = new StringContent(model.request_id);
        content.Add(requestIdContent, "request_id");
        
        var templateIdContent = new StringContent(model.template_id);
        content.Add(templateIdContent, "template_id");
        
        var fileContent = new StreamContent(versionBody);
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
          Name = "file",
          FileName = docName
        };
        content.Add(fileContent, "file");
        
        var attributesContent = new StringContent(model.attributes, System.Text.Encoding.UTF8, "text/xml");
        content.Add(attributesContent, "attributes");
        
        // Запись тела запроса.
        using (Stream requestStream = request.GetRequestStream())
        {
          content.CopyToAsync(requestStream).Wait();
        }
        request.ContentType = content.Headers.ContentType.ToString();
      }

      return SendDataToExternalSystem(request, result);
    }
    
    /// <summary>
    /// Заполнить данные для подключения к внешней системе.
    /// </summary>
    /// <param name="actionName">Имя действия.</param>
    /// <param name="address">Адрес внешней системы.</param>
    /// <param name="port">Порт.</param>
    /// <param name="login">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    /// <param name="contentType">Тип контента.</param>
    /// <param name="needAuthorizationHeader">Признак необходимости передачи заголовка Authorization.</param>
    /// <returns>Подключение к внешней системе.</returns>
    private HttpWebRequest GetWebRequest(string actionName, string address, string port, string login, string password, string contentType, bool needAuthorizationHeader)
    {
      var url = string.Format("{0}:{1}/{2}", address, port, actionName);
      Logger.DebugFormat("GetWebRequest. URL: {0}", url);
      // Формирование заголовков.
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
      request.Method = "POST";
      request.ContentType = contentType;
      
      if (needAuthorizationHeader == true)
      {
        var authenticationString = string.Format("{0}: {1}", login, password);
        var bytes = System.Text.Encoding.UTF8.GetBytes(authenticationString);
        authenticationString = System.Convert.ToBase64String(bytes);
        request.Headers.Add("Authorization", string.Format("{0} {1}", "Basic", authenticationString));
      }
      
      var credential = new NetworkCredential(login, password);
      CredentialCache credentialCache = new CredentialCache();
      credentialCache.Add(new Uri(url), "Basic", credential);
      request.Credentials = credentialCache;
      return request;
    }
    
    /// <summary>
    /// Отправить данные во внешнюю систему.
    /// </summary>
    /// <param name="request">Подключение к внешней системе.</param>
    /// <param name="result">Структура - Код результата, Текст ответа, Текст ошибки.</param>
    /// <returns>Структура - Код результата, Текст ответа, Текст ошибки.</returns>
    private Structures.Module.IResultSendingMessage SendDataToExternalSystem(HttpWebRequest request, Structures.Module.IResultSendingMessage result)
    {
      var resultResponse = request.BeginGetResponse(null, null);
      resultResponse.AsyncWaitHandle.WaitOne();
      try
      {
        using (var response = request.EndGetResponse(resultResponse) as HttpWebResponse)
        {
          using (var streamReader = new StreamReader(response.GetResponseStream(), true))
          {
            result.Code = 200;
            result.AnswerText = streamReader.ReadToEnd();
          }
        }
      }
      catch (WebException e)
      {
        if (e.Response == null)
        {
          result.Code = 503;
          result.ErrorText = "Service Unavailable";
        }
        else
        {
          var response = (HttpWebResponse)e.Response;
          result.Code = (int)response.StatusCode;
          var answerText = string.Empty;
          using (var streamReader = new StreamReader(response.GetResponseStream(), true))
          {
            answerText = streamReader.ReadToEnd();
          }
          result.ErrorText = string.IsNullOrEmpty(answerText) ? e.Message : string.Format("{0} Details: {1}", e.Message, answerText);
        }
      }
      return result;
    }
    
  }
}