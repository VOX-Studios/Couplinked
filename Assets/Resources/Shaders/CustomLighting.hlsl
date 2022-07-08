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

void FUCK_float(UnityTexture2D Image, UnitySamplerState SS, float x, out UnityTexture2D Out)
{
    float2 uv = float2(x, 0);

    //Color = Image.Sample(SS, uv);
    Out = Image;
}

void Shit_float(UnityTexture2D positions, float2 positionScale, UnityTexture2D colors, float2 uv, float lightShrinkFactor, float lightFocusFactor, out float4 Out)
{
    float4 result = 0;
    float2 pos = 0;
    for (int i = 0; i < 2; i++)
    {
        float2 lookup = float2(i * .5, 0);
        float4 pos = tex2D(positions, lookup);
        float4 color = tex2D(colors, lookup);
        
        float lightStrength = distance(uv, pos.xy * positionScale) * lightShrinkFactor;
        lightStrength = pow(lightStrength, lightFocusFactor);
        lightStrength = clamp(lightStrength, 0, 1);
        lightStrength = 1 - lightStrength;

        color *= lightStrength;
        color = clamp(color, 0, 1);

        result += color;
    }

    result = clamp(result, 0, 1);
    Out = result;
}


#endif