using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GreyscaleEffect : MonoBehaviour
{
    public Material greyscaleMaterial;
    [Range(0, 1)]
    public float grayscaleIntensity = 0f;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (greyscaleMaterial != null)
        {
            greyscaleMaterial.SetFloat("_GrayscaleIntensity", grayscaleIntensity);
            Graphics.Blit(src, dest, greyscaleMaterial);
        }
        else
        {
            Graphics.Blit(src, dest); // fallback to normal
        }
    }
}
