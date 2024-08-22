using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ColourGenerator
{
    ColourSettings settings;
    Texture2D texture;
    const int textureResolution = 50;
    INoiseFilter biomeNoiseFilter;

    public void UpdateSettings(ColourSettings settings)
    {
        this.settings = settings;

        if (texture == null || texture.height != settings.biomeColourSettings.biomes.Length)
            texture = new Texture2D(textureResolution * 2, settings.biomeColourSettings.biomes.Length, TextureFormat.RGBA32, false);

        biomeNoiseFilter = NoiseFilterFactory.CreateNoiseFilter(settings.biomeColourSettings.noise);
    }
    public void UpdateElevation(MinMax elevationMinMax)
    {
        settings.planetMaterial.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }

    public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
    {
        // Calculate the elevation based on the distance from the sphere's center
        float elevation = pointOnUnitSphere.magnitude;

        // Normalize the elevation (for a unit sphere, this should already be between 0 and 1)
        float heightPercent = Mathf.Clamp01((elevation - 1) / 1f);

        // Apply noise to introduce variations to the biome distribution
        heightPercent += (biomeNoiseFilter.Evaluate(pointOnUnitSphere) - settings.biomeColourSettings.noiseOffset) * settings.biomeColourSettings.noiseStrength;

        float biomeIndex = 0;
        int numBiomes = settings.biomeColourSettings.biomes.Length;
        float blendRange = settings.biomeColourSettings.blendAmount / 2f + .001f;

        // Determine which biome the point belongs to, using blending between adjacent biomes
        for (int i = 0; i < numBiomes; i++)
        {
            float dst = heightPercent - settings.biomeColourSettings.biomes[i].startHeight;
            float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);

            biomeIndex *= (1 - weight);
            biomeIndex += i * weight;
        }

        // Normalize the biome index to a 0-1 range for texture mapping
        return biomeIndex / Mathf.Max(1, numBiomes - 1);
    }

    public void UpdateColours()
    {
        Color[] colours = new Color[texture.width * texture.height];
        int colourIndex = 0;

        for (int y = 0; y < settings.biomeColourSettings.biomes.Length; y++)
        {
            for (int x = 0; x < textureResolution * 2; x++)
            {
                Color gradientCol;
                if (x < textureResolution)
                    gradientCol = settings.oceanColour.Evaluate(x / (textureResolution - 1f)); // Ocean gradient
                else
                    gradientCol = settings.biomeColourSettings.biomes[y].gradient.Evaluate((x - textureResolution) / (textureResolution - 1f)); // Biome gradient

                Color tintCol = settings.biomeColourSettings.biomes[y].tint;
                colours[colourIndex] = gradientCol * (1 - settings.biomeColourSettings.biomes[y].tintPercent) + tintCol * settings.biomeColourSettings.biomes[y].tintPercent;
                colourIndex++;
            }
        }

        texture.SetPixels(colours);
        texture.Apply();
        settings.planetMaterial.SetTexture("_texture", texture);
    }
}
