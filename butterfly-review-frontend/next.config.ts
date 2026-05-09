import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  // Ignore ESLint errors during production build
  eslint: {
    ignoreDuringBuilds: true,
  },
  // Ignore TypeScript errors during production build
  typescript: {
    ignoreBuildErrors: true,
  },
  // Disable standalone output, use traditional startup mode
  // output: 'standalone',
};

export default nextConfig;
