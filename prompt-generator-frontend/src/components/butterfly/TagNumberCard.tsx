"use client";

import React from "react";
import Link from "next/link";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { 
  TagNumberSummary,
  formatDate,
  getStatusDisplay
} from "@/types/butterfly";
import { 
  Sparkles,
  Calendar,
  MapPin,
  Eye,
  Clock,
  ArrowRight
} from "lucide-react";

interface TagNumberCardProps {
  summary: TagNumberSummary;
  onViewTrajectory?: (tagNumber: string) => void;
}

/**
 * Tag Number Card Component
 * Displays a single tag number summary with key information
 */
export default function TagNumberCard({ 
  summary, 
  onViewTrajectory 
}: TagNumberCardProps) {
  const statusDisplay = getStatusDisplay(summary.status);
  const releaseDate = formatDate(summary.releaseDate);
  const lastSightingDate = formatDate(summary.lastSightingDate);

  const handleViewTrajectory = () => {
    if (onViewTrajectory) {
      onViewTrajectory(summary.tagNumber);
    }
  };

  return (
    <Card className="hover:shadow-xl transition-all duration-200 border-2 border-transparent hover:border-orange-400 dark:hover:border-orange-600 rounded-2xl bg-white dark:bg-gray-800">
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="flex items-center space-x-3">
            <div className="flex items-center justify-center w-12 h-12 bg-orange-100 dark:bg-orange-900/30 rounded-full">
              <Sparkles className="w-6 h-6 text-orange-600 dark:text-orange-400" />
            </div>
            <div className="flex-1">
              <CardTitle className="text-xl font-bold text-gray-900 dark:text-white">
                {summary.tagNumber}
              </CardTitle>
              <CardDescription className="mt-1 text-gray-600 dark:text-gray-300">
                标签号
              </CardDescription>
            </div>
          </div>
          <Badge 
            className={`${statusDisplay.bgColor} ${statusDisplay.color} border-0`}
          >
            {statusDisplay.text}
          </Badge>
        </div>
      </CardHeader>
      
      <CardContent>
        <div className="space-y-4">
          {/* Release Date */}
          <div className="flex items-center text-sm text-gray-600 dark:text-gray-300">
            <Calendar className="w-4 h-4 mr-2 text-orange-600 dark:text-orange-400 flex-shrink-0" />
            <span className="font-medium mr-2">释放日期:</span>
            <span>{releaseDate}</span>
          </div>

          {/* Last Sighting Date */}
          {summary.lastSightingDate ? (
            <div className="flex items-center text-sm text-gray-600 dark:text-gray-300">
              <Eye className="w-4 h-4 mr-2 text-blue-600 dark:text-blue-400 flex-shrink-0" />
              <span className="font-medium mr-2">最后目击:</span>
              <span>{lastSightingDate}</span>
            </div>
          ) : (
            <div className="flex items-center text-sm text-gray-500 dark:text-gray-400">
              <Eye className="w-4 h-4 mr-2 flex-shrink-0" />
              <span className="font-medium mr-2">最后目击:</span>
              <span>暂无目击记录</span>
            </div>
          )}

          {/* Sighting Count */}
          <div className="flex items-center text-sm text-gray-600 dark:text-gray-300">
            <Eye className="w-4 h-4 mr-2 text-purple-600 dark:text-purple-400 flex-shrink-0" />
            <span className="font-medium mr-2">目击次数:</span>
            <span className="font-semibold text-purple-600 dark:text-purple-400">
              {summary.sightingCount} 次
            </span>
          </div>

          {/* Survival Days */}
          {summary.survivalDays !== undefined && (
            <div className="flex items-center text-sm text-gray-600 dark:text-gray-300">
              <Clock className="w-4 h-4 mr-2 text-green-600 dark:text-green-400 flex-shrink-0" />
              <span className="font-medium mr-2">存活天数:</span>
              <span className="font-semibold text-green-600 dark:text-green-400">
                {summary.survivalDays} 天
              </span>
            </div>
          )}

          {/* Release Location (if available) */}
          {summary.releaseLocation && (
            <div className="flex items-start text-sm text-gray-600 dark:text-gray-300">
              <MapPin className="w-4 h-4 mr-2 text-red-600 dark:text-red-400 flex-shrink-0 mt-0.5" />
              <div className="flex-1">
                <span className="font-medium mr-2">释放地点:</span>
                <span className="text-xs">
                  {summary.releaseLocation.address || 
                   `${summary.releaseLocation.latitude.toFixed(4)}, ${summary.releaseLocation.longitude.toFixed(4)}`}
                </span>
              </div>
            </div>
          )}

          {/* Divider */}
          <div className="border-t border-gray-200 dark:border-gray-700 pt-4">
            <Link href={`/map/${summary.tagNumber}`}>
              <Button 
                className="w-full bg-orange-600 hover:bg-orange-700 dark:bg-orange-500 dark:hover:bg-orange-600 text-white"
                onClick={handleViewTrajectory}
              >
                <MapPin className="w-4 h-4 mr-2" />
                查看轨迹
                <ArrowRight className="w-4 h-4 ml-2" />
              </Button>
            </Link>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

