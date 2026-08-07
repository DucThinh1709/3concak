const navbar = document.querySelector('.navbar');
const toggle = document.querySelector('.mobile-toggle');
const cartCount = document.getElementById('cartCount');
const toast = document.getElementById('toast');
const cards = [...document.querySelectorAll('.product-card')];
const categoryCards = document.querySelectorAll('.category-card');
const searchInput = document.getElementById('searchInput');
const sortSelect = document.getElementById('sortSelect');
const productGrid = document.getElementById('productGrid');
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

function showToast(message = 'Đã thêm sản phẩm vào giỏ hàng', isError = false) {
  if (!toast) return;

  toast.textContent = message;
  toast.classList.toggle('error', isError);
  toast?.classList.add('show');
  window.clearTimeout(toast._hideTimer);
  toast._hideTimer = window.setTimeout(() => toast.classList.remove('show'), 2200);
}

const miniCartHosts = [...document.querySelectorAll('[data-mini-cart]')];

function setMiniCartState(host, isOpen) {
  const trigger = host.querySelector('[data-mini-cart-trigger]');

  host.classList.toggle('open', isOpen);
  trigger?.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
}

async function loadMiniCart(host, force = false) {
  const content = host.querySelector('[data-mini-cart-content]');
  const url = host.dataset.miniCartUrl;

  if (!content || !url || host.dataset.loading === 'true') return;
  if (!force && host.dataset.loaded === 'true') return;

  host.dataset.loading = 'true';
  content.innerHTML = '<div class="mini-cart-loading">Đang tải giỏ hàng...</div>';

  try {
    const response = await fetch(url, {
      credentials: 'same-origin',
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });

    if (!response.ok) {
      throw new Error('Không thể tải giỏ hàng.');
    }

    content.innerHTML = await response.text();
    host.dataset.loaded = 'true';
  } catch (error) {
    content.innerHTML = `
      <div class="mini-cart-load-error">
        Không thể tải giỏ hàng. Vui lòng mở trang giỏ hàng để kiểm tra.
      </div>`;
  } finally {
    host.dataset.loading = 'false';
  }
}

async function openAndRefreshMiniCart() {
  const host = miniCartHosts.find(item => item.dataset.authenticated === 'true');

  if (!host) return;

  setMiniCartState(host, true);
  await loadMiniCart(host, true);
}

miniCartHosts.forEach(host => {
  const trigger = host.querySelector('[data-mini-cart-trigger]');
  const isAuthenticated = host.dataset.authenticated === 'true';

  trigger?.addEventListener('click', event => {
    if (!isAuthenticated) return;

    event.preventDefault();
    const shouldOpen = !host.classList.contains('open');
    setMiniCartState(host, shouldOpen);

    if (shouldOpen) {
      loadMiniCart(host);
    }
  });

  if (isAuthenticated && window.matchMedia('(hover: hover)').matches) {
    host.addEventListener('mouseenter', () => {
      setMiniCartState(host, true);
      loadMiniCart(host);
    });

    host.addEventListener('mouseleave', () => {
      window.setTimeout(() => {
        if (!host.matches(':hover')) {
          setMiniCartState(host, false);
        }
      }, 180);
    });
  }

  host.addEventListener('keydown', event => {
    if (event.key !== 'Escape') return;

    setMiniCartState(host, false);
    trigger?.focus();
  });
});

document.addEventListener('click', event => {
  miniCartHosts.forEach(host => {
    if (!host.contains(event.target)) {
      setMiniCartState(host, false);
    }
  });

  document.querySelectorAll('[data-quick-add-panel].open').forEach(panel => {
    if (!panel.contains(event.target)) {
      panel.classList.remove('open');
      panel.querySelector('[data-quick-add-toggle]')?.setAttribute('aria-expanded', 'false');
    }
  });
});

document.addEventListener('click', event => {
  const toggleButton = event.target.closest?.('[data-quick-add-toggle]');

  if (!toggleButton) return;

  const panel = toggleButton.closest('[data-quick-add-panel]');
  const shouldOpen = !panel?.classList.contains('open');

  panel?.classList.toggle('open', shouldOpen);
  toggleButton.setAttribute('aria-expanded', shouldOpen ? 'true' : 'false');
});

document.addEventListener('submit', async event => {
  const form = event.target.closest?.('[data-quick-add-form]');

  if (!form) return;

  event.preventDefault();

  const submitButton = form.querySelector('.quick-add-submit');
  const status = form.querySelector('[data-quick-add-status]');
  const originalButtonText = submitButton?.textContent;

  if (submitButton) {
    submitButton.disabled = true;
    submitButton.textContent = 'Đang thêm...';
  }

  if (status) {
    status.textContent = '';
    status.classList.remove('error');
  }

  try {
    const response = await fetch(form.dataset.quickAddUrl || form.action, {
      method: 'POST',
      body: new FormData(form),
      credentials: 'same-origin',
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });

    const result = await response.json();

    if (response.status === 401 && result.loginUrl) {
      window.location.href = result.loginUrl;
      return;
    }

    if (!response.ok || !result.success) {
      throw new Error(result.message || 'Không thể thêm sản phẩm vào giỏ.');
    }

    if (cartCount) {
      cartCount.textContent = String(result.cartQuantity ?? 0);
    }

    if (status) {
      status.textContent = result.message;
    }

    showToast(result.message);
    await openAndRefreshMiniCart();
  } catch (error) {
    const message = error instanceof Error
      ? error.message
      : 'Không thể thêm sản phẩm vào giỏ.';

    if (status) {
      status.textContent = message;
      status.classList.add('error');
    }

    showToast(message, true);
  } finally {
    if (submitButton) {
      submitButton.disabled = false;
      submitButton.textContent = originalButtonText || 'Thêm 1 sản phẩm';
    }
  }
});

document.addEventListener('submit', async event => {
  const form = event.target.closest?.('[data-mini-cart-remove]');

  if (!form) return;

  event.preventDefault();

  const button = form.querySelector('button[type="submit"]');
  if (button) button.disabled = true;

  try {
    const response = await fetch(form.action, {
      method: 'POST',
      body: new FormData(form),
      credentials: 'same-origin',
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });

    const result = await response.json();

    if (!response.ok || !result.success) {
      throw new Error(result.message || 'Không thể xóa sản phẩm khỏi giỏ.');
    }

    if (cartCount) {
      cartCount.textContent = String(result.cartQuantity ?? 0);
    }

    showToast(result.message);

    const host = form.closest('[data-mini-cart]');
    if (host) {
      await loadMiniCart(host, true);
      setMiniCartState(host, true);
    }

    if (document.querySelector('.cart-page')) {
      window.location.reload();
    }
  } catch (error) {
    const message = error instanceof Error
      ? error.message
      : 'Không thể xóa sản phẩm khỏi giỏ.';

    showToast(message, true);
    if (button) button.disabled = false;
  }
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
