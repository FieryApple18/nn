class updateWidgetMyVacations {
  execute(personId) {
  var urlTemplate = `/api/functions/UpdateWidgetMyVacations?personId=${personId}`;
    ESS.network.sendRequest(urlTemplate).then(function (errorText) {
      if (errorText && errorText.length != 0) {
        ESS.toasts.showToast(errorText, 'danger', false);
      } else {
        ESS.widgets.updateWidgets();
      }
    })
    .catch(function (error) {
      ESS.toasts.showToast(error.message, 'danger', false);
    });
  }
}