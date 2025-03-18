package com.example.a1

import android.content.Context
import android.content.SharedPreferences
import android.net.Uri
import android.util.Log
import java.io.File
import java.io.FileOutputStream

// Import the Settings class
import com.example.a1.Settings

class SettingsManager(private val context: Context) {
    private val prefs: SharedPreferences = context.getSharedPreferences("app_settings", Context.MODE_PRIVATE)
    private val imageDir = File(context.filesDir, "pose_images").apply { mkdirs() }

    fun saveSettings(settings: Settings) {
        try {
            // First, clear old files
            clearOldImages()
            // Save new images and get their paths
            val pose1SavedPath = settings.pose1Uri?.let { saveImageToInternalStorage(it, "pose1") }
            val pose2SavedPath = settings.pose2Uri?.let { saveImageToInternalStorage(it, "pose2") }
            val pose3SavedPath = settings.pose3Uri?.let { saveImageToInternalStorage(it, "pose3") }

            Log.d("SettingsManager", "Saving settings - URL: ${settings.modelUrl}, Poses: $pose1SavedPath, $pose2SavedPath, $pose3SavedPath")

            // Save to SharedPreferences
            prefs.edit().apply {
                putString("model_url", settings.modelUrl)
                putString("pose1_uri", pose1SavedPath)
                putString("pose2_uri", pose2SavedPath)
                putString("pose3_uri", pose3SavedPath)
            }.commit() // Use commit() for synchronous write
        } catch (e: Exception) {
            Log.e("SettingsManager", "Error saving settings", e)
            throw e
        }
    }

    private fun clearOldImages() {
        imageDir.listFiles()?.forEach { it.delete() }
    }

    private fun saveImageToInternalStorage(imageUri: Uri, imageName: String): String? {
        val poseImagesDir = File(context.filesDir, "pose_images")
        if (!poseImagesDir.exists()) {
            poseImagesDir.mkdirs() // Create the directory if it doesn't exist
        }

        val imageFile = File(poseImagesDir, imageName)
        try {
            val inputStream = context.contentResolver.openInputStream(imageUri)
            val outputStream = FileOutputStream(imageFile)
            inputStream?.use { input ->
                outputStream.use { output ->
                    input.copyTo(output)
                }
            }
            Log.d("SettingsManager", "Saved image to: ${imageFile.absolutePath}")
            return imageFile.absolutePath
        } catch (e: Exception) {
            Log.e("SettingsManager", "Error saving image", e)
            return null
        }
    }

    fun loadSettings(): Settings {
        return try {
            val modelUrl = prefs.getString("model_url", "") ?: ""
            val pose1Uri = prefs.getString("pose1_uri", null)
            val pose2Uri = prefs.getString("pose2_uri", null)
            val pose3Uri = prefs.getString("pose3_uri", null)

            Log.d("SettingsManager", "Loading settings - URL: $modelUrl, Poses: $pose1Uri, $pose2Uri, $pose3Uri")

            Settings(
                modelUrl = modelUrl,
                pose1Uri = pose1Uri?.let { Uri.parse(it) },
                pose2Uri = pose2Uri?.let { Uri.parse(it) },
                pose3Uri = pose3Uri?.let { Uri.parse(it) }
            )
        } catch (e: Exception) {
            Log.e("SettingsManager", "Error loading settings", e)
            e.printStackTrace()
            Settings()
        }
    }


    fun clearSettings() {
        prefs.edit().clear().commit()
        clearOldImages()
    }
}
