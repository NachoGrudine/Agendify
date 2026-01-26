export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResponseDto {
  token: string;
  userId: number;
  email: string;
  businessId: number;
}

export interface DecodedToken {
  userId: number;
  businessId: number;
  email: string;
  exp: number;
}

