using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Module.HRCommons.Server
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Выполняет SQL-запрос к БД для получения ID сущностей,
    /// подписи которых удовлетворяют формальным критериям (currentformat меньше Cades-XL, не запрашивалось усовершенствование, усовершенствование доступно).
    /// </summary>
    /// <returns>Список ID сущностей.</returns>
    public List<long> GetEntityIdsForCadesXL()
    {
      var entityIds = new List<long>();

      using (var connection = SQL.CreateConnection())
        using (var command = connection.CreateCommand())
      {
        command.CommandText = Queries.Module.GetEntityIdsForCadesXL;

        using (var reader = command.ExecuteReader())
        {
          while (reader.Read())
          {
            entityIds.Add(reader.GetInt64(0));
          }
        }
      }
      
      Logger.DebugFormat("DirRX.OverrideSolution.Module.HRCommons.GetEntityIdsForCadesXL. {0} entityIds found.", entityIds.Count);
      
      return entityIds;
    }

    /// <summary>
    /// Получить ID подписей УКЭП, подходящих для усовершенствования до CAdES-XL.
    /// </summary>
    /// <returns>Список ID подписей УКЭП, подходящих для усовершенствования до CAdES-XL.</returns>
    public List<long> GetSignaturesToUpgradeToCadesXL()
    {
      var list = new List<long>();

      var entityIds = GetEntityIdsForCadesXL();

      Logger.DebugFormat("DirRX.OverrideSolution.Module.HRCommons.GetSignaturesToUpgradeToCadesXL. Found {0} unique entities preliminary (before PEP filter).", entityIds.Count);
      
      if (!entityIds.Any())
      {
        Logger.Debug("DirRX.OverrideSolution.Module.HRCommons.GetSignaturesToUpgradeToCadesXL. No entities found, returning empty list.");
        
        return list;
      }

      foreach (var entityId in entityIds)
      {
        try
        {
          var document = Sungero.Docflow.OfficialDocuments.Get(entityId);
          
          if (document != null)
          {
            Logger.DebugFormat("DirRX.OverrideSolution.Module.HRCommons.GetSignaturesToUpgradeToCadesXL. Processing document ID: {0}", document.Id);
            
            var signatures = Signatures.Get(document).Where(s =>
                                                            (!s.CurrentFormat.HasValue ||
                                                             (s.CurrentFormat.HasValue && s.CurrentFormat != SignatureFormat.CadesA && s.CurrentFormat != SignatureFormat.CadesXL)) &&
                                                            s.ImproveRequested == false &&
                                                            s.ImproveUnavailable == false &&
                                                            s.SignCertificate != null &&
                                                            !DirRX.NNCommons.PublicFunctions.Module.IsSimpleSignature(s)
                                                           ).Select(s => s.Id);

            list.AddRange(signatures);
            
            Logger.DebugFormat("DirRX.OverrideSolution.Module.HRCommons.GetSignaturesToUpgradeToCadesXL. Added {0} signature IDs from document {1} to the result list.", signatures.Count(), document.Id);
          }
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("DirRX.OverrideSolution.Module.HRCommons.GetSignaturesToUpgradeToCadesXL.Error processing document ID {0}: {1}. StackTrace: {2}", entityId, ex.Message, ex.StackTrace);
        }
      }

      Logger.DebugFormat("DirRX.OverrideSolution.Module.HRCommons.GetSignaturesToUpgradeToCadesXL. Final list contains {0} UKEP signatures eligible for improvement to CAdES-XL.", list.Count);

      return list;
    }

    /// <summary>
    /// Сконвертировать документ в PDF/a с установкой штампов подписей.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="versionId">Ид версии документа.</param>
    /// <returns>Результат конвертации.</returns>
    /// <remarks>Функция создана копированием из ф-и ConvertToPdfWithStamp модуля Docflow.</remarks>
    [Public]
    public override HRPro.HRDocFlow.Structures.Module.IСonversionToPdfAResult ConvertToPdfAndAddSignatureMarks(Sungero.Docflow.IOfficialDocument document, long versionId)
    {
      if (NNIntegration.PublicFunctions.Module.IsUseStampingService())
      {
        var result = ConvertToPdf(document, versionId);
        
        if (result.HasErrors)
          return result;
        
        return SendDocumentOnStamping(document);
      }
      else
      {
        return base.ConvertToPdfAndAddSignatureMarks(document, versionId);
      }
    }

    /// <summary>
    /// Сконвертировать документ в PDF/a.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="versionId">Ид версии документа.</param>
    /// <returns>Результат конвертации.</returns>
    /// <remarks>Функция создана копированием из ф-и ConvertToPdfWithStamp модуля Docflow.</remarks>
    [Public]
    public virtual HRPro.HRDocFlow.Structures.Module.IСonversionToPdfAResult ConvertToPdf(Sungero.Docflow.IOfficialDocument document, long versionId)
    {
      // Предпроверки.
      var result = HRPro.HRDocFlow.Structures.Module.СonversionToPdfAResult.Create();
      var version = document.Versions.SingleOrDefault(v => v.Id == versionId);
      if (version == null)
      {
        result.HasErrors = true;
        result.ErrorMessage = OfficialDocuments.Resources.NoVersionWithNumberErrorFormat(versionId);
        return result;
      }
      
      // Проверить версию тела документа на наличие публичного тела в формате pdf.
      var hasPublicBody = version.PublicBody != null && version.PublicBody.Size > 0;
      if (hasPublicBody && version.AssociatedApplication.Extension == "pdf")
      {
        Logger.DebugFormat("The document body version already has a public body in pdf format. document id: {0}, version id: {1}.", document.Id, version.Id);
        return result;
      }
      
      Logger.DebugFormat("Start Convert to PDF: document id {0}, version id {1}: application - {2}, original application - {3}.",
                         document.Id, version.Id, document.AssociatedApplication, version.BodyAssociatedApplication);
      
      var documentBody = version.Body;
      var extension = version.BodyAssociatedApplication.Extension;
      
      System.IO.Stream pdfDocumentStream = null;
      using (var inputStream = new System.IO.MemoryStream())
      {
        // Выключить error-логирование при доступе к зашифрованным бинарным данным.
        AccessRights.SuppressSecurityEvents(() => documentBody.Read().CopyTo(inputStream));
        try
        {
          pdfDocumentStream = DirRX.OverrideSolution.Module.HRCommons.IsolatedFunctions.PdfConverter.GeneratePdf(inputStream, extension);
        }
        catch (Exception ex)
        {
          if (ex is AppliedCodeException)
            Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), ex.InnerException);
          else
            Logger.Error(Sungero.Docflow.Resources.PdfConvertErrorFormat(document.Id), ex);
          
          result.HasErrors = true;
          result.ErrorMessage = ex.Message;
        }
      }
      
      if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        return result;
      
      // Выключить error-логирование при доступе к зашифрованным бинарным данным/версии.
      AccessRights.SuppressSecurityEvents(
        () =>
        {
          version.PublicBody.Write(pdfDocumentStream);
          version.AssociatedApplication = Sungero.Content.AssociatedApplications.GetByExtension("pdf");
        });
      
      pdfDocumentStream.Close();
      Logger.DebugFormat("{4}: document id {0}, version id {1}: application - {2}, original application - {3}.",
                         document.Id, version.Id, document.AssociatedApplication, version.BodyAssociatedApplication,
                         "Generate public body");
      
      try
      {
        var paramToWriteInHistory = Sungero.Docflow.PublicConstants.OfficialDocument.AddHistoryCommentAboutPDFConvert;
        ((Sungero.Domain.Shared.IExtendedEntity)document).Params[paramToWriteInHistory] = true;
        document.Save();
        ((Sungero.Domain.Shared.IExtendedEntity)document).Params.Remove(paramToWriteInHistory);
        
        Sungero.Docflow.PublicFunctions.OfficialDocument.PreparePreview(document);
        
        result.HasErrors = false;
      }
      catch (Exception e)
      {
        Logger.Error(e.Message);
        result.HasErrors = true;;
        result.ErrorMessage = e.Message;
      }

      Logger.DebugFormat("End Convert to PDF document id {0} version id {1}: application - {2}, original application - {3}.",
                         document.Id, version.Id, document.AssociatedApplication, version.BodyAssociatedApplication);
      
      return result;
    }

    /// <summary>
    /// Отправить документ в сервис проштамповки.
    /// </summary>
    /// <param name="doc">Документ.</param>
    /// <returns>Информация о результате генерации PublicBody для документа.</returns>
    public HRPro.HRDocFlow.Structures.Module.IСonversionToPdfAResult SendDocumentOnStamping(Sungero.Docflow.IOfficialDocument doc)
    {
      var info = HRPro.HRDocFlow.Structures.Module.СonversionToPdfAResult.Create();
      // Проверить наличие версий.
      if (!doc.HasVersions)
      {
        info.HasErrors = true;
        info.ErrorMessage = OfficialDocuments.Resources.NoVersionError;
        return info;
      }
      // Проверить наличие подписи.
      var signatory = HRPro.HRProSolution.PublicFunctions.OfficialDocument.GetSignatureForMark(HRPro.HRProSolution.OfficialDocuments.As(doc), doc.LastVersion.Id);
      if (signatory == null)
      {
        info.HasErrors = true;
        info.ErrorMessage = HRCommons.Resources.DocNotApproved;
        return info;
      }
      var idDoc = new List<long>() { doc.Id };
      var idVersion = new List<long>() { doc.LastVersion.Id };
      NNIntegration.PublicFunctions.Module.StartCreatedOutgoingMessages(NNIntegration.PublicConstants.Module.ExternalSystemsFlowGuids.SendDocumentToStamping, idDoc, idVersion);
      return info;
    }

  }
}