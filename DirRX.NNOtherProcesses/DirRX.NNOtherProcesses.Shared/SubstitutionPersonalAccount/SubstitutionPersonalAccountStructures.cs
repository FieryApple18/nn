using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNOtherProcesses.Structures.SubstitutionPersonalAccount
{

  /// <summary>
  /// Информация о замещении.
  /// </summary>
  [Public]
  partial class SubstitutionInfo
  {
    /// <summary>
    /// Система.
    /// </summary>
    public DirRX.NNOtherProcesses.ISubstitutionSystem SubstitutionSystem { get; set; }

    /// <summary>
    /// Замещающий.
    /// </summary>
    public DirRX.OverrideSolution.IEmployee Substitute { get; set; }

    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateTime EndDate { get; set; }
  }

}