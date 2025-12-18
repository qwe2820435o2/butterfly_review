"use client";

import React from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Users, Settings, Database, Shield, BarChart, UserCheck } from "lucide-react";
import { useDispatch } from "react-redux";
import { hideLoading } from "@/store/slices/loadingSlice";

export default function HomePage() {
  const dispatch = useDispatch();

  React.useEffect(() => {
    // Close global loading when entering homepage
    dispatch(hideLoading());
  }, [dispatch]);

  const features = [
    {
      icon: <Users className="w-6 h-6" />,
      title: "User Management",
      description: "Comprehensive user management system with create, read, update, and delete operations."
    },
    {
      icon: <Settings className="w-6 h-6" />,
      title: "System Configuration",
      description: "Easy configuration and management of system settings and preferences."
    },
    {
      icon: <Database className="w-6 h-6" />,
      title: "Data Management",
      description: "Efficient data storage and retrieval with pagination and search capabilities."
    },
    {
      icon: <Shield className="w-6 h-6" />,
      title: "Security & Access",
      description: "Secure user authentication and role-based access control."
    },
    {
      icon: <BarChart className="w-6 h-6" />,
      title: "Analytics & Reports",
      description: "Detailed analytics and reporting for system usage and user activity."
    },
    {
      icon: <UserCheck className="w-6 h-6" />,
      title: "User Verification",
      description: "User verification and validation processes to ensure data integrity."
    }
  ];

  return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
        {/* Hero Section */}
        <section className="pt-20 pb-16 px-4">
          <div className="max-w-7xl mx-auto text-center">
            <div className="mb-8">
              <div className="inline-flex items-center justify-center w-20 h-20 bg-blue-600 dark:bg-blue-500 rounded-full mb-6">
                <Users className="w-10 h-10 text-white" />
              </div>
              <h1 className="text-5xl md:text-6xl font-bold text-gray-900 dark:text-white mb-6">
                User Management
                <span className="text-blue-600 dark:text-blue-400"> System</span>
              </h1>
              <p className="text-xl text-gray-600 dark:text-gray-300 mb-8 max-w-3xl mx-auto">
                Comprehensive user management system with full CRUD operations, pagination, and modern UI components.
              </p>

              <div className="flex flex-col sm:flex-row gap-4 justify-center">
                <Link href="/users">
                  <Button size="lg" className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 text-lg px-8 py-3 text-white">
                    Manage Users
                  </Button>
                </Link>
                <Link href="/users/create">
                  <Button size="lg" variant="outline" className="text-lg px-8 py-3 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
                    Add New User
                  </Button>
                </Link>
              </div>
            </div>
          </div>
        </section>

        {/* Features Section */}
        <section className="py-16 px-4">
          <div className="max-w-7xl mx-auto">
            <div className="text-center mb-12">
              <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
                Everything You Need for User Management
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
                Our platform provides all the tools you need for comprehensive user management.
              </p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {features.map((feature, index) => (
                  <Card key={index} className="border-2 border-blue-200 dark:border-blue-800 hover:border-blue-400 dark:hover:border-blue-600 transition-colors bg-white dark:bg-gray-800 shadow-lg dark:shadow-gray-900/50">
                    <CardHeader>
                      <div className="w-12 h-12 bg-blue-100 dark:bg-blue-900/30 rounded-lg flex items-center justify-center text-blue-600 dark:text-blue-400 mb-4">
                        {feature.icon}
                      </div>
                      <CardTitle className="text-xl text-gray-900 dark:text-white">{feature.title}</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <CardDescription className="text-base text-gray-600 dark:text-gray-300">
                        {feature.description}
                      </CardDescription>
                    </CardContent>
                  </Card>
              ))}
            </div>
          </div>
        </section>

        {/* CTA Section */}
        <section className="py-16 px-4">
          <div className="max-w-4xl mx-auto text-center">
            <Card className="border-2 border-blue-600 dark:border-blue-500 shadow-2xl bg-white dark:bg-gray-800">
              <CardContent className="pt-12 pb-12">
                <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
                  Ready to Start Managing Users?
                </h2>
                <p className="text-lg text-gray-600 dark:text-gray-300 mb-8">
                  Get started with our comprehensive user management system today.
                </p>
                <div className="flex flex-col sm:flex-row gap-4 justify-center">
                  <Link href="/users">
                    <Button size="lg" className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 text-lg px-8 py-3 text-white">
                      Manage Users
                    </Button>
                  </Link>
                  <Link href="/users/create">
                    <Button size="lg" variant="outline" className="text-lg px-8 py-3 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800">
                      Add New User
                    </Button>
                  </Link>
                </div>
              </CardContent>
            </Card>
          </div>
        </section>

        {/* Footer */}
        <footer className="bg-gray-900 dark:bg-black text-white py-12 px-4">
          <div className="max-w-7xl mx-auto text-center">
            <div className="flex items-center justify-center space-x-2 mb-4">
              <div className="w-8 h-8 bg-blue-600 dark:bg-blue-500 rounded-full flex items-center justify-center">
                <Users className="w-4 h-4 text-white" />
              </div>
              <span className="text-xl font-bold">User Management System</span>
            </div>
            <p className="text-gray-400 dark:text-gray-300 mb-4">
              Comprehensive user management with modern UI and full CRUD operations
            </p>
            <p className="text-gray-500 dark:text-gray-400 text-sm">
              © 2025 User Management System. All rights reserved.
            </p>
          </div>
        </footer>
      </div>
  );
}