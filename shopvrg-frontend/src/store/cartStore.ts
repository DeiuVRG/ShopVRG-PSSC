import { create } from 'zustand';
import { Product } from '../api/client';

export interface CartItem {
  product: Product;
  quantity: number;
}

interface CartState {
  items: CartItem[];
  addItem: (product: Product, quantity: number) => void;
  removeItem: (productCode: string) => void;
  updateQuantity: (productCode: string, quantity: number) => void;
  clearCart: () => void;
  getTotalPrice: () => number;
  getTotalItems: () => number;
}

export const useCartStore = create<CartState>((set, get) => ({
  items: [],

  addItem: (product: Product, quantity: number) => {
    set((state) => {
      const existingItem = state.items.find(
        (item) => item.product.code === product.code
      );

      if (existingItem) {
        return {
          items: state.items.map((item) =>
            item.product.code === product.code
              ? { ...item, quantity: item.quantity + quantity }
              : item
          ),
        };
      }

      return {
        items: [...state.items, { product, quantity }],
      };
    });
  },

  removeItem: (productCode: string) => {
    set((state) => ({
      items: state.items.filter((item) => item.product.code !== productCode),
    }));
  },

  updateQuantity: (productCode: string, quantity: number) => {
    if (quantity <= 0) {
      get().removeItem(productCode);
      return;
    }

    set((state) => ({
      items: state.items.map((item) =>
        item.product.code === productCode
          ? { ...item, quantity }
          : item
      ),
    }));
  },

  clearCart: () => {
    set({ items: [] });
  },

  getTotalPrice: () => {
    return get().items.reduce(
      (total, item) => total + item.product.price * item.quantity,
      0
    );
  },

  getTotalItems: () => {
    return get().items.reduce((total, item) => total + item.quantity, 0);
  },
}));
