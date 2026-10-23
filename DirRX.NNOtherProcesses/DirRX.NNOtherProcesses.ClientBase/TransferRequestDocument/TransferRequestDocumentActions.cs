using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNOtherProcesses.TransferRequestDocument;

namespace DirRX.NNOtherProcesses.Client
{
  partial class TransferRequestDocumentActions
  {
    public virtual void FillFromSAP(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      
    }

    public virtual bool CanFillFromSAP(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}