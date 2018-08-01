namespace View.FSM.Actions
{
	public class CMove : CChampionAction
	{
		protected override void OnEnter( object[] param )
		{
			this.owner.graphic.animator.SetBool( "run", true );
		}

		protected override void OnExit()
		{
			this.owner.graphic.animator.SetBool( "run", false );
		}
	}
}