"use client";

import React, { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { 
  formatUserDisplayName,
  getUserInitials,
  formatUserCreatedDateTime
} from "@/types/user";
import { 
  ArrowLeft, 
  User, 
  Mail, 
  Calendar, 
  Edit, 
  Trash2
} from "lucide-react";
import { userService } from "@/services/userService";
import { User as UserType } from "@/types/user";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import Link from "next/link";
import UserEditDialog from "@/components/users/UserEditDialog";

export default function UserDetailPage() {
  const params = useParams();
  const router = useRouter();
  const dispatch = useDispatch();
  const [user, setUser] = useState<UserType | null>(null);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (params.id) {
      loadUser(params.id as string);
    }
  }, [params.id]);

  const loadUser = async (userId: string) => {
    try {
      setIsLoading(true);
      const userData = await userService.getUserById(userId);
      setUser(userData);
    } catch (error: unknown) {
      if (error && typeof error === "object" && "isAxiosError" in error) {
        const axiosError = error as AxiosError;
        if (axiosError.response?.status === 404) {
          toast.error("User not found");
          router.push("/users");
        } else {
          toast.error("Failed to load user");
        }
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleEditUser = () => {
    setIsEditDialogOpen(true);
  };


  const handleDeleteUser = async () => {
    if (!user) return;
    
    if (window.confirm(`Are you sure you want to delete user "${user.userName}"? This action cannot be undone.`)) {
      try {
        dispatch(showLoading());
        await userService.deleteUser(user.id);
        toast.success("User deleted successfully!");
        router.push("/users");
      } catch (error) {
        toast.error("Failed to delete user");
      } finally {
        dispatch(hideLoading());
      }
    }
  };

  const handleSaveUser = async (userId: string, updateData: any) => {
    try {
      await userService.updateUser(userId, updateData);
      toast.success("User updated successfully!");
      // Reload user data
      await loadUser(userId);
    } catch (error) {
      toast.error("Failed to update user");
    }
  };


  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 py-8">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-center py-16">
            <div className="text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
              <p className="mt-4 text-gray-600 dark:text-gray-300">Loading user...</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 py-8">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center py-16">
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">User Not Found</h1>
            <p className="text-gray-600 dark:text-gray-300 mb-8">The user you're looking for doesn't exist.</p>
            <Link href="/users">
              <Button className="bg-blue-600 hover:bg-blue-700 text-white">
                <ArrowLeft className="w-4 h-4 mr-2" />
                Back to Users
              </Button>
            </Link>
          </div>
        </div>
      </div>
    );
  }

  const displayName = formatUserDisplayName(user);
  const initials = getUserInitials(user);
  const createdDate = formatUserCreatedDateTime(user);

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 py-8">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <Link href="/users" className="inline-flex items-center text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white mb-4">
            <ArrowLeft className="w-4 h-4 mr-2" />
            Back to User Management
          </Link>
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white">User Details</h1>
              <p className="text-gray-600 dark:text-gray-300 mt-2">View and manage user information</p>
            </div>
            <div className="flex space-x-2">
              <Button
                onClick={handleEditUser}
                className="bg-blue-600 hover:bg-blue-700 text-white"
              >
                <Edit className="w-4 h-4 mr-2" />
                Edit User
              </Button>
              <Button
                onClick={handleDeleteUser}
                variant="outline"
                className="text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
              >
                <Trash2 className="w-4 h-4 mr-2" />
                Delete User
              </Button>
            </div>
          </div>
        </div>

        {/* User Information */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* User Profile Card */}
          <div className="lg:col-span-1">
            <Card className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-xl">
              <CardHeader className="text-center">
                <Avatar className="h-24 w-24 mx-auto mb-4">
                  <AvatarImage src={user.avatar} alt={displayName} />
                  <AvatarFallback className="bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300 font-semibold text-2xl">
                    {initials}
                  </AvatarFallback>
                </Avatar>
                <CardTitle className="text-2xl font-bold text-gray-900 dark:text-white">
                  {displayName}
                </CardTitle>
                <CardDescription className="text-gray-600 dark:text-gray-300">
                  {user.email}
                </CardDescription>
              </CardHeader>
              <CardContent className="text-center">
                <Badge variant="outline" className="border-blue-300 dark:border-blue-600 text-blue-700 dark:text-blue-300">
                  Active User
                </Badge>
              </CardContent>
            </Card>
          </div>

          {/* User Details */}
          <div className="lg:col-span-2">
            <Card className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-xl">
              <CardHeader>
                <CardTitle className="text-gray-900 dark:text-white">User Information</CardTitle>
                <CardDescription className="text-gray-600 dark:text-gray-300">
                  Detailed information about this user
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {/* Basic Information */}
                  <div className="space-y-4">
                    <h3 className="text-lg font-semibold text-gray-900 dark:text-white">Basic Information</h3>
                    
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="flex items-center space-x-3">
                        <User className="w-5 h-5 text-gray-400" />
                        <div>
                          <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Username</p>
                          <p className="text-gray-900 dark:text-white">{user.userName}</p>
                        </div>
                      </div>
                      
                      <div className="flex items-center space-x-3">
                        <Mail className="w-5 h-5 text-gray-400" />
                        <div>
                          <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Email</p>
                          <p className="text-gray-900 dark:text-white">{user.email}</p>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Account Information */}
                  <div className="space-y-4">
                    <h3 className="text-lg font-semibold text-gray-900 dark:text-white">Account Information</h3>
                    
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="flex items-center space-x-3">
                        <Calendar className="w-5 h-5 text-gray-400" />
                        <div>
                          <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Created Date</p>
                          <p className="text-gray-900 dark:text-white">{createdDate}</p>
                        </div>
                      </div>
                      
                      <div className="flex items-center space-x-3">
                        <div className="w-5 h-5 flex items-center justify-center">
                          <div className="w-2 h-2 bg-blue-500 rounded-full"></div>
                        </div>
                        <div>
                          <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Status</p>
                          <p className="text-blue-600 dark:text-blue-400">Active</p>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* User ID */}
                  <div className="pt-4 border-t border-gray-200 dark:border-gray-700">
                    <div className="flex items-center space-x-3">
                      <div className="w-5 h-5 flex items-center justify-center">
                        <div className="w-2 h-2 bg-gray-400 rounded-full"></div>
                      </div>
                      <div>
                        <p className="text-sm font-medium text-gray-500 dark:text-gray-400">User ID</p>
                        <p className="text-gray-900 dark:text-white font-mono text-sm">{user.id}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>

      {/* Edit Dialog */}
      <UserEditDialog
        user={user}
        isOpen={isEditDialogOpen}
        onClose={() => setIsEditDialogOpen(false)}
        onSave={handleSaveUser}
      />
    </div>
  );
}
