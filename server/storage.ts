import { videos, type Video, type InsertVideo } from "@shared/schema";

export interface IStorage {
  getVideo(id: number): Promise<Video | undefined>;
  getVideoByYoutubeId(videoId: string): Promise<Video | undefined>;
  createVideo(video: InsertVideo): Promise<Video>;
  updateVideo(id: number, video: Partial<InsertVideo>): Promise<Video>;
}

export class MemStorage implements IStorage {
  private videos: Map<number, Video>;
  private currentId: number;

  constructor() {
    this.videos = new Map();
    this.currentId = 1;
  }

  async getVideo(id: number): Promise<Video | undefined> {
    return this.videos.get(id);
  }

  async getVideoByYoutubeId(videoId: string): Promise<Video | undefined> {
    return Array.from(this.videos.values()).find(
      (video) => video.videoId === videoId
    );
  }

  async createVideo(insertVideo: InsertVideo): Promise<Video> {
    const id = this.currentId++;
    const video: Video = { ...insertVideo, id };
    this.videos.set(id, video);
    return video;
  }

  async updateVideo(id: number, update: Partial<InsertVideo>): Promise<Video> {
    const existing = await this.getVideo(id);
    if (!existing) {
      throw new Error("Video not found");
    }
    const updated = { ...existing, ...update };
    this.videos.set(id, updated);
    return updated;
  }
}

export const storage = new MemStorage();
