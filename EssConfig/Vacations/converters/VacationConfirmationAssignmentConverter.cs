using System;
using System.Globalization;
using Directum.SelfServiceOffice.Domain.Core;

/// <summary>
/// Конвертер для заданий VacationConfirmationAssignment, VacationAlignDateAssignment.
/// </summary>
public class VacationConfirmationAssignmentConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    string result = value?.ToString();
    if ("ru".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        // VacationConfirmationAssignment
        "Complete" => "Подтверждено",
        "Shift" => "Выполнено",
        // VacationAlignDateAssignment
        "GetVacation" => "Выполнено",
        "Skip" => "Выполнено",
        // NNVacationConfirmationAssignment
        "Confirm" => "Подтверждено",
        _ => result
      };
    }
    if ("en".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        // VacationConfirmationAssignment
        "Complete" => "Approved",
        "Shift" => "Completed",
        // VacationAlignDateAssignment
        "GetVacation" => "Completed",
        "Skip" => "Completed",
        _ => result
      };
    }
    return result;
  }
}