// User related types based on backend API
export interface User {
  id: string;
  userName: string;
  email: string;
  avatar?: string;
  createdAt: string;
}

export interface CreateUserDto {
  userName: string;
  email: string;
  password: string;
  avatar?: string;
}

export interface UpdateUserDto {
  userName: string;
  avatar?: string;
}

// Pagination types
export interface PaginatedResultDto<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Helper functions
export const formatUserDisplayName = (user: User): string => {
  return user.userName || user.email.split('@')[0];
};

export const getUserInitials = (user: User): string => {
  const name = formatUserDisplayName(user);
  return name
    .split(' ')
    .map(word => word.charAt(0).toUpperCase())
    .join('')
    .slice(0, 2);
};

export const formatUserCreatedDateTime = (user: User): string => {
  return new Date(user.createdAt).toLocaleString('zh-CN', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};
