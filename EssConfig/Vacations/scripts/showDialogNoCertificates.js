class showDialogNoCertificates {
  execute(params) {
    const dialogId = 'no-certificates-dialog';
    const warningText = {
      type: ESS.enums.DialogItemType.TEXT,
      name: 'warningText',
      properties: {
        text: ESS.L('@HRPROVACATIONS.FACILITIES_DIALOG_NO_CERTIFICATES_ERROR'),
      },
    };
    const items = [warningText];
    const okButton = {
      name: 'ok-button',
      caption: 'ОК',
      onClick: form => {
        ESS.dialogs.closeDialog(dialogId);
      },
      isPrimary: true,
    };
    const buttons = [okButton];
    ESS.dialogs.createDialog({
      id: dialogId,
      title: ESS.L('@HRPROVACATIONS.ERROR_COMMON'),
      items,
      buttons,
      properties: {
        height: '200px',
      }
    });
  }
}