using UnityEngine;

public class GridColorManager
{
	private Material _gridMaterial;
	
	public GridColorManager(Material gridMaterial, RenderTexture baseLightTexture)
    {
		_gridMaterial = gridMaterial;

		_gridMaterial.SetTexture("_Light_Texture", baseLightTexture);

	}

	public void SetColor(Color color)
    {
		_gridMaterial.SetColor("_Color", color);
	}
}