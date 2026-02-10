export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResponseDto {
  access_token: string;
  refresh_token: string;
  access_token_expires_at: string;
  user_id: number;
  email: string;
  business_id: number;
}

export interface RegisterDto {
  email: string;
  password: string;
  businessName: string;
  industry: string;
  providerName: string;
  providerSpecialty: string;
}

export interface DecodedToken {
  userId: number;
  businessId: number;
  providerId: number;
  email: string;
  exp: number;
}

export interface RefreshTokenDto {
  refresh_token: string;
}
