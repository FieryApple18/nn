class validateShiftVacationData {
  execute(params) {
    const { functions, initiatorId, statementId, noticeId, vacationTypeIdsArray, beginDateArray, endDateArray, durationArray, formattedVacationsOld, assignmentId } = params;
    return new Promise(function (resolve, reject) {
    var body = JSON.stringify({
              employeeId : new Number(initiatorId),
              documentId : new Number(statementId),
              noticeId : new Number(noticeId),
              vacationTypeIds : vacationTypeIdsArray,
              beginDates : beginDateArray,
              endDates : endDateArray,
              durations : durationArray,
              oldVacationsJson : formattedVacationsOld,
              assignmentId : new Number(assignmentId)
      });
      return ESS.network.sendRequest('/api/functions/NNValidateShiftVacationData', { body: body, method: 'POST' })
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