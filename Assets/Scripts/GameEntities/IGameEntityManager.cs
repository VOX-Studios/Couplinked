using System.Collections.Generic;

public interface IGameEntityManager<T> where T : IGameEntity
{
	List<T> ActiveGameEntities { get; }

	void DeactivateGameEntity(int index);

	void DeactivateGameEntity(T gameEntity);
}
