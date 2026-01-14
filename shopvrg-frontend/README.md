# ShopVRG Frontend - React + TypeScript Storefront

A fully functioning e-commerce storefront for ShopVRG PC Components Store, built with React and TypeScript, integrating with the .NET 9 API backend.

## Features

✅ **Product Browsing**
- Browse PC components organized by category
- View detailed product information
- Real-time stock availability

✅ **Shopping Cart**
- Add/remove products
- Update quantities
- Real-time cart totals

✅ **Complete Checkout Flow**
- Shipping address collection
- Payment processing
- Carrier selection
- Order confirmation with tracking

✅ **Order Management**
- Order status tracking
- Payment confirmation
- Shipment tracking
- Timeline visualization

✅ **State Management**
- Zustand stores for cart, products, and checkout
- Type-safe state management
- Persistence-ready architecture

✅ **API Integration**
- RESTful API client
- All CRUD operations supported
- Error handling
- Loading states

## Installation

### Prerequisites
- Node.js 18+ and npm installed
- ShopVRG API running on `http://localhost:5000`

### Setup Steps

1. **Install Dependencies**
```bash
cd shopvrg-frontend
npm install
```

2. **Start Development Server**
```bash
npm start
```

The storefront will open at `http://localhost:3000`

3. **Build for Production**
```bash
npm run build
```

## Technologies Used

- **React 18** - UI framework
- **TypeScript** - Type-safe code
- **React Router v6** - Navigation
- **Zustand** - State management
- **Axios** - HTTP client
- **CSS3** - Styling with modern features
