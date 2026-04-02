#!/usr/bin/env python3
# -*- coding: utf-8 -*-

from docx import Document
from docx.shared import Pt, Inches
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.style import WD_STYLE_TYPE

def create_document():
    doc = Document()

    # Set default font for the document
    style = doc.styles['Normal']
    font = style.font
    font.name = 'Calibri'
    font.size = Pt(11)

    # Title
    title = doc.add_heading('CHG00127788. Разработка сервиса Замещения в разных информационных системах', 0)
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER

    # Section 1: SubstitutionSystem
    doc.add_heading('Справочник «Список систем для замещения» (SubstitutionSystem)', level=1)
    doc.add_paragraph('Хранение реестра систем, поддерживающих механизм замещения сотрудников.')
    
    doc.add_heading('Общая информация:', level=2)
    info_list = [
        "Модуль: NNOtherProcesses;",
        "Базовый тип: Sungero.CoreEntities.DatabookEntry;",
        "GUID: 55f01030-a8b4-45be-9734-c09ee9ee0862;",
        "Отображение в проводнике: Да;",
        "Способ создания: Вручную (администратором);",
        "Доступ: Чтение доступно всем пользователям, создание/изменение/удаление только администраторами системы."
    ]
    for item in info_list:
        doc.add_paragraph(item, style='List Bullet')
    
    doc.add_paragraph('Примечание. При инициализации будет создана одна запись справочника по умолчанию для системы КЭДО.', style='Intense Quote')

    doc.add_heading('Таблица 1. Свойства справочника SubstitutionSystem', level=3)
    table = doc.add_table(rows=1, cols=6)
    table.style = 'Table Grid'
    hdr_cells = table.rows[0].cells
    headers = ['Наименование', 'Код', 'Тип данных', 'Обязательное', 'Уникальное', 'Описание']
    for i, text in enumerate(headers):
        hdr_cells[i].text = text
        hdr_cells[i].paragraphs[0].runs[0].bold = True
    
    data_props = [
        ['ИД', 'Id', 'Идентификатор', 'Да', 'Да', 'Системный идентификатор (автогенерируемый)'],
        ['Состояние', 'Status', 'Перечисление', 'Да', 'Нет', 'Статус записи'],
        ['Наименование', 'Name', 'Строка (250)', 'Да', 'Нет', 'Полное название системы'],
        ['Краткое наименование', 'ShortName', 'Строка (250)', 'Да', 'Нет', 'Краткое наименование системы'],
        ['Функции', 'Functions', 'Текст', 'Да', 'Нет', 'Функции системы'],
        ['ИД системы', 'SystemId', 'Строка (250)', 'Да', 'Да', 'Внешний идентификатор системы']
    ]
    
    for row_data in data_props:
        row_cells = table.add_row().cells
        for i, text in enumerate(row_data):
            row_cells[i].text = text

    doc.add_heading('Таблица 2. Значения перечисления «Состояние» для справочника SubstitutionSystem', level=3)
    table_enum = doc.add_table(rows=1, cols=2)
    table_enum.style = 'Table Grid'
    hdr_cells_enum = table_enum.rows[0].cells
    hdr_cells_enum[0].text = 'Код'
    hdr_cells_enum[1].text = 'Наименование'
    hdr_cells_enum[0].paragraphs[0].runs[0].bold = True
    hdr_cells_enum[1].paragraphs[0].runs[0].bold = True
    
    enum_data = [['Active', 'Действующая'], ['Inactive', 'Закрытая']]
    for row_data in enum_data:
        row_cells = table_enum.add_row().cells
        for i, text in enumerate(row_data):
            row_cells[i].text = text

    # Section 2: SubstitutionPersonalAccount
    doc.add_heading('Справочник «Замещения ЛК» (SubstitutionPersonalAccount)', level=1)
    doc.add_paragraph('Журнал истории создания замещений в различных системах (на данный момент только в КЭДО).')
    
    doc.add_heading('Общая информация:', level=2)
    info_list_2 = [
        "Модуль: NNOtherProcesses;",
        "Базовый тип: Sungero.CoreEntities.DatabookEntry;",
        "GUID типа: 3404F89F-BB72-47E6-8F82-CEB5C72349DC;",
        "Отображение в проводнике: Да;",
        "Способ создания: программно;",
        "Доступ: Чтение доступно всем пользователям, создание/изменение/удаление только программное."
    ]
    for item in info_list_2:
        doc.add_paragraph(item, style='List Bullet')

    doc.add_heading('Таблица 3. Свойства справочника SubstitutionPersonalAccount', level=3)
    table_sp = doc.add_table(rows=1, cols=6)
    table_sp.style = 'Table Grid'
    hdr_cells_sp = table_sp.rows[0].cells
    for i, text in enumerate(headers):
        hdr_cells_sp[i].text = text
        hdr_cells_sp[i].paragraphs[0].runs[0].bold = True
    
    data_props_sp = [
        ['ИД', 'Id', 'Идентификатор', 'Да', 'Да', 'Системный идентификатор (автогенерируемый)'],
        ['Наименование', 'Name', 'Строка (250)', 'Да', 'Нет', 'Формируется автоматически по шаблону: «Замещающий <Замещающий.ФИО> для <Замещаемый.ФИО> в <Система.Краткое наименование> с <Дата начала> по <Дата окончания>»'],
        ['Дата создания', 'Created', 'Дата и время', 'Нет', 'Нет', 'Время создания записи'],
        ['Система', 'SubstitutionSystem', 'Ссылка (Список систем для замещения)', 'Да', 'Нет', 'Система'],
        ['Замещаемый', 'Employee', 'Ссылка (Сотрудник)', 'Да', 'Нет', 'Сотрудник, которого замещают'],
        ['Замещающий', 'Substitute', 'Ссылка (Сотрудник)', 'Да', 'Нет', 'Сотрудник, который замещает'],
        ['Дата начала', 'StartDate', 'Дата', 'Да', 'Нет', 'Начало периода замещения'],
        ['Дата окончания', 'EndDate', 'Дата', 'Нет', 'Нет', 'Окончание периода замещения'],
        ['Статус отправки', 'SentStatus', 'Перечисление', 'Да', 'Нет', 'Статус отправки во внешнюю систему'],
        ['Текст ошибки', 'ErrorText', 'Текст', 'Нет', 'Нет', 'Сообщение об ошибке при отправке во внешнюю систему'],
        ['Источник', 'SourceTask', 'Строка (250)', 'Нет', 'Нет', 'ИД задачи, в рамках который создано замещение']
    ]
    
    for row_data in data_props_sp:
        row_cells = table_sp.add_row().cells
        for i, text in enumerate(row_data):
            row_cells[i].text = text

    doc.add_heading('Таблица 4. Значения перечисления «Статус отправки» для справочника SubstitutionPersonalAccount', level=3)
    table_enum_sp = doc.add_table(rows=1, cols=2)
    table_enum_sp.style = 'Table Grid'
    hdr_cells_enum_sp = table_enum_sp.rows[0].cells
    hdr_cells_enum_sp[0].text = 'Код'
    hdr_cells_enum_sp[1].text = 'Наименование'
    hdr_cells_enum_sp[0].paragraphs[0].runs[0].bold = True
    hdr_cells_enum_sp[1].paragraphs[0].runs[0].bold = True
    
    enum_data_sp = [
        ['Created', 'Создано'], 
        ['Sent', 'Отправлено в систему'], 
        ['Assigned', 'Замещение назначено'], 
        ['Error', 'Ошибка обработки'], 
        ['Cancelled', 'Отменено']
    ]
    for row_data in enum_data_sp:
        row_cells = table_enum_sp.add_row().cells
        for i, text in enumerate(row_data):
            row_cells[i].text = text

    # Section 3: Task Extensions
    doc.add_heading('Расширения типов задач', level=1)
    doc.add_paragraph('В типы задач «Задача на рассмотрение заявлений сотрудников» (StatementTask) и «Задача на подтверждение дат отпуска (по графику)» (RegistrationOfAnnualLeave) добавлена коллекция «Замещения» (SubstitutionsDirRX) для хранения данных о назначенных замещающих на период отпуска.')
    
    doc.add_heading('Таблица 5. Свойства коллекции «Замещения» (SubstitutionsDirRX)', level=3)
    table_coll = doc.add_table(rows=1, cols=6)
    table_coll.style = 'Table Grid'
    hdr_cells_coll = table_coll.rows[0].cells
    for i, text in enumerate(headers):
        hdr_cells_coll[i].text = text
        hdr_cells_coll[i].paragraphs[0].runs[0].bold = True
    
    data_props_coll = [
        ['ИД', 'Id', 'Идентификатор', 'Да', 'Да', 'Системный идентификатор строки (автогенерируемый)'],
        ['Замещающий', 'Substitute', 'Ссылка (Сотрудник)', 'Да', 'Нет', 'Замещающий сотрудник'],
        ['Система', 'SubstitutionSystem', 'Ссылка (Справочник)', 'Да', 'Нет', 'Система для замещения'],
        ['Дата начала', 'StartDate', 'Дата', 'Да', 'Нет', 'Начало периода замещения'],
        ['Дата окончания', 'EndDate', 'Дата', 'Да', 'Нет', 'Окончание периода замещения']
    ]
    
    for row_data in data_props_coll:
        row_cells = table_coll.add_row().cells
        for i, text in enumerate(row_data):
            row_cells[i].text = text

    doc.add_paragraph('Для типа задачи «Задача на рассмотрение заявлений сотрудников» StatementTask добавлены строковые свойства (таблица 6), чтобы при старте задачи в шаге вызывающих услуг заполнить коллекцию замещающих. Заполнение этих свойств происходит через строковые параметры услуги с разделителями «;». В серверном событии «До сохранения» (BeforeSave) задачи эти свойства преобразуются в строки коллекции.')
    doc.add_paragraph('Примечание. Преобразование выполняется с помощью серверного метода задачи AddSubstitutions() только когда задача создана, но еще не сохранена, т.е её признак IsInserted = true. В остальных случаях данное преобразование не будет выполнено.', style='Intense Quote')

    doc.add_heading('Таблица 6. Строковые свойства в задаче StatementTask для передачи данных из ЛК', level=3)
    table_str = doc.add_table(rows=1, cols=4)
    table_str.style = 'Table Grid'
    hdr_cells_str = table_str.rows[0].cells
    str_headers = ['Наименование', 'Код', 'Тип данных', 'Описание']
    for i, text in enumerate(str_headers):
        hdr_cells_str[i].text = text
        hdr_cells_str[i].paragraphs[0].runs[0].bold = True
        
    data_props_str = [
        ['ИД замещающих', 'SubstituteIdsDirRX', 'Строка (1000)', 'ID замещающих через «;»'],
        ['ИД систем', 'SystemIdsDirRX', 'Строка (1000)', 'ID систем через «;»'],
        ['Даты с', 'StartDatesDirRX', 'Строка (1000)', 'Даты начала через «;»'],
        ['Даты по', 'EndDatesDirRX', 'Строка (1000)', 'Даты окончания через «;»']
    ]
    
    for row_data in data_props_str:
        row_cells = table_str.add_row().cells
        for i, text in enumerate(row_data):
            row_cells[i].text = text

    # Section 4: Constants
    doc.add_heading('Константы модуля', level=1)
    doc.add_paragraph('В модуле NNOtherProcesses создана группа констант «Замещения» для хранения настроек функциональности замещений. В группе размещены следующие константы:')
    const_list = [
        "Константа «Замещения. Количество дней хранения записей справочника «Замещение ЛК»» типа «Целое» со значением по умолчанию 365 дней, доступна для редактирования администратором.",
        "Константа «Замещения. ИД системы КЭДО по умолчанию для создания замещений в RX» типа «Строка» со значением по умолчанию IS0000098, доступна для редактирования администратором."
    ]
    for item in const_list:
        doc.add_paragraph(item, style='List Number')

    # Section 5: Background Job
    doc.add_heading('Фоновый процесс «Очистка записей справочника «Замещения ЛК»» (CleanSubstitutionPersonalAccountJob)', level=1)
    doc.add_paragraph('Автоматическое удаление устаревших записей из справочника «Замещения ЛК» по истечении срока хранения.')
    
    doc.add_heading('Логика работы:', level=2)
    job_steps = [
        "Получить значение константы «Замещения. Количество дней хранения записей справочника «Замещение ЛК»».",
        "Расчитать Граничную дату: Граничная дата = Текущая дата – Значение константы.",
        "Найти записи в справочнике «Замещения ЛК», где «Дата окончания» строго меньше Граничной даты.",
        "Удалить найденные записи в транзакции.",
        "Залогировать количество удаленных записей и ошибок."
    ]
    for step in job_steps:
        doc.add_paragraph(step, style='List Number')
    doc.add_paragraph('Примечание. Если по каким-то причинам значение в константе будет пустым, то будет использовано значение 365.', style='Intense Quote')

    # Section 6: Script Blocks
    doc.add_heading('Блоки «Скрипт»', level=1)
    
    doc.add_heading('Скрипт «Создать замещения»', level=2)
    doc.add_paragraph('Данный блок «Скрипт» отвечает за создание записей в справочниках «Замещения» и «Замещения ЛК».')
    doc.add_heading('Логика работы:', level=3)
    script1_steps = [
        "Получить и провалидировать необходимые данные из задачи: ведущая задача, приказ, сотрудник.",
        "Получить строки коллекции «Замещения» из ведущей задачи.",
        "Для каждой строки коллекции создать запись в справочнике «Замещения ЛК».",
        "Для каждой строки коллекции создать запись в справочнике «Замещения», если в качестве системы указана система КЭДО."
    ]
    for step in script1_steps:
        doc.add_paragraph(step, style='List Number')
    doc.add_paragraph('Если запись была создана, то для записи справочника «Замещения ЛК» установить значения:', style='List Bullet')
    sub_steps = [
        "«Дата создания» = дата создания записи в справочнике «Замещения»;",
        "«Статус отправки» = «Замещение назначено»."
    ]
    for step in sub_steps:
        doc.add_paragraph(step, style='List Bullet')

    doc.add_heading('Скрипт «Обновить замещения»', level=2)
    doc.add_paragraph('Данный блок «Скрипт» отвечает за:')
    update_purposes = [
        "Актуализацию дат замещения в существующих записях, за закрытие существующих записей, а также за создание новых записей в справочнике «Замещения».",
        "Закрытие существующих записей и создание новых записей в справочнике «Замещения ЛК»."
    ]
    for purpose in update_purposes:
        doc.add_paragraph(purpose, style='List Number')
        
    doc.add_heading('Логика работы:', level=3)
    script2_steps = [
        "Получить и провалидировать необходимые данные из задачи: ведущая задача, приказ, сотрудник, заявление, исходный период отпуска, период отзыва/прерывания.",
        "Найти существующие записи в справочнике «Замещения» по сотруднику и исходному периоду отпуска.",
        "Для каждой найденной записи выполнить проверки:"
    ]
    for step in script2_steps:
        doc.add_paragraph(step, style='List Number')
        
    sub_checks = [
        ("3.1", "Если даты замещения за пределами периода отзыва/прерывания, то перейти к проверке следующей записи. Иначе перейти к п.3.2."),
        ("3.2", "Найти существующие записи в справочнике «Замещения ЛК», где «Статус отправки» = «Замещение назначено», по обрабатываемой записи справочника «Замещения». Для всех найденных записей установить «Статус отправки» = «Отменено»."),
        ("3.3", "Если даты замещения полностью совпадают с периодом отзыва/прерывания, то установить «Состояние» = «Закрытая» и перейти к проверке следующей записи. Иначе перейти к п.3.4."),
        ("3.4", "Если период отзыва/прерывания попадает на середину дат замещения, то:")
    ]
    
    for code, text in sub_checks:
        p = doc.add_paragraph(style='List Number')
        runner = p.add_run(f"{code} {text}")
        if code == "3.4":
             # Add sub-steps for 3.4
            pass
            
    sub_3_4 = [
        "Установить «Дата окончания» = Дата начала отзыва/прерывания – 1 день.",
        "Создать копированием новую запись и установить «Дата начала» = Дата окончания отзыва/прерывания + 1 день.",
        "Создать две новых записи в справочнике «Замещения ЛК».",
        "Перейти к проверке следующей записи."
    ]
    for step in sub_3_4:
        doc.add_paragraph(step, style='List Bullet')
        
    doc.add_paragraph("Иначе перейти к п.3.5.")
    
    sub_3_5 = [
        ("3.5", "Если период отзыва/прерывания попадает на начало дат замещения, то установить «Дата начала» = Дата окончания отзыва/прерывания + 1 день, создать новую запись в справочнике «Замещения ЛК», перейти к проверке следующей записи. Иначе перейти к п.3.6."),
        ("3.6", "Если период отзыва/прерывания попадает на конец дат замещения, то установить «Дата окончания» = Дата начала отзыва/прерывания - 1 день, создать новую запись в справочнике «Замещения ЛК», перейти к проверке следующей записи.")
    ]
    
    for code, text in sub_3_5:
        doc.add_paragraph(f"{code} {text}", style='List Number')
        
    doc.add_paragraph('Примечание. Для п.3.5 и п.3.6 дополнительно производится проверка, что новая дата начала/окончания не больше/меньше изначальной даты окончания/начала. Если окажется, что больше, то для записи будет установлено «Состояние» = «Закрытая».', style='Intense Quote')

    doc.add_heading('Скрипт «Закрыть замещения»', level=2)
    doc.add_paragraph('Данный блок «Скрипт» отвечает за закрытие записей в справочниках «Замещения» и «Замещения ЛК».')
    doc.add_heading('Логика работы:', level=3)
    script3_steps = [
        "Получить и провалидировать необходимые данные из задачи: ведущая задача, приказ, сотрудник, заявление, исходный период отпуска.",
        "Найти существующие записи в справочнике «Замещения» по сотруднику и исходному периоду отпуска.",
        "Для каждой найденной записи:"
    ]
    for step in script3_steps:
        doc.add_paragraph(step, style='List Number')
        
    sub_final = [
        "Найти существующие записи в справочнике «Замещения ЛК», где «Статус отправки» = «Замещение назначено», по обрабатываемой записи справочника «Замещения». Для всех найденных записей установить «Статус отправки» = «Отменено».",
        "Установить «Состояние» = «Закрытая»."
    ]
    for step in sub_final:
        doc.add_paragraph(step, style='List Bullet')

    # Section 7: Server Methods
    doc.add_heading('Серверные методы', level=1)
    doc.add_paragraph('Вся бизнес-логика инкапсулирована в модуле NNVacations. В таблице 7 представлен список серверных методов модуля.')
    
    doc.add_heading('Таблица 7. Список серверных методов модуля NNVacations', level=3)
    table_methods = doc.add_table(rows=1, cols=4)
    table_methods.style = 'Table Grid'
    hdr_cells_meth = table_methods.rows[0].cells
    meth_headers = ['Метод', 'Назначение', 'Параметры', 'Когда вызывается']
    for i, text in enumerate(meth_headers):
        hdr_cells_meth[i].text = text
        hdr_cells_meth[i].paragraphs[0].runs[0].bold = True
        
    data_methods = [
        ['CreateSubstitutionsFromTask', 'Создание замещений по данным из задачи', 'task (Задача)', 'После утверждения приказа об отпуске'],
        ['UpdateSubstitutionsFromTask', 'Обновление дат замещения', 'task (Задача)', 'При отзыве сотрудника из отпуска'],
        ['CloseSubstitutionsFromTask', 'Закрытие замещений', 'task (Задача)', 'При переносе или отмене отпуска'],
        ['TryExtractDataForSubstitutionFromTask', 'Валидация и сбор данных из задачи', '—', 'Структура данных или null'],
        ['GetVacationOrderDocumentFromTask', 'Получение приказа из задачи', '—', 'Документ «Приказ на отпуск»'],
        ['GetVacationStatementDocumentFromTask', 'Получение заявления из задачи', '—', 'Документ «Заявление на отпуск»'],
        ['GetOriginalPeriodFromStatement', 'Получение исходного периода из заявления', '—', 'Период отпуска'],
        ['GetRecallPeriodFromStatement', 'Получение периода отзыва из заявления', '—', 'Период отзыва'],
        ['FindExistingSubstitutions', 'Поиск существующих замещений', '—', 'Список замещений'],
        ['GetSubstitutionInfosFromTask', 'Чтение замещений из задачи', '—', 'Список структур с информацией'],
        ['GetSubstitutionTextForEmployee', 'Формирование текста для инструкции', '—', 'Форматированная строка'],
        ['GetCreateDateForSubstitution', 'Получение даты создания из истории', '—', 'Дата и время'],
        ['GetSubstitutionSystemsInfo', 'Получение HTML-списка активных систем', 'GET', 'При инициализации услуги'],
        ['GetSubstitutionSystems', 'Получение списка активных систем (JSON)', 'GET', 'При инициализации услуги'],
        ['GetEmployeesForSubstitution', 'Получение сотрудников для выбора замещающего', 'GET', 'При выборе сотрудника'],
        ['ValidateSubstitute', 'Проверка доступности замещающего', 'POST', 'При выборе сотрудника'],
        ['GetSubstituteAndSystemNames', 'Получение наименований по идентификаторам', 'POST', 'Перед отправкой услуги'],
        ['GetVacationPeriodsByAssignmentId', 'Получение периодов отпуска по заданию', 'GET', 'При открытии карточки задания'],
        ['GetSubstitutionsForTaskFromAssignment', 'Получение замещений из задачи', 'GET', 'При открытии услуги доработки'],
        ['SaveSubstitutionsToTask', 'Сохранение замещений в задачу', 'POST', 'В схеме потока услуги'],
        ['SaveSubstitutionsToTaskFromAssignment', 'Сохранение замещений по заданию', 'POST', 'В схеме потока услуги доработки'],
        ['GetDocumentTimeCollisions', 'Поиск документов с коллизией дат в Directum RX', '—', '—'],
        ['ValidateEmployeeVacations', 'Проверка отпусков в АСУП (SAP)', '—', '—'],
        ['GenerateStructForValidateEmployeeVacations', 'Формирование структуры для запроса в SAP', '—', '—'],
        ['SendMessageToExternalSystem', 'Отправка сообщения во внешнюю систему', '—', '—']
    ]
    
    for row_data in data_methods:
        row_cells = table_methods.add_row().cells
        for i, text in enumerate(row_data):
            row_cells[i].text = text

    # Section 8: Personal Account Service
    doc.add_heading('Личный кабинет', level=1)
    doc.add_heading('Услуга «Настройка замещений» (NNSubstitutionSetup)', level=2)
    doc.add_paragraph('Единый интерфейс для выбора замещающих сотрудников на период отпуска. Реализована поддержка назначения замещающих на несколько периодов, что актуально для услуг по переносу отпуска. Также сотрудник может дробить даты замещения на один период.')
    doc.add_paragraph('Идентификатор услуги: 2a5f2cfc-8646-4bb8-920c-a985b15da804')
    
    doc.add_heading('Таблица 8. Параметры услуги NNSubstitutionSetup', level=3)
    table_params = doc.add_table(rows=1, cols=4)
    table_params.style = 'Table Grid'
    hdr_cells_par = table_params.rows[0].cells
    par_headers = ['Параметр', 'Тип', 'Скрытый', 'Описание']
    for i, text in enumerate(par_headers):
        hdr_cells_par[i].text = text
        hdr_cells_par[i].paragraphs[0].runs[0].bold = True
        
    data_params = [
        ['InitiatorId', 'Число (Int64)', 'Нет', 'ИД инициатора (замещаемого)'],
        ['VacationStartDate', 'Дата', 'Нет', 'Дата начала отпуска'],
        ['VacationEndDate', 'Дата', 'Нет', 'Дата окончания отпуска'],
        ['VacationPeriodsJson', 'Строка', 'Да', 'JSON с периодами отпуска'],
        ['AssignmentId', 'Строка', 'Да', 'ИД задания'],
        ['CompleteAssignmentAfterSubmit', 'Булево', 'Да', 'Завершить задание после отправки'],
        ['SubstituteIdsString', 'Строка', 'Да', 'ID замещающих через «;»'],
        ['SystemIdsString', 'Строка', 'Да', 'ID систем через «;»'],
        ['StartDatesString', 'Строка', 'Да', 'Даты начала через «;»'],
        ['EndDatesString', 'Строка', 'Да', 'Даты окончания через «;»']
    ]
    
    for row_data in data_params:
        row_cells = table_params.add_row().cells
        for i, text in enumerate(row_data):
            row_cells[i].text = text
            
    doc.add_paragraph('Требуется описать логику передачи данных между услугами!!!', style='Heading 3')

    # Section 9: Client Validation
    doc.add_heading('Клиентский скрипт валидации validateSubstitutionDates.js', level=1)
    doc.add_paragraph('Предварительная проверка корректности введенных дат на стороне клиента без обращения к серверу.')
    doc.add_heading('Использование:', level=2)
    usage_list = [
        "в услуге «Настройка замещений» – при изменении дат в таблице;",
        "в услугах оформления отпуска – в обработчике перед отправкой (не давать указывать замещающих на даты, которые вне периода отпуска);",
        "в услугах доработки – в обработчике перед отправкой (не давать указывать замещающих на даты, которые вне периода отпуска)."
    ]
    for item in usage_list:
        doc.add_paragraph(item, style='List Bullet')

    # Section 10: Modified Services
    doc.add_heading('Доработанные услуги и карточки заданий', level=1)
    doc.add_paragraph('В таблице 9 представлен перечень доработанных услуг и карточек заданий.')
    
    doc.add_heading('Таблица 9. Услуги и карточки заданий', level=3)
    table_serv = doc.add_table(rows=1, cols=4)
    table_serv.style = 'Table Grid'
    hdr_cells_serv = table_serv.rows[0].cells
    serv_headers = ['№', 'Тип', 'Наименование', 'Файл']
    for i, text in enumerate(serv_headers):
        hdr_cells_serv[i].text = text
        hdr_cells_serv[i].paragraphs[0].runs[0].bold = True
        
    data_services = [
        ['1', 'Услуга', 'Оформление ежегодного отпуска', 'GetAnnualVacation.xml'],
        ['2', 'Услуга', 'Оформление прочего отпуска и оформление отпуска без сохранения з/п', 'GetOtherVacation.xml'],
        ['3', 'Услуга', 'Досрочное прерывание отпуска', 'NNSelfRecallVacationEmployee.xml'],
        ['4', 'Услуга', 'Перенос отпуска из заявления', 'NNShiftVacationByStatement.xml'],
        ['5', 'Услуга', 'Перенос отпуска из задания на подтверждение', 'ShiftAnnualVacationFromConfirmationAssignment.xml'],
        ['6', 'Услуга', 'Доработка отпуска', 'ReworkVacation.xml'],
        ['7', 'Услуга', 'Доработка ежегодного отпуска', 'ReworkAnnualVacation.xml'],
        ['8', 'Услуга', 'Доработка прочего отпуска и отпуска без сохранения з/п', 'ReworkOtherVacation.xml'],
        ['9', 'Услуга', 'Доработка досрочного прерывания', 'NNReworkSelfRecallVacationEmployee.xml'],
        ['10', 'Услуга', 'Доработка переноса из заявления', 'NNReworkShiftVacationByStatement.xml'],
        ['11', 'Карточка', 'Задание на подтверждение дат отпуска', 'INNVacationConfirmationAssignments'],
        ['12', 'Карточка', 'Задание на подписание согласия на отзыв', 'IHRDocFlowESignAssignments']
    ]
    
    for row_data in data_services:
        row_cells = table_serv.add_row().cells
        for i, text in enumerate(row_data):
            row_cells[i].text = text
            
    doc.add_paragraph('Требуется добавить описание что именно изменено в услугах/карточках!!!', style='Heading 3')

    # Section 11: No-code Function
    doc.add_heading('No-code. Функция вычисляемых выражений «Получить информацию о замещениях» (GetSubstitutionTextForEmployee)', level=1)
    doc.add_paragraph('Получение форматированного текста о замещающих для использования в инструкциях заданий, текстах уведомлений. Возвращаемое значение:')
    doc.add_paragraph('Иванов И.И. в КЭДО с 01.06.2024 по 14.06.2024\nПетров П.П. в КЭДО с 01.06.2024 по 14.06.2024', style='Quote')

    return doc

if __name__ == "__main__":
    doc = create_document()
    output_file = 'Описание_функциональности_Замещения.docx'
    doc.save(output_file)
    print(f"Document saved as {output_file}")
