using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNCommons.Structures.Module
{

  /// <summary>
  /// Структура для передачи информации о сотруднике в ЛК.
  /// </summary>
  [Public(Isolated=true)]
  partial class EmployeeGoAndTopInfo
  {
    /// <summary>
    /// Признак того, что сотрудник обладает привилегиями ТОПа.
    /// </summary>
    public bool IsTop { get; set; }
    
    /// <summary>
    /// Признак того, что сотрудник закреплён за организацией ГО.
    /// </summary>
    public bool IsGo { get; set; }
  }

  /// <summary>
  /// Универсальный элемент списка для параметра FacilityParameterQuery.
  /// </summary>
  [Public]
  partial class OptionItem
  {
    /// <summary>
    /// ИД записи.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Отображаемое значение.
    /// </summary>
    public string DisplayValue { get; set; }
  }

  /// <summary>
  /// Структура для замены текстовой метки на новое текстовое значение.
  /// </summary>
  [Public(Isolated=true)]
  partial class TextReplacement
  {
    /// <summary>
    /// Исходное значение.
    /// </summary>
    public string OldValue { get; set; }
    
    /// <summary>
    /// Новое значение.
    /// </summary>
    public string NewValue { get; set; }
    
    /// <summary>
    /// Признак необходимости создания нового параграфа (абзаца).
    /// </summary>
    public bool IsNewParagraph { get; set; }
  }

  /// <summary>
  /// Данные о сотруднике.
  /// </summary>
  [Public]
  partial class EmployeeInfo
  {
    /// <summary>
    /// ИД сотрудника.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// ФИО сотрудника.
    /// </summary>
    public string Name { get; set; }
  }
  
  /// <summary>
  /// Данные по шаблону заявлений и вида документа для ЛК.
  /// </summary>
  [Public(Isolated = true)]
  partial class StatementData
  {
    /// <summary>
    /// ИД шаблона заявления.
    /// </summary>
    public long TemplateId { get; set; }
    
    /// <summary>
    /// ИД вида документа.
    /// </summary>
    public long DocumentKindId { get; set; }
    
    /// <summary>
    /// Текст ошибки.
    /// </summary>
    public string Error { get; set; }
  }
  
  /// <summary>
  /// Структура для получения данных о задаче, данные будут преобразовываться в json и использоваться в Личном Кабинете в виджете с запущенными задачами.
  /// </summary>
  [Public(Isolated = true)]
  partial class TaskDataForWidget
  {
    /// <summary>
    /// ИД задачи.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Имя типа задачи.
    /// </summary>
    public string ProcessName { get; set; }
    
    /// <summary>
    /// Тема.
    /// </summary>
    public string Subject { get; set; }
    
    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime Created { get; set; }
    
    /// <summary>
    /// Статус.
    /// </summary>
    public string Status { get; set; }
  }
  
  /// <summary>
  /// Данные о документе из ЛК.
  /// </summary>
  [Public(Isolated = true)]
  partial class DocumentInfoFromLK
  {
    /// <summary>
    /// ИД документа.
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Имя документа.
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Имя целевой системы.
    /// </summary>
    public string System { get; set; }
    
    /// <summary>
    /// Тип документа.
    /// </summary>
    public string Type { get; set; }
  }
  
  /// <summary>
  /// Структура с информацией по обобщённой роли.
  /// </summary>
  [Public]
  partial class GeneralRoleInfo
  {
    /// <summary>
    /// Обобщённая роль.
    /// </summary>
    public IGeneralRole GeneralRole { get; set; }
    
    /// <summary>
    /// Признак успешной установки блокироваки на роль.
    /// </summary>
    public bool IsLocked { get; set; }
  }
  
  /// <summary>
  /// Данные для заполнения табличной части в шаблоне документа.
  /// </summary>
  [Public(Isolated = true)]
  partial class TableData
  {
    /// <summary>
    /// Имя тега, обозначающего номер строки - является ключевым для поиска строки, которую копируем. Если там содержится ##, то значение ячейки заполняется порядковым номером.
    /// </summary>
    public string NumberKey { get; set; }

    /// <summary>
    /// Список данных для строки в формате "имя тега" - "значение ячейки". Каждый элемент списка - это строка. Каждый элемент словаря - ячейка строки.
    /// </summary>
    public System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, string>> Values { get; set; }
  }
  
  /// <summary>
  /// Общий результат проверки документа.
  /// </summary>
  [Public(Isolated = true)]
  partial class DocumentVerifyInfo
  {
    /// <summary>
    /// ИД документа.
    /// </summary>
    public long DocumentId { get; set; }
    
    /// <summary>
    /// Имя документа.
    /// </summary>
    public string DocumentName { get; set; }
    
    /// <summary>
    /// Ссылка на документ.
    /// </summary>
    public string Hyperlink { get; set; }
    
    /// <summary>
    /// Версии документа.
    /// </summary>
    public string Versions { get; set; }
    
    /// <summary>
    /// Текст ошибки проверки версий.
    /// </summary>
    public string Error { get; set; }
    
    /// <summary>
    /// Статус проверки документа.
    /// </summary>
    public bool Valid { get; set; }
  }
  
  /// <summary>
  /// Результат проверки версии документа.
  /// </summary>
  [Public(Isolated = true)]
  partial class VersionVerifyInfo
  {
    /// <summary>
    /// Номер версии.
    /// </summary>
    public int? Number { get; set; }
    
    /// <summary>
    /// Статус проверки Body версии.
    /// </summary>
    public bool Valid { get; set; }
    
    /// <summary>
    /// Текст ошибки проверки Body.
    /// </summary>
    public string Error { get; set; }
    
    /// <summary>
    /// Признак что проверяется публичное тело документа.
    /// </summary>
    public bool IsPublicBody { get; set; }
  }
}