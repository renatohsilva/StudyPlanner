export interface AuthResponse {
  token: string;
  userId: string;
  name: string;
  email: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}
