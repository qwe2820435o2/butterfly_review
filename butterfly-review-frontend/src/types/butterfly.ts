// Butterfly related types based on backend API

/**
 * Jotform Answer Raw DTO (simplified for frontend)
 */
export interface JotformAnswerRawDto {
  name?: string;
  order?: string;
  text?: string;
  type?: string;
  answer?: unknown; // Can be string, number, object, array, etc.
  cfname?: string;
  prettyFormat?: string;
  sublabels?: string;
}

/**
 * Base submission fields (common to both Release and Sighting)
 */
export interface JotformSubmissionBase {
  id?: string;
  submissionId: string;
  formId: string;
  ip?: string;
  status: string;
  createdAtRaw: string;
  createdAtUtc: string;
  updatedAtRaw?: string;
  insertedAtUtc: string;
  updatedAtUtc?: string;
  answers?: Record<string, JotformAnswerRawDto>;
}

/**
 * Release Submission (Butterfly Release Record)
 */
export interface ReleaseSubmission extends JotformSubmissionBase {
  email?: string;
  tagNumber: string;
  releaseDateTimeUtc?: string;
  releaseDatePretty?: string;
  notes?: string;
  wind?: string;
  sex?: string;
  sun?: string;
  latitude?: number;
  longitude?: number;
  address?: string;
  mapLocatorRaw?: string;
  gpsLocationRaw?: string;
}

/**
 * Sighting Submission (Butterfly Sighting Record)
 */
export interface SightingSubmission extends JotformSubmissionBase {
  email?: string;
  name?: string;
  phone?: string;
  tagNumber: string;
  sightingDateTimeUtc?: string;
  sightingDatePretty?: string;
  condition?: string;
  deadOrAlive?: string;
  nearbyButterflies?: string;
  nearbyPlants?: string;
  latitude?: number;
  longitude?: number;
  address?: string;
  mapLocatorRaw?: string;
  gpsLocationRaw?: string;
  howSunny?: string;
  howWindy?: string;
}

/**
 * API Response wrapper
 */
export interface ApiResponse<T> {
  code: number;
  message: string;
  data?: T;
}

/**
 * Extended Release Submission with statistics
 * Used for search results display
 * Note: Uses Omit to override the status property from base interface
 */
export interface ReleaseSubmissionWithStats extends Omit<ReleaseSubmission, 'status'> {
  // Statistics calculated on the frontend or backend
  lastSightingDate?: string;
  sightingCount?: number;
  status?: 'Alive' | 'Dead' | 'Unknown';
  survivalDays?: number;
}

/**
 * Tag Number Summary
 * Used for displaying search results
 */
export interface TagNumberSummary {
  tagNumber: string;
  releaseDate?: string;
  releaseDatePretty?: string;
  lastSightingDate?: string;
  lastSightingDatePretty?: string;
  status: 'Alive' | 'Dead' | 'Unknown';
  sightingCount: number;
  survivalDays?: number;
  releaseLocation?: {
    latitude: number;
    longitude: number;
    address?: string;
  };
}

/**
 * True when API release status indicates a soft-deleted submission (any casing, trimmed).
 */
export const isReleaseSoftDeleted = (status: string | undefined): boolean =>
  typeof status === 'string' &&
  status.trim().length > 0 &&
  status.trim().toLowerCase() === 'deleted';

/**
 * Helper function to determine butterfly status
 */
export const getButterflyStatus = (
  release: ReleaseSubmission,
  sightings: SightingSubmission[]
): 'Alive' | 'Dead' | 'Unknown' => {
  if (isReleaseSoftDeleted(release.status)) {
    return 'Unknown';
  }

  if (sightings.length === 0) {
    return 'Unknown';
  }

  const lastSighting = sightings[sightings.length - 1];
  if (lastSighting.deadOrAlive === 'Dead' || lastSighting.deadOrAlive === 'dead') {
    return 'Dead';
  }

  if (lastSighting.deadOrAlive === 'Alive' || lastSighting.deadOrAlive === 'alive') {
    return 'Alive';
  }

  return 'Unknown';
};

/**
 * Helper function to calculate survival days
 */
export const calculateSurvivalDays = (
  releaseDate: string | undefined,
  lastSightingDate: string | undefined
): number | undefined => {
  if (!releaseDate || !lastSightingDate) {
    return undefined;
  }

  const release = new Date(releaseDate);
  const lastSighting = new Date(lastSightingDate);
  const diffTime = Math.abs(lastSighting.getTime() - release.getTime());
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays;
};

/**
 * Helper function to format date for display
 */
export const formatDate = (dateString: string | undefined): string => {
  if (!dateString) {
    return 'Unknown';
  }

  try {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  } catch {
    return dateString;
  }
};

/**
 * Helper function to format date with time for display
 */
export const formatDateTime = (dateString: string | undefined): string => {
  if (!dateString) {
    return 'Unknown';
  }

  try {
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  } catch {
    return dateString;
  }
};

/**
 * Helper function to get status display text and color
 */
export const getStatusDisplay = (status: 'Alive' | 'Dead' | 'Unknown'): {
  text: string;
  color: string;
  bgColor: string;
} => {
  switch (status) {
    case 'Alive':
      return {
        text: 'Alive',
        color: 'text-green-600 dark:text-green-400',
        bgColor: 'bg-green-100 dark:bg-green-900/30'
      };
    case 'Dead':
      return {
        text: 'Dead',
        color: 'text-red-600 dark:text-red-400',
        bgColor: 'bg-red-100 dark:bg-red-900/30'
      };
    case 'Unknown':
    default:
      return {
        text: 'Unknown',
        color: 'text-gray-600 dark:text-gray-400',
        bgColor: 'bg-gray-100 dark:bg-gray-800'
      };
  }
};

/**
 * Year in Review Types (flattened structure)
 */
export interface YearInReview {
  year: number;
  totalReleases: number;
  totalSightings: number;
  uniqueVolunteers: number;
  uniqueRegions: number;
  averageFlightDistanceKm?: number;
  averageDaysToFirstSighting?: number;
  releaseLocationsCount: number;
  sightingLocationsCount: number;
  mostActiveReleaseLocationAddress?: string;
  mostActiveReleaseLocationCount: number;
  mostActiveSightingLocationAddress?: string;
  mostActiveSightingLocationCount: number;
}

export interface GeographicDistribution {
  releaseLocations: LocationPoint[];
  sightingLocations: LocationPoint[];
  mostActiveReleaseLocation?: LocationWithCount;
  mostActiveSightingLocation?: LocationWithCount;
  bounds?: GeographicBounds;
}

export interface LocationPoint {
  latitude: number;
  longitude: number;
  address?: string;
  date?: string;
}

export interface LocationWithCount {
  latitude: number;
  longitude: number;
  address?: string;
  count: number;
}

export interface GeographicBounds {
  minLatitude: number;
  maxLatitude: number;
  minLongitude: number;
  maxLongitude: number;
}

/**
 * Trajectory Overview Types
 * Flat structure: each point is a separate object with tagNumber
 */
export interface TrajectoryPoint {
  tagNumber: string;
  type: number; // 1 = Release, 2 = Sighting
  latitude: number;
  longitude: number;
  address?: string;
}

export interface TrajectoryForMap {
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
}

