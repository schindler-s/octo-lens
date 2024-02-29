# HoloLens 2 Codebase Setup Guide

This guide will help you set up a HoloLens 2 codebase.

## Prerequisites

- Windows 10
- Visual Studio 2019 or later
- Unity 2019.4 or later
- MRTK (Mixed Reality Toolkit) v2.5.1 or later

## Steps

1. **Install the Prerequisites**: Make sure you have all the prerequisites installed on your system.

2. **Create a New Unity Project**: Open Unity Hub and create a new 3D project.

3. **Import MRTK**: Download and import the MRTK packages into your Unity project.

4. **Configure the Project for HoloLens 2**: In Unity, go to `Edit > Project Settings > Player > XR Settings` and enable `Virtual Reality Supported`. Add `Windows Mixed Reality` to the Virtual Reality SDKs list.

5. **Build the Project**: Go to `File > Build Settings`, add your scenes, and click `Build`. Choose a directory for the build.

6. **Open in Visual Studio**: Navigate to the build directory and open the solution in Visual Studio.

7. **Deploy to HoloLens 2**: In Visual Studio, set the target device and deploy the application to your HoloLens 2 device.

## Conclusion

You should now have a basic HoloLens 2 codebase set up. For more detailed instructions, refer to the [official Microsoft documentation](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/unity/tutorials/mr-learning-base-02).


