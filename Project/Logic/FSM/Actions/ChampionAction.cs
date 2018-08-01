namespace Logic.FSM.Actions
{
	public abstract class ChampionAction : AbsAction
	{
		protected Champion owner { get; private set; }

		public override FSMState state
		{
			set
			{
				base.state = value;
				this.owner = ( Champion )value.owner;
			}
		}
	}
}