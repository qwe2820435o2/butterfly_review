"use client";

import React, { useMemo, useState, useEffect, useCallback } from "react";
import { GoogleMap, useLoadScript, Marker, Polyline, InfoWindow } from "@react-google-maps/api";

// Extend Window interface for Google Maps
declare global {
  interface Window {
    google: typeof google;
  }
}

interface TrajectoryPoint {
  lat: number;
  lng: number;
  label: string;
  description?: string;
  type: "release" | "sighting";
  date?: string;
  tagNumber: string;
}

interface Trajectory {
  tagNumber: string;
  releasePoint?: TrajectoryPoint;
  sightingPoints: TrajectoryPoint[];
  color: string;
}

// New simplified structure from API
interface TrajectoryPointFromApi {
  type: number; // 1 = Release, 2 = Sighting
  latitude: number;
  longitude: number;
  address?: string;
}

interface TrajectoryFromApi {
  tagNumber: string;
  points: TrajectoryPointFromApi[];
}

interface OverviewMapProps {
  trajectories: Trajectory[];
  className?: string;
  apiKey?: string;
}

// Map container style
const mapContainerStyle = {
  width: "100%",
  height: "100%",
};

// Default center (Auckland, New Zealand)
const defaultCenter = {
  lat: -36.8485,
  lng: 174.7633,
};

/**
 * Overview Map Component
 * Displays multiple trajectories on a single map with color coding
 */
// Helper function to group points by location (with tolerance for floating point precision)
const groupPointsByLocation = (points: TrajectoryPoint[], tolerance: number = 0.0001) => {
  const groups: Map<string, TrajectoryPoint[]> = new Map();
  
  points.forEach(point => {
    // Round coordinates to create location key
    const latKey = Math.round(point.lat / tolerance) * tolerance;
    const lngKey = Math.round(point.lng / tolerance) * tolerance;
    const locationKey = `${latKey.toFixed(6)},${lngKey.toFixed(6)}`;
    
    if (!groups.has(locationKey)) {
      groups.set(locationKey, []);
    }
    groups.get(locationKey)!.push(point);
  });
  
  return groups;
};

export default function OverviewMap({
  trajectories = [],
  className = "h-[600px] w-full",
  apiKey = process.env.NEXT_PUBLIC_GOOGLE_MAPS_API_KEY || "AIzaSyAl3nBJPlQzHAje4DbjISunBDuoVU6P2ZE"
}: OverviewMapProps) {
  const [selectedLocation, setSelectedLocation] = useState<{
    position: { lat: number; lng: number };
    points: TrajectoryPoint[];
  } | null>(null);
  const [map, setMap] = useState<google.maps.Map | null>(null);

  // Load Google Maps script
  const { isLoaded, loadError } = useLoadScript({
    googleMapsApiKey: apiKey,
  });

  // Calculate center point from all trajectories
  const center = useMemo(() => {
    const allPoints: { lat: number; lng: number }[] = [];
    
    trajectories.forEach(trajectory => {
      if (trajectory.releasePoint) {
        allPoints.push({ lat: trajectory.releasePoint.lat, lng: trajectory.releasePoint.lng });
      }
      trajectory.sightingPoints.forEach(point => {
        allPoints.push({ lat: point.lat, lng: point.lng });
      });
    });

    if (allPoints.length === 0) return defaultCenter;

    const avgLat = allPoints.reduce((sum, p) => sum + p.lat, 0) / allPoints.length;
    const avgLng = allPoints.reduce((sum, p) => sum + p.lng, 0) / allPoints.length;
    
    return { lat: avgLat, lng: avgLng };
  }, [trajectories]);

  // Calculate bounds to fit all points
  const bounds = useMemo(() => {
    const allPoints: { lat: number; lng: number }[] = [];
    
    trajectories.forEach(trajectory => {
      if (trajectory.releasePoint) {
        allPoints.push({ lat: trajectory.releasePoint.lat, lng: trajectory.releasePoint.lng });
      }
      trajectory.sightingPoints.forEach(point => {
        allPoints.push({ lat: point.lat, lng: point.lng });
      });
    });

    if (allPoints.length === 0) return null;

    const lats = allPoints.map(p => p.lat);
    const lngs = allPoints.map(p => p.lng);

    return {
      north: Math.max(...lats),
      south: Math.min(...lats),
      east: Math.max(...lngs),
      west: Math.min(...lngs),
    };
  }, [trajectories]);

  // Fit bounds when map is ready
  useEffect(() => {
    if (map && bounds && isLoaded && typeof google !== 'undefined') {
      const boundsObj = new google.maps.LatLngBounds(
        new google.maps.LatLng(bounds.south, bounds.west),
        new google.maps.LatLng(bounds.north, bounds.east)
      );
      map.fitBounds(boundsObj);
    }
  }, [map, bounds, isLoaded]);

  // Handle map load
  const onMapLoad = useCallback((mapInstance: google.maps.Map) => {
    setMap(mapInstance);
    // Fit bounds if available
    if (bounds && typeof google !== 'undefined') {
      const boundsObj = new google.maps.LatLngBounds(
        new google.maps.LatLng(bounds.south, bounds.west),
        new google.maps.LatLng(bounds.north, bounds.east)
      );
      mapInstance.fitBounds(boundsObj);
    }
  }, [bounds]);

  // Collect all points and group by location
  const allPointsByLocation = useMemo(() => {
    const allPoints: TrajectoryPoint[] = [];
    
    trajectories.forEach(trajectory => {
      if (trajectory.releasePoint && trajectory.releasePoint.tagNumber === trajectory.tagNumber) {
        allPoints.push(trajectory.releasePoint);
      }
      trajectory.sightingPoints
        .filter(point => point.tagNumber === trajectory.tagNumber)
        .forEach(point => allPoints.push(point));
    });
    
    return groupPointsByLocation(allPoints);
  }, [trajectories]);

  // Create marker icon based on type and count
  const createMarkerIcon = useCallback((
    type: "release" | "sighting", 
    color: string,
    count: number = 1
  ) => {
    if (!isLoaded || typeof google === 'undefined') return undefined;
    
    // Use different icons for release (green) and sighting (red)
    const iconUrl = type === "release"
      ? "https://maps.google.com/mapfiles/ms/icons/green-dot.png"
      : "https://maps.google.com/mapfiles/ms/icons/red-dot.png";
    
    // If multiple points at same location, make marker slightly larger
    const size = count > 1 ? 45 : 40;
    
    return {
      url: iconUrl,
      scaledSize: new google.maps.Size(size, size),
    };
  }, [isLoaded]);

  // Create polyline options for trajectory
  const createPolylineOptions = useCallback((color: string) => {
    if (!isLoaded || typeof google === 'undefined') return null;
    return {
      strokeColor: color,
      strokeOpacity: 0.7,
      strokeWeight: 3,
      geodesic: true,
      icons: [
        {
          icon: {
            path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
            scale: 3,
            strokeColor: color,
          },
          offset: "100%",
          repeat: "200px",
        },
      ],
    };
  }, [isLoaded]);

  // Loading state
  if (!isLoaded) {
    return (
      <div className={className} style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <div className="text-gray-500">Loading map...</div>
      </div>
    );
  }

  // Error state
  if (loadError) {
    return (
      <div className={className} style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', flexDirection: 'column' }}>
        <div className="text-red-500 mb-2">Error loading Google Maps</div>
        <div className="text-sm text-gray-500">{loadError.message}</div>
      </div>
    );
  }

  // If no API key, show error message
  if (!apiKey) {
    return (
      <div className={className} style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', flexDirection: 'column' }}>
        <div className="text-red-500 mb-2">Google Maps API Key is required</div>
        <div className="text-sm text-gray-500">Please set NEXT_PUBLIC_GOOGLE_MAPS_API_KEY in your environment variables</div>
      </div>
    );
  }

  return (
    <div className={className} style={{ position: 'relative' }}>
      {/* Legend */}
      <div className="absolute top-4 right-4 z-10 bg-white rounded-lg shadow-lg p-4 border border-gray-200">
        <h4 className="font-semibold text-sm mb-3 text-gray-800">Legend</h4>
        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <div className="w-6 h-6 flex items-center justify-center">
              <img 
                src="https://maps.google.com/mapfiles/ms/icons/green-dot.png" 
                alt="Release Point"
                className="w-6 h-6"
              />
            </div>
            <span className="text-sm text-gray-700">Release Point</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-6 h-6 flex items-center justify-center">
              <img 
                src="https://maps.google.com/mapfiles/ms/icons/red-dot.png" 
                alt="Sighting Point"
                className="w-6 h-6"
              />
            </div>
            <span className="text-sm text-gray-700">Sighting Point</span>
          </div>
          <div className="pt-2 border-t border-gray-200 mt-2">
            <p className="text-xs text-gray-500 mb-1">Trajectory Lines:</p>
            <p className="text-xs text-gray-600">Each tagNumber has a unique color</p>
          </div>
        </div>
      </div>

      <GoogleMap
        mapContainerStyle={mapContainerStyle}
        center={center}
        zoom={trajectories.length > 0 ? 8 : 5}
        onLoad={onMapLoad}
        options={{
          mapTypeControl: true,
          streetViewControl: true,
          fullscreenControl: true,
          zoomControl: true,
        }}
      >
        {/* Render all trajectories */}
        {trajectories.map((trajectory) => {
          // Build trajectory path for THIS tagNumber only
          // Each tagNumber has its own independent trajectory line
          // CRITICAL: Only include points that belong to this specific tagNumber
          const trajectoryPath: google.maps.LatLngLiteral[] = [];
          
          // Add release point to path (only one release point per tagNumber)
          if (trajectory.releasePoint) {
            // Verify this point belongs to the current tagNumber
            if (trajectory.releasePoint.tagNumber === trajectory.tagNumber) {
              trajectoryPath.push({ lat: trajectory.releasePoint.lat, lng: trajectory.releasePoint.lng });
            } else {
              console.warn(`Release point tagNumber mismatch in OverviewMap: expected ${trajectory.tagNumber}, got ${trajectory.releasePoint.tagNumber}`);
            }
          }
          
          // Add all sighting points to path (only points belonging to this tagNumber)
          const validSightingPoints = trajectory.sightingPoints.filter(point => {
            if (point.tagNumber !== trajectory.tagNumber) {
              console.warn(`Sighting point tagNumber mismatch in OverviewMap: expected ${trajectory.tagNumber}, got ${point.tagNumber}`);
              return false;
            }
            return true;
          });
          
          validSightingPoints.forEach(point => {
            trajectoryPath.push({ lat: point.lat, lng: point.lng });
          });

          // Only render trajectory line if there are at least 2 points (need 2 points to draw a line)
          // Each tagNumber has its own independent polyline
          // IMPORTANT: trajectoryPath should ONLY contain points from this tagNumber
          const shouldRenderPolyline = trajectoryPath.length > 1;
          
          // Debug: Log if trajectory path has points from different tagNumbers (should never happen)
          if (shouldRenderPolyline && trajectoryPath.length > 0) {
            // This is just for debugging - the path should only contain points from trajectory.tagNumber
            // We've already filtered above, so this should be safe
          }

          return (
            <React.Fragment key={trajectory.tagNumber}>
              {/* Trajectory Polyline - Only connects points of the same tagNumber */}
              {shouldRenderPolyline && createPolylineOptions(trajectory.color) && (
                <Polyline
                  key={`${trajectory.tagNumber}-polyline`}
                  path={trajectoryPath}
                  options={createPolylineOptions(trajectory.color)!}
                />
              )}
            </React.Fragment>
          );
        })}

        {/* Render markers grouped by location with count labels */}
        {Array.from(allPointsByLocation.entries()).map(([locationKey, points]) => {
          const basePosition = {
            lat: parseFloat(locationKey.split(',')[0]),
            lng: parseFloat(locationKey.split(',')[1])
          };
          
          // Separate release and sighting points
          const releasePoints = points.filter(p => p.type === "release");
          const sightingPoints = points.filter(p => p.type === "sighting");
          
          // Show release point first (if exists), then sighting points
          // Only show one marker per location type, but with count label
          return (
            <React.Fragment key={locationKey}>
              {/* Release Point Marker - Show count if multiple points */}
              {releasePoints.length > 0 && (
                <Marker
                  key={`release-${locationKey}`}
                  position={basePosition}
                  icon={createMarkerIcon("release", "", points.length)}
                  onClick={() => setSelectedLocation({ position: basePosition, points })}
                  title={`${releasePoints.length} Release Point(s)${points.length > releasePoints.length ? `, ${points.length - releasePoints.length} Sighting(s)` : ''} - ${points.length} total`}
                  label={points.length > 1 ? {
                    text: points.length.toString(),
                    color: "white",
                    fontSize: "14px",
                    fontWeight: "bold",
                  } : undefined}
                />
              )}
              
              {/* Sighting Points Marker - Only show if no release points, or show separately */}
              {releasePoints.length === 0 && sightingPoints.length > 0 && (
                <Marker
                  key={`sighting-${locationKey}`}
                  position={basePosition}
                  icon={createMarkerIcon("sighting", "", points.length)}
                  onClick={() => setSelectedLocation({ position: basePosition, points })}
                  title={`${sightingPoints.length} Sighting Point(s)`}
                  label={points.length > 1 ? {
                    text: points.length.toString(),
                    color: "white",
                    fontSize: "14px",
                    fontWeight: "bold",
                  } : undefined}
                />
              )}
            </React.Fragment>
          );
        })}

        {/* Info Window - Show all points at selected location */}
        {selectedLocation && (
          <InfoWindow
            position={selectedLocation.position}
            onCloseClick={() => setSelectedLocation(null)}
          >
            <div className="p-3 min-w-[250px] max-w-[400px] max-h-[400px] overflow-y-auto">
              <h3 className="font-bold mb-3 text-lg">
                {selectedLocation.points.length > 1 
                  ? `${selectedLocation.points.length} Points at This Location`
                  : selectedLocation.points[0]?.type === "release" ? "Release Point" : "Sighting Point"}
              </h3>
              
              <div className="space-y-3">
                {selectedLocation.points.map((point, index) => (
                  <div 
                    key={`${point.tagNumber}-${point.type}-${index}`}
                    className={`p-2 rounded border-l-4 ${
                      point.type === "release" ? "border-red-500 bg-red-50" : "border-blue-500 bg-blue-50"
                    }`}
                  >
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <p className={`font-semibold text-sm ${
                          point.type === "release" ? "text-red-600" : "text-blue-600"
                        }`}>
                          {point.type === "release" ? "Release" : "Sighting"} - {point.tagNumber}
                        </p>
                        <p className="text-xs text-gray-700 mt-1">{point.label}</p>
                        {point.description && (
                          <p className="text-xs text-gray-600 mt-1">{point.description}</p>
                        )}
                        {point.date && (
                          <p className="text-xs text-gray-500 mt-1">Date: {point.date}</p>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
              
              <p className="text-xs text-gray-400 mt-3">
                Coordinates: {selectedLocation.position.lat.toFixed(6)}, {selectedLocation.position.lng.toFixed(6)}
              </p>
            </div>
          </InfoWindow>
        )}
      </GoogleMap>
    </div>
  );
}

