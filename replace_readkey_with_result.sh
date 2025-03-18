#!/bin/bash

# Make specific replacements for Console.ReadKey calls that assign to variables

# Replace the key in Run method
sed -i '255s/var key = Console.ReadKey(true);/var key = SafeReadKeyWithResult();/' AIYogaTrainerWin/Program.cs

# Replace the modeKey in InteractiveTrainingMode
sed -i '451s/var modeKey = Console.ReadKey(true);/var modeKey = SafeReadKeyWithResult();/' AIYogaTrainerWin/Program.cs

# Replace the key in InteractiveTrainingMode menu
sed -i '492s/var key = Console.ReadKey(true);/var key = SafeReadKeyWithResult();/' AIYogaTrainerWin/Program.cs

# Replace the key in QuickPoseTestingMode
sed -i '681s/var key = Console.ReadKey(true);/var key = SafeReadKeyWithResult();/' AIYogaTrainerWin/Program.cs

echo "Replacements complete"
