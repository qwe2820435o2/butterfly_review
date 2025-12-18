"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Users } from "lucide-react";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { usePathname, useRouter } from "next/navigation";
import { ThemeToggle } from "@/components/common/ThemeToggle";

export default function Header() {
    const pathname = usePathname();
    const router = useRouter();

    return (
        <header className="bg-white/80 dark:bg-gray-900/80 backdrop-blur-md border-b border-gray-200 dark:border-gray-700 sticky top-0 z-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between items-center h-16">
                    {/* Logo */}
                    <Link href="/" className="flex items-center space-x-2">
                        <div className="w-8 h-8 bg-blue-600 dark:bg-blue-500 rounded-full flex items-center justify-center">
                            <Users className="w-4 h-4 text-white" />
                        </div>
                        <span className="text-xl font-bold text-gray-900 dark:text-white">User Management</span>
                    </Link>

                    {/* Navigation - Desktop */}
                    <nav className="hidden md:flex items-center justify-center flex-1">
                        <Tabs value={pathname} className="w-fit">
                            <TabsList className="bg-white/90 dark:bg-gray-800/90 shadow rounded-lg px-2">
                                <TabsTrigger
                                    value="/users"
                                    className="px-6 py-2 text-base font-medium transition-all
                                        data-[state=active]:text-blue-700 dark:data-[state=active]:text-blue-400 data-[state=active]:font-bold
                                        data-[state=active]:border-b-2 data-[state=active]:border-blue-600 dark:data-[state=active]:border-blue-500
                                        hover:bg-blue-50 dark:hover:bg-blue-900/20 hover:text-blue-700 dark:hover:text-blue-400 active:scale-95
                                        text-gray-700 dark:text-gray-300"
                                    onClick={() => { if (pathname !== "/users") router.push("/users"); }}
                                >
                                    User Management
                                </TabsTrigger>
                            </TabsList>
                        </Tabs>
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