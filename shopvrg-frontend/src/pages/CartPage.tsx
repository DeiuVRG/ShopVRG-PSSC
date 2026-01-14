import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useCartStore } from '../store/cartStore';
import './CartPage.css';

const CartPage = () => {
  const navigate = useNavigate();
  const { items, removeItem, updateQuantity, clearCart, getTotalPrice } =
    useCartStore();
  const totalPrice = getTotalPrice();

  if (items.length === 0) {
    return (
      <div className="cart-page">
        <h1 className="mb-4">Shopping Cart</h1>
        <div className="empty-cart text-center p-5">
          <p className="fs-4 mb-4">🛒 Your cart is empty</p>
          <button
            className="btn btn-primary btn-lg d-inline-flex align-items-center gap-2"
            onClick={() => navigate('/')}
          >
            <i className="bi bi-shop"></i>
            <span>Continue Shopping</span>
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="cart-page">
      <h1 className="mb-4">Shopping Cart</h1>

      <div className="row g-4">
        <div className="col-12 col-lg-8">
          <div className="cart-items">
            {items.map((item) => (
              <div key={item.product.code} className="cart-item p-3 p-md-4">
                <div className="row align-items-center g-3">
                  <div className="col-12 col-md-5">
                    <h3 className="fs-5 mb-2">{item.product.name}</h3>
                    <p className="item-code text-muted mb-1">{item.product.code}</p>
                    <p className="item-price mb-0">
                      ${item.product.price.toFixed(2)} each
                    </p>
                  </div>

                  <div className="col-6 col-md-3">
                    <label htmlFor="quantity-input" className="form-label small">Quantity:</label>
                    <input
                      id="quantity-input"
                      type="number"
                      min="1"
                      value={item.quantity}
                      onChange={(e) =>
                        updateQuantity(
                          item.product.code,
                          Number.parseInt(e.target.value) || 1
                        )
                      }
                      className="form-control"
                    />
                  </div>

                  <div className="col-6 col-md-2 text-end">
                    <div className="item-total">
                      <p className="fs-5 fw-bold mb-0">
                        ${(item.product.price * item.quantity).toFixed(2)}
                      </p>
                    </div>
                  </div>

                  <div className="col-12 col-md-2 text-end">
                    <button
                      className="btn btn-danger btn-sm d-inline-flex align-items-center gap-2"
                      onClick={() => removeItem(item.product.code)}
                    >
                      <i className="bi bi-trash"></i>
                      <span>Remove</span>
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="col-12 col-lg-4">
          <div className="cart-summary position-sticky" style={{top: '100px'}}>
            <h2 className="fs-4 mb-4">Order Summary</h2>
            <div className="d-flex justify-content-between py-2 border-bottom">
              <span>Items:</span>
              <span>{items.length}</span>
            </div>
            <div className="d-flex justify-content-between py-2 border-bottom">
              <span>Total Quantity:</span>
              <span>
                {items.reduce((total, item) => total + item.quantity, 0)}
              </span>
            </div>
            <div className="d-flex justify-content-between py-3 border-top border-primary border-3 fw-bold fs-5">
              <span>Total Price:</span>
              <span>${totalPrice.toFixed(2)}</span>
            </div>

            <div className="d-grid gap-3 mt-4">
              <button
                className="btn btn-primary btn-lg d-flex align-items-center justify-content-center gap-2"
                onClick={() => navigate('/checkout')}
              >
                <i className="bi bi-credit-card"></i>
                <span>Proceed to Checkout</span>
              </button>

              <button
                className="btn btn-outline-secondary d-flex align-items-center justify-content-center gap-2"
                onClick={() => navigate('/')}
              >
                <i className="bi bi-shop"></i>
                <span>Continue Shopping</span>
              </button>

              <button
                className="btn btn-outline-danger d-flex align-items-center justify-content-center gap-2"
                onClick={clearCart}
              >
                <i className="bi bi-trash"></i>
                <span>Clear Cart</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CartPage;
