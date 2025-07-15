# KeyboardValidator

A standalone console application for validating OptiKey dynamic keyboard XML files.

## Building

### Requirements
- .NET Framework 4.8 or later
- MSBuild (comes with Visual Studio)

### Build Steps
1. Run `build.bat` to build the project
2. The executable will be created in `bin\x64\Release\`

## Usage

```
KeyboardValidator.exe <keyboard_file_path>
```

### Examples
```
KeyboardValidator.exe MyKeyboard.xml
KeyboardValidator.exe "C:\Path\To\Keyboard.xml"
```

### Exit Codes
- `0`: Validation successful
- `1`: Validation failed or error occurred

### Output
- Success messages are written to stdout
- Error messages are written to stderr
- Validation results include checkmarks (✓) for passed validations and crosses (✗) for failures

## Validation Checks

The validator performs the same validation checks as OptiKey:

1. **XML Structure Validation**
   - Valid XML deserialization
   - Grid definition exists and has valid dimensions
   - Window properties are valid (WindowState, Position, DockSize)
   - Numeric properties are within valid ranges (-9999 to 9999)

2. **Legacy Keys Validation** (for keyboards using `<Keys>` element)
   - No duplicate key positions
   - All keys fit within grid boundaries
   - Key dimensions don't exceed grid bounds

3. **Dynamic Content Validation** (for keyboards using `<Content>` element)
   - Items don't overlap
   - Items fit within grid boundaries
   - Proper positioning logic

## Dependencies

The validator uses the OptiKey Core DLL and its dependencies. When distributing, ensure these files are included:
- JuliusSweetland.OptiKey.Core.dll
- log4net.dll
- Any other required OptiKey dependencies

## Error Examples

```
✗ Grid size is 0 rows and 5 columns
✗ Duplicate row/column values for keys: A, B (1, 2)
✗ Key "Hello" at (5, 3) is outside grid bounds (0-4, 0-9)
✗ Item 2 of 5 with label 'MyButton' at row 2 column 3 overlaps with another item
```