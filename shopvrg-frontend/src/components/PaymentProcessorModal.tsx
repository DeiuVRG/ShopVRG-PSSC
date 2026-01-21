import React, { useState } from 'react';
import './PaymentProcessorModal.css';

interface PaymentProcessorModalProps {
  orderId: string;
  amount: number;
  cardHolderName: string;
  isOpen: boolean;
  isLoading: boolean;
  onConfirmPayment: () => Promise<void>;
  onCancel: () => void;
}

const PaymentProcessorModal: React.FC<PaymentProcessorModalProps> = ({
  orderId,
  amount,
  cardHolderName,
  isOpen,
  isLoading,
  onConfirmPayment,
  onCancel,
}) => {
  const [isProcessing, setIsProcessing] = useState(false);
  const [processingStep, setProcessingStep] = useState<'confirm' | 'processing' | 'complete' | 'error'>('confirm');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleConfirmPayment = async () => {
    setIsProcessing(true);
    setProcessingStep('processing');
    setErrorMessage(null);

    try {
      // Simulate some processing time
      await new Promise((resolve) => setTimeout(resolve, 1500));

      // Call the parent's confirmation handler
      await onConfirmPayment();

      setProcessingStep('complete');

      // Auto-close after showing success
      setTimeout(() => {
        handleClose();
      }, 1500);
    } catch (error: any) {
      console.error('Modal error:', error);
      setIsProcessing(false);
      setProcessingStep('error');
      setErrorMessage(error?.message || 'An error occurred while processing payment');
    }
  };

  const handleClose = () => {
    setProcessingStep('confirm');
    setIsProcessing(false);
    setErrorMessage(null);
    onCancel();
  };

  const handleRetry = async () => {
    setErrorMessage(null);
    await handleConfirmPayment();
  };

  if (!isOpen) return null;

  return (
    <div className="payment-processor-overlay">
      <div className="payment-processor-modal">
        {processingStep === 'confirm' && (
          <div className="processor-content">
            <div className="processor-header">
              <h2 className="processor-title">Payment Processor</h2>
              <p className="processor-subtitle">Simulated Payment Gateway</p>
            </div>

            <div className="processor-body">
              <div className="processor-card">
                <div className="card-icon">
                  <i className="bi bi-credit-card"></i>
                </div>

                <div className="card-details">
                  <div className="detail-row">
                    <span className="detail-label">Order ID:</span>
                    <span className="detail-value">{orderId}</span>
                  </div>
                  <div className="detail-row">
                    <span className="detail-label">Cardholder:</span>
                    <span className="detail-value">{cardHolderName}</span>
                  </div>
                  <div className="detail-row">
                    <span className="detail-label">Amount:</span>
                    <span className="detail-value">${amount.toFixed(2)}</span>
                  </div>
                </div>

                <div className="processor-info">
                  <i className="bi bi-info-circle"></i>
                  <p>Click "Process Payment" to complete the transaction through our simulated payment processor.</p>
                </div>
              </div>
            </div>

            <div className="processor-footer">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={handleClose}
                disabled={isLoading}
              >
                Cancel
              </button>
              <button
                type="button"
                className="btn btn-primary"
                onClick={handleConfirmPayment}
                disabled={isLoading || isProcessing}
              >
                {isLoading || isProcessing ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2"></span>
                    Processing...
                  </>
                ) : (
                  <>
                    <i className="bi bi-check-circle me-2"></i>
                    Process Payment
                  </>
                )}
              </button>
            </div>
          </div>
        )}

        {processingStep === 'processing' && (
          <div className="processor-content">
            <div className="processor-body processing-state">
              <div className="processing-spinner">
                <div className="spinner-circle"></div>
                <div className="spinner-circle"></div>
                <div className="spinner-circle"></div>
              </div>
              <h3>Processing Payment...</h3>
              <p>Please wait while we process your payment securely.</p>
            </div>
          </div>
        )}

        {processingStep === 'complete' && (
          <div className="processor-content">
            <div className="processor-body success-state">
              <div className="success-icon">
                <i className="bi bi-check-circle-fill"></i>
              </div>
              <h3>Payment Processed Successfully</h3>
              <p>Your payment has been confirmed and your order is being prepared for shipment.</p>
              <div className="success-details">
                <p><strong>Order ID:</strong> {orderId}</p>
                <p><strong>Amount Charged:</strong> ${amount.toFixed(2)}</p>
              </div>
            </div>
          </div>
        )}

        {processingStep === 'error' && (
          <div className="processor-content">
            <div className="processor-body error-state">
              <div className="error-icon">
                <i className="bi bi-exclamation-circle-fill"></i>
              </div>
              <h3>Payment Processing Failed</h3>
              <p className="error-message">{errorMessage}</p>
              <div className="error-details">
                <p><strong>Order ID:</strong> {orderId}</p>
              </div>
            </div>

            <div className="processor-footer">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={handleClose}
              >
                <i className="bi bi-x-circle me-2"></i>
                Cancel
              </button>
              <button
                type="button"
                className="btn btn-danger"
                onClick={handleRetry}
              >
                <i className="bi bi-arrow-clockwise me-2"></i>
                Retry
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default PaymentProcessorModal;
