using System;
using System.Globalization;
using Directum.SelfServiceOffice.Domain.Core;

/// <summary>
/// Конвертер для заданий VacationChangeApprovalAssignment, VacationChangeReworkAssignment, VacationChangeSignAssignment.
/// </summary>
public class VacationChangeAssignmentConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    string result = value?.ToString();
    if ("ru".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        // VacationChangeApprovalAssignment
        "Approve" => "Согласовано",
        "ForRework" => "Отправлено на доработку",
        "Deny" => "Отказано",
        // VacationChangeReworkAssignment
        "ForReapproving" => "Выполнено",
        "Abort" => "Прекращено",
        // VacationChangeSignAssignment
        "Sign" => "Подписано",
        "Refuse" => "Отказано",
        _ => result
      };
    }
    if ("en".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        // VacationChangeApprovalAssignment
        "Approve" => "Approved",
        "ForRework" => "Rework",
        "Deny" => "Denied",
        // VacationChangeReworkAssignment
        "ForReapproving" => "Completed",
        "Abort" => "Aborted",
        // VacationChangeSignAssignment
        "Sign" => "Signed",
        "Refuse" => "Denied",
        _ => result
      };
    }
    return result;
  }
}