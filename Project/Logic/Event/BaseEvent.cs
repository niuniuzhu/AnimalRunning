namespace Logic.Event
{
	public abstract class BaseEvent
	{
		public int type { get; protected set; }

		protected void BeginInvoke()
		{
			EventCenter.BeginInvoke( this );
		}

		protected void Invoke()
		{
			EventCenter.Invoke( this );
		}

		public abstract void Release();
	}
}