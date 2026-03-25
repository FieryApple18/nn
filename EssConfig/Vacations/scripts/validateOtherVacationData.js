class validateOtherVacationData {
  execute(params) {
    const { functions, initiatorId, statementId, vacationType, vacationReason, beginDate, endDate, duration } = params;
    return new Promise(function (resolve, reject) {
      var urlValidateOtherVacationData = `/api/functions/NNValidateOtherVacationData?employeeId=${initiatorId}&documentId=${statementId}&vacationTypeId=${vacationType}&vacationReasonId=${vacationReason}&beginDate=${beginDate}&endDate=${endDate}&duration=${duration}`;
      return ESS.network.sendRequest(urlValidateOtherVacationData)
      .then(function (validateVacationResult) {
        if (validateVacationResult.ErrorText && validateVacationResult.ErrorText.length != 0) {
          ESS.toasts.showToast(validateVacationResult.ErrorText, 'danger', false);
          reject();
        }
        else {
          resolve();
        }
      })
      .catch(function (error) {
        ESS.toasts.showToast(error.message, 'danger', false);
        reject();
      });
    });
  }
}