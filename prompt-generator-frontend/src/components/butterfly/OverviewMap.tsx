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
export default function OverviewMap({
  trajectories = [],
  className = "h-[600px] w-full",
  apiKey = process.env.NEXT_PUBLIC_GOOGLE_MAPS_API_KEY || "AIzaSyAl3nBJPlQzHAje4DbjISunBDuoVU6P2ZE"
}: OverviewMapProps) {
  const [selectedPoint, setSelectedPoint] = useState<TrajectoryPoint | null>(null);
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

  // Create marker icon based on type
  const createMarkerIcon = useCallback((type: "release" | "sighting", color: string) => {
    if (!isLoaded || typeof google === 'undefined') return undefined;
    
    // Use different icons for release (red) and sighting (blue)
    const iconUrl = type === "release"
      ? "https://maps.google.com/mapfiles/ms/icons/red-dot.png"
      : "https://maps.google.com/mapfiles/ms/icons/blue-dot.png";
    
    return {
      url: iconUrl,
      scaledSize: new google.maps.Size(40, 40),
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
              {/* Release Point Marker - Only for this tagNumber */}
              {trajectory.releasePoint && trajectory.releasePoint.tagNumber === trajectory.tagNumber && (
                <Marker
                  position={{ lat: trajectory.releasePoint.lat, lng: trajectory.releasePoint.lng }}
                  icon={createMarkerIcon("release", trajectory.color)}
                  onClick={() => setSelectedPoint(trajectory.releasePoint!)}
                  title={`${trajectory.tagNumber} - Release`}
                />
              )}

              {/* Sighting Points Markers - Only for this tagNumber */}
              {trajectory.sightingPoints
                .filter(point => point.tagNumber === trajectory.tagNumber)
                .map((point, index) => (
                  <Marker
                    key={`${trajectory.tagNumber}-sighting-${index}`}
                    position={{ lat: point.lat, lng: point.lng }}
                    icon={createMarkerIcon("sighting", trajectory.color)}
                    onClick={() => setSelectedPoint(point)}
                    title={`${trajectory.tagNumber} - Sighting ${index + 1}`}
                  />
                ))}

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

        {/* Info Window */}
        {selectedPoint && (
          <InfoWindow
            position={{ lat: selectedPoint.lat, lng: selectedPoint.lng }}
            onCloseClick={() => setSelectedPoint(null)}
          >
            <div className="p-2 min-w-[200px]">
              <h3 className={`font-bold mb-2 ${selectedPoint.type === "release" ? "text-red-600" : "text-blue-600"}`}>
                {selectedPoint.type === "release" ? "Release Point" : "Sighting Point"}
              </h3>
              <p className="text-sm font-semibold mb-1">Tag: {selectedPoint.tagNumber}</p>
              <p className="text-sm font-semibold mb-1">{selectedPoint.label}</p>
              {selectedPoint.description && (
                <p className="text-xs text-gray-600 mb-1">{selectedPoint.description}</p>
              )}
              {selectedPoint.date && (
                <p className="text-xs text-gray-500">Date: {selectedPoint.date}</p>
              )}
            </div>
          </InfoWindow>
        )}
      </GoogleMap>
    </div>
  );
}

