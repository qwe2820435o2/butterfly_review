"use client";

import React from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Sparkles, Search, BarChart3, MapPin } from "lucide-react";
import { useDispatch } from "react-redux";
import { hideLoading } from "@/store/slices/loadingSlice";

export default function HomePage() {
  const dispatch = useDispatch();

  React.useEffect(() => {
    // Close global loading when entering homepage
    dispatch(hideLoading());
  }, [dispatch]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      {/* Hero Section */}
      <section className="pt-16 pb-8 px-4">
        <div className="max-w-7xl mx-auto text-center">
          <div className="mb-6">
            {/* Butterfly Icon */}
            <div className="inline-flex items-center justify-center w-20 h-20 bg-gradient-to-br from-orange-500 to-yellow-500 dark:from-orange-600 dark:to-yellow-600 rounded-full mb-4 shadow-lg">
              <Sparkles className="w-10 h-10 text-white animate-pulse" />
            </div>
            
            {/* Main Title */}
            <h1 className="text-4xl md:text-5xl font-bold text-gray-900 dark:text-white mb-4">
              Butterfly Tracking
            </h1>
            
            {/* Description */}
            <p className="text-lg text-gray-600 dark:text-gray-300 max-w-3xl mx-auto leading-relaxed">
              Find your released butterfly tags, visualize flight trajectories, and explore annual data reports
            </p>
          </div>
        </div>
      </section>

      {/* Features Navigation Section */}
      <section className="py-8 px-4 bg-white dark:bg-gray-800">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-8">
            <h2 className="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white mb-2">
              Explore Our Features
            </h2>
            <p className="text-base text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
              Discover the tools to track and visualize butterfly journeys
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {/* Search Tags Feature */}
            <Card className="border-2 border-orange-200 dark:border-orange-800 hover:border-orange-400 dark:hover:border-orange-600 transition-all duration-300 hover:shadow-xl bg-white dark:bg-gray-800 shadow-lg">
              <CardHeader>
                <div className="w-14 h-14 bg-gradient-to-br from-orange-500 to-yellow-500 dark:from-orange-600 dark:to-yellow-600 rounded-lg flex items-center justify-center mb-4">
                  <Search className="w-7 h-7 text-white" />
                </div>
                <CardTitle className="text-xl text-gray-900 dark:text-white">Search Tags</CardTitle>
              </CardHeader>
              <CardContent>
                <CardDescription className="text-base text-gray-600 dark:text-gray-300 mb-4">
                  Find all butterfly tags associated with your email address. View release dates, sighting records, and status information.
                </CardDescription>
                <Link href="/search">
                  <Button className="w-full bg-orange-600 hover:bg-orange-700 dark:bg-orange-500 dark:hover:bg-orange-600 text-white">
                    Start Searching
                  </Button>
                </Link>
              </CardContent>
            </Card>

            {/* View Trajectory Feature */}
            <Card className="border-2 border-blue-200 dark:border-blue-800 hover:border-blue-400 dark:hover:border-blue-600 transition-all duration-300 hover:shadow-xl bg-white dark:bg-gray-800 shadow-lg">
              <CardHeader>
                <div className="w-14 h-14 bg-gradient-to-br from-blue-500 to-indigo-500 dark:from-blue-600 dark:to-indigo-600 rounded-lg flex items-center justify-center mb-4">
                  <MapPin className="w-7 h-7 text-white" />
                </div>
                <CardTitle className="text-xl text-gray-900 dark:text-white">View Trajectory</CardTitle>
              </CardHeader>
              <CardContent>
                <CardDescription className="text-base text-gray-600 dark:text-gray-300 mb-4">
                  Visualize the complete flight path of a butterfly on an interactive map. See release points, sighting locations, and calculate flight distance.
                </CardDescription>
                <Link href="/search">
                  <Button variant="outline" className="w-full border-2 border-blue-600 dark:border-blue-500 text-blue-600 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900/20">
                    Find Your Tag
                  </Button>
                </Link>
              </CardContent>
            </Card>

            {/* Year in Review Feature */}
            <Card className="border-2 border-purple-200 dark:border-purple-800 hover:border-purple-400 dark:hover:border-purple-600 transition-all duration-300 hover:shadow-xl bg-white dark:bg-gray-800 shadow-lg">
              <CardHeader>
                <div className="w-14 h-14 bg-gradient-to-br from-purple-500 to-pink-500 dark:from-purple-600 dark:to-pink-600 rounded-lg flex items-center justify-center mb-4">
                  <BarChart3 className="w-7 h-7 text-white" />
                </div>
                <CardTitle className="text-xl text-gray-900 dark:text-white">Year in Review</CardTitle>
              </CardHeader>
              <CardContent>
                <CardDescription className="text-base text-gray-600 dark:text-gray-300 mb-4">
                  Explore comprehensive annual statistics, geographic distributions, and project achievements in an interactive report.
                </CardDescription>
                <Link href={`/year-in-review/${new Date().getFullYear()}`}>
                  <Button variant="outline" className="w-full border-2 border-purple-600 dark:border-purple-500 text-purple-600 dark:text-purple-400 hover:bg-purple-50 dark:hover:bg-purple-900/20">
                    View Report
                  </Button>
                </Link>
              </CardContent>
            </Card>
          </div>
        </div>
      </section>

      {/* Footer Section */}
      <footer className="bg-gradient-to-br from-orange-50 to-yellow-50 dark:from-gray-800 dark:to-gray-900 border-t border-orange-200 dark:border-gray-700 py-12 px-4 mt-16">
        <div className="max-w-7xl mx-auto">
          <div className="flex flex-col md:flex-row items-center justify-between gap-6">
            {/* Logo and Description */}
            <div className="flex flex-col items-center md:items-start text-center md:text-left">
              <div className="flex items-center space-x-2 mb-3">
                <div className="w-8 h-8 bg-gradient-to-br from-orange-500 to-yellow-500 dark:from-orange-600 dark:to-yellow-600 rounded-full flex items-center justify-center">
                  <Sparkles className="w-4 h-4 text-white" />
                </div>
                <span className="text-xl font-bold text-gray-900 dark:text-white">Butterfly Tracking</span>
              </div>
              <p className="text-gray-600 dark:text-gray-300 text-sm max-w-md">
                Track and visualize butterfly journeys with interactive maps and comprehensive data reports
              </p>
            </div>

            {/* Quick Links */}
            <div className="flex flex-col md:flex-row gap-6 md:gap-8">
              <div className="flex flex-col items-center md:items-start">
                <h3 className="text-sm font-semibold mb-3 text-gray-700 dark:text-gray-300">Quick Links</h3>
                <div className="flex flex-col gap-2">
                  <Link href="/search" className="text-gray-600 dark:text-gray-400 hover:text-orange-600 dark:hover:text-orange-400 transition-colors text-sm">
                    Search Tags
                  </Link>
                  <Link href={`/year-in-review/${new Date().getFullYear()}`} className="text-gray-600 dark:text-gray-400 hover:text-orange-600 dark:hover:text-orange-400 transition-colors text-sm">
                    Year in Review
                  </Link>
                </div>
              </div>
            </div>
          </div>

          {/* Copyright */}
          <div className="mt-8 pt-8 border-t border-orange-200 dark:border-gray-700 text-center">
            <p className="text-gray-600 dark:text-gray-400 text-sm">
              © {new Date().getFullYear()} Butterfly Tracking. All rights reserved.
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}