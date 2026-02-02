export interface ServiceResponse {
  id: number;
  businessId: number;
  name: string;
  defaultDuration: number;
  price: number | null;
}

export interface CreateServiceDto {
  name: string;
  defaultDuration: number;
  price: number | null;
}

export interface UpdateServiceDto {
  name: string;
  defaultDuration: number;
  price: number | null;
}
