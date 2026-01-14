import { create } from 'zustand';
import { Product } from '../api/client';

interface ProductState {
  products: Product[];
  loading: boolean;
  error: string | null;
  setProducts: (products: Product[]) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  getProductByCode: (code: string) => Product | undefined;
}

export const useProductStore = create<ProductState>((set, get) => ({
  products: [],
  loading: false,
  error: null,

  setProducts: (products: Product[]) => {
    set({ products });
  },

  setLoading: (loading: boolean) => {
    set({ loading });
  },

  setError: (error: string | null) => {
    set({ error });
  },

  getProductByCode: (code: string) => {
    return get().products.find((p) => p.code === code);
  },
}));
