using System;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace GameCore
{
    /// <summary>Native file saves; mini-game persistent storage requires a platform implementation.</summary>
    public class LocalSaveModule : Module, ILocalSaveModule
    {
        private Utility.OptimizedAES aes;
        public string RootPath { get; set; }
        public override void OnInit()
        {
            RootPath = Path.Combine(Application.persistentDataPath, "LocalSave");
            aes = new Utility.OptimizedAES("gameboys84", "LocalSaveModule");
        }
        public override void Shutdown() { aes = null; }
        private string GetPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName)
                || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || fileName.Contains("/") || fileName.Contains("\\") || fileName == "." || fileName == "..")
                throw new ArgumentException("Save name must be a single file name.", nameof(fileName));
            return Path.Combine(RootPath, fileName);
        }
        private string Serialize(ILocalSaveData data, bool encrypt)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            string text = data.Serialize();
            return encrypt ? Utility.Encryption.AES_Encrypt(text, aes) : text;
        }
        private T Deserialize<T>(string text, bool encrypt) where T : ILocalSaveData, new()
        {
            T data = new T();
            data.Deserialize(encrypt ? Utility.Encryption.AES_Decrypt(text.TrimStart('\uFEFF'), aes) : text.TrimStart('\uFEFF'));
            return data;
        }
        public void Save(string fileName, ILocalSaveData data, bool encrypt = false)
        {
            string path = GetPath(fileName);
            string text = Serialize(data, encrypt);
            Directory.CreateDirectory(RootPath);
            File.WriteAllText(path, text, Encoding.UTF8);
        }
        public T Load<T>(string fileName, bool encrypt = false) where T : ILocalSaveData, new()
        {
            string path = GetPath(fileName);
            return File.Exists(path) ? Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), encrypt) : default;
        }
        public async UniTask SaveAsync(string fileName, ILocalSaveData data, bool encrypt = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = GetPath(fileName);
            byte[] bytes = Encoding.UTF8.GetBytes(Serialize(data, encrypt));
            Directory.CreateDirectory(RootPath);
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        }
        public async UniTask<T> LoadAsync<T>(string fileName, bool encrypt = false, CancellationToken cancellationToken = default) where T : ILocalSaveData, new()
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = GetPath(fileName);
            if (!File.Exists(path)) return default;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using (var memory = new MemoryStream())
            {
                await stream.CopyToAsync(memory, 4096, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                return Deserialize<T>(Encoding.UTF8.GetString(memory.ToArray()), encrypt);
            }
        }
    }
}
