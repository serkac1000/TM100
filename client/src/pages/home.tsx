import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { YoutubePlayer } from "@/components/player/youtube-player";
import { TranslationControls } from "@/components/player/translation-controls";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { youtubeUrlSchema } from "@shared/schema";
import { Form, FormControl, FormField, FormItem } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useMutation } from "@tanstack/react-query";
import { apiRequest } from "@/lib/queryClient";
import { useToast } from "@/hooks/use-toast";

export default function Home() {
  const [videoId, setVideoId] = useState<string>();
  const { toast } = useToast();
  
  const form = useForm({
    resolver: zodResolver(youtubeUrlSchema),
    defaultValues: {
      youtubeUrl: "",
    },
  });

  const urlMutation = useMutation({
    mutationFn: async (url: string) => {
      const res = await apiRequest("POST", "/api/videos", { youtubeUrl: url });
      return res.json();
    },
    onSuccess: (data) => {
      setVideoId(data.videoId);
    },
    onError: (error) => {
      toast({
        title: "Error",
        description: error instanceof Error ? error.message : "Failed to load video",
        variant: "destructive",
      });
    },
  });

  const onSubmit = (data: { youtubeUrl: string }) => {
    urlMutation.mutate(data.youtubeUrl);
  };

  return (
    <div className="min-h-screen bg-background p-6">
      <div className="max-w-4xl mx-auto space-y-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-3xl font-bold bg-gradient-to-r from-primary to-primary/60 bg-clip-text text-transparent">
              YouTube Translator
            </CardTitle>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                  control={form.control}
                  name="youtubeUrl"
                  render={({ field }) => (
                    <FormItem>
                      <FormControl>
                        <div className="flex gap-2">
                          <Input
                            placeholder="Enter YouTube URL..."
                            {...field}
                            className="flex-1"
                          />
                          <Button 
                            type="submit"
                            disabled={urlMutation.isPending}
                          >
                            Load Video
                          </Button>
                        </div>
                      </FormControl>
                    </FormItem>
                  )}
                />
              </form>
            </Form>
          </CardContent>
        </Card>

        {videoId && (
          <div className="space-y-4">
            <YoutubePlayer videoId={videoId} />
            <TranslationControls videoId={videoId} />
          </div>
        )}
      </div>
    </div>
  );
}
