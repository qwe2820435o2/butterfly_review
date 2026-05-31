"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Bird, Eye, Users, Sparkles, LogOut } from "lucide-react";
import { cn } from "@/lib/utils";
import { authStorage } from "@/lib/auth";
import { ThemeToggle } from "@/components/common/ThemeToggle";

const navItems = [
  { href: "/admin/releases", label: "Releases", icon: Bird },
  { href: "/admin/sightings", label: "Sightings", icon: Eye },
  { href: "/admin/users", label: "Users", icon: Users },
];

export default function AdminSidebar() {
  const pathname = usePathname();
  const router = useRouter();

  const handleLogout = () => {
    authStorage.clearToken();
    router.replace("/admin/login");
  };

  return (
    <aside className="w-56 flex-shrink-0 flex flex-col bg-card border-r border-border">
      {/* Logo */}
      <div className="h-16 flex items-center gap-2 px-5 border-b border-border">
        <div className="w-8 h-8 bg-gradient-to-br from-orange-500 to-yellow-500 dark:from-orange-600 dark:to-yellow-600 rounded-full flex items-center justify-center">
          <Sparkles className="w-4 h-4 text-white" />
        </div>
        <span className="text-lg font-bold text-foreground">Butterfly Admin</span>
      </div>

      {/* Nav */}
      <nav className="flex-1 py-4 px-2 space-y-1">
        {navItems.map(({ href, label, icon: Icon }) => {
          const isActive = pathname.startsWith(href);
          return (
            <Link
              key={href}
              href={href}
              className={cn(
                "flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors",
                isActive
                  ? "bg-orange-600 text-white dark:bg-orange-500"
                  : "text-muted-foreground hover:bg-orange-50 dark:hover:bg-orange-900/20 hover:text-orange-700 dark:hover:text-orange-400"
              )}
            >
              <Icon className="w-4 h-4 flex-shrink-0" />
              {label}
            </Link>
          );
        })}
      </nav>

      {/* Footer */}
      <div className="p-3 border-t border-border flex items-center justify-between gap-2">
        <button
          onClick={handleLogout}
          className="flex items-center gap-2 px-3 py-2 rounded-md text-sm font-medium text-muted-foreground hover:bg-red-50 hover:text-red-600 dark:hover:bg-red-900/20 transition-colors"
        >
          <LogOut className="w-4 h-4" />
          Log out
        </button>
        <ThemeToggle />
      </div>
    </aside>
  );
}
