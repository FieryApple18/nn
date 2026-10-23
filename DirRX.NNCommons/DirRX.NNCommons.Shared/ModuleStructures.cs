using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.NNCommons.Structures.Module
{

  #region Структуры для формирования отчета проверки корректности маршрутов.
  
  /// <summary>
  /// Структура входных параметров для пакетного запроса экспорта схем.
  /// </summary>
  [Public(Isolated=true)]
  partial class ExportPredictedSchemeRequest
  {
    /// <summary>
    /// Выдал ид
    /// </summary>
    public long AssignedById { get; set; }
    
    /// <summary>
    /// Исполнитель.
    /// </summary>
    public long AssigneeId { get; set; }
    
    /// <summary>
    /// Тип документа.
    /// </summary>
    public string DocumentTypeName { get; set; }
    
    /// <summary>Id вида документа.</summary>
    public long? DocDocumentKind { get; set; }

    /// <summary>Id автора (Author).</summary>
    public long? DocAuthor { get; set; }

    /// <summary>Id нашей организации (BusinessUnit).</summary>
    public long? DocBusinessUnit { get; set; }

    /// <summary>Id подразделения (Department).</summary>
    public long? DocDepartment { get; set; }

    /// <summary>Id подписанта (Signatory / OurSignatory).</summary>
    public long? DocSignatory { get; set; }

    /// <summary>Id создателя / подготовившего (CreatedBy / PreparedBy).</summary>
    public long? DocCreatedBy { get; set; }

    /// <summary>Id адресата (Addressee) — для Memo.</summary>
    public long? DocAddressee { get; set; }

    /// <summary>Id подготовившего (PreparedBy) — отдельное поле для Memo/Statement.</summary>
    public long? DocPreparedBy { get; set; }
    
    /// <summary>
    /// Тип задачи.
    /// </summary>
    public string TaskTypeName { get; set; }
  }

  /// <summary>
  /// Данные для формирования отчета.
  /// </summary>
  [Public(Isolated=true)]
  partial class ProcessKindSchemeResult
  {
    /// <summary>
    /// Ид варианта процесса.
    /// </summary>
    public long ProcessKindId { get; set; }
    
    /// <summary>
    /// Имя варианта процесса.
    /// </summary>
    public string ProcessKindName { get; set; }
    
    /// <summary>
    /// Список блоков схемы.
    /// </summary>
    public DirRX.NNCommons.Structures.Module.ISchemeExportInfo Scheme { get; set; }
    
    /// <summary>
    /// Ошибки.
    /// </summary>
    public string Error { get; set; }

    // --- Данные из карточки сотрудника ---
    
    /// <summary>
    /// Имя нашей организации.
    /// </summary>
    public string OrganizationName { get; set; }
    
    /// <summary>
    /// Ид раздела персоны.
    /// </summary>
    public long? PersonnelSectionId { get; set; }
    
    /// <summary>
    /// Имя раздела персоны.
    /// </summary>
    public string PersonnelSectionName { get; set; }
    
    /// <summary>
    /// Код раздела персоны.
    /// </summary>
    public string PersonnelNumber { get; set; }
    
    /// <summary>
    /// Имя сотрудника.
    /// </summary>
    public string EmployeeName { get; set; }
    
    /// <summary>
    /// Sap Ид должности.
    /// </summary>
    public string SapIdJobTitle { get; set; }
    
    /// <summary>
    /// Имя должности.
    /// </summary>
    public string JobTitleName { get; set; }
    
    /// <summary>
    /// Sap Ид подразделения.
    /// </summary>
    public string SapIdDepartment { get; set; }
    
    /// <summary>
    /// Имя подразделения.
    /// </summary>
    public string DepartmentName { get; set; }

    // --- Сформированные строки маршрута ---
    
    /// <summary>
    /// Найденный маршрут.
    /// </summary>
    public string ApprovalRoute { get; set; }
    
    /// <summary>
    /// Ненайденный маршрут.
    /// </summary>
    public string MissingSteps { get; set; }
    
  }
  
  /// <summary>
  /// Информация о блоках для задачи.
  /// </summary>
  [Public(Isolated=true)]
  partial class SchemeExportInfo
  {
    /// <summary>
    /// Ид задачи.
    /// </summary>
    public long TaskId { get; set; }
    
    /// <summary>
    /// Список блоков.
    /// </summary>
    public List<DirRX.NNCommons.Structures.Module.ISchemeBlockInfo> Blocks { get; set; }
  }

  /// <summary>
  /// Информация о блоке.
  /// </summary>
  [Public(Isolated=true)]
  partial class SchemeBlockInfo
  {
    /// <summary>
    /// Ид.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Название блока.
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Тип блока.
    /// </summary>
    public string BlockType { get; set; }
    
    /// <summary>
    /// Описание.
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Срок исполнения.
    /// </summary>
    public DateTime? PredictedDeadline { get; set; }
    
    /// <summary>
    /// Исполнители.
    /// </summary>
    public List<string> PredictedPerformers { get; set; }

    /// <summary>
    /// Список переходов.
    /// </summary>
    public List<DirRX.NNCommons.Structures.Module.ISchemeEdgeInfo> OutputEdges { get; set; }
  }
  
  /// <summary>
  /// Информация о переходе
  /// </summary>
  [Public(Isolated=true)]
  partial class SchemeEdgeInfo
  {
    /// <summary>
    /// Переход безусловный.
    /// </summary>
    public bool Unconditional { get; set; }
    
    /// <summary>
    /// Признак перехода Иначе.
    /// </summary>
    public bool Another { get; set; }
    
    /// <summary>
    /// Значение перехода.
    /// </summary>
    public string Value { get; set; }
  }

  /// <summary>
  /// Хранит пару предсозданных тестовых ресурсов (документ + задача) для переиспользования.
  /// </summary>
  partial class TestResource
  {
    public Sungero.Docflow.IOfficialDocument Document { get; set; }
    public Sungero.Workflow.ITask Task { get; set; }
  }
  
  #endregion
  
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