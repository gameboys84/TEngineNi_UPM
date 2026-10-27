namespace GameCore
{
    public interface ILocalSaveModule
    {
        void Save(string fileName, ILocalSaveData data, bool encrypt = false);
        T Load<T>(string fileName, bool encrypt = false) where T : ILocalSaveData, new();
        Cysharp.Threading.Tasks.UniTask SaveAsync(string fileName, ILocalSaveData data, bool encrypt = false, System.Threading.CancellationToken cancellationToken = default);
        Cysharp.Threading.Tasks.UniTask<T> LoadAsync<T>(string fileName, bool encrypt = false, System.Threading.CancellationToken cancellationToken = default) where T : ILocalSaveData, new();
    }
}
