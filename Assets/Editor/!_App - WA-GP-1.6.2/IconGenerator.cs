using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class IconGenerator
{
    private static readonly string androidResPath = "Assets/Plugins/Android/OneSignalConfig.androidlib/src/main/res";

    // [MenuItem("Assets/Generate Resized Icons in Same Folder")]
    private static void GenerateResizedImagesInSameFolder()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is Texture2D selectedTexture)
            {
                string originalPath = AssetDatabase.GetAssetPath(selectedTexture);
                string directory = Path.GetDirectoryName(originalPath);

                Texture2D originalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(originalPath);
                if (originalTexture == null)
                {
                    Debug.LogError("Failed to load texture: " + originalPath);
                    continue;
                }

                string resFolderPath = Path.Combine(directory, "res");
                if (!Directory.Exists(resFolderPath))
                {
                    Directory.CreateDirectory(resFolderPath);
                }

                Texture2D resizedTextureMain = ResizeTexture(originalTexture, 512, 512);
                if (resizedTextureMain == null)
                {
                    Debug.LogError("ResizeTexture failed for size: " + 512);
                    continue;
                }

                string newFileName = $"play_store_512.png";
                string newFilePath = Path.Combine(directory, newFileName);

                byte[] pngData = resizedTextureMain.EncodeToPNG();
                File.WriteAllBytes(newFilePath, pngData);
                AssetDatabase.ImportAsset(newFilePath);

                string[] mipmapFolders = {
                    "mipmap-hdpi",
                    "mipmap-mdpi",
                    "mipmap-xhdpi",
                    "mipmap-xxhdpi",
                    "mipmap-xxxhdpi"
                };

                foreach (string folder in mipmapFolders)
                {
                    string folderPath = Path.Combine(resFolderPath, folder);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                }

                var mipmapSettings = new Dictionary<string, (int totalSize, int padding)>
                {
                    { "mipmap-hdpi", (162, 27) },    // hdpi
                    { "mipmap-mdpi", (108, 18) },    // mdpi
                    { "mipmap-xhdpi", (216, 36) },   // xhdpi
                    { "mipmap-xxhdpi", (324, 54) },  // xxhdpi
                    { "mipmap-xxxhdpi", (432, 72) }  // xxxhdpi
                };

                var icLauncherSettings = new Dictionary<string, (int totalSize, int padding)>
                {
                    { "mipmap-hdpi", (72, 7) },     // hdpi
                    { "mipmap-mdpi", (48, 5) },     // mdpi
                    { "mipmap-xhdpi", (96, 10) },   // xhdpi
                    { "mipmap-xxhdpi", (144, 15) }, // xxhdpi
                    { "mipmap-xxxhdpi", (192, 20) } // xxxhdpi
                };

                foreach (var kvp in mipmapSettings)
                {
                    string folderName = kvp.Key;
                    int totalSize = kvp.Value.totalSize;
                    int padding = kvp.Value.padding;

                    Texture2D backgroundTexture = CreateSolidColorTexture(totalSize, totalSize, new Color(0.4157f, 0.2392f, 0.9098f));
                    string backgroundFileName = "ic_launcher_background.png";
                    string backgroundFilePath = Path.Combine(resFolderPath, folderName, backgroundFileName);
                    File.WriteAllBytes(backgroundFilePath, backgroundTexture.EncodeToPNG());
                    AssetDatabase.ImportAsset(backgroundFilePath);

                    int imageSize = totalSize - 2 * padding;
                    Texture2D resizedTexture = ResizeTexture(originalTexture, imageSize, imageSize);
                    Texture2D foregroundTexture = AddTransparentPadding(resizedTexture, totalSize, padding);
                    string foregroundFileName = "ic_launcher_foreground.png";
                    string foregroundFilePath = Path.Combine(resFolderPath, folderName, foregroundFileName);
                    File.WriteAllBytes(foregroundFilePath, foregroundTexture.EncodeToPNG());
                    AssetDatabase.ImportAsset(foregroundFilePath);
                }

                foreach (var kvp in icLauncherSettings)
                {
                    string folderName = kvp.Key;
                    int totalSize = kvp.Value.totalSize;
                    int padding = kvp.Value.padding;

                    int imageSize = totalSize - 2 * padding;
                    Texture2D resizedTexture = ResizeTexture(originalTexture, imageSize, imageSize);

                    Texture2D launcherTexture = AddTransparentPadding(resizedTexture, totalSize, padding);

                    string launcherFileName = "ic_launcher.png";
                    string launcherFilePath = Path.Combine(resFolderPath, folderName, launcherFileName);
                    File.WriteAllBytes(launcherFilePath, launcherTexture.EncodeToPNG());
                    AssetDatabase.ImportAsset(launcherFilePath);
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Resized icons generated successfully in 'res' folder!");
    }

    private static Texture2D AddTransparentPadding(Texture2D image, int totalSize, int padding)
    {
        Texture2D result = new Texture2D(totalSize, totalSize, TextureFormat.ARGB32, false);
        Color transparent = new Color(0, 0, 0, 0);

        for (int y = 0; y < totalSize; y++)
        {
            for (int x = 0; x < totalSize; x++)
            {
                result.SetPixel(x, y, transparent);
            }
        }

        for (int y = 0; y < image.height; y++)
        {
            for (int x = 0; x < image.width; x++)
            {
                result.SetPixel(x + padding, y + padding, image.GetPixel(x, y));
            }
        }

        result.Apply();
        return result;
    }

    private static Texture2D CreateSolidColorTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Color[] pixels = Enumerable.Repeat(color, width * height).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private static Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        newTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        newTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return newTexture;
    }

    [MenuItem("Assets/Generate Icons")]
    private static void GenerateIcons()
    {
        AdaptiveIconSetter.DeleteAllIcons();
        AssetDatabase.Refresh();
        string directoryPass = "";
        string path = "";
        foreach (Object obj in Selection.objects)
        {
            if (obj is Texture2D texture)
            {
                path = AssetDatabase.GetAssetPath(texture);
                string directory = Path.GetDirectoryName(path);
                directoryPass = directory;
            }
        }
        GenerateResizedImagesInSameFolder();
        AdaptiveIconSetter.SetAllIcons(directoryPass);
        AssetDatabase.Refresh();
    }
   
    [MenuItem("Assets/Generate Icons For OneSignal")]
    private static void GenerateAndroidIcons()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is Texture2D texture)
            {
                string path = AssetDatabase.GetAssetPath(texture);
                Texture2D originalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (originalTexture == null)
                {
                    Debug.LogError("Failed to load texture: " + path);
                    continue;
                }

                string[] subdirectories = Directory.GetDirectories(androidResPath);
                foreach (string subdir in subdirectories)
                {
                    string[] files = Directory.GetFiles(subdir, "*.png");
                    foreach (string file in files)
                    {
                        // Загружаем изображение из файла
                        Texture2D fileTexture = new Texture2D(2, 2);
                        byte[] fileData = File.ReadAllBytes(file);
                        fileTexture.LoadImage(fileData);

                        // Получаем разрешение изображения
                        int width = fileTexture.width;
                        int height = fileTexture.height;

                        // Изменяем размер оригинальной текстуры под разрешение изображения
                        Texture2D resizedTexture = ResizeTexture(originalTexture, width, height);
                        byte[] pngData = resizedTexture.EncodeToPNG();
                        string newFilePath = Path.Combine(subdir, Path.GetFileName(file));
                        File.WriteAllBytes(newFilePath, pngData);
                        AssetDatabase.ImportAsset(newFilePath);
                    }
                }

                AssetDatabase.Refresh();
            }
        }
    }

    [MenuItem("Assets/Generate Icons", true)]
    private static bool ValidateGenerateIcons()
    {
        return Selection.activeObject is Texture2D;
    }
}