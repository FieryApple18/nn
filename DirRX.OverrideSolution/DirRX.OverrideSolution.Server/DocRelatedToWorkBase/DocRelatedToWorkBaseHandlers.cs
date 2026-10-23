using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.DocRelatedToWorkBase;

namespace DirRX.OverrideSolution
{
  partial class DocRelatedToWorkBasePersonnelSectionDirRXPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> PersonnelSectionDirRXFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_obj.PreparedBy != null)
        query = query.Where(q => Equals(q.BusinessUnit, _obj.PreparedBy.BusinessUnit));
      
      return query;
    }
  }

  partial class DocRelatedToWorkBaseFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      var needDisableFiltering = PublicFunctions.DocRelatedToWorkBase.NeedDisableFilteringForRoleModel();
      
      if (Employees.Current == null || needDisableFiltering)
        return base.Filtering(query, e);
      
      // Проверяем, есть ли параметр, указывающий на отсутствие доступа к своей НОР сотрудника.
      // Параметр добавляется в методе Prefiltering, если НОР сотрудника отсутствует в списке доступных НОР.
      var employeeParams = ((Sungero.Domain.Shared.IExtendedEntity)Employees.Current).Params;
      object paramValue;
      if (employeeParams.TryGetValue(string.Format(NNCommons.PublicConstants.Module.RoleModel.AccessToBusinesUnitParamName, Employees.Current.Id), out paramValue))
      {
        // Если доступа к своей НОР нет, то применяем специальную фильтрацию
        var hasAccessToBusinessUnit = (bool)paramValue;
        if (!hasAccessToBusinessUnit)
        {
          var businessUnitsIds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
            .GetAvailableBusinessUnits(Employees.Current);
          
          var businessUnitsWithGeneralLimitation = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
            .GetBusinessUnitsWithGeneralLimitation(Employees.Current, businessUnitsIds);
          
          var allowedSectionIds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
            .GetAllowedPersonnelSectionIds(Employees.Current, businessUnitsIds, businessUnitsWithGeneralLimitation);
          
          // Получить список ИД документов, на которые у пользователя есть явные права в рамках его НОР.
          //HACK: Seleznyov_DV. Используется нелегальный LINQ запрос к таблице EdocReadAcess для ускорения работы фильтрации.
          var session = Sungero.Domain.Session.Current;
          
          if(Employees.Current != null && Employees.Current.Department != null)
            query = query.Where(x => (x.PersonnelSectionDirRX != null && allowedSectionIds.Contains(x.PersonnelSectionDirRX.Id)) || !BusinessUnits.Equals(x.BusinessUnit, Employees.Current.Department.BusinessUnit) ||
                                (BusinessUnits.Equals(x.BusinessUnit, Employees.Current.Department.BusinessUnit) &&
                                 session.GetAll<Sungero.Content.Server.IDocumentReadAccess>().Any(r => r.EntityId == x.Id && r.RecipientId == Employees.Current.Id)));
          
          // Удаляем параметр после использования, чтобы он не влиял на последующие запросы
          employeeParams.Remove(string.Format(NNCommons.PublicConstants.Module.RoleModel.AccessToBusinesUnitParamName, Users.Current.Id));
          
          return query;
        }
      }
      
      return base.Filtering(query, e);
    }

    public override IQueryable<T> PreFiltering(IQueryable<T> query, Sungero.Domain.PreFilteringEventArgs e)
    {
      var needFiltering = NNCommons.PublicFunctions.Module.NeedDocumentsFiltration();
      var employee = Employees.Current;
      if (!needFiltering || employee == null)
        return base.PreFiltering(query, e);

      // 1. Получаем доступные НОР
      var businessUnitsIds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
        .GetAvailableBusinessUnits(employee);

      // 2. Фильтрация по НОР
      query = query.Where(x => x.BusinessUnit != null &&
                          (businessUnitsIds.Contains(x.BusinessUnit.Id) || x.BusinessUnit.Id == employee.BusinessUnit.Id));

      // 3. Определяем НОР с общим ограничением (без раздела)
      var businessUnitsWithGeneralLimitation = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
        .GetBusinessUnitsWithGeneralLimitation(employee, businessUnitsIds);

      // 4. Получаем все разрешенные разделы
      var allowedSectionIds = NNCommons.PublicFunctions.DocumentsVisabilityLimitation
        .GetAllowedPersonnelSectionIds(employee, businessUnitsIds, businessUnitsWithGeneralLimitation);

      // 5. Если НОР сотрудника нет в общих ограничениях, добавляем параметр
      if (!businessUnitsWithGeneralLimitation.Any(id => id == employee.BusinessUnit.Id))
        ((Sungero.Domain.Shared.IExtendedEntity)employee).Params
          .Add(string.Format(NNCommons.PublicConstants.Module.RoleModel.AccessToBusinesUnitParamName, employee.Id), false);
      
      // 6. Фильтрация по разделам.
      query = query.Where(x =>
                          // Фильтруем по НОР сотрудника на случай, если есть экземплярные права.
                          employee.BusinessUnit.Id == x.BusinessUnit.Id ||
                          
                          // НОР с общим ограничением -> все разделы доступны
                          businessUnitsWithGeneralLimitation.Contains(x.BusinessUnit.Id) ||
                          
                          // Для остальных НОР -> проверяем по списку разрешенных разделов
                          (x.PersonnelSectionDirRX != null &&
                           allowedSectionIds.Contains(x.PersonnelSectionDirRX.Id))
                         );

      return query;
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
      _obj.IsNGDDirRX = false;
      
      // HACK: Seleznyov_DV. Обход очистки НОР для ненумеруемых документов.
      // При сохранении документа вызывается коробочная разделяемая функция ClearBusinessUnit() официального документа,
      // которая чистит НОР для ненумеруемых видов документа, если поле НОР не отображается на форме.
      _obj.State.Properties.BusinessUnit.IsVisible = true;
    }

  }
}