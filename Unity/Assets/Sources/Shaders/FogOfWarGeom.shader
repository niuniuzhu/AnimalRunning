Shader "Game/FogOfWar" {
	Properties{
		_Color("Main Color", Color) = (0,0,0,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Frequency("Frequency", Range(0,50)) = 6.0
		_Amplitude("Amplitude", Range(0,0.2)) = 0.05
	}

		SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf NoLighting vertex:vert alpha:blend

		sampler2D _MainTex;
		fixed4 _Color;
		float _Frequency;
		float _Amplitude;
		int _NumPlayers;
		float4 _PlayerPositions[5];
		float _PlayerFovs[5];

		struct Input {
			float2 location;
		};

		float AlphaContribution(float4 pos, float fov, float2 nearVertex) {
			float r = fov * (1 + sin(_Time.g * _Frequency) * _Amplitude);
			float atten = saturate(length(pos.xz - nearVertex.xy) / r);
			return tex2D(_MainTex, float2(atten, 0)).r;
		}

		void vert(inout appdata_full vertexData, out Input outData) {
			float4 pos = UnityObjectToClipPos(vertexData.vertex);
			float4 posWorld = mul(unity_ObjectToWorld, vertexData.vertex);
			outData.location = posWorld.xz;
		}

		void surf(Input IN, inout SurfaceOutput o) {
			float alphaContribution = 0;
			for (int i = 0; i < _NumPlayers; i++) {
				alphaContribution += AlphaContribution(_PlayerPositions[i], _PlayerFovs[i], IN.location);
			}

			o.Albedo = _Color.rgb;
			o.Alpha = (1.0 - saturate(alphaContribution)) * _Color.a;
		}
		
		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

	ENDCG
	}
	Fallback "Transparent/VertexLit"
}