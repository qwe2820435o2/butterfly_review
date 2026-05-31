import axiosInstance from "./axiosInstance";

export interface CaptchaResponse {
  captchaId: string;
  question: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  captchaId: string;
  captchaCode: string;
}

export interface AuthResponse {
  userId: string;
  userName: string;
  email: string;
  role: string;
  token: string;
}

export const authService = {
  async getCaptcha(): Promise<CaptchaResponse> {
    const res = await axiosInstance.get("/api/auth/captcha");
    return res.data.data;
  },

  async login(req: LoginRequest): Promise<AuthResponse> {
    const res = await axiosInstance.post("/api/auth/login", req);
    return res.data.data;
  },
};
