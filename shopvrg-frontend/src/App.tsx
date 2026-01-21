import React, { useEffect, useState } from 'react';
import { BrowserRouter, Routes, Route, Link, useNavigate } from 'react-router-dom';
import { useCartStore } from './store/cartStore';
import { useProductStore } from './store/productStore';
import { apiClient } from './api/client';
import ProductsPage from './pages/ProductsPage';
import CartPage from './pages/CartPage';
import CheckoutPage from './pages/CheckoutPage';
import OrderConfirmationPage from './pages/OrderConfirmationPage';
import ContactPage from './pages/ContactPage';
import './App.css';

// Category definitions - must match database category names exactly
const categories = [
  { name: 'Toate Produsele', icon: 'bi-grid-fill', filter: '' },
  // PC Components
  { name: 'Placi Video (GPU)', icon: 'bi-gpu-card', filter: 'GPU' },
  { name: 'Procesoare (CPU)', icon: 'bi-cpu-fill', filter: 'CPU' },
  { name: 'Memorii RAM', icon: 'bi-memory', filter: 'RAM' },
  { name: 'Placi de baza', icon: 'bi-motherboard-fill', filter: 'Motherboard' },
  { name: 'Storage (SSD/HDD)', icon: 'bi-device-ssd-fill', filter: 'Storage' },
  { name: 'Surse (PSU)', icon: 'bi-lightning-charge-fill', filter: 'PSU' },
  { name: 'Carcase', icon: 'bi-pc-display', filter: 'Case' },
  { name: 'Coolere', icon: 'bi-fan', filter: 'Cooler' },
  // Peripherals
  { name: 'Monitoare', icon: 'bi-display', filter: 'Monitor' },
  { name: 'Tastaturi', icon: 'bi-keyboard', filter: 'Keyboard' },
  { name: 'Mouse', icon: 'bi-mouse', filter: 'Mouse' },
  { name: 'Mousepads', icon: 'bi-square', filter: 'Mousepad' },
  { name: 'Casti', icon: 'bi-headphones', filter: 'Headset' },
  { name: 'Boxe', icon: 'bi-speaker', filter: 'Speakers' },
  { name: 'Microfoane', icon: 'bi-mic', filter: 'Microphone' },
  { name: 'Webcam', icon: 'bi-webcam', filter: 'Webcam' },
  { name: 'Controllere', icon: 'bi-controller', filter: 'Controller' },
  // Gaming Furniture
  { name: 'Scaune Gaming', icon: 'bi-person-workspace', filter: 'Gaming Chair' },
  { name: 'Birouri Gaming', icon: 'bi-table', filter: 'Gaming Desk' },
  // Other
  { name: 'Networking', icon: 'bi-wifi', filter: 'Networking' },
  { name: 'Streaming', icon: 'bi-broadcast', filter: 'Streaming' },
  { name: 'VR Headset', icon: 'bi-badge-vr', filter: 'VR Headset' },
];

function AppContent() {
  const cartItems = useCartStore((state) => state.getTotalItems());
  const { setProducts, setLoading, setError } = useProductStore();
  const [searchQuery, setSearchQuery] = useState('');
  const [showCategoryDropdown, setShowCategoryDropdown] = useState(false);
  const navigate = useNavigate();

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

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    navigate(`/?search=${encodeURIComponent(searchQuery)}`);
  };

  const handleCategoryClick = (filter: string) => {
    setShowCategoryDropdown(false);
    if (filter) {
      navigate(`/?category=${encodeURIComponent(filter)}`);
    } else {
      navigate('/');
    }
  };

  return (
    <div className="app">
      {/* Top Header - Logo & Search */}
      <header className="top-header">
        <div className="top-header-content">
          {/* Logo */}
          <Link to="/" className="logo-section">
            <div className="logo-icon">
              <i className="bi bi-pc-display-horizontal"></i>
            </div>
            <div className="logo-text">
              <h1>Shop<span>VRG</span></h1>
              <p>PC Components Store</p>
            </div>
          </Link>

          {/* Search Bar */}
          <div className="search-section">
            <form className="search-bar" onSubmit={handleSearch}>
              <input
                type="text"
                placeholder="Cauta produse..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
              <button type="submit">
                <i className="bi bi-search"></i>
              </button>
            </form>
          </div>

          {/* Header Actions */}
          <div className="header-actions">
            <Link to="/contact" className="header-action-btn">
              <i className="bi bi-telephone-fill"></i>
              <span>Contact</span>
            </Link>
            <Link to="/cart" className="header-action-btn cart-btn">
              <i className="bi bi-cart-fill"></i>
              <span>Cos</span>
              {cartItems > 0 && (
                <span className="cart-badge">{cartItems}</span>
              )}
            </Link>
          </div>
        </div>
      </header>

      {/* Category Bar */}
      <nav className="category-bar">
        <div className="category-bar-content">
          {/* All Categories Dropdown */}
          <div
            className="all-categories-dropdown"
            onMouseEnter={() => setShowCategoryDropdown(true)}
            onMouseLeave={() => setShowCategoryDropdown(false)}
          >
            <button className="all-categories-btn">
              <i className="bi bi-list"></i>
              <span>Toate Categoriile</span>
              <i className="bi bi-chevron-down"></i>
            </button>

            <div className={`dropdown-menu-categories ${showCategoryDropdown ? 'show' : ''}`} style={{
              opacity: showCategoryDropdown ? 1 : 0,
              visibility: showCategoryDropdown ? 'visible' : 'hidden',
              transform: showCategoryDropdown ? 'translateY(0)' : 'translateY(-10px)'
            }}>
              {categories.map((cat, index) => (
                <button
                  key={index}
                  type="button"
                  onClick={() => handleCategoryClick(cat.filter)}
                >
                  <i className={`bi ${cat.icon}`}></i>
                  {cat.name}
                </button>
              ))}
            </div>
          </div>

          {/* Quick Category Links */}
          <div className="category-links">
            <Link to="/" className="category-link">
              <i className="bi bi-house-fill me-1"></i> Acasa
            </Link>
            <Link to="/?category=GPU" className="category-link">
              Placi Video
            </Link>
            <Link to="/?category=CPU" className="category-link">
              Procesoare
            </Link>
            <Link to="/?category=RAM" className="category-link">
              Memorii
            </Link>
            <Link to="/contact" className="category-link">
              Contact
            </Link>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="main-content">
        <Routes>
          <Route path="/" element={<ProductsPage />} />
          <Route path="/cart" element={<CartPage />} />
          <Route path="/checkout" element={<CheckoutPage />} />
          <Route path="/order-confirmation" element={<OrderConfirmationPage />} />
          <Route path="/contact" element={<ContactPage />} />
        </Routes>
      </main>

      {/* Footer */}
      <footer className="footer">
        <div className="footer-content">
          <div className="footer-section">
            <h3>ShopVRG</h3>
            <p>Magazinul tau de incredere pentru componente PC de calitate premium.</p>
            <p>Proiect PSSC - Universitatea Politehnica Timisoara</p>
          </div>

          <div className="footer-section">
            <h3>Componente PC</h3>
            <Link to="/?category=GPU">Placi Video</Link>
            <Link to="/?category=CPU">Procesoare</Link>
            <Link to="/?category=RAM">Memorii RAM</Link>
            <Link to="/?category=Cooler">Coolere</Link>
          </div>

          <div className="footer-section">
            <h3>Periferice</h3>
            <Link to="/?category=Monitor">Monitoare</Link>
            <Link to="/?category=Keyboard">Tastaturi</Link>
            <Link to="/?category=Mouse">Mouse</Link>
            <Link to="/?category=Gaming Chair">Scaune Gaming</Link>
          </div>

          <div className="footer-section">
            <h3>Informatii</h3>
            <Link to="/contact">Contact</Link>
            <Link to="/cart">Cosul meu</Link>
            <span className="footer-link-disabled">Livrare</span>
            <span className="footer-link-disabled">Garantie</span>
          </div>

          <div className="footer-section">
            <h3>Contact</h3>
            <p><i className="bi bi-geo-alt-fill me-2" style={{color: '#ed1c24'}}></i>Bv. Vasile Parvan 2, Timisoara</p>
            <p><i className="bi bi-telephone-fill me-2" style={{color: '#ed1c24'}}></i>+40 256 403 000</p>
            <p><i className="bi bi-envelope-fill me-2" style={{color: '#ed1c24'}}></i>contact@shopvrg.ro</p>
          </div>
        </div>

        <div className="footer-bottom">
          <p>&copy; 2026 ShopVRG - PC Components Store. Toate drepturile rezervate.</p>
        </div>
      </footer>
    </div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AppContent />
    </BrowserRouter>
  );
}

export default App;
