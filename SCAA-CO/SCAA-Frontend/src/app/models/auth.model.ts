export interface LoginRequestDto {
  userName: string;
  password: string;
}

export interface LoginResponseDto {
  accessToken: string;
}

export interface RegistrationRequestDto {
  email: string;
  password: string;
}

export interface AuthUser {
  email: string | null;
  userId: string | null;
  roles: string[];
  expiresAt: number | null;
}
