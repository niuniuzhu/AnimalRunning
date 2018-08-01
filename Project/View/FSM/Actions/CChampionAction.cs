using Logic.FSM;
using Logic.FSM.Actions;

namespace View.FSM.Actions
{
	public abstract class CChampionAction : AbsAction
	{
		protected CChampion owner { get; private set; }

		public override FSMState state
		{
			set
			{
				base.state = value;
				this.owner = ( CChampion )value.owner;
			}
		}
	}
}