class validateVacationData {
  execute(params) {
    const { functions, initiatorId, documentKindId, statementId, vacationTypeIdsArray, beginDateArray, endDateArray, durationArray } = params;
    return new Promise(function (resolve, reject) {
      var body = JSON.stringify({
              employeeId : new Number(initiatorId),
              documentKindId : new Number(documentKindId),
              documentId : new Number(statementId),
              vacationTypeIds : vacationTypeIdsArray,
              beginDates : beginDateArray,
              endDates : endDateArray,
              durations : durationArray
      });
      return ESS.network.sendRequest('/api/functions/NNValidateVacationData', { body: body, method: 'POST' })
      .then(function (validateVacationResult) {
        if (validateVacationResult.ErrorText && validateVacationResult.ErrorText.length != 0) {
          ESS.toasts.showToast(validateVacationResult.ErrorText, 'danger', false);
          reject();
        }
        else {
          functions.setValue('VacationDaysExcess', validateVacationResult.VacationDaysExcess);
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