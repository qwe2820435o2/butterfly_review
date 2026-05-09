"use client";

import React, { useState, useEffect } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Plus, User as UserIcon } from "lucide-react";
import { userService } from "@/services/userService";
import { 
  User, 
  PaginatedResultDto
} from "@/types/user";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import UserCard from "@/components/users/UserCard";
import UserEditDialog from "@/components/users/UserEditDialog";
import {
    Pagination,
    PaginationContent,
    PaginationEllipsis,
    PaginationItem,
    PaginationLink,
    PaginationNext,
    PaginationPrevious,
} from "@/components/ui/pagination";

export default function UsersPage() {
  const [searchResult, setSearchResult] = useState<PaginatedResultDto<User> | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(20);
  const [sortBy] = useState<string>("createdAt");
  const [sortDescending] = useState<boolean>(true);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const dispatch = useDispatch();

  useEffect(() => {
    // Load users on initial page load and when dependencies change
    loadUsers();
  }, [currentPage, sortBy, sortDescending]);

  const loadUsers = async () => {
    try {
      dispatch(showLoading());
      const result = await userService.getUsersWithPagination(currentPage, pageSize, sortBy, sortDescending);
      setSearchResult(result);
    } catch (error: unknown) {
      if (error && typeof error === "object" && "isAxiosError" in error) {
        const axiosError = error as AxiosError;
        if (axiosError.response?.status !== 401) {
          toast.error("Failed to load users");
        }
      }
    } finally {
      dispatch(hideLoading());
    }
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleEditUser = (user: User) => {
    setEditingUser(user);
    setIsEditDialogOpen(true);
  };

  const handleDeleteUser = async (user: User) => {
    if (window.confirm(`Are you sure you want to delete user "${user.userName}"?`)) {
      try {
        dispatch(showLoading());
        await userService.deleteUser(user.id);
        toast.success("User deleted successfully!");
        // Refresh the users list
        loadUsers();
      } catch (error) {
        toast.error("Failed to delete user");
      } finally {
        dispatch(hideLoading());
      }
    }
  };

  const handleSaveUser = async (userId: string, updateData: any) => {
    await userService.updateUser(userId, updateData);
    toast.success("User updated successfully!");
    // Refresh the users list
    loadUsers();
  };

  const displayUsers = searchResult && Array.isArray(searchResult.items) ? searchResult.items : [];
  const totalPages = searchResult ? searchResult.totalPages : 1;

  const renderPagination = () => {
    if (totalPages <= 1) return null;

    const pages = [];
    const maxVisiblePages = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
    const endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return (
      <Pagination className="mt-8">
        <PaginationContent>
          {searchResult?.hasPreviousPage && (
            <PaginationItem>
              <PaginationPrevious 
                href="#" 
                size="default"
                onClick={(e) => {
                  e.preventDefault();
                  handlePageChange(currentPage - 1);
                }}
              />
            </PaginationItem>
          )}
          
          {startPage > 1 && (
            <>
              <PaginationItem>
                <PaginationLink 
                  href="#" 
                  size="default"
                  onClick={(e) => {
                    e.preventDefault();
                    handlePageChange(1);
                  }}
                >
                  1
                </PaginationLink>
              </PaginationItem>
              {startPage > 2 && (
                <PaginationItem>
                  <PaginationEllipsis />
                </PaginationItem>
              )}
            </>
          )}

          {pages.map((page) => (
            <PaginationItem key={page}>
              <PaginationLink 
                href="#" 
                size="default"
                isActive={page === currentPage}
                onClick={(e) => {
                  e.preventDefault();
                  handlePageChange(page);
                }}
              >
                {page}
              </PaginationLink>
            </PaginationItem>
          ))}

          {endPage < totalPages && (
            <>
              {endPage < totalPages - 1 && (
                <PaginationItem>
                  <PaginationEllipsis />
                </PaginationItem>
              )}
              <PaginationItem>
                <PaginationLink 
                  href="#" 
                  size="default"
                  onClick={(e) => {
                    e.preventDefault();
                    handlePageChange(totalPages);
                  }}
                >
                  {totalPages}
                </PaginationLink>
              </PaginationItem>
            </>
          )}

          {searchResult?.hasNextPage && (
            <PaginationItem>
              <PaginationNext 
                href="#" 
                size="default"
                onClick={(e) => {
                  e.preventDefault();
                  handlePageChange(currentPage + 1);
                }}
              />
            </PaginationItem>
          )}
        </PaginationContent>
      </Pagination>
    );
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900 py-8">
      <div className="max-w-6xl mx-auto">
        <div className="bg-white dark:bg-gray-900 border-2 border-blue-600 dark:border-blue-500 rounded-2xl shadow-lg p-8">
          {/* Header */}
          <div className="mb-8 flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-blue-700 dark:text-blue-400">User Management</h1>
              <p className="text-gray-600 dark:text-gray-300 mt-2">Manage system users and their permissions</p>
              {searchResult && (
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  Showing {displayUsers.length} of {searchResult.totalCount} users
                  {totalPages > 1 && ` (page ${currentPage} of ${totalPages})`}
                </p>
              )}
            </div>
            <div className="flex items-center space-x-4">
              <Link href="/users/create">
                <Button className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 rounded-lg shadow font-bold text-white">
                  <Plus className="w-4 h-4 mr-2" />
                  Add User
                </Button>
              </Link>
            </div>
          </div>
          
          {/* Simple header with add button */}
          <div className="mb-6">
            <div className="flex items-center justify-between">
              <div>
                <h2 className="text-xl font-semibold text-gray-900 dark:text-white">All Users</h2>
                <p className="text-gray-600 dark:text-gray-300">Manage system users</p>
              </div>
              <Link href="/users/create">
                <Button className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 text-white">
                  <Plus className="w-4 h-4 mr-2" />
                  Add User
                </Button>
              </Link>
            </div>
          </div>
          
          {/* Users Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {displayUsers.map((user) => (
              <UserCard
                key={user.id}
                user={user}
                onEdit={handleEditUser}
                onDelete={handleDeleteUser}
                showActions={true}
              />
            ))}
          </div>

          {displayUsers.length === 0 && (
            <div className="flex flex-col items-center justify-center py-16 text-center text-gray-400 dark:text-gray-500">
              <div className="mb-4">
                <UserIcon className="w-12 h-12 mx-auto text-blue-200 dark:text-blue-700" />
              </div>
              <div className="font-bold text-lg mb-2 text-gray-600 dark:text-gray-300">No users found</div>
              <div className="text-sm mb-4 text-gray-500 dark:text-gray-400">Try adjusting your search criteria or add a new user</div>
              <Link href="/users/create">
                <Button className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 rounded-lg shadow font-bold text-white">
                  <Plus className="w-4 h-4 mr-2" />
                  Add User
                </Button>
              </Link>
            </div>
          )}
          
          {/* Pagination */}
          {renderPagination()}
        </div>
      </div>

      {/* Edit Dialog */}
      <UserEditDialog
        user={editingUser}
        isOpen={isEditDialogOpen}
        onClose={() => {
          setIsEditDialogOpen(false);
          setEditingUser(null);
        }}
        onSave={handleSaveUser}
      />
    </div>
  );
}
