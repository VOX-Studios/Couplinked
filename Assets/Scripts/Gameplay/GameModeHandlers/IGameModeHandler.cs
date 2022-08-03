interface IGameModeHandler : ICollisionHandler<Hit>, ICollisionHandler<HitSplit>, ICollisionHandler<NoHit>
{
    void Initialize();

    void Run(bool isPaused, float deltaTime);

    void OnGameEntityOffScreen(IGameEntity gameEntity);
}
