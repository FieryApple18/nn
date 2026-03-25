using System;
using Sungero.Core;

namespace DirRX.NNVacations.Constants
{
  public static class AcquiantanceVacScheduleReport
  {

    /// <summary>
    /// Guid типа сущности График отпусков.
    /// </summary>
    public const string VacationScheduleDocumentDiscriminator = "f8cad65f-5981-4fa8-b57b-2efe1dc47c1e";
    
    /// <summary>
    /// Guid типа сущности Сотрудник.
    /// </summary>
    public const string EmployeeDiscriminator = "b7905516-2be5-4931-961c-cb38d5677565";

    /// <summary>
    /// Guid типа сущности Подразделение.
    /// </summary>
    public const string DepartmentDiscriminator = "61b1c19f-26e2-49a5-b3d3-0d3618151e12";

    /// <summary>
    /// Имя временной таблицы.
    /// </summary>
    public const string SourceTableName = "dirrx_reports_dephierarchy";

  }
}