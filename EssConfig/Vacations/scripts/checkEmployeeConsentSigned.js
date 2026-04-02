class checkEmployeeConsentSigned {
    execute(params) {
      const { functions, context, employeeId } = params;
      functions.removeError(context.errorInitiator);
      ESS.network.sendRequest(`/api/functions/CheckEmployeeConsentSigned?employeeId=${employeeId}`)
        .then(function (isConsentSigned) {
            if (!isConsentSigned) {
                context.errorInitiator = functions.showError(ESS.L('@HRPROVACATIONS.FACILITIES_GETVACATION_CONSENT_ERROR'));
            }
        })
        .catch(function (error) {
            ESS.toasts.showToast(error.message, ESS.enums.ToastType.ERROR, false);
        });
    }
}