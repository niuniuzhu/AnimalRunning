namespace View
{
	public delegate void EffectHolderDestroied( IEffectHolder holder ); 

	public interface IEffectHolder
	{
		EffectHolderDestroied destroyNotifier { set; }
	}
}