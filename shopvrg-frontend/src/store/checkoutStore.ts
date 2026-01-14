import { create } from 'zustand';

export type OrderStatus = 'Placed' | 'Paid' | 'Shipped' | 'Delivered' | 'Cancelled';
export type PaymentStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed';
export type ShippingStatus = 'Pending' | 'InTransit' | 'Delivered';

interface Order {
  orderId: string;
  customerName: string;
  customerEmail: string;
  totalPrice: number;
  status: OrderStatus;
  createdAt: string;
  paidAt?: string;
  shippedAt?: string;
}

interface Payment {
  paymentId: string;
  orderId: string;
  amount: number;
  status: PaymentStatus;
  transactionReference: string;
  processedAt: string;
}

interface Shipment {
  id: number;
  orderId: string;
  trackingNumber: string;
  carrier: string;
  shippedAt: string;
  estimatedDelivery: string;
}

interface CheckoutState {
  currentOrder: Order | null;
  currentPayment: Payment | null;
  currentShipment: Shipment | null;
  setOrder: (order: Order) => void;
  setPayment: (payment: Payment) => void;
  setShipment: (shipment: Shipment) => void;
  updateOrderStatus: (orderId: string, status: OrderStatus) => void;
  clearCheckout: () => void;
}

export const useCheckoutStore = create<CheckoutState>((set) => ({
  currentOrder: null,
  currentPayment: null,
  currentShipment: null,

  setOrder: (order: Order) => {
    set({ currentOrder: order });
  },

  setPayment: (payment: Payment) => {
    set({ currentPayment: payment });
  },

  setShipment: (shipment: Shipment) => {
    set({ currentShipment: shipment });
  },

  updateOrderStatus: (orderId: string, status: OrderStatus) => {
    set((state) => ({
      currentOrder: state.currentOrder
        ? { ...state.currentOrder, status }
        : null,
    }));
  },

  clearCheckout: () => {
    set({
      currentOrder: null,
      currentPayment: null,
      currentShipment: null,
    });
  },
}));
