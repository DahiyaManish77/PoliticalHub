(function(){
  'use strict';
  function initTabs(){document.querySelectorAll('[data-campaign-tab]').forEach(function(btn){btn.addEventListener('click',function(){var key=btn.getAttribute('data-campaign-tab');document.querySelectorAll('[data-campaign-tab]').forEach(function(x){x.classList.toggle('is-active',x===btn);});document.querySelectorAll('[data-campaign-panel]').forEach(function(x){x.classList.toggle('is-active',x.getAttribute('data-campaign-panel')===key);});});});}
  function initRails(){document.querySelectorAll('[data-campaign-carousel]').forEach(function(root){var rail=root.querySelector('.campaign-rail');if(!rail)return;var paused=false;var amount=function(){return Math.max(320,Math.round(rail.clientWidth*.72));};var prev=root.querySelector('.campaign-rail-prev');var next=root.querySelector('.campaign-rail-next');function move(direction){var max=rail.scrollWidth-rail.clientWidth;if(direction>0&&rail.scrollLeft>=max-8){rail.scrollTo({left:0,behavior:'smooth'});}else if(direction<0&&rail.scrollLeft<=8){rail.scrollTo({left:max,behavior:'smooth'});}else{rail.scrollBy({left:direction*amount(),behavior:'smooth'});}}if(prev)prev.addEventListener('click',function(){move(-1);});if(next)next.addEventListener('click',function(){move(1);});root.addEventListener('mouseenter',function(){paused=true;});root.addEventListener('mouseleave',function(){paused=false;});root.addEventListener('focusin',function(){paused=true;});root.addEventListener('focusout',function(){paused=false;});window.setInterval(function(){if(!paused&&!document.hidden&&rail.scrollWidth>rail.clientWidth){move(1);}},4200);});}
  function initHero(){if(typeof Swiper==='undefined'||!document.querySelector('.campaignHeroSwiper'))return;new Swiper('.campaignHeroSwiper',{loop:true,effect:'fade',speed:900,autoplay:{delay:6500,disableOnInteraction:false},pagination:{el:'.campaign-hero-pagination',clickable:true}});}
  function initShowcaseTracks(){document.querySelectorAll('[data-showcase-track][data-auto-scroll="true"]').forEach(function(viewport){var paused=false;var direction=1;viewport.addEventListener('mouseenter',function(){paused=true;});viewport.addEventListener('mouseleave',function(){paused=false;});viewport.addEventListener('focusin',function(){paused=true;});viewport.addEventListener('focusout',function(){paused=false;});window.setInterval(function(){if(paused||document.hidden)return;viewport.scrollLeft+=direction;if(viewport.scrollLeft>=viewport.scrollWidth-viewport.clientWidth-2){direction=-1;}else if(viewport.scrollLeft<=1){direction=1;}},28);});}
  document.addEventListener('DOMContentLoaded',function(){initTabs();initRails();initShowcaseTracks();initHero();});
})();

document.addEventListener('DOMContentLoaded', function () {
  var menuButton = document.querySelector('.campaign-mobile-menu');
  var menu = document.querySelector('.campaign-nav-links');
  if (menuButton && menu) {
    menuButton.addEventListener('click', function () {
      var open = menu.classList.toggle('open');
      menuButton.setAttribute('aria-expanded', open ? 'true' : 'false');
    });
  }
  var savedZoom = parseFloat(window.localStorage.getItem('portal-page-zoom') || '1');
  if (isNaN(savedZoom)) savedZoom = 1;
  document.body.style.zoom = savedZoom;
  document.querySelectorAll('[data-font-change]').forEach(function (button) {
    button.addEventListener('click', function () {
      var current = parseFloat(window.localStorage.getItem('portal-page-zoom') || '1');
      if (isNaN(current)) current = 1;
      current += button.getAttribute('data-font-change') === 'increase' ? 0.1 : -0.1;
      current = Math.max(0.8, Math.min(1.3, Math.round(current * 10) / 10));
      window.localStorage.setItem('portal-page-zoom', current.toString());
      document.body.style.zoom = current;
      button.setAttribute('aria-label', (button.getAttribute('data-font-change') === 'increase' ? 'Zoom in' : 'Zoom out') + '. Current zoom ' + Math.round(current * 100) + ' percent');
    });
  });
});
