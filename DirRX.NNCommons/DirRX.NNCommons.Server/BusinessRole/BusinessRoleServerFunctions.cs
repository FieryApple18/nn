using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.NNCommons.BusinessRole;

namespace DirRX.NNCommons.Server
{
  partial class BusinessRoleFunctions
  {

    /// <summary>
    /// Получить расширенный список ИД бизнесс ролей с учётом иерархии справочника Бизнесс процессы.
    /// </summary>
    /// <param name="businessRolesIds">Исходный список ИД бизнесс ролей.</param>
    /// <returns>Расширенный список бизнесс ролей с учётом иерархии справочника Бизнесс процессы.</returns>
    [Public]
    public static List<long> GetBusinessRolesByProccessHierarchy(List<long> businessRolesIds)
    {
      var idsList = new List<long>();
      using (var connection = SQL.CreateConnection())
      {
        var command = connection.CreateCommand();
        command.CommandText = string.Format(Queries.Module.GetBusinessRolesByProccessRecursive, string.Join(",", businessRolesIds));
        using (var rds = command.ExecuteReader())
        {
          if (rds != null)
            while (rds.Read())
          {
            idsList.Add(rds.GetInt64(0));
          }
          
          rds.Close();
        };
      };
      
      return idsList;
    }
    
    /// <summary>
    /// Получить список дублирующихся записей.
    /// </summary>
    /// <returns></returns>
    [Public, Remote(IsPure = true)]
    public virtual IQueryable<IBusinessRole> GetDuplicates()
    {
      
      if (_obj.RoleType == RoleType.ByKind)
        return DirRX.NNCommons.BusinessRoles.GetAll(c =>  c.DocumentKind.Equals(_obj.DocumentKind) &&
                                                    c.Top == _obj.Top &&
                                                    !c.Equals(_obj));
      else if (_obj.RoleType == RoleType.ByProcess)
        return DirRX.NNCommons.BusinessRoles.GetAll(c =>  c.Process.Equals(_obj.Process) &&
                                                    c.Top == _obj.Top &&
                                                    !c.Equals(_obj));
      return null;
    }
  }
}