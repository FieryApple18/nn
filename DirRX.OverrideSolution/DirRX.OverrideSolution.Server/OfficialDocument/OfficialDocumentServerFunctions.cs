using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.OfficialDocument;

namespace DirRX.OverrideSolution.Server
{
  partial class OfficialDocumentFunctions
  {

    /// <summary>
    /// Проверяет наличие подписи на последней версии документа.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>true - подписан, иначе false.</returns>
    [ExpressionElement("ExpressionFuncName_HasSignatures", "ExpressionFuncDesc_ReturnBool")]
    public static bool HasSignatures(Sungero.Docflow.IOfficialDocument document)
    {
      return document.HasVersions && Signatures.Get(document.LastVersion, q => q.Where(s => s.SignatureType == SignatureType.Approval)).Any();
    }

    /// <summary>
    /// Создать версию документа из строки base64.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="base64">Строка в base64.</param>
    /// <param name="extension">Расширение файла документа.</param>
    /// <returns>Текст ошибки, либо string.Empty.</returns>
    /// <remarks>Документ должен быть заблокирован заранее.</remarks>
    [Public]
    public static string CreateVersionFromBase64(DirRX.OverrideSolution.IOfficialDocument document, string base64, string extension)
    {
      var errorMessage = string.Empty;
      var logPrefix = string.Format("CreateVersionFromBase64. documentId = {0}", document.Id);
      
      if (!Locks.GetLockInfo(document).IsLockedByMe)
      {
        errorMessage = "Переданный документ не заблокирован.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return errorMessage;
      }
      try
      {
        using (var memory = new MemoryStream(Convert.FromBase64String(base64)))
          document.CreateVersionFrom(memory, extension);
        
      }
      catch (Exception e)
      {
        errorMessage = string.Format("Не удалось создать версию из файла. Причина: {0}", e.Message);
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}. StackTrace: {3}", logPrefix, errorMessage, e.Message, e.StackTrace);
        return errorMessage;
      }

      try
      {
        document.Save();
        return string.Empty;
      }
      catch (Exception e)
      {
        errorMessage = string.Format("Не удалось сохранить документ. Причина: {0}", e.Message);
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}. StackTrace: {3}", logPrefix, errorMessage, e.Message, e.StackTrace);
      }

      return errorMessage;
    }

    /// <summary>
    /// Преобразовать документ в PDF и поставить отметку об ЭП.
    /// </summary>
    /// <param name="versionId">Id версии документа.</param>
    /// <returns>Результат преобразования в PDF.</returns>
    [Remote]
    public override Sungero.Docflow.Structures.OfficialDocument.IConversionToPdfResult ConvertToPdfAndAddSignatureMark(long versionId)
    {
      if (NNIntegration.PublicFunctions.Module.IsUseStampingService())
      {
        var result = this.ConvertToPdf();
        if (result.HasErrors || result.HasConvertionError || result.HasLockError)
        {
          Logger.DebugFormat("ConvertToPdfAndAddSignatureMark (StampService). Ошибка преобразования в PDF: {0}.", result.ErrorMessage);
          return result;
        }
        
        result = SendDocumentOnStamping();
        if (!string.IsNullOrEmpty(result.ErrorMessage))
          Logger.DebugFormat("ConvertToPdfAndAddSignatureMark (StampService). Ошибка отправки на проштамповку: {0}.", result.ErrorMessage);
        
        return result;
      }
      else
      {
        return base.ConvertToPdfAndAddSignatureMark(versionId);
      }
    }

    /// <summary>
    /// Выполнить проверку возможности отправки документа на повторную проштамповку.
    /// </summary>
    /// <returns>Тект ошибки, либо string.Empty если проверка пройдена.</returns>
    [Public, Remote(IsPure = true)]
    public string CanSendToRestamping()
    {
      var logPrefix = string.Format("CanSendToRestamping (StampService). ИД документа: {0}", _obj.Id);
      var info = CheckLastVersionApproveStatus();
      if (info.HasErrors)
      {
        Logger.DebugFormat("{0}. Ошибка: {1}.", logPrefix, info.ErrorMessage);
        return info.ErrorMessage;
      }
      
      // Проверить, что для последней версии документа установлен обработчик pdf.
      var pdfApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.PublicConstants.OfficialDocument.Extensions.Pdf);
      if (!Sungero.Content.AssociatedApplications.Equals(_obj.LastVersion.AssociatedApplication, pdfApplication))
      {
        var error = "Документ должен быть в формате pdf";
        Logger.DebugFormat("{0}. Ошибка: {1}.", logPrefix, error);
        return error;
      }
      
      return string.Empty;
    }

    /// <summary>
    /// Выполнить проверку наличия подписи на документе.
    /// </summary>
    /// <returns>Структура с результатом проверки.</returns>
    public Sungero.Docflow.Structures.OfficialDocument.IConversionToPdfResult CheckLastVersionApproveStatus()
    {
      var info = Sungero.Docflow.Structures.OfficialDocument.ConversionToPdfResult.Create();
      
      // Проверить наличие версий.
      if (!_obj.HasVersions)
      {
        info.HasErrors = true;
        info.ErrorTitle = OfficialDocuments.Resources.ConvertionErrorTitleBase;
        info.ErrorMessage = OfficialDocuments.Resources.NoVersionError;
        return info;
      }
      
      var signatory = HRPro.HRProSolution.PublicFunctions.OfficialDocument.GetSignatureForMark(_obj, _obj.LastVersion.Id);
      if (signatory == null)
      {
        info.HasErrors = true;
        info.ErrorMessage = DirRX.OverrideSolution.Module.HRCommons.Resources.DocNotApproved;
        return info;
      }
      
      return info;
    }

    /// <summary>
    /// Отправить документ в сервис проштамповки.
    /// </summary>
    /// <returns>Информация о результате генерации PublicBody для документа.</returns>
    public Sungero.Docflow.Structures.OfficialDocument.IConversionToPdfResult SendDocumentOnStamping()
    {
      var logPrefix = string.Format("SendDocumentOnStamping (StampService). ИД документа: {0}", _obj.Id);
      
      // Проверить последнюю версию на наличие отметки о согласовании.
      var info = CheckLastVersionApproveStatus();
      if (info.HasErrors)
      {
        Logger.DebugFormat("{0}. Ошибка: {1}.", logPrefix, info.ErrorMessage);
        return info;
      }
      
      var idDoc = new List<long>() { _obj.Id };
      var idVersion = new List<long>() { _obj.LastVersion.Id };
      
      // Запустить процесс отправки докуементов на проштамповку во внешней системе.
      NNIntegration.PublicFunctions.Module.StartCreatedOutgoingMessages(NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.SendDocumentToStamping, idDoc, idVersion);
      return info;
    }

    /// <summary>
    /// Перезаписать publicbody у версии документа из строки base64.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="versionId">Id версии.</param>
    /// <param name="base64">Строка в base64.</param>
    /// <returns>Текст ошибки, либо string.Empty.</returns>
    /// <remarks>Документ должен быть заблокирован заранее.</remarks>
    [Public]
    public static string RewritePublicBodyForVersionDocument(DirRX.OverrideSolution.IOfficialDocument document, long versionId, string base64)
    {
      var errorMessage = string.Empty;
      var logPrefix = string.Format("RewritePublicBodyVersionDocument. documentId = {0}, versionId = {1}", document.Id, versionId);
      
      var version = document.Versions.FirstOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        errorMessage = "Не найдена версия документа.";
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return errorMessage;
      }
      
      if (!Locks.GetLockInfo(document).IsLockedByMe)
      {
        errorMessage = "Переданный документ не заблокирован";
        Logger.ErrorFormat("{0}. {1}", logPrefix, errorMessage);
        return errorMessage;
      }
      try
      {
        using (var inputStream = new System.IO.MemoryStream(Convert.FromBase64String(base64)))
        {
          version.PublicBody.Write(inputStream);
        }
        
      }
      catch (Exception e)
      {
        errorMessage = "Не удалось перезаписать publicbody у версии документа";
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}. StackTrace: {3}", logPrefix, errorMessage, e.Message, e.StackTrace);
        return errorMessage;
      }

      try
      {
        document.Save();
      }
      catch (Exception e)
      {
        errorMessage = "Не удалось сохранить документ";
        Logger.ErrorFormat("{0}. {1}. Текст ошибки: {2}. StackTrace: {3}", logPrefix, errorMessage, e.Message, e.StackTrace);
      }

      return errorMessage;
    }

    /// <summary>
    /// Преобразовать документ в PDF.
    /// </summary>
    /// <returns>Результат преобразования.</returns>
    /// <remarks>Функция создана копированием ConvertToPdfWithStamp(), убрано формирование штампа и запись в историю.</remarks>
    public Sungero.Docflow.Structures.OfficialDocument.IConversionToPdfResult ConvertToPdf()
    {
      var result = Sungero.Docflow.Structures.OfficialDocument.ConversionToPdfResult.Create();
      var logPrefix = string.Format("ConvertToPdf (StampService). ИД документа: {0}", _obj.Id);
      
      // Предпроверки.
      var version = _obj.LastVersion;
      if (version == null)
      {
        result.HasConvertionError = true;
        result.ErrorMessage = OfficialDocuments.Resources.NoVersionWithNumberErrorFormat(version.Id);
        Logger.DebugFormat("{0}. {1}.", logPrefix, result.ErrorMessage);
        return result;
      }
      
      result = ValidateVersionBeforeConversion(version.Id);
      if (result.HasErrors)
      {
        Logger.DebugFormat("{0}. Ошибка валидации документа при преобразовании в PDF. Ошибка: {1}.", logPrefix, result.ErrorMessage);
        return result;
      }
      
      // Проверить версию тела документа на наличие публичного тела в формате pdf.
      var hasPublicBody = version.PublicBody != null && version.PublicBody.Size > 0;
      if (hasPublicBody && version.AssociatedApplication.Extension == "pdf")
      {
        Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("Версия тела документа уже имеет публичное тело в формате PDF.", _obj, version);
        return result;
      }
      
      Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("Старт конвертации в PDF", _obj, version);
      
      // Получить тело версии для преобразования в PDF.
      var body = Sungero.Docflow.PublicFunctions.OfficialDocument.GetBodyToConvertToPdf(_obj, version, true);
      if (body == null || body.Body == null || body.Body.Length == 0)
      {
        Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("Не удалось получить тело для конвертации в PDF", _obj, version);
        result.HasConvertionError = true;
        result.ErrorMessage = DirRX.OverrideSolution.OfficialDocuments.Resources.ConvertionToPdfError;
        return result;
      }
      
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream(body.Body))
      {
        try
        {
          pdfDocumentStream = Sungero.Docflow.IsolatedFunctions.PdfConverter.GeneratePdf(inputStream, body.Extension);
        }
        catch (Exception ex)
        {
          if (ex is AppliedCodeException)
            Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(_obj.Id), ex.InnerException);
          else
            Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(_obj.Id), ex);
          
          result.HasConvertionError = true;
          result.HasLockError = false;
          result.ErrorMessage = DirRX.OverrideSolution.OfficialDocuments.Resources.ConvertionToPdfError;
        }
      }
      
      if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        return result;
      
      // Выключить error-логирование при доступе к зашифрованным бинарным данным/версии.
      AccessRights.SuppressSecurityEvents(
        () =>
        {
          version.PublicBody.Write(pdfDocumentStream);
          version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension(Sungero.Docflow.PublicConstants.OfficialDocument.Extensions.Pdf);
        });
      
      pdfDocumentStream.Close();
      
      Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("Создание публичного тела документа", _obj, version);
      try
      {
        _obj.Save();
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        result.HasConvertionError = true;
        result.HasLockError = false;
        result.ErrorMessage = e.Message;
      }
      Sungero.Docflow.PublicFunctions.Module.LogPdfConverting("Конец конвертации документа в PDF", _obj, version);
      
      return result;
    }

    /// <summary>
    /// Проверить версию документа перед преобразованием в PDF.
    /// </summary>
    /// <param name="versionId">ИД версии документа.</param>
    /// <returns>Результат проверки версии.</returns>
    /// <remarks>В коробочную функцию добавлено логирование.</remarks>
    public override Sungero.Docflow.Structures.OfficialDocument.IConversionToPdfResult ValidateVersionBeforeConversion(long versionId)
    {
      var info =  Sungero.Docflow.Structures.OfficialDocument.ConversionToPdfResult.Create();
      
      var logPrefix = string.Format("ValidateDocumentBeforeConvertion (StampService). ИД документа: {0}. ИД версии: {1}", _obj.Id, versionId);
      
      // Проверить наличие версии.
      var version = _obj.Versions.FirstOrDefault(x => x.Id == versionId);
      
      if (version == null)
      {
        info.HasErrors = true;
        info.ErrorTitle = OfficialDocuments.Resources.ConvertionErrorTitleBase;
        info.ErrorMessage = OfficialDocuments.Resources.NoVersionError;
        
        Logger.DebugFormat("{0}. {1}.", logPrefix, info.ErrorMessage);
        
        return info;
      }
      
      // Проверить тело версии на пустоту.
      var versionBody = this.GetBodyToConvertToPdfWithMarks(version);
      
      if (versionBody.Body == null || versionBody.Body.Length == 0)
      {
        info.HasErrors = true;
        info.ErrorMessage = OfficialDocuments.Resources.ConvertionErrorTitleBase;
        
        Logger.DebugFormat("{0}. {1}.", logPrefix, info.ErrorMessage);
        
        return info;
      }
      
      // Проверить заблокированность тела версии.
      var lockInfo = Locks.GetLockInfo(version.Body);
      
      if (lockInfo.IsLocked)
      {
        info.HasErrors = true;
        info.HasLockError = true;
        info.ErrorTitle = OfficialDocuments.Resources.ConvertionErrorTitleBase;
        info.ErrorMessage = lockInfo.LockedMessage;
        
        Logger.DebugFormat("{0}. {1}.", logPrefix, info.ErrorMessage);
        
        return info;
      }
      
      // Проверить, участвовал ли документ в электронном обмене (МКДО).
      if (this.IsExchangeDocument(versionId))
      {
        info.IsExchangeDocument = true;

        Logger.DebugFormat("{0}. {1}.", logPrefix, info.ErrorMessage);
        
        return info;
      }
      
      // Проверить формат.
      var versionExtension = version.BodyAssociatedApplication.Extension.ToLower();
      if (!this.CheckPdfConvertibilityByExtension(versionExtension))
      {
        var result = this.GetExtensionValidationErrorRemote(versionExtension);
        
        Logger.DebugFormat("{0}. {1}.", logPrefix, result.ErrorMessage);
      }
      
      return info;
    }

    /// <summary>
    /// Получить последнюю по времени электронную подпись для простановки отметки.
    /// </summary>
    /// <param name="versionId">Номер версии.</param>
    /// <returns>Электронная подпись.</returns>
    [Public]
    public override Sungero.Domain.Shared.ISignature GetSignatureForMark(long versionId)
    {
      return this.GetSignatureForMarkBySignatureType(versionId, null);
    }

    /// <summary>
    /// Получить последнюю по времени электронную подпись для простановки отметки.
    /// </summary>
    /// <param name="versionId">Номер версии.</param>
    /// <param name="isSimpleSignature">Признак того, что подпись - ПЭП.</param>
    /// <returns>Электронная подпись.</returns>
    [Public]
    public virtual Sungero.Domain.Shared.ISignature GetSignatureForMarkBySignatureType(long versionId, bool? isSimpleSignature)
    {
      var logPrefix = string.Format("GetSignatureForMark (StampService). Ид документа: {0}. Ид версии: {1}", _obj.Id, versionId);
      var version = _obj.Versions.FirstOrDefault(x => x.Id == versionId);
      if (version == null)
      {
        Logger.DebugFormat("{0}. Не найдена версия документа по ИД.", logPrefix);
        return null;
      }
      
      // Все подписи.
      var versionSignatures = Signatures.Get(version).ToList();
      if (!versionSignatures.Any())
      {
        Logger.DebugFormat("{0}. Не найдено ни одной подписи для версии документа.", logPrefix);
        return null;
      }
      
      // Если указан тип ЭП, то фильтруем по нему.
      if (isSimpleSignature.HasValue)
        versionSignatures = versionSignatures
          .Where(x => isSimpleSignature == NNCommons.PublicFunctions.Module.IsSimpleSignature(x))
          .ToList();
      
      // Берем последнюю по времени подпись.
      return versionSignatures
        .OrderByDescending(s => s.SigningDate)
        .FirstOrDefault();
    }

  }
}