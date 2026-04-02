class validateRecallVacationData {
  execute(params) {
    const { functions, initiatorId, statementId, vacationTypeId, beginDateRecall, endDateRecall, durationRecall, beginDateShift, endDateShift, durationShift, dayCountCompensation, currentVacationData } = params;
    return new Promise(function (resolve, reject) {
      var urlValidateRecallVacationData = `/api/functions/NNValidateRecallVacationData?employeeId=${initiatorId}&documentId=${statementId}&typeId=${vacationTypeId}&beginRecall=${beginDateRecall}&endRecall=${endDateRecall}&daysRecall=${durationRecall}&beginShift=${beginDateShift}&endShift=${endDateShift}&daysShift=${durationShift}&daysCompensation=${dayCountCompensation}&currentVacationData=${currentVacationData}`;
      return ESS.network.sendRequest(urlValidateRecallVacationData)
      .then(function (validateVacationResult) {
        if (validateVacationResult.ErrorText && validateVacationResult.ErrorText.length != 0) {
          ESS.toasts.showToast(validateVacationResult.ErrorText, 'danger', false);
          reject();
        }
        else {
          functions.setValue('IsVacationPeriodBroken', validateVacationResult.IsVacationPeriodBroken);
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