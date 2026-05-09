"use client";

import React, { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { User, UpdateUserDto } from "@/types/user";
import { toast } from "sonner";

interface UserEditDialogProps {
  user: User | null;
  isOpen: boolean;
  onClose: () => void;
  onSave: (userId: string, updateData: UpdateUserDto) => Promise<void>;
}

export default function UserEditDialog({ user, isOpen, onClose, onSave }: UserEditDialogProps) {
  const [formData, setFormData] = useState<UpdateUserDto>({
    userName: "",
    avatar: ""
  });
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (user) {
      setFormData({
        userName: user.userName,
        avatar: user.avatar || ""
      });
    }
  }, [user]);

  const handleInputChange = (field: keyof UpdateUserDto, value: string) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!user) return;

    // Validation
    if (!formData.userName.trim()) {
      toast.error("Please enter a username");
      return;
    }

    try {
      setIsLoading(true);
      await onSave(user.id, formData);
      toast.success("User updated successfully!");
      onClose();
    } catch (error) {
      toast.error("Failed to update user");
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    if (!isLoading) {
      onClose();
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Edit User</DialogTitle>
          <DialogDescription>
            Update the user information below. Click save when you're done.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="userName" className="text-right">
                Username
              </Label>
              <Input
                id="userName"
                value={formData.userName}
                onChange={(e) => handleInputChange("userName", e.target.value)}
                className="col-span-3"
                required
              />
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="avatar" className="text-right">
                Avatar URL
              </Label>
              <Input
                id="avatar"
                value={formData.avatar}
                onChange={(e) => handleInputChange("avatar", e.target.value)}
                className="col-span-3"
                placeholder="https://example.com/avatar.jpg"
              />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleClose} disabled={isLoading}>
              Cancel
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading ? "Saving..." : "Save Changes"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
