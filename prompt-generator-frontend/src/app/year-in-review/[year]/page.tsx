"use client";

import React, { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ArrowLeft, Loader2, Sparkles, Calendar, Eye, Users, MapPin, TrendingUp, Award, AlertCircle } from "lucide-react";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import dynamic from "next/dynamic";
import { butterflyService } from "@/services/butterflyService";
import { YearInReview } from "@/types/butterfly";
import ScrollProgress from "@/components/butterfly/ScrollProgress";
import ScrollReveal from "@/components/butterfly/ScrollReveal";
import AnimatedNumber from "@/components/butterfly/AnimatedNumber";

// Dynamically import map component to avoid SSR issues
const GeographicDistributionMap = dynamic(
  () => import("@/components/butterfly/GeographicDistributionMap"),
  { 
    ssr: false,
    loading: () => (
      <div className="h-[600px] w-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 rounded-lg">
        <Loader2 className="w-8 h-8 animate-spin text-orange-600" />
      </div>
    )
  }
);

export default function YearInReviewPage() {
  const params = useParams();
  const router = useRouter();
  const dispatch = useDispatch();
  const year = params.year ? parseInt(params.year as string) : new Date().getFullYear();

  const [data, setData] = useState<YearInReview | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (year && year >= 2000 && year <= 2100) {
      loadYearInReview();
    } else {
      setError("Invalid year");
      setIsLoading(false);
    }
  }, [year]);

  const loadYearInReview = async () => {
    try {
      setIsLoading(true);
      setError(null);
      dispatch(showLoading());

      const yearData = await butterflyService.getYearInReview(year);
      setData(yearData);
    } catch (err) {
      console.error("Load year in review error:", err);
      setError("Failed to load year in review");

      let errorMessage = "Failed to load, please try again later";
      if (err && typeof err === "object" && "isAxiosError" in err) {
        const axiosError = err as AxiosError;
        if (axiosError.response?.status === 404) {
          errorMessage = "No data found for this year";
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
              <p className="text-gray-600 dark:text-gray-300">Loading year in review...</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !data) {
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
                    {error || "No Data Found"}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-6">
                    No data records found for year {year}
                  </p>
                  <div className="flex gap-4 justify-center">
                    <Link href="/search">
                      <Button variant="outline">
                        <ArrowLeft className="w-4 h-4 mr-2" />
                        Back to Search
                      </Button>
                    </Link>
                    <Button onClick={() => router.push(`/year-in-review/${new Date().getFullYear()}`)}>
                      View Current Year
                    </Button>
                  </div>
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
      {/* Scroll Progress Indicator */}
      <ScrollProgress />

      {/* Navigation */}
      <div className="sticky top-0 z-40 bg-white/80 dark:bg-gray-900/80 backdrop-blur-sm border-b border-gray-200 dark:border-gray-800">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <Link href="/search">
              <Button variant="ghost" size="sm">
                <ArrowLeft className="w-4 h-4 mr-2" />
                Back
              </Button>
            </Link>
            <div className="text-sm text-gray-600 dark:text-gray-400">
              {year} Year in Review
            </div>
          </div>
        </div>
      </div>

      {/* Cover Section */}
      <section className="min-h-screen flex items-center justify-center px-4 py-16">
        <ScrollReveal direction="fade" delay={200}>
          <div className="text-center max-w-4xl mx-auto">
            <div className="mb-8">
              <Sparkles className="w-24 h-24 text-orange-500 mx-auto mb-6 animate-pulse" />
              <h1 className="text-5xl md:text-7xl font-bold text-gray-900 dark:text-white mb-4">
                {year} Butterfly
              </h1>
              <h2 className="text-3xl md:text-5xl font-bold text-orange-600 dark:text-orange-400 mb-6">
                Year in Review
              </h2>
              <p className="text-xl text-gray-600 dark:text-gray-300 mb-8">
                Thank you to all volunteers for their contributions. Let's review the amazing data from this year
              </p>
            </div>
            <div className="flex justify-center">
              <Button
                size="lg"
                onClick={() => {
                  document.getElementById('overview-section')?.scrollIntoView({ behavior: 'smooth' });
                }}
                className="bg-orange-600 hover:bg-orange-700 text-white transition-transform hover:scale-105"
              >
                Start Exploring
              </Button>
            </div>
          </div>
        </ScrollReveal>
      </section>

      {/* Overview Statistics Section */}
      <section id="overview-section" className="min-h-screen flex items-center justify-center px-4 py-16">
        <div className="container mx-auto max-w-6xl">
          <ScrollReveal direction="up" delay={0}>
            <div className="text-center mb-12">
              <h2 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
                Overview Statistics
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-300">
                Core data metrics for the {year} project
              </p>
            </div>
          </ScrollReveal>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {/* Total Releases */}
            <ScrollReveal direction="up" delay={100}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">Total Releases</CardTitle>
                    <Sparkles className="w-6 h-6 text-orange-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-orange-600 dark:text-orange-400 mb-2">
                    <AnimatedNumber value={data.overview.totalReleases} duration={1500} />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Total butterflies released this year
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Total Sightings */}
            <ScrollReveal direction="up" delay={200}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">Total Sightings</CardTitle>
                    <Eye className="w-6 h-6 text-blue-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-blue-600 dark:text-blue-400 mb-2">
                    <AnimatedNumber value={data.overview.totalSightings} duration={1500} />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Total number of sighting records
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Unique Volunteers */}
            <ScrollReveal direction="up" delay={300}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">Volunteers</CardTitle>
                    <Users className="w-6 h-6 text-green-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-green-600 dark:text-green-400 mb-2">
                    <AnimatedNumber value={data.overview.uniqueVolunteers} duration={1500} />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Total number of volunteers participating
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Unique Regions */}
            <ScrollReveal direction="up" delay={400}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">Regions Covered</CardTitle>
                    <MapPin className="w-6 h-6 text-purple-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-purple-600 dark:text-purple-400 mb-2">
                    <AnimatedNumber value={data.overview.uniqueRegions} duration={1500} />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Number of geographic regions covered by butterfly activity
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Average Survival Days */}
            {data.overview.averageSurvivalDays !== undefined && (
              <ScrollReveal direction="up" delay={500}>
                <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle className="text-lg">Average Survival Days</CardTitle>
                      <Calendar className="w-6 h-6 text-red-600" />
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="text-4xl font-bold text-red-600 dark:text-red-400 mb-2">
                      <AnimatedNumber 
                        value={Math.round(data.overview.averageSurvivalDays)} 
                        duration={1500} 
                      />
                    </div>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      Average survival time for all butterflies
                    </p>
                  </CardContent>
                </Card>
              </ScrollReveal>
            )}

            {/* Total Flight Distance */}
            <ScrollReveal direction="up" delay={600}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">Total Flight Distance</CardTitle>
                    <TrendingUp className="w-6 h-6 text-indigo-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-indigo-600 dark:text-indigo-400 mb-2">
                    <AnimatedNumber 
                      value={data.overview.totalFlightDistanceKm} 
                      duration={1500} 
                      decimals={1}
                      suffix=" km"
                    />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Sum of all flight trajectories
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Survival Rate */}
            {data.overview.survivalRate !== undefined && (
              <ScrollReveal direction="up" delay={700}>
                <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105 md:col-span-2 lg:col-span-1">
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle className="text-lg">Survival Rate</CardTitle>
                      <Award className="w-6 h-6 text-yellow-600" />
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="text-4xl font-bold text-yellow-600 dark:text-yellow-400 mb-2">
                      <AnimatedNumber 
                        value={data.overview.survivalRate} 
                        duration={1500} 
                        decimals={1}
                        suffix="%"
                      />
                    </div>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      Percentage of butterflies that survived
                    </p>
                  </CardContent>
                </Card>
              </ScrollReveal>
            )}
          </div>
        </div>
      </section>

      {/* Geographic Distribution Section */}
      <section className="min-h-screen flex items-center justify-center px-4 py-16">
        <div className="container mx-auto max-w-6xl">
          <ScrollReveal direction="up" delay={0}>
            <div className="text-center mb-12">
              <h2 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
                Geographic Distribution
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-300">
                View the distribution of release and sighting locations for all butterflies in {year}
              </p>
            </div>
          </ScrollReveal>

          <ScrollReveal direction="up" delay={200}>
            <Card className="shadow-lg mb-6">
            <CardHeader>
              <CardTitle>Activity Range Map</CardTitle>
              <CardDescription>
                Red markers: Release points | Blue markers: Sighting points | Orange circles: Most active release location | Blue circles: Most active sighting location
              </CardDescription>
            </CardHeader>
            <CardContent>
              <GeographicDistributionMap
                geographicDistribution={data.geographicDistribution}
                className="h-[600px] w-full rounded-lg"
              />
              </CardContent>
            </Card>
          </ScrollReveal>

          {/* Statistics Cards */}
          <ScrollReveal direction="up" delay={400}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
            {/* Release Locations Count */}
            <Card className="shadow-lg">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">Release Locations</CardTitle>
                  <MapPin className="w-5 h-5 text-red-600" />
                </div>
              </CardHeader>
              <CardContent>
                <div className="text-3xl font-bold text-red-600 dark:text-red-400 mb-2">
                  {data.geographicDistribution.releaseLocations.length.toLocaleString()}
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  release location(s)
                </p>
                {data.geographicDistribution.mostActiveReleaseLocation && (
                  <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
                    <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">Most Active Location</p>
                    <p className="text-sm font-medium text-gray-900 dark:text-white">
                      {data.geographicDistribution.mostActiveReleaseLocation.address || "Unknown Location"}
                    </p>
                    <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                      {data.geographicDistribution.mostActiveReleaseLocation.count} release(s)
                    </p>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Sighting Locations Count */}
            <Card className="shadow-lg">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">Sighting Locations</CardTitle>
                  <Eye className="w-5 h-5 text-blue-600" />
                </div>
              </CardHeader>
              <CardContent>
                <div className="text-3xl font-bold text-blue-600 dark:text-blue-400 mb-2">
                  {data.geographicDistribution.sightingLocations.length.toLocaleString()}
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  sighting location(s)
                </p>
                {data.geographicDistribution.mostActiveSightingLocation && (
                  <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
                    <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">Most Active Location</p>
                    <p className="text-sm font-medium text-gray-900 dark:text-white">
                      {data.geographicDistribution.mostActiveSightingLocation.address || "Unknown Location"}
                    </p>
                    <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                      {data.geographicDistribution.mostActiveSightingLocation.count} sighting(s)
                    </p>
                  </div>
                )}
              </CardContent>
            </Card>
            </div>
          </ScrollReveal>

          {/* Geographic Bounds Info */}
          {data.geographicDistribution.bounds && (
            <ScrollReveal direction="up" delay={600}>
            <Card className="shadow-lg mt-6">
              <CardHeader>
                <CardTitle className="text-lg">Activity Range</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                  <div>
                    <p className="text-gray-600 dark:text-gray-400 mb-1">Northernmost Latitude</p>
                    <p className="font-mono font-semibold text-gray-900 dark:text-white">
                      {data.geographicDistribution.bounds.maxLatitude.toFixed(4)}°
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400 mb-1">Southernmost Latitude</p>
                    <p className="font-mono font-semibold text-gray-900 dark:text-white">
                      {data.geographicDistribution.bounds.minLatitude.toFixed(4)}°
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400 mb-1">Easternmost Longitude</p>
                    <p className="font-mono font-semibold text-gray-900 dark:text-white">
                      {data.geographicDistribution.bounds.maxLongitude.toFixed(4)}°
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400 mb-1">Westernmost Longitude</p>
                    <p className="font-mono font-semibold text-gray-900 dark:text-white">
                      {data.geographicDistribution.bounds.minLongitude.toFixed(4)}°
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
            </ScrollReveal>
          )}
        </div>
      </section>

      {/* Footer Section */}
      <section className="min-h-screen flex items-center justify-center px-4 py-16">
        <ScrollReveal direction="fade" delay={200}>
          <div className="container mx-auto max-w-6xl text-center">
            <h2 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
              Thank You to All Volunteers
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-300 mb-8">
              Let's look forward to more amazing data in {year + 1}
            </p>
            <div className="flex gap-4 justify-center">
              <Link href="/search">
                <Button variant="outline" className="transition-transform hover:scale-105">
                  <ArrowLeft className="w-4 h-4 mr-2" />
                  Back to Search
                </Button>
              </Link>
              <Button
                onClick={() => {
                  window.scrollTo({ top: 0, behavior: 'smooth' });
                }}
                className="transition-transform hover:scale-105"
              >
                Back to Top
              </Button>
            </div>
          </div>
        </ScrollReveal>
      </section>
    </div>
  );
}

