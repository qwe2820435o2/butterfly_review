"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Sparkles, Search, BarChart3, MapPin } from "lucide-react";
import { usePathname } from "next/navigation";
import { ThemeToggle } from "@/components/common/ThemeToggle";

export default function Header() {
    const pathname = usePathname();

    return (
        <header className="bg-white/80 dark:bg-gray-900/80 backdrop-blur-md border-b border-gray-200 dark:border-gray-700 sticky top-0 z-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between items-center h-16">
                    {/* Logo */}
                    <Link href="/" className="flex items-center space-x-2">
                        <div className="w-8 h-8 bg-gradient-to-br from-orange-500 to-yellow-500 dark:from-orange-600 dark:to-yellow-600 rounded-full flex items-center justify-center">
                            <Sparkles className="w-4 h-4 text-white" />
                        </div>
                        <span className="text-xl font-bold text-gray-900 dark:text-white">Butterfly Tracking</span>
                    </Link>

                    {/* Navigation - Desktop */}
                    <nav className="hidden md:flex items-center justify-center flex-1">
                        <div className="flex items-center gap-2">
                            <Link href="/search">
                                <Button
                                    variant={pathname === "/search" ? "default" : "ghost"}
                                    className={`px-6 py-2 text-base font-medium transition-all
                                        ${pathname === "/search" 
                                            ? "bg-orange-600 hover:bg-orange-700 dark:bg-orange-500 dark:hover:bg-orange-600 text-white" 
                                            : "text-gray-700 dark:text-gray-300 hover:bg-orange-50 dark:hover:bg-orange-900/20 hover:text-orange-700 dark:hover:text-orange-400"
                                        }`}
                                >
                                    <Search className="w-4 h-4 mr-2" />
                                    Search
                                </Button>
                            </Link>
                            <Link href="/map/overview">
                                <Button
                                    variant={pathname === "/map/overview" ? "default" : "ghost"}
                                    className={`px-6 py-2 text-base font-medium transition-all
                                        ${pathname === "/map/overview" 
                                            ? "bg-orange-600 hover:bg-orange-700 dark:bg-orange-500 dark:hover:bg-orange-600 text-white" 
                                            : "text-gray-700 dark:text-gray-300 hover:bg-orange-50 dark:hover:bg-orange-900/20 hover:text-orange-700 dark:hover:text-orange-400"
                                        }`}
                                >
                                    <MapPin className="w-4 h-4 mr-2" />
                                    Overview
                                </Button>
                            </Link>
                            <Link href={`/year-in-review/${new Date().getFullYear()}`}>
                                <Button
                                    variant={pathname?.startsWith("/year-in-review") ? "default" : "ghost"}
                                    className={`px-6 py-2 text-base font-medium transition-all
                                        ${pathname?.startsWith("/year-in-review") 
                                            ? "bg-orange-600 hover:bg-orange-700 dark:bg-orange-500 dark:hover:bg-orange-600 text-white" 
                                            : "text-gray-700 dark:text-gray-300 hover:bg-orange-50 dark:hover:bg-orange-900/20 hover:text-orange-700 dark:hover:text-orange-400"
                                        }`}
                                >
                                    <BarChart3 className="w-4 h-4 mr-2" />
                                    Year in Review
                                </Button>
                            </Link>
                        </div>
                    </nav>

                    {/* User Actions */}
                    <div className="flex items-center space-x-4">
                        {/* Theme Toggle */}
                        <ThemeToggle />
                    </div>
                </div>
            </div>
        </header>
    );
}