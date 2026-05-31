import axiosInstance from "./axiosInstance";
import { PaginatedResultDto } from "@/types/user";
import { ReleaseSubmission, ReleaseSubmissionInput } from "@/types/submission";

export const adminReleaseService = {
  async getById(id: string): Promise<ReleaseSubmission> {
    const res = await axiosInstance.get(`/api/releasesubmissions/${id}`);
    return res.data.data;
  },

  async create(dto: ReleaseSubmissionInput): Promise<ReleaseSubmission> {
    const res = await axiosInstance.post(`/api/releasesubmissions`, dto);
    return res.data.data;
  },

  async update(id: string, dto: ReleaseSubmissionInput): Promise<ReleaseSubmission> {
    const res = await axiosInstance.put(`/api/releasesubmissions/${id}`, dto);
    return res.data.data;
  },
  async getPaginated(
    page: number = 1,
    pageSize: number = 20,
    sortDescending: boolean = true,
    search?: string
  ): Promise<PaginatedResultDto<ReleaseSubmission>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortDescending: sortDescending.toString(),
    });
    if (search && search.trim()) params.append("search", search.trim());
    const res = await axiosInstance.get(`/api/releasesubmissions/paginated?${params}`);
    return res.data.data;
  },

  async delete(id: string): Promise<void> {
    await axiosInstance.delete(`/api/releasesubmissions/${id}`);
  },
};
