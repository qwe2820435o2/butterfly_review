"use client";

import React from "react";
import { cn } from "@/lib/utils";

interface AvatarProps {
  avatar?: string;
  userName?: string;
  size?: "sm" | "md" | "lg" | "xl";
  className?: string;
}

const SIZE_CLASSES = {
  sm: "w-8 h-8 text-sm",
  md: "w-12 h-12 text-base", 
  lg: "w-16 h-16 text-lg",
  xl: "w-24 h-24 text-xl"
};

export default function Avatar({ avatar, userName, size = "md", className }: AvatarProps) {
  const sizeClass = SIZE_CLASSES[size];
  
  // If avatar is provided and valid, show the avatar image
  if (avatar && avatar.startsWith("avatar") && avatar.endsWith(".png")) {
    return (
      <div className={cn("relative rounded-full overflow-hidden", sizeClass, className)}>
        <img
          src={`/avatars/${avatar}`}
          alt={`${userName || 'User'} avatar`}
          className="w-full h-full object-cover"
        />
      </div>
    );
  }
  
  // Fallback to initials if no avatar or invalid avatar
  const initials = userName 
    ? userName.split(' ').map(n => n.charAt(0).toUpperCase()).join('').slice(0, 2)
    : 'U';
    
  return (
    <div className={cn(
      "bg-gray-300 dark:bg-gray-600 rounded-full flex items-center justify-center font-medium text-gray-700 dark:text-gray-300",
      sizeClass,
      className
    )}>
      {initials}
    </div>
  );
} 