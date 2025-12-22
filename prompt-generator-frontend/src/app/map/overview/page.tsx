"use client";

import React, { useState, useEffect } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Loader2, MapPin, ArrowLeft, AlertCircle } from "lucide-react";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import dynamic from "next/dynamic";
import { butterflyService } from "@/services/butterflyService";

// Dynamically import overview map component to avoid SSR issues
const OverviewMap = dynamic(
  () => import("@/components/butterfly/OverviewMap"),
  { 
    ssr: false,
    loading: () => (
      <div className="h-[600px] w-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 rounded-lg">
        <Loader2 className="w-8 h-8 animate-spin text-orange-600" />
      </div>
    )
  }
);

/**
 * Map Overview Page
 * Displays all tagNumber trajectories on a single map
 */
export default function MapOverviewPage() {
  const dispatch = useDispatch();
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [trajectories, setTrajectories] = useState<any[]>([]);

  useEffect(() => {
    loadAllTrajectories();
  }, []);

  const loadAllTrajectories = async () => {
    try {
      setIsLoading(true);
      setError(null);
      dispatch(showLoading());

      // Get all trajectories
      const trajectories = await butterflyService.getAllTrajectories();
      setTrajectories(trajectories);

      if (trajectories.length === 0) {
        toast.info("No trajectories found with location data");
      } else {
        toast.success(`Loaded ${trajectories.length} trajectory(s)`);
      }
    } catch (err) {
      console.error("Load trajectories error:", err);
      setError("Failed to load trajectories");
      
      let errorMessage = "Failed to load, please try again later";
      if (err && typeof err === "object" && "isAxiosError" in err) {
        const axiosError = err as AxiosError;
        if (axiosError.response?.status === 404) {
          errorMessage = "No trajectories found";
        }
      }
      
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
      dispatch(hideLoading());
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
        <div className="container mx-auto px-4 py-16">
          <div className="flex items-center justify-center min-h-[400px]">
            <div className="text-center">
              <Loader2 className="w-12 h-12 animate-spin text-orange-600 mx-auto mb-4" />
              <p className="text-gray-600 dark:text-gray-300">Loading trajectories...</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
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
                    {error}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-6">
                    Unable to load trajectory data
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
            All Trajectories Overview
          </h1>
          <p className="text-gray-600 dark:text-gray-300">
            View all butterfly tag trajectories on a single map
          </p>
        </div>

        {/* Map Section */}
        <Card className="shadow-lg">
          <CardHeader>
            <CardTitle>Trajectory Map</CardTitle>
            <CardDescription>
              All release and sighting points with trajectory lines. Each tagNumber has a unique color.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <OverviewMap
              trajectories={trajectories}
              className="h-[600px] w-full rounded-lg"
            />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

