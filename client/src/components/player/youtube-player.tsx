/// <reference types="youtube" />
import { useEffect, useRef, useState } from "react";
import { Card } from "@/components/ui/card";
import { Loader2 } from "lucide-react";
import { loadYouTubeAPI } from "@/lib/youtube";

interface YoutubePlayerProps {
  videoId: string;
}

export function YoutubePlayer({ videoId }: YoutubePlayerProps) {
  const playerRef = useRef<HTMLDivElement>(null);
  const player = useRef<YT.Player>();
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string>();

  useEffect(() => {
    let mounted = true;
    console.log("Initializing player with videoId:", videoId);

    const initPlayer = async () => {
      try {
        setIsLoading(true);
        setError(undefined);
        await loadYouTubeAPI();

        if (!mounted || !playerRef.current) return;

        if (player.current) {
          player.current.destroy();
        }

        console.log("Creating new YT.Player instance");
        player.current = new window.YT.Player(playerRef.current, {
          videoId,
          playerVars: {
            autoplay: 1,
            controls: 1,
            modestbranding: 1,
          },
          events: {
            onReady: (event) => {
              console.log("Player ready");
              if (mounted) {
                setIsLoading(false);
                event.target.playVideo();
              }
            },
            onError: (event) => {
              console.error("Player error:", event.data);
              if (mounted) {
                setError("Failed to load video");
                setIsLoading(false);
              }
            },
            onStateChange: (event) => {
              console.log("Player state changed:", event.data);
            },
          },
        });
      } catch (err) {
        console.error("Player initialization error:", err);
        if (mounted) {
          setError("Failed to initialize YouTube player");
          setIsLoading(false);
        }
      }
    };

    initPlayer();

    return () => {
      mounted = false;
      if (player.current) {
        player.current.destroy();
      }
    };
  }, [videoId]);

  return (
    <Card className="overflow-hidden">
      <div className="aspect-video relative">
        <div ref={playerRef} className="w-full h-full" />
        {isLoading && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/10">
            <Loader2 className="w-8 h-8 animate-spin text-primary" />
          </div>
        )}
        {error && (
          <div className="absolute inset-0 flex items-center justify-center bg-black/10">
            <p className="text-red-500">{error}</p>
          </div>
        )}
      </div>
    </Card>
  );
}