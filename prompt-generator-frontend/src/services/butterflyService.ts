import axiosInstance from './axiosInstance';
import {
  ReleaseSubmission,
  SightingSubmission,
  ApiResponse,
  TagNumberSummary,
  YearInReview,
  TrajectoryPoint,
  TrajectoryForMap
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

      // Group sightings by tag number (from this email) and sort by date
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

      // Get all unique tag numbers from releases
      const allTagNumbers = Array.from(releasesByTag.keys());

      // Fetch all sightings for each tag number (not limited to this email)
      // This ensures we get the correct total sighting count
      const allSightingsByTag = new Map<string, SightingSubmission[]>();
      await Promise.all(
        allTagNumbers.map(async (tagNumber) => {
          try {
            const allSightings = await this.getSightingSubmissionsByTagNumber(tagNumber, false);
            // Sort by date
            allSightings.sort((a, b) => {
              const dateA = a.sightingDateTimeUtc ? new Date(a.sightingDateTimeUtc).getTime() : 0;
              const dateB = b.sightingDateTimeUtc ? new Date(b.sightingDateTimeUtc).getTime() : 0;
              return dateA - dateB;
            });
            allSightingsByTag.set(tagNumber, allSightings);
          } catch (error) {
            console.error(`Error fetching all sightings for tag ${tagNumber}:`, error);
            // If error, fall back to email-specific sightings
            allSightingsByTag.set(tagNumber, sightingsByTag.get(tagNumber) || []);
          }
        })
      );

      // Build summary for each tag number
      const summaries: TagNumberSummary[] = [];

      releasesByTag.forEach((release, tagNumber) => {
        // Use sightings from this email for lastSightingDate calculation
        const tagSightings = sightingsByTag.get(tagNumber) || [];
        const lastSighting = tagSightings.length > 0 ? tagSightings[tagSightings.length - 1] : null;

        // Use all sightings (from all emails) for total count
        const allTagSightings = allSightingsByTag.get(tagNumber) || [];
        // Find the actual last sighting from all sightings (not just this email)
        const actualLastSighting = allTagSightings.length > 0 
          ? allTagSightings[allTagSightings.length - 1] 
          : null;

        const status = getButterflyStatus(release, allTagSightings);
        const survivalDays = calculateSurvivalDays(
          release.releaseDateTimeUtc,
          actualLastSighting?.sightingDateTimeUtc
        );

        summaries.push({
          tagNumber: tagNumber,
          releaseDate: release.releaseDateTimeUtc,
          releaseDatePretty: release.releaseDatePretty,
          lastSightingDate: actualLastSighting?.sightingDateTimeUtc,
          lastSightingDatePretty: actualLastSighting?.sightingDatePretty,
          status: status,
          sightingCount: allTagSightings.length, // Use total count from all sightings
          survivalDays: survivalDays,
          releaseLocation: release.latitude && release.longitude ? {
            latitude: release.latitude,
            longitude: release.longitude,
            address: release.address
          } : undefined
        });
      });

      // Sort summaries by tag number (descending: largest to smallest)
      // Example: WAA148 > WAA147 > ... > WAA120
      summaries.sort((a, b) => {
        const tagA = a.tagNumber || '';
        const tagB = b.tagNumber || '';
        
        // Extract numeric part from tag number (e.g., "WAA148" -> 148)
        const extractNumber = (tag: string): number => {
          const match = tag.match(/\d+/);
          return match ? parseInt(match[0], 10) : 0;
        };
        
        // Extract prefix (letters) from tag number (e.g., "WAA148" -> "WAA")
        const extractPrefix = (tag: string): string => {
          const match = tag.match(/^[A-Za-z]+/);
          return match ? match[0].toUpperCase() : '';
        };
        
        const prefixA = extractPrefix(tagA);
        const prefixB = extractPrefix(tagB);
        const numA = extractNumber(tagA);
        const numB = extractNumber(tagB);
        
        // First compare by prefix (alphabetically)
        if (prefixA !== prefixB) {
          return prefixB.localeCompare(prefixA);
        }
        
        // If prefix is same, compare by number (descending: larger number first)
        return numB - numA;
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
  },

  /**
   * Get all unique tag numbers from the API
   * @returns Array of unique tag numbers
   */
  async getAllTagNumbers(): Promise<string[]> {
    try {
      const response = await axiosInstance.get<ApiResponse<string[]>>(
        `/api/Trajectories/tagNumbers`
      );

      if (response.data.code !== 0 || !response.data.data) {
        throw new Error(response.data.message || 'Failed to get tag numbers');
      }

      return response.data.data;
    } catch (error) {
      console.error('Error getting all tag numbers:', error);
      throw error;
    }
  },

  /**
   * Get all trajectories for overview map
   * This method gets all trajectory data with coordinates from the API
   * @returns Array of trajectory data grouped by tagNumber with colors
   */
  async getAllTrajectories(year?: number): Promise<TrajectoryForMap[]> {
    try {
      // Call the endpoint that returns all trajectory points (flat structure)
      const params = new URLSearchParams();
      if (year) {
        params.append("year", year.toString());
      }

      const response = await axiosInstance.get<ApiResponse<TrajectoryPoint[]>>(
        `/api/Trajectories/all${params.toString() ? `?${params.toString()}` : ""}`
      );

      if (response.data.code !== 0 || !response.data.data) {
        throw new Error(response.data.message || 'Failed to get all trajectories');
      }

      const allPoints = response.data.data;

      // Group points by tagNumber
      // Ensure each point is only added to its own tagNumber group
      // CRITICAL: Use exact tagNumber matching (case-sensitive) to prevent grouping errors
      const pointsByTag = new Map<string, TrajectoryPoint[]>();
      allPoints.forEach(point => {
        // Validate that point has a tagNumber
        if (!point.tagNumber || point.tagNumber.trim() === '') {
          console.warn('Point missing tagNumber, skipping:', point);
          return;
        }
        
        // Use exact tagNumber (case-sensitive, no trimming) to ensure correct grouping
        // This prevents "Wag856" and "WAE331" from being grouped together
        const tagNumber = point.tagNumber; // Keep original case and spacing
        
        if (!pointsByTag.has(tagNumber)) {
          pointsByTag.set(tagNumber, []);
        }
        
        // Double-check: ensure point's tagNumber exactly matches the group key
        if (point.tagNumber === tagNumber) {
          pointsByTag.get(tagNumber)!.push(point);
        } else {
          console.error(`CRITICAL: Point tagNumber mismatch: expected "${tagNumber}", got "${point.tagNumber}". This should never happen!`, point);
        }
      });
      
      // Debug: Log all tagNumbers to verify grouping
      console.log('Grouped trajectories by tagNumber:', Array.from(pointsByTag.keys()));
      
      // Debug: Check for specific tagNumbers mentioned by user
      if (pointsByTag.has('Wag856')) {
        const wag856Points = pointsByTag.get('Wag856')!;
        console.log('Wag856 points:', wag856Points.map(p => ({ type: p.type, tagNumber: p.tagNumber, address: p.address })));
      }
      if (pointsByTag.has('WAE331')) {
        const wae331Points = pointsByTag.get('WAE331')!;
        console.log('WAE331 points:', wae331Points.map(p => ({ type: p.type, tagNumber: p.tagNumber, address: p.address })));
      }

      // Generate colors for each tagNumber
      const colors = [
        "#FF6B35", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8",
        "#F7DC6F", "#BB8FCE", "#85C1E2", "#F8B739", "#E74C3C",
        "#3498DB", "#2ECC71", "#9B59B6", "#E67E22", "#1ABC9C",
        "#F39C12", "#E91E63", "#00BCD4", "#8BC34A", "#FF9800"
      ];

      // Helper function to validate coordinates are within New Zealand bounds
      const isWithinNewZealandBounds = (lat: number, lng: number): boolean => {
        // New Zealand bounds: Lat: -47.5 to -33.5, Lng: 166.0 to 179.0
        const inNZBounds = lat >= -47.5 && lat <= -33.5 &&
                           lng >= 166.0 && lng <= 179.0;
        
        if (!inNZBounds) {
          return false;
        }
        
        // Must NOT be in Australia or other nearby countries
        // Australia bounds: Lat: -44.0 to -10.0, Lng: 113.0 to 154.0
        const inAustraliaBounds = lat >= -44.0 && lat <= -10.0 &&
                                  lng >= 113.0 && lng <= 154.0;
        
        if (inAustraliaBounds) {
          return false;
        }
        
        return true;
      };

      // Convert to map format with colors
      const tagNumbers = Array.from(pointsByTag.keys());
      return tagNumbers.map((tagNumber, index) => {
        const points = pointsByTag.get(tagNumber)!;
        
        // Separate release and sighting points
        // Business logic: A butterfly is only released once, so each tagNumber should have only one release point
        // If multiple release points exist (data quality issue), we'll take the first one
        // CRITICAL: Filter to ensure all points belong to this tagNumber
        const releasePoints = points.filter(p => p.type === 1 && p.tagNumber === tagNumber);
        const sightingPoints = points.filter(p => p.type === 2 && p.tagNumber === tagNumber);
        
        // Log warning if any points don't match the tagNumber (should not happen)
        const mismatchedPoints = points.filter(p => p.tagNumber !== tagNumber);
        if (mismatchedPoints.length > 0) {
          console.warn(`Found ${mismatchedPoints.length} point(s) with mismatched tagNumber for group ${tagNumber}:`, mismatchedPoints);
        }

        // Warn if multiple release points found (should not happen based on backend logic)
        if (releasePoints.length > 1) {
          console.warn(`Multiple release points found for tagNumber ${tagNumber}. Taking the first one.`);
        }

        // Get the first release point (should only be one based on backend deduplication)
        // Also validate coordinates are within New Zealand bounds (frontend validation)
        const releasePoint = releasePoints.length > 0 && isWithinNewZealandBounds(releasePoints[0].latitude, releasePoints[0].longitude)
          ? {
              lat: releasePoints[0].latitude,
              lng: releasePoints[0].longitude,
              label: releasePoints[0].address || `Release Point`,
              description: undefined,
              type: "release" as const,
              date: undefined,
              tagNumber: tagNumber
            }
          : undefined;

        // Convert sighting points
        // Business logic: A tagNumber can be sighted multiple times, so keep all sighting points
        // Filter out points that are not within New Zealand bounds AND ensure tagNumber matches
        const sightingPointsForMap = sightingPoints
          .filter(point => {
            // CRITICAL: Ensure point belongs to this tagNumber
            if (point.tagNumber !== tagNumber) {
              console.warn(`Sighting point tagNumber mismatch: expected ${tagNumber}, got ${point.tagNumber}`);
              return false;
            }
            return isWithinNewZealandBounds(point.latitude, point.longitude);
          })
          .map((point, idx) => ({
            lat: point.latitude,
            lng: point.longitude,
            label: point.address || `Sighting Point ${idx + 1}`,
            description: undefined,
            type: "sighting" as const,
            date: undefined,
            tagNumber: tagNumber // Explicitly set to ensure consistency
          }));

        // Final validation: Ensure all points in this trajectory belong to the same tagNumber
        const allTrajectoryPoints = [
          ...(releasePoint ? [releasePoint] : []),
          ...sightingPointsForMap
        ];
        
        const mismatchedInTrajectory = allTrajectoryPoints.filter(p => p.tagNumber !== tagNumber);
        if (mismatchedInTrajectory.length > 0) {
          console.error(`CRITICAL ERROR: Trajectory for ${tagNumber} contains points with different tagNumbers:`, mismatchedInTrajectory);
        }
        
        return {
          tagNumber: tagNumber,
          releasePoint: releasePoint,
          sightingPoints: sightingPointsForMap,
          color: colors[index % colors.length]
        };
      });
    } catch (error) {
      console.error('Error getting all trajectories:', error);
      throw error;
    }
  },

  /**
   * Get all trajectories by collecting data from all unique tagNumbers
   * This is a workaround method that requires knowing all tagNumbers
   * @param tagNumbers Array of tagNumbers to get trajectories for
   * @returns Array of trajectory data
   */
  async getTrajectoriesByTagNumbers(tagNumbers: string[]): Promise<Array<{
    tagNumber: string;
    releasePoint?: {
      lat: number;
      lng: number;
      label: string;
      description?: string;
      type: "release";
      date?: string;
      tagNumber: string;
    };
    sightingPoints: Array<{
      lat: number;
      lng: number;
      label: string;
      description?: string;
      type: "sighting";
      date?: string;
      tagNumber: string;
    }>;
    color: string;
  }>> {
    try {
      // Generate colors for each tagNumber
      const colors = [
        "#FF6B35", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8",
        "#F7DC6F", "#BB8FCE", "#85C1E2", "#F8B739", "#E74C3C",
        "#3498DB", "#2ECC71", "#9B59B6", "#E67E22", "#1ABC9C",
        "#F39C12", "#E91E63", "#00BCD4", "#8BC34A", "#FF9800"
      ];

      // Get trajectories for all tagNumbers in parallel
      const trajectoryPromises = tagNumbers.map(async (tagNumber, index) => {
        try {
          const trajectory = await this.getTrajectoryByTagNumber(tagNumber);
          
          // Convert to map format
          const releasePoint = trajectory.release && trajectory.release.latitude && trajectory.release.longitude
            ? {
                lat: trajectory.release.latitude,
                lng: trajectory.release.longitude,
                label: trajectory.release.address || `Release Point`,
                description: trajectory.release.notes,
                type: "release" as const,
                date: trajectory.release.releaseDatePretty || (trajectory.release.releaseDateTimeUtc ? new Date(trajectory.release.releaseDateTimeUtc).toLocaleDateString('en-US') : undefined),
                tagNumber: tagNumber
              }
            : undefined;

          const sightingPoints = trajectory.sightings
            .filter(s => s.latitude && s.longitude)
            .map((sighting, idx) => ({
              lat: sighting.latitude!,
              lng: sighting.longitude!,
              label: sighting.address || `Sighting Point ${idx + 1}`,
              description: sighting.condition,
              type: "sighting" as const,
              date: sighting.sightingDatePretty || (sighting.sightingDateTimeUtc ? new Date(sighting.sightingDateTimeUtc).toLocaleDateString('en-US') : undefined),
              tagNumber: tagNumber
            }));

          // Only return if there's at least one point with coordinates
          if (releasePoint || sightingPoints.length > 0) {
            return {
              tagNumber: tagNumber,
              releasePoint: releasePoint,
              sightingPoints: sightingPoints,
              color: colors[index % colors.length]
            };
          }
          return null;
        } catch (error) {
          console.error(`Error getting trajectory for tag ${tagNumber}:`, error);
          return null;
        }
      });

      const trajectories = await Promise.all(trajectoryPromises);
      
      // Filter out null results
      return trajectories.filter((t): t is NonNullable<typeof t> => t !== null);
    } catch (error) {
      console.error('Error getting trajectories by tag numbers:', error);
      throw error;
    }
  }
};

