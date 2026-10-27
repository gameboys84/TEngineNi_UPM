// #if USE_XLUA
// using XLua;
// /// <summary>
// /// 扩展Unity Object, 解决Xlua 端判空问题
// /// </summary>
// // [LuaCallCSharp]
// // [ReflectionUse]
// #endif
namespace GameCore
{
    public static class UnityEngineObjectExtention
    {
        public static bool IsDestroyed(this UnityEngine.Object o)
        {
            return o == null;
        }
    }
}
