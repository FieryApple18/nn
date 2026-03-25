using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.VacationNoticeDocument;

namespace DirRX.OverrideSolution.Shared
{
  partial class VacationNoticeDocumentFunctions
  {

    /// <summary>
    /// Признак необходимости отображения панели регистрации.
    /// </summary>
    /// <param name="additionalCondition">Дополнительное условие при наследовании.</param>
    /// <returns>True, если надо показать панель.</returns>
    public override bool NeedShowRegistrationPane(bool additionalCondition)
    {
      return true;
    }

    /// <summary>
    /// Сменить доступность реквизитов документа.
    /// </summary>
    /// <param name="isEnabled">True, если свойства должны быть доступны.</param>
    /// <param name="repeatRegister">Перерегистрация.</param>
    public override void ChangeDocumentPropertiesAccess(bool isEnabled, bool repeatRegister)
    {
      base.ChangeDocumentPropertiesAccess(isEnabled, repeatRegister);
      
      _obj.State.Properties.InternalApprovalState.IsEnabled = true;
    }

  }
}