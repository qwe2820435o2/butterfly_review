"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Sparkles } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { authService } from "@/services/authService";
import { authStorage } from "@/lib/auth";

export default function AdminLoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [captchaId, setCaptchaId] = useState("");
  const [captchaQuestion, setCaptchaQuestion] = useState("");
  const [captchaCode, setCaptchaCode] = useState("");
  const [loading, setLoading] = useState(false);

  const loadCaptcha = async () => {
    try {
      const data = await authService.getCaptcha();
      setCaptchaId(data.captchaId);
      setCaptchaQuestion(data.question);
      setCaptchaCode("");
    } catch {
      toast.error("Failed to load captcha");
    }
  };

  useEffect(() => {
    loadCaptcha();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !password || !captchaCode) {
      toast.error("Please fill in all fields");
      return;
    }

    setLoading(true);
    try {
      const result = await authService.login({ email, password, captchaId, captchaCode });

      if (result.role?.toLowerCase() !== "admin") {
        toast.error("This account does not have admin access");
        loadCaptcha();
        return;
      }

      authStorage.setToken(result.token);
      const params = new URLSearchParams(window.location.search);
      const redirectTo = params.get("redirect") || "/admin";
      toast.success(`Welcome back, ${result.userName}`);
      router.push(redirectTo);
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
        "Login failed, please try again";
      toast.error(msg);
      loadCaptcha();
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-orange-50 to-yellow-50 dark:from-gray-950 dark:to-gray-900 px-4">
      <div className="w-full max-w-sm bg-card rounded-xl shadow-md border border-border p-8 space-y-6">
        <div className="text-center space-y-3">
          <div className="mx-auto w-12 h-12 bg-gradient-to-br from-orange-500 to-yellow-500 dark:from-orange-600 dark:to-yellow-600 rounded-full flex items-center justify-center">
            <Sparkles className="w-6 h-6 text-white" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-foreground">Butterfly Admin</h1>
            <p className="mt-1 text-sm text-muted-foreground">Sign in to access the admin panel</p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              autoComplete="email"
              placeholder="Enter your email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </div>

          <div className="space-y-1">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              autoComplete="current-password"
              placeholder="Enter your password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>

          <div className="space-y-1">
            <Label htmlFor="captcha">Captcha</Label>
            <div className="flex items-center gap-2">
              <span
                className="flex-shrink-0 min-w-[110px] px-3 py-2 bg-muted text-sm font-mono rounded-md border border-border cursor-pointer select-none text-foreground"
                onClick={loadCaptcha}
                title="Click to refresh"
              >
                {captchaQuestion || "Loading..."}
              </span>
              <Input
                id="captcha"
                type="text"
                inputMode="numeric"
                placeholder="Answer"
                value={captchaCode}
                onChange={(e) => setCaptchaCode(e.target.value)}
                className="w-24"
              />
            </div>
            <p className="text-xs text-muted-foreground">Click the question to refresh</p>
          </div>

          <Button
            type="submit"
            className="w-full bg-gradient-to-r from-orange-500 to-yellow-500 hover:from-orange-600 hover:to-yellow-600 text-white border-0"
            disabled={loading}
          >
            {loading ? "Signing in..." : "Sign In"}
          </Button>
        </form>
      </div>
    </div>
  );
}
