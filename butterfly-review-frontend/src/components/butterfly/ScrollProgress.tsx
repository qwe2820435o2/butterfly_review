"use client";

import React, { useState, useEffect } from "react";

/**
 * Scroll Progress Indicator Component
 * Shows the reading progress of the page
 */
export default function ScrollProgress() {
  const [progress, setProgress] = useState(0);

  useEffect(() => {
    const updateProgress = () => {
      const windowHeight = window.innerHeight;
      const documentHeight = document.documentElement.scrollHeight;
      const scrollTop = window.scrollY;
      const scrollableHeight = documentHeight - windowHeight;
      const progressPercent = scrollableHeight > 0 
        ? (scrollTop / scrollableHeight) * 100 
        : 0;
      setProgress(Math.min(100, Math.max(0, progressPercent)));
    };

    window.addEventListener("scroll", updateProgress);
    updateProgress(); // Initial calculation

    return () => window.removeEventListener("scroll", updateProgress);
  }, []);

  return (
    <div className="fixed top-0 left-0 right-0 h-1 bg-gray-200 dark:bg-gray-800 z-50">
      <div
        className="h-full bg-gradient-to-r from-orange-500 via-yellow-500 to-orange-500 transition-all duration-150 ease-out"
        style={{ width: `${progress}%` }}
      />
    </div>
  );
}

