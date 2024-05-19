To use the post processing you have to tell the HDRP it exist. Please follow these instructions:
1. Go to upper panel and click Edit->ProjectSettings->HDRP Default Settings
2. Scroll nearly to the bottom of the page until you see Custom Post Process Orders
3. Here click on the + in the Before Post Process box and select outline (https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@7.1/manual/Custom-Post-Process.html)
4. Also click on the + in the After Post Process box and select posterization
5. Now just use it as any other post processing (You can find it by Add override->Post Processing->Meister->Posterization/Outline)