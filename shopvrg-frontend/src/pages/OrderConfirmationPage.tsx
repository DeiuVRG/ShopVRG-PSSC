import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useCheckoutStore } from '../store/checkoutStore';
import './OrderConfirmationPage.css';

const OrderConfirmationPage = () => {
  const navigate = useNavigate();
  const { currentOrder, currentPayment, currentShipment } = useCheckoutStore();

  if (!currentOrder) {
    return (
      <div className="confirmation-page">
        <div className="error">No order found. Please start a new order.</div>
        <button className="btn btn-primary" onClick={() => navigate('/')}>
          Continue Shopping
        </button>
      </div>
    );
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Placed':
        return '📦';
      case 'Paid':
        return '✓';
      case 'Shipped':
        return '🚚';
      case 'Delivered':
        return '🎉';
      default:
        return '•';
    }
  };

  return (
    <div className="confirmation-page">
      <div className="success-header">
        <div className="success-icon">🎉</div>
        <h1>Order Confirmed!</h1>
        <p>Thank you for your purchase</p>
      </div>

      <div className="confirmation-content">
        {/* Order Information */}
        <div className="section">
          <h2>Order Information</h2>
          <div className="info-grid">
            <div className="info-item">
              <span className="label">Order ID:</span>
              <span className="value">{currentOrder.orderId}</span>
            </div>
            <div className="info-item">
              <span className="label">Status:</span>
              <span className={`status status-${(currentOrder.status || 'placed').toLowerCase()}`}>
                {getStatusIcon(currentOrder.status || 'Placed')} {currentOrder.status || 'Placed'}
              </span>
            </div>
            <div className="info-item">
              <span className="label">Order Date:</span>
              <span className="value">
                {new Date(currentOrder.createdAt).toLocaleDateString()}
              </span>
            </div>
            <div className="info-item">
              <span className="label">Total Amount:</span>
              <span className="value price">${currentOrder.totalPrice.toFixed(2)}</span>
            </div>
          </div>
        </div>

        {/* Customer Information */}
        <div className="section">
          <h2>Customer Information</h2>
          <div className="info-grid">
            <div className="info-item">
              <span className="label">Name:</span>
              <span className="value">{currentOrder.customerName}</span>
            </div>
            <div className="info-item">
              <span className="label">Email:</span>
              <span className="value">{currentOrder.customerEmail}</span>
            </div>
          </div>
        </div>

        {/* Payment Information */}
        {currentPayment && (
          <div className="section">
            <h2>Payment Information</h2>
            <div className="info-grid">
              <div className="info-item">
                <span className="label">Payment ID:</span>
                <span className="value">{currentPayment.paymentId}</span>
              </div>
              <div className="info-item">
                <span className="label">Status:</span>
                <span className={`status status-${(currentPayment.status || 'processing').toLowerCase()}`}>
                  ✓ {currentPayment.status || 'Processing'}
                </span>
              </div>
              <div className="info-item">
                <span className="label">Amount:</span>
                <span className="value">${currentPayment.amount.toFixed(2)}</span>
              </div>
              <div className="info-item">
                <span className="label">Transaction Ref:</span>
                <span className="value">{currentPayment.transactionReference}</span>
              </div>
            </div>
          </div>
        )}

        {/* Shipping Information */}
        {currentShipment && (
          <div className="section">
            <h2>Shipping Information</h2>
            <div className="info-grid">
              <div className="info-item">
                <span className="label">Carrier:</span>
                <span className="value">🚚 {currentShipment.carrier}</span>
              </div>
              <div className="info-item">
                <span className="label">Tracking Number:</span>
                <span className="value tracking">{currentShipment.trackingNumber}</span>
              </div>
              <div className="info-item">
                <span className="label">Shipped:</span>
                <span className="value">
                  {new Date(currentShipment.shippedAt).toLocaleDateString()}
                </span>
              </div>
              <div className="info-item">
                <span className="label">Est. Delivery:</span>
                <span className="value">
                  {new Date(currentShipment.estimatedDelivery).toLocaleDateString()}
                </span>
              </div>
            </div>
          </div>
        )}

        {/* Order Timeline */}
        <div className="section">
          <h2>Order Timeline</h2>
          <div className="timeline">
            <div className="timeline-item completed">
              <div className="timeline-marker">1</div>
              <div className="timeline-content">
                <h3>Order Placed</h3>
                <p>{new Date(currentOrder.createdAt).toLocaleString()}</p>
              </div>
            </div>

            <div className={`timeline-item ${currentPayment ? 'completed' : ''}`}>
              <div className="timeline-marker">2</div>
              <div className="timeline-content">
                <h3>Payment Processed</h3>
                <p>
                  {currentPayment
                    ? new Date(currentPayment.processedAt).toLocaleString()
                    : 'Processing...'}
                </p>
              </div>
            </div>

            <div className={`timeline-item ${currentShipment ? 'completed' : ''}`}>
              <div className="timeline-marker">3</div>
              <div className="timeline-content">
                <h3>Order Shipped</h3>
                <p>
                  {currentShipment
                    ? `via ${currentShipment.carrier}`
                    : 'Being prepared...'}
                </p>
              </div>
            </div>

            <div className="timeline-item">
              <div className="timeline-marker">4</div>
              <div className="timeline-content">
                <h3>Order Delivered</h3>
                <p>Coming soon</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="actions">
        <button
          className="btn btn-primary"
          onClick={() => navigate('/')}
        >
          Continue Shopping
        </button>
      </div>
    </div>
  );
};

export default OrderConfirmationPage;
