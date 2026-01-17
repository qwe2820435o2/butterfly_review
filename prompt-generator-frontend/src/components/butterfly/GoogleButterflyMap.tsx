"use client";

import React, { useMemo, useState, useEffect, useCallback } from "react";
import { GoogleMap, useLoadScript, Marker, Polyline, InfoWindow } from "@react-google-maps/api";

// Extend Window interface for Google Maps
declare global {
  interface Window {
    google: typeof google;
  }
}

interface MapPoint {
  lat: number;
  lng: number;
  label: string;
  address?: string; // Address for geocoding (priority over lat/lng)
  description?: string;
  type: "release" | "sighting";
  date?: string;
}

interface GoogleButterflyMapProps {
  releasePoint?: MapPoint;
  sightingPoints?: MapPoint[];
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
 * Google Maps Butterfly Trajectory Map Component
 * Displays butterfly release and sighting locations on Google Maps with trajectory
 */
export default function GoogleButterflyMap({
  releasePoint,
  sightingPoints = [],
  className = "h-[600px] w-full",
  apiKey = process.env.NEXT_PUBLIC_GOOGLE_MAPS_API_KEY || "AIzaSyAl3nBJPlQzHAje4DbjISunBDuoVU6P2ZE"
}: GoogleButterflyMapProps) {
  const [selectedPoint, setSelectedPoint] = useState<MapPoint | null>(null);
  const [map, setMap] = useState<google.maps.Map | null>(null);
  const [geocodedPoints, setGeocodedPoints] = useState<{
    release?: MapPoint;
    sightings: MapPoint[];
  }>({ sightings: [] });

  // Load Google Maps script (Geocoding is part of the core API, no need to load separately)
  const { isLoaded, loadError } = useLoadScript({
    googleMapsApiKey: apiKey,
    libraries: ['places'],
  });

  // Geocode address to coordinates (priority: address > lat/lng)
  const geocodeAddress = useCallback(async (point: MapPoint): Promise<MapPoint> => {
    // If address is available, use geocoding
    if (point.address && isLoaded && typeof google !== 'undefined') {
      try {
        const geocoder = new google.maps.Geocoder();
        const result = await new Promise<google.maps.GeocoderResult[]>((resolve, reject) => {
          geocoder.geocode(
            { address: point.address },
            (results, status) => {
              if (status === 'OK' && results && results.length > 0) {
                resolve(results);
              } else {
                reject(new Error(`Geocoding failed: ${status}`));
              }
            }
          );
        });

        const location = result[0].geometry.location;
        return {
          ...point,
          lat: location.lat(),
          lng: location.lng(),
        };
      } catch (error) {
        console.warn(`Geocoding failed for address "${point.address}", using provided coordinates:`, error);
        // Fall back to provided lat/lng
        return point;
      }
    }
    // If no address, use provided lat/lng
    return point;
  }, [isLoaded]);

  // Geocode all points when data is loaded
  useEffect(() => {
    if (!isLoaded || typeof google === 'undefined') return;

    const geocodeAllPoints = async () => {
      const geocodedRelease = releasePoint ? await geocodeAddress(releasePoint) : undefined;
      const geocodedSightings = await Promise.all(
        sightingPoints.map(point => geocodeAddress(point))
      );

      setGeocodedPoints({
        release: geocodedRelease,
        sightings: geocodedSightings,
      });
    };

    geocodeAllPoints();
  }, [isLoaded, releasePoint, sightingPoints, geocodeAddress]);

  // Calculate center point (use geocoded points if available)
  const center = useMemo(() => {
    const release = geocodedPoints.release || releasePoint;
    const sightings = geocodedPoints.sightings.length > 0 ? geocodedPoints.sightings : sightingPoints;
    
    if (release) {
      return { lat: release.lat, lng: release.lng };
    }
    if (sightings.length > 0) {
      const first = sightings[0];
      return { lat: first.lat, lng: first.lng };
    }
    return defaultCenter;
  }, [geocodedPoints, releasePoint, sightingPoints]);

  // Create trajectory path (from release to all sightings in order, use geocoded points)
  const trajectoryPath = useMemo(() => {
    const path: google.maps.LatLngLiteral[] = [];
    const release = geocodedPoints.release || releasePoint;
    const sightings = geocodedPoints.sightings.length > 0 ? geocodedPoints.sightings : sightingPoints;
    
    if (release) {
      path.push({ lat: release.lat, lng: release.lng });
    }
    
    sightings.forEach((point) => {
      path.push({ lat: point.lat, lng: point.lng });
    });
    
    return path;
  }, [geocodedPoints, releasePoint, sightingPoints]);

  // Calculate bounds to fit all points (use geocoded points)
  const bounds = useMemo(() => {
    const release = geocodedPoints.release || releasePoint;
    const sightings = geocodedPoints.sightings.length > 0 ? geocodedPoints.sightings : sightingPoints;
    
    const allPoints = [
      ...(release ? [{ lat: release.lat, lng: release.lng }] : []),
      ...sightings.map(p => ({ lat: p.lat, lng: p.lng }))
    ];

    if (allPoints.length === 0) return null;

    const lats = allPoints.map(p => p.lat);
    const lngs = allPoints.map(p => p.lng);

    return {
      north: Math.max(...lats),
      south: Math.min(...lats),
      east: Math.max(...lngs),
      west: Math.min(...lngs),
    };
  }, [geocodedPoints, releasePoint, sightingPoints]);

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

  // Create marker icon (only when Google Maps is loaded)
  const createMarkerIcon = useCallback((color: string) => {
    if (!isLoaded || typeof google === 'undefined') return undefined;
    return {
      url: color === "red" 
        ? "https://maps.google.com/mapfiles/ms/icons/red-dot.png"
        : "https://maps.google.com/mapfiles/ms/icons/blue-dot.png",
      scaledSize: new google.maps.Size(40, 40),
    };
  }, [isLoaded]);

  // Create polyline options (only when Google Maps is loaded)
  const polylineOptions = useMemo(() => {
    if (!isLoaded || typeof google === 'undefined' || trajectoryPath.length <= 1) return null;
    return {
      strokeColor: "#FF6B35",
      strokeOpacity: 0.8,
      strokeWeight: 4,
      geodesic: true,
      icons: [
        {
          icon: {
            path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
            scale: 4,
            strokeColor: "#FF6B35",
          },
          offset: "100%",
          repeat: "200px",
        },
      ],
    };
  }, [isLoaded, trajectoryPath.length]);

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
        zoom={releasePoint || sightingPoints.length > 0 ? 10 : 5}
        onLoad={onMapLoad}
        options={{
          mapTypeControl: true,
          streetViewControl: true,
          fullscreenControl: true,
          zoomControl: true,
        }}
      >
        {/* Release Point Marker (Red) - Use geocoded point if available */}
        {(geocodedPoints.release || releasePoint) && (() => {
          const point = geocodedPoints.release || releasePoint!;
          return (
            <Marker
              position={{ lat: point.lat, lng: point.lng }}
              icon={createMarkerIcon("red")}
              onClick={() => setSelectedPoint(point)}
              title={point.label}
            />
          );
        })()}

        {/* Sighting Points Markers (Blue) - Use geocoded points if available */}
        {(geocodedPoints.sightings.length > 0 ? geocodedPoints.sightings : sightingPoints).map((point, index) => (
          <Marker
            key={`sighting-${index}`}
            position={{ lat: point.lat, lng: point.lng }}
            icon={createMarkerIcon("blue")}
            onClick={() => setSelectedPoint(point)}
            title={point.label}
            label={{
              text: `${index + 1}`,
              color: "white",
              fontSize: "12px",
              fontWeight: "bold",
            }}
          />
        ))}

        {/* Trajectory Polyline */}
        {trajectoryPath.length > 1 && polylineOptions && (
          <Polyline
            path={trajectoryPath}
            options={polylineOptions}
          />
        )}

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

