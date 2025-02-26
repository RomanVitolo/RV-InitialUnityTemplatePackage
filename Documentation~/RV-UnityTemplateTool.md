"### RV - Template Tool Documentation\n\n" +
            "**Overview**\n" +
            "This tool allows you to generate a structured folder and file setup for Unity projects. It provides a graphical interface for managing directories, subfolders, and various file types.\n\n" +
            
            "**Features & Fixes Implemented:**\n" +
            "- ✅ **Fixed File Type Selection in Subfolders & Directories**\n" +
            "  - Previously, subfolder and directory files were always created as `.cs`. Now, the correct file type is used when adding files.\n" +
            "- ✅ **Corrected File Type Selection Dropdowns**\n" +
            "  - The dropdown now **saves the selected file type** for each folder.\n" +
            "- ✅ **Fixed Subfolder Files Not Appearing in Preview Tab**\n" +
            "  - Subfolder files now appear correctly in the Preview tab, using the correct path format.\n" +
            "- ✅ **Fixed Incorrect `subKey` Format for Subfolders**\n" +
            "  - The subfolder key now uses `/` instead of `_`, ensuring files are properly retrieved.\n" +
            "- ✅ **Ensured `.mat` Files Are Properly Created in Unity**\n" +
            "  - `.mat` files are now correctly created as Unity assets using `AssetDatabase.CreateAsset()`.\n" +
            "- ✅ **Added Separator Lines for Better UI Structure**\n" +
            "  - The tool now has **horizontal separators** for better readability in the UI.\n\n" +
            
            "**How to Use the Tool:**\n\n" +
            "1. **Set Up the Main Root**\n" +
            "   - Enter a name in the 'Main Root Name' field.\n\n" +
            "2. **Add & Configure Directories**\n" +
            "   - Click the '+' button to add new directories.\n" +
            "   - Select a **default file type** for each directory.\n" +
            "   - Add **files inside each directory** using the 'Files in Directory' list.\n\n" +
            "3. **Manage Subfolders**\n" +
            "   - Each directory can contain **subfolders**.\n" +
            "   - Click the '+' button inside a directory's **Subfolders** section to create new subfolders.\n" +
            "   - Assign a **default file type** to each subfolder.\n" +
            "   - Add **files inside subfolders** using the 'Files in Subfolder' list.\n\n" +
            "4. **Customize File Creation**\n" +
            "   - Select the file type before adding files to directories or subfolders.\n" +
            "   - Supported file types: `.cs`, `.txt`, `.md`, `.unity`, `.json`, `.mat`, `.asmdef`.\n\n" +
            "5. **Save & Load Configurations**\n" +
            "   - Use the 'Save Configuration' button to export your folder structure to a JSON file.\n" +
            "   - Reload previous configurations using 'Load Configuration'.\n\n" +
            "6. **Generate the Template Structure**\n" +
            "   - Click 'Create Template Resources' to generate the entire directory and file structure in Unity.\n" +
            "   - Unity will create `.mat` files correctly using `AssetDatabase.CreateAsset()`.\n\n" +
            "7. **Preview the Folder Structure**\n" +
            "   - Switch to the 'Preview' tab to see the hierarchy of created folders and files.\n" +
            "   - All subfolders and files are now correctly displayed.\n\n" +
            
            "**Developer Info:**\n" +
            "- **Author:** Roman Vitolo\n" +
            "- **Website:** [https://romanvitolo.com](https://romanvitolo.com)\n" +
            "- **GitHub Repository:** [https://github.com/RomanVitolo](https://github.com/RomanVitolo)\n" +
            "- **For Issues & Feedback:** You can contact me via email (available on my website).\n";