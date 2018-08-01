namespace View
{
	public class CTerminus : CCollider
	{
		public void Bright()
		{
			this.graphic.model.Find( "Bone001/Dummy001/Effect" ).gameObject.SetActive( true );
			this.graphic.animator.SetBool( "play", true );
		}
	}
}