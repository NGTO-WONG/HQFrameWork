using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace QFramework
{
    #region API

    /// <summary>
    /// Object pool.
    /// </summary>
    internal class SafeObjectPool<T> : Pool<T>, ISingleton where T : IPoolable, new()
    {
        #region Singleton

        void ISingleton.OnSingletonInit()
        {
        }

        protected SafeObjectPool()
        {
            mFactory = new DefaultObjectFactory<T>();
        }

        public static SafeObjectPool<T> Instance
        {
            get { return SingletonProperty<SafeObjectPool<T>>.Instance; }
        }

        public void Dispose()
        {
            SingletonProperty<SafeObjectPool<T>>.Dispose();
        }

        #endregion

        /// <summary>
        /// Init the specified maxCount and initCount.
        /// </summary>
        /// <param name="maxCount">Max Cache count.</param>
        /// <param name="initCount">Init Cache count.</param>
        public void Init(int maxCount, int initCount)
        {
            MaxCacheCount = maxCount;

            if (maxCount > 0)
            {
                initCount = Math.Min(maxCount, initCount);
            }

            if (CurCount < initCount)
            {
                for (var i = CurCount; i < initCount; ++i)
                {
                    Recycle(new T());
                }
            }
        }

        /// <summary>
        /// Gets or sets the max cache count.
        /// </summary>
        /// <value>The max cache count.</value>
        public int MaxCacheCount
        {
            get { return mMaxCount; }
            set
            {
                mMaxCount = value;

                if (mCacheStack != null)
                {
                    if (mMaxCount > 0)
                    {
                        if (mMaxCount < mCacheStack.Count)
                        {
                            int removeCount = mCacheStack.Count - mMaxCount;
                            while (removeCount > 0)
                            {
                                mCacheStack.Pop();
                                --removeCount;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allocate T instance.
        /// </summary>
        public override T Allocate()
        {
            var result = base.Allocate();
            result.IsRecycled = false;
            return result;
        }

        /// <summary>
        /// Recycle the T instance
        /// </summary>
        /// <param name="t">T.</param>
        public override bool Recycle(T t)
        {
            if (t == null || t.IsRecycled)
            {
                return false;
            }

            if (mMaxCount > 0)
            {
                if (mCacheStack.Count >= mMaxCount)
                {
                    t.OnRecycled();
                    return false;
                }
            }

            t.IsRecycled = true;
            t.OnRecycled();
            mCacheStack.Push(t);

            return true;
        }
    }

    /// <summary>
    /// Object pool 4 class who no public constructor
    /// such as SingletonClass.QEventSystem
    /// </summary>
    internal class NonPublicObjectPool<T> : Pool<T>, ISingleton where T : class, IPoolable
    {
        #region Singleton

        public void OnSingletonInit()
        {
        }

        public static NonPublicObjectPool<T> Instance
        {
            get { return SingletonProperty<NonPublicObjectPool<T>>.Instance; }
        }

        protected NonPublicObjectPool()
        {
            mFactory = new NonPublicObjectFactory<T>();
        }

        public void Dispose()
        {
            SingletonProperty<NonPublicObjectPool<T>>.Dispose();
        }

        #endregion

        /// <summary>
        /// Init the specified maxCount and initCount.
        /// </summary>
        /// <param name="maxCount">Max Cache count.</param>
        /// <param name="initCount">Init Cache count.</param>
        public void Init(int maxCount, int initCount)
        {
            if (maxCount > 0)
            {
                initCount = Math.Min(maxCount, initCount);
            }

            if (CurCount >= initCount) return;

            for (var i = CurCount; i < initCount; ++i)
            {
                Recycle(mFactory.Create());
            }
        }

        /// <summary>
        /// Gets or sets the max cache count.
        /// </summary>
        /// <value>The max cache count.</value>
        public int MaxCacheCount
        {
            get { return mMaxCount; }
            set
            {
                mMaxCount = value;

                if (mCacheStack == null) return;
                if (mMaxCount <= 0) return;
                if (mMaxCount >= mCacheStack.Count) return;
                var removeCount = mMaxCount - mCacheStack.Count;
                while (removeCount > 0)
                {
                    mCacheStack.Pop();
                    --removeCount;
                }
            }
        }

        /// <summary>
        /// Allocate T instance.
        /// </summary>
        public override T Allocate()
        {
            var result = base.Allocate();
            result.IsRecycled = false;
            return result;
        }

        /// <summary>
        /// Recycle the T instance
        /// </summary>
        /// <param name="t">T.</param>
        public override bool Recycle(T t)
        {
            if (t == null || t.IsRecycled)
            {
                return false;
            }

            if (mMaxCount > 0)
            {
                if (mCacheStack.Count >= mMaxCount)
                {
                    t.OnRecycled();
                    return false;
                }
            }

            t.IsRecycled = true;
            t.OnRecycled();
            mCacheStack.Push(t);

            return true;
        }
    }

    internal abstract class AbstractPool<T> where T : AbstractPool<T>, new()
    {
        private static Stack<T> mPool = new Stack<T>(10);

        protected bool mInPool = false;

        public static T Allocate()
        {
            var node = mPool.Count == 0 ? new T() : mPool.Pop();
            node.mInPool = false;
            return node;
        }

        public void Recycle2Cache()
        {
            OnRecycle();
            mInPool = true;
            mPool.Push(this as T);
        }

        protected abstract void OnRecycle();
    }

    #endregion

    #region interfaces

    /// <summary>
    /// ???????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IPool<T>
    {
        /// <summary>
        /// ????????????
        /// </summary>
        /// <returns></returns>
        T Allocate();

        /// <summary>
        /// ????????????
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Recycle(T obj);
    }

    /// <summary>
    /// I pool able.
    /// </summary>
    internal interface IPoolable
    {
        void OnRecycled();
        bool IsRecycled { get; set; }
    }

    /// <summary>
    /// I cache type.
    /// </summary>
    internal interface IPoolType
    {
        void Recycle2Cache();
    }

    /// <summary>
    /// ?????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class Pool<T> : IPool<T>
    {
        #region ICountObserverable

        /// <summary>
        /// Gets the current count.
        /// </summary>
        /// <value>The current count.</value>
        public int CurCount
        {
            get { return mCacheStack.Count; }
        }

        #endregion

        protected IObjectFactory<T> mFactory;

        /// <summary>
        /// ????????????????????????
        /// </summary>
        protected readonly Stack<T> mCacheStack = new Stack<T>();

        /// <summary>
        /// default is 5
        /// </summary>
        protected int mMaxCount = 12;

        public virtual T Allocate()
        {
            return mCacheStack.Count == 0
                ? mFactory.Create()
                : mCacheStack.Pop();
        }

        public abstract bool Recycle(T obj);
    }

    #endregion

    #region DataStructurePool

    /// <summary>
    /// ??????????????????????????????????????????
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class DictionaryPool<TKey, TValue>
    {
        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        static Stack<Dictionary<TKey, TValue>> mListStack = new Stack<Dictionary<TKey, TValue>>(8);

        /// <summary>
        /// ??????????????????????????????????????????
        /// </summary>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Get()
        {
            if (mListStack.Count == 0)
            {
                return new Dictionary<TKey, TValue>(8);
            }

            return mListStack.Pop();
        }

        /// <summary>
        /// ??????????????????????????????????????? 
        /// </summary>
        /// <param name="toRelease"></param>
        public static void Release(Dictionary<TKey, TValue> toRelease)
        {
            toRelease.Clear();
            mListStack.Push(toRelease);
        }
    }

    /// <summary>
    /// ??????????????? ???????????????
    /// </summary>
    internal static class DictionaryPoolExtensions
    {
        /// <summary>
        /// ??????????????? ???????????? ?????????
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="toRelease"></param>
        public static void Release2Pool<TKey, TValue>(this Dictionary<TKey, TValue> toRelease)
        {
            DictionaryPool<TKey, TValue>.Release(toRelease);
        }
    }

    /// <summary>
    /// ????????????????????????????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class ListPool<T>
    {
        /// <summary>
        /// ????????????????????????List
        /// </summary>
        static Stack<List<T>> mListStack = new Stack<List<T>>(8);

        /// <summary>
        /// ?????????????????????List??????
        /// </summary>
        /// <returns></returns>
        public static List<T> Get()
        {
            if (mListStack.Count == 0)
            {
                return new List<T>(8);
            }

            return mListStack.Pop();
        }

        /// <summary>
        /// ????????????List?????????????????????
        /// </summary>
        /// <param name="toRelease"></param>
        public static void Release(List<T> toRelease)
        {
            toRelease.Clear();
            mListStack.Push(toRelease);
        }
    }

    /// <summary>
    /// ??????????????? ???????????????
    /// </summary>
    internal static class ListPoolExtensions
    {
        /// <summary>
        /// ???List?????? ???????????? ?????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toRelease"></param>
        public static void Release2Pool<T>(this List<T> toRelease)
        {
            ListPool<T>.Release(toRelease);
        }
    }

    #endregion

    #region Factories

    /// <summary>
    /// ????????????
    /// </summary>
    internal class ObjectFactory
    {
        /// <summary>
        /// ??????????????????????????????????????????????????????
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructorArgs"></param>
        /// <returns></returns>
        public static object Create(Type type, params object[] constructorArgs)
        {
            return Activator.CreateInstance(type, constructorArgs);
        }

        /// <summary>
        /// ???????????????????????????????????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="constructorArgs"></param>
        /// <returns></returns>
        public static T Create<T>(params object[] constructorArgs)
        {
            return (T)Create(typeof(T), constructorArgs);
        }

        /// <summary>
        /// ???????????????????????????????????????/?????????????????????
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CreateNonPublicConstructorObject(Type type)
        {
            // ????????????????????????
            var constructorInfos = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            // ????????????????????????
            var ctor = Array.Find(constructorInfos, c => c.GetParameters().Length == 0);

            if (ctor == null)
            {
                throw new Exception("Non-Public Constructor() not found! in " + type);
            }

            return ctor.Invoke(null);
        }

        /// <summary>
        /// ???????????????????????????????????????/?????????????????????  ????????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateNonPublicConstructorObject<T>()
        {
            return (T)CreateNonPublicConstructorObject(typeof(T));
        }

        /// <summary>
        /// ?????????????????????????????? ??????
        /// </summary>
        /// <param name="type"></param>
        /// <param name="onObjectCreate"></param>
        /// <param name="constructorArgs"></param>
        /// <returns></returns>
        public static object CreateWithInitialAction(Type type, Action<object> onObjectCreate,
            params object[] constructorArgs)
        {
            var obj = Create(type, constructorArgs);
            onObjectCreate(obj);
            return obj;
        }

        /// <summary>
        /// ?????????????????????????????? ?????????????????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onObjectCreate"></param>
        /// <param name="constructorArgs"></param>
        /// <returns></returns>
        public static T CreateWithInitialAction<T>(Action<T> onObjectCreate,
            params object[] constructorArgs)
        {
            var obj = Create<T>(constructorArgs);
            onObjectCreate(obj);
            return obj;
        }
    }

    /// <summary>
    /// ??????????????????????????????????????? ???????????? 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CustomObjectFactory<T> : IObjectFactory<T>
    {
        public CustomObjectFactory(Func<T> factoryMethod)
        {
            mFactoryMethod = factoryMethod;
        }

        protected Func<T> mFactoryMethod;

        public T Create()
        {
            return mFactoryMethod();
        }
    }

    /// <summary>
    /// ??????????????????????????????????????????New ?????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DefaultObjectFactory<T> : IObjectFactory<T> where T : new()
    {
        public T Create()
        {
            return new T();
        }
    }

    /// <summary>
    /// ??????????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IObjectFactory<T>
    {
        /// <summary>
        /// ????????????
        /// </summary>
        /// <returns></returns>
        T Create();
    }

    /// <summary>
    /// ??????????????????????????????????????????????????????????????????????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class NonPublicObjectFactory<T> : IObjectFactory<T> where T : class
    {
        public T Create()
        {
            var ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
            var ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);

            if (ctor == null)
            {
                throw new Exception("Non-Public Constructor() not found! in " + typeof(T) + "\n ?????????????????? public ???????????????");
            }

            return ctor.Invoke(null) as T;
        }
    }

    #endregion


    #region SingletonKit For Pool

    /// <summary>
    /// ????????????
    /// </summary>
    internal interface ISingleton
    {
        /// <summary>
        /// ???????????????(????????????????????????????????????????????????)
        /// </summary>
        void OnSingletonInit();
    }

    /// <summary>
    /// ??????????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class Singleton<T> : ISingleton where T : Singleton<T>
    {
        /// <summary>
        /// ????????????
        /// </summary>
        protected static T mInstance;

        /// <summary>
        /// ???????????????????????????????????????????????????????????????????????????????????????????????????
        /// ??????????????????????????????????????????????????????????????????????????????????????????????????????????????????
        /// </summary>
        static object mLock = new object();

        /// <summary>
        /// ????????????
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (mLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = SingletonCreator.CreateSingleton<T>();
                    }
                }

                return mInstance;
            }
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public virtual void Dispose()
        {
            mInstance = null;
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        public virtual void OnSingletonInit()
        {
        }
    }

    /// <summary>
    /// ???????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class SingletonProperty<T> where T : class, ISingleton
    {
        /// <summary>
        /// ????????????
        /// </summary>
        private static T mInstance;

        /// <summary>
        /// ?????????
        /// </summary>
        private static readonly object mLock = new object();

        /// <summary>
        /// ????????????
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (mLock)
                {
                    if (mInstance == null)
                    {
                        mInstance = SingletonCreator.CreateSingleton<T>();
                    }
                }

                return mInstance;
            }
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public static void Dispose()
        {
            mInstance = null;
        }
    }

    /// <summary>
    /// ?????????????????????
    /// </summary>
    internal static class SingletonCreator
    {
        static T CreateNonPublicConstructorObject<T>() where T : class
        {
            var type = typeof(T);
            // ????????????????????????
            var constructorInfos = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            // ????????????????????????
            var ctor = Array.Find(constructorInfos, c => c.GetParameters().Length == 0);

            if (ctor == null)
            {
                throw new Exception("Non-Public Constructor() not found! in " + type);
            }

            return ctor.Invoke(null) as T;
        }

        public static T CreateSingleton<T>() where T : class, ISingleton
        {
            var type = typeof(T);
            var monoBehaviourType = typeof(MonoBehaviour);

            if (monoBehaviourType.IsAssignableFrom(type))
            {
                return CreateMonoSingleton<T>();
            }
            else
            {
                var instance = CreateNonPublicConstructorObject<T>();
                instance.OnSingletonInit();
                return instance;
            }
        }


        /// <summary>
        /// ?????????????????? ??????
        /// </summary>
        public static bool IsUnitTestMode { get; set; }

        /// <summary>
        /// ??????Obj?????????????????????Obj????????????
        /// </summary>
        /// <param name="root">?????????</param>
        /// <param name="subPath">????????????????????????</param>
        /// <param name="index">??????</param>
        /// <param name="build">true</param>
        /// <param name="dontDestroy">???????????? ??????</param>
        /// <returns></returns>
        private static GameObject FindGameObject(GameObject root, string[] subPath, int index, bool build,
            bool dontDestroy)
        {
            GameObject client = null;

            if (root == null)
            {
                client = GameObject.Find(subPath[index]);
            }
            else
            {
                var child = root.transform.Find(subPath[index]);
                if (child != null)
                {
                    client = child.gameObject;
                }
            }

            if (client == null)
            {
                if (build)
                {
                    client = new GameObject(subPath[index]);
                    if (root != null)
                    {
                        client.transform.SetParent(root.transform);
                    }

                    if (dontDestroy && index == 0 && !IsUnitTestMode)
                    {
                        GameObject.DontDestroyOnLoad(client);
                    }
                }
            }

            if (client == null)
            {
                return null;
            }

            return ++index == subPath.Length ? client : FindGameObject(client, subPath, index, build, dontDestroy);
        }

        /// <summary>
        /// ?????????????????????MonoBehaviour??????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateMonoSingleton<T>() where T : class, ISingleton
        {
            T instance = null;
            var type = typeof(T);

            //??????T?????????????????????????????????
            if (!IsUnitTestMode && !Application.isPlaying)
                return instance;

            //?????????????????????????????????T??????
            instance = UnityEngine.Object.FindObjectOfType(type) as T;
            if (instance != null)
            {
                instance.OnSingletonInit();
                return instance;
            }

            //MemberInfo????????????????????????????????????????????????????????????????????????
            MemberInfo info = typeof(T);
            //??????T?????? ?????????????????????????????????????????????????????????????????????T??????
            var attributes = info.GetCustomAttributes(true);
            foreach (var atribute in attributes)
            {
                var defineAttri = atribute as MonoSingletonPath;
                if (defineAttri == null)
                {
                    continue;
                }

                instance = CreateComponentOnGameObject<T>(defineAttri.PathInHierarchy, true);
                break;
            }

            //????????????????????????instance  ????????????????????????Obj ????????????????????? ??????
            if (instance == null)
            {
                var obj = new GameObject(typeof(T).Name);
                if (!IsUnitTestMode)
                    UnityEngine.Object.DontDestroyOnLoad(obj);
                instance = obj.AddComponent(typeof(T)) as T;
            }

            instance.OnSingletonInit();
            return instance;
        }

        /// <summary>
        /// ???GameObject?????????T??????????????????
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">?????????????????????Hierarchy????????????????????????</param>
        /// <param name="dontDestroy">???????????? ??????</param>
        /// <returns></returns>
        private static T CreateComponentOnGameObject<T>(string path, bool dontDestroy) where T : class
        {
            var obj = FindGameObject(path, true, dontDestroy);
            if (obj == null)
            {
                obj = new GameObject("Singleton of " + typeof(T).Name);
                if (dontDestroy && !IsUnitTestMode)
                {
                    UnityEngine.Object.DontDestroyOnLoad(obj);
                }
            }

            return obj.AddComponent(typeof(T)) as T;
        }

        /// <summary>
        /// ??????Obj??????????????? ???????????????
        /// </summary>
        /// <param name="path">??????</param>
        /// <param name="build">true</param>
        /// <param name="dontDestroy">???????????? ??????</param>
        /// <returns></returns>
        private static GameObject FindGameObject(string path, bool build, bool dontDestroy)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var subPath = path.Split('/');
            if (subPath == null || subPath.Length == 0)
            {
                return null;
            }

            return FindGameObject(null, subPath, 0, build, dontDestroy);
        }
    }

    /// <summary>
    /// ????????????MonoBehaviour????????????
    /// ????????????Where????????????T??????????????????MonoSingleton<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class MonoSingleton<T> : MonoBehaviour, ISingleton where T : MonoSingleton<T>
    {
        /// <summary>
        /// ????????????
        /// </summary>
        protected static T mInstance;

        /// <summary>
        /// ???????????????????????????????????????
        /// </summary>
        public static T Instance
        {
            get
            {
                if (mInstance == null && !mOnApplicationQuit)
                {
                    mInstance = SingletonCreator.CreateMonoSingleton<T>();
                }

                return mInstance;
            }
        }

        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        public virtual void OnSingletonInit()
        {
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public virtual void Dispose()
        {
            if (SingletonCreator.IsUnitTestMode)
            {
                var curTrans = transform;
                do
                {
                    var parent = curTrans.parent;
                    DestroyImmediate(curTrans.gameObject);
                    curTrans = parent;
                } while (curTrans != null);

                mInstance = null;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// ?????????????????????????????? ??????
        /// </summary>
        protected static bool mOnApplicationQuit = false;

        /// <summary>
        /// ??????????????????????????????????????????????????????GameObject
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            mOnApplicationQuit = true;
            if (mInstance == null) return;
            Destroy(mInstance.gameObject);
            mInstance = null;
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        protected virtual void OnDestroy()
        {
            mInstance = null;
        }

        /// <summary>
        /// ????????????????????????????????????
        /// </summary>
        public static bool IsApplicationQuit
        {
            get { return mOnApplicationQuit; }
        }
    }

    /// <summary>
    /// MonoSingleton??????
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)] //???????????????????????????Class???
    internal class MonoSingletonPath : Attribute
    {
        private string mPathInHierarchy;

        public MonoSingletonPath(string pathInHierarchy)
        {
            mPathInHierarchy = pathInHierarchy;
        }

        public string PathInHierarchy
        {
            get { return mPathInHierarchy; }
        }
    }

    /// <summary>
    /// ??????Mono??????????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class MonoSingletonProperty<T> where T : MonoBehaviour, ISingleton
    {
        private static T mInstance;

        public static T Instance
        {
            get
            {
                if (null == mInstance)
                {
                    mInstance = SingletonCreator.CreateMonoSingleton<T>();
                }

                return mInstance;
            }
        }

        public static void Dispose()
        {
            if (SingletonCreator.IsUnitTestMode)
            {
                UnityEngine.Object.DestroyImmediate(mInstance.gameObject);
            }
            else
            {
                UnityEngine.Object.Destroy(mInstance.gameObject);
            }

            mInstance = null;
        }
    }

    /// <summary>
    /// ????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class PersistentMonoSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T mInstance;
        protected bool mEnabled;

        /// <summary>
        /// Singleton design pattern
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = FindObjectOfType<T>();
                    if (mInstance == null)
                    {
                        var obj = new GameObject();
                        mInstance = obj.AddComponent<T>();
                    }
                }

                return mInstance;
            }
        }

        /// <summary>
        /// On awake, we check if there's already a copy of the object in the scene. If there's one, we destroy it.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (mInstance == null)
            {
                //If I am the first instance, make me the Singleton
                mInstance = this as T;
                DontDestroyOnLoad(transform.gameObject);
                mEnabled = true;
            }
            else
            {
                //If a Singleton already exists and you find
                //another reference in scene, destroy it!
                if (this != mInstance)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// ????????????????????????????????????????????????????????????????????????????????????????????????
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ReplaceableMonoSingleton<T> : MonoBehaviour where T : Component
    {
        protected static T mInstance;
        public float InitializationTime;

        /// <summary>
        /// Singleton design pattern
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = FindObjectOfType<T>();
                    if (mInstance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.hideFlags = HideFlags.HideAndDontSave;
                        mInstance = obj.AddComponent<T>();
                    }
                }

                return mInstance;
            }
        }

        /// <summary>
        /// On awake, we check if there's already a copy of the object in the scene. If there's one, we destroy it.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            InitializationTime = Time.time;

            DontDestroyOnLoad(this.gameObject);
            // we check for existing objects of the same type
            T[] check = FindObjectsOfType<T>();
            foreach (T searched in check)
            {
                if (searched != this)
                {
                    // if we find another object of the same type (not this), and if it's older than our current object, we destroy it.
                    if (searched.GetComponent<ReplaceableMonoSingleton<T>>().InitializationTime < InitializationTime)
                    {
                        Destroy(searched.gameObject);
                    }
                }
            }

            if (mInstance == null)
            {
                mInstance = this as T;
            }
        }
    }

    #endregion
}