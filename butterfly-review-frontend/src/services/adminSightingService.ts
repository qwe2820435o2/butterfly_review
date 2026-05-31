import axiosInstance from "./axiosInstance";
import { PaginatedResultDto } from "@/types/user";
import { SightingSubmission, SightingSubmissionInput } from "@/types/submission";

export const adminSightingService = {
  async getById(id: string): Promise<SightingSubmission> {
    const res = await axiosInstance.get(`/api/sightingsubmissions/${id}`);
    return res.data.data;
  },

  async create(dto: SightingSubmissionInput): Promise<SightingSubmission> {
    const res = await axiosInstance.post(`/api/sightingsubmissions`, dto);
    return res.data.data;
  },

  async update(id: string, dto: SightingSubmissionInput): Promise<SightingSubmission> {
    const res = await axiosInstance.put(`/api/sightingsubmissions/${id}`, dto);
    return res.data.data;
  },
  async getPaginated(
    page: number = 1,
    pageSize: number = 20,
    sortDescending: boolean = true,
    search?: string
  ): Promise<PaginatedResultDto<SightingSubmission>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortDescending: sortDescending.toString(),
    });
    if (search && search.trim()) params.append("search", search.trim());
    const res = await axiosInstance.get(`/api/sightingsubmissions/paginated?${params}`);
    return res.data.data;
  },

  async delete(id: string): Promise<void> {
    await axiosInstance.delete(`/api/sightingsubmissions/${id}`);
  },
};
