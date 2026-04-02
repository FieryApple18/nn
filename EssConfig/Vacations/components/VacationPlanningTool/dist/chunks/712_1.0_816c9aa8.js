(self.webpackChunkvacation_planning_tool=self.webpackChunkvacation_planning_tool||[]).push([[712],{5135:(e,t,i)=>{"use strict";i.r(t),i.d(t,{default:()=>ri});var o={};i.r(o),i.d(o,{default:()=>oi});var n={};i.r(n),i.d(n,{default:()=>si});var a=i(71468),s=i(5338),r=i(25699),l=i(27656),c=i.n(l),d=i(32107),m=i(17508);const u="Vacations",h="EssPlatform",g="ru",p="year",f="month",y=60,v="planningToolContainer",b={en:{translation:{employee:"Employee",vacationKind:"Vacation Kind",startDate:"Start date",endDate:"End date",duration:"Duration",overlaps:"Overlaps",approve:"Approve",onRework:"On rework",month:"Month",year:"Year",unscheduled:"Not scheduled",shorterDays:"d.",tooltipDateFormat:"DD.MM.YYYY"}},ru:{translation:{createVacation:"Создать отпуск",updateVacation:"Обновить отпуск",employee:"Сотрудник",vacationKind:"Вид отпуска",startDate:"Дата начала",endDate:"Дата окончания",duration:"Продолжительность",overlaps:"Пересечения",approve:"Согласовать",onRework:"На доработку",month:"Месяц",year:"Год",unscheduled:"Не запланировано",shorterDays:"дн.",tooltipDateFormat:"DD.MM.YYYY"}}};c().use(d.A).use(m.A).use(r.r9).init({resources:b,lng:g,fallbackLng:g,debug:!0,interpolation:{escapeValue:!1}});i(62796);var E=i(38267);const C=E.I4.div`${({theme:e})=>E.AH`
  position: absolute;
  inset: 0;
  width: 100%;
  font-family: ${e.font.family};
  font-size: 14px;
  color: ${e.color};
`}`,x=a.wA.withTypes(),S=a.d4.withTypes();var A=i(57115),T=i.n(A),j=i(99621);const k="full-name",w="change-color",D="need-highlight",z="group-highlight",I="unscheduled",$="hide-month";var L=i(74353),M=i.n(L),V=i(20181),F=i.n(V);const H=({className:e,functions:t,timelineRef:i,onTimePeriodChange:o,isSetYear:n,onClickHeader:a})=>{const s=(0,A.useRef)(new Date),r=(0,A.useRef)(n),l=S((e=>e.vacations)),{initialDrawComplete:c}=(0,A.useContext)(Ut),{employeeStruct:d,vacations:m}=S((e=>e.timelineData));(0,A.useEffect)((()=>{r.current=n}),[n]),(0,A.useEffect)((()=>{const e=i.current;if(e){e.timeline.setOptions(t.getOptions());const i=async()=>{try{const i=await t.getOptionsAsync();e.timeline.setOptions(i),e.timeline.redraw()}catch(e){console.log(e)}};i()}}),[t]),(0,A.useEffect)((()=>{const e=l.value;return(()=>{const o=i.current;if(o){o.groups.clear(),o.items.clear();const i=e=>JSON.parse(JSON.stringify(e));d.forEach((e=>o.groups.add(i(e)))),m.forEach((e=>o.items.add(i(e)))),e.length>0&&t.addVacationsFromContext(o,e),o.forceUpdate()}t.style.highlightingSubscribe()})(),()=>{t.style.highlightingUnsubscribe()}}),[l,d,m]);const u=(0,A.useCallback)((e=>{if(e.byUser){const i=e.start>s.current;s.current=e.start;const n=M()(e.start),a=n.daysInMonth()-t.adapter.theme.DAY_NUMBER_FOR_SCROLL;let l="";r.current?i&&n.month()>t.adapter.theme.MONTH_NUMBER_FOR_SCROLL?l=(n.year()+1).toString():!i&&n.month()<t.adapter.theme.MONTH_NUMBER_FOR_SCROLL+1&&(l=n.year().toString()):i&&n.date()>a?l=t.getTimePeriodFormat(n.add(1,"month")):!i&&n.date()<a&&(l=t.getTimePeriodFormat(n)),""!==l&&o(l)}}),[]),h=(0,A.useCallback)((e=>{if(r.current&&"I"!==e.event.target.nodeName&&"IMG"!==e.event.target.nodeName){const i=new Set(["vis-top","vis-minor"]);[...e.event.target.classList,...e.event.target.offsetParent.classList].some((e=>i.has(e)))&&a(t.getTimePeriodFormat(e.time))}}),[]),g=(0,A.useCallback)((()=>{const e=i.current;if(e){const i=e.timeline.getWindow(),o=t.getViewPeriods(n,i.start);e.timeline.setOptions(o)}}),[n,t.getViewPeriods]);(0,A.useEffect)((()=>{g()}),[g]);const p=(0,A.useCallback)(F()((()=>{if(t.isUserScrolling){document.querySelectorAll(".vis-text.vis-day1").forEach((e=>e.classList.add($))),t.isUserScrolling=!1}}),1e3),[t.isUserScrolling]),f=(0,A.useCallback)((e=>{e.byUser&&!t.isUserScrolling&&(t.isUserScrolling=!0)}),[t.isUserScrolling]);(0,A.useEffect)((()=>{const e=i.current;if(e)return e.timeline.on("rangechange",u),e.timeline.on("click",h),e.timeline.on("rangechanged",p),e.timeline.on("rangechange",f),addEventListener("resize",g),()=>{e.timeline.off("rangechange",u),e.timeline.off("click",h),e.timeline.off("rangechanged",p),e.timeline.off("rangechange",f),removeEventListener("resize",g)}}),[h,u,p,f,g]),(0,A.useEffect)((()=>{if(i.current?.timeline&&e){const t=i.current?.timeline;t.body.dom.container.classList.value=e}}),[e]);const y=(0,A.useCallback)((()=>{const e=()=>{const t=i.current?.groups?.get();if(!t?.length)return void requestAnimationFrame(e);const o=t[t.length-1].id;document.querySelector(`.id-${o}`)?c():requestAnimationFrame(e)};requestAnimationFrame(e)}),[c]),v=(0,A.useMemo)((()=>({onInitialDrawComplete:y})),[y]);return T().createElement(j.A,{ref:i,options:v})},O=(0,A.memo)(H);var N=i(95093),B=i.n(N);const R=B().months().map((e=>`.vis-${e.toLowerCase()}`)),_=(0,E.Ay)(O)`${({theme:e,isSetYear:t})=>E.AH`
  height: calc(100% - (${e.size.toggleButtonHeight} + 2 * ${e.size.toggleButtonMargin} + ${e.size.overlapsHeight}px) + ${e.size.monthNavigatorMarginBottom});
  ${((e,t)=>E.AH`
  ${t&&E.AH`
    .vis-text.vis-minor.vis-january {
      z-index: 1;
    }
  `}

  /* Размеры шапки */
  .vis-panel.vis-top {
    height: ${e.size.headerFullHeight}px;
  }

  /* Размер дней в шапке */
  .vis-text.vis-minor {
    padding: 0;
    flex-direction: column;
    display: flex;
    justify-content: center;
    font-size: 13px;
    height: ${e.size.headerFullHeight}px;
    text-align: center;
    line-height: 18px;

    // Стиль для ячейки с месяцем внутри шапки
    p {
      display: flex;
      flex-direction: column;
      justify-content: center;
      height: ${e.size.columnHeight}px;
    }

    // Стиль для ячейки с количеством сотрудников в отпуске внутри шапки
    i {
      box-shadow: inset 0 1px 0 0 ${e.timeLine.borderColor};
      color: ${e.timeLine.overlapsTxtColor};
      font-style: normal;
      font-size: ${e.font.primarySize};
      height: ${e.size.overlapsHeight}px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
  }

  /* Подсветка праздничных дней */
  /* Считаем, что если у даты есть дочерний span, то это праздничный день */
  .vis-text.vis-minor > span {
    color: ${e.timeLine.holidayColor};
  }

  // Подсветка ячеек с месяцами в шапке в преставлении год (подсвечивем только ячейку с датой)
  ${R.map(((e,t)=>11===t?`.vis-text${e} p:hover`:`.vis-text${e} p:hover, `))} {
    ${t?E.AH`
        color: ${e.timeLine.topColor};
        box-sizing: border-box;
        background-color: ${e.timeLine.groupBackgroundColor};
        cursor: pointer;
      `:""}
  }

  // Стили для первого дня месяца
  .vis-text.vis-minor.vis-day1 {
    overflow: visible;
    z-index: 1;
    padding-left: 0;
    padding-right: 0;
    
    // Прячем месяц
    &.${$} > div {
      opacity: 0;
    }

    // Стили для месяца
    & > div {
      // 3 px за счет left: 2px - это рамка между месяцами и 1px это рамка справа от месяца
      top: 0;
      width: calc(200% - 3px);
      height: ${e.size.columnHeight}px;
      position: absolute;
      display: flex;
      align-items: center;
      justify-content: center;
      left: 2px;
      background-color: ${e.timeLine.baseBodyBackgroundColor};
      font-size: 15px;
      transition: opacity 250ms ease-out;
    }
  }
`)(e,t)}
  ${(e=>E.AH`
  // Скролл справа в левой панели сотрудников
  .vis-left.vis-panel.vis-vertical-scroll {
    direction: ltr;
  }

  // Ширина групп в панели слева
  .vis-panel.vis-left {
    width: ${e.size.groupWidth}px;
  }
  
  /* Размер групп */
  .vis-label .vis-inner {
    height: ${e.size.columnHeight}px;
  }

  /* Подсветка групп */
  .${z} {
    box-sizing: border-box;
    background-color: ${e.timeLine.groupBackgroundColor};
  }

  // Установка текста незапланировано для сотрудников без отпусков
  .vis-foreground .${I} {
    // ts-styled-plugin ругается, когда после подстановки стоит ":".
    // Поэтому after выделен в отдельный селектор через конкатенацию.
    &::after {
      line-height: ${e.size.columnHeight}px;
      padding-left: 10px;
      content: "${(0,l.t)("unscheduled")}";
      display: inline-block;
      font-style: ${e.font.style};
      color: ${e.timeLine.notPlannedTxtColor};
    }
  }
`)(e)}
  ${((e,t)=>E.AH`
  // Устанавливаем отпуска выше таймлайна
  .vis-content {
    z-index: 2;
  }

  .vis-item.vis-range {
    // Рамка и цвет "колбаски" отпуска
    border-color: ${e.timeLine.periodBorderColor};
    background-color: ${e.timeLine.periodBackgroundColor};
    border-radius: 3px;
    cursor: pointer;
    color: ${e.timeLine.textColor};

    /* Размер элементов отпусков в таймлайне */
    height: 24px;
    
    // Расстояние между началом строки и отпуском
    // vis-timeline устанавливает его как top
    top: 6px !important;

    // Подсветка при наведении на отпуск
    &:hover {
      box-shadow: 0 0 0 1px ${e.timeLine.periodBoxShadow};
    }
  }

  && .vis-item-overflow {
    overflow: visible;
  }

  && .vis-item-content {
    // Стили для ItemTemplate
    padding: 0;
    width: 100%;
    transform: translateX(0px) !important;
    // Установка курсора для отпусков
    cursor: pointer;
    
    ${t&&E.AH`
      padding-left: 0;
    `}
    
    label {
      cursor: inherit;
    }
  }
`)(e,t)}
  ${((e,t)=>E.AH`
  // Убрать лишние рамки, которые появляются из-за "overflow: unset" у ".vis-timeline"
  .vis-timeline.vis-bottom.vis-ltr {
    border-bottom: none;
    border-right: none;
  }

  // Убрать место для скролла справа от таймлайна.
  .vis-right.vis-panel.vis-vertical-scroll {
    display: none;
  }
// Для выхода подсказки (tooltip) за таймлайн
  .vis-timeline {
    overflow: unset;
  }

  .vis-time-axis.vis-background .vis-january:first-child {
    border-color: ${e.timeLine.disabledBorderColor};
    border-width: 1px;
  }

  /* Подсветка начала месяца */
  .vis-grid.vis-vertical {
    &.vis-day1 {
      border-color: ${e.timeLine.beginMonthBorderColor}; 
      border-width: 2px;
      z-index: 1;
    }

    ${t&&E.AH`
      &.vis-january {
        border-color: ${e.timeLine.beginMonthBorderColor};
        border-width: 2px;
        z-index: 1;
      }
    `}
  }
  
  // Цвет текста
  .vis-labelset .vis-label{
    color: ${e.timeLine.textColor};
  }
  .vis-time-axis .vis-text {
    color: ${e.timeLine.textColor};
  }

  // Цвет границ
  .vis-timeline {
    border-color: ${e.timeLine.borderColor};
  }
  .vis-grid.vis-vertical {
    border-color: ${e.timeLine.borderColor};
  }
  .vis-foreground .vis-group {
    border-color: ${e.timeLine.borderColor};
  }
  .vis-panel.vis-center,
  .vis-panel.vis-left,
  .vis-panel.vis-right,
  .vis-panel.vis-top,
  .vis-panel.vis-bottom {
    border-color: ${e.timeLine.borderColor};
  }
  .vis-labelset .vis-label {
    border-color: ${e.timeLine.borderColor};
  }
`)(e,t)}
  ${(e=>E.AH`
  .vis-tooltip {
    position: absolute;
    box-shadow: 0px 2px 8px 0px ${e.timeLine.boxShadow};
    background-color: ${e.timeLine.backgroundColor};
    color: ${e.timeLine.textColor};;
    border: none;
    font-family: ${e.font.family};
    font-size: 14px;
    padding: 10px;
    border-radius: 5px;
    line-height: 18px;
    p {
      margin: 0;
      margin-top: 4px;
    }

    p:first-child {
      margin-top: 0;
    }
  }
`)(e)}
  ${E.AH`
  .vis-text.vis-minor {
    p {
      margin-bottom: 0px;
    }
  }
`}
`}`;var W=i(90563);const Y=E.DU`${({theme:e,dialogId:t,isLoading:i})=>E.AH`
  // Стили для кнопок в шапке.
  .${W.A.paper}.${W.A.paper} {
    color: ${e.button.textColor};
    background-color: ${e.timeLine.baseBodyBackgroundColor};
  }
  // Стили таймлайна для ЛК, чтобы он занимал 100% свободного места за вычетом высоты компонента с кнопками,
  // для того чтобы управление было только у самого контрола и не передавалось диалогу.
  div[class*=${t}] > form {
    position: relative;
    height: 100%;
    > * {
      margin: 0 !important;
    }
    #${v} {
      position: absolute;
      inset: 0;
      ${i&&E.AH`overflow: hidden;`}
      ${C} {
        height: 100%;
      }
      ${_} {
        height: calc(100% - (${e.size.toggleButtonHeight} + 2 * ${e.size.toggleButtonMargin} + ${e.size.monthNavigatorMarginBottom}));
      }
    }
  }
  `}`,P=E.Ay.div`${({theme:e})=>E.AH`
  position: absolute;
  inset: 0;
  z-index: 3;
  background: ${e.timeLine.baseBodyBackgroundColor};
  outline: 4px solid ${e.timeLine.baseBodyBackgroundColor};
  display: flex;
  justify-content: center;
  align-items: center;
`}`;var K=i(78717);const U=()=>T().createElement(P,null,T().createElement(K.A,null));var q=i(47994),G=i(72363),X=i(20038);const Z={employeeStruct:[],vacations:[]},J=(0,X.Z0)({name:"timelineData",initialState:Z,reducers:{setEmployeeStruct:(e,t)=>{e.employeeStruct=t.payload},setVacations:(e,t)=>{e.vacations=t.payload},setTimelineData:(e,t)=>{const i=e;i.vacations=t.payload.vacations,i.employeeStruct=t.payload.employeeStruct}}}),{setVacations:Q,setEmployeeStruct:ee,setTimelineData:te}=J.actions,ie=J.reducer;let oe=function(e){return e.ESSVacationEmployeeSchedulingAssignments="f178357e-6bfe-42d7-8a55-7013389de185",e.RXVacationEmployeeSchedulingAssignments="f03a4dcd-b007-43e3-9c67-71b771c1d392",e.ESSVacationApprovalTasks="d1899307-092b-4a08-87b5-3c7c14f2feff",e.RXVacationApprovalTasks="096abbce-40bd-4f02-a3f8-9db679b6682c",e.ESSVacationApprovalAssignments="fb83fd0b-2a8d-43b6-8c0f-c29849fb8fc0",e.RXVacationApprovalAssignments="6cefd82a-6bf6-4966-b883-eaeaff8b3ed6",e.ESSVacationReworkAssignments="82835b9b-0070-4df6-b5de-2a7a5e99677f",e.RXVacationReworkAssignments="34e36333-d4dd-492f-b275-c8441a615dbc",e}({}),ne=function(e){return e.RX="RX",e.LK="LK",e}({});class ae{constructor(e){this.adapter=e}getEmployeesInfo=async(e,t,i=null)=>this.adapter.get(`GetEmployeesInfo?context='${e}'&employeeId=${t}&assignmentId=${i}`,u);getVacationKinds=async e=>this.adapter.get(`GetAvailableVacationKinds?employeeId=${e}`,u);getVacationEndDate=async(e,t,i,o)=>this.adapter.get(`GetVacationEndDate?dateBegin=${e.format("YYYY-MM-DD")}&duration=${t}&userId=${i}&vacationKindId=${o}`,u);getPeriods=async(e,t=null)=>this.adapter.get(`GetAvailablePeriods?context='${e}'&assignmentId=${t}`,u);getHolidaysAndWeekendsInPeriod=async(e,t,i)=>this.adapter.get(`GetHolidaysAndWeekendsInPeriod?periodBegin=${e.toJSON()}&periodEnd=${t.toJSON()}&recipientId=${i||null}`,u);getTimeZone=async()=>this.adapter.get("GetTimeZone",u);getAllVacationKinds=async()=>this.adapter.get("GetVacationKinds","");getInitiatorId=async e=>this.adapter.get(`GetInitiatorId?userId=${e}`,h)}class se{constructor(e){this.adapter=e}highlightingSubscribe=()=>{this.getGroupElements().forEach((e=>{e.addEventListener("mouseover",this.groupOverHandler),e.addEventListener("mouseout",this.groupOutHandler),e.addEventListener("touchstart",this.groupOverHandler),e.addEventListener("touchend",this.groupOutHandler)}))};highlightingUnsubscribe=()=>{this.getGroupElements().forEach((e=>{e.removeEventListener("mouseover",this.groupOverHandler),e.removeEventListener("mouseout",this.groupOutHandler),e.removeEventListener("touchstart",this.groupOverHandler),e.removeEventListener("touchend",this.groupOutHandler)}))};groupOverHandler=e=>{const t=z,i=e.currentTarget,o=i?.classList,n=Array.from(o).find((e=>e.startsWith("id-"))),a=document.querySelectorAll(`.${D}.${n}`);Array.from(a).forEach((e=>{e.classList.add(t),e.querySelector(`.${k}`)?.classList.add(w)})),o.add(t)};groupOutHandler=e=>{const t=z,i=e.currentTarget,o=i?.classList,n=Array.from(o).find((e=>e.startsWith("id-"))),a=document.querySelectorAll(`.${D}.${n}`);Array.from(a).forEach((e=>{e.classList.remove(t),e.querySelector(`.${k}`)?.classList.remove(w)})),o.remove(t)};getGroupElements=()=>{const e=new Array,t=document.getElementsByClassName("vis-group-level-0");return Object.entries(t).forEach((t=>{e.push(t[1]);const i=t[1].classList[t[1].classList.length-1],o=document.getElementsByClassName(`vis-group ${i}`);Object.entries(o).forEach((t=>{t[1]["vis-group"]&&e.push(t[1])}))})),e}}const re=({viewDate:e,scale:t,functions:i})=>{const{countEmployeesOnVacation:o,holidays:n,isUserScrolling:a,getFormattedMonth:s}=i,r=S((e=>e.vacations)),{vacations:l}=S((e=>e.timelineData)),c=(0,A.useMemo)((()=>[...r.value,...l]),[r.value,l]),d=(0,A.useCallback)(((e,t)=>{let i=T().createElement(T().Fragment,null,T().createElement("p",null,e.format("dd").toUpperCase(),T().createElement("br",null),e.format("D")),T().createElement("i",null,t>1?t:""));if(n.has(e.format("YYYY-MM-DD"))&&(i=T().createElement("span",null,i)),1===e.date()&&a){let t=s(e);t=t.replace(".",""),i=T().createElement(T().Fragment,null,i,T().createElement("div",null,`${t} ${e.format("YYYY")}`))}return i}),[n,a,s]),m=(0,A.useCallback)(((e,t)=>T().createElement(T().Fragment,null,T().createElement("p",null,s(e),T().createElement("br",null),e.format("YYYY")),T().createElement("i",null,1===t?T().createElement("img",{src:"data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTciIGhlaWdodD0iMTYiIHZpZXdCb3g9IjAgMCAxNyAxNiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTguMjA4MDEgMTRDMTEuNTIxNyAxNCAxNC4yMDggMTEuMzEzNyAxNC4yMDggOEMxNC4yMDggNC42ODYyOSAxMS41MjE3IDIgOC4yMDgwMSAyQzQuODk0MyAyIDIuMjA4MDEgNC42ODYyOSAyLjIwODAxIDhDMi4yMDgwMSAxMS4zMTM3IDQuODk0MyAxNCA4LjIwODAxIDE0WiIgc3Ryb2tlPSIjODk4OTg5IiBzdHJva2UtbWl0ZXJsaW1pdD0iMTAiLz4KPHBhdGggZD0iTTguMjA4MDEgNVY4LjUiIHN0cm9rZT0iIzg5ODk4OSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIi8+CjxwYXRoIGQ9Ik04LjIwODAxIDExLjVDOC42MjIyMiAxMS41IDguOTU4MDEgMTEuMTY0MiA4Ljk1ODAxIDEwLjc1QzguOTU4MDEgMTAuMzM1OCA4LjYyMjIyIDEwIDguMjA4MDEgMTBDNy43OTM3OSAxMCA3LjQ1ODAxIDEwLjMzNTggNy40NTgwMSAxMC43NUM3LjQ1ODAxIDExLjE2NDIgNy43OTM3OSAxMS41IDguMjA4MDEgMTEuNVoiIGZpbGw9IiM4OTg5ODkiLz4KPC9zdmc+"}):""))),[s]);return(0,A.useMemo)((()=>{const i=B()(e);let n;if(0==c.length)return"day"===t&&(n=d(i,0)),"month"===t&&(n=m(i,0)),n;const a=o(i,c,t);return"day"===t&&(n=d(i,a)),"month"===t&&(n=m(i,a)),n}),[e,t,d,m,o,c])},le=(0,A.memo)(re);var ce=i(71783);const de=E.I4.div`
  display: flex;
  justify-content: space-between;
  padding-left: 5px;
  height: 100%;
  align-items: center;
`,me=E.I4.div`${({theme:e})=>E.AH`
  padding-left: 5px;
  padding-right: 5px;
  display: flex;
  flex: 1;
  flex-direction: column;
  line-height: 18px;
  width: ${e.size.maxTextWidth};
`}`,ue=(0,E.I4)(ce.A)`
  && {
    height: 36px;
    width: 36px;
    background-color: transparent;
  }
`,he=E.I4.label`
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  max-width: 100%;
`,ge=(0,E.I4)(he).attrs({className:k})`${({theme:e})=>E.AH`
  &.${w} {
    color: ${e.employee.fullNameColor};
  }
  font-size: ${e.font.primarySize};
  font-weight: ${e.font.mediumWeight};
  line-height: ${e.font.primaryLineHeight};
`}`,pe=(0,E.I4)(he)`${({theme:e})=>E.AH`
  font-size: ${e.font.primarySize};
  color: ${e.employee.jobTitleColor};
  font-weight: ${e.font.regularWeight};
`}`;var fe,ye,ve;function be(){return be=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var i=arguments[t];for(var o in i)({}).hasOwnProperty.call(i,o)&&(e[o]=i[o])}return e},be.apply(null,arguments)}var Ee=function(e){return A.createElement("svg",be({xmlns:"http://www.w3.org/2000/svg",width:32,height:32,fill:"none"},e),fe||(fe=A.createElement("rect",{width:32,height:32,fill:"#BCCEE6",rx:16})),ye||(ye=A.createElement("path",{stroke:"#151414",strokeMiterlimit:10,d:"M16 18a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z"})),ve||(ve=A.createElement("path",{stroke:"#151414",strokeLinecap:"round",strokeLinejoin:"round",d:"M9.938 21.5a7 7 0 0 1 12.125 0"})))};const Ce=({employee:e})=>{const t=S((e=>e.isMobile));return T().createElement(de,null,T().createElement(ue,null,T().createElement(Ee,null)),T().createElement(me,null,T().createElement(ge,null,t.value?e.ShortName:e.Name),T().createElement(pe,null,e.JobTitle)))},xe=(0,A.memo)(Ce),Se=E.I4.div`
  padding-top: 2px;
  line-height: 16px;
`,Ae=E.I4.p`${({theme:e})=>E.AH`
  font-size: ${e.font.seconadrySize};
  margin-top: 5px;
  margin-left: -1px;
`}`,Te=E.I4.p`${({theme:e})=>E.AH`
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  max-width: 100%;
  font-size: ${e.font.secondarySize};
  margin-left: 4px;
  margin-top: 0;
  margin-bottom: 0;
`}`;var je=i(18492);const ke=({vacation:e})=>{const{t}=(0,je.B)(),i=S((e=>e.isSetYear));return T().createElement(Se,null,T().createElement(Te,null,i.value?" ":e.VacationKind),T().createElement(Ae,null,`${e.Duration} ${t("shorterDays")}`))},we=(0,A.memo)(ke),De=({adapter:e,children:t})=>{const i=S((e=>e.isDarkTheme)).value,o=(0,A.useMemo)((()=>(e.fillTheme(),{...e.theme})),[i,e]);return T().createElement(E.NP,{theme:o},t)},ze=(0,A.memo)(De),Ie=(0,X.Z0)({name:"context",initialState:{value:null},reducers:{setContext:(e,t)=>{e.value=t.payload}}}),{setContext:$e}=Ie.actions,Le=Ie.reducer,Me=(0,X.Z0)({name:"isDarkTheme",initialState:{value:!1},reducers:{setIsDarkTheme:(e,t)=>{e.value=t.payload}}}),{setIsDarkTheme:Ve}=Me.actions,Fe=Me.reducer,He=(0,X.Z0)({name:"isMobile",initialState:{value:!1},reducers:{setIsMobile:(e,t)=>{e.value=t.payload}}}),{setIsMobile:Oe}=He.actions,Ne=He.reducer,Be=(0,X.Z0)({name:"isSetYear",initialState:{value:!0},reducers:{setValue:(e,t)=>{e.value=t.payload}}}),{setValue:Re}=Be.actions,_e=Be.reducer,We=(0,X.Z0)({name:"vacations",initialState:{value:[]},reducers:{setVacations:(e,t)=>{e.value=t.payload}}}),{setVacations:Ye}=We.actions,Pe=We.reducer,Ke=(0,X.U1)({reducer:{isSetYear:_e,vacations:Pe,context:Le,isDarkTheme:Fe,isMobile:Ne,timelineData:ie}}),Ue=({adapter:e,children:t})=>T().createElement(a.Kq,{store:Ke},T().createElement(ze,{adapter:e},t)),qe=(0,A.memo)(Ue);var Ge=i(65848);const Xe=({vacation:e})=>{const{t}=(0,je.B)(),i=t("tooltipDateFormat");return S((e=>e.isMobile)).value?null:T().createElement(T().Fragment,null,T().createElement("p",null,e.VacationKind),T().createElement("p",null,`${B()(e.start).format(i)}-${B()(e.end).format(i)}`),T().createElement("p",null,`${e.Duration} ${t("shorterDays")}`))},Ze=(0,A.memo)(Xe);const Je=class{constructor(e){this.adapter=e,this.request=new ae(e),this.style=new se(e),this.holidays=new Set,this.isUserScrolling=!1,this.serverTimezone=0}getPeriods=async e=>{let t;const i=this.getCallContext();[oe.ESSVacationEmployeeSchedulingAssignments,oe.RXVacationEmployeeSchedulingAssignments,oe.ESSVacationApprovalAssignments,oe.RXVacationApprovalAssignments,oe.ESSVacationReworkAssignments,oe.RXVacationReworkAssignments].includes(i)&&(t=e);const o=await this.request.getPeriods(i,t);return o.Start=this.convertToTimezone(o.Start),o.End=this.convertToTimezone(o.End),o};getOptions=()=>{const e={showCurrentTime:!1,width:"100%",maxHeight:"100%",groupHeightMode:"fixed",stack:!1,orientation:{axis:"top",item:"top"},autoResize:!0,zoomable:!1,verticalScroll:!0,horizontalScroll:!0,showMajorLabels:!1};return e.locale=this.adapter.language,e.template=this.itemTemplate,e.groupTemplate=this.groupTemplate,e.format={minorLabels:this.dateTemplate},e.tooltip={overflowMethod:"none",template:this.tooltipTemplate},e};getOptionsAsync=async()=>{const e={},t=await this.getPeriods(this.adapter.id);e.min=t.Start,e.max=t.End;const i=new Date(t.Start),o=this.getViewPeriods(!0,i);return e.start=o.start,e.end=o.end,e};itemTemplate=(e,t)=>{if(t&&e){const i="data-is-rendered",o=t.hasAttribute(i),n=T().createElement(qe,{adapter:this.adapter},T().createElement(we,{vacation:e}));if(o)t.timelineItemRoot.render(n);else{t.setAttribute(i,"true");const e=(0,s.createRoot)(t);t.timelineItemRoot=e,e.render(n)}}return""};groupTemplate=(e,t)=>{if(t&&e){const i="data-is-rendered",o=t.hasAttribute(i),n=T().createElement(qe,{adapter:this.adapter},T().createElement(xe,{employee:e}));if(o)t.timelineGroupRoot.render(n);else{t.setAttribute(i,"true");const e=(0,s.createRoot)(t);t.timelineGroupRoot=e,e.render(n)}}return""};dateTemplate=(e,t)=>Ge.renderToString(T().createElement(a.Kq,{store:Ke},T().createElement(le,{viewDate:e,scale:t,functions:this})));tooltipTemplate=e=>{const t=e;return Ge.renderToString(T().createElement(qe,{adapter:this.adapter},T().createElement(Ze,{vacation:t})))};getFormattedMonth=e=>{let t=e.format("MMM");return t=this.capitalizeFirstLetter(t),t};getEmployeeVacationsStructure=async()=>{let e,t=this.adapter.userId;const i=this.getCallContext();[oe.ESSVacationEmployeeSchedulingAssignments,oe.RXVacationEmployeeSchedulingAssignments,oe.ESSVacationApprovalAssignments,oe.RXVacationApprovalAssignments,oe.ESSVacationReworkAssignments,oe.RXVacationReworkAssignments].includes(i)&&(e=this.adapter.id),i===oe.RXVacationApprovalTasks&&(t=this.adapter.api.getEntity().Employee.Value),i===oe.RXVacationReworkAssignments&&(t=this.adapter.api.getEntity().Author.Value);const o=await this.request.getEmployeesInfo(i,t,e);o.forEach((e=>{e.id=e.Id,e.className=`${D} id-${e.Id} ${e.Vacations.length?"":I}`}));const n=new Array;return o.forEach((e=>e.Vacations.map((t=>{const i={};i.id=t.Id,i.group=e.Id,i.start=this.convertToTimezone(t.StartDate),i.end=this.convertToTimezone(t.EndDate),i.AssignmentId=t.AssignmentId,i.VacationKind=t.VacationKind,i.VacationKindId=t.VacationKindId,i.Duration=t.Duration,n.push(i)})))),{employeeStruct:o,vacations:n}};removeClass=(e,t)=>e.split(" ").filter((e=>e!==t)).join(" ");addVacationsFromContext=(e,t)=>{t.forEach((t=>e.items.add(t)));const i=e.groups.get()[0];if(i?.className){const t=this.removeClass(i.className,I);i.className=t,e.groups.update(i)}};getTimePeriods=(e,t)=>{const i=new Array,o=new Set,n=e.startOf("month"),a=t.startOf("month");let s=n;for(;s.isBefore(a)||s.isSame(a);)i.push(this.getTimePeriodFormat(s)),o.add(s.year().toString()),s=s.add(1,"month");return Array.from(o).concat(i)};getTimePeriodFormat=e=>{let t=e;return M().isDayjs(t)||(t=M()(t)),`${this.capitalizeFirstLetter(t.format("MMMM"))} ${t.year()}`};capitalizeFirstLetter=e=>e.charAt(0).toUpperCase()+e.slice(1);getHolidays=async()=>{const e=await this.getPeriods(this.adapter.id),t=await this.request.getHolidaysAndWeekendsInPeriod(new Date(e.Start),new Date(e.End),this.adapter.id);this.holidays=new Set(t)};getViewPeriods=(e,t)=>{const i={},o=e?"month":"day";return i.timeAxis={scale:o,step:1},i.start=t,i.end=e?M()(t).add(this.adapter.theme.MONTHS_IN_YEAR_FOR_VIEW+this.getAdaptiveDays(e),"month").toDate():M()(t).add(this.adapter.theme.DAYS_IN_MONTHS_FOR_VIEW+this.getAdaptiveDays(e),"day").toDate(),i};getStartOfDay=e=>{let t=M()(e);return t.hour()>0&&(t=t.startOf("day").add(1,"day")),t};getServerTimezone=async()=>{this.serverTimezone=await this.request.getTimeZone()};getTimezoneSpan=e=>{const t=new Date(e).getTimezoneOffset()/y;return this.serverTimezone+t};convertToTimezone=e=>{"string"!=typeof e&&(e=M()(e).format());const t=this.getTimezoneSpan(e);return M()(e).add(t,"hour").format()};getAdaptiveDays=e=>{const t=e?this.adapter.theme.size.columnYearWidth:this.adapter.theme.size.columnMonthWidth,i=(Number(this.adapter.container.clientWidth)-this.adapter.theme.size.timelineWidth)/t;return Math.floor(i)};removeFunctionsFromObject(e){return JSON.parse(JSON.stringify(e))}getVacationFromTable=e=>{const t=[];return e.forEach((e=>{e.group&&e.VacationKind&&e.start&&e.Duration&&e.end&&t.push(e)})),t};getCallContext=()=>this.adapter.getCallContext();countEmployeesOnVacation(e,t,i="day"){const o=B()(e),n=new Set;if("day"===i&&t.filter((e=>e.group)).forEach((e=>{o.isBetween(e.start,e.end,"day","[]")&&n.add(String(e.group))})),"month"===i){const e=o.clone().startOf("month"),i=o.clone().endOf("month"),n=t.filter((t=>t.group&&B()(t.end)>=e&&B()(t.start)<=i)).map((t=>({group:t.group,start:B().max(B()(t.start),e),end:B().min(B()(t.end),i)})));for(let e=0;e<n.length;e++)for(let t=e+1;t<n.length;t++){const i=n[e],o=n[t];if(i.group!=o.group&&(i.start<=o.end&&i.end>=o.start))return 1}}return n.size}};function Qe(e){const[t,i]=(0,A.useState)(e),o=(0,A.useRef)(e);return[t,(0,A.useCallback)((e=>{o.current="function"==typeof e?e(o.current):e,i(o.current)}),[i]),o]}const et=E.Ay.div`${({theme:e})=>E.AH`
  display: flex;
  flex-wrap: wrap;
  position: relative;
  margin-bottom: ${e.size.monthNavigatorMarginBottom};
`}`;var tt=i(37525),it=i(75949);const ot=(0,E.Ay)(tt.A)`${({theme:e})=>E.AH`
  &&& {
    height: ${e.size.toggleButtonHeight};
    text-transform: none;
    border: none;
    border-radius: 3px;
    margin: ${e.size.toggleButtonMargin};
    font-family: ${e.font.family};
    font-size: ${e.font.primarySize};
    color: ${e.button.textColor};
    line-height: ${e.font.primaryLineHeight};
    font-weight: ${e.font.regularWeight};
    &:hover {
      background-color: ${e.button.hoveredBackgroundColor};
    }
  }
  
  &&.${it.A.selected} {
    color: ${e.button.selectedTextColor};
    background-color: ${e.button.backgroundColor};
  }
`}`;var nt=i(32511);const at=({isSetYear:e,onIsSetYearChange:t})=>{const i=(0,A.useMemo)((()=>e?p:f),[e]),{t:o}=(0,je.B)();return T().createElement(nt.A,{value:i,exclusive:!0},T().createElement(ot,{disableRipple:!0,onClick:()=>t(!1),value:f},o(f)),T().createElement(ot,{disableRipple:!0,onClick:()=>t(!0),value:p},o(p)))},st=(0,A.memo)(at),rt=({isSetYear:e,onIsSetYearChange:t})=>T().createElement(et,null,T().createElement(st,{isSetYear:e,onIsSetYearChange:t})),lt=(0,A.memo)(rt);var ct=i(17681),dt=i(52561);const mt=(0,E.Ay)(ct.A)`${({$isYear:e,theme:t})=>E.AH`
  ${e&&E.AH`
    && { 
      display: none;
    }
  `}
  && {
    font-family: ${t.font.family};
  }
`}`,ut=(0,E.Ay)(dt.A)`${({disabled:e,theme:t})=>E.AH`
  height: 30px;
  fieldset {
    border: none;
  }
  ${e&&E.AH`
    svg {
      display: none;
    }
  `}
  div {
    padding-top: 0;
    padding-bottom: 0;
  }
  && div {
     color: ${t.timeLine.textColor};
     -webkit-text-fill-color: currentColor;
     font-family: ${t.font.family};
     font-size: ${t.font.primarySize};
  }
`}`;var ht;function gt(){return gt=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var i=arguments[t];for(var o in i)({}).hasOwnProperty.call(i,o)&&(e[o]=i[o])}return e},gt.apply(null,arguments)}var pt=function(e){return A.createElement("svg",gt({xmlns:"http://www.w3.org/2000/svg",width:10,height:7,fill:"none"},e),ht||(ht=A.createElement("path",{fill:"#798288",fillRule:"evenodd",d:"M9.707 2.293 8.293.879 5 4.172 1.707.879.293 2.293 5 7z",clipRule:"evenodd"})))};function ft(){return ft=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var i=arguments[t];for(var o in i)({}).hasOwnProperty.call(i,o)&&(e[o]=i[o])}return e},ft.apply(null,arguments)}const yt=({isSetYear:e,selectedValue:t,values:i,onTimePeriodChange:o,...n})=>{const a=(0,A.useCallback)((e=>{o(e.target.value)}),[]);return T().createElement(ut,ft({},n,{disabled:e,value:t,onChange:a,MenuProps:{style:{height:350}},IconComponent:pt}),i.map((e=>T().createElement(mt,{$isYear:4===e.length,key:e,value:e},e))))},vt=(0,A.memo)(yt),bt=E.I4.div`${({theme:e})=>E.AH`
  position: absolute;
  left: 1px;
  height: ${e.size.headerFullHeight}px;
  width: ${e.size.groupWidth}px;
  z-index: 2;
  display: flex;
  flex-direction: column;
  margin-top: 1px;
`}`,Et=(0,E.I4)(vt)`${()=>E.AH`
  svg {
    top: 12px;
    right: 15px;
  }
`}`,Ct=E.I4.div`${({theme:e})=>E.AH`
  box-shadow: inset 0 1px 0 0 ${e.timeLine.borderColor};
  padding: 0px 8px;
  font-size: ${e.font.primarySize};
  height: ${e.size.overlapsHeight}px;
  display: flex;
  align-items: center;
`}`,xt=E.I4.div`${({theme:e})=>E.AH`
  height: ${e.size.columnHeight}px;
  display: flex;
  align-items: center;
`}`,St=e=>T().createElement(bt,null,T().createElement(xt,null,T().createElement(Et,e)),T().createElement(Ct,null,(0,l.t)("overlaps"))),At=(0,A.memo)(St),Tt=({functions:e})=>{const[t,i,o]=Qe(!0),[n,a,s]=Qe(""),[r,l]=(0,A.useState)([]),c=(0,A.useRef)(null),d=x();d(Re(t)),d(Oe(e.adapter.context.isMobile)),(0,A.useEffect)((()=>{(async()=>{try{const t=await e.getPeriods(e.adapter.id),i=e.getTimePeriods(M()(t.Start),M()(t.End));l(i),a(i[0])}catch(e){console.log(e)}})()}),[e]);const m=(0,A.useCallback)((t=>{if(t){const i="MMMM YYYY",n=B()(t,i),a=n.toDate(),s=e.adapter.theme.DAYS_IN_MONTHS_FOR_VIEW+e.getAdaptiveDays(o.current),r=n.add(s,"day").toDate();c.current?.timeline.setWindow(a,r)}}),[o,e.getAdaptiveDays]),u=(0,A.useCallback)((e=>{e!==n&&(a(e),m(e))}),[n]),h=(0,A.useCallback)((e=>{let t=e;o.current&&(t=t.substring(0,4)),t!==s.current&&a(t)}),[]),g=(0,A.useCallback)((t=>{i(t),a((i=>t?i.substring(i.length-4):e.getTimePeriodFormat(e.getStartOfDay(c.current?.timeline.getWindow().start).toDate())))}),[]),p=(0,A.useCallback)((async e=>{await i(!1),a(e),m(e)}),[]);return T().createElement(C,null,T().createElement(lt,{isSetYear:t,onIsSetYearChange:g}),T().createElement(At,{isSetYear:t,selectedValue:n,values:r,onTimePeriodChange:u}),T().createElement(_,{timelineRef:c,functions:e,onTimePeriodChange:h,isSetYear:t,onClickHeader:p}))},jt=(0,A.memo)(Tt),kt=({adapter:e,functions:t})=>{const i=S((e=>e.context)).value??e.context,[o,n]=(0,A.useState)(e.language),a=x();(0,A.useEffect)((()=>{a(Ve(e.isDarkTheme))}),[]),(0,A.useEffect)((()=>{if(i){const t=e.isDarkTheme;e.context={...e.context,...i},n(i.currentCulture??e.language),t!==e.isDarkTheme&&a(Ve(e.isDarkTheme))}}),[i,e.language,e.context,e.isDarkTheme]);const{i18n:s}=(0,je.B)();return(0,A.useEffect)((()=>{B().locale(o),M().locale(o),s.changeLanguage(o)}),[o]),T().createElement(jt,{functions:t})},wt=(0,A.memo)(kt),Dt=({api:e,initialContext:t,functions:i})=>{const o=x(),n=(0,A.useCallback)((()=>[]),[e.getEntity,t]),a=(0,A.useCallback)(F()((e=>{o($e(i.removeFunctionsFromObject(e))),o(Ye(n()))}),300),[n]);return(0,A.useEffect)((()=>{e.onControlUpdate=a,o(Ye(n()))}),[]),null},zt=(0,A.memo)(Dt),It=({api:e,initialContext:t,functions:i})=>{const[o,n]=(0,A.useState)(new Array),a=x(),[s,r]=(0,A.useState)(0),l=e.items.getValues();(0,A.useEffect)((()=>{(async()=>{try{const e=await i.request.getInitiatorId(l.PerformerId);r(e);const t=await i.request.getAllVacationKinds();n(t)}catch(e){console.log(e)}})()}),[]);const c=(0,A.useCallback)(((e,t,i,n,a)=>({id:0,group:e,start:M()(t).format(),Duration:i,end:M()(n).add(1,"day").add(-1,"second").format(),VacationKindId:a,VacationKind:o.find((e=>e.Id===Number(a)))?.Name??""})),[o]);return(0,A.useEffect)((()=>{const e=i.convertToTimezone(l.EndDate),t=c(s,l.BeginDate,l.Duration,e,l.VacationKind),o=[];o.push(t,l.PerformerId);const n=i.getVacationFromTable(o);a(Ye(n))}),[e.items.getValues,i.getVacationFromTable,i.convertToTimezone,c]),null},$t=(0,A.memo)(It),Lt=({api:e,initialContext:t,functions:i})=>{const[o,n]=(0,A.useState)(new Array),a=x(),s=e.items.getValues();(0,A.useEffect)((()=>{(async()=>{try{const e=await i.request.getAllVacationKinds();n(e)}catch(e){console.log(e)}})()}),[]);const r=(0,A.useCallback)(((e,t,i,n,a)=>({id:0,group:e,start:M()(t).format(),Duration:i,end:M()(n).add(1,"day").add(-1,"second").format(),VacationKindId:a,VacationKind:o.find((e=>e.Id===Number(a)))?.Name??""})),[o]);return(0,A.useEffect)((()=>{const t=i.convertToTimezone(s.EndDate),o=Number(e.items.getValues().PerformerId),n=r(o,s.BeginDate,s.Duration,t,s.VacationKind),l=[];l.push(n,s.PerformerId);const c=i.getVacationFromTable(l);a(Ye(c))}),[e.items.getValues,i.getVacationFromTable,i.convertToTimezone,r]),null},Mt=(0,A.memo)(Lt),Vt=({api:e,initialContext:t,functions:i})=>{const[o,n]=(0,A.useState)(new Array),a=x();(0,A.useEffect)((()=>{(async()=>{try{const e=await i.request.getAllVacationKinds();n(e)}catch(e){console.log(e)}})()}),[]);const s=(0,A.useCallback)(((e,t)=>({id:null===e.VacationId?Math.random():Number(e.VacationId),group:t,start:M()(e.BeginDate).format(),Duration:Number(e.VacationDuration),end:M()(e.EndDate).add(1,"day").add(-1,"second").format(),VacationKindId:Number(e.AvailableVacationId),VacationKind:o.find((t=>t.Id===Number(e.AvailableVacationId)))?.Name??""})),[o]),r=(0,A.useCallback)((()=>{const t=JSON.parse(e.items.getValues().Vacations),o=Number(e.items.getValues().PerformerId),n=[];t.forEach((e=>n.push(s(e.rowValues,o))));return i.getVacationFromTable(n)}),[e.items.getValues,i.getVacationFromTable,s]);return(0,A.useEffect)((()=>{a(Ye(r()))}),[r]),null},Ft=(0,A.memo)(Vt),Ht=({api:e,initialContext:t,functions:i})=>{const o=x(),n=(0,A.useCallback)(((e,t)=>{const o=i.convertToTimezone(e.DateEnd);return{id:e.Vacation?.Value??Math.random(),group:t,start:i.convertToTimezone(e.DateBegin),Duration:e.Duration,end:M()(o).add(1,"day").add(-1,"second").format(),VacationKindId:e.VacationKind.Value,VacationKind:e.VacationKind.DisplayValue}}),[i.convertToTimezone]),a=(0,A.useCallback)((()=>{const t=e.getEntity()?.Vacations,o=e.getEntity()?.Performer.Value,a=[];t?.forEach((e=>a.push(n(e,o))));return i.getVacationFromTable(a)}),[e.getEntity,t,i.getVacationFromTable,n]),s=(0,A.useCallback)(F()((e=>{o($e(i.removeFunctionsFromObject(e))),o(Ye(a()))}),300),[a]);return(0,A.useEffect)((()=>{e.onControlUpdate=s,o(Ye(a()))}),[]),null},Ot=(0,A.memo)(Ht),Nt=({api:e,functions:t})=>{const i=x(),o=(0,A.useCallback)(((e,i)=>{const o=t.convertToTimezone(e.DateEnd);return{id:0,group:i,start:t.convertToTimezone(e.DateBegin),Duration:e.Duration,end:M()(o).add(1,"day").add(-1,"second").format(),VacationKindId:e.VacationKind.Value,VacationKind:e.VacationKind.DisplayValue}}),[t.convertToTimezone]),n=(0,A.useCallback)((()=>{const i=e.getEntity(),n=i.Employee.Value,a=[],s=o(i,n);a.push(s);return t.getVacationFromTable(a)}),[e.getEntity]),a=(0,A.useCallback)(F()((e=>{i($e(t.removeFunctionsFromObject(e))),i(Ye(n()))}),300),[n]);return(0,A.useEffect)((()=>{e.onControlUpdate=a,i(Ye(n()))}),[]),null},Bt=(0,A.memo)(Nt),Rt=({api:e,functions:t})=>{const i=x(),o=(0,A.useCallback)(((e,i)=>{const o=t.convertToTimezone(e.DateEnd);return{id:0,group:i,start:t.convertToTimezone(e.DateBegin),Duration:e.Duration,end:M()(o).add(1,"day").add(-1,"second").format(),VacationKindId:e.VacationKind.Value,VacationKind:e.VacationKind.DisplayValue}}),[t.convertToTimezone]),n=(0,A.useCallback)((()=>{const i=e.getEntity(),n=i.Author.Value,a=[],s=o(i,n);a.push(s);return t.getVacationFromTable(a)}),[e.getEntity]),a=(0,A.useCallback)(F()((e=>{i($e(t.removeFunctionsFromObject(e))),i(Ye(n()))}),300),[n]);return(0,A.useEffect)((()=>{e.onControlUpdate=a,i(Ye(n()))}),[]),null},_t=(0,A.memo)(Rt),Wt={[oe.RXVacationEmployeeSchedulingAssignments]:Ot,[oe.ESSVacationEmployeeSchedulingAssignments]:Ft,[oe.RXVacationApprovalTasks]:Bt,[oe.ESSVacationApprovalTasks]:$t,[oe.ESSVacationApprovalAssignments]:zt,[oe.RXVacationApprovalAssignments]:zt,[oe.ESSVacationReworkAssignments]:Mt,[oe.RXVacationReworkAssignments]:_t},Yt=({system:e,api:t,functions:i,initialContext:o})=>{const n=i.getCallContext(),a=Wt[n];return T().createElement(a,{api:t,initialContext:o,functions:i})},Pt=(0,A.memo)(Yt),Kt=(0,q.A)({typography:{fontFamily:"Segoe UI, serif"}}),Ut=T().createContext({initialDrawComplete:()=>{}}),qt=({adapter:e,dialogId:t})=>{const[i,o]=(0,A.useState)(null),[n,a]=(0,A.useState)(!0),s=x();(0,A.useEffect)((()=>()=>{s(te(Z))}),[]),(0,A.useEffect)((()=>{(async()=>{const t=new Je(e);try{await t.getServerTimezone(),await t.getHolidays()}catch(e){console.log(e)}o(t)})()}),[e]),(0,A.useEffect)((()=>{(async()=>{try{if(null==i)return;const e=await i.getEmployeeVacationsStructure();s(te(e))}catch(e){console.error("VacationCalendarTool. getEmployeeVacationsStructure: ",e)}})()}),[i]);const r=(0,A.useCallback)((()=>{a(!1)}),[]);return T().createElement(G.A,{theme:Kt},T().createElement(qe,{adapter:e},T().createElement(Ut.Provider,{value:{initialDrawComplete:r}},T().createElement(Y,{dialogId:t,isLoading:n}),i?T().createElement(T().Fragment,null,T().createElement(Pt,{api:e.api,initialContext:e.context,system:e.system,functions:i}),T().createElement(wt,{adapter:e,functions:i})):null,n&&T().createElement(U,null))))},Gt=(0,A.memo)(qt);class Xt{constructor(){this.button=new Zt,this.timeLine=new Jt,this.employee=new Qt,this.color="",this.size=new ei,this.font=new ti,this.DAYS_IN_MONTHS_FOR_VIEW=19,this.MONTHS_IN_YEAR_FOR_VIEW=12,this.DAY_NUMBER_FOR_SCROLL=8,this.MONTH_NUMBER_FOR_SCROLL=10,this.marginTop="43px"}fillDefaultTheme=()=>{this.button.textColor="#121416",this.button.backgroundColor="#ECECEC",this.button.hoveredBackgroundColor="",this.timeLine.holidayColor="#FF3333",this.timeLine.topColor="#FF9900",this.button.selectedTextColor="#13406D",this.timeLine.baseBodyBackgroundColor="#FFFFFF",this.timeLine.groupBackgroundColor="#ECECEC33",this.timeLine.notPlannedTxtColor="#727272",this.timeLine.overlapsTxtColor="#727272",this.timeLine.periodBorderColor="#5783DB",this.timeLine.periodBackgroundColor="#5783db33",this.timeLine.periodBoxShadow="#5783DB",this.timeLine.disabledBorderColor="#DFE0E3",this.timeLine.beginMonthBorderColor="#FF9900",this.timeLine.textColor="#121416",this.timeLine.borderColor="#DFE0E3",this.timeLine.boxShadow="#0000001A",this.timeLine.backgroundColor="#FFFFFF",this.employee.fullNameColor="#FF9900",this.employee.jobTitleColor="#727272",this.color="#121416",this.font.family="'Segoe UI', -apple-system, BlinkMacSystemFont, Roboto, 'Helvetica Neue', Helvetica, Ubuntu, Arial, 'sans-serif'",this.font.style="italic",this.fillSize()};fillDarkTheme=()=>{this.button.textColor="#F4F4F4",this.button.selectedTextColor="#407FB9",this.button.backgroundColor="#3F3F3F",this.button.hoveredBackgroundColor="#3F3F3F",this.timeLine.holidayColor="#FF7C7C",this.timeLine.topColor="#FF9900",this.timeLine.baseBodyBackgroundColor="#353535",this.timeLine.groupBackgroundColor="#46464633",this.timeLine.notPlannedTxtColor="#999999",this.timeLine.periodBorderColor="#5783DB",this.timeLine.periodBackgroundColor="#5783DB33",this.timeLine.periodBoxShadow="#5783DB",this.timeLine.disabledBorderColor="#464646",this.timeLine.beginMonthBorderColor="#FF9900",this.timeLine.textColor="#F4F4F4",this.timeLine.borderColor="#464646",this.timeLine.boxShadow="#0000001A",this.timeLine.backgroundColor="#4E4C4C",this.employee.fullNameColor="#FF9900",this.employee.jobTitleColor="#999999",this.timeLine.overlapsTxtColor="#999999",this.color="#F4F4F4",this.font.family="'Segoe UI', -apple-system, BlinkMacSystemFont, Roboto, 'Helvetica Neue', Helvetica, Ubuntu, Arial, 'sans-serif'",this.font.style="italic",this.fillSize()};fillLKTheme=(e,t)=>{this.button.textColor=e.widget.tag.color,this.button.selectedTextColor=e.widget.tag.color,this.button.backgroundColor=e.widget.tag.backgroundColor,this.button.hoveredBackgroundColor=e.widget.itemBackground,this.timeLine.holidayColor=e.widget.showAllButtonText,this.timeLine.topColor=e.desktop.menu.markColor,this.timeLine.baseBodyBackgroundColor="#FFFFFF",this.timeLine.groupBackgroundColor="rgb(from var(--ess_SurfaceContainer80Color) r g b / 0.3)",this.timeLine.notPlannedTxtColor=e.widget.subtitle,this.timeLine.overlapsTxtColor=e.widget.subtitle,this.timeLine.periodBorderColor="#84A5D1",this.timeLine.periodBackgroundColor="#84A5D1",this.timeLine.periodBoxShadow=e.widget.infoBlockBackground,this.timeLine.disabledBorderColor="var(--ess_Supporting20Color)",this.timeLine.beginMonthBorderColor=e.desktop.menu.markColor,this.timeLine.textColor=e.widget.tag.color,this.timeLine.borderColor="var(--ess_Supporting20Color)",this.timeLine.boxShadow="",this.timeLine.backgroundColor=e.widget.background,this.employee.fullNameColor=e.widget.tag.color,this.employee.jobTitleColor=e.widget.subtitle,this.color=e.widget.tag.color,this.font.family=e.font.fontFamily,this.font.style="",this.marginTop="48px",this.fillLKSize(t),t?(this.font.primarySize=e.typography.desktop.primary.fontSize,this.font.secondarySize=e.typography.desktop.heading4.fontSize,this.font.regularWeight=e.typography.desktop.primary.fontWeight,this.font.mediumWeight=e.typography.desktop.primaryStrong.fontWeight,this.font.primaryLineHeight=e.typography.desktop.primary.lineHeight,this.font.secondaryLineHeight=e.typography.desktop.heading4.lineHeight,this.DAYS_IN_MONTHS_FOR_VIEW=4,this.MONTHS_IN_YEAR_FOR_VIEW=3,this.DAY_NUMBER_FOR_SCROLL=2):(this.font.primarySize=e.typography.desktop.primaryStrong.fontSize,this.font.secondarySize=e.typography.desktop.heading4.fontSize,this.font.regularWeight=e.typography.desktop.primary.fontWeight,this.font.mediumWeight=e.typography.desktop.primaryStrong.fontWeight,this.font.primaryLineHeight=e.typography.desktop.primary.lineHeight,this.font.secondaryLineHeight=e.typography.desktop.heading4.lineHeight)};fillSize=()=>{this.size.timelineWidth=1200,this.size.groupWidth=300,this.size.columnMonthWidth=(this.size.timelineWidth-this.size.groupWidth)/this.DAYS_IN_MONTHS_FOR_VIEW,this.size.columnYearWidth=(this.size.timelineWidth-this.size.groupWidth)/this.MONTHS_IN_YEAR_FOR_VIEW,this.size.columnHeight=48,this.size.overlapsHeight=32,this.size.headerFullHeight=this.size.columnHeight+this.size.overlapsHeight,this.size.maxTextWidth="230px"};fillLKSize=e=>{e?(this.size.timelineWidth=343,this.size.groupWidth=171.5,this.size.maxTextWidth="130px"):(this.size.timelineWidth=1316,this.size.groupWidth=277,this.size.maxTextWidth="230px"),this.size.columnHeight=56,this.size.overlapsHeight=40,this.size.headerFullHeight=this.size.columnHeight+this.size.overlapsHeight,this.size.columnMonthWidth=(this.size.timelineWidth-this.size.groupWidth)/this.DAYS_IN_MONTHS_FOR_VIEW,this.size.columnYearWidth=(this.size.timelineWidth-this.size.groupWidth)/this.MONTHS_IN_YEAR_FOR_VIEW}}class Zt{constructor(){this.textColor="",this.selectedTextColor="",this.backgroundColor="",this.hoveredBackgroundColor=""}}class Jt{constructor(){this.holidayColor="",this.topColor="",this.baseBodyBackgroundColor="",this.groupBackgroundColor="",this.notPlannedTxtColor="",this.overlapsTxtColor="",this.periodBorderColor="",this.periodBackgroundColor="",this.periodBoxShadow="",this.disabledBorderColor="",this.beginMonthBorderColor="",this.textColor="",this.borderColor="",this.boxShadow="",this.backgroundColor=""}}class Qt{constructor(){this.fullNameColor="",this.jobTitleColor=""}}class ei{constructor(){this.timelineWidth=0,this.groupWidth=0,this.columnMonthWidth=0,this.columnYearWidth=0,this.columnHeight=0,this.overlapsHeight=0,this.headerFullHeight=0,this.maxTextWidth="",this.toggleButtonHeight="24px",this.toggleButtonMargin="1px",this.monthNavigatorMarginBottom="8px"}}class ti{constructor(){this.family="",this.primarySize="",this.secondarySize="12px",this.regularWeight="",this.mediumWeight="",this.primaryLineHeight="",this.secondaryLineHeight="",this.style=""}}class ii{ESS_FUNCTIONS_PATH="/api/functions";DEFAULT_NAMESPACE="Vacations";id=0;system=ne.LK;constructor(e){this.api=e.api,this.context=e.initialContext,this.requestHeaders=new Headers,this.requestHeaders.set("Accept","application.json"),this.requestHeaders.set("Content-Type","application/json"),this.isMobile=this.context.isMobile,this.language=this.context?.language||g,this.getId(),this.theme=new Xt,this.fillTheme(),this.container=e.container,this.userId=this.api.items.getValues().PerformerId}get isDarkTheme(){return!1}fillTheme=()=>{this.theme.fillLKTheme(this.context.theme,this.isMobile)};get=async e=>{e=e.replace(/'/g,"");return await ESS.network.sendRequest(`${this.ESS_FUNCTIONS_PATH}/${e}`)};post=async(e,t)=>await this.doRequest(e,"POST",t);doRequest=async(e,t,i)=>{const o={method:t,headers:this.requestHeaders,body:JSON.stringify(i)};return await ESS.network.sendRequest(`${this.ESS_FUNCTIONS_PATH}/${e}`,o)};startAssigment=async e=>new Promise((()=>{}));getId=()=>{this.id=this.api.items?Number(this.api.items.getValues().AssignmentId):Number(this.api.getValues().AssignmentId)};getCallContext=()=>this.api.items.getValues().EntityGuid}const oi=e=>{const t=new ii(e),i=(0,s.createRoot)(e.container);return e.container.setAttribute("id",v),i.render(T().createElement(a.Kq,{store:Ke},T().createElement(Gt,{adapter:t,dialogId:e["dialog-id"]}))),Promise.resolve((()=>i.unmount()))};var ni=i(53525);class ai{INTEGRATION_SERVICE_PATH="/Integration/odata";DEFAULT_NAMESPACE="Vacations";id=0;system=ne.RX;constructor(e){this.api=e.api,this.context=e.initialContext,this.requestHeaders=new Headers,this.requestHeaders.set("Accept","application.json"),this.requestHeaders.set("Content-Type","application/json"),this.isMobile=!1,this.language=e.initialContext.currentCulture||g,this.getId(),this.theme=new Xt,this.fillTheme(),this.container=e.container,this.userId=this.context.userId}get isDarkTheme(){return this.context.theme===ni.Theme.Night}fillTheme=()=>{this.isDarkTheme?this.theme.fillDarkTheme():this.theme.fillDefaultTheme()};get=async(e,t=this.DEFAULT_NAMESPACE)=>{let i=e.includes("?")?e.replace("?","(").replace(/&/g,","):`${e}(`;i+=")";const o={headers:this.requestHeaders},n=await fetch(`${this.INTEGRATION_SERVICE_PATH}/${t}/${i}`,o),a=await n.json();return void 0!==a.value?a.value:a};post=async(e,t,i=this.DEFAULT_NAMESPACE)=>this.doRequest(e,"POST",t,i);doRequest=async(e,t,i,o=this.DEFAULT_NAMESPACE)=>{const n={method:t,headers:this.requestHeaders,body:JSON.stringify(i)},a=await fetch(`${this.INTEGRATION_SERVICE_PATH}/${o}/${e}`,n),s=await a.json();return s.value?s.value:s};startAssigment=async e=>this.api.executeAction(e);getId=()=>{this.id=this.api.getEntity().Id};getCallContext=()=>this.api.getEntity().Info.typeId}const si=e=>{const t=new ai(e),i=(0,s.createRoot)(e.container);return i.render(T().createElement(a.Kq,{store:Ke},T().createElement(Gt,{adapter:t}))),Promise.resolve((()=>i.unmount()))},ri={"rx-loader":n,"lk-loader":o}},35358:(e,t,i)=>{var o={"./af":25177,"./af.js":25177,"./ar":61509,"./ar-dz":41488,"./ar-dz.js":41488,"./ar-kw":58676,"./ar-kw.js":58676,"./ar-ly":42353,"./ar-ly.js":42353,"./ar-ma":24496,"./ar-ma.js":24496,"./ar-ps":6947,"./ar-ps.js":6947,"./ar-sa":82682,"./ar-sa.js":82682,"./ar-tn":89756,"./ar-tn.js":89756,"./ar.js":61509,"./az":95533,"./az.js":95533,"./be":28959,"./be.js":28959,"./bg":47777,"./bg.js":47777,"./bm":54903,"./bm.js":54903,"./bn":61290,"./bn-bd":17357,"./bn-bd.js":17357,"./bn.js":61290,"./bo":31545,"./bo.js":31545,"./br":11470,"./br.js":11470,"./bs":44429,"./bs.js":44429,"./ca":7306,"./ca.js":7306,"./cs":56464,"./cs.js":56464,"./cv":73635,"./cv.js":73635,"./cy":64226,"./cy.js":64226,"./da":93601,"./da.js":93601,"./de":77853,"./de-at":26111,"./de-at.js":26111,"./de-ch":54697,"./de-ch.js":54697,"./de.js":77853,"./dv":60708,"./dv.js":60708,"./el":54691,"./el.js":54691,"./en-au":53872,"./en-au.js":53872,"./en-ca":28298,"./en-ca.js":28298,"./en-gb":56195,"./en-gb.js":56195,"./en-ie":66584,"./en-ie.js":66584,"./en-il":65543,"./en-il.js":65543,"./en-in":9033,"./en-in.js":9033,"./en-nz":79402,"./en-nz.js":79402,"./en-sg":43004,"./en-sg.js":43004,"./eo":32934,"./eo.js":32934,"./es":97650,"./es-do":20838,"./es-do.js":20838,"./es-mx":17730,"./es-mx.js":17730,"./es-us":56575,"./es-us.js":56575,"./es.js":97650,"./et":3035,"./et.js":3035,"./eu":3508,"./eu.js":3508,"./fa":119,"./fa.js":119,"./fi":90527,"./fi.js":90527,"./fil":95995,"./fil.js":95995,"./fo":52477,"./fo.js":52477,"./fr":85498,"./fr-ca":26435,"./fr-ca.js":26435,"./fr-ch":37892,"./fr-ch.js":37892,"./fr.js":85498,"./fy":37071,"./fy.js":37071,"./ga":41734,"./ga.js":41734,"./gd":70217,"./gd.js":70217,"./gl":77329,"./gl.js":77329,"./gom-deva":32124,"./gom-deva.js":32124,"./gom-latn":93383,"./gom-latn.js":93383,"./gu":95050,"./gu.js":95050,"./he":11713,"./he.js":11713,"./hi":43861,"./hi.js":43861,"./hr":26308,"./hr.js":26308,"./hu":90609,"./hu.js":90609,"./hy-am":17160,"./hy-am.js":17160,"./id":74063,"./id.js":74063,"./is":89374,"./is.js":89374,"./it":88383,"./it-ch":21827,"./it-ch.js":21827,"./it.js":88383,"./ja":23827,"./ja.js":23827,"./jv":89722,"./jv.js":89722,"./ka":41794,"./ka.js":41794,"./kk":27088,"./kk.js":27088,"./km":96870,"./km.js":96870,"./kn":84451,"./kn.js":84451,"./ko":63164,"./ko.js":63164,"./ku":98174,"./ku-kmr":6181,"./ku-kmr.js":6181,"./ku.js":98174,"./ky":78474,"./ky.js":78474,"./lb":79680,"./lb.js":79680,"./lo":15867,"./lo.js":15867,"./lt":45766,"./lt.js":45766,"./lv":69532,"./lv.js":69532,"./me":58076,"./me.js":58076,"./mi":41848,"./mi.js":41848,"./mk":30306,"./mk.js":30306,"./ml":73739,"./ml.js":73739,"./mn":99053,"./mn.js":99053,"./mr":86169,"./mr.js":86169,"./ms":73386,"./ms-my":92297,"./ms-my.js":92297,"./ms.js":73386,"./mt":77075,"./mt.js":77075,"./my":72264,"./my.js":72264,"./nb":22274,"./nb.js":22274,"./ne":8235,"./ne.js":8235,"./nl":92572,"./nl-be":43784,"./nl-be.js":43784,"./nl.js":92572,"./nn":54566,"./nn.js":54566,"./oc-lnc":69330,"./oc-lnc.js":69330,"./pa-in":29849,"./pa-in.js":29849,"./pl":94418,"./pl.js":94418,"./pt":79834,"./pt-br":48303,"./pt-br.js":48303,"./pt.js":79834,"./ro":24457,"./ro.js":24457,"./ru":82271,"./ru.js":82271,"./sd":1221,"./sd.js":1221,"./se":33478,"./se.js":33478,"./si":17538,"./si.js":17538,"./sk":5784,"./sk.js":5784,"./sl":46637,"./sl.js":46637,"./sq":86794,"./sq.js":86794,"./sr":45719,"./sr-cyrl":3322,"./sr-cyrl.js":3322,"./sr.js":45719,"./ss":56e3,"./ss.js":56e3,"./sv":41011,"./sv.js":41011,"./sw":40748,"./sw.js":40748,"./ta":11025,"./ta.js":11025,"./te":11885,"./te.js":11885,"./tet":28861,"./tet.js":28861,"./tg":86571,"./tg.js":86571,"./th":55802,"./th.js":55802,"./tk":59527,"./tk.js":59527,"./tl-ph":29231,"./tl-ph.js":29231,"./tlh":31052,"./tlh.js":31052,"./tr":85096,"./tr.js":85096,"./tzl":79846,"./tzl.js":79846,"./tzm":81765,"./tzm-latn":97711,"./tzm-latn.js":97711,"./tzm.js":81765,"./ug-cn":48414,"./ug-cn.js":48414,"./uk":16618,"./uk.js":16618,"./ur":57777,"./ur.js":57777,"./uz":57609,"./uz-latn":72475,"./uz-latn.js":72475,"./uz.js":57609,"./vi":21135,"./vi.js":21135,"./x-pseudo":64051,"./x-pseudo.js":64051,"./yo":82218,"./yo.js":82218,"./zh-cn":52648,"./zh-cn.js":52648,"./zh-hk":1632,"./zh-hk.js":1632,"./zh-mo":31541,"./zh-mo.js":31541,"./zh-tw":50304,"./zh-tw.js":50304};function n(e){var t=a(e);return i(t)}function a(e){if(!i.o(o,e)){var t=new Error("Cannot find module '"+e+"'");throw t.code="MODULE_NOT_FOUND",t}return o[e]}n.keys=function(){return Object.keys(o)},n.resolve=a,e.exports=n,n.id=35358}}]);
//# sourceMappingURL=712_1.0_816c9aa8.js.map