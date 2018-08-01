namespace View
{
	public class CRail : CCollider
	{
		internal override void OnRemoveFromBattle()
		{
			//base.OnRemoveFromBattle();

			//this.MarkToDestroy();
			this.graphic.animator.SetBool( "fall", true );
		}
	}
}