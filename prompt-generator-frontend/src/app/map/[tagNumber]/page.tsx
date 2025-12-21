"use client";

import React, { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft, Loader2, MapPin, Calendar, Eye, Clock, AlertCircle } from "lucide-react";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import dynamic from "next/dynamic";
import { butterflyService } from "@/services/butterflyService";
import { ReleaseSubmission, SightingSubmission, formatDate, getStatusDisplay, calculateSurvivalDays } from "@/types/butterfly";

// Dynamically import map component to avoid SSR issues
const ButterflyMap = dynamic(
  () => import("@/components/butterfly/ButterflyMap"),
  { 
    ssr: false,
    loading: () => (
      <div className="h-[600px] w-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 rounded-lg">
        <Loader2 className="w-8 h-8 animate-spin text-orange-600" />
      </div>
    )
  }
);

interface MapPoint {
  lat: number;
  lng: number;
  label: string;
  description?: string;
  type: "release" | "sighting";
  date?: string;
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
        setError("未找到该标签号的记录");
        toast.info("未找到记录", {
          description: `标签号 ${tagNumber} 没有相关的释放或目击记录`
        });
      }
    } catch (err) {
      console.error("Load trajectory error:", err);
      setError("加载轨迹数据失败");
      
      let errorMessage = "加载失败，请稍后重试";
      if (err && typeof err === "object" && "isAxiosError" in err) {
        const axiosError = err as AxiosError;
        if (axiosError.response?.status === 404) {
          errorMessage = "未找到该标签号的记录";
        }
      }
      
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
      dispatch(hideLoading());
    }
  };

  // Convert data to map points
  const getMapPoints = (): { releasePoint?: MapPoint; sightingPoints: MapPoint[] } => {
    const releasePoint: MapPoint | undefined = release && release.latitude && release.longitude
      ? {
          lat: release.latitude,
          lng: release.longitude,
          label: release.address || `释放点`,
          description: release.notes,
          type: "release",
          date: release.releaseDatePretty || formatDate(release.releaseDateTimeUtc)
        }
      : undefined;

    const sightingPoints: MapPoint[] = sightings
      .filter(s => s.latitude && s.longitude)
      .map((sighting, index) => ({
        lat: sighting.latitude!,
        lng: sighting.longitude!,
        label: sighting.address || `目击点 ${index + 1}`,
        description: sighting.condition,
        type: "sighting" as const,
        date: sighting.sightingDatePretty || formatDate(sighting.sightingDateTimeUtc)
      }));

    return { releasePoint, sightingPoints };
  };

  // Calculate total distance
  const calculateTotalDistance = (): number => {
    const points = getMapPoints();
    let totalDistance = 0;

    if (points.releasePoint && points.sightingPoints.length > 0) {
      // Calculate distance from release to first sighting
      const firstSighting = points.sightingPoints[0];
      totalDistance += calculateHaversineDistance(
        points.releasePoint.lat,
        points.releasePoint.lng,
        firstSighting.lat,
        firstSighting.lng
      );

      // Calculate distances between sightings
      for (let i = 0; i < points.sightingPoints.length - 1; i++) {
        const current = points.sightingPoints[i];
        const next = points.sightingPoints[i + 1];
        totalDistance += calculateHaversineDistance(
          current.lat,
          current.lng,
          next.lat,
          next.lng
        );
      }
    }

    return totalDistance;
  };

  // Haversine formula to calculate distance between two points
  const calculateHaversineDistance = (lat1: number, lon1: number, lat2: number, lon2: number): number => {
    const R = 6371; // Earth's radius in km
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLon = (lon2 - lon1) * Math.PI / 180;
    const a =
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
      Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return R * c;
  };

  const { releasePoint, sightingPoints } = getMapPoints();
  const totalDistance = calculateTotalDistance();
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
              <Loader2 className="w-12 h-12 animate-spin text-orange-600 mx-auto mb-4" />
              <p className="text-gray-600 dark:text-gray-300">正在加载轨迹数据...</p>
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
                    {error || "未找到记录"}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-6">
                    标签号 <span className="font-mono font-semibold">{tagNumber}</span> 没有相关的释放或目击记录
                  </p>
                  <Link href="/search">
                    <Button variant="outline">
                      <ArrowLeft className="w-4 h-4 mr-2" />
                      返回搜索
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
              返回搜索
            </Button>
          </Link>
          <h1 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-2">
            {tagNumber} 的轨迹
          </h1>
          <p className="text-gray-600 dark:text-gray-300">
            查看蝴蝶从释放到目击的完整飞行路径
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Map Section */}
          <div className="lg:col-span-2">
            <Card className="shadow-lg">
              <CardHeader>
                <CardTitle>飞行轨迹地图</CardTitle>
                <CardDescription>
                  红色标记：释放点 | 蓝色标记：目击点 | 橙色线条：飞行路径
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ButterflyMap
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
                <CardTitle>标签信息</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">标签号</p>
                  <p className="text-lg font-mono font-semibold text-gray-900 dark:text-white">
                    {tagNumber}
                  </p>
                </div>

                {status && (
                  <div>
                    <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">状态</p>
                    <Badge className={`${status.bgColor} ${status.color} border-0`}>
                      {status.text}
                    </Badge>
                  </div>
                )}

                {release?.releaseDateTimeUtc && (
                  <div className="flex items-start">
                    <Calendar className="w-4 h-4 mr-2 text-orange-600 dark:text-orange-400 mt-0.5 flex-shrink-0" />
                    <div>
                      <p className="text-sm text-gray-600 dark:text-gray-400">释放日期</p>
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
                      <p className="text-sm text-gray-600 dark:text-gray-400">最后目击</p>
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
                      <p className="text-sm text-gray-600 dark:text-gray-400">存活天数</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-white">
                        {survivalDays} 天
                      </p>
                    </div>
                  </div>
                )}

                {totalDistance > 0 && (
                  <div className="flex items-start">
                    <MapPin className="w-4 h-4 mr-2 text-purple-600 dark:text-purple-400 mt-0.5 flex-shrink-0" />
                    <div>
                      <p className="text-sm text-gray-600 dark:text-gray-400">总飞行距离</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-white">
                        {totalDistance.toFixed(2)} 公里
                      </p>
                    </div>
                  </div>
                )}

                <div>
                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">目击次数</p>
                  <p className="text-lg font-semibold text-purple-600 dark:text-purple-400">
                    {sightings.length} 次
                  </p>
                </div>
              </CardContent>
            </Card>

            {/* Release Details */}
            {release && (
              <Card className="shadow-lg">
                <CardHeader>
                  <CardTitle>释放详情</CardTitle>
                </CardHeader>
                <CardContent className="space-y-2 text-sm">
                  {release.address && (
                    <div>
                      <p className="text-gray-600 dark:text-gray-400">地点</p>
                      <p className="text-gray-900 dark:text-white">{release.address}</p>
                    </div>
                  )}
                  {release.notes && (
                    <div>
                      <p className="text-gray-600 dark:text-gray-400">备注</p>
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

