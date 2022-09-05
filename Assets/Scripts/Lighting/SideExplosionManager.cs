using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Lighting
{
    public class SideExplosionManager : MonoBehaviour
    {
		[SerializeField]
		private SideExplosion _sideExplosionPrefab;

		private List<SideExplosion> _activeExplosions;
		private List<SideExplosion> _inactiveExplosions;
		private float _scale = 1;

		public void Initialize(int poolSize)
		{
			_activeExplosions = new List<SideExplosion>();
			_inactiveExplosions = new List<SideExplosion>();

			for (int i = 0; i < poolSize; i++)
			{
				SideExplosion explosion = Instantiate(_sideExplosionPrefab);
				explosion.transform.SetParent(transform, false);
				//explosion.Initialize();
				explosion.gameObject.SetActive(false);
				_inactiveExplosions.Add(explosion);
			}
		}

		/// <summary>
		/// This should only be called once after initialize.
		/// </summary>
		/// <param name="scale"></param>
		public void SetScale(float scale)
		{
			_scale = scale;
		}

		public void Run(float deltaTime)
		{
			int activeCount = _activeExplosions.Count;
			for (int i = activeCount - 1; i >= 0; i--)
			{
				SideExplosion explosion = _activeExplosions[i];
				explosion.Run(deltaTime);

				//if (!explosion.GetComponent<ParticleSystem>().IsAlive())
				//{
				//	DeactivateExplosion(i);
				//}
			}
		}

		public void ActivateExplosion(float yPosition, Color color)
		{
			int inactiveCount = _inactiveExplosions.Count;
			if (inactiveCount > 0)
			{
				SideExplosion explosion = _inactiveExplosions[inactiveCount - 1];
				explosion.gameObject.SetActive(true);
				explosion.GetComponent<SideExplosion>().Explode(new Vector2(GameManager.LeftX, yPosition), color);
				_inactiveExplosions.RemoveAt(inactiveCount - 1);
				_activeExplosions.Add(explosion);
			}
		}

		public void DeactiveExplosions()
		{
			for (int i = _activeExplosions.Count - 1; i >= 0; i--)
			{
				DeactivateExplosion(i);
			}
		}

		public void DeactivateExplosion(int index)
		{
			SideExplosion explosion = _activeExplosions[index];
			explosion.gameObject.SetActive(false);
			_activeExplosions.RemoveAt(index);
			_inactiveExplosions.Add(explosion);
		}
	}
}
