using UnityEngine;

public class Node : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    public ParticleSystem ParticleSystem;
    public SpriteRenderer BlurSpriteRenderer;
    public int NodeId;
    public int TeamId;
    public float Scale { get; private set; } 

    private MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        Scale = 1;
    }

    public void SetScale(float scale)
    {
        Scale = scale;
        transform.localScale = new Vector3(scale, scale, 1);

        ParticleSystem.MainModule particleMain = ParticleSystem.main;
        particleMain.startSize = new ParticleSystem.MinMaxCurve(particleMain.startSize.constant * scale);
        //particleMain.startSpeed = new ParticleSystem.MinMaxCurve(particleMain.startSpeed.constant * scale);

        ParticleSystem.ShapeModule particleShape = ParticleSystem.shape;
        particleShape.radius *= scale;
    }

    public void SetColors(Color insideColor, Color outsideColor)
    {
        _setSpriteColors(insideColor, outsideColor);
        _setBlurColor(outsideColor);
    }

    public void SetParticleColor(Color color)
    {
        ParticleSystem.MainModule particleSystemMain = ParticleSystem.main;

        particleSystemMain.startColor = color;
    }

    private void _setSpriteColors(Color insideColor, Color outsideColor)
    {
        //get the current value of properties in the renderer
        SpriteRenderer.GetPropertyBlock(_propertyBlock);

        _propertyBlock.SetColor("_InsideColor", insideColor);
        _propertyBlock.SetColor("_OutsideColor", outsideColor);

        //apply values to the renderer
        SpriteRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void _setBlurColor(Color BlurColor)
    {
        //get the current value of properties in the renderer
        BlurSpriteRenderer.GetPropertyBlock(_propertyBlock);

        _propertyBlock.SetColor("_OutsideColor", BlurColor);

        //apply values to the renderer
        BlurSpriteRenderer.SetPropertyBlock(_propertyBlock);
    }
}
