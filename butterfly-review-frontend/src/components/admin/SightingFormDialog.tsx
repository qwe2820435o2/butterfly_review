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
import { adminSightingService } from "@/services/adminSightingService";
import { SightingSubmission, SightingSubmissionInput } from "@/types/submission";

interface Props {
  open: boolean;
  mode: "create" | "edit";
  record?: SightingSubmission | null;
  onClose: () => void;
  onSaved: () => void;
}

type FormState = {
  tagNumber: string;
  email: string;
  name: string;
  phone: string;
  sightingDateTimeUtc: string;
  condition: string;
  deadOrAlive: string;
  nearbyButterflies: string;
  nearbyPlants: string;
  latitude: string;
  longitude: string;
  address: string;
  howSunny: string;
  howWindy: string;
};

const EMPTY: FormState = {
  tagNumber: "",
  email: "",
  name: "",
  phone: "",
  sightingDateTimeUtc: "",
  condition: "",
  deadOrAlive: "",
  nearbyButterflies: "",
  nearbyPlants: "",
  latitude: "",
  longitude: "",
  address: "",
  howSunny: "",
  howWindy: "",
};

export default function SightingFormDialog({ open, mode, record, onClose, onSaved }: Props) {
  const [form, setForm] = useState<FormState>(EMPTY);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!open) return;
    if (mode === "edit" && record) {
      setForm({
        tagNumber: record.tagNumber ?? "",
        email: record.email ?? "",
        name: record.name ?? "",
        phone: record.phone ?? "",
        sightingDateTimeUtc: record.sightingDateTimeUtc ? record.sightingDateTimeUtc.slice(0, 16) : "",
        condition: record.condition ?? "",
        deadOrAlive: record.deadOrAlive ?? "",
        nearbyButterflies: record.nearbyButterflies ?? "",
        nearbyPlants: record.nearbyPlants ?? "",
        latitude: record.latitude != null ? String(record.latitude) : "",
        longitude: record.longitude != null ? String(record.longitude) : "",
        address: record.address ?? "",
        howSunny: record.howSunny ?? "",
        howWindy: record.howWindy ?? "",
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

    const dto: SightingSubmissionInput = {
      tagNumber: form.tagNumber.trim(),
      email: form.email || null,
      name: form.name || null,
      phone: form.phone || null,
      sightingDateTimeUtc: form.sightingDateTimeUtc || null,
      condition: form.condition || null,
      deadOrAlive: form.deadOrAlive || null,
      nearbyButterflies: form.nearbyButterflies || null,
      nearbyPlants: form.nearbyPlants || null,
      latitude: form.latitude !== "" ? Number(form.latitude) : null,
      longitude: form.longitude !== "" ? Number(form.longitude) : null,
      address: form.address || null,
      howSunny: form.howSunny || null,
      howWindy: form.howWindy || null,
    };

    setSaving(true);
    try {
      if (mode === "edit" && record) {
        await adminSightingService.update(record.id, dto);
        toast.success("Updated");
      } else {
        await adminSightingService.create(dto);
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
          <DialogTitle>{mode === "edit" ? "Edit Sighting" : "New Sighting"}</DialogTitle>
          <DialogDescription>
            {mode === "edit"
              ? "Update the sighting submission and save."
              : "Create a new sighting submission."}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="space-y-1">
              <Label htmlFor="s-tag">Tag # *</Label>
              <Input id="s-tag" value={form.tagNumber} onChange={(e) => set("tagNumber", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-name">Reporter</Label>
              <Input id="s-name" value={form.name} onChange={(e) => set("name", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-email">Email</Label>
              <Input id="s-email" type="email" value={form.email} onChange={(e) => set("email", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-phone">Phone</Label>
              <Input id="s-phone" value={form.phone} onChange={(e) => set("phone", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-date">Sighted</Label>
              <Input id="s-date" type="datetime-local" value={form.sightingDateTimeUtc} onChange={(e) => set("sightingDateTimeUtc", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-doa">Dead or Alive</Label>
              <Input id="s-doa" value={form.deadOrAlive} onChange={(e) => set("deadOrAlive", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-condition">Condition</Label>
              <Input id="s-condition" value={form.condition} onChange={(e) => set("condition", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-sunny">How Sunny</Label>
              <Input id="s-sunny" value={form.howSunny} onChange={(e) => set("howSunny", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-windy">How Windy</Label>
              <Input id="s-windy" value={form.howWindy} onChange={(e) => set("howWindy", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-lat">Latitude</Label>
              <Input id="s-lat" type="number" step="any" value={form.latitude} onChange={(e) => set("latitude", e.target.value)} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="s-lng">Longitude</Label>
              <Input id="s-lng" type="number" step="any" value={form.longitude} onChange={(e) => set("longitude", e.target.value)} />
            </div>
            <div className="space-y-1 sm:col-span-2">
              <Label htmlFor="s-address">Location</Label>
              <Input id="s-address" value={form.address} onChange={(e) => set("address", e.target.value)} />
            </div>
            <div className="space-y-1 sm:col-span-2">
              <Label htmlFor="s-butterflies">Nearby Butterflies</Label>
              <Textarea id="s-butterflies" value={form.nearbyButterflies} onChange={(e) => set("nearbyButterflies", e.target.value)} />
            </div>
            <div className="space-y-1 sm:col-span-2">
              <Label htmlFor="s-plants">Nearby Plants</Label>
              <Textarea id="s-plants" value={form.nearbyPlants} onChange={(e) => set("nearbyPlants", e.target.value)} />
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
