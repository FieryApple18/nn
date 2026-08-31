using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sungero.Core;
using Sungero.CoreEntities;
using HRPro.HRProWredSolution;
using HRPro.HRWred;
using Sungero.Domain.Shared;

namespace DirRX.OverrideSolution.Module.HRWred.Server
{
  partial class ModuleFunctions
  {

    /// <summary>
    /// Создать операцию подписания.
    /// </summary>
    /// <param name="mainDocs">Список основных документов.</param>
    /// <param name="addendaDocs">Список приложений.</param>
    /// <param name="completedBy">Сотрудник, который выполнил задание.</param>
    /// <param name="performer">Сотрудник, которому пришло задание.</param>
    /// <param name="isEmployer">Подписывает руководитель?</param>
    /// <param name="note">Примечание.</param>
    /// <remarks>Сделано перекрытие, чтобы сработало пререкрытие GetOperationState(). Иначе попадает в базовую GetOperationState().</remarks>
    [Public]
    public override void CreateSigningOperation(System.Collections.Generic.IEnumerable<Sungero.Docflow.IOfficialDocument> mainDocs,
                                                System.Collections.Generic.IEnumerable<Sungero.Docflow.IOfficialDocument> addendaDocs,
                                                Sungero.Company.IEmployee completedBy, Sungero.Company.IEmployee performer, bool isEmployer, string note)
    {
      // Сформировать данные для АО.
      var signDate = Calendar.Now;
      var documentIdsList = new List<long> ();
      var mainDocsIds = mainDocs.Where(m => DirRX.OverrideSolution.DocRelatedToWorkBases.Is(m)).Select(m => m.Id).ToList();
      var addendaDocsIds = addendaDocs.Where(a => DirRX.OverrideSolution.DocRelatedToWorkBases.Is(a)).Select(a => a.Id).ToList();
      documentIdsList.AddRange(mainDocsIds);
      documentIdsList.AddRange(addendaDocsIds);
      
      base.CreateSigningOperation(mainDocs, addendaDocs, completedBy, performer, isEmployer, note);
      
      // Запуск АО на заполнение даты подписания в докумнетах.
      var asyncUpdateDocs = NNCommons.AsyncHandlers.UpdateSigningDateInDocs.Create();
      asyncUpdateDocs.DocsIds = string.Join(NNCommons.PublicConstants.Module.Separator.ToString(), documentIdsList);
      asyncUpdateDocs.IsEmployer = isEmployer;
      asyncUpdateDocs.SigningDate = signDate;
      asyncUpdateDocs.ExecuteAsync();
    }

    /// <summary>
    /// Создать операцию подписания.
    /// </summary>
    /// <param name="mainDocs">Список основных документов.</param>
    /// <param name="addendaDocs">Список приложений.</param>
    /// <param name="signatory">Подписант.</param>
    /// <param name="isEmployer">Подписывает руководитель?</param>
    /// <param name="note">Примечание.</param>
    /// <remarks>Сделано перекрытие, чтобы сработало пререкрытие GetOperationState(). Иначе попадает в базовую GetOperationState().</remarks>
    [Public]
    public override void CreateSigningOperation(System.Collections.Generic.IEnumerable<Sungero.Docflow.IOfficialDocument> mainDocs,
                                                System.Collections.Generic.IEnumerable<Sungero.Docflow.IOfficialDocument> addendaDocs,
                                                Sungero.Company.IEmployee signatory, bool isEmployer, string note)
    {
      // Сформировать данные для АО.
      var signDate = Calendar.Now;
      var documentIdsList = new List<long> ();
      var mainDocsIds = mainDocs.Where(m => DirRX.OverrideSolution.DocRelatedToWorkBases.Is(m)).Select(m => m.Id).ToList();
      var addendaDocsIds = addendaDocs.Where(a => DirRX.OverrideSolution.DocRelatedToWorkBases.Is(a)).Select(a => a.Id).ToList();
      documentIdsList.AddRange(mainDocsIds);
      documentIdsList.AddRange(addendaDocsIds);
      
      base.CreateSigningOperation(mainDocs, addendaDocs, signatory, isEmployer, note);
      
      // Запуск АО на заполнение даты подписания в докумнетах.
      var asyncUpdateDocs = NNCommons.AsyncHandlers.UpdateSigningDateInDocs.Create();
      asyncUpdateDocs.DocsIds = string.Join(NNCommons.PublicConstants.Module.Separator.ToString(), documentIdsList);
      asyncUpdateDocs.IsEmployer = isEmployer;
      asyncUpdateDocs.SigningDate = signDate;
      asyncUpdateDocs.ExecuteAsync();
    }

    /// <summary>
    /// Создать операцию подписания.
    /// </summary>
    /// <param name="mainDocs">Список основных документов.</param>
    /// <param name="addendaDocs">Список приложений.</param>
    /// <param name="signatory">Подписант.</param>
    /// <param name="isEmployer">Подписывает руководитель?</param>
    /// <remarks>Сделано перекрытие, чтобы сработало пререкрытие GetOperationState(). Иначе попадает в базовую GetOperationState().</remarks>
    [Public]
    public override void CreateSigningOperation(System.Collections.Generic.IEnumerable<Sungero.Docflow.IOfficialDocument> mainDocs,
                                                System.Collections.Generic.IEnumerable<Sungero.Docflow.IOfficialDocument> addendaDocs,
                                                Sungero.Company.IEmployee signatory, bool isEmployer)
    {
      // Сформировать данные для АО.
      var signDate = Calendar.Now;
      var documentIdsList = new List<long> ();
      var mainDocsIds = mainDocs.Where(m => DirRX.OverrideSolution.DocRelatedToWorkBases.Is(m)).Select(m => m.Id).ToList();
      var addendaDocsIds = addendaDocs.Where(a => DirRX.OverrideSolution.DocRelatedToWorkBases.Is(a)).Select(a => a.Id).ToList();
      documentIdsList.AddRange(mainDocsIds);
      documentIdsList.AddRange(addendaDocsIds);
      
      base.CreateSigningOperation(mainDocs, addendaDocs, signatory, isEmployer);
      
      // Запуск АО на заполнение даты подписания в докумнетах.
      var asyncUpdateDocs = NNCommons.AsyncHandlers.UpdateSigningDateInDocs.Create();
      asyncUpdateDocs.DocsIds = string.Join(NNCommons.PublicConstants.Module.Separator.ToString(), documentIdsList);
      asyncUpdateDocs.IsEmployer = isEmployer;
      asyncUpdateDocs.SigningDate = signDate;
      asyncUpdateDocs.ExecuteAsync();
    }

    /// <summary>
    /// Создать операцию подписания.
    /// </summary>
    /// <param name="mainDocs">Список основных документов.</param>
    /// <param name="addendaDocs">Список приложений.</param>
    /// <param name="completedBy">Сотрудник, который выполнил задание.</param>
    /// <param name="performer">Сотрудник, которому пришло задание.</param>
    /// <param name="isEmployer">Подписывает руководитель?</param>
    /// <remarks>Сделано перекрытие, чтобы сработало пререкрытие GetOperationState(). Иначе попадает в базовую GetOperationState().</remarks>
    [Public]
    public override void CreateSigningOperation(System.Collections.Generic.IEnumerable<Sungero.Docflow.IOfficialDocument> mainDocs,
                                                System.Collections.Generic.IEnumerable<Sungero.Docflow.IOfficialDocument> addendaDocs,
                                                Sungero.Company.IEmployee completedBy, Sungero.Company.IEmployee performer, bool isEmployer)
    {
      // Сформировать данные для АО.
      var signDate = Calendar.Now;
      var documentIdsList = new List<long> ();
      var mainDocsIds = mainDocs.Where(m => DirRX.OverrideSolution.DocRelatedToWorkBases.Is(m)).Select(m => m.Id).ToList();
      var addendaDocsIds = addendaDocs.Where(a => DirRX.OverrideSolution.DocRelatedToWorkBases.Is(a)).Select(a => a.Id).ToList();
      documentIdsList.AddRange(mainDocsIds);
      documentIdsList.AddRange(addendaDocsIds);
      
      base.CreateSigningOperation(mainDocs, addendaDocs, completedBy, performer, isEmployer);
      
      // Запуск АО на заполнение даты подписания в докумнетах.
      var asyncUpdateDocs = NNCommons.AsyncHandlers.UpdateSigningDateInDocs.Create();
      asyncUpdateDocs.DocsIds = string.Join(NNCommons.PublicConstants.Module.Separator.ToString(), documentIdsList);
      asyncUpdateDocs.IsEmployer = isEmployer;
      asyncUpdateDocs.SigningDate = signDate;
      asyncUpdateDocs.ExecuteAsync();
    }

    /// <summary>
    /// Сформировать электронную подпись, уполномоченную по доверенности.
    /// </summary>
    /// <param name="signingOperation">Операция подписания.</param>
    /// <param name="document">Документ.</param>
    /// <param name="signatureID">ИД подписи.</param>
    /// <param name="filename">Имя файла.</param>
    /// <returns>Подпись.</returns>
    /// <remarks>Переработанная функция из HRPro 2.7</remarks>
    public override Wred.SignatureWithPOA FormSignatureWithPOA(ISigningOperation signingOperation, Sungero.Docflow.IOfficialDocument document, long? signatureID, string filename)
    {
      var signatureWithPOA = new Wred.SignatureWithPOA();
      
      if (signatureID == null)
        return signatureWithPOA;

      var signature = Signatures.Get(document, s => s.Where(sign => sign.Id == signatureID.Value)).FirstOrDefault();
      
      if (signature == null)
        return signatureWithPOA;

      if (!NNCommons.PublicFunctions.Module.IsSimpleSignature(signature))
        return base.FormSignatureWithPOA(signingOperation, document, signatureID, filename);

      signatureWithPOA.Date = signature.SigningDate;
      signatureWithPOA.Simple = true;
      
      return signatureWithPOA;
    }

    /// <summary>
    /// Создать контейнер для выгрузки.
    /// </summary>
    /// <param name="signingOperation">Операция подписания.</param>
    /// <param name="exportOperation">Операция выгрузки.</param>
    /// <param name="errors">Список ошибок.</param>
    /// <returns>Контейнер для выгрузки.</returns>
    /// <remarks>Переработанная функция из HRPro 2.7</remarks>
    public override Wred.ZipPacker.ContainerInfo? CreateContainer(string signingOperation, HRPro.HRWred.IExportOperation exportOperation, List<string> errors)
    {
      var filesXml = new List<HRPro.HRWred.Structures.Module.IFileXml>();

      var employeeSigningOperation = SigningOperations.Get(long.Parse(signingOperation));
      var container = new Wred.ZipPacker.ContainerInfo();
      container.ContainerName = TransformFilenameToWredFormat(employeeSigningOperation.DocumentName, employeeSigningOperation.SignatoryContainerName);
      container.FolderName = employeeSigningOperation.SignatoryContainerName;
      container.Files = new List<Wred.ZipPacker.FileInfo>();

      // Если xml еще не сформирован, то формируем его и переводим статус операции подписания:
      // 1. "Контейнер выгружен", если xml валидна
      // 2. "Ошибка при формировании контейнера", если xml невалидна
      var xmlFile = FormXMLFile(employeeSigningOperation);
      
      if (CheckFormXml(xmlFile))
      {
        if (employeeSigningOperation.OperationState != HRPro.HRWred.SigningOperation.OperationState.Formed)
        {
          employeeSigningOperation.OperationState = HRPro.HRWred.SigningOperation.OperationState.Formed;
          employeeSigningOperation.Save();
        }
      }
      else
      {
        string error = string.Empty;
        AddInvalidSigningOperation(employeeSigningOperation, exportOperation, error);
        
        return null;
      }

      // Добавляем xml-описатель в контейнер
      var content = System.Text.Encoding.UTF8.GetBytes(xmlFile);
      var fileName = HRPro.HRWred.PublicConstants.Module.ExportXmlFilename + ".xml";
      var fileInfo = GetFileInfo(content, fileName);
      container.Files.Add(fileInfo);
      
      var validationXml = GetValidationFiles(content, container.ContainerName);
      filesXml = validationXml.FilesXml;
      
      foreach (var error in validationXml.Errors)
      {
        errors.Add(error);
      }

      // Выбираем нужные версии для выгружаемого комплекта документов
      var documentsVersions = employeeSigningOperation.Attachments.Select(a => a.Document.Versions.Where(v => v.Id == a.AttachmentDocumentVersion).FirstOrDefault())
        .Concat(employeeSigningOperation.Document.Versions.Where(v => v.Id == employeeSigningOperation.DocumentVersion));
      
      // Добавляем каждую версию в контейнер
      foreach (var documentVersion in documentsVersions)
      {
        var body = documentVersion.Body;
        
        var documentFileName = employeeSigningOperation.DocumentVersion == documentVersion.Id ?
          employeeSigningOperation.DocumentFileName :
          employeeSigningOperation.Attachments.Where(a => a.AttachmentDocumentVersion == documentVersion.Id).Select(a => a.DocumentFileName).FirstOrDefault();
        
        var documentFileInfo = GetFileInfo(body, documentFileName);
        
        if (documentFileInfo.HasValue)
          container.Files.Add(documentFileInfo.Value);
        
        var errorXmlFile = CheckXMLInfo(filesXml, body.Size, documentFileName, container.ContainerName);
        
        if (!string.IsNullOrEmpty(errorXmlFile))
          errors.Add(errorXmlFile);
      }

      // Выбираем подписи сотрудника и руководителя (при наличии) со всех документов комплекта в этой операции подписания.
      var signsAndFileNamesSetDocuments = new Dictionary<ISignature, string>();
      
      if (Signatures.Get(employeeSigningOperation.Document, s => s.Where(sign => sign.Id == employeeSigningOperation.SignatureID)).Any(sign => sign.SignCertificate != null))
      {
        signsAndFileNamesSetDocuments.Add(Signatures.Get(employeeSigningOperation.Document, s => s.Where(sign => sign.Id == employeeSigningOperation.SignatureID))
                                          .FirstOrDefault(sign => sign.SignCertificate != null),
                                          employeeSigningOperation.SignatureFileName);
      }
      
      foreach (var attachment in employeeSigningOperation.Attachments)
      {
        var attachmentSignature = Signatures.Get(attachment.Document, s => s.Where(sign => sign.Id == attachment.SignatureId)).FirstOrDefault();
        
        if (!signsAndFileNamesSetDocuments.ContainsKey(attachmentSignature) && attachmentSignature.SignCertificate != null)
          signsAndFileNamesSetDocuments.Add(attachmentSignature, attachment.SignatureFileName);
      }
      
      var employerSigningOperation = employeeSigningOperation.EmployerSigningOperation;
      
      if (employerSigningOperation != null)
      {
        
        var signature = Signatures.Get(employerSigningOperation.Document, s => s.Where(sign => sign.Id == employerSigningOperation.SignatureID)).FirstOrDefault();
        if (!signsAndFileNamesSetDocuments.ContainsKey(signature) && signature.SignCertificate != null)
          signsAndFileNamesSetDocuments.Add(signature, employerSigningOperation.SignatureFileName);
        
        // Выбираем для руководителя только те версии документов приложений, которые есть в операции подписания сотрудника.
        var attachments = employerSigningOperation.Attachments.Where(a => employeeSigningOperation.Attachments.Select(e => e.AttachmentDocumentVersion).Contains(a.AttachmentDocumentVersion));
        
        foreach (var attachment in attachments)
        {
          var attachmentSignature = Signatures.Get(attachment.Document, s => s.Where(sign => sign.Id == attachment.SignatureId)).FirstOrDefault();
          
          if (!signsAndFileNamesSetDocuments.ContainsKey(attachmentSignature) && attachmentSignature.SignCertificate != null)
            signsAndFileNamesSetDocuments.Add(attachmentSignature, attachment.SignatureFileName);
        }
      }

      // Добавление подписей.
      foreach (var signAndFileName in signsAndFileNamesSetDocuments)
      {
        var signature = signAndFileName.Key;
        var signatureFileName = signAndFileName.Value;
        var errorsSignature = CheckValiditySignature(signature);
        
        if (!string.IsNullOrEmpty(errorsSignature))
          errors.Add(errorsSignature);
        
        // Добавлена проверка ПЭП, остальной код из коробки HRPro 2.2.
        if (NNCommons.PublicFunctions.Module.IsSimpleSignature(signature))
          continue;

        // BUG38556: Закомментирована проверка файлов подписи, так как при усовершенствовании подписи до формата CADES-XL
        // размер ЭП в xml-описании не совпадает с фактическим (размер ЭП после усовершенствоания увеличивается).
        // Ждём разъяснений МинТруда по данному вопросу.
        // var errorXmlSign = CheckXMLInfo(filesXml, signature.GetDataSignature().LongLength, signatureFileName, container.ContainerName);
        // 
        // if (!string.IsNullOrEmpty(errorXmlSign))
        // errors.Add(errorXmlSign);
        
        var body = signature.GetDataSignature();
        var signFileName = signAndFileName.Value;
        var signFileInfo = GetFileInfo(body, signFileName);
        container.Files.Add(signFileInfo);
      }
      
      // Добавление доверенности и подписи к доверенности.
      if (employerSigningOperation != null && employerSigningOperation.PowerOfAttorney.Any())
      {
        var powerOfAttorney = employerSigningOperation.PowerOfAttorney.FirstOrDefault();
        var body = powerOfAttorney.Document.Versions.FirstOrDefault().Body;
        var poaFileName = powerOfAttorney.DocumentFileName;
        var poaFileInfo = GetFileInfo(body, poaFileName);
        
        if (poaFileInfo.HasValue)
          container.Files.Add(poaFileInfo.Value);
        
        // Добавление подписи к доверенности.
        var attorneySignature = Signatures.Get(powerOfAttorney.Document, s => s.Where(sign => sign.Id == powerOfAttorney.SignatureID)).FirstOrDefault();
        var signBody = attorneySignature.GetDataSignature();
        var signFileName = powerOfAttorney.SignatureFileName;
        var signFileInfo = GetFileInfo(signBody, signFileName);
        container.Files.Add(signFileInfo);
      }
      
      // Добавление основания доверенности и ее подписи
      if (employerSigningOperation != null && employerSigningOperation.MainPowerOfAttorney.Any())
      {
        var mainPoADocument = employerSigningOperation.MainPowerOfAttorney.FirstOrDefault();
        var body = mainPoADocument.Document.Versions.FirstOrDefault().Body;
        var poaFileName = mainPoADocument.DocumentFileName;
        var poaFileInfo = GetFileInfo(body, poaFileName);
        
        if (poaFileInfo.HasValue)
          container.Files.Add(poaFileInfo.Value);
        
        // Добавление подписи основания доверенности.
        var mainPoASign = Signatures.Get(mainPoADocument.Document, s => s.Where(sign => sign.Id == mainPoADocument.SignatureID)).FirstOrDefault();
        var signBody = mainPoASign.GetDataSignature();
        var signFileName = mainPoADocument.SignatureFileName;
        var signFileInfo = GetFileInfo(signBody, signFileName);
        container.Files.Add(signFileInfo);
      }
      
      return container;
    }

  }
}