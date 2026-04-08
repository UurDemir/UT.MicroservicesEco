export interface BasketItemDto {
  id: string;
  userName: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  addedAtUtc: string;
}

export interface CreateOrderLineRequest {
  productId: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateOrderRequest {
  userName: string;
  shippingAddress: string;
  lines: CreateOrderLineRequest[];
}
