const fs = require("fs");
const path = require("path");
const sharp = require("sharp");

const out = __dirname;
const colors = {
  saffron: "#ff671f",
  saffronDark: "#e85d0c",
  green: "#138808",
  blue: "#0d6efd",
  cyan: "#18b8c8",
  ink: "#111827",
  text: "#374151",
  muted: "#64748b",
  soft: "#f5f7fb",
  line: "#e5e7eb",
  white: "#ffffff",
  navy: "#172033",
  adminDark: "#343a40"
};

function esc(text) {
  return String(text).replace(/[&<>"]/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));
}

function svg(width, height, body) {
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="0 0 ${width} ${height}">
  <defs>
    <filter id="shadow" x="-20%" y="-20%" width="140%" height="140%"><feDropShadow dx="0" dy="12" stdDeviation="14" flood-color="#0f172a" flood-opacity=".14"/></filter>
    <linearGradient id="hero" x1="0" x2="1"><stop offset="0" stop-color="#ffffff"/><stop offset=".58" stop-color="#fff7f0"/><stop offset="1" stop-color="#effaf0"/></linearGradient>
    <linearGradient id="chart" x1="0" x2="0" y1="0" y2="1"><stop offset="0" stop-color="${colors.blue}"/><stop offset="1" stop-color="${colors.cyan}"/></linearGradient>
  </defs>
  <style>
    .font{font-family:Poppins,Roboto,Arial,sans-serif}.h{font-weight:900;fill:${colors.ink}}.t{fill:${colors.text}}.m{fill:${colors.muted}}.w{fill:#fff}.small{font-size:13px}.xs{font-size:11px}.tag{fill:${colors.saffronDark};font-size:11px;font-weight:900}.label{fill:#fff;font-size:10px;font-weight:900}.dash{fill:none;stroke:${colors.saffron};stroke-width:2;stroke-dasharray:7 5}.card{fill:#fff;stroke:${colors.line};rx:8;filter:url(#shadow)}
  </style>${body}</svg>`;
}

function text(x, y, value, size = 14, cls = "t", weight = 600) {
  return `<text class="font ${cls}" x="${x}" y="${y}" font-size="${size}" font-weight="${weight}">${esc(value)}</text>`;
}

function multiline(x, y, lines, size = 14, cls = "m", gap = 22) {
  return lines.map((line, i) => text(x, y + i * gap, line, size, cls, 500)).join("");
}

function tag(x, y, value) {
  return `<rect x="${x}" y="${y - 20}" width="${Math.max(120, value.length * 7.2)}" height="28" rx="6" fill="#fff7f0" stroke="#ffd1b8"/>${text(x + 10, y - 1, value, 11, "tag", 900)}`;
}

function annotate(x, y, w, h, label) {
  return `<rect class="dash" x="${x}" y="${y}" width="${w}" height="${h}" rx="8"/><rect x="${x + w - Math.max(96, label.length * 6.4) - 10}" y="${y - 14}" width="${Math.max(96, label.length * 6.4)}" height="22" rx="5" fill="${colors.saffron}"/>${text(x + w - Math.max(96, label.length * 6.4), y + 2, label, 10, "label", 900)}`;
}

function card(x, y, w, h) {
  return `<rect class="card" x="${x}" y="${y}" width="${w}" height="${h}"/>`;
}

function publicDesktop() {
  let b = `<rect width="1440" height="1850" fill="${colors.soft}"/>
  <rect width="1440" height="54" fill="#0f172a"/>${text(28, 34, "Public Website Homepage - Desktop", 18, "w", 900)}${text(1090, 34, "Annotated responsive visual, 1440px", 13, "m", 700)}
  <rect x="0" y="54" width="1440" height="120" fill="#fff"/><rect x="0" y="54" width="1440" height="38" fill="${colors.ink}"/>
  ${text(72, 78, "Helpline: +91 00000 00000 | Assembly Constituency Placeholder", 12, "w", 700)}${text(1115, 78, "English | Hindi | Search | Social Links", 12, "w", 700)}
  <circle cx="96" cy="132" r="24" fill="${colors.saffron}"/><circle cx="96" cy="132" r="13" fill="${colors.green}"/>${text(132, 126, "Leader Name Campaign", 17, "h", 900)}${text(132, 146, "ELECTION MANAGEMENT PORTAL", 11, "m", 800)}
  ${["Home","About Leader","Mera Kshetra","Events","Gallery","Citizen Connect"].map((n,i)=>text(390+i*115,136,n,14,"h",800)).join("")}
  <rect x="1180" y="112" width="102" height="42" rx="6" fill="#fff" stroke="#ffd1b8"/>${text(1204,138,"Manifesto",13,"tag",900)}
  <rect x="1292" y="112" width="118" height="42" rx="6" fill="${colors.saffron}"/>${text(1310,138,"Join Volunteer",13,"w",900)}
  ${annotate(18, 66, 1404, 102, "Header + navbar")}
  <rect x="0" y="174" width="1440" height="626" fill="url(#hero)"/>
  ${tag(72,245,"Section 4: Hero Slider")}${text(72,320,"Leader Name for",56,"h",900)}${text(72,382,"a Stronger",56,"h",900)}${text(72,444,"Constituency",56,"h",900)}
  ${multiline(72,490,["Clear campaign message placeholder for development, public service,","voter connection, booth coverage, and transparent communication."],17,"t",30)}
  <rect x="72" y="555" width="138" height="44" rx="6" fill="${colors.saffron}"/>${text(95,583,"Volunteer Now",13,"w",900)}
  <rect x="224" y="555" width="130" height="44" rx="6" fill="#fff" stroke="#ffd1b8"/>${text(246,583,"Share Feedback",13,"tag",900)}
  ${[["245","Booths Covered"],["18K","Volunteer Network"],["92","Upcoming Meetings"]].map((s,i)=>`${card(72+i*176,630,158,92)}${text(94+i*176,675,s[0],26,"h",900)}${text(94+i*176,700,s[1],12,"m",800)}`).join("")}
  <rect x="810" y="255" width="462" height="488" rx="8" fill="#ffefe5" stroke="#ffe2cf"/><circle cx="1041" cy="485" r="150" fill="#ecfdf3"/><rect x="972" y="342" width="138" height="310" rx="60" fill="${colors.navy}"/><circle cx="1041" cy="302" r="58" fill="#ffd7bd"/><rect x="790" y="632" width="228" height="86" rx="8" fill="#fff" filter="url(#shadow)"/><rect x="790" y="632" width="5" height="86" fill="${colors.green}"/>${text(808,670,"Today: 6 Jan Sampark visits",17,"h",900)}${text(808,694,"Leader schedule and public meeting cue",12,"m",700)}
  ${annotate(42, 198, 1356, 560, "Hero + leader identity")}
  <rect x="0" y="800" width="1440" height="52" fill="#fff" stroke="${colors.line}"/>${text(72,833,"Latest Update",14,"tag",900)}${text(190,833,"Public meeting at Village Placeholder | New manifesto uploaded | Booth worker training starts tomorrow",13,"m",800)}${annotate(18,812,1404,30,"News ticker")}
  <rect x="0" y="852" width="1440" height="330" fill="#fff"/>${tag(72,925,"Section 3: Leader Introduction")}${text(72,974,"Service, development, and",34,"h",900)}${text(72,1014,"accountable leadership",34,"h",900)}${multiline(850,950,["Compact, CMS-managed introduction blocks","show biography, vision, achievements,","and constituency priorities."],14,"m",24)}
  ${[["V","Vision & Mission"],["A","Achievements"],["C","Citizen Connect"]].map((s,i)=>`${card(72+i*432,1025,400,150)}<rect x="${96+i*432}" y="1050" width="42" height="42" rx="8" fill="#eefaf0"/>${text(111+i*432,1077,s[0],18,"tag",900)}${text(96+i*432,1120,s[1],18,"h",900)}${text(96+i*432,1148,"CMS placeholder for public content and calls to action.",13,"m",600)}`).join("")}${annotate(42,880,1356,276,"Leader intro + vision")}
  <rect x="0" y="1182" width="1440" height="384" fill="#f8fafc"/>${tag(72,1250,"Section 10: Campaign Modules")}${text(72,1298,"Upcoming events and public pulse",34,"h",900)}
  ${[["24 JUL","Booth Committee Meeting"],["25 JUL","Women Outreach Sabha"],["26 JUL","Youth Volunteer Training"]].map((s,i)=>`${card(72,1340+i*72,760,58)}<rect x="92" y="${1352+i*72}" width="62" height="34" rx="6" fill="#fff7f0"/>${text(102,1375+i*72,s[0],12,"tag",900)}${text(174,1364+i*72,s[1],16,"h",900)}${text(174,1386+i*72,"Location, time, owner, attendance and media team cues",12,"m",700)}<rect x="742" y="${1352+i*72}" width="70" height="34" rx="6" fill="#fff" stroke="#ffd1b8"/>${text(760,1375+i*72,"Details",12,"tag",900)}`).join("")}
  ${card(872,1338,430,210)}${tag(894,1384,"Survey / Poll")}${text(894,1422,"Top campaign focus this week?",21,"h",900)}${["Employment 42%","Roads 27%","Farmers 19%"].map((s,i)=>`<rect x="894" y="${1445+i*42}" width="360" height="34" rx="6" fill="${i?'#fff':'#effdf3'}" stroke="${i?colors.line:'#c9f0cf'}"/>${text(910,1468+i*42,s,13,i?"h":"tag",800)}`).join("")}${annotate(42,1215,1356,318,"Events + poll widget")}
  <rect x="0" y="1566" width="1440" height="284" fill="#fff"/>${tag(72,1630,"Gallery / Media / CMS reorder")}${text(72,1678,"Campaign gallery and community action",34,"h",900)}
  ${[0,1,2,3].map(i=>`<rect x="${72+i*318}" y="1710" width="292" height="110" rx="8" fill="${[colors.saffron,colors.green,colors.blue,"#f5b301"][i]}"/>${text(96+i*318,1782,["Rally Moments","Village Visit","Media Coverage","Volunteer Work"][i],17,"w",900)}`).join("")}${annotate(42,1590,1356,236,"Gallery + CTA + reorder cue")}`;
  return svg(1440,1850,b);
}

function adminDesktop() {
  let b = `<rect width="1440" height="1120" fill="#f4f6f9"/><rect width="1440" height="54" fill="#0f172a"/>${text(28,34,"Admin Dashboard - Desktop",18,"w",900)}${text(1010,34,"Annotated AdminLTE enterprise command center",13,"m",700)}
  <rect x="0" y="54" width="270" height="1066" fill="${colors.adminDark}"/>${text(58,94,"Campaign Admin",15,"w",900)}${text(58,113,"Election War Room",11,"m",700)}<circle cx="31" cy="98" r="18" fill="${colors.blue}"/>
  <rect x="12" y="132" width="246" height="64" rx="8" fill="#414950"/><circle cx="44" cy="164" r="20" fill="${colors.cyan}"/>${text(75,160,"Campaign Manager",13,"w",900)}${text(75,179,"Role: Admin",11,"m",700)}
  ${["Dashboard","Constituency & Booths","Events & Rallies","Volunteers & Teams","Tasks & Follow-up","Surveys & Polls","Content CMS","Media Gallery","Expenses","Reports & Analytics","Settings & Roles"].map((n,i)=>`<rect x="8" y="${230+i*48}" width="254" height="42" rx="6" fill="${i===0?colors.blue:'transparent'}"/>${text(44,257+i*48,n,13,i===0?"w":"m",800)}${text(22,257+i*48,["▣","◎","◷","♟","☑","✉","▤","◉","₹","⚑","⚙"][i],14,i===0?"w":"m",800)}`).join("")}${annotate(8,70,254,650,"Role-based sidebar")}
  <rect x="270" y="54" width="1170" height="64" fill="#fff" stroke="#dee2e6"/>${text(328,93,"War Room Live Summary",16,"h",900)}<rect x="1120" y="72" width="112" height="36" rx="6" fill="#fff" stroke="#ffd1b8"/>${text(1148,95,"Website",12,"tag",900)}<rect x="1244" y="72" width="116" height="36" rx="6" fill="${colors.saffron}"/>${text(1264,95,"+ New Event",12,"w",900)}${annotate(286,64,1128,44,"Top bar")}
  <rect x="298" y="146" width="1114" height="84" fill="transparent"/>${tag(298,178,"Section 22: Admin Dashboard")}${text(298,216,"Election Campaign Command Center",30,"h",900)}${text(298,242,"Real-time view of booths, volunteers, public engagement, events, and campaign alerts.",14,"m",600)}${annotate(286,136,1128,110,"Page title + actions")}
  ${[["245 / 312","Booths Covered",colors.blue],["18,420","Total Volunteers",colors.saffron],["92","Upcoming Events",colors.green],["31","Open Alerts","#dc3545"]].map((s,i)=>`${card(298+i*276,270,252,138)}<rect x="${318+i*276}" y="290" width="44" height="44" rx="8" fill="${s[2]}"/>${text(378+i*276,322,["B","V","E","!"][i],20,"m",900)}${text(318+i*276,372,s[0],28,"h",900)}${text(318+i*276,394,s[1],12,"m",800)}`).join("")}${annotate(286,256,1128,166,"Key metrics")}
  ${card(298,440,720,352)}${tag(322,486,"Booth Activity Analytics")}${text(322,526,"Volunteer activity by zone",22,"h",900)}${[62,78,48,86,70,56].map((h,i)=>`<rect x="${335+i*104}" y="${720-h*2}" width="58" height="${h*2}" rx="6" fill="url(#chart)"/>${text(332+i*104,752,["North","South","East","West","Rural","Urban"][i],11,"m",800)}`).join("")}${annotate(286,428,744,382,"Analytics chart")}
  ${card(1042,440,370,352)}${tag(1066,486,"Fast workflow")}${text(1066,526,"Quick actions",22,"h",900)}${["Add Volunteer","Schedule Event","Assign Booth Task","Publish Poll","Update Homepage","Generate Report"].map((n,i)=>`<rect x="${1066+(i%2)*162}" y="${560+Math.floor(i/2)*74}" width="144" height="58" rx="8" fill="#f8fafc" stroke="${colors.line}"/>${text(1082+(i%2)*162,596+Math.floor(i/2)*74,n,12,"h",900)}`).join("")}${annotate(1030,428,394,382,"Quick actions")}
  ${card(298,830,535,250)}${tag(322,876,"Today schedule")}${text(322,916,"Upcoming events",21,"h",900)}${["Booth Meeting | Ward 12 | Ready","Road Show | Main Bazaar | Vehicle pending","Youth Training | Block Office | Attendance on"].map((n,i)=>`${text(322,958+i*38,n,13,"t",700)}<line x1="322" y1="${972+i*38}" x2="800" y2="${972+i*38}" stroke="${colors.line}"/>`).join("")}${annotate(286,818,560,276,"Operational table")}
  ${card(866,830,546,250)}${tag(890,876,"Campaign alerts")}${text(890,916,"Priority follow-ups",21,"h",900)}${["Low volunteer coverage | Booth 089 | Today","Survey response spike | Village A | Monitor","Expense approval needed | Event Team | 2 hrs"].map((n,i)=>`${text(890,958+i*38,n,13,"t",700)}<line x1="890" y1="${972+i*38}" x2="1360" y2="${972+i*38}" stroke="${colors.line}"/>`).join("")}${annotate(854,818,570,276,"Alerts + reports")}`;
  return svg(1440,1120,b);
}

function mobile(name, title, admin = false) {
  let b = `<rect width="430" height="1180" fill="${admin ? "#f4f6f9" : "#fff"}"/><rect width="430" height="54" fill="#0f172a"/>${text(18,34,title,16,"w",900)}`;
  if (!admin) {
    b += `<rect y="54" width="430" height="104" fill="#fff"/><rect y="54" width="430" height="34" fill="${colors.ink}"/>${text(16,76,"Helpline + Language",10,"w",700)}${text(360,76,"Search",10,"w",700)}<circle cx="38" cy="123" r="20" fill="${colors.saffron}"/>${text(70,119,"Leader Campaign",13,"h",900)}${text(70,136,"PUBLIC WEBSITE",9,"m",800)}${text(386,130,"☰",24,"h",900)}${annotate(10,90,410,62,"Mobile nav")}
    <rect y="158" width="430" height="476" fill="url(#hero)"/>${tag(16,206,"Hero")}${text(16,256,"Leader Name for",32,"h",900)}${text(16,296,"a Stronger",32,"h",900)}${text(16,336,"Constituency",32,"h",900)}${multiline(16,374,["Mobile-first message with volunteer, feedback,","and event actions kept visible."],14,"t",22)}<rect x="16" y="424" width="398" height="42" rx="6" fill="${colors.saffron}"/>${text(162,451,"Volunteer Now",13,"w",900)}<rect x="16" y="482" width="190" height="70" rx="8" fill="#fff" stroke="${colors.line}"/>${text(38,514,"245",25,"h",900)}${text(38,538,"Booths",12,"m",800)}<rect x="224" y="482" width="190" height="70" rx="8" fill="#fff" stroke="${colors.line}"/>${text(246,514,"18K",25,"h",900)}${text(246,538,"Volunteers",12,"m",800)}<rect x="92" y="568" width="246" height="58" rx="8" fill="#ffefe5" stroke="#ffe2cf"/>${text(145,602,"Leader Image Placeholder",13,"m",800)}${annotate(10,170,410,456,"Stacked hero")}
    <rect y="634" width="430" height="48" fill="#fff" stroke="${colors.line}"/>${text(16,664,"Latest",13,"tag",900)}${text(78,664,"Meeting at Village Placeholder",12,"m",800)}
    ${tag(16,730,"Intro")}${text(16,778,"Service and accountable",25,"h",900)}${text(16,808,"leadership",25,"h",900)}${card(16,840,398,110)}${text(40,885,"Vision & Mission",18,"h",900)}${text(40,914,"Priorities and public development agenda.",13,"m",600)}${card(16,978,398,150)}${tag(40,1024,"Poll")}${text(40,1062,"Top focus this week?",20,"h",900)}<rect x="40" y="1082" width="340" height="34" rx="6" fill="#effdf3" stroke="#c9f0cf"/>${text(56,1105,"Employment 42%",13,"tag",800)}${annotate(10,704,410,440,"Mobile sections")}`;
  } else {
    b += `<rect y="54" width="430" height="46" fill="${colors.adminDark}"/>${text(16,84,"☰ Campaign Admin / Role: Manager",14,"w",900)}${annotate(8,62,414,30,"Collapsed sidebar")}
    <rect y="100" width="430" height="74" fill="#fff" stroke="#dee2e6"/><rect x="16" y="116" width="80" height="36" rx="6" fill="#fff" stroke="#ffd1b8"/>${text(38,139,"Hindi",12,"tag",900)}<rect x="292" y="116" width="112" height="36" rx="6" fill="${colors.saffron}"/>${text(318,139,"+ Task",12,"w",900)}${annotate(8,108,414,50,"Mobile top bar")}
    ${tag(16,226,"Admin Dashboard")}${text(16,272,"Campaign Command",24,"h",900)}${text(16,302,"Center",24,"h",900)}${text(16,332,"Stacked metrics, actions, alerts, and tables for field operators.",13,"m",600)}${annotate(10,200,410,150,"Mobile page title")}
    ${card(16,382,398,126)}<rect x="38" y="405" width="44" height="44" rx="8" fill="${colors.blue}"/>${text(100,436,"245 / 312",28,"h",900)}${text(100,466,"Booths Covered",12,"m",800)}${card(16,530,398,126)}<rect x="38" y="553" width="44" height="44" rx="8" fill="${colors.saffron}"/>${text(100,584,"18,420",28,"h",900)}${text(100,614,"Total Volunteers",12,"m",800)}${annotate(10,370,410,300,"Stacked metrics")}
    ${card(16,704,398,320)}${tag(40,750,"Fast workflow")}${text(40,790,"Quick actions",22,"h",900)}${["Add Volunteer","Schedule Event","Assign Booth Task"].map((n,i)=>`<rect x="40" y="${828+i*58}" width="340" height="44" rx="8" fill="#f8fafc" stroke="${colors.line}"/>${text(60,856+i*58,n,13,"h",900)}`).join("")}${annotate(10,692,410,346,"Mobile quick actions")}`;
  }
  return svg(430,1180,b);
}

async function write(name, content) {
  const svgPath = path.join(out, `${name}.svg`);
  const pngPath = path.join(out, `${name}.png`);
  fs.writeFileSync(svgPath, content, "utf8");
  await sharp(Buffer.from(content)).png().toFile(pngPath);
}

(async () => {
  await write("public-homepage-desktop", publicDesktop());
  await write("admin-dashboard-desktop", adminDesktop());
  await write("public-homepage-mobile", mobile("public-homepage-mobile", "Public Homepage - Mobile"));
  await write("admin-dashboard-mobile", mobile("admin-dashboard-mobile", "Admin Dashboard - Mobile Cue", true));
})();
