"use client";

import React, { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ArrowLeft, User, Mail, UserPlus } from "lucide-react";
import { userService } from "@/services/userService";
import { CreateUserDto } from "@/types/user";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import Link from "next/link";

export default function CreateUserPage() {
  const router = useRouter();
  const dispatch = useDispatch();
  const [formData, setFormData] = useState<CreateUserDto>({
    userName: "",
    email: "",
    password: "defaultPassword123", // Default password for new users
    avatar: ""
  });

  const handleInputChange = (field: keyof CreateUserDto, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validation
    if (!formData.userName.trim()) {
      toast.error("Please enter a username");
      return;
    }
    if (!formData.email.trim()) {
      toast.error("Please enter an email address");
      return;
    }
    // Password is set to default, no validation needed

    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      toast.error("Please enter a valid email address");
      return;
    }

    try {
      dispatch(showLoading());
      await userService.createUser(formData);
      toast.success("User created successfully!");
      router.push("/users");
    } catch (error: unknown) {
      if (error && typeof error === "object" && "isAxiosError" in error) {
        const axiosError = error as AxiosError;
        if (axiosError.response?.status !== 401) {
          toast.error("Failed to create user, please try again");
        }
      }
    } finally {
      dispatch(hideLoading());
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 py-8">
      <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <Link href="/users" className="inline-flex items-center text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white mb-4">
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to User Management
          </Link>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Create New User</h1>
          <p className="text-gray-600 dark:text-gray-300 mt-2">Add a new user to the system</p>
        </div>

        <Card className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-xl">
          <CardHeader>
            <CardTitle className="text-gray-900 dark:text-white">User Information</CardTitle>
            <CardDescription className="text-gray-600 dark:text-gray-300">Fill in the user details below</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-6">
              {/* Basic Information */}
              <div className="space-y-4">
                <div>
                  <Label htmlFor="userName" className="text-gray-700 dark:text-gray-200">Username *</Label>
                  <div className="relative">
                    <User className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <Input
                      id="userName"
                      value={formData.userName}
                      onChange={(e) => handleInputChange("userName", e.target.value)}
                      placeholder="Enter username"
                      required
                      className="pl-10 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-400"
                    />
                  </div>
                </div>

                <div>
                  <Label htmlFor="email" className="text-gray-700 dark:text-gray-200">Email Address *</Label>
                  <div className="relative">
                    <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                    <Input
                      id="email"
                      type="email"
                      value={formData.email}
                      onChange={(e) => handleInputChange("email", e.target.value)}
                      placeholder="Enter email address"
                      required
                      className="pl-10 bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-400"
                    />
                  </div>
                </div>

                <div className="bg-blue-50 dark:bg-blue-900/20 p-4 rounded-lg">
                  <p className="text-sm text-blue-700 dark:text-blue-300">
                    <strong>Note:</strong> New users will be created with a default password. 
                    Users can change their password after logging in.
                  </p>
                </div>

                <div>
                  <Label htmlFor="avatar" className="text-gray-700 dark:text-gray-200">Avatar URL (optional)</Label>
                  <Input
                    id="avatar"
                    value={formData.avatar}
                    onChange={(e) => handleInputChange("avatar", e.target.value)}
                    placeholder="https://example.com/avatar.jpg"
                    className="bg-white dark:bg-gray-700 border-gray-300 dark:border-gray-600 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-400"
                  />
                </div>
              </div>

              {/* Submit Buttons */}
              <div className="flex gap-4 pt-6">
                <Button type="submit" className="flex-1 bg-blue-600 hover:bg-blue-700 text-white dark:bg-blue-700 dark:hover:bg-blue-800">
                  <UserPlus className="w-4 h-4 mr-2" />
                  Create User
                </Button>
                <Link href="/users" className="flex-1">
                  <Button type="button" variant="outline" className="w-full text-gray-700 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white">
                    Cancel
                  </Button>
                </Link>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
