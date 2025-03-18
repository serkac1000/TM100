/// <reference types="youtube" />

declare global {
  interface Window {
    onYouTubeIframeAPIReady: () => void;
    YT: typeof YT;
  }
}

let youtubePromise: Promise<void> | null = null;

export function loadYouTubeAPI(): Promise<void> {
  if (youtubePromise) return youtubePromise;

  youtubePromise = new Promise((resolve, reject) => {
    try {
      // Check if API is already loaded
      if (window.YT) {
        resolve();
        return;
      }

      const tag = document.createElement("script");
      tag.src = "https://www.youtube.com/iframe_api";

      tag.onerror = () => {
        reject(new Error("Failed to load YouTube IFrame API"));
      };

      const firstScriptTag = document.getElementsByTagName("script")[0];
      firstScriptTag.parentNode?.insertBefore(tag, firstScriptTag);

      window.onYouTubeIframeAPIReady = () => {
        resolve();
      };
    } catch (error) {
      reject(error);
    }
  });

  return youtubePromise;
}

export function getVideoIdFromUrl(url: string): string | null {
  try {
    const urlObj = new URL(url);
    if (urlObj.hostname === "youtu.be") {
      return urlObj.pathname.slice(1);
    }
    return urlObj.searchParams.get("v");
  } catch {
    return null;
  }
}