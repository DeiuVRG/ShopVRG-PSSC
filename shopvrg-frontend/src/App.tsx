import React, { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { useCartStore } from './store/cartStore';
import { useProductStore } from './store/productStore';
import { apiClient } from './api/client';
import ProductsPage from './pages/ProductsPage';
import CartPage from './pages/CartPage';
import CheckoutPage from './pages/CheckoutPage';
import OrderConfirmationPage from './pages/OrderConfirmationPage';
import './App.css';

function App() {
  const cartItems = useCartStore((state) => state.getTotalItems());
  const { setProducts, setLoading, setError } = useProductStore();

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);
        const products = await apiClient.getActiveProducts();
        console.log('Loaded products:', products);
        if (!Array.isArray(products)) {
          setError('Invalid product data format received from server');
          return;
        }
        setProducts(products);
      } catch (error) {
        setError('Failed to load products');
        console.error('Error loading products:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [setProducts, setLoading, setError]);

  return (
    <BrowserRouter>
      <div className="app">
        <nav className="navbar navbar-expand-lg navbar-dark sticky-top" style={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          boxShadow: '0 4px 20px rgba(0, 0, 0, 0.3)',
          borderBottom: '3px solid rgba(118, 75, 162, 0.5)'
        }}>
          <div className="container-fluid px-3 px-md-5">
            <Link to="/" className="navbar-brand d-flex align-items-center gap-3">
              <div className="d-flex align-items-center justify-content-center bg-white rounded-circle" style={{width: '50px', height: '50px'}}>
                <i className="bi bi-pc-display-horizontal text-primary fs-3"></i>
              </div>
              <div>
                <h1 className="mb-0 fw-bold" style={{fontSize: '1.8rem', textShadow: '2px 2px 4px rgba(0,0,0,0.3)'}}>
                  ShopVRG
                </h1>
                <p className="mb-0 small opacity-90 d-none d-md-block">Premium PC Components</p>
              </div>
            </Link>
            
            <button className="navbar-toggler border-0" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
              <span className="navbar-toggler-icon"></span>
            </button>
            
            <div className="collapse navbar-collapse" id="navbarNav">
              <ul className="navbar-nav ms-auto gap-2">
                <li className="nav-item">
                  <Link to="/" className="nav-link btn btn-light btn-lg d-flex align-items-center gap-2 px-4 py-2" style={{
                    background: 'rgba(255,255,255,0.15)',
                    border: '2px solid rgba(255,255,255,0.3)',
                    backdropFilter: 'blur(10px)',
                    transition: 'all 0.3s'
                  }}>
                    <i className="bi bi-shop-window fs-5"></i>
                    <span className="fw-semibold">Shop</span>
                  </Link>
                </li>
                <li className="nav-item">
                  <Link to="/cart" className="nav-link btn btn-warning btn-lg d-flex align-items-center gap-2 px-4 py-2 position-relative" style={{
                    background: 'linear-gradient(135deg, #ffd700, #ffed4e)',
                    color: '#000',
                    border: 'none',
                    fontWeight: '600',
                    boxShadow: '0 4px 15px rgba(255, 215, 0, 0.4)'
                  }}>
                    <i className="bi bi-cart-fill fs-5"></i>
                    <span className="fw-semibold">Cart</span>
                    {cartItems > 0 && (
                      <span className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger" style={{
                        fontSize: '0.75rem',
                        padding: '0.35em 0.65em',
                        animation: 'pulse 2s infinite'
                      }}>
                        {cartItems}
                      </span>
                    )}
                  </Link>
                </li>
              </ul>
            </div>
          </div>
        </nav>

        <main className="container py-3 py-md-4">
          <Routes>
            <Route path="/" element={<ProductsPage />} />
            <Route path="/cart" element={<CartPage />} />
            <Route path="/checkout" element={<CheckoutPage />} />
            <Route path="/order-confirmation" element={<OrderConfirmationPage />} />
          </Routes>
        </main>

        <footer className="footer">
          <p>&copy; 2026 ShopVRG - PC Components Store. All rights reserved.</p>
        </footer>
      </div>
    </BrowserRouter>
  );
}

export default App;
