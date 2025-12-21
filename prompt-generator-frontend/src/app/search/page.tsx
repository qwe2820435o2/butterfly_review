"use client";

import React, { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Search, AlertCircle, Sparkles } from "lucide-react";
import { useDispatch } from "react-redux";
import { showLoading, hideLoading } from "@/store/slices/loadingSlice";
import { toast } from "sonner";
import { AxiosError } from "axios";
import EmailSearchForm from "@/components/butterfly/EmailSearchForm";
import TagNumberCard from "@/components/butterfly/TagNumberCard";
import { butterflyService } from "@/services/butterflyService";
import { TagNumberSummary } from "@/types/butterfly";

export default function SearchPage() {
  const [searchResults, setSearchResults] = useState<TagNumberSummary[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [hasSearched, setHasSearched] = useState(false);
  const dispatch = useDispatch();

  /**
   * Handle email search
   */
  const handleSearch = async (email: string) => {
    try {
      setIsLoading(true);
      dispatch(showLoading());
      setHasSearched(false);

      const results = await butterflyService.searchTagNumbersByEmail(email);
      
      setSearchResults(results);
      setHasSearched(true);

      if (results.length === 0) {
        toast.info("未找到相关记录", {
          description: `邮箱 ${email} 没有关联的蝴蝶标签号`
        });
      } else {
        toast.success(`找到 ${results.length} 个标签号`, {
          description: `已为您找到 ${results.length} 个相关的蝴蝶标签`
        });
      }
    } catch (error) {
      console.error("Search error:", error);
      setHasSearched(true);
      setSearchResults([]);
      
      let errorMessage = "查询失败，请稍后重试";
      
      if (error && typeof error === "object" && "isAxiosError" in error) {
        const axiosError = error as AxiosError;
        if (axiosError.response?.status === 400) {
          errorMessage = "请求参数错误，请检查邮箱格式";
        } else if (axiosError.response?.status === 404) {
          errorMessage = "未找到相关记录";
        } else if (axiosError.response?.status === 500) {
          errorMessage = "服务器错误，请稍后重试";
        }
      }
      
      toast.error(errorMessage);
    } finally {
      setIsLoading(false);
      dispatch(hideLoading());
    }
  };

  /**
   * Handle view trajectory
   */
  const handleViewTrajectory = (tagNumber: string) => {
    // Navigation is handled by Link in TagNumberCard
    // This callback can be used for analytics or other side effects
    console.log("Viewing trajectory for:", tagNumber);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 via-white to-yellow-50 dark:from-gray-900 dark:via-gray-800 dark:to-gray-900">
      <div className="container mx-auto px-4 py-16">
        <div className="max-w-6xl mx-auto">
          {/* Header Section */}
          <div className="text-center mb-12">
            <div className="inline-flex items-center justify-center w-20 h-20 bg-orange-500 dark:bg-orange-600 rounded-full mb-6">
              <Search className="w-10 h-10 text-white" />
            </div>
            <h1 className="text-4xl md:text-5xl font-bold text-gray-900 dark:text-white mb-4">
              查找我的蝴蝶标签
            </h1>
            <p className="text-lg text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
              输入您的邮箱地址，查找您释放的蝴蝶标签号，查看它们的飞行轨迹
            </p>
          </div>

          {/* Search Card */}
          <Card className="shadow-lg mb-8">
            <CardHeader>
              <CardTitle>搜索</CardTitle>
              <CardDescription>
                请输入您注册时使用的邮箱地址
              </CardDescription>
            </CardHeader>
            <CardContent>
              <EmailSearchForm 
                onSearch={handleSearch} 
                isLoading={isLoading}
              />
            </CardContent>
          </Card>

          {/* Results Section */}
          {hasSearched && (
            <div className="mt-8">
              {isLoading ? (
                <Card className="shadow-lg">
                  <CardContent className="py-12 text-center">
                    <div className="flex flex-col items-center justify-center space-y-4">
                      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-orange-600"></div>
                      <p className="text-gray-600 dark:text-gray-300">正在查询...</p>
                    </div>
                  </CardContent>
                </Card>
              ) : searchResults.length > 0 ? (
                <>
                  <div className="mb-6">
                    <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                      找到 {searchResults.length} 个标签号
                    </h2>
                    <p className="text-gray-600 dark:text-gray-300">
                      点击"查看轨迹"按钮可以在地图上查看蝴蝶的完整飞行路径
                    </p>
                  </div>
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {searchResults.map((summary) => (
                      <TagNumberCard
                        key={summary.tagNumber}
                        summary={summary}
                        onViewTrajectory={handleViewTrajectory}
                      />
                    ))}
                  </div>
                </>
              ) : (
                <Card className="shadow-lg">
                  <CardContent className="py-12 text-center">
                    <div className="flex flex-col items-center justify-center space-y-4">
                      <div className="w-16 h-16 bg-gray-100 dark:bg-gray-800 rounded-full flex items-center justify-center">
                        <AlertCircle className="w-8 h-8 text-gray-400" />
                      </div>
                      <div>
                        <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                          未找到相关记录
                        </h3>
                        <p className="text-gray-600 dark:text-gray-300 max-w-md">
                          该邮箱地址没有关联的蝴蝶标签号。请确认您输入的邮箱地址是否正确，或者该邮箱尚未参与蝴蝶释放项目。
                        </p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}
            </div>
          )}

          {/* Info Section (shown when no search has been performed) */}
          {!hasSearched && !isLoading && (
            <Card className="shadow-lg mt-8 border-orange-200 dark:border-orange-800">
              <CardHeader>
                <div className="flex items-center space-x-2">
                  <Sparkles className="w-5 h-5 text-orange-600 dark:text-orange-400" />
                  <CardTitle>使用说明</CardTitle>
                </div>
              </CardHeader>
              <CardContent>
                <ul className="space-y-3 text-gray-600 dark:text-gray-300">
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>输入您注册时使用的邮箱地址进行查询</span>
                  </li>
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>系统将显示该邮箱关联的所有蝴蝶标签号</span>
                  </li>
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>点击"查看轨迹"按钮可以在地图上查看蝴蝶的完整飞行路径</span>
                  </li>
                  <li className="flex items-start">
                    <span className="mr-2">•</span>
                    <span>每个标签号卡片显示释放日期、最后目击日期、存活状态等信息</span>
                  </li>
                </ul>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

