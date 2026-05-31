import axios from "axios";
import {toast} from "sonner";
import { API_CONFIG } from "@/lib/config";
import { authStorage } from "@/lib/auth";

// Debug: Log environment variables
console.log("🔍 Environment Variables Debug:");
console.log("NEXT_PUBLIC_API_URL:", process.env.NEXT_PUBLIC_API_URL);
console.log("NODE_ENV:", process.env.NODE_ENV);

// Use environment variable for API URL
const instance = axios.create({
    baseURL: API_CONFIG.BASE_URL,
    timeout: API_CONFIG.REQUEST_CONFIG.TIMEOUT,
    // baseURL: process.env.NEXT_PUBLIC_API_URL || http://localhost:5161 // original code
});

// Debug: Log the actual baseURL being used
console.log("🚀 Axios instance created with baseURL:", instance.defaults.baseURL);

// Attach the admin auth token when present (harmless for anonymous public calls)
instance.interceptors.request.use((config) => {
    const token = authStorage.getToken();
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Handle errors
instance.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response && error.response.status === 401) {
            const onAdmin =
                typeof window !== "undefined" &&
                window.location.pathname.startsWith("/admin") &&
                !window.location.pathname.startsWith("/admin/login");

            if (onAdmin) {
                // Session expired inside the admin area: drop the token and bounce to login.
                authStorage.clearToken();
                toast.error("Session expired, please sign in again", { id: "unauthorized" });
                window.location.href = "/admin/login";
            } else {
                toast.error("Unauthorized", {
                    description: "Please check your credentials.",
                    id: "unauthorized"
                });
            }
        }
        return Promise.reject(error);
    }
);

export default instance;