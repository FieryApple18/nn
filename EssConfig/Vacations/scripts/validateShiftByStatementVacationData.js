class validateShiftByStatementVacationData {
  execute(params) {
    const { functions, initiatorId, statementId, vacationTypeIdsArray, beginDateArray, endDateArray, durationArray, vacationsJson, isPaid } = params;
    return new Promise(function (resolve, reject) {
    var body = JSON.stringify({
              employeeId : new Number(initiatorId),
              documentId : new Number(statementId),
              vacationTypeIds : vacationTypeIdsArray,
              beginDates : beginDateArray,
              endDates : endDateArray,
              durations : durationArray,
              oldVacationsJson : vacationsJson,
              isPaid : isPaid
      });
      return ESS.network.sendRequest('/api/functions/NNValidateShiftByStatementVacationData', { body: body, method: 'POST' })
      .then(function (validateVacationResult) {
        if (validateVacationResult.ErrorText && validateVacationResult.ErrorText.length != 0) {
          ESS.toasts.showToast(validateVacationResult.ErrorText, 'danger', false);
          reject();
        }
        else {
          functions.setValue('IsVacationPeriodBroken', validateVacationResult.AsupCheckResult);
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