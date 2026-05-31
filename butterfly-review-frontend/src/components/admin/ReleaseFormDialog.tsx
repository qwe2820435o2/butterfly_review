"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { adminReleaseService } from "@/services/adminReleaseService";
import { ReleaseSubmission, ReleaseSubmissionInput } from "@/types/submission";

interface Props {
  open: boolean;
  mode: "create" | "edit";
  record?: ReleaseSubmission | null;
  onClose: () => void;
  onSaved: () => void;
}

type FormState = {
  tagNumber: string;
  email: string;
  releaseDateTimeUtc: string;
  sex: string;
  wind: string;
  sun: string;
  latitude: string;
  longitude: string;
  address: string;
  notes: string;
};

const EMPTY: FormState = {
  tagNumber: "",
  email: "",
  releaseDateTimeUtc: "",
  sex: "",
  wind: "",
  sun: "",
  latitude: "",
  longitude: "",
  address: "",
  notes: "",
};

export default function ReleaseFormDialog({ open, mode, record, onClose, onSaved }: Props) {
  const [form, setForm] = useState<FormState>(EMPTY);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!open) return;
    if (mode === "edit" && record) {
      setForm({
        tagNumber: record.tagNumber ?? "",
        email: record.email ?? "",
        releaseDateTimeUtc: record.releaseDateTimeUtc ? record.releaseDateTimeUtc.slice(0, 16) : "",
        sex: record.sex ?? "",
        wind: record.wind ?? "",
        sun: record.sun ?? "",
        latitude: record.latitude != null ? String(record.latitude) : "",
        longitude: record.longitude != null ? String(record.longitude) : "",
        address: record.address ?? "",
        notes: record.notes ?? "",
      });
    } else {
      setForm(EMPTY);
    }
  }, [open, mode, record]);

  const set = (key: keyof FormState, value: string) =>
    setForm((prev) => ({ ...prev, [key]: value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.tagNumber.trim()) {
      toast.error("Tag # is required");
      return;
    }

    const dto: ReleaseSubmissionInput = {
      tagNumber: form.tagNumber.trim(),
      email: form.email || null,
      releaseDateTimeUtc: form.releaseDateTimeUtc || null,
      sex: form.sex || null,
      wind: form.wind || null,
      sun: form.sun || null,
      latitude: form.latitude !== "" ? Number(form.latitude) : null,
      longitude: form.longitude !== "" ? Number(form.longitude) : null,
      address: form.address || null,
      notes: form.notes || null,
    };

    setSaving(true);
    try {
      if (mode === "edit" && record) {
        await adminReleaseService.update(record.id, dto);
        toast.success("Updated");
      } else {
        await adminReleaseService.create(dto);
        toast.success("Created");
      }
      onSaved();
    } catch {
      toast.error(mode === "edit" ? "Update failed" : "Create failed");
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={(o) => !o && !saving && onClose()}>
      <DialogContent className="sm:max-w-2xl max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{mode === "edit" ? "Edit Release" : "New Release"}</DialogTitle>
          <DialogDescription>
            {mode === "edit"
              ? "Update the release submission and save."
              : "Create a new release submission."}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="space-y-1">
              <Label htmlFor="r-tag">Tag # *</Label>
              <Input id="r-tag" value={form.tagNumber} onChange={(e) => set("tagNumber", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="r-email">Email</Label>
              <Input id="r-email" type="email" value={form.email} onChange={(e) => set("email", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="r-date">Released</Label>
              <Input id="r-date" type="datetime-local" value={form.releaseDateTimeUtc} onChange={(e) => set("releaseDateTimeUtc", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="r-sex">Sex</Label>
              <Input id="r-sex" value={form.sex} onChange={(e) => set("sex", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="r-wind">Wind</Label>
              <Input id="r-wind" value={form.wind} onChange={(e) => set("wind", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="r-sun">Sun</Label>
              <Input id="r-sun" value={form.sun} onChange={(e) => set("sun", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="r-lat">Latitude</Label>
              <Input id="r-lat" type="number" step="any" value={form.latitude} onChange={(e) => set("latitude", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="r-lng">Longitude</Label>
              <Input id="r-lng" type="number" step="any" value={form.longitude} onChange={(e) => set("longitude", e.target.value)} />
            </div>
            <div className="space-y-1 sm:col-span-2">
              <Label htmlFor="r-address">Location</Label>
              <Input id="r-address" value={form.address} onChange={(e) => set("address", e.target.value)} />
            </div>
            <div className="space-y-1 sm:col-span-2">
              <Label htmlFor="r-notes">Notes</Label>
              <Textarea id="r-notes" value={form.notes} onChange={(e) => set("notes", e.target.value)} />
            </div>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose} disabled={saving}>
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={saving}
              className="bg-gradient-to-r from-orange-500 to-yellow-500 hover:from-orange-600 hover:to-yellow-600 text-white border-0"
            >
              {saving ? "Saving..." : "Save"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
