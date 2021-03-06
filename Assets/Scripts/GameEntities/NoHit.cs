using UnityEngine;

public class NoHit : GameEntity
{
	private ICollisionHandler<NoHit> _noHitCollisionHandler;

	public float Scale { get; private set; }

    // Use this for initialization
    public void Initialize(ICollisionHandler<NoHit> noHitCollisionHandler) 
	{
		_noHitCollisionHandler = noHitCollisionHandler;
		Speed = _GameManager.GameDifficultyManager.ObjectSpeed;
	}

	public void SetScale(float scale)
    {
		Scale = scale;
		transform.localScale = new Vector3(scale, scale, 1);
	}

	public override void Move(float time) 
	{
		transform.position -= new Vector3(Speed * Scale, 0, 0) * time;
		//_GameManager.Grid.Logic.ApplyImplosiveForce(1 * Scale, transform.position, 1 * Scale);
	}

	public void ReleaseLightIndex()
    {
		_GameManager.LightingManager.ReleaseLightIndex(LightIndex);
		LightIndex = -1;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (!gameObject.activeSelf)
			return;

		_noHitCollisionHandler.OnCollision(this, other);
	}
}
