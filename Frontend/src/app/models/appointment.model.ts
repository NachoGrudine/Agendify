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

export interface UpdateAppointmentDto {
  providerId: number;
  customerId?: number;
  customerName?: string;
  serviceId?: number;
  serviceName?: string;
  startTime: string;  // ISO DateTime
  endTime: string;    // ISO DateTime
  status: string;     // AppointmentStatus
  notes?: string;
}

export interface AppointmentResponse {
  id: number;
  businessId: number;  // camelCase en respuestas
  providerId: number;
  providerName: string;
  customerId?: number;
  customerName?: string; // Agregar customerName
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

export interface CreateProviderDto {
  name: string;
  specialty: string;
  isActive?: boolean;
}

export interface UpdateProviderDto {
  name: string;
  specialty: string;
  isActive: boolean;
}

// DTOs para Customer
export interface CustomerResponse {
  id: number;
  businessId: number;
  name: string;
  phone?: string;
  email?: string;
}

export interface CreateCustomerDto {
  name: string;
  phone?: string;
  email?: string;
}

export interface UpdateCustomerDto {
  name: string;
  phone?: string;
  email?: string;
}

// DTO para próximo turno
export interface NextAppointmentResponse {
  customerName: string;
  startTime: string;  // ISO DateTime
  endTime: string;    // ISO DateTime
  day: string;        // ISO DateTime (fecha del día)
}

