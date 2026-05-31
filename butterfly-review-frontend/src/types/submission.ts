// Submission types mirroring the butterfly-review-api entities.

export interface ReleaseSubmission {
  id: string;
  submissionId: string;
  email?: string | null;
  tagNumber: string;
  releaseDateTimeUtc?: string | null;
  releaseDatePretty?: string | null;
  notes?: string | null;
  wind?: string | null;
  sex?: string | null;
  sun?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  address?: string | null;
  status: string;
  createdAtUtc: string;
}

// Editable subset sent to the admin create/update endpoints.
export interface ReleaseSubmissionInput {
  tagNumber: string;
  email?: string | null;
  releaseDateTimeUtc?: string | null;
  notes?: string | null;
  wind?: string | null;
  sex?: string | null;
  sun?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  address?: string | null;
}

export interface SightingSubmissionInput {
  tagNumber: string;
  email?: string | null;
  name?: string | null;
  phone?: string | null;
  sightingDateTimeUtc?: string | null;
  condition?: string | null;
  deadOrAlive?: string | null;
  nearbyButterflies?: string | null;
  nearbyPlants?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  address?: string | null;
  howSunny?: string | null;
  howWindy?: string | null;
}

export interface SightingSubmission {
  id: string;
  submissionId: string;
  email?: string | null;
  name?: string | null;
  phone?: string | null;
  tagNumber: string;
  sightingDateTimeUtc?: string | null;
  sightingDatePretty?: string | null;
  condition?: string | null;
  deadOrAlive?: string | null;
  nearbyButterflies?: string | null;
  nearbyPlants?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  address?: string | null;
  howSunny?: string | null;
  howWindy?: string | null;
  status: string;
  createdAtUtc: string;
}
