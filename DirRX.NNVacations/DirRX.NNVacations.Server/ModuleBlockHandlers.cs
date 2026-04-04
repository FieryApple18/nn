using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace DirRX.NNVacations.Server.NNVacationsBlocks
{
  partial class CloseSubstitutionsScriptBlockHandlers
  {

    public virtual void CloseSubstitutionsScriptBlockExecute()
    {
      DirRX.NNVacations.PublicFunctions.Module.CloseSubstitutionsFromTask(_obj);
    }
  }

  partial class UpdateSubstitutionsScriptBlockHandlers
  {

    public virtual void UpdateSubstitutionsScriptBlockExecute()
    {
      DirRX.NNVacations.PublicFunctions.Module.UpdateSubstitutionsFromTask(_obj);
    }
  }

  partial class CreateSubstitutionsScriptBlockHandlers
  {

    public virtual void CreateSubstitutionsScriptBlockExecute()
    {
      DirRX.NNVacations.PublicFunctions.Module.CreateSubstitutionsFromTask(_obj);
    }
  }

}