import { useEffect, useRef } from "react";
import { Card } from "@/components/ui/card";
import { loadYouTubeAPI } from "@/lib/youtube";

interface YoutubePlayerProps {
  videoId: string;
}

export function YoutubePlayer({ videoId }: YoutubePlayerProps) {
  const playerRef = useRef<HTMLDivElement>(null);
  const player = useRef<YT.Player>();

  useEffect(() => {
    let mounted = true;

    const initPlayer = async () => {
      await loadYouTubeAPI();

      if (!mounted || !playerRef.current) return;

      player.current = new YT.Player(playerRef.current, {
        height: "390",
        width: "640",
        videoId,
        playerVars: {
          autoplay: 0,
          controls: 1,
          modestbranding: 1,
        },
      });
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
      <div className="aspect-video">
        <div ref={playerRef} className="w-full h-full" />
      </div>
    </Card>
  );
}
