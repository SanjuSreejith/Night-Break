using UnityEngine;

public class TextureQualityController : MonoBehaviour
{
    // Adjust textures' quality settings at runtime
    void Start()
    {
        // Find all textures in the Resources folder or assigned objects
        Texture[] allTextures = Resources.FindObjectsOfTypeAll<Texture>();

        foreach (Texture texture in allTextures)
        {
            if (texture is Texture2D tex2D)
            {
                Texture2D improvedTexture = ImproveTextureQuality(tex2D);

                // Optionally log texture updates
                Debug.Log($"Improved texture: {tex2D.name}");
            }
        }
    }

    // Function to improve texture quality
    Texture2D ImproveTextureQuality(Texture2D texture)
    {
        // Set high-quality settings for the texture
        Texture2D newTexture = texture;

        // Prevent compression for the texture
        newTexture.Compress(false);

        // Increase anisotropic filtering for clearer textures at oblique angles
        newTexture.anisoLevel = 16;

        // Set filter mode to Trilinear for smoother texture scaling
        newTexture.filterMode = FilterMode.Trilinear;

        return newTexture;
    }
}
