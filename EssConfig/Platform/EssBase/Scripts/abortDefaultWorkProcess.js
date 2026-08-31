class abortDefaultWorkProcess {
  execute(args) {
    const [parent, system, processName, processId] = args;
    const params = {
      title: ESS.L('#DEFAULT.CONFIRMATION'),
      text: ESS.L('@ESSBASE.ABORT_DEFAULT_WORK_PROCESS_TEXT'),
      buttonsType: ESS.enums.ButtonsType.YES_NO,
      onCloseCallback: async button => {
        if (button === ESS.enums.ConfirmationDialogButtonName.YES) {
          const overlayId = `abortDefaultWorkProcess_${new Date().getTime()}`;
          ESS.overlay.showOverlay(overlayId);
          try {
            await ESS.methods.workProcessMethods.StopWorkProcessMethod(system, processName, processId);
            ESS.closeElement(parent);
            ESS.widgets.updateWidgets();
            ESS.toasts.showToast(ESS.L('@ESSBASE.ABORT_DEFAULT_WORK_PROCESS_SUCCESS_MESSAGE'), ESS.enums.ToastType.SUCCESS);
          } catch (error) {
            ESS.toasts.showErrorToastWithLog(ESS.L('@ESSBASE.ABORT_DEFAULT_WORK_PROCESS_ERROR'), error);
          }
          ESS.overlay.hideOverlay(overlayId);
        }
      },
      dialogSettings: {
        small: true,
        height: 'auto',
      },
    };
    ESS.dialogs.showConfirmationDialog(params);
  }
}
