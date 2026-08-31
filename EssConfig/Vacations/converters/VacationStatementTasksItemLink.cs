using System;
using System.Globalization;
using Directum.SelfServiceOffice.Domain.Core;

/// <summary>
/// Конвертер для получения ссылки на задачу из виджета с отправленными заявлениями.
/// </summary>
public class VacationStatementTasksItemLink : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    var itemData = (parameter as IValueObject);
    var processName = itemData["ProcessName"];
    return $"/workprocesses/EssBase/{processName}/{value}";
  }
}