using System;
using Sungero.Core;

namespace DirRX.NNVacations.Constants
{
  public static class Module
  {

    /// <summary>
    /// Префикс типа отпуска для лимитов отпусков, используемых в виджете MyVacation.
    /// </summary>
    public const string vacationLimitPrefix = "vacation";
    
    #region Группы константы.
    /// <summary>
    /// Группы константы.
    /// </summary>
    [Public]
    public static class ConstantsGroup
    {
      /// <summary>
      /// Отпуска. Регистрация отпусков.
      /// </summary>
      [Public]
      public static class RegistrationOfVacations
      {
        [Public]
        public const string Guid = "fd9e7cc5-7c4f-4c2e-8549-77cfd741faa6";
        public const string Name = "Отпуска. Регистрация отпусков";
      }
      
      /// <summary>
      /// Отпуска. Ведение графика отпусков.
      /// </summary>
      [Public]
      public static class MaintenanceOfVacationSchedule
      {
        [Public]
        public const string Guid = "53abf78d-bf15-4960-940c-24f1fb4cf63";
        public const string Name = "Отпуска. Ведение графика отпусков";
      }
    }
    #endregion
    
    #region Прикладные константы

    [Public]
    public static class ConstantsSetting
    {
      /// <summary>
      /// Отпуска. ИД типа отпуска Учебный (с сохранением ЗП).
      /// </summary>
      [Public]
      public static class StudyWithSalaryVacationTypeId
      {
        [Public]
        public const string Guid = "95f02861-74c3-4e69-b5d5-a90f7c8f97f2";
        public const string Name = "Отпуска. ИД типа отпуска Учебный (с сохранением ЗП)";
      }

      /// <summary>
      /// Отпуска. ИД типа отпуска Учебный (без сохранения ЗП).
      /// </summary>
      [Public]
      public static class StudyWithoutSalaryVacationTypeId
      {
        [Public]
        public const string Guid = "8257852c-388d-499c-819e-139f558933c7";
        public const string Name = "Отпуска. ИД типа отпуска Учебный (без сохранения ЗП)";
      }

      /// <summary>
      /// Формат документа "Приказ на ежегодный отпуск по заявлению".
      /// </summary>
      [Public]
      public static class AnnualVacationDocumentExtension
      {
        [Public]
        public const string Guid = "653d2e5a-0151-487d-91d8-56605e7506d3";
        public const string Name = "Формат документа \"Приказ на ежегодный отпуск\"";
      }

      /// <summary>
      /// Формат документа "Письмо в авиакомпанию".
      /// </summary>
      [Public]
      public static class LetterToAirlineExtension
      {
        [Public]
        public const string Guid = "14f1e8a3-6a6f-464d-941f-c6c99e445b40";
        public const string Name = "Формат документа \"Письмо в авиакомпанию\"";
      }

      /// <summary>
      /// Формат документа "Письмо в ОВД".
      /// </summary>
      [Public]
      public static class LetterToOVDExtension
      {
        [Public]
        public const string Guid = "0c74f135-d89e-469b-8d42-0020f87d954a";
        public const string Name = "Формат документа \"Письмо в ОВД\"";
      }

      /// <summary>
      /// Формат документа "Приказ на прочий отпуск по заявлению".
      /// </summary>
      [Public]
      public static class OtherVacationDocumentExtension
      {
        [Public]
        public const string Guid = "e615b760-506e-4051-9f68-37539885649d";
        public const string Name = "Формат документа \"Приказ на прочий отпуск\"";
      }


      /// <summary>
      /// Формат документа "График отпусков".
      /// </summary>
      [Public]
      public static class VacationScheduleDocumentExtension
      {
        [Public]
        public const string Guid = "436a84d3-7b5e-4e59-9344-e0e4809c662e";
        public const string Name = "Формат документа \"График отпусков\"";
      }

      /// <summary>
      /// Интеграция. Формат файла для документов "Приказ об отзыве из отпуска" и "Приказ о замене части отпуска работнику денежной компенсации", принимаемых из АСУП.
      /// </summary>
      [Public]
      public static class RecallAndCompensationVacationDocumentExtension
      {
        [Public]
        public const string Guid = "B37EF057-0EB3-41A7-A622-5EEEA60AD710";
        public const string Name = "Интеграция. Формат файла для документов \"Приказ об отзыве из отпуска\" и \"Приказ о замене части отпуска работнику денежной компенсации\", принимаемых из АСУП";
      }

      /// <summary>
      /// Отпуска. Количество обрабатываемых записей в АО "Сформировать листы ознакомления" в одной итерации.
      /// </summary>
      [Public]
      public static class CountCreateAcquiantanceListsInIteration
      {
        [Public]
        public const string Guid = "78901234-5678-9012-3456-789012345678";
        public const string Name = "Отпуска. Количество обрабатываемых записей в АО \"Сформировать листы ознакомления\" в одной итерации";
      }

      /// <summary>
      /// Отпуска. Кол-во сотрудников для формирования индивидуальных листов ознакомления.
      /// </summary>
      [Public]
      public static class EmployeesAmountForIndividualAcquaintanceList
      {
        [Public]
        public const string Guid = "56789012-3456-7890-1234-567890123456";
        public const string Name = "Отпуска. Кол-во сотрудников для формирования индивидуальных листов ознакомления.";
      }

      /// <summary>
      /// Отпуска. Количество обрабатываемых записей в АО "Создать и запустить задачу Ведение графика отпусков" в одной итерации.
      /// </summary>
      [Public]
      public static class CountCreateVacationInfoAndStartTaskInIteration
      {
        [Public]
        public const string Guid = "34567890-1234-5678-9012-345678901234";
        public const string Name = "Отпуска. Количество обрабатываемых записей в АО \"Создать и запустить задачу Ведение графика отпусков\" в одной итерации";
      }

      /// <summary>
      /// Отпуска. Текст инструкции по прикреплению документов.
      /// </summary>
      [Public]
      public static class InstructionForAttachingDocuments
      {
        [Public]
        public const string Guid = "DDFBE6FD-F047-435B-85E7-95959554E1C2";
        public const string Name = "Отпуска. Текст инструкции по прикреплению документов";
      }
      
      /// <summary>
      /// Отпуска. Коды плановых отпусков.
      /// </summary>
      [Public]
      public static class PlannedVacationsCodes
      {
        [Public]
        public const string Guid = "90FA5A6B-0BCF-4364-9A4B-307CEF16EC38";
        public const string Name = "Отпуска. Коды плановых отпусков";
      }
      
      /// <summary>
      /// Отпуска. Коды зарегистрированных отпусков.
      /// </summary>
      [Public]
      public static class RegisteredVacationsCodes
      {
        [Public]
        public const string Guid = "CF186742-A4D1-4B3D-B296-DE403C650F35";
        public const string Name = "Отпуска. Коды зарегистрированных отпусков";
      }
      
      /// <summary>
      /// Отпуска. Коды ежегодных отпусков.
      /// </summary>
      [Public]
      public static class AnnualVacationsCodes
      {
        [Public]
        public const string Guid = "63179DEA-222D-4432-A727-FA9718645A80";
        public const string Name = "Отпуска. Коды ежегодных отпусков";
      }
      
      /// <summary>
      /// Отпуска. ИД варианта процесса для уведомления ответственных за ошибки настройки сроков согласования отпусков.
      /// </summary>
      [Public]
      public static class VacationApprovalDateNotificationProcessId
      {
        [Public]
        public const string Guid = "FF028D51-8D5B-40B4-A025-38B0CAEE0CB8";
        public const string Name = "Отпуска. ИД варианта процесса для уведомления ответственных за ошибки настройки сроков согласования отпусков";
      }
    }

    /// <summary>
    /// Значения по умолчанию для прикладных констант.
    /// </summary>
    [Public]
    public static class ConstantsDefaultValues
    {
      /// <summary>
      /// Количество обрабатываемых записей в АО "Сформировать листы ознакомления" в одной итерации.
      /// </summary>
      [Public]
      public const int CountCreateAcquiantanceListsInIteration = 300;
      
      /// <summary>
      /// Кол-во сотрудников для формирования индивидуальных листов ознакомления.
      /// </summary>
      [Public]
      public const int EmployeesAmountForIndividualAcquiantanceList = 100;
      
      /// <summary>
      /// Количество обрабатываемых записей в АО "Создать и запустить задачу Ведение графика отпусков" в одной итерации.
      /// </summary>
      [Public]
      public const int CountCreateVacationInfoAndStartTaskInIteration = 200;
      
      /// <summary>
      /// Текст инструкции по прикреплению документов.
      /// </summary>
      [Public]
      public const string InstructionForAttachingDocuments = "Прикрепите документы. Если не помните, какие, смотрите {0}";
    }
    
    #endregion
    
    /// <summary>
    /// Guid видов документов.
    /// </summary>
    [Public]
    public static class DocumentKind
    {
      /// <summary>
      /// Guid вида документа "Индивидуальный лист ознакомления с графиком отпусков".
      /// </summary>
      [Public]
      public static readonly Guid IndividualAcquiantanceVacScheduleListKind = Guid.Parse("486C6680-2D83-469F-9B31-4C128CD123D3");
      
      /// <summary>
      /// Guid вида документа "Групповой лист ознакомления с графиком отпусков".
      /// </summary>
      [Public]
      public static readonly Guid GroupAcquiantanceVacScheduleListKind = Guid.Parse("C251C32F-BF56-40EC-8A7D-A5949471693E");
      
      /// <summary>
      /// Guid вида документа График отпусков.
      /// </summary>
      [Public]
      public static readonly Guid VacationScheduleKind = Guid.Parse("3c13dc09-8153-4b52-85c4-f2fb61a945c4");
      
      /// <summary>
      /// Guid вида документа "Компенсация части ежегодного оплачиваемого отпуска (в том числе дополнительного)".
      /// </summary>
      [Public]
      public static readonly Guid CompensationVacationKind = Guid.Parse("B1E1F33A-47A4-4990-A36E-8110E2D218E7");
      
      /// <summary>
      /// Guid вида документа "Компенсация части ежегодного дополнительного оплачиваемого отпуска (за ВУТ/ОУТ)".
      /// </summary>
      [Public]
      public static readonly Guid CompensationAdditionalVacationKind = Guid.Parse("7D511964-7E0E-432F-96E3-4F944DED7A91");
      
      /// <summary>
      /// Guid вида документа "Заявление на прочий отпуск".
      /// </summary>
      [Public]
      public static readonly Guid OtherVacationKind = Guid.Parse("C27EE478-9746-49D8-BD5D-641B36A2B154");
      
      /// <summary>
      /// Guid вида документа "Приказ на прочий отпуск".
      /// </summary>
      [Public]
      public static readonly Guid OtherVacationOrderKind = Guid.Parse("11692A61-82AA-400F-83F2-81A3E73E8164");
      
      /// <summary>
      /// Guid вида документа "Приказ на предоставление учебного отпуска".
      /// </summary>
      [Public]
      public static readonly Guid StudyVacationOrderKind = Guid.Parse("C147489E-9B30-4071-9B4A-6C01C17ADEB9");
      
      /// <summary>
      /// Guid вида документа "Приказ о замене части отпуска работнику денежной компенсации".
      /// </summary>
      [Public]
      public static readonly Guid VacationCompensationOrderKind = Guid.Parse("587A8BC5-6825-47DC-BA8B-41A52033B80D");
      
      /// <summary>
      /// Guid вида документа "Приказ о переносе сроков отпуска".
      /// </summary>
      [Public]
      public static readonly Guid ChangeVacationPeriodOrder = Guid.Parse("efa778d2-bb96-4c5c-9408-3906511ce7ee");
      /// <summary>
      /// Guid вида документа "Приказ об отмене приказа".
      /// </summary>
      [Public]
      public static readonly Guid CancelVacationOrder = Guid.Parse("da512715-2126-4200-8b60-f9d22b566bf1");
    }
    
    /// <summary>
    /// GUID'ы типов документов.
    /// </summary>
    public static class DocumentTypes
    {
      /// <summary>
      /// Уведомление о начале отпуска.
      /// </summary>
      [Public]
      public static readonly Guid VacationNoticeDocument = Guid.Parse("9ce75c03-7d0d-4077-9aa8-842a6db23dd2");
      
      /// <summary>
      /// Приказ об отпуске.
      /// </summary>
      [Public]
      public static readonly Guid VacationOrderDocument = Guid.Parse("3e06927c-551a-4189-b361-8b441978dc37");
      
      /// <summary>
      /// График отпусков.
      /// </summary>
      [Public]
      public static readonly Guid VacationScheduleDocument = Guid.Parse("d4770e59-9565-4b66-8143-649d2589ba0c");
      
      /// <summary>
      /// Заявление и согласие по отпуску.
      /// </summary>
      [Public]
      public static readonly Guid VacationStatementDocument = Guid.Parse("0c317400-d24f-4f67-8d3f-01ba229f6e16");
      
      /// <summary>
      /// Документ, связанный с работой.
      /// </summary>
      [Public]
      public static readonly Guid DocRelatedToWork = Guid.Parse("114f0f21-edfa-4b43-bd7a-9202087bf256");
    }
    
    /// <summary>
    /// Список имен объектов личного кабинета сотрудника.
    /// </summary>
    public static class ObjectCardTypeNames
    {
      /// <summary>
      /// Задание на подтверждение дат отпуска.
      /// </summary>
      [Public]
      public const string VacationConfirmationAssignment = "INNVacationConfirmationAssignments";
    }
    
    /// <summary>
    /// Разделитель при передаче id записей в АО.
    /// </summary>
    [Public]
    public const char ListIdsDelimiter = ';';
    
    /// <summary>
    /// Разделитель элементов.
    /// </summary>
    [Public]
    public const string ListDelimiter = ", ";
    
    /// <summary>
    /// Разделитель для строк в ЛК.
    /// </summary>
    [Public]
    public const string DelimiterNewLineForLK = "<br/>    ";
    
    /// <summary>
    /// Ключ кеша по сотруднику для виджета "Мои отпуска".
    /// </summary>
    public const string MyVacationsCacheKey = "MyVacationsCacheKey_{0}";
    
    /// <summary>
    /// Количество обрабатываемых записей по умолчанию в АО в одной итерации.
    /// </summary>
    [Public]
    public const int DefaultCountInIterationAO = 200;
    
    /// <summary>
    /// Количество итераций в АО "Автоматическое формирование задач на подтверждение отпуска".
    /// </summary>
    public const int CreateVacationNoticesAndStartTasksCountIteration = 10;
    
    /// <summary>
    /// Количество итераций в АО "Автоматическое формирование задач на подтверждение отпуска" в случае ошибки в начале обработки.
    /// </summary>
    public const int CreateVacationNoticesAndStartTasksCountIterationError = 3;
    
    /// <summary>
    /// Расширение файла для документа График отпусков.
    /// </summary>
    [Public]
    public const string VacationScheduleDocExtension = "docx";
    
    /// <summary>
    /// Количество попыток создания записей справочника Информация по отпуску и формирования задачи в АО "Создать и запустить задачу Ведение графика отпусков".
    /// </summary>
    [Public]
    public const int CountCreateVacationsInformation = 100;
    
    
    /// <summary>
    /// Ключи параметров таблицы отпусков в шаблоне.
    /// </summary>
    public static class TableDataKeys
    {
      /// <summary>
      /// Таблица.
      /// </summary>
      [Public]
      public const string Table = "Table";
      
      /// <summary>
      /// Таблица по старым отпускам.
      /// </summary>
      [Public]
      public const string OldVacations = "OldVacations";
      
      /// <summary>
      /// Тип отпуска.
      /// </summary>
      [Public]
      public const string VacationTypeName = "VacationTypeName";
      
      /// <summary>
      /// Кол-во дней.
      /// </summary>
      [Public]
      public const string DaysAmount = "DaysAmount";
      
      /// <summary>
      /// Дата начала.
      /// </summary>
      [Public]
      public const string BeginDate = "BeginDate";
      
      /// <summary>
      /// Дата окончания.
      /// </summary>
      [Public]
      public const string EndDate = "EndDate";
    }
    
    /// <summary>
    /// Коды причин мероприятий.
    /// </summary>
    public static class EventReasonCode
    {
      /// <summary>
      /// Ежегодный отпуск.
      /// </summary>
      public const string Annual = "01";
      
      /// <summary>
      /// Учебный отпуск.
      /// </summary>
      public const string Study = "05";
    }
    
    /// <summary>
    /// Результат обработки отправки уведомления об отпуске.
    /// </summary>
    public static class CreateVacationNoticeResult
    {
      /// <summary>
      /// Данные успешно получены.
      /// </summary>
      public const string Successfully = "Successfully";
      
      /// <summary>
      /// Ошибка обработки.
      /// </summary>
      public const string Error = "Error";
    }
    
    /// <summary>
    /// Константы кодов видов документов в АСУП.
    /// </summary>
    [Public]
    public static class SapKindsCodes
    {
      /// <summary>
      /// Код вида документа "Индивидуальный лист ознакомления с графиком отпусков".
      /// </summary>
      [Public]
      public const string IndividualAcquiantanceVacScheduleListKind = "18";
      /// <summary>
      /// Код вида документа "Групповой лист ознакомления с графиком отпусков".
      /// </summary>
      [Public]
      public const string GroupAcquiantanceVacScheduleListKind = "";
      /// <summary>
      /// Код вида документа "Заявление на прочий отпуск".
      /// </summary>
      [Public]
      public const string OtherVacationKind = "647";
      /// <summary>
      /// Код вида документа "Заявление на компенсацию части отпуска".
      /// </summary>
      [Public]
      public const string CompensationVacationKind = "26";
      /// <summary>
      /// Код вида документа "Приказ на прочий отпуск".
      /// </summary>
      [Public]
      public const string OtherVacationOrderKind = "10";
      /// <summary>
      /// Код вида документа "Приказ о замене части отпуска работнику денежной компенсации".
      /// </summary>
      [Public]
      public const string VacationCompensationOrderKind = "01";
      /// <summary>
      /// Код вида документа "Приказ на предоставление учебного отпуска".
      /// </summary>
      [Public]
      public const string StudyVacationOrderKind = "11";
      /// <summary>
      /// Код вида документа "Приказ на ежегодный отпуск".
      /// </summary>
      [Public]
      public const string VacationOrderKind = "04";
      /// <summary>
      /// Код вида документа "Заявление о переносе отпуска".
      /// </summary>
      [Public]
      public const string VacationChangePeriodStatement = "14";
      /// <summary>
      /// Код вида документа "Заявление на ежегодный отпуск".
      /// </summary>
      [Public]
      public const string VacationStatement = "13";
      /// <summary>
      /// Код вида документа "Уведомление о начале отпуска".
      /// </summary>
      [Public]
      public const string VacationBeginningNoticeKind = "19";
      /// <summary>
      /// Код вида документа "Подтверждающий документ к заявлению на ежегодный отпуск".
      /// </summary>
      [Public]
      public const string SupportingDocumentForAnnualVacationDocKind = "";
      /// <summary>
      /// Код вида документа "Подтверждающий документ к заявлению на прочий отпуск".
      /// </summary>
      [Public]
      public const string SupportingDocumentForOtherVacationDocKind = "";
      /// <summary>
      /// Код вида документа "Письмо в авиакомпанию".
      /// </summary>
      [Public]
      public const string AirlineLetterDocKind = "";
      /// <summary>
      /// Код вида документа "Письмо в ОВД".
      /// </summary>
      [Public]
      public const string OVDLetterDocKind = "";
      /// <summary>
      /// Код вида документа "Компенсация части ежегодного дополнительного оплачиваемого отпуска (за ВУТ/ОУТ)".
      /// </summary>
      [Public]
      public const string CompensationAdditionalVacationKind = "";
      /// <summary>
      /// Код вида документа "Заявление о досрочном прекращении отпуска".
      /// </summary>
      [Public]
      public const string VacationSelfRecallStatement = "27";
      /// <summary>
      /// Код вида документа "Согласие на отзыв из отпуска".
      /// </summary>
      [Public]
      public const string VacationRecallConsent = "28";
      /// <summary>
      /// Код вида документа "Приказ об отзыве из отпуска".
      /// </summary>
      [Public]
      public const string RecallVacationOrder = "02";
      /// <summary>
      /// Код вида документа "Приказ о переносе сроков отпуска".
      /// </summary>
      [Public]
      public const string ChangeVacationPeriodOrder = "29";
      /// <summary>
      /// Код вида документа "Приказ об отмене приказа".
      /// </summary>
      [Public]
      public const string CancelVacationOrder = "30";
    }
    
    /// <summary>
    /// Автоматическое создание мероприятия «Отзыв из отпуска» из КЭДО в АСУП.
    /// </summary>
    public const string RecallEventSAP = "Т2";
    
    /// <summary>
    /// Паттерн валидации времени.
    /// </summary>
    [Public]
    public const string PatternValidTime = @"^([01]?[0-9]|2[0-3]):([0-5][0-9])$";
  }
}