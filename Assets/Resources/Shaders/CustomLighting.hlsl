#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

void ReadGradient_float(Gradient Gradient, float Index, out float4 Color)
{
    float3 color = 0;
    if (Index < Gradient.colorsLength)
    {
        color = Gradient.colors[Index].rgb;

#ifdef UNITY_COLORSPACE_GAMMA
        color = LinearToSRGB(color);
#endif
    }

    float alpha = 0;
    if (Index < Gradient.alphasLength)
    {
        alpha = Gradient.alphas[Index].x;
    }

    Color = float4(color, alpha);
}

void Shit_half (UnityTexture2D positions, float2 positionScale, UnityTexture2D colors, float numDataPoints, float2 uv, float lightRange, float lightBrightness, float lightAperture, float lightFocusFactor, float lightPunchOut, out float4 Out)
{
    float maxBrightness = 10;
    float4 result = 0;
    float2 pos = 0;
    float blackout = 1;

    for (int i = 0; i < numDataPoints; i++)
    {
        float2 lookup = float2((i + .5) / numDataPoints, 0);
        float4 pos = tex2D(positions, lookup);
        float4 color = tex2D(colors, lookup);

        if (color.a == 0)
        {
            continue;
        }
        
        float lightStrength = distance(uv, pos.xy * positionScale);
        float lightMask = step(lightStrength, lightRange);

        blackout = min(blackout, 1 - step(lightStrength, lightPunchOut));

        float actualBrightness = maxBrightness - lightBrightness;
        lightStrength *= actualBrightness;
        lightStrength = pow(lightStrength, lightAperture);
        lightStrength = clamp(lightStrength, 0, 1);
        lightStrength = 1 - lightStrength;

        lightStrength = min(lightStrength, lightFocusFactor);

        color *= lightStrength;
        color = clamp(color, 0, 1);

        color *= lightMask;

        result += color;
    }

    result *= blackout;
    result = clamp(result, 0, 1);
    Out = result;
}
#endif