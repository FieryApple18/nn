class printErrorReschedulingDates {
  execute(params) {
    const { functions, recallDate, vacationId, vacationKindIdsArray, vacationDurationArray, Compensation } = params;
    var urlTemplate = `/api/functions/CheckRecallVacationsDurations?recallDate=${recallDate}&vacationId=${vacationId}&vacationKindIds=[${vacationKindIdsArray}]&durations=[${vacationDurationArray}]&isCompensation=${Compensation}&isInfo=${false}`;
    ESS.network.sendRequest(urlTemplate).then(function (funcDurationErrors){
      if(funcDurationErrors.length > 0) {
        functions.removeError(window.funcDurationErrors);
        window.funcDurationErrors = functions.showError(funcDurationErrors);
      } else {
        functions.removeError(window.funcDurationErrors);
      }
    });
  }
}