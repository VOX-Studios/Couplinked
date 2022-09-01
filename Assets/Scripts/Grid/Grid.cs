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

	public GridColorManager ColorManager;

	public void Initialize(GameManager gameManager)
	{
		ColorManager = new GridColorManager(_gridMaterial, gameManager.LightingManager.BaseLightTexture);
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

		ColorManager.SetColor(gridColor);
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

		for (int y = 0; y < _ySize; y++)
		{
			for (int x = 0; x < _xSize; x++)
			{
				_lineRenderersVertical[x].SetPosition(y, Logic.Points[x, y].Position);
				_lineRenderersHorizontal[y].SetPosition(x, Logic.Points[x, y].Position);
			}
		}
	}
}