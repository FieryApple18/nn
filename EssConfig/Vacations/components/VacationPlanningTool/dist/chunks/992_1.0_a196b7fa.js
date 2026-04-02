(self.webpackChunkvacation_planning_tool=self.webpackChunkvacation_planning_tool||[]).push([[992],{60519:(e,t,a)=>{"use strict";a.r(t),a.d(t,{default:()=>ua});var i={};a.r(i),a.d(i,{default:()=>la});var n={};a.r(n),a.d(n,{default:()=>ma});var o=a(71468),s=a(5338),r=a(25699),l=a(27656),c=a.n(l),d=a(32107),m=a(17508);const u="Vacations",h="EssPlatform",g="ru",p="year",f="month",v=60,y="planningToolContainer",b={en:{translation:{employee:"Employee",vacationKind:"Vacation Kind",startDate:"Start date",endDate:"End date",duration:"Duration",overlaps:"Overlaps",approve:"Approve",onRework:"On rework",month:"Month",year:"Year",unscheduled:"Not scheduled",shorterDays:"d.",tooltipDateFormat:"DD.MM.YYYY"}},ru:{translation:{createVacation:"Создать отпуск",updateVacation:"Обновить отпуск",employee:"Сотрудник",vacationKind:"Вид отпуска",startDate:"Дата начала",endDate:"Дата окончания",duration:"Продолжительность",overlaps:"Пересечения",approve:"Согласовать",onRework:"На доработку",month:"Месяц",year:"Год",unscheduled:"Не запланировано",shorterDays:"дн.",tooltipDateFormat:"DD.MM.YYYY"}}};c().use(d.A).use(m.A).use(r.r9).init({resources:b,lng:g,fallbackLng:g,debug:!0,interpolation:{escapeValue:!1}});a(62796);var S=a(38267);const E=S.I4.div`${({theme:e})=>S.AH`
  position: absolute;
  inset: 0;
  width: 100%;
  font-family: ${e.font.family};
  font-size: 14px;
  color: ${e.color};
`}`;let C=function(e){return e.ESSVacationEmployeeSchedulingAssignments="f178357e-6bfe-42d7-8a55-7013389de185",e.RXVacationEmployeeSchedulingAssignments="f03a4dcd-b007-43e3-9c67-71b771c1d392",e.ESSVacationApprovalTasks="d1899307-092b-4a08-87b5-3c7c14f2feff",e.RXVacationApprovalTasks="096abbce-40bd-4f02-a3f8-9db679b6682c",e.ESSVacationApprovalAssignments="fb83fd0b-2a8d-43b6-8c0f-c29849fb8fc0",e.RXVacationApprovalAssignments="6cefd82a-6bf6-4966-b883-eaeaff8b3ed6",e.ESSVacationReworkAssignments="82835b9b-0070-4df6-b5de-2a7a5e99677f",e.RXVacationReworkAssignments="34e36333-d4dd-492f-b275-c8441a615dbc",e.RXVacationChangeTasks="38037034-d6cd-45bd-ba94-64af95fa0657",e.ESSVacationChangeTasks="6e7a8c95-aa2b-44f3-ae0c-6f748a0e68ac",e.RXVacationChangeReworkAssignments="64b7682c-597f-44c0-b2f6-56c2ae5d04d7",e.ESSReworkChangeVacationAssignments="d54a4fd2-a4cc-4e00-aa31-e4f692b334a6",e.ESSVacationChangeApprovalAssignments="ae04a4d6-3b1a-4b00-8178-57cb65cfcde5",e.RXVacationChangeApprovalAssignments="a4780d00-7026-4bcb-a880-64ed6d4c72b4",e.ESSSelfRecallVacationEmployee="64f4996e-265b-4b36-af64-c64630c7a900",e.ESSSelfRecallReworkAssignments="cbd8fbf7-e341-4f84-8e13-d8cb8e41f7ff",e}({}),A=function(e){return e.RX="RX",e.LK="LK",e}({}),V=function(e){return e.hrproUnavailable="hrpro-unavailable",e}({});const x=o.wA.withTypes(),T=o.d4.withTypes();var k=a(57115),w=a.n(k),D=a(99621);const j="full-name",I="change-color",z="need-highlight",$="group-highlight",L="unscheduled",R="hide-month";var M=a(74353),F=a.n(M),O=a(20181),H=a.n(O);const N=({className:e,functions:t,timelineRef:a,onTimePeriodChange:i,isSetYear:n,onClickHeader:o})=>{const s=(0,k.useRef)(new Date),r=(0,k.useRef)(n),l=T((e=>e.vacations)),{initialDrawComplete:c}=(0,k.useContext)(Zt),{employeeStruct:d,vacations:m}=T((e=>e.timelineData));(0,k.useEffect)((()=>{r.current=n}),[n]),(0,k.useEffect)((()=>{const e=a.current;if(e){e.timeline.setOptions(t.getOptions());const a=async()=>{try{await t.setUnavailablePeriod();const a=await t.getOptionsAsync();e.timeline.setOptions(a),e.timeline.redraw()}catch(e){console.log(e)}};a()}}),[t]),(0,k.useEffect)((()=>{const e=l.value;return(()=>{const i=a.current;if(i){i.groups.clear(),i.items.clear();const a=e=>JSON.parse(JSON.stringify(e));d.forEach((e=>i.groups.add(a(e)))),m.forEach((e=>i.items.add(a(e)))),e.length>0&&t.addVacationsFromContext(i,e),i.forceUpdate()}t.style.highlightingSubscribe()})(),()=>{t.style.highlightingUnsubscribe()}}),[l,d,m]);const u=(0,k.useCallback)((e=>{if(e.byUser){const a=e.start>s.current;s.current=e.start;const n=F()(e.start),o=n.daysInMonth()-t.adapter.theme.DAY_NUMBER_FOR_SCROLL;let l="";r.current?a&&n.month()>t.adapter.theme.MONTH_NUMBER_FOR_SCROLL?l=(n.year()+1).toString():!a&&n.month()<t.adapter.theme.MONTH_NUMBER_FOR_SCROLL+1&&(l=n.year().toString()):a&&n.date()>o?l=t.getTimePeriodFormat(n.add(1,"month")):!a&&n.date()<o&&(l=t.getTimePeriodFormat(n)),""!==l&&i(l)}}),[]),h=(0,k.useCallback)((e=>{if(!r.current||!e?.event?.target)return;if(["I","IMG","SVG","PATH","RECT"].includes(e.event.target.nodeName))return;const{target:a}=e.event,i=a.classList?Array.from(a.classList).map(String):[],n=a.offsetParent&&a.offsetParent.classList?Array.from(a.offsetParent.classList).map(String):[],s=new Set(["vis-top","vis-minor"]);[...i,...n].some((e=>s.has(e)))&&o(t.getTimePeriodFormat(F()(e.time)))}),[]),g=(0,k.useCallback)((()=>{const e=a.current;if(e){const a=e.timeline.getWindow(),i=t.getViewPeriods(n,a.start);e.timeline.setOptions(i)}}),[n,t.getViewPeriods]);(0,k.useEffect)((()=>{g()}),[g]);const p=(0,k.useCallback)(H()((()=>{if(t.isUserScrolling){document.querySelectorAll(".vis-text.vis-day1").forEach((e=>e.classList.add(R))),t.isUserScrolling=!1}}),1e3),[t.isUserScrolling]),f=(0,k.useCallback)((e=>{e.byUser&&!t.isUserScrolling&&(t.isUserScrolling=!0)}),[t.isUserScrolling]);(0,k.useEffect)((()=>{const e=a.current;if(e){let a;e.timeline.on("rangechange",u),e.timeline.on("click",h),e.timeline.on("rangechanged",p),e.timeline.on("rangechange",f);const i=()=>{t.adapter.isMobile?(clearTimeout(a),a=setTimeout(g,300)):addEventListener("resize",g)};return addEventListener("resize",i),()=>{e.timeline.off("rangechange",u),e.timeline.off("click",h),e.timeline.off("rangechanged",p),e.timeline.off("rangechange",f),removeEventListener("resize",i),clearTimeout(a)}}}),[h,u,p,f,g]),(0,k.useEffect)((()=>{if(a.current?.timeline&&e){const t=a.current?.timeline;t.body.dom.container.classList.value=e}}),[e]);const v=(0,k.useCallback)((()=>{const e=()=>{const t=a.current?.groups?.get();if(!t?.length)return void requestAnimationFrame(e);const i=t[t.length-1].id;document.querySelector(`.id-${i}`)?c():requestAnimationFrame(e)};requestAnimationFrame(e)}),[c]);(0,k.useEffect)((()=>{const e=t.getCallContext();if(!a.current||e!==C.ESSVacationChangeTasks&&e!==C.RXVacationChangeTasks&&e!==C.RXVacationChangeReworkAssignments&&e!==C.ESSReworkChangeVacationAssignments&&e!==C.RXVacationChangeApprovalAssignments&&e!==C.ESSVacationChangeApprovalAssignments&&e!==C.ESSSelfRecallVacationEmployee&&e!==C.ESSSelfRecallReworkAssignments)return;const i=async()=>{const e=await t.getUnavailablePeriod();e.HasUnavailablePeriod&&((e,i)=>{const n=a.current;if(!n)return!1;const o=n.groups.get();if(!o?.length)return!1;const s=n.items.get().filter((e=>e.id&&String(e.id).startsWith("unavailable_period_")));s.length&&n.items.remove(s.map((e=>e.id)));const r=o.map((a=>({id:`unavailable_period_${a.id}`,type:"background",start:t.convertToTimezone(e),end:t.convertToTimezone(i),group:a.id,className:V.hrproUnavailable,editable:!1,selectable:!1})));n.items.add(r)})(e.Start,e.End)},n=a.current;if(n?.timeline){const e=()=>{const t=n.groups.get();t?.length&&(n.timeline.off("changed",e),i())};n.timeline.on("changed",e);const t=n.groups.get();return t?.length&&i(),()=>{n.timeline.off("changed",e)}}}),[a,t,l]);const y=(0,k.useMemo)((()=>({onInitialDrawComplete:v,rollingMode:{}})),[v]);return w().createElement(D.A,{ref:a,options:y})},B=(0,k.memo)(N);var P=a(95093),_=a.n(P);const W=_().months().map((e=>`.vis-${e.toLowerCase()}`)),U=(0,S.Ay)(B)`${({theme:e,isSetYear:t})=>S.AH`
  height: calc(100% - (${e.size.toggleButtonHeight} + 2 * ${e.size.toggleButtonMargin} + ${e.size.overlapsHeight}px) + ${e.size.monthNavigatorMarginBottom});
  ${((e,t)=>S.AH`
  ${t&&S.AH`
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

  .vis-text:has(b) {
    background: ${e.timeLine.unavailableColor} !important;
  }

  .vis-text div:has(b) {
    background: ${e.timeLine.unavailableFirstDay} !important;
  }

  /* Подсветка праздничных дней */
  /* Считаем, что если у даты есть дочерний span, то это праздничный день */
  .vis-text.vis-minor > span {
    color: ${e.timeLine.holidayColor};
  }

  // Подсветка ячеек с месяцами в шапке в преставлении год (подсвечивем только ячейку с датой)
  ${W.map(((e,t)=>11===t?`.vis-text${e} p:hover`:`.vis-text${e} p:hover, `))} {
    ${t?S.AH`
        color: ${e.timeLine.topColor};
        box-sizing: border-box;
        background-color: ${e.timeLine.groupBackgroundColor};
        cursor: pointer;
      `:""}}

  // Стили для первого дня месяца
  .vis-text.vis-minor.vis-day1 {
    overflow: visible;
    z-index: 1;
    padding-left: 0;
    padding-right: 0;
    
    // Прячем месяц
    &.${R} > div {
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
  ${(e=>S.AH`
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
  .${$} {
    box-sizing: border-box;
    background-color: ${e.timeLine.groupBackgroundColor};
  }

  // Установка текста незапланировано для сотрудников без отпусков
  .vis-foreground .${L} {
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
  ${((e,t)=>S.AH`
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
    
    ${t&&S.AH`
      padding-left: 0;
    `}
    
    label {
      cursor: inherit;
    }
  }
`)(e,t)}
  ${((e,t)=>S.AH`
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

    ${t&&S.AH`
      &.vis-january {
        border-color: ${e.timeLine.beginMonthBorderColor};
        border-width: 2px;
        z-index: 1;
      }
    `}
  }
  
  /* Подсветка ячеек недоступного периода */
  .hrpro-unavailable {
    background-color: ${e.timeLine.unavailableColor} !important;
    pointer-events: none;
    z-index: -1;

    /* Скрыть контент в ячейках недоступного периода */
    .vis-item-content {
      display: none !important;
    }
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
  ${(e=>S.AH`
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
  ${S.AH`
  .vis-text.vis-minor {
    p {
      margin-bottom: 0px;
    }
  }
`}
`}`;var Y=a(90563);const K=S.DU`${({theme:e,dialogId:t,isLoading:a})=>S.AH`
  // Стили для кнопок в шапке.
  .${Y.A.paper}.${Y.A.paper} {
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
    #${y} {
      position: absolute;
      inset: 0;
      ${a&&S.AH`overflow: hidden;`}
      ${E} {
        height: 100%;
      }
      ${U} {
        height: calc(100% - (${e.size.toggleButtonHeight} + 2 * ${e.size.toggleButtonMargin} + ${e.size.monthNavigatorMarginBottom}));
      }
    }
  }
  `}`,X=S.Ay.div`${({theme:e})=>S.AH`
  position: absolute;
  inset: 0;
  z-index: 3;
  background: ${e.timeLine.baseBodyBackgroundColor};
  outline: 4px solid ${e.timeLine.baseBodyBackgroundColor};
  display: flex;
  justify-content: center;
  align-items: center;
`}`;var q=a(78717);const G=()=>w().createElement(X,null,w().createElement(q.A,null));var J=a(47994),Z=a(72363),Q=a(20038);const ee={employeeStruct:[],vacations:[]},te=(0,Q.Z0)({name:"timelineData",initialState:ee,reducers:{setEmployeeStruct:(e,t)=>{e.employeeStruct=t.payload},setVacations:(e,t)=>{e.vacations=t.payload},setTimelineData:(e,t)=>{const a=e;a.vacations=t.payload.vacations,a.employeeStruct=t.payload.employeeStruct}}}),{setVacations:ae,setEmployeeStruct:ie,setTimelineData:ne}=te.actions,oe=te.reducer;class se{constructor(e){this.adapter=e}getEmployeesInfo=async(e,t,a=null,i=null)=>this.adapter.get(`GetEmployeesInfo?context='${e}'&employeeId=${t}&assignmentId=${a}&vacationId=${i}`,u);getVacationKinds=async e=>this.adapter.get(`GetAvailableVacationKinds?employeeId=${e}`,u);getVacationEndDate=async(e,t,a,i)=>this.adapter.get(`GetVacationEndDate?dateBegin=${e.format("YYYY-MM-DD")}&duration=${t}&userId=${a}&vacationKindId=${i}`,u);getPeriods=async(e,t=null)=>this.adapter.get(`GetAvailablePeriods?context='${e}'&assignmentId=${t}`,u);getHolidaysAndWeekendsInPeriod=async(e,t,a)=>this.adapter.get(`GetHolidaysAndWeekendsInPeriod?periodBegin=${e.toJSON()}&periodEnd=${t.toJSON()}&recipientId=${a||null}`,u);getTimeZone=async()=>this.adapter.get("GetTimeZone",u);getAllVacationKinds=async()=>this.adapter.get("GetVacationKinds","");getInitiatorId=async e=>this.adapter.get(`GetInitiatorId?userId=${e}`,h);getPeriodsForShiftSimple=async(e,t=null,a=null)=>await this.adapter.get(`GetPeriodsForShiftSimple?employeeId=${e}&vacationId=${t}&assignmentId=${a}`);getUnavailablePeriodStart=async(e,t="")=>await this.adapter.get(`GetUnavalablePeriodStart?employeeId=${e}`,t);getEmployeeForApprovalAssignment=async e=>await this.adapter.get(`GetEmployeeForApprovalAssignment?assignmentId=${e}`,u)}class re{constructor(e){this.adapter=e}highlightingSubscribe=()=>{this.getGroupElements().forEach((e=>{e.addEventListener("mouseover",this.groupOverHandler),e.addEventListener("mouseout",this.groupOutHandler),e.addEventListener("touchstart",this.groupOverHandler),e.addEventListener("touchend",this.groupOutHandler)}))};highlightingUnsubscribe=()=>{this.getGroupElements().forEach((e=>{e.removeEventListener("mouseover",this.groupOverHandler),e.removeEventListener("mouseout",this.groupOutHandler),e.removeEventListener("touchstart",this.groupOverHandler),e.removeEventListener("touchend",this.groupOutHandler)}))};groupOverHandler=e=>{const t=$,a=e.currentTarget,i=a?.classList,n=Array.from(i).find((e=>e.startsWith("id-"))),o=document.querySelectorAll(`.${z}.${n}`);Array.from(o).forEach((e=>{e.classList.add(t),e.querySelector(`.${j}`)?.classList.add(I)})),i.add(t)};groupOutHandler=e=>{const t=$,a=e.currentTarget,i=a?.classList,n=Array.from(i).find((e=>e.startsWith("id-"))),o=document.querySelectorAll(`.${z}.${n}`);Array.from(o).forEach((e=>{e.classList.remove(t),e.querySelector(`.${j}`)?.classList.remove(I)})),i.remove(t)};getGroupElements=()=>{const e=new Array,t=document.getElementsByClassName("vis-group-level-0");return Object.entries(t).forEach((t=>{e.push(t[1]);const a=t[1].classList[t[1].classList.length-1],i=document.getElementsByClassName(`vis-group ${a}`);Object.entries(i).forEach((t=>{t[1]["vis-group"]&&e.push(t[1])}))})),e}}const le=({viewDate:e,scale:t,functions:a,isUnavailable:i})=>{const{countEmployeesOnVacation:n,holidays:o,isUserScrolling:s,getFormattedMonth:r}=a,l=T((e=>e.vacations)),{vacations:c}=T((e=>e.timelineData)),d=(0,k.useMemo)((()=>[...l.value,...c]),[l.value,c]),m=(0,k.useCallback)(((e,t)=>{let a=w().createElement(w().Fragment,null,w().createElement("p",null,e.format("dd").toUpperCase(),w().createElement("br",null),e.format("D")),i&&w().createElement("b",null),w().createElement("i",null,t>1?t:""));if(o.has(e.format("YYYY-MM-DD"))&&(a=w().createElement("span",null,a)),1===e.date()&&s){let t=r(e);t=t.replace(".",""),a=w().createElement(w().Fragment,null,a,w().createElement("div",null,`${t} ${e.format("YYYY")}`,i&&w().createElement("b",null)))}return a}),[o,s,r,i]),u=(0,k.useCallback)(((e,t)=>w().createElement(w().Fragment,null,w().createElement("p",null,r(e),w().createElement("br",null),e.format("YYYY")),i&&w().createElement("b",null),w().createElement("i",null,1===t?w().createElement("img",{src:"data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTciIGhlaWdodD0iMTYiIHZpZXdCb3g9IjAgMCAxNyAxNiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTguMjA4MDEgMTRDMTEuNTIxNyAxNCAxNC4yMDggMTEuMzEzNyAxNC4yMDggOEMxNC4yMDggNC42ODYyOSAxMS41MjE3IDIgOC4yMDgwMSAyQzQuODk0MyAyIDIuMjA4MDEgNC42ODYyOSAyLjIwODAxIDhDMi4yMDgwMSAxMS4zMTM3IDQuODk0MyAxNCA4LjIwODAxIDE0WiIgc3Ryb2tlPSIjODk4OTg5IiBzdHJva2UtbWl0ZXJsaW1pdD0iMTAiLz4KPHBhdGggZD0iTTguMjA4MDEgNVY4LjUiIHN0cm9rZT0iIzg5ODk4OSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIiBzdHJva2UtbGluZWpvaW49InJvdW5kIi8+CjxwYXRoIGQ9Ik04LjIwODAxIDExLjVDOC42MjIyMiAxMS41IDguOTU4MDEgMTEuMTY0MiA4Ljk1ODAxIDEwLjc1QzguOTU4MDEgMTAuMzM1OCA4LjYyMjIyIDEwIDguMjA4MDEgMTBDNy43OTM3OSAxMCA3LjQ1ODAxIDEwLjMzNTggNy40NTgwMSAxMC43NUM3LjQ1ODAxIDExLjE2NDIgNy43OTM3OSAxMS41IDguMjA4MDEgMTEuNVoiIGZpbGw9IiM4OTg5ODkiLz4KPC9zdmc+"}):""))),[r,i]);return(0,k.useMemo)((()=>{const a=_()(e);let i;if(0==d.length)return"day"===t&&(i=m(a,0)),"month"===t&&(i=u(a,0)),i;const o=n(a,d,t);return"day"===t&&(i=m(a,o)),"month"===t&&(i=u(a,o)),i}),[e,t,m,u,n,d])},ce=(0,k.memo)(le);var de=a(71783);const me=S.I4.div`
  display: flex;
  justify-content: space-between;
  padding-left: 5px;
  height: 100%;
  align-items: center;
`,ue=S.I4.div`${({theme:e})=>S.AH`
  padding-left: 5px;
  padding-right: 5px;
  display: flex;
  flex: 1;
  flex-direction: column;
  line-height: 18px;
  width: ${e.size.maxTextWidth};
`}`,he=(0,S.I4)(de.A)`
  && {
    height: 36px;
    width: 36px;
    background-color: transparent;
  }
`,ge=S.I4.label`
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  max-width: 100%;
`,pe=(0,S.I4)(ge).attrs({className:j})`${({theme:e})=>S.AH`
  &.${I} {
    color: ${e.employee.fullNameColor};
  }
  font-size: ${e.font.primarySize};
  font-weight: ${e.font.mediumWeight};
  line-height: ${e.font.primaryLineHeight};
`}`,fe=(0,S.I4)(ge)`${({theme:e})=>S.AH`
  font-size: ${e.font.primarySize};
  color: ${e.employee.jobTitleColor};
  font-weight: ${e.font.regularWeight};
`}`;var ve,ye,be;function Se(){return Se=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var a=arguments[t];for(var i in a)({}).hasOwnProperty.call(a,i)&&(e[i]=a[i])}return e},Se.apply(null,arguments)}var Ee=function(e){return k.createElement("svg",Se({xmlns:"http://www.w3.org/2000/svg",width:32,height:32,fill:"none"},e),ve||(ve=k.createElement("rect",{width:32,height:32,fill:"#BCCEE6",rx:16})),ye||(ye=k.createElement("path",{stroke:"#151414",strokeMiterlimit:10,d:"M16 18a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z"})),be||(be=k.createElement("path",{stroke:"#151414",strokeLinecap:"round",strokeLinejoin:"round",d:"M9.938 21.5a7 7 0 0 1 12.125 0"})))};const Ce=({employee:e})=>{const t=T((e=>e.isMobile));return w().createElement(me,null,w().createElement(he,null,w().createElement(Ee,null)),w().createElement(ue,null,w().createElement(pe,null,t.value?e.ShortName:e.Name),w().createElement(fe,null,e.JobTitle)))},Ae=(0,k.memo)(Ce),Ve=S.I4.div`
  padding-top: 2px;
  line-height: 16px;
`,xe=S.I4.p`${({theme:e})=>S.AH`
  font-size: ${e.font.seconadrySize};
  margin-top: 5px;
  margin-left: -1px;
`}`,Te=S.I4.p`${({theme:e})=>S.AH`
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  max-width: 100%;
  font-size: ${e.font.secondarySize};
  margin-left: 4px;
  margin-top: 0;
  margin-bottom: 0;
`}`;var ke=a(18492);const we=({vacation:e})=>{const{t}=(0,ke.B)(),a=T((e=>e.isSetYear));return w().createElement(Ve,null,w().createElement(Te,null,a.value?" ":e.VacationKind),w().createElement(xe,null,`${e.Duration} ${t("shorterDays")}`))},De=(0,k.memo)(we),je=({adapter:e,children:t})=>{const a=T((e=>e.isDarkTheme)).value,i=(0,k.useMemo)((()=>(e.fillTheme(),{...e.theme})),[a,e]);return w().createElement(S.NP,{theme:i},t)},Ie=(0,k.memo)(je),ze=(0,Q.Z0)({name:"context",initialState:{value:null},reducers:{setContext:(e,t)=>{e.value=t.payload}}}),{setContext:$e}=ze.actions,Le=ze.reducer,Re=(0,Q.Z0)({name:"isDarkTheme",initialState:{value:!1},reducers:{setIsDarkTheme:(e,t)=>{e.value=t.payload}}}),{setIsDarkTheme:Me}=Re.actions,Fe=Re.reducer,Oe=(0,Q.Z0)({name:"isMobile",initialState:{value:!1},reducers:{setIsMobile:(e,t)=>{e.value=t.payload}}}),{setIsMobile:He}=Oe.actions,Ne=Oe.reducer,Be=(0,Q.Z0)({name:"isSetYear",initialState:{value:!0},reducers:{setValue:(e,t)=>{e.value=t.payload}}}),{setValue:Pe}=Be.actions,_e=Be.reducer,We=(0,Q.Z0)({name:"vacations",initialState:{value:[]},reducers:{setVacations:(e,t)=>{e.value=t.payload}}}),{setVacations:Ue}=We.actions,Ye=We.reducer,Ke=(0,Q.U1)({reducer:{isSetYear:_e,vacations:Ye,context:Le,isDarkTheme:Fe,isMobile:Ne,timelineData:oe}}),Xe=({adapter:e,children:t})=>w().createElement(o.Kq,{store:Ke},w().createElement(Ie,{adapter:e},t)),qe=(0,k.memo)(Xe);var Ge=a(65848);const Je=({vacation:e})=>{const{t}=(0,ke.B)(),a=t("tooltipDateFormat");return T((e=>e.isMobile)).value?null:w().createElement(w().Fragment,null,w().createElement("p",null,e.VacationKind),w().createElement("p",null,`${_()(e.start).format(a)}-${_()(e.end).format(a)}`),w().createElement("p",null,`${e.Duration} ${t("shorterDays")}`))},Ze=(0,k.memo)(Je);const Qe=class{unavailablePeriod={Start:"",End:"",HasUnavailablePeriod:!1};constructor(e){this.adapter=e,this.request=new se(e),this.style=new re(e),this.holidays=new Set,this.isUserScrolling=!1,this.serverTimezone=0}setUnavailablePeriod=async()=>{const e=this.getCallContext();if(e!==C.ESSVacationChangeTasks&&e!==C.RXVacationChangeTasks&&e!==C.RXVacationChangeReworkAssignments&&e!==C.ESSReworkChangeVacationAssignments&&e!==C.RXVacationChangeApprovalAssignments&&e!==C.ESSVacationChangeApprovalAssignments&&e!==C.ESSSelfRecallVacationEmployee&&e!==C.ESSSelfRecallReworkAssignments)return;const t=await this.getUnavailablePeriod();t.HasUnavailablePeriod&&(this.unavailablePeriod=t)};getUnavailablePeriod=async()=>{const e=this.getCallContext();let t=this.adapter.userId;e!==C.RXVacationChangeTasks&&e!=C.RXVacationChangeReworkAssignments&&e!=C.RXVacationChangeApprovalAssignments||(t=this.adapter.api.getEntity().Employee.Value);let a="",i="",n=!1;if(e===C.ESSVacationChangeTasks||e===C.ESSReworkChangeVacationAssignments||e===C.ESSSelfRecallVacationEmployee||e===C.ESSSelfRecallReworkAssignments){const e=this.adapter.api.items.getValues().OldVacations;({End:i,HasUnavailablePeriod:n}=await this.handlePeriodsForShiftSimple(e)),n&&(a=await this.getUnavailablePeriodStart(Number(t)))}else if(e===C.ESSVacationChangeApprovalAssignments){const e=this.adapter.id;({End:i,HasUnavailablePeriod:n}=await this.handlePeriodsForShiftSimple(null,e)),n&&(a=await this.getUnavailablePeriodStart(Number(t)))}else if(e===C.RXVacationChangeApprovalAssignments){const e=this.adapter.id,o=await this.handlePeriodsForShiftSimple(null,e);({End:i,HasUnavailablePeriod:n}=JSON.parse(o.toString())),n&&(a=await this.getUnavailablePeriodStart(Number(t),u))}else({End:i,HasUnavailablePeriod:n}=JSON.parse(this.adapter.api.getEntity().AvailableDays)),n&&(a=await this.getUnavailablePeriodStart(Number(t),u));return{Start:a,End:i,HasUnavailablePeriod:n}};assignmentContexts=[C.ESSVacationEmployeeSchedulingAssignments,C.RXVacationEmployeeSchedulingAssignments,C.ESSVacationApprovalAssignments,C.RXVacationApprovalAssignments,C.ESSVacationReworkAssignments,C.RXVacationReworkAssignments,C.RXVacationChangeReworkAssignments,C.ESSReworkChangeVacationAssignments,C.ESSVacationChangeApprovalAssignments,C.RXVacationChangeApprovalAssignments];approvalAssignmentContexts=[C.ESSVacationChangeApprovalAssignments,C.RXVacationChangeApprovalAssignments,C.ESSVacationApprovalAssignments,C.RXVacationApprovalAssignments];getPeriods=async e=>{let t,a,i;const n=this.getCallContext();if(this.assignmentContexts.includes(n)&&(t=e),n===C.RXVacationChangeTasks||n===C.RXVacationChangeReworkAssignments)({Start:a,End:i}=JSON.parse(this.adapter.api.getEntity().AvailableDays));else if(n===C.ESSVacationChangeTasks||n===C.ESSReworkChangeVacationAssignments||n===C.ESSSelfRecallVacationEmployee||n===C.ESSSelfRecallReworkAssignments){const e=this.adapter.api.items.getValues().OldVacations;({Start:a,End:i}=await this.handlePeriodsForShiftSimple(e))}else({Start:a,End:i}=await this.request.getPeriods(n,t));return{Start:this.convertToTimezone(a),End:this.convertToTimezone(i)}};getOptions=()=>{const e={showCurrentTime:!1,width:"100%",maxHeight:"100%",groupHeightMode:"fixed",stack:!1,orientation:{axis:"top",item:"top"},autoResize:!0,zoomable:!1,verticalScroll:!0,horizontalScroll:!0,showMajorLabels:!1};return e.locale=this.adapter.language,e.template=this.itemTemplate,e.groupTemplate=this.groupTemplate,e.format={minorLabels:this.dateTemplate},e.tooltip={overflowMethod:"none",template:this.tooltipTemplate},e};getOptionsAsync=async()=>{const e={},t=await this.getPeriods(this.adapter.id);e.min=t.Start,e.max=t.End;const a=new Date(t.Start),i=this.getViewPeriods(!0,a);return e.start=i.start,e.end=i.end,e};itemTemplate=(e,t)=>{if(t&&e){const a="data-is-rendered",i=t.hasAttribute(a),n=w().createElement(qe,{adapter:this.adapter},w().createElement(De,{vacation:e}));if(i)t.timelineItemRoot.render(n);else{t.setAttribute(a,"true");const e=(0,s.createRoot)(t);t.timelineItemRoot=e,e.render(n)}}return""};groupTemplate=(e,t)=>{if(t&&e){const a="data-is-rendered",i=t.hasAttribute(a),n=w().createElement(qe,{adapter:this.adapter},w().createElement(Ae,{employee:e}));if(i)t.timelineGroupRoot.render(n);else{t.setAttribute(a,"true");const e=(0,s.createRoot)(t);t.timelineGroupRoot=e,e.render(n)}}return""};dateTemplate=(e,t)=>{const{Start:a,End:i,HasUnavailablePeriod:n}=this.unavailablePeriod;let s=!1;if(n&&a&&i){const t=_()(e),n=_()(a),o=_()(i);s=t.isBetween(n,o,"day","[]")}return Ge.renderToString(w().createElement(o.Kq,{store:Ke},w().createElement(ce,{viewDate:e,scale:t,functions:this,isUnavailable:s})))};tooltipTemplate=e=>{const t=e;return Ge.renderToString(w().createElement(qe,{adapter:this.adapter},w().createElement(Ze,{vacation:t})))};getFormattedMonth=e=>{let t=e.format("MMM");return t=this.capitalizeFirstLetter(t),t};getEmployeeVacationsStructure=async()=>{let e,t=this.adapter.userId;const a=this.getCallContext();this.assignmentContexts.includes(a)&&(e=this.adapter.id,this.approvalAssignmentContexts.includes(a)&&(t=await this.request.getEmployeeForApprovalAssignment(e))),a!==C.RXVacationChangeTasks&&a!==C.RXVacationChangeReworkAssignments||(t=this.adapter.api.getEntity().Employee.Value),a===C.RXVacationReworkAssignments&&(t=this.adapter.api.getEntity().Author.Value),a===C.RXVacationApprovalTasks&&(t=this.adapter.api.getEntity().Employee.Value);let i=await this.request.getEmployeesInfo(a,t,e);const n=[];let o,s;if(a===C.RXVacationChangeTasks||a===C.RXVacationChangeReworkAssignments){const r=this.adapter.api.getEntity(),l=r.Vacations;s=this.adapter.api.getEntity().Compensation,o=r.RecallDate,r?.Vacation1?.Value&&(i=await this.request.getEmployeesInfo(a,t,e,r.Vacation1.Value),(l.length>0&&l[0]?.DateEnd||s)&&(n.push(r.Vacation1.Value),r?.Vacation2?.Value&&(n.push(r.Vacation2.Value),r?.Vacation3?.Value&&n.push(r.Vacation3.Value))))}else if(a===C.ESSVacationChangeTasks||a===C.ESSReworkChangeVacationAssignments||a===C.ESSSelfRecallVacationEmployee||a===C.ESSSelfRecallReworkAssignments){const{items:r}=this.adapter.api,l=r.getValues().OldVacations;let c;if(s=r.getValues().Compensation,o=r.getValues().RecallDate,r.getValues().Vacations&&(c=JSON.parse(r.getValues().Vacations)),l){const o=JSON.parse(l);o[0]&&(i=await this.request.getEmployeesInfo(a,t,e,o[0]),(c[0]?.rowValues.EndDate||s)&&(n.push(Number(o[0])),o[1]&&(n.push(Number(o[1])),o[2]&&n.push(Number(o[2])))))}}const r=new Array;return i&&(i.forEach((e=>{e.id=e.Id;const t=e.Vacations.length>0&&e.Vacations.every((e=>n.includes(e.Id)&&(!o||o&&new Date(o).getDate()==new Date(e.StartDate).getDate())));e.className=`${z} id-${e.Id} ${!e.Vacations.length||t?L:""}`})),i.forEach((e=>e.Vacations.map((t=>{if(n.includes(t.Id)){if(o&&new Date(o).getDate()>new Date(t.StartDate).getDate()){const a={};a.id=Math.random(),a.group=e.Id,a.start=this.convertToTimezone(t.StartDate),a.end=this.convertToTimezone(_()(o).endOf("day").add(-1,"day").toDate()),a.AssignmentId=t.AssignmentId,a.VacationKind=t.VacationKind,a.VacationKindId=t.VacationKindId;const i=_()(a.end),n=_()(a.start).startOf("day");a.Duration=i.diff(n,"days")+1,r.push(a)}}else{const a={};a.id=t.Id,a.group=e.Id,a.start=this.convertToTimezone(t.StartDate),a.end=this.convertToTimezone(t.EndDate),a.AssignmentId=t.AssignmentId,a.VacationKind=t.VacationKind,a.VacationKindId=t.VacationKindId,a.Duration=t.Duration,r.push(a)}}))))),{employeeStruct:i,vacations:r}};removeClass=(e,t)=>e.split(" ").filter((e=>e!==t)).join(" ");addVacationsFromContext=(e,t)=>{t.forEach((t=>e.items.add(t)));const a=e.groups.get()[0];if(a?.className){const t=this.removeClass(a.className,L);a.className=t,e.groups.update(a)}};getTimePeriods=(e,t)=>{const a=new Array,i=new Set,n=e.startOf("month"),o=t.startOf("month");let s=n;for(;s.isBefore(o)||s.isSame(o);)a.push(this.getTimePeriodFormat(s)),i.add(s.year().toString()),s=s.add(1,"month");return Array.from(i).concat(a)};getTimePeriodFormat=e=>{let t=e;return F().isDayjs(t)||(t=F()(t)),`${this.capitalizeFirstLetter(t.format("MMMM"))} ${t.year()}`};capitalizeFirstLetter=e=>e.charAt(0).toUpperCase()+e.slice(1);getHolidays=async()=>{const e=await this.getPeriods(this.adapter.id),t=await this.request.getHolidaysAndWeekendsInPeriod(new Date(e.Start),new Date(e.End),this.adapter.id);this.holidays=new Set(t)};getViewPeriods=(e,t)=>{const a={},i=e?"month":"day";return a.timeAxis={scale:i,step:1},a.start=t,a.end=e?F()(t).add(this.adapter.theme.MONTHS_IN_YEAR_FOR_VIEW+this.getAdaptiveDays(e),"month").toDate():F()(t).add(this.adapter.theme.DAYS_IN_MONTHS_FOR_VIEW+this.getAdaptiveDays(e),"day").toDate(),a};getStartOfDay=e=>{let t=F()(e);return t.hour()>0&&(t=t.startOf("day").add(1,"day")),t};getServerTimezone=async()=>{this.serverTimezone=await this.request.getTimeZone()};getTimezoneSpan=e=>{const t=new Date(e).getTimezoneOffset()/v;return this.serverTimezone+t};convertToTimezone=e=>{"string"!=typeof e&&(e=F()(e).format());const t=this.getTimezoneSpan(e);return F()(e).add(t,"hour").format()};getAdaptiveDays=e=>{const t=e?this.adapter.theme.size.columnYearWidth:this.adapter.theme.size.columnMonthWidth,a=(Number(this.adapter.container.clientWidth)-this.adapter.theme.size.timelineWidth)/t;return Math.floor(a)};removeFunctionsFromObject(e){return JSON.parse(JSON.stringify(e))}getVacationFromTable=e=>{const t=[];return e.forEach((e=>{e.group&&e.VacationKind&&e.start&&e.Duration&&e.end&&t.push(e)})),t};getCallContext=()=>this.adapter.getCallContext();countEmployeesOnVacation(e,t,a="day"){const i=_()(e),n=new Set;if("day"===a&&t.filter((e=>e.group)).forEach((e=>{i.isBetween(e.start,e.end,"day","[]")&&n.add(String(e.group))})),"month"===a){const e=i.clone().startOf("month"),a=i.clone().endOf("month"),n=t.filter((t=>t.group&&_()(t.end)>=e&&_()(t.start)<=a)).map((t=>({group:t.group,start:_().max(_()(t.start),e),end:_().min(_()(t.end),a)})));for(let e=0;e<n.length;e++)for(let t=e+1;t<n.length;t++){const a=n[e],i=n[t];if(a.group!=i.group&&(a.start<=i.end&&a.end>=i.start))return 1}}return n.size}getPeriodsForShiftSimple=async(e,t=null,a=null)=>await this.request.getPeriodsForShiftSimple(e,t,a);handlePeriodsForShiftSimple=async(e,t)=>{let a=null;e&&(a=JSON.parse(e)[0]);return await this.getPeriodsForShiftSimple(this.adapter.userId,a,t)};getUnavailablePeriodStart=async(e,t="")=>await this.request.getUnavailablePeriodStart(e,t)};function et(e){const[t,a]=(0,k.useState)(e),i=(0,k.useRef)(e);return[t,(0,k.useCallback)((e=>{i.current="function"==typeof e?e(i.current):e,a(i.current)}),[a]),i]}const tt=S.Ay.div`${({theme:e})=>S.AH`
  display: flex;
  flex-wrap: wrap;
  position: relative;
  margin-bottom: ${e.size.monthNavigatorMarginBottom};
`}`;var at=a(37525),it=a(75949);const nt=(0,S.Ay)(at.A)`${({theme:e})=>S.AH`
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
`}`;var ot=a(32511);const st=({isSetYear:e,onIsSetYearChange:t})=>{const a=(0,k.useMemo)((()=>e?p:f),[e]),{t:i}=(0,ke.B)();return w().createElement(ot.A,{value:a,exclusive:!0},w().createElement(nt,{disableRipple:!0,onClick:()=>t(!1),value:f},i(f)),w().createElement(nt,{disableRipple:!0,onClick:()=>t(!0),value:p},i(p)))},rt=(0,k.memo)(st),lt=({isSetYear:e,onIsSetYearChange:t})=>w().createElement(tt,null,w().createElement(rt,{isSetYear:e,onIsSetYearChange:t})),ct=(0,k.memo)(lt);var dt=a(17681),mt=a(52561);const ut=(0,S.Ay)(dt.A)`${({$isYear:e,theme:t})=>S.AH`
  ${e&&S.AH`
    && { 
      display: none;
    }
  `}
  && {
    font-family: ${t.font.family};
  }
`}`,ht=(0,S.Ay)(mt.A)`${({disabled:e,theme:t})=>S.AH`
  height: 30px;
  fieldset {
    border: none;
  }
  ${e&&S.AH`
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
`}`;var gt;function pt(){return pt=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var a=arguments[t];for(var i in a)({}).hasOwnProperty.call(a,i)&&(e[i]=a[i])}return e},pt.apply(null,arguments)}var ft=function(e){return k.createElement("svg",pt({xmlns:"http://www.w3.org/2000/svg",width:10,height:7,fill:"none"},e),gt||(gt=k.createElement("path",{fill:"#798288",fillRule:"evenodd",d:"M9.707 2.293 8.293.879 5 4.172 1.707.879.293 2.293 5 7z",clipRule:"evenodd"})))};function vt(){return vt=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var a=arguments[t];for(var i in a)({}).hasOwnProperty.call(a,i)&&(e[i]=a[i])}return e},vt.apply(null,arguments)}const yt=({isSetYear:e,selectedValue:t,values:a,onTimePeriodChange:i,...n})=>{const o=(0,k.useCallback)((e=>{i(e.target.value)}),[]);return w().createElement(ht,vt({},n,{disabled:e,value:t,onChange:o,MenuProps:{style:{height:350}},IconComponent:ft}),a.map((e=>w().createElement(ut,{$isYear:4===e.length,key:e,value:e},e))))},bt=(0,k.memo)(yt),St=S.I4.div`${({theme:e})=>S.AH`
  position: absolute;
  left: 1px;
  height: ${e.size.headerFullHeight}px;
  width: ${e.size.groupWidth}px;
  z-index: 2;
  display: flex;
  flex-direction: column;
  margin-top: 1px;
`}`,Et=(0,S.I4)(bt)`${()=>S.AH`
  svg {
    top: 12px;
    right: 15px;
  }
`}`,Ct=S.I4.div`${({theme:e})=>S.AH`
  box-shadow: inset 0 1px 0 0 ${e.timeLine.borderColor};
  padding: 0px 8px;
  font-size: ${e.font.primarySize};
  height: ${e.size.overlapsHeight}px;
  display: flex;
  align-items: center;
`}`,At=S.I4.div`${({theme:e})=>S.AH`
  height: ${e.size.columnHeight}px;
  display: flex;
  align-items: center;
`}`,Vt=e=>w().createElement(St,null,w().createElement(At,null,w().createElement(Et,e)),w().createElement(Ct,null,(0,l.t)("overlaps"))),xt=(0,k.memo)(Vt),Tt=({functions:e,context:t})=>{const[a,i,n]=et(!0),[o,s,r]=et(""),[l,c]=(0,k.useState)([]),d=(0,k.useRef)(null),m=x();(0,k.useEffect)((()=>{m(Pe(a)),m(He(e.adapter.context.isMobile))}),[m,a,e.adapter.context.isMobile]),(0,k.useEffect)((()=>{(async()=>{try{const t=await e.getPeriods(e.adapter.id),a=e.getTimePeriods(F()(t.Start),F()(t.End));c(a),s(a[0])}catch(e){console.log(e)}})()}),[e,t]);const u=(0,k.useCallback)((t=>{if(t){const a="MMMM YYYY",i=_()(t,a),o=i.toDate(),s=e.adapter.theme.DAYS_IN_MONTHS_FOR_VIEW+e.getAdaptiveDays(n.current),r=i.add(s,"day").toDate();d.current?.timeline.setWindow(o,r)}}),[n,e.getAdaptiveDays]),h=(0,k.useCallback)((e=>{e!==o&&(s(e),u(e))}),[o]),g=(0,k.useCallback)((e=>{let t=e;n.current&&(t=t.substring(0,4)),t!==r.current&&s(t)}),[]),p=(0,k.useCallback)((t=>{i(t),s((a=>t?a.substring(a.length-4):e.getTimePeriodFormat(e.getStartOfDay(d.current?.timeline.getWindow().start).toDate())))}),[]),f=(0,k.useCallback)((async e=>{await i(!1),s(e),u(e)}),[]);return w().createElement(E,null,w().createElement(ct,{isSetYear:a,onIsSetYearChange:p}),w().createElement(xt,{isSetYear:a,selectedValue:o,values:l,onTimePeriodChange:h}),w().createElement(U,{timelineRef:d,functions:e,onTimePeriodChange:g,isSetYear:a,onClickHeader:f}))},kt=(0,k.memo)(Tt),wt=({adapter:e,functions:t})=>{const a=T((e=>e.context)).value??e.context,[i,n]=(0,k.useState)(e.language),o=x();(0,k.useEffect)((()=>{o(Me(e.isDarkTheme))}),[]),(0,k.useEffect)((()=>{if(a){const t=e.isDarkTheme;e.context={...e.context,...a},n(a.currentCulture??e.language),t!==e.isDarkTheme&&o(Me(e.isDarkTheme))}}),[a,e.language,e.context,e.isDarkTheme]);const{i18n:s}=(0,ke.B)();return(0,k.useEffect)((()=>{_().locale(i),F().locale(i),s.changeLanguage(i)}),[i]),w().createElement(kt,{functions:t,context:t.getCallContext()})},Dt=(0,k.memo)(wt),jt=({api:e,functions:t})=>{const a=x(),i=(0,k.useCallback)(H()((e=>{a($e(t.removeFunctionsFromObject(e)))}),300),[]);return(0,k.useEffect)((()=>{e.onControlUpdate=i}),[]),null},It=(0,k.memo)(jt),zt=({api:e,functions:t})=>{const[a,i]=(0,k.useState)(new Array),n=x();(0,k.useEffect)((()=>{(async()=>{try{const e=await t.request.getAllVacationKinds();i(e)}catch(e){console.log(e)}})()}),[]);const o=(0,k.useCallback)(((e,t)=>({id:Math.random(),group:t,start:F()(e.BeginDate).format(),Duration:Number(e.VacationDuration),end:F()(e.EndDate).add(1,"day").add(-1,"second").format(),VacationKindId:Number(e.VacationKindId),VacationKind:a.find((t=>t.Id===Number(e.VacationKindId)))?.Name??""})),[a]),s=(0,k.useCallback)((()=>{const a=JSON.parse(e.items.getValues().Vacations),i=Number(e.items.getValues().PerformerId),n=[];a.forEach((e=>n.push(o(e.rowValues,i))));return t.getVacationFromTable(n)}),[e.items.getValues,t.getVacationFromTable,o]);return(0,k.useEffect)((()=>{n(Ue(s()))}),[s]),null},$t=(0,k.memo)(zt),Lt=({api:e,functions:t})=>{const[a,i]=(0,k.useState)(new Array),n=x(),[o,s]=(0,k.useState)(0),r=e.items.getValues();(0,k.useEffect)((()=>{(async()=>{try{const e=await t.request.getInitiatorId(r.PerformerId);s(e);const a=await t.request.getAllVacationKinds();i(a)}catch(e){console.log(e)}})()}),[]);const l=(0,k.useCallback)(((e,t,i,n,o)=>({id:0,group:e,start:F()(t).format(),Duration:i,end:F()(n).add(1,"day").add(-1,"second").format(),VacationKindId:o,VacationKind:a.find((e=>e.Id===Number(o)))?.Name??""})),[a]);return(0,k.useEffect)((()=>{const e=t.convertToTimezone(r.EndDate),a=l(o,r.BeginDate,r.Duration,e,r.VacationKind),i=[];i.push(a,r.PerformerId);const s=t.getVacationFromTable(i);n(Ue(s))}),[e.items.getValues,t.getVacationFromTable,t.convertToTimezone,l]),null},Rt=(0,k.memo)(Lt),Mt=({api:e,functions:t})=>{const[a,i]=(0,k.useState)(new Array),n=x();(0,k.useEffect)((()=>{(async()=>{try{const e=await t.request.getAllVacationKinds();i(e)}catch(e){console.log(e)}})()}),[]);const o=(0,k.useCallback)(((e,t)=>({id:null===e.VacationId?Math.random():Number(e.VacationId),group:t,start:F()(e.BeginDate).format(),Duration:Number(e.VacationDuration),end:F()(e.EndDate).add(1,"day").add(-1,"second").format(),VacationKindId:Number(e.AvailableVacationId),VacationKind:a.find((t=>t.Id===Number(e.AvailableVacationId)))?.Name??""})),[a]),s=(0,k.useCallback)((()=>{const a=JSON.parse(e.items.getValues().Vacations),i=Number(e.items.getValues().PerformerId),n=[];a.forEach((e=>n.push(o(e.rowValues,i))));return t.getVacationFromTable(n)}),[e.items.getValues,t.getVacationFromTable,o]);return(0,k.useEffect)((()=>{n(Ue(s()))}),[s]),null},Ft=(0,k.memo)(Mt),Ot=({api:e,functions:t})=>{const[a,i]=(0,k.useState)(new Array),n=x(),o=e.items.getValues();(0,k.useEffect)((()=>{(async()=>{try{const e=await t.request.getAllVacationKinds();i(e)}catch(e){console.log(e)}})()}),[]);const s=(0,k.useCallback)(((e,t,i,n,o)=>({id:0,group:e,start:F()(t).format(),Duration:i,end:F()(n).add(1,"day").add(-1,"second").format(),VacationKindId:o,VacationKind:a.find((e=>e.Id===Number(o)))?.Name??""})),[a]);return(0,k.useEffect)((()=>{const a=t.convertToTimezone(o.EndDate),i=Number(e.items.getValues().PerformerId),r=s(i,o.BeginDate,o.Duration,a,o.VacationKind),l=[];l.push(r,o.PerformerId);const c=t.getVacationFromTable(l);n(Ue(c))}),[e.items.getValues,t.getVacationFromTable,t.convertToTimezone,s]),null},Ht=(0,k.memo)(Ot),Nt=({api:e,functions:t})=>{const a=x(),i=(0,k.useCallback)(((e,a)=>{const i=t.convertToTimezone(e.DateEnd);return{id:0,group:a,start:t.convertToTimezone(e.DateBegin),Duration:e.Duration,end:F()(i).add(1,"day").add(-1,"second").format(),VacationKindId:e.VacationKind.Value,VacationKind:e.VacationKind.DisplayValue}}),[t.convertToTimezone]),n=(0,k.useCallback)((()=>{const a=e.getEntity(),n=a.Employee.Value,o=[],s=i(a,n);o.push(s);return t.getVacationFromTable(o)}),[e.getEntity]),o=(0,k.useCallback)(H()((e=>{a($e(t.removeFunctionsFromObject(e))),a(Ue(n()))}),300),[n]);return(0,k.useEffect)((()=>{e.onControlUpdate=o,a(Ue(n()))}),[]),null},Bt=(0,k.memo)(Nt),Pt=({api:e,functions:t})=>{const a=x(),i=(0,k.useCallback)(((e,a)=>({id:e.Vacation?.Value??Math.random(),group:a,start:t.convertToTimezone(e.DateBegin),Duration:e.Duration,end:F()(t.convertToTimezone(e.DateEnd)).add(1,"day").add(-1,"second").format(),VacationKindId:e.VacationKind.Value,VacationKind:e.VacationKind.DisplayValue})),[t.convertToTimezone]),n=(0,k.useCallback)((()=>{const a=e.getEntity(),n=a?.Vacations,o=a?.Employee.Value,s=[];return n?.forEach((e=>s.push(i(e,o)))),t.getVacationFromTable(s)}),[e.getEntity,i]),o=(0,k.useCallback)(H()((e=>{a($e(t.removeFunctionsFromObject(e))),a(Ue(n()))}),300),[n]);return(0,k.useEffect)((()=>{e.onControlUpdate=o,a(Ue(n()))}),[]),null},_t=(0,k.memo)(Pt),Wt=({api:e,functions:t})=>{const a=x(),i=(0,k.useCallback)(((e,a)=>{const i=t.convertToTimezone(e.DateEnd);return{id:e.Vacation?.Value??Math.random(),group:a,start:t.convertToTimezone(e.DateBegin),Duration:e.Duration,end:F()(i).add(1,"day").add(-1,"second").format(),VacationKindId:e.VacationKind.Value,VacationKind:e.VacationKind.DisplayValue}}),[t.convertToTimezone]),n=(0,k.useCallback)((()=>{const a=e.getEntity()?.Vacations,n=e.getEntity()?.Performer.Value,o=[];a?.forEach((e=>o.push(i(e,n))));return t.getVacationFromTable(o)}),[e.getEntity,t.getVacationFromTable,i]),o=(0,k.useCallback)(H()((e=>{a($e(t.removeFunctionsFromObject(e))),a(Ue(n()))}),300),[n]);return(0,k.useEffect)((()=>{e.onControlUpdate=o,a(Ue(n()))}),[]),null},Ut=(0,k.memo)(Wt),Yt=({api:e,functions:t})=>{const a=x(),i=(0,k.useCallback)(((e,a)=>{const i=t.convertToTimezone(e.DateEnd);return{id:0,group:a,start:t.convertToTimezone(e.DateBegin),Duration:e.Duration,end:F()(i).add(1,"day").add(-1,"second").format(),VacationKindId:e.VacationKind.Value,VacationKind:e.VacationKind.DisplayValue}}),[t.convertToTimezone]),n=(0,k.useCallback)((()=>{const a=e.getEntity(),n=a.Author.Value,o=[],s=i(a,n);o.push(s);return t.getVacationFromTable(o)}),[e.getEntity]),o=(0,k.useCallback)(H()((e=>{a($e(t.removeFunctionsFromObject(e))),a(Ue(n()))}),300),[n]);return(0,k.useEffect)((()=>{e.onControlUpdate=o,a(Ue(n()))}),[]),null},Kt=(0,k.memo)(Yt),Xt={[C.RXVacationEmployeeSchedulingAssignments]:Ut,[C.ESSVacationEmployeeSchedulingAssignments]:Ft,[C.RXVacationApprovalTasks]:Bt,[C.ESSVacationApprovalTasks]:Rt,[C.ESSVacationApprovalAssignments]:It,[C.RXVacationApprovalAssignments]:It,[C.ESSVacationReworkAssignments]:Ht,[C.RXVacationReworkAssignments]:Kt,[C.RXVacationChangeTasks]:_t,[C.ESSVacationChangeTasks]:$t,[C.RXVacationChangeReworkAssignments]:_t,[C.ESSReworkChangeVacationAssignments]:$t,[C.ESSVacationChangeApprovalAssignments]:It,[C.RXVacationChangeApprovalAssignments]:It,[C.ESSSelfRecallVacationEmployee]:$t,[C.ESSSelfRecallReworkAssignments]:$t},qt=({api:e,functions:t})=>{const a=t.getCallContext(),i=Xt[a];return w().createElement(i,{api:e,functions:t})},Gt=(0,k.memo)(qt),Jt=(0,J.A)({typography:{fontFamily:"Segoe UI, serif"}}),Zt=w().createContext({initialDrawComplete:()=>{}}),Qt=({adapter:e,dialogId:t})=>{const[a,i]=(0,k.useState)(null),[n,o]=(0,k.useState)(!0),s=x();(0,k.useEffect)((()=>()=>{s(ne(ee))}),[]),(0,k.useEffect)((()=>{(async()=>{const t=new Qe(e);try{await t.getServerTimezone(),await t.getHolidays()}catch(e){console.log(e)}i(t)})()}),[e]),(0,k.useEffect)((()=>{(async()=>{try{if(null==a)return;const e=await a.getEmployeeVacationsStructure();s(ne(e))}catch(e){console.error("VacationCalendarTool. getEmployeeVacationsStructure: ",e)}})()}),[a]);const r=(0,k.useCallback)((()=>{o(!1)}),[]);return w().createElement(Z.A,{theme:Jt},w().createElement(qe,{adapter:e},w().createElement(Zt.Provider,{value:{initialDrawComplete:r}},w().createElement(K,{dialogId:t,isLoading:n}),a?w().createElement(w().Fragment,null,w().createElement(Gt,{api:e.api,initialContext:e.context,functions:a}),w().createElement(Dt,{adapter:e,functions:a})):null,n&&w().createElement(G,null))))},ea=(0,k.memo)(Qt);class ta{constructor(){this.button=new aa,this.timeLine=new ia,this.employee=new na,this.color="",this.size=new oa,this.font=new sa,this.DAYS_IN_MONTHS_FOR_VIEW=19,this.MONTHS_IN_YEAR_FOR_VIEW=12,this.DAY_NUMBER_FOR_SCROLL=8,this.MONTH_NUMBER_FOR_SCROLL=10,this.marginTop="43px"}fillDefaultTheme=()=>{this.button.textColor="#121416",this.button.backgroundColor="#ECECEC",this.button.hoveredBackgroundColor="",this.timeLine.holidayColor="#FF3333",this.timeLine.topColor="#FF9900",this.button.selectedTextColor="#13406D",this.timeLine.baseBodyBackgroundColor="#FFFFFF",this.timeLine.groupBackgroundColor="#ECECEC33",this.timeLine.notPlannedTxtColor="#727272",this.timeLine.overlapsTxtColor="#727272",this.timeLine.periodBorderColor="#5783DB",this.timeLine.periodBackgroundColor="#5783db33",this.timeLine.periodBoxShadow="#5783DB",this.timeLine.disabledBorderColor="#DFE0E3",this.timeLine.beginMonthBorderColor="#FF9900",this.timeLine.textColor="#121416",this.timeLine.borderColor="#DFE0E3",this.timeLine.boxShadow="#0000001A",this.timeLine.backgroundColor="#FFFFFF",this.timeLine.unavailableColor="#DFE0E34D",this.timeLine.unavailableFirstDay="#F5F6F7",this.employee.fullNameColor="#FF9900",this.employee.jobTitleColor="#727272",this.color="#121416",this.font.family="'Segoe UI', -apple-system, BlinkMacSystemFont, Roboto, 'Helvetica Neue', Helvetica, Ubuntu, Arial, 'sans-serif'",this.font.style="italic",this.fillSize()};fillDarkTheme=()=>{this.button.textColor="#F4F4F4",this.button.selectedTextColor="#407FB9",this.button.backgroundColor="#3F3F3F",this.button.hoveredBackgroundColor="#3F3F3F",this.timeLine.holidayColor="#FF7C7C",this.timeLine.topColor="#FF9900",this.timeLine.baseBodyBackgroundColor="#353535",this.timeLine.groupBackgroundColor="#46464633",this.timeLine.notPlannedTxtColor="#999999",this.timeLine.periodBorderColor="#5783DB",this.timeLine.periodBackgroundColor="#5783DB33",this.timeLine.periodBoxShadow="#5783DB",this.timeLine.disabledBorderColor="#464646",this.timeLine.beginMonthBorderColor="#FF9900",this.timeLine.textColor="#F4F4F4",this.timeLine.borderColor="#464646",this.timeLine.boxShadow="#0000001A",this.timeLine.backgroundColor="#4E4C4C",this.timeLine.unavailableColor="#5F5E5E4D",this.timeLine.unavailableFirstDay="#414141",this.employee.fullNameColor="#FF9900",this.employee.jobTitleColor="#999999",this.timeLine.overlapsTxtColor="#999999",this.color="#F4F4F4",this.font.family="'Segoe UI', -apple-system, BlinkMacSystemFont, Roboto, 'Helvetica Neue', Helvetica, Ubuntu, Arial, 'sans-serif'",this.font.style="italic",this.fillSize()};fillLKTheme=(e,t)=>{this.button.textColor=e.widget.tag.color,this.button.selectedTextColor=e.widget.tag.color,this.button.backgroundColor=e.widget.tag.backgroundColor,this.button.hoveredBackgroundColor=e.widget.itemBackground,this.timeLine.holidayColor=e.widget.showAllButtonText,this.timeLine.topColor=e.desktop.menu.markColor,this.timeLine.baseBodyBackgroundColor="#FFFFFF",this.timeLine.groupBackgroundColor="rgb(from var(--ess_SurfaceContainer80Color) r g b / 0.3)",this.timeLine.notPlannedTxtColor=e.widget.subtitle,this.timeLine.overlapsTxtColor=e.widget.subtitle,this.timeLine.periodBorderColor="#84A5D1",this.timeLine.periodBackgroundColor="#84A5D1",this.timeLine.periodBoxShadow=e.widget.infoBlockBackground,this.timeLine.disabledBorderColor="var(--ess_Supporting20Color)",this.timeLine.beginMonthBorderColor=e.desktop.menu.markColor,this.timeLine.textColor=e.widget.tag.color,this.timeLine.borderColor="var(--ess_Supporting20Color)",this.timeLine.boxShadow="",this.timeLine.backgroundColor=e.widget.background,this.timeLine.unavailableColor="rgb(from var(--ess_SurfaceContainer80Color) r g b / 0.3)",this.timeLine.unavailableFirstDay="var(--ess_SurfaceContainer60Color)",this.employee.fullNameColor=e.widget.tag.color,this.employee.jobTitleColor=e.widget.subtitle,this.color=e.widget.tag.color,this.font.family=e.font.fontFamily,this.font.style="",this.marginTop="48px",this.fillLKSize(t),t?(this.font.primarySize=e.typography.desktop.primary.fontSize,this.font.secondarySize=e.typography.desktop.heading4.fontSize,this.font.regularWeight=e.typography.desktop.primary.fontWeight,this.font.mediumWeight=e.typography.desktop.primaryStrong.fontWeight,this.font.primaryLineHeight=e.typography.desktop.primary.lineHeight,this.font.secondaryLineHeight=e.typography.desktop.heading4.lineHeight,this.DAYS_IN_MONTHS_FOR_VIEW=4,this.MONTHS_IN_YEAR_FOR_VIEW=3,this.DAY_NUMBER_FOR_SCROLL=2):(this.font.primarySize=e.typography.desktop.primaryStrong.fontSize,this.font.secondarySize=e.typography.desktop.heading4.fontSize,this.font.regularWeight=e.typography.desktop.primary.fontWeight,this.font.mediumWeight=e.typography.desktop.primaryStrong.fontWeight,this.font.primaryLineHeight=e.typography.desktop.primary.lineHeight,this.font.secondaryLineHeight=e.typography.desktop.heading4.lineHeight)};fillSize=()=>{this.size.timelineWidth=1200,this.size.groupWidth=300,this.size.columnMonthWidth=(this.size.timelineWidth-this.size.groupWidth)/this.DAYS_IN_MONTHS_FOR_VIEW,this.size.columnYearWidth=(this.size.timelineWidth-this.size.groupWidth)/this.MONTHS_IN_YEAR_FOR_VIEW,this.size.columnHeight=48,this.size.overlapsHeight=32,this.size.headerFullHeight=this.size.columnHeight+this.size.overlapsHeight,this.size.maxTextWidth="230px"};fillLKSize=e=>{e?(this.size.timelineWidth=343,this.size.groupWidth=171.5,this.size.maxTextWidth="130px"):(this.size.timelineWidth=1316,this.size.groupWidth=277,this.size.maxTextWidth="230px"),this.size.columnHeight=56,this.size.overlapsHeight=40,this.size.headerFullHeight=this.size.columnHeight+this.size.overlapsHeight,this.size.columnMonthWidth=(this.size.timelineWidth-this.size.groupWidth)/this.DAYS_IN_MONTHS_FOR_VIEW,this.size.columnYearWidth=(this.size.timelineWidth-this.size.groupWidth)/this.MONTHS_IN_YEAR_FOR_VIEW}}class aa{constructor(){this.textColor="",this.selectedTextColor="",this.backgroundColor="",this.hoveredBackgroundColor=""}}class ia{constructor(){this.holidayColor="",this.topColor="",this.baseBodyBackgroundColor="",this.groupBackgroundColor="",this.notPlannedTxtColor="",this.overlapsTxtColor="",this.periodBorderColor="",this.periodBackgroundColor="",this.periodBoxShadow="",this.disabledBorderColor="",this.beginMonthBorderColor="",this.textColor="",this.borderColor="",this.boxShadow="",this.backgroundColor="",this.unavailableColor="",this.unavailableFirstDay=""}}class na{constructor(){this.fullNameColor="",this.jobTitleColor=""}}class oa{constructor(){this.timelineWidth=0,this.groupWidth=0,this.columnMonthWidth=0,this.columnYearWidth=0,this.columnHeight=0,this.overlapsHeight=0,this.headerFullHeight=0,this.maxTextWidth="",this.toggleButtonHeight="24px",this.toggleButtonMargin="1px",this.monthNavigatorMarginBottom="8px"}}class sa{constructor(){this.family="",this.primarySize="",this.secondarySize="12px",this.regularWeight="",this.mediumWeight="",this.primaryLineHeight="",this.secondaryLineHeight="",this.style=""}}class ra{ESS_FUNCTIONS_PATH="/api/functions";DEFAULT_NAMESPACE="Vacations";id=0;system=A.LK;constructor(e){this.api=e.api,this.context=e.initialContext,this.requestHeaders=new Headers,this.requestHeaders.set("Accept","application.json"),this.requestHeaders.set("Content-Type","application/json"),this.isMobile=this.context.isMobile,this.language=this.context?.language||g,this.getId(),this.theme=new ta,this.fillTheme(),this.container=e.container,this.userId=this.api.items.getValues().PerformerId}get isDarkTheme(){return!1}fillTheme=()=>{this.theme.fillLKTheme(this.context.theme,this.isMobile)};get=async e=>{e=e.replace(/'/g,"");return await ESS.network.sendRequest(`${this.ESS_FUNCTIONS_PATH}/${e}`)};post=async(e,t)=>await this.doRequest(e,"POST",t);doRequest=async(e,t,a)=>{const i={method:t,headers:this.requestHeaders,body:JSON.stringify(a)};return await ESS.network.sendRequest(`${this.ESS_FUNCTIONS_PATH}/${e}`,i)};startAssigment=async e=>new Promise((()=>{}));getId=()=>{this.id=this.api.items?Number(this.api.items.getValues().AssignmentId):Number(this.api.getValues().AssignmentId)};getCallContext=()=>this.api.items.getValues().EntityGuid}const la=e=>{const t=new ra(e),a=(0,s.createRoot)(e.container);return e.container.setAttribute("id",y),a.render(w().createElement(o.Kq,{store:Ke},w().createElement(ea,{adapter:t,dialogId:e["dialog-id"]}))),Promise.resolve((()=>a.unmount()))};var ca=a(53525);class da{INTEGRATION_SERVICE_PATH="/Integration/odata";DEFAULT_NAMESPACE="Vacations";id=0;system=A.RX;constructor(e){this.api=e.api,this.context=e.initialContext,this.requestHeaders=new Headers,this.requestHeaders.set("Accept","application.json"),this.requestHeaders.set("Content-Type","application/json"),this.isMobile=!1,this.language=e.initialContext.currentCulture||g,this.getId(),this.theme=new ta,this.fillTheme(),this.container=e.container,this.userId=this.context.userId}get isDarkTheme(){return this.context.theme===ca.Theme.Night}fillTheme=()=>{this.isDarkTheme?this.theme.fillDarkTheme():this.theme.fillDefaultTheme()};get=async(e,t=this.DEFAULT_NAMESPACE)=>{let a=e.includes("?")?e.replace("?","(").replace(/&/g,","):`${e}(`;a+=")";const i={headers:this.requestHeaders},n=await fetch(`${this.INTEGRATION_SERVICE_PATH}/${t}/${a}`,i),o=await n.json();return void 0!==o.value?o.value:o};post=async(e,t,a=this.DEFAULT_NAMESPACE)=>this.doRequest(e,"POST",t,a);doRequest=async(e,t,a,i=this.DEFAULT_NAMESPACE)=>{const n={method:t,headers:this.requestHeaders,body:JSON.stringify(a)},o=await fetch(`${this.INTEGRATION_SERVICE_PATH}/${i}/${e}`,n),s=await o.json();return s.value?s.value:s};startAssigment=async e=>this.api.executeAction(e);getId=()=>{this.id=this.api.getEntity().Id};getCallContext=()=>this.api.getEntity().Info.typeId}const ma=e=>{const t=new da(e),a=(0,s.createRoot)(e.container);return a.render(w().createElement(o.Kq,{store:Ke},w().createElement(ea,{adapter:t}))),Promise.resolve((()=>a.unmount()))},ua={"rx-loader":n,"lk-loader":i}},35358:(e,t,a)=>{var i={"./af":25177,"./af.js":25177,"./ar":61509,"./ar-dz":41488,"./ar-dz.js":41488,"./ar-kw":58676,"./ar-kw.js":58676,"./ar-ly":42353,"./ar-ly.js":42353,"./ar-ma":24496,"./ar-ma.js":24496,"./ar-ps":6947,"./ar-ps.js":6947,"./ar-sa":82682,"./ar-sa.js":82682,"./ar-tn":89756,"./ar-tn.js":89756,"./ar.js":61509,"./az":95533,"./az.js":95533,"./be":28959,"./be.js":28959,"./bg":47777,"./bg.js":47777,"./bm":54903,"./bm.js":54903,"./bn":61290,"./bn-bd":17357,"./bn-bd.js":17357,"./bn.js":61290,"./bo":31545,"./bo.js":31545,"./br":11470,"./br.js":11470,"./bs":44429,"./bs.js":44429,"./ca":7306,"./ca.js":7306,"./cs":56464,"./cs.js":56464,"./cv":73635,"./cv.js":73635,"./cy":64226,"./cy.js":64226,"./da":93601,"./da.js":93601,"./de":77853,"./de-at":26111,"./de-at.js":26111,"./de-ch":54697,"./de-ch.js":54697,"./de.js":77853,"./dv":60708,"./dv.js":60708,"./el":54691,"./el.js":54691,"./en-au":53872,"./en-au.js":53872,"./en-ca":28298,"./en-ca.js":28298,"./en-gb":56195,"./en-gb.js":56195,"./en-ie":66584,"./en-ie.js":66584,"./en-il":65543,"./en-il.js":65543,"./en-in":9033,"./en-in.js":9033,"./en-nz":79402,"./en-nz.js":79402,"./en-sg":43004,"./en-sg.js":43004,"./eo":32934,"./eo.js":32934,"./es":97650,"./es-do":20838,"./es-do.js":20838,"./es-mx":17730,"./es-mx.js":17730,"./es-us":56575,"./es-us.js":56575,"./es.js":97650,"./et":3035,"./et.js":3035,"./eu":3508,"./eu.js":3508,"./fa":119,"./fa.js":119,"./fi":90527,"./fi.js":90527,"./fil":95995,"./fil.js":95995,"./fo":52477,"./fo.js":52477,"./fr":85498,"./fr-ca":26435,"./fr-ca.js":26435,"./fr-ch":37892,"./fr-ch.js":37892,"./fr.js":85498,"./fy":37071,"./fy.js":37071,"./ga":41734,"./ga.js":41734,"./gd":70217,"./gd.js":70217,"./gl":77329,"./gl.js":77329,"./gom-deva":32124,"./gom-deva.js":32124,"./gom-latn":93383,"./gom-latn.js":93383,"./gu":95050,"./gu.js":95050,"./he":11713,"./he.js":11713,"./hi":43861,"./hi.js":43861,"./hr":26308,"./hr.js":26308,"./hu":90609,"./hu.js":90609,"./hy-am":17160,"./hy-am.js":17160,"./id":74063,"./id.js":74063,"./is":89374,"./is.js":89374,"./it":88383,"./it-ch":21827,"./it-ch.js":21827,"./it.js":88383,"./ja":23827,"./ja.js":23827,"./jv":89722,"./jv.js":89722,"./ka":41794,"./ka.js":41794,"./kk":27088,"./kk.js":27088,"./km":96870,"./km.js":96870,"./kn":84451,"./kn.js":84451,"./ko":63164,"./ko.js":63164,"./ku":98174,"./ku-kmr":6181,"./ku-kmr.js":6181,"./ku.js":98174,"./ky":78474,"./ky.js":78474,"./lb":79680,"./lb.js":79680,"./lo":15867,"./lo.js":15867,"./lt":45766,"./lt.js":45766,"./lv":69532,"./lv.js":69532,"./me":58076,"./me.js":58076,"./mi":41848,"./mi.js":41848,"./mk":30306,"./mk.js":30306,"./ml":73739,"./ml.js":73739,"./mn":99053,"./mn.js":99053,"./mr":86169,"./mr.js":86169,"./ms":73386,"./ms-my":92297,"./ms-my.js":92297,"./ms.js":73386,"./mt":77075,"./mt.js":77075,"./my":72264,"./my.js":72264,"./nb":22274,"./nb.js":22274,"./ne":8235,"./ne.js":8235,"./nl":92572,"./nl-be":43784,"./nl-be.js":43784,"./nl.js":92572,"./nn":54566,"./nn.js":54566,"./oc-lnc":69330,"./oc-lnc.js":69330,"./pa-in":29849,"./pa-in.js":29849,"./pl":94418,"./pl.js":94418,"./pt":79834,"./pt-br":48303,"./pt-br.js":48303,"./pt.js":79834,"./ro":24457,"./ro.js":24457,"./ru":82271,"./ru.js":82271,"./sd":1221,"./sd.js":1221,"./se":33478,"./se.js":33478,"./si":17538,"./si.js":17538,"./sk":5784,"./sk.js":5784,"./sl":46637,"./sl.js":46637,"./sq":86794,"./sq.js":86794,"./sr":45719,"./sr-cyrl":3322,"./sr-cyrl.js":3322,"./sr.js":45719,"./ss":56e3,"./ss.js":56e3,"./sv":41011,"./sv.js":41011,"./sw":40748,"./sw.js":40748,"./ta":11025,"./ta.js":11025,"./te":11885,"./te.js":11885,"./tet":28861,"./tet.js":28861,"./tg":86571,"./tg.js":86571,"./th":55802,"./th.js":55802,"./tk":59527,"./tk.js":59527,"./tl-ph":29231,"./tl-ph.js":29231,"./tlh":31052,"./tlh.js":31052,"./tr":85096,"./tr.js":85096,"./tzl":79846,"./tzl.js":79846,"./tzm":81765,"./tzm-latn":97711,"./tzm-latn.js":97711,"./tzm.js":81765,"./ug-cn":48414,"./ug-cn.js":48414,"./uk":16618,"./uk.js":16618,"./ur":57777,"./ur.js":57777,"./uz":57609,"./uz-latn":72475,"./uz-latn.js":72475,"./uz.js":57609,"./vi":21135,"./vi.js":21135,"./x-pseudo":64051,"./x-pseudo.js":64051,"./yo":82218,"./yo.js":82218,"./zh-cn":52648,"./zh-cn.js":52648,"./zh-hk":1632,"./zh-hk.js":1632,"./zh-mo":31541,"./zh-mo.js":31541,"./zh-tw":50304,"./zh-tw.js":50304};function n(e){var t=o(e);return a(t)}function o(e){if(!a.o(i,e)){var t=new Error("Cannot find module '"+e+"'");throw t.code="MODULE_NOT_FOUND",t}return i[e]}n.keys=function(){return Object.keys(i)},n.resolve=o,e.exports=n,n.id=35358}}]);
//# sourceMappingURL=992_1.0_a196b7fa.js.map