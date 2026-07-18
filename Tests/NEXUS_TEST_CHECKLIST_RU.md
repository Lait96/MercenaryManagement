# Чеклист тестирования Mercenary Management перед публикацией на Nexus

Целевая версия: Bannerlord 1.4.7, Bannerlord.Harmony 2.4.2.225, Bannerlord.UIExtenderEx 2.13.2.

Все изменяющие состояние команды выполняйте только в отдельном тестовом сохранении. Не отмечайте пункт выполненным, если фактический результат отличается от ожидаемого хотя бы визуально.

## Обозначения и получение идентификаторов

Перед тестом откройте консоль Bannerlord и выполните:

```text
campaign.mm_help
campaign.mm_list_kingdoms
campaign.mm_list_mercenary_clans
```

### Визуальная проверка истории контрактов

Повторить во всех четырёх ролевых сохранениях: свободный клан, наёмник, вассал
и король.

1. Открыть `Clan` → `Наёмник`.
2. Убедиться, что кнопка `История контрактов` видна над блоком финансов и не
   перемещается при прокрутке описания действующего контракта.
3. Нажать кнопку и убедиться, что открывается отдельный экран истории. При
   отсутствии записей должна отображаться фраза о пустой истории.
4. Повторно нажать `История контрактов` и убедиться, что экран возвращается в
   исходное состояние.
5. В сохранении наёмника выполнить на копии сохранения:

   ```text
   campaign.mm_history_test <CLAN_ID> confirm
   ```

6. После `SUMMARY ... result=PASS` снова открыть вкладку и визуально проверить
   появившуюся строку: бывший работодатель, срок, причина, вклад, репутация,
   оценки сторон и отношения.
7. Не сохранять игру поверх исходной тестовой копии.

Сохраните значения:

- `<KINGDOM_ID>` — поле `id=` активного королевства из `campaign.mm_list_kingdoms`.
- `<WAR_KINGDOM_ID>` — `id=` активного королевства, которое сейчас участвует в войне. Наличие войны понадобится для контракта `until war ends`.
- `<CLAN_ID>` — поле `id=` свободного наёмного клана со `state=free` из `campaign.mm_list_mercenary_clans`.
- `<SECOND_CLAN_ID>` — `id=` другого свободного наёмного клана.
- `<MAX_RATE>` — верхнее число из `allowed_rate=MIN..MAX` в строке выбранного клана.
- `<PLAYER_CLAN_NAME>` — точное название клана игрока из экрана Clan. Если в названии есть пробелы, передавайте его в кавычках: `"Название клана"`.

Чтобы заранее подобрать клан с несколькими партиями, можно временно нанять кандидата и выполнить:

```text
campaign.mm_hire_clan <KINGDOM_ID> | <CLAN_ID>
campaign.mm_party_state <CLAN_ID>
campaign.mm_end_service <CLAN_ID>
```

Для основного `<CLAN_ID>` желательно выбрать клан с одной активной партией, для `<SECOND_CLAN_ID>` — с двумя или более. Если таких кланов нет, сравнение количества партий выполняется только через `mm_contribution_probe`.

---

## Часть A. Полностью автоматические проверки без игры

### A1. Запустить единый набор проверок

Из корня репозитория выполните в PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tests\run-all.ps1
```

Ожидаемый итог:

```text
Сборка успешно завершена.
Предупреждений: 0
Ошибок: 0
PASS: 16 deterministic business assertions.
ALL PRE-RUNTIME CHECKS PASSED.
```

Проверяется автоматически:

- Debug-компиляция;
- contribution и суточная нормализация на уровне клана;
- отсутствие пассивного contribution у Independent Operations;
- отсутствие преимущества от количества партий;
- satisfaction, employer satisfaction, reliability и performance;
- DepartureRisk и EmployerTerminationRisk, границы 0–100 и плохие контракты;
- отсутствие отдельного contribution input в termination risk;
- расчёт условий Offer Services;
- EN/RU XML и обязательные строки;
- prefab XML;
- saveable properties, типы, enum и контейнеры;
- статическая регистрация Harmony patches и target references;
- цепочка Offer Services;
- scrollbar, mouse wheel и clipping списка королевств;
- уменьшение ширины вкладок Clan Screen.

Признак ошибки: любое `FAIL`, ошибка сборки, warning, исключение PowerShell или отсутствие финальной строки `ALL PRE-RUNTIME CHECKS PASSED`.

Результат: [ ] пройдено.

---

## Часть B. Подготовка одной runtime-кампании

### B1. Создать чистую тестовую кампанию

1. Включите мод и заявленные версии зависимостей.
2. Создайте новую кампанию.
3. Дождитесь появления карты кампании.
4. Не используйте важное пользовательское сохранение.

Ожидается: кампания запускается без crash и повторяющихся исключений.

Признаки ошибки: crash при старте, зависший первый tick, красные Harmony/UIExtenderEx исключения.

Результат: [ ] пройдено.

### B2. Проверить регистрацию команд и Harmony

```text
campaign.mm_help
campaign.mm_harmony_status
```

Ожидается:

- выводится список `campaign.mm_*`;
- перечислены применённые методы с owners, начинающимися с `MercenaryManagement`;
- присутствуют патчи финансов наёмников, армейских весов, AI behavior scores и выбора Clan tab.

Признак ошибки: `No applied MercenaryManagement Harmony patches were found`, отсутствует критический target или команда неизвестна.

Результат: [ ] пройдено.

---

## Часть C. Offer Services для независимого игрока

Выполняйте эту часть до вступления клана игрока в королевство. У игрока не должно быть владений или действующего контракта.

### C1. Проверить обычные предложения

```text
campaign.mm_offers
```

Запишите `kingdom=` одного существующего предложения как `<OFFER_KINGDOM_ID>`. Если предложений нет, используйте любой допустимый `<KINGDOM_ID>`.

Откройте `Clan → Наёмник`.

Ожидается: существующие предложения отображаются с королевством, ставкой, длительностью и приоритетом.

Признаки ошибки: консоль показывает действующее предложение, но UI его не показывает; строки дублируются.

Результат: [ ] пройдено.

### C2. Проверить смысл и фильтрацию «Предложить услуги»

Если предложение `<OFFER_KINGDOM_ID>` уже существует, сначала удалите его:

```text
campaign.mm_offer_reject <OFFER_KINGDOM_ID>
```

Снова откройте или обновите `Clan → Наёмник`, нажмите «Предложить услуги».

Ожидается:

- в списке находятся только допустимые королевства без действующего предложения;
- королевство с действующим предложением не дублируется в этом списке;
- если предложения существуют от всех допустимых королевств, кнопка полностью скрыта;
- список прокручивается колесом мыши;
- справа остаётся выбранное королевство и кнопка отправки запроса.

Признаки ошибки: список повторяет обычные предложения, выходит за нижнюю границу, не реагирует на колесо или не имеет scrollbar при переполнении.

Результат: [ ] пройдено.

### C3. Создать, отклонить и принять запрошенное предложение

```text
campaign.mm_offer_services <OFFER_KINGDOM_ID>
campaign.mm_offers
campaign.mm_offer_reject <OFFER_KINGDOM_ID>
campaign.mm_offers
campaign.mm_offer_services <OFFER_KINGDOM_ID>
campaign.mm_offer_accept <OFFER_KINGDOM_ID>
campaign.mm_contract_state "<PLAYER_CLAN_NAME>"
```

Ожидается:

- первый запрос создаёт ровно одно сохранённое предложение;
- reject удаляет его;
- повторный запрос создаёт новое предложение;
- accept начинает контракт с показанными ставкой, длительностью и приоритетом;
- после принятия «Предложить услуги» недоступно.

Признаки ошибки: несколько предложений одного королевства, ставка меняется при принятии, контракт не создаётся или действие остаётся доступным.

Завершите контракт игрока перед продолжением:

```text
campaign.mm_contract_finish "<PLAYER_CLAN_NAME>"
```

Результат: [ ] пройдено.

---

## Часть D. Найм AI-кланов и состояние контракта

### D1. Нанять два тестовых клана

```text
campaign.mm_hire_clan <KINGDOM_ID> | <CLAN_ID>
campaign.mm_hire_clan <KINGDOM_ID> | <SECOND_CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_contract_state <SECOND_CLAN_ID>
```

Ожидается: оба клана состоят в королевстве; у каждого один активный контракт со ставкой в `allowed_rate`, нейтральными начальными оценками и конечным сроком около 60 дней.

Признаки ошибки: отсутствующий/двойной контракт, неверный employer, нулевая ставка, клан не вошёл в королевство.

Результат: [ ] пройдено.

### D2. Проверить вкладки Clan Screen

Откройте экран Clan на клане ниже максимального уровня.

Ожидается:

- вкладки «Члены», «Отряды», «Влад.», «Наёмник», «Другое» помещаются до шкалы опыта;
- «Другое» не перекрывает значение уровня или progress bar;
- выбор каждой вкладки открывает правильную панель.

Повторите при доступных UI scale/разрешениях, которые планируются к поддержке.

Признак ошибки: перекрытие шкалы, обрезанный текст, неправильная категория после клика.

Результат: [ ] пройдено.

---

## Часть E. Contribution и количество партий

### E1. Independent Operations не создаёт пассивный вклад

```text
campaign.mm_set_priority independent <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_contract_tick 3 <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_contribution_probe <CLAN_ID>
```

Запишите исходный и итоговый `contribution`.

Ожидается:

- три contract-only ticks обновляют оценки, но не создают contribution;
- `resulting_passive_points_now=0` для Independent Operations независимо от состояния партий.

Признак ошибки: contribution увеличился без подтверждённого события.

Результат: [ ] пройдено.

### E2. Подтверждённое событие учитывается ровно один раз

```text
campaign.mm_contribution_add 5 confirmed_test_event | <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
```

Ожидается: contribution увеличился ровно на 5, `last_activity=Diagnostic:confirmed_test_event`.

Признак ошибки: прибавлено другое количество или событие не записано.

Результат: [ ] пройдено.

### E3. Сравнить клан с одной и несколькими партиями

```text
campaign.mm_set_priority armies <CLAN_ID>
campaign.mm_set_priority armies <SECOND_CLAN_ID>
campaign.mm_party_state <CLAN_ID>
campaign.mm_party_state <SECOND_CLAN_ID>
campaign.mm_contribution_probe <CLAN_ID>
campaign.mm_contribution_probe <SECOND_CLAN_ID>
```

Ожидается:

- каждая партия выводится отдельной строкой с behavior, army, size ratio и food days;
- `eligible_parties` может отличаться;
- `resulting_passive_points_now` всегда 0 или 1, а не количество подходящих партий.

Для подтверждения реальной доставки `DailyTickPartyEvent` пропустите ровно один игровой день и повторите:

```text
campaign.mm_contract_state <CLAN_ID>
campaign.mm_contract_state <SECOND_CLAN_ID>
```

Ожидается: пассивный суточный прирост каждого подходящего клана не превышает 1.

Признак ошибки: прирост линейно зависит от количества партий.

Результат: [ ] пройдено.

---

## Часть F. Оценки и риски

### F1. Проверить нейтральный и успешный контракт

```text
campaign.mm_set_evaluation 50 50 50 | <CLAN_ID>
campaign.mm_contract_tick 1 <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_contribution_add 30 excellent_test_result | <CLAN_ID>
campaign.mm_contract_tick 3 <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
```

Ожидается:

- без результатов employer satisfaction постепенно снижается, а не обрушивается;
- подтверждённый результат улучшает performance, reliability и employer satisfaction;
- performance для 30 очков — «Отличная».

Признак ошибки: отличный вклад ухудшает employer satisfaction или reliability выходит за 0–100.

Результат: [ ] пройдено.

### F2. Проверить расшифровку рисков

```text
campaign.mm_risks <CLAN_ID>
campaign.mm_set_evaluation 5 5 10 | <CLAN_ID>
campaign.mm_set_rate <MAX_RATE> <CLAN_ID>
campaign.mm_risks <CLAN_ID>
```

Ожидается:

- выводятся итоговые `departure` и `employer_termination`;
- перечислены отдельные факторы satisfaction, reliability, rate, relation, losses, military need, priority и term;
- плохой дорогой контракт создаёт высокий риск;
- оба результата находятся в 0–100%;
- проценты во вкладке UI совпадают с консолью после обновления экрана.

Признаки ошибки: процент вне диапазона, contribution отдельно присутствует в termination formula, UI и консоль расходятся.

Результат: [ ] пройдено.

---

## Часть G. Детерминированные AI-решения

### G1. Увольнение работодателем

```text
campaign.mm_ai_evaluate dismissal preview <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_ai_evaluate dismissal apply <CLAN_ID>
campaign.mm_list_mercenary_clans
```

Ожидается: preview показывает `recommended=true` и не меняет контракт; apply завершает рекомендованный контракт; клан снова `state=free`.

Признаки ошибки: preview меняет состояние, apply расходится с напечатанным решением или клан остаётся в королевстве.

Результат: [ ] пройдено.

### G2. Уход наёмника при плохих условиях

```text
campaign.mm_hire_clan <KINGDOM_ID> | <CLAN_ID>
campaign.mm_set_start_days_ago 31 <CLAN_ID>
campaign.mm_set_evaluation 0 50 0 | <CLAN_ID>
campaign.mm_set_rate 1 <CLAN_ID>
campaign.mm_risks <CLAN_ID>
campaign.mm_ai_evaluate departure preview <CLAN_ID>
campaign.mm_ai_evaluate departure apply <CLAN_ID>
```

Команда ставки `1` должна быть отклонена, если она ниже разрешённого минимума; это отдельная проверка валидации. Высокий departure risk обеспечивают низкие satisfaction/reliability и возраст контракта.

Ожидается: preview не меняет состояние; при `recommended=true` apply завершает службу.

Признак ошибки: недопустимая ставка принимается, preview изменяет контракт или apply не соответствует решению.

Результат: [ ] пройдено.

### G3. Продление хорошего контракта

```text
campaign.mm_hire_clan <KINGDOM_ID> | <CLAN_ID>
campaign.mm_set_evaluation 90 90 90 | <CLAN_ID>
campaign.mm_ai_evaluate renewal preview <CLAN_ID>
campaign.mm_ai_evaluate renewal apply <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
```

Ожидается: `recommended=true`; preview не меняет end time; apply устанавливает срок около 60 дней от текущего момента.

Признак ошибки: плохие риски дают renewal либо apply не меняет срок рекомендованного контракта.

Результат: [ ] пройдено.

### G4. AI-оценка найма

Для другого свободного клана и королевства выполните:

```text
campaign.mm_ai_hire preview <KINGDOM_ID>
campaign.mm_ai_hire apply <KINGDOM_ID>
```

Ожидается: preview показывает military need, кандидата и решение; apply нанимает кандидата только при `recommended=true`.

Признак ошибки: preview меняет членство или apply нанимает при `recommended=false`.

Результат: [ ] пройдено.

---

## Часть H. Сроки и lifecycle контракта

### H1. Обычный срочный контракт

Убедитесь, что `<CLAN_ID>` снова нанят, затем:

```text
campaign.mm_contract_term 30 <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_set_contract_days 0 <CLAN_ID>
campaign.mm_contract_check normal <CLAN_ID>
campaign.mm_list_mercenary_clans
```

Ожидается: сначала длительность 30 дней; после установки нулевого остатка lifecycle check немедленно завершает службу.

Признак ошибки: истёкший контракт остаётся активным или меняется глобальное время.

Результат: [ ] пройдено.

### H2. Бессрочный контракт

```text
campaign.mm_hire_clan <KINGDOM_ID> | <CLAN_ID>
campaign.mm_contract_term indefinite <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_contract_check normal <CLAN_ID>
```

Ожидается: duration `Indefinite`, lifecycle check не завершает контракт.

Признак ошибки: появляется конечный end time или normal check завершает службу.

Результат: [ ] пройдено.

### H3. Until current war ends

Завершите предыдущий контракт, затем наймите клан в воюющее королевство:

```text
campaign.mm_contract_finish <CLAN_ID>
campaign.mm_hire_clan <WAR_KINGDOM_ID> | <CLAN_ID>
campaign.mm_contract_term war <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_contract_check normal <CLAN_ID>
campaign.mm_contract_check force_war_ended <CLAN_ID>
```

Ожидается:

- duration `UntilCurrentWarEnds` и сохранённый enemy id;
- normal check сохраняет контракт, пока записанная война активна;
- force check завершает только контракт, не меняя дипломатическое состояние войны.

Признак ошибки: normal check завершает активную войну/контракт или force check изменяет глобальную дипломатию.

Результат: [ ] пройдено.

---

## Часть I. Save/load и миграция

### I1. Сохранение и повторная загрузка с модом

Создайте активный контракт и предложение, затем снимите состояние:

```text
campaign.mm_hire_clan <KINGDOM_ID> | <CLAN_ID>
campaign.mm_contribution_add 7 save_test | <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_offer_reject <OFFER_KINGDOM_ID>
campaign.mm_offer_services <OFFER_KINGDOM_ID>
campaign.mm_offers
campaign.mm_harmony_status
```

1. Сохраните игру через обычное меню.
2. Выйдите в главное меню.
3. Загрузите сохранение.
4. Повторите:

```text
campaign.mm_contract_state <CLAN_ID>
campaign.mm_offers
campaign.mm_harmony_status
```

Ожидается: employer, ставка, duration, priority, evaluations, contribution, last activity и сохранённые предложения совпадают; Harmony patches снова применены.

Признаки ошибки: сброс полей, дубликаты, потеря participants, исчезновение предложения или патча.

Результат: [ ] пройдено.

### I2. Старое сохранение без мода

1. Подготовьте отдельное сохранение Bannerlord 1.4.7, которое никогда не загружалось с модом.
2. Включите мод и загрузите его.
3. Выполните:

```text
campaign.mm_list_mercenary_clans
campaign.mm_contract_state <CLAN_ID_ИЗ_СТАРОГО_СОХРАНЕНИЯ>
```

Ожидается: существующая нативная mercenary service получает нейтральный контракт; нет load error, нулевой ставки или немедленного необъяснимого ухода.

Признак ошибки: сохранение не загружается, ссылки clan/kingdom потеряны, значения вне диапазона.

Результат: [ ] пройдено.

---

## Часть J. Обязательные визуальные и lifecycle-проверки

Эти проверки нельзя честно заменить статическим тестом или консольной командой.

### J1. Права интерфейса по статусу игрока

Последовательно проверьте четыре состояния:

1. Независимый клан: `Clan → Наёмник` показывает офферы и Offer Services; Kingdom screen недоступен.
2. Наёмник: собственный контракт редактируется только разрешёнными действиями; Kingdom → Mercenaries read-only, собственный клан выделен.
3. Вассал: обе панели информационные, расходование средств и управление наёмниками недоступны.
4. Правитель: Kingdom → Mercenaries полностью управляем; личный Offer Services недоступен.

Признаки ошибки: действие доступно неверной роли, неправильный клан выделен, данные не обновились после смены статуса.

Результат: [ ] пройдено.

### J2. Gauntlet/UIExtenderEx

Проверьте:

- вкладки Clan Screen не перекрывают шкалу уровня;
- вкладка «Наёмник» корректно выбирается и закрывается;
- список обычных офферов прокручивается;
- список Offer Services прокручивается колесом и scrollbar;
- выбранное королевство и кнопки остаются видимыми;
- нет обрезания на поддерживаемых разрешениях/UI scale;
- русские и английские строки помещаются в элементы.

Результат: [ ] пройдено.

### J3. Реальные Harmony/AI события

1. Включите `campaign.mm_ai_diagnostics_start <CLAN_ID>`.
2. Пропустите несколько игровых часов и один день.
3. Выполните:

```text
campaign.mm_ai_diagnostics_report
campaign.mm_contribution_probe <CLAN_ID>
campaign.mm_contract_state <CLAN_ID>
campaign.mm_ai_diagnostics_stop
```

Ожидается: diagnostics содержат реальные native/final AI weights; игра не падает; passive contribution не превышает один за клан-день.

Признак ошибки: повторяющиеся исключения, пустой report при активных партиях, IL/transpiler crash, линейный вклад по партиям.

Результат: [ ] пройдено.

### J4. Реальная война и армия

Проверьте без `force_war_ended`:

- war-bound контракт сохраняется во время записанной войны;
- завершается после реального мира именно с записанным противником;
- окончание другой войны его не завершает;
- party join/leave army не происходит в бою, осаде, при критической еде или низкой силе;
- cooldown предотвращает немедленное повторное вступление.

Результат: [ ] пройдено.

### J5. Финальная стабильность Bannerlord 1.4.7

1. Загрузите тестовое сохранение с несколькими активными контрактами.
2. Пропустите минимум семь игровых дней.
3. Несколько раз откройте Clan и Kingdom screens.
4. Выполните найм, смену ставки/приоритета, продление и увольнение через UI.
5. Сохраните и повторно загрузите игру.

Ожидается: нет crash, повторяющихся exceptions, зависания tick, дублированных контрактов или некорректного членства.

Результат: [ ] пройдено.

---

## Критерий допуска к публикации

Мод готов к Nexus только если:

- A1 завершён строкой `ALL PRE-RUNTIME CHECKS PASSED`;
- все пункты B–J отмечены как пройденные;
- Debug-сборка содержит 0 warnings и 0 errors;
- не осталось воспроизводимых crash, save corruption, role-permission ошибок или критических UI-перекрытий;
- найденные некритичные замечания отдельно записаны и не заявлены в описании мода как готовый функционал.
