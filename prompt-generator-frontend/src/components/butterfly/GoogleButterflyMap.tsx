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

  // Load Google Maps script
  const { isLoaded, loadError } = useLoadScript({
    googleMapsApiKey: apiKey,
  });

  // Calculate center point
  const center = useMemo(() => {
    if (releasePoint) {
      return { lat: releasePoint.lat, lng: releasePoint.lng };
    }
    if (sightingPoints.length > 0) {
      const first = sightingPoints[0];
      return { lat: first.lat, lng: first.lng };
    }
    return defaultCenter;
  }, [releasePoint, sightingPoints]);

  // Create trajectory path (from release to all sightings in order)
  const trajectoryPath = useMemo(() => {
    const path: google.maps.LatLngLiteral[] = [];
    
    if (releasePoint) {
      path.push({ lat: releasePoint.lat, lng: releasePoint.lng });
    }
    
    sightingPoints.forEach((point) => {
      path.push({ lat: point.lat, lng: point.lng });
    });
    
    return path;
  }, [releasePoint, sightingPoints]);

  // Calculate bounds to fit all points
  const bounds = useMemo(() => {
    const allPoints = [
      ...(releasePoint ? [{ lat: releasePoint.lat, lng: releasePoint.lng }] : []),
      ...sightingPoints.map(p => ({ lat: p.lat, lng: p.lng }))
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
  }, [releasePoint, sightingPoints]);

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
        {/* Release Point Marker (Red) */}
        {releasePoint && (
          <Marker
            position={{ lat: releasePoint.lat, lng: releasePoint.lng }}
            icon={createMarkerIcon("red")}
            onClick={() => setSelectedPoint(releasePoint)}
            title={releasePoint.label}
          />
        )}

        {/* Sighting Points Markers (Blue) */}
        {sightingPoints.map((point, index) => (
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

