using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Sungero.Core;
using DirRX.NNIntegration.Structures.Module;

namespace DirRX.NNIntegration.Isolated.EssDatabase
{
  public partial class IsolatedFunctions
  {
    /// <summary>
    /// Получить данные фильтра из профиля ЛК из БД Ess.
    /// </summary>
    /// <param name="insertQuery">SQL запрос.</param>
    /// <param name="connectionString">Строка подключения.</param>
    /// <param name="personUid">Ид персоны ESS (tenant@personId).</param>
    /// <returns>Данные фильтра из профиля пользователя ЛК.</returns>
    [Public]
    public IEssCurrentPeriod GetEssUserProfileCurrentPeriod(string selectQuery, string connectionString, string personUid)
    {
      var profileSerializedValue = string.Empty;
      using (var connection = new NpgsqlConnection(connectionString))
      {
        try
        {
          connection.Open();
          
          using (var command = new NpgsqlCommand(selectQuery, connection))
          {
            command.Parameters.AddWithValue("@personUid", personUid);
            
            using (var reader = command.ExecuteReader())
            {
              if (reader.Read())
              {
                profileSerializedValue = reader["Profile_SerializedValue"].ToString();
                Logger.DebugFormat("GetEssUserProfile. PersonUid: {0}. Профиль пользователя найден. UserProfile: {1}. ", personUid, profileSerializedValue);
              }
            }
          }
        }
        catch (NpgsqlException ex)
        {
          Logger.ErrorFormat("GetEssUserProfile. PersonUid: {0}. Ошибка при работе с базой данных: {1}. ", personUid, ex.Message);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("GetEssUserProfile. PersonUid: {0}. Произошла ошибка: {1}. StackTrace: {2}", personUid, ex.Message, ex.StackTrace);
        }
        finally
        {
          if (connection.State == System.Data.ConnectionState.Open)
            connection.Close();
        }
      }
      
      var result = ParseEssCurrentPeriodFromJson(profileSerializedValue);
      if (result != null)
        Logger.DebugFormat("GetEssUserProfile. PersonUid: {0}. Length: {1}. StartDate: {2}. EndDate: {3}.",
                           personUid, result.Length, result.BeginDate, result.EndDate);
      else
        Logger.ErrorFormat("GetEssUserProfile. PersonUid: {0}. Не удалось извлечь параметры фильтра из профиля БД Ess.", personUid);
      
      return result;
    }
    
    /// <summary>
    /// Получить период профиля пользователя из json.
    /// </summary>
    private IEssCurrentPeriod ParseEssCurrentPeriodFromJson(string jsonString)
    {
      if (string.IsNullOrEmpty(jsonString))
        return null;
      
      try
      {
        var result = DirRX.NNIntegration.Structures.Module.EssCurrentPeriod.Create();
        
        // Парсим JSON
        var obj = JObject.Parse(jsonString);
        
        // Ищем currentPeriod
        JToken period = obj["CurrentPeriod"];
        if (period == null)
          return null;

        // Если currentPeriod это строка - парсим её как JSON
        if (period.Type == JTokenType.String)
        {
          string periodStr = period.Value<string>();
          periodStr = periodStr.Replace("\\", "").Trim('"');
          period = JObject.Parse(periodStr);
        }
        
        // Заполняем поля
        result.Length = period["Length"]?.ToObject<int?>();
        result.BeginDate = period["BeginDate"]?.ToObject<DateTime?>();
        result.EndDate = period["EndDate"]?.ToObject<DateTime?>();
        return result;
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("ParseEssCurrentPeriodFromJson. Json: {0}. Произошла ошибка: {1}. StackTrace: {2}", jsonString, ex.Message, ex.StackTrace);
        return null;
      }
    }
  }
}