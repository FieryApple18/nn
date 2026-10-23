using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.OverrideSolution.Module.Shell.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      base.Initializing(e);
      
      GrantRightOnFolders();
    }
    
    /// <summary>
    /// Выдать права на спец. папки модуля.
    /// </summary>
    public static void GrantRightOnFolders()
    {
      Logger.Debug("Init: Grant right on shell special folders to all users.");
      
      var allUsers = Roles.AllUsers;
      Shell.SpecialFolders.OnEventHoldingDirRX.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
      Shell.SpecialFolders.OnEventHoldingDirRX.AccessRights.Save();
    }
  }
}
