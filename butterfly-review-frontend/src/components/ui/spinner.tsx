import React from "react";

export function Spinner({ className = "" }: { className?: string }) {
    return (
        <div className={`animate-spin rounded-full border-4 border-blue-600 border-t-transparent w-12 h-12 ${className}`}></div>
    );
}