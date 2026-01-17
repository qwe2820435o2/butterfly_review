"use client";

import React, { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ArrowLeft, Loader2, Sparkles, Calendar, Eye, Users, MapPin, TrendingUp, AlertCircle, Clock } from "lucide-react";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import { butterflyService } from "@/services/butterflyService";
import { YearInReview } from "@/types/butterfly";
import ScrollProgress from "@/components/butterfly/ScrollProgress";
import ScrollReveal from "@/components/butterfly/ScrollReveal";
import AnimatedNumber from "@/components/butterfly/AnimatedNumber";

export default function YearInReviewPage() {
  const params = useParams();
  const router = useRouter();
  const dispatch = useDispatch();
  const currentYear = new Date().getFullYear();
  const year = params.year ? parseInt(params.year as string) : currentYear;

  const [data, setData] = useState<YearInReview | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Generate year options (from 2020 to current year, or earlier if needed)
  const startYear = 2020;
  const yearOptions = Array.from({ length: currentYear - startYear + 1 }, (_, i) => currentYear - i);

  useEffect(() => {
    const loadYearInReview = async () => {
      if (!year || year < 2000 || year > 2100) {
        setError("Invalid year");
        setIsLoading(false);
        return;
      }

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

    loadYearInReview();
  }, [year, dispatch]);

  const handleYearChange = (newYear: string) => {
    const yearNum = parseInt(newYear);
    if (yearNum && yearNum >= 2000 && yearNum <= 2100) {
      router.push(`/year-in-review/${yearNum}`);
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
                    <Link href="/">
                      <Button variant="outline">
                        <ArrowLeft className="w-4 h-4 mr-2" />
                        Back to Home
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
        <div className="container mx-auto px-4 py-3">
          <div className="flex items-center justify-between gap-4">
            <Link href="/">
              <Button variant="ghost" size="sm">
                <ArrowLeft className="w-4 h-4 mr-2" />
                Back to Home
              </Button>
            </Link>

          </div>
        </div>
      </div>

      {/* Hero Section */}
      <section className="pt-16 pb-12 px-4">
        <div className="container mx-auto max-w-4xl">
          <ScrollReveal direction="fade" delay={100}>
            <div className="text-center">
              <Sparkles className="w-16 h-16 text-orange-500 mx-auto mb-4 animate-pulse" />
              <h1 className="text-4xl md:text-6xl font-bold text-gray-900 dark:text-white mb-3">
                Year in Review
              </h1>
              <div className="flex items-center justify-center gap-3 mb-4">
                <Calendar className="w-5 h-5 text-gray-600 dark:text-gray-400" />
                <Select value={year.toString()} onValueChange={handleYearChange}>
                  <SelectTrigger className="w-[140px] text-lg font-semibold">
                    <SelectValue placeholder="Select year" />
                  </SelectTrigger>
                  <SelectContent>
                    {yearOptions.map((y) => (
                      <SelectItem key={y} value={y.toString()}>
                        {y}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <p className="text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
                A comprehensive overview of butterfly tracking data and achievements
              </p>
            </div>
          </ScrollReveal>
        </div>
      </section>

      {/* Overview Statistics Section */}
      <section id="overview-section" className="py-12 px-4 bg-white/50 dark:bg-gray-800/50">
        <div className="container mx-auto max-w-7xl">
          <ScrollReveal direction="up" delay={0}>
            <div className="text-center mb-10">
              <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
                Key Statistics
              </h2>
              <p className="text-gray-600 dark:text-gray-400">
                Core metrics for {year}
              </p>
            </div>
          </ScrollReveal>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Total Releases */}
            <ScrollReveal direction="up" delay={100}>
              <Card className="shadow-md hover:shadow-lg transition-all duration-300 border-l-4 border-l-orange-500">
                <CardContent className="p-5">
                  <div className="flex items-center justify-between mb-3">
                    <Sparkles className="w-5 h-5 text-orange-600" />
                    <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">Releases</CardTitle>
                  </div>
                  <div className="text-3xl font-bold text-orange-600 dark:text-orange-400 mb-1">
                    <AnimatedNumber value={data.totalReleases} duration={1500} />
                  </div>
                  <p className="text-xs text-gray-500 dark:text-gray-500">
                    Butterflies released
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Total Sightings */}
            <ScrollReveal direction="up" delay={200}>
              <Card className="shadow-md hover:shadow-lg transition-all duration-300 border-l-4 border-l-blue-500">
                <CardContent className="p-5">
                  <div className="flex items-center justify-between mb-3">
                    <Eye className="w-5 h-5 text-blue-600" />
                    <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">Sightings</CardTitle>
                  </div>
                  <div className="text-3xl font-bold text-blue-600 dark:text-blue-400 mb-1">
                    <AnimatedNumber value={data.totalSightings} duration={1500} />
                  </div>
                  <p className="text-xs text-gray-500 dark:text-gray-500">
                    Sighting records
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Unique Volunteers */}
            <ScrollReveal direction="up" delay={300}>
              <Card className="shadow-md hover:shadow-lg transition-all duration-300 border-l-4 border-l-green-500">
                <CardContent className="p-5">
                  <div className="flex items-center justify-between mb-3">
                    <Users className="w-5 h-5 text-green-600" />
                    <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">Participants</CardTitle>
                  </div>
                  <div className="text-3xl font-bold text-green-600 dark:text-green-400 mb-1">
                    <AnimatedNumber value={data.uniqueVolunteers} duration={1500} />
                  </div>
                  <p className="text-xs text-gray-500 dark:text-gray-500">
                    Active participants
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Unique Regions */}
            <ScrollReveal direction="up" delay={400}>
              <Card className="shadow-md hover:shadow-lg transition-all duration-300 border-l-4 border-l-purple-500">
                <CardContent className="p-5">
                  <div className="flex items-center justify-between mb-3">
                    <MapPin className="w-5 h-5 text-purple-600" />
                    <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">Regions</CardTitle>
                  </div>
                  <div className="text-3xl font-bold text-purple-600 dark:text-purple-400 mb-1">
                    <AnimatedNumber value={data.uniqueRegions} duration={1500} />
                  </div>
                  <p className="text-xs text-gray-500 dark:text-gray-500">
                    Regions covered
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Average Days to First Sighting */}
            <ScrollReveal direction="up" delay={500}>
              <Card className="shadow-md hover:shadow-lg transition-all duration-300 border-l-4 border-l-teal-500">
                <CardContent className="p-5">
                  <div className="flex items-center justify-between mb-3">
                    <Clock className="w-5 h-5 text-teal-600" />
                    <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">Days to First Sighting</CardTitle>
                  </div>
                  <div className="text-3xl font-bold text-teal-600 dark:text-teal-400 mb-1">
                    {data.averageDaysToFirstSighting !== undefined ? (
                      <>
                        <AnimatedNumber 
                          value={Math.round(data.averageDaysToFirstSighting)} 
                          duration={1500} 
                        />
                        <span className="text-lg ml-1">days</span>
                      </>
                    ) : (
                      <span className="text-lg">N/A</span>
                    )}
                  </div>
                  <p className="text-xs text-gray-500 dark:text-gray-500">
                    Average time to first sighting
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Total Flight Distance */}
            <ScrollReveal direction="up" delay={600}>
              <Card className="shadow-md hover:shadow-lg transition-all duration-300 border-l-4 border-l-indigo-500">
                <CardContent className="p-5">
                  <div className="flex items-center justify-between mb-3">
                    <TrendingUp className="w-5 h-5 text-indigo-600" />
                    <CardTitle className="text-sm font-medium text-gray-600 dark:text-gray-400">Flight Distance</CardTitle>
                  </div>
                  <div className="text-3xl font-bold text-indigo-600 dark:text-indigo-400 mb-1">
                    <AnimatedNumber 
                      value={data.totalFlightDistanceKm} 
                      duration={1500} 
                      decimals={1}
                    />
                    <span className="text-lg ml-1">km</span>
                  </div>
                  <p className="text-xs text-gray-500 dark:text-gray-500">
                    Total distance traveled
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>
          </div>
        </div>
      </section>

      {/* Geographic Distribution Section */}
      <section className="py-12 px-4">
        <div className="container mx-auto max-w-6xl">
          <ScrollReveal direction="up" delay={0}>
            <div className="text-center mb-8">
              <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-2">
                Geographic Distribution
              </h2>
              <p className="text-gray-600 dark:text-gray-400">
                Location statistics for {year}
              </p>
            </div>
          </ScrollReveal>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Release Locations Count */}
            <ScrollReveal direction="up" delay={200}>
              <Card className="shadow-md">
                <CardHeader className="pb-3">
                  <div className="flex items-center gap-2">
                    <MapPin className="w-5 h-5 text-red-600" />
                    <CardTitle className="text-lg">Release Locations</CardTitle>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-red-600 dark:text-red-400 mb-4">
                    {data.releaseLocationsCount.toLocaleString()}
                  </div>
                  {data.mostActiveReleaseLocationAddress && (
                    <div className="pt-4 border-t border-gray-200 dark:border-gray-700">
                      <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">Most Active</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-white line-clamp-2">
                        {data.mostActiveReleaseLocationAddress}
                      </p>
                      <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                        {data.mostActiveReleaseLocationCount} release(s)
                      </p>
                    </div>
                  )}
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Sighting Locations Count */}
            <ScrollReveal direction="up" delay={300}>
              <Card className="shadow-md">
                <CardHeader className="pb-3">
                  <div className="flex items-center gap-2">
                    <Eye className="w-5 h-5 text-blue-600" />
                    <CardTitle className="text-lg">Sighting Locations</CardTitle>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-blue-600 dark:text-blue-400 mb-4">
                    {data.sightingLocationsCount.toLocaleString()}
                  </div>
                  {data.mostActiveSightingLocationAddress && (
                    <div className="pt-4 border-t border-gray-200 dark:border-gray-700">
                      <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">Most Active</p>
                      <p className="text-sm font-medium text-gray-900 dark:text-white line-clamp-2">
                        {data.mostActiveSightingLocationAddress}
                      </p>
                      <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                        {data.mostActiveSightingLocationCount} sighting(s)
                      </p>
                    </div>
                  )}
                </CardContent>
              </Card>
            </ScrollReveal>
          </div>
        </div>
      </section>

      {/* Footer Section */}
      <section className="py-16 px-4 bg-gradient-to-br from-orange-50 to-yellow-50 dark:from-gray-800 dark:to-gray-900">
        <div className="container mx-auto max-w-4xl text-center">
          <ScrollReveal direction="fade" delay={100}>
            <h2 className="text-3xl font-bold text-gray-900 dark:text-white mb-3">
              Thank You to All Participants
            </h2>
            <p className="text-gray-600 dark:text-gray-300 mb-8">
              Looking forward to more amazing data in {year + 1}
            </p>
            <div className="flex gap-4 justify-center">
              <Link href="/">
                <Button variant="outline" className="transition-transform hover:scale-105">
                  <ArrowLeft className="w-4 h-4 mr-2" />
                  Back to Home
                </Button>
              </Link>
            </div>
          </ScrollReveal>
        </div>
      </section>
    </div>
  );
}

