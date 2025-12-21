"use client";

import React, { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ArrowLeft, Loader2, Sparkles, Calendar, Eye, Users, MapPin, TrendingUp, Award, AlertCircle } from "lucide-react";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import dynamic from "next/dynamic";
import { butterflyService } from "@/services/butterflyService";
import { YearInReview } from "@/types/butterfly";
import ScrollProgress from "@/components/butterfly/ScrollProgress";
import ScrollReveal from "@/components/butterfly/ScrollReveal";
import AnimatedNumber from "@/components/butterfly/AnimatedNumber";

// Dynamically import map component to avoid SSR issues
const GeographicDistributionMap = dynamic(
  () => import("@/components/butterfly/GeographicDistributionMap"),
  { 
    ssr: false,
    loading: () => (
      <div className="h-[600px] w-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 rounded-lg">
        <Loader2 className="w-8 h-8 animate-spin text-orange-600" />
      </div>
    )
  }
);

export default function YearInReviewPage() {
  const params = useParams();
  const router = useRouter();
  const dispatch = useDispatch();
  const year = params.year ? parseInt(params.year as string) : new Date().getFullYear();

  const [data, setData] = useState<YearInReview | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (year && year >= 2000 && year <= 2100) {
      loadYearInReview();
    } else {
      setError("无效的年份");
      setIsLoading(false);
    }
  }, [year]);

  const loadYearInReview = async () => {
    try {
      setIsLoading(true);
      setError(null);
      dispatch(showLoading());

      const yearData = await butterflyService.getYearInReview(year);
      setData(yearData);
    } catch (err) {
      console.error("Load year in review error:", err);
      setError("加载年度报告失败");

      let errorMessage = "加载失败，请稍后重试";
      if (err && typeof err === "object" && "isAxiosError" in err) {
        const axiosError = err as AxiosError;
        if (axiosError.response?.status === 404) {
          errorMessage = "未找到该年份的数据";
        }
      }

      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
      dispatch(hideLoading());
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
        <div className="container mx-auto px-4 py-16">
          <div className="flex items-center justify-center min-h-[400px]">
            <div className="text-center">
              <Loader2 className="w-12 h-12 animate-spin text-orange-600 mx-auto mb-4" />
              <p className="text-gray-600 dark:text-gray-300">正在加载年度报告...</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
        <div className="container mx-auto px-4 py-16">
          <Card className="max-w-2xl mx-auto">
            <CardContent className="py-12 text-center">
              <div className="flex flex-col items-center justify-center space-y-4">
                <div className="w-16 h-16 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center">
                  <AlertCircle className="w-8 h-8 text-gray-400" />
                </div>
                <div>
                  <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                    {error || "未找到数据"}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-6">
                    {year} 年没有相关的数据记录
                  </p>
                  <div className="flex gap-4 justify-center">
                    <Link href="/search">
                      <Button variant="outline">
                        <ArrowLeft className="w-4 h-4 mr-2" />
                        返回搜索
                      </Button>
                    </Link>
                    <Button onClick={() => router.push(`/year-in-review/${new Date().getFullYear()}`)}>
                      查看当前年份
                    </Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      {/* Scroll Progress Indicator */}
      <ScrollProgress />

      {/* Navigation */}
      <div className="sticky top-0 z-40 bg-white/80 dark:bg-gray-900/80 backdrop-blur-sm border-b border-gray-200 dark:border-gray-800">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <Link href="/search">
              <Button variant="ghost" size="sm">
                <ArrowLeft className="w-4 h-4 mr-2" />
                返回
              </Button>
            </Link>
            <div className="text-sm text-gray-600 dark:text-gray-400">
              {year} 年度报告
            </div>
          </div>
        </div>
      </div>

      {/* Cover Section */}
      <section className="min-h-screen flex items-center justify-center px-4 py-16">
        <ScrollReveal direction="fade" delay={200}>
          <div className="text-center max-w-4xl mx-auto">
            <div className="mb-8">
              <Sparkles className="w-24 h-24 text-orange-500 mx-auto mb-6 animate-pulse" />
              <h1 className="text-5xl md:text-7xl font-bold text-gray-900 dark:text-white mb-4">
                {year} Butterfly
              </h1>
              <h2 className="text-3xl md:text-5xl font-bold text-orange-600 dark:text-orange-400 mb-6">
                Year in Review
              </h2>
              <p className="text-xl text-gray-600 dark:text-gray-300 mb-8">
                感谢所有志愿者的贡献，让我们一起回顾这一年的精彩数据
              </p>
            </div>
            <div className="flex justify-center">
              <Button
                size="lg"
                onClick={() => {
                  document.getElementById('overview-section')?.scrollIntoView({ behavior: 'smooth' });
                }}
                className="bg-orange-600 hover:bg-orange-700 text-white transition-transform hover:scale-105"
              >
                开始探索
              </Button>
            </div>
          </div>
        </ScrollReveal>
      </section>

      {/* Overview Statistics Section */}
      <section id="overview-section" className="min-h-screen flex items-center justify-center px-4 py-16">
        <div className="container mx-auto max-w-6xl">
          <ScrollReveal direction="up" delay={0}>
            <div className="text-center mb-12">
              <h2 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
                总览数据
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-300">
                {year} 年项目的核心数据指标
              </p>
            </div>
          </ScrollReveal>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {/* Total Releases */}
            <ScrollReveal direction="up" delay={100}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">总释放数量</CardTitle>
                    <Sparkles className="w-6 h-6 text-orange-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-orange-600 dark:text-orange-400 mb-2">
                    <AnimatedNumber value={data.overview.totalReleases} duration={1500} />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    本年度释放的蝴蝶总数
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Total Sightings */}
            <ScrollReveal direction="up" delay={200}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">总目击次数</CardTitle>
                    <Eye className="w-6 h-6 text-blue-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-blue-600 dark:text-blue-400 mb-2">
                    <AnimatedNumber value={data.overview.totalSightings} duration={1500} />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    所有目击记录的总数
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Unique Volunteers */}
            <ScrollReveal direction="up" delay={300}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">参与志愿者</CardTitle>
                    <Users className="w-6 h-6 text-green-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-green-600 dark:text-green-400 mb-2">
                    <AnimatedNumber value={data.overview.uniqueVolunteers} duration={1500} />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    参与项目的志愿者总数
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Unique Regions */}
            <ScrollReveal direction="up" delay={400}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">覆盖地区</CardTitle>
                    <MapPin className="w-6 h-6 text-purple-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-purple-600 dark:text-purple-400 mb-2">
                    <AnimatedNumber value={data.overview.uniqueRegions} duration={1500} />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    蝴蝶活动覆盖的地理区域数量
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Average Survival Days */}
            {data.overview.averageSurvivalDays !== undefined && (
              <ScrollReveal direction="up" delay={500}>
                <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle className="text-lg">平均存活天数</CardTitle>
                      <Calendar className="w-6 h-6 text-red-600" />
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="text-4xl font-bold text-red-600 dark:text-red-400 mb-2">
                      <AnimatedNumber 
                        value={Math.round(data.overview.averageSurvivalDays)} 
                        duration={1500} 
                      />
                    </div>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      所有蝴蝶的平均存活时间
                    </p>
                  </CardContent>
                </Card>
              </ScrollReveal>
            )}

            {/* Total Flight Distance */}
            <ScrollReveal direction="up" delay={600}>
              <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105">
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-lg">总飞行距离</CardTitle>
                    <TrendingUp className="w-6 h-6 text-indigo-600" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-4xl font-bold text-indigo-600 dark:text-indigo-400 mb-2">
                    <AnimatedNumber 
                      value={data.overview.totalFlightDistanceKm} 
                      duration={1500} 
                      decimals={1}
                      suffix=" 公里"
                    />
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    所有轨迹的总和
                  </p>
                </CardContent>
              </Card>
            </ScrollReveal>

            {/* Survival Rate */}
            {data.overview.survivalRate !== undefined && (
              <ScrollReveal direction="up" delay={700}>
                <Card className="shadow-lg hover:shadow-xl transition-all duration-300 hover:scale-105 md:col-span-2 lg:col-span-1">
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle className="text-lg">存活率</CardTitle>
                      <Award className="w-6 h-6 text-yellow-600" />
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="text-4xl font-bold text-yellow-600 dark:text-yellow-400 mb-2">
                      <AnimatedNumber 
                        value={data.overview.survivalRate} 
                        duration={1500} 
                        decimals={1}
                        suffix="%"
                      />
                    </div>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      存活蝴蝶占总数的百分比
                    </p>
                  </CardContent>
                </Card>
              </ScrollReveal>
            )}
          </div>
        </div>
      </section>

      {/* Geographic Distribution Section */}
      <section className="min-h-screen flex items-center justify-center px-4 py-16">
        <div className="container mx-auto max-w-6xl">
          <ScrollReveal direction="up" delay={0}>
            <div className="text-center mb-12">
              <h2 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
                地理分布
              </h2>
              <p className="text-lg text-gray-600 dark:text-gray-300">
                查看 {year} 年所有蝴蝶的释放和目击地点分布
              </p>
            </div>
          </ScrollReveal>

          <ScrollReveal direction="up" delay={200}>
            <Card className="shadow-lg mb-6">
            <CardHeader>
              <CardTitle>活动范围地图</CardTitle>
              <CardDescription>
                红色标记：释放点 | 蓝色标记：目击点 | 橙色圆圈：最活跃释放地点 | 蓝色圆圈：最活跃目击地点
              </CardDescription>
            </CardHeader>
            <CardContent>
              <GeographicDistributionMap
                geographicDistribution={data.geographicDistribution}
                className="h-[600px] w-full rounded-lg"
              />
              </CardContent>
            </Card>
          </ScrollReveal>

          {/* Statistics Cards */}
          <ScrollReveal direction="up" delay={400}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-6">
            {/* Release Locations Count */}
            <Card className="shadow-lg">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">释放地点</CardTitle>
                  <MapPin className="w-5 h-5 text-red-600" />
                </div>
              </CardHeader>
              <CardContent>
                <div className="text-3xl font-bold text-red-600 dark:text-red-400 mb-2">
                  {data.geographicDistribution.releaseLocations.length.toLocaleString()}
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  个释放地点
                </p>
                {data.geographicDistribution.mostActiveReleaseLocation && (
                  <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
                    <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">最活跃地点</p>
                    <p className="text-sm font-medium text-gray-900 dark:text-white">
                      {data.geographicDistribution.mostActiveReleaseLocation.address || "未知地点"}
                    </p>
                    <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                      {data.geographicDistribution.mostActiveReleaseLocation.count} 次释放
                    </p>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Sighting Locations Count */}
            <Card className="shadow-lg">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">目击地点</CardTitle>
                  <Eye className="w-5 h-5 text-blue-600" />
                </div>
              </CardHeader>
              <CardContent>
                <div className="text-3xl font-bold text-blue-600 dark:text-blue-400 mb-2">
                  {data.geographicDistribution.sightingLocations.length.toLocaleString()}
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  个目击地点
                </p>
                {data.geographicDistribution.mostActiveSightingLocation && (
                  <div className="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
                    <p className="text-xs text-gray-500 dark:text-gray-400 mb-1">最活跃地点</p>
                    <p className="text-sm font-medium text-gray-900 dark:text-white">
                      {data.geographicDistribution.mostActiveSightingLocation.address || "未知地点"}
                    </p>
                    <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                      {data.geographicDistribution.mostActiveSightingLocation.count} 次目击
                    </p>
                  </div>
                )}
              </CardContent>
            </Card>
            </div>
          </ScrollReveal>

          {/* Geographic Bounds Info */}
          {data.geographicDistribution.bounds && (
            <ScrollReveal direction="up" delay={600}>
            <Card className="shadow-lg mt-6">
              <CardHeader>
                <CardTitle className="text-lg">活动范围</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                  <div>
                    <p className="text-gray-600 dark:text-gray-400 mb-1">最北纬度</p>
                    <p className="font-mono font-semibold text-gray-900 dark:text-white">
                      {data.geographicDistribution.bounds.maxLatitude.toFixed(4)}°
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400 mb-1">最南纬度</p>
                    <p className="font-mono font-semibold text-gray-900 dark:text-white">
                      {data.geographicDistribution.bounds.minLatitude.toFixed(4)}°
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400 mb-1">最东经度</p>
                    <p className="font-mono font-semibold text-gray-900 dark:text-white">
                      {data.geographicDistribution.bounds.maxLongitude.toFixed(4)}°
                    </p>
                  </div>
                  <div>
                    <p className="text-gray-600 dark:text-gray-400 mb-1">最西经度</p>
                    <p className="font-mono font-semibold text-gray-900 dark:text-white">
                      {data.geographicDistribution.bounds.minLongitude.toFixed(4)}°
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
            </ScrollReveal>
          )}
        </div>
      </section>

      {/* Footer Section */}
      <section className="min-h-screen flex items-center justify-center px-4 py-16">
        <ScrollReveal direction="fade" delay={200}>
          <div className="container mx-auto max-w-6xl text-center">
            <h2 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
              感谢所有志愿者的贡献
            </h2>
            <p className="text-lg text-gray-600 dark:text-gray-300 mb-8">
              让我们一起期待 {year + 1} 年更多的精彩数据
            </p>
            <div className="flex gap-4 justify-center">
              <Link href="/search">
                <Button variant="outline" className="transition-transform hover:scale-105">
                  <ArrowLeft className="w-4 h-4 mr-2" />
                  返回搜索
                </Button>
              </Link>
              <Button
                onClick={() => {
                  window.scrollTo({ top: 0, behavior: 'smooth' });
                }}
                className="transition-transform hover:scale-105"
              >
                返回顶部
              </Button>
            </div>
          </div>
        </ScrollReveal>
      </section>
    </div>
  );
}

