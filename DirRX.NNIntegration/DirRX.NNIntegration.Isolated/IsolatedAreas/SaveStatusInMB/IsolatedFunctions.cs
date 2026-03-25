using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using DirRX.NNIntegration.Structures.Module;
using Npgsql;

namespace DirRX.NNIntegration.Isolated.SaveStatusInMB
{
  public partial class IsolatedFunctions
  {
    /// <summary>
    /// Сохранение статуса сообщения полученного из Суперники.
    /// </summary>
    /// <param name="insertQuery">SQL запрос.</param>
    /// <param name="connectionString">Строка подключения.</param>
    /// <param name="rxIncomingMessageId">Идентификатор записи справочника "Входящее интеграционное сообщение".</param>
    /// <param name="mbMessageId">Идентификатор записи в таблице "Messages".</param>
    /// <param name="posted">Дата и время создания записи справочника "Входящее интеграционное сообщение".</param>
    /// <param name="status">Статус-код обработки.</param>
    /// <param name="selectedButton">Тип нажатой кнопки.</param>
    /// <param name="comment">Комментарий в случае если нажата кнопка.</param>
    /// <param name="errorMessage">Текст ошибки, полученный из ПМД.</param>
    /// <returns>Результат вставки данных.</returns>
    [Public]
    public bool SaveStatusSupernikaPushNotification(string insertQuery, string connectionString, long rxIncomingMessageId, string mbMessageId, DateTime posted, string status, string selectedButton, string comment, string errorMessage)
    {
      var result = false;

      using (var connection = new NpgsqlConnection(connectionString))
      {
        try
        {
          connection.Open();
          
          using (var command = new NpgsqlCommand(insertQuery, connection))
          {
            var dateTimeOffset = new DateTimeOffset(posted, TimeSpan.Zero);
            command.Parameters.AddWithValue("@rxIncomingMessageId", rxIncomingMessageId);
            command.Parameters.AddWithValue("@mbMessageId", Guid.Parse(mbMessageId));
            command.Parameters.AddWithValue("@dateTime", dateTimeOffset);
            command.Parameters.AddWithValue("@status", status);
            command.Parameters.AddWithValue("@selectedButton", selectedButton);
            command.Parameters.AddWithValue("@comment", comment);
            command.Parameters.AddWithValue("@errorMessage", errorMessage);
            
            int rowsAffected = command.ExecuteNonQuery();
            result = true;
            Logger.DebugFormat("SaveStatusSupernikaPushNotification. RXIncomingMessageId: {0}, MBMessageId: {1}. Вставлено строк: {2}.", rxIncomingMessageId, mbMessageId, rowsAffected);
          }
          
        }
        catch (NpgsqlException ex)
        {
          Logger.ErrorFormat("SaveStatusSupernikaPushNotification. RXIncomingMessageId: {0}, MBMessageId: {1}. Ошибка при работе с базой данных: {2}. ", rxIncomingMessageId, mbMessageId, ex.Message);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("SaveStatusSupernikaPushNotification. RXIncomingMessageId: {0}, MBMessageId: {1}. Произошла ошибка: {2}. StackTrace: {3}", rxIncomingMessageId, mbMessageId, ex.Message, ex.StackTrace);
        }
        finally
        {
          if (connection.State == System.Data.ConnectionState.Open)
            connection.Close();
        }
      }
      
      return result;
    }
  }
}