// DTOs para Appointment
// NOTA: Todo en camelCase - el interceptor convierte automáticamente a snake_case al enviar
export interface CreateAppointmentDto {
  providerId: number;
  customerId?: number;
  customerName?: string;
  serviceId?: number;
  serviceName?: string;
  startTime: string;  // ISO DateTime
  endTime: string;    // ISO DateTime
  notes?: string;
}

export interface AppointmentResponse {
  id: number;
  businessId: number;  // camelCase en respuestas
  providerId: number;
  providerName: string;
  customerId?: number;
  serviceId?: number;
  serviceName?: string;
  startTime: string;
  endTime: string;
  status: string;
  notes?: string;
}

// DTOs para Provider
export interface ProviderResponse {
  id: number;
  businessId: number;  // camelCase porque el interceptor convierte la respuesta
  name: string;
  specialty: string;
  isActive: boolean;  // camelCase porque el interceptor convierte la respuesta
}

// DTOs para Customer
export interface CustomerResponse {
  id: number;
  businessId: number;  // camelCase
  name: string;
  phone?: string;
  email?: string;
}

// DTOs para Service
export interface ServiceResponse {
  id: number;
  businessId: number;  // camelCase
  name: string;
  defaultDuration: number;  // camelCase
  price?: number;
}
