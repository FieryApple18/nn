using System;
using Sungero.Core;

namespace DirRX.NNOtherProcesses.Constants
{
  
  public static class Module
  {

    /// <summary>
    /// Ключ для необходимости фильтрации.
    /// </summary>
    [Public]
    public const string FilterLrdPaymentInRX = "CheckLrdPaymentInRX{0}";
    
    /// <summary>
    /// Вариант передачи пустого СНИЛСа.
    /// </summary>
    [Public]
    public const string EmptyInilaV1 = "000-000-000 00";
    
    /// <summary>
    /// Вариант передачи пустого СНИЛСа.
    /// </summary>
    [Public]
    public const string EmptyInilaV2 = "000 000 000 00";
    
    /// <summary>
    /// Количество документов, обрабатываемых за раз фоновым процессом UpdateTopAttributeInDocumentsJob.
    /// </summary>
    public const int UpdateTopAttributeInDocumentsJobBatchSize = 100;
    
    #region Группы константы.
    /// <summary>
    /// Группы константы.
    /// </summary>
    [Public]
    public static class ConstantsGroup
    {

      /// <summary>
      ///  Регистрация командировок.
      /// </summary>
      [Public]
      public static class RegistrationOfBusinessTrips
      {
        [Public]
        public const string Guid = "e0f0b0e7-0b25-4f19-952c-9379df7bec12";
        public const string Name = "Регистрация командировок";
      }

      /// <summary>
      /// Программа личных пожертвований.
      /// </summary>
      [Public]
      public static class PersonalDonationProgram
      {
        [Public]
        public const string Guid = "9af4f160-650a-4a9d-899c-3b1d16cc5b82";
        public const string Name = "Программа личных пожертвований";
      }

      /// <summary>
      /// Регистрация работы ВД/ПД/СР.
      /// </summary>
      [Public]
      public static class RegistrationOfWorkVDPDSR
      {
        [Public]
        public const string Guid = "ee24a1eb-56ba-42a8-a5ca-8b237cb81f14";
        public const string Name = "Регистрация работы ВД/ПД/СР";
      }

      /// <summary>
      /// Регистрация выплат по ЛНА.
      /// </summary>
      [Public]
      public static class RegistrationPaymentsOfLNA
      {
        [Public]
        public const string Guid = "9a0594f8-2b23-4879-9f3a-8e96dba16826";
        public const string Name = "Регистрация выплат по ЛНА";
      }

      /// <summary>
      /// Замещения ЛК.
      /// </summary>
      [Public]
      public static class Substitution
      {
        [Public]
        public const string Guid = "3404F89F-BB72-47E6-8F82-CEB5C72349DC";
        public const string Name = "Замещения";
      }

      /// <summary>
      /// Массовое формирование и подписание ДС о пересмотре ЗП.
      /// </summary>
      [Public]
      public static class SigningEmploymentContractAgreement
      {
        [Public]
        public const string Guid = "6B104686-4C9F-4682-82F7-C7F3EE2845C2";
        public const string Name = "Доп. соглашение о пересмотре ЗП";
      }
    }
    #endregion
    
    /// <summary>
    /// Идентификаторы ролей.
    /// </summary>
    [Public]
    public static class RolesGuid
    {
      /// <summary>
      /// Ответственный за РВ и СУ.
      /// </summary>
      [Public]
      public static readonly Guid ResponsibleWeekendWork = Guid.Parse("0ADDEFB4-0457-4BF1-987F-133849D63C40");
      
      /// <summary>
      /// Инициатор перевода.
      /// </summary>
      [Public]
      public static readonly Guid ResponsibleTransferProcess = Guid.Parse("7776512C-7D1D-4521-9FB7-BF6D34C9A19C");
    }
    
    /// <summary>
    /// GUID'ы типов документов.
    /// </summary>
    public static class DocumentType
    {
      // Приказ о направлении в командировку.
      [Public]
      public static readonly Guid NNBusinessTripOrderDocument = Guid.Parse("87714b4e-a63a-4825-b0ba-08c838cf01ff");
      // Заявление на удержание (программа личных пожертвований).
      [Public]
      public static readonly Guid NNRetentionStatementDocument = Guid.Parse("df37b42b-665c-4829-94f8-cfaabaf2e721");
      // Заявка на привлечение к работе в ВД/ПД/СР.
      [Public]
      public static readonly Guid WeekendWorkStatementDocument = Guid.Parse("c4470d2c-11b0-4e0b-bac0-d560ec1398e7");
      // Простой кадровый документ.
      [Public]
      public static readonly Guid HRSimpleDocument = Guid.Parse("bd6e61c4-7d12-40c6-9459-1ce3eb6d0cd0");
      // Приказ (распоряжение) о работе в ВД/ПД/СР.
      [Public]
      public static readonly Guid WeekendWorkOrderDocument = Guid.Parse("65cccf95-bb8c-43ea-abb6-7335e6942c94");
      // Согласие на работу в ВД/ПД/СР.
      [Public]
      public static readonly Guid WeekendWorkAgreementDocument = Guid.Parse("28c370c6-6160-4f25-9707-3f9451f69e82");
      // Заявление на выплату по ЛНА.
      [Public]
      public static readonly Guid LrdPaymentStatementDocument = Guid.Parse("578f6cfb-5fe7-4aec-ad13-94ecf361ac09");
      // Приказ на выплату по ЛНА.
      [Public]
      public static readonly Guid LrdPaymentOrderDocument = Guid.Parse("a62e018b-df7f-4c54-a5c4-2c9f0b02810a");
      // Заявление на доп. день отдыха.
      [Public]
      public static readonly Guid WeekendWorkRestStatementDocument = Guid.Parse("d03d2b52-ffdb-4984-9e0b-48204e17ed2f");
      // Приказ на доп. день отдыха.
      [Public]
      public static readonly Guid WeekendWorkRestOrderDocument = Guid.Parse("300f4e3f-c4ce-4a6d-b710-c21273226ffa");
    }
    
    /// <summary>
    /// Guid видов документов.
    /// </summary>
    public static class DocumentKind
    {
      // Приказ о направлении в командировку.
      [Public]
      public static readonly Guid BusinessTripOrder = Guid.Parse("164c2316-a489-4e60-af47-44bf2225198c");
      // Приказ об отмене командировки.
      [Public]
      public static readonly Guid CancelBusinessTripOrder = Guid.Parse("65c8aec0-0db1-4fc9-912c-dbcc8c50931b");
      // Приказ об изменении командировки.
      [Public]
      public static readonly Guid ChangeBusinessTripOrder = Guid.Parse("4a383fa2-a9dd-4b5a-880f-13e57678e15b");
      // Заявление на удержание (программа личных пожертвований).
      [Public]
      public static readonly Guid RetentionStatementDocument = Guid.Parse("0a714e8d-ba7a-40b6-be9c-fed73908c5b2");
      // Заявка на привлечение к работе в выходные или/и праздничные дни.
      [Public]
      public static readonly Guid WeekendWorkStatementDocument = Guid.Parse("1c8cc7d5-4d44-4b38-baff-0cbecde898cc");
      // Заявка на привлечение к работе в сверхурочное время.
      [Public]
      public static readonly Guid OvertimeWorkStatementDocument = Guid.Parse("be02cd65-6ac9-4482-af5c-9425dab371c9");
      // Согласие на привлечение к работе в выходной и/или праздничный день.
      [Public]
      public static readonly Guid WeekendWorkAgreementDocument = Guid.Parse("a0eb6907-66bd-4419-9196-f622cf65ff53");
      // Согласие на привлечение к работе в сверхурочное время.
      [Public]
      public static readonly Guid OvertimeWorkAgreementDocument = Guid.Parse("5c9078ec-b57a-4b47-9e8c-9312947a9169");
      // Приказ на привлечение к работе в сверхурочное время.
      [Public]
      public static readonly Guid OvertimeWorkOrderDocument = Guid.Parse("ac5cc7bd-535f-4d63-abe9-18ba9fc22ece");
      // Приказ (распоряжение) о работе в выходные/нерабочие праздничные дни.
      [Public]
      public static readonly Guid WeekendWorkOrderDocument = Guid.Parse("0633c531-ad3f-40ec-b885-3c0b6da84ed0");
      // Согласие на привлечение к работе в сверхурочное время.
      [Public]
      public static readonly Guid WeekendWorkCancelationOrderDocument = Guid.Parse("733a1004-0fc3-4886-825f-0579998e8b91");
      // Заявлению на выплату по ЛНА.
      [Public]
      public static readonly Guid LrdPaymentStatementDocument = Guid.Parse("c7a90606-13f3-43cb-93ae-7d88706efc24");
      // Приказ на выплату по ЛНА.
      [Public]
      public static readonly Guid LrdPaymentOrderDocument = Guid.Parse("5b952fd0-b8e2-487c-ae0e-72cb09c00e4c");
      // Дополнительное соглашение к трудовому договору.
      [Public]
      public static readonly Guid AdditionalAgreementKind = Guid.Parse("78348fc9-0e4f-498f-82f5-812989723356");
      // Заявление на доп. день отдыха за РВД.
      [Public]
      public static readonly Guid WeekendWorkRestStatementDocument = Guid.Parse("2C444D26-ED9A-4120-B2D2-2E4CF43AFB76");
      // Приказ на предоставление доп. дня отдыха.
      [Public]
      public static readonly Guid GrantRestOrderDocument = Guid.Parse("03FEF5A9-512C-4CF8-97E9-63B9F880AAD8");
      // Приказ об отмене доп. дня отдыха.
      [Public]
      public static readonly Guid CancelationRestOrderDocument = Guid.Parse("06CF32E1-4F04-4B8E-92D3-70B9C8973CC4");
    }

    /// <summary>
    /// Коды по классификации Минтруда.
    /// </summary>
    public static class CodeMinistryLabor
    {
      // Код прочих документов.
      [Public]
      public const string OtherDocuments = "12.999";
    }
    
    #region Прикладные константы
    /// <summary>
    /// Константы кодов видов документов в АСУП.
    /// </summary>
    [Public]
    public static class SapKindsCodes
    {
      /// <summary>
      /// Код вида документа "Мнение профсоюзной организации".
      /// </summary>
      [Public]
      public const string TradeUnionOrganizationOpinionDocKind = "";
      /// <summary>
      /// Код вида документа "Подтверждающий документ к заявлению на выплату по ЛНА".
      /// </summary>
      [Public]
      public const string SupportingDocumentForLNADocKind = "";
      /// <summary>
      /// Код вида документа "Приказ о направлении в командировку".
      /// </summary>
      [Public]
      public const string BusinessTripOrderDocKind = "07";
      /// <summary>
      /// Код вида документа "Приказ об отмене командировки".
      /// </summary>
      [Public]
      public const string CancelBusinessTripOrder = "08";
      /// <summary>
      /// Код вида документа "Приказ об изменении командировки".
      /// </summary>
      [Public]
      public const string ChangeBusinessTripOrder = "09";
      /// <summary>
      /// Код вида документа "Заявление на удержание".
      /// </summary>
      [Public]
      public const string RetentionStatementDocKind = "12";
      /// <summary>
      /// Код вида документа "Заявка на привлечение к работе в выходные или/и праздничные дни".
      /// </summary>
      [Public]
      public const string WeekendWorkStatementDocKind = "";
      /// <summary>
      /// Код вида документа "Заявка на привлечение к работе в сверхурочное время".
      /// </summary>
      [Public]
      public const string OvertimeWorkStatementDocKind = "";
      /// <summary>
      /// Код вида документа "Согласие на привлечение к работе в выходной и/или праздничный день".
      /// </summary>
      [Public]
      public const string WeekendWorkAgreementDocKind = "21";
      /// <summary>
      /// Код вида документа "Согласие на привлечение к работе в сверхурочное время".
      /// </summary>
      [Public]
      public const string OvertimeWorkAgreementDocKind = "22";
      /// <summary>
      /// Код вида документа "Приказ (распоряжение) о работе в ВД/ПД/СР".
      /// </summary>
      [Public]
      public const string WeekendWorkOrderDocKind = "23";
      /// <summary>
      /// Код вида документа "Приказ (распоряжение) о сверхурочной работе".
      /// </summary>
      [Public]
      public const string OvertimeWorkOrderDocKind = "24";
      /// <summary>
      /// Код вида документа "Приказ (распоряжение) об отмене работы по приказу (распоряжению)".
      /// </summary>
      [Public]
      public const string WeekendWorkCancelationOrderDocKind = "25";
      /// <summary>
      /// Код вида документа "Заявление на выплату по ЛНА".
      /// </summary>
      [Public]
      public const string LrdPaymentStatementDocKind = "20";
      /// <summary>
      /// Код вида документа "Приказ (распоряжение) о выплате по ЛНА".
      /// </summary>
      [Public]
      public const string LrdPaymentOrderDocKind = "03";
      /// <summary>
      /// Код вида документа "Дополнительное соглашение к трудовому договору".
      /// </summary>
      [Public]
      public const string AdditionalAgreementDocKind = "31";
      /// <summary>
      /// Код вида документа "Заявление на доп. день отдыха за РВД".
      /// </summary>
      [Public]
      public const string WeekendWorkRestStatementDocKind = "714";
      /// <summary>
      /// Код вида документа "Приказ на предоставление доп. дня отдыха".
      /// </summary>
      [Public]
      public const string GrantRestOrderDocKind = "32";
      /// <summary>
      /// Код вида документа "Приказ об отмене доп. дня отдыха".
      /// </summary>
      [Public]
      public const string CancelationRestOrderDocKind = "33";
    }

    /// <summary>
    /// Формат ключа для кэширования запроса для получения списка детей сотрудника.
    /// </summary>
    public const string LrdPaymentChildrenCacheKeyFormat = "LrdPaymentChildren{0}";
    
    [Public]
    public static class ConstantsSetting
    {
      /// <summary>
      /// Максимальное количество доп. согласующих.
      /// </summary>
      [Public]
      public static class MaxNumberAdditionalMatchingElement
      {
        [Public]
        public const string Guid = "644ED187-F181-4758-B959-44558F8C69EF";
        public const string Name = "Максимальное количество доп. согласующих";
      }
    
      /// <summary>
      /// Общее. Оформление заявления по основному месту работы.
      /// </summary>
      [Public]
      public static class RegistrationStatementMainPlace
      {
        [Public]
        public const string Guid = "a27b641b-afd5-4008-9de5-9d6625697c99";
        [Public]
        public const string Name = "Общее. Оформление заявления по основному месту работы";
      }
      
      /// <summary>
      /// Формат документа "Заявление на удержание".
      /// </summary>
      [Public]
      public static class RetentionStatementDocumentExtension
      {
        [Public]
        public const string Guid = "af65cc8a-c97a-413b-a5df-0c90e8281591";
        public const string Name = "Интеграция. Формат файла для документа \"Заявление на удержание\", принимаемый из АСУП";
      }

      /// <summary>
      /// Ид варианта процесса "Отправка статуса документа 'Заявление на удержание' в АСУП".
      /// </summary>
      [Public]
      public static class SendRetentionStatementStatusProcessKindId
      {
        [Public]
        public const string Guid = "14550231-6dc7-4a22-a605-4c1cc397363a";
        public const string Name = "Интеграция. Благотворительность. Ид варианта процесса 'Отправка статуса документа \"Заявление на удержание\" в АСУП'";
      }

      /// <summary>
      /// Ид варианта процесса "Задачи на ручное разблокирование ИТ0015".
      /// </summary>
      [Public]
      public static class ManualHoldingEventProcessKindId
      {
        [Public]
        public const string Guid = "63c24c57-3b5d-43dc-905c-0bdbcb69a33a";
        public const string Name = "Интеграция. Благотворительность. Ид варианта процесса 'Задача на ручное разблокирование ИТ0015'";
      }

      /// <summary>
      /// Приказ о направлении в командировку/служебную поездку.
      /// </summary>
      [Public]
      public static class BusinessTripOrder
      {
        [Public]
        public const string Guid = "89f67595-c1ba-40e7-9b0a-aa0db53369b8";
        public const string Name = "Интеграция. Коды видов приказа о направлении в командировку/служебную поездку, принимаемые из АСУП (заполняются через разделитель \",\")";
      }

      /// <summary>
      /// Приказ об изменении командировки/служебной поездки.
      /// </summary>
      [Public]
      public static class ChangeBusinessTripOrder
      {
        [Public]
        public const string Guid = "e9325243-3d1b-4a42-ba6d-987a969e0c71";
        public const string Name = "Интеграция. Коды видов приказа о изменении командировки/служебной поездки, принимаемые из АСУП (заполняются через разделитель \",\")";
      }

      /// <summary>
      /// Приказ об отмене командировки.
      /// </summary>
      [Public]
      public static class CancelBusinessTripOrder
      {
        [Public]
        public const string Guid = "131d53d2-8224-4e36-b6fe-8c6e9470eff4";
        public const string Name = "Интеграция. Коды видов приказа о отмене командировки/служебной поездки, принимаемые из АСУП (заполняются через разделитель \",\")";
      }

      /// <summary>
      /// Формат документа "Приказ на командировку".
      /// </summary>
      [Public]
      public static class BusinessTripOrderExtension
      {
        [Public]
        public const string Guid = "12906b08-a311-44c5-bc38-12160f3b58f8";
        public const string Name = "Интеграция. Формат файла для документа \"Приказ на командировку\", принимаемый из АСУП";
      }

      /// <summary>
      /// Приказы на командировку. Требуется подписание?
      /// </summary>
      [Public]
      public static class BusinessTripOrderNeedApprove
      {
        [Public]
        public const string Guid = "9b7b9315-1b96-472c-ae48-e6b6075a5f39";
        public const string Name = "Интеграция. Приказы на командировку. Значение свойства \"Требуется подписание\" в задаче на обработку документов, связанных с работой, при отправке документов из АСУП";
      }

      /// <summary>
      /// Приказы на командировку. Требуется ознакомление сотрудников?
      /// </summary>
      [Public]
      public static class BusinessTripOrderNeedAcquaintance
      {
        [Public]
        public const string Guid = "4721841e-30ae-4d75-92ef-837d77d5a8cd";
        public const string Name = "Интеграция. Приказы на командировку. Значение свойства \"Требуется ознакомление\" в задаче на обработку документов, связанных с работой, при отправке документов из АСУП";
      }
      
      /// <summary>
      /// Формат документа "Приказ на ВД/ПД/СР".
      /// </summary>
      [Public]
      public static class WeekendWorkOrderExtension
      {
        [Public]
        public const string Guid = "88f6d61e-1d1b-46ec-a3ce-2e4c0c1f1e7c";
        public const string Name = "Интеграция. Формат файла для документа \"Приказ на ВД/ПД/СР\", принимаемый из АСУП";
      }
      
      /// <summary>
      /// Список ИД типов выплат, для которых в заявлении необходимо указывать ребёнка сотрудника.
      /// </summary>
      [Public]
      public static class ChildSupportPaymentTypesIds
      {
        [Public]
        public const string Guid = "8633d2f1-0864-41b9-b39a-ad28b9cea6ca";
        public const string Name = "Выплаты по ЛНА. Список ИД типов выплат, для которых в заявлении необходимо указывать ребёнка сотрудника";
      }
      
      /// <summary>
      /// ИД типа выплаты, для которого в заявлении необходимо указывать ребёнка-инвалида.
      /// </summary>
      [Public]
      public static class DisabledChildPaymentTypeId
      {
        [Public]
        public const string Guid = "75824cb6-595c-469c-b8e0-8bf188b97044";
        public const string Name = "Выплаты по ЛНА. ИД типа выплаты, для которого в заявлении необходимо указывать ребёнка-инвалида";
      }
      
      /// <summary>
      /// ИД типа выплаты, на рождение ребенка, а также усыновление детей-сирот и детей, оставшихся без попечения родителей.
      /// </summary>
      [Public]
      public static class ChildBirthPaymentTypeId
      {
        [Public]
        public const string Guid = "CF88E192-D33B-4BC9-8C4A-B7384FDFAFDE";
        public const string Name = "Выплаты по ЛНА. ИД типа выплаты, на рождение ребенка, а также усыновление детей-сирот и детей, оставшихся без попечения родителей";
      }
      
      /// <summary>
      /// ИД типа выплаты, для которого в заявлении необходимо указывать дату увольнения.
      /// </summary>
      [Public]
      public static class RetirementPaymentTypeIds
      {
        [Public]
        public const string Guid = "0ba50b7b-710b-419f-b83f-9687a8e74d9c";
        public const string Name = "Выплаты по ЛНА. Список ИД типов выплат, для которых в заявлении необходимо указывать дату увольнения";
      }
      
      /// <summary>
      /// Ид варианта процесса для запроса мнений.
      /// </summary>
      [Public]
      public static class OpinionTask
      {
        [Public]
        public const string Guid = "d7ae75ef-c225-4929-a921-4495dfa91a48";
        public const string Name = "Общее. Ид варианта процесса для запроса мнений";
      }
      
      /// <summary>
      /// Время кэширования в минутах для запроса во внешнюю систему для получения информации о детях сотрудника.
      /// </summary>
      [Public]
      public static class LrdPaymentChildrenCacheTime
      {
        [Public]
        public const string Guid = "c636e90a-735d-4613-b6d8-3e2c6f03dc88";
        public const string Name = "Выплаты по ЛНА. Время кэширования в минутах для запроса во внешнюю систему для получения информации о детях сотрудника";
      }
      
      /// <summary>
      /// Формат документа "Приказ на вплату по ЛНА".
      /// </summary>
      [Public]
      public static class LrdPaymentOrderExtension
      {
        [Public]
        public const string Guid = "e55790ac-6330-4b01-83b2-adf653f8038a";
        public const string Name = "Интеграция. Формат файла для документа \"Приказ на выплату по ЛНА\", принимаемый из АСУП";
      }
      
      /// <summary>
      /// Должность руководителя организации в "Заявление на выплату по ЛНА", если организация сотрудника "ПАО 'ГМК Норильский Никель'".
      /// </summary>
      [Public]
      public static class LrdPaymentStatementCEOJobTitle
      {
        [Public]
        public const string Guid = "96938C71-4DB1-4D5F-BE54-E1ECCC412424";
        public const string Name = "Заявление на выплату по ЛНА. Должность руководителя организации, кому";
      }
      
      /// <summary>
      /// Руководитель организации в "Заявление на выплату по ЛНА", если организация сотрудника "ПАО 'ГМК Норильский Никель'".
      /// </summary>
      [Public]
      public static class LrdPaymentStatementCEO
      {
        [Public]
        public const string Guid = "BCEDFD63-ABC0-483B-9D82-57BDE82491B2";
        public const string Name = "Заявление на выплату по ЛНА. Руководитель организации, кому";
      }
      
      /// <summary>
      /// НОР "ПАО 'ГМК Норильский Никель'" для проверки организации сотрудника в "Заявление на выплату по ЛНА".
      /// </summary>
      [Public]
      public static class LrdPaymentStatementBusinessUnit
      {
        [Public]
        public const string Guid = "6A24654B-278C-41CA-B5C7-7B2DC5D6B5DB";
        public const string Name = "Заявление на выплату по ЛНА. Организация \"ПАО ГМК Норильский Никель\"";
      }
      
      /// <summary>
      /// Количество дней хранения записей справочника "Замещение ЛК".
      /// </summary>
      [Public]
      public static class SubstitutionPersonalAccountRetentionDays
      {
        [Public]
        public const string Guid = "CBCFF959-6A18-486D-889B-2BB4BC32B5FE";
        public const string Name = "Замещения. Количество дней хранения записей справочника \"Замещение ЛК\"";
      }
      
      /// <summary>
      /// Идентификатор системы КЭДО по умолчанию в справочнике "Список систем для замещения".
      /// </summary>
      [Public]
      public static class SubstitutionSystemKedoSystemId
      {
        [Public]
        public const string Guid = "46877AE3-E69A-4D03-BBCC-E0DC4BBA17AC";
        public const string Name = "Замещения. ИД системы КЭДО по умолчанию для создания замещений в RX";
      }
      
      /// <summary>
      /// Формат документа "Дополнительное соглашение к трудовому договору".
      /// </summary>
      [Public]
      public static class EmploymentContractAdditionalAgreementExtension
      {
        [Public]
        public const string Guid = "98042DE0-AD18-4082-BC5D-92D2712F276C";
        public const string Name = "Интеграция. Формат файла для документа \"Дополнительное соглашение к трудовому договору\", принимаемый из АСУП";
      }
      
      /// <summary>
      /// ДС. Тема письма Специалистам ОКА.
      /// </summary>
      [Public]
      public static class NotSignedAdditionalAgreementLetterSubject
      {
        [Public]
        public const string Guid = "53497B11-4D4C-47F8-9E1A-684EBAB1584B";
        public const string Name = "ДС. Тема письма Специалистам ОКА";
      }
      
      /// <summary>
      /// ИД видов компенсации, для которых необходимо создавать заявление на доп. день отдыха.
      /// </summary>
      [Public]
      public static class WeekendWorkCompensationKindsForRestStatement
      {
        [Public]
        public const string Guid = "A38A8C1F-6132-4366-A851-80AC16FA75D4";
        public const string Name = "Регистрация работы ВД/ПД/СР. ИД видов компенсации, для которых необходимо создавать заявление на доп. день отдыха";
      }
      
      /// <summary>
      /// Формат документа "Приказ на доп. день отдыха".
      /// </summary>
      [Public]
      public static class WeekendWorkRestOrderExtension
      {
        [Public]
        public const string Guid = "F7F35F11-7898-487B-B0A8-30A6CE299930";
        public const string Name = "Интеграция. Формат файла для документа \"Приказ на доп. день отдыха\", принимаемый из АСУП";
      }
      
      /// <summary>
      /// Оплачиваемый перерыв.
      /// </summary>
      [Public]
      public static class PaidBreak
      {
        [Public]
        public const string Guid = "53FADA99-A3AB-4361-9311-9D3D2B36042B";
        public const string Name = "Оплачиваемый перерыв";
      }
    }

    /// <summary>
    /// Значения по умолчанию для прикладных констант.
    /// </summary>
    [Public]
    public static class ConstantsDefaultValues
    {
      /// <summary>
      /// Максимальное количество доп. согласующих по умолчанию.
      /// </summary>
      public const int MaxNumberAdditionalMatchingElement = 2;
      /// <summary>
      /// Значение по умолчанию. Общее. Оформление заявления по основному месту работы.
      /// </summary>
      [Public]
      public const string RegistrationStatementMainPlace = "Оформление заявления доступно только по основному месту работы сотрудника";
      /// <summary>
      /// Ид по умолчанию, Вариант процесса "Задача на ручное разблокирование ИТ0015".
      /// </summary>
      public const long ManualHoldingEventProcessKindId = 0;
      /// <summary>
      /// Значение по умолчанию. Приказ о направлении в командировку/служебную поездку.
      /// </summary>
      public const string BusinessTripOrder = "01";
      /// <summary>
      /// Значение по умолчанию. Приказ об изменении командировки/служебной поездки.
      /// </summary>
      public const string ChangeBusinessTripOrder = "02";
      /// <summary>
      /// Значение по умолчанию. Приказ об отмене командировки.
      /// </summary>
      public const string CancelBusinessTripOrder = "03";
      /// <summary>
      /// Значение по умолчанию. Формат документа "Приказ на командировку".
      /// </summary>
      public const string BusinessTripOrderExtension = "doc";
      /// <summary>
      /// Значение по умолчанию. Приказы на командировку. Требуется подписание?
      /// </summary>
      public const bool BusinessTripOrderNeedApprove = true;
      /// <summary>
      /// Значение по умолчанию. Приказы на командировку. Требуется ознакомление сотрудников?
      /// </summary>
      public const bool BusinessTripOrderNeedAcquaintance = false;
      /// <summary>
      /// Значение по умолчанию. Формат документа "Приказ на ВД/ПД/СР".
      /// </summary>
      public const string WeekendWorkOrderExtension = "doc";
      /// <summary>
      /// Значение по умолчанию. Ид варианта процесса для запроса мнений.
      /// </summary>
      public const long OpinionTask = -1;
      /// <summary>
      /// Время кэширования в минутах для запроса во внешнюю систему для получения информации о детях сотрудника.
      /// </summary>
      public const int LrdPaymentChildrenCacheTime = 60;
      /// <summary>
      /// Значение по умолчанию. Формат документа "Приказ на вплату по ЛНА".
      /// </summary>
      public const string LrdPaymentOrderExtension = "doc";
      
      /// <summary>
      /// Значение по умолчанию. Количество дней хранения записей справочника "Замещение ЛК".
      /// </summary>
      public const int SubstitutionPersonalAccountRetentionDays = 365;

      /// <summary>
      /// Идентификатор системы КЭДО по умолчанию в справочнике "Список систем для замещения".
      /// </summary>
      [Public]
      public const string SubstitutionSystemKedoSystemId = "IS0000098";
      
      /// <summary>
      /// Значение по умолчанию. Оплачиваемый перерыв.
      /// </summary>
      public const string PaidBreak = "Перерыв для отдыха и питания предусмотрен при условии работы более 4 часов в день (на работах, где по условиям производства (работы) предоставление перерыва для отдыха и питания невозможно, работодатель обязан обеспечить работнику возможность отдыха и приема пищи в рабочее время. Перечень таких работ, а также места для отдыха и приема пищи устанавливаются правилами внутреннего трудового распорядка).";
      
      /// <summary>
      /// Значение по умолчанию. Формат документа "Дополнительное соглашение к трудовому договору".
      /// </summary>
      public const string EmploymentContractAdditionalAgreementExtension = "doc";
      /// <summary>
      /// Значение по умолчанию. ДС. Тема письма Специалистам ОКА.
      /// </summary>
      public const string NotSignedAdditionalAgreementLetterSubject = "Сотрудники, не подписавшие Дополнительные соглашения к трудовому договору";
      
      /// <summary>
      /// ИД видов компенсации, для которых необходимо создавать заявление на доп. день отдыха.
      /// </summary>
      [Public]
      public const string WeekendWorkCompensationKindsForRestStatement = "";
      /// <summary>
      /// Значение по умолчанию. Формат документа "Приказ на доп. день отдыха".
      /// </summary>
      public const string WeekendWorkRestOrderExtension = "doc";
    }
    
    #endregion
  }
}