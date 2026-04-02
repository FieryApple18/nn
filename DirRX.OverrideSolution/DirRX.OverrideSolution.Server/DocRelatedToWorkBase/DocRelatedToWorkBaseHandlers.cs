using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocRelatedToWorkBase;

namespace DirRX.OverrideSolution
{
  partial class DocRelatedToWorkBaseFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      if (Employees.Current == null)
        return base.Filtering(query, e);
      
      var employeeParams = ((Sungero.Domain.Shared.IExtendedEntity)Employees.Current).Params;
      object paramValue;
      
      if (employeeParams.TryGetValue(string.Format(NNCommons.PublicConstants.Module.RoleModel.AccessToBusinesUnitParamName, Employees.Current.Id), out paramValue))
      {
        var hasAccessToBusinessUnit = (bool)paramValue;
        
        if (!hasAccessToBusinessUnit)
        {
          // Получить список ИД документов, на которые у пользователя есть явные права в рамках его НОР.
          //HACK: Seleznyov_DV. Используется нелегальный LINQ запрос к таблице EdocReadAcess для ускорения работы фильтрации.
          var session = Sungero.Domain.Session.Current;
          
          if(Employees.Current != null && Employees.Current.Department != null)
            query = query.Where(x => !BusinessUnits.Equals(x.BusinessUnit, Employees.Current.Department.BusinessUnit) ||
                                (BusinessUnits.Equals(x.BusinessUnit, Employees.Current.Department.BusinessUnit) &&
                                 session.GetAll<Sungero.Content.Server.IDocumentReadAccess>().Any(r => r.EntityId == x.Id && r.RecipientId == Employees.Current.Id)));
          
          employeeParams.Remove(string.Format(NNCommons.PublicConstants.Module.RoleModel.AccessToBusinesUnitParamName, Users.Current.Id));
          
          return query;
        }
      }
      
      return base.Filtering(query, e);
    }

    public override IQueryable<T> PreFiltering(IQueryable<T> query, Sungero.Domain.PreFilteringEventArgs e)
    {
      var needFiltering = NNCommons.PublicFunctions.Module.NeedDocumentsFiltration();
      
      if (!needFiltering || Employees.Current == null)
        return base.PreFiltering(query, e);
      
      var businessUnitsIds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation.GetAvailableBusinessUnits(Employees.Current);
      
      if (!businessUnitsIds.Any(id => id == Employees.Current.BusinessUnit.Id))
        ((Sungero.Domain.Shared.IExtendedEntity)Employees.Current).Params
          .Add(string.Format(NNCommons.PublicConstants.Module.RoleModel.AccessToBusinesUnitParamName, Employees.Current.Id), false);
      
      return query.Where(x => x.BusinessUnit != null &&
                         (businessUnitsIds.Contains(x.BusinessUnit.Id) || x.BusinessUnit.Id == Employees.Current.BusinessUnit.Id));
    }
  }

  partial class DocRelatedToWorkBaseServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      // Выдача экземплярных прав для бизнес ролей.
      PublicFunctions.DocRelatedToWorkBase.IssueRightsForBusinessRoles(_obj);
      
      var errorText = NNCommons.PublicFunctions.Module.CheckPossibilityStatusChange(_obj);
      if (!string.IsNullOrEmpty(errorText))
        e.AddError(errorText);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      // В HR св-во продолжает использоваться. Есть метод SetLifeCycleStateByHRLifeCycleState который "правильно" 
      // синхронизирует значения из HRLifeCycleStateHRPro в LifeCycleState и InternalApprovalState.
      // Используется в HRLifeCycleStateHRProChanged официального документа.
      _obj.HRLifeCycleStateHRPro = DocRelatedToWorkBase.HRLifeCycleStateHRPro.Draft;
      _obj.TopDirRX = false;
      
      // HACK: Seleznyov_DV. Обход очистки НОР для ненумеруемых документов.
      // При сохранении документа вызывается коробочная разделяемая функция ClearBusinessUnit() официального документа,
      // которая чистит НОР для ненумеруемых видов документа, если поле НОР не отображается на форме.
      _obj.State.Properties.BusinessUnit.IsVisible = true;
    }

  }
}