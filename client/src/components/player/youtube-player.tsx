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
            mute: 1, // Start muted to allow autoplay
          },
          events: {
            onReady: (event) => {
              console.log("Player ready");
              if (mounted) {
                setIsLoading(false);
                event.target.mute(); // Ensure muted state
                event.target.playVideo();
              }
            },
            onError: (event) => {
              console.error("Player error:", event.data);
              let errorMessage = "Failed to load video";
              switch (event.data) {
                case 2:
                  errorMessage = "Invalid video ID";
                  break;
                case 5:
                  errorMessage = "HTML5 player error";
                  break;
                case 100:
                  errorMessage = "Video not found";
                  break;
                case 101:
                case 150:
                  errorMessage = "Video playback not allowed";
                  break;
              }
              if (mounted) {
                setError(errorMessage);
                setIsLoading(false);
              }
            },
            onStateChange: (event) => {
              console.log("Player state changed:", event.data);
              // -1: unstarted, 0: ended, 1: playing, 2: paused, 3: buffering, 5: video cued
              if (event.data === YT.PlayerState.PLAYING) {
                console.log("Video started playing");
              }
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