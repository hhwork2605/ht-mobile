export interface UserRow {
  id: number;
  email: string;
  fullName?: string | null;
  roles: string[];
  lockedOut: boolean;
  emailConfirmed: boolean;
}

export interface CreateUserRequest {
  email: string;
  fullName?: string | null;
  password: string;
  roles: string[];
}
