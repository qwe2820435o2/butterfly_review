import axiosInstance from './axiosInstance';
import {
  ReleaseSubmission,
  SightingSubmission,
  ApiResponse,
  TagNumberSummary,
  YearInReview
} from '@/types/butterfly';
import {
  getButterflyStatus,
  calculateSurvivalDays
} from '@/types/butterfly';

/**
 * Butterfly API Service
 * Handles all API calls related to butterfly release and sighting data
 */
export const butterflyService = {
  /**
   * Get release submissions by email
   * @param email Email address to search for
   * @param includeAnswers Whether to include answers field (default: false)
   * @returns Array of release submissions
   */
  async getReleaseSubmissionsByEmail(
    email: string,
    includeAnswers: boolean = false
  ): Promise<ReleaseSubmission[]> {
    const params = new URLSearchParams({
      email: email,
      includeAnswers: includeAnswers.toString()
    });

    const response = await axiosInstance.get<ApiResponse<ReleaseSubmission[]>>(
      `/api/ReleaseSubmissions?${params}`
    );
    
    return response.data.data || [];
  },

  /**
   * Get sighting submissions by email
   * @param email Email address to search for
   * @param includeAnswers Whether to include answers field (default: false)
   * @returns Array of sighting submissions
   */
  async getSightingSubmissionsByEmail(
    email: string,
    includeAnswers: boolean = false
  ): Promise<SightingSubmission[]> {
    const params = new URLSearchParams({
      email: email,
      includeAnswers: includeAnswers.toString()
    });

    const response = await axiosInstance.get<ApiResponse<SightingSubmission[]>>(
      `/api/SightingSubmissions?${params}`
    );
    
    return response.data.data || [];
  },

  /**
   * Get release submissions by tag number
   * @param tagNumber Tag number to search for
   * @param includeAnswers Whether to include answers field (default: false)
   * @returns Array of release submissions
   */
  async getReleaseSubmissionsByTagNumber(
    tagNumber: string,
    includeAnswers: boolean = false
  ): Promise<ReleaseSubmission[]> {
    const params = new URLSearchParams({
      tagNumber: tagNumber,
      includeAnswers: includeAnswers.toString()
    });

    const response = await axiosInstance.get<ApiResponse<ReleaseSubmission[]>>(
      `/api/ReleaseSubmissions?${params}`
    );
    
    return response.data.data || [];
  },

  /**
   * Get sighting submissions by tag number
   * @param tagNumber Tag number to search for
   * @param includeAnswers Whether to include answers field (default: false)
   * @returns Array of sighting submissions
   */
  async getSightingSubmissionsByTagNumber(
    tagNumber: string,
    includeAnswers: boolean = false
  ): Promise<SightingSubmission[]> {
    const params = new URLSearchParams({
      tagNumber: tagNumber,
      includeAnswers: includeAnswers.toString()
    });

    const response = await axiosInstance.get<ApiResponse<SightingSubmission[]>>(
      `/api/SightingSubmissions?${params}`
    );
    
    return response.data.data || [];
  },

  /**
   * Search for tag numbers by email and return summary
   * This is a convenience method that combines release and sighting data
   * @param email Email address to search for
   * @returns Array of tag number summaries with statistics
   */
  async searchTagNumbersByEmail(email: string): Promise<TagNumberSummary[]> {
    try {
      // Fetch both release and sighting data in parallel
      const [releases, sightings] = await Promise.all([
        this.getReleaseSubmissionsByEmail(email, false),
        this.getSightingSubmissionsByEmail(email, false)
      ]);

      // Group releases by tag number
      const releasesByTag = new Map<string, ReleaseSubmission>();
      releases.forEach(release => {
        const tagNumber = release.tagNumber;
        // Keep the latest release if there are multiple
        if (!releasesByTag.has(tagNumber) || 
            (release.releaseDateTimeUtc && 
             releasesByTag.get(tagNumber)?.releaseDateTimeUtc && 
             new Date(release.releaseDateTimeUtc) > new Date(releasesByTag.get(tagNumber)!.releaseDateTimeUtc!))) {
          releasesByTag.set(tagNumber, release);
        }
      });

      // Group sightings by tag number and sort by date
      const sightingsByTag = new Map<string, SightingSubmission[]>();
      sightings.forEach(sighting => {
        const tagNumber = sighting.tagNumber;
        if (!sightingsByTag.has(tagNumber)) {
          sightingsByTag.set(tagNumber, []);
        }
        sightingsByTag.get(tagNumber)!.push(sighting);
      });

      // Sort sightings by date for each tag number
      sightingsByTag.forEach((sightingList) => {
        sightingList.sort((a, b) => {
          const dateA = a.sightingDateTimeUtc ? new Date(a.sightingDateTimeUtc).getTime() : 0;
          const dateB = b.sightingDateTimeUtc ? new Date(b.sightingDateTimeUtc).getTime() : 0;
          return dateA - dateB;
        });
      });

      // Build summary for each tag number
      const summaries: TagNumberSummary[] = [];

      releasesByTag.forEach((release, tagNumber) => {
        const tagSightings = sightingsByTag.get(tagNumber) || [];
        const lastSighting = tagSightings.length > 0 ? tagSightings[tagSightings.length - 1] : null;

        const status = getButterflyStatus(release, tagSightings);
        const survivalDays = calculateSurvivalDays(
          release.releaseDateTimeUtc,
          lastSighting?.sightingDateTimeUtc
        );

        summaries.push({
          tagNumber: tagNumber,
          releaseDate: release.releaseDateTimeUtc,
          releaseDatePretty: release.releaseDatePretty,
          lastSightingDate: lastSighting?.sightingDateTimeUtc,
          lastSightingDatePretty: lastSighting?.sightingDatePretty,
          status: status,
          sightingCount: tagSightings.length,
          survivalDays: survivalDays,
          releaseLocation: release.latitude && release.longitude ? {
            latitude: release.latitude,
            longitude: release.longitude,
            address: release.address
          } : undefined
        });
      });

      // Sort summaries by release date (newest first)
      summaries.sort((a, b) => {
        const dateA = a.releaseDate ? new Date(a.releaseDate).getTime() : 0;
        const dateB = b.releaseDate ? new Date(b.releaseDate).getTime() : 0;
        return dateB - dateA;
      });

      return summaries;
    } catch (error) {
      console.error('Error searching tag numbers by email:', error);
      throw error;
    }
  },

  /**
   * Get complete trajectory data for a tag number
   * This combines release and sighting data to provide trajectory information
   * @param tagNumber Tag number to get trajectory for
   * @returns Object containing release point and sighting points
   */
  async getTrajectoryByTagNumber(tagNumber: string): Promise<{
    release: ReleaseSubmission | null;
    sightings: SightingSubmission[];
  }> {
    try {
      const [releases, sightings] = await Promise.all([
        this.getReleaseSubmissionsByTagNumber(tagNumber, false),
        this.getSightingSubmissionsByTagNumber(tagNumber, false)
      ]);

      // Get the latest release (or first if no date)
      const release = releases.length > 0 
        ? releases.reduce((latest, current) => {
            if (!latest.releaseDateTimeUtc) return current;
            if (!current.releaseDateTimeUtc) return latest;
            return new Date(current.releaseDateTimeUtc) > new Date(latest.releaseDateTimeUtc)
              ? current
              : latest;
          })
        : null;

      // Sort sightings by date
      const sortedSightings = [...sightings].sort((a, b) => {
        const dateA = a.sightingDateTimeUtc ? new Date(a.sightingDateTimeUtc).getTime() : 0;
        const dateB = b.sightingDateTimeUtc ? new Date(b.sightingDateTimeUtc).getTime() : 0;
        return dateA - dateB;
      });

      return {
        release,
        sightings: sortedSightings
      };
    } catch (error) {
      console.error('Error getting trajectory by tag number:', error);
      throw error;
    }
  },

  /**
   * Get year in review data for a specific year
   * @param year Year to get report for (e.g., 2024)
   * @returns Year in review data
   */
  async getYearInReview(year: number): Promise<YearInReview> {
    try {
      const response = await axiosInstance.get<ApiResponse<YearInReview>>(
        `/api/YearInReview/${year}`
      );

      if (response.data.code !== 0 || !response.data.data) {
        throw new Error(response.data.message || 'Failed to get year in review data');
      }

      return response.data.data;
    } catch (error) {
      console.error('Error getting year in review:', error);
      throw error;
    }
  }
};

