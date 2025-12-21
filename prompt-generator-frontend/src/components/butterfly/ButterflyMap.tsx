"use client";

import React, { useEffect, useRef } from "react";
import dynamic from "next/dynamic";
import L from "leaflet";
import { LatLngExpression } from "leaflet";

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

const Polyline = dynamic(
  () => import("react-leaflet").then((mod) => mod.Polyline),
  { ssr: false }
);

interface MapPoint {
  lat: number;
  lng: number;
  label: string;
  description?: string;
  type: "release" | "sighting";
  date?: string;
}

interface ButterflyMapProps {
  releasePoint?: MapPoint;
  sightingPoints?: MapPoint[];
  className?: string;
}

/**
 * Butterfly Trajectory Map Component
 * Displays butterfly release and sighting locations on a map
 */
export default function ButterflyMap({
  releasePoint,
  sightingPoints = [],
  className = "h-[600px] w-full"
}: ButterflyMapProps) {
  const mapRef = useRef<L.Map | null>(null);

  // Calculate center point
  const getCenter = (): LatLngExpression => {
    if (releasePoint) {
      return [releasePoint.lat, releasePoint.lng];
    }
    if (sightingPoints.length > 0) {
      const first = sightingPoints[0];
      return [first.lat, first.lng];
    }
    // Default center (can be adjusted based on your data location)
    return [-36.8485, 174.7633]; // Auckland, New Zealand (example)
  };

  // Calculate bounds to fit all points
  const getBounds = (): LatLngExpression[] => {
    const points: LatLngExpression[] = [];
    if (releasePoint) {
      points.push([releasePoint.lat, releasePoint.lng]);
    }
    sightingPoints.forEach((point) => {
      points.push([point.lat, point.lng]);
    });
    return points.length > 0 ? points : [getCenter()];
  };

  // Create polyline path
  const getPolylinePath = (): LatLngExpression[] => {
    const path: LatLngExpression[] = [];
    if (releasePoint) {
      path.push([releasePoint.lat, releasePoint.lng]);
    }
    sightingPoints.forEach((point) => {
      path.push([point.lat, point.lng]);
    });
    return path;
  };

  const center = getCenter();
  const bounds = getBounds();
  const polylinePath = getPolylinePath();

  return (
    <div className={className}>
      <MapContainer
        center={center}
        zoom={releasePoint || sightingPoints.length > 0 ? 10 : 5}
        style={{ height: "100%", width: "100%", zIndex: 0 }}
        scrollWheelZoom={true}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        {/* Release Point Marker (Red) */}
        {releasePoint && (
          <Marker
            position={[releasePoint.lat, releasePoint.lng]}
            icon={L.icon({
              iconUrl: "https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png",
              shadowUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
              iconSize: [25, 41],
              iconAnchor: [12, 41],
              popupAnchor: [1, -34],
              shadowSize: [41, 41]
            })}
          >
            <Popup>
              <div className="p-2">
                <h3 className="font-bold text-red-600 mb-1">释放点</h3>
                <p className="text-sm">{releasePoint.label}</p>
                {releasePoint.description && (
                  <p className="text-xs text-gray-600 mt-1">{releasePoint.description}</p>
                )}
                {releasePoint.date && (
                  <p className="text-xs text-gray-500 mt-1">日期: {releasePoint.date}</p>
                )}
              </div>
            </Popup>
          </Marker>
        )}

        {/* Sighting Points Markers (Blue) */}
        {sightingPoints.map((point, index) => (
          <Marker
            key={index}
            position={[point.lat, point.lng]}
            icon={L.icon({
              iconUrl: "https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-blue.png",
              shadowUrl: "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
              iconSize: [25, 41],
              iconAnchor: [12, 41],
              popupAnchor: [1, -34],
              shadowSize: [41, 41]
            })}
          >
            <Popup>
              <div className="p-2">
                <h3 className="font-bold text-blue-600 mb-1">目击点 #{index + 1}</h3>
                <p className="text-sm">{point.label}</p>
                {point.description && (
                  <p className="text-xs text-gray-600 mt-1">{point.description}</p>
                )}
                {point.date && (
                  <p className="text-xs text-gray-500 mt-1">日期: {point.date}</p>
                )}
              </div>
            </Popup>
          </Marker>
        ))}

        {/* Trajectory Polyline */}
        {polylinePath.length > 1 && (
          <Polyline
            positions={polylinePath}
            color="#FF6B35"
            weight={3}
            opacity={0.7}
            dashArray="10, 5"
          />
        )}
      </MapContainer>
    </div>
  );
}

