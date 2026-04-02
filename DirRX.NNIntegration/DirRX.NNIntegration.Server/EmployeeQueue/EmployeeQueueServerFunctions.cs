using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNIntegration.EmployeeQueue;
using Sungero.Parties;
using System.Text.RegularExpressions;

namespace DirRX.NNIntegration.Server
{
  partial class EmployeeQueueFunctions
  {

    /// <summary>
    /// Установить признак НГД.
    /// </summary>
    /// <param name="ngd">Строка НГД.</param>
    /// <returns>Признак успешности установки.</returns>
    public bool TrySetNGD(string ngd)
    {
      if (!string.IsNullOrEmpty(ngd))
      {
        switch (ngd)
        {
          case Constants.EmployeeQueue.NGDCodes.General:
            _obj.IsNGD = false;
            break;
          case Constants.EmployeeQueue.NGDCodes.NGD:
            _obj.IsNGD = true;
            break;
          default:
            return false;
        }
      }
      else
        _obj.IsNGD = false;
      
      return true;
    }
    
    /// <summary>
    /// Проверить группу МР сотрудника.
    /// </summary>
    /// <returns>Ошибка.</returns>
    public string CheckMPGroup()
    {
      if (string.IsNullOrEmpty(_obj.MPGroup))
        return DirRX.NNIntegration.EmployeeQueues.Resources.MPGroupIsEmptyError;
      
      var groupMPIsFound = NNIntegration.PublicFunctions.Module.GetActiveMPGroups(_obj.MPGroup).Any();
      if (!groupMPIsFound)
        return DirRX.NNIntegration.EmployeeQueues.Resources.MPGroupNotFoundErrorFormat(_obj.MPGroup);
      
      return string.Empty;
    }

    /// <summary>
    /// Проверить СНИЛС на валидность.
    /// </summary>
    /// <returns>Ошибка.</returns>
    public string CheckInila()
    {
      // Проверить СНИЛС на валидность.
      var error = DirRX.EssPlatformSolution.PublicFunctions.Person.CheckInila(_obj.INILA);
      if (!string.IsNullOrEmpty(error))
      {
        // HACK: Seleznyov_DV. Перекрытие текста ошибки для контрольной суммы.
        error = error == DirRX.EssPlatformSolution.People.Resources.NotValidINILA ? DirRX.NNIntegration.Resources.InilaSumNotValidError : error;
        return error;
      }
      
      // Проверить СНИЛС на соответсвие формату передачи данных из внешней системы.
      if (!Regex.IsMatch(_obj.INILA, Constants.EmployeeQueue.InilaPattern))
        return DirRX.NNIntegration.EmployeeQueues.Resources.InilaFormatError;
      
      return string.Empty;
    }
    
    /// <summary>
    /// Установить пол персоны по его коду.
    /// </summary>
    /// <param name="person">Персона.</param>
    /// <param name="sex">Пол.</param>
    /// <returns>Результат. True - удалось установить пол. False - не удалось установить пол.</returns>
    public bool TryParsePersonSex(string sex)
    {
      if (sex == Constants.EmployeeQueue.SexCodes.Male)
      {
        _obj.Sex = DirRX.NNIntegration.EmployeeQueue.Sex.Male;
        return true;
      }
      
      if (sex == Constants.EmployeeQueue.SexCodes.Female)
      {
        _obj.Sex = DirRX.NNIntegration.EmployeeQueue.Sex.Female;
        return true;
      }
      return false;
    }
    
    /// <summary>
    /// Проверить код вида занятости сотрудника на корректность.
    /// </summary>
    /// <param name="empType">Код вида занятости.</param>
    /// <returns>Результат. True - удалось определить вид занятости по коду. False - не удалось определить вид занятости по коду.</returns>
    public static bool CheckEmployemntType(string empType)
    {
      return empType == Constants.EmployeeQueue.EmploymentTypeCodes.MainPlace ||
        empType == Constants.EmployeeQueue.EmploymentTypeCodes.InternalConcurr ||
        empType == Constants.EmployeeQueue.EmploymentTypeCodes.ExternalConcurr;
    }
  }
}