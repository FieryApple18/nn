using System;
using Sungero.Core;

namespace DirRX.NNIntegration.Constants
{
  public static class Module
  {
    #region Группы констант.
    /// <summary>
    /// Группы констант.
    /// </summary>
    [Public]
    public static class ConstantsGroup
    {
      /// <summary>
      /// Интеграция с КСШ.
      /// </summary>
      [Public]
      public static class IntegrationWithKss
      {
        [Public]
        public const string Guid = "c547fb24-9cdf-42ea-96cd-e6676d845413";
        public const string Name = "Интеграция с КСШ";
      }
      
      /// <summary>
      /// Интеграция.
      /// </summary>
      [Public]
      public static class Integration
      {
        [Public]
        public const string Guid = "2a0736ab-8a03-4730-8171-0a8deb729b58";
        public const string Name = "Интеграция";
      }
      
      /// <summary>
      /// Синхронизация орг. структуры.
      /// </summary>
      [Public]
      public static class SynchronizationOfOrganizationalStructure
      {
        [Public]
        public const string Guid = "e19706ea-159e-4711-bc5e-6f0d19c7fd63";
        public const string Name = "Синхронизация орг. структуры";
      }
      
      /// <summary>
      /// Интеграция с Суперникой.
      /// </summary>
      [Public]
      public static class IntegrationWithSupernika
      {
        [Public]
        public const string Guid = "984727ae-2e69-4c0d-a1db-8d3399f7b170";
        public const string Name = "Интеграция с Суперникой";
      }
      
      /// <summary>
      /// Прочие процессы.
      /// </summary>
      [Public]
      public static class OtherProcesses
      {
        [Public]
        public const string Guid = "149e7217-f717-4626-a494-a01d2dded2e2";
        public const string Name = "Прочие процессы";
      }
    }
    #endregion
    
    #region Прикладные константы.
    
    [Public]
    public static class ConstantsSetting
    {
      /// <summary>
      /// Интеграция. Количество записей для удаления в ФП "Очистка реестра интеграционных сообщений".
      /// </summary>
      [Public]
      public static class IntegrationMessagesCleaningJobCount
      {
        [Public]
        public const string Guid = "91d9ac7a-3c39-4b5c-9512-ebb669c6b329";
        public const string Name = "Интеграция. Количество записей для удаления за одну итерацию в ФП \"Очистка реестра интеграционных сообщений\"";
      }
      
      /// <summary>
      /// Интеграция. Фиксировать текст запроса в лог-файлах.
      /// </summary>
      [Public]
      public static class NeedLogKSIPMessagesDetails
      {
        [Public]
        public const string Guid = "A0F76D7E-4C3D-41B1-AB4C-CF2A9C2EFA81";
        public const string Name = "Интеграция. Фиксировать текст запроса в лог-файлах";
      }

      /// <summary>
      /// Интеграция. Количество обрабатываемых записей в АО "Создать и отправить исходящие сообщения" в одной итерации.
      /// </summary>
      [Public]
      public static class CountCreateOutgoingMessageInIteration
      {
        [Public]
        public const string Guid = "C1F7B8E9-5D6D-4A51-872A-5B12E4AAB563";
        public const string Name = "Интеграция. Количество обрабатываемых записей в АО \"Создать и отправить исходящие сообщения\" в одной итерации";
      }

      /// <summary>
      /// Интеграция. Количество дней для хранения записей в справочниках "Очереди синхронизации орг. структуры".
      /// </summary>
      [Public]
      public static class CompanyQueuesClearingJobDays
      {
        [Public]
        public const string Guid = "D8E5F0B3-0E6B-4E1B-A29D-58A4B24962D9";
        public const string Name = "Интеграция. Количество дней для хранения записей в справочниках \"Очереди синхронизации орг. структуры\"";
      }

      /// <summary>
      /// Интеграция. Оповещать о задержках синхронизации орг. структуры в рабочих днях.
      /// </summary>
      [Public]
      public static class UnmodifiedBusinessUnitDays
      {
        [Public]
        public const string Guid = "E9B28B9F-4F1A-4A3B-BDA7-BC5F9D732A6D";
        public const string Name = "Интеграция. Оповещать о задержках синхронизации орг. структуры в рабочих днях";
      }

      /// <summary>
      /// Интеграция. ИД группы ответственных за интеграцию.
      /// </summary>
      [Public]
      public static class IdGroupResponibleForIntegrationKSIP
      {
        [Public]
        public const string Guid = "F2B6E8B1-9D7B-43A4-BD94-DC8A5DB7A8A3";
        public const string Name = "Интеграция. ИД группы ответственных за интеграцию";
      }

      /// <summary>
      /// Интеграция. Срок хранения интеграционного сообщения.
      /// </summary>
      [Public]
      public static class IntegrationMessagesCleaningJobDays
      {
        [Public]
        public const string Guid = "B64D2A9E-BE73-4D8B-83D0-1B579FEC2B15";
        public const string Name = "Интеграция. Срок хранения интеграционного сообщения";
      }

      /// <summary>
      /// Интеграция. Проштамповка. Использовать сервис проштамповки.
      /// </summary>
      [Public]
      public static class IsUseStampingService
      {
        [Public]
        public const string Guid = "bd21cd5f-a3b9-40c3-82d2-5079d8e9f126";
        public const string Name = "Интеграция.  Проштамповка. Использовать сервис проштамповки";
      }

      /// <summary>
      /// Интеграция. Проштамповка. Идентификатор шаблона штампа для ПЭП.
      /// </summary>
      [Public]
      public static class TemplateIdSimpleSignature
      {
        [Public]
        public const string Guid = "D4E2C1A8-5F8F-48D3-A1A3-43D4E99D9C43";
        public const string Name = "Интеграция. Проштамповка. Идентификатор шаблона штампа для ПЭП";
      }

      /// <summary>
      /// Интеграция. Проштамповка. Идентификатор шаблона штампа для УКЭП.
      /// </summary>
      [Public]
      public static class TemplateIdQualifiedSignature
      {
        [Public]
        public const string Guid = "E5B5F2DA-0A71-42C2-B05C-2C21EB26B2D2";
        public const string Name = "Интеграция. Проштамповка. Идентификатор шаблона штампа для УКЭП";
      }
      
      /// <summary>
      /// Интеграция. Проштамповка. Список ИД вариантов процесса для отправки документов на повторную проштамповку.
      /// </summary>
      [Public]
      public static class RestampingProcessKindsIds
      {
        [Public]
        public const string Guid = "9A20FB05-A6A5-46BA-9114-38512DBB8EBB";
        public const string Name = " Интеграция. Проштамповка. Список ИД вариантов процесса для отправки документов на повторную проштамповку";
      }
      
      /// <summary>
      /// Интеграция. ИД варианта процесса для уведомления ответственных за ошибки интеграции.
      /// </summary>
      [Public]
      public static class IntegrationNotificationProcessId
      {
        [Public]
        public const string Guid = "F3D2F8C1-15AA-4C9E-87C5-30A8C1D3D3C1";
        public const string Name = "Интеграция. ИД варианта процесса для уведомления ответственных за ошибки интеграции.";
      }

      /// <summary>
      /// Интеграция. Суперника. Шаблон гиперссылки для формирования ссылки на SmartApp КЭДО.
      /// </summary>
      [Public]
      public static class HyperLinkTemplateToSmartAppKedo
      {
        [Public]
        public const string Guid = "A67F8D1E-6F6E-4C6E-9A1E-2C2EFC6EAA7F";
        public const string Name = "Шаблон гиперссылки для формирования ссылки на SmartApp КЭДО. Значение \"{0}\" заменяется на ссылку на объект в ЛК HR Pro";
      }
      
      /// <summary>
      /// Интеграция. Код вида документа АСУП. Заявление о предоставлении учебного отпуска.
      /// </summary>
      [Public]
      public static class StudyVacationDocKindSAPCode
      {
        [Public]
        public const string Guid = "d03f8777-f891-49a2-9aef-770f14dfe0aa";
        public const string Name = "Интеграция. Код вида документа АСУП. Заявление о предоставлении учебного отпуска";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Шаблон homescreen_express для гиперссылки на SmartApp КЭДО.
      /// </summary>
      [Public]
      public static class HomescreenExpress
      {
        [Public]
        public const string Guid = "DD83832E-97D2-401C-B5E4-BDD3A22E6AA9";
        [Public]
        public const string Name = "Имя SmartApp КЭДО (app_id)";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Шаблон текста для гиперссылки на SmartApp КЭДО.
      /// </summary>
      [Public]
      public static class TextForHyperlink
      {
        [Public]
        public const string Guid = "BB03C7A0-2220-4A62-A056-B2E03CEE580C";
        [Public]
        public const string Name = "Шаблон текста для гиперссылки на SmartApp КЭДО";
      }
      
      /// <summary>
      /// Интеграция. Код вида документа АСУП. Заявление на дополнительный отпуск на обустройство по программе Релокация.
      /// </summary>
      [Public]
      public static class VacationByRelocationDocKindSAPCode
      {
        [Public]
        public const string Guid = "1A511A89-853E-4A5A-9636-EA138BD88FCA";
        [Public]
        public const string Name = "Интеграция. Код вида документа АСУП. Заявление на дополнительный отпуск на обустройство по программе Релокация";
      }
      
      /// <summary>
      /// Интеграция. Код вида документа АСУП. Заявление на отпуск (по прочим основаниям).
      /// </summary>
      [Public]
      public static class OtherVacationDocKindSAPCode
      {
        [Public]
        public const string Guid = "AFEEE0A7-308F-45DE-A0C1-4C2E51314719";
        [Public]
        public const string Name = "Интеграция. Код вида документа АСУП. Заявление на отпуск (по прочим основаниям)";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Максимальное число push-уведомлений, которые удаляются в рамках одного запуска фонового процесса.
      /// </summary>
      [Public]
      public static class SupernikaNotificationsCleaningNumber
      {
        [Public]
        public const string Guid = "d9ff8dfc-eb9d-4e61-83f4-25866721d90e";
        [Public]
        public const string Name = "Максимальное число push-уведомлений, которые удаляются в рамках одного запуска фонового процесса";
      }
      
      /// <summary>
      /// Интеграция. Код вида документа АСУП. Заявление на предоставление отпуска по КД (работник-инвалид).
      /// </summary>
      [Public]
      public static class VacationDisabledEmployeeDocKindSAPCode
      {
        [Public]
        public const string Guid = "2F0AA3A5-1AF8-4329-A4A6-2A48257F6A2B";
        [Public]
        public const string Name = "Интеграция. Код вида документа АСУП. Заявление на предоставление отпуска по КД (работник-инвалид)";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Максимальное количество повторов асинхронного обработчика "Асинхронное формирование сообщения через Сервис сообщений".
      /// </summary>
      [Public]
      public static class SendNotificationToSupernikaAsyncHandlerMaxRetry
      {
        [Public]
        public const string Guid = "503a4a88-54ca-4386-8b7a-ee368bfab554";
        [Public]
        public const string Name = "Максимальное количество повторов асинхронного обработчика \"Асинхронное формирование сообщения через Сервис сообщений\"";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Срок хранения push-уведомления.
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsCleaningJobDays
      {
        [Public]
        public const string Guid = "e2ed77a5-b081-4946-8d79-8bfdae8208dd";
        [Public]
        public const string Name = "Срок хранения push-уведомления в календарных днях";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Адрес сервера СУБД с базой MessageBroker.
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsDBServer
      {
        [Public]
        public const string Guid = "8e7ccfbe-6771-40fc-9397-451e157474a0";
        [Public]
        public const string Name = "Адрес сервера СУБД с базой MessageBroker";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Порт сервера СУБД с базой MessageBroker.
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsDBPort
      {
        [Public]
        public const string Guid = "61ce0988-58e1-409e-be54-0fd0b65bd6a2";
        [Public]
        public const string Name = "Порт сервера СУБД с базой MessageBroker";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Имя пользователя для подключения к серверу СУБД с базой MessageBroker.
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsDBUserName
      {
        [Public]
        public const string Guid = "482df8ce-f51a-4b53-a316-334ca820e7cf";
        [Public]
        public const string Name = "Имя пользователя для подключения к серверу СУБД с базой MessageBroker";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Пароль для подключения к серверу СУБД с базой MessageBroker.
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsDBPassword
      {
        [Public]
        public const string Guid = "e2fd9e2b-93a1-4b68-ba91-d79cb5141ed3";
        [Public]
        public const string Name = "Пароль для подключения к серверу СУБД с базой MessageBroker";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Имя базы данных MessageBroker.
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsDBName
      {
        [Public]
        public const string Guid = "112c7292-c24c-4d3b-bb0c-9997e2c0e083";
        [Public]
        public const string Name = "Имя базы данных MessageBroker";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Тема задания по умолчанию для push-сообщения.
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsAssignmentName
      {
        [Public]
        public const string Guid = "e2d44a2b-46cc-426e-bf52-aa2d8c8d078b";
        [Public]
        public const string Name = "Тема задания для push-сообщения";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Тема уведомления по умолчанию для push-сообщения.
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsNoticeName
      {
        [Public]
        public const string Guid = "8c1b3137-9fb5-4da7-8acb-b749756d5fb6";
        [Public]
        public const string Name = "Тема уведомления для push-сообщения";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Количество сообщений обрабатываемых ФП "Отправка сообщений из справочника Push-уведомления в Супернику".
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsSendMessageCount
      {
        [Public]
        public const string Guid = "1DB209A7-5C33-4BD8-98E3-C951E75C2323";
        [Public]
        public const string Name = "Количество сообщений обрабатываемых ФП \"Отправка сообщений из справочника Push-уведомления в Супернику\"";
      }
      
      /// <summary>
      /// Интеграция. Суперника. Добавлять дату и время формирования задания в текст Push-уведомления.
      /// </summary>
      [Public]
      public static class SupernikaPushNotificationsIsTimestampRequired
      {
        [Public]
        public const string Guid = "01E797D1-F7C1-4521-BB6C-0BA8A6A9E034";
        [Public]
        public const string Name = "Добавлять дату и время формирования задания в текст Push-уведомления";
      }
      
      /// <summary>
      /// Интеграция. Перевыпуск сертификата УНЭП при синхронизации орг. структуры.
      /// </summary>
      [Public]
      public static class ReissueCertificateOnOrgStructureSync
      {
        [Public]
        public const string Guid = "F17EAE55-A271-4C27-922A-0BB147BA0163";
        [Public]
        public const string Name = "Интеграция. Перевыпуск сертификата УНЭП при синхронизации орг. структуры";
      }
      
      /// <summary>
      /// Интеграция. Проштамповка. Идентификатор шаблона штампа для УНЭП.
      /// </summary>
      [Public]
      public static class TemplateIdUnqualifiedSignature
      {
        [Public]
        public const string Guid = "AFC3F83D-5510-4547-ADA3-413F6C7FF7C5";
        [Public]
        public const string Name = "Интеграция. Проштамповка. Идентификатор шаблона штампа для УНЭП";
      }
    }

    /// <summary>
    /// Значения по умолчанию для прикладных констант.
    /// </summary>
    [Public]
    public static class ConstantsDefaultValues
    {
      /// <summary>
      /// Количество обрабатываемых записей Исходящих сообщений в одной итерации.
      /// </summary>
      [Public]
      public const int CountCreateOutgoingMessageInIteration = 100;
      
      /// <summary>
      /// Количество дней по умолчанию, начиная с даты последней обработки записи очереди, по истечению которых необходо удалить запись.
      /// </summary>
      public const int CompanyQueuesClearingJobDays = 90;
      
      /// <summary>
      /// Количество рабочих дней по умолчанию, по истечению которых необходимо оповещать о задержках синхронизации организационной структуры.
      /// </summary>
      public const int UnmodifiedBusinessUnitDays = 5;
      
      /// <summary>
      /// Количество дней по умолчанию, начиная с даты последней обработки интеграционного сообщения, по истечении которых необходимо его удалить.
      /// </summary>
      public const int IntegrationMessagesCleaningJobDays = 30;
      
      /// <summary>
      /// Количество записей Исходящих сообщений для удаления в одной итерации.
      /// </summary>
      public const int IntegrationMessagesCleaningJobCount = 1000;
      
      /// <summary>
      /// Максимальное количество повторов асинхронного обработчика "Асинхронное формирование сообщения через Сервис сообщений.
      /// </summary>
      [Public]
      public const int SendNotificationToSupernikaAsyncHandlerMaxRetry = 100;
      
      /// <summary>
      /// Количество дней по умолчанию, начиная с даты получения ответа на push уведомление из Суперники, по истечении которых необходимо удалить это push уведомление.
      /// </summary>
      public const int SupernikaPushNotificationsCleaningJobDaysDefault = 120;
      
      /// <summary>
      /// Количество push-уведомлений по умолчанию, которые удаляются в рамках одного запуска фонового процесса.
      /// </summary>
      [Public]
      public const int SupernikaNotificationsCleaningNumberDefault = 100;
      
      /// <summary>
      /// Использовать сервис проштамповки по умолчанию.
      /// </summary>
      [Public]
      public const bool IsUseStampingService = true;
      
      /// <summary>
      /// Идентификатор шаблона для проштамповки ПЭП сервиса проштамповки.
      /// </summary>
      [Public]
      public const string TemplateIdSimpleSignature = "19";
      
      /// <summary>
      /// Идентификатор шаблона для проштамповки УКЭП сервиса проштамповки.
      /// </summary>
      [Public]
      public const string TemplateIdQualifiedSignature = "20";
      
      /// <summary>
      /// ИД варианта процесса для уведомления ответственных за ошибки интеграции.
      /// </summary>
      [Public]
      public const long IntegrationNotificationProcessId = 0;
      
      /// <summary>
      /// Шаблон гиперссылки для формирования ссылки на SmartApp КЭДО. Значение "{0}" заменяется на ссылку на объект в ЛК HR Pro.
      /// </summary>
      [Public]
      public const string HyperLinkTemplateToSmartAppKedo = "https://xlnk.ms/open/smartapp/kedo-smartapp?route={0}&ets_id=ea241bea-fc0b-58d7-b8a4-b5f8a844f0ae";
      
      /// <summary>
      /// Интеграция. Код вида документа АСУП. Заявление о предоставлении учебного отпуска.
      /// </summary>
      [Public]
      public const string StudyVacationDocKindSAPCode = "446";
      
      /// <summary>
      /// Шаблон текста для формирования ссылки на SmartApp КЭДО.
      /// </summary>
      [Public]
      public const string TextTemplateToSmartAppKedo = "Открыть в SmartApp";
      
      /// <summary>
      /// Шаблон app_id для гиперссылки на SmartApp КЭДО
      /// </summary>
      [Public]
      public const string KedoSmartappBot ="kedo-smartapp";
      
      /// <summary>
      /// Интеграция. Код вида документа АСУП. Заявление на дополнительный отпуск на обустройство по программе Релокация.
      /// </summary>
      [Public]
      public const string VacationByRelocationDocKindSAPCode = "841";
      
      /// <summary>
      /// Интеграция. Код вида документа АСУП. Заявление на отпуск (по прочим основаниям).
      /// </summary>
      [Public]
      public const string OtherVacationDocKindSAPCode = "647";
      
      /// <summary>
      /// Интеграция. Код вида документа АСУП. Заявление на предоставление отпуска по КД (работник-инвалид).
      /// </summary>
      [Public]
      public const string VacationDisabledEmployeeDocKindSAPCode = "849";
      
      /// <summary>
      /// Интеграция. Суперника. Порт сервера СУБД с базой MessageBroker.
      /// </summary>
      [Public]
      public const int SupernikaPushNotificationsDBPort = 5432;
      
      /// <summary>
      /// Интеграция. Суперника. Тема задания по умолчанию для push-сообщения.
      /// </summary>
      [Public]
      public const string SupernikaPushNotificationsAssignmentName = "У вас новое задание";
      
      /// <summary>
      /// Интеграция. Суперника. Тема уведомления по умолчанию для push-сообщения.
      /// </summary>
      [Public]
      public const string SupernikaPushNotificationsNoticeName = "У вас новое уведомление";
      
      /// <summary>
      /// Интеграция. Суперника. Количество сообщений обрабатываемых ФП "Отправка сообщений из справочника Push-уведомления в Супернику".
      /// </summary>
      [Public]
      public const int SupernikaPushNotificationsSendMessageCount = 100;
      
      /// <summary>
      /// Интеграция. Суперника. Добавлять дату и время формирования задания в текст Push-уведомления.
      /// </summary>
      [Public]
      public const bool SupernikaPushNotificationsIsTimestampRequired = true;
    }
    #endregion

    #region Синхронизация организационной структуры.

    /// <summary>
    /// Максимальное число повторов асинхронного обработчика.
    /// </summary>
    [Public]
    public const int MaxRetry = 100;

    /// <summary>
    /// Коды пола персоны.
    /// </summary>
    public static class SexCodes
    {
      /// <summary>
      /// Константа кода для мужского пола.
      /// </summary>
      public const string Male = "М";
      
      /// <summary>
      /// Константа кода для женского пола.
      /// </summary>
      public const string Female = "Ж";
    }

    /// <summary>
    /// Наименование РФ в справочнике Страны.
    /// </summary>
    public const string RussianFederationCode = "643";

    /// <summary>
    /// Название региона по умолчанию.
    /// </summary>
    public const string DefaultRegionName = "Не определено";

    /// <summary>
    /// Название страны по умолчанию.
    /// </summary>
    public const string DefaultCountryName = "Не определено";

    /// <summary>
    /// Код страны по умолачнию.
    /// </summary>
    public const string DefaultCountryCode = "999";

    /// <summary>
    /// Индекс, по которому делиться код НОР из КСИП на SAP Тип организации и SAP код юр. лица.
    /// </summary>
    public const int BusinessUinitSapSplitIndex = 1;

    /// <summary>
    /// Коды поля Атрибут "Группа оценки".
    /// </summary>
    [Public]
    public static class EvaluationGroupCodes
    {
      /// <summary>
      /// Константа кода для Топ100.
      /// </summary>
      [Public]
      public const string Top100 = "ТОП-100";
      
      /// <summary>
      /// Константа кода для Топ10.
      /// </summary>
      [Public]
      public const string Top10 = "ТОП-10";
      
      /// <summary>
      /// Константа кода для Вне категорий.
      /// </summary>
      [Public]
      public const string OutOfCategories = "Вне категорий";
    }

    /// <summary>
    /// Запрос на обновление пользователя при смене номера телефона.
    /// </summary>
    [Public]
    public const string PatchEssUser = "api/Users/{0}";
    
    #endregion

    #region Внешние системы и потоки.
    
    /// <summary>
    /// Список guid Внешних систем.
    /// </summary>
    [Public]
    public static class ExternalSystemsGuids
    {
      /// <summary>
      /// Guid КСИП.
      /// </summary>
      [Public]
      public const string KSIP = "92B43F39-BA24-4484-B2B7-E155409575B2";
    }

    /// <summary>
    /// Список guid Потоков внешних систем.
    /// </summary>
    [Public]
    public static class ExternalSystemsFlowGuids
    {
      // IF-S-9401. Передача информации о документе из КЭДО в АСУП (АСУП -> КЭДО).
      public const string SapFioriOutgoing = "3b538530-c934-44c7-8e34-d2b61b5f91b1";
      // IF-S-9310, IF-S-9331, IF-S-9330. Синхронизация организационной структуры.
      public const string CompanySynchronization = "346F8E20-E37F-42D8-9459-54B06EA4C795";
      // IF-S-9313. Автоматическое формирование задания на ручное проведения мероприятия в АСУП.
      public const string ManualHoldingEvent = "ba6800d2-09fc-4f76-9870-282e8b1e89bd";
      // IF-S-9312. Автоматическое создание карточки документа из АСУП в КЭДО.
      public const string CreateRetentionStatementDocument = "13d93d82-b18e-40bf-a786-905deb9d58a7";
      // IF-S-11622. Получение результата асинхронной отправки данных из КСИП в АСУП.
      public const string GetSendingAsyncDataResultFromKSIP = "0EAB7B9A-833F-416C-8093-A877A063EF15";
      // IF-S-6365. Передача графика отпусков. Создание карточки согласования графика отпусков из АСУП в КЭДО.
      public const string CreateVacationScheduleDocument = "E47F0A19-EB92-423D-B8A1-CDD78415C18F";
      // IF-S-10408. Передача приказа на предоставление отпуска из АСУП в КЭДО (АСУП -> КЭДО).
      public const string CreateAnnualVacationOrder = "6f22f227-3c8e-4e20-9eaa-836559265c2d";
      // IF-S-9326. Передача приказа на предоставление прочего отпуска из АСУП в КЭДО (АСУП -> КЭДО).
      public const string CreateOtherVacationOrder = "87d33a25-a4f2-4f43-9243-adfd2e3a241c";
      // IF-S-10396. Передача приказа на компенсацию части ежегодного оплачиваемого отпуска из АСУП в КЭДО (АСУП -> КЭДО).
      public const string CreateVacationCompensationOrder = "164F2B62-6480-4C6B-A132-0A85004B6124";
      // IF-S-10391. Передача приказа об отзыве из отпуска из АСУП в КЭДО (АСУП -> КЭДО).
      public const string CreateRecallVacationOrder = "86D4827A-A057-4E50-8024-5E03E41F6610";
      // IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП.
      public const string CreateBusinessTripOrder = "2324b1a7-5df5-4a01-b710-c697d9cae6c7";
      // IF-S-10532. Передача статуса приказа (КЭДО -> АСУП).
      public const string StatusDocumentInSap = "87C0BEDC-C961-4222-9A90-115296EFBD4B";
      // IF-S-9336. Передача уведомления об отпуске из АСУП в КЭДО
      [Public]
      public const string SendingVacationNotice = "6ADA5791-83C1-4452-AEDD-5BF3C2844B60";
      // IF-S-10403. Передача данных о доступных остатках отпуска сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
      [Public]
      public const string GetVacationAvailableLimits = "F831ACD6-CBD1-4BFA-B17C-1FE879C617C8";
      // IF-S-10384. Получение данных о доступном количестве дней на компенсацию из АСУП в КЭДО (АСУП -> КЭДО).
      [Public]
      public const string GetCompensationAvailableDays = "9A591524-1EE3-4E31-80BC-0E42543C052F";
      // IF-S-10392. Получение данных о текущем отпуске сотрудника из АСУП в КЭДО (АСУП -> КЭДО).
      [Public]
      public const string GetCurrentVacationData = "FD653553-A0B3-4C7F-8688-B292113FF8F5";
      // IF-S-10405. Проверка данных при оформлении заявления на отпуск (АСУП -> КЭДО).
      [Public]
      public const string ValidateEmployeeVacations = "1063F5F5-C22E-4145-9167-C3544CE51DA9";
      // IF-S-10394. Проверка данных в АСУП для оформления отзыва из отпуска (АСУП -> КЭДО).
      [Public]
      public const string ValidateVacationRecall = "C7D388D9-552B-4531-B209-97E4F37B40BA";
      // IF-S-11187. Проверка данных в АСУП для заявления на компенсацию части ежегодного отпуска (АСУП -> КЭДО).
      [Public]
      public const string ValidateVacationCompensation = "020A7624-DA3F-4C12-9925-26B6820846E6";
      // IF-S-10409. Проверка заявления на предоставление прочего отпуска (АСУП -> КЭДО).
      [Public]
      public const string ValidateEmployeeOtherVacations = "B2618238-F619-4530-B0D9-23883920A7CD";
      // IF-S-10406. Проверка заявления на перенос отпуска (АСУП -> КЭДО).
      [Public]
      public const string ValidateEmployeeShiftVacations = "55D8533F-7BE5-4B61-B3D1-E931DE129108";
      [Public]
      public const string SendDocumentToStamping = "47471976-0499-494D-94DA-C132977CC50D";
      [Public]
      public const string GetResultStamping = "7A40C2C8-DA8D-4B2C-9E4D-C5558CDDC516";
      // IF-S-10536. Формирование проводки на перенос отпуска из КЭДО в АСУП (КЭДО -> АСУП).
      public const string RegistrationShiftVacation = "D50A5D6A-4DEC-4A08-8702-CE28B9ABC059";
      // IF-S-6372. Регистрация ежегодного отпуска (по заявлению) из КЭДО в АСУП (КЭДО -> АСУП).
      public const string RegistrationAnnualVacationByStatement = "F2F76821-2A47-41CE-8360-30E2D3668FD1";
      // IF-S-6376. Регистрация прочего отпуска из КЭДО в АСУП (КЭДО -> АСУП).
      public const string RegistrationOtherVacation = "D1DC6EC3-2798-4241-8597-B8D85E089B28";
      // IF-S-6373. Регистрация ежегодного отпуска (по графику отпусков) из КЭДО в АСУП (КЭДО -> АСУП).
      public const string RegistrationAnnualVacationBySchedule = "9F36B265-CF4C-4E7E-A471-9CF8096B68F6";
      // IS-F-25401. Передача результата нажатия на действие в центре уведомлений ПМД (ПМД -> КЭДО).
      [Public]
      public const string GetSupernikaActionResult = "71FF6F67-63F0-4205-B8D3-2A9F6C91554A";
      // IF-S-25553. Передача статусов отправки уведомлений об отпуске из КЭДО в АСУП (КЭДО -> АСУП).
      [Public]
      public const string SendStatusVacationNoticeInSap = "78861873-E952-43DF-AE8E-B618EDF9F36A";
      // IF-S-6369. Автоматическое создание мероприятия «Компенсация части ежегодного оплачиваемого отпуска» из КЭДО в АСУП.
      public const string CompensationForPartOfAnnualPaidLeave = "3471A403-1BDD-41A2-BB60-67A1BA725600";
      // IF-S-6361. Автоматическое создание мероприятия "Отзыв из отпуска" из КЭДО в АСУП (КЭДО -> АСУП).
      public const string RegistrationRecallVacation = "4DAFAE5F-1FD6-4F17-B8AE-592DE3A89623";
      // IF-S-10412. Автоматическая проверка заявления на работу в выходной и/или праздничный день из КЭДО в АСУП.
      [Public]
      public const string ValidateWeekendWorkStatement = "92EA163E-AD96-4B27-BED7-19A0E4375D46";
      // IF-S-10413. Передача приказа на выход сотрудника в выходной и/или праздничный день из АСУП в КЭДО (АСУП -> КЭДО).
      [Public]
      public const string CreateWeekendWorkOrderDocument = "4C926C6E-F216-4119-880C-82029D4AC4DC";
      // IF-S-6363. Автоматическое создание мероприятия «Регистрация работы в выходной и/или праздничный день» из КЭДО в АСУП (КЭДО -> АСУП).
      [Public]
      public const string WeekendWorkEvent = "BEA9FE63-DAA8-4B85-BA59-4DAC5206A8DB";
      // IF-S-6362. Автоматическое внесение изменений в действующий приказ из КЭДО в АСУП (КЭДО -> АСУП).
      [Public]
      public const string WeekendWorkCancelationEvent = "2E2F944F-8DB7-47A8-8C39-850A45EC915D";
      // IF-S-6366. Автоматическое создание мероприятия «Регистрация выплат ЛНА» из КЭДО в АСУП (КЭДО -> АСУП).
      [Public]
      public const string LrdPaymentEvent = "61285EDA-0275-43DD-A21C-FAD8670C29FD";
      // IF-S-10400. Проверка данных по заявлению на выплату ЛНА (АСУП -> КЭДО).
      [Public]
      public const string LrdPaymentCheck = "E900FADD-F8BC-433A-8C81-79A8052FEFFB";
      // IF-S-25360. Получение списка детей работника. (АСУП -> КЭДО).
      [Public]
      public const string LrdPaymentChildren = "CE70EA72-EA83-4255-B068-225473D70432";
      // IF-S-10399. Передача приказа на выплату по ЛНА из АСУП в КЭДО (АСУП → КЭДО).
      [Public]
      public const string LrdPaymentOrder = "8FF52F2E-01D2-4939-8112-985F1D5A886C";
      // IF-S-28097. Синхронизация календарей рабочего времени из АСУП в КЭДО (АСУП -> КЭДО).
      [Public]
      public const string UpdateOrCreateWorkingTimeCalendar = "C3054A3F-2C3A-4789-9819-410196794C65";
      
      //IF-S-28562. Создание карточки дополнительного соглашения к трудовому договору в КЭДО из АСУП (АСУП → КЭДО)"
      [Public]
      public const string EmploymentContractAdditionalAgreement = "F6F2A5BA-07B2-4B55-8174-ADF5D01A9A0B";

      // IF-S-28522 Проверка оплаты отсутствий (КЭДО → АСУП).
      public const string SendVacationsForPaymentChecking = "11FB6BC4-B705-4560-A60F-A99B6B64F452";
      
      // IF-S-28542 Данные по оплатам отсутствий из АСУП в КЭДО (АСУП → КЭДО).
      public const string VacationsPaymentInfo = "FF17FC96-32E6-40FD-BC2F-5B14E1CD9A66";
    }

    /// <summary>
    /// Список наименований Потоков внешних систем.
    /// </summary>
    [Public]
    public static class ExternalSystemsFlowNames
    {
      public const string CompanySynchronization = "IF-S-9310, IF-S-9331, IF-S-9330. Синхронизация организационной структуры";
      public const string GetSendingAsyncDataResultFromKSIP = "IF-S-11622. Получение результата асинхронной отправки данных из КСИП в АСУП";
      public const string SendingVacationNotice = "IF-S-9336. Передача уведомления об отпуске из АСУП в КЭДО";
      public const string CreateRetentionStatementDocument = "IF-S-10397. Автоматическое создание карточки документа из АСУП в КЭДО";
      public const string CreateVacationScheduleDocument = "IF-S-6365. Создание карточки согласования графика отпусков из АСУП в КЭДО";
      public const string SendDocumentInSapFiori = "IF-S-9401. Передача информации о документе из КЭДО в АСУП (АСУП -> КЭДО)";
      public const string CreateAnnualVacationOrder = "IF-S-10408. Передача приказа на предоставление отпуска из АСУП в КЭДО (АСУП -> КЭДО)";
      public const string CreateOtherVacationOrder = "IF-S-10411. Передача приказа на предоставление прочего отпуска из АСУП в КЭДО (АСУП -> КЭДО)";
      public const string CreateVacationCompensationOrder = "IF-S-10396. Передача приказа на компенсацию части ежегодного оплачиваемого отпуска из АСУП в КЭДО (АСУП -> КЭДО).";
      public const string CreateRecallVacationOrder = "IF-S-10391. Передача приказа об отзыве из отпуска из АСУП в КЭДО (АСУП -> КЭДО)";
      public const string ManualHoldingEvent = "IF-S-10398. Автоматическое формирование задания на ручное разблокирование ИТ0015 в АСУП (АСУП -> КЭДО)";
      public const string SendStatusDocumentInSap = "IF-S-10532. Передача статуса приказа (КЭДО -> АСУП)";
      public const string CreateBusinessTripOrder = "IF-S-6374. Создание карточки согласования приказа на командировку в КЭДО из АСУП.";
      public const string GetVacationAvailableLimits = "IF-S-10403. Передача данных о доступных остатках отпуска сотрудника из АСУП в КЭДО (АСУП -> КЭДО)";
      public const string GetCompensationAvailableDays = "IF-S-10384. Получение данных о доступном количестве дней на компенсацию из АСУП в КЭДО (АСУП -> КЭДО)";
      public const string GetCurrentVacationData = "IF-S-10392. Получение данных о текущем отпуске сотрудника из АСУП в КЭДО (АСУП -> КЭДО)";
      public const string ValidateEmployeeVacations = "IF-S-10405. Проверка данных при оформлении заявления на отпуск (АСУП -> КЭДО)";
      public const string ValidateVacationRecall = "IF-S-10394. Проверка данных в АСУП для оформления отзыва из отпуска (АСУП -> КЭДО)";
      public const string ValidateVacationCompensation = "IF-S-11187. Проверка данных в АСУП для заявления на компенсацию части ежегодного отпуска (АСУП -> КЭДО)";
      public const string ValidateEmployeeOtherVacations = "IF-S-10409. Проверка заявления на предоставление прочего отпуска (АСУП -> КЭДО)";
      public const string ValidateEmployeeShiftVacations = "IF-S-10406. Проверка заявления на перенос отпуска (АСУП -> КЭДО)";
      public const string SendDocumentToStamping = "IF-S-7601. Передача документа на проштамповку";
      public const string GetResultStamping = "IF-S-7538. Получение результата проштамповки";
      public const string RegistrationShiftVacation = "IF-S-10536. Формирование проводки на перенос отпуска из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string RegistrationAnnualVacationByStatement = "IF-S-6372. Регистрация ежегодного отпуска (по заявлению) из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string RegistrationOtherVacation = "IF-S-6376. Регистрация прочего отпуска из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string RegistrationAnnualVacationBySchedule = "IF-S-6373. Регистрация ежегодного отпуска (по графику отпусков) из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string GetSupernikaActionResult = "IF-S-25401. Передача результата нажатия на действие в центре уведомлений ПМД (ПМД -> КЭДО)";
      public const string CompensationForPartOfAnnualPaidLeave = "IF-S-6369. Компенсация части ежегодного оплачиваемого отпуска из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string RegistrationRecallVacation = "IF-S-6361. Автоматическое создание мероприятия \"Отзыв из отпуска\" из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string SendStatusVacationNoticeInSap = "IF-S-25553. Передача статусов отправки уведомлений об отпуске из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string ValidateWeekendWorkStatement = "IF-S-10412. Автоматическая проверка заявления на работу в выходной и/или праздничный день (АСУП -> КЭДО)";
      public const string CreateWeekendWorkOrderDocument = "IF-S-10413. Передача приказа на выход сотрудника в выходной и/или праздничный день из АСУП в КЭДО (АСУП -> КЭДО)";
      public const string WeekendWorkEvent = "IF-S-6363. Автоматическое создание мероприятия «Регистрация работы в выходной и/или праздничный день» из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string WeekendWorkCancelationEvent = "IF-S-6362. Автоматическое внесение изменений в действующий приказ из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string LrdPaymentEvent = "IF-S-6366. Автоматическое создание мероприятия «Регистрация выплат ЛНА» из КЭДО в АСУП (КЭДО -> АСУП)";
      public const string LrdPaymentCheck = "IF-S-10400. Проверка данных по заявлению на выплату ЛНА (АСУП -> КЭДО)";
      public const string LrdPaymentChildren = "IF-S-25360. Получение списка детей работника. (АСУП -> КЭДО)";
      public const string LrdPaymentOrder = "IF-S-10399. Передача приказа на выплату по ЛНА из АСУП в КЭДО. (АСУП → КЭДО)";
      public const string UpdateOrCreateWorkingTimeCalendar = "IF-S-28097. Синхронизация календарей рабочего времени из АСУП в КЭДО. (АСУП → КЭДО)";
      public const string EmploymentContractAdditionalAgreement = "IF-S-28562. Создание карточки дополнительного соглашения к трудовому договору в КЭДО из АСУП (АСУП → КЭДО)";
      public const string SendVacationsForPaymentChecking = "IF-S-28522 Проверка оплаты отсутствий (КЭДО → АСУП)";
      public const string VacationsPaymentInfo = "IF-S-28542 Данные по оплатам отсутствий из АСУП в КЭДО (АСУП → КЭДО)";
    }

    /// <summary>
    /// Список описаний Потоков внешних систем.
    /// </summary>
    [Public]
    public static class ExternalSystemsFlowDescriptions
    {
      public const string CompanySynchronization = "Интеграционный поток предназначен для синхронизации справочников Подразделения, Должности, Сотрудники.";
      public const string ManualHoldingEvent = "Интеграционный поток предназначен для формирования задания на ручное разблокирование ИТ0015.";
      public const string GetSendingAsyncDataResultFromKSIP = "Интеграционный поток предназначен для получения результата асинхронной отправки данных из КСИП в АСУП.";
      public const string SendingVacationNotice = "Интеграционный поток предназначен для получения информации по уведомлениям об отпуске.";
      public const string CreateRetentionStatementDocument = "Интеграционный поток предназначен для создания или обновления заявления на удержание.";
      public const string CreateVacationScheduleDocument = "Интеграционный поток предназначен для создания карточки согласования графика отпусков из АСУП в КЭДО.";
      public const string SendDocumentInSapFiori = "Интеграционный поток предназначен для отправки документов в SAP Fiori.";
      public const string CreateAnnualVacationOrder = "Интеграционный поток предназначен для создания или обновления приказа на ежегодный отпуск по заявлению.";
      public const string CreateOtherVacationOrder = "Интеграционный поток предназначен для создания или обновления приказа на прочий отпуск по заявлению.";
      public const string CreateVacationCompensationOrder = "Интеграционный поток предназначен для создания или обновления приказа на компенсацию части ежегодного оплачиваемого отпуска.";
      public const string CreateRecallVacationOrder = "Интеграционный поток предназначен для создания или обновления приказа об отзыве из отпуска.";
      public const string CreateBusinessTripOrder = "Интеграционный поток предназначен для создания или обновления 'Приказа на командировку'.";
      public const string SendStatusDocumentInSap = "Интеграционный поток предназначен для отправки статусов приказов из КЭДО в АСУП.";
      public const string GetVacationAvailableLimits = "Интеграционный поток предназначен для получения доступных лимитов по отпускам сотрудника.";
      public const string GetCompensationAvailableDays = "Интеграционный поток предназначен для получения доступных дней на компенсацию отпуска сотрудника.";
      public const string GetCurrentVacationData = "Интеграционный поток предназначен для получения данных о текущем отпуске сотрудника.";
      public const string ValidateEmployeeVacations = "Интеграционный поток предназначен для проверки отпусков сотрудника при оформлении заявления на отпуск.";
      public const string ValidateVacationRecall = "Интеграционный поток предназначен для проверки данных в АСУП для оформления отзыва из отпуска.";
      public const string ValidateVacationCompensation = "Интеграционный поток предназначен для проверки отпуска сотрудника при оформление заявления на компенсацию части ежегодного отпуска.";
      public const string ValidateEmployeeOtherVacations = "Интеграционный поток предназначен для проверки отпусков сотрудника при оформлении заявления на прочий отпуск.";
      public const string ValidateEmployeeShiftVacations = "Интеграционный поток предназначен для проверки отпусков сотрудника при оформлении заявления на перенос отпуска.";
      public const string SendDocumentToStamping = "Интеграционный поток предназначен для передачи документа на проштамповку в сервис проштамповки.";
      public const string GetResultStamping = "Интеграционный поток предназначен для получения и обработки результата проштамповки документа.";
      public const string RegistrationShiftVacation = "Интеграционный поток предназначен для формирования проводки на перенос отпуска.";
      public const string RegistrationAnnualVacationByStatement = "Интеграционный поток предназначен для регистрации ежегодного отпуска по заявлению.";
      public const string RegistrationOtherVacation = "Интеграционный поток предназначен для регистрации прочего отпуска.";
      public const string RegistrationAnnualVacationBySchedule = "Интеграционный поток предназначен для регистрации ежегодного отпуска по графику отпусков.";
      public const string GetSupernikaActionResult = "Интеграционный поток предназначен для передачи результата нажатия на действие в центр уведомлений ПМД.";
      public const string CompensationForPartOfAnnualPaidLeave = "Интеграционный поток предназначен для компенсации части ежегодного оплачиваемого отпуска.";
      public const string RegistrationRecallVacation = "Интеграционный поток предназначен для создания мероприятия «Отзыв из отпуска» из КЭДО в АСУП.";
      public const string SendStatusVacationNoticeInSap = "Интеграционный поток предназначен для передачи статусов отправки уведомлений об отпуске.";
      public const string ValidateWeekendWorkStatement = "Интеграционный поток предназначен для проверки заявления на работу в выходной и/или праздничный день.";
      public const string CreateWeekendWorkOrderDocument = "Интеграционный поток предназначен для передачи приказа на выход сотрудника в выходной и/или праздничный день из АСУП в КЭДО.";
      public const string WeekendWorkEvent = "Интеграционный поток предназначен для создания мероприятия «Регистрация работы в выходной и/или праздничный день» из КЭДО в АСУП.";
      public const string WeekendWorkCancelationEvent = "Интеграционный поток предназначен для внесения изменений в действующий приказ на выход сотрудника в выходной и/или праздничный день.";
      public const string LrdPaymentEvent = "Интеграционный поток предназначен для создания мероприятия «Регистрация выплат ЛНА» из КЭДО в АСУП.";
      public const string LrdPaymentCheck = "Интеграционный поток предназначен для проверки заявления на выплату ЛНА.";
      public const string LrdPaymentChildren = "Интеграционный поток предназначен для получения списка детей работника.";
      public const string LrdPaymentOrder = "Интеграционный поток предназначен для передачи приказа на выплату по ЛНА из АСУП в КЭДО.";
      public const string UpdateOrCreateWorkingTimeCalendar = "Интеграционный поток предназначен для синхронизации календаря рабочего времени сотрудника.";
      public const string EmploymentContractAdditionalAgreement = "Интеграционный поток предназначен для передачи дополнительного соглашения к трудовому договору из АСУП в КЭДО.";
      public const string SendVacationsForPaymentChecking = "Интеграционный поток предназначен для запроса признака оплаты отпусков по сотруднику.";
      public const string VacationsPaymentInfo = "Интеграционный поток предназначен для передачи статусов оплаты отпусков по сотруднику.";
    }

    /// <summary>
    /// Список наименований действий Потоков внешних систем.
    /// </summary>
    [Public]
    public static class ExternalSystemsFlowActionNames
    {
      public const string GetSendingAsyncDataResultFromKSIP = "UpdateMessageSendingAsyncResult";
      
      public const string GetResultStamping = "GetResultStamping";
      
    }

    /// <summary>
    /// Коды результата асинхронной отправки данных из КСИП в АСУП.
    /// </summary>
    public static class SendingAsyncDataResultFromKSIP
    {
      /// <summary>
      /// Код успешной отправки данных из КСИП в АСУП.
      /// </summary>
      public const string Successfully = "Successfully";
      
      /// <summary>
      /// Код ошибки отправки данных из КСИП в АСУП.
      /// </summary>
      public const string Error = "Error";
    }

    #endregion

    #region Общие константы (ответы серверов, коды ответов).

    /// <summary>
    /// Общие значения в запросах.
    /// </summary>
    [Public]
    public static class Request
    {
      /// <summary>
      /// Константы статуса обработки на запрос.
      /// </summary>
      [Public]
      public static class ResultCode
      {
        // Данные успешно получены в RX.
        public const string Successfully = "Successfully";
        // Запись заблокирована, изменение нельзя внести. Необходимо повторить запрос позже.
        public const string Block = "Block";
        // Ошибка обработки.
        public const string Error = "Error";
      }
      
      /// <summary>
      /// Код результата обработки исходящего сообщения.
      /// </summary>
      [Public]
      public static class ResponseCode
      {
        // Сообщение успешно обработано.
        public const int SuccessfullProcessing = 200;
        // Сервис недоступен.
        public const int ServiceNotFound = 503;
      }
      
      /// <summary>
      /// Типы HTTP-методов.
      /// </summary>
      [Public]
      public static class TypeHttpMethods
      {
        // Get.
        public const string Get = "GET";
        
        // Post.
        public const string Post = "POST";
      }
      
      /// <summary>
      /// Типы контента.
      /// </summary>
      [Public]
      public static class ContentTypes
      {
        // Тип multipart/form-data.
        public const string MultipartFormData = "multipart/form-data";
        
        // Тип xml.
        public const string Xml = "application/xml";
      }
      
      // Авторизация.
      public const string Authorization = "Authorization";
      
      // Тип авторизации по умолчанию.
      public const string TypeAuthorization = "Basic";
      
    }

    #endregion

    #region Создание УЗ RX и КЭДО.

    /// <summary>
    /// Константы для создания учетных записей RX и КЭДО.
    /// </summary>
    [Public]
    public static class CreateAccount
    {
      /// <summary>
      /// Провайдеры.
      /// </summary>
      [Public]
      public static class Providers
      {
        [Public]
        public const string Ad = "AD";
        [Public]
        public const string Keycloak = "Keycloak";
      }
      
      /// <summary>
      /// Тип УЗ.
      /// </summary>
      [Public]
      public static class AccountType
      {
        [Public]
        public const string Rx = "RX";
        [Public]
        public const string Kedo = "KEDO";
      }
    }

    /// <summary>
    /// Первые цифры личного телефона сотрудника, который может быть использован для выпуска сертификата.
    /// </summary>
    [Public]
    public const string PersonalPhoneFirstDigits = "79";
    
    #endregion

    #region Поток "Передача документа на проштамповку".
    
    /// <summary>
    /// Статус выполнения проштамповки.
    /// </summary>
    [Public]
    public static class StatusResultStamping
    {
      // Успешно.
      public const string Success = "SUCCESS";
      
      // Неуспешно.
      public const string Fail = "FAIL";
      
    }
    
    #endregion
    
    #region Действия Push уведомлений в Супернику.
    
    /// <summary>
    /// Константы действий Push уведомлений в Супернику.
    /// </summary>
    [Public]
    public static class SupernikaActions
    {
      /// <summary>
      /// Заголовки действий.
      /// </summary>
      public static class ActionNames
      {
        // Пометить как прочитанное.
        public const string MarkRead = "Пометить как прочитанное";
        // Выполнить.
        public const string Complete = "Выполнить";
        // Согласовать.
        public const string Approved = "Согласовать";
        // На доработку.
        public const string ForRework = "На доработку";
        // Отказать.
        public const string Refuse = "Отказать";
        // Принято к сведению.
        public const string Noted = "Принято к сведению";
      }
      
      /// <summary>
      /// Типы действий в Супернике.
      /// </summary>
      public static class ActionTypes
      {
        // Согласовать.
        public const string Approve = "approve";
        // Отклонить.
        public const string Reject = "reject";
        // Другое.
        public const string Other = "other";
      }
      
      /// <summary>
      /// Guid действий.
      /// </summary>
      public static class ActionGuids
      {
        // Пометить как прочитанное.
        public static readonly Guid MarkRead = Guid.Parse("1F118D88-18E0-4D49-B16E-5D347E3C49CA");
        // Выполнить.
        public static readonly Guid Complete = Guid.Parse("927F340D-8BC7-4BF7-9C49-262F1FD9E0A9");
        // Согласовать.
        public static readonly Guid Approved = Guid.Parse("C7773B57-3EE3-4EA8-9184-B7959DFBC268");
        // На доработку.
        public static readonly Guid ForRework = Guid.Parse("94711B50-6035-42B8-9241-D5C603599AF0");
        // Отказать.
        public static readonly Guid Refuse = Guid.Parse("9B57B3CC-9CA0-4D18-B924-95BABE3B37E7");
        // Принято к сведению.
        public static readonly Guid Noted = Guid.Parse("C1CE44FC-CD5D-47FB-83C4-376E9F88879E");
      }
      
      /// <summary>
      /// Наименования блоков в RX.
      /// </summary>
      public static class BlockNamesRx
      {
        // Простое задание HR Pro.
        public const string HRAssignment = "Простое задание HR Pro";
        // Задание на согласование документов.
        public const string DocumentApprovalAssignment = "Задание на согласование документов";
        // Уведомление HR Pro.
        public const string HRNotice = "Уведомление HR Pro";
      }
      
      /// <summary>
      /// Наименования результатов выполнения в RX.
      /// </summary>
      public static class ResultNamesRx
      {
        // Пометить как прочитанное.
        public const string MarkRead = "MarkRead";
        // Выполнить.
        public const string Complete = "Complete";
        // Согласовать.
        public const string Approved = "Approved";
        // На доработку.
        public const string ForRework = "ForRework";
        // Отказать.
        public const string Refuse = "Refuse";
        // Принято к сведению.
        public const string Noted = "Noted";
      }
    }
    
    /// <summary>
    /// Параметры сообщений в Message broker.
    /// </summary>
    [Public]
    public static class MessageBrokerNotificationParameters
    {
      /// <summary>
      /// Методы доставки.
      /// </summary>
      [Public]
      public static class DeliveryMethod
      {
        // Код метода Push.
        [Public]
        public const int Push = 0;
        [Public]
        // Код метода Pull.
        public const int Pull = 1;
      }
      
      /// <summary>
      /// Приоритет.
      /// </summary>
      [Public]
      public static class Priority
      {
        // Низкий.
        [Public]
        public const int Low = -1;
        // Средний.
        [Public]
        public const int Normal = 0;
        // Высокий.
        [Public]
        public const int High = 1;
      }
      
      /// <summary>
      /// Имена действий.
      /// </summary>
      [Public]
      public static class ActionParams
      {
        // Кнопка.
        [Public]
        public const string Button = "button";
        // Метка.
        [Public]
        public const string Label = "label";
      }
      
      /// <summary>
      /// Имя Api метода для запроса в Message broker.
      /// </summary>
      [Public]
      public const string RequestApi = "Messages";
      
      /// <summary>
      /// Статус-код нажатия кнопки.
      /// </summary>
      [Public]
      public const string CompletedButtonStatusCode = "100";
      
      /// <summary>
      /// Атрибуты push-сообщения для MessageBroker.
      /// </summary>
      [Public]
      public static class PushMessageProperties
      {
        /// <summary>
        /// Ключ для поиска заголовка push-уведомления.
        /// </summary>
        [Public]
        public const string PushTitleKey = "pushTitle";

        /// <summary>
        /// Ключ для поиска тела push-уведомления.
        /// </summary>
        [Public]
        public const string PushContentKey = "pushContent";
      }
    }

    #endregion

    /// <summary>
    /// Разделитель при передаче id записей в АО.
    /// </summary>
    [Public]
    public const char ListIdsDelimiter = ';';


    /// <summary>
    /// Возможные значения статуса согласования приказа для передачи из КЭДО в АСУП.
    /// </summary>
    /// <remarks>Используется в "IF-S-10532. Передача статуса приказа (КЭДО -> АСУП)".</remarks>
    [Public]
    public static class ApprovalStatusDoc
    {
      /// <summary>
      /// Подписано.
      /// </summary>
      public const string Sign = "1";
      
      /// <summary>
      /// Отклонен.
      /// </summary>
      public const string Reject = "2";
    }

    /// <summary>
    /// Возможные значения типа идентификатора для получения документа в методе GetDocument.
    /// </summary>
    [Public]
    public static class IdentityTypes
    {
      /// <summary>
      /// Тип RX.
      /// </summary>
      [Public]
      public const string RX = "RX";
      
      /// <summary>
      /// Тип SAP.
      /// </summary>
      [Public]
      public const string SAP = "SAP";
    }

    [Public]
    /// <summary>
    /// Описание замещения.
    /// </summary>
    public const string CommentSubstitute = "Автоматическое замещение для совместителей";

    /// <summary>
    /// Константа для мобильного представления при формировании гиперссылки на SmartApp в Супернике.
    /// </summary>
    [Public]
    public const string ViewMobile = "&view=mobile";

    /// <summary>
    /// Возможные значения типа идентификатора для получения документа в методе GetDocument.
    /// </summary>
    [Public]
    public static class WeekendWorkStatementKindCodes
    {
      /// <summary>
      /// Заявка на привлечение к работе в выходной и/или праздничный день.
      /// </summary>
      [Public]
      public const string WeekendWork = "02";
      
      /// <summary>
      /// Заявка на привлечение к работе в сверхурочное время.
      /// </summary>
      [Public]
      public const string OvertimeWork = "01";
    }

    /// <summary>
    /// Коды фактических отсутствий.
    /// </summary>
    [Public]
    public static class ActualAbsence
    {
      /// <summary>
      /// Заявление на отпуск (по прочим основаниям).
      /// </summary>
      [Public]
      public const string OtherVacation = "1107";
      
      /// <summary>
      /// Заявление на предоставление отпуска по КД (работник-инвалид).
      /// </summary>
      [Public]
      public const string VacationDisabledEmployee = "1110";
      
      /// <summary>
      /// Заявление на дополнительный отпуск на обустройство по программе Релокация.
      /// </summary>
      [Public]
      public const string VacationByRelocation = "1111";
    }
    
    /// <summary>
    /// Возможные значения кодов вида документа приказа на выход сотрудника в выходной и/или праздничный день.
    /// </summary>
    [Public]
    public static class WeekendWorkOrderKindCodes
    {
      /// <summary>
      /// Сверхурочная работа.
      /// </summary>
      [Public]
      public const string OvertimeWork = "1";
      
      /// <summary>
      /// Работа в выходной.
      /// </summary>
      [Public]
      public const string WeekendWork = "2";
      
      /// <summary>
      /// Отмена приказа.
      /// </summary>
      [Public]
      public const string Cancelation = "3";
    }
  }
}