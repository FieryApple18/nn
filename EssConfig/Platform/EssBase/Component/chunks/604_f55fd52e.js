"use strict";(self.webpackChunkreact_remote_component_example=self.webpackChunkreact_remote_component_example||[]).push([[604],{4768:(e,t,n)=>{n.r(t),n.d(t,{default:()=>Z});var r={};n.r(r),n.d(r,{default:()=>Y});var o=n(7581);const a=o.Ay.p`
  margin-bottom: 0;
`;var l=n(7115),c=n.n(l);const i=(0,l.createContext)(null),s=()=>(0,l.useContext)(i),E=(0,l.createContext)({isMobile:!1}),S=(0,l.createContext)({}),d=(0,l.createContext)({language:"ru"}),u=({theme:e,isMobile:t,icons:n,classes:r,language:a,children:s})=>{const u=(0,l.useMemo)((()=>({isMobile:t})),[t]),m=(0,l.useMemo)((()=>({language:a})),[a]);return c().createElement(o.NP,{theme:e},c().createElement(E.Provider,{value:u},c().createElement(S.Provider,{value:n},c().createElement(i.Provider,{value:r},c().createElement(d.Provider,{value:m},s)))))},m=({children:e,variant:t="primary",className:n=""})=>{const r=s(),o=t&&r?r.typography[t]:"";return c().createElement(a,{className:`${n} ${o}`},e)},p=({children:e,error:t})=>c().createElement(m,{variant:"secondary"},t||e);var T=n(5338);const A=o.Ay.div`${({theme:e})=>o.AH`
  min-height: ${e.skeleton.minHeight};
`}`,g=()=>{const e=s();return c().createElement(A,{className:e?.skeleton})};let x=function(e){return e[e.NOT_STARTED=0]="NOT_STARTED",e[e.COMPLETED=1]="COMPLETED",e[e.IN_PROCESS=2]="IN_PROCESS",e[e.ABORTED=3]="ABORTED",e[e.SUSPENDED=4]="SUSPENDED",e}({});const{getColor:f}=window.ESS,{Colors:C}=window.ESS.enums,v=o.Ay.div`
  display: flex;
  flex-direction: column;
  gap: 12px;
`,h=o.Ay.div`${({$state:e})=>o.AH`
  display: flex;
  gap: 6px;

  ${e!==x.IN_PROCESS&&o.AH`
    ${a} {
      color: ${f(C.TextSecondary50Color)};
    }
  `}

  ${v} {
    padding-bottom: 28px;
  }

  &:last-child ${v} {
    padding-bottom: 0;
  }
`}`,_=o.Ay.div`
  display: flex;
  flex-direction: column;
  gap: 4px;
`,k=(0,o.Ay)(m)`
  border-left: 1px solid ${f(C.Supporting20Color)};
  padding-left: 8px;
`,w=(0,o.Ay)(m)`${({$isExpired:e})=>o.AH`
  color: ${window.ESS.getColor(window.ESS.enums.Colors[e?"Danger50Color":"TextSecondary50Color"])};
`}`,{L:D}=window.ESS,$=e=>{const{text:t,isExpired:n}=(({completed:e,deadline:t,isExpired:n,state:r})=>{const o=(0,l.useCallback)((e=>window.converters.EssBase?.simplifyDate?.(e)||""),[]);return(0,l.useMemo)((()=>e?{text:D("@ESSBASE.TASK_SCHEME_DONE",o(e)),isExpired:!1}:t&&[x.NOT_STARTED,x.IN_PROCESS].includes(r)?{text:D("@ESSBASE.TASK_SCHEME_EXPECTED_DEADLINE",o(t)),isExpired:n}:{text:null,isExpired:!1}),[e,t,n,r,o])})(e);return t?c().createElement(w,{variant:"heading5",$isExpired:n},t):null},y=(0,o.Ay)(m)`${({$isDetails:e})=>o.AH`
  ${e&&`color: ${window.ESS.getColor(window.ESS.enums.Colors.TextSecondary50Color)}`}
`}`,O=({performerInfo:e})=>e?.length?c().createElement("div",null,e.filter(Boolean).map(((e,t)=>{const n=t>0;return c().createElement(y,{key:t,variant:n?"heading4":void 0,$isDetails:n},e)}))):null,b=o.Ay.div`
  display: flex;
  align-items: center;
  gap: 6px;
`;var N=n(7436);const P=(0,o.Ay)((({html:e,className:t})=>{const n=(0,N.Parser)().parse(e);return c().cloneElement(n,{className:t})}))`${({theme:e,$color:t=e.icon.color,$size:n=e.icon.size})=>o.AH`
  width: ${n};
  height: ${n};

  &, * {
    &[fill]:not([fill="none"]) {
      fill: ${t};
    }

    &[stroke]:not([stroke="none"]) {
      stroke: ${t};
    }
  }
`}`;function R(){return R=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var r in n)({}).hasOwnProperty.call(n,r)&&(e[r]=n[r])}return e},R.apply(null,arguments)}const M=({name:e,...t})=>{const n=(0,l.useContext)(S)[e];return n?c().createElement(P,R({},t,{html:n})):null},{L:I,getColor:L}=window.ESS,{Colors:H}=window.ESS.enums,B={[x.NOT_STARTED]:{icon:null,text:null,colorToken:null},[x.COMPLETED]:{icon:"check",text:"@ESSBASE.TASK_SCHEME_COMPLETED_STATE",colorToken:"Success50Color"},[x.IN_PROCESS]:{icon:"in-progress",text:"@ESSBASE.TASK_SCHEME_IN_PROCESS_STATE",colorToken:"Secondary50Color"},[x.ABORTED]:{icon:"discontinued",text:"@ESSBASE.TASK_SCHEME_ABORTED_STATE",colorToken:"Danger50Color"},[x.SUSPENDED]:{icon:"suspended",text:"@ESSBASE.TASK_SCHEME_SUSPENDED_STATE",colorToken:"Primary50Color"}},K=({state:e})=>{const{icon:t,text:n,color:r}=(e=>(0,l.useMemo)((()=>{const t=B[e];return{icon:t.icon,text:t.text&&I(t.text),color:t.colorToken?L(H[t.colorToken]):void 0}}),[e]))(e);return t&&n?c().createElement(b,null,c().createElement(M,{name:t,$color:r,$size:"16px"}),c().createElement(m,null,n)):null},U=(e,t)=>{const{step:n}=e.stepper,r={[x.NOT_STARTED]:n.default.background,[x.COMPLETED]:n.previous.background,[x.IN_PROCESS]:n.active.background,[x.ABORTED]:n.blocked.background,[x.SUSPENDED]:n.blocked.background};return void 0!==t?r[t]:"transparent"},j=o.Ay.div`
  position: relative;
  width: 8px;
  height: 8px;
  border-radius: 50%;
`,F=o.Ay.div`${({theme:e,$state:t,$prevState:n})=>o.AH`
  display: flex;
  align-items: center;
  flex-direction: column;
  flex-shrink: 0;
  overflow: hidden;

  &::before,
  &::after {
    content: '';
    display: block;
    width: 1px;
  }

  &::before {
    height: 6px;
    background: ${U(e,n)};
  }
  
  &::after {
    flex-grow: 1;
    background: ${U(e,t)};
  }

  ${j} {
    background: ${U(e,t)};
  }
`}`,z=e=>c().createElement(F,e,c().createElement(j,null)),G=e=>c().createElement(h,{$state:e.State},c().createElement(z,{$state:e.State,$prevState:e.prevState}),c().createElement(v,null,c().createElement(m,{variant:"primaryStrong"},e.Subject||window.ESS.L("@ESSBASE.TASK_SCHEME_STAGE")),c().createElement(K,{state:e.State}),e.WorkflowItemsInfo.map(((t,n)=>c().createElement(_,{key:n},c().createElement(O,{performerInfo:t.PerformerInfo}),t.Comment&&c().createElement(k,null,t.Comment),c().createElement($,{completed:t.Completed,deadline:t.DeadLine,isExpired:t.IsExpired,state:e.State}))))));var q=n(4177);function W(){return W=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var r in n)({}).hasOwnProperty.call(n,r)&&(e[r]=n[r])}return e},W.apply(null,arguments)}const X=e=>{const{data:t,isLoading:n}=((e,t)=>{const[n,r]=(0,l.useState)([]),[o,a]=(0,l.useState)(!0),{showBoundary:c}=(0,q.Md)();return(0,l.useEffect)((()=>{const n=t||"GetTaskScheme";window.ESS.network.sendRequest(`/api/functions/${n}?taskId=${e}`).then(r).catch((()=>c(window.ESS.L("#DEFAULT.ERR_NO_DATA")))).finally((()=>a(!1)))}),[e,c,t]),{data:n,isLoading:o}})(e.taskId,e.getTaskSchemeFunctionName);return n?c().createElement(g,null):t.length?c().createElement(c().Fragment,null,t.map(((e,n)=>c().createElement(G,W({key:n,prevState:t[n-1]?.State},e))))):c().createElement(m,{variant:"secondary"},window.ESS.L("#DEFAULT.NO_DATA"))},J=({initialContext:e,...t})=>c().createElement(u,e,c().createElement(q.tH,{fallbackRender:p},c().createElement(X,t)));function Q(){return Q=Object.assign?Object.assign.bind():function(e){for(var t=1;t<arguments.length;t++){var n=arguments[t];for(var r in n)({}).hasOwnProperty.call(n,r)&&(e[r]=n[r])}return e},Q.apply(null,arguments)}const{L:V}=window.ESS,Y=({container:e,...t})=>{const n=(0,T.createRoot)(e);let r=t["task-id"];return r||window.ESS.enums.Parent.CARD!==t.parent||(r=t.controlInfo.data.id),r?n.render(c().createElement(J,Q({},t,{taskId:r,getTaskSchemeFunctionName:t["get-task-scheme-function-name"]}))):(console.error(V("@ESSBASE.TASK_SCHEME_NO_TASK_ID_ERROR")),n.render(c().createElement(p,null,V("#DEFAULT.ERR_NO_DATA")))),Promise.resolve((()=>n.unmount()))},Z={"task-scheme":r}}}]);