using UnityEngine;

public class Grid : MonoBehaviour
{
	private GameManager _gameManager;

	public GridLogic Logic;
	private int _xSize;
	private int _ySize;

	[SerializeField]
	private GameObject _lineRendererPrefab;

	[SerializeField]
	private Material _gridMaterial;

	private LineRenderer[] _lineRenderersVertical;
	private LineRenderer[] _lineRenderersHorizontal;

	public void Initialize(GameManager gameManager)
	{
		_gameManager = gameManager;

		Create();
	}

	public void Create()
    {
        QualitySettingEnum gridDensity = _gameManager.DataManager.GridDensity.Get();

		int gridDensityValue;

		switch (gridDensity)
		{
			default:
			case QualitySettingEnum.Off:
				gridDensityValue = 0;
				break;
			case QualitySettingEnum.Low:
				gridDensityValue = 3;
				break;
			case QualitySettingEnum.Medium:
				gridDensityValue = 4;
				break;
			case QualitySettingEnum.High:
				gridDensityValue = 5;
				break;
		}

		Logic = new GridLogic(GameManager.RightX - GameManager.LeftX, GameManager.TopY - GameManager.BotY, gridDensityValue);

		_xSize = Logic.Points.GetLength(0);
		_ySize = Logic.Points.GetLength(1);

		_clear();

		_lineRenderersVertical = new LineRenderer[_xSize];
		_lineRenderersHorizontal = new LineRenderer[_ySize];

		for (int i = 0; i < _lineRenderersVertical.Length; i++)
		{
			GameObject lineRenderer = Instantiate(_lineRendererPrefab);
			lineRenderer.transform.parent = transform;
			_lineRenderersVertical[i] = lineRenderer.GetComponent<LineRenderer>();
			_lineRenderersVertical[i].positionCount = _ySize;
			_lineRenderersVertical[i].material = _gridMaterial;
		}

		for (int i = 0; i < _lineRenderersHorizontal.Length; i++)
		{
			GameObject lineRenderer = Instantiate(_lineRendererPrefab);
			lineRenderer.transform.parent = transform;
			_lineRenderersHorizontal[i] = lineRenderer.GetComponent<LineRenderer>();
			_lineRenderersHorizontal[i].positionCount = _xSize;
			_lineRenderersHorizontal[i].material = _gridMaterial;
		}

		//pulling first player grid color since we don't support different color grids
		Color gridColor = _gameManager.DataManager.PlayerColors[0].GridColor.Get();
		SetColor(gridColor);
	}

	public void SetColor(Color color)
    {
		_gridMaterial.SetColor("_Color", color);
	}

	public void SetLightPosition(Vector2 worldPosition, Vector2 worldPosition2, Color color, Color color2)
    {
        Vector3 screenPosition = Camera.main.WorldToViewportPoint(worldPosition);
		Vector3 screenPosition2 = Camera.main.WorldToViewportPoint(worldPosition2);
		Texture2D texture2D = new Texture2D(2, 1);
		texture2D.filterMode = FilterMode.Point;
		texture2D.SetPixel(0, 0, new Color(screenPosition.x, screenPosition.y, 0)); //TODO: handle H > W
		texture2D.SetPixel(1, 0, new Color(screenPosition2.x, screenPosition2.y, 0)); //TODO: handle H > W
		texture2D.Apply();

		Texture2D colorTexture2D = new Texture2D(2, 1);
		colorTexture2D.filterMode = FilterMode.Point;
		colorTexture2D.SetPixel(0, 0, color);
		colorTexture2D.SetPixel(1, 0, color2);
		colorTexture2D.Apply();

		_gridMaterial.SetTexture("_Positions", texture2D);
		_gridMaterial.SetTexture("_Colors", colorTexture2D);
	}

	private void _clear()
    {
		if (_lineRenderersVertical != null)
        {
			for (int i = _lineRenderersVertical.Length - 1; i >= 0; i--)
			{
				Destroy(_lineRenderersVertical[i]);
			}
		}

		if (_lineRenderersHorizontal != null)
		{
			for (int i = _lineRenderersHorizontal.Length - 1; i >= 0; i--)
			{
				Destroy(_lineRenderersHorizontal[i]);
			}
		}
	}


	public void Run()
	{
        Logic.Update();

		for (int i = 0, y = 0; y < _ySize; y++)
		{
			for (int x = 0; x < _xSize; x++, i++)
			{
				_lineRenderersVertical[x].SetPosition(y, Logic.Points[x, y].Position);
				_lineRenderersHorizontal[y].SetPosition(x, Logic.Points[x, y].Position);
			}
		}
	}
}