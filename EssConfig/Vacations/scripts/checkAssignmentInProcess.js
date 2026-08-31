class checkAssignmentInProcess {
  execute(params) {
    const {assignmentId } = params;
    return new Promise((resolve, reject) => {
      ESS.network.sendRequest(`/api/functions/CheckAssignmentInProcess?assignmentId=${assignmentId}`)
      .then(function (isAssignmentInProcess) {
          if (!isAssignmentInProcess) {
            ESS.toasts.showToast(ESS.L('@HRPROSTATEMENTS.ASSIGNMENT_IS_ALREADY_COMPLETED'), ESS.enums.ToastType.ERROR, false);
            reject();
          }
          else 
          {
            resolve();
          }
      })
      .catch(reject);
    });
  }
}