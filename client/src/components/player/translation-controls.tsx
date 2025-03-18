import { useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { useMutation } from "@tanstack/react-query";
import { apiRequest } from "@/lib/queryClient";
import { useToast } from "@/hooks/use-toast";
import { Volume2, VolumeX } from "lucide-react";

interface TranslationControlsProps {
  videoId: string;
}

export function TranslationControls({ videoId }: TranslationControlsProps) {
  const [isMuted, setIsMuted] = useState(false);
  const [isTranslating, setIsTranslating] = useState(false);
  const { toast } = useToast();

  const translateMutation = useMutation({
    mutationFn: async (text: string) => {
      const res = await apiRequest("POST", "/api/translate", { text });
      return res.json();
    },
    onSuccess: (data) => {
      // Here we would use Web Speech API to speak the translation
      const utterance = new SpeechSynthesisUtterance(data.translated);
      utterance.lang = "ru-RU";
      window.speechSynthesis.speak(utterance);
    },
    onError: () => {
      toast({
        title: "Translation Error",
        description: "Failed to translate the text",
        variant: "destructive",
      });
    },
  });

  const handleTranslate = () => {
    // For demo, translating a fixed text
    // In production, this would come from video captions
    translateMutation.mutate("Hello, how are you?");
  };

  return (
    <Card>
      <CardContent className="pt-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <Button
              variant="outline"
              size="icon"
              onClick={() => setIsMuted(!isMuted)}
            >
              {isMuted ? (
                <VolumeX className="h-5 w-5" />
              ) : (
                <Volume2 className="h-5 w-5" />
              )}
            </Button>
            <div className="flex items-center space-x-2">
              <Switch
                id="auto-translate"
                checked={isTranslating}
                onCheckedChange={setIsTranslating}
              />
              <Label htmlFor="auto-translate">Auto-translate</Label>
            </div>
          </div>
          <Button 
            onClick={handleTranslate}
            disabled={translateMutation.isPending}
          >
            Translate Now
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
