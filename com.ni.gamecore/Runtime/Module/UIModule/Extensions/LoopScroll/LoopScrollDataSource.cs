using UnityEngine;
using System.Collections;

namespace GameCore
{
    public abstract class LoopScrollDataSource
    {
        public abstract void ProvideData(Transform transform, int idx);
    }

	public class LoopScrollSendIndexSource : LoopScrollDataSource
    {
		public static readonly LoopScrollSendIndexSource Instance = new LoopScrollSendIndexSource();

		LoopScrollSendIndexSource(){}

        public override void ProvideData(Transform transform, int idx)
        {
            transform.SendMessage("ScrollCellIndex", idx);
        }

        //void MySrollCellIndex(int idx)

        public void ProvideData(Transform transform, int idx,int RankLevel)
        {
            transform.SendMessage("MySrollCellIndexRank", RankLevel);
            transform.SendMessage("MySrollCellIndex", idx);
        }

    }



    public class LoopScrollArraySource<T> : LoopScrollDataSource
    {
        T[] objectsToFill;

		public LoopScrollArraySource(T[] objectsToFill)
        {
            this.objectsToFill = objectsToFill;
        }

        public override void ProvideData(Transform transform, int idx)
        {
            transform.SendMessage("ScrollCellContent", objectsToFill[idx]);
        }
    }
}