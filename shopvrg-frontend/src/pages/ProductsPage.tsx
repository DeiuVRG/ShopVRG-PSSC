import React, { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useProductStore } from '../store/productStore';
import { useCartStore } from '../store/cartStore';
import './ProductsPage.css';

const ProductsPage = () => {
  const { products, loading, error } = useProductStore();
  const { addItem } = useCartStore();
  const [selectedQuantity, setSelectedQuantity] = useState<{ [key: string]: number }>({});
  const [addedProducts, setAddedProducts] = useState<string[]>([]);
  const [searchParams] = useSearchParams();

  const categoryFilter = searchParams.get('category') || '';
  const searchQuery = searchParams.get('search') || '';

  const handleAddToCart = (productCode: string) => {
    const product = products.find((p) => p.code === productCode);
    if (product) {
      const quantity = selectedQuantity[productCode] || 1;
      addItem(product, quantity);
      setAddedProducts([...addedProducts, productCode]);
      setTimeout(() => {
        setAddedProducts((prev) =>
          prev.filter((code) => code !== productCode)
        );
      }, 2000);
    }
  };

  if (loading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
        <p>Se incarca produsele...</p>
      </div>
    );
  }

  if (error) {
    return <div className="alert alert-error">{error}</div>;
  }

  // Filter products based on category and search
  let filteredProducts = products;

  if (categoryFilter) {
    filteredProducts = filteredProducts.filter((p) =>
      p.category.toLowerCase() === categoryFilter.toLowerCase()
    );
  }

  if (searchQuery) {
    const query = searchQuery.toLowerCase();
    filteredProducts = filteredProducts.filter((p) =>
      p.name.toLowerCase().includes(query) ||
      p.description.toLowerCase().includes(query) ||
      p.code.toLowerCase().includes(query)
    );
  }

  // Get page title based on filters
  const getPageTitle = () => {
    if (searchQuery) return `Rezultate pentru "${searchQuery}"`;
    if (categoryFilter) return categoryFilter;
    return 'Toate Produsele';
  };

  return (
    <div className="products-page">
      {/* Section Header */}
      <div className="section-header">
        <h1 className="section-title">
          <i className="bi bi-grid-fill"></i>
          {getPageTitle()}
        </h1>
        <span className="products-count">
          {filteredProducts.length} produse
        </span>
      </div>

      {filteredProducts.length === 0 ? (
        <div className="no-products">
          <i className="bi bi-search"></i>
          <h3>Niciun produs gasit</h3>
          <p>Incercati o alta cautare sau categorie.</p>
        </div>
      ) : (
        <div className="products-grid">
          {filteredProducts.map((product) => (
            <div key={product.code} className="product-card">
              {/* Product Image Placeholder */}
              <div className="product-image">
                <i className="bi bi-cpu"></i>
                {product.stock < 10 && product.stock > 0 && (
                  <span className="product-badge">Stoc Limitat</span>
                )}
                {product.stock === 0 && (
                  <span className="product-badge out-of-stock-badge">Stoc Epuizat</span>
                )}
              </div>

              {/* Product Info */}
              <div className="product-info">
                <span className="product-category">{product.category}</span>
                <h3 className="product-name">{product.name}</h3>
                <p className="product-description">{product.description}</p>

                <div className="product-footer">
                  <div className="price-stock-row">
                    <div className="product-price">
                      {product.price.toFixed(2)} <span className="currency">RON</span>
                    </div>
                    <span className={`product-stock ${product.stock === 0 ? 'out-of-stock' : ''}`}>
                      <i className={`bi ${product.stock > 0 ? 'bi-check-circle-fill' : 'bi-x-circle-fill'}`}></i>
                      {product.stock > 0 ? `${product.stock} in stoc` : 'Indisponibil'}
                    </span>
                  </div>

                  {/* Quantity & Add to Cart */}
                  <div className="add-to-cart-row">
                    <div className="quantity-selector">
                      <button
                        className="qty-btn"
                        onClick={() => setSelectedQuantity({
                          ...selectedQuantity,
                          [product.code]: Math.max(1, (selectedQuantity[product.code] || 1) - 1)
                        })}
                        disabled={product.stock === 0}
                      >
                        -
                      </button>
                      <span className="qty-value">{selectedQuantity[product.code] || 1}</span>
                      <button
                        className="qty-btn"
                        onClick={() => setSelectedQuantity({
                          ...selectedQuantity,
                          [product.code]: Math.min(product.stock, (selectedQuantity[product.code] || 1) + 1)
                        })}
                        disabled={product.stock === 0}
                      >
                        +
                      </button>
                    </div>

                    <button
                      className={`add-to-cart-btn ${addedProducts.includes(product.code) ? 'added' : ''}`}
                      onClick={() => handleAddToCart(product.code)}
                      disabled={product.stock === 0}
                    >
                      {addedProducts.includes(product.code) ? (
                        <>
                          <i className="bi bi-check-circle-fill"></i>
                          Adaugat
                        </>
                      ) : (
                        <>
                          <i className="bi bi-cart-plus-fill"></i>
                          Adauga
                        </>
                      )}
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ProductsPage;
