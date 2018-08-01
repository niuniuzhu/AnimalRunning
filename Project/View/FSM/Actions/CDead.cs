namespace View.FSM.Actions
{
	public class CDead : CChampionAction
	{
		protected override void OnEnter( object[] param )
		{
			this.owner.graphic.animator.SetBool( "dead", true );
		}

		protected override void OnExit()
		{
			this.owner.graphic.animator.SetBool( "dead", false );
		}
	}
}