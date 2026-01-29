# Rail HMI Door Simulator – Build Guide (Windows)
 
## Prerequisites
- Windows 10 / 11 (x64)
- Visual Studio 2022
  - Desktop development with C++
  - .NET desktop development
- Git for Windows
- (Optional) PEAK PCAN driver for real CAN hardware
 
## Repository Layout
- apps/     → WPF host applications
- native/   → Native C++ core libraries
- external/ → Third-party dependencies
- build/vs/ → Visual Studio solution and props
- tools/    → Build / deploy scripts
- docs/     → Documentation
 
## Build
1. Open `build/vs/RailPoc.sln`
2. Select x64 + Debug/Release
3. Build Solution
 
## Notes
- Do not commit build outputs
- All paths are relative