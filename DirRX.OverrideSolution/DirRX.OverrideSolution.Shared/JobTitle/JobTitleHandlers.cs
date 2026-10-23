using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.JobTitle;

namespace DirRX.OverrideSolution
{
  partial class JobTitleSharedHandlers
  {

    public virtual void IsTopDirRXChanged(Sungero.Domain.Shared.BooleanPropertyChangedEventArgs e)
    {
      if (e.NewValue != null && !Equals(e.NewValue, e.OriginalValue))
        _obj.EvaluationGroupDirRX = e.NewValue == true ? OverrideSolution.JobTitle.EvaluationGroupDirRX.Top100 : OverrideSolution.JobTitle.EvaluationGroupDirRX.OutOfCategories;
    }

  }
}