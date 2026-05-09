import React from "react";
import { Spinner } from "@/components/ui/spinner";
import {useSelector} from "react-redux";
import {RootState} from "@/store";

export default function GlobalLoading() {
    const isLoading = useSelector((state: RootState) => state.loading.isLoading);

    if (!isLoading) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30">
            <Spinner className="w-16 h-16" />
        </div>
    );
}