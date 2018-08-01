namespace Logic
{
	public abstract class GPoolObject
	{
		public string rid { get; protected set; }

		internal void Dispose()
		{
			this.InternalDispose();
		}

		protected abstract void InternalDispose();
	}
}