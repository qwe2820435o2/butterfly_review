"use client";

import React, { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft, MapPin, Calendar, Eye, Clock, AlertCircle } from "lucide-react";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import dynamic from "next/dynamic";
import { butterflyService } from "@/services/butterflyService";
import { ReleaseSubmission, SightingSubmission, formatDate, getStatusDisplay, calculateSurvivalDays } from "@/types/butterfly";

// Dynamically import Google Maps component to avoid SSR issues
const GoogleButterflyMap = dynamic(
  () => import("@/components/butterfly/GoogleButterflyMap"),
  { 
    ssr: false,
    loading: () => (
      <div className="h-[600px] w-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 rounded-lg">
        <p className="text-gray-500 dark:text-gray-400">Loading map...</p>
      </div>
    )
  }
);

interface MapPoint {
  lat: number;
  lng: number;
  label: string;
  address?: string; // Address for geocoding (priority over lat/lng)
  description?: string;
  type: "release" | "sighting";
  date?: string;
}

/**
 * Parse coordinates from gpsLocationRaw string
 * Supports two patterns:
 * 1. "Longitude: 176.9123\nLatitude: -39.4926"
 * 2. "... \n-39.09176, 174.10500"
 */
function parseCoordinatesFromGpsLocationRaw(gpsLocationRaw: string | undefined): { lat: number | null; lng: number | null } {
  if (!gpsLocationRaw || !gpsLocationRaw.trim()) {
    return { lat: null, lng: null };
  }

  const lower = gpsLocationRaw.toLowerCase();
  
  // Pattern 1: "Longitude: 176.9123\nLatitude: -39.4926"
  if (lower.includes("longitude") && lower.includes("latitude")) {
    const lines = gpsLocationRaw.split('\n').filter(line => line.trim());
    let lat: number | null = null;
    let lng: number | null = null;

    for (const line of lines) {
      // Parse Longitude
      if (line.toLowerCase().includes("longitude")) {
        const match = line.match(/longitude\s*:?\s*(-?\d+\.?\d*)/i);
        if (match) {
          const parsed = parseFloat(match[1]);
          if (!isNaN(parsed)) {
            lng = parsed;
          }
        }
      }

      // Parse Latitude
      if (line.toLowerCase().includes("latitude")) {
        const match = line.match(/latitude\s*:?\s*(-?\d+\.?\d*)/i);
        if (match) {
          const parsed = parseFloat(match[1]);
          if (!isNaN(parsed)) {
            lat = parsed;
          }
        }
      }
    }

    if (lat !== null && lng !== null) {
      return { lat, lng };
    }
  }

  // Pattern 2: "... \n-39.09176, 174.10500"
  const parts = gpsLocationRaw.split('\n').filter(part => part.trim());
  if (parts.length > 0) {
    const last = parts[parts.length - 1].trim();
    const coordParts = last.split(',').map(p => p.trim());
    
    if (coordParts.length === 2) {
      const lat2 = parseFloat(coordParts[0]);
      const lng2 = parseFloat(coordParts[1]);
      
      if (!isNaN(lat2) && !isNaN(lng2)) {
        return { lat: lat2, lng: lng2 };
      }
    }
  }

  return { lat: null, lng: null };
}

/**
 * Get coordinates from release or sighting submission
 * Priority: gpsLocationRaw > latitude/longitude fields
 */
function getCoordinates(
  gpsLocationRaw: string | undefined,
  latitude: number | undefined,
  longitude: number | undefined
): { lat: number | null; lng: number | null } {
  // First try to parse from gpsLocationRaw
  if (gpsLocationRaw) {
    const coords = parseCoordinatesFromGpsLocationRaw(gpsLocationRaw);
    if (coords.lat !== null && coords.lng !== null) {
      return coords;
    }
  }

  // Fall back to latitude/longitude fields
  if (latitude !== undefined && longitude !== undefined) {
    return { lat: latitude, lng: longitude };
  }

  return { lat: null, lng: null };
}

export default function TrajectoryPage() {
  const params = useParams();
  const dispatch = useDispatch();
  const tagNumber = params.tagNumber as string;

  const [release, setRelease] = useState<ReleaseSubmission | null>(null);
  const [sightings, setSightings] = useState<SightingSubmission[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (tagNumber) {
      loadTrajectoryData();
    }
  }, [tagNumber]);

  const loadTrajectoryData = async () => {
    try {
      setIsLoading(true);
      setError(null);
      dispatch(showLoading());

      const data = await butterflyService.getTrajectoryByTagNumber(tagNumber);
      setRelease(data.release);
      setSightings(data.sightings);

      if (!data.release && data.sightings.length === 0) {
        setError("No records found for this tag number");
        toast.info("No records found", {
          description: `Tag number ${tagNumber} has no associated release or sighting records`
        });
      }
    } catch (err) {
      console.error("Load trajectory error:", err);
      setError("Failed to load trajectory data");
      
      let errorMessage = "Failed to load, please try again later";
      if (err && typeof err === "object" && "isAxiosError" in err) {
        const axiosError = err as AxiosError;
        if (axiosError.response?.status === 404) {
          errorMessage = "No records found for this tag number";
        }
      }
      
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
      dispatch(hideLoading());
    }
  };

  // Convert data to map points (include address for geocoding)
  // Priority: Use coordinates from gpsLocationRaw, fall back to latitude/longitude fields
  const getMapPoints = (): { releasePoint?: MapPoint; sightingPoints: MapPoint[] } => {
    let releasePoint: MapPoint | undefined = undefined;
    
    if (release) {
      // Get coordinates from gpsLocationRaw first, then fall back to latitude/longitude
      const coords = getCoordinates(release.gpsLocationRaw, release.latitude, release.longitude);
      
      // Debug: Log release coordinates
      console.log('Release coordinates:', {
        gpsLocationRaw: release.gpsLocationRaw,
        latitude: release.latitude,
        longitude: release.longitude,
        parsedCoords: coords,
        address: release.address
      });
      
      if (coords.lat !== null && coords.lng !== null) {
        releasePoint = {
          lat: coords.lat,
          lng: coords.lng,
          label: release.address || `Release Point`,
          address: release.address, // Include address for geocoding
          description: release.notes,
          type: "release",
          date: release.releaseDatePretty || formatDate(release.releaseDateTimeUtc)
        };
        console.log('Created release point with coordinates:', releasePoint);
      } else if (release.address) {
        // If no coordinates but has address, still create point (will be geocoded)
        releasePoint = {
          lat: 0, // Will be updated by geocoding
          lng: 0,
          label: release.address,
          address: release.address,
          description: release.notes,
          type: "release",
          date: release.releaseDatePretty || formatDate(release.releaseDateTimeUtc)
        };
        console.log('Created release point with address only (will be geocoded):', releasePoint);
      } else {
        console.warn('Release point has no coordinates and no address:', release);
      }
    } else {
      console.warn('No release data available');
    }

    const sightingPoints: MapPoint[] = sightings
      .map((sighting, index) => {
        // Get coordinates from gpsLocationRaw first, then fall back to latitude/longitude
        const coords = getCoordinates(sighting.gpsLocationRaw, sighting.latitude, sighting.longitude);
        
        // Only include if we have coordinates or address
        if (coords.lat !== null && coords.lng !== null) {
          return {
            lat: coords.lat,
            lng: coords.lng,
            label: sighting.address || `Sighting Point ${index + 1}`,
            address: sighting.address, // Include address for geocoding
            description: sighting.condition,
            type: "sighting" as const,
            date: sighting.sightingDatePretty || formatDate(sighting.sightingDateTimeUtc)
          };
        } else if (sighting.address) {
          // If no coordinates but has address, still create point (will be geocoded)
          return {
            lat: 0, // Will be updated by geocoding
            lng: 0,
            label: sighting.address,
            address: sighting.address,
            description: sighting.condition,
            type: "sighting" as const,
            date: sighting.sightingDatePretty || formatDate(sighting.sightingDateTimeUtc)
          };
        }
        return null;
      })
      .filter((point): point is MapPoint => point !== null);

    return { releasePoint, sightingPoints };
  };

  const { releasePoint, sightingPoints } = getMapPoints();
  const survivalDays = release && sightings.length > 0
    ? calculateSurvivalDays(
        release.releaseDateTimeUtc,
        sightings[sightings.length - 1].sightingDateTimeUtc
      )
    : undefined;
  const status = release && sightings.length > 0
    ? getStatusDisplay(
        sightings[sightings.length - 1].deadOrAlive === "Dead" || sightings[sightings.length - 1].deadOrAlive === "dead"
          ? "Dead"
          : sightings[sightings.length - 1].deadOrAlive === "Alive" || sightings[sightings.length - 1].deadOrAlive === "alive"
          ? "Alive"
          : "Unknown"
      )
    : null;

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
        <div className="container mx-auto px-4 py-16">
          <div className="flex items-center justify-center min-h-[400px]">
            <div className="text-center">
              <p className="text-gray-600 dark:text-gray-300">Loading trajectory data...</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || (!release && sightings.length === 0)) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
        <div className="container mx-auto px-4 py-16">
          <Card className="max-w-2xl mx-auto">
            <CardContent className="py-12 text-center">
              <div className="flex flex-col items-center justify-center space-y-4">
                <div className="w-16 h-16 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center">
                  <AlertCircle className="w-8 h-8 text-gray-400" />
                </div>
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                    {error || "No Records Found"}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-6">
                    Tag number <span className="font-mono font-semibold">{tagNumber}</span> has no associated release or sighting records
                  </p>
                  <Link href="/search">
                    <Button variant="outline">
                      <ArrowLeft className="w-4 h-4 mr-2" />
                      Back to Search
                    </Button>
                  </Link>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-6">
          <Link href="/search">
            <Button variant="ghost" className="mb-4">
              <ArrowLeft className="w-4 h-4 mr-2" />
              Back to Search
            </Button>
          </Link>
          <h1 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-2">
            Trajectory of {tagNumber}
          </h1>
          <p className="text-gray-600 dark:text-gray-300">
            View the complete flight path from release to sighting
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Map Section */}
          <div className="lg:col-span-2">
            <Card className="shadow-lg">
              <CardHeader>
                <CardTitle>Flight Trajectory Map</CardTitle>
                <CardDescription>
                  Green markers: Release points | Red markers: Sighting points | Orange lines: Flight paths
                </CardDescription>
              </CardHeader>
              <CardContent>
                <GoogleButterflyMap
                  releasePoint={releasePoint}
                  sightingPoints={sightingPoints}
                  className="h-[600px] w-full rounded-lg"
                />
              </CardContent>
            </Card>
          </div>

          {/* Info Sidebar */}
          <div className="space-y-6">
            {/* Summary Card */}
            <Card className="shadow-lg">
              <CardHeader>
                <CardTitle>Tag Information</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Tag Number</p>
                  <p className="text-lg font-mono font-semibold text-gray-900 dark:text-white">
                    {tagNumber}
                  </p>
                </div>

                {status && (
                  <div>
                    <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">Status</p>
                    <Badge className={`${status.bgColor} ${status.color} border-0`}>
                      {status.text}
                    </Badge>
                  </div>
                )}

                {release?.releaseDateTimeUtc && (
                  <div className="flex items-start">
                    <Calendar className="w-4 h-4 mr-2 text-orange-600 dark:text-orange-400 mt-0.5 flex-shrink-0" />
                    <div>
                      <p className="text-sm text-gray-600 dark:text-gray-400">Release Date</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-white">
                        {formatDate(release.releaseDateTimeUtc)}
                      </p>
                    </div>
                  </div>
                )}

                {sightings.length > 0 && sightings[sightings.length - 1].sightingDateTimeUtc && (
                  <div className="flex items-start">
                    <Eye className="w-4 h-4 mr-2 text-blue-600 dark:text-blue-400 mt-0.5 flex-shrink-0" />
                    <div>
                      <p className="text-sm text-gray-600 dark:text-gray-400">Last Sighting</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-white">
                        {formatDate(sightings[sightings.length - 1].sightingDateTimeUtc)}
                      </p>
                    </div>
                  </div>
                )}

                {survivalDays !== undefined && (
                  <div className="flex items-start">
                    <Clock className="w-4 h-4 mr-2 text-green-600 dark:text-green-400 mt-0.5 flex-shrink-0" />
                    <div>
                      <p className="text-sm text-gray-600 dark:text-gray-400">Survival Days</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-white">
                        {survivalDays} day(s)
                      </p>
                    </div>
                  </div>
                )}

                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Sighting Count</p>
                  <p className="text-lg font-semibold text-purple-600 dark:text-purple-400">
                    {sightings.length} time(s)
                  </p>
                </div>
              </CardContent>
            </Card>

            {/* Release Details */}
            {release && (
              <Card className="shadow-lg">
                <CardHeader>
                  <CardTitle>Release Details</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  {release.address && (
                    <div>
                      <p className="text-gray-600 dark:text-gray-400">Location</p>
                      <p className="text-gray-900 dark:text-white">{release.address}</p>
                    </div>
                  )}
                  {release.notes && (
                    <div>
                      <p className="text-gray-600 dark:text-gray-400">Notes</p>
                      <p className="text-gray-900 dark:text-white">{release.notes}</p>
                    </div>
                  )}
                </CardContent>
              </Card>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

