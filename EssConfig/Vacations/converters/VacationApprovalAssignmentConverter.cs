using System;
using System.Globalization;
using Directum.SelfServiceOffice.Domain.Core;

/// <summary>
/// Конвертер для заданий VacationApprovalAssignment, VacationReworkAssignment.
/// </summary>
public class VacationApprovalAssignmentConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    string result = value?.ToString();
    if ("ru".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        // VacationApprovalAssignment
        "Complete" => "Согласовано",
        "ForRework" => "Отправлено на доработку",
        "Deny" => "Отказано",
        // VacationReworkAssignment
        "ForReapproving" => "Выполнено",
        "Abort" => "Прекращено",
        _ => result
      };
    }
    if ("en".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        // VacationApprovalAssignment
        "Complete" => "Approved",
        "ForRework" => "Rework",
        "Deny" => "Denied",
        // VacationReworkAssignment
        "ForReapproving" => "Completed",
        "Abort" => "Aborted",
        _ => result
      };
    }
    return result;
  }
}