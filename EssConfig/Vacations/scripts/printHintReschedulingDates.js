class printHintReschedulingDates {
  execute(params) {
    const { field, functions, recallDate, vacationId, vacationKindIdsArray, vacationDurationArray, Compensation } = params;
    var urlTemplate = `/api/functions/CheckRecallVacationsDurations?recallDate=${recallDate}&vacationId=${vacationId}&vacationKindIds=[${vacationKindIdsArray}]&durations=[${vacationDurationArray}]&isCompensation=${Compensation}&isInfo=${true}`;
    ESS.network.sendRequest(urlTemplate).then(function (durationInfo) {
      if (durationInfo.length > 0) {
        functions.setValue(field, durationInfo);
      }
      else{
        functions.setValue(field, null);
      }
    });
  }
}