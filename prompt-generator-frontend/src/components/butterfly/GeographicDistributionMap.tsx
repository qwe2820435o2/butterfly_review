"use client";

import React, { useEffect } from "react";
import dynamic from "next/dynamic";
import L from "leaflet";
import { LatLngExpression, LatLngBounds } from "leaflet";
import { GeographicDistribution, LocationPoint } from "@/types/butterfly";

// Dynamically import MapContainer to avoid SSR issues
const MapContainer = dynamic(
  () => import("react-leaflet").then((mod) => mod.MapContainer),
  { ssr: false }
);

const TileLayer = dynamic(
  () => import("react-leaflet").then((mod) => mod.TileLayer),
  { ssr: false }
);

const Marker = dynamic(
  () => import("react-leaflet").then((mod) => mod.Marker),
  { ssr: false }
);

const Popup = dynamic(
  () => import("react-leaflet").then((mod) => mod.Popup),
  { ssr: false }
);

const CircleMarker = dynamic(
  () => import("react-leaflet").then((mod) => mod.CircleMarker),
  { ssr: false }
);

const Rectangle = dynamic(
  () => import("react-leaflet").then((mod) => mod.Rectangle),
  { ssr: false }
);

interface GeographicDistributionMapProps {
  geographicDistribution: GeographicDistribution;
  className?: string;
}

/**
 * Geographic Distribution Map Component
 * Displays all release and sighting locations on a map for year in review
 */
export default function GeographicDistributionMap({
  geographicDistribution,
  className = "h-[600px] w-full"
}: GeographicDistributionMapProps) {
  // Calculate center point from bounds or locations
  const getCenter = (): LatLngExpression => {
    if (geographicDistribution.bounds) {
      const bounds = geographicDistribution.bounds;
      return [
        (bounds.minLatitude + bounds.maxLatitude) / 2,
        (bounds.minLongitude + bounds.maxLongitude) / 2
      ];
    }

    const allLocations = [
      ...geographicDistribution.releaseLocations,
      ...geographicDistribution.sightingLocations
    ];

    if (allLocations.length > 0) {
      const avgLat = allLocations.reduce((sum, loc) => sum + loc.latitude, 0) / allLocations.length;
      const avgLng = allLocations.reduce((sum, loc) => sum + loc.longitude, 0) / allLocations.length;
      return [avgLat, avgLng];
    }

    // Default center (Auckland, New Zealand)
    return [-36.8485, 174.7633];
  };

  // Calculate zoom level based on bounds
  const getZoom = (): number => {
    if (geographicDistribution.bounds) {
      const bounds = geographicDistribution.bounds;
      const latDiff = bounds.maxLatitude - bounds.minLatitude;
      const lngDiff = bounds.maxLongitude - bounds.minLongitude;
      const maxDiff = Math.max(latDiff, lngDiff);

      if (maxDiff > 10) return 5;
      if (maxDiff > 5) return 6;
      if (maxDiff > 2) return 7;
      if (maxDiff > 1) return 8;
      if (maxDiff > 0.5) return 9;
      return 10;
    }

    const allLocations = [
      ...geographicDistribution.releaseLocations,
      ...geographicDistribution.sightingLocations
    ];

    if (allLocations.length === 0) return 5;
    if (allLocations.length === 1) return 10;
    return 8;
  };

  // Get bounds for map fit
  const getBounds = (): LatLngBounds | null => {
    if (geographicDistribution.bounds) {
      const bounds = geographicDistribution.bounds;
      return L.latLngBounds(
        [bounds.minLatitude, bounds.minLongitude],
        [bounds.maxLatitude, bounds.maxLongitude]
      );
    }

    const allLocations = [
      ...geographicDistribution.releaseLocations,
      ...geographicDistribution.sightingLocations
    ];

    if (allLocations.length === 0) return null;

    const lats = allLocations.map(loc => loc.latitude);
    const lngs = allLocations.map(loc => loc.longitude);

    return L.latLngBounds(
      [Math.min(...lats), Math.min(...lngs)],
      [Math.max(...lats), Math.max(...lngs)]
    );
  };

  const center = getCenter();
  const zoom = getZoom();
  const bounds = getBounds();

  // Format date for display
  const formatDate = (dateString?: string): string => {
    if (!dateString) return "";
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('zh-CN', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    } catch {
      return dateString;
    }
  };

  return (
    <div className={className}>
      <MapContainer
        center={center}
        zoom={zoom}
        style={{ height: "100%", width: "100%", zIndex: 0 }}
        scrollWheelZoom={true}
        bounds={bounds || undefined}
        boundsOptions={{ padding: [50, 50] }}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        {/* Geographic Bounds Rectangle (optional, for visualization) */}
        {geographicDistribution.bounds && (
          <Rectangle
            bounds={L.latLngBounds(
              [geographicDistribution.bounds.minLatitude, geographicDistribution.bounds.minLongitude],
              [geographicDistribution.bounds.maxLatitude, geographicDistribution.bounds.maxLongitude]
            )}
            pathOptions={{
              color: "#FF6B35",
              fillColor: "#FF6B35",
              fillOpacity: 0.1,
              weight: 2,
              dashArray: "10, 5"
            }}
          />
        )}

        {/* Release Locations (Red Markers) */}
        {geographicDistribution.releaseLocations.map((location, index) => (
          <Marker
            key={`release-${index}`}
            position={[location.latitude, location.longitude]}
            icon={L.icon({
              iconUrl: "https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png",
              shadowUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
              iconSize: [20, 35],
              iconAnchor: [10, 35],
              popupAnchor: [1, -34],
              shadowSize: [35, 35]
            })}
          >
            <Popup>
              <div className="p-2 min-w-[150px]">
                <h3 className="font-bold text-red-600 mb-1 text-sm">释放点</h3>
                {location.address && (
                  <p className="text-xs text-gray-700 mb-1">{location.address}</p>
                )}
                {location.date && (
                  <p className="text-xs text-gray-500">日期: {formatDate(location.date)}</p>
                )}
                <p className="text-xs text-gray-400 mt-1">
                  {location.latitude.toFixed(4)}, {location.longitude.toFixed(4)}
                </p>
              </div>
            </Popup>
          </Marker>
        ))}

        {/* Sighting Locations (Blue Markers) */}
        {geographicDistribution.sightingLocations.map((location, index) => (
          <Marker
            key={`sighting-${index}`}
            position={[location.latitude, location.longitude]}
            icon={L.icon({
              iconUrl: "https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-blue.png",
              shadowUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
              iconSize: [18, 30],
              iconAnchor: [9, 30],
              popupAnchor: [1, -30],
              shadowSize: [30, 30]
            })}
          >
            <Popup>
              <div className="p-2 min-w-[150px]">
                <h3 className="font-bold text-blue-600 mb-1 text-sm">目击点</h3>
                {location.address && (
                  <p className="text-xs text-gray-700 mb-1">{location.address}</p>
                )}
                {location.date && (
                  <p className="text-xs text-gray-500">日期: {formatDate(location.date)}</p>
                )}
                <p className="text-xs text-gray-400 mt-1">
                  {location.latitude.toFixed(4)}, {location.longitude.toFixed(4)}
                </p>
              </div>
            </Popup>
          </Marker>
        ))}

        {/* Most Active Release Location (Highlighted) */}
        {geographicDistribution.mostActiveReleaseLocation && (
          <CircleMarker
            center={[
              geographicDistribution.mostActiveReleaseLocation.latitude,
              geographicDistribution.mostActiveReleaseLocation.longitude
            ]}
            radius={15}
            pathOptions={{
              color: "#FF6B35",
              fillColor: "#FF6B35",
              fillOpacity: 0.3,
              weight: 3
            }}
          >
            <Popup>
              <div className="p-2 min-w-[200px]">
                <h3 className="font-bold text-orange-600 mb-1">🏆 最活跃释放地点</h3>
                {geographicDistribution.mostActiveReleaseLocation.address && (
                  <p className="text-sm text-gray-700 mb-1">
                    {geographicDistribution.mostActiveReleaseLocation.address}
                  </p>
                )}
                <p className="text-sm font-semibold text-orange-600">
                  释放次数: {geographicDistribution.mostActiveReleaseLocation.count}
                </p>
              </div>
            </Popup>
          </CircleMarker>
        )}

        {/* Most Active Sighting Location (Highlighted) */}
        {geographicDistribution.mostActiveSightingLocation && (
          <CircleMarker
            center={[
              geographicDistribution.mostActiveSightingLocation.latitude,
              geographicDistribution.mostActiveSightingLocation.longitude
            ]}
            radius={15}
            pathOptions={{
              color: "#3B82F6",
              fillColor: "#3B82F6",
              fillOpacity: 0.3,
              weight: 3
            }}
          >
            <Popup>
              <div className="p-2 min-w-[200px]">
                <h3 className="font-bold text-blue-600 mb-1">🏆 最活跃目击地点</h3>
                {geographicDistribution.mostActiveSightingLocation.address && (
                  <p className="text-sm text-gray-700 mb-1">
                    {geographicDistribution.mostActiveSightingLocation.address}
                  </p>
                )}
                <p className="text-sm font-semibold text-blue-600">
                  目击次数: {geographicDistribution.mostActiveSightingLocation.count}
                </p>
              </div>
            </Popup>
          </CircleMarker>
        )}
      </MapContainer>
    </div>
  );
}

