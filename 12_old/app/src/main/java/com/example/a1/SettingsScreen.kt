package com.example.a1

import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalFocusManager
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage
import coil.request.ImageRequest
import java.io.File
import java.io.FileOutputStream
import android.util.Log
import android.app.Application
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.activity.compose.BackHandler
import android.content.Intent
import android.provider.MediaStore

@Composable
fun SettingsScreen(onNavigateBack: () -> Unit) {
    val context = LocalContext.current
    val viewModel: SettingsViewModel = remember {
        SettingsViewModel(context.applicationContext as Application)
    }
    val state by viewModel.settingsState.collectAsStateWithLifecycle()
    val focusManager = LocalFocusManager.current

    // Add effect to reload settings when screen becomes active
    LaunchedEffect(Unit) {
        viewModel.loadSavedSettings()
    }

    // Back handler to navigate back
    BackHandler {
        onNavigateBack()
    }

    Scaffold { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
                .padding(16.dp)
                .verticalScroll(rememberScrollState()),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            // Back button
            IconButton(onClick = onNavigateBack) {
                Text("← Back")
            }

            // URL Input field with its own save button
            Column {
                Text(
                    text = "Teachable Machine Model URL",
                    style = MaterialTheme.typography.bodyMedium,
                    fontWeight = FontWeight.Bold,
                    modifier = Modifier.padding(bottom = 4.dp)
                )
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    OutlinedTextField(
                        value = state.modelUrl,
                        onValueChange = { viewModel.updateModelUrl(it) },
                        modifier = Modifier.weight(1f),
                        singleLine = true
                    )
                    Button(
                        onClick = { viewModel.toggleUrlSave() },
                        enabled = state.modelUrl.trim().isNotEmpty(),
                        colors = ButtonDefaults.buttonColors(
                            containerColor = if (state.isUrlSaved)
                                MaterialTheme.colorScheme.secondary
                            else
                                MaterialTheme.colorScheme.primary
                        )
                    ) {
                        Text(if (state.isUrlSaved) "Saved" else "Save URL")
                    }
                }
            }

            // Pose fields section
            Text(
                text = "Reference Poses",
                style = MaterialTheme.typography.titleMedium,
                fontWeight = FontWeight.Bold,
                modifier = Modifier.padding(top = 8.dp, bottom = 4.dp)
            )

            // Pose 1 with save button
            Column {
                Text(
                    text = "First Reference Pose",
                    style = MaterialTheme.typography.bodyMedium,
                    modifier = Modifier.padding(bottom = 4.dp)
                )
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                    verticalAlignment = Alignment.Bottom
                ) {
                    Box(modifier = Modifier.weight(1f)) {
                        PoseSelectionField(
                            label = "Pose 1",
                            description = "",
                            selectedUri = state.pose1Uri,
                            onImageSelected = { viewModel.updatePose1Uri(it) },
                            height = 100.dp
                        )
                    }
                    Button(
                        onClick = { viewModel.togglePose1Save() },
                        enabled = state.pose1Uri != null,
                        colors = ButtonDefaults.buttonColors(
                            containerColor = if (state.isPose1Saved)
                                MaterialTheme.colorScheme.secondary
                            else
                                MaterialTheme.colorScheme.primary
                        )
                    ) {
                        Text(if (state.isPose1Saved) "Saved" else "Save")
                    }
                }
            }

            // Pose 2 with save button
            Column {
                Text(
                    text = "Second Reference Pose",
                    style = MaterialTheme.typography.bodyMedium,
                    modifier = Modifier.padding(bottom = 4.dp)
                )
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                    verticalAlignment = Alignment.Bottom
                ) {
                    Box(modifier = Modifier.weight(1f)) {
                        PoseSelectionField(
                            label = "Pose 2",
                            description = "",
                            selectedUri = state.pose2Uri,
                            onImageSelected = { viewModel.updatePose2Uri(it) },
                            height = 100.dp
                        )
                    }
                    Button(
                        onClick = { viewModel.togglePose2Save() },
                        enabled = state.pose2Uri != null,
                        colors = ButtonDefaults.buttonColors(
                            containerColor = if (state.isPose2Saved)
                                MaterialTheme.colorScheme.secondary
                            else
                                MaterialTheme.colorScheme.primary
                        )
                    ) {
                        Text(if (state.isPose2Saved) "Saved" else "Save")
                    }
                }
            }

            // Pose 3 with save button
            Column {
                Text(
                    text = "Third Reference Pose",
                    style = MaterialTheme.typography.bodyMedium,
                    modifier = Modifier.padding(bottom = 4.dp)
                )
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                    verticalAlignment = Alignment.Bottom
                ) {
                    Box(modifier = Modifier.weight(1f)) {
                        PoseSelectionField(
                            label = "Pose 3",
                            description = "",
                            selectedUri = state.pose3Uri,
                            onImageSelected = { viewModel.updatePose3Uri(it) },
                            height = 100.dp
                        )
                    }
                    Button(
                        onClick = { viewModel.togglePose3Save() },
                        enabled = state.pose3Uri != null,
                        colors = ButtonDefaults.buttonColors(
                            containerColor = if (state.isPose3Saved)
                                MaterialTheme.colorScheme.secondary
                            else
                                MaterialTheme.colorScheme.primary
                        )
                    ) {
                        Text(if (state.isPose3Saved) "Saved" else "Save")
                    }
                }
            }

            Spacer(modifier = Modifier.height(16.dp))
        }
    }
}

@Composable
fun PoseSelectionField(
    label: String,
    description: String,
    selectedUri: Uri?,
    onImageSelected: (Uri?) -> Unit,
    height: Dp
) {
    val context = LocalContext.current
    val imagePicker = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.GetContent()
    ) { uri: Uri? ->
        if (uri != null) {
            try {
                // Create a file in the app's internal storage
                val poseDir = File(context.filesDir, "temp_poses").apply { mkdirs() }
                val tempFile = File(poseDir, "temp_${System.currentTimeMillis()}.jpg")

                // Copy the selected image to our temp file
                context.contentResolver.openInputStream(uri)?.use { input ->
                    FileOutputStream(tempFile).use { output ->
                        input.copyTo(output)
                    }
                }

                // Use the file URI
                onImageSelected(Uri.fromFile(tempFile))
            } catch (e: Exception) {
                Log.e("PoseSelectionField", "Error handling image selection", e)
                e.printStackTrace()
                onImageSelected(uri)
            }
        } else {
            onImageSelected(null)
        }
    }

    Column {
        Text(
            text = description,
            style = MaterialTheme.typography.bodyMedium,
            modifier = Modifier.padding(bottom = 4.dp)
        )

        Box(
            modifier = Modifier
                .fillMaxWidth()
                .height(height)
                .border(1.dp, Color.Gray, RoundedCornerShape(8.dp))
                .clip(RoundedCornerShape(8.dp))
                .clickable { imagePicker.launch("image/*") },
            contentAlignment = Alignment.Center
        ) {
            if (selectedUri != null) {
                AsyncImage(
                    model = ImageRequest.Builder(LocalContext.current)
                        .data(selectedUri)
                        .crossfade(true)
                        .build(),
                    contentDescription = label,
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(4.dp),
                    contentScale = ContentScale.Fit
                )
            } else {
                Column(
                    horizontalAlignment = Alignment.CenterHorizontally,
                    verticalArrangement = Arrangement.Center
                ) {
                    Text(label)
                    Text("+", style = MaterialTheme.typography.titleLarge)
                }
            }
        }
    }
}

private fun confirmExit(): Boolean {
    // This could be replaced with a more sophisticated confirmation dialog
    return true
} 