using System;
using System.Globalization;
using Directum.SelfServiceOffice.Domain.Core;

/// <summary>
/// Конвертер для получения кнопки по умолчанию для виджета с отправленными заявлениями.
/// </summary>
public class StatementTasksDefaultAction : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    switch (value)
	{
      case "В работе":
      case "На согласовании": 
      case "На доработке": 
      case" На исполнении":
      case "In process":
      case "Under approval":
      case "On rework":
      case "Being executed":
	    return "abort";
      default:
	    return "";				  
	}
  }
}