"use client";

import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Search, Loader2 } from "lucide-react";

interface EmailSearchFormProps {
  onSearch: (email: string) => Promise<void>;
  isLoading?: boolean;
}

/**
 * Email Search Form Component
 * Provides email input with validation and search functionality
 */
export default function EmailSearchForm({ onSearch, isLoading = false }: EmailSearchFormProps) {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");

  /**
   * Validate email format
   */
  const validateEmail = (emailValue: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(emailValue);
  };

  /**
   * Handle input change
   */
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setEmail(value);
    
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
    
    // Trim email
    const trimmedEmail = email.trim();
    
    // Validation
    if (!trimmedEmail) {
      setError("请输入邮箱地址");
      return;
    }

    if (!validateEmail(trimmedEmail)) {
      setError("请输入有效的邮箱地址");
      return;
    }

    // Clear previous error
    setError("");

    try {
      await onSearch(trimmedEmail);
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
        <Label htmlFor="email">邮箱地址</Label>
        <div className="relative">
          <Input
            id="email"
            type="email"
            placeholder="例如: user@example.com"
            value={email}
            onChange={handleInputChange}
            onKeyPress={handleKeyPress}
            disabled={isLoading}
            className={error ? "border-red-500 focus-visible:border-red-500" : ""}
            aria-invalid={error ? "true" : "false"}
            aria-describedby={error ? "email-error" : undefined}
          />
        </div>
        {error && (
          <p 
            id="email-error" 
            className="text-sm text-red-500 dark:text-red-400"
            role="alert"
          >
            {error}
          </p>
        )}
      </div>

      <Button 
        type="submit" 
        className="w-full" 
        size="lg"
        disabled={isLoading || !email.trim()}
      >
        {isLoading ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            查询中...
          </>
        ) : (
          <>
            <Search className="mr-2 h-4 w-4" />
            查询
          </>
        )}
      </Button>
    </form>
  );
}

