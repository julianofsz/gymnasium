document.addEventListener("DOMContentLoaded", function() {
  // Carrossel de Cards
  const cardRow = document.getElementById('cardRow');
  const nextButton = document.getElementById('nextButton');
  const prevButton = document.getElementById('prevButton');

  if (cardRow && nextButton && prevButton) {
    let startX, scrollLeft;
    let scrollAmount = 0;
    const cardWidth = cardRow.firstElementChild ? cardRow.firstElementChild.clientWidth : 0; // Verifica se há elementos filhos
    const maxScroll = cardRow.scrollWidth - cardRow.clientWidth;

    const scrollToPosition = (amount) => {
      cardRow.scrollTo({
        left: amount,
        behavior: 'smooth',
      });
    };

    nextButton.addEventListener('click', () => {
      if (scrollAmount < maxScroll) {
        scrollAmount += cardWidth;
      } else {
        scrollAmount = 0;
      }
      scrollToPosition(scrollAmount);
    });

    prevButton.addEventListener('click', () => {
      if (scrollAmount > 0) {
        scrollAmount -= cardWidth;
      } else {
        scrollAmount = maxScroll;
      }
      scrollToPosition(scrollAmount);
    });

    cardRow.addEventListener('touchstart', (e) => {
      startX = e.touches[0].pageX - cardRow.offsetLeft;
      scrollLeft = cardRow.scrollLeft;
    });

    cardRow.addEventListener('touchmove', (e) => {
      if (!startX) return;
      e.preventDefault();
      const x = e.touches[0].pageX - cardRow.offsetLeft;
      const walk = (x - startX) * 2;
      cardRow.scrollLeft = scrollLeft - walk;
    });
  }

  // Animação do Navbar
  const header = document.getElementById('navbar');
  if (header) {
    window.addEventListener('scroll', () => {
      const scrollPosition = window.scrollY;
      if (scrollPosition > 50) {
        header.classList.add('scrolled');
      } else {
        header.classList.remove('scrolled');
      }
    });
  }

  // Controle de rolagem para Assinaturas
  const container = document.querySelector('.assinaturas-content');
  const leftButton = document.querySelector('.left-btn');
  const rightButton = document.querySelector('.right-btn');

  if (container && leftButton && rightButton) {
    const scrollAmount = 200;

    leftButton.addEventListener('click', () => {
      container.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    });

    rightButton.addEventListener('click', () => {
      container.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    });

    let isDown = false;
    let startX;
    let scrollLeft;

    container.addEventListener('mousedown', (e) => {
      isDown = true;
      startX = e.pageX - container.offsetLeft;
      scrollLeft = container.scrollLeft;
    });

    container.addEventListener('mouseleave', () => {
      isDown = false;
    });

    container.addEventListener('mouseup', () => {
      isDown = false;
    });

    container.addEventListener('mousemove', (e) => {
      if (!isDown) return;
      e.preventDefault();
      const x = e.pageX - container.offsetLeft;
      const walk = (x - startX) * 2;
      container.scrollLeft = scrollLeft - walk;
    });
  }
});

// Animação do Li da pagina de perfil
const listItems = document.querySelectorAll(".nav-profile-style .nav-text-color");

listItems.forEach(item => {
    item.addEventListener("click", () => {
        listItems.forEach(i => i.classList.remove("nav-active"));
        item.classList.add("nav-active");
    });
});


// Ancoras
const menuItems = document.querySelectorAll('.navbar-collapse a[href^="#"]')

menuItems.forEach(item => {
  item.addEventListener('click', scrollToIdOnClick)
})

function scrollToIdOnClick(event) {
  event.preventDefault();
  const to = getScrollTopByHref(event.target) - 20;
  scrollToPosition(to);
}

function scrollToPosition(to){
  smoothScrollTo(0, to)
}

function getScrollTopByHref(element){
  const id = element.getAttribute('href');
  return document.querySelector(id).offsetTop;
}


//suporte a browsers antigos / que não suportam scroll smooth nativo
/**
 * Smooth scroll animation
 * @param {int} endX: destination x coordinate
 * @param {int) endY: destination y coordinate
* @param {int} duration: animation duration in ms
*/
function smoothScrollTo(endX, endY, duration) {
 const startX = window.scrollX || window.pageXOffset;
 const startY = window.scrollY || window.pageYOffset;
 const distanceX = endX - startX;
 const distanceY = endY - startY;
 const startTime = new Date().getTime();

 duration = typeof duration !== 'undefined' ? duration : 700;

 // Easing function
 const easeInOutQuart = (time, from, distance, duration) => {
   if ((time /= duration / 2) < 1) return distance / 2 * time * time * time * time + from;
   return -distance / 2 * ((time -= 2) * time * time * time - 2) + from;
 };

 const timer = setInterval(() => {
   const time = new Date().getTime() - startTime;
   const newX = easeInOutQuart(time, startX, distanceX, duration);
   const newY = easeInOutQuart(time, startY, distanceY, duration);
   if (time >= duration) {
     clearInterval(timer);
   }
   window.scroll(newX, newY);
 }, 1000 / 60); // 60 fps
};