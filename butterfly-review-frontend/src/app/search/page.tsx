"use client";

import React, { useState, useRef, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { Search, AlertCircle, Sparkles, Eye } from "lucide-react";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import SearchForm from "@/components/butterfly/SearchForm";
import TagNumberCard from "@/components/butterfly/TagNumberCard";
import { butterflyService } from "@/services/butterflyService";
import { TagNumberSummary } from "@/types/butterfly";

export default function SearchPage() {
  const [searchResults, setSearchResults] = useState<TagNumberSummary[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [hasSearched, setHasSearched] = useState(false);
  const [showSightingTagsDialog, setShowSightingTagsDialog] = useState(false);
  const [searchType, setSearchType] = useState<"email" | "tagNumber">("email");
  const [searchQuery, setSearchQuery] = useState("");
  const [highlightedTag, setHighlightedTag] = useState<string | null>(null);
  const cardRefs = useRef<Map<string, HTMLDivElement>>(new Map());
  const dispatch = useDispatch();

  /**
   * Handle search (email or tag number)
   */
  const handleSearch = async (query: string, type: "email" | "tagNumber") => {
    try {
      setIsLoading(true);
      dispatch(showLoading());
      setHasSearched(false);
      setSearchType(type);
      setSearchQuery(query);

      let results: TagNumberSummary[];
      
      if (type === "email") {
        results = await butterflyService.searchTagNumbersByEmail(query);
      } else {
        results = await butterflyService.searchTagNumbersByTagNumber(query);
      }
      
      setSearchResults(results);
      setHasSearched(true);

      if (results.length === 0) {
        toast.info("No records found", {
          description: type === "email" 
            ? `No butterfly tags associated with email ${query}`
            : `No butterfly tags found for tag number ${query}`
        });
      } else {
        toast.success(`Found ${results.length} tag(s)`, {
          description: type === "email"
            ? `Found ${results.length} butterfly tag(s) associated with your email`
            : `Found butterfly tag ${query}`
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
          errorMessage = type === "email"
            ? "Invalid request parameters, please check email format"
            : "Invalid request parameters, please check tag number format";
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

  /**
   * Handle tag number click in dialog - scroll to corresponding card
   */
  const handleTagNumberClick = (tagNumber: string) => {
    // Close dialog
    setShowSightingTagsDialog(false);
    
    // Wait for dialog to close, then scroll to card
    setTimeout(() => {
      const cardElement = cardRefs.current.get(tagNumber);
      if (cardElement) {
        // Scroll to card with smooth behavior
        cardElement.scrollIntoView({ 
          behavior: 'smooth', 
          block: 'center' 
        });
        
        // Highlight the card temporarily
        setHighlightedTag(tagNumber);
        setTimeout(() => {
          setHighlightedTag(null);
        }, 2000); // Remove highlight after 2 seconds
      }
    }, 100);
  };

  /**
   * Set card ref
   */
  const setCardRef = (tagNumber: string, element: HTMLDivElement | null) => {
    if (element) {
      cardRefs.current.set(tagNumber, element);
    } else {
      cardRefs.current.delete(tagNumber);
    }
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
              Enter your email address or tag number to find your released butterfly tags and view their flight trajectories
            </p>
          </div>

          {/* Search Card */}
          <Card className="shadow-lg mb-8">
            <CardHeader>
              <CardTitle>Search</CardTitle>
              <CardDescription>
                Enter your email address or tag number
              </CardDescription>
            </CardHeader>
            <CardContent>
              <SearchForm 
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
                    <p className="text-lg text-gray-700 dark:text-gray-200 mb-2">
                      Total sighting count:{" "}
                      <button
                        onClick={() => setShowSightingTagsDialog(true)}
                        className="text-orange-600 dark:text-orange-400 hover:text-orange-700 dark:hover:text-orange-300 font-semibold underline underline-offset-2 cursor-pointer transition-colors"
                      >
                        {searchResults.reduce((sum, result) => sum + result.sightingCount, 0)}
                      </button>
                    </p>
                    <p className="text-gray-600 dark:text-gray-300">
                      Click the "View Trajectory" button to see the complete flight path on the map
                    </p>
                  </div>

                  {/* Sighting Tags Dialog */}
                  <Dialog open={showSightingTagsDialog} onOpenChange={setShowSightingTagsDialog}>
                    <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
                      <DialogHeader>
                        <DialogTitle className="flex items-center gap-2">
                          <Eye className="w-5 h-5" />
                          Tags with Sightings
                        </DialogTitle>
                        <DialogDescription>
                          All tags with their sighting counts (sorted by sighting count, descending). Click on a tag number to scroll to its details.
                        </DialogDescription>
                      </DialogHeader>
                      <div className="mt-4">
                        {searchResults
                          .filter(result => result.sightingCount > 0)
                          .sort((a, b) => b.sightingCount - a.sightingCount)
                          .map((result) => (
                            <div
                              key={result.tagNumber}
                              onClick={() => handleTagNumberClick(result.tagNumber)}
                              className="flex items-center justify-between p-3 mb-2 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800 hover:border-orange-400 dark:hover:border-orange-600 transition-all cursor-pointer"
                              title={`Click to scroll to ${result.tagNumber} details`}
                            >
                              <div className="flex-1">
                                <div className="font-semibold text-gray-900 dark:text-white">
                                  {result.tagNumber}
                                </div>
                                {result.releaseDatePretty && (
                                  <div className="text-sm text-gray-600 dark:text-gray-400">
                                    Released: {result.releaseDatePretty}
                                  </div>
                                )}
                                {result.lastSightingDatePretty && (
                                  <div className="text-sm text-gray-600 dark:text-gray-400">
                                    Last sighting: {result.lastSightingDatePretty}
                                  </div>
                                )}
                              </div>
                              <div className="ml-4 text-right">
                                <div className="text-lg font-bold text-orange-600 dark:text-orange-400">
                                  {result.sightingCount}
                                </div>
                                <div className="text-xs text-gray-500 dark:text-gray-400">
                                  {result.sightingCount === 1 ? "sighting" : "sightings"}
                                </div>
                              </div>
                            </div>
                          ))}
                        {searchResults.filter(result => result.sightingCount > 0).length === 0 && (
                          <div className="text-center py-8 text-gray-500 dark:text-gray-400">
                            No tags with sightings found.
                          </div>
                        )}
                      </div>
                    </DialogContent>
                  </Dialog>
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {searchResults.map((summary) => (
                      <div
                        key={summary.tagNumber}
                        ref={(el) => setCardRef(summary.tagNumber, el)}
                        className={`transition-all duration-500 ${
                          highlightedTag === summary.tagNumber
                            ? 'ring-4 ring-orange-500 dark:ring-orange-400 rounded-2xl shadow-2xl scale-105'
                            : ''
                        }`}
                      >
                        <TagNumberCard
                          summary={summary}
                          onViewTrajectory={handleViewTrajectory}
                        />
                      </div>
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
                          {searchType === "email"
                            ? "No butterfly tags are associated with this email address. Please verify that the email address is correct, or this email may not have participated in the butterfly release project."
                            : `No butterfly tags found for tag number "${searchQuery}". Please verify that the tag number is correct.`}
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
                    <span>Search by email address: Enter the email address you used for registration to find all associated butterfly tags</span>
                  </li>
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>Search by tag number: Enter a specific tag number (e.g., WAA187) to find that butterfly's information</span>
                  </li>
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>The system will display all butterfly tag numbers matching your search</span>
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

