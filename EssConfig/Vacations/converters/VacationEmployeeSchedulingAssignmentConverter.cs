using System;
using System.Globalization;
using Directum.SelfServiceOffice.Domain.Core;

/// <summary>
/// Конвертер для VacationEmployeeSchedulingAssignment.
/// </summary>
public class VacationEmployeeSchedulingAssignmentConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    string result = value?.ToString();
    if ("ru".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        "Complete" => "Выполнено",
        "Completed" => "Выполнено",
        _ => result
      };
    }
    if ("en".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        "Complete" => "Performed",
        "Completed" => "Performed",
        _ => result
      };
    }
    return result;
  }
}