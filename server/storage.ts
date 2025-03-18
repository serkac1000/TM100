import { videos, type Video, type InsertVideo } from "@shared/schema";
import { db } from "./db";
import { eq } from "drizzle-orm";

export interface IStorage {
  getVideo(id: number): Promise<Video | undefined>;
  getVideoByYoutubeId(videoId: string): Promise<Video | undefined>;
  createVideo(video: InsertVideo): Promise<Video>;
  updateVideo(id: number, video: Partial<InsertVideo>): Promise<Video>;
}

export class DatabaseStorage implements IStorage {
  async getVideo(id: number): Promise<Video | undefined> {
    const [video] = await db.select().from(videos).where(eq(videos.id, id));
    return video;
  }

  async getVideoByYoutubeId(videoId: string): Promise<Video | undefined> {
    const [video] = await db.select().from(videos).where(eq(videos.videoId, videoId));
    return video;
  }

  async createVideo(insertVideo: InsertVideo): Promise<Video> {
    const [video] = await db
      .insert(videos)
      .values(insertVideo)
      .returning();
    return video;
  }

  async updateVideo(id: number, update: Partial<InsertVideo>): Promise<Video> {
    const [video] = await db
      .update(videos)
      .set(update)
      .where(eq(videos.id, id))
      .returning();
    if (!video) {
      throw new Error("Video not found");
    }
    return video;
  }
}

export const storage = new DatabaseStorage();