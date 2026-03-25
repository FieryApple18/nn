class signFromObjectCard {
  execute(params) {
    const { data, documents, callback } = params;
    var serviceUrl = `/api/functions/GetEmployeeCertificates`;
    ESS.network.sendRequest(serviceUrl)
    .then(function (certs) {
      ESS.signing.sign({
        certificates: certs,
        documents,
        documentGroup: 'IElectronicDocuments',
        systemName: data.system,
        signatureType: 'Approval',
        packageTitle: data.title,
        signedBy: data.performer,
        callback,
      });
    })
    .catch(function (err) {
      ESS.toasts.showErrorToastWithLog(err);
    });
  }
}