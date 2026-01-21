import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCartStore } from '../store/cartStore';
import { useCheckoutStore } from '../store/checkoutStore';
import { apiClient, PlaceOrderRequest } from '../api/client';
import PaymentProcessorModal from '../components/PaymentProcessorModal';
import './CheckoutPage.css';

const CheckoutPage = () => {
  const navigate = useNavigate();
  const { items, getTotalPrice, clearCart } = useCartStore();
  const { setOrder, setPayment, setShipment } = useCheckoutStore();

  const [step, setStep] = useState<'shipping' | 'payment' | 'confirmation'>(
    'shipping'
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Payment processor modal state
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [pendingOrderData, setPendingOrderData] = useState<{
    orderId: string;
    totalPrice: number;
    cardHolderName: string;
  } | null>(null);

  // Shipping form state
  const [shippingForm, setShippingForm] = useState({
    customerName: '',
    customerEmail: '',
    shippingStreet: '',
    shippingCity: '',
    shippingPostalCode: '',
    shippingCountry: '',
  });

  // Payment form state
  const [paymentForm, setPaymentForm] = useState({
    cardNumber: '',
    cardHolderName: '',
    expiryDate: '',
    cvv: '',
  });

  // Shipping selection
  const [selectedCarrier, setSelectedCarrier] = useState('FedEx');

  const totalPrice = getTotalPrice();

  if (items.length === 0) {
    return (
      <div className="checkout-page">
        <div className="error">Cart is empty. Please add items first.</div>
        <button
          className="btn btn-primary"
          onClick={() => navigate('/')}
        >
          Continue Shopping
        </button>
      </div>
    );
  }

  const handleShippingSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (
      !shippingForm.customerName ||
      !shippingForm.customerEmail ||
      !shippingForm.shippingStreet ||
      !shippingForm.shippingCity ||
      !shippingForm.shippingPostalCode ||
      !shippingForm.shippingCountry
    ) {
      setError('Please fill in all shipping fields');
      return;
    }
    setError(null);
    setStep('payment');
  };

  const handlePaymentSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (
      !paymentForm.cardNumber ||
      !paymentForm.cardHolderName ||
      !paymentForm.expiryDate ||
      !paymentForm.cvv
    ) {
      setError('Please fill in all payment fields');
      return;
    }

    setLoading(true);
    setError(null);

    // Build order request outside try block so it's accessible in catch
    const orderRequest: PlaceOrderRequest = {
      ...shippingForm,
      orderLines: items.map((item) => ({
        productCode: item.product.code,
        productName: item.product.name,
        quantity: item.quantity,
        unitPrice: item.product.price,
        lineTotal: item.product.price * item.quantity,
      })),
    };

    try {
      console.log('Placing order with request:', JSON.stringify(orderRequest, null, 2));
      const orderResponse = await apiClient.placeOrder(orderRequest);
      setOrder({
        orderId: orderResponse.orderId,
        status: (orderResponse.status || 'Placed') as any,
        totalPrice: orderResponse.totalPrice,
        createdAt: orderResponse.createdAt,
        customerName: shippingForm.customerName,
        customerEmail: shippingForm.customerEmail,
      });

      // Store pending data and show payment processor modal
      setPendingOrderData({
        orderId: orderResponse.orderId,
        totalPrice: orderResponse.totalPrice,
        cardHolderName: paymentForm.cardHolderName,
      });
      setShowPaymentModal(true);
      setLoading(false);
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || err.message || 'Failed to place order. Please try again.';
      const errorDetails = err.response?.data?.errors?.join(', ') || '';
      setError(errorDetails ? `${errorMessage}: ${errorDetails}` : errorMessage);
      console.error('Order placement error:', err);
      console.error('Order request was:', orderRequest);
      setLoading(false);
    }
  };

  const handleConfirmPayment = async () => {
    if (!pendingOrderData) return;

    try {
      console.log('Starting payment confirmation for order:', pendingOrderData.orderId);
      
      // Step 2: Process Payment
      const paymentResponse = await apiClient.processPayment({
        orderId: pendingOrderData.orderId,
        amount: pendingOrderData.totalPrice,
        cardNumber: paymentForm.cardNumber,
        cardHolderName: paymentForm.cardHolderName,
        expiryDate: paymentForm.expiryDate,
        cvv: paymentForm.cvv,
      });

      console.log('Payment processed:', paymentResponse);

      setPayment({
        paymentId: paymentResponse.paymentId,
        orderId: paymentResponse.orderId,
        amount: paymentResponse.amount,
        status: paymentResponse.status as any,
        transactionReference: paymentResponse.transactionReference,
        processedAt: paymentResponse.processedAt,
      });

      // Step 3: Ship Order
      const shipmentResponse = await apiClient.shipOrder({
        orderId: pendingOrderData.orderId,
        carrier: selectedCarrier,
      });

      console.log('Shipment created:', shipmentResponse);

      setShipment({
        id: shipmentResponse.id,
        orderId: shipmentResponse.orderId,
        trackingNumber: shipmentResponse.trackingNumber,
        carrier: shipmentResponse.carrier,
        shippedAt: shipmentResponse.shippedAt,
        estimatedDelivery: shipmentResponse.estimatedDelivery,
      });

      // Clear cart and go to confirmation
      clearCart();
      console.log('Navigating to order confirmation...');
      navigate('/order-confirmation');
    } catch (err: any) {
      const errorMsg =
        err.response?.data?.message || err.message || 'Failed to process payment. Please try again.';
      console.error('Payment processing error:', err);
      setError(errorMsg);
      setShowPaymentModal(false);
      setPendingOrderData(null);
    }
  };

  const handleCancelPayment = () => {
    setShowPaymentModal(false);
    setPendingOrderData(null);
  };

  return (
    <div className="checkout-page">
      {/* Payment Processor Modal */}
      <PaymentProcessorModal
        isOpen={showPaymentModal}
        orderId={pendingOrderData?.orderId || ''}
        amount={pendingOrderData?.totalPrice || 0}
        cardHolderName={pendingOrderData?.cardHolderName || ''}
        isLoading={loading}
        onConfirmPayment={handleConfirmPayment}
        onCancel={handleCancelPayment}
      />

      <div className="container">
        <h1 className="text-center mb-5">
          <i className="bi bi-bag-check-fill me-3"></i>
          Secure Checkout
        </h1>

        {/* Progress Stepper */}
        <div className="row mb-5">
          <div className="col-12">
            <div className="d-flex justify-content-between align-items-center position-relative" style={{padding: '0 2rem'}}>
              {/* Progress Bar Background */}
              <div className="position-absolute w-100" style={{height: '4px', background: '#e0e0e0', top: '50%', transform: 'translateY(-50%)', left: 0, zIndex: 0}}></div>
              <div className="position-absolute" style={{
                height: '4px', 
                background: 'linear-gradient(90deg, #667eea, #764ba2)', 
                top: '50%', 
                transform: 'translateY(-50%)', 
                left: 0, 
                width: step === 'shipping' ? '0%' : step === 'payment' ? '50%' : '100%',
                transition: 'width 0.5s ease',
                zIndex: 0
              }}></div>

              {/* Step 1: Shipping */}
              <div className="text-center position-relative" style={{zIndex: 1}}>
                <div className={`rounded-circle d-flex align-items-center justify-content-center mx-auto mb-2 ${step === 'shipping' ? 'bg-gradient' : step === 'payment' || step === 'confirmation' ? 'bg-success' : 'bg-light'}`} 
                     style={{width: '60px', height: '60px', boxShadow: '0 4px 15px rgba(0,0,0,0.2)', background: step === 'shipping' ? 'linear-gradient(135deg, #667eea, #764ba2)' : undefined}}>
                  <i className={`bi ${step === 'payment' || step === 'confirmation' ? 'bi-check-lg' : 'bi-truck'} fs-4 text-white`}></i>
                </div>
                <div className="fw-semibold small">Shipping</div>
              </div>

              {/* Step 2: Payment */}
              <div className="text-center position-relative" style={{zIndex: 1}}>
                <div className={`rounded-circle d-flex align-items-center justify-content-center mx-auto mb-2 ${step === 'payment' ? 'bg-gradient' : step === 'confirmation' ? 'bg-success' : 'bg-light'}`}
                     style={{width: '60px', height: '60px', boxShadow: '0 4px 15px rgba(0,0,0,0.2)', background: step === 'payment' ? 'linear-gradient(135deg, #667eea, #764ba2)' : undefined}}>
                  <i className={`bi ${step === 'confirmation' ? 'bi-check-lg' : 'bi-credit-card'} fs-4 ${step === 'payment' || step === 'confirmation' ? 'text-white' : 'text-muted'}`}></i>
                </div>
                <div className={`fw-semibold small ${step === 'payment' || step === 'confirmation' ? '' : 'text-muted'}`}>Payment</div>
              </div>

              {/* Step 3: Confirmation */}
              <div className="text-center position-relative" style={{zIndex: 1}}>
                <div className={`rounded-circle d-flex align-items-center justify-content-center mx-auto mb-2 ${step === 'confirmation' ? 'bg-gradient' : 'bg-light'}`}
                     style={{width: '60px', height: '60px', boxShadow: '0 4px 15px rgba(0,0,0,0.2)', background: step === 'confirmation' ? 'linear-gradient(135deg, #667eea, #764ba2)' : undefined}}>
                  <i className={`bi bi-check-circle fs-4 ${step === 'confirmation' ? 'text-white' : 'text-muted'}`}></i>
                </div>
                <div className={`fw-semibold small ${step === 'confirmation' ? '' : 'text-muted'}`}>Complete</div>
              </div>
            </div>
          </div>
        </div>

        {error && (
          <div className="alert alert-danger d-flex align-items-center mb-4" role="alert">
            <i className="bi bi-exclamation-triangle-fill me-2"></i>
            {error}
          </div>
        )}

        <div className="row g-4">
          {/* Main Content */}
          <div className="col-lg-8">
            <div className="card shadow-sm border-0">
              <div className="card-body p-4 p-md-5">
                {step === 'shipping' && (
                  <form onSubmit={handleShippingSubmit}>
                    <h3 className="mb-4 d-flex align-items-center">
                      <i className="bi bi-geo-alt-fill text-primary me-2"></i>
                      Shipping Information
                    </h3>

                    <div className="row g-3">
                      <div className="col-md-6">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-person me-2"></i>Full Name
                        </label>
                        <input
                          type="text"
                          className="form-control form-control-lg"
                          value={shippingForm.customerName}
                          onChange={(e) =>
                            setShippingForm({
                              ...shippingForm,
                              customerName: e.target.value,
                            })
                          }
                          placeholder="John Doe"
                          required
                        />
                      </div>

                      <div className="col-md-6">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-envelope me-2"></i>Email Address
                        </label>
                        <input
                          type="email"
                          className="form-control form-control-lg"
                          value={shippingForm.customerEmail}
                          onChange={(e) =>
                            setShippingForm({
                              ...shippingForm,
                              customerEmail: e.target.value,
                            })
                          }
                          placeholder="john@example.com"
                          required
                        />
                      </div>

                      <div className="col-12">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-house me-2"></i>Street Address
                        </label>
                        <input
                          type="text"
                          className="form-control form-control-lg"
                          value={shippingForm.shippingStreet}
                          onChange={(e) =>
                            setShippingForm({
                              ...shippingForm,
                              shippingStreet: e.target.value,
                            })
                          }
                          placeholder="123 Main Street, Apt 4B"
                          required
                        />
                      </div>

                      <div className="col-md-4">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-building me-2"></i>City
                        </label>
                        <input
                          type="text"
                          className="form-control form-control-lg"
                          value={shippingForm.shippingCity}
                          onChange={(e) =>
                            setShippingForm({
                              ...shippingForm,
                              shippingCity: e.target.value,
                            })
                          }
                          placeholder="New York"
                          required
                        />
                      </div>

                      <div className="col-md-4">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-mailbox me-2"></i>Postal Code
                        </label>
                        <input
                          type="text"
                          className="form-control form-control-lg"
                          value={shippingForm.shippingPostalCode}
                          onChange={(e) =>
                            setShippingForm({
                              ...shippingForm,
                              shippingPostalCode: e.target.value,
                            })
                          }
                          placeholder="10001"
                          required
                        />
                      </div>

                      <div className="col-md-4">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-flag me-2"></i>Country
                        </label>
                        <input
                          type="text"
                          className="form-control form-control-lg"
                          value={shippingForm.shippingCountry}
                          onChange={(e) =>
                            setShippingForm({
                              ...shippingForm,
                              shippingCountry: e.target.value,
                            })
                          }
                          placeholder="United States"
                          required
                        />
                      </div>
                    </div>

                    <div className="d-grid mt-4">
                      <button type="submit" className="btn btn-lg btn-primary" style={{
                        background: 'linear-gradient(135deg, #667eea, #764ba2)',
                        border: 'none',
                        padding: '1rem'
                      }}>
                        Continue to Payment
                        <i className="bi bi-arrow-right ms-2"></i>
                      </button>
                    </div>
                  </form>
                )}

                {step === 'payment' && (
                  <form onSubmit={handlePaymentSubmit}>
                    <h3 className="mb-4 d-flex align-items-center">
                      <i className="bi bi-credit-card-2-front-fill text-success me-2"></i>
                      Payment Details
                    </h3>

                    <div className="row g-3 mb-4">
                      <div className="col-12">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-credit-card me-2"></i>Card Number
                        </label>
                        <input
                          type="text"
                          className="form-control form-control-lg"
                          value={paymentForm.cardNumber}
                          onChange={(e) =>
                            setPaymentForm({
                              ...paymentForm,
                              cardNumber: e.target.value.replace(/\s/g, ''),
                            })
                          }
                          placeholder="1234 5678 9012 3456"
                          maxLength={16}
                          required
                        />
                      </div>

                      <div className="col-12">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-person-badge me-2"></i>Cardholder Name
                        </label>
                        <input
                          type="text"
                          className="form-control form-control-lg"
                          value={paymentForm.cardHolderName}
                          onChange={(e) =>
                            setPaymentForm({
                              ...paymentForm,
                              cardHolderName: e.target.value,
                            })
                          }
                          placeholder="JOHN DOE"
                          required
                          style={{textTransform: 'uppercase'}}
                        />
                      </div>

                      <div className="col-6">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-calendar-event me-2"></i>Expiry Date
                        </label>
                        <input
                          type="text"
                          className="form-control form-control-lg"
                          value={paymentForm.expiryDate}
                          onChange={(e) =>
                            setPaymentForm({
                              ...paymentForm,
                              expiryDate: e.target.value,
                            })
                          }
                          placeholder="MM/YY"
                          maxLength={5}
                          required
                        />
                      </div>

                      <div className="col-6">
                        <label className="form-label fw-semibold">
                          <i className="bi bi-shield-lock me-2"></i>CVV
                        </label>
                        <input
                          type="text"
                          className="form-control form-control-lg"
                          value={paymentForm.cvv}
                          onChange={(e) =>
                            setPaymentForm({
                              ...paymentForm,
                              cvv: e.target.value,
                            })
                          }
                          placeholder="123"
                          maxLength={4}
                          required
                        />
                      </div>
                    </div>

                    <hr className="my-4" />

                    <h4 className="mb-3 d-flex align-items-center">
                      <i className="bi bi-box-seam text-warning me-2"></i>
                      Select Shipping Carrier
                    </h4>
                    <div className="row g-3 mb-4">
                      {['FedEx', 'UPS', 'DHL', 'USPS'].map((carrier) => (
                        <div key={carrier} className="col-6 col-md-3">
                          <div 
                            className={`card h-100 ${selectedCarrier === carrier ? 'border-primary border-3' : 'border-secondary'}`}
                            style={{cursor: 'pointer', transition: 'all 0.3s'}}
                            onClick={() => setSelectedCarrier(carrier)}
                          >
                            <div className="card-body text-center p-3">
                              <i className="bi bi-truck fs-2 mb-2" style={{color: selectedCarrier === carrier ? '#667eea' : '#999'}}></i>
                              <div className="fw-bold">{carrier}</div>
                              {selectedCarrier === carrier && (
                                <i className="bi bi-check-circle-fill text-primary mt-2"></i>
                              )}
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>

                    <div className="d-flex gap-3 mt-4">
                      <button
                        type="button"
                        className="btn btn-lg btn-outline-secondary flex-grow-1"
                        onClick={() => setStep('shipping')}
                      >
                        <i className="bi bi-arrow-left me-2"></i>
                        Back
                      </button>
                      <button
                        type="submit"
                        className="btn btn-lg btn-success flex-grow-1"
                        disabled={loading}
                        style={{background: loading ? undefined : 'linear-gradient(135deg, #43a047, #66bb6a)'}}
                      >
                        {loading ? (
                          <>
                            <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                            Processing...
                          </>
                        ) : (
                          <>
                            <i className="bi bi-check-circle me-2"></i>
                            Complete Order
                          </>
                        )}
                      </button>
                    </div>
                  </form>
                )}
              </div>
            </div>
          </div>

          {/* Order Summary Sidebar */}
          <div className="col-lg-4">
            <div className="card shadow-sm border-0 sticky-top" style={{top: '100px'}}>
              <div className="card-header" style={{background: 'linear-gradient(135deg, #667eea, #764ba2)', color: 'white'}}>
                <h4 className="mb-0">
                  <i className="bi bi-receipt me-2"></i>
                  Order Summary
                </h4>
              </div>
              <div className="card-body">
                <div className="mb-3">
                  {items.map((item) => (
                    <div key={item.product.code} className="d-flex justify-content-between align-items-start mb-3 pb-3 border-bottom">
                      <div className="flex-grow-1">
                        <div className="fw-semibold">{item.product.name}</div>
                        <div className="small text-muted">Qty: {item.quantity} × ${item.product.price.toFixed(2)}</div>
                      </div>
                      <div className="fw-bold text-end">${(item.product.price * item.quantity).toFixed(2)}</div>
                    </div>
                  ))}
                </div>

                <div className="border-top pt-3 mb-3">
                  <div className="d-flex justify-content-between mb-2">
                    <span className="text-muted">Subtotal:</span>
                    <span>${totalPrice.toFixed(2)}</span>
                  </div>
                  <div className="d-flex justify-content-between mb-2">
                    <span className="text-muted">Shipping:</span>
                    <span className="text-success">FREE</span>
                  </div>
                  <div className="d-flex justify-content-between mb-2">
                    <span className="text-muted">Tax:</span>
                    <span>${(totalPrice * 0.1).toFixed(2)}</span>
                  </div>
                </div>

                <div className="border-top pt-3">
                  <div className="d-flex justify-content-between align-items-center">
                    <span className="fs-5 fw-bold">Total:</span>
                    <span className="fs-4 fw-bold text-primary">${(totalPrice * 1.1).toFixed(2)}</span>
                  </div>
                </div>

                <div className="alert alert-info mt-3 mb-0 d-flex align-items-center">
                  <i className="bi bi-shield-check me-2"></i>
                  <small>Secure SSL Encrypted Payment</small>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CheckoutPage;
