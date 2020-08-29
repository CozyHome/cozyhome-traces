Shader "Custom/Toony"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		[HDR] _Ambient("Ambient", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert finalcolor:fragmenttoon


		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		fixed4 _Color;
		fixed4 _Ambient;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

			//https://www.nbdtech.com/Blog/archive/2008/04/27/Calculating-the-Perceived-Brightness-of-a-Color.aspx
			// Credit to NirDobovizki on they're fast light intensity approximation formula
			// I didn't design this toon shader with time in mind. This legit took like 15 mins to cook up so I do not recommend using this
			// shader outside of it being a placeholder for anything you're doing.
			// here is a link to an amazing tutorial by Erik Roystan on a Toon Shader based on BOTW : 

			// Link: https://roystan.net/articles/toon-shader.html

			float brightApprox(float3 c)
			{
				return sqrt(
					c.x * c.x * .241 +
					c.y * c.y * .691 +
					c.z * c.z * .068);
			}

			void fragmenttoon(Input IN, SurfaceOutput o, inout fixed4 color)
			{
				float i = brightApprox(color);
				i -= (i % 0.25);

				i = min(i, 1.0);
				i = max(i, 0.3);

				color = color * i;
				color *= _Ambient;

				color = tex2D(_MainTex, IN.uv_MainTex) * color * _Color;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				// Albedo comes from a texture tinted by color
				float4 w = (1, 1, 1, 1); //little hack I learned with amplify is to just input a white color to the surface shader.
				o.Albedo = w; //then, I'll multiply by my base color and everything after.
			}

			ENDCG
	}
		FallBack "Diffuse"
}
