using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using DirRX.OverrideSolution.HRProcessTask;

namespace DirRX.OverrideSolution.Server.HRProcessTaskBlocks
{
  partial class SetAgreementApproverIDsDirRXHandlers
  {

    public virtual void SetAgreementApproverIDsDirRXExecute()
    {
      try
      {
        if (_block.AgreementApproversDirRX == null || _block.AgreementApproversDirRX.Count() == 0)
        {
          Logger.Debug("SetAgreementApproverIDsDirRXExecute. Список доп. согласующих пустой.");
        }
        else
        {
          foreach (var approver in _block.AgreementApproversDirRX)
          {
            var id = _obj.AdditionalAgreementApproverIDsDirRX.AddNew();
            id.ApproverID = approver.Id;
            id.Save();
          }
          
          _obj.Save();
          
          Logger.DebugFormat("SetAgreementApproverIDsDirRXExecute. Количество доп. согласующих: {0}. Список ИД доп. согласующих успешно сохранен в поле \"ИД доп. согласующих\".", _block.AgreementApproversDirRX.Count());
        }
      }
      catch (Exception ex)
      {
        Logger.DebugFormat("SetAgreementApproverIDsDirRXExecute. Ошибка при сохранении списка ИД доп. согласующих в поле \"ИД доп. согласующих\". Текст ошибки: {0}. StackTrace: {1}.", ex.Message, ex.StackTrace);
      }
    }
  }

}