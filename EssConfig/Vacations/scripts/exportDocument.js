class exportDocument {
  execute(params) {
    return new Promise(resolve => {
      const docId = JSON.parse(params.SigningDocumentFromRX)[0].Id;
      const serviceUrl = `/api/Documents/HRPro/IElectronicDocuments/${docId}/export?version=last`;
      ESS.network.sendRequest(serviceUrl, { method: 'POST', body: JSON.stringify({}) })
        .then(result => resolve(JSON.stringify({
          name: result.name + result.extension,
          binaryDataId: result.binaryDataId,
          documentId: docId
        })));
    });
  }
}