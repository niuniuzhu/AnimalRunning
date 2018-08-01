using UnityEngine;
using Debug = UnityEngine.Debug;

public class NewBehaviourScript : MonoBehaviour
{
	void Start()
	{
		Matrix4x4 m = Matrix4x4.TRS( new Vector3( 1, -2, 3 ), Quaternion.Euler( 90, 0, 0 ), new Vector3( 2, 3, 4 ) );
		Debug.Log( m );
		m = Matrix4x4.Inverse( m );
		Debug.Log( m );
		Debug.Log( m.MultiplyPoint( new Vector3( 1, 0, -1 ) ) );

		var q = Quaternion.AngleAxis( -43, new Vector3( 3, 2, 4 ).normalized );
		Matrix4x4 m2 = Matrix4x4.Rotate( q );
		Debug.Log( m2 );

		var m3 = m * m2;
		Debug.Log( m3 );
	}
}
