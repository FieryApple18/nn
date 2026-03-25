using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace DirRX.OverrideSolution.Module.EssPlatform.Server
{
  public partial class ModuleInitializer
  {
    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      base.Initializing(e);
      this.CreateRoles();
    }
    
    /// <summary>
    /// Создать предопределенные роли.
    /// </summary>
    public void CreateRoles()
    {
      InitializationLogger.Debug("Init OverrideSolution.EssPlatform module: Create Roles.");
      
      // Видимость плитки в ЛК "Отпуска".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileMyVacations,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileMyVacations,
                                                                      Constants.Module.RolesGuid.ShowTileMyVacations);
      
      // Видимость плитки в ЛК "Ежегодный отпуск".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileGetAnnualVacation,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileGetAnnualVacation,
                                                                      Constants.Module.RolesGuid.ShowTileGetAnnualVacation);
      
      // Видимость плитки в ЛК "Отпуск без сохранения заработной платы".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileGetVacationWithoutPay,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileGetVacationWithoutPay,
                                                                      Constants.Module.RolesGuid.ShowTileGetVacationWithoutPay);
      
      // Видимость плитки в ЛК "Остальные отпуска".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileGetOtherVacation,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileGetOtherVacation,
                                                                      Constants.Module.RolesGuid.ShowTileGetOtherVacation);
      
      // Видимость плитки в ЛК "Перенос отпуска".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileNNShiftVacationByStatement,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileNNShiftVacationByStatement,
                                                                      Constants.Module.RolesGuid.ShowTileNNShiftVacationByStatement);
      
      // Видимость плитки в ЛК "Компенсация отпуска".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileCompensationVacation,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileCompensationVacation,
                                                                      Constants.Module.RolesGuid.ShowTileCompensationVacation);
      
      // Видимость плитки в ЛК "Отзыв сотрудника из отпуска".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileNNRecallVacationEmployee,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileNNRecallVacationEmployee,
                                                                      Constants.Module.RolesGuid.ShowTileNNRecallVacationEmployee);
      
      // Видимость плитки в ЛК "Прерывание отпуска".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileNNSelfRecallVacationEmployee,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileNNSelfRecallVacationEmployee,
                                                                      Constants.Module.RolesGuid.ShowTileNNSelfRecallVacationEmployee);
      
      // Видимость плитки в ЛК "Заявления".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileStatements,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileStatements,
                                                                      Constants.Module.RolesGuid.ShowTileStatements);
      
      // Видимость плитки в ЛК "Заявка на работу в выходной/праздничный день".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileWeekendWork,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileWeekendWork,
                                                                      Constants.Module.RolesGuid.ShowTileWeekendWork);
      
      // Видимость плитки в ЛК "Заявка на работу в сверхурочное время".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileOvertimeWork,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileOvertimeWork,
                                                                      Constants.Module.RolesGuid.ShowTileOvertimeWork);
      
      // Видимость плитки в ЛК "Заявление на выплату по ЛНА".
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleName_ShowTileGetLrdPayment,
                                                                      DirRX.OverrideSolution.Module.EssPlatform.Resources.RoleDescription_ShowTileGetLrdPayment,
                                                                      Constants.Module.RolesGuid.ShowTileGetLrdPayment);
    }
  }
}
