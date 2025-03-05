package com.example.a1

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Button
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            var currentScreen by remember { mutableStateOf("main") }
            
            when (currentScreen) {
                "main" -> MainScreen(
                    onSettingsClick = { currentScreen = "settings" }
                )
                "settings" -> SettingsScreen(
                    onNavigateBack = { currentScreen = "main" }
                )
            }
        }
    }
}

@Composable
fun MainScreen(onSettingsClick: () -> Unit) {
    Scaffold { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Button(
                onClick = onSettingsClick,
                modifier = Modifier
                    .padding(8.dp)
                    .width(200.dp)
            ) {
                Text("Settings")
            }
            
            Button(
                onClick = { /* TODO: Handle Start click */ },
                modifier = Modifier
                    .padding(8.dp)
                    .width(200.dp)
            ) {
                Text("Start")
            }
        }
    }
}