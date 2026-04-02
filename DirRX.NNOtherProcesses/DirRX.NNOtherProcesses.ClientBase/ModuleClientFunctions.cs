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
    
    #endregion

  }
}