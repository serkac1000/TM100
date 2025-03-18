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
