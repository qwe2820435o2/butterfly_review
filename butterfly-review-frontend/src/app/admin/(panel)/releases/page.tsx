"use client";

import { useCallback, useEffect, useState } from "react";
import { toast } from "sonner";
import { Plus, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { adminReleaseService } from "@/services/adminReleaseService";
import { ReleaseSubmission } from "@/types/submission";
import ReleaseFormDialog from "@/components/admin/ReleaseFormDialog";

const PAGE_SIZE = 20;

const fmtDate = (value?: string | null) =>
  value
    ? new Date(value).toLocaleString("en-US", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
      })
    : "—";

export default function AdminReleasesPage() {
  const [data, setData] = useState<ReleaseSubmission[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] = useState<"create" | "edit">("create");
  const [editRecord, setEditRecord] = useState<ReleaseSubmission | null>(null);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");

  const totalPages = Math.ceil(totalCount / PAGE_SIZE) || 1;

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const result = await adminReleaseService.getPaginated(page, PAGE_SIZE, true, search);
      setData(result.items);
      setTotalCount(result.totalCount);
    } catch {
      toast.error("Failed to load releases");
    } finally {
      setLoading(false);
    }
  }, [page, search]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    setSearch(searchInput.trim());
  };

  const handleClearSearch = () => {
    setSearchInput("");
    setSearch("");
    setPage(1);
  };

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this release record?")) return;
    try {
      await adminReleaseService.delete(id);
      toast.success("Deleted");
      loadData();
    } catch {
      toast.error("Delete failed");
    }
  };

  const handleCreate = () => {
    setDialogMode("create");
    setEditRecord(null);
    setDialogOpen(true);
  };

  const handleEdit = async (id: string) => {
    try {
      const full = await adminReleaseService.getById(id);
      setEditRecord(full);
      setDialogMode("edit");
      setDialogOpen(true);
    } catch {
      toast.error("Failed to load record");
    }
  };

  const handleSaved = () => {
    setDialogOpen(false);
    loadData();
  };

  return (
    <div className="p-6 space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold text-foreground">Release Submissions</h1>
        <div className="flex items-center gap-3">
          <span className="text-sm text-muted-foreground">{totalCount} total</span>
          <Button
            size="sm"
            onClick={handleCreate}
            className="bg-gradient-to-r from-orange-500 to-yellow-500 hover:from-orange-600 hover:to-yellow-600 text-white border-0"
          >
            <Plus className="w-4 h-4 mr-1" />
            Create
          </Button>
        </div>
      </div>

      <form onSubmit={handleSearch} className="flex items-center gap-2">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <Input
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Search by email or tag #"
            className="pl-9"
          />
        </div>
        <Button type="submit" variant="outline" size="sm">Search</Button>
        {search && (
          <Button type="button" variant="ghost" size="sm" onClick={handleClearSearch}>
            Clear
          </Button>
        )}
      </form>

      <div className="bg-card rounded-lg border border-border overflow-hidden">
        {loading ? (
          <div className="py-16 text-center text-muted-foreground">Loading...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border bg-muted/50">
                  {["Tag #", "Email", "Released", "Location", "Sex", "Submitted", "Actions"].map((h) => (
                    <th
                      key={h}
                      className="px-4 py-3 text-left font-semibold text-muted-foreground whitespace-nowrap"
                    >
                      {h}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {data.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-4 py-10 text-center text-muted-foreground">
                      No data
                    </td>
                  </tr>
                ) : (
                  data.map((item) => (
                    <tr
                      key={item.id}
                      className="border-b border-border/60 hover:bg-muted/40 transition-colors"
                    >
                      <td className="px-4 py-3 font-medium text-foreground whitespace-nowrap">
                        {item.tagNumber || "—"}
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{item.email || "—"}</td>
                      <td className="px-4 py-3 text-muted-foreground whitespace-nowrap">
                        {item.releaseDatePretty || fmtDate(item.releaseDateTimeUtc)}
                      </td>
                      <td className="px-4 py-3 text-muted-foreground max-w-[220px] truncate" title={item.address || ""}>
                        {item.address || "—"}
                      </td>
                      <td className="px-4 py-3 text-muted-foreground">{item.sex || "—"}</td>
                      <td className="px-4 py-3 text-muted-foreground whitespace-nowrap">
                        {fmtDate(item.createdAtUtc)}
                      </td>
                      <td className="px-4 py-3 whitespace-nowrap">
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-orange-600 hover:text-orange-700 hover:bg-orange-50 dark:hover:bg-orange-900/20"
                          onClick={() => handleEdit(item.id)}
                        >
                          Edit
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-red-500 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20"
                          onClick={() => handleDelete(item.id)}
                        >
                          Delete
                        </Button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {totalPages > 1 && (
        <Pagination>
          <PaginationContent>
            <PaginationItem>
              <PaginationPrevious
                href="#"
                onClick={(e) => { e.preventDefault(); if (page > 1) setPage(page - 1); }}
                aria-disabled={page === 1}
                className={page === 1 ? "pointer-events-none opacity-50" : ""}
              />
            </PaginationItem>
            {Array.from({ length: Math.min(totalPages, 5) }, (_, i) => i + 1).map((p) => (
              <PaginationItem key={p}>
                <PaginationLink
                  href="#"
                  isActive={p === page}
                  onClick={(e) => { e.preventDefault(); setPage(p); }}
                >
                  {p}
                </PaginationLink>
              </PaginationItem>
            ))}
            <PaginationItem>
              <PaginationNext
                href="#"
                onClick={(e) => { e.preventDefault(); if (page < totalPages) setPage(page + 1); }}
                aria-disabled={page === totalPages}
                className={page === totalPages ? "pointer-events-none opacity-50" : ""}
              />
            </PaginationItem>
          </PaginationContent>
        </Pagination>
      )}

      <ReleaseFormDialog
        open={dialogOpen}
        mode={dialogMode}
        record={editRecord}
        onClose={() => setDialogOpen(false)}
        onSaved={handleSaved}
      />
    </div>
  );
}
