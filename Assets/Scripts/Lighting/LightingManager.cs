using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Lighting
{
    public class LightingManager : MonoBehaviour
    {
        public RenderTexture BaseLightTexture;
        public Renderer BaseLightRenderer;
        private Material _baseLightMaterial;

        private Texture2D _positionsTexture;
        private Texture2D _colorsTexture;

        private Queue<int> _availableDataPoints;
        private HashSet<int> _unavailableDataPoints;

        private readonly Color _emptyColor = new Color(0, 0, 0, 0);

        private int _numDataPoints;
        private int _dataTextureWidth;
        private readonly int _texelSize = 1;

        private float _lightPunchOut;

        public void Initialize()
        {
            _baseLightMaterial = BaseLightRenderer.material;
            BaseLightTexture.width = Screen.width;
            BaseLightTexture.height = Screen.height;

            //60 max objects + 8 max player nodes (rounded up)
            _numDataPoints = 70;

            _dataTextureWidth = _numDataPoints * _texelSize;

            _positionsTexture = _makeDataTexture(_dataTextureWidth);
            _colorsTexture = _makeDataTexture(_dataTextureWidth);

            _availableDataPoints = new Queue<int>();
            _unavailableDataPoints = new HashSet<int>();

            //track all indexes
            for (int i = 0; i < _numDataPoints; i++)
            {
                _availableDataPoints.Enqueue(i);
            }

            _baseLightMaterial.SetTexture("_Positions", _positionsTexture);
            _baseLightMaterial.SetTexture("_Colors", _colorsTexture);

            _baseLightMaterial.SetFloat("_Number_Of_Data_Points", _numDataPoints);

            _lightPunchOut = _baseLightMaterial.GetFloat("_Light_Punch_Out");
        }

        public void SetScale(float scale)
        {
            _baseLightMaterial.SetFloat("_Light_Punch_Out", _lightPunchOut * scale);
        }

        private Texture2D _makeDataTexture(int textureWidth)
        {
            Texture2D texture = new Texture2D(textureWidth, 1, TextureFormat.RGBAHalf, false);
            texture.filterMode = FilterMode.Point;

            Color[] colorArray = texture.GetPixels();

            for (int i = 0; i < colorArray.Length; ++i)
            {
                colorArray[i] = _emptyColor;
            }

            texture.SetPixels(colorArray);
            texture.Apply();

            return texture;
        }

        public void SetColor(Color color)
        {
            _baseLightMaterial.SetColor("_Color", color);
        }

        public int GetLightIndex()
        {
            int index = _availableDataPoints.Dequeue();
            _unavailableDataPoints.Add(index);

            return index;
        }

        public void ReleaseLightIndex(int lightIndex)
        {
            if (!_unavailableDataPoints.Contains(lightIndex))
            {
                Debug.LogWarning($"Trying to release a light index that is not in the unavailable list. (Light Index: {lightIndex})");
                return;
            }

            SetLightColor(lightIndex, _emptyColor);
            _unavailableDataPoints.Remove(lightIndex);
            _availableDataPoints.Enqueue(lightIndex);
        }

        public void SetLightPosition(int lightIndex, Vector2 worldPosition)
        {
            if (lightIndex < 0 || lightIndex >= _numDataPoints)
            {
                Debug.LogWarning($"Trying to set light data with an invalid index. (Light Index: {lightIndex})");
                return;
            }

            Vector2 screenPosition = Camera.main.WorldToViewportPoint(worldPosition);
            _applyToTexture(_positionsTexture, lightIndex, new Color(screenPosition.x, screenPosition.y, 0));
        }

        public void SetLightColor(int lightIndex, Color color)
        {
            if (lightIndex < 0 || lightIndex >= _numDataPoints)
            {
                Debug.LogWarning($"Trying to set light data with an invalid index. (Light Index: {lightIndex})");
                return;
            }

            _applyToTexture(_colorsTexture, lightIndex, color);
        }

        private void _applyToTexture(Texture2D texture, int index, Color color)
        {
            //get pixels
            Color[] colors = texture.GetPixels(
                x: index * _texelSize,
                y: 0,
                blockWidth: _texelSize,
                blockHeight: 1
                );

            //fill the array with the new color
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            //set pixels
            texture.SetPixels(
                x: index * _texelSize,
                y: 0,
                blockWidth: _texelSize,
                blockHeight: 1,
                colors: colors
                );

            //apply changes to the texture
            texture.Apply();
        }
    }
}
