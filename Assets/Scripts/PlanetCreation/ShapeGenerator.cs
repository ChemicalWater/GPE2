using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator
{

    ShapeSettings settings;
    INoiseFilter[] noiseFilter;
    public MinMax elevationMinMax;

    public void UpdateSettings(ShapeSettings settings)
    {
        this.settings = settings;
        noiseFilter = new INoiseFilter[settings.noiseLayers.Length];

        for (int i = 0; i < noiseFilter.Length; i++)
        {
            noiseFilter[i] = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayers[i].noiseSettings);
        }
        elevationMinMax = new MinMax();
    }

    public float CalculateUnscaledElevation(Vector3 pointOnUnitSphere)
    {
        float firstLayerValue = 0;
        float elevation = 0;
        if(noiseFilter.Length > 0)
        {
            firstLayerValue = noiseFilter[0].Evaluate(pointOnUnitSphere);
            if (settings.noiseLayers[0].enabled)
                elevation = firstLayerValue;
        }
        for (int i = 1; i < noiseFilter.Length; i++)
        {
            if (settings.noiseLayers[i].enabled)
            {
                float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;

                elevation += noiseFilter[i].Evaluate(pointOnUnitSphere) * mask;
            }
        }

        elevationMinMax.AddValue(elevation);

        return elevation;
    } 

    public float GetScaledElevation(float unscaledElevation)
    {
        float elevation = Mathf.Max(0, unscaledElevation);
        elevation = settings.planetRadius * (1 + elevation);
        return elevation;
    }
}
