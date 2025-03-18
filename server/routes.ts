import type { Express } from "express";
import { createServer, type Server } from "http";
import { storage } from "./storage";
import { youtubeUrlSchema, insertVideoSchema } from "@shared/schema";
import { z } from "zod";

export async function registerRoutes(app: Express): Promise<Server> {
  // Video management endpoints
  app.post("/api/videos", async (req, res) => {
    try {
      const validatedData = await youtubeUrlSchema.parseAsync(req.body.youtubeUrl);
      const youtubeUrl = validatedData;

      // Extract video ID from URL
      let videoId = "";
      try {
        const url = new URL(youtubeUrl);
        if (url.hostname === "youtu.be") {
          videoId = url.pathname.slice(1);
        } else {
          videoId = url.searchParams.get("v") || "";
        }
      } catch (e) {
        throw new Error("Invalid YouTube URL");
      }

      if (!videoId) {
        throw new Error("Could not extract video ID from URL");
      }

      const video = await storage.createVideo({
        youtubeUrl,
        videoId,
        autoTranslate: false,
        translatedText: null,
      });

      res.json(video);
    } catch (e) {
      if (e instanceof z.ZodError) {
        res.status(400).json({ message: "Invalid input" });
        return;
      }
      res.status(400).json({ message: e instanceof Error ? e.message : "Unknown error" });
    }
  });

  // Translation endpoint
  app.post("/api/translate", async (req, res) => {
    try {
      const { text } = await z.object({ text: z.string() }).parseAsync(req.body);

      // Here we would integrate with a translation API
      // For demo purposes, prepending "RU: " to simulate translation
      const translated = `RU: ${text}`;

      res.json({ translated });
    } catch (e) {
      res.status(400).json({ message: "Invalid input" });
    }
  });

  const httpServer = createServer(app);
  return httpServer;
}