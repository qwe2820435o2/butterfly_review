import axiosInstance from './axiosInstance';
import { 
  User, 
  CreateUserDto, 
  UpdateUserDto, 
  PaginatedResultDto
} from '@/types/user';

export const userService = {
  // User management - Basic CRUD operations
  async getUsersWithPagination(
    page: number = 1,
    pageSize: number = 20,
    sortBy?: string,
    sortDescending: boolean = false
  ): Promise<PaginatedResultDto<User>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortDescending: sortDescending.toString()
    });
    
    if (sortBy) {
      params.append('sortBy', sortBy);
    }

    const response = await axiosInstance.get(`/api/user/paginated?${params}`);
    return response.data.data;
  },

  async getUserById(id: string): Promise<User> {
    const response = await axiosInstance.get(`/api/user/${id}`);
    return response.data.data;
  },

  async createUser(userData: CreateUserDto): Promise<User> {
    const response = await axiosInstance.post('/api/user', userData);
    return response.data.data;
  },

  async updateUser(id: string, updateData: UpdateUserDto): Promise<User> {
    const response = await axiosInstance.put(`/api/user/${id}`, updateData);
    return response.data.data;
  },

  async deleteUser(id: string): Promise<boolean> {
    const response = await axiosInstance.delete(`/api/user/${id}`);
    return response.data.data;
  }
};
