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