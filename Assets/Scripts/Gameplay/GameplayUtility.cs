using UnityEngine;

public class GameplayUtility
{
    private const float _EXPLOSIVE_FORCE = 10f;
    private const float _EXPLOSIVE_RADIUS = 1.25f;

    private GridLogic _gridLogic;

    public GameplayUtility(GridLogic gridLogic)
    {
        _gridLogic = gridLogic;
    }

    public void AddExplosiveForceToGrid(Vector3 position)
    {
        _gridLogic.ApplyExplosiveForce(_EXPLOSIVE_FORCE, position, _EXPLOSIVE_RADIUS);
    }
}
