let cart = JSON.parse(localStorage.getItem('cyberzone_cart')) || [];

function updateCartBadge() {
    const badge = document.getElementById('cartBadge');
    const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);

    if (totalItems > 0) {
        badge.style.display = 'flex';
        badge.innerText = totalItems > 99 ? '99+' : totalItems;
    } else {
        badge.style.display = 'none';
        badge.innerText = '';
    }
}

function openSuccessModal(message) {
    const msgElement = document.getElementById('successMessageText');
    if (message) msgElement.innerText = message;
    document.getElementById('successModal').style.display = 'flex';
}

function closeSuccessModal() {
    document.getElementById('successModal').style.display = 'none';
}

function addToCart(id, name, price) {
    const existingItem = cart.find(i => i.id === id);
    if (existingItem) {
        existingItem.quantity += 1;
    } else {
        cart.push({ id, name, price: parseFloat(price), quantity: 1 });
    }
    saveCart();
    updateCartBadge();
}

function updateQuantity(id, delta) {
    const item = cart.find(i => i.id === id);
    if (item) {
        item.quantity += delta;
        if (item.quantity <= 0) {
            cart = cart.filter(i => i.id !== id);
        }
        saveCart();
        renderCart();
    }
}

function removeItem(id) {
    cart = cart.filter(i => i.id !== id);
    saveCart();
    renderCart();
}

function saveCart() {
    localStorage.setItem('cyberzone_cart', JSON.stringify(cart));
    updateCartBadge();
}

function openCart() {
    renderCart();
    document.getElementById('cartError').innerText = '';
    document.getElementById('cartModal').style.display = 'flex';
}

function closeCart() {
    document.getElementById('cartModal').style.display = 'none';
}

function renderCart() {
    const container = document.getElementById('cartItemsContainer');
    const totalSpan = document.getElementById('cartTotalSum');
    container.innerHTML = '';

    if (cart.length === 0) {
        container.innerHTML = '<p class="empty-cart-msg">Кошик порожній</p>';
        totalSpan.innerText = '0';
        document.getElementById('btnCheckout').disabled = true;
        return;
    }

    document.getElementById('btnCheckout').disabled = false;
    let total = 0;

    cart.forEach(item => {
        total += item.price * item.quantity;
        const itemDiv = document.createElement('div');
        itemDiv.className = 'cart-item';
        itemDiv.innerHTML = `
                    <div class="cart-item-info">
                        <span class="cart-item-name">${item.name}</span>
                        <span class="cart-item-price">${item.price} грн</span>
                    </div>
                    <div class="cart-item-controls">
                        <button onclick="updateQuantity('${item.id}', -1)">-</button>
                        <span>${item.quantity}</span>
                        <button onclick="updateQuantity('${item.id}', 1)">+</button>
                        <button class="btn-remove" onclick="removeItem('${item.id}')">
                           <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 6h18M19 6v14a2 2 0 01-2 2H7a2 2 0 01-2-2V6m3 0V4a2 2 0 012-2h4a2 2 0 012 2v2"/></svg>
                        </button>
                    </div>
                `;
        container.appendChild(itemDiv);
    });

    totalSpan.innerText = total;
}

async function checkout() {
    if (cart.length === 0) return;

    const btn = document.getElementById('btnCheckout');
    const errorDiv = document.getElementById('cartError');
    btn.disabled = true;
    btn.innerText = 'Обробка...';
    errorDiv.innerText = '';

    const payload = {
        Items: cart.map(i => ({ MenuItemId: i.id, Quantity: i.quantity }))
    };

    try {
        const response = await fetch('/Order/Checkout', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await response.json();

        if (response.ok && data.success) {
            closeCart();
            openSuccessModal(data.message);

            cart = [];
            saveCart();
        } else {
            errorDiv.innerText = data.message || 'Помилка при оплаті';
        }
    } catch (err) {
        errorDiv.innerText = 'Помилка з\'єднання з сервером';
    } finally {
        btn.disabled = false;
        btn.innerText = 'Оплатити';
    }
}

updateCartBadge();