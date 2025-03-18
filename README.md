<<<<<<< HEAD
# YouTube Video Player with Russian Translation

A web application that allows users to watch YouTube videos with real-time Russian translation capabilities.

## Features

- YouTube video playback
- Russian translation of video content
- Text-to-speech for translated content
- Automatic translation toggle
- Real-time caption translation

## Tech Stack

- Frontend: React + TypeScript
- Backend: Express.js
- Database: PostgreSQL
- Styling: Tailwind CSS + shadcn/ui
- State Management: TanStack Query

## Getting Started

1. Clone the repository
2. Install dependencies:
   ```bash
   npm install
   ```
3. Set up the database:
   ```bash
   npm run db:push
   ```
4. Start the development server:
   ```bash
   npm run dev
   ```

## Environment Variables

The following environment variables are required:
- `DATABASE_URL`: PostgreSQL database connection string

## License

MIT
=======
# Pose Recognition System - Skeleton Comparison

A Windows desktop application for AI-powered pose recognition and guidance using skeleton detection technology.

## Features

- Real-time pose detection using skeleton tracking
- Comparison of user poses with pre-defined poses
- Automatic progression between poses when threshold is achieved
- Customizable detection threshold (default: 50%)
- Training statistics and performance tracking
- Configuration settings for personalized experience

## Technology Stack

- C# / .NET Core
- Computer Vision AI for skeleton detection
- Real-time webcam input processing
- Interactive user interface

## How It Works

The system uses computer vision to detect a person's skeleton from webcam input. It then compares the detected skeleton with pre-trained pose templates. When a user achieves a match above the configurable threshold (default 50%), the system can automatically progress to the next pose in the sequence.

## Settings

- Detection threshold: Configurable match percentage required for pose recognition
- Hold duration: How long a pose must be maintained
- Auto progression: Automatically advance to next pose when current pose is completed

## Latest Updates

- Converted from yoga-specific to general pose recognition system
- Fixed class naming inconsistencies (PoseAppSettings → YogaAppSettings)
- Implemented automatic pose progression system
- Updated interface to display generic pose names
- Improved build system with minimal warnings
>>>>>>> d47fca8336474ee3766690733c00cdc83cb4196b
