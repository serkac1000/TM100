import { pgTable, text, serial, integer, boolean } from "drizzle-orm/pg-core";
import { createInsertSchema } from "drizzle-zod";
import { z } from "zod";

export const videos = pgTable("videos", {
  id: serial("id").primaryKey(),
  youtubeUrl: text("youtube_url").notNull(),
  videoId: text("video_id").notNull(),
  autoTranslate: boolean("auto_translate").notNull().default(false),
  translatedText: text("translated_text"),
});

export const insertVideoSchema = createInsertSchema(videos).pick({
  youtubeUrl: true,
  videoId: true,
  autoTranslate: true,
  translatedText: true,
});

export type InsertVideo = z.infer<typeof insertVideoSchema>;
export type Video = typeof videos.$inferSelect;

// URL validation schema
export const youtubeUrlSchema = z.string().refine((url) => {
  try {
    const urlObj = new URL(url);
    return urlObj.hostname === "www.youtube.com" || urlObj.hostname === "youtube.com" || urlObj.hostname === "youtu.be";
  } catch {
    return false;
  }
}, "Invalid YouTube URL");
