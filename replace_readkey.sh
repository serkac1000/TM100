#!/bin/bash

# Locations where Console.ReadKey needs to be replaced with SafeReadKeyWithResult()
locations=(
    "141:                        var key = Console.ReadKey(true);"
    "701:            var key = Console.ReadKey(true);"
    "981:                    var key = Console.ReadKey(true);"
    "1108:                    var key = Console.ReadKey(true);"
    "1156:                var key = Console.ReadKey(true);"
    "1232:                var key = Console.ReadKey(true);"
)

# Replace each location with SafeReadKeyWithResult()
for location in "${locations[@]}"; do
    line_number=$(echo $location | cut -d':' -f1)
    indent=$(echo $location | sed -r 's/[0-9]+:([ ]*).*/\1/')
    sed -i "${line_number}s/.*/${indent}var key = SafeReadKeyWithResult();/" AIYogaTrainerWin/Program.cs
done

# Replace plain Console.ReadKey(true) with SafeReadKey()
sed -i '200s/Console.ReadKey(true);/SafeReadKey();/' AIYogaTrainerWin/Program.cs

# Skip line 218 which is the original SafeReadKey implementation

echo "All ReadKey replacements complete"
