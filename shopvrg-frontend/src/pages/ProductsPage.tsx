import React, { useState } from 'react';
import { useProductStore } from '../store/productStore';
import { useCartStore } from '../store/cartStore';
import './ProductsPage.css';

const ProductsPage = () => {
  const { products, loading, error } = useProductStore();
  const { addItem } = useCartStore();
  const [selectedQuantity, setSelectedQuantity] = useState<{ [key: string]: number }>({});
  const [addedProducts, setAddedProducts] = useState<string[]>([]);

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
        <p>Loading products...</p>
      </div>
    );
  }

  if (error) {
    return <div className="error">{error}</div>;
  }

  const categories = Array.from(new Set(products.map((p) => p.category)));

  return (
    <div className="products-page">
      <h1>PC Components Catalog</h1>
      <p className="subtitle">Premium Computer Parts for Gaming & Workstations</p>

      {categories.map((category) => (
        <div key={category} className="category-section mb-5">
          <h2 className="category-title">{category}</h2>
          <div className="row row-cols-1 row-cols-sm-2 row-cols-lg-3 row-cols-xl-4 g-3 g-md-4">
            {products
              .filter((p) => p.category === category)
              .map((product) => (
                <div key={product.code} className="col">
                  <div className="product-card h-100">
                    <div className="product-header">
                      <span className="product-code">{product.code}</span>
                      {product.stock < 10 && (
                        <span className="low-stock">Low Stock</span>
                      )}
                    </div>
                    <h3 className="product-name">{product.name}</h3>
                    <p className="product-description">{product.description}</p>
                    <div className="product-footer mt-auto">
                      <div className="product-price">
                        <span className="price">${product.price.toFixed(2)}</span>
                        <span className="stock">
                          {product.stock} in stock
                        </span>
                      </div>
                      <div className="quantity-selector">
                        <input
                          type="number"
                          min="1"
                          max={product.stock}
                          value={selectedQuantity[product.code] || 1}
                          onChange={(e) =>
                            setSelectedQuantity({
                              ...selectedQuantity,
                              [product.code]: parseInt(e.target.value) || 1,
                            })
                          }
                          className="quantity-input form-control"
                        />
                      </div>
                      <button
                        className={`btn btn-primary w-100 d-flex align-items-center justify-content-center gap-2 ${
                          addedProducts.includes(product.code) ? 'added' : ''
                        }`}
                        onClick={() => handleAddToCart(product.code)}
                        disabled={product.stock === 0}
                      >
                        {addedProducts.includes(product.code) ? (
                          <>
                            <i className="bi bi-check-circle-fill"></i>
                            <span>Added to Cart</span>
                          </>
                        ) : (
                          <>
                            <i className="bi bi-cart-plus-fill"></i>
                            <span>Add to Cart</span>
                          </>
                        )}
                      </button>
                    </div>
                  </div>
                </div>
              ))}
          </div>
        </div>
      ))}
    </div>
  );
};

export default ProductsPage;
