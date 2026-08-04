const navbar = document.querySelector('.navbar');
const toggle = document.querySelector('.mobile-toggle');
const cartCount = document.getElementById('cartCount');
const toast = document.getElementById('toast');
const cards = [...document.querySelectorAll('.product-card')];
const categoryCards = document.querySelectorAll('.category-card');
const searchInput = document.getElementById('searchInput');
const sortSelect = document.getElementById('sortSelect');
const productGrid = document.getElementById('productGrid');
let cart = 0;
let currentFilter = 'all';

toggle?.addEventListener('click', () => {
  navbar?.classList.toggle('open');
});

const accountMenus = document.querySelectorAll('[data-account-menu]');

function setAccountMenuState(menu, isOpen) {
  const trigger = menu.querySelector('.account-menu-trigger');

  menu.classList.toggle('open', isOpen);
  trigger?.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
}

accountMenus.forEach(menu => {
  const trigger = menu.querySelector('.account-menu-trigger');

  trigger?.addEventListener('click', event => {
    event.stopPropagation();
    setAccountMenuState(menu, !menu.classList.contains('open'));
  });

  menu.addEventListener('keydown', event => {
    if (event.key !== 'Escape') return;

    setAccountMenuState(menu, false);
    trigger?.focus();
  });
});

document.addEventListener('click', event => {
  accountMenus.forEach(menu => {
    if (!menu.contains(event.target)) {
      setAccountMenuState(menu, false);
    }
  });
});

function showToast() {
  toast?.classList.add('show');
  setTimeout(() => toast?.classList.remove('show'), 1800);
}

document.querySelectorAll('.add-cart').forEach(button => {
  button.addEventListener('click', () => {
    cart += 1;
    if (cartCount) cartCount.textContent = cart;
    showToast();
  });
});

function applyFilter() {
  const keyword = searchInput?.value.trim().toLowerCase() ?? '';

  cards.forEach(card => {
    const category = card.dataset.category;
    const name = card.dataset.name.toLowerCase();
    const matchCategory = currentFilter === 'all' || category === currentFilter;
    const matchKeyword = name.includes(keyword);
    card.classList.toggle('hidden', !(matchCategory && matchKeyword));
  });
}

categoryCards.forEach(card => {
  card.addEventListener('click', () => {
    categoryCards.forEach(item => item.classList.remove('active'));
    card.classList.add('active');
    currentFilter = card.dataset.filter;
    applyFilter();
  });
});

searchInput?.addEventListener('input', applyFilter);

sortSelect?.addEventListener('change', () => {
  const sorted = [...cards].sort((a, b) => {
    const priceA = Number(a.dataset.price);
    const priceB = Number(b.dataset.price);

    if (sortSelect.value === 'low-high') return priceA - priceB;
    if (sortSelect.value === 'high-low') return priceB - priceA;
    return 0;
  });

  sorted.forEach(card => productGrid?.appendChild(card));
});
