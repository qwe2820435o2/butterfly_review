"use client";

import React from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { 
  User, 
  formatUserDisplayName,
  getUserInitials,
  formatUserCreatedDateTime
} from "@/types/user";
import { 
  User as UserIcon,
  Mail, 
  Calendar, 
  Edit, 
  Trash2
} from "lucide-react";

interface UserCardProps {
  user: User;
  onEdit?: (user: User) => void;
  onDelete?: (user: User) => void;
  showActions?: boolean;
}

export default function UserCard({ 
  user, 
  onEdit, 
  onDelete, 
  showActions = true 
}: UserCardProps) {
  const displayName = formatUserDisplayName(user);
  const initials = getUserInitials(user);
  const createdDate = formatUserCreatedDateTime(user);

  return (
    <Card className="hover:shadow-xl transition-shadow border-2 border-transparent hover:border-blue-600 dark:hover:border-blue-500 rounded-2xl bg-white dark:bg-gray-800">
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="flex items-center space-x-3">
            <Avatar className="h-12 w-12">
              <AvatarImage src={user.avatar} alt={displayName} />
              <AvatarFallback className="bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300 font-semibold">
                {initials}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <CardTitle className="text-lg font-bold text-blue-700 dark:text-blue-400">
                {displayName}
              </CardTitle>
              <CardDescription className="mt-1 text-gray-600 dark:text-gray-300">
                {user.email}
              </CardDescription>
            </div>
          </div>
          {showActions && (
            <div className="flex space-x-2">
              {onEdit && (
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => onEdit(user)}
                  className="text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
                >
                  <Edit className="w-4 h-4" />
                </Button>
              )}
              {onDelete && (
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => onDelete(user)}
                  className="text-red-600 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
                >
                  <Trash2 className="w-4 h-4" />
                </Button>
              )}
            </div>
          )}
        </div>
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          {/* User Info */}
          <div className="flex items-center text-sm text-gray-600 dark:text-gray-300">
            <UserIcon className="w-4 h-4 mr-2" />
            <span className="font-medium">Username:</span>
            <span className="ml-2">{user.userName}</span>
          </div>
          
          <div className="flex items-center text-sm text-gray-600 dark:text-gray-300">
            <Mail className="w-4 h-4 mr-2" />
            <span className="font-medium">Email:</span>
            <span className="ml-2">{user.email}</span>
          </div>
          
          <div className="flex items-center text-sm text-gray-600 dark:text-gray-300">
            <Calendar className="w-4 h-4 mr-2" />
            <span className="font-medium">Created:</span>
            <span className="ml-2">{createdDate}</span>
          </div>

          {/* Status Badge */}
          <div className="flex items-center justify-between pt-2 border-t border-gray-200 dark:border-gray-700">
            <Badge variant="outline" className="border-blue-300 dark:border-blue-600 text-blue-700 dark:text-blue-300">
              Active
            </Badge>
            <div className="text-xs text-gray-500 dark:text-gray-400">
              ID: {user.id}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
