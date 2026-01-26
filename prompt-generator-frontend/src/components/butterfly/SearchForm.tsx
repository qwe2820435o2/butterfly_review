"use client";

import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Search, Loader2 } from "lucide-react";

interface SearchFormProps {
  onSearch: (query: string, searchType: "email" | "tagNumber") => Promise<void>;
  isLoading?: boolean;
}

/**
 * Search Form Component
 * Automatically detects if input is email or tag number
 */
export default function SearchForm({ onSearch, isLoading = false }: SearchFormProps) {
  const [query, setQuery] = useState("");
  const [error, setError] = useState("");

  /**
   * Check if input looks like an email (contains @)
   */
  const isEmail = (value: string): boolean => {
    return value.includes("@");
  };

  /**
   * Validate email format
   */
  const validateEmail = (emailValue: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(emailValue);
  };

  /**
   * Validate tag number format (alphanumeric, at least 1 character)
   */
  const validateTagNumber = (tagValue: string): boolean => {
    const trimmed = tagValue.trim();
    return trimmed.length > 0 && /^[A-Za-z0-9]+$/.test(trimmed);
  };

  /**
   * Handle input change
   */
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setQuery(value);
    
    // Clear error when user starts typing
    if (error) {
      setError("");
    }
  };

  /**
   * Handle form submission
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Trim query
    const trimmedQuery = query.trim();
    
    // Validation
    if (!trimmedQuery) {
      setError("Please enter an email address or tag number");
      return;
    }

    // Determine search type based on input
    const searchType: "email" | "tagNumber" = isEmail(trimmedQuery) ? "email" : "tagNumber";

    // Validate based on detected type
    if (searchType === "email") {
      if (!validateEmail(trimmedQuery)) {
        setError("Please enter a valid email address");
        return;
      }
    } else {
      if (!validateTagNumber(trimmedQuery)) {
        setError("Please enter a valid tag number (alphanumeric characters only)");
        return;
      }
    }

    // Clear previous error
    setError("");

    try {
      // Normalize tag number to uppercase if it's a tag number
      const normalizedQuery = searchType === "tagNumber" ? trimmedQuery.toUpperCase() : trimmedQuery;
      await onSearch(normalizedQuery, searchType);
    } catch (error) {
      // Error handling is done in parent component
      console.error("Search error:", error);
    }
  };

  /**
   * Handle Enter key press
   */
  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" && !isLoading) {
      handleSubmit(e);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="search">Email Address or Tag Number</Label>
        <div className="relative">
          <Input
            id="search"
            type="text"
            placeholder="e.g., user@example.com or WAA187"
            value={query}
            onChange={handleInputChange}
            onKeyPress={handleKeyPress}
            disabled={isLoading}
            className={error ? "border-red-500 focus-visible:border-red-500" : ""}
            aria-invalid={error ? "true" : "false"}
            aria-describedby={error ? "search-error" : undefined}
            style={{ textTransform: isEmail(query) ? "none" : "uppercase" }}
          />
        </div>
        {error && (
          <p 
            id="search-error" 
            className="text-sm text-red-500 dark:text-red-400"
            role="alert"
          >
            {error}
          </p>
        )}
        <p className="text-xs text-gray-500 dark:text-gray-400">
          Enter an email address or tag number to search
        </p>
      </div>

      <Button 
        type="submit" 
        className="w-full" 
        size="lg"
        disabled={isLoading || !query.trim()}
      >
        {isLoading ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            Searching...
          </>
        ) : (
          <>
            <Search className="mr-2 h-4 w-4" />
            Search
          </>
        )}
      </Button>
    </form>
  );
}

