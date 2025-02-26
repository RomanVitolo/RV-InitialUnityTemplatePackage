### **UPM Branch README (Unity Package Manager)**

```markdown
# RV Initial Unity Template Package (UPM)

This branch contains the **Unity Package Manager (UPM)** version of the **RV Initial Unity Template Package**, facilitating easy installation and updates through UPM without altering the existing project structure.

## 📥 Installation

### 🔹 Method 1: Adding via Git URL

1. **Open Unity**: Launch your Unity project.
2. **Access Package Manager**: Navigate to `Window > Package Manager`.
3. **Add Package**: Click on the **+** button and select **"Add package from git URL..."**.
4. **Enter URL**: Input the following Git URL: https://github.com/RomanVitolo/RV-InitialUnityTemplatePackage.git#upm

5. **Add Package**: Click **Add** to install the package.

### 🔹 Method 2: Modifying `manifest.json`

1. **Locate Manifest File**: Open your Unity project's `Packages/manifest.json` file.
2. **Add Dependency**: Insert the following line into the dependencies section:
```json
{
  "dependencies": {
    "com.romanvitolo.initialtemplatepackage": "https://github.com/RomanVitolo/RV-InitialUnityTemplatePackage.git#upm"
  }
}
