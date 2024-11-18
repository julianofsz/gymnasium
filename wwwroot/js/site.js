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
