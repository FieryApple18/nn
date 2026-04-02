using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.HRCommons.Server
{
  partial class ModuleAsyncHandlers
  {

    public override void AddSignaturesToQueueForImprovement(HRPro.HRCommons.Server.AsyncHandlerInvokeArgs.AddSignaturesToQueueForImprovementInvokeArgs args)
    {
      if (string.IsNullOrEmpty(args.SignaturesIds))
      {
        var signaturesIds = Functions.Module.GetSignaturesToUpgradeToCadesXL();

        var batchSize = HRPro.HRCommons.PublicConstants.Module.BatchSizeForUpgradeSignaturesToCadesA;
        
        while (signaturesIds.Any())
        {
          var async = HRPro.HRCommons.AsyncHandlers.AddSignaturesToQueueForImprovement.Create();
          async.SignaturesIds = string.Join(",", signaturesIds.Take(batchSize).Select(id => id.ToString()));
          async.ExecuteAsync();

          signaturesIds = signaturesIds.Skip(batchSize).ToList();
        }
      }
      else
      {
        var signaturesIdsStr = args.SignaturesIds.Split(',').ToList();
        // Запрашиваем усовершенствование до CAdES-XL, а не до CAdES-A
        Sungero.Core.Signatures.RequestImprovement(signaturesIdsStr.Select(s => long.Parse(s)), RequestImprovementSignatureFormat.CadesXL);
        
        Logger.DebugFormat("DirRX.OverrideSolution.Module.HRCommons.AddSignaturesToQueueForImprovement. Request sent to improve {0} signatures.", signaturesIdsStr.Count);
      }
    }

    /// <summary>
    /// Обновление записи после усовершенствования.
    /// </summary>
    /// <param name="args">Ид записи.</param>
    [Obsolete("Устаревший АО. В будущем будет удален, оставлен для совместимости")]
    public override void UpdateImproveStatus(HRPro.HRCommons.Server.AsyncHandlerInvokeArgs.UpdateImproveStatusInvokeArgs args)
    {
      // HACK. Вычисление целиком дублирует коробочную логику. Тут добавлена дополнительная проверка на блокировку и переповтор.
      // Попрален порядок получения даты истечения сертификата. Для промежуточного формата Cades-T нужно брать дату сертификата а не штампа времени.
      Logger.DebugFormat("UpdateImproveStatus: Id: {0}. Начало обработки.", args.entryId);
      var entry = HRPro.HRCommons.ImproveSignatureDocuments.Get(args.entryId);
      
      if(Locks.TryLock(entry))
      {
        var signatures = Signatures.Get(entry.Document.Versions.SingleOrDefault(v => v.Id == entry.DocumentVersionId)).Where(l => l.SignCertificate != null);
        entry.DocumentsSignature.Clear();
        
        foreach (var signature in signatures)
        {
          var signatureInfo = Signatures.GetSignatureInfo(signature);
          var signatureKind = signatureInfo.SignatureFormat.ToString();
          var tableRow = entry.DocumentsSignature.AddNew();
          tableRow.SignatureKind = signatureKind;
          tableRow.CertificateExpirationDate = DirRX.NNCommons.PublicFunctions.Module.GetCertificateExpirationDateInSignature(signatureInfo);
        }
        
        entry.UpdateDate = HRCommons.Server.ModuleFunctions.GetMinValidityDate(signatures.ToList());
        entry.LastImproveDate = args.LastImproveDate;
        if (args.status.Equals(HRPro.HRCommons.ImproveSignatureDocument.Status.Active.ToString()))
        {
          entry.Status = HRPro.HRCommons.ImproveSignatureDocument.Status.Active;
        }
        else if (args.status.Equals(HRPro.HRCommons.ImproveSignatureDocument.Status.Error.ToString()))
        {
          entry.Status = HRPro.HRCommons.ImproveSignatureDocument.Status.Error;
        }
        entry.Save();
        Logger.DebugFormat("UpdateImproveStatus: Id: {0}. Запись сохранена.", args.entryId);
        
        Locks.Unlock(entry);
      }
      else
      {
        var lockInfo = Locks.GetLockInfo(entry);
        Logger.DebugFormat("UpdateImproveStatus: Id: {0}. Запись заблокирована {1} в {2}. Выполнение АО будет отложено.", args.entryId, lockInfo.OwnerName, lockInfo.LockTime);
        args.Retry = true;
      }
      
      Logger.DebugFormat("UpdateImproveStatus: Id: {0}. Окончание обработки.", args.entryId);
    }

  }
}