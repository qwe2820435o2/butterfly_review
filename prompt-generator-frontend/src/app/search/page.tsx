"use client";

import React, { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Search, AlertCircle, Sparkles } from "lucide-react";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import EmailSearchForm from "@/components/butterfly/EmailSearchForm";
import TagNumberCard from "@/components/butterfly/TagNumberCard";
import { butterflyService } from "@/services/butterflyService";
import { TagNumberSummary } from "@/types/butterfly";

export default function SearchPage() {
  const [searchResults, setSearchResults] = useState<TagNumberSummary[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [hasSearched, setHasSearched] = useState(false);
  const dispatch = useDispatch();

  /**
   * Handle email search
   */
  const handleSearch = async (email: string) => {
    try {
      setIsLoading(true);
      dispatch(showLoading());
      setHasSearched(false);

      const results = await butterflyService.searchTagNumbersByEmail(email);
      
      setSearchResults(results);
      setHasSearched(true);

      if (results.length === 0) {
        toast.info("No records found", {
          description: `No butterfly tags associated with email ${email}`
        });
      } else {
        toast.success(`Found ${results.length} tag(s)`, {
          description: `Found ${results.length} butterfly tag(s) associated with your email`
        });
      }
    } catch (error) {
      console.error("Search error:", error);
      setHasSearched(true);
      setSearchResults([]);
      
      let errorMessage = "Search failed, please try again later";
      
      if (error && typeof error === "object" && "isAxiosError" in error) {
        const axiosError = error as AxiosError;
        if (axiosError.response?.status === 400) {
          errorMessage = "Invalid request parameters, please check email format";
        } else if (axiosError.response?.status === 404) {
          errorMessage = "No records found";
        } else if (axiosError.response?.status === 500) {
          errorMessage = "Server error, please try again later";
        }
      }
      
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
      dispatch(hideLoading());
    }
  };

  /**
   * Handle view trajectory
   */
  const handleViewTrajectory = (tagNumber: string) => {
    // Navigation is handled by Link in TagNumberCard
    // This callback can be used for analytics or other side effects
    console.log("Viewing trajectory for:", tagNumber);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      <div className="container mx-auto px-4 py-16">
        <div className="max-w-6xl mx-auto">
          {/* Header Section */}
          <div className="text-center mb-12">
            <div className="inline-flex items-center justify-center w-20 h-20 bg-orange-500 dark:bg-orange-600 rounded-full mb-6">
              <Search className="w-10 h-10 text-white" />
            </div>
            <h1 className="text-4xl md:text-5xl font-bold text-gray-900 dark:text-white mb-4">
              Find My Butterfly Tags
            </h1>
            <p className="text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
              Enter your email address to find your released butterfly tags and view their flight trajectories
            </p>
          </div>

          {/* Search Card */}
          <Card className="shadow-lg mb-8">
            <CardHeader>
              <CardTitle>Search</CardTitle>
              <CardDescription>
                Please enter your email
              </CardDescription>
            </CardHeader>
            <CardContent>
              <EmailSearchForm 
                onSearch={handleSearch} 
                isLoading={isLoading}
              />
            </CardContent>
          </Card>

          {/* Results Section */}
          {hasSearched && (
            <div className="mt-8">
              {isLoading ? (
                <Card className="shadow-lg">
                  <CardContent className="py-12 text-center">
                    <div className="flex flex-col items-center justify-center space-y-4">
                      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-orange-600"></div>
                      <p className="text-gray-600 dark:text-gray-300">Searching...</p>
                    </div>
                  </CardContent>
                </Card>
              ) : searchResults.length > 0 ? (
                <>
                  <div className="mb-6">
                    <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                      Found {searchResults.length} tag(s)
                    </h2>
                    <p className="text-gray-600 dark:text-gray-300">
                      Click the "View Trajectory" button to see the complete flight path on the map
                    </p>
                  </div>
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {searchResults.map((summary) => (
                      <TagNumberCard
                        key={summary.tagNumber}
                        summary={summary}
                        onViewTrajectory={handleViewTrajectory}
                      />
                    ))}
                  </div>
                </>
              ) : (
                <Card className="shadow-lg">
                  <CardContent className="py-12 text-center">
                    <div className="flex flex-col items-center justify-center space-y-4">
                      <div className="w-16 h-16 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center">
                        <AlertCircle className="w-8 h-8 text-gray-400" />
                      </div>
                      <div>
                        <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                          No Records Found
                        </h3>
                        <p className="text-gray-600 dark:text-gray-300 max-w-md">
                          No butterfly tags are associated with this email address. Please verify that the email address is correct, or this email may not have participated in the butterfly release project.
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}
            </div>
          )}

          {/* Info Section (shown when no search has been performed) */}
          {!hasSearched && !isLoading && (
            <Card className="shadow-lg mt-8 border-orange-200 dark:border-orange-800">
              <CardHeader>
                <div className="flex items-center space-x-2">
                  <Sparkles className="w-5 h-5 text-orange-600 dark:text-orange-400" />
                  <CardTitle>Instructions</CardTitle>
                </div>
              </CardHeader>
              <CardContent>
                <ul className="space-y-3 text-gray-600 dark:text-gray-300">
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>Enter the email address you used for registration to search</span>
                  </li>
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>The system will display all butterfly tag numbers associated with that email</span>
                  </li>
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>Click the "View Trajectory" button to see the complete flight path on the map</span>
                  </li>
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>Each tag card displays release date, last sighting date, survival status, and more</span>
                  </li>
                </ul>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

