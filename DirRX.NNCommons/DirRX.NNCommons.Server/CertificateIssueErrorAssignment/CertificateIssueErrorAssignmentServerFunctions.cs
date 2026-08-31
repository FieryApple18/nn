using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.CertificateIssueErrorAssignment;

namespace DirRX.NNCommons.Server
{
  partial class CertificateIssueErrorAssignmentFunctions
  {

    /// <summary>
    /// Построить модель состояния инструкции.
    /// </summary>
    /// <returns>Модель состояния.</returns>
    [Remote(IsPure = true)]
    public StateView GetCertificateIssueErrorAssignmentState()
    {
      var instruction = DirRX.SignPlatform.CertificateIssueTasks.Resources.CertificateIssueErrorInstruction;    
      return DirRX.OverrideSolution.Module.SignPlatform.PublicFunctions.Module.GetCertificateIssueErrorAssignmentStateView(instruction);
    }

  }
}