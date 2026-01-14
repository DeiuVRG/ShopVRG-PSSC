import axios, { AxiosInstance } from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

export interface Product {
  code: string;
  name: string;
  description: string;
  category: string;
  price: number;
  stock: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface OrderLine {
  productCode: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface PlaceOrderRequest {
  customerName: string;
  customerEmail: string;
  shippingStreet: string;
  shippingCity: string;
  shippingPostalCode: string;
  shippingCountry: string;
  orderLines: OrderLine[];
}

export interface PlaceOrderResponse {
  orderId: string;
  status?: string;
  totalPrice: number;
  createdAt: string;
  customerName?: string;
  customerEmail?: string;
}

export interface ProcessPaymentRequest {
  orderId: string;
  amount: number;
  cardNumber: string;
  cardHolderName: string;
  expiryDate: string;
  cvv: string;
}

export interface PaymentResponse {
  paymentId: string;
  orderId: string;
  amount: number;
  status: string;
  transactionReference: string;
  processedAt: string;
}

export interface ShipOrderRequest {
  orderId: string;
  carrier: string;
}

export interface ShipmentResponse {
  id: number;
  orderId: string;
  trackingNumber: string;
  carrier: string;
  shippedAt: string;
  estimatedDelivery: string;
}

class ApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });
  }

  // Products
  async getProducts(): Promise<Product[]> {
    const response = await this.client.get<any>('/products');
    const data = response.data;
    return Array.isArray(data) ? data : (data.data || data.products || []);
  }

  async getActiveProducts(): Promise<Product[]> {
    const response = await this.client.get<any>('/products/active');
    const data = response.data;
    return Array.isArray(data) ? data : (data.data || data.products || []);
  }

  async getProductByCode(code: string): Promise<Product> {
    const response = await this.client.get<Product>(`/products/${code}`);
    return response.data;
  }

  // Orders
  async placeOrder(order: PlaceOrderRequest): Promise<PlaceOrderResponse> {
    const response = await this.client.post<any>('/orders', order);
    const data = response.data;
    // Handle both wrapped (ApiResponse) and direct responses
    if (data.data) {
      return {
        orderId: data.data.orderId,
        status: data.data.status,
        totalPrice: data.data.totalPrice,
        createdAt: data.data.createdAt,
        customerName: data.data.customerName,
        customerEmail: data.data.customerEmail
      };
    }
    return data;
  }

  // Payments
  async processPayment(payment: ProcessPaymentRequest): Promise<PaymentResponse> {
    const response = await this.client.post<any>('/payments', payment);
    const data = response.data;
    // Handle both wrapped (ApiResponse) and direct responses
    if (data.data) {
      return data.data;
    }
    return data;
  }

  // Shipping
  async shipOrder(shipment: ShipOrderRequest): Promise<ShipmentResponse> {
    const response = await this.client.post<any>('/shipping', shipment);
    const data = response.data;
    // Handle both wrapped (ApiResponse) and direct responses
    if (data.data) {
      return data.data;
    }
    return data;
  }

  async getShippingCarriers(): Promise<string[]> {
    const response = await this.client.get<string[]>('/shipping/carriers');
    return response.data;
  }
}

export const apiClient = new ApiClient();
