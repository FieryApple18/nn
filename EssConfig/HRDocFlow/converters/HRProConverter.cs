using System;
using System.Globalization;
using Directum.SelfServiceOffice.Domain.Core;

/// <summary>
/// Конвертер для DocumentApprovalAssignment, HRAssignment, ESignAssignment, EAcquaintanceAssignment,
/// ConsentAssignment, AgreementAssignment.
/// </summary>
public class HRProConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    string result = value?.ToString();
    if ("ru".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        "Complete" => "Выполнено",
        "Approved" => "Согласовано",
        "ForRework" => "На доработку",
        "ForRevision" => "На доработку",
        "Aborted" => "Прекращено",
        "Terminate" => "Прекращено",
        "Refuse" => "Отказано",
        "Reject" => "Отказано",
        "Sign" => "Подписано",
        "Accept" => "Принято",
        _ => result
      };
    }
    if ("en".Equals(culture.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
    {
      return result switch
      {
        "Complete" => "Performed",
        "Approved" => "Approved",
        "ForRework" => "For rework",
        "ForRevision" => "For revision",
        "Aborted" => "Aborted",
        "Terminate" => "Terminate",
        "Refuse" => "Denied",
        "Reject" => "Denied",
        "Sign" => "Signed",
        "Accept" => "Accepted",
        _ => result
      };
    }
    return result;
  }
}