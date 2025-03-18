package com.example.a1

import android.net.Uri

data class Settings(
    val modelUrl: String = "",
    val pose1Uri: Uri? = null,
    val pose2Uri: Uri? = null,
    val pose3Uri: Uri? = null
)
