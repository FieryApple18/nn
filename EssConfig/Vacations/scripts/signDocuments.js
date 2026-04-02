class signDocuments {
  execute(params) {
    var certUrl = `/api/functions/GetEmployeeCertificates`;
    var documentsForSign = [];
    const document = JSON.parse(params.SigningDocument);
    const docFromRx = JSON.parse(params.SigningDocumentFromRX);
    documentsForSign.push({
      name: document.Name,
      binaryDataId: document.BinaryDataId
    });
    documentsForSign.push({
      name: docFromRx.Name,
      binaryDataId: docFromRx.BinaryDataId
    });
    return new Promise(resolve => {
      ESS.network.sendRequest(certUrl)
        .then(certs => ESS.signing.createPackageSignatures({
          certificates: certs,
          documents: documentsForSign,
          cancelationCallback: params.cancelationCallback,
        }))
        .then(signatures => {
          const documentInfo = JSON.parse(params.DocumentInfo)[0];
          const documentId = documentInfo.Id;
          const signedBy = params.employeeId;
          const signatureData = signatures[1];
          const binaryDataId = docFromRx.BinaryDataId;
          const signatureType = params.SignatureType;
          const serviceUrl = `/api/Documents/HRPro/IElectronicDocuments/${documentId}/signatures`;
          ESS.network.sendRequest(serviceUrl, { method: 'PUT', body: JSON.stringify({ type: signatureType, data: signatureData, signedBy: signedBy }) })
            .then(function () {
              resolve(JSON.stringify({
                ...documentsForSign[0],
                Signature: signatures[0],
                SignatureType: params.SignatureType,
              }));
            })
            .catch(function (err) {
              ESS.toasts.showErrorToastWithLog(err);
              reject();
            });
        })
    });
  }
}